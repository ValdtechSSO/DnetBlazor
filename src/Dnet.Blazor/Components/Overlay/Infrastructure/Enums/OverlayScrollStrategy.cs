namespace Dnet.Blazor.Components.Overlay.Infrastructure.Enums
{
    /// <summary>
    /// Defines how an overlay reacts while its document or an ancestor scrolls.
    /// </summary>
    public enum OverlayScrollStrategy
    {
        Noop,
        Reposition,
        Close,
        Block
    }
}
