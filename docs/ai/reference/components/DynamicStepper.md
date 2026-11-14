# DynamicStepper

## `<DnetDynamicStep>`

```razor
<DnetDynamicStep
    Label="..."
    Completed="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `StepHeaderTemplate` | `RenderFragment?` | — | Gets or sets the template used to render step header. |
| `Label` | `string` | — | Gets or sets the label displayed for the component. |
| `Order` | `int` | `0` | Gets or sets the order used by this component. |
| `Completed` | `bool` | — | Gets or sets the completed used by this component. |

## `<DnetDynamicStepper>`

```razor
<DnetDynamicStepper
    SelectedStepId="..."
    LabelPosition="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSelectionChange` | `EventCallback<Tuple<int, int>>` | — | Raised when the selection changes. |
| `AllOtherAttributes` | `Dictionary<string, object>?` | — | Gets or sets the all other attributes used by this component. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `ShowButtons` | `bool` | `true` | Gets or sets whether the component show buttons. |
| `SelectedStepId` | `int` | — | Gets or sets the identifier of the selected step. |
| `LabelPosition` | `int` | — | Gets or sets the label position used by this component. |
| `Linear` | `bool` | `false` | Gets or sets the linear used by this component. |
| `Editable` | `bool` | `true` | Gets or sets the editable used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-step-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-step-icon-background` | `#757575` <br><sub>via `--dnet-sys-on-surface-subtle`</sub> |
| `--dnet-step-icon-background-current` | `color-mix(in srgb, var(--dnet-sys-primary) 80%, var(--dnet-ref-neutral-1000))` <br><sub>via `--dnet-sys-primary-emphasis`</sub> |
| `--dnet-step-icon-background-done` | `none` |
| `--dnet-step-icon-color` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-step-icon-font-size` | `inherit` |
| `--dnet-step-icon-font-weight` | `inherit` |
| `--dnet-step-icon-margin` | `0 8px 0 0` |
| `--dnet-step-icon-scale-current` | `none` |
| `--dnet-step-icon-shadow` | `none` |
| `--dnet-step-icon-shadow-current` | `none` |
| `--dnet-step-label-color-upcoming` | `inherit` |
| `--dnet-step-label-weight` | `inherit` |
| `--dnet-step-label-weight-current` | `var(--_label-weight)` |

```css
:root { --dnet-step-icon-background: /* your value */; }
```
