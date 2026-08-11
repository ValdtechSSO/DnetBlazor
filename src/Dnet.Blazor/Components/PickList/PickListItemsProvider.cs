namespace Dnet.Blazor.Components.PickList;

/// <summary>
/// Loads one window of items for a <see cref="PickList{TItem,TKey}"/>.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
/// <param name="request">The requested page, search text and count requirements.</param>
/// <returns>The requested page and any requested counts.</returns>
public delegate ValueTask<PickListItemsProviderResult<TItem>> PickListItemsProvider<TItem>(
    PickListItemsProviderRequest request);
