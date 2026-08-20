using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Microsoft.JSInterop;
using System.Threading;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Services
{
    public class ViewportRuler : IViewportRuler, IDisposable, IAsyncDisposable
    {
        private readonly DnetOverlayInterop _dnetOverlayInterop;
        private readonly IJSRuntime _jsRuntime;
        private EventHandler<Models.Size>? _onResized;
        private DotNetObjectReference<ViewportRuler>? _selfReference;
        private readonly SemaphoreSlim _listenerGate = new(1, 1);
        private readonly object _subscriptionGate = new();
        private Task _listenerSynchronization = Task.CompletedTask;
        private long? _listenerId;
        private bool _listenersAttached;
        private bool _manualListenersRequested;

        private bool _disposed;

        public event EventHandler<Models.Size> OnResized
        {
            add => Subscribe(value);
            remove => Unsubscribe(value);
        }

        private Models.Size? _viewportSize;

        public ViewportRuler(DnetOverlayInterop dnetOverlayInterop, IJSRuntime jsRuntime)
        {
            _dnetOverlayInterop = dnetOverlayInterop;
            _jsRuntime = jsRuntime;
        }

        public async Task<Models.Size> GetViewportSize()
        {
            if (_viewportSize is null)
            {
                await UpdateViewportSize();
            }

            return _viewportSize!;
        }

        public async Task<ClientRect> GetViewportRect()
        {
            // Use the document element's bounding rect rather than the window scroll properties
            // (e.g. pageYOffset, scrollY) due to in issue in Chrome and IE where window scroll
            // properties and client coordinates (boundingClientRect, clientX/Y, etc.) are in different
            // conceptual viewports. Under most circumstances these viewports are equivalent, but they
            // can disagree when the page is pinch-zoomed (on devices that support touch).
            // We use the documentElement instead of the body because, by default (without a css reset)
            // browsers typically give the document body an 8px margin, which is not included in
            // getBoundingClientRect().
            var scrollPosition = await GetViewportScrollPosition();

            var viewportSize = await GetViewportSize();

            var clientRect = new ClientRect()
            {
                Top = scrollPosition.Top,
                Left = scrollPosition.Left,
                Bottom = scrollPosition.Top + viewportSize.Height,
                Right = scrollPosition.Left + viewportSize.Width,
                Height = viewportSize.Height,
                Width = viewportSize.Width
            };

            return clientRect;
        }

        public async Task<ClientRect> GetViewportRectNoScroll(int viewportMargin)
        {
            // We recalculate the viewport rect here ourselves, rather than using the ViewportRuler,
            // because we want to use the `clientWidth` and `clientHeight` as the base. The difference
            // being that the client properties don't include the scrollbar, as opposed to `innerWidth`
            // and `innerHeight` that do. This is necessary, because the overlay container uses
            // 100% `width` and `height` which don't include the scrollbar either.

            var scrollPosition = await GetViewportScrollPosition();

            var viewportSizeNoScroll = await ViewportSizeNoScroll();

            var viewportRect = new ClientRect
            {
                Top = scrollPosition.Top + viewportMargin,
                Left = scrollPosition.Left + viewportMargin,
                Right = scrollPosition.Left + viewportSizeNoScroll.Width - viewportMargin,
                Bottom = scrollPosition.Top + viewportSizeNoScroll.Height - viewportMargin,
                Width = viewportSizeNoScroll.Width - (2 * viewportMargin),
                Height = viewportSizeNoScroll.Height - (2 * viewportMargin)
            };

            return viewportRect;
        }

        /** Gets the (top, left) scroll position of the viewport. */
        public async Task<ViewportScrollPosition> GetViewportScrollPosition()
        {
            // The top-left-corner of the viewport is determined by the scroll position of the document
            // body, normally just (scrollLeft, scrollTop). However, Chrome and Firefox disagree about
            // whether `document.body` or `document.documentElement` is the scrolled element, so reading
            // `scrollTop` and `scrollLeft` is inconsistent. However, using the bounding rect of
            // `document.documentElement` works consistently, where the `top` and `left` values will
            // equal negative the scroll position.

            var position = await _dnetOverlayInterop.GetViewportScrollPosition();

            return position;
        }

        private async Task<Models.Size> ViewportSizeNoScroll()
        {
           return await _dnetOverlayInterop.GetViewportSizeNoScroll();
        }

        private async Task UpdateViewportSize()
        {
            var viewportSize = await _dnetOverlayInterop.GetViewportSize();

            _viewportSize = viewportSize;
        }

        [JSInvokable]
        public void OnWindowResized(Models.Size size)
        {
            _viewportSize = size;
            _onResized?.Invoke(this, size);
        }

        private void Unsubscribe(EventHandler<Models.Size> value)
        {
            lock (_subscriptionGate)
            {
                _onResized -= value;
            }

            RequestListenerSynchronization();
        }

        private void Subscribe(EventHandler<Models.Size> value)
        {
            lock (_subscriptionGate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _onResized += value;
            }

            RequestListenerSynchronization();
        }

        public async ValueTask<bool> AddWindowEventListeners()
        {
            lock (_subscriptionGate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _manualListenersRequested = true;
            }

            await SynchronizeWindowEventListenersAsync();
            return _listenersAttached;
        }

        public async ValueTask RemoveWindowEventListeners()
        {
            lock (_subscriptionGate)
            {
                _manualListenersRequested = false;
            }

            await SynchronizeWindowEventListenersAsync();
        }

        private void RequestListenerSynchronization()
        {
            _listenerSynchronization = SynchronizeWindowEventListenersAsync().AsTask();
            _ = ObserveBackgroundSynchronizationAsync(_listenerSynchronization);
        }

        private static async Task ObserveBackgroundSynchronizationAsync(Task synchronization)
        {
            try
            {
                await synchronization.ConfigureAwait(false);
            }
            catch (JSDisconnectedException)
            {
                // A disconnected browser cannot retain a listener.
            }
            catch (ObjectDisposedException)
            {
                // The service may be disposed while an event subscription is being changed.
            }
        }

        private bool ShouldListen()
        {
            lock (_subscriptionGate)
            {
                return _manualListenersRequested || _onResized is not null;
            }
        }

        private async ValueTask SynchronizeWindowEventListenersAsync()
        {
            await _listenerGate.WaitAsync();
            try
            {
                if (ShouldListen() && !_disposed && !_listenersAttached)
                {
                    _selfReference ??= DotNetObjectReference.Create(this);
                    _listenerId = await _jsRuntime.InvokeAsync<long>("dnetoverlay.addWindowEventListeners", _selfReference);
                    _listenersAttached = _listenerId is > 0;
                }

                // The desired state can change while the JavaScript registration is awaited.
                // Re-evaluate it while holding the gate so a late registration is immediately removed.
                if (!ShouldListen() || _disposed)
                {
                    if (_listenersAttached && _listenerId is long listenerId)
                    {
                        try
                        {
                            await _jsRuntime.InvokeVoidAsync("dnetoverlay.removeWindowEventListeners", listenerId);
                        }
                        catch (JSDisconnectedException)
                        {
                            // The browser is gone, so its event listeners no longer matter.
                        }
                    }

                    _listenersAttached = false;
                    _listenerId = null;
                    _selfReference?.Dispose();
                    _selfReference = null;
                }
            }
            finally
            {
                _listenerGate.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_subscriptionGate)
            {
                _disposed = true;
                _manualListenersRequested = false;
                _onResized = null;
            }

            RequestListenerSynchronization();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed && _selfReference is null)
            {
                return;
            }

            lock (_subscriptionGate)
            {
                _onResized = null;
                _disposed = true;
                _manualListenersRequested = false;
            }

            await SynchronizeWindowEventListenersAsync();
            GC.SuppressFinalize(this);
        }
    }

}
