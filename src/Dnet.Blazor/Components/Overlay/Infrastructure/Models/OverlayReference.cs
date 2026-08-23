using System;
using System.Collections.Generic;
using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models
{
    public class OverlayReference : IDisposable, IAsyncDisposable
    {
        public event Action<OverlayResult> Close;

        public event Action? Attached;

        public event Action<OverlayResult>? Detached;

        internal int OverlayReferenceId { get; set; }

        private readonly Action<OverlayResult>? _detach;

        private readonly Action<int>? _requestPositionUpdate;

        private readonly Action<int>? _requestConfigurationUpdate;

        private Action<Type, IDictionary<string, object>>? _updateContent;

        public bool IsAttached { get; private set; }

        public OverlayConfig? Config { get; private set; }

        public OverlayReference(int overlayReferenceId)
        {
            OverlayReferenceId = overlayReferenceId;
        }

        internal OverlayReference(
            int overlayReferenceId,
            OverlayConfig config,
            Action<OverlayResult> detach,
            Action<int> requestPositionUpdate,
            Action<int> requestConfigurationUpdate)
            : this(overlayReferenceId)
        {
            Config = config;
            _detach = detach;
            _requestPositionUpdate = requestPositionUpdate;
            _requestConfigurationUpdate = requestConfigurationUpdate;
        }

        internal void MarkAttached()
        {
            if (IsAttached)
            {
                return;
            }

            IsAttached = true;
            Attached?.Invoke();
        }

        internal void CloseOverlayReference(OverlayResult overlayDataResult)
        {
            if (!IsAttached)
            {
                return;
            }

            IsAttached = false;
            Close?.Invoke(overlayDataResult);
            Detached?.Invoke(overlayDataResult);
        }

        public int GetOverlayReferenceId()
        {
            return OverlayReferenceId;
        }

        public void Detach(CloseReason closeReason = CloseReason.Cancel)
        {
            if (!IsAttached)
            {
                return;
            }

            _detach?.Invoke(new OverlayResult
            {
                OverlayReferenceId = OverlayReferenceId,
                CloseReason = closeReason
            });
        }

        public ValueTask DetachAsync(CloseReason closeReason = CloseReason.Cancel)
        {
            Detach(closeReason);
            return ValueTask.CompletedTask;
        }

        public void RequestPositionUpdate()
        {
            if (IsAttached)
            {
                _requestPositionUpdate?.Invoke(OverlayReferenceId);
            }
        }

        /// <summary>
        /// Updates the supplied sizing constraints and refreshes the attached pane.
        /// </summary>
        public void UpdateSize(OverlaySize size)
        {
            ArgumentNullException.ThrowIfNull(size);

            if (!IsAttached || Config is null)
            {
                return;
            }

            Config.Width = size.Width ?? Config.Width;
            Config.Height = size.Height ?? Config.Height;
            Config.MinWidth = size.MinWidth ?? Config.MinWidth;
            Config.MinHeight = size.MinHeight ?? Config.MinHeight;
            Config.MaxWidth = size.MaxWidth ?? Config.MaxWidth;
            Config.MaxHeight = size.MaxHeight ?? Config.MaxHeight;
            _requestConfigurationUpdate?.Invoke(OverlayReferenceId);
        }

        /// <summary>
        /// Replaces the dynamic component and parameters rendered by an overlay that supports
        /// live content updates. Returns <see langword="false"/> for other overlay types.
        /// </summary>
        public bool UpdateContent(
            Type componentType,
            IDictionary<string, object> parameters)
        {
            ArgumentNullException.ThrowIfNull(componentType);
            ArgumentNullException.ThrowIfNull(parameters);

            if (!IsAttached || _updateContent is null)
            {
                return false;
            }

            _updateContent(componentType, parameters);
            _requestConfigurationUpdate?.Invoke(OverlayReferenceId);
            return true;
        }

        internal void SetContentUpdater(
            Action<Type, IDictionary<string, object>> updateContent) =>
            _updateContent = updateContent ?? throw new ArgumentNullException(nameof(updateContent));

        public void Dispose() => Detach();

        public ValueTask DisposeAsync() => DetachAsync();
    }
}
