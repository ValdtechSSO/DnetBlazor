using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Tooltip.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Tooltip.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using System.Threading;

namespace Dnet.Blazor.Components.Tooltip.Infrastructure.Services
{
    public class TooltipService : ITooltipService, IDisposable
    {
        private readonly IOverlayService _overlayService;
        private readonly Dictionary<int, OverlayReference> _activeTooltips = new();
        private readonly Dictionary<int, System.Threading.Timer> _showTimers = new();
        private readonly Dictionary<int, System.Threading.Timer> _hideTimers = new();
        private readonly Dictionary<int, TooltipConfig> _tooltipConfigs = new();
        private readonly Dictionary<int, int> _placeholderToRealIdMap = new(); // Mapeo de placeholder ID a overlay real ID
        private readonly Dictionary<string, int> _tooltipIdsByTrigger = new();
        private readonly Dictionary<int, string> _triggerKeysByTooltipId = new();
        private readonly object _lock = new object();
        private int _nextId;

        public TooltipService(IOverlayService overlayService)
        {
            _overlayService = overlayService;
        }

        public OverlayReference Show(TooltipConfig tooltipConfig, ElementReference elementReference)
        {
            return ShowInternal(null, null, tooltipConfig, elementReference);
        }

        public OverlayReference Show<TComponent>(TooltipConfig tooltipConfig, IDictionary<string, object> parameters, ElementReference elementReference) where TComponent : ComponentBase
        {
            return ShowInternal(typeof(TComponent), parameters, tooltipConfig, elementReference);
        }

        private OverlayReference ShowInternal(Type? componentType, IDictionary<string, object>? parameters, TooltipConfig tooltipConfig, ElementReference elementReference)
        {
            lock (_lock)
            {
                var triggerKey = GetTriggerKey(elementReference);
                if (triggerKey != null && _tooltipIdsByTrigger.TryGetValue(triggerKey, out var existingTooltipId))
                {
                    var activeTooltipId = _placeholderToRealIdMap.TryGetValue(existingTooltipId, out var realId)
                        ? realId
                        : existingTooltipId;

                    if (_activeTooltips.TryGetValue(activeTooltipId, out var existingTooltip))
                    {
                        CancelHideTimer(activeTooltipId);
                        return existingTooltip;
                    }

                    RemoveTriggerAssociation(existingTooltipId);
                }

                if (tooltipConfig.ShowDelay > 0)
                {
                    // Placeholder IDs are negative so they cannot collide with OverlayService IDs.
                    var placeholderId = -GenerateUniqueId();
                    var placeholderRef = new OverlayReference(placeholderId);
                    _tooltipConfigs[placeholderId] = tooltipConfig;
                    AssociateTrigger(triggerKey, placeholderId);

                    // Guardar el placeholder
                    _activeTooltips[placeholderId] = placeholderRef;

                    // Programar la creación del tooltip real
                    var timer = new System.Threading.Timer(_ =>
                    {
                        lock (_lock)
                        {
                            // Solo crear si aún está en la lista (no fue cancelado)
                            if (_activeTooltips.ContainsKey(placeholderId))
                            {
                                var actualRef = Open(componentType, parameters, tooltipConfig, elementReference);
                                var realId = actualRef.GetOverlayReferenceId();
                                
                                // Mapear placeholder ID a real ID
                                _placeholderToRealIdMap[placeholderId] = realId;
                                _activeTooltips[realId] = actualRef;
                                TransferTriggerAssociation(placeholderId, realId);
                                
                                // Mover la configuración al ID real
                                _tooltipConfigs[realId] = tooltipConfig;
                                _tooltipConfigs.Remove(placeholderId);
                                
                                // Remover el placeholder
                                _activeTooltips.Remove(placeholderId);
                            }
                            _showTimers.Remove(placeholderId);
                        }
                    }, null, tooltipConfig.ShowDelay, System.Threading.Timeout.Infinite);

                    _showTimers[placeholderId] = timer;
                    return placeholderRef;
                }

                // Sin delay, crear inmediatamente
                var result = Open(componentType, parameters, tooltipConfig, elementReference);
                var resultId = result.GetOverlayReferenceId();
                _activeTooltips[resultId] = result;
                AssociateTrigger(triggerKey, resultId);
                _tooltipConfigs[resultId] = tooltipConfig;
                
                return result;
            }
        }

        private int GenerateUniqueId()
        {
            // Keep IDs unique for the lifetime of the scoped service. Overflow is harmless because
            // the dictionary still protects against the only possible collision.
            return Interlocked.Increment(ref _nextId);
        }

        private static string? GetTriggerKey(ElementReference elementReference) =>
            string.IsNullOrEmpty(elementReference.Id) ? null : elementReference.Id;

        private void AssociateTrigger(string? triggerKey, int tooltipId)
        {
            if (triggerKey == null)
            {
                return;
            }

            _tooltipIdsByTrigger[triggerKey] = tooltipId;
            _triggerKeysByTooltipId[tooltipId] = triggerKey;
        }

        private void TransferTriggerAssociation(int previousTooltipId, int nextTooltipId)
        {
            if (!_triggerKeysByTooltipId.Remove(previousTooltipId, out var triggerKey))
            {
                return;
            }

            _triggerKeysByTooltipId[nextTooltipId] = triggerKey;
            _tooltipIdsByTrigger[triggerKey] = nextTooltipId;
        }

        private void RemoveTriggerAssociation(int tooltipId)
        {
            if (!_triggerKeysByTooltipId.Remove(tooltipId, out var triggerKey))
            {
                return;
            }

            if (_tooltipIdsByTrigger.TryGetValue(triggerKey, out var associatedTooltipId) && associatedTooltipId == tooltipId)
            {
                _tooltipIdsByTrigger.Remove(triggerKey);
            }
        }

        private void CancelHideTimer(int tooltipId)
        {
            if (_hideTimers.TryGetValue(tooltipId, out var hideTimer))
            {
                hideTimer.Dispose();
                _hideTimers.Remove(tooltipId);
            }
        }

        private OverlayReference Open(Type? componentType, IDictionary<string, object>? parameters, TooltipConfig tooltipConfig, ElementReference elementReference)
        {
            if (!typeof(ComponentBase).IsAssignableFrom(componentType) && componentType != null)
            {
                throw new ArgumentException($"{componentType.FullName} must be a Blazor Component");
            }

            var positions = new List<ConnectedPosition>
            {
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.Start,
                    OriginY = VerticalConnectionPos.Center,
                    OverlayX = HorizontalConnectionPos.End,
                    OverlayY = VerticalConnectionPos.Center
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.End,
                    OriginY = VerticalConnectionPos.Center,
                    OverlayX = HorizontalConnectionPos.Start,
                    OverlayY = VerticalConnectionPos.Center
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.Center,
                    OriginY = VerticalConnectionPos.Top,
                    OverlayX = HorizontalConnectionPos.Center,
                    OverlayY = VerticalConnectionPos.Bottom
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.Center,
                    OriginY = VerticalConnectionPos.Bottom,
                    OverlayX = HorizontalConnectionPos.Center,
                    OverlayY = VerticalConnectionPos.Top
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.Start,
                    OriginY = VerticalConnectionPos.Bottom,
                    OverlayX = HorizontalConnectionPos.Start,
                    OverlayY = VerticalConnectionPos.Top
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.Start,
                    OriginY = VerticalConnectionPos.Top,
                    OverlayX = HorizontalConnectionPos.Start,
                    OverlayY = VerticalConnectionPos.Bottom
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.End,
                    OriginY = VerticalConnectionPos.Bottom,
                    OverlayX = HorizontalConnectionPos.Start,
                    OverlayY = VerticalConnectionPos.Top
                },
                new ConnectedPosition
                {
                    OriginX = HorizontalConnectionPos.End,
                    OriginY = VerticalConnectionPos.Top,
                    OverlayX = HorizontalConnectionPos.Start,
                    OverlayY = VerticalConnectionPos.Bottom
                }
            };

            var flexibleConnectedPositionStrategyBuilder = new FlexibleConnectedPositionStrategyBuilder()
                .WithViewportMargin(8)
                .WithFlexibleDimensions(false)
                .SetOrigin(elementReference)
                .WithPositions(positions);

            var overlayConfig = new OverlayConfig
            {
                HasBackdrop = false,
                HasTransparentBackdrop = true,
                PositionStrategy = PositionStrategy.FlexibleConnectedTo,
                FlexibleConnectedPositionStrategyBuilder = flexibleConnectedPositionStrategyBuilder,
                PanelClass = "dnet-tooltip-panel",
                PanelStyle = tooltipConfig.PanelStyle,
                ThemeScope = tooltipConfig.ThemeScope,
                Width = tooltipConfig.Width,
                Height = tooltipConfig.Height,
                MinHeight = tooltipConfig.MinHeight,
                MinWidth = tooltipConfig.MinWidth,
                MaxHeight = tooltipConfig.MaxHeight,
                MaxWidth = tooltipConfig.MaxWidth,
                ComponentType = ComponentType.ToolTip
            };

            var tooltip = new RenderFragment(x =>
            {
                x.OpenComponent(0, typeof(DnetTooltipPanel));
                x.AddAttribute(1, "Text", tooltipConfig.Text);
                x.AddAttribute(2, "TooltipClass", tooltipConfig.TooltipClass);
                x.AddAttribute(3, "TooltipColor", tooltipConfig.TooltipColor);
                x.AddAttribute(4, "TooltipForeground", tooltipConfig.TooltipForeground);
                x.AddAttribute(5, "MaxWidth", tooltipConfig.MaxWidth);
                x.AddAttribute(6, "MaxHeight", tooltipConfig.MaxHeight);
                x.AddAttribute(7, "ComponentType", componentType);
                x.AddAttribute(8, "Parameters", parameters);
                x.AddAttribute(9, "TriggerElement", elementReference);
                x.CloseComponent();
            });

            var overlayReference = _overlayService.Attach(tooltip, overlayConfig);

            return overlayReference;
        }

        public void Close(OverlayResult overlayDataResult)
        {
            if (overlayDataResult == null) return;

            var requestedId = overlayDataResult.OverlayReferenceId;

            lock (_lock)
            {
                // Resolver el ID real si es un placeholder
                var tooltipId = _placeholderToRealIdMap.TryGetValue(requestedId, out var realId) ? realId : requestedId;

                // Cancelar el timer de show si existe (usando el ID solicitado, que puede ser el placeholder)
                if (_showTimers.TryGetValue(requestedId, out var showTimer))
                {
                    showTimer?.Dispose();
                    _showTimers.Remove(requestedId);
                    _activeTooltips.Remove(requestedId);
                    _tooltipConfigs.Remove(requestedId);
                    _placeholderToRealIdMap.Remove(requestedId);
                    RemoveTriggerAssociation(requestedId);
                    return; // Si aún no se mostró, solo cancelamos y salimos
                }

                // Verificar si el tooltip existe (buscar por ID real)
                if (!_activeTooltips.ContainsKey(tooltipId))
                {
                    return;
                }

                // Obtener la configuración del HideDelay
                var hideDelay = 0;
                if (_tooltipConfigs.TryGetValue(tooltipId, out var config))
                {
                    hideDelay = config.HideDelay;
                }

                if (hideDelay > 0)
                {
                    // Si ya hay un timer de hide pendiente, no hacer nada
                    if (_hideTimers.ContainsKey(tooltipId))
                    {
                        return;
                    }

                    // Crear timer para cerrar después del delay
                    var timer = new System.Threading.Timer(_ =>
                    {
                        CloseImmediate(new OverlayResult { OverlayReferenceId = tooltipId });
                    }, null, hideDelay, System.Threading.Timeout.Infinite);

                    _hideTimers[tooltipId] = timer;
                }
                else
                {
                    // Sin delay, cerrar inmediatamente
                    CloseImmediate(new OverlayResult { OverlayReferenceId = tooltipId });
                }
            }
        }

        private void CloseImmediate(OverlayResult overlayDataResult)
        {
            if (overlayDataResult == null) return;

            var tooltipId = overlayDataResult.OverlayReferenceId;

            lock (_lock)
            {
                // Cancelar cualquier timer de show pendiente
                if (_showTimers.TryGetValue(tooltipId, out var showTimer))
                {
                    showTimer?.Dispose();
                    _showTimers.Remove(tooltipId);
                }

                // Cancelar cualquier timer de hide pendiente
                CancelHideTimer(tooltipId);

                // Solo detach si el tooltip realmente existe
                if (_activeTooltips.ContainsKey(tooltipId))
                {
                    _overlayService.Detach(overlayDataResult);
                    _activeTooltips.Remove(tooltipId);
                    _tooltipConfigs.Remove(tooltipId);
                    RemoveTriggerAssociation(tooltipId);
                    
                    // Remover mapeo si existe
                    var placeholderId = _placeholderToRealIdMap.FirstOrDefault(x => x.Value == tooltipId).Key;
                    if (placeholderId != 0)
                    {
                        _placeholderToRealIdMap.Remove(placeholderId);
                    }
                }
            }
        }

        /// <summary>
        /// Closes all active tooltips.
        /// </summary>
        public void CloseAll()
        {
            lock (_lock)
            {
                foreach (var tooltip in _activeTooltips.Values.ToList())
                {
                    if (tooltip != null)
                    {
                        CloseImmediate(new OverlayResult { OverlayReferenceId = tooltip.GetOverlayReferenceId() });
                    }
                }

                // Limpiar todos los timers
                foreach (var timer in _showTimers.Values)
                {
                    timer?.Dispose();
                }
                _showTimers.Clear();

                foreach (var timer in _hideTimers.Values)
                {
                    timer?.Dispose();
                }
                _hideTimers.Clear();

                _activeTooltips.Clear();
                _tooltipConfigs.Clear();
                _placeholderToRealIdMap.Clear();
                _tooltipIdsByTrigger.Clear();
                _triggerKeysByTooltipId.Clear();
            }
        }

        public void Dispose()
        {
            CloseAll();
        }
    }
}
