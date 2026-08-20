# FloatingDoubleList

## `<DnetFloatingDoubleList>` — generic over TItem

```razor
<DnetFloatingDoubleList TItem="..."
    InputTextToUpper="..."
    ItemAutoSelection="..."
    Disabled="..."
    DisabledClearButton="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSearchLeft` | `EventCallback<SearchModel>` | — | Raised when search left occurs. |
| `OnSearchRight` | `EventCallback<SearchModel>` | — | Raised when search right occurs. |
| `OnSort` | `EventCallback<SearchModel>` | — | Raised when sort occurs. |
| `RightItems` | `List<TItem>?` | — | Gets or sets the right items used by this component. |
| `LeftItems` | `List<TItem>?` | — | Gets or sets the left items used by this component. |
| `OnSelectionChanged` | `EventCallback<TransferredItems<TItem>>` | — | Raised when the selection changes. |
| `ListItemContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for list item. |
| `ItemPrefixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item prefix. |
| `ItemSufixContent` | `RenderFragment<TItem>?` | — | Gets or sets content rendered for item sufix. |
| `LeftListOptions` | `ListOptions<TItem>?` | — | Gets or sets the left list options used by this component. |
| `RightListOptions` | `ListOptions<TItem>?` | — | Gets or sets the right list options used by this component. |
| `ItemChildContent` | `RenderFragment?` | — | Gets or sets content rendered for item child. |
| `Width` | `string?` | — | Gets or sets the component width. |
| `Height` | `string?` | `"300px"` | Gets or sets the component height. |
| `MinWidth` | `string?` | — | Gets or sets the minimum component width. |
| `MinHeight` | `string?` | — | Gets or sets the minimum component height. |
| `MaxWidth` | `string?` | — | Gets or sets the maximum component width. |
| `MaxHeight` | `string?` | — | Gets or sets the maximum component height. |
| `ItemHeight` | `int` | `40` | Gets or sets the item height used by this component. |
| `BorderRadius` | `string` | `"5px"` | Gets or sets the border radius used by this component. |
| `MarginTop` | `string` | `"5px"` | Gets or sets the margin top used by this component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `InputItemHeight` | `int` | `34` | Gets or sets the input item height used by this component. |
| `InputTextToUpper` | `bool` | — | Gets or sets the input text to upper used by this component. |
| `ItemAutoSelection` | `bool` | — | Gets or sets the item auto selection used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `DisabledClearButton` | `bool` | — | Gets or sets the disabled clear button used by this component. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `SearchControlOptions` | `SearchControlOptions?` | — | Gets or sets the search control options used by this component. |
| `EqualityComparer` | `IEqualityComparer<TItem>?` | — | Gets or sets the comparer used to match item values. |
| `DisplayValueConverter` | `Func<TItem, string>?` | — | Gets or sets the function that converts an item to display text. |
| `SideForSelections` | `ListToShow` | — | Gets or sets the side for selections used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-dialog-border-radius` | `5px` <br><sub>via `--dnet-sys-radius-md`</sub> |

```css
:root { --dnet-dialog-border-radius: /* your value */; }
```
