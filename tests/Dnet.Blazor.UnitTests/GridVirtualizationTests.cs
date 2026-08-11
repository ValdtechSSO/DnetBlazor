using System.Reflection;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridVirtualizationTests
{
    [Fact]
    public void Default_overscan_matches_current_blazor_virtualize_default()
    {
        var options = new GridOptions<GridItem>();

        Assert.Equal(15, options.OverscanCount);
    }

    [Theory]
    [InlineData(40, 15, 300)]
    [InlineData(40, 3, 60)]
    [InlineData(40, 0, 50)]
    public void Observer_margin_prefetches_before_the_overscan_buffer_is_exhausted(
        int rowHeight,
        int overscanCount,
        int expectedMargin)
    {
        var grid = new BlgGrid<GridItem>();
        SetPublicProperty(grid, nameof(BlgGrid<GridItem>.GridOptions), new GridOptions<GridItem>
        {
            RowHeight = rowHeight,
            OverscanCount = overscanCount
        });

        var margin = InvokePrivate<int>(grid, "GetVirtualizationRootMargin");

        Assert.Equal(expectedMargin, margin);
    }

    [Theory]
    [InlineData(100, 56, true)]
    [InlineData(112, 56, true)]
    [InlineData(113, 56, false)]
    [InlineData(100_000, 56, false)]
    public void Small_paged_sets_avoid_unnecessary_window_swaps(
        int itemCount,
        int visibleItemCapacity,
        bool expected)
    {
        var result = InvokePrivateStatic<bool>(
            typeof(BlgGrid<GridItem>),
            "FitsInTwoVirtualWindows",
            itemCount,
            visibleItemCapacity);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Initial_virtual_window_is_seeded_before_the_observer_runs()
    {
        var grid = new BlgGrid<GridItem>();
        SetPublicProperty(grid, nameof(BlgGrid<GridItem>.GridOptions), new GridOptions<GridItem>
        {
            UseVirtualization = true,
            OverscanCount = 3
        });
        var rows = Enumerable.Range(1, 25)
            .Select(index => new RowNode<GridItem>
            {
                RowNodeId = index,
                RowData = new GridItem(index)
            })
            .ToList();

        SetPrivateProperty(grid, "_renderedRowNodes", rows);
        InvokePrivate(grid, "SeedInitialVirtualWindow");

        var rendered = GetPrivateField<List<RowNode<GridItem>>>(grid, "_itemsToShow");
        Assert.Equal(7, rendered.Count);
        Assert.Equal(1, rendered[0].RowData!.Id);
        Assert.Equal(25, grid.GetDiagnostics().TotalItemCount);
    }

    [Fact]
    public void Repeated_source_instance_uses_unique_internal_render_keys()
    {
        var repeatedItem = new GridItem(1);
        var grid = new BlgGrid<GridItem>();
        SetPublicProperty(grid, nameof(BlgGrid<GridItem>.GridOptions), new GridOptions<GridItem>());
        grid._gridData = [repeatedItem, repeatedItem];

        var tree = InvokePrivate<TreeRowNode<GridItem>>(grid, "BuildRowNodes");
        var rows = tree.Children!.Select(child => child.Data!).ToList();

        Assert.All(rows, row => Assert.Null(row.StableKey));
        Assert.Equal(2, rows.Select(row => row.RowNodeId).Distinct().Count());
    }

    private static void SetPrivateProperty<TValue>(object instance, string name, TValue value) =>
        instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(instance, value);

    private static void SetPublicProperty<TValue>(object instance, string name, TValue value) =>
        instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)!.SetValue(instance, value);

    private static TValue GetPrivateField<TValue>(object instance, string name) =>
        (TValue)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;

    private static void InvokePrivate(object instance, string name) =>
        instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, null);

    private static TResult InvokePrivate<TResult>(object instance, string name) =>
        (TResult)instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, null)!;

    private static TResult InvokePrivateStatic<TResult>(Type type, string name, params object[] arguments) =>
        (TResult)type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!;

    private sealed record GridItem(int Id);
}
