using Bunit;
using Dnet.Blazor.Components.Grid.BlgRow;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridRowHeightTests : BunitContext
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Grouping_cells_follow_row_height_instead_of_group_column_height(bool isGroup)
    {
        var row = CreateRow(isGroup);
        var options = new GridOptions<GridItem>
        {
            RowHeight = 56
        };
        var groupColumn = new GridColumn<GridItem>
        {
            Height = 40,
            Width = 200
        };
        var columns = new List<GridColumn<GridItem>>
        {
            new()
            {
                ColumnId = 1,
                DataField = nameof(GridItem.Name),
                CellDataFn = parameters => parameters.RowData!.Name
            }
        };

        var cut = Render<BlgRow<GridItem>>(parameters => parameters
            .Add(component => component.RowNode, row)
            .Add(component => component.RowId, 1)
            .Add(component => component.GridColumns, columns)
            .Add(component => component.GroupGridColumn, groupColumn)
            .Add(component => component.GridOptions, options)
            .Add(component => component.HasGrouping, true)
            .Add(component => component.Pinned, Pinned.None));

        var groupingCellStyle = Assert.IsType<string>(
            cut.Find(".blg-cell").GetAttribute("style"));

        Assert.Contains("height: 56px", groupingCellStyle);
        Assert.DoesNotContain("height: 40px", groupingCellStyle);
    }

    private static RowNode<GridItem> CreateRow(bool isGroup) => new()
    {
        RowNodeId = 1,
        RowData = new GridItem("Role"),
        GroupValue = "Client",
        Level = 1,
        IsGroup = isGroup,
        RowSpanSkippedCells = new Dictionary<GridColumn<GridItem>, bool>(),
        RowSpanTargetCells = new Dictionary<GridColumn<GridItem>, uint>(),
        FirstSpanRow = new Dictionary<GridColumn<GridItem>, uint>(),
        FirstSpanRowData = new Dictionary<GridColumn<GridItem>, object>()
    };

    private sealed record GridItem(string Name);
}
