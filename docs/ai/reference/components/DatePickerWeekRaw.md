# DatePickerWeekRaw

Components: `<DnetDatePickerWeekRaw>`

## `<DnetDatePickerWeekRaw>`

| Parameter | Type | Default |
|---|---|---|
| `OnWeekSelected` | `EventCallback<List<CalendarDay>>` | — |
| `Format` | `string` | `$"yyyy/MM/dd"` |

## Minimal usage

```razor
<DnetDatePickerWeekRaw />
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-datepickerweekca-raw-day-disabled-color` | — |
| `--dnet-datepickerweekca-raw-day-height` | — |
| `--dnet-datepickerweekca-raw-selected-bg-color` | — |
| `--dnet-datepickerweekca-raw-today-bg-color` | — |

```css
:root { --dnet-datepickerweekca-raw-day-disabled-color: /* your value */; }
```
