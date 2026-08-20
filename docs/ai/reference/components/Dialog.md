# Dialog

Components: `<DnetDialog>`

## `<DnetDialog>`

| Parameter | Type | Default |
|---|---|---|
| `ComponentType` | `Type?` | — |
| `Parameters` | `IDictionary<string, object>?` | — |
| `Title` | `string` | `string.Empty` |
| `DialogClass` | `string` | `string.Empty` |

## Minimal usage

```razor
<DnetDialog
    ComponentType="..."
    Parameters="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-dialog-background` | `var(--dnet-sys-surface)` |
| `--dnet-dialog-border-radius` | `var(--dnet-sys-radius-md)` |
| `--dnet-dialog-button-margin` | — |
| `--dnet-dialog-elevation` | `0 11px 15px -7px var(--dnet-sys-shadow-color), 0 24px 38px 3px var(--dnet-sys-shadow-color), 0 9px 46px 8px var(--dnet-sys-shadow-color)` |
| `--dnet-dialog-foreground` | `var(--dnet-sys-on-surface-emphasis)` |
| `--dnet-dialog-header-margin-bottom` | `15px` |
| `--dnet-dialog-height-compensation` | `var(--dnet-sys-space-unit)` |
| `--dnet-dialog-icon-color` | `var(--dnet-sys-on-surface)` |
| `--dnet-dialog-max-height` | `100%` |
| `--dnet-dialog-padding-left-right` | `24px` |
| `--dnet-dialog-padding-top-bottom` | `10px` |
| `--dnet-dialog-title-font-size` | `var(--dnet-sys-text-xl)` |

```css
:root { --dnet-dialog-background: /* your value */; }
```
