# Paginator

## `<DnetPaginator>`

```razor
<DnetPaginator
    CurrentPage="..."
    PageSize="..."
    TotalItems="..."
    Disabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnGoToFirstPage` | `EventCallback<int>` | — | Raised when go to first page occurs. |
| `OnGoToPreviousPage` | `EventCallback<int>` | — | Raised when go to previous page occurs. |
| `OnGoToNextPage` | `EventCallback<int>` | — | Raised when go to next page occurs. |
| `OnGoToLastPage` | `EventCallback<int>` | — | Raised when go to last page occurs. |
| `OnGoToSpecificPage` | `EventCallback<int>` | — | Raised when go to specific page occurs. |
| `CurrentPage` | `int` | — | Gets or sets the current page used by this component. |
| `PageSize` | `int` | — | Gets or sets the number of items displayed on each page. |
| `TotalItems` | `int` | — | Gets or sets the total items used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `PaginationLabel` | `string` | `"Pagination"` | Gets or sets the pagination label used by this component. |
| `PageText` | `string` | `"Page"` | Gets or sets the page text used by this component. |
| `OfText` | `string` | `"of"` | Gets or sets the of text used by this component. |
| `PageInputLabel` | `string` | `"Current page"` | Gets or sets the page input label used by this component. |
| `FirstPageLabel` | `string` | `"First page"` | Gets or sets the first page label used by this component. |
| `PreviousPageLabel` | `string` | `"Previous page"` | Gets or sets the previous page label used by this component. |
| `NextPageLabel` | `string` | `"Next page"` | Gets or sets the next page label used by this component. |
| `LastPageLabel` | `string` | `"Last page"` | Gets or sets the last page label used by this component. |
| `PageNumberZone` | `int` | `2` | Gets or sets the page number zone used by this component. |
| `ControlsZone` | `int` | `3` | Gets or sets the controls zone used by this component. |
| `RangeZone` | `int` | `4` | Gets or sets the range zone used by this component. |
| `OneRow` | `bool` | `false` | Gets or sets the one row used by this component. |
| `GridGap` | `int` | `20` | Gets or sets the grid gap used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
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
