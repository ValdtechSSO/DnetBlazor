using System.Globalization;
using Bunit;
using Dnet.Blazor.Components.DatePicker;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Enums;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Models;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class DatePickerTests : BunitContext
{
    public DatePickerTests()
    {
        JSInterop.SetupVoid("dnetinterop.focusElementById", _ => true);
        JSInterop.Setup<bool>("dnetinterop.matchesMedia", _ => true).SetResult(false);
    }

    [Fact]
    public async Task Legacy_string_api_parses_and_enforces_constraints()
    {
        RegisterOverlayService();
        var value = "2026/05/10";
        var cut = Render<DnetDatePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<string>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.MinDayValue, new DateTime(2026, 5, 1))
            .Add(component => component.MaxDayValue, new DateTime(2026, 5, 31))
            .Add(component => component.OpenOnFocus, false));

        var input = cut.Find("input");
        Assert.Equal("dialog", input.GetAttribute("aria-haspopup"));

        await input.InputAsync("2026/05/20");
        Assert.Equal("2026/05/20", value);

        await input.InputAsync("2026/06/01");
        Assert.Equal("2026/05/20", value);
        Assert.Equal("2026/06/01", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Calendar_uses_culture_grid_roles_and_keyboard_navigation()
    {
        DateOnly? selected = null;
        var cut = Render<DnetCalendar>(parameters => parameters
            .Add(component => component.StartAt, new DateOnly(2026, 5, 15))
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("es-ES"))
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, value => selected = value)));

        Assert.Equal("LU", cut.Find("[role=columnheader]").TextContent.Trim(), ignoreCase: true);
        Assert.Equal("lunes", cut.Find("[role=columnheader]").GetAttribute("aria-label"), ignoreCase: true);
        Assert.Equal(42, cut.FindAll("[role=gridcell]").Count);

        var active = cut.Find("[role=gridcell][tabindex='0']");
        var activeId = active.Id;
        await active.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        var moved = cut.Find("[role=gridcell][tabindex='0']");
        Assert.NotEqual(activeId, moved.Id);

        await moved.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        Assert.NotNull(selected);
    }

    [Fact]
    public async Task Picker_opens_programmatically_and_commits_panel_selection()
    {
        var overlay = RegisterOverlayService();
        RenderFragment? content = null;
        OverlayConfig? config = null;
        overlay.OnAttach += (fragment, options) => { content = fragment; config = options; };
        var value = string.Empty;
        var cut = Render<DnetDatePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<string>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.FirstDayToShow, new DateTime(2026, 5, 1))
            .Add(component => component.OpenOnFocus, false)
            .Add(component => component.Responsive, false));

        await cut.Instance.OpenAsync();
        Assert.True(cut.Instance.IsOpen);
        Assert.Equal(PositionStrategy.FlexibleConnectedTo, config!.PositionStrategy);

        var panel = Render(content!);
        await panel.FindAll("[role=gridcell]").First(element => element.TextContent.Trim() == "12" && !element.HasAttribute("disabled")).ClickAsync(new());

        Assert.Equal("2026/05/12", value);
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task Responsive_picker_uses_modal_focus_trapped_overlay()
    {
        JSInterop.Setup<bool>("dnetinterop.matchesMedia", _ => true).SetResult(true);
        var overlay = RegisterOverlayService();
        OverlayConfig? config = null;
        overlay.OnAttach += (_, options) => config = options;
        var value = string.Empty;
        var cut = Render<DnetDatePicker>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.OpenOnFocus, false));

        await cut.Instance.OpenAsync();

        Assert.Equal(PositionStrategy.Global, config!.PositionStrategy);
        Assert.True(config.AriaModal);
        Assert.True(config.TrapFocus);
        Assert.Equal(OverlayScrollStrategy.Block, config.ScrollStrategy);
        Assert.Contains("dnet-datepicker-overlay-touch", config.PanelClass);
    }

    private OverlayService RegisterOverlayService()
    {
        var service = new OverlayService();
        Services.AddSingleton<IOverlayService>(service);
        Services.AddTransient<IFormEventService, FormEventService>();
        return service;
    }
}
