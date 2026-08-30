using Bunit;
using Dnet.Blazor.Components.SlideToggle;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class SlideToggleTests : BunitContext
{
    [Fact]
    public async Task Native_change_updates_value_and_accessible_state()
    {
        var value = false;
        var cut = Render<DnetSlideToggle>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<bool>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .AddChildContent("Notifications"));

        var input = cut.Find("input[type=checkbox]");
        Assert.Equal("switch", input.GetAttribute("role"));
        Assert.Equal("false", input.GetAttribute("aria-checked"));

        await input.ChangeAsync(true);

        Assert.True(value);
        Assert.Contains("dnet-slide-toggle-checked", cut.Find(".dnet-slide-toggle").ClassList);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Options_and_unmatched_attributes_reach_the_expected_elements()
    {
        var value = true;
        var cut = Render<DnetSlideToggle>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<bool>(this, next => value = next))
            .Add(component => component.ValueExpression, () => value)
            .Add(component => component.Disabled, true)
            .Add(component => component.Required, true)
            .Add(component => component.TextPlacedBefore, true)
            .Add(component => component.FullWidth, true)
            .AddUnmatched("name", "updates")
            .AddUnmatched("aria-label", "Automatic updates")
            .AddUnmatched("class", "custom-toggle")
            .AddUnmatched("style", "--dnet-slide-toggle-track-width: 60px"));

        var root = cut.Find(".dnet-slide-toggle");
        var input = cut.Find("input");

        Assert.Contains("dnet-slide-toggle-disabled", root.ClassList);
        Assert.Contains("dnet-slide-toggle-full-width", root.ClassList);
        Assert.Contains("custom-toggle", root.ClassList);
        Assert.Contains("text-before", cut.Find("label").ClassList);
        Assert.True(input.HasAttribute("disabled"));
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));
        Assert.Equal("updates", input.GetAttribute("name"));
        Assert.Equal("Automatic updates", input.GetAttribute("aria-label"));
        Assert.Contains("--dnet-slide-toggle-track-width: 60px", root.GetAttribute("style"));
    }
}
