using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class PopupSurfaceTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Opened_panels_share_the_time_picker_surface_and_keep_public_overrides(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        var pageErrors = new List<string>();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        page.PageError += (_, error) => pageErrors.Add(error);
        var persons = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = persons
        }));

        await NavigateAsync(page, baseUrl, "/TimePicker", "button[aria-label='Open time picker']", theme);
        await page.Locator("button[aria-label='Open time picker']").First.ClickAsync();
        var referencePanel = page.Locator(".dnet-timepicker-panel");
        var referenceFrame = page.Locator(".dnet-timepicker-overlay");
        await Assertions.Expect(referencePanel).ToBeVisibleAsync();
        var background = await referencePanel.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        var radius = await referenceFrame.EvaluateAsync<string>("element => getComputedStyle(element).borderRadius");
        var shadow = await referenceFrame.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");
        Assert.NotEqual("none", shadow);
        Assert.NotEqual("0px", radius);
        await CloseWithEscapeAsync(page, referencePanel);

        Popup[] popups =
        [
            new("Select", "/Select", ".dnet-desktop-content .dnet-plain-select", ".dnet-select-list-wrapper", "--dnet-list-background-color"),
            new("Autocomplete", "/Autocomplete", "input[role=combobox]", ".dnet-autocomplete-list-wrapper", Focus: true),
            new("DatePicker", "/DatePicker", ".dnet-form-field-plain-suffix .dnet-datepicker-icon-wrapper", ".dnet-datepicker-content", "--dnet-calendar-background"),
            new("Dialog", "/Dialog", "button:has-text('Show Dialog')", ".dnet-dialog-container", "--dnet-dialog-background"),
            new("ConnectedPanel", "/ConnectedPanel", "svg.bi-filter", ".dnet-connected-panel-container"),
            new("FloatingPanel", "/FloatingPanel", "button:has-text('Show Panel')", ".dnet-floating-panel-container"),
            // These legacy panels leave OverlayConfig.CloseOnEscape disabled and expose their own close actions.
            new("FloatingDoubleList", "/FloatingDoubleList", "#floating-doublelist-field .dnet-plain-select", ".dnet-connected-panel-container-dl", CloseTrigger: ".dnet-dialog-icon-wrapper"),
            new("Grid advanced filter", "/BlGrid", ".blg-adv-toggle-button-image", ".blg-advanced-filter-container", Grid: true, CloseTrigger: ".blg-adv-filter-buttom-reset")
        ];

        foreach (var popup in popups)
        {
            await NavigateAsync(page, baseUrl, popup.Route, popup.Trigger, theme);
            var trigger = page.Locator(popup.Trigger).First;
            if (popup.Grid)
            {
                await page.WaitForFunctionAsync("() => document.querySelector('.blg-center-cols-container [data-blg-row-id]') !== null");
                await BringGridTriggerIntoViewAsync(trigger);
            }
            if (popup.Focus) await trigger.FocusAsync();
            else await trigger.ClickAsync();

            var panel = page.Locator(popup.Panel).Last;
            await Assertions.Expect(panel).ToBeVisibleAsync();
            var actual = await panel.EvaluateAsync<Surface>(
                "element => { const style = getComputedStyle(element); return { background: style.backgroundColor, radius: style.borderRadius, shadow: style.boxShadow, readable: style.color !== style.backgroundColor && style.color !== 'transparent' && style.color !== 'rgba(0, 0, 0, 0)', border: style.borderStyle === 'none' || parseFloat(style.borderWidth) === 0 || style.borderColor === style.backgroundColor || style.borderColor === 'transparent' || style.borderColor === 'rgba(0, 0, 0, 0)' }; }");
            Assert.True(actual.Background == background, $"{popup.Name} background: expected {background}, got {actual.Background}.");
            Assert.True(actual.Radius == radius, $"{popup.Name} radius: expected {radius}, got {actual.Radius}.");
            Assert.True(actual.Shadow == shadow, $"{popup.Name} shadow: expected {shadow}, got {actual.Shadow}.");
            Assert.True(actual.Readable, $"{popup.Name} must retain visible foreground text.");
            Assert.True(actual.Border, $"{popup.Name} must not introduce a contrasting panel outline.");
            if (popup.Name == "DatePicker")
                await Assertions.Expect(panel.Locator(".dnet-calendar")).ToHaveCSSAsync("background-color", background);

            if (popup.BackgroundToken is not null)
                await CheckBackgroundOverrideAsync(page, panel, popup.BackgroundToken, background);
            if (popup.CloseTrigger is null)
                await CloseWithEscapeAsync(page, panel);
            else
            {
                await panel.Locator(popup.CloseTrigger).ClickAsync();
                await Assertions.Expect(panel).ToBeHiddenAsync();
            }
        }

        Assert.Empty(pageErrors);
    }

    private static async Task NavigateAsync(IPage page, string baseUrl, string route, string trigger, string theme)
    {
        await BrowserLanguage.UseEnglishAsync(page);
        await page.GotoAsync(baseUrl + route);
        await Assertions.Expect(page.Locator(trigger).First).ToBeVisibleAsync();
        await page.EvaluateAsync("theme => document.documentElement.dataset.dnetTheme = theme", theme);
    }

    private static async Task CloseWithEscapeAsync(IPage page, ILocator panel)
    {
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    private static async Task CheckBackgroundOverrideAsync(IPage page, ILocator panel, string token, string original)
    {
        const string rootColor = "rgb(45, 65, 85)";
        const string hostColor = "rgb(70, 35, 95)";
        await page.EvaluateAsync("({ token, color }) => document.documentElement.style.setProperty(token, color)", new { token, color = rootColor });
        await Assertions.Expect(panel).ToHaveCSSAsync("background-color", rootColor);
        // PanelStyle is applied to the overlay host; inherited overrides there must beat :root.
        await panel.EvaluateAsync("(element, { token, color }) => element.closest('[data-dnet-overlay-id]').style.setProperty(token, color)", new { token, color = hostColor });
        await Assertions.Expect(panel).ToHaveCSSAsync("background-color", hostColor);
        if (token == "--dnet-calendar-background")
            await Assertions.Expect(panel.Locator(".dnet-calendar")).ToHaveCSSAsync("background-color", hostColor);
        await panel.EvaluateAsync("(element, token) => element.closest('[data-dnet-overlay-id]').style.removeProperty(token)", token);
        await page.EvaluateAsync("token => document.documentElement.style.removeProperty(token)", token);
        await Assertions.Expect(panel).ToHaveCSSAsync("background-color", original);
    }

    private static Task BringGridTriggerIntoViewAsync(ILocator trigger) => trigger.EvaluateAsync(
        """
        async element => {
            const root = element.closest('.blg-root');
            const viewport = root.querySelector('.blg-header-viewport');
            if (getComputedStyle(root).overflowX === 'auto') {
                root.scrollLeft += element.getBoundingClientRect().left - root.getBoundingClientRect().left - 24;
            } else {
                const scrollbar = root.parentElement.querySelector('.blg-body-horizontal-scroll-viewport');
                scrollbar.scrollLeft += element.getBoundingClientRect().left - viewport.getBoundingClientRect().left - 24;
                scrollbar.dispatchEvent(new Event('scroll'));
            }
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        }
        """);

    private sealed record Popup(string Name, string Route, string Trigger, string Panel,
        string? BackgroundToken = null, bool Focus = false, bool Grid = false, string? CloseTrigger = null);

    private sealed class Surface
    {
        public string Background { get; set; } = "";
        public string Radius { get; set; } = "";
        public string Shadow { get; set; } = "";
        public bool Readable { get; set; }
        public bool Border { get; set; }
    }
}
