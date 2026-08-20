namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models;

public sealed class OverlayOutsidePointerEventArgs
{
    public int? PointerDownOverlayId { get; set; }

    public int? TargetOverlayId { get; set; }
}
