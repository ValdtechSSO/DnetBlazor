namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    /// <summary>Source and display geometry reported by the browser once the image has been decoded.</summary>
    public class ImageEditorSourceData
    {
        /// <summary>Gets or sets the width of the image in source pixels.</summary>
        public int SourceWidth { get; set; }

        /// <summary>Gets or sets the height of the image in source pixels.</summary>
        public int SourceHeight { get; set; }

        /// <summary>Gets or sets the detected MIME type of the source image.</summary>
        public string SourceFormat { get; set; }

        /// <summary>Gets or sets the width the image is displayed at.</summary>
        public int DisplayWidth { get; set; }

        /// <summary>Gets or sets the height the image is displayed at.</summary>
        public int DisplayHeight { get; set; }

        /// <summary>Gets or sets the zoom factor the picture is displayed with, where 1 shows one source pixel per CSS pixel.</summary>
        public double Zoom { get; set; } = 1;

        /// <summary>Gets or sets the initial left offset of the crop box, in display pixels.</summary>
        public double CropLeft { get; set; }

        /// <summary>Gets or sets the initial top offset of the crop box, in display pixels.</summary>
        public double CropTop { get; set; }

        /// <summary>Gets or sets the initial width of the crop box, in display pixels.</summary>
        public double CropWidth { get; set; }

        /// <summary>Gets or sets the initial height of the crop box, in display pixels.</summary>
        public double CropHeight { get; set; }
    }
}
