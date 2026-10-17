using Bunit;
using Dnet.Blazor.Components.Button;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ButtonSizingTests : BunitContext
{
    [Fact]
    public void Changing_size_keeps_consumer_attributes_button_semantics_and_click_callback()
    {
        var clicks = 0;
        var cut = Render<DnetButton>(parameters => parameters
            .Add(component => component.Size, DnetButtonSize.Small)
            .Add(component => component.ButtonType, "submit")
            .Add(component => component.OnClick, EventCallback.Factory.Create(this, () => clicks++))
            .AddUnmatched("id", "save")
            .AddUnmatched("class", "consumer-action")
            .AddUnmatched("style", "--dnet-btn-height:48px;--dnet-btn-radius:3px;")
            .AddUnmatched("aria-label", "Save project")
            .AddChildContent("Save"));

        cut.Find("button").Click();
        cut.Render(parameters => parameters.Add(component => component.Size, DnetButtonSize.Large));
        var button = cut.Find("#save");
        Assert.Contains("dnet-button-size-large", button.ClassList);
        Assert.DoesNotContain("dnet-button-size-small", button.ClassList);
        Assert.Contains("consumer-action", button.ClassList);
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Save project", button.GetAttribute("aria-label"));
        Assert.Equal("--dnet-btn-height:48px;--dnet-btn-radius:3px;", button.GetAttribute("style"));
        button.Click();
        Assert.Equal(2, clicks);
        Assert.Single(cut.FindAll("button"));
    }

    [Fact]
    public async Task Adding_an_icon_and_changing_size_preserves_stateful_child_content()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<StatefulContent>(0);
            builder.CloseComponent();
        };
        RenderFragment icon = builder => builder.AddMarkupContent(0,
            "<svg viewBox='0 0 24 24'><title>Decorative plus</title><path d='M12 4v16M4 12h16'/></svg>");
        var cut = Render<DnetButton>(parameters => parameters.Add(component => component.ChildContent, content));
        Assert.Equal(DnetButtonSize.Medium, cut.Instance.Size);
        Assert.Empty(cut.FindAll(".dnet-button-icon"));
        var child = cut.FindComponent<StatefulContent>().Instance;
        await cut.InvokeAsync(child.Increment);

        cut.Render(parameters => parameters
            .Add(component => component.IconContent, icon)
            .Add(component => component.Size, DnetButtonSize.Small));

        Assert.Same(child, cut.FindComponent<StatefulContent>().Instance);
        Assert.Equal("Save 1", cut.Find(".button-stateful-label").TextContent);
        var iconHost = cut.Find(".dnet-button-icon");
        Assert.Equal("true", iconHost.GetAttribute("aria-hidden"));
        Assert.Equal(iconHost, cut.Find(".dnet-button-wrapper").FirstElementChild);
        Assert.Single(iconHost.QuerySelectorAll("svg"));

        cut.Render(parameters => parameters.Add(component => component.IconContent, (RenderFragment?)null));
        Assert.Empty(cut.FindAll(".dnet-button-icon"));
        Assert.Same(child, cut.FindComponent<StatefulContent>().Instance);
        Assert.Equal("Save 1", cut.Find(".button-stateful-label").TextContent);
        Assert.False(child.Disposed);
    }

    [Fact]
    public void Unknown_sizes_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<DnetButton>(parameters => parameters
            .Add(component => component.Size, (DnetButtonSize)99)));
    }

    public sealed class StatefulContent : ComponentBase, IDisposable
    {
        private int _count;
        public bool Disposed { get; private set; }

        public void Increment()
        {
            _count++;
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "button-stateful-label");
            builder.AddContent(2, $"Save {_count}");
            builder.CloseElement();
        }

        public void Dispose() => Disposed = true;
    }
}
