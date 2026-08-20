# FloatingDoubleList

Components: `<DnetFloatingDoubleList>`

## `<DnetFloatingDoubleList>` — generic over TItem

| Parameter | Type | Default |
|---|---|---|
| `OnSearchLeft` | `EventCallback<SearchModel>` | — |
| `OnSearchRight` | `EventCallback<SearchModel>` | — |
| `OnSort` | `EventCallback<SearchModel>` | — |
| `RightItems` | `List<TItem>?` | — |
| `LeftItems` | `List<TItem>?` | — |
| `OnSelectionChanged` | `EventCallback<TransferredItems<TItem>>` | — |
| `ListItemContent` | `RenderFragment<TItem>?` | — |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — |
| `LeftListOptions` | `ListOptions<TItem>?` | — |
| `RightListOptions` | `ListOptions<TItem>?` | — |
| `ItemChildContent` | `RenderFragment?` | — |
| `Width` | `string?` | — |
| `Height` | `string?` | `"300px"` |
| `MinWidth` | `string?` | — |
| `MinHeight` | `string?` | — |
| `MaxWidth` | `string?` | — |
| `MaxHeight` | `string?` | — |
| `ItemHeight` | `int` | `40` |
| `BorderRadius` | `string` | `"5px"` |
| `MarginTop` | `string` | `"5px"` |
| `DebounceTime` | `int` | `300` |
| `InputItemHeight` | `int` | `34` |
| `InputTextToUpper` | `bool` | — |
| `ItemAutoSelection` | `bool` | — |
| `Disabled` | `bool` | — |
| `DisabledClearButton` | `bool` | — |
| `IsRequired` | `bool` | — |
| `SearchControlOptions` | `SearchControlOptions?` | — |
| `EqualityComparer` | `IEqualityComparer<TItem>?` | — |
| `DisplayValueConverter` | `Func<TItem, string>?` | — |
| `SideForSelections` | `ListToShow` | — |

## Minimal usage

```razor
<DnetFloatingDoubleList TTItem="..."
    RightItems="..."
    LeftItems="..."
    LeftListOptions="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-dialog-border-radius` | — |

```css
:root { --dnet-dialog-border-radius: /* your value */; }
```
