using Dnet.Blazor.Components.Overlay;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Services
{
    public class DnetOverlayInterop
    {

        private readonly IJSRuntime _jsRuntime;

        public DnetOverlayInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask<long> ActivateFocusTrap(ElementReference element, string? initialFocusSelector, bool restoreFocus)
        {
            return _jsRuntime.InvokeAsync<long>("dnetoverlay.activateFocusTrap", element, initialFocusSelector, restoreFocus);
        }

        public ValueTask DeactivateFocusTrap(long focusTrapId)
        {
            return _jsRuntime.InvokeVoidAsync("dnetoverlay.deactivateFocusTrap", focusTrapId);
        }

        public ValueTask<long> ObserveOverlaySize(
            ElementReference pane,
            ElementReference origin,
            DotNetObjectReference<DnetOverlayPane> callback)
        {
            return _jsRuntime.InvokeAsync<long>("dnetoverlay.observeOverlaySize", pane, origin, callback);
        }

        public ValueTask StopObservingOverlaySize(long observerId)
        {
            return _jsRuntime.InvokeVoidAsync("dnetoverlay.stopObservingOverlaySize", observerId);
        }

        public ValueTask<ViewportScrollPosition> GetViewportScrollPosition()
        {
            return _jsRuntime.InvokeAsync<ViewportScrollPosition>("dnetoverlay.getViewportScrollPosition");
        }

        public ValueTask<Models.Size> GetViewportSize()
        {
            return _jsRuntime.InvokeAsync<Models.Size>("dnetoverlay.getViewportSize");
        }

        public ValueTask<Models.Size> GetViewportSizeNoScroll()
        {
            return _jsRuntime.InvokeAsync<Models.Size>("dnetoverlay.getViewportSizeNoScroll");
        }

        public ValueTask<FlexibleConnectedPositionStrategyOrigin> GetBoundingClientRect(ElementReference element)
        {
            return _jsRuntime.InvokeAsync<FlexibleConnectedPositionStrategyOrigin>("dnetoverlay.getBoundingClientRect", element);
        }

        public ValueTask<FlexibleConnectedPositionStrategyOrigin> GetDocumentBoundingClientRect()
        {
            return _jsRuntime.InvokeAsync<FlexibleConnectedPositionStrategyOrigin>("dnetoverlay.getDocumentBoundingClientRect");
        }

        public ValueTask<int> GetDocumentClientHeight()
        {
            return _jsRuntime.InvokeAsync<int>("dnetoverlay.getDocumentClientHeight");
        }

        public ValueTask<int> GetDocumentClientWidth()
        {
            return _jsRuntime.InvokeAsync<int>("dnetoverlay.getDocumentClientWidth");
        }
        
        public ValueTask<int> GetWindowWidth()
        {
            return _jsRuntime.InvokeAsync<int>("dnetoverlay.getWindowWidth");
        }

		public ValueTask<int> GetContainerWidth(string containerId)
		{
			return _jsRuntime.InvokeAsync<int>("dnetoverlay.getContainerWidth", containerId);
		}
	}
}
