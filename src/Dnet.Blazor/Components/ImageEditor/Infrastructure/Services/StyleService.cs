using System.Globalization;
using Dnet.Blazor.Infrastructure.Services.CssBuilder;

namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Services
{
    public class StyleService : IStyleService
    {
        public string GetStyles(double left, double top, double height, double width)
        {
            var styles = new StyleBuilder("--_crop-left", $"{left.ToString(CultureInfo.InvariantCulture)}px")
                .AddStyle("--_crop-top", $"{top.ToString(CultureInfo.InvariantCulture)}px")
                .AddStyle("--_crop-width", $"{width.ToString(CultureInfo.InvariantCulture)}px")
                .AddStyle("--_crop-height", $"{height.ToString(CultureInfo.InvariantCulture)}px")
                .Build();

            return styles;
        }

        public string GetCropContainerStyles(int height, int width)
        {
            var styles = new StyleBuilder("--_editor-max-width", $"{width}px")
                .AddStyle("--_editor-max-height", $"{height}px")
                .Build();

            return styles;
        }

        public string GetImagePreviewStyles(int height, int width)
        {
            var styles = new StyleBuilder("--_preview-max-width", $"{width}px")
                .AddStyle("--_preview-max-height", $"{height}px")
                .Build();

            return styles;
        }
    }
}
