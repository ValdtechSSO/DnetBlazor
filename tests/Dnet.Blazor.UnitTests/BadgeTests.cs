using Bunit;
using Dnet.Blazor.Components.Badge;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class BadgeTests : BunitContext
{
    [Fact]
    public void Renders_content_with_accessible_description()
    {
        var cut = Render<DnetBadge>(parameters => parameters
            .Add(component => component.Content, 5)
            .Add(component => component.Description, "5 unread messages")
            .AddChildContent("Inbox"));

        var root = cut.Find(".dnet-badge");
        var indicator = cut.Find(".dnet-badge-indicator");

        Assert.Contains("dnet-badge-above", root.ClassList);
        Assert.Contains("dnet-badge-after", root.ClassList);
        Assert.Contains("dnet-badge-size-medium", root.ClassList);
        Assert.Equal("5", indicator.TextContent);
        Assert.Equal("true", indicator.GetAttribute("aria-hidden"));
        Assert.Equal("5 unread messages", cut.Find(".dnet-badge-description").TextContent);
    }

    [Fact]
    public void Applies_options_and_forwards_host_attributes()
    {
        var cut = Render<DnetBadge>(parameters => parameters
            .Add(component => component.Content, "New")
            .Add(component => component.Position, DnetBadgePosition.BelowBefore)
            .Add(component => component.Size, DnetBadgeSize.Large)
            .Add(component => component.Color, DnetBadgeColor.Success)
            .Add(component => component.Overlap, false)
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "custom-badge")
            .AddUnmatched("data-testid", "status")
            .AddUnmatched("aria-disabled", "false")
            .AddChildContent("Downloads"));

        var root = cut.Find(".dnet-badge");

        Assert.Contains("dnet-badge-below", root.ClassList);
        Assert.Contains("dnet-badge-before", root.ClassList);
        Assert.Contains("dnet-badge-size-large", root.ClassList);
        Assert.Contains("dnet-badge-color-success", root.ClassList);
        Assert.Contains("dnet-badge-no-overlap", root.ClassList);
        Assert.Contains("dnet-badge-disabled", root.ClassList);
        Assert.Contains("custom-badge", root.ClassList);
        Assert.Equal("status", root.GetAttribute("data-testid"));
        Assert.Equal("true", root.GetAttribute("aria-disabled"));
    }

    [Theory]
    [InlineData(DnetBadgePosition.Before, "dnet-badge-above", "dnet-badge-before")]
    [InlineData(DnetBadgePosition.After, "dnet-badge-above", "dnet-badge-after")]
    [InlineData(DnetBadgePosition.Above, "dnet-badge-above", "dnet-badge-after")]
    [InlineData(DnetBadgePosition.Below, "dnet-badge-below", "dnet-badge-after")]
    public void Supports_position_shortcuts(
        DnetBadgePosition position,
        string verticalClass,
        string horizontalClass)
    {
        var cut = Render<DnetBadge>(parameters => parameters
            .Add(component => component.Content, 1)
            .Add(component => component.Position, position));

        var root = cut.Find(".dnet-badge");
        Assert.Contains(verticalClass, root.ClassList);
        Assert.Contains(horizontalClass, root.ClassList);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("0", true)]
    public void Empty_content_is_hidden_but_zero_is_visible(string? content, bool isVisible)
    {
        var cut = Render<DnetBadge>(parameters => parameters
            .Add(component => component.Content, content));

        Assert.Equal(isVisible, cut.FindAll(".dnet-badge-indicator").Count == 1);
        Assert.Equal(!isVisible, cut.Find(".dnet-badge").ClassList.Contains("dnet-badge-hidden"));
    }

    [Fact]
    public void Hidden_suppresses_only_the_visual_indicator()
    {
        var cut = Render<DnetBadge>(parameters => parameters
            .Add(component => component.Content, 3)
            .Add(component => component.Hidden, true)
            .Add(component => component.Description, "Three notifications")
            .AddChildContent("Notifications"));

        Assert.Empty(cut.FindAll(".dnet-badge-indicator"));
        Assert.Equal("Three notifications", cut.Find(".dnet-badge-description").TextContent);
        Assert.Contains("Notifications", cut.Markup);
    }
}
