# List

Components: `<DnetList>`

## `<DnetList>` — generic over TItem

| Parameter | Type | Default |
|---|---|---|
| `OnSelectionChange` | `EventCallback<List<TItem>>` | — |
| `OnCheckboxClick` | `EventCallback<TItem>` | — |
| `OnPaginationChanged` | `EventCallback<SearchModel>` | — |
| `OnSearch` | `EventCallback<SearchModel>` | — |
| `OnSort` | `EventCallback<SearchModel>` | — |
| `OnDragStart` | `EventCallback` | — |
| `OnDrop` | `EventCallback<List<TItem>>` | — |
| `Items` | `ICollection<TItem>?` | — |
| `ListItemContent` | `RenderFragment<TItem>?` | — |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — |
| `ListOptions` | `ListOptions<TItem>` | `new()` |
| `PlaceHolder` | `string` | `""` |
| `Label` | `string` | `""` |
| `UsePlainControl` | `bool` | `true` |
| `OnTransfer` | `EventCallback<TItem>` | — |

## Minimal usage

```razor
<DnetList TTItem="..."
    Items="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-list-background-color` | `transparent` |
| `--dnet-list-border-color` | `var(--dnet-sys-border)` |
| `--dnet-list-border-radius` | `var(--dnet-sys-radius-lg)` |
| `--dnet-list-box-shadow` | `none` |
| `--dnet-list-check-width` | `24px` |
| `--dnet-list-header-footer-height` | `50px` |
| `--dnet-list-headline-font-size` | `var(--dnet-sys-text-md)` |
| `--dnet-list-headline-text-color` | `inherit` |
| `--dnet-list-hover-background-color` | `var(--dnet-sys-surface-hover)` |
| `--dnet-list-hover-border-radius` | `var(--dnet-sys-radius-lg)` |
| `--dnet-list-item-height` | `var(--dnet-sys-control-height)` |
| `--dnet-list-padding-horizontal` | `5px` |
| `--dnet-list-padding-vertical` | `4px` |
| `--dnet-list-prefix-suffix-min-width` | `40px` |
| `--dnet-list-supporting-text-color` | `var(--dnet-sys-on-surface-muted)` |
| `--dnet-list-supporting-text-font-size` | `var(--dnet-sys-text-sm)` |
| `--dnet-list-wrapper-horizontal-padding` | `15px` |

```css
:root { --dnet-list-background-color: /* your value */; }
```
