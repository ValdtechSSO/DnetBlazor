using Bunit;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Infrastructure.Services;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridSelectAllTests : BunitContext
{
    [Fact]
    public async Task SelectAll_skips_rows_the_grid_disables_like_the_header_checkbox()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = RenderGrid(new[] { new Item("Ana", false), new Item("Luis", true), new Item("Marta", false) });

        await cut.InvokeAsync(() => cut.Instance.SelectAll());

        Assert.Equal(new[] { "Ana", "Marta" }, SelectedNames(cut));

        await cut.InvokeAsync(() => cut.Instance.DeselectAll());

        Assert.Empty(SelectedNames(cut));
    }

    private static string[] SelectedNames(IRenderedComponent<BlgGrid<Item>> cut) =>
        cut.Instance.GetSelectedNodes(visibleOnly: false)
            .Where(node => node.RowData is not null)
            .Select(node => node.RowData!.Name)
            .Order()
            .ToArray();

    private IRenderedComponent<BlgGrid<Item>> RenderGrid(Item[] items) =>
        Render<BlgGrid<Item>>(parameters => parameters
            .Add(component => component.GridData, items)
            .Add(component => component.GridColumns, new List<GridColumn<Item>>
            {
                new()
                {
                    ColumnId = 1,
                    DataField = nameof(Item.Name),
                    HeaderName = "Name",
                    Width = 200
                }
            })
            .Add(component => component.GridOptions, new GridOptions<Item>
            {
                UseVirtualization = false,
                CheckboxSelectionColumn = true,
                RowSelectionType = RowSelectionType.Multiple,
                DisableRow = item => item.Locked
            }));

    private sealed record Item(string Name, bool Locked);
}
