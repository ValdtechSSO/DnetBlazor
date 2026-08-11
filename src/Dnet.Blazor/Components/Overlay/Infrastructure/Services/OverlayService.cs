using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Services
{

    public class OverlayService : IOverlayService
    {
        public event Action<RenderFragment, OverlayConfig> OnAttach;

        public event Action<OverlayResult> OnDetach;

        public event Action OnBackdropClicked;

        private readonly object _syncRoot = new();
        private readonly List<OverlayReference> _overlayReferences = new();
        private int _sequenceNumber;

        public OverlayReference Attach(RenderFragment overlayContent, OverlayConfig overlayConfig)
        {
            ArgumentNullException.ThrowIfNull(overlayContent);
            ArgumentNullException.ThrowIfNull(overlayConfig);

            var overlayReference = new OverlayReference(Interlocked.Increment(ref _sequenceNumber));

            lock (_syncRoot)
            {
                _overlayReferences.Add(overlayReference);
            }

            overlayConfig.OverlayReferenceId = overlayReference.OverlayReferenceId;

            OnAttach?.Invoke(overlayContent, overlayConfig);

            return overlayReference;
        }

        public void Detach(OverlayResult overlayDataResult)
        {
            ArgumentNullException.ThrowIfNull(overlayDataResult);

            OverlayReference? item;
            lock (_syncRoot)
            {
                item = _overlayReferences.Find(p => p.OverlayReferenceId == overlayDataResult.OverlayReferenceId);
                if (item is null)
                {
                    return;
                }

                _overlayReferences.Remove(item);
            }

            OnDetach?.Invoke(overlayDataResult);

            item.CloseOverlayReference(overlayDataResult);
        }

        public void BackdropClicked(OverlayResult overlayDataResult)
        {
            Detach(overlayDataResult);
        }
    }
}
