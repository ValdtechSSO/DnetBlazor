namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models;

/// <summary>
/// A partial size update for an attached overlay.
/// Only non-null values replace the current configuration.
/// </summary>
public sealed class OverlaySize
{
    public string? Width { get; init; }

    public string? Height { get; init; }

    public string? MinWidth { get; init; }

    public string? MinHeight { get; init; }

    public string? MaxWidth { get; init; }

    public string? MaxHeight { get; init; }
}
