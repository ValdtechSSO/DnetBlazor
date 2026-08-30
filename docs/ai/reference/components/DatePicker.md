# DatePicker

## `<DnetCalendar>`

```razor
<DnetCalendar
    RangeSelection="..."
    Disabled="..."
    AutoFocus="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `DateOnly?` | — | Gets or sets the component value. |
| `Range` | `DnetDateRange?` | — | Gets or sets the range used by this component. |
| `PreviewRange` | `DnetDateRange?` | — | Gets or sets the preview range used by this component. |
| `ComparisonRange` | `DnetDateRange?` | — | Gets or sets the comparison range used by this component. |
| `RangeSelection` | `bool` | — | Gets or sets the range selection used by this component. |
| `Min` | `DateOnly?` | — | Gets or sets the min used by this component. |
| `Max` | `DateOnly?` | — | Gets or sets the max used by this component. |
| `StartAt` | `DateOnly?` | — | Gets or sets the start at used by this component. |
| `StartView` | `DnetCalendarView` | `DnetCalendarView.Month` | Gets or sets the start view used by this component. |
| `FirstDayOfWeek` | `DayOfWeek?` | — | Gets or sets the first day of week used by this component. |
| `Culture` | `CultureInfo?` | — | Gets or sets the culture used to format and parse values. |
| `IsDateEnabled` | `Func<DateOnly, bool>?` | — | Gets or sets whether date enabled. |
| `DateClass` | `Func<DateOnly, string?>?` | — | Gets or sets the date class used by this component. |
| `RangeSelectionStrategy` | `IDnetDateRangeSelectionStrategy?` | — | Gets or sets the range selection strategy used by this component. |
| `Labels` | `DnetDatePickerLabels` | `new()` | Gets or sets the labels used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `AutoFocus` | `bool` | — | Gets or sets the auto focus used by this component. |
| `DayContent` | `RenderFragment<DnetCalendarCellContext>?` | — | Gets or sets content rendered for day. |
| `HeaderContent` | `RenderFragment<DnetCalendarHeaderContext>?` | — | Gets or sets content rendered for header. |
| `FooterContent` | `RenderFragment?` | — | Gets or sets content rendered in the component footer. |
| `ValueChanged` | `EventCallback<DateOnly?>` | — | Raised when the component value changes. |
| `DateSelected` | `EventCallback<DateOnly>` | — | Raised when date selected changes. |
| `RangeChanged` | `EventCallback<DnetDateRange>` | — | Raised when range changes. |
| `PreviewRangeChanged` | `EventCallback<DnetDateRange>` | — | Raised when preview range changes. |
| `YearSelected` | `EventCallback<int>` | — | Raised when year selected changes. |
| `MonthSelected` | `EventCallback<DateOnly>` | — | Raised when month selected changes. |
| `ViewChanged` | `EventCallback<DnetCalendarView>` | — | Raised when view changes. |
| `EscapePressed` | `EventCallback` | — | Raised when escape pressed changes. |

## `<DnetDatePicker>`

```razor
<DnetDatePicker
    Disabled="..."
    ReadOnly="..."
    Required="..."
    CalendarDisabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnError` | `EventCallback<string>` | — | Raised when parsing or date validation fails. |
| `Disabled` | `bool` | — | Raised when parsing or date validation fails. |
| `ReadOnly` | `bool` | — | Raised when parsing or date validation fails. |
| `Required` | `bool` | — | Raised when parsing or date validation fails. |
| `CalendarDisabled` | `bool` | — | Raised when parsing or date validation fails. |
| `ToggleDisabled` | `bool` | — | Raised when parsing or date validation fails. |
| `OpenOnFocus` | `bool` | `true` | Gets or sets the open on focus used by this component. |
| `Format` | `string` | `"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |
| `Formats` | `string[]` | `["yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d"]` | Gets or sets the formats used by this component. |
| `Culture` | `CultureInfo?` | — | Gets or sets the culture used to format and parse values. |
| `MaxDayValue` | `DateTime?` | — | Gets or sets the max day value used by this component. |
| `MinDayValue` | `DateTime?` | — | Gets or sets the min day value used by this component. |
| `FirstDayToShow` | `DateTime?` | — | Gets or sets the first day to show used by this component. |
| `DatepickerFilter` | `Func<CalendarDay, bool>?` | — | Gets or sets the datepicker filter used by this component. |
| `IsDateEnabled` | `Func<DateOnly, bool>?` | — | Gets or sets whether date enabled. |
| `OnDaySelected` | `EventCallback<CalendarDay>` | — | Raised when day selected occurs. |
| `OnDateInput` | `EventCallback<string?>` | — | Raised when date input occurs. |
| `OnDateChange` | `EventCallback<CalendarDay>` | — | Raised when date change occurs. |
| `OnYearSelected` | `EventCallback<int>` | — | Raised when year selected occurs. |
| `OnMonthSelected` | `EventCallback<DateOnly>` | — | Raised when month selected occurs. |
| `OnViewChanged` | `EventCallback<DnetCalendarView>` | — | Raised when view changed occurs. |
| `OnOpened` | `EventCallback` | — | Raised when opened occurs. |
| `OnClosed` | `EventCallback` | — | Raised when closed occurs. |
| `StartView` | `DnetCalendarView` | `DnetCalendarView.Month` | Gets or sets the start view used by this component. |
| `FirstDayOfWeek` | `DayOfWeek?` | — | Gets or sets the first day of week used by this component. |
| `DateClass` | `Func<DateOnly, string?>?` | — | Gets or sets the date class used by this component. |
| `DayContent` | `RenderFragment<DnetCalendarCellContext>?` | — | Gets or sets content rendered for day. |
| `HeaderContent` | `RenderFragment<DnetCalendarHeaderContext>?` | — | Gets or sets content rendered for header. |
| `FooterContent` | `RenderFragment?` | — | Gets or sets content rendered in the component footer. |
| `ShowActions` | `bool` | — | Gets or sets whether the component show actions. |
| `ShowReset` | `bool` | `true` | Gets or sets whether the component show reset. |
| `Labels` | `DnetDatePickerLabels` | `new()` | Gets or sets the labels used by this component. |
| `TouchUi` | `bool` | — | Gets or sets the touch ui used by this component. |
| `Responsive` | `bool` | `true` | Gets or sets the responsive used by this component. |
| `ResponsiveBreakpoint` | `int` | `600` | Gets or sets the responsive breakpoint used by this component. |
| `RestoreFocus` | `bool` | `true` | Gets or sets the restore focus used by this component. |
| `PanelClass` | `string?` | — | Gets or sets the panel class used by this component. |
| `PanelStyle` | `string?` | — | Gets or sets the panel style used by this component. |
| `BorderRadius` | `string?` | — | Gets or sets the border radius used by this component. |
| `MarginTop` | `string?` | — | Gets or sets the margin top used by this component. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |
| `ShowInternalErrors` | `bool` | `true` | Gets or sets whether the component show internal errors. |

## `<DnetDateRangePicker>`

```razor
<DnetDateRangePicker
    Disabled="..."
    ReadOnly="..."
    Required="..."
    CalendarDisabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `ReadOnly` | `bool` | — | Gets or sets the read only used by this component. |
| `Required` | `bool` | — | Gets or sets the required used by this component. |
| `CalendarDisabled` | `bool` | — | Gets or sets the calendar disabled used by this component. |
| `ToggleDisabled` | `bool` | — | Gets or sets the toggle disabled used by this component. |
| `OpenOnFocus` | `bool` | `true` | Gets or sets the open on focus used by this component. |
| `Format` | `string` | `"yyyy/MM/dd"` | Gets or sets the display and parsing format for the value. |
| `Formats` | `IReadOnlyList<string>?` | — | Gets or sets the formats used by this component. |
| `Culture` | `CultureInfo?` | — | Gets or sets the culture used to format and parse values. |
| `Min` | `DateOnly?` | — | Gets or sets the min used by this component. |
| `Max` | `DateOnly?` | — | Gets or sets the max used by this component. |
| `StartAt` | `DateOnly?` | — | Gets or sets the start at used by this component. |
| `IsDateEnabled` | `Func<DateOnly, bool>?` | — | Gets or sets whether date enabled. |
| `ComparisonRange` | `DnetDateRange?` | — | Gets or sets the comparison range used by this component. |
| `SelectionStrategy` | `IDnetDateRangeSelectionStrategy?` | — | Gets or sets the selection strategy used by this component. |
| `StartView` | `DnetCalendarView` | `DnetCalendarView.Month` | Gets or sets the start view used by this component. |
| `FirstDayOfWeek` | `DayOfWeek?` | — | Gets or sets the first day of week used by this component. |
| `DateClass` | `Func<DateOnly, string?>?` | — | Gets or sets the date class used by this component. |
| `DayContent` | `RenderFragment<DnetCalendarCellContext>?` | — | Gets or sets content rendered for day. |
| `HeaderContent` | `RenderFragment<DnetCalendarHeaderContext>?` | — | Gets or sets content rendered for header. |
| `FooterContent` | `RenderFragment?` | — | Gets or sets content rendered in the component footer. |
| `StartPlaceholder` | `string?` | — | Gets or sets the start placeholder used by this component. |
| `EndPlaceholder` | `string?` | — | Gets or sets the end placeholder used by this component. |
| `Labels` | `DnetDatePickerLabels` | `new()` | Gets or sets the labels used by this component. |
| `ShowActions` | `bool` | `true` | Gets or sets whether the component show actions. |
| `TouchUi` | `bool` | — | Gets or sets the touch ui used by this component. |
| `Responsive` | `bool` | `true` | Gets or sets the responsive used by this component. |
| `ResponsiveBreakpoint` | `int` | `600` | Gets or sets the responsive breakpoint used by this component. |
| `RestoreFocus` | `bool` | `true` | Gets or sets the restore focus used by this component. |
| `PanelClass` | `string?` | — | Gets or sets the panel class used by this component. |
| `PanelStyle` | `string?` | — | Gets or sets the panel style used by this component. |
| `OnRangeInput` | `EventCallback<DnetDateRange?>` | — | Raised when range input occurs. |
| `OnRangeChange` | `EventCallback<DnetDateRange?>` | — | Raised when range change occurs. |
| `OnStartDateChange` | `EventCallback<DateOnly?>` | — | Raised when start date change occurs. |
| `OnEndDateChange` | `EventCallback<DateOnly?>` | — | Raised when end date change occurs. |
| `OnYearSelected` | `EventCallback<int>` | — | Raised when year selected occurs. |
| `OnMonthSelected` | `EventCallback<DateOnly>` | — | Raised when month selected occurs. |
| `OnViewChanged` | `EventCallback<DnetCalendarView>` | — | Raised when view changed occurs. |
| `OnOpened` | `EventCallback` | — | Raised when opened occurs. |
| `OnClosed` | `EventCallback` | — | Raised when closed occurs. |
| `OnError` | `EventCallback<string>` | — | Raised when error occurs. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-calendar-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-calendar-comparison-color` | — |
| `--dnet-calendar-day-disabled-color` | `color-mix(in srgb, var(--dnet-sys-on-surface) 38%, transparent)` <br><sub>via `--dnet-sys-state-disabled-fg`</sub> |
| `--dnet-calendar-day-height` | `40px` |
| `--dnet-calendar-preview-bg-color` | `color-mix(in srgb, var(--dnet-sys-primary) 10%, transparent)` |
| `--dnet-calendar-range-bg-color` | `color-mix(in srgb, var(--dnet-sys-primary) 18%, transparent)` |
| `--dnet-calendar-selected-bg-color` | `#4fc3f7` <br><sub>via `--dnet-sys-primary`</sub> |
| `--dnet-calendar-today-bg-color` | `color-mix(in srgb, var(--dnet-sys-primary) 12%, transparent)` <br><sub>via `--dnet-sys-state-selected`</sub> |

```css
:root { --dnet-calendar-background: /* your value */; }
```
