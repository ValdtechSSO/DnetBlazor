using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.DatePickerWeek;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class DatePickerWeekFieldTests : BunitContext
{
    private readonly OverlayService _overlay = new();

    public DatePickerWeekFieldTests()
    {
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddSingleton<IOverlayService>(_overlay);
    }

    [Theory]
    [InlineData("Saved week selection")]
    [InlineData("")]
    public void Initial_selection_and_automatically_selected_current_week_reserve_the_label_line(string initialValue)
    {
        var model = new WeekModel { Value = initialValue };
        var cut = RenderField(model);

        Assert.False(string.IsNullOrWhiteSpace(model.Value));
        if (initialValue.Length > 0) Assert.Equal(initialValue, model.Value);
        Assert.Equal(model.Value, cut.Find(".mat-select-value-text").TextContent.Trim());
        Assert.Equal("Current week", cut.Find("label").TextContent);
        AssertRaised(cut, true);
    }

    [Fact]
    public void External_clear_and_replacement_update_the_visible_value_and_label_without_remounting_the_picker()
    {
        var model = new WeekModel { Value = "Initial selection" };
        var cut = RenderField(model);
        var picker = cut.FindComponent<DnetDatePickerWeek>().Instance;

        model.Value = string.Empty;
        cut.Render(parameters => parameters.Add(component => component.Model, model));
        AssertRaised(cut, false);
        Assert.Empty(cut.FindAll(".mat-select-value-text"));
        Assert.Equal("Choose a week", cut.Find(".mat-select-placeholder").TextContent.Trim());

        model.Value = "External replacement week";
        cut.Render(parameters => parameters.Add(component => component.Model, model));
        Assert.Equal(model.Value, cut.Find(".mat-select-value-text").TextContent.Trim());
        AssertRaised(cut, true);
        Assert.Same(picker, cut.FindComponent<DnetDatePickerWeek>().Instance);
    }

    [Fact]
    public async Task Programmatic_date_update_populates_the_week_and_raises_a_previously_empty_label()
    {
        var model = new WeekModel { Value = "Initial selection" };
        var cut = RenderField(model);
        var picker = cut.FindComponent<DnetDatePickerWeek>();
        model.Value = string.Empty;
        cut.Render(parameters => parameters.Add(component => component.Model, model));
        AssertRaised(cut, false);

        await cut.InvokeAsync(() => picker.Instance.UpdateValue("2026-05-13"));

        var monday = new DateTime(2026, 5, 11).ToString("MMMM dd", CultureInfo.CurrentCulture);
        var sunday = new DateTime(2026, 5, 17).ToString("MMMM dd", CultureInfo.CurrentCulture);
        Assert.Equal($"2026 / {monday} - {sunday}", model.Value);
        Assert.Equal(model.Value, cut.Find(".mat-select-value-text").TextContent.Trim());
        AssertRaised(cut, true);
    }

    [Fact]
    public async Task Selecting_a_day_in_the_opened_panel_restores_the_week_value_and_label_after_an_external_clear()
    {
        RenderFragment? attachedContent = null;
        _overlay.OnAttach += (content, _) => attachedContent = content;
        var model = new WeekModel();
        var cut = RenderField(model);
        await cut.Find(".mat-select").ClickAsync(new());
        Assert.NotNull(attachedContent);
        var panel = Render(attachedContent!);

        model.Value = string.Empty;
        cut.Render(parameters => parameters.Add(component => component.Model, model));
        AssertRaised(cut, false);
        await panel.FindAll(".dnet-datepickerweekca-day")
            .First(day => day.TextContent.Trim() == "12").ClickAsync(new());

        cut.WaitForAssertion(() =>
        {
            Assert.False(string.IsNullOrWhiteSpace(model.Value));
            Assert.Equal(model.Value, cut.Find(".mat-select-value-text").TextContent.Trim());
            AssertRaised(cut, true);
        });
    }

    private IRenderedComponent<EditForm> RenderField(WeekModel model)
    {
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<DnetFormField>(0);
            builder.AddAttribute(1, nameof(DnetFormField.Label), "Current week");
            builder.AddAttribute(2, nameof(DnetFormField.ChildContent), (RenderFragment)(child =>
            {
                child.OpenComponent<DnetDatePickerWeek>(0);
                child.AddAttribute(1, nameof(DnetDatePickerWeek.Value), model.Value);
                child.AddAttribute(2, nameof(DnetDatePickerWeek.ValueExpression), (Expression<Func<string>>)(() => model.Value));
                child.AddAttribute(3, nameof(DnetDatePickerWeek.ValueChanged),
                    EventCallback.Factory.Create<string>(this, value => model.Value = value));
                child.AddAttribute(4, nameof(DnetDatePickerWeek.PlaceHolder), "Choose a week");
                child.AddAttribute(5, nameof(DnetDatePickerWeek.Culture), CultureInfo.CurrentCulture);
                child.CloseComponent();
            }));
            builder.CloseComponent();
        };
        return Render<EditForm>(parameters => parameters
            .Add(component => component.Model, model)
            .Add(component => component.ChildContent, content));
    }

    private static void AssertRaised(IRenderedComponent<EditForm> cut, bool raised) =>
        Assert.Equal(raised, cut.Find(".dnet-form-field-plain-wrapper").ClassList.Contains("dnet-form-field-label-raised"));

    private sealed class WeekModel
    {
        public string Value { get; set; } = string.Empty;
    }
}
