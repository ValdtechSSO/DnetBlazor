using Bunit;
using Dnet.Blazor.Components.Slider;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class SliderTests : BunitContext
{
    [Fact]
    public async Task Single_slider_updates_bound_value_and_accessible_text()
    {
        var value = 25d;
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, EventCallback.Factory.Create<double>(this, next => value = next))
            .Add(component => component.Min, 0)
            .Add(component => component.Max, 100)
            .Add(component => component.Step, 5)
            .Add(component => component.AriaLabel, "Volume")
            .Add(component => component.DisplayWith, number => $"{number}%"));

        var input = cut.Find("input[type=range]");
        Assert.Equal("Volume", input.GetAttribute("aria-label"));
        Assert.Equal("25%", input.GetAttribute("aria-valuetext"));
        Assert.Equal("25", input.GetAttribute("value"));

        await input.InputAsync("42");

        Assert.Equal(40, value);
        Assert.Equal("40%", cut.Find("input").GetAttribute("aria-valuetext"));
        Assert.Contains("--_end-percent: 40%", cut.Find(".dnet-slider").GetAttribute("style"));
    }

    [Fact]
    public async Task Range_slider_renders_two_constrained_inputs()
    {
        var start = 20d;
        var end = 70d;
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Range, true)
            .Add(component => component.StartValue, start)
            .Add(component => component.StartValueChanged, EventCallback.Factory.Create<double>(this, next => start = next))
            .Add(component => component.EndValue, end)
            .Add(component => component.EndValueChanged, EventCallback.Factory.Create<double>(this, next => end = next))
            .Add(component => component.Min, 0)
            .Add(component => component.Max, 100)
            .Add(component => component.Step, 10)
            .Add(component => component.StartAriaLabel, "Minimum price")
            .Add(component => component.EndAriaLabel, "Maximum price"));

        var inputs = cut.FindAll("input[type=range]");
        Assert.Equal(2, inputs.Count);
        Assert.Equal("70", inputs[0].GetAttribute("max"));
        Assert.Equal("20", inputs[1].GetAttribute("min"));

        await inputs[0].InputAsync("60");
        Assert.Equal(60, start);
        Assert.Equal(70, end);

        await cut.Find(".dnet-slider-input-end").InputAsync("50");
        Assert.Equal(60, end);
    }

    [Fact]
    public void Range_slider_orders_inverted_initial_values_without_losing_an_endpoint()
    {
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Range, true)
            .Add(component => component.StartValue, 80)
            .Add(component => component.EndValue, 20)
            .Add(component => component.Min, 0)
            .Add(component => component.Max, 100));

        var inputs = cut.FindAll("input[type=range]");
        Assert.Equal("20", inputs[0].GetAttribute("value"));
        Assert.Equal("80", inputs[1].GetAttribute("value"));
        Assert.Contains("--_start-percent: 20%", cut.Find(".dnet-slider").GetAttribute("style"));
        Assert.Contains("--_end-percent: 80%", cut.Find(".dnet-slider").GetAttribute("style"));
    }

    [Fact]
    public void Tick_marks_follow_step_and_active_range()
    {
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, 50)
            .Add(component => component.Min, 0)
            .Add(component => component.Max, 100)
            .Add(component => component.Step, 25)
            .Add(component => component.ShowTickMarks, true));

        Assert.Equal(5, cut.FindAll(".dnet-slider-tick").Count);
        Assert.Equal(3, cut.FindAll(".dnet-slider-tick-active").Count);
    }

    [Fact]
    public void Discrete_slider_uses_formatted_visible_and_accessible_labels()
    {
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, 1_500)
            .Add(component => component.Max, 2_000)
            .Add(component => component.Step, 100)
            .Add(component => component.Discrete, true)
            .Add(component => component.DisplayWith, number =>
                $"{(number / 1_000).ToString("0.#", CultureInfo.InvariantCulture)}k"));

        Assert.Equal("1.5k", cut.Find(".dnet-slider-value-indicator").TextContent);
        Assert.Equal("1.5k", cut.Find("input").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public async Task Input_change_and_drag_events_identify_the_thumb()
    {
        var events = new List<(string Name, DnetSliderThumb Thumb, double Value)>();
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, 10)
            .Add(component => component.OnInput, EventCallback.Factory.Create<DnetSliderEventArgs>(
                this, args => events.Add(("input", args.Thumb, args.Value))))
            .Add(component => component.OnChange, EventCallback.Factory.Create<DnetSliderEventArgs>(
                this, args => events.Add(("change", args.Thumb, args.Value))))
            .Add(component => component.OnDragStart, EventCallback.Factory.Create<DnetSliderEventArgs>(
                this, args => events.Add(("start", args.Thumb, args.Value))))
            .Add(component => component.OnDragEnd, EventCallback.Factory.Create<DnetSliderEventArgs>(
                this, args => events.Add(("end", args.Thumb, args.Value)))));

        var input = cut.Find("input");
        await input.TriggerEventAsync("onpointerdown", new PointerEventArgs());
        await input.InputAsync("20");
        await input.ChangeAsync("20");
        await input.TriggerEventAsync("onpointerup", new PointerEventArgs());

        Assert.Collection(
            events,
            item => Assert.Equal(("start", DnetSliderThumb.Single, 10d), item),
            item => Assert.Equal(("input", DnetSliderThumb.Single, 20d), item),
            item => Assert.Equal(("change", DnetSliderThumb.Single, 20d), item),
            item => Assert.Equal(("end", DnetSliderThumb.Single, 20d), item));
    }

    [Fact]
    public void Host_and_input_attributes_are_applied_to_the_expected_elements()
    {
        var inputAttributes = new Dictionary<string, object>
        {
            ["class"] = "custom-input",
            ["aria-describedby"] = "slider-help"
        };
        var cut = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, 30)
            .Add(component => component.InputAttributes, inputAttributes)
            .AddUnmatched("class", "custom-slider")
            .AddUnmatched("data-testid", "slider")
            .AddUnmatched("style", "--dnet-slider-active-track-color: rebeccapurple"));

        var root = cut.Find(".dnet-slider");
        var input = cut.Find("input");
        Assert.Contains("custom-slider", root.ClassList);
        Assert.Equal("slider", root.GetAttribute("data-testid"));
        Assert.Contains("--dnet-slider-active-track-color: rebeccapurple", root.GetAttribute("style"));
        Assert.Contains("custom-input", input.ClassList);
        Assert.Equal("slider-help", input.GetAttribute("aria-describedby"));
    }

    [Theory]
    [InlineData(10, 10, 1)]
    [InlineData(0, 100, 0)]
    [InlineData(0, 100, -1)]
    public void Invalid_bounds_or_step_fail_fast(double min, double max, double step)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<DnetSlider>(parameters => parameters
            .Add(component => component.Min, min)
            .Add(component => component.Max, max)
            .Add(component => component.Step, step)));

        Assert.Contains("must be", exception.ToString());
    }
}
