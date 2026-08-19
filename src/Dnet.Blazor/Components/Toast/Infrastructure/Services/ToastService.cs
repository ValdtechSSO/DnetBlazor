using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Toast.Infrastructure.Enums;
using Dnet.Blazor.Components.Toast.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Toast.Infrastructure.Models;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Toast.Infrastructure.Services
{
    public class ToastService : IToastService
    {
        private readonly IOverlayService _overlayService;

        private readonly object _syncRoot = new();
        private readonly Dictionary<ToastPostion, Dictionary<int, int>> _positionTracker = new();

        public ToastService(IOverlayService overlayService)
        {
            _overlayService = overlayService;
        }

        public void Show(ToastConfig toastConfig, Type componentType, IDictionary<string, object> parameters, RenderFragment dialogContent)
        {
            ArgumentNullException.ThrowIfNull(toastConfig);

            if (componentType is not null && !typeof(ComponentBase).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"{componentType.FullName} must be a Blazor Component");
            }

            var globalPositionStrategy = new GlobalPositionStrategyBuilder();

            var offsetBottom = Math.Max(0, toastConfig.OffsetBottom.GetValueOrDefault());
            var offsetRight = Math.Max(0, toastConfig.OffsetRight.GetValueOrDefault());
            var offsetTop = Math.Max(0, toastConfig.OffsetTop.GetValueOrDefault());
            var offsetLeft = Math.Max(0, toastConfig.OffsetLeft.GetValueOrDefault());
            var position = GetNextPosition(toastConfig.ToastPostion);
            var stackOffset = (toastConfig.Height + toastConfig.Margin) * position;

            switch (toastConfig.ToastPostion)
            {
                case ToastPostion.BottomCenter:

                    globalPositionStrategy.Bottom($"{offsetBottom + stackOffset}px");
                    globalPositionStrategy.CenterHorizontally("");

                    break;

                case ToastPostion.BottomRight:

                    globalPositionStrategy.Bottom($"{offsetBottom + stackOffset}px");
                    globalPositionStrategy.Right(offsetRight + "px");

                    break;

                case ToastPostion.BottomLeft:

                    globalPositionStrategy.Bottom($"{offsetBottom + stackOffset}px");
                    globalPositionStrategy.Left(offsetLeft + "px");

                    break;

                case ToastPostion.TopCenter:

                    globalPositionStrategy.Top($"{offsetTop + stackOffset}px");
                    globalPositionStrategy.CenterHorizontally("");

                    break;

                case ToastPostion.TopRight:

                    globalPositionStrategy.Top($"{offsetTop + stackOffset}px");
                    globalPositionStrategy.Right(offsetRight + "px");

                    break;

                case ToastPostion.TopLeft:

                    globalPositionStrategy.Top($"{offsetTop + stackOffset}px");
                    globalPositionStrategy.Left(offsetLeft + "px");

                    break;
                case ToastPostion.LeftCenter:

                    globalPositionStrategy.Left(offsetLeft + "px");
                    globalPositionStrategy.Top($"{offsetTop + stackOffset}px");

                    break;

                case ToastPostion.RightCenter:

                    globalPositionStrategy.Right(offsetRight + "px");
                    globalPositionStrategy.Top($"{offsetTop + stackOffset}px");

                    break;
            }

            var overlayConfig = new OverlayConfig()
            {
                HasBackdrop = toastConfig.HasBackdrop,
                HasTransparentBackdrop = toastConfig.HasTransparentBackdrop,
                Width = toastConfig.Width + "px",
                Height = toastConfig.Height + "px",
                PanelClass = toastConfig.PanelClass,
                PanelStyle = toastConfig.PanelStyle,
                ThemeScope = toastConfig.ThemeScope,
                GlobalPositionStrategy = globalPositionStrategy,
                MaxHeight = "170px",
                ComponentType = ComponentType.Toast
            };

            var toast = new RenderFragment(x =>
            {
                x.OpenComponent(0, typeof(DnetToast));
                x.AddAttribute(1, "Title", toastConfig.Title);
                x.AddAttribute(2, "ToastClass", toastConfig.ToastClass);
                x.AddAttribute(3, "Text", toastConfig.Text);
                x.AddAttribute(4, "ToastType", toastConfig.ToastType);
                x.AddAttribute(5, "ToastTypeIconClass", toastConfig.ToastTypeIconClass);
                x.AddAttribute(6, "TypeIconClass", toastConfig.ToastTypeIconClass);
                x.AddAttribute(7, "ExcutionTime", toastConfig.ExcutionTime);
                x.AddAttribute(8, "ShowExcutionTime", toastConfig.ShowExcutionTime);
                x.AddAttribute(9, "CloseIconClass", toastConfig.ToastCloseIconClass);
                x.AddAttribute(10, "ToastTypeColor", toastConfig.ToastTypeColor);
                if (componentType is not null && parameters?.Any() == true) x.AddAttribute(11, "Parameters", parameters);
                if (componentType is not null) x.AddAttribute(12, "ComponentType", componentType);
                if (dialogContent is not null) x.AddAttribute(13, "ContentChild", dialogContent);
                x.CloseComponent();
            });

            var reference = _overlayService.Attach(toast, overlayConfig);

            lock (_syncRoot)
            {
                if (!_positionTracker.TryGetValue(toastConfig.ToastPostion, out var positions))
                {
                    positions = new Dictionary<int, int>();
                    _positionTracker.Add(toastConfig.ToastPostion, positions);
                }

                positions.Add(position, reference.GetOverlayReferenceId());
            }
        }

        public void Close(OverlayResult overlayDataResult)
        {
            ArgumentNullException.ThrowIfNull(overlayDataResult);

            lock (_syncRoot)
            {
                foreach (var (toastPosition, positions) in _positionTracker.ToArray())
                {
                    var item = positions.FirstOrDefault(p => p.Value == overlayDataResult.OverlayReferenceId);
                    if (item.Value == 0)
                    {
                        continue;
                    }

                    positions.Remove(item.Key);
                    if (positions.Count == 0)
                    {
                        _positionTracker.Remove(toastPosition);
                    }

                    break;
                }
            }

            _overlayService.Detach(overlayDataResult);
        }

        private int GetNextPosition(ToastPostion toastPosition)
        {
            lock (_syncRoot)
            {
                if (!_positionTracker.TryGetValue(toastPosition, out var positions))
                {
                    return 0;
                }

                var position = 0;
                while (positions.ContainsKey(position))
                {
                    position++;
                }

                return position;
            }
        }
    }
}
