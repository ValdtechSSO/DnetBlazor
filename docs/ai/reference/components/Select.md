# Select

## `<DnetSelect>` — generic over TItem, TValue

```razor
<DnetSelect TItem="..." TValue="..."
    InputTextToUpper="..."
    ItemAutoSelection="..."
    Disabled="..."
    DisabledClearButton="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnItemSelected` | `EventCallback<TItem>` | — | Raised when the user selects an item. |
| `OnSelectionChanged` | `EventCallback<List<TItem>>` | — | Raised when the selection changes. |
| `DisplayValueConverter` | `Func<TItem, string>?` | — | Gets or sets the function that converts an item to display text. |
| `SupportTextValueConverter` | `Func<TItem, string>?` | — | Gets or sets the support text value converter used by this component. |
| `ValueConverter` | `Expression<Func<TItem, TValue>>?` | — | Gets or sets the value converter used by this component. |
| `IsSelectedFn` | `Func<TItem, TItem, bool>?` | — | Gets or sets whether selected fn. |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item prefix. |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item sufix. |
| `ItemChildContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item child. |
| `ValueDisplayContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for value display. |
| `Items` | `List<TItem>` | `new()` | Gets or sets the collection of items rendered by the component. |
| `SelectedItems` | `List<TItem>` | `new()` | Gets or sets the currently selected items. |
| `Width` | `string?` | `"300px"` | Gets or sets the component width. |
| `Height` | `string?` | `"300px"` | Gets or sets the component height. |
| `MinWidth` | `string?` | — | Gets or sets the minimum component width. |
| `MinHeight` | `string?` | — | Gets or sets the minimum component height. |
| `MaxWidth` | `string?` | — | Gets or sets the maximum component width. |
| `MaxHeight` | `string?` | — | Gets or sets the maximum component height. |
| `ItemHeight` | `int` | `40` | Gets or sets the item height used by this component. |
| `BorderRadius` | `string` | `"5px"` | Gets or sets the border radius used by this component. |
| `MarginTop` | `string` | `"5px"` | Gets or sets the margin top used by this component. |
| `InputTextToUpper` | `bool` | — | Gets or sets the input text to upper used by this component. |
| `ItemAutoSelection` | `bool` | — | Gets or sets the item auto selection used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `Hint` | `string?` | — | Gets or sets the hint used by this component. |
| `DisabledClearButton` | `bool` | — | Gets or sets the disabled clear button used by this component. |
| `MultiSelect` | `bool` | — | Gets or sets whether the component multi select. |
| `ConfirmButtons` | `bool` | — | Gets or sets whether the component confirm buttons. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |
| `ResponsiveLabel` | `string?` | — | Gets or sets the responsive label used by this component. |
| `OverlayPanelClass` | `string?` | — | Gets or sets the overlay panel class used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-list-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-list-border-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-list-border-radius` | `5px` <br><sub>via `--dnet-sys-radius-md`</sub> |
| `--dnet-list-box-shadow` | `0 2px 1px -1px var(--dnet-sys-shadow-color), 0 1px 2px 0 var(--dnet-sys-shadow-color), 0 1px 10px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-1`</sub> |
| `--dnet-list-check-width` | `24px` |
| `--dnet-list-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-list-header-footer-height` | `50px` |
| `--dnet-list-headline-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-list-headline-text-color` | `var(--_foreground)` |
| `--dnet-list-hover-background-color` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-list-hover-border-radius` | `5px` <br><sub>via `--dnet-sys-radius-md`</sub> |
| `--dnet-list-item-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-list-padding-horizontal` | `5px` |
| `--dnet-list-padding-vertical` | `4px` |
| `--dnet-list-prefix-suffix-min-width` | `40px` |
| `--dnet-list-supporting-text-color` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-list-supporting-text-font-size` | `0.75rem` <br><sub>via `--dnet-sys-text-sm`</sub> |
| `--dnet-list-wrapper-horizontal-padding` | `15px` |
| `--dnet-select-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-select-list-border-color`, `--dnet-select-list-border-radius`, `--dnet-select-list-check-width`, `--dnet-select-list-foreground`, `--dnet-select-list-header-footer-height`, `--dnet-select-list-headline-font-size`, `--dnet-select-list-headline-text-color`, `--dnet-select-list-hover-background-color`, `--dnet-select-list-hover-border-radius`, `--dnet-select-list-item-height`, `--dnet-select-list-padding-horizontal`, `--dnet-select-list-padding-vertical`, `--dnet-select-list-prefix-suffix-min-width`, `--dnet-select-list-supporting-text-color`, `--dnet-select-list-supporting-text-font-size`, `--dnet-select-list-wrapper-horizontal-padding`

</details>

```css
:root { --dnet-list-background-color: /* your value */; }
```
