# Autocomplete

Components: `<DnetAutocomplete>`

## `<DnetAutocomplete>` — generic over TItem, TValue

| Parameter | Type | Default |
|---|---|---|
| `OnItemSelected` | `EventCallback<TItem>` | — |
| `OnStopTyping` | `EventCallback<string>` | — |
| `OnFocus` | `EventCallback<string>` | — |
| `OnFocusin` | `EventCallback<string>` | — |
| `OnClearInput` | `EventCallback<bool>` | — |
| `DisplayValueConverter` | `Func<TItem, string>?` | — |
| `ValueConverter` | `Expression<Func<TItem, TValue>>?` | — |
| `SortBy` | `CustomSort<TItem>?` | — |
| `FilterBy` | `CustomFilter<TItem>?` | — |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — |
| `ItemChildContent` | `RenderFragment<TItem>?` | — |
| `FooterContent` | `RenderFragment<TItem>?` | — |
| `Items` | `List<TItem>` | `new()` |
| `OrderBy` | `string` | `"ASC"` |
| `Width` | `string?` | — |
| `Height` | `string?` | `"300px"` |
| `MinWidth` | `string?` | `"300px"` |
| `MinHeight` | `string?` | — |
| `MaxWidth` | `string?` | — |
| `MaxHeight` | `string?` | — |
| `MarginTop` | `string` | `"5px"` |
| `ItemHeight` | `int` | `50` |
| `DebounceTime` | `int` | `300` |
| `OverscanCount` | `int` | `3` |
| `InputTextToUpper` | `bool` | — |
| `ItemAutoSelection` | `bool` | — |
| `Disabled` | `bool` | — |
| `EnableServerSide` | `bool` | — |
| `OrderStartingBySearchedString` | `bool` | `false` |
| `IsRequired` | `bool` | — |
| `PlaceHolder` | `string` | `string.Empty` |
| `OverlayPanelClass` | `string?` | — |
| `PanelLabel` | `string` | `string.Empty` |

## Minimal usage

```razor
<DnetAutocomplete TTItem TTValue="..."
    DisplayValueConverter="..."
    ValueConverter="..."
    SortBy="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-autocomplete-list-background-color` | — |
| `--dnet-autocomplete-list-border-color` | — |
| `--dnet-autocomplete-list-border-radius` | — |
| `--dnet-autocomplete-list-box-shadow` | — |
| `--dnet-autocomplete-list-check-width` | — |
| `--dnet-autocomplete-list-header-footer-height` | — |
| `--dnet-autocomplete-list-headline-font-size` | — |
| `--dnet-autocomplete-list-headline-text-color` | — |
| `--dnet-autocomplete-list-hover-background-color` | — |
| `--dnet-autocomplete-list-hover-border-radius` | — |
| `--dnet-autocomplete-list-item-height` | — |
| `--dnet-autocomplete-list-padding-horizontal` | — |
| `--dnet-autocomplete-list-padding-vertical` | — |
| `--dnet-autocomplete-list-prefix-suffix-min-width` | — |
| `--dnet-autocomplete-list-supporting-text-color` | — |
| `--dnet-autocomplete-list-supporting-text-font-size` | — |
| `--dnet-autocomplete-list-wrapper-horizontal-padding` | — |
| `--dnet-list-background-color` | `var(--dnet-autocomplete-list-background-color, var(--dnet-sys-surface))` |
| `--dnet-list-border-color` | `var(--dnet-autocomplete-list-border-color, var(--dnet-sys-border))` |
| `--dnet-list-border-radius` | `var(--dnet-autocomplete-list-border-radius, var(--dnet-sys-radius-sm))` |
| `--dnet-list-box-shadow` | `var(--dnet-autocomplete-list-box-shadow, var(--dnet-sys-elevation-1))` |
| `--dnet-list-check-width` | `var(--dnet-autocomplete-list-check-width, 24px)` |
| `--dnet-list-header-footer-height` | `var(--dnet-autocomplete-list-header-footer-height, 50px)` |
| `--dnet-list-headline-font-size` | `var(--dnet-autocomplete-list-headline-font-size, var(--dnet-sys-text-md))` |
| `--dnet-list-headline-text-color` | `var(--dnet-autocomplete-list-headline-text-color, var(--dnet-sys-on-surface))` |
| `--dnet-list-hover-background-color` | `var(--dnet-autocomplete-list-hover-background-color, var(--dnet-sys-surface-hover))` |
| `--dnet-list-hover-border-radius` | `var(--dnet-autocomplete-list-hover-border-radius, var(--dnet-sys-radius-lg))` |
| `--dnet-list-item-height` | `var(--dnet-autocomplete-list-item-height, var(--dnet-sys-control-height))` |
| `--dnet-list-padding-horizontal` | `var(--dnet-autocomplete-list-padding-horizontal, 5px)` |
| `--dnet-list-padding-vertical` | `var(--dnet-autocomplete-list-padding-vertical, 4px)` |
| `--dnet-list-prefix-suffix-min-width` | `var(--dnet-autocomplete-list-prefix-suffix-min-width, 40px)` |
| `--dnet-list-supporting-text-color` | `var(--dnet-autocomplete-list-supporting-text-color, var(--dnet-sys-on-surface-muted))` |
| `--dnet-list-supporting-text-font-size` | `var(--dnet-autocomplete-list-supporting-text-font-size, var(--dnet-sys-text-xs))` |
| `--dnet-list-wrapper-horizontal-padding` | `var(--dnet-autocomplete-list-wrapper-horizontal-padding, 15px)` |

```css
:root { --dnet-autocomplete-list-background-color: /* your value */; }
```
