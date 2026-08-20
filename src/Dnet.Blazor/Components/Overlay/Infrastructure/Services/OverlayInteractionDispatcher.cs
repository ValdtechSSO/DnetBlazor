using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Microsoft.JSInterop;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Services;

/// <summary>
/// Owns the single document-level keyboard, pointer and scroll subscription for the current circuit.
/// It deliberately routes raw interaction only; overlay policy stays in <see cref="DnetOverlay"/>.
/// </summary>
public sealed class OverlayInteractionDispatcher : IOverlayInteractionDispatcher
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DotNetObjectReference<OverlayInteractionDispatcher>? _selfReference;
    private Task _synchronization = Task.CompletedTask;
    private long? _subscriptionId;
    private bool _requested;
    private bool _disposed;

    public OverlayInteractionDispatcher(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public event EventHandler<OverlayKeyEventArgs>? KeyDown;

    public event EventHandler<OverlayOutsidePointerEventArgs>? OutsidePointer;

    public event EventHandler<OverlayScrollEventArgs>? DocumentScrolled;

    public void Start()
    {
        if (_disposed)
        {
            return;
        }

        _requested = true;
        RequestSynchronization();
    }

    public void Stop()
    {
        _requested = false;
        RequestSynchronization();
    }

    [JSInvokable]
    public void OnDocumentKeyDown(OverlayKeyEventArgs args) => KeyDown?.Invoke(this, args);

    [JSInvokable]
    public void OnOutsidePointer(OverlayOutsidePointerEventArgs args) => OutsidePointer?.Invoke(this, args);

    [JSInvokable]
    public void OnDocumentScrolled(OverlayScrollEventArgs args) => DocumentScrolled?.Invoke(this, args);

    private void RequestSynchronization()
    {
        _synchronization = SynchronizeAsync().AsTask();
        _ = ObserveAsync(_synchronization);
    }

    private static async Task ObserveAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // The document no longer exists, so the listener cannot outlive the circuit.
        }
        catch (ObjectDisposedException)
        {
            // Disposal can race a UI event subscription.
        }
    }

    private async ValueTask SynchronizeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_requested && !_disposed && _subscriptionId is null)
            {
                _selfReference ??= DotNetObjectReference.Create(this);
                _subscriptionId = await _jsRuntime.InvokeAsync<long>("dnetoverlay.addInteractionEventListeners", _selfReference);
            }

            if ((!_requested || _disposed) && _subscriptionId is long subscriptionId)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("dnetoverlay.removeInteractionEventListeners", subscriptionId);
                }
                catch (JSDisconnectedException)
                {
                    // See ObserveAsync.
                }

                _subscriptionId = null;
                _selfReference?.Dispose();
                _selfReference = null;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed && _subscriptionId is null)
        {
            return;
        }

        _disposed = true;
        _requested = false;
        KeyDown = null;
        OutsidePointer = null;
        DocumentScrolled = null;
        await SynchronizeAsync();
        GC.SuppressFinalize(this);
    }
}
