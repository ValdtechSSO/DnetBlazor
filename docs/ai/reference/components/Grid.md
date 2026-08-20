# Grid

Components: `<BlgAdvancedFilterToggle>`, `<BlgBody>`, `<BlgHeader>`, `<BlgRow>`

## `<BlgAdvancedFilterToggle>`

| Parameter | Type | Default |
|---|---|---|
| `FilterData` | `AdvancedFilterModel?` | — |
| `IsFiltered` | `bool` | — |
| `DefaultAdvancedFilterOperator` | `FilterOperator` | — |
| `OnFilter` | `EventCallback<bool>` | — |
| `DateFormat` | `string?` | — |

## `<BlgBody>` — generic over TItem

| Parameter | Type | Default |
|---|---|---|
| `RowNodes` | `List<RowNode<TItem>>` | — |
| `GridColumns` | `List<GridColumn<TItem>>` | — |
| `GroupGridColumn` | `GridColumn<TItem>` | — |
| `GridOptions` | `GridOptions<TItem>` | — |
| `GridApi` | `GridApi<TItem>` | — |
| `HasGrouping` | `bool` | — |
| `ComponentName` | `BlGridMessageEmitter` | — |
| `OnChangeExpanded` | `EventCallback<long>` | — |
| `OnCellClicked` | `EventCallback<CellClikedEventData>` | — |
| `OnRowClicked` | `EventCallback<long>` | — |
| `OnRowDoubleClicked` | `EventCallback<long>` | — |
| `OnSelectionChanged` | `EventCallback<List<long>>` | — |
| `OnMouseOver` | `EventCallback<long>` | — |
| `Pinned` | `Pinned` | — |
| `CheckboxSelectionPinned` | `bool` | — |
| `RowIndexOffset` | `int` | — |

## `<BlgHeader>` — generic over TItem

| Parameter | Type | Default |
|---|---|---|
| `GridColumns` | `List<GridColumn<TItem>>` | — |
| `GroupGridColumn` | `GridColumn<TItem>` | — |
| `GroupByColumns` | `List<string>` | — |
| `HasGrouping` | `bool` | — |
| `IsExpanded` | `bool` | — |
| `CheckboxSelectionPinned` | `bool` | — |
| `Pinned` | `Pinned` | — |
| `GridOptions` | `GridOptions<TItem>` | — |
| `OnFilter` | `EventCallback` | — |
| `OnAdvancedFilter` | `EventCallback` | — |
| `OnSort` | `EventCallback<string>` | — |
| `OnAddGroup` | `EventCallback<string>` | — |
| `OnDeleteGroup` | `EventCallback<string>` | — |
| `OnChangeSelectAllNodes` | `EventCallback<bool>` | — |
| `OnMouseDown` | `EventCallback<Tuple<string, int>>` | — |
| `OnHeaderWithChange` | `EventCallback<int>` | — |
| `OnExpandCollapse` | `EventCallback<bool>` | — |

## `<BlgRow>` — generic over TItem

| Parameter | Type | Default |
|---|---|---|
| `OnCellClicked` | `EventCallback<CellClikedEventData>` | — |
| `OnRowClicked` | `EventCallback<long>` | — |
| `OnRowDoubleClicked` | `EventCallback<long>` | — |
| `OnChangeExpanded` | `EventCallback<long>` | — |
| `OnCheckboxClicked` | `EventCallback<long>` | — |
| `OnMouseOver` | `EventCallback<long>` | — |
| `RowNode` | `RowNode<TItem>?` | — |
| `RowId` | `int` | — |
| `GridColumns` | `List<GridColumn<TItem>>?` | — |
| `GroupGridColumn` | `GridColumn<TItem>?` | — |
| `GridOptions` | `GridOptions<TItem>?` | — |
| `GridApi` | `GridApi<TItem>?` | — |
| `HasGrouping` | `bool` | — |
| `Pinned` | `Pinned` | — |
| `CheckboxSelectionPinned` | `bool` | — |
| `IsInitialFocusCell` | `bool` | — |
| `ComponentName` | `BlGridMessageEmitter` | — |

## Minimal usage

```razor
<BlgAdvancedFilterToggle
    FilterData="..."
    IsFiltered="..."
    DefaultAdvancedFilterOperator="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-grid-background-color` | — |
| `--dnet-grid-border-color` | — |
| `--dnet-grid-cell-border-bottom` | — |
| `--dnet-grid-cell-default-background-color` | — |
| `--dnet-grid-cell-horizontal-padding` | — |
| `--dnet-grid-cell-pinnedleft-border-bottom` | — |
| `--dnet-grid-cell-pinnedright-border-bottom` | — |
| `--dnet-grid-clicked-color` | — |
| `--dnet-grid-disabled-color` | — |
| `--dnet-grid-focus-color` | — |
| `--dnet-grid-font-family` | — |
| `--dnet-grid-font-size` | — |
| `--dnet-grid-font-weight` | — |
| `--dnet-grid-foreground-color` | — |
| `--dnet-grid-grid-background-color` | — |
| `--dnet-grid-grid-foreground-color` | — |
| `--dnet-grid-grid-scroll-width` | — |
| `--dnet-grid-header-background-color` | — |
| `--dnet-grid-header-foreground-color` | — |
| `--dnet-grid-headercell-default-background-color` | — |
| `--dnet-grid-hover-color` | — |
| `--dnet-grid-odd-row-background-color` | — |
| `--dnet-grid-range-selection-background-color` | — |
| `--dnet-grid-root-wrapper-border` | — |
| `--dnet-grid-row-height` | — |
| `--dnet-grid-secondary-font-family` | — |
| `--dnet-grid-secondary-font-size` | — |
| `--dnet-grid-secondary-font-weight` | — |
| `--dnet-grid-selected-color` | — |

```css
:root { --dnet-grid-background-color: /* your value */; }
```
