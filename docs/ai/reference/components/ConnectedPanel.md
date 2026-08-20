# ConnectedPanel

## `<DnetConnectedFloatingPanel>`

```razor
<DnetConnectedFloatingPanel
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ConnectedPanelClasses` | `string?` | — | Gets or sets the connected panel classes used by this component. |
| `ComponentType` | `Type?` | — | Gets or sets the component type rendered dynamically. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-connected-panel-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-connected-panel-padding` | `2px` |
| `--dnet-dialog-border-radius` | `5px` <br><sub>via `--dnet-sys-radius-md`</sub> |

```css
:root { --dnet-connected-panel-background: /* your value */; }
```
