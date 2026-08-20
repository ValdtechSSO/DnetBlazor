namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models;

public sealed class OverlayKeyEventArgs
{
    public string Key { get; set; } = string.Empty;

    public bool DefaultPrevented { get; set; }
}
