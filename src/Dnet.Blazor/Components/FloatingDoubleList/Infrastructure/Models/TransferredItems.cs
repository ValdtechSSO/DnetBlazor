namespace Dnet.Blazor.Components.FloatingDoubleList.Infrastructure.Models;

public class TransferredItems<TItem>
{
    public List<TItem> RightItems = new();

    public List<TItem> RightAddedItems = new();

    public List<TItem> RightRemovedItems = new();

    public List<TItem> LeftItems = new();

    public List<TItem> LeftAddedItems = new();

    public List<TItem> LeftRemovedItems = new();
}
