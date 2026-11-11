using Bunit;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Models;
using Dnet.Blazor.Infrastructure.Services;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridHeaderMeasurementTests : BunitContext
{
    [Fact]
    public void Legacy_overscan_applies_only_to_the_grid_that_receives_it()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var sharedOptions = new GridOptions<GridItem> { OverscanCount = 15, UseVirtualization = false };

        var overridden = RenderGrid(sharedOptions, legacyOverscanCount: 4);
        var usingOptions = RenderGrid(sharedOptions);

        Assert.Equal(15, sharedOptions.OverscanCount);
        Assert.Equal(4, overridden.Instance.GetDiagnostics().OverscanCount);
        Assert.Equal(15, usingOptions.Instance.GetDiagnostics().OverscanCount);

#pragma warning disable CS0618
        overridden.Render(parameters => parameters.Add(grid => grid.OverscanCount, -1));
#pragma warning restore CS0618
        Assert.Equal(15, overridden.Instance.GetDiagnostics().OverscanCount);
    }

    [Fact]
    public void Simultaneous_grids_measure_their_own_headers_and_preserve_their_ids_on_refresh()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.Setup<ClientRectValues>("blginterop.getBoundingClientRect", _ => true)
            .SetResult(new ClientRectValues { Width = 600, Height = 400 });
        JSInterop.Setup<int>("blginterop.getHeaderWidth", _ => true).SetResult(724);

        var background = RenderGrid();
        var preview = RenderGrid();
        var backgroundId = background.Find(".blg-header-container").Id;
        var previewId = preview.Find(".blg-header-container").Id;

        Assert.False(string.IsNullOrEmpty(backgroundId));
        Assert.False(string.IsNullOrEmpty(previewId));
        Assert.NotEqual(backgroundId, previewId);
        var measurements = JSInterop.Invocations["blginterop.getHeaderWidth"];
        Assert.Contains(measurements, call => Equals(call.Arguments[0], backgroundId));
        Assert.Contains(measurements, call => Equals(call.Arguments[0], previewId));
        Assert.DoesNotContain(measurements, call => Equals(call.Arguments[0], "blgHeaderConatiner"));

        preview.Render(parameters => parameters.Add(component => component.GridData,
            new[] { new GridItem("Updated resource") }));

        Assert.Equal(previewId, preview.Find(".blg-header-container").Id);
        Assert.Equal(backgroundId, background.Find(".blg-header-container").Id);
    }

    private IRenderedComponent<BlgGrid<GridItem>> RenderGrid(GridOptions<GridItem>? options = null, int legacyOverscanCount = -1) =>
        Render<BlgGrid<GridItem>>(parameters => parameters
            .Add(component => component.GridData, new[] { new GridItem("Resource") })
            .Add(component => component.GridColumns, new List<GridColumn<GridItem>>
            {
                new()
                {
                    ColumnId = 1,
                    DataField = nameof(GridItem.Name),
                    HeaderName = "Resource",
                    Width = 724
                }
            })
            .Add(component => component.GridOptions, options ?? new GridOptions<GridItem> { UseVirtualization = false })
#pragma warning disable CS0618
            .Add(component => component.OverscanCount, legacyOverscanCount));
#pragma warning restore CS0618

    private sealed record GridItem(string Name);
}
