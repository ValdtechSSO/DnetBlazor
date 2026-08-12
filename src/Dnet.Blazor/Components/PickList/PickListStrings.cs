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
    public string SearchPlaceholder { get; init; } = "Search";

    /// <summary>Gets the selected-items counter label.</summary>
    public string SelectedLabel { get; init; } = "Selected";

    /// <summary>Gets the select-current-page action label.</summary>
    public string SelectVisibleLabel { get; init; } = "Select page";

    /// <summary>Gets the clear-selection action label.</summary>
    public string ClearLabel { get; init; } = "Clear";

    /// <summary>Gets the empty-list label.</summary>
    public string EmptyLabel { get; init; } = "No items";

    /// <summary>Gets the no-search-results label.</summary>
    public string NoResultsLabel { get; init; } = "No results";

    /// <summary>Gets the accessible label for the pager.</summary>
    public string PaginationLabel { get; init; } = "Pagination";

    /// <summary>Gets the text displayed before the current page input.</summary>
    public string PageText { get; init; } = "Page";

    /// <summary>Gets the text displayed between page or range values and their total.</summary>
    public string OfText { get; init; } = "of";

    /// <summary>Gets the accessible label for the first-page action.</summary>
    public string FirstPageLabel { get; init; } = "First page";

    /// <summary>Gets the accessible label for the previous-page action.</summary>
    public string PreviousPageLabel { get; init; } = "Previous page";

    /// <summary>Gets the format for a numbered page label. Placeholder {0} is the one-based page number.</summary>
    public string PageLabelFormat { get; init; } = "Page {0}";

    /// <summary>Gets the accessible label for the next-page action.</summary>
    public string NextPageLabel { get; init; } = "Next page";

    /// <summary>Gets the accessible label for the last-page action.</summary>
    public string LastPageLabel { get; init; } = "Last page";

    /// <summary>Gets the label shown during the first provider load.</summary>
    public string LoadingLabel { get; init; } = "Loading…";
}
