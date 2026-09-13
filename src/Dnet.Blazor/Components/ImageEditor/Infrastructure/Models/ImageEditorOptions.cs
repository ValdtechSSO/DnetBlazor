namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    /// <summary>Options the editor sends to the browser when the image is decoded.</summary>
    public class ImageEditorOptions
    {
        /// <summary>Gets or sets the smallest width the crop box may have, in display pixels.</summary>
        public double MinCropWidth { get; set; } = 50;

        /// <summary>Gets or sets the smallest height the crop box may have, in display pixels.</summary>
        public double MinCropHeight { get; set; } = 50;

        /// <summary>Gets or sets the width of the preview box, in pixels.</summary>
        public int PreviewWidth { get; set; }

        /// <summary>Gets or sets the height of the preview box, in pixels.</summary>
        public int PreviewHeight { get; set; }

        /// <summary>Gets or sets the longest edge allowed in the exported image. Zero keeps the native resolution.</summary>
        public int MaxOutputDimension { get; set; }

        /// <summary>Gets or sets the format the export must use. A null or empty value keeps the source format.</summary>
        public string OutputFormat { get; set; }

        /// <summary>Gets or sets the quality used by lossy output formats, from 1 to 100.</summary>
        public int OutputQuality { get; set; } = 95;
    }
}
