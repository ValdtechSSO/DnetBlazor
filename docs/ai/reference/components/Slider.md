# Slider

An accessible single-value or range slider built on native range inputs.

## `<DnetSlider>`

```razor
<DnetSlider
    Value="..."
    Range="..."
    StartValue="..."
    Min="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the visible label rendered above the slider. |
| `Value` | `double` | — | Gets or sets the value of a single-value slider. |
| `ValueChanged` | `EventCallback<double>` | — | Raised continuously when the single slider value changes. |
| `ValueExpression` | `Expression<Func<double>>?` | — | Gets or sets the expression used for EditForm integration. |
| `Range` | `bool` | — | Gets or sets whether the component renders two range thumbs. |
| `StartValue` | `double` | — | Gets or sets the lower value of a range slider. |
| `StartValueChanged` | `EventCallback<double>` | — | Raised continuously when the range start value changes. |
| `StartValueExpression` | `Expression<Func<double>>?` | — | Gets or sets the expression used to validate the range start value. |
| `EndValue` | `double` | `100` | Gets or sets the upper value of a range slider. |
| `EndValueChanged` | `EventCallback<double>` | — | Raised continuously when the range end value changes. |
| `EndValueExpression` | `Expression<Func<double>>?` | — | Gets or sets the expression used to validate the range end value. |
| `Min` | `double` | — | Gets or sets the minimum permitted value. |
| `Max` | `double` | `100` | Gets or sets the maximum permitted value. |
| `Step` | `double` | `1` | Gets or sets the interval between permitted values. |
| `Discrete` | `bool` | — | Gets or sets whether value labels appear while a thumb is active or focused. |
| `ShowTickMarks` | `bool` | — | Gets or sets whether tick marks are shown along the track. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction is disabled. |
| `Required` | `bool` | — | Gets or sets whether the native range inputs are required. |
| `Color` | `DnetSliderColor` | `DnetSliderColor.Primary` | Gets or sets the slider's semantic color. |
| `DisplayWith` | `Func<double, string>?` | — | Gets or sets a function that formats value labels and accessible value text. |
| `AriaLabel` | `string?` | — | Gets or sets the accessible label for a single slider. |
| `StartAriaLabel` | `string?` | — | Gets or sets the accessible label for the start thumb. |
| `EndAriaLabel` | `string?` | — | Gets or sets the accessible label for the end thumb. |
| `Name` | `string?` | — | Gets or sets the form name for a single slider. |
| `StartName` | `string?` | — | Gets or sets the form name for the range start input. |
| `EndName` | `string?` | — | Gets or sets the form name for the range end input. |
| `OnInput` | `EventCallback<DnetSliderEventArgs>` | — | Raised continuously while either thumb changes. |
| `OnChange` | `EventCallback<DnetSliderEventArgs>` | — | Raised when the user commits a value change. |
| `OnDragStart` | `EventCallback<DnetSliderEventArgs>` | — | Raised when pointer dragging starts. |
| `OnDragEnd` | `EventCallback<DnetSliderEventArgs>` | — | Raised when pointer dragging ends or is cancelled. |
| `InputAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets extra attributes for the single native input. |
| `StartInputAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets extra attributes for the range start native input. |
| `EndInputAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets extra attributes for the range end native input. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched attributes applied to the component host. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-slider-active-track-color` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-slider-disabled-opacity` | `0.38` |
| `--dnet-slider-focus-ring` | `#42b0d5` <br><sub>via `--dnet-sys-focus-ring`</sub> |
| `--dnet-slider-inactive-track-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-slider-label-background` | `var(--_active-track)` |
| `--dnet-slider-label-foreground` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-slider-thumb-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-slider-thumb-border-color` | `var(--_active-track)` |
| `--dnet-slider-tick-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |

```css
:root { --dnet-slider-active-track-color: /* your value */; }
```
