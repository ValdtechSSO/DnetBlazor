using Bunit;
using Dnet.Blazor.Components.List;
using Dnet.Blazor.Components.Paginator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ListDragAndDropTests : BunitContext
{
    private sealed record Item(string Name);

    public ListDragAndDropTests()
    {
        Services.AddScoped(typeof(DnetListDragAndDropService<>), typeof(DnetListDragAndDropService<>));
        Services.AddScoped<IPaginator, Paginator>();
    }

    [Fact]
    public void Dragged_items_reach_the_connected_list_without_a_custom_item_template()
    {
        // The double list renders the lists without an item template, so the
        // fallback markup is what users drag. It used to mark the item as
        // draggable without ever saying which item travels, which left every drop
        // carrying an empty list: the connected list simply ignored it.
        List<Item>? dropped = null;

        var source = RenderSource();
        var target = RenderTarget(items => dropped = items);

        Assert.NotEmpty(source.FindAll(".dnet-list-item"));
        Assert.Equal("true", source.FindAll(".dnet-list-item")[0].GetAttribute("draggable"));

        source.FindAll(".dnet-list-item")[0].DragStart();
        target.Find(".dnet-list-items").Drop();

        Assert.NotNull(dropped);
        Assert.Single(dropped!);
        Assert.Equal("Ana", dropped![0].Name);
    }

    [Fact]
    public void A_drop_on_a_list_that_is_not_connected_is_ignored()
    {
        List<Item>? dropped = null;

        var source = RenderSource();
        var unrelated = Render<DnetList<Item>>(parameters => parameters
            .Add(component => component.Items, new List<Item> { new("Zoe") })
            .Add(component => component.ListOptions, new ListOptions<Item>
                {
                    ContainerName = "OtherContainer",
                    ConnectedTo = "NothingContainer",
                    DisplayValueConverter = item => item.Name
                })
            .Add(component => component.OnDrop, items => dropped = items));

        source.FindAll(".dnet-list-item")[0].DragStart();
        unrelated.Find(".dnet-list-items").Drop();

        Assert.Null(dropped);
    }

    private IRenderedComponent<DnetList<Item>> RenderSource()
        => Render<DnetList<Item>>(parameters => parameters
            .Add(component => component.Items, new List<Item> { new("Ana"), new("Luis") })
            .Add(component => component.ListOptions, new ListOptions<Item>
                {
                    ContainerName = "LeftContainer",
                    ConnectedTo = "RightContainer",
                    DisplayValueConverter = item => item.Name
                }));

    private IRenderedComponent<DnetList<Item>> RenderTarget(Action<List<Item>> onDrop)
        => Render<DnetList<Item>>(parameters => parameters
            .Add(component => component.Items, new List<Item> { new("Sofia") })
            .Add(component => component.ListOptions, new ListOptions<Item>
                {
                    ContainerName = "RightContainer",
                    ConnectedTo = "LeftContainer",
                    DisplayValueConverter = item => item.Name
                })
            .Add(component => component.OnDrop, items => onDrop(items)));
}
