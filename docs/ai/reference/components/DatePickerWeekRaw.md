# DatePickerWeekRaw

## `<DnetDatePickerWeekRaw>`

```razor
<DnetDatePickerWeekRaw
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Culture` | `CultureInfo?` | — | Gets or sets the culture used to format and parse values. |
| `OnWeekSelected` | `EventCallback<List<CalendarDay>>` | — | Raised when week selected occurs. |
| `Format` | `string` | `$"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-calendar-background` | `#ffffff` <br><sub>via `--dnet-sys-surface-raised`</sub> |
| `--dnet-datepickerweekca-raw-day-disabled-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 60.5%, transparent)` |
| `--dnet-datepickerweekca-raw-day-height` | `40px` |
| `--dnet-datepickerweekca-raw-selected-bg-color` | `#5b5ce0` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-datepickerweekca-raw-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 15.9%, transparent)` |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-calendar-day-height`

</details>

```css
:root { --dnet-calendar-background: /* your value */; }
```
