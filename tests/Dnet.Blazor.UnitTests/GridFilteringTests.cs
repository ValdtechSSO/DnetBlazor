using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Components.Grid.Infrastructure.Services;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridFilteringTests
{
    [Fact]
    public void FilterBy_matches_text_without_normalizing_each_cell_value()
    {
        var matching = new RowNode<GridItem> { RowData = new GridItem("Árbol"), Show = true };
        var notMatching = new RowNode<GridItem> { RowData = new GridItem("Río"), Show = true };
        var tree = new TreeRowNode<GridItem>
        {
            Data = new RowNode<GridItem> { IsGroup = true, Show = true },
            Children =
            [
                new TreeRowNode<GridItem> { Data = matching, Children = [] },
                new TreeRowNode<GridItem> { Data = notMatching, Children = [] }
            ]
        };
        var column = new GridColumn<GridItem>
        {
            DataField = nameof(GridItem.Name),
            CellDataType = CellDataType.Text,
            CellDataFn = cell => cell.RowData!.Name
        };

        new Filtering<GridItem>().FilterBy(
            tree,
            [new FilterModel { DataDield = nameof(GridItem.Name), Filter = "ÁRB" }],
            [column],
            new CellParams<GridItem>());

        Assert.True(matching.Show);
        Assert.False(notMatching.Show);
    }

    private sealed record GridItem(string Name);
}
