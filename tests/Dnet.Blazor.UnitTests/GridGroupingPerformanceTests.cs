using System.Reflection;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridGroupingPerformanceTests
{
    [Fact]
    public void Grouping_does_not_reapply_empty_filters()
    {
        var grid = CreateGrid(new GridColumn<GridItem>
        {
            DataField = nameof(GridItem.Name),
            CellDataType = CellDataType.Text,
            Filter = string.Empty
        });

        InvokeWithoutArguments(grid, "ReapplyActiveFiltersAfterGrouping");

        Assert.False(InvokeFilterCheck(grid, "HasActiveSimpleFilters"));
        Assert.False(InvokeFilterCheck(grid, "HasActiveAdvancedFilters"));
    }

    [Fact]
    public void Grouping_preserves_active_simple_and_advanced_filters()
    {
        var simpleGrid = CreateGrid(new GridColumn<GridItem>
        {
            DataField = nameof(GridItem.Name),
            CellDataType = CellDataType.Text,
            Filter = "Ada"
        });
        var advancedColumn = new GridColumn<GridItem>
        {
            DataField = nameof(GridItem.Name),
            CellDataType = CellDataType.Text
        };
        advancedColumn.AdvancedFilterModel.Value = "Ada";
        var advancedGrid = CreateGrid(advancedColumn);

        Assert.True(InvokeFilterCheck(simpleGrid, "HasActiveSimpleFilters"));
        Assert.True(InvokeFilterCheck(advancedGrid, "HasActiveAdvancedFilters"));
    }

    private static BlgGrid<GridItem> CreateGrid(GridColumn<GridItem> column)
    {
        var grid = new BlgGrid<GridItem>();
        grid._gridColumns = [column];
        return grid;
    }

    private static bool InvokeFilterCheck(BlgGrid<GridItem> grid, string methodName) =>
        (bool)typeof(BlgGrid<GridItem>)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(grid, null)!;

    private static void InvokeWithoutArguments(BlgGrid<GridItem> grid, string methodName) =>
        typeof(BlgGrid<GridItem>)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(grid, null);

    private sealed record GridItem(string Name);
}
