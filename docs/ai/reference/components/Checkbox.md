# Checkbox

An input component for editing bool values.

## `<DnetInputCheckbox>`

```razor
<DnetInputCheckbox
    Disabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `TextPlacedBefore` | `bool` | `false` | Gets or sets the text placed before used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-checkbox-background` | `#4fc3f7` <br><sub>via `--dnet-sys-primary`</sub> |
| `--dnet-checkbox-border-color` | `color-mix(in srgb, var(--dnet-sys-on-surface-subtle) 57.2%, var(--dnet-sys-surface))` |
| `--dnet-checkbox-border-radius` | `0px` |
| `--dnet-checkbox-border-width` | `1px` |
| `--dnet-checkbox-checkmark-path` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-checkbox-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-checkbox-size` | `14px` |

```css
:root { --dnet-checkbox-background: /* your value */; }
```
