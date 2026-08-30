# TimePicker

## `<DnetTimePicker>`

```razor
<DnetTimePicker
    Disabled="..."
    ReadOnly="..."
    Required="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the picker is disabled. |
| `ReadOnly` | `bool` | — | Gets or sets whether the time can be viewed but not changed. |
| `Required` | `bool` | — | Gets or sets whether the underlying form control is required. |
| `Placeholder` | `string?` | — | Gets or sets the placeholder displayed while the input is empty. |
| `Format` | `string` | `"HH:mm"` | Gets or sets the format used to display and parse time values. |
| `Formats` | `IReadOnlyList<string>?` | — | Gets or sets additional accepted input formats. |
| `Culture` | `CultureInfo?` | — | Gets or sets the culture used to display and parse time values. |
| `Min` | `TimeOnly?` | — | Gets or sets the earliest selectable time. |
| `Max` | `TimeOnly?` | — | Gets or sets the latest selectable time. |
| `Interval` | `TimeSpan` | `TimeSpan.FromMinutes(30)` | Gets or sets the interval used to generate options when TimeOptions is not supplied. |
| `TimeOptions` | `IReadOnlyList<TimeOnly>?` | — | Gets or sets an explicit set of selectable times. When supplied, these options take precedence over Interval and are filtered by Min and Max. |
| `OptionsAriaLabel` | `string` | `"Choose a time"` | Gets or sets the accessible label announced for the options list. |
| `PanelClass` | `string?` | — | Gets or sets an additional CSS class applied to the overlay panel. |
| `PanelStyle` | `string?` | — | Gets or sets instance-level styles applied to the overlay panel. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-timepicker-accent-color` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-timepicker-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-timepicker-option-active-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 8%, transparent)` <br><sub>via `--dnet-sys-state-hover`</sub> |
| `--dnet-timepicker-option-selected-background` | `color-mix(in srgb, var(--dnet-sys-primary) 12%, transparent)` <br><sub>via `--dnet-sys-state-selected`</sub> |
| `--dnet-timepicker-panel-background` | `#ffffff` <br><sub>via `--dnet-sys-surface-raised`</sub> |

```css
:root { --dnet-timepicker-accent-color: /* your value */; }
```
