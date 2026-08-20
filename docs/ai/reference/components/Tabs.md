# Tabs

## `<DnetTab>`

```razor
<DnetTab
    Order="..."
    Disabled="..."
    IsHidden="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `Label` | `string?` | — | Gets or sets the label displayed for the component. |
| `HeaderTemplate` | `Type?` | — | Gets or sets the template used to render header. |
| `Order` | `int` | — | Gets or sets the order used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `IsHidden` | `bool` | — | Gets or sets whether hidden. |

## `<DnetTabBody>`

```razor
<DnetTabBody
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment` | — | Gets or sets the child content rendered by the component. |

## `<DnetTabGroup>`

```razor
<DnetTabGroup
    SelectedTabId="..."
    TabHeaderBorder="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `SelectedTabId` | `int` | — | Gets or sets the identifier of the selected tab. |
| `TabHeaderBorder` | `bool` | — | Gets or sets the tab header border used by this component. |
| `OnTabClicked` | `EventCallback<TabClikedEventData>` | — | Raised when tab clicked occurs. |
| `AllOtherAttributes` | `Dictionary<string, object>?` | — | Gets or sets the all other attributes used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-tabs-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-tabs-disabled-foreground` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-tabs-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-tabs-indicator-color` | `#3f51b5` <br><sub>via `--dnet-sys-primary-emphasis`</sub> |

```css
:root { --dnet-tabs-background: /* your value */; }
```
