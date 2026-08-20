namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models;

public sealed class OverlayScrollEventArgs : EventArgs
{
    /// <summary>The overlay containing the scroll target, when any.</summary>
    public int? SourceOverlayId { get; set; }
}
