using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class PopupStableSizingTests
{
    private const string AutocompletePanel = ".dnet-autocomplete-list-wrapper";
    private const string AutocompleteRows = ".dnet-autocomplete-list-item";
    private const string SelectPanel = ".dnet-select-list-wrapper";
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
        ?? "http://127.0.0.1:5101").TrimEnd('/');

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 1024)]
    [InlineData("dark", 1024)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Autocomplete_and_select_keep_panel_dimensions_when_virtual_rows_and_results_change(
        string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await NewPageAsync(browser, width, theme);
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        var fixture = await RouteContactsAsync(page);
        using var people = JsonDocument.Parse(fixture);
        var firstName = people.RootElement[0].GetProperty("name").GetString()!;

        foreach (var demo in new[] { "basic", "rich", "decorated" })
        {
            await page.GotoAsync($"{BaseUrl}/Autocomplete");
            var input = page.Locator($"#autocomplete-{demo}-input");
            await Assertions.Expect(input).ToBeVisibleAsync();
            await input.FocusAsync();
            var panel = page.Locator(AutocompletePanel);
            await Assertions.Expect(panel).ToBeVisibleAsync();
            await Assertions.Expect(panel.Locator(AutocompleteRows).First).ToBeVisibleAsync();
            await SettleAsync(page);
            var opening = await MeasureAsync(panel);
            var label = $"{demo}, {width}px, {theme}";
            AssertFitsViewport(opening, width, label);
            if (width > 550)
                Near(await ControlWidthAsync(input), opening.Width, $"Default panel follows its origin ({label})");

            foreach (var fraction in new[] { 0.25, 0.5, 1, 0 })
            {
                await ScrollListAsync(page, panel, fraction);
                Stable(opening, await MeasureAsync(panel), $"Virtual scroll {fraction} ({label})");
            }

            await FilterAsync(page, input, "Lorena");
            await Assertions.Expect(panel.Locator(AutocompleteRows)).ToHaveCountAsync(1);
            await Assertions.Expect(panel.Locator(AutocompleteRows).First).ToContainTextAsync("Lorena Jensen");
            await SettleAsync(page);
            Stable(opening, await MeasureAsync(panel), $"One result ({label})");

            await FilterAsync(page, input, "__popup_no_matching_contact__");
            await Assertions.Expect(panel.Locator(AutocompleteRows)).ToHaveCountAsync(0);
            await SettleAsync(page);
            Stable(opening, await MeasureAsync(panel), $"No results ({label})");

            await FilterAsync(page, input, "a");
            await page.WaitForFunctionAsync(
                "() => document.querySelectorAll('.dnet-autocomplete-list-item').length > 5");
            await SettleAsync(page);
            Stable(opening, await MeasureAsync(panel), $"Many results ({label})");

            await FilterAsync(page, input, "");
            // Wait for the original unfiltered window, rather than accepting the previous nonempty list.
            await Assertions.Expect(panel.Locator(AutocompleteRows).First).ToContainTextAsync(firstName);
            await SettleAsync(page);
            Stable(opening, await MeasureAsync(panel), $"Restored results ({label})");
            await CloseAsync(page, panel);
        }

        await page.GotoAsync($"{BaseUrl}/Select");
        for (var index = 0; index < 3; index++)
        {
            var trigger = page.Locator(".dnet-desktop-content .dnet-plain-select").Nth(index);
            await CheckSelectAsync(page, trigger, width, $"Select example {index}");
        }
        await page.GotoAsync($"{BaseUrl}/Forms");
        await CheckSelectAsync(page, page.Locator(".dnet-desktop-content .dnet-plain-select").First, width, "Forms gender");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(1440, 1280)]
    [InlineData(1024, 900)]
    [InlineData(390, 430)]
    public async Task Open_autocomplete_follows_origin_and_viewport_resize_then_remains_stable(
        int initialWidth, int resizedWidth)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await NewPageAsync(browser, initialWidth, "dark");
        await RouteContactsAsync(page);
        await page.GotoAsync($"{BaseUrl}/Autocomplete");
        var input = page.Locator("#autocomplete-basic-input");
        await Assertions.Expect(input).ToBeVisibleAsync();
        await input.FocusAsync();
        var panel = page.Locator(AutocompletePanel);
        await Assertions.Expect(panel.Locator(AutocompleteRows).First).ToBeVisibleAsync();
        await SettleAsync(page);
        var original = await MeasureAsync(panel);

        await page.SetViewportSizeAsync(resizedWidth, 1000);
        await page.WaitForFunctionAsync(
            """
            ({ previousWidth, mobile }) => {
                const input = document.getElementById('autocomplete-basic-input');
                const panel = document.querySelector('.dnet-autocomplete-list-wrapper');
                const control = input.closest('.dnet-form-field-plain-control-container');
                const actual = panel.getBoundingClientRect().width;
                return Math.abs(actual - previousWidth) > 1
                    && (mobile || Math.abs(actual - control.getBoundingClientRect().width) <= 1);
            }
            """, new { previousWidth = original.Width, mobile = resizedWidth <= 550 });
        await SettleAsync(page);
        var resized = await MeasureAsync(panel);
        AssertFitsViewport(resized, resizedWidth, "Resized open panel");
        Assert.True(Math.Abs(original.Width - resized.Width) > 1, "The changed origin/viewport must exercise a width update.");
        Near(original.Height, resized.Height, "Width-only viewport resize preserves the configured panel height");
        if (resizedWidth > 550)
            Near(await ControlWidthAsync(input), resized.Width, "Resized panel follows its resized origin");
        await ScrollListAsync(page, panel, 1);
        Stable(resized, await MeasureAsync(panel), "Virtual scroll after viewport resize");
        await FilterAsync(page, input, "Lorena");
        await Assertions.Expect(panel.Locator(AutocompleteRows)).ToHaveCountAsync(1);
        await SettleAsync(page);
        Stable(resized, await MeasureAsync(panel), "Filtered result after viewport resize");
    }

    private static async Task<IPage> NewPageAsync(IBrowser browser, int width, string theme)
    {
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        var serialized = JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serialized}));");
        return page;
    }

    private static async Task<string> RouteContactsAsync(IPage page)
    {
        var people = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = people
        }));
        return people;
    }

    private static async Task CheckSelectAsync(IPage page, ILocator trigger, int width, string label)
    {
        await Assertions.Expect(trigger).ToBeVisibleAsync();
        await trigger.ClickAsync();
        var panel = page.Locator(SelectPanel);
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(panel.Locator(".dnet-select-list-item").First).ToBeVisibleAsync();
        await SettleAsync(page);
        var opening = await MeasureAsync(panel);
        AssertFitsViewport(opening, width, label);
        foreach (var fraction in new[] { 1, 0 })
        {
            await ScrollListAsync(page, panel, fraction);
            Stable(opening, await MeasureAsync(panel), $"Select scroll {fraction} ({label})");
        }
        for (var choice = 0; choice < 2; choice++)
        {
            var rows = panel.Locator(".dnet-select-list-item");
            await (choice == 0 ? rows.First : rows.Last).ClickAsync();
            await Assertions.Expect(panel).ToBeHiddenAsync();
            await trigger.ClickAsync();
            await Assertions.Expect(panel).ToBeVisibleAsync();
            await SettleAsync(page);
            Stable(opening, await MeasureAsync(panel), $"Select reopened after choice {choice} ({label})");
        }
        await CloseAsync(page, panel);
    }

    private static async Task FilterAsync(IPage page, ILocator input, string value)
    {
        await input.FillAsync(value);
        // Autocomplete filters on debounced keyup; FillAsync alone intentionally does not trigger it.
        await input.PressAsync("End");
    }

    private static async Task ScrollListAsync(IPage page, ILocator panel, double fraction)
    {
        await panel.Locator(".dnet-autocomplete-list-items, .dnet-select-list-items").EvaluateAsync(
            "(element, fraction) => { element.scrollTop = (element.scrollHeight - element.clientHeight) * fraction; element.dispatchEvent(new Event('scroll')); }",
            fraction);
        await page.WaitForTimeoutAsync(250);
        await SettleAsync(page);
    }

    private static Task SettleAsync(IPage page) => page.EvaluateAsync(
        "() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");

    private static async Task CloseAsync(IPage page, ILocator panel)
    {
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    private static Task<double> ControlWidthAsync(ILocator input) => input.EvaluateAsync<double>(
        "element => element.closest('.dnet-form-field-plain-control-container').getBoundingClientRect().width");

    private static Task<PopupSize> MeasureAsync(ILocator panel) => panel.EvaluateAsync<PopupSize>(
        """
        element => {
            const rect = element.getBoundingClientRect();
            const pane = element.closest('.cdk-overlay-pane').getBoundingClientRect();
            return { width: rect.width, height: rect.height, left: rect.left, right: rect.right,
                paneWidth: pane.width, paneHeight: pane.height };
        }
        """);

    private static void Stable(PopupSize expected, PopupSize actual, string label)
    {
        Near(expected.Width, actual.Width, $"Panel width: {label}");
        Near(expected.Height, actual.Height, $"Panel height: {label}");
        Near(expected.PaneWidth, actual.PaneWidth, $"Overlay pane width: {label}");
        Near(expected.PaneHeight, actual.PaneHeight, $"Overlay pane height: {label}");
    }

    private static void AssertFitsViewport(PopupSize size, int width, string label)
    {
        Assert.InRange(size.Width, 1, width);
        Assert.True(size.Height > 0, $"Panel height must be positive: {label}.");
        Assert.InRange(size.Left, -1, width);
        Assert.InRange(size.Right, 1, width + 1);
    }

    private static void Near(double expected, double actual, string label) =>
        Assert.True(Math.Abs(expected - actual) <= 1, $"{label}: expected {expected}px, got {actual}px.");

    private sealed class PopupSize
    {
        public double Width { get; init; }
        public double Height { get; init; }
        public double Left { get; init; }
        public double Right { get; init; }
        public double PaneWidth { get; init; }
        public double PaneHeight { get; init; }
    }
}
