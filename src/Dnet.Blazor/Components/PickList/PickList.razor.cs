using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.PickList;

/// <summary>
/// A key-based, paged multi-selection component. Selection is controlled by
/// <see cref="SelectedKeys"/> and therefore survives searches and page changes.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
/// <typeparam name="TKey">The stable item key type.</typeparam>
public partial class PickList<TItem, TKey> : ComponentBase, IDisposable
    where TKey : notnull
{
    private const int MaxVisiblePages = 5;

    private int _pageIndex;
    private int? _filteredCount;
    private int? _totalCount;
    private bool _isLoading;
    private string? _searchText;
    private bool _searchInitialized;
    private IReadOnlyList<TItem> _visibleItems = Array.Empty<TItem>();
    private CancellationTokenSource? _loadCts;
    private long _requestGeneration;
    private long _selectionRenderGeneration;
    private long _searchRenderGeneration;
    private PickListItemsProvider<TItem>? _itemsProviderReference;
    private string? _providerSearchText;
    private int _lastPageSize;
    private IReadOnlySet<TKey>? _pendingSelectedKeys;
    private bool _hasPendingSelectionChange;
    private string? _pendingSearchText;
    private bool _hasPendingSearchChange;

    /// <summary>Gets or sets the complete local collection. It cannot be used with <see cref="ItemsProvider"/>.</summary>
    [Parameter]
    public IReadOnlyList<TItem>? Items { get; set; }

    /// <summary>Gets or sets the provider used for paged, server-side data.</summary>
    [Parameter]
    public PickListItemsProvider<TItem>? ItemsProvider { get; set; }

    /// <summary>Gets or sets the stable key selector for an item.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, TKey> ItemKey { get; set; } = default!;

    /// <summary>Gets or sets the visual content of an item.</summary>
    [Parameter, EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = default!;

    /// <summary>Gets or sets the local-search text selector. It is required in local mode.</summary>
    [Parameter]
    public Func<TItem, string?>? SearchTextSelector { get; set; }

    /// <summary>Gets or sets the globally selected keys.</summary>
    [Parameter]
    public IReadOnlySet<TKey> SelectedKeys { get; set; } = new HashSet<TKey>();

    /// <summary>Raised with a new selected-key set when the user changes selection.</summary>
    [Parameter]
    public EventCallback<IReadOnlySet<TKey>> SelectedKeysChanged { get; set; }

    /// <summary>Gets or sets the externally controlled search text.</summary>
    [Parameter]
    public string? SearchText { get; set; }

    /// <summary>Raised when a controlled search text changes. Without it, search is managed internally.</summary>
    [Parameter]
    public EventCallback<string?> SearchTextChanged { get; set; }

    /// <summary>Gets or sets the number of visible items per page.</summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>Gets or sets instance-specific UI strings.</summary>
    [Parameter]
    public PickListStrings? Strings { get; set; }

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    private bool IsSearchControlled => SearchTextChanged.HasDelegate;

    private string? EffectiveSearchText => IsSearchControlled ? SearchText : _searchText;

    private PickListStrings EffectiveStrings =>
        Strings ?? Services.GetService(typeof(PickListStrings)) as PickListStrings ?? PickListStrings.Default;

    private int PageCount => _filteredCount is null
        ? 0
        : (int)Math.Ceiling(_filteredCount.Value / (double)PageSize);

    private bool HasKnownPages => _filteredCount is not null;

    private bool CanSelectVisible => !_isLoading && _visibleItems.Count > 0;

    protected override async Task OnParametersSetAsync()
    {
        ValidateConfiguration();

        var pageSizeChanged = _lastPageSize != PageSize;
        _lastPageSize = PageSize;

        ResolvePendingControlledChanges();

        if (!_searchInitialized)
        {
            _searchText = SearchText;
            _searchInitialized = true;
        }

        if (ItemsProvider is null)
        {
            if (_itemsProviderReference is not null)
            {
                CancelCurrentRequest();
                _itemsProviderReference = null;
                _providerSearchText = null;
            }

            RebuildLocalItems();
            return;
        }

        // Delegate.Equals compares invocation lists (target + method). This avoids
        // reloading when a parent recreates an equivalent delegate on every render.
        var providerChanged = !Equals(_itemsProviderReference, ItemsProvider);
        var searchChanged = !string.Equals(_providerSearchText, EffectiveSearchText, StringComparison.Ordinal);

        if (providerChanged)
        {
            CancelCurrentRequest();
            _itemsProviderReference = ItemsProvider;
            _providerSearchText = EffectiveSearchText;
            _pageIndex = 0;
            _totalCount = null;
            _filteredCount = null;
            _visibleItems = Array.Empty<TItem>();
            await ReloadProviderAsync();
            return;
        }

        if (searchChanged)
        {
            _providerSearchText = EffectiveSearchText;
            _pageIndex = 0;
            _filteredCount = null;
            await ReloadProviderAsync();
            return;
        }

        if (pageSizeChanged)
        {
            ClampPageIndex();
            await ReloadProviderAsync();
        }
    }

    /// <summary>
    /// Reloads local data or the current provider window. A provider refresh always
    /// invalidates the filtered count, and may also invalidate the global count.
    /// </summary>
    /// <param name="invalidateTotalCount">Whether the global count must be requested again.</param>
    public async Task RefreshAsync(bool invalidateTotalCount = false)
    {
        if (ItemsProvider is null)
        {
            RebuildLocalItems();
            await InvokeAsync(StateHasChanged);
            return;
        }

        _filteredCount = null;
        if (invalidateTotalCount)
        {
            _totalCount = null;
        }

        await ReloadProviderAsync();
    }

    private void ValidateConfiguration()
    {
        if (Items is not null && ItemsProvider is not null)
        {
            throw new InvalidOperationException("PickList accepts either Items or ItemsProvider, but not both.");
        }

        if (ItemKey is null)
        {
            throw new InvalidOperationException("PickList requires ItemKey.");
        }

        if (ItemTemplate is null)
        {
            throw new InvalidOperationException("PickList requires ItemTemplate.");
        }

        if (!SelectedKeysChanged.HasDelegate)
        {
            throw new InvalidOperationException(
                "PickList requires two-way binding for selection. Use @bind-SelectedKeys.");
        }

        if (PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(PageSize), PageSize, "PageSize must be greater than zero.");
        }

        if (Items is not null && SearchTextSelector is null)
        {
            throw new InvalidOperationException("PickList requires SearchTextSelector when using Items.");
        }
    }

    private void RebuildLocalItems()
    {
        var allItems = Items ?? Array.Empty<TItem>();
        var searchText = EffectiveSearchText;
        IEnumerable<TItem> filteredItems = allItems;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filteredItems = allItems.Where(item =>
                (SearchTextSelector!(item) ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = filteredItems.ToArray();
        _totalCount = allItems.Count;
        _filteredCount = filtered.Length;
        ClampPageIndex();
        _visibleItems = NormalizeVisibleItems(filtered
            .Skip(_pageIndex * PageSize)
            .Take(PageSize));
        _isLoading = false;
    }

    private async Task ReloadProviderAsync()
    {
        var provider = ItemsProvider;
        if (provider is null)
        {
            return;
        }

        CancelCurrentRequest();
        var cts = new CancellationTokenSource();
        _loadCts = cts;
        var generation = ++_requestGeneration;
        _isLoading = true;

        var requestTotalCount = _totalCount is null;
        var requestFilteredCount = _filteredCount is null;
        var request = new PickListItemsProviderRequest(
            _pageIndex,
            PageSize,
            EffectiveSearchText,
            requestTotalCount,
            requestFilteredCount,
            cts.Token);

        try
        {
            var result = await provider(request);
            if (!IsCurrentRequest(generation, cts))
            {
                return;
            }

            if (result is null || result.Items is null)
            {
                throw new InvalidOperationException("PickList ItemsProvider returned a null result or item collection.");
            }

            if (requestTotalCount && result.TotalCount is null)
            {
                throw new InvalidOperationException("PickList ItemsProvider must return TotalCount when requested.");
            }

            if (requestFilteredCount && result.FilteredCount is null)
            {
                throw new InvalidOperationException("PickList ItemsProvider must return FilteredCount when requested.");
            }

            if (result.TotalCount is < 0)
            {
                throw new InvalidOperationException("PickList ItemsProvider returned a negative TotalCount.");
            }

            if (result.FilteredCount is < 0)
            {
                throw new InvalidOperationException("PickList ItemsProvider returned a negative FilteredCount.");
            }

            if (result.TotalCount is { } totalCount)
            {
                _totalCount = totalCount;
            }

            if (result.FilteredCount is { } filteredCount)
            {
                _filteredCount = filteredCount;
            }

            _visibleItems = NormalizeVisibleItems(result.Items);

            var previousPage = _pageIndex;
            ClampPageIndex();
            if (_pageIndex != previousPage)
            {
                await ReloadProviderAsync();
                return;
            }
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // A newer request owns the visual state.
        }
        finally
        {
            if (IsCurrentRequest(generation, cts))
            {
                _isLoading = false;
                _loadCts = null;
                cts.Dispose();
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private bool IsCurrentRequest(long generation, CancellationTokenSource cts) =>
        generation == _requestGeneration && ReferenceEquals(_loadCts, cts);

    private void CancelCurrentRequest()
    {
        if (_loadCts is null)
        {
            return;
        }

        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadCts = null;
        _requestGeneration++;
    }

    private IReadOnlyList<TItem> NormalizeVisibleItems(IEnumerable<TItem> items)
    {
        var deduplicated = new Dictionary<TKey, TItem>();
        foreach (var item in items)
        {
            var key = ItemKey(item);
#if DEBUG
            Debug.Assert(!deduplicated.ContainsKey(key), "PickList received duplicate ItemKey values in one page.");
#endif
            deduplicated[key] = item;
        }

        return deduplicated.Values.ToArray();
    }

    private async Task SetSelectedAsync(TKey key, bool isSelected)
    {
        var next = new HashSet<TKey>(SelectedKeys);
        if (isSelected)
        {
            next.Add(key);
        }
        else
        {
            next.Remove(key);
        }

        _pendingSelectedKeys = next;
        _hasPendingSelectionChange = true;
        await SelectedKeysChanged.InvokeAsync(next);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectVisibleAsync()
    {
        if (!CanSelectVisible)
        {
            return;
        }

        var next = new HashSet<TKey>(SelectedKeys);
        foreach (var item in _visibleItems)
        {
            next.Add(ItemKey(item));
        }

        await SelectedKeysChanged.InvokeAsync(next);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ClearSelectionAsync()
    {
        await SelectedKeysChanged.InvokeAsync(new HashSet<TKey>());
        await InvokeAsync(StateHasChanged);
    }

    private async Task SetSearchTextAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        if (IsSearchControlled)
        {
            _pendingSearchText = value;
            _hasPendingSearchChange = true;
            await SearchTextChanged.InvokeAsync(value);
        }
        else
        {
            _searchText = value;
        }

        await ApplySearchChangeAsync();
    }

    private void ResolvePendingControlledChanges()
    {
        ResolvePendingSelectionChange();
        ResolvePendingSearchChange();
    }

    private void ResolvePendingSelectionChange()
    {
        if (!_hasPendingSelectionChange)
        {
            return;
        }

        if (_pendingSelectedKeys is not null && !SelectedKeys.SetEquals(_pendingSelectedKeys))
        {
            // A controlled parent rejected or transformed the native checkbox
            // change. Recreate the input so its DOM property returns to source-of-truth.
            _selectionRenderGeneration++;
        }

        _pendingSelectedKeys = null;
        _hasPendingSelectionChange = false;
    }

    private void ResolvePendingSearchChange()
    {
        if (!_hasPendingSearchChange)
        {
            return;
        }

        if (!string.Equals(SearchText, _pendingSearchText, StringComparison.Ordinal))
        {
            // Only a rejected/transformed controlled value requires DOM
            // recreation. Accepted typing must keep the same focused input.
            _searchRenderGeneration++;
        }

        _pendingSearchText = null;
        _hasPendingSearchChange = false;
    }

    private async Task ApplySearchChangeAsync()
    {
        if (ItemsProvider is null)
        {
            _pageIndex = 0;
            RebuildLocalItems();
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (string.Equals(_providerSearchText, EffectiveSearchText, StringComparison.Ordinal))
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        _providerSearchText = EffectiveSearchText;
        _pageIndex = 0;
        _filteredCount = null;
        await ReloadProviderAsync();
    }

    private async Task GoToPageAsync(int pageIndex)
    {
        if (_isLoading || !HasKnownPages || PageCount == 0)
        {
            return;
        }

        var normalizedPage = Math.Clamp(pageIndex, 0, PageCount - 1);
        if (normalizedPage == _pageIndex)
        {
            return;
        }

        _pageIndex = normalizedPage;
        if (ItemsProvider is null)
        {
            RebuildLocalItems();
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            await ReloadProviderAsync();
        }
    }

    private Task GoToFirstPageAsync() => GoToPageAsync(0);

    private Task GoToPreviousPageAsync() => GoToPageAsync(_pageIndex - 1);

    private Task GoToNextPageAsync() => GoToPageAsync(_pageIndex + 1);

    private Task GoToLastPageAsync() => GoToPageAsync(PageCount - 1);

    private IEnumerable<int> GetVisiblePageNumbers()
    {
        if (PageCount == 0)
        {
            return Array.Empty<int>();
        }

        var start = Math.Max(0, _pageIndex - (MaxVisiblePages / 2));
        var end = Math.Min(PageCount - 1, start + MaxVisiblePages - 1);
        start = Math.Max(0, end - MaxVisiblePages + 1);
        return Enumerable.Range(start, end - start + 1);
    }

    private string GetPageLabel(int zeroBasedPage) =>
        string.Format(CultureInfo.CurrentCulture, EffectiveStrings.PageLabelFormat, zeroBasedPage + 1);

    private void ClampPageIndex()
    {
        if (_filteredCount is null)
        {
            return;
        }

        var lastPage = Math.Max(0, PageCount - 1);
        _pageIndex = Math.Clamp(_pageIndex, 0, lastPage);
    }

    private bool IsNoResults => _filteredCount == 0 && !string.IsNullOrWhiteSpace(EffectiveSearchText);

    private bool IsEmpty => _filteredCount == 0 && string.IsNullOrWhiteSpace(EffectiveSearchText);

    public void Dispose()
    {
        CancelCurrentRequest();
    }
}
