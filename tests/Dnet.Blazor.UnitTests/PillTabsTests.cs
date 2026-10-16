using Bunit;
using Dnet.Blazor.Components.PillTabs;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class PillTabsTests : BunitContext
{
    private static readonly IReadOnlyList<DnetPillTabItem<string>> Tabs =
    [
        new("pending", "Pendientes", 0),
        new("restricted", "Restringido", 9, Disabled: true),
        new("approved", "Aprobados", 2),
        new("all", "Todos")
    ];

    private static RenderFragment<DnetPillTabItem<string>> Panel => item => builder =>
        builder.AddContent(0, $"Content: {item.Value}");

    [Fact]
    public void Initial_selection_counts_and_panel_relationships_are_accessible()
    {
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.AriaLabel, "Estado")
            .Add(component => component.ChildContent, Panel));

        Assert.Equal("Estado", cut.Find("[role=tablist]").GetAttribute("aria-label"));
        var selected = cut.Find("[aria-selected=true], [aria-pressed=true]");
        Assert.Contains("Pendientes", selected.TextContent);
        Assert.Equal("0", selected.QuerySelector(".dnet-pill-tabs-count")!.TextContent);
        Assert.Equal("0", selected.GetAttribute("tabindex"));
        Assert.Single(cut.FindAll(".dnet-pill-tabs-tab[tabindex='0']"));
        Assert.Null(cut.FindAll(".dnet-pill-tabs-tab")[3].QuerySelector(".dnet-pill-tabs-count"));
        foreach (var tab in cut.FindAll(".dnet-pill-tabs-tab"))
        {
            var panel = cut.Find($"#{tab.GetAttribute("aria-controls")}");
            Assert.Equal(tab.Id, panel.GetAttribute("aria-labelledby"));
            Assert.Equal(tab.GetAttribute("aria-selected") != "true", panel.HasAttribute("hidden"));
            Assert.Equal("button", tab.GetAttribute("type"));
        }
        Assert.Equal("Content: pending", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
    }

    [Fact]
    public void Uncontrolled_clicks_change_content_and_emit_once()
    {
        var changes = new List<string>();
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.ValueChanged, value => changes.Add(value))
            .Add(component => component.ChildContent, Panel));

        cut.FindAll(".dnet-pill-tabs-tab")[2].Click();
        cut.FindAll(".dnet-pill-tabs-tab")[2].Click();

        Assert.Equal(["approved"], changes);
        Assert.Contains("Aprobados", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
        Assert.Equal("Content: approved", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
        Assert.DoesNotContain("Content: pending", cut.Markup);
    }

    [Fact]
    public void Controlled_selection_waits_for_the_parent_to_accept_the_requested_value()
    {
        string? requested = null;
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.Value, "pending")
            .Add(component => component.ValueChanged, value => requested = value)
            .Add(component => component.ChildContent, Panel));

        cut.FindAll(".dnet-pill-tabs-tab")[2].Click();
        Assert.Equal("approved", requested);
        Assert.Contains("Pendientes", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);

        cut.Render(parameters => parameters.Add(component => component.Value, requested!));
        Assert.Contains("Aprobados", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
        Assert.Equal("Content: approved", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
    }

    [Fact]
    public void Reordering_and_updating_counts_preserves_selection_and_removal_selects_an_enabled_replacement()
    {
        var changes = new List<string>();
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.ValueChanged, value => changes.Add(value)));
        cut.FindAll(".dnet-pill-tabs-tab")[2].Click();

        cut.Render(parameters => parameters.Add(component => component.Items,
            new[] { Tabs[3], Tabs[2] with { Count = 5 }, Tabs[0] }));
        Assert.Contains("Aprobados", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
        Assert.Equal("5", cut.Find("[aria-selected=true] .dnet-pill-tabs-count, [aria-pressed=true] .dnet-pill-tabs-count").TextContent);

        cut.Render(parameters => parameters.Add(component => component.Items, new[] { Tabs[1], Tabs[3] }));
        Assert.Contains("Todos", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
        Assert.Equal(["approved"], changes);
    }

    [Fact]
    public void Disabled_items_and_disabled_component_do_not_emit_changes()
    {
        var changes = new List<string>();
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.ValueChanged, value => changes.Add(value)));

        cut.FindAll(".dnet-pill-tabs-tab")[1].Click();
        Assert.Contains("Pendientes", cut.Find("[aria-selected=true], [aria-pressed=true]").TextContent);

        cut.Render(parameters => parameters.Add(component => component.Disabled, true));
        cut.FindAll(".dnet-pill-tabs-tab")[2].Click();
        Assert.All(cut.FindAll(".dnet-pill-tabs-tab"), tab =>
        {
            Assert.True(tab.HasAttribute("disabled"));
            Assert.Equal("-1", tab.GetAttribute("tabindex"));
        });
        Assert.Empty(changes);
    }

    [Fact]
    public void Generic_zero_value_is_distinct_from_an_omitted_value()
    {
        IReadOnlyList<DnetPillTabItem<int>> items = [new(10, "First"), new(0, "Zero")];
        var uncontrolled = Render<DnetPillTabs<int>>(parameters => parameters.Add(component => component.Items, items));
        var controlled = Render<DnetPillTabs<int>>(parameters => parameters
            .Add(component => component.Items, items)
            .Add(component => component.Value, 0));

        Assert.Contains("First", uncontrolled.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
        Assert.Contains("Zero", controlled.Find("[aria-selected=true], [aria-pressed=true]").TextContent);
    }

    [Fact]
    public void Unknown_controlled_value_has_no_selected_panel_and_keeps_one_enabled_tab_stop()
    {
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.Value, "unknown")
            .Add(component => component.ChildContent, Panel));

        Assert.Empty(cut.FindAll("[aria-selected=true], [aria-pressed=true]"));
        Assert.Empty(cut.FindAll("[role=tabpanel]:not([hidden])"));
        Assert.Single(cut.FindAll(".dnet-pill-tabs-tab[tabindex='0']"));
    }

    [Fact]
    public void Empty_and_all_disabled_collections_have_no_tab_stop()
    {
        var cut = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Array.Empty<DnetPillTabItem<string>>()));
        Assert.Empty(cut.FindAll(".dnet-pill-tabs-tab"));

        cut.Render(parameters => parameters.Add(component => component.Items, new[] { Tabs[1] }));
        Assert.Empty(cut.FindAll("[aria-selected=true], [aria-pressed=true]"));
        Assert.Empty(cut.FindAll(".dnet-pill-tabs-tab[tabindex='0']"));
    }

    [Fact]
    public void Duplicate_values_have_an_actionable_error()
    {
        var exception = Assert.Throws<ArgumentException>(() => Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, new[] { Tabs[0], Tabs[0] with { Label = "Duplicate" } })));
        Assert.Contains("unique, non-null", exception.Message);
    }

    [Fact]
    public void Templates_host_attributes_and_instance_ids_are_preserved()
    {
        RenderFragment<DnetPillTabItem<string>> label = item => builder =>
            builder.AddMarkupContent(0, $"<strong>{item.Label}</strong>");
        var first = Render<DnetPillTabs<string>>(parameters => parameters
            .Add(component => component.Items, Tabs)
            .Add(component => component.ItemTemplate, label)
            .Add(component => component.FullWidth, true)
            .AddUnmatched("class", "custom-tabs")
            .AddUnmatched("id", "status-tabs")
            .AddUnmatched("dir", "rtl")
            .AddUnmatched("style", "--dnet-pill-tabs-radius: 8px"));
        var second = Render<DnetPillTabs<string>>(parameters => parameters.Add(component => component.Items, Tabs));

        var root = first.Find(".dnet-pill-tabs");
        Assert.Contains("custom-tabs", root.ClassList);
        Assert.Contains("dnet-pill-tabs-full-width", root.ClassList);
        Assert.Equal("status-tabs", root.Id);
        Assert.Equal("rtl", root.GetAttribute("dir"));
        Assert.Equal("group", first.Find(".dnet-pill-tabs-list").GetAttribute("role"));
        Assert.Equal("true", first.Find(".dnet-pill-tabs-tab").GetAttribute("aria-pressed"));
        Assert.False(first.Find(".dnet-pill-tabs-tab").HasAttribute("aria-controls"));
        Assert.Contains("--dnet-pill-tabs-radius: 8px", root.GetAttribute("style"));
        Assert.Equal("Pendientes", first.Find("strong").TextContent);
        Assert.NotEqual(first.Find(".dnet-pill-tabs-tab").Id, second.Find(".dnet-pill-tabs-tab").Id);
    }
}
