# Toast

## `<DnetToast>`

```razor
<DnetToast
    ToastType="..."
    ExcutionTime="..."
    ShowExcutionTime="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ComponentType` | `Type?` | — | Gets or sets the component type rendered dynamically. |
| `ContentChild` | `RenderFragment?` | — | Gets or sets custom content rendered by the component. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `Title` | `string?` | — | Gets or sets the title displayed by the component. |
| `Text` | `string?` | — | Gets or sets the text used by this component. |
| `ToastType` | `ToastType` | — | Gets or sets the toast type used by this component. |
| `TypeIconClass` | `string?` | — | Gets or sets the type icon class used by this component. |
| `ToastTypeIconClass` | `string?` | — | Gets or sets the toast type icon class used by this component. |
| `CloseIconClass` | `string?` | — | Gets or sets the close icon class used by this component. |
| `ToastTypeColor` | `string?` | — | Gets or sets the toast type color used by this component. |
| `ToastClass` | `string?` | — | Gets or sets the toast class used by this component. |
| `ExcutionTime` | `int` | — | Gets or sets the excution time used by this component. |
| `ShowExcutionTime` | `bool` | — | Gets or sets whether the component show excution time. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-toast-bg-danger` | `#b80012` <br><sub>via `--dnet-sys-danger`</sub> |
| `--dnet-toast-bg-info` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-toast-bg-success` | `#40ab35` <br><sub>via `--dnet-sys-success`</sub> |
| `--dnet-toast-bg-warning` | `#ffd029` <br><sub>via `--dnet-sys-warning`</sub> |
| `--dnet-toast-border-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |
| `--dnet-toast-margin` | `5px 5px 5px 5px` |

```css
:root { --dnet-toast-border-radius: /* your value */; }
```
