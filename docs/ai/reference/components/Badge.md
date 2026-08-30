# Badge

## `<DnetBadge>`

```razor
<DnetBadge
    Disabled="..."
    Hidden="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the content decorated by the badge. |
| `Content` | `object?` | — | Gets or sets the short text or number displayed inside the badge. |
| `Position` | `DnetBadgePosition` | `DnetBadgePosition.AboveAfter` | Gets or sets the badge position. The default is above and after. |
| `Size` | `DnetBadgeSize` | `DnetBadgeSize.Medium` | Gets or sets the visual badge size. |
| `Color` | `DnetBadgeColor` | `DnetBadgeColor.Primary` | Gets or sets the semantic badge color. |
| `Overlap` | `bool` | `true` | Gets or sets whether the badge overlaps the decorated content. |
| `Disabled` | `bool` | — | Gets or sets whether the badge is visually disabled. |
| `Hidden` | `bool` | — | Gets or sets whether the visual badge is hidden. |
| `Description` | `string?` | — | Gets or sets an accessible description for the badge state. The visual indicator is hidden from assistive technologies to avoid duplicate output. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets additional attributes applied to the badge host. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-badge-background` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-badge-disabled-opacity` | `0.38` |
| `--dnet-badge-foreground` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-badge-outline-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |

```css
:root { --dnet-badge-background: /* your value */; }
```
