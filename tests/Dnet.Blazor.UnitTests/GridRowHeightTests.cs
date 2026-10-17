using Bunit;
using Dnet.Blazor.Components.Grid.BlgHeader;
using Dnet.Blazor.Components.Grid.BlgRow;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Dnet.Blazor.Infrastructure.Services;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridRowHeightTests : BunitContext
{
    [Fact]
    public void Filter_cells_allow_the_field_height_instead_of_fixing_placeholders_to_40_pixels()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var columns = new List<GridColumn<GridItem>>
        {
            new()
            {
                ColumnId = 1,
                DataField = nameof(GridItem.Name),
                HeaderName = "Name",
                CellDataType = CellDataType.Text,
                EnableSimpleFilter = true
            },
            new()
            {
                ColumnId = 2,
                DataField = "Date",
                HeaderName = "Date",
                CellDataType = CellDataType.Date,
                EnableSimpleFilter = true,
                DateFormat = "yyyy-MM-dd"
            }
        };
        var cut = Render<BlgHeader<GridItem>>(parameters => parameters
            .Add(component => component.GridColumns, columns)
            .Add(component => component.GroupGridColumn, new GridColumn<GridItem> { Width = 200, Height = 40 })
            .Add(component => component.GroupByColumns, new List<string>())
            .Add(component => component.GridOptions, new GridOptions<GridItem>
            {
                SuppressFilterRow = false,
                CheckboxSelectionColumn = true,
                EnableGrouping = true,
                IsDebugMode = true
            })
            .Add(component => component.HasGrouping, true)
            .Add(component => component.CheckboxSelectionPinned, true)
            .Add(component => component.Pinned, Pinned.None));

        var cells = cut.FindAll(".blg-filter-cell");
        Assert.Equal(5, cells.Count);
        Assert.All(cells, cell => Assert.DoesNotContain("height:", cell.GetAttribute("style") ?? string.Empty));
        Assert.Equal(2, cut.FindAll(".blg-filter-cell .blg-filter-field input").Count);
        Assert.Contains("height: 40px", cut.Find(".blg-header-cell:not(.blg-filter-cell):not([role])").GetAttribute("style"));
    }

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
