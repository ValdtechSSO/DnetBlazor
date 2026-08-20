# Stepper

## `<DnetStep>`

```razor
<DnetStep
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

## `<DnetStepper>`

```razor
<DnetStepper
    SelectedStepId="..."
    LabelPosition="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSelectionChange` | `EventCallback<Tuple<int, int>>` | — | Raised when the selection changes. |
| `AllOtherAttributes` | `Dictionary<string, object>?` | — | Gets or sets the all other attributes used by this component. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `SelectedStepId` | `int` | — | Gets or sets the identifier of the selected step. |
| `Orientation` | `int` | `(int)StepperOrientation.Horizontal` | Gets or sets the orientation used by this component. |
| `LabelPosition` | `int` | — | Gets or sets the label position used by this component. |
| `Linear` | `bool` | `false` | Gets or sets the linear used by this component. |
| `Editable` | `bool` | `true` | Gets or sets the editable used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-step-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |

```css
:root { --dnet-step-font-size: /* your value */; }
```
