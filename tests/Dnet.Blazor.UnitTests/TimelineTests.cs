using Bunit;
using Dnet.Blazor.Components.Timeline;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class TimelineTests : BunitContext
{
    [Fact]
    public void Events_render_as_an_ordered_list_with_explicit_status_and_current_step()
    {
        var cut = Render<DnetTimeline>(parameters => parameters
            .Add(component => component.AriaLabel, "Delivery progress")
            .AddChildContent(Items(
                ("Received", DnetTimelineItemStatus.Completed),
                ("Preparing", DnetTimelineItemStatus.Current),
                ("Delivered", DnetTimelineItemStatus.Pending))));

        Assert.Equal("list", cut.Find("ol").GetAttribute("role"));
        Assert.Equal("Delivery progress", cut.Find("ol").GetAttribute("aria-label"));
        var items = cut.FindAll("ol > li");
        Assert.Equal(3, items.Count);
        Assert.Contains("dnet-timeline-item-completed", items[0].ClassList);
        Assert.Contains("dnet-timeline-item-current", items[1].ClassList);
        Assert.Contains("dnet-timeline-item-pending", items[2].ClassList);
        Assert.Equal("step", items[1].GetAttribute("aria-current"));
        Assert.Single(cut.FindAll("[aria-current]"));
        Assert.Equal(["Received", "Preparing", "Delivered"], cut.FindAll(".dnet-timeline-title").Select(element => element.TextContent));
        Assert.All(cut.FindAll(".dnet-timeline-bullet"), bullet => Assert.Equal("true", bullet.GetAttribute("aria-hidden")));
        Assert.Equal(["Completado", "Actual", "Pendiente"], cut.FindAll(".dnet-timeline-status").Select(element => element.TextContent));
    }

    [Theory]
    [InlineData(DnetTimelineAlignment.Start, "dnet-timeline-align-start")]
    [InlineData(DnetTimelineAlignment.End, "dnet-timeline-align-end")]
    [InlineData(DnetTimelineAlignment.Alternate, "dnet-timeline-align-alternate")]
    public void Alignment_and_host_attributes_are_applied_to_the_ordered_list(DnetTimelineAlignment alignment, string expectedClass)
    {
        var cut = Render<DnetTimeline>(parameters => parameters
            .Add(component => component.Alignment, alignment)
            .AddUnmatched("id", "activity")
            .AddUnmatched("class", "custom-timeline")
            .AddUnmatched("dir", "rtl")
            .AddUnmatched("style", "--dnet-timeline-gap: 40px"));

        var root = cut.Find("ol");
        Assert.Equal("activity", root.Id);
        Assert.Contains(expectedClass, root.ClassList);
        Assert.Contains("custom-timeline", root.ClassList);
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Equal("--dnet-timeline-gap: 40px", root.GetAttribute("style"));
        Assert.Empty(cut.FindAll("li"));
    }

    [Fact]
    public void Rich_content_custom_bullet_secondary_metadata_and_localized_status_are_preserved()
    {
        var cut = Render<DnetTimelineItem>(parameters => parameters
            .Add(component => component.Title, "Review")
            .Add(component => component.Status, DnetTimelineItemStatus.Current)
            .Add(component => component.StatusLabel, "En curso")
            .Add(component => component.BulletContent, "A")
            .Add(component => component.OppositeContent, "Design team")
            .AddChildContent("<a href='/details'>Read the proposal</a>")
            .AddUnmatched("id", "review")
            .AddUnmatched("class", "my-event")
            .AddUnmatched("data-project", "42"));

        Assert.Equal("review", cut.Find("li").Id);
        Assert.Contains("my-event", cut.Find("li").ClassList);
        Assert.Contains("dnet-timeline-item-custom-bullet", cut.Find("li").ClassList);
        Assert.Equal("42", cut.Find("li").GetAttribute("data-project"));
        Assert.Equal("A", cut.Find(".dnet-timeline-bullet").TextContent);
        Assert.Equal("Design team", cut.Find(".dnet-timeline-opposite").TextContent);
        Assert.Equal("En curso", cut.Find(".dnet-timeline-status").TextContent);
        Assert.Equal("/details", cut.Find(".dnet-timeline-body a").GetAttribute("href"));
        Assert.Empty(cut.FindAll("time"));
    }

    [Fact]
    public void Human_timestamps_can_include_machine_readable_dates_and_remain_optional()
    {
        var cut = Render<DnetTimelineItem>(parameters => parameters
            .Add(component => component.Timestamp, "Today, 09:15")
            .Add(component => component.DateTime, "2026-10-16T09:15:00+02:00"));

        Assert.Equal("2026-10-16T09:15:00+02:00", cut.Find("time").GetAttribute("datetime"));
        Assert.Equal("Today, 09:15", cut.Find("time").TextContent);
        cut.Render(parameters => parameters.Add(component => component.DateTime, (string?)null));
        Assert.Empty(cut.FindAll("time"));
        Assert.Equal("Today, 09:15", cut.Find("span.dnet-timeline-timestamp").TextContent);
        cut.Render(parameters => parameters
            .Add(component => component.DateTime, " ")
            .Add(component => component.Timestamp, "Now"));
        Assert.Empty(cut.FindAll("time"));
        Assert.Equal("Now", cut.Find("span.dnet-timeline-timestamp").TextContent);
        cut.Render(parameters => parameters.Add(component => component.Timestamp, " "));
        Assert.Empty(cut.FindAll("time"));
        Assert.Empty(cut.FindAll(".dnet-timeline-timestamp"));
        Assert.Empty(cut.FindAll(".dnet-timeline-title"));
    }

    [Fact]
    public void Reordered_or_inserted_items_keep_their_explicit_status_without_registration()
    {
        var cut = Render<DnetTimeline>(parameters => parameters.AddChildContent(Items(
            ("Draft", DnetTimelineItemStatus.Completed),
            ("Review", DnetTimelineItemStatus.Current))));

        cut.Render(parameters => parameters.AddChildContent(Items(
            ("Publish", DnetTimelineItemStatus.Pending),
            ("Review", DnetTimelineItemStatus.Current),
            ("Draft", DnetTimelineItemStatus.Completed))));

        Assert.Equal(["Publish", "Review", "Draft"], cut.FindAll(".dnet-timeline-title").Select(element => element.TextContent));
        Assert.Contains("Review", cut.Find("[aria-current=step]").TextContent);
        Assert.Contains("dnet-timeline-item-pending", cut.FindAll("li")[0].ClassList);
        Assert.Contains("dnet-timeline-item-completed", cut.FindAll("li")[2].ClassList);
    }

    [Fact]
    public void Updating_the_status_removes_the_current_step_attribute_and_keeps_content()
    {
        var cut = Render<DnetTimelineItem>(parameters => parameters
            .Add(component => component.Status, DnetTimelineItemStatus.Current)
            .Add(component => component.Title, "Approval")
            .AddChildContent("Still mounted"));

        cut.Render(parameters => parameters.Add(component => component.Status, DnetTimelineItemStatus.Completed));

        Assert.Null(cut.Find("li").GetAttribute("aria-current"));
        Assert.Contains("dnet-timeline-item-completed", cut.Find("li").ClassList);
        Assert.Equal("Completado", cut.Find(".dnet-timeline-status").TextContent);
        Assert.Equal("Still mounted", cut.Find(".dnet-timeline-body").TextContent);
    }

    [Fact]
    public void Plain_text_titles_are_encoded_and_no_accessible_name_is_forced_when_omitted()
    {
        var cut = Render<DnetTimeline>(parameters => parameters
            .Add(component => component.AriaLabel, (string?)null)
            .AddChildContent(Items(("<script>not markup</script>", DnetTimelineItemStatus.Pending))));

        Assert.Null(cut.Find("ol").GetAttribute("aria-label"));
        Assert.Equal("<script>not markup</script>", cut.Find(".dnet-timeline-title").TextContent);
        Assert.Empty(cut.FindAll("script"));
    }

    [Fact]
    public void Unknown_enum_values_are_rejected_instead_of_creating_unstyled_states()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<DnetTimeline>(parameters => parameters
            .Add(component => component.Alignment, (DnetTimelineAlignment)123)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<DnetTimelineItem>(parameters => parameters
            .Add(component => component.Status, (DnetTimelineItemStatus)123)));
    }

    private static RenderFragment Items(params (string Title, DnetTimelineItemStatus Status)[] items) => builder =>
    {
        foreach (var item in items)
        {
            builder.OpenComponent<DnetTimelineItem>(0);
            builder.SetKey(item.Title);
            builder.AddAttribute(1, nameof(DnetTimelineItem.Title), item.Title);
            builder.AddAttribute(2, nameof(DnetTimelineItem.Status), item.Status);
            builder.CloseComponent();
        }
    };
}
