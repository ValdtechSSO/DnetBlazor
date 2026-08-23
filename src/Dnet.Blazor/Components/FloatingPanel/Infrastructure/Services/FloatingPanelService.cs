using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Enums;
using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Interfaces;
using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.FloatingPanel.Infrastructure.Services
{
    public class FloatingPanelService : IFloatingPanelService
    {
        private readonly IOverlayService _overlayService;

        public FloatingPanelService(IOverlayService overlayService)
        {
            _overlayService = overlayService;
        }

        public OverlayReference Show(Type componentType, IDictionary<string, object> parameters, FloatingPanelConfig floatingPanelConfig)
        {
            var reference = Open(componentType, parameters, floatingPanelConfig);

            return reference;
        }

        private OverlayReference Open(Type componentType, IDictionary<string, object> parameters, FloatingPanelConfig floatingPanelConfig)
        {
            EnsureComponentType(componentType);

            var content = new FloatingPanelContent(componentType, parameters);

            var globalPositionStrategy = new GlobalPositionStrategyBuilder();

            var offsetBottom = floatingPanelConfig.OffsetBottom > 0 ? floatingPanelConfig.OffsetBottom : 0;

            var offsetRight = floatingPanelConfig.OffsetRight > 0 ? floatingPanelConfig.OffsetRight : 0;

            var offsetTop = floatingPanelConfig.OffsetTop > 0 ? floatingPanelConfig.OffsetTop : 0;

            var offsetLeft = floatingPanelConfig.OffsetLeft > 0 ? floatingPanelConfig.OffsetLeft : 0;

            switch (floatingPanelConfig.Postion)
            {
                case FloatingPanelPostion.BottomCenter:

                    globalPositionStrategy.Bottom($"{offsetBottom + (floatingPanelConfig.Margin)}px");
                    globalPositionStrategy.CenterHorizontally("");

                    break;

                case FloatingPanelPostion.BottomRight:

                    globalPositionStrategy.Bottom($"{offsetBottom + (floatingPanelConfig.Margin)}px");
                    globalPositionStrategy.Right(offsetRight + "px");

                    break;

                case FloatingPanelPostion.BottomLeft:

                    globalPositionStrategy.Bottom(offsetBottom + "px");
                    globalPositionStrategy.Left(offsetLeft + "px");

                    break;

                case FloatingPanelPostion.TopCenter:

                    globalPositionStrategy.Top(offsetTop + "px");
                    globalPositionStrategy.CenterHorizontally("");

                    break;

                case FloatingPanelPostion.TopRight:

                    globalPositionStrategy.Top($"{offsetTop + (floatingPanelConfig.Margin)}px");
                    globalPositionStrategy.Right(offsetRight + "px");

                    break;

                case FloatingPanelPostion.TopLeft:

                    globalPositionStrategy.Top($"{offsetTop + (floatingPanelConfig.Margin)}px");
                    globalPositionStrategy.Left(offsetLeft + "px");

                    break;
                case FloatingPanelPostion.LeftCenter:

                    globalPositionStrategy.Left(offsetLeft + "px");
                    globalPositionStrategy.CenterVertically("");

                    break;

                case FloatingPanelPostion.RightCenter:

                    globalPositionStrategy.Right(offsetRight + "px");
                    globalPositionStrategy.CenterVertically("");

                    break;
            }

            var overlayConfig = new OverlayConfig()
            {
                HasBackdrop = floatingPanelConfig.HasBackdrop,
                HasTransparentBackdrop = floatingPanelConfig.HasTransparentBackdrop,
                DisableBackdropClick  = floatingPanelConfig.DisableBackdropClick,
                CloseOnEscape = floatingPanelConfig.CloseOnEscape,
                CloseOnOutsidePointer = !floatingPanelConfig.HasBackdrop && floatingPanelConfig.CloseOnOutsidePointer,
                ScrollStrategy = OverlayScrollStrategy.Block,
                Role = floatingPanelConfig.Role,
                AriaLabel = floatingPanelConfig.AriaLabel,
                AriaLabelledBy = floatingPanelConfig.AriaLabelledBy,
                AriaDescribedBy = floatingPanelConfig.AriaDescribedBy,
                AriaLive = floatingPanelConfig.AriaLive,
                AriaAtomic = floatingPanelConfig.AriaAtomic,
                TrapFocus = floatingPanelConfig.TrapFocus,
                RestoreFocus = floatingPanelConfig.RestoreFocus,
                InitialFocusSelector = floatingPanelConfig.InitialFocusSelector,
                Width = floatingPanelConfig.Width == null ? "100vw" : floatingPanelConfig.Width + "px",
                Height = floatingPanelConfig.Height == null ? "100vh" : floatingPanelConfig.Height + "px",
                PanelClass = floatingPanelConfig.PanelClass,
                PanelStyle = floatingPanelConfig.PanelStyle,
                ThemeScope = floatingPanelConfig.ThemeScope,
                GlobalPositionStrategy = globalPositionStrategy,
                ComponentType = ComponentType.FloatingPanel
            };

            var floatingPanel = new RenderFragment(x =>
            {
                x.OpenComponent(0, typeof(DnetFloatingPanel));
                x.AddAttribute(1, "ComponentType", content.ComponentType);
                x.AddAttribute(2, "Parameters", content.Parameters);
                x.AddAttribute(3, "FloatingPanelClass", floatingPanelConfig.FloatingPanelClass);
                x.CloseComponent();
            });

            var overlayReference = _overlayService.Attach(floatingPanel, overlayConfig);
            overlayReference.SetContentUpdater((updatedComponentType, updatedParameters) =>
            {
                EnsureComponentType(updatedComponentType);
                content.ComponentType = updatedComponentType;
                content.Parameters = updatedParameters;
            });

            return overlayReference;
        }

        private static void EnsureComponentType(Type componentType)
        {
            if (!typeof(ComponentBase).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"{componentType.FullName} must be a Blazor Component");
            }
        }

        private sealed class FloatingPanelContent(
            Type componentType,
            IDictionary<string, object> parameters)
        {
            public Type ComponentType { get; set; } = componentType;

            public IDictionary<string, object> Parameters { get; set; } = parameters;
        }

        public void Close(OverlayResult overlayDataResult)
        {
            _overlayService.Detach(overlayDataResult);
        }
    }
}
