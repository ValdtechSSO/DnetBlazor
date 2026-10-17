using Dnet.Blazor.Localization;

namespace Dnet.Blazor.Components.PickList;

/// <summary>
/// Text used by <see cref="PickList{TItem,TKey}"/>. Register an instance in DI
/// for an application-wide default, or set <c>Strings</c> on a component instance.
/// </summary>
public sealed record PickListStrings
{
    /// <summary>Gets the default English strings.</summary>
    public static PickListStrings Default { get; } = new();

    /// <summary>Gets the placeholder and accessible label for the search field.</summary>
    public string SearchPlaceholder { get => _searchPlaceholder ?? "Search"; init => _searchPlaceholder = value; }

    /// <summary>Gets the selected-items counter label.</summary>
    public string SelectedLabel { get => _selectedLabel ?? "Selected"; init => _selectedLabel = value; }

    /// <summary>Gets the select-current-page action label.</summary>
    public string SelectVisibleLabel { get => _selectVisibleLabel ?? "Select page"; init => _selectVisibleLabel = value; }

    /// <summary>Gets the clear-selection action label.</summary>
    public string ClearLabel { get => _clearLabel ?? "Clear"; init => _clearLabel = value; }

    /// <summary>Gets the empty-list label.</summary>
    public string EmptyLabel { get => _emptyLabel ?? "No items"; init => _emptyLabel = value; }

    /// <summary>Gets the no-search-results label.</summary>
    public string NoResultsLabel { get => _noResultsLabel ?? "No results"; init => _noResultsLabel = value; }

    /// <summary>Gets the accessible label for the pager.</summary>
    public string PaginationLabel { get => _paginationLabel ?? "Pagination"; init => _paginationLabel = value; }

    /// <summary>Gets the text displayed before the current page input.</summary>
    public string PageText { get => _pageText ?? "Page"; init => _pageText = value; }

    /// <summary>Gets the text displayed between page or range values and their total.</summary>
    public string OfText { get => _ofText ?? "of"; init => _ofText = value; }

    /// <summary>Gets the accessible label for the first-page action.</summary>
    public string FirstPageLabel { get => _firstPageLabel ?? "First page"; init => _firstPageLabel = value; }

    /// <summary>Gets the accessible label for the previous-page action.</summary>
    public string PreviousPageLabel { get => _previousPageLabel ?? "Previous page"; init => _previousPageLabel = value; }

    /// <summary>Gets the format for a numbered page label. Placeholder {0} is the one-based page number.</summary>
    public string PageLabelFormat { get => _pageLabelFormat ?? "Page {0}"; init => _pageLabelFormat = value; }

    /// <summary>Gets the accessible label for the next-page action.</summary>
    public string NextPageLabel { get => _nextPageLabel ?? "Next page"; init => _nextPageLabel = value; }

    /// <summary>Gets the accessible label for the last-page action.</summary>
    public string LastPageLabel { get => _lastPageLabel ?? "Last page"; init => _lastPageLabel = value; }

    /// <summary>Gets the label shown during the first provider load.</summary>
    public string LoadingLabel { get => _loadingLabel ?? "Loading…"; init => _loadingLabel = value; }

    // Null backing fields distinguish an omitted label from an explicit legacy English override.
    private string? _searchPlaceholder;
    private string? _selectedLabel;
    private string? _selectVisibleLabel;
    private string? _clearLabel;
    private string? _emptyLabel;
    private string? _noResultsLabel;
    private string? _paginationLabel;
    private string? _pageText;
    private string? _ofText;
    private string? _firstPageLabel;
    private string? _previousPageLabel;
    private string? _pageLabelFormat;
    private string? _nextPageLabel;
    private string? _lastPageLabel;
    private string? _loadingLabel;

    internal static PickListStrings Resolve(PickListStrings? instance, PickListStrings? registered, IDnetLocalizer localizer) => new()
    {
        SearchPlaceholder = instance?._searchPlaceholder ?? registered?._searchPlaceholder ?? localizer["Common.Search"],
        SelectedLabel = instance?._selectedLabel ?? registered?._selectedLabel ?? localizer["PickList.Selected"],
        SelectVisibleLabel = instance?._selectVisibleLabel ?? registered?._selectVisibleLabel ?? localizer["PickList.SelectPage"],
        ClearLabel = instance?._clearLabel ?? registered?._clearLabel ?? localizer["Common.Clear"],
        EmptyLabel = instance?._emptyLabel ?? registered?._emptyLabel ?? localizer["Common.NoItems"],
        NoResultsLabel = instance?._noResultsLabel ?? registered?._noResultsLabel ?? localizer["Common.NoResults"],
        PaginationLabel = instance?._paginationLabel ?? registered?._paginationLabel ?? localizer["PickList.Pagination"],
        PageText = instance?._pageText ?? registered?._pageText ?? localizer["PickList.Page"],
        OfText = instance?._ofText ?? registered?._ofText ?? localizer["PickList.Of"],
        FirstPageLabel = instance?._firstPageLabel ?? registered?._firstPageLabel ?? localizer["PickList.FirstPage"],
        PreviousPageLabel = instance?._previousPageLabel ?? registered?._previousPageLabel ?? localizer["PickList.PreviousPage"],
        PageLabelFormat = instance?._pageLabelFormat ?? registered?._pageLabelFormat ?? localizer["PickList.PageLabel"],
        NextPageLabel = instance?._nextPageLabel ?? registered?._nextPageLabel ?? localizer["PickList.NextPage"],
        LastPageLabel = instance?._lastPageLabel ?? registered?._lastPageLabel ?? localizer["PickList.LastPage"],
        LoadingLabel = instance?._loadingLabel ?? registered?._loadingLabel ?? localizer["Common.Loading"],
    };
}
