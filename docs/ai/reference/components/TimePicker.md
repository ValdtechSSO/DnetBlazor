# TimePicker

## `<DnetTimePicker>`

```razor
<DnetTimePicker
    Disabled="..."
    ReadOnly="..."
    Required="..."
    TouchUi="..."
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
| `Mode` | `DnetTimePickerMode` | `DnetTimePickerMode.List` | Gets or sets how the time is chosen: from a list (the default) or on a clock face in a centred dialog. The dial is 12-hour when Format uses h, and 24-hour otherwise; it ignores Interval and TimeOptions, and keeps to Min and Max. |
| `OptionsAriaLabel` | `string` | Computed | Gets or sets the accessible label announced for the options list. |
| `CancelLabel` | `string` | Computed | Gets or sets the label of the responsive dialog cancel action. |
| `PanelClass` | `string?` | — | Gets or sets an additional CSS class applied to the overlay panel. |
| `PanelStyle` | `string?` | — | Gets or sets instance-level styles applied to the overlay panel. |
| `TouchUi` | `bool` | — | Gets or sets whether the picker always uses its touch-friendly modal presentation. |
| `Responsive` | `bool` | `true` | Gets or sets whether the picker automatically switches to its touch presentation on small viewports. |
| `ResponsiveBreakpoint` | `int` | `600` | Gets or sets the viewport width at which the responsive touch presentation is used. |
| `RestoreFocus` | `bool` | `true` | Gets or sets whether focus returns to the element that opened the touch overlay after it closes. |

## `<DnetTimePickerDial>`

```razor
<DnetTimePickerDial
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `TimeOnly?` | — | Gets or sets the time the dial starts from. |
| `Use24Hour` | `bool` | `true` | Gets or sets whether the dial shows 24 hours on two rings instead of 12 with AM and PM. |
| `Min` | `TimeOnly?` | — | Gets or sets the earliest time that can be chosen. |
| `Max` | `TimeOnly?` | — | Gets or sets the latest time that can be chosen. |
| `Culture` | `CultureInfo` | Computed | Gets or sets the culture used for the AM and PM designators. |
| `Title` | `string?` | Computed | Gets or sets the dialog title. |
| `CancelLabel` | `string?` | Computed | Gets or sets the label of the cancel action. |
| `OkLabel` | `string?` | Computed | Gets or sets the label of the confirm action. |
| `Cancel` | `EventCallback` | — | Raised when the user cancels the dialog. |
| `Confirm` | `EventCallback<TimeOnly>` | — | Raised with the chosen time when the user confirms it. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-timepicker-accent-color` | `#5b5ce0` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-timepicker-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-timepicker-icon-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-timepicker-option-active-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 8%, transparent)` <br><sub>via `--dnet-sys-state-hover`</sub> |
| `--dnet-timepicker-option-selected-background` | `color-mix(in srgb, var(--dnet-sys-primary) 12%, transparent)` <br><sub>via `--dnet-sys-state-selected`</sub> |
| `--dnet-timepicker-panel-actions-padding` | `var(--dnet-sys-space-2) var(--dnet-sys-space-3) var(--dnet-sys-space-3)` |
| `--dnet-timepicker-panel-background` | `#ffffff` <br><sub>via `--dnet-sys-material-popup`</sub> |
| `--dnet-timepicker-panel-header-background` | `var(--_panel-background)` |
| `--dnet-timepicker-panel-header-padding` | `var(--dnet-sys-space-3) var(--dnet-sys-space-3) var(--dnet-sys-space-2)` |
| `--dnet-timepicker-panel-options-padding` | `calc(var(--dnet-sys-space-unit) * 2)` <br><sub>via `--dnet-sys-space-2`</sub> |
| `--dnet-timepicker-panel-touch-options-padding` | `0 var(--dnet-sys-space-3) var(--dnet-sys-space-3)` |

```css
:root { --dnet-timepicker-accent-color: /* your value */; }
```
