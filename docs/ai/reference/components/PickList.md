# PickList

A key-based, paged multi-selection component. Selection is controlled by SelectedKeys and therefore survives searches and page changes.

## `<PickList>` — generic over TItem, TKey

```razor
<PickList TItem="..." TKey="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<TItem>?` | — | Gets or sets the complete local collection. It cannot be used with ItemsProvider. |
| `ItemsProvider` | `PickListItemsProvider<TItem>?` | — | Gets or sets the provider used for paged, server-side data. |
| `ItemKey` | `Func<TItem, TKey>` | `default!` | Gets or sets the stable key selector for an item. |
| `ItemTemplate` | `RenderFragment<TItem>` | `default!` | Gets or sets the visual content of an item. |
| `SearchTextSelector` | `Func<TItem, string?>?` | — | Gets or sets the local-search text selector. It is required in local mode. |
| `SelectedKeys` | `IReadOnlySet<TKey>` | `new HashSet<TKey>()` | Gets or sets the globally selected keys. |
| `SelectedKeysChanged` | `EventCallback<IReadOnlySet<TKey>>` | — | Raised with a new selected-key set when the user changes selection. |
| `SearchText` | `string?` | — | Gets or sets the externally controlled search text. |
| `SearchTextChanged` | `EventCallback<string?>` | — | Raised when a controlled search text changes. Without it, search is managed internally. |
| `PageSize` | `int` | `10` | Gets or sets the number of visible items per page. |
| `Strings` | `PickListStrings?` | — | Gets or sets instance-specific UI strings. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-picklist-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-picklist-border` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-picklist-check-width` | `24px` |
| `--dnet-picklist-footer-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-picklist-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-picklist-header-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-picklist-item-background` | `#ffffff` <br><sub>via `--dnet-sys-surface-raised`</sub> |
| `--dnet-picklist-item-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-picklist-item-hover` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-picklist-item-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-picklist-item-selected` | `color-mix(in srgb, var(--dnet-sys-primary) 12%, transparent)` <br><sub>via `--dnet-sys-state-selected`</sub> |
| `--dnet-picklist-items-max-height` | `250px` |
| `--dnet-picklist-muted` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-picklist-primary` | `#4fc3f7` <br><sub>via `--dnet-sys-primary`</sub> |
| `--dnet-picklist-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-picklist-scrollbar` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-picklist-scrollbar-track` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-picklist-search-radius` | `9999px` <br><sub>via `--dnet-sys-radius-pill`</sub> |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-list-border-color`, `--dnet-list-border-radius`, `--dnet-list-check-width`, `--dnet-list-header-footer-height`, `--dnet-list-hover-background-color`, `--dnet-list-hover-border-radius`, `--dnet-list-item-height`, `--dnet-list-supporting-text-color`, `--pick-list-background`, `--pick-list-border`, `--pick-list-footer-height`, `--pick-list-foreground`, `--pick-list-header-height`, `--pick-list-item-background`, `--pick-list-item-height`, `--pick-list-item-hover`, `--pick-list-item-selected`, `--pick-list-items-max-height`, `--pick-list-muted`, `--pick-list-primary`, `--pick-list-radius`, `--pick-list-scrollbar`

</details>

```css
:root { --dnet-picklist-background: /* your value */; }
```
