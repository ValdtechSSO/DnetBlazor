# DatePickerWeek

Components: `<DnetDatePickerWeek>`

## `<DnetDatePickerWeek>`

| Parameter | Type | Default |
|---|---|---|
| `Disabled` | `bool` | `false` |
| `Format` | `string` | `$"yyyy/MM/dd"` |
| `PlaceHolder` | `string` | `""` |
| `MaxDayValue` | `DateTime?` | — |
| `MinDayValue` | `DateTime?` | — |
| `FirstDayToShow` | `DateTime?` | — |
| `DatepickerFilter` | `Func<CalendarDay, bool>?` | — |
| `OnWeekSelected` | `EventCallback<List<CalendarDay>>` | — |
| `PrefixContent` | `RenderFragment?` | — |
| `HintContent` | `RenderFragment?` | — |
| `ErrorContent` | `RenderFragment?` | — |
| `ComponentTheme` | `ComponentTheme` | `ComponentTheme.Material` |

## Minimal usage

```razor
<DnetDatePickerWeek
    MaxDayValue="..."
    MinDayValue="..."
    FirstDayToShow="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-datepickerweek-input-container-border` | — |
| `--dnet-datepickerweek-input-height` | — |
| `--dnet-datepickerweekca-day-disabled-color` | — |
| `--dnet-datepickerweekca-day-height` | — |
| `--dnet-datepickerweekca-selected-bg-color` | — |
| `--dnet-datepickerweekca-today-bg-color` | — |

```css
:root { --dnet-datepickerweek-input-container-border: /* your value */; }
```
