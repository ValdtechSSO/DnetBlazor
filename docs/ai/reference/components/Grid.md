# Grid

## `<BlgAdvancedFilterToggle>`

```razor
<BlgAdvancedFilterToggle
    IsFiltered="..."
    DefaultAdvancedFilterOperator="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `FilterData` | `AdvancedFilterModel?` | — | Gets or sets the filter data used by this component. |
| `IsFiltered` | `bool` | — | Gets or sets whether filtered. |
| `DefaultAdvancedFilterOperator` | `FilterOperator` | — | Gets or sets the default advanced filter operator used by this component. |
| `OnFilter` | `EventCallback<bool>` | — | Raised when filter occurs. |
| `DateFormat` | `string?` | — | Gets or sets the format used to display date values. |

## `<BlgBody>` — generic over TItem

```razor
<BlgBody TItem="..."
    RowNodes="..."
    GridColumns="..."
    GroupGridColumn="..."
    GridOptions="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `RowNodes` | `List<RowNode<TItem>>` | — | Gets or sets the row nodes used by this component. |
| `GridColumns` | `List<GridColumn<TItem>>` | — | Gets or sets the grid columns used by this component. |
| `GroupGridColumn` | `GridColumn<TItem>` | — | Gets or sets the group grid column used by this component. |
| `GridOptions` | `GridOptions<TItem>` | — | Gets or sets the grid options used by this component. |
| `GridApi` | `GridApi<TItem>` | — | Gets or sets the grid api used by this component. |
| `HasGrouping` | `bool` | — | Gets or sets the has grouping used by this component. |
| `ComponentName` | `BlGridMessageEmitter` | — | Gets or sets the component name used by this component. |
| `OnChangeExpanded` | `EventCallback<long>` | — | Raised when change expanded occurs. |
| `OnCellClicked` | `EventCallback<CellClikedEventData>` | — | Raised when cell clicked occurs. |
| `OnRowClicked` | `EventCallback<long>` | — | Raised when row clicked occurs. |
| `OnRowDoubleClicked` | `EventCallback<long>` | — | Raised when row double clicked occurs. |
| `OnSelectionChanged` | `EventCallback<List<long>>` | — | Raised when the selection changes. |
| `OnMouseOver` | `EventCallback<long>` | — | Raised when mouse over occurs. |
| `Pinned` | `Pinned` | — | Gets or sets the pinned used by this component. |
| `CheckboxSelectionPinned` | `bool` | — | Gets or sets the checkbox selection pinned used by this component. |
| `RowIndexOffset` | `int` | — | Gets or sets the row index offset used by this component. |

## `<BlgGrid>` — generic over TItem

```razor
<BlgGrid TItem="..."
    HasGrouping="..."
    ItemsCount="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnCellClicked` | `EventCallback<CellClikedData<TItem>>` | — | Raised when cell clicked occurs. |
| `OnRowClicked` | `EventCallback<RowNode<TItem>>` | — | Raised when row clicked occurs. |
| `OnRowDoubleClicked` | `EventCallback<RowNode<TItem>>` | — | Raised when row double clicked occurs. |
| `OnSelectionChanged` | `EventCallback<List<RowNode<TItem>>>` | — | Raised when the selection changes. |
| `OnPaginationChanged` | `EventCallback<SearchModel>` | — | Raised when pagination changed occurs. |
| `OnSortingChanged` | `EventCallback<SearchModel>` | — | Raised when sorting changed occurs. |
| `OnFilterChanged` | `EventCallback<SearchModel>` | — | Raised when filter changed occurs. |
| `OnAdvancedFilterChanged` | `EventCallback<SearchModel>` | — | Raised when advanced filter changed occurs. |
| `OnGroupingChanged` | `EventCallback<GroupModel>` | — | Raised when grouping changed occurs. |
| `GridData` | `IEnumerable<TItem>` | `new List<TItem>()` | Gets or sets the grid data used by this component. |
| `GridColumns` | `List<GridColumn<TItem>>` | `new()` | Gets or sets the grid columns used by this component. |
| `GroupGridColumn` | `GridColumn<TItem>` | `new()` | Gets or sets the group grid column used by this component. |
| `GridOptions` | `GridOptions<TItem>` | `new()` | Gets or sets the grid options used by this component. |
| `HasGrouping` | `bool` | — | Gets or sets the has grouping used by this component. |
| `ItemsCount` | `bool` | — | Gets or sets the items count used by this component. |
| `OverscanCount` | `int` | `-1` | Compatibility alias for GridOptions{TItem}.OverscanCount. Prefer configuring the value in GridOptions so all Grid options live in one place. |
| `PaginatorHeight` | `int` | `50` | Gets or sets the paginator height used by this component. |

## `<BlgHeader>` — generic over TItem

```razor
<BlgHeader TItem="..."
    GridColumns="..."
    GroupGridColumn="..."
    GroupByColumns="..."
    HasGrouping="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `GridColumns` | `List<GridColumn<TItem>>` | — | Gets or sets the grid columns used by this component. |
| `GroupGridColumn` | `GridColumn<TItem>` | — | Gets or sets the group grid column used by this component. |
| `GroupByColumns` | `List<string>` | — | Gets or sets the group by columns used by this component. |
| `HasGrouping` | `bool` | — | Gets or sets the has grouping used by this component. |
| `IsExpanded` | `bool` | — | Gets or sets whether expanded. |
| `CheckboxSelectionPinned` | `bool` | — | Gets or sets the checkbox selection pinned used by this component. |
| `Pinned` | `Pinned` | — | Gets or sets the pinned used by this component. |
| `GridOptions` | `GridOptions<TItem>` | — | Gets or sets the grid options used by this component. |
| `OnFilter` | `EventCallback` | — | Raised when filter occurs. |
| `OnAdvancedFilter` | `EventCallback` | — | Raised when advanced filter occurs. |
| `OnSort` | `EventCallback<string>` | — | Raised when sort occurs. |
| `OnAddGroup` | `EventCallback<string>` | — | Raised when add group occurs. |
| `OnDeleteGroup` | `EventCallback<string>` | — | Raised when delete group occurs. |
| `OnChangeSelectAllNodes` | `EventCallback<bool>` | — | Raised when change select all nodes occurs. |
| `OnMouseDown` | `EventCallback<Tuple<string, int>>` | — | Raised when mouse down occurs. |
| `OnHeaderWithChange` | `EventCallback<int>` | — | Raised when header with change occurs. |
| `OnExpandCollapse` | `EventCallback<bool>` | — | Raised when expand collapse occurs. |

## `<BlgRow>` — generic over TItem

```razor
<BlgRow TItem="..."
    RowId="..."
    HasGrouping="..."
    Pinned="..."
    CheckboxSelectionPinned="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnCellClicked` | `EventCallback<CellClikedEventData>` | — | Raised when cell clicked occurs. |
| `OnRowClicked` | `EventCallback<long>` | — | Raised when row clicked occurs. |
| `OnRowDoubleClicked` | `EventCallback<long>` | — | Raised when row double clicked occurs. |
| `OnChangeExpanded` | `EventCallback<long>` | — | Raised when change expanded occurs. |
| `OnCheckboxClicked` | `EventCallback<long>` | — | Raised when checkbox clicked occurs. |
| `OnMouseOver` | `EventCallback<long>` | — | Raised when mouse over occurs. |
| `RowNode` | `RowNode<TItem>?` | — | Gets or sets the row node used by this component. |
| `RowId` | `int` | — | Gets or sets the row id used by this component. |
| `GridColumns` | `List<GridColumn<TItem>>?` | — | Gets or sets the grid columns used by this component. |
| `GroupGridColumn` | `GridColumn<TItem>?` | — | Gets or sets the group grid column used by this component. |
| `GridOptions` | `GridOptions<TItem>?` | — | Gets or sets the grid options used by this component. |
| `GridApi` | `GridApi<TItem>?` | — | Gets or sets the grid api used by this component. |
| `HasGrouping` | `bool` | — | Gets or sets the has grouping used by this component. |
| `Pinned` | `Pinned` | — | Gets or sets the pinned used by this component. |
| `CheckboxSelectionPinned` | `bool` | — | Gets or sets the checkbox selection pinned used by this component. |
| `IsInitialFocusCell` | `bool` | — | Gets or sets whether initial focus cell. |
| `ComponentName` | `BlGridMessageEmitter` | — | Gets or sets the component name used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-grid-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-grid-border-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-grid-cell-border-bottom` | `1px solid var(--dnet-sys-border)` |
| `--dnet-grid-cell-default-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-grid-cell-horizontal-padding` | `12px` |
| `--dnet-grid-cell-pinnedleft-border-bottom` | `1px solid var(--dnet-sys-border)` |
| `--dnet-grid-cell-pinnedright-border-bottom` | `1px solid var(--dnet-sys-border)` |
| `--dnet-grid-clicked-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 14%, transparent)` <br><sub>via `--dnet-sys-state-pressed`</sub> |
| `--dnet-grid-disabled-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 38%, transparent)` <br><sub>via `--dnet-sys-state-disabled-fg`</sub> |
| `--dnet-grid-focus-color` | `currentColor` |
| `--dnet-grid-font-family` | `"Roboto", "Helvetica Neue", Helvetica, Arial, sans-serif` <br><sub>via `--dnet-sys-font`</sub> |
| `--dnet-grid-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-grid-font-weight` | `400` |
| `--dnet-grid-foreground-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-grid-grid-background-color` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-grid-grid-foreground-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 41%, var(--dnet-sys-surface))` |
| `--dnet-grid-grid-scroll-width` | `6px` |
| `--dnet-grid-header-background-color` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-grid-header-foreground-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-grid-headercell-default-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-grid-hover-color` | `color-mix(in srgb, var(--dnet-sys-primary) 8%, var(--dnet-sys-surface))` |
| `--dnet-grid-odd-row-background-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 2%, var(--dnet-sys-surface))` |
| `--dnet-grid-range-selection-background-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 40%, transparent)` |
| `--dnet-grid-root-wrapper-border` | `1px solid var(--dnet-sys-transparent)` |
| `--dnet-grid-row-height` | `40px` |
| `--dnet-grid-secondary-font-family` | `"Roboto", "Helvetica Neue", Helvetica, Arial, sans-serif` <br><sub>via `--dnet-sys-font`</sub> |
| `--dnet-grid-secondary-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-grid-secondary-font-weight` | `400` |
| `--dnet-grid-selected-color` | `color-mix(in srgb, var(--dnet-sys-primary) 8%, var(--dnet-sys-surface))` |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--blg-background-color`, `--blg-border-color`, `--blg-cell-border-bottom`, `--blg-cell-default-background-color`, `--blg-cell-horizontal-padding`, `--blg-cell-pinnedleft-border-bottom`, `--blg-cell-pinnedright-border-bottom`, `--blg-clicked-color`, `--blg-disabled-color`, `--blg-focus-color`, `--blg-font-family`, `--blg-font-size`, `--blg-font-weight`, `--blg-foreground-color`, `--blg-grid-background-color`, `--blg-grid-foreground-color`, `--blg-grid-scroll-width`, `--blg-header-background-color`, `--blg-header-foreground-color`, `--blg-headercell-default-background-color`, `--blg-hover-color`, `--blg-odd-row-background-color`, `--blg-range-selection-background-color`, `--blg-root-wrapper-border`, `--blg-row-height`, `--blg-secondary-font-family`, `--blg-secondary-font-size`, `--blg-secondary-font-weight`, `--blg-selected-color`

</details>

```css
:root { --dnet-grid-background-color: /* your value */; }
```
