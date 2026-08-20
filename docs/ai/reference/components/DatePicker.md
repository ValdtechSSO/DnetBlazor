# DatePicker

Components: `<DnetDatePicker>`

## `<DnetDatePicker>`

| Parameter | Type | Default |
|---|---|---|
| `OnError` | `EventCallback<string>` | — |
| `Disabled` | `bool` | `false` |
| `Format` | `string` | `$"yyyy/MM/dd"` |
| `Formats` | `string[]` | `new[] { "yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d" }` |
| `Culture` | `CultureInfo?` | `CultureInfo.CurrentCulture` |
| `MaxDayValue` | `DateTime?` | — |
| `MinDayValue` | `DateTime?` | — |
| `FirstDayToShow` | `DateTime?` | — |
| `DatepickerFilter` | `Func<CalendarDay, bool>?` | — |
| `OnDaySelected` | `EventCallback<CalendarDay>` | — |
| `BorderRadius` | `string` | `"5px"` |
| `MarginTop` | `string` | `"5px"` |
| `PlaceHolder` | `string?` | — |
| `ShowInternalErrors` | `bool` | `true` |

## Minimal usage

```razor
<DnetDatePicker
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
| `--dnet-calendar-background` | `var(--dnet-sys-surface)` |
| `--dnet-calendar-day-disabled-color` | `var(--_tint-38)` |
| `--dnet-calendar-day-height` | `40px` |
| `--dnet-calendar-selected-bg-color` | `var(--dnet-sys-primary)` |
| `--dnet-calendar-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 15.9%, transparent)` |

```css
:root { --dnet-calendar-background: /* your value */; }
```
