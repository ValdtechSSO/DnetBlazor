using System.Reflection;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridVirtualizationTests
{
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

    private sealed record GridItem(int Id);
}
