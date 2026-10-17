using Bunit;
using Bunit.TestDoubles;
using Dnet.Blazor.Components.FloatingDoubleList;
using Dnet.Blazor.Components.FloatingDoubleList.Infrastructure.Models;
using Dnet.Blazor.Components.List;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class FloatingDoubleListTests : BunitContext
{
    private readonly CapturingOverlayService _overlay = new();
    private readonly Model _model = new();

    public FloatingDoubleListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IOverlayService>(_overlay);
        ComponentFactories.AddStub<DnetFloatingDoubleListPanel<Contact>>();
    }

    [Fact]
    public async Task Opening_passes_custom_rows_search_labels_and_existing_options_to_the_panel()
    {
        RenderFragment<Contact> row = contact => builder => builder.AddContent(0, $"Person: {contact.Name}");
        var search = new SearchControlOptions
        {
            LeftLabel = "Search available", RightLabel = "Search assigned",
            LeftPlaceHolder = "Find a person", RightPlaceHolder = "Find a teammate"
        };
        var available = new List<Contact> { new("Alex Rivera") };
        var assigned = new List<Contact> { new("Taylor Morgan") };
        var leftOptions = new ListOptions<Contact> { HeaderText = "Available" };
        var rightOptions = new ListOptions<Contact> { HeaderText = "Assigned" };
        var cut = RenderField(parameters => parameters
            .Add(component => component.ListItemContent, row)
            .Add(component => component.SearchControlOptions, search)
            .Add(component => component.LeftItems, available)
            .Add(component => component.RightItems, assigned)
            .Add(component => component.LeftListOptions, leftOptions)
            .Add(component => component.RightListOptions, rightOptions));

        await cut.Find("[role=combobox]").ClickAsync(new());
        var panel = Render(_overlay.LastContent!).FindComponent<Stub<DnetFloatingDoubleListPanel<Contact>>>().Instance;

        Assert.Same(row, panel.Parameters.Get(component => component.ListItemContent));
        Assert.Same(search, panel.Parameters.Get(component => component.SearchControlOptions));
        Assert.Same(available, panel.Parameters.Get(component => component.LeftItems));
        Assert.Same(assigned, panel.Parameters.Get(component => component.RightItems));
        Assert.Same(leftOptions, panel.Parameters.Get(component => component.LeftListOptions));
        Assert.Same(rightOptions, panel.Parameters.Get(component => component.RightListOptions));
        Assert.Equal("Person: Alex Rivera", Render(panel.Parameters.Get(component => component.ListItemContent)!(available[0])).Markup);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    [InlineData("ArrowDown")]
    public async Task Opening_keys_attach_one_escape_closable_panel_and_expose_expanded_state(string key)
    {
        var cut = RenderField();
        var trigger = cut.Find("[role=combobox]");
        Assert.Equal("Project team", trigger.GetAttribute("aria-label"));
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Null(trigger.GetAttribute("aria-controls"));
        Assert.Equal("false", trigger.GetAttribute("aria-disabled"));
        Assert.Equal("0", trigger.GetAttribute("tabindex"));

        await trigger.KeyDownAsync(new KeyboardEventArgs { Key = "Tab" });
        Assert.Equal(0, _overlay.AttachCount);
        await trigger.KeyDownAsync(new KeyboardEventArgs { Key = key });
        await trigger.KeyDownAsync(new KeyboardEventArgs { Key = key });
        await trigger.ClickAsync(new());

        Assert.Equal(1, _overlay.AttachCount);
        Assert.Equal("true", cut.Find("[role=combobox]").GetAttribute("aria-expanded"));
        Assert.Equal($"cdk-overlay-pane-{_overlay.LastReference!.GetOverlayReferenceId()}",
            cut.Find("[role=combobox]").GetAttribute("aria-controls"));
        Assert.True(_overlay.LastConfig!.CloseOnEscape);
        Assert.Equal("dialog", _overlay.LastConfig.Role);
        Assert.Equal("Project team", _overlay.LastConfig.AriaLabel);
    }

    [Fact]
    public async Task Disabled_field_rejects_pointer_and_keyboard_opening_until_enabled()
    {
        var cut = RenderField(parameters => parameters.Add(component => component.Disabled, true));
        var trigger = cut.Find("[role=combobox]");
        Assert.Equal("true", trigger.GetAttribute("aria-disabled"));
        Assert.Equal("-1", trigger.GetAttribute("tabindex"));
        await trigger.ClickAsync(new());
        foreach (var key in new[] { "Enter", " ", "ArrowDown" })
            await trigger.KeyDownAsync(new KeyboardEventArgs { Key = key });
        Assert.Equal(0, _overlay.AttachCount);
        Assert.Equal("false", cut.Find("[role=combobox]").GetAttribute("aria-expanded"));

        cut.Render(parameters => parameters.Add(component => component.Disabled, false));
        Assert.Equal("false", cut.Find("[role=combobox]").GetAttribute("aria-disabled"));
        await cut.Find("[role=combobox]").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal(1, _overlay.AttachCount);
    }

    [Fact]
    public async Task External_close_resets_state_returns_focus_and_allows_reopening()
    {
        var cut = RenderField();
        await cut.Find("[role=combobox]").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        var first = _overlay.LastReference!;

        await cut.InvokeAsync(() => first.Detach());

        cut.WaitForAssertion(() => Assert.Equal("false", cut.Find("[role=combobox]").GetAttribute("aria-expanded")));
        Assert.Null(cut.Find("[role=combobox]").GetAttribute("aria-controls"));
        Assert.False(first.IsAttached);
        cut.WaitForAssertion(() => Assert.Single(JSInterop.Invocations["Blazor._internal.domWrapper.focus"]));
        Assert.Equal(true, JSInterop.Invocations["Blazor._internal.domWrapper.focus"][0].Arguments[1]);
        await cut.Find("[role=combobox]").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal(2, _overlay.AttachCount);
        Assert.NotSame(first, _overlay.LastReference);
        Assert.Equal("true", cut.Find("[role=combobox]").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Transfer_notifications_keep_the_existing_callback_and_visible_summary()
    {
        TransferredItems<Contact>? received = null;
        var cut = RenderField(parameters => parameters
            .Add(component => component.SideForSelections, ListToShow.Right)
            .Add(component => component.OnSelectionChanged,
                EventCallback.Factory.Create<TransferredItems<Contact>>(this, transfer => received = transfer)));
        await cut.Find("[role=combobox]").ClickAsync(new());
        var panel = Render(_overlay.LastContent!).FindComponent<Stub<DnetFloatingDoubleListPanel<Contact>>>().Instance;
        var service = panel.Parameters.Get(component => component.FloatingDoubleListService)!;
        var transfer = new TransferredItems<Contact> { RightItems = [new("Alex Rivera")] };

        await cut.InvokeAsync(() => service.UpdateTransferredItems(transfer));

        cut.WaitForAssertion(() => Assert.Same(transfer, received));
        Assert.Contains("Alex Rivera", cut.Find(".dnet-floating-doublelist-value-text").TextContent);
    }

    [Fact]
    public async Task Disposal_detaches_the_panel_without_focusing_a_removed_trigger()
    {
        var cut = RenderField();
        await cut.Find("[role=combobox]").ClickAsync(new());
        var reference = _overlay.LastReference!;

        await DisposeComponentsAsync();

        Assert.False(reference.IsAttached);
        Assert.Empty(JSInterop.Invocations["Blazor._internal.domWrapper.focus"]);
    }

    private IRenderedComponent<DnetFloatingDoubleList<Contact>> RenderField(
        Action<ComponentParameterCollectionBuilder<DnetFloatingDoubleList<Contact>>>? configure = null) =>
        Render<DnetFloatingDoubleList<Contact>>(parameters =>
        {
            parameters
                .Add(component => component.Value, _model.Value)
                .Add(component => component.ValueExpression, () => _model.Value)
                .Add(component => component.DisplayName, "Project team")
                .Add(component => component.DisplayValueConverter, contact => contact.Name);
            configure?.Invoke(parameters);
        });

    private sealed class CapturingOverlayService : OverlayService, IOverlayService
    {
        public RenderFragment? LastContent { get; private set; }
        public OverlayConfig? LastConfig { get; private set; }
        public OverlayReference? LastReference { get; private set; }
        public int AttachCount { get; private set; }

        OverlayReference IOverlayService.Attach(RenderFragment content, OverlayConfig config)
        {
            LastContent = content;
            LastConfig = config;
            AttachCount++;
            return LastReference = base.Attach(content, config);
        }
    }

    private sealed class Model
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed record Contact(string Name);
}
