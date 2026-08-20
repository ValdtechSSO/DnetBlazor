using System.Threading;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ViewportRulerTests
{
    [Fact]
    public void AddDnetBlazor_registers_a_shared_viewport_ruler_per_scope()
    {
        var services = new ServiceCollection();

        services.AddDnetBlazor();

        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(IViewportRuler));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public async Task Subscriptions_share_one_listener_and_remove_it_after_the_last_unsubscribe()
    {
        var jsRuntime = new OverlayJsRuntime();
        await using var ruler = CreateRuler(jsRuntime);
        EventHandler<Size> first = (_, _) => { };
        EventHandler<Size> second = (_, _) => { };

        ruler.OnResized += first;
        ruler.OnResized += second;

        await jsRuntime.WaitForAddAsync();
        Assert.Equal(1, jsRuntime.AddCount);

        ruler.OnResized -= first;
        await Task.Yield();
        Assert.Equal(0, jsRuntime.RemoveCount);

        ruler.OnResized -= second;
        await jsRuntime.WaitForRemoveAsync();

        Assert.Equal(1, jsRuntime.RemoveCount);
        Assert.Equal(jsRuntime.LastAddedListenerId, jsRuntime.LastRemovedListenerId);
    }

    [Fact]
    public async Task Dispose_removes_the_listener_registered_before_disposal()
    {
        var jsRuntime = new OverlayJsRuntime();
        var ruler = CreateRuler(jsRuntime);
        EventHandler<Size> listener = (_, _) => { };
        ruler.OnResized += listener;

        await jsRuntime.WaitForAddAsync();
        await ruler.DisposeAsync();

        Assert.Equal(1, jsRuntime.RemoveCount);
        Assert.Equal(jsRuntime.LastAddedListenerId, jsRuntime.LastRemovedListenerId);
    }

    [Fact]
    public async Task Window_resize_updates_the_cache_and_notifies_subscribers()
    {
        var jsRuntime = new OverlayJsRuntime();
        await using var ruler = CreateRuler(jsRuntime);
        Size? notifiedSize = null;
        EventHandler<Size> listener = (_, size) => notifiedSize = size;
        ruler.OnResized += listener;
        await jsRuntime.WaitForAddAsync();

        ruler.OnWindowResized(new Size { Width = 1280, Height = 720 });

        Assert.NotNull(notifiedSize);
        Assert.Equal(1280, notifiedSize!.Width);
        Assert.Equal(720, notifiedSize.Height);
    }

    private static ViewportRuler CreateRuler(IJSRuntime jsRuntime) =>
        new(new DnetOverlayInterop(jsRuntime), jsRuntime);

    private sealed class OverlayJsRuntime : IJSRuntime
    {
        private readonly TaskCompletionSource _addCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _removeCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private long _nextListenerId;

        public int AddCount { get; private set; }

        public int RemoveCount { get; private set; }

        public long LastAddedListenerId { get; private set; }

        public long LastRemovedListenerId { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            switch (identifier)
            {
                case "dnetoverlay.addWindowEventListeners":
                    AddCount++;
                    LastAddedListenerId = ++_nextListenerId;
                    _addCompleted.TrySetResult();
                    return ValueTask.FromResult((TValue)(object)LastAddedListenerId);

                case "dnetoverlay.removeWindowEventListeners":
                    RemoveCount++;
                    LastRemovedListenerId = Assert.IsType<long>(args![0]);
                    _removeCompleted.TrySetResult();
                    return ValueTask.FromResult(default(TValue)!);

                default:
                    throw new InvalidOperationException($"Unexpected JavaScript interop call: {identifier}");
            }
        }

        public Task WaitForAddAsync() => _addCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        public Task WaitForRemoveAsync() => _removeCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }
}
