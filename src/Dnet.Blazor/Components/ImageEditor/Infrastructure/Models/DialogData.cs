using Dnet.Blazor.Components.ImageEditor.Infrastructure.Enums;

namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    /// <summary>Carries everything the editor dialog needs to render and export an image.</summary>
    public class DialogData
    {
        /// <summary>Gets or sets the image being edited.</summary>
        public MemoryStream imageFile { get; set; }

        /// <summary>Gets or sets the working image stream. Points at the same image as <see cref="imageFile"/>.</summary>
        public MemoryStream WorkingImageStream { get; set; }

        public int ImageContainerHeight { get; set; }

        public int ImageContainerWidth { get; set; }

        public int ImagePreviewHeight { get; set; }

        public int ImagePreviewWidth { get; set; }

        public int ModalDialogHeight { get; set; }

        public int ModalDialogWidth { get; set; }

        /// <summary>Gets or sets the largest image, in bytes, that the editor may read back from the browser.</summary>
        public long MaxFileSizes { get; set; }

        public List<string> AllowedFormats { get; set; }

        public List<ImageEditorControlType> ImageEditingControls { get; set; }

        /// <summary>Gets or sets the longest edge allowed in the exported image. Zero keeps the native resolution of the selection.</summary>
        public int MaxOutputDimension { get; set; }

        /// <summary>Gets or sets the format the editor must export. A null or empty value keeps the format of the source image.</summary>
        public string OutputFormat { get; set; }

        /// <summary>Gets or sets the quality used by lossy output formats, from 1 to 100.</summary>
        public int OutputQuality { get; set; } = 95;
    }
}
