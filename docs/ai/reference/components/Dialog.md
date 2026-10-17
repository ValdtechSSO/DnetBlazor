# Dialog

## `<DnetDialog>`

```razor
<DnetDialog
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ComponentType` | `Type?` | — | Gets or sets the component type rendered dynamically. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `Title` | `string` | `string.Empty` | Gets or sets the title displayed by the component. |
| `DialogClass` | `string` | `string.Empty` | Gets or sets the dialog class used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-dialog-background` | `#ffffff` <br><sub>via `--dnet-sys-surface-raised`</sub> |
| `--dnet-dialog-border-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-dialog-button-margin` | `calc(var(--dnet-sys-space-unit) * 2)` <br><sub>via `--dnet-sys-space-2`</sub> |
| `--dnet-dialog-elevation` | `0 3px 5px -1px var(--dnet-sys-shadow-color), 0 6px 10px 0 var(--dnet-sys-shadow-color), 0 1px 18px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-3`</sub> |
| `--dnet-dialog-foreground` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-dialog-header-margin-bottom` | `15px` |
| `--dnet-dialog-height-compensation` | `4px` <br><sub>via `--dnet-sys-space-unit`</sub> |
| `--dnet-dialog-icon-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-dialog-max-height` | `100%` |
| `--dnet-dialog-padding-left-right` | `24px` |
| `--dnet-dialog-padding-top-bottom` | `10px` |
| `--dnet-dialog-title-font-size` | `1.25rem` <br><sub>via `--dnet-sys-text-xl`</sub> |

```css
:root { --dnet-dialog-background: /* your value */; }
```
