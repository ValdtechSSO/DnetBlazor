namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    /// <summary>Describes the image the browser has already encoded and kept ready for the caller.</summary>
    public class ImageEditorExportResult
    {
        /// <summary>Gets or sets the width of the encoded image, in pixels.</summary>
        public int Width { get; set; }

        /// <summary>Gets or sets the height of the encoded image, in pixels.</summary>
        public int Height { get; set; }

        /// <summary>Gets or sets the MIME type the image was encoded with.</summary>
        public string Format { get; set; }

        /// <summary>Gets or sets the size of the encoded image, in bytes.</summary>
        public long Size { get; set; }
    }
}
