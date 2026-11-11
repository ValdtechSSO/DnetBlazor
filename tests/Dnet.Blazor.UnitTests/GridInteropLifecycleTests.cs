using System.Reflection;
using Bunit;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Models;
using Dnet.Blazor.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridInteropLifecycleTests : BunitContext
{
    [Fact]
    public async Task Prerendered_grid_disposes_without_calling_javascript()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Strict;
        services.AddSingleton(JSInterop.JSRuntime);

        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BlgGrid<string>.GridData)] = new[] { "Record" },
                [nameof(BlgGrid<string>.GridColumns)] = new List<GridColumn<string>>(),
                [nameof(BlgGrid<string>.GridOptions)] = new GridOptions<string> { UseVirtualization = false }
            });
            var result = await renderer.RenderComponentAsync<BlgGrid<string>>(parameters);
            Assert.Contains("role=\"grid\"", result.ToHtmlString());
        });
    }

    [Fact]
    public async Task Interactive_grid_removes_its_registered_listeners_on_disposal()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Render<BlgGrid<string>>(parameters => parameters
            .Add(grid => grid.GridData, new[] { "Record" })
            .Add(grid => grid.GridColumns, new List<GridColumn<string>>())
            .Add(grid => grid.GridOptions, new GridOptions<string> { UseVirtualization = false }));

        var windowListener = Assert.Single(JSInterop.Invocations["blginterop.addWindowEventListeners"]);
        var touchListener = Assert.Single(JSInterop.Invocations["blginterop.addTouchListeners"]);

        await DisposeComponentsAsync();

        Assert.Collection(JSInterop.Invocations["blginterop.removeEventListeners"],
            removal => Assert.Equal(windowListener.Arguments[0], removal.Arguments[0]),
            removal => Assert.Equal(touchListener.Arguments[0], removal.Arguments[0]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Virtualized_grid_disposes_its_dotnet_reference_even_when_javascript_disconnects(bool disconnected)
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
        if (disconnected)
        {
            JSInterop.SetupVoid("blginterop.dispose", _ => true)
                .SetException(new JSDisconnectedException("The circuit disconnected."));
        }

        Render<BlgGrid<string>>(parameters => parameters
            .Add(grid => grid.GridData, new[] { "Record" })
            .Add(grid => grid.GridColumns, new List<GridColumn<string>>())
            .Add(grid => grid.GridOptions, new GridOptions<string> { UseVirtualization = true }));
        var reference = Assert.Single(JSInterop.Invocations["blginterop.init"]).Arguments[0]!;
        // The reference wraps the Grid's internal interop type.
        var value = reference.GetType().GetProperty(nameof(DotNetObjectReference<object>.Value))!;
        Assert.NotNull(value.GetValue(reference));

        await DisposeComponentsAsync();

        Assert.Same(reference, Assert.Single(JSInterop.Invocations["blginterop.dispose"]).Arguments[0]);
        var error = Assert.Throws<TargetInvocationException>(() => value.GetValue(reference));
        Assert.IsType<ObjectDisposedException>(error.InnerException);
    }
}
