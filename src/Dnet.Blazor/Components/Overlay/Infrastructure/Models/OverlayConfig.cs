using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Models
{
    public class OverlayConfig
    {
        public int OverlayReferenceId { get; set; }

        public string? PanelClass { get; set; }

        public string? PanelStyle { get; set; }

        public string? ThemeScope { get; set; }

        internal int? PanelZindex { get; set; }

        internal int? HostZindex { get; set; }

        public bool HasBackdrop { get; set; } = true;

        public bool HasTransparentBackdrop { get; set; }

        public bool DisableBackdropClick { get; set; }

        /// <summary>Closes a non-modal overlay when the user presses Escape.</summary>
        public bool CloseOnEscape { get; set; }

        /// <summary>Closes an overlay without a backdrop when the user clicks outside it.</summary>
        public bool CloseOnOutsidePointer { get; set; }

        /// <summary>Defines how the overlay reacts to document scrolling.</summary>
        public OverlayScrollStrategy ScrollStrategy { get; set; } = OverlayScrollStrategy.Noop;

        /// <summary>Allows a <see cref="OverlayScrollStrategy.Close"/> overlay to close on its own panel scroll.</summary>
        public bool CloseOnOverlayScroll { get; set; }

        /// <summary>Optional ARIA role applied to the overlay pane.</summary>
        public string? Role { get; set; }

        public string? AriaLabel { get; set; }

        public string? AriaLabelledBy { get; set; }

        public string? AriaDescribedBy { get; set; }

        /// <summary>Optional live-region politeness applied to the overlay pane.</summary>
        public string? AriaLive { get; set; }

        /// <summary>Indicates whether live-region changes should be announced atomically.</summary>
        public bool AriaAtomic { get; set; }

        public bool AriaModal { get; set; }

        public string? Direction { get; set; }

        /// <summary>Traps Tab navigation inside the pane while it is attached.</summary>
        public bool TrapFocus { get; set; }

        /// <summary>Restores the previously focused element when the pane closes.</summary>
        public bool RestoreFocus { get; set; } = true;

        /// <summary>Optional selector for the first element to focus inside a modal overlay.</summary>
        public string? InitialFocusSelector { get; set; }

        public string? BackdropClass { get; set; }

        public string? Width { get; set; }

        public string? Height { get; set; }

        public string? MinWidth { get; set; }

        public string? MinHeight { get; set; }

        public string? MaxWidth { get; set; }

        public string? MaxHeight { get; set; }

        public string? MarginTop { get; set; }

        public string? MarginBottom { get; set; }

        internal int? LastZindex { get; set; } = 0;

        internal ComponentType? ComponentType { get; set; }

        public PositionStrategy PositionStrategy { get; set; } = PositionStrategy.Global;

        public GlobalPositionStrategyBuilder GlobalPositionStrategy { get; set; } = new();

        public FlexibleConnectedPositionStrategyBuilder FlexibleConnectedPositionStrategyBuilder { get; set; } = new();
    }
}
