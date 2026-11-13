namespace Dnet.Blazor.Components.SlideToggle;

/// <summary>Defines how a slide toggle paints its track and handle.</summary>
public enum DnetSlideToggleVariant
{
    /// <summary>The standard switch: a solid track and a handle inside it.</summary>
    Standard,

    /// <summary>
    /// A tinted track and a clear lens handle a little taller than the track. Its
    /// <c>--dnet-slide-toggle-lens-*</c> tokens fall back to the standard ones, so it
    /// looks like <see cref="Standard"/> until a theme paints it, as the Liquid Glass
    /// themes do.
    /// </summary>
    Lens
}
