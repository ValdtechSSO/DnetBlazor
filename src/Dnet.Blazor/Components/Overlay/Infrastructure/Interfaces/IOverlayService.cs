using System;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces
{
    public interface IOverlayService
    {
        event Action OnBackdropClicked;

        event Action<int> OnPositionUpdate;

        event Action<int> OnConfigurationUpdate;

        OverlayReference Attach(RenderFragment overlayContent, OverlayConfig overlayConfig);

        void Detach(OverlayResult overlayDataResult);

        void BackdropClicked(OverlayResult overlayDataResult);

        void RequestPositionUpdate(int overlayReferenceId);

        void RequestConfigurationUpdate(int overlayReferenceId);
    }
}
