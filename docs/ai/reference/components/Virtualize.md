# Virtualize

## `<Virtualize>` — generic over TItem

```razor
<Virtualize TItem="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment<TItem>?` | — | Gets or sets the item template for the list. |
| `ItemContent` | `RenderFragment<TItem>?` | — | Gets or sets the item template for the list. |
| `Placeholder` | `RenderFragment<PlaceholderContext>?` | — | Gets or sets the template for items that have not yet been loaded in memory. |
| `ItemSize` | `float` | `50f` | Gets the size of each item in pixels. Defaults to 50px. |
| `ItemsProvider` | `ItemsProviderDelegate<TItem>?` | — | Gets or sets the function providing items to the list. |
| `Items` | `ICollection<TItem>?` | — | Gets or sets the fixed item source. |
| `OverscanCount` | `int` | `3` | Gets or sets a value that determines how many additional items will be rendered before and after the visible region. This help to reduce the frequency of rendering during scrolling. However, higher values mean that more elements will be present in the page. |
