namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Models
{
    public class DraggedData
    {
        public double Top { get; set; } = 0.0;

        public double Left { get; set; } = 0.0;

        public double Height { get; set; } = 0.0;

        public double Width { get; set; } = 0.0;

        /// <summary>Gets or sets the width of the selection in source pixels, which is the size the export will have.</summary>
        public double OutputWidth { get; set; } = 0.0;

        /// <summary>Gets or sets the height of the selection in source pixels, which is the size the export will have.</summary>
        public double OutputHeight { get; set; } = 0.0;
    }
}
