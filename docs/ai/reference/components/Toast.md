# Toast

Components: `<DnetToast>`

## `<DnetToast>`

| Parameter | Type | Default |
|---|---|---|
| `ComponentType` | `Type?` | — |
| `ContentChild` | `RenderFragment?` | — |
| `Parameters` | `IDictionary<string, object>?` | — |
| `Title` | `string?` | — |
| `Text` | `string?` | — |
| `ToastType` | `ToastType` | — |
| `TypeIconClass` | `string?` | — |
| `ToastTypeIconClass` | `string?` | — |
| `CloseIconClass` | `string?` | — |
| `ToastTypeColor` | `string?` | — |
| `ToastClass` | `string?` | — |
| `ExcutionTime` | `int` | — |
| `ShowExcutionTime` | `bool` | — |

## Minimal usage

```razor
<DnetToast
    ComponentType="..."
    Parameters="..."
    Title="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-toast-bg-danger` | `var(--dnet-sys-danger)` |
| `--dnet-toast-bg-info` | `var(--dnet-sys-primary-strong)` |
| `--dnet-toast-bg-success` | `var(--dnet-sys-success)` |
| `--dnet-toast-bg-warning` | `var(--dnet-sys-warning)` |
| `--dnet-toast-border-radius` | `var(--dnet-sys-radius-sm)` |
| `--dnet-toast-margin` | `5px 5px 5px 5px` |

```css
:root { --dnet-toast-bg-danger: /* your value */; }
```
