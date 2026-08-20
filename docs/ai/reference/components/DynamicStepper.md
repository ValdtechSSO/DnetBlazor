# DynamicStepper

Components: `<DnetDynamicStep>`, `<DnetDynamicStepper>`

## `<DnetDynamicStep>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `StepHeaderTemplate` | `RenderFragment?` | — |
| `Label` | `string` | — |
| `Order` | `int` | `0` |
| `Completed` | `bool` | — |

## `<DnetDynamicStepper>`

| Parameter | Type | Default |
|---|---|---|
| `OnSelectionChange` | `EventCallback<Tuple<int, int>>` | — |
| `ChildContent` | `RenderFragment?` | — |
| `ShowButtons` | `bool` | `true` |
| `SelectedStepId` | `int` | — |
| `LabelPosition` | `int` | — |
| `Linear` | `bool` | `false` |
| `Editable` | `bool` | `true` |

## Minimal usage

```razor
<DnetDynamicStep
    Label="..."
    Completed="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-step-font-size` | `var(--dnet-sys-text-md)` |

```css
:root { --dnet-step-font-size: /* your value */; }
```
