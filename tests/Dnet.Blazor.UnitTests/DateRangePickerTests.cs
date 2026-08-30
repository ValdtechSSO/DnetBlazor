using Bunit;
using System.Globalization;
using Dnet.Blazor.Components.DatePicker;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Models;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Services;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class DateRangePickerTests : BunitContext
{
    public DateRangePickerTests()
    {
        JSInterop.SetupVoid("dnetinterop.focusElementById", _ => true);
        JSInterop.Setup<bool>("dnetinterop.matchesMedia", _ => true).SetResult(false);
    }

    [Fact]
    public async Task Two_inputs_update_a_typed_range_and_reject_inverted_boundaries()
    {
        RegisterOverlayService();
        DnetDateRange? value = null;
        var cut = Render<DnetDateRangePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<DnetDateRange?>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.OpenOnFocus, false));

        var inputs = cut.FindAll("input");
        await inputs[0].InputAsync("2026/05/10");
        await inputs[1].InputAsync("2026/05/20");

        Assert.Equal(new DnetDateRange(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 20)), value);

        await inputs[1].InputAsync("2026/05/01");
        Assert.Equal(new DateOnly(2026, 5, 20), value!.End);
        Assert.Equal("2026/05/01", cut.FindAll("input")[1].GetAttribute("value"));
    }

    [Fact]
    public void Default_strategy_builds_ordered_range_and_preview()
    {
        var strategy = new DefaultDateRangeSelectionStrategy();
        var partial = strategy.SelectionFinished(new DateOnly(2026, 5, 20), new DnetDateRange());
        var preview = strategy.CreatePreview(new DateOnly(2026, 5, 10), partial);
        var complete = strategy.SelectionFinished(new DateOnly(2026, 5, 10), partial);

        Assert.Equal(new DnetDateRange(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 20)), preview);
        Assert.Equal(preview, complete);
    }

    [Fact]
    public void Calendar_marks_range_preview_comparison_and_overlap()
    {
        var cut = Render<DnetCalendar>(parameters => parameters
            .Add(component => component.StartAt, new DateOnly(2026, 5, 1))
            .Add(component => component.RangeSelection, true)
            .Add(component => component.Range, new DnetDateRange(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 20)))
            .Add(component => component.PreviewRange, new DnetDateRange(new DateOnly(2026, 5, 5), new DateOnly(2026, 5, 8)))
            .Add(component => component.ComparisonRange, new DnetDateRange(new DateOnly(2026, 5, 15), new DateOnly(2026, 5, 25))));

        Assert.NotEmpty(cut.FindAll(".dnet-calendar-day-in-range"));
        Assert.NotEmpty(cut.FindAll(".dnet-calendar-day-in-preview"));
        Assert.NotEmpty(cut.FindAll(".dnet-calendar-day-in-comparison"));
        Assert.NotEmpty(cut.FindAll(".dnet-calendar-day-in-overlap"));
    }

    [Fact]
    public async Task Pointer_and_click_sequence_completes_one_range_without_restarting_it()
    {
        var selected = new DnetDateRange();
        var cut = Render<DnetDatePickerPanel>(parameters => parameters
            .Add(component => component.RangeSelection, true)
            .Add(component => component.Range, selected)
            .Add(component => component.StartAt, new DateOnly(2026, 9, 1))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.ShowActions, false)
            .Add(component => component.RangeChanged, EventCallback.Factory.Create<DnetDateRange>(this, range => selected = range)));

        var start = cut.Find("[aria-label='Tuesday, September 1, 2026']");
        await Assert.ThrowsAsync<MissingEventHandlerException>(() =>
            start.TriggerEventAsync("onpointerdown", new PointerEventArgs()));
        await start.ClickAsync(new());

        var end = cut.Find("[aria-label='Saturday, September 5, 2026']");
        await Assert.ThrowsAsync<MissingEventHandlerException>(() =>
            end.TriggerEventAsync("onpointerup", new PointerEventArgs()));
        await end.ClickAsync(new());

        Assert.Equal(new DnetDateRange(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5)), selected);
        Assert.Equal(5, cut.FindAll(".dnet-calendar-day-in-range").Count);
    }

    [Fact]
    public async Task Apply_commits_the_range_selected_in_the_overlay()
    {
        var overlay = new OverlayService();
        Services.AddSingleton<IOverlayService>(overlay);
        Services.AddTransient<IFormEventService, FormEventService>();
        RenderFragment? content = null;
        overlay.OnAttach += (fragment, _) => content = fragment;
        DnetDateRange? value = null;
        var picker = Render<DnetDateRangePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<DnetDateRange?>(this, range => value = range))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.StartAt, new DateOnly(2026, 9, 1))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.Responsive, false));

        await picker.Instance.OpenAsync();
        var panel = Render(content!);
        await panel.Find("[aria-label='Tuesday, September 1, 2026']").ClickAsync(new());
        await panel.Find("[aria-label='Saturday, September 5, 2026']").ClickAsync(new());
        Assert.True(panel.FindComponent<DnetDatePickerPanel>().Instance.RangeApplied.HasDelegate);
        await panel.Find(".dnet-datepicker-action-primary").ClickAsync(new());

        Assert.Equal(new DnetDateRange(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5)), value);
        Assert.False(picker.Instance.IsOpen);
    }

    [Fact]
    public async Task Panel_apply_emits_its_local_range()
    {
        DnetDateRange? applied = null;
        var cut = Render<DnetDatePickerPanel>(parameters => parameters
            .Add(component => component.RangeSelection, true)
            .Add(component => component.StartAt, new DateOnly(2026, 9, 1))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.ShowActions, true)
            .Add(component => component.RangeApplied, EventCallback.Factory.Create<DnetDateRange>(this, range => applied = range)));

        await cut.Find("[aria-label='Tuesday, September 1, 2026']").ClickAsync(new());
        await cut.Find("[aria-label='Saturday, September 5, 2026']").ClickAsync(new());
        await cut.Find(".dnet-datepicker-action-primary").ClickAsync(new());

        Assert.Equal(new DnetDateRange(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5)), applied);
    }

    [Fact]
    public async Task Responsive_range_picker_uses_modal_and_keeps_explicit_actions()
    {
        JSInterop.Setup<bool>("dnetinterop.matchesMedia", _ => true).SetResult(true);
        var overlay = new OverlayService();
        Services.AddSingleton<IOverlayService>(overlay);
        Services.AddTransient<IFormEventService, FormEventService>();
        RenderFragment? content = null;
        OverlayConfig? config = null;
        overlay.OnAttach += (fragment, options) => { content = fragment; config = options; };
        DnetDateRange? value = null;
        var picker = Render<DnetDateRangePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.ShowActions, false));

        await picker.Instance.OpenAsync();
        var panel = Render(content!);

        Assert.Equal(PositionStrategy.Global, config!.PositionStrategy);
        Assert.True(config.AriaModal);
        Assert.True(config.TrapFocus);
        Assert.Contains("dnet-datepicker-overlay-touch", config.PanelClass);
        Assert.NotNull(panel.Find(".dnet-datepicker-action-primary"));
    }

    private void RegisterOverlayService()
    {
        Services.AddSingleton<IOverlayService>(new OverlayService());
        Services.AddTransient<IFormEventService, FormEventService>();
    }
}
