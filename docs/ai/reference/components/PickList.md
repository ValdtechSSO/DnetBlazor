# PickList

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-list-border-color` | — |
| `--dnet-list-border-radius` | — |
| `--dnet-list-check-width` | — |
| `--dnet-list-header-footer-height` | — |
| `--dnet-list-hover-background-color` | — |
| `--dnet-list-hover-border-radius` | — |
| `--dnet-list-item-height` | — |
| `--dnet-list-supporting-text-color` | — |
| `--dnet-picklist-background` | `var(--pick-list-background, var(--dnet-sys-surface))` |
| `--dnet-picklist-border` | `var(--pick-list-border, var(--dnet-list-border-color, var(--dnet-sys-border)))` |
| `--dnet-picklist-check-width` | — |
| `--dnet-picklist-footer-height` | — |
| `--dnet-picklist-foreground` | `var(--pick-list-foreground, var(--dnet-sys-on-surface))` |
| `--dnet-picklist-header-height` | — |
| `--dnet-picklist-item-background` | `var(--pick-list-item-background, var(--dnet-sys-surface-raised))` |
| `--dnet-picklist-item-height` | `var(--pick-list-item-height, var(--dnet-list-item-height, var(--dnet-sys-control-height)))` |
| `--dnet-picklist-item-hover` | `var(--pick-list-item-hover, var(--dnet-list-hover-background-color, var(--dnet-sys-surface-hover)))` |
| `--dnet-picklist-item-radius` | — |
| `--dnet-picklist-item-selected` | `var(--pick-list-item-selected, var(--dnet-sys-state-selected))` |
| `--dnet-picklist-items-max-height` | — |
| `--dnet-picklist-muted` | `var(--pick-list-muted, var(--dnet-list-supporting-text-color, var(--dnet-sys-on-surface-muted)))` |
| `--dnet-picklist-primary` | `var(--pick-list-primary, var(--dnet-sys-primary))` |
| `--dnet-picklist-radius` | `var(--pick-list-radius, var(--dnet-list-border-radius, var(--dnet-sys-radius-lg)))` |
| `--dnet-picklist-scrollbar` | `var(--pick-list-scrollbar, var(--dnet-sys-border-strong))` |
| `--dnet-picklist-scrollbar-track` | — |
| `--dnet-picklist-search-radius` | — |

```css
:root { --dnet-list-border-color: /* your value */; }
```
