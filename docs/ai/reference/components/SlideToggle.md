# SlideToggle

An accessible on/off switch for editing bool values.

## `<DnetSlideToggle>`

```razor
<DnetSlideToggle
    Disabled="..."
    TextPlacedBefore="..."
    FullWidth="..."
    Required="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the label rendered beside the switch. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the switch is disabled. |
| `TextPlacedBefore` | `bool` | — | Gets or sets whether the label is rendered before the switch. |
| `FullWidth` | `bool` | — | Gets or sets whether the switch and label fill the available width. |
| `Required` | `bool` | — | Gets or sets whether the underlying form control is required. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-slide-toggle-focus-ring` | `#42b0d5` <br><sub>via `--dnet-sys-focus-ring`</sub> |
| `--dnet-slide-toggle-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-slide-toggle-gap` | `calc(var(--dnet-sys-space-unit) * 3)` <br><sub>via `--dnet-sys-space-3`</sub> |
| `--dnet-slide-toggle-handle-background` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-slide-toggle-handle-background-checked` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-slide-toggle-handle-size` | `24px` |
| `--dnet-slide-toggle-label-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-slide-toggle-track-background` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-slide-toggle-track-background-checked` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-slide-toggle-track-height` | `32px` |
| `--dnet-slide-toggle-track-width` | `52px` |

```css
:root { --dnet-slide-toggle-handle-background: /* your value */; }
```
