using Bunit;
using Dnet.Blazor.Components.Skeleton;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class SkeletonTests : BunitContext
{
    [Fact]
    public void A_standalone_placeholder_is_decorative_and_animated_by_default()
    {
        var cut = Render<DnetSkeleton>();
        var root = cut.Find(".dnet-skeleton");

        Assert.Equal("true", root.GetAttribute("data-visible"));
        Assert.Equal("true", root.GetAttribute("data-animate"));
        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.False(root.HasAttribute("aria-busy"));
        Assert.False(root.HasAttribute("style"));
        Assert.Empty(cut.FindAll(".dnet-skeleton-content"));
    }

    [Fact]
    public void Wrapped_loading_content_is_busy_hidden_and_inert()
    {
        var cut = Render<DnetSkeleton>(parameters => parameters
            .AddChildContent("<button type=\"button\">Continue</button>"));
        var root = cut.Find(".dnet-skeleton");
        var content = cut.Find(".dnet-skeleton-content");

        Assert.Contains("dnet-skeleton-with-content", root.ClassList);
        Assert.Equal("true", root.GetAttribute("aria-busy"));
        Assert.False(root.HasAttribute("aria-hidden"));
        Assert.True(content.HasAttribute("inert"));
        Assert.Equal("true", content.GetAttribute("aria-hidden"));
        Assert.Equal("Continue", cut.Find("button").TextContent);
    }

    [Fact]
    public void Revealing_content_removes_the_loading_accessibility_restrictions()
    {
        var cut = Render<DnetSkeleton>(parameters => parameters
            .Add(component => component.Visible, false)
            .AddChildContent("<a href=\"/\">Read more</a>"));

        Assert.Equal("false", cut.Find(".dnet-skeleton").GetAttribute("data-visible"));
        Assert.Equal("false", cut.Find(".dnet-skeleton").GetAttribute("aria-busy"));
        Assert.False(cut.Find(".dnet-skeleton-content").HasAttribute("inert"));
        Assert.False(cut.Find(".dnet-skeleton-content").HasAttribute("aria-hidden"));
        Assert.Equal("/", cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public async Task Toggling_loading_keeps_the_same_child_component_and_its_state()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<StatefulProbe>(0);
            builder.CloseComponent();
        };
        var cut = Render<DnetSkeleton>(parameters => parameters
            .Add(component => component.Visible, false)
            .Add(component => component.ChildContent, content));
        var child = cut.FindComponent<StatefulProbe>().Instance;
        await cut.InvokeAsync(() => child.UpdateNote("Preserved value"));

        cut.Render(parameters => parameters.Add(component => component.Visible, true));
        Assert.Same(child, cut.FindComponent<StatefulProbe>().Instance);
        Assert.Equal("Preserved value", child.Note);
        cut.Render(parameters => parameters.Add(component => component.Visible, false));
        Assert.Same(child, cut.FindComponent<StatefulProbe>().Instance);
        Assert.False(child.Disposed);
        Assert.Equal("Preserved value", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void Explicit_geometry_is_optional_and_merges_with_the_consumers_style()
    {
        var cut = Render<DnetSkeleton>(parameters => parameters
            .Add(component => component.Width, "70%")
            .Add(component => component.Height, "2.5rem")
            .AddUnmatched("style", "--dnet-skeleton-background: var(--dnet-sys-primary); margin: 4px;"));

        var style = cut.Find(".dnet-skeleton").GetAttribute("style");
        Assert.Contains("--dnet-skeleton-background: var(--dnet-sys-primary)", style);
        Assert.Contains("margin: 4px", style);
        Assert.Contains("--_width: 70%", style);
        Assert.Contains("--_height: 2.5rem", style);

        cut.Render(parameters => parameters
            .Add(component => component.Width, (string?)null)
            .Add(component => component.Height, (string?)null));
        style = cut.Find(".dnet-skeleton").GetAttribute("style");
        Assert.DoesNotContain("--_width", style);
        Assert.DoesNotContain("--_height", style);
        Assert.Contains("margin: 4px", style);
    }

    [Fact]
    public void Circle_and_static_modes_can_be_combined_with_custom_attributes()
    {
        var cut = Render<DnetSkeleton>(parameters => parameters
            .Add(component => component.Circle, true)
            .Add(component => component.Animate, false)
            .Add(component => component.Height, "48px")
            .Add(component => component.Width, "200px")
            .AddUnmatched("id", "avatar-placeholder")
            .AddUnmatched("class", "custom-placeholder")
            .AddUnmatched("dir", "rtl")
            .AddUnmatched("data-example", "avatar"));
        var root = cut.Find(".dnet-skeleton");

        Assert.Contains("dnet-skeleton-circle", root.ClassList);
        Assert.Contains("custom-placeholder", root.ClassList);
        Assert.Equal("false", root.GetAttribute("data-animate"));
        Assert.Equal("avatar-placeholder", root.Id);
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Equal("avatar", root.GetAttribute("data-example"));
    }

    [Fact]
    public void Hidden_standalone_placeholders_remain_decorative()
    {
        var cut = Render<DnetSkeleton>(parameters => parameters
            .Add(component => component.Visible, false));

        Assert.Equal("false", cut.Find(".dnet-skeleton").GetAttribute("data-visible"));
        Assert.Equal("true", cut.Find(".dnet-skeleton").GetAttribute("aria-hidden"));
        Assert.False(cut.Find(".dnet-skeleton").HasAttribute("aria-busy"));
    }

    public sealed class StatefulProbe : ComponentBase, IDisposable
    {
        public string Note { get; set; } = "Initial value";
        public bool Disposed { get; private set; }

        public void UpdateNote(string value)
        {
            Note = value;
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "value", Note);
            builder.CloseElement();
        }

        public void Dispose() => Disposed = true;
    }
}
