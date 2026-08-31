# Toast

## `<DnetToast>`

```razor
<DnetToast
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
| `ToastType` | `ToastType` | `ToastType.Information` | Gets or sets the toast type used by this component. |
| `TypeIconClass` | `string?` | — | Gets or sets the type icon class used by this component. |
| `ToastTypeIconClass` | `string?` | — | Gets or sets the legacy toast type icon class. |
| `CloseIconClass` | `string?` | — | Gets or sets the close icon class used by this component. |
| `ToastTypeColor` | `string?` | — | Gets or sets an optional accent color used by this component. |
| `ToastClass` | `string?` | — | Gets or sets additional classes applied to this component. |
| `ExcutionTime` | `int` | — | Gets or sets the legacy visible duration in seconds. |
| `ShowExcutionTime` | `bool` | — | Gets or sets whether the legacy execution-time state is exposed. |
| `DurationMilliseconds` | `int?` | — | Gets or sets the visible duration in milliseconds. |
| `Actions` | `IReadOnlyList<ToastAction>?` | — | Gets or sets up to two actions displayed by this toast. |
| `Strings` | `ToastStrings?` | — | Gets or sets localized strings for this toast. |
| `Dismissed` | `EventCallback` | — | Raised after the closing transition has completed. |

## `<DnetToastStack>`

```razor
<DnetToastStack
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Service` | `ToastService` | `default!` | Gets or sets the service used by this component. |
| `Position` | `ToastPostion` | `ToastPostion.BottomRight` | Gets or sets the position used by this component. |
| `ThemeScope` | `string?` | — | Gets or sets the theme scope used by this component. |
| `Width` | `int` | `392` | Gets or sets the component width. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-toast-accent` | `#4fc3f7` <br><sub>via `--dnet-sys-primary`</sub> |
| `--dnet-toast-border` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-toast-fg` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-toast-fg-muted` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-toast-icon-bg` | `color-mix(in srgb, var(--_accent) 13%, var(--dnet-sys-transparent))` |
| `--dnet-toast-icon-size` | `32px` |
| `--dnet-toast-margin` | `0` |
| `--dnet-toast-padding` | `calc(var(--dnet-sys-space-unit) * 4)` <br><sub>via `--dnet-sys-space-4`</sub> |
| `--dnet-toast-progress-height` | `2px` |
| `--dnet-toast-radius` | `10px` <br><sub>via `--dnet-sys-radius-lg`</sub> |
| `--dnet-toast-shadow` | `0 3px 5px -1px var(--dnet-sys-shadow-color), 0 6px 10px 0 var(--dnet-sys-shadow-color), 0 1px 18px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-3`</sub> |
| `--dnet-toast-stack-gap` | `calc(var(--dnet-sys-space-unit) * 3)` <br><sub>via `--dnet-sys-space-3`</sub> |
| `--dnet-toast-stack-inset` | `calc(var(--dnet-sys-space-unit) * 5)` <br><sub>via `--dnet-sys-space-5`</sub> |
| `--dnet-toast-stack-z` | `1000` |
| `--dnet-toast-width` | `392px` |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-toast-bg`, `--dnet-toast-bg-danger`, `--dnet-toast-bg-info`, `--dnet-toast-bg-success`, `--dnet-toast-bg-warning`, `--dnet-toast-border-radius`

</details>

```css
:root { --dnet-toast-radius: /* your value */; }
```
