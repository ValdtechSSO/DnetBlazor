using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Toast.Infrastructure.Enums;
using Dnet.Blazor.Components.Toast.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Toast.Infrastructure.Models;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Toast.Infrastructure.Services;

public class ToastService : IToastService
{
    private readonly IOverlayService _overlayService;
    private readonly object _syncRoot = new();
    private readonly Dictionary<ToastStackKey, ToastStackState> _stacks = new();

    internal event Action<ToastStackKey>? StackChanged;

    public ToastService(IOverlayService overlayService)
    {
        _overlayService = overlayService;
    }

    public void Show(
        ToastConfig toastConfig,
        Type componentType = null!,
        IDictionary<string, object> parameters = null!,
        RenderFragment dialogContent = null!)
    {
        ArgumentNullException.ThrowIfNull(toastConfig);

        if (componentType is not null && !typeof(ComponentBase).IsAssignableFrom(componentType))
        {
            throw new ArgumentException($"{componentType.FullName} must be a Blazor Component", nameof(componentType));
        }

#if DEBUG
        if (toastConfig.Actions?.Count > 2)
        {
            System.Diagnostics.Debug.WriteLine("ToastConfig supports a maximum of two actions; additional actions were ignored.");
        }
#endif

        var stackKey = new ToastStackKey(toastConfig.ToastPostion, toastConfig.ThemeScope);
        var entry = CreateEntry(toastConfig, componentType, parameters, dialogContent);
        ToastStackState? stateToAttach = null;

        lock (_syncRoot)
        {
            if (!_stacks.TryGetValue(stackKey, out var state))
            {
                state = new ToastStackState
                {
                    Key = stackKey,
                    LayoutConfig = toastConfig,
                    MaxVisible = Math.Max(1, toastConfig.MaxVisible)
                };
                _stacks.Add(stackKey, state);
                stateToAttach = state;
            }

            if (state.Visible.Count < state.MaxVisible)
            {
                state.Visible.Add(entry);
            }
            else
            {
                state.Pending.Enqueue(entry);
            }
        }

        if (stateToAttach is not null)
        {
            AttachStack(stateToAttach);
        }
        else
        {
            StackChanged?.Invoke(stackKey);
        }
    }

    public void Close(OverlayResult overlayDataResult)
    {
        ArgumentNullException.ThrowIfNull(overlayDataResult);

        ToastStackState? state;
        lock (_syncRoot)
        {
            state = _stacks.Values.FirstOrDefault(candidate =>
                candidate.OverlayReferenceId == overlayDataResult.OverlayReferenceId);

            if (state is not null)
            {
                _stacks.Remove(state.Key);
            }
        }

        _overlayService.Detach(overlayDataResult);
    }

    internal IReadOnlyList<ToastEntry> GetVisibleEntries(ToastStackKey stackKey)
    {
        lock (_syncRoot)
        {
            return _stacks.TryGetValue(stackKey, out var state)
                ? state.Visible.ToArray()
                : Array.Empty<ToastEntry>();
        }
    }

    internal void CompleteClose(ToastStackKey stackKey, Guid entryId)
    {
        int? overlayReferenceId = null;
        var shouldNotify = false;

        lock (_syncRoot)
        {
            if (!_stacks.TryGetValue(stackKey, out var state))
            {
                return;
            }

            var removed = state.Visible.RemoveAll(entry => entry.Id == entryId) > 0;
            if (!removed)
            {
                return;
            }

            if (state.Pending.TryDequeue(out var next))
            {
                state.Visible.Add(next);
            }

            if (state.Visible.Count == 0 && state.Pending.Count == 0)
            {
                _stacks.Remove(stackKey);
                overlayReferenceId = state.OverlayReferenceId;
            }
            else
            {
                shouldNotify = true;
            }
        }

        if (overlayReferenceId is { } referenceId)
        {
            _overlayService.Detach(new OverlayResult { OverlayReferenceId = referenceId });
        }
        else if (shouldNotify)
        {
            StackChanged?.Invoke(stackKey);
        }
    }

    private void AttachStack(ToastStackState state)
    {
        var config = state.LayoutConfig;
        var globalPositionStrategy = BuildPositionStrategy(config);

        var overlayConfig = new OverlayConfig
        {
            HasBackdrop = config.HasBackdrop,
            HasTransparentBackdrop = config.HasTransparentBackdrop,
            BackdropClass = config.BackdropClass,
            Width = "auto",
            Height = config.Height > 0 ? $"{config.Height}px" : "auto",
            MaxWidth = "100vw",
            MaxHeight = "100dvh",
            PanelClass = config.PanelClass,
            PanelStyle = config.PanelStyle,
            ThemeScope = config.ThemeScope,
            GlobalPositionStrategy = globalPositionStrategy,
            RestoreFocus = false,
            ComponentType = ComponentType.Toast
        };

        RenderFragment stack = builder =>
        {
            builder.OpenComponent<DnetToastStack>(0);
            builder.AddAttribute(1, nameof(DnetToastStack.Service), this);
            builder.AddAttribute(2, nameof(DnetToastStack.Position), state.Key.Position);
            builder.AddAttribute(3, nameof(DnetToastStack.ThemeScope), state.Key.ThemeScope);
            builder.AddAttribute(4, nameof(DnetToastStack.Width), config.Width);
            builder.CloseComponent();
        };

        var reference = _overlayService.Attach(stack, overlayConfig);

        lock (_syncRoot)
        {
            if (_stacks.TryGetValue(state.Key, out var current))
            {
                current.OverlayReferenceId = reference.GetOverlayReferenceId();
            }
        }

        reference.Detached += _ => RemoveDetachedStack(reference.GetOverlayReferenceId());
    }

    private void RemoveDetachedStack(int overlayReferenceId)
    {
        lock (_syncRoot)
        {
            var state = _stacks.Values.FirstOrDefault(candidate =>
                candidate.OverlayReferenceId == overlayReferenceId);

            if (state is not null)
            {
                _stacks.Remove(state.Key);
            }
        }
    }

    private static ToastEntry CreateEntry(
        ToastConfig config,
        Type? componentType,
        IDictionary<string, object>? parameters,
        RenderFragment? contentChild) => new()
    {
        Title = config.Title,
        Text = config.Text,
        ToastType = config.ToastType,
        TypeIconClass = config.ToastTypeIconClass,
        CloseIconClass = config.ToastCloseIconClass,
        ToastTypeColor = config.ToastTypeColor,
        ToastClass = config.ToastClass,
        DurationMilliseconds = config.GetDurationMilliseconds(),
        Actions = config.Actions?
            .Where(action => !string.IsNullOrWhiteSpace(action.Label))
            .Take(2)
            .ToArray() ?? Array.Empty<ToastAction>(),
        Strings = config.Strings,
        ComponentType = componentType,
        Parameters = parameters,
        ContentChild = contentChild
    };

    private static GlobalPositionStrategyBuilder BuildPositionStrategy(ToastConfig config)
    {
        var strategy = new GlobalPositionStrategyBuilder();
        var bottom = Math.Max(0, config.OffsetBottom.GetValueOrDefault());
        var right = Math.Max(0, config.OffsetRight.GetValueOrDefault());
        var top = Math.Max(0, config.OffsetTop.GetValueOrDefault());
        var left = Math.Max(0, config.OffsetLeft.GetValueOrDefault());

        switch (config.ToastPostion)
        {
            case ToastPostion.BottomCenter:
                strategy.Bottom($"{bottom}px");
                strategy.CenterHorizontally("");
                break;
            case ToastPostion.BottomLeft:
                strategy.Bottom($"{bottom}px");
                strategy.Left($"{left}px");
                break;
            case ToastPostion.TopCenter:
                strategy.Top($"{top}px");
                strategy.CenterHorizontally("");
                break;
            case ToastPostion.TopRight:
                strategy.Top($"{top}px");
                strategy.Right($"{right}px");
                break;
            case ToastPostion.TopLeft:
                strategy.Top($"{top}px");
                strategy.Left($"{left}px");
                break;
            case ToastPostion.LeftCenter:
                strategy.Left($"{left}px");
                strategy.CenterVertically("");
                break;
            case ToastPostion.RightCenter:
                strategy.Right($"{right}px");
                strategy.CenterVertically("");
                break;
            default:
                strategy.Bottom($"{bottom}px");
                strategy.Right($"{right}px");
                break;
        }

        return strategy;
    }
}
