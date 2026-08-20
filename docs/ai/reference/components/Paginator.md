# Paginator

Components: `<DnetPaginator>`

## `<DnetPaginator>`

| Parameter | Type | Default |
|---|---|---|
| `OnGoToFirstPage` | `EventCallback<int>` | — |
| `OnGoToPreviousPage` | `EventCallback<int>` | — |
| `OnGoToNextPage` | `EventCallback<int>` | — |
| `OnGoToLastPage` | `EventCallback<int>` | — |
| `OnGoToSpecificPage` | `EventCallback<int>` | — |
| `CurrentPage` | `int` | — |
| `PageSize` | `int` | — |
| `TotalItems` | `int` | — |
| `Disabled` | `bool` | — |
| `PaginationLabel` | `string` | `"Pagination"` |
| `PageText` | `string` | `"Page"` |
| `OfText` | `string` | `"of"` |
| `PageInputLabel` | `string` | `"Current page"` |
| `FirstPageLabel` | `string` | `"First page"` |
| `PreviousPageLabel` | `string` | `"Previous page"` |
| `NextPageLabel` | `string` | `"Next page"` |
| `LastPageLabel` | `string` | `"Last page"` |
| `PageNumberZone` | `int` | `2` |
| `ControlsZone` | `int` | `3` |
| `RangeZone` | `int` | `4` |
| `OneRow` | `bool` | `false` |
| `GridGap` | `int` | `20` |

## Minimal usage

```razor
<DnetPaginator
    CurrentPage="..."
    PageSize="..."
    TotalItems="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-icon-button-border-radius` | `50%` |
| `--dnet-icon-button-size` | `25px` |
| `--dnet-paginator-items-per-page-label-margin` | `0 0px` |
| `--dnet-paginator-page-size-margin-right` | `0px` |
| `--dnet-paginator-selector-margin` | `6px 4px 0 4px` |
| `--dnet-paginator-selector-trigger-fill-width` | `64px` |
| `--dnet-paginator-selector-trigger-outline-width` | `64px` |
| `--dnet-paginator-selector-trigger-width` | `56px` |

```css
:root { --dnet-icon-button-border-radius: /* your value */; }
```
