using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Virtualize;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Globalization;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private void SeedInitialVirtualWindow()
    {
        _itemCount = _renderedRowNodes.Count;
        _itemsBefore = 0;

        var fallbackCapacity = Math.Max(1, (Math.Max(0, GridOptions.OverscanCount) * 2) + 1);
        _visibleItemCapacity = Math.Max(1, _visibleItemCapacity > 0 ? _visibleItemCapacity : fallbackCapacity);

        _loadedItems = _renderedRowNodes;
        _loadedItemsStartIndex = 0;
        _itemsToShow = _renderedRowNodes.Take(_visibleItemCapacity).ToList();

        foreach (var rowNode in _itemsToShow)
        {
            rowNode.First = false;
        }

        if (_itemsToShow.Count > 0)
        {
            _itemsToShow[0].First = true;
        }

        _hasResolvedInitialVirtualWindow = true;
    }

    private async Task DefaultVirtualization()
    {
        _lastRenderedItemCount = 0;

        CalculateItemDistribution(0, 0, _containerSize, out var itemsBefore, out var visibleItemCapacity);

        // Before the observer reports the viewport size, request at least one
        // row so an overscan value of zero cannot leave the initial grid blank.
        visibleItemCapacity = Math.Max(1, visibleItemCapacity);

        await UpdateItemDistributionAsync(itemsBefore, visibleItemCapacity, true);
    }

    private async Task UpdateGridData()
    {
        if (_renderedRefreshVersion != Volatile.Read(ref _refreshVersion))
        {
            return;
        }

        var lastItemIndex = Math.Min(_itemsBefore + _visibleItemCapacity, _itemCount);
        // Si la ventana que queremos mostrar aún no está completamente cargada
        // en _loadedItems, mantenemos la vista anterior para evitar zonas en blanco
        if (_loadedItems == null ||
            _itemsBefore < _loadedItemsStartIndex ||
            lastItemIndex > _loadedItemsStartIndex + _loadedItems.Count)
        {
            _updatingGridData = false;
            return;
        }

        var loadedItemsOffset = _itemsBefore - _loadedItemsStartIndex;
        var itemsToRender = Math.Max(0, lastItemIndex - _itemsBefore);

        // The in-memory provider returns an already-materialized List for the
        // exact requested window. Reuse it to avoid a second allocation and a
        // second Skip/Take traversal on every scroll update.
        if (loadedItemsOffset == 0 && itemsToRender == _loadedItems.Count && _loadedItems is List<RowNode<TItem>> loadedList)
        {
            _itemsToShow = loadedList;
        }
        else
        {
            _itemsToShow = _loadedItems
                .Skip(loadedItemsOffset)
                .Take(itemsToRender)
                .ToList();
        }

        foreach (var rowNode in _itemsToShow)
            rowNode.First = false;

        if (_itemsToShow.Count > 0)
            _itemsToShow[0].First = true;

        _hasResolvedInitialVirtualWindow = true;

        _blgCenter?.ActiveRender();

        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();

        if (_pinnedRight) _blgPinnedRight?.ActiveRender();

        await InvokeAsync(StateHasChanged);

        _updatingGridData = false;
    }

    private string GetSpacerStyle(int itemsInSpacer) =>
        $"height: {(itemsInSpacer * _itemSize).ToString(CultureInfo.InvariantCulture)}px; width: {eBodyHorizontalScrollContainerWidth}px";

    private int GetVirtualizationRootMargin()
    {
        const int blazorDefaultRootMargin = 50;
        var rowHeight = Math.Max(1, GridOptions.RowHeight);
        var overscanCount = Math.Max(0, GridOptions.OverscanCount);

        // Virtualize<TItem> uses a 50 px observer margin. A Grid row is much
        // heavier than a typical list item, so begin the next render while half
        // of the overscan buffer is still available. Keeping the margin below
        // the full buffer prevents a spacer from remaining intersecting after
        // redistribution and causing a render loop.
        var halfOverscanBuffer = (long)rowHeight * overscanCount / 2;
        return (int)Math.Min(int.MaxValue, Math.Max(blazorDefaultRootMargin, halfOverscanBuffer));
    }

    async Task IVirtualizeJsCallbacks.OnBeforeSpacerVisible(float spacerSize, float spacerSeparation, float containerSize)
    {
        if (_previousHoverRowNode != null) _previousHoverRowNode.HoverThisNode(false);

        _updatingGridData = true;

        _containerSize = containerSize;

        CalculateItemDistribution(spacerSize, spacerSeparation, containerSize, out var itemsBefore, out var visibleItemCapacity);

        // Since we know the before spacer is now visible, we absolutely have to slide the window up
        // by at least one element. If we're not doing that, the previous item size info we had must
        // have been wrong, so just move along by one in that case to trigger an update and apply the
        // new size info.
        if (itemsBefore == _itemsBefore && itemsBefore > 0)
        {
            itemsBefore--;
        }

        await UpdateItemDistributionAsync(itemsBefore, visibleItemCapacity);
    }

    async Task IVirtualizeJsCallbacks.OnAfterSpacerVisible(float spacerSize, float spacerSeparation, float containerSize)
    {
        if (_previousHoverRowNode != null) _previousHoverRowNode.HoverThisNode(false);

        _updatingGridData = true;

        _containerSize = containerSize;

        CalculateItemDistribution(spacerSize, spacerSeparation, containerSize, out var itemsAfter, out var visibleItemCapacity);

        var itemsBefore = Math.Max(0, _itemCount - itemsAfter - visibleItemCapacity);

        // Since we know the after spacer is now visible, we absolutely have to slide the window down
        // by at least one element. If we're not doing that, the previous item size info we had must
        // have been wrong, so just move along by one in that case to trigger an update and apply the
        // new size info.
        if (itemsBefore == _itemsBefore && itemsBefore < _itemCount - visibleItemCapacity)
        {
            itemsBefore++;
        }

        await UpdateItemDistributionAsync(itemsBefore, visibleItemCapacity);
    }

    private void CalculateItemDistribution(
        float spacerSize,
        float spacerSeparation,
        float containerSize,
        out int itemsInSpacer,
        out int visibleItemCapacity)
    {
        if (_lastRenderedItemCount > 0)
        {
            _itemSize = (spacerSeparation - (_lastRenderedPlaceholderCount * _itemSize)) / _lastRenderedItemCount;
        }

        if (_itemSize <= 0)
        {
            // At this point, something unusual has occurred, likely due to misuse of this component.
            // Reset the calculated item size to the user-provided item size.
            _itemSize = GridOptions.RowHeight;
        }

        // Align with Virtualize<TItem> in .NET 10: no extra -1
        var overscanCount = Math.Max(0, GridOptions.OverscanCount);
        itemsInSpacer = Math.Max(0, (int)Math.Floor(spacerSize / _itemSize) - overscanCount);

        visibleItemCapacity = (int)Math.Ceiling(containerSize / _itemSize) + 2 * overscanCount;
    }

    private async Task UpdateItemDistributionAsync(int itemsBefore, int visibleItemCapacity, bool forceRefresh = false)
    {
        if (FitsInTwoVirtualWindows(_itemCount, visibleItemCapacity))
        {
            // For a small, usually paged data set, swapping almost the entire
            // row tree costs more than keeping it mounted. Rendering the two
            // adjacent windows together also makes scrollbar-thumb jumps fully
            // synchronous in the browser, with no spacer exposed for a frame.
            itemsBefore = 0;
            visibleItemCapacity = _itemCount;
        }

        if (itemsBefore + visibleItemCapacity > _itemCount)
        {
            itemsBefore = Math.Max(0, _itemCount - visibleItemCapacity);
        }

        if (itemsBefore == _itemsBefore && visibleItemCapacity == _visibleItemCapacity && forceRefresh == false)
        {
            return;
        }

        _itemsBefore = itemsBefore;

        _visibleItemCapacity = visibleItemCapacity;

        await RefreshVirtualDataAsync();
        await UpdateGridData();
    }

    private static bool FitsInTwoVirtualWindows(int itemCount, int visibleItemCapacity) =>
        itemCount > 0 && visibleItemCapacity > 0 && itemCount <= (long)visibleItemCapacity * 2;

    private async Task RefreshVirtualDataAsync()
    {
        var usesDefaultProvider = _itemsProvider == DefaultItemsProvider;
        CancellationToken cancellationToken;

        if (usesDefaultProvider)
        {
            // All Grid rows are already in memory. Match Blazor's optimized
            // default-provider path: no CancellationTokenSource allocation and
            // no cancellation churn for a ValueTask that completes synchronously.
            _refreshCts?.Dispose();
            _refreshCts = null;
            cancellationToken = CancellationToken.None;
        }
        else
        {
            if (_refreshCts is not null && !_refreshCts.IsCancellationRequested)
            {
                Interlocked.Increment(ref _virtualRequestsCancelled);
            }

            _refreshCts?.Cancel();
            _refreshCts?.Dispose();
            _refreshCts = new CancellationTokenSource();
            cancellationToken = _refreshCts.Token;
        }

        var refreshVersion = Interlocked.Increment(ref _refreshVersion);
        Interlocked.Increment(ref _virtualRequestsStarted);
        var stopwatch = Stopwatch.StartNew();

        var request = new ItemsProviderRequest(_itemsBefore, _visibleItemCapacity, cancellationToken);

        try
        {
            var result = await _itemsProvider(request);

            // Only apply result if the task was not canceled.
            if (!cancellationToken.IsCancellationRequested && refreshVersion == Volatile.Read(ref _refreshVersion))
            {
                _itemCount = result.TotalItemCount;
                _loadedItems = result.Items as IReadOnlyList<RowNode<TItem>> ?? result.Items.ToList();
                _loadedItemsStartIndex = request.StartIndex;
                _renderedRefreshVersion = refreshVersion;
                _refreshException = null;
                Interlocked.Increment(ref _virtualResponsesApplied);
            }
            else
            {
                Interlocked.Increment(ref _virtualResponsesDiscarded);
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
            {
                // No-op; we canceled the operation, so it's fine to suppress this exception.
            }
            else
            {
                // Cache this exception so the renderer can throw it.
                if (refreshVersion == Volatile.Read(ref _refreshVersion))
                {
                    _refreshException = e;
                    StateHasChanged();
                }
            }
        }
        finally
        {
            stopwatch.Stop();
            Interlocked.Add(ref _virtualRefreshDurationMilliseconds, stopwatch.ElapsedMilliseconds);
        }
    }

    private ValueTask<ItemsProviderResult<RowNode<TItem>>> DefaultItemsProvider(ItemsProviderRequest request)
    {
        var totalItemCount = _renderedRowNodes.Count;
        var startIndex = Math.Clamp(request.StartIndex, 0, totalItemCount);
        var itemCount = Math.Clamp(request.Count, 0, totalItemCount - startIndex);
        var items = _renderedRowNodes.GetRange(startIndex, itemCount);

        return ValueTask.FromResult(new ItemsProviderResult<RowNode<TItem>>(items, totalItemCount));
    }

    [JSInvokable]
    public async Task OnTouchMove(ScrollInfo scrollInfo)
    {
        _transformX = $"-{Math.Floor(scrollInfo.ElementScrollLeft)}px";

        StateHasChanged();
    }
}
