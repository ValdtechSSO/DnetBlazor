using System.Collections.Generic;
using Dnet.Blazor.Components.ImageEditor.Infrastructure.Constants;
using Dnet.Blazor.Components.ImageEditor.Infrastructure.Models;

namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Services
{
    public class ImageEditorService : IImageEditorService
    {
        public List<ResizerData> InitializeResizers()
        {
            var resizerHeight = 8;
            var resizerWidth = 8;

            var resizerData = new List<ResizerData>
            {
                new ResizerData
                {
                    ResizerType = ResizerType.TopCenter,
                    Cursor = "ns-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.BottomCenter,
                    Cursor = "ns-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.LeftCenter,
                    Cursor = "ew-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.RightCenter,
                    Cursor = "ew-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.TopLeft,
                    Cursor = "nwse-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.TopRight,
                    Cursor = "nesw-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.BottomLeft,
                    Cursor = "nesw-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                },
                new ResizerData
                {
                    ResizerType = ResizerType.BottomRight,
                    Cursor = "nwse-resize",
                    Height = resizerHeight,
                    Width = resizerWidth,
                }
            };

            return resizerData;
        }
    }
}
