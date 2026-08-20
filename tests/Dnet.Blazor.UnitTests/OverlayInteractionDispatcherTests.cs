using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.JSInterop;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class OverlayInteractionDispatcherTests
{
    [Fact]
    public async Task Starts_one_document_subscription_and_releases_it_when_stopped()
    {
        var jsRuntime = new InteractionJsRuntime();
        await using var dispatcher = new OverlayInteractionDispatcher(jsRuntime);

        dispatcher.Start();
        dispatcher.Start();
        await jsRuntime.WaitForAddAsync();

        dispatcher.Stop();
        await jsRuntime.WaitForRemoveAsync();

        Assert.Equal(1, jsRuntime.AddCount);
        Assert.Equal(1, jsRuntime.RemoveCount);
        Assert.Equal(jsRuntime.LastAddedSubscriptionId, jsRuntime.LastRemovedSubscriptionId);
    }

    [Fact]
    public void Routes_js_events_without_embedding_overlay_policy()
    {
        var dispatcher = new OverlayInteractionDispatcher(new InteractionJsRuntime());
        string? key = null;
        OverlayOutsidePointerEventArgs? pointer = null;
        var scrollEvents = 0;
        dispatcher.KeyDown += (_, args) => key = args.Key;
        dispatcher.OutsidePointer += (_, args) => pointer = args;
        dispatcher.DocumentScrolled += (_, _) => scrollEvents++;

        dispatcher.OnDocumentKeyDown(new OverlayKeyEventArgs { Key = "Escape" });
        dispatcher.OnOutsidePointer(new OverlayOutsidePointerEventArgs { PointerDownOverlayId = 2, TargetOverlayId = null });
        dispatcher.OnDocumentScrolled(new OverlayScrollEventArgs());

        Assert.Equal("Escape", key);
        Assert.NotNull(pointer);
        Assert.Equal(2, pointer!.PointerDownOverlayId);
        Assert.Equal(1, scrollEvents);
    }

    private sealed class InteractionJsRuntime : IJSRuntime
    {
        private readonly TaskCompletionSource _addCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _removeCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int AddCount { get; private set; }

        public int RemoveCount { get; private set; }

        public long LastAddedSubscriptionId { get; private set; }

        public long LastRemovedSubscriptionId { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            switch (identifier)
            {
                case "dnetoverlay.addInteractionEventListeners":
                    AddCount++;
                    LastAddedSubscriptionId = 42;
                    _addCompleted.TrySetResult();
                    return ValueTask.FromResult((TValue)(object)LastAddedSubscriptionId);
                case "dnetoverlay.removeInteractionEventListeners":
                    RemoveCount++;
                    LastRemovedSubscriptionId = Assert.IsType<long>(args![0]);
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
