# Checkbox

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-checkbox-background` | `var(--dnet-sys-primary)` |
| `--dnet-checkbox-border-color` | `color-mix(in srgb, var(--dnet-sys-on-surface-subtle) 57.2%, var(--dnet-sys-surface))` |
| `--dnet-checkbox-border-radius` | `0px` |
| `--dnet-checkbox-border-width` | `1px` |
| `--dnet-checkbox-checkmark-path` | `var(--dnet-sys-surface)` |
| `--dnet-checkbox-font-size` | `var(--dnet-sys-text-md)` |
| `--dnet-checkbox-size` | `14px` |

```css
:root { --dnet-checkbox-background: /* your value */; }
```
