# Spinner

## `<DnetSpinner>`

```razor
<DnetSpinner
    CanRun="..."
    BindToView="..."
    ShowMask="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `CanRun` | `bool` | — | Gets or sets the can run used by this component. |
| `BindToView` | `bool` | — | Gets or sets the can run used by this component. Gets or sets the bind to view used by this component. |
| `ShowMask` | `bool` | — | Gets or sets the can run used by this component. Gets or sets the bind to view used by this component. Gets or sets whether a loading mask is displayed. |
| `DebounceTime` | `int` | `250` | Gets or sets the bind to view used by this component. Gets or sets whether a loading mask is displayed. Gets or sets the delay before a debounced input action is raised. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-spinner-color` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-spinner-mask-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 16%, transparent)` |

```css
:root { --dnet-spinner-color: /* your value */; }
```
