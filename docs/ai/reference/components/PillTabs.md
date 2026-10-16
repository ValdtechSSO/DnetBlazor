# PillTabs

## `<DnetPillTabs>` — generic over TValue

```razor
<DnetPillTabs TValue="..."
    Disabled="..."
    FullWidth="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<DnetPillTabItem<TValue>>` | `[]` | Gets or sets the tabs in their display order. Values must be unique and non-null. |
| `Value` | `TValue?` | — | Gets or sets the selected value. Omit this parameter to select the first enabled tab automatically. |
| `ValueChanged` | `EventCallback<TValue>` | — | Raised when the user requests a different selected value. Supports @bind-Value. |
| `AriaLabel` | `string` | `"Tabs"` | Gets or sets the accessible name of the tab list. |
| `Disabled` | `bool` | — | Gets or sets whether all tabs are disabled. |
| `FullWidth` | `bool` | — | Gets or sets whether the tab list fills its container with equally sized tabs. |
| `ItemTemplate` | `RenderFragment<DnetPillTabItem<TValue>>?` | — | Gets or sets custom label content. Counts are rendered separately. |
| `ChildContent` | `RenderFragment<DnetPillTabItem<TValue>>?` | — | Gets or sets the selected tab's panel content. Only the active panel's content is mounted. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched HTML attributes, including class, style, id and dir, on the component root. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-pill-tabs-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-pill-tabs-badge-background` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-pill-tabs-badge-selected-background` | `color-mix(in srgb, currentColor 22%, transparent)` |
| `--dnet-pill-tabs-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-pill-tabs-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-pill-tabs-hover-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 8%, transparent)` <br><sub>via `--dnet-sys-state-hover`</sub> |
| `--dnet-pill-tabs-padding` | `calc(var(--dnet-sys-space-unit) * 1)` <br><sub>via `--dnet-sys-space-1`</sub> |
| `--dnet-pill-tabs-radius` | `9999px` <br><sub>via `--dnet-sys-radius-pill`</sub> |
| `--dnet-pill-tabs-selected-background` | `#3f51b5` <br><sub>via `--dnet-sys-primary-emphasis`</sub> |
| `--dnet-pill-tabs-selected-foreground` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-pill-tabs-shadow` | `0 2px 1px -1px var(--dnet-sys-shadow-color), 0 1px 2px 0 var(--dnet-sys-shadow-color), 0 1px 10px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-1`</sub> |
| `--dnet-pill-tabs-tab-height` | `38px` <br><sub>via `--dnet-sys-control-height-sm`</sub> |

```css
:root { --dnet-pill-tabs-background: /* your value */; }
```
