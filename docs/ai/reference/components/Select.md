# Select

Components: `<DnetSelect>`

## `<DnetSelect>` — generic over TItem, TValue

| Parameter | Type | Default |
|---|---|---|
| `OnItemSelected` | `EventCallback<TItem>` | — |
| `OnSelectionChanged` | `EventCallback<List<TItem>>` | — |
| `DisplayValueConverter` | `Func<TItem, string>?` | — |
| `SupportTextValueConverter` | `Func<TItem, string>?` | — |
| `ValueConverter` | `Expression<Func<TItem, TValue>>?` | — |
| `IsSelectedFn` | `Func<TItem, TItem, bool>?` | — |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — |
| `ItemChildContent` | `RenderFragment<TItem>?` | — |
| `ValueDisplayContent` | `RenderFragment<TItem>?` | — |
| `Items` | `List<TItem>` | `new()` |
| `SelectedItems` | `List<TItem>` | `new()` |
| `Width` | `string?` | `"300px"` |
| `Height` | `string?` | `"300px"` |
| `MinWidth` | `string?` | — |
| `MinHeight` | `string?` | — |
| `MaxWidth` | `string?` | — |
| `MaxHeight` | `string?` | — |
| `ItemHeight` | `int` | `40` |
| `BorderRadius` | `string` | `"5px"` |
| `MarginTop` | `string` | `"5px"` |
| `InputTextToUpper` | `bool` | — |
| `ItemAutoSelection` | `bool` | — |
| `Disabled` | `bool` | — |
| `Hint` | `string?` | — |
| `DisabledClearButton` | `bool` | — |
| `MultiSelect` | `bool` | — |
| `ConfirmButtons` | `bool` | — |
| `IsRequired` | `bool` | — |
| `PlaceHolder` | `string?` | — |
| `ResponsiveLabel` | `string?` | — |
| `OverlayPanelClass` | `string?` | — |

## Minimal usage

```razor
<DnetSelect TTItem TTValue="..."
    DisplayValueConverter="..."
    SupportTextValueConverter="..."
    ValueConverter="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-list-background-color` | `var(--dnet-sys-surface)` |
| `--dnet-list-border-color` | `var(--dnet-select-list-border-color, var(--dnet-sys-border))` |
| `--dnet-list-border-radius` | `var(--dnet-select-list-border-radius, var(--dnet-sys-radius-md))` |
| `--dnet-list-box-shadow` | `var(--dnet-sys-elevation-1)` |
| `--dnet-list-check-width` | `var(--dnet-select-list-check-width, 24px)` |
| `--dnet-list-foreground` | `var(--dnet-select-list-foreground, var(--dnet-sys-on-surface))` |
| `--dnet-list-header-footer-height` | `var(--dnet-select-list-header-footer-height, 50px)` |
| `--dnet-list-headline-font-size` | `var(--dnet-select-list-headline-font-size, var(--dnet-sys-text-md))` |
| `--dnet-list-headline-text-color` | `var(--dnet-select-list-headline-text-color, var(--_foreground))` |
| `--dnet-list-hover-background-color` | `var(--dnet-select-list-hover-background-color, var(--dnet-sys-surface-hover))` |
| `--dnet-list-hover-border-radius` | `var(--dnet-select-list-hover-border-radius, var(--dnet-sys-radius-md))` |
| `--dnet-list-item-height` | `var(--dnet-select-list-item-height, var(--dnet-sys-control-height))` |
| `--dnet-list-padding-horizontal` | `var(--dnet-select-list-padding-horizontal, 5px)` |
| `--dnet-list-padding-vertical` | `var(--dnet-select-list-padding-vertical, 4px)` |
| `--dnet-list-prefix-suffix-min-width` | `var(--dnet-select-list-prefix-suffix-min-width, 40px)` |
| `--dnet-list-supporting-text-color` | `var(--dnet-select-list-supporting-text-color, var(--dnet-sys-on-surface-muted))` |
| `--dnet-list-supporting-text-font-size` | `var(--dnet-select-list-supporting-text-font-size, var(--dnet-sys-text-sm))` |
| `--dnet-list-wrapper-horizontal-padding` | `var(--dnet-select-list-wrapper-horizontal-padding, 15px)` |
| `--dnet-select-foreground` | `var(--dnet-sys-on-surface)` |
| `--dnet-select-list-border-color` | — |
| `--dnet-select-list-border-radius` | — |
| `--dnet-select-list-check-width` | — |
| `--dnet-select-list-foreground` | — |
| `--dnet-select-list-header-footer-height` | — |
| `--dnet-select-list-headline-font-size` | — |
| `--dnet-select-list-headline-text-color` | — |
| `--dnet-select-list-hover-background-color` | — |
| `--dnet-select-list-hover-border-radius` | — |
| `--dnet-select-list-item-height` | — |
| `--dnet-select-list-padding-horizontal` | — |
| `--dnet-select-list-padding-vertical` | — |
| `--dnet-select-list-prefix-suffix-min-width` | — |
| `--dnet-select-list-supporting-text-color` | — |
| `--dnet-select-list-supporting-text-font-size` | — |
| `--dnet-select-list-wrapper-horizontal-padding` | — |

```css
:root { --dnet-list-background-color: /* your value */; }
```
