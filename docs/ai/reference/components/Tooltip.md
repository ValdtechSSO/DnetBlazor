# Tooltip

## `<DnetTooltipPanel>`

```razor
<DnetTooltipPanel
    TriggerElement="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ContentChild` | `RenderFragment?` | — | Gets or sets custom content rendered by the component. |
| `ComponentType` | `Type?` | `null` | Gets or sets the component type rendered dynamically. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `Text` | `string?` | — | Gets or sets the text used by this component. |
| `TooltipColor` | `string?` | — | Gets or sets the tooltip color used by this component. |
| `TooltipForeground` | `string?` | — | Gets or sets the tooltip foreground used by this component. |
| `TooltipClass` | `string?` | — | Gets or sets the tooltip class used by this component. |
| `TriggerElement` | `ElementReference` | — | Gets or sets the trigger element used by this component. |
| `MaxWidth` | `string?` | — | Gets or sets the maximum component width. |
| `MaxHeight` | `string?` | — | Gets or sets the maximum component height. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-tooltip-background` | `#000000` <br><sub>via `--dnet-sys-surface-inverse`</sub> |
| `--dnet-tooltip-border-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-tooltip-font-size` | `0.75rem` <br><sub>via `--dnet-sys-text-sm`</sub> |
| `--dnet-tooltip-foreground` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-tooltip-horizontal-padding` | `calc(var(--dnet-sys-space-unit) * 3)` <br><sub>via `--dnet-sys-space-3`</sub> |
| `--dnet-tooltip-max-width` | `250px` |
| `--dnet-tooltip-motion` | `150ms` <br><sub>via `--dnet-sys-motion-fast`</sub> |
| `--dnet-tooltip-shadow` | `0 4px 6px -2px var(--dnet-sys-shadow-color), 0 12px 24px -4px var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-2`</sub> |
| `--dnet-tooltip-vertical-padding` | `calc(var(--dnet-sys-space-unit) * 2)` <br><sub>via `--dnet-sys-space-2`</sub> |

```css
:root { --dnet-tooltip-background: /* your value */; }
```
