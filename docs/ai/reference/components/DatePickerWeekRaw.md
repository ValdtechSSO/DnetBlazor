# DatePickerWeekRaw

## `<DnetDatePickerWeekRaw>`

```razor
<DnetDatePickerWeekRaw
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnWeekSelected` | `EventCallback<List<CalendarDay>>` | — | Raised when week selected occurs. |
| `Format` | `string` | `$"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-datepickerweekca-raw-day-disabled-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 60.5%, transparent)` |
| `--dnet-datepickerweekca-raw-day-height` | `30px` |
| `--dnet-datepickerweekca-raw-selected-bg-color` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-datepickerweekca-raw-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 15.9%, transparent)` |

```css
:root { --dnet-datepickerweekca-raw-day-disabled-color: /* your value */; }
```
