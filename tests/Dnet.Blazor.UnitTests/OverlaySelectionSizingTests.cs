using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Bunit;
using Dnet.Blazor.Components.Autocomplete;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Select;
using Dnet.Blazor.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class OverlaySelectionSizingTests : BunitContext
{
    private readonly CapturingOverlayService _overlay = new();
    private readonly TestViewport _viewport = new();
    private readonly FlexibleConnectedPositionStrategyOrigin _origin = new()
    {
        Left = 40, Top = 40, Width = 248.5, Height = 56, Right = 288.5, Bottom = 96
    };

    public OverlaySelectionSizingTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.Setup<FlexibleConnectedPositionStrategyOrigin>("dnetoverlay.getBoundingClientRect", _ => true).SetResult(_origin);
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddSingleton<IOverlayService>(_overlay);
        Services.AddSingleton<IViewportRuler>(_viewport);
        Services.AddSingleton(new DnetOverlayInterop(JSInterop.JSRuntime));
        Services.AddSingleton<IOverlayPositionScheduler>(new OverlayPositionScheduler());
        Services.AddSingleton<IOverlayInteractionDispatcher>(new OverlayInteractionDispatcher(JSInterop.JSRuntime));
    }

    [Theory]
    [InlineData(false, 1440, "248.5px")]
    [InlineData(true, 1440, "248.5px")]
    [InlineData(false, 1024, "248.5px")]
    [InlineData(true, 390, "95%")]
    public async Task Selection_panels_follow_the_origin_preserve_constraints_and_honor_live_explicit_width(
        bool select, int viewportWidth, string initialWidth)
    {
        PrepareViewport(viewportWidth);
        JSInterop.Setup<long>("dnetoverlay.observeOverlaySize", _ => true).SetResult(42);
        var host = Render<DnetOverlay>();
        var field = RenderSelection(select);
        await OpenAsync(field, select);
        var reference = Assert.IsType<OverlayReference>(_overlay.LastReference);

        host.WaitForAssertion(() =>
        {
            Assert.Equal(initialWidth, Style(host, "width"));
            Assert.Equal("120px", Style(host, "min-width"));
            Assert.Equal("600px", Style(host, "max-width"));
            Assert.Equal(viewportWidth <= 768 ? "65dvh" : "320px", Style(host, "height"));
            Assert.Equal("480px", Style(host, "max-height"));
            Assert.Equal(2, _viewport.SubscriberCount);
        });
        Assert.Null(reference.Config!.Width);

        SetOriginWidth(336.25);
        await host.InvokeAsync(() => _viewport.OnWindowResized(new Size { Width = viewportWidth, Height = 900 }));
        host.WaitForAssertion(() => Assert.Equal(viewportWidth <= 768 ? "95%" : "336.25px", Style(host, "width")));
        Assert.Null(reference.Config.Width);

        await host.InvokeAsync(() => reference.UpdateSize(new OverlaySize
        {
            Width = "420px", MinWidth = "80px", MaxWidth = "500px"
        }));
        host.WaitForAssertion(() =>
        {
            Assert.Equal(viewportWidth <= 768 ? "95%" : "420px", Style(host, "width"));
            Assert.Equal("80px", Style(host, "min-width"));
            Assert.Equal("500px", Style(host, "max-width"));
        });

        // A subsequent origin resize must not replace a consumer's explicit width.
        SetOriginWidth(310);
        await host.InvokeAsync(() => _viewport.OnWindowResized(new Size { Width = viewportWidth, Height = 900 }));
        host.WaitForAssertion(() => Assert.Equal(viewportWidth <= 768 ? "95%" : "420px", Style(host, "width")));

        await host.InvokeAsync(() => reference.Detach());
        host.WaitForAssertion(() =>
        {
            Assert.Empty(host.FindAll("[data-dnet-overlay-id]"));
            Assert.Equal(1, _viewport.SubscriberCount);
            var stopped = Assert.Single(JSInterop.Invocations["dnetoverlay.stopObservingOverlaySize"]);
            Assert.Equal(42L, stopped.Arguments[0]);
        });
    }

    [Fact]
    public async Task Closing_during_observer_registration_releases_the_late_observer_and_viewport_subscription()
    {
        PrepareViewport(1024);
        var observer = JSInterop.Setup<long>("dnetoverlay.observeOverlaySize", _ => true);
        var host = Render<DnetOverlay>();
        var field = RenderSelection(false);
        var opening = OpenAsync(field, false);
        host.WaitForAssertion(() => Assert.Single(JSInterop.Invocations["dnetoverlay.observeOverlaySize"]));

        var reference = Assert.IsType<OverlayReference>(_overlay.LastReference);
        await host.InvokeAsync(() => reference.Detach());
        observer.SetResult(73);
        await opening;

        host.WaitForAssertion(() =>
        {
            Assert.Empty(host.FindAll("[data-dnet-overlay-id]"));
            Assert.Equal(1, _viewport.SubscriberCount);
            var stopped = Assert.Single(JSInterop.Invocations["dnetoverlay.stopObservingOverlaySize"]);
            Assert.Equal(73L, stopped.Arguments[0]);
        });
    }

    private void PrepareViewport(int width)
    {
        _viewport.Width = width;
        JSInterop.Setup<int>("dnetoverlay.getWindowWidth", _ => true).SetResult(width);
    }

    private void SetOriginWidth(double width)
    {
        _origin.Width = width;
        _origin.Right = _origin.Left + width;
    }

    private IRenderedComponent<DnetFormField> RenderSelection(bool select)
    {
        var model = new Contact();
        RenderFragment content = builder =>
        {
            builder.OpenComponent(0, select ? typeof(DnetSelect<Contact, string>) : typeof(DnetAutocomplete<Contact, string>));
            builder.AddAttribute(1, "Value", model.Name);
            builder.AddAttribute(2, "ValueExpression", (Expression<Func<string>>)(() => model.Name));
            builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<string>(this, value => model.Name = value));
            builder.AddAttribute(4, "Items", new List<Contact>());
            builder.AddAttribute(5, "DisplayValueConverter", (Func<Contact, string>)(contact => contact.Name));
            builder.AddAttribute(6, "ValueConverter", (Expression<Func<Contact, string>>)(contact => contact.Name));
            builder.AddAttribute(7, "Width", (string?)null);
            builder.AddAttribute(8, "Height", "320px");
            builder.AddAttribute(9, "MinWidth", "120px");
            builder.AddAttribute(10, "MaxWidth", "600px");
            builder.AddAttribute(11, "MaxHeight", "480px");
            if (!select)
            {
                builder.AddAttribute(12, "FilterBy", CustomFilter<Contact>.FilterBy(contact => contact.Name));
                builder.AddAttribute(13, "SortBy", CustomSort<Contact>.ByAscending<string>(contact => contact.Name));
            }
            builder.CloseComponent();
        };
        return Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, "Contact")
            .Add(component => component.ChildContent, content));
    }

    private static Task OpenAsync(IRenderedComponent<DnetFormField> field, bool select) => select
        ? field.Find(".dnet-plain-select").ClickAsync(new MouseEventArgs())
        : field.Find("input").TriggerEventAsync("onfocusin", new FocusEventArgs());

    private static string Style(IRenderedComponent<DnetOverlay> host, string property)
    {
        var style = host.Find("[data-dnet-overlay-id]").GetAttribute("style") ?? string.Empty;
        var matches = Regex.Matches(style, $@"(?:^|;)\s*{Regex.Escape(property)}\s*:\s*([^;]*)");
        return matches.Count == 0 ? string.Empty : matches[^1].Groups[1].Value.Trim();
    }

    private sealed class CapturingOverlayService : OverlayService, IOverlayService
    {
        public OverlayReference? LastReference { get; private set; }

        OverlayReference IOverlayService.Attach(RenderFragment content, OverlayConfig config) =>
            LastReference = base.Attach(content, config);
    }

    private sealed class Contact
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestViewport : IViewportRuler
    {
        private EventHandler<Size>? _resized;
        public double Width { get; set; } = 1440;
        public double Height { get; set; } = 900;
        public int SubscriberCount => _resized?.GetInvocationList().Length ?? 0;
        public event EventHandler<Size> OnResized
        {
            add => _resized += value;
            remove => _resized -= value;
        }

        public void OnWindowResized(Size size)
        {
            Width = size.Width;
            Height = size.Height;
            _resized?.Invoke(this, size);
        }

        public Task<Size> GetViewportSize() => Task.FromResult(new Size { Width = Width, Height = Height });
        public Task<ViewportScrollPosition> GetViewportScrollPosition() => Task.FromResult(new ViewportScrollPosition());
        public Task<ClientRect> GetViewportRect() => GetViewportRectNoScroll(0);
        public Task<ClientRect> GetViewportRectNoScroll(int margin) => Task.FromResult(new ClientRect
        {
            Left = margin, Top = margin, Right = Width - margin, Bottom = Height - margin,
            Width = Width - 2 * margin, Height = Height - 2 * margin
        });
        public ValueTask<bool> AddWindowEventListeners() => ValueTask.FromResult(true);
        public ValueTask RemoveWindowEventListeners() => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() { _resized = null; return ValueTask.CompletedTask; }
    }
}
