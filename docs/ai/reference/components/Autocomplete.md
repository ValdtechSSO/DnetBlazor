# Autocomplete

## `<DnetAutocomplete>` — generic over TItem, TValue

```razor
<DnetAutocomplete TItem="..." TValue="..."
    InputTextToUpper="..."
    ItemAutoSelection="..."
    Disabled="..."
    EnableServerSide="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnItemSelected` | `EventCallback<TItem>` | — | Raised when the user selects an item. |
| `OnStopTyping` | `EventCallback<string>` | — | Raised after the user stops typing. |
| `OnFocus` | `EventCallback<string>` | — | Raised when the component receives focus. |
| `OnFocusin` | `EventCallback<string>` | — | Raised when focus enters the component. |
| `OnClearInput` | `EventCallback<bool>` | — | Raised when the user clears the input. |
| `DisplayValueConverter` | `Func<TItem, string>?` | — | Gets or sets the function that converts an item to display text. |
| `ValueConverter` | `Expression<Func<TItem, TValue>>?` | — | Gets or sets the value converter used by this component. |
| `SortBy` | `CustomSort<TItem>?` | — | Gets or sets the sort by used by this component. |
| `FilterBy` | `CustomFilter<TItem>?` | — | Gets or sets the filter by used by this component. |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item prefix. |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item sufix. |
| `ItemChildContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item child. |
| `FooterContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered in the component footer. |
| `Items` | `List<TItem>` | `new()` | Gets or sets the collection of items rendered by the component. |
| `OrderBy` | `string` | `"ASC"` | Gets or sets the order by used by this component. |
| `Width` | `string?` | — | Gets or sets the component width. |
| `Height` | `string?` | `"300px"` | Gets or sets the component height. |
| `MinWidth` | `string?` | `"300px"` | Gets or sets the minimum component width. |
| `MinHeight` | `string?` | — | Gets or sets the minimum component height. |
| `MaxWidth` | `string?` | — | Gets or sets the maximum component width. |
| `MaxHeight` | `string?` | — | Gets or sets the maximum component height. |
| `MarginTop` | `string` | `"5px"` | Gets or sets the margin top used by this component. |
| `ItemHeight` | `int` | `50` | Gets or sets the item height used by this component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `OverscanCount` | `int` | `3` | Gets or sets the number of additional items rendered outside the visible viewport. |
| `InputTextToUpper` | `bool` | — | Gets or sets the input text to upper used by this component. |
| `ItemAutoSelection` | `bool` | — | Gets or sets the item auto selection used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `EnableServerSide` | `bool` | — | Gets or sets whether the component enable server side. |
| `OrderStartingBySearchedString` | `bool` | `false` | Gets or sets the order starting by searched string used by this component. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `PlaceHolder` | `string` | `string.Empty` | Gets or sets placeholder text displayed when the input is empty. |
| `OverlayPanelClass` | `string?` | — | Gets or sets the overlay panel class used by this component. |
| `PanelLabel` | `string` | `string.Empty` | Gets or sets the panel label used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-list-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-list-border-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-list-border-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |
| `--dnet-list-box-shadow` | `0 2px 1px -1px var(--dnet-sys-shadow-color), 0 1px 2px 0 var(--dnet-sys-shadow-color), 0 1px 10px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-1`</sub> |
| `--dnet-list-check-width` | `24px` |
| `--dnet-list-header-footer-height` | `50px` |
| `--dnet-list-headline-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-list-headline-text-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-list-hover-background-color` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-list-hover-border-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-list-item-height` | `50px` <br><sub>via `--dnet-sys-control-height`</sub> |
| `--dnet-list-padding-horizontal` | `5px` |
| `--dnet-list-padding-vertical` | `4px` |
| `--dnet-list-prefix-suffix-min-width` | `40px` |
| `--dnet-list-supporting-text-color` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-list-supporting-text-font-size` | `0.625rem` <br><sub>via `--dnet-sys-text-xs`</sub> |
| `--dnet-list-wrapper-horizontal-padding` | `15px` |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-autocomplete-list-background-color`, `--dnet-autocomplete-list-border-color`, `--dnet-autocomplete-list-border-radius`, `--dnet-autocomplete-list-box-shadow`, `--dnet-autocomplete-list-check-width`, `--dnet-autocomplete-list-header-footer-height`, `--dnet-autocomplete-list-headline-font-size`, `--dnet-autocomplete-list-headline-text-color`, `--dnet-autocomplete-list-hover-background-color`, `--dnet-autocomplete-list-hover-border-radius`, `--dnet-autocomplete-list-item-height`, `--dnet-autocomplete-list-padding-horizontal`, `--dnet-autocomplete-list-padding-vertical`, `--dnet-autocomplete-list-prefix-suffix-min-width`, `--dnet-autocomplete-list-supporting-text-color`, `--dnet-autocomplete-list-supporting-text-font-size`, `--dnet-autocomplete-list-wrapper-horizontal-padding`

</details>

```css
:root { --dnet-list-background-color: /* your value */; }
```
