# Spoiler

## `<DnetSpoiler>`

```razor
<DnetSpoiler
    Expanded="..."
    DefaultExpanded="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `MaxHeight` | `int` | `100` | Gets or sets the visible height, in pixels, when collapsed. Zero hides the entire preview. |
| `Expanded` | `bool` | — | Gets or sets the controlled expanded state. Use @bind-Expanded to accept user changes. |
| `ExpandedChanged` | `EventCallback<bool>` | — | Raised when the user requests a different expanded state. |
| `DefaultExpanded` | `bool` | — | Gets or sets the initial state when Expanded is omitted. Later changes do not reset user selection. |
| `ShowLabel` | `string` | `"Show more"` | Gets or sets the label of the button that reveals the full content. |
| `HideLabel` | `string` | `"Show less"` | Gets or sets the label of the button that collapses the content. |
| `AriaLabel` | `string?` | — | Gets or sets the accessible name of the content region. Omit it to avoid adding a region landmark. |
| `ShowAriaLabel` | `string?` | — | Gets or sets an optional accessible label for the collapsed state's button. |
| `HideAriaLabel` | `string?` | — | Gets or sets an optional accessible label for the expanded state's button. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the content. It stays mounted when collapsed. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched HTML attributes, including class, style, id and dir, on the component root. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-spoiler-control-color` | `color-mix(in srgb, var(--dnet-sys-primary-emphasis) 60%, var(--dnet-sys-on-surface))` |
| `--dnet-spoiler-control-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-spoiler-control-hover-color` | `color-mix(in srgb, var(--_control-color) 75%, var(--dnet-sys-on-surface))` |
| `--dnet-spoiler-gap` | `calc(var(--dnet-sys-space-unit) * 2)` <br><sub>via `--dnet-sys-space-2`</sub> |
| `--dnet-spoiler-transition-duration` | `200ms` <br><sub>via `--dnet-sys-motion-normal`</sub> |

```css
:root { --dnet-spoiler-control-color: /* your value */; }
```
