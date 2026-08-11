namespace Dnet.Blazor.Components.PickList;

/// <summary>
/// A page returned by a <see cref="PickListItemsProvider{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public sealed record PickListItemsProviderResult<TItem>(
    IReadOnlyList<TItem> Items,
    int? TotalCount,
    int? FilteredCount);
