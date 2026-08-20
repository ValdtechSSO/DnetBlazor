# Tabs

Components: `<DnetTab>`, `<DnetTabBody>`, `<DnetTabGroup>`

## `<DnetTab>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `Label` | `string?` | — |
| `HeaderTemplate` | `Type?` | — |
| `Order` | `int` | — |
| `Disabled` | `bool` | — |
| `IsHidden` | `bool` | — |

## `<DnetTabBody>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment` | — |

## `<DnetTabGroup>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `SelectedTabId` | `int` | — |
| `TabHeaderBorder` | `bool` | — |
| `OnTabClicked` | `EventCallback<TabClikedEventData>` | — |

## Minimal usage

```razor
<DnetTab
    Label="..."
    HeaderTemplate="..."
    Order="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-tabs-background` | `var(--dnet-sys-surface)` |
| `--dnet-tabs-disabled-foreground` | `var(--dnet-sys-on-surface-muted)` |
| `--dnet-tabs-foreground` | `var(--dnet-sys-on-surface)` |
| `--dnet-tabs-indicator-color` | `var(--dnet-sys-primary-emphasis)` |

```css
:root { --dnet-tabs-background: /* your value */; }
```
