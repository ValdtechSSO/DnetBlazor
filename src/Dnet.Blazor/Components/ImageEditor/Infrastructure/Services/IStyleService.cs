namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Services
{
    /// <summary>
    /// Builds the inline styles the image editor needs. Crop geometry is written as
    /// custom properties (<c>--_crop-*</c>) rather than as finished <c>top/left</c>
    /// values: the stylesheet derives the box, the masks and the resizers from them,
    /// and the interop layer rewrites the very same four properties on every pointer
    /// frame, so a gesture never needs a round trip to .NET.
    /// </summary>
    public interface IStyleService
    {
        /// <summary>Returns the crop box geometry for the given rectangle, in display pixels.</summary>
        string GetStyles(double left, double top, double height, double width);

        /// <summary>Returns the styles that bound the box hosting the image and the crop overlay.</summary>
        string GetCropContainerStyles(int height, int width);

        /// <summary>Returns the styles that bound the preview box.</summary>
        string GetImagePreviewStyles(int height, int width);
    }
}
