# DatePicker

## `<DnetDatePicker>`

```razor
<DnetDatePicker
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnError` | `EventCallback<string>` | — | Raised when error occurs. |
| `Disabled` | `bool` | `false` | Gets or sets whether user interaction with the component is disabled. |
| `Format` | `string` | `$"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |
| `Formats` | `string[]` | `new[] { "yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d" }` | Gets or sets the formats used by this component. |
| `Culture` | `CultureInfo?` | `CultureInfo.CurrentCulture` | Gets or sets the culture used to format and parse values. |
| `MaxDayValue` | `DateTime?` | — | Gets or sets the max day value used by this component. |
| `MinDayValue` | `DateTime?` | — | Gets or sets the min day value used by this component. |
| `FirstDayToShow` | `DateTime?` | — | Gets or sets the first day to show used by this component. |
| `DatepickerFilter` | `Func<CalendarDay, bool>?` | — | Gets or sets the datepicker filter used by this component. |
| `OnDaySelected` | `EventCallback<CalendarDay>` | — | Raised when day selected occurs. |
| `BorderRadius` | `string` | `"5px"` | Gets or sets the border radius used by this component. |
| `MarginTop` | `string` | `"5px"` | Gets or sets the margin top used by this component. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |
| `ShowInternalErrors` | `bool` | `true` | Gets or sets whether the component show internal errors. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-calendar-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-calendar-day-disabled-color` | `var(--_tint-38)` |
| `--dnet-calendar-day-height` | `40px` |
| `--dnet-calendar-selected-bg-color` | `#4fc3f7` <br><sub>via `--dnet-sys-primary`</sub> |
| `--dnet-calendar-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 15.9%, transparent)` |

```css
:root { --dnet-calendar-background: /* your value */; }
```
