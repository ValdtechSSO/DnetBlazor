using System.Globalization;
using Bunit;
using Dnet.Blazor.Components.DatePicker;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Models;
using Dnet.Blazor.Components.DatePickerWeek;
using Dnet.Blazor.Components.DatePickerWeekRaw;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.TimePicker;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WeekDay = Dnet.Blazor.Components.DatePickerWeek.Infrastructure.Models.CalendarDay;

namespace Dnet.Blazor.UnitTests;

public sealed class DateTimeLocalizationTests : BunitContext
{
    private readonly OverlayService _overlay = new();

    public DateTimeLocalizationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.Setup<bool>("dnetinterop.matchesMedia", _ => true).SetResult(false);
        Services.AddLogging();
        Services.AddDnetLocalization();
        Services.AddSingleton<IOverlayService>(_overlay);
        Services.AddTransient<IFormEventService, FormEventService>();
    }

    private IDnetLocalizer Localizer => Services.GetRequiredService<IDnetLocalizer>();

    [Fact]
    public void Calendar_without_localization_registration_uses_the_immutable_Spanish_fallback()
    {
        using var context = new BunitContext();
        var cut = context.Render<DnetCalendar>(parameters => parameters
            .Add(component => component.StartAt, new DateOnly(2026, 5, 1)));

        Assert.Equal("Calendario", cut.Find(".dnet-calendar").GetAttribute("aria-label"));
        Assert.Contains("mayo", cut.Find(".dnet-calendar-period-button").TextContent.Trim());
        Assert.Equal("Periodo anterior", cut.Find(".dnet-calendar-previous-button").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task Partial_date_labels_keep_explicit_English_overrides_and_translate_unset_fields_live()
    {
        var date = new DateOnly(2026, 5, 12);
        var labels = new DnetDatePickerLabels { Cancel = "Cancel" };
        var cut = Render<DnetDatePickerPanel>(parameters => parameters
            .Add(component => component.Value, date)
            .Add(component => component.StartAt, date)
            .Add(component => component.ShowActions, true)
            .Add(component => component.AutoFocus, false)
            .Add(component => component.Labels, labels));
        Assert.Equal("Cancel", cut.Find(".dnet-datepicker-action-secondary").TextContent.Trim());
        Assert.Equal("Aplicar", cut.Find(".dnet-datepicker-action-primary").TextContent.Trim());
        Assert.Contains("mayo", cut.Find(".dnet-calendar-period-button").TextContent.Trim());

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("Apply", cut.Find(".dnet-datepicker-action-primary").TextContent.Trim()));
        Assert.Equal("Cancel", cut.Find(".dnet-datepicker-action-secondary").TextContent.Trim());
        Assert.Contains("May", cut.Find(".dnet-calendar-period-button").TextContent.Trim());
        Assert.Equal("12", cut.Find(".dnet-calendar-day-selected").TextContent.Trim());
        Assert.Equal(date, cut.Instance.Value);
    }

    [Fact]
    public async Task Explicit_calendar_culture_survives_UI_language_changes_without_resetting_selection()
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var date = new DateOnly(2026, 5, 12);
        var cut = Render<DnetCalendar>(parameters => parameters
            .Add(component => component.Value, date)
            .Add(component => component.Culture, culture));
        var period = cut.Find(".dnet-calendar-period-button").TextContent.Trim();
        Assert.Contains("mai", period);

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("Calendar", cut.Find(".dnet-calendar").GetAttribute("aria-label")));
        Assert.Equal(period, cut.Find(".dnet-calendar-period-button").TextContent.Trim());
        Assert.Equal("12", cut.Find(".dnet-calendar-day-selected").TextContent.Trim());
    }

    [Fact]
    public async Task Already_open_date_overlay_updates_labels_and_culture_without_losing_its_pending_date()
    {
        RenderFragment? content = null;
        OverlayConfig? config = null;
        _overlay.OnAttach += (fragment, options) => { content = fragment; config = options; };
        var model = new DateModel();
        var picker = Render<DnetDatePicker>(parameters => parameters
            .Add(component => component.Value, model.Value)
            .Add(component => component.ValueExpression, () => model.Value)
            .Add(component => component.FirstDayToShow, new DateTime(2026, 5, 1))
            .Add(component => component.ShowActions, true)
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.Responsive, false));
        await picker.InvokeAsync(picker.Instance.OpenAsync);
        var panel = Render(content!);
        var calendar = panel.FindComponent<DnetCalendar>();
        await panel.FindAll("button[role=gridcell]")
            .First(button => button.GetAttribute("aria-label") == new DateTime(2026, 5, 12).ToString("D", Localizer.Culture))
            .ClickAsync(new());
        Assert.Equal("12", panel.Find(".dnet-calendar-day-selected").TextContent.Trim());

        await picker.InvokeAsync(() => Localizer.SetCulture("en-US"));

        panel.WaitForAssertion(() => Assert.Equal("Apply", panel.Find(".dnet-datepicker-action-primary").TextContent.Trim()));
        Assert.Contains("May", panel.Find(".dnet-calendar-period-button").TextContent.Trim());
        Assert.Equal("12", panel.Find(".dnet-calendar-day-selected").TextContent.Trim());
        Assert.Same(calendar.Instance, panel.FindComponent<DnetCalendar>().Instance);
        Assert.Equal("Calendar", config!.AriaLabel);
        Assert.True(picker.Instance.IsOpen);
        Assert.Equal(string.Empty, model.Value);
    }

    [Fact]
    public async Task Typed_date_range_reformats_its_display_but_keeps_typed_values_and_instance_labels()
    {
        DnetDateRange? value = new(new DateOnly(2026, 5, 12), new DateOnly(2026, 5, 15));
        var original = value;
        var changes = 0;
        var cut = Render<DnetDateRangePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<DnetDateRange?>(this, next => { value = next; changes++; }))
            .Add(component => component.Format, "D")
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.Labels, new DnetDatePickerLabels { StartDate = "Project kickoff" }));
        Assert.Equal("Project kickoff", cut.Find(".dnet-date-range-start").GetAttribute("aria-label"));
        Assert.Equal("Fecha de fin", cut.Find(".dnet-date-range-end").GetAttribute("aria-label"));

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("End date", cut.Find(".dnet-date-range-end").GetAttribute("aria-label")));
        Assert.Equal(new DateTime(2026, 5, 12).ToString("D", Localizer.Culture), cut.Find(".dnet-date-range-start").GetAttribute("value"));
        Assert.Equal("Project kickoff", cut.Find(".dnet-date-range-start").GetAttribute("aria-label"));
        Assert.Same(original, value);
        Assert.Equal(0, changes);
    }

    [Fact]
    public async Task Date_validation_messages_use_the_current_language_and_keep_invalid_input_visible()
    {
        var model = new DateModel();
        var error = string.Empty;
        var cut = Render<DnetDatePicker>(parameters => parameters
            .Add(component => component.Value, model.Value)
            .Add(component => component.ValueExpression, () => model.Value)
            .Add(component => component.MinDayValue, new DateTime(2026, 5, 1))
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.OnError, EventCallback.Factory.Create<string>(this, next => error = next)));
        await cut.Find("input").InputAsync("2026/04/30");
        Assert.StartsWith("La fecha debe ser igual o posterior", error);
        Assert.Equal("2026/04/30", cut.Find("input").GetAttribute("value"));
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        await cut.Find("input").InputAsync("2026/04/29");
        Assert.StartsWith("Date must be on or after", error);
    }

    [Fact]
    public async Task Language_changes_keep_pending_invalid_range_text_and_its_last_valid_selection()
    {
        DnetDateRange? value = new(new DateOnly(2026, 5, 12), new DateOnly(2026, 5, 15));
        var cut = Render<DnetDateRangePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.OpenOnFocus, false));
        var original = value;
        await cut.Find(".dnet-date-range-start").InputAsync("not-a-date");

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("End date", cut.Find(".dnet-date-range-end").GetAttribute("aria-label")));
        Assert.Equal("not-a-date", cut.Find(".dnet-date-range-start").GetAttribute("value"));
        Assert.Same(original, cut.Instance.Value);
        Assert.Same(original, value);
    }

    [Fact]
    public async Task Already_open_time_overlay_translates_defaults_and_reformats_options_without_changing_selection()
    {
        RenderFragment? content = null;
        OverlayConfig? config = null;
        _overlay.OnAttach += (fragment, options) => { content = fragment; config = options; };
        TimeOnly? value = new(9, 30);
        var picker = Render<DnetTimePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.TimeOptions, new[] { new TimeOnly(9, 30), new TimeOnly(10, 0) })
            .Add(component => component.Format, "t")
            .Add(component => component.TouchUi, true));
        await picker.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        var panel = Render(content!);
        Assert.Equal("Cancelar", panel.Find(".dnet-timepicker-action").TextContent.Trim());
        Assert.Equal("Elegir hora", panel.Find(".dnet-timepicker-panel-header").TextContent.Trim());

        await picker.InvokeAsync(() => Localizer.SetCulture("en-US"));

        panel.WaitForAssertion(() => Assert.Equal("Cancel", panel.Find(".dnet-timepicker-action").TextContent.Trim()));
        Assert.Equal("Choose a time", panel.Find(".dnet-timepicker-panel-header").TextContent.Trim());
        Assert.Equal("Choose a time", config!.AriaLabel);
        Assert.Equal(new TimeOnly(9, 30).ToString("t", Localizer.Culture), panel.Find(".dnet-timepicker-option-selected span").TextContent.Trim());
        Assert.Equal(value, picker.Instance.Value);
        Assert.Equal("true", picker.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Time_panel_preserves_explicit_English_cancel_and_format_culture_overrides()
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var time = new TimeOnly(9, 30);
        var cut = Render<DnetTimePickerPanel>(parameters => parameters
            .Add(component => component.Options, new[] { time })
            .Add(component => component.Culture, culture)
            .Add(component => component.Format, "t")
            .Add(component => component.CancelLabel, "Cancel")
            .Add(component => component.TouchUi, true));
        Assert.Equal("Cancel", cut.Find(".dnet-timepicker-action").TextContent.Trim());
        Assert.Equal("Elegir hora", cut.Find(".dnet-timepicker-panel-header").TextContent.Trim());
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Choose a time", cut.Find(".dnet-timepicker-panel-header").TextContent.Trim()));
        Assert.Equal("Cancel", cut.Find(".dnet-timepicker-action").TextContent.Trim());
        Assert.Equal(time.ToString("t", culture), cut.Find("[role=option] span").TextContent.Trim());
    }

    [Fact]
    public async Task Time_validation_uses_localized_messages_and_keeps_the_existing_bound_value()
    {
        var model = new TimeModel();
        var context = new EditContext(model);
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<DnetTimePicker>(0);
            builder.AddAttribute(1, nameof(DnetTimePicker.Value), model.Value);
            builder.AddAttribute(2, nameof(DnetTimePicker.ValueExpression), (System.Linq.Expressions.Expression<Func<TimeOnly?>>)(() => model.Value));
            builder.AddAttribute(3, nameof(DnetTimePicker.Min), new TimeOnly(8, 0));
            builder.CloseComponent();
        };
        var cut = Render<EditForm>(parameters => parameters.Add(component => component.EditContext, context).Add(component => component.ChildContent, content));
        await cut.Find("input").InputAsync("07:30");
        Assert.StartsWith("La hora debe ser igual o posterior", Assert.Single(context.GetValidationMessages()));
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        await cut.Find("input").InputAsync("07:00");
        Assert.StartsWith("Time must be at or after", Assert.Single(context.GetValidationMessages()));
        Assert.Null(model.Value);
    }

    [Fact]
    public async Task Week_panels_translate_months_and_weekdays_without_changing_ISO_dates_or_selected_week()
    {
        var today = DateTime.Today;
        var cut = Render<DnetDatePickerWeekPanel>(parameters => parameters
            .Add(component => component.SelectedDay, new WeekDay { Date = today, Year = today.Year, Month = today.Month, DayNumber = today.Day }));
        var selected = cut.Instance._days.Where(day => day.IsSelected).Select(day => day.FormattedDate).ToArray();
        Assert.Equal("lun", cut.Find(".dnet-datepickerweekca-content-header-item").TextContent.Trim());
        Assert.Equal("Restablecer", cut.Find(".dnet-datepickerweek-reset-test").TextContent.Trim());
        Assert.All(cut.Instance._days, day => Assert.Equal(day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), day.FormattedDate));

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("Mon", cut.Find(".dnet-datepickerweekca-content-header-item").TextContent.Trim()));
        Assert.Equal("Reset", cut.Find(".dnet-datepickerweek-reset-test").TextContent.Trim());
        Assert.Equal(selected, cut.Instance._days.Where(day => day.IsSelected).Select(day => day.FormattedDate));
    }

    [Fact]
    public async Task Raw_week_calendar_honors_explicit_culture_while_translating_navigation_ARIA()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var cut = Render<DnetDatePickerWeekRaw>(parameters => parameters.Add(component => component.Culture, culture));
        var selected = cut.Instance._days.Where(day => day.IsSelected).Select(day => day.FormattedDate).ToArray();
        Assert.Equal("Mon", cut.Find(".dnet-datepickerweekca-raw-content-header-item").TextContent.Trim());
        Assert.Equal("Periodo siguiente", cut.Find(".dnet-datepickerweekca-raw-next-button").GetAttribute("aria-label"));
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Next period", cut.Find(".dnet-datepickerweekca-raw-next-button").GetAttribute("aria-label")));
        Assert.Equal("Mon", cut.Find(".dnet-datepickerweekca-raw-content-header-item").TextContent.Trim());
        Assert.Equal(selected, cut.Instance._days.Where(day => day.IsSelected).Select(day => day.FormattedDate));
        Assert.All(cut.Instance._days, day => Assert.Equal(day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), day.FormattedDate));
    }

    [Fact]
    public async Task Week_selector_reformats_its_own_display_without_mutating_the_saved_value_on_language_change()
    {
        var model = new DateModel();
        var changes = 0;
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<DnetDatePickerWeek>(0);
            builder.AddAttribute(1, nameof(DnetDatePickerWeek.Value), model.Value);
            builder.AddAttribute(2, nameof(DnetDatePickerWeek.ValueExpression),
                (System.Linq.Expressions.Expression<Func<string>>)(() => model.Value));
            builder.AddAttribute(3, nameof(DnetDatePickerWeek.ValueChanged),
                EventCallback.Factory.Create<string>(this, next => { model.Value = next; changes++; }));
            builder.CloseComponent();
        };
        var cut = Render<EditForm>(parameters => parameters
            .Add(component => component.Model, model)
            .Add(component => component.ChildContent, content));
        var savedValue = model.Value;
        var initialChanges = changes;
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Choose week", cut.Find(".mat-select").GetAttribute("aria-label")));
        Assert.Equal(savedValue, model.Value);
        Assert.Equal(initialChanges, changes);
        Assert.Contains(DateTime.Today.ToString("MMMM", Localizer.Culture), cut.Find(".mat-select-value-text").TextContent.Trim());
    }

    private sealed class DateModel { public string Value { get; set; } = string.Empty; }
    private sealed class TimeModel { public TimeOnly? Value { get; set; } }
}
