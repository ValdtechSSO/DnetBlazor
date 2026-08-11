namespace Dnet.Blazor.Components.PickList;

/// <summary>
/// Describes a server-side <see cref="PickList{TItem,TKey}"/> data request.
/// </summary>
public sealed record PickListItemsProviderRequest(
    int PageIndex,
    int PageSize,
    string? SearchText,
    bool RequestTotalCount,
    bool RequestFilteredCount,
    CancellationToken CancellationToken);
