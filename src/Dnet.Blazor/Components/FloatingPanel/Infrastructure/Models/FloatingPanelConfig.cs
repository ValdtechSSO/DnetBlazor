using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Enums;

namespace Dnet.Blazor.Components.FloatingPanel.Infrastructure.Models
{
    public class FloatingPanelConfig
    {
        public string? FloatingPanelClass { get; set; }

        public string? PanelClass { get; set; }

        public string? PanelStyle { get; set; }

        public string? ThemeScope { get; set; }

        public bool HasBackdrop { get; set; } = true;

        public bool HasTransparentBackdrop { get; set; }

        public bool DisableBackdropClick { get; set; }

        public bool CloseOnEscape { get; set; } = true;

        public bool CloseOnOutsidePointer { get; set; } = true;

        /// <summary>Optional ARIA role applied to the panel overlay.</summary>
        public string? Role { get; set; }

        /// <summary>Optional accessible name applied to the panel overlay.</summary>
        public string? AriaLabel { get; set; }

        /// <summary>Optional id of the element that labels the panel overlay.</summary>
        public string? AriaLabelledBy { get; set; }

        /// <summary>Optional id of the element that describes the panel overlay.</summary>
        public string? AriaDescribedBy { get; set; }

        /// <summary>Optional live-region politeness applied to the panel overlay.</summary>
        public string? AriaLive { get; set; }

        /// <summary>Indicates whether live-region changes should be announced atomically.</summary>
        public bool AriaAtomic { get; set; }

        /// <summary>Traps focus while the panel is open. Keep false for non-modal panels.</summary>
        public bool TrapFocus { get; set; }

        /// <summary>Restores focus when a focus-trapping panel closes.</summary>
        public bool RestoreFocus { get; set; } = true;

        /// <summary>Optional selector for the initial focused element in a focus-trapping panel.</summary>
        public string? InitialFocusSelector { get; set; }

        public string? BackdropClass { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int Margin { get; set; } = 0;

        public FloatingPanelPostion Postion { get; set; } = FloatingPanelPostion.BottomRight;

        public int? OffsetLeft { get; set; } = 0;

        public int? OffsetRight { get; set; } = 0;

        public int? OffsetTop { get; set; } = 0;

        public int? OffsetBottom { get; set; } = 0;
    }
}
