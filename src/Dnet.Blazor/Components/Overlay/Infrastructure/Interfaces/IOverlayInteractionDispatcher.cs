using Dnet.Blazor.Components.Overlay.Infrastructure.Models;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;

public interface IOverlayInteractionDispatcher : IAsyncDisposable
{
    event EventHandler<OverlayKeyEventArgs>? KeyDown;

    event EventHandler<OverlayOutsidePointerEventArgs>? OutsidePointer;

    event EventHandler<OverlayScrollEventArgs>? DocumentScrolled;

    void Start();

    void Stop();

    void OnDocumentKeyDown(OverlayKeyEventArgs args);

    void OnOutsidePointer(OverlayOutsidePointerEventArgs args);

    void OnDocumentScrolled(OverlayScrollEventArgs args);
}
