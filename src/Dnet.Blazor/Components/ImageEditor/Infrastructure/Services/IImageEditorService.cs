using System.Collections.Generic;
using Dnet.Blazor.Components.ImageEditor.Infrastructure.Models;

namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Services
{
    public interface IImageEditorService
    {
        /// <summary>Returns the eight handles of the crop box. Their geometry lives in the component stylesheet.</summary>
        List<ResizerData> InitializeResizers();
    }
}
