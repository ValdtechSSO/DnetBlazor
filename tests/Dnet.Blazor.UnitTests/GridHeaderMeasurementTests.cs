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

    private IRenderedComponent<BlgGrid<GridItem>> RenderGrid() =>
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
            .Add(component => component.GridOptions, new GridOptions<GridItem>
            {
                UseVirtualization = false
            }));

    private sealed record GridItem(string Name);
}
