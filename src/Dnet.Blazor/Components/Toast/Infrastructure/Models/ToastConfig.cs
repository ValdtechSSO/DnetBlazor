using Dnet.Blazor.Components.Toast.Infrastructure.Enums;
using Dnet.Blazor.Components.Toast;

namespace Dnet.Blazor.Components.Toast.Infrastructure.Models
{
    public class ToastConfig
    {
        private int? _duration;

        internal bool DurationWasSet { get; private set; }

        public string? Title { get; set; }

        public string? Text { get; set; }

        public ToastType ToastType { get; set; }

        public string? ToastTypeIconClass { get; set; }

        public string? ToastCloseIconClass { get; set; }

        public string? ToastTypeColor { get; set; }

        public string? ToastClass { get; set; }

        public string? PanelClass { get; set; }

        public string? PanelStyle { get; set; }

        public string? ThemeScope { get; set; }

        public bool HasBackdrop { get; set; }

        public bool HasTransparentBackdrop { get; set; }

        public string? BackdropClass { get; set; }

        public int Width { get; set; } = 392;

        public int Height { get; set; }

        public int Margin { get; set; } = 8;

        public ToastPostion ToastPostion { get; set; } = ToastPostion.BottomRight;

        public int? OffsetLeft { get; set; } = 15;

        public int? OffsetRight { get; set; } = 15;

        public int? OffsetTop { get; set; } = 15;

        public int? OffsetBottom { get; set; } = 15;

        public int ExcutionTime { get; set; } = 5;

        public bool ShowExcutionTime { get; set; } = false;

        /// <summary>
        /// Gets or sets the visible duration in milliseconds. Set to <see langword="null"/>
        /// for a persistent toast. When omitted, <see cref="ExcutionTime"/> remains the
        /// backwards-compatible duration source.
        /// </summary>
        public int? Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                DurationWasSet = true;
            }
        }

        /// <summary>Gets or sets the maximum number of simultaneously visible toasts.</summary>
        public int MaxVisible { get; set; } = 4;

        /// <summary>Gets or sets up to two actions displayed below the message.</summary>
        public IReadOnlyList<ToastAction>? Actions { get; set; }

        /// <summary>Gets or sets localized strings for this toast.</summary>
        public ToastStrings? Strings { get; set; }

        internal int? GetDurationMilliseconds()
        {
            if (DurationWasSet)
            {
                return _duration is > 0 ? _duration : null;
            }

            return ExcutionTime > 0 ? checked(ExcutionTime * 1000) : null;
        }
    }
}
