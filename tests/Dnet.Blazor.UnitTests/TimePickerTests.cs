using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.TimePicker;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class TimePickerTests : BunitContext
{
    public TimePickerTests()
    {
        JSInterop.SetupVoid("dnetinterop.scrollElementIntoViewById", _ => true);
    }

    [Fact]
    public async Task Typed_input_updates_value_and_enforces_bounds()
    {
        RegisterOverlayService();
        TimeOnly? value = new(9, 0);
        var cut = Render<DnetTimePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<TimeOnly?>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.Min, new TimeOnly(8, 0))
            .Add(component => component.Max, new TimeOnly(18, 0))
            .Add(component => component.Required, true)
            .AddUnmatched("aria-label", "Start time"));

        var input = cut.Find("input");
        Assert.Equal("combobox", input.GetAttribute("role"));
        Assert.Equal("listbox", input.GetAttribute("aria-haspopup"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        Assert.Equal("Start time", input.GetAttribute("aria-label"));
        Assert.True(input.HasAttribute("required"));
        Assert.Contains("mat-input-element", input.ClassList);
        Assert.Empty(cut.FindAll(".dnet-timepicker-field"));

        await input.InputAsync("10:45");
        Assert.Equal(new TimeOnly(10, 45), value);

        await input.InputAsync("07:30");
        Assert.Equal(new TimeOnly(10, 45), value);
        Assert.Equal("07:30", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Generated_options_open_in_listbox_and_selection_updates_value()
    {
        var overlay = RegisterOverlayService();
        RenderFragment? attachedContent = null;
        OverlayConfig? attachedConfig = null;
        overlay.OnAttach += (content, config) =>
        {
            attachedContent = content;
            attachedConfig = config;
        };

        TimeOnly? value = new(9, 0);
        var cut = Render<DnetTimePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<TimeOnly?>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.Min, new TimeOnly(9, 0))
            .Add(component => component.Max, new TimeOnly(10, 0))
            .Add(component => component.Interval, TimeSpan.FromMinutes(30))
            .AddUnmatched("style", "width: 320px; --dnet-timepicker-panel-background: rebeccapurple"));

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });

        Assert.NotNull(attachedContent);
        Assert.NotNull(attachedConfig);
        Assert.Equal("listbox", attachedConfig.Role);
        Assert.Contains("dnet-timepicker-overlay", attachedConfig.PanelClass);
        Assert.Contains("--dnet-timepicker-panel-background: rebeccapurple", attachedConfig.PanelStyle);
        Assert.DoesNotContain("width: 320px", attachedConfig.PanelStyle);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.NotNull(cut.Find("input").GetAttribute("aria-controls"));

        var panel = Render(attachedContent!);
        var options = panel.FindAll("[role=option]");
        Assert.Equal(3, options.Count);
        Assert.Equal("true", options[0].GetAttribute("aria-selected"));

        await options[1].ClickAsync(new());

        Assert.Equal(new TimeOnly(9, 30), value);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Explicit_options_take_precedence_and_are_filtered_by_range()
    {
        var overlay = RegisterOverlayService();
        RenderFragment? attachedContent = null;
        overlay.OnAttach += (content, _) => attachedContent = content;

        TimeOnly? value = null;
        var cut = Render<DnetTimePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<TimeOnly?>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.Min, new TimeOnly(9, 0))
            .Add(component => component.Max, new TimeOnly(17, 0))
            .Add(component => component.TimeOptions, new[]
            {
                new TimeOnly(8, 0),
                new TimeOnly(10, 10),
                new TimeOnly(16, 40),
                new TimeOnly(18, 0)
            }));

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        var panel = Render(attachedContent!);

        Assert.Collection(
            panel.FindAll("[role=option]"),
            option => Assert.Equal("10:10", option.TextContent.Trim()),
            option => Assert.Equal("16:40", option.TextContent.Trim()));
    }

    [Fact]
    public async Task Form_field_owns_the_shell_and_its_suffix_and_clear_actions_control_the_picker()
    {
        var overlay = RegisterOverlayService();
        RenderFragment? attachedContent = null;
        OverlayConfig? attachedConfig = null;
        overlay.OnAttach += (content, config) =>
        {
            attachedContent = content;
            attachedConfig = config;
        };

        TimeOnly? value = new(11, 30);
        Expression<Func<TimeOnly?>> valueExpression = () => value;
        RenderFragment childContent = builder =>
        {
            builder.OpenComponent<DnetTimePicker>(0);
            builder.AddAttribute(1, nameof(DnetTimePicker.Value), value);
            builder.AddAttribute(
                2,
                nameof(DnetTimePicker.ValueChanged),
                EventCallback.Factory.Create<TimeOnly?>(this, next => value = next));
            builder.AddAttribute(3, nameof(DnetTimePicker.ValueExpression), valueExpression);
            builder.CloseComponent();
        };
        RenderFragment suffixContent = builder => builder.AddMarkupContent(
            0,
            "<span class=\"dnet-timepicker-icon\" aria-hidden=\"true\"></span>");

        var cut = Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, "Start time")
            .Add(component => component.UseClearButton, true)
            .Add(component => component.ChildContent, childContent)
            .Add(component => component.SufixContent, suffixContent));

        Assert.Single(cut.FindAll(".dnet-form-field-plain-control-container"));
        Assert.Empty(cut.FindAll(".dnet-timepicker-field"));
        Assert.Single(cut.FindAll(".dnet-component-clear-button"));

        await cut.Find(".dnet-form-field-plain-suffix").ClickAsync(new());

        Assert.NotNull(attachedContent);
        Assert.NotNull(attachedConfig);
        Assert.NotNull(attachedConfig.FlexibleConnectedPositionStrategyBuilder);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));

        await cut.Find(".dnet-component-clear-button").ClickAsync(new());

        Assert.Null(value);
        Assert.Null(cut.Find("input").GetAttribute("value"));
        Assert.Empty(cut.FindAll(".dnet-component-clear-button"));
    }

    [Fact]
    public void Invalid_interval_is_rejected()
    {
        RegisterOverlayService();
        TimeOnly? value = null;

        var error = Assert.ThrowsAny<Exception>(() => Render<DnetTimePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.Interval, TimeSpan.Zero)));

        Assert.Contains("Interval", error.ToString());
    }

    private OverlayService RegisterOverlayService()
    {
        var service = new OverlayService();
        Services.AddSingleton<IOverlayService>(service);
        Services.AddTransient<IFormEventService, FormEventService>();
        return service;
    }
}
