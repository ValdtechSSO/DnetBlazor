# Button

## `<DnetButton>`

```razor
<DnetButton
    InitialFocus="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `InitialFocus` | `bool` | — | Gets or sets whether the component receives focus after it is rendered. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched HTML attributes applied to the rendered element. |
| `OnClick` | `EventCallback` | — | Raised when the user clicks the component. |
| `ButtonType` | `string` | `"button"` | Gets or sets the HTML button type rendered by the component. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-btn-background` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-btn-background-hover` | `color-mix(in srgb, var(--dnet-sys-on-surface) 8%, transparent)` <br><sub>via `--dnet-sys-state-hover`</sub> |
| `--dnet-btn-focus-overlay` | `color-mix(in srgb, var(--dnet-sys-on-surface) 14%, transparent)` <br><sub>via `--dnet-sys-state-pressed`</sub> |
| `--dnet-btn-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-btn-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-btn-height` | `36px` |
| `--dnet-btn-margin` | `0` |
| `--dnet-btn-min-width` | `64px` |
| `--dnet-btn-padding` | `0 var(--dnet-sys-space-4)` |
| `--dnet-btn-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-button-background-color`, `--dnet-button-border-radius`, `--dnet-button-font-size`, `--dnet-button-hover-color`, `--dnet-button-line-height`, `--dnet-button-margin`, `--dnet-button-min-width`, `--dnet-button-padding`

</details>

```css
:root { --dnet-btn-background: /* your value */; }
```
