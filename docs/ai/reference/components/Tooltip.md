# Tooltip

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-tooltip-background` | `var(--dnet-sys-surface-inverse)` |
| `--dnet-tooltip-border-radius` | `var(--dnet-sys-radius-lg)` |
| `--dnet-tooltip-font-size` | `var(--dnet-sys-text-sm)` |
| `--dnet-tooltip-foreground` | `var(--dnet-sys-surface)` |
| `--dnet-tooltip-horizontal-padding` | `var(--dnet-sys-space-3)` |
| `--dnet-tooltip-max-width` | `250px` |
| `--dnet-tooltip-motion` | `var(--dnet-sys-motion-fast)` |
| `--dnet-tooltip-shadow` | `var(--dnet-sys-elevation-2)` |
| `--dnet-tooltip-vertical-padding` | `var(--dnet-sys-space-2)` |

```css
:root { --dnet-tooltip-background: /* your value */; }
```
