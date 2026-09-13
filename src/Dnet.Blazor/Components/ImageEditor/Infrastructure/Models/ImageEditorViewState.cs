namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    /// <summary>Source geometry, zoom and selection reported after an operation that changes the view.</summary>
    public class ImageEditorViewState
    {
        /// <summary>Gets or sets the width of the working image in source pixels.</summary>
        public int SourceWidth { get; set; }

        /// <summary>Gets or sets the height of the working image in source pixels.</summary>
        public int SourceHeight { get; set; }

        /// <summary>Gets or sets the zoom the picture is displayed with.</summary>
        public double Zoom { get; set; } = 1;

        /// <summary>Gets or sets the selection the crop box frames.</summary>
        public DraggedData Selection { get; set; } = new();
    }
}
