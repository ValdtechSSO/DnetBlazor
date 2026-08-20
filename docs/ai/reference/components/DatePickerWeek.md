# DatePickerWeek

## `<DnetDatePickerWeek>`

```razor
<DnetDatePickerWeek
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Disabled` | `bool` | `false` | Gets or sets whether user interaction with the component is disabled. |
| `Format` | `string` | `$"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |
| `PlaceHolder` | `string` | `""` | Gets or sets placeholder text displayed when the input is empty. |
| `MaxDayValue` | `DateTime?` | — | Gets or sets the max day value used by this component. |
| `MinDayValue` | `DateTime?` | — | Gets or sets the min day value used by this component. |
| `FirstDayToShow` | `DateTime?` | — | Gets or sets the first day to show used by this component. |
| `DatepickerFilter` | `Func<CalendarDay, bool>?` | — | Gets or sets the datepicker filter used by this component. |
| `OnWeekSelected` | `EventCallback<List<CalendarDay>>` | — | Raised when week selected occurs. |
| `PrefixContent` | `RenderFragment?` | — | Gets or sets content rendered before the main control content. |
| `HintContent` | `RenderFragment?` | — | Gets or sets supporting content displayed with the control. |
| `ErrorContent` | `RenderFragment?` | — | Gets or sets content displayed when validation reports an error. |
| `ComponentTheme` | `ComponentTheme` | `ComponentTheme.Material` | Gets or sets the component theme used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-datepickerweek-input-container-border` | `1px solid var(--dnet-sys-border)` |
| `--dnet-datepickerweek-input-height` | `28px` |
| `--dnet-datepickerweekca-day-disabled-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 60.5%, transparent)` |
| `--dnet-datepickerweekca-day-height` | `30px` |
| `--dnet-datepickerweekca-selected-bg-color` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-datepickerweekca-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 15.9%, transparent)` |

```css
:root { --dnet-datepickerweekca-day-disabled-color: /* your value */; }
```
