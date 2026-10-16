using Bunit;
using Dnet.Blazor.Components.Spoiler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class SpoilerTests : BunitContext
{
    [Fact]
    public void Defaults_render_a_collapsed_preview_and_link_the_control_to_unique_content()
    {
        var first = Render<DnetSpoiler>(parameters => parameters.AddChildContent("Long description"));
        var second = Render<DnetSpoiler>();

        Assert.Equal("100", first.Find(".dnet-spoiler").GetAttribute("data-max-height"));
        Assert.Equal("false", first.Find(".dnet-spoiler").GetAttribute("data-expanded"));
        var control = first.Find("button");
        Assert.Equal("button", control.GetAttribute("type"));
        Assert.Equal("false", control.GetAttribute("aria-expanded"));
        Assert.Equal("Show more", control.TextContent.Trim());
        Assert.Equal(first.Find(".dnet-spoiler-viewport").Id, control.GetAttribute("aria-controls"));
        Assert.NotEqual(control.GetAttribute("aria-controls"), second.Find("button").GetAttribute("aria-controls"));
        Assert.Empty(first.FindAll("[role=region]"));
        Assert.Equal("Long description", first.Find(".dnet-spoiler-body").TextContent);
    }

    [Fact]
    public void Internal_state_emits_changes_and_keeps_child_components_mounted()
    {
        var changes = new List<bool>();
        RenderFragment content = builder =>
        {
            builder.OpenComponent<ContentProbe>(0);
            builder.CloseComponent();
        };
        var cut = Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .Add(component => component.ExpandedChanged, value => changes.Add(value)));
        var child = cut.FindComponent<ContentProbe>().Instance;

        cut.Find("button").Click();
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal("Show less", cut.Find("button").TextContent.Trim());
        cut.Find("button").Click();
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal([true, false], changes);
        Assert.Same(child, cut.FindComponent<ContentProbe>().Instance);
    }

    [Fact]
    public void Controlled_state_waits_for_parent_acceptance_and_false_overrides_default_expansion()
    {
        bool? requested = null;
        var cut = Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.DefaultExpanded, true)
            .Add(component => component.Expanded, false)
            .Add(component => component.ExpandedChanged, value => requested = value));

        cut.Find("button").Click();
        Assert.True(requested);
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));

        cut.Render(parameters => parameters.Add(component => component.Expanded, true));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        cut.Render(parameters => parameters.Add(component => component.MaxHeight, 240));
        cut.Find("button").Click();
        Assert.False(requested);
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Default_expansion_is_only_applied_once_and_content_updates_do_not_emit_changes()
    {
        var changes = new List<bool>();
        var cut = Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.DefaultExpanded, true)
            .Add(component => component.ExpandedChanged, value => changes.Add(value))
            .AddChildContent("Initial content"));

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        cut.Render(parameters => parameters
            .Add(component => component.DefaultExpanded, false)
            .Add(component => component.MaxHeight, 150)
            .AddChildContent("Updated content"));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal("Updated content", cut.Find(".dnet-spoiler-body").TextContent);
        Assert.Empty(changes);
    }

    [Fact]
    public void Localized_labels_accessible_names_and_host_attributes_are_preserved()
    {
        var cut = Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.ShowLabel, "Ver más")
            .Add(component => component.HideLabel, "Ver menos")
            .Add(component => component.ShowAriaLabel, "Leer el artículo completo")
            .Add(component => component.HideAriaLabel, "Resumir el artículo")
            .Add(component => component.AriaLabel, "Artículo")
            .AddUnmatched("id", "article")
            .AddUnmatched("dir", "rtl")
            .AddUnmatched("class", "custom-spoiler")
            .AddUnmatched("style", "--dnet-spoiler-gap: 12px"));

        var root = cut.Find(".dnet-spoiler");
        Assert.Equal("article", root.Id);
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Contains("custom-spoiler", root.ClassList);
        Assert.Contains("--dnet-spoiler-gap: 12px", root.GetAttribute("style"));
        Assert.Equal("Artículo", cut.Find("[role=region]").GetAttribute("aria-label"));
        Assert.Equal("Leer el artículo completo", cut.Find("button").GetAttribute("aria-label"));
        Assert.Equal("Ver más", cut.Find("button").TextContent.Trim());
        cut.Find("button").Click();
        Assert.Equal("Resumir el artículo", cut.Find("button").GetAttribute("aria-label"));
        Assert.Equal("Ver menos", cut.Find("button").TextContent.Trim());
    }

    [Fact]
    public void Zero_height_is_valid_and_keeps_content_available_for_expansion()
    {
        var cut = Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.MaxHeight, 0)
            .AddChildContent("Details"));
        Assert.Equal("0", cut.Find(".dnet-spoiler").GetAttribute("data-max-height"));
        cut.Find("button").Click();
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal("Details", cut.Find(".dnet-spoiler-body").TextContent);
    }

    [Theory]
    [InlineData(-1, "Show", "Hide")]
    [InlineData(100, "", "Hide")]
    [InlineData(100, "Show", " ")]
    public void Invalid_geometry_or_empty_labels_are_rejected(int maxHeight, string showLabel, string hideLabel)
    {
        Assert.ThrowsAny<ArgumentException>(() => Render<DnetSpoiler>(parameters => parameters
            .Add(component => component.MaxHeight, maxHeight)
            .Add(component => component.ShowLabel, showLabel)
            .Add(component => component.HideLabel, hideLabel)));
    }

    public sealed class ContentProbe : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "Mounted content");
    }
}
