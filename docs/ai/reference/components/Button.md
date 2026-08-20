# Button

Components: `<DnetButton>`

## `<DnetButton>`

| Parameter | Type | Default |
|---|---|---|
| `InitialFocus` | `bool` | — |
| `OnClick` | `EventCallback` | — |
| `ButtonType` | `string` | `"button"` |
| `ChildContent` | `RenderFragment?` | — |

## Minimal usage

```razor
<DnetButton
    InitialFocus="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-btn-background` | `var(--dnet-button-background-color, var(--dnet-sys-transparent))` |
| `--dnet-btn-background-hover` | `var(--dnet-button-hover-color, var(--dnet-sys-state-hover))` |
| `--dnet-btn-focus-overlay` | `var(--dnet-sys-state-pressed)` |
| `--dnet-btn-font-size` | `var(--dnet-button-font-size, var(--dnet-sys-text-md))` |
| `--dnet-btn-foreground` | `var(--dnet-sys-on-surface)` |
| `--dnet-btn-height` | `var(--dnet-button-line-height, 36px)` |
| `--dnet-btn-margin` | `var(--dnet-button-margin, 0)` |
| `--dnet-btn-min-width` | `var(--dnet-button-min-width, 64px)` |
| `--dnet-btn-padding` | `var(--dnet-button-padding, 0 var(--dnet-sys-space-4))` |
| `--dnet-btn-radius` | `var(--dnet-button-border-radius, var(--dnet-sys-radius-sm))` |
| `--dnet-button-background-color` | — |
| `--dnet-button-border-radius` | — |
| `--dnet-button-font-size` | — |
| `--dnet-button-hover-color` | — |
| `--dnet-button-line-height` | — |
| `--dnet-button-margin` | — |
| `--dnet-button-min-width` | — |
| `--dnet-button-padding` | — |

```css
:root { --dnet-btn-background: /* your value */; }
```
