# FloatingPanel

## `<DnetFloatingPanel>`

```razor
<DnetFloatingPanel
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ComponentType` | `Type?` | `null` | Gets or sets the component type rendered dynamically. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `FloatingPanelClass` | `string` | `string.Empty` | Gets or sets the floating panel class used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-floating-panel-background` | `#ffffff` <br><sub>via `--dnet-sys-material-popup`</sub> |

```css
:root { --dnet-floating-panel-background: /* your value */; }
```
