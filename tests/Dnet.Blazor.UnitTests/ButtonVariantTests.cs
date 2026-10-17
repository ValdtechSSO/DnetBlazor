using Bunit;
using Dnet.Blazor.Components.Button;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ButtonVariantTests : BunitContext
{
    [Fact]
    public void Default_button_is_filled_without_inline_appearance_that_would_block_theme_tokens()
    {
        var cut = Render<DnetButton>(parameters => parameters.AddChildContent("Save"));

        Assert.Equal(DnetButtonVariant.Filled, cut.Instance.Variant);
        Assert.Equal(DnetButtonSize.Medium, cut.Instance.Size);
        var button = cut.Find("button");
        Assert.Contains("dnet-button-variant-filled", button.ClassList);
        Assert.DoesNotContain("dnet-button-variant-outlined", button.ClassList);
        Assert.Null(button.GetAttribute("style"));
        Assert.Equal("button", button.GetAttribute("type"));
    }

    [Fact]
    public async Task Changing_variant_preserves_child_state_size_consumer_tokens_and_actions()
    {
        var clicks = 0;
        var cut = Render<DnetButton>(parameters => parameters
            .Add(component => component.Size, DnetButtonSize.Large)
            .Add(component => component.ButtonType, "submit")
            .Add(component => component.OnClick, EventCallback.Factory.Create(this, () => clicks++))
            .Add(component => component.ChildContent, builder =>
            {
                builder.OpenComponent<ButtonSizingTests.StatefulContent>(0);
                builder.CloseComponent();
            })
            .AddUnmatched("id", "save")
            .AddUnmatched("class", "consumer-action")
            .AddUnmatched("style", "--dnet-btn-background:rebeccapurple;--dnet-btn-radius:3px;")
            .AddUnmatched("aria-label", "Save profile"));
        var child = cut.FindComponent<ButtonSizingTests.StatefulContent>().Instance;
        await cut.InvokeAsync(child.Increment);
        cut.Find("button").Click();

        cut.Render(parameters => parameters.Add(component => component.Variant, DnetButtonVariant.Outlined));

        var button = cut.Find("#save");
        Assert.Contains("dnet-button-variant-outlined", button.ClassList);
        Assert.DoesNotContain("dnet-button-variant-filled", button.ClassList);
        Assert.Contains("dnet-button-size-large", button.ClassList);
        Assert.Contains("consumer-action", button.ClassList);
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Save profile", button.GetAttribute("aria-label"));
        Assert.Equal("--dnet-btn-background:rebeccapurple;--dnet-btn-radius:3px;", button.GetAttribute("style"));
        Assert.Same(child, cut.FindComponent<ButtonSizingTests.StatefulContent>().Instance);
        Assert.Equal("Save 1", cut.Find(".button-stateful-label").TextContent);
        button.Click();
        Assert.Equal(2, clicks);

        cut.Render(parameters => parameters.Add(component => component.Variant, DnetButtonVariant.Filled));
        Assert.Contains("dnet-button-variant-filled", cut.Find("button").ClassList);
        Assert.DoesNotContain("dnet-button-variant-outlined", cut.Find("button").ClassList);
        Assert.Same(child, cut.FindComponent<ButtonSizingTests.StatefulContent>().Instance);
        Assert.False(child.Disposed);
    }

    [Fact]
    public void Unknown_variants_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<DnetButton>(parameters => parameters
            .Add(component => component.Variant, (DnetButtonVariant)99)));
    }
}
