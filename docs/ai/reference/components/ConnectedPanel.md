# ConnectedPanel

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-connected-panel-background` | `var(--dnet-sys-surface)` |
| `--dnet-connected-panel-padding` | `2px` |
| `--dnet-dialog-border-radius` | — |

```css
:root { --dnet-connected-panel-background: /* your value */; }
```
