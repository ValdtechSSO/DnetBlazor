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
| `Variant` | `DnetSlideToggleVariant` | `DnetSlideToggleVariant.Standard` | Gets or sets how the switch paints its track and handle. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-slide-toggle-disabled-opacity` | `0.38` |
| `--dnet-slide-toggle-focus-outline` | `none` |
| `--dnet-slide-toggle-focus-ring` | `#5b5ce0` <br><sub>via `--dnet-sys-focus-ring`</sub> |
| `--dnet-slide-toggle-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-slide-toggle-gap` | `calc(var(--dnet-sys-space-unit) * 3)` <br><sub>via `--dnet-sys-space-3`</sub> |
| `--dnet-slide-toggle-halo-size` | `8px` |
| `--dnet-slide-toggle-handle-background` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-slide-toggle-handle-background-checked` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-slide-toggle-handle-shadow` | `var(--dnet-sys-material-highlight), var(--dnet-sys-elevation-1)` |
| `--dnet-slide-toggle-handle-size` | `24px` |
| `--dnet-slide-toggle-icon-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-slide-toggle-label-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-slide-toggle-lens-handle-backdrop` | `none` |
| `--dnet-slide-toggle-lens-handle-background` | `var(--_base-handle)` |
| `--dnet-slide-toggle-lens-handle-shadow` | `var(--_base-handle-shadow)` |
| `--dnet-slide-toggle-lens-handle-size` | `var(--_base-handle-size)` |
| `--dnet-slide-toggle-lens-track-background` | `var(--_base-track)` |
| `--dnet-slide-toggle-lens-track-background-checked` | `var(--_base-track-checked)` |
| `--dnet-slide-toggle-lens-track-height` | `var(--_base-track-height)` |
| `--dnet-slide-toggle-lens-track-shadow` | `var(--_base-track-shadow)` |
| `--dnet-slide-toggle-lens-track-shadow-checked` | `var(--_base-track-shadow-checked)` |
| `--dnet-slide-toggle-track-background` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-slide-toggle-track-background-checked` | `#5b5ce0` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-slide-toggle-track-border-width` | `2px` |
| `--dnet-slide-toggle-track-height` | `32px` |
| `--dnet-slide-toggle-track-shadow-checked` | `none` |
| `--dnet-slide-toggle-track-width` | `52px` |

```css
:root { --dnet-slide-toggle-handle-background: /* your value */; }
```
