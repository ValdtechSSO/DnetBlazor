# List

## `<DnetList>` — generic over TItem

```razor
<DnetList TItem="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSelectionChange` | `EventCallback<List<TItem>>` | — | Raised when the selection changes. |
| `OnCheckboxClick` | `EventCallback<TItem>` | — | Raised when checkbox click occurs. |
| `OnPaginationChanged` | `EventCallback<SearchModel>` | — | Raised when pagination changed occurs. |
| `OnSearch` | `EventCallback<SearchModel>` | — | Raised when search occurs. |
| `OnSort` | `EventCallback<SearchModel>` | — | Raised when sort occurs. |
| `OnDragStart` | `EventCallback` | — | Raised when drag start occurs. |
| `OnDrop` | `EventCallback<List<TItem>>` | — | Raised when drop occurs. |
| `Items` | `ICollection<TItem>?` | — | Gets or sets the collection of items rendered by the component. |
| `ListItemContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for list item. |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item prefix. |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item sufix. |
| `ListOptions` | `ListOptions<TItem>` | `new()` | Gets or sets the list options used by this component. |
| `PlaceHolder` | `string` | `""` | Gets or sets placeholder text displayed when the input is empty. |
| `Label` | `string` | `""` | Gets or sets the label displayed for the component. |
| `UsePlainControl` | `bool` | `true` | Gets or sets whether the component use plain control. |
| `OnTransfer` | `EventCallback<TItem>` | — | Raised when transfer occurs. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-list-background-color` | `transparent` |
| `--dnet-list-border-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-list-border-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-list-box-shadow` | `none` |
| `--dnet-list-check-width` | `24px` |
| `--dnet-list-header-footer-height` | `50px` |
| `--dnet-list-headline-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-list-headline-text-color` | `inherit` |
| `--dnet-list-hover-background-color` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-list-hover-border-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-list-item-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-list-padding-horizontal` | `5px` |
| `--dnet-list-padding-vertical` | `4px` |
| `--dnet-list-prefix-suffix-min-width` | `40px` |
| `--dnet-list-supporting-text-color` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-list-supporting-text-font-size` | `0.75rem` <br><sub>via `--dnet-sys-text-sm`</sub> |
| `--dnet-list-wrapper-horizontal-padding` | `15px` |

```css
:root { --dnet-list-background-color: /* your value */; }
```
