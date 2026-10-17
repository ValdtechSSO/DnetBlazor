using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ThemingPlaygroundTests
{
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
        ?? "http://127.0.0.1:5101").TrimEnd('/');

    [Fact]
    public async Task Playground_applies_customizations_to_the_live_preview_and_exports_the_applied_values()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        await OpenAsync(page, "dark");

        await page.Locator("#theme-primary-color").FillAsync("#0f6cbd");
        await SetDensityAsync(page, 6);
        await ExpectInlinePropertyAsync(page, "--dnet-sys-primary", "#0f6cbd");
        await ExpectInlinePropertyAsync(page, "--dnet-sys-space-unit", "6px");
        await Assertions.Expect(page.Locator("#theme-preview-primary"))
            .ToHaveCSSAsync("background-color", "rgb(15, 108, 189)");
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync("--dnet-sys-primary: #0f6cbd;");
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync("--dnet-sys-space-unit: 6px;");

        foreach (var (id, radius) in new[] { ("square", 0), ("soft", 6), ("pill", 999) })
        {
            var option = page.Locator($"#theme-radius-{id}");
            await option.ClickAsync();
            await Assertions.Expect(option).ToHaveAttributeAsync("aria-pressed", "true");
            await ExpectRadiusAsync(page, radius);
            await Assertions.Expect(page.Locator("#theme-css-output"))
                .ToContainTextAsync($"--dnet-btn-radius: {radius}px;");
        }

        // Explicit overrides in the inheritance examples retain their own colors.
        var instanceButton = page.GetByRole(AriaRole.Button, new() { Name = "Instance override", Exact = true });
        Assert.Equal("rgb(166, 54, 54)", await instanceButton.EvaluateAsync<string>(
            "element => getComputedStyle(element).backgroundColor"));

        // Exercise the actual export action without requiring access to the OS clipboard.
        await page.EvaluateAsync("() => { window.__copiedThemeCss = null; window.dnetinterop.copyText = async css => { window.__copiedThemeCss = css; }; }");
        await page.Locator("#theme-copy").ClickAsync();
        await Assertions.Expect(page.Locator("#theme-copy-status"))
            .ToContainTextAsync(new Regex("copied", RegexOptions.IgnoreCase));
        var copied = await page.EvaluateAsync<string>("() => window.__copiedThemeCss");
        Assert.Equal((await page.Locator("#theme-css-output").InnerTextAsync()).Trim(), copied.Trim());
        Assert.Contains("[data-dnet-theme=\"dark\"]", copied);
        Assert.Contains("--dnet-btn-radius: 999px;", copied);
    }

    [Fact]
    public async Task Presets_reset_to_their_real_defaults_and_theme_selection_stays_synchronized_and_persisted()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        await OpenAsync(page, "modern");

        await Assertions.Expect(page.Locator("#theme-primary-color")).ToHaveValueAsync("#5b5ce0");
        await Assertions.Expect(page.Locator("#theme-density")).ToHaveValueAsync("5");
        await ExpectRadiusAsync(page, 9999);
        await Assertions.Expect(page.Locator("#theme-radius-pill")).ToHaveAttributeAsync("aria-pressed", "true");

        await page.EvaluateAsync("() => document.documentElement.style.setProperty('--dnet-btn-height', '44px')");
        await page.Locator("#theme-primary-color").FillAsync("#0f6cbd");
        await SetDensityAsync(page, 6);
        await page.Locator("#theme-radius-soft").ClickAsync();
        await ExpectRadiusAsync(page, 6);
        await page.Locator("#theme-reset").ClickAsync();

        foreach (var property in new[] { "--dnet-sys-primary", "--dnet-sys-space-unit", "--dnet-btn-radius" })
            await ExpectInlinePropertyAsync(page, property, string.Empty);
        await ExpectInlinePropertyAsync(page, "--dnet-btn-height", "44px");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", "modern");
        await Assertions.Expect(page.Locator("#theme-select")).ToHaveValueAsync("modern");
        Assert.Equal("modern", await StoredThemeAsync(page));
        await Assertions.Expect(page.Locator("#theme-primary-color")).ToHaveValueAsync("#5b5ce0");
        await Assertions.Expect(page.Locator("#theme-density")).ToHaveValueAsync("5");
        await ExpectRadiusAsync(page, 9999);
        await Assertions.Expect(page.Locator("#theme-radius-pill")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync("--dnet-btn-radius: 9999px;");

        // Theme changes made through either visible selector update the other one.
        var headerTheme = BrowserHeaderPreferences.Control(page, "theme");
        await BrowserHeaderPreferences.SelectAsync(page, "theme", "compact");
        await Assertions.Expect(page.Locator("#theme-select")).ToHaveValueAsync("compact");
        await Assertions.Expect(page.Locator("#theme-density")).ToHaveValueAsync("3");
        await page.Locator("#theme-select").SelectOptionAsync("");
        await Assertions.Expect(headerTheme).ToHaveAttributeAsync("title", "Theme: Light");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", "");
        Assert.Equal("", await StoredThemeAsync(page));

        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("#theme-select")).ToHaveValueAsync("");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", "");
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync(":root {");
    }

    [Fact]
    public async Task Clipboard_failure_reports_a_status_and_keeps_the_css_available_for_manual_copy()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await OpenAsync(page, "dark");
        await page.EvaluateAsync("() => { window.dnetinterop.copyText = async () => { throw new Error('Clipboard blocked for this test'); }; }");

        await page.Locator("#theme-copy").ClickAsync();
        var status = page.Locator("#theme-copy-status");
        await Assertions.Expect(status).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(status)
            .ToContainTextAsync(new Regex("clipboard|manually|copy.*below", RegexOptions.IgnoreCase));
        await Assertions.Expect(page.Locator("#theme-css-output")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync("--dnet-sys-primary:");
        await Assertions.Expect(page.Locator("#theme-copy")).ToBeEnabledAsync();
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Studio_and_its_controls_fit_a_390_pixel_viewport()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 390, Height = 844 }
        });
        await OpenAsync(page, "dark");

        var studio = page.Locator("#theme-studio");
        var dimensions = await studio.EvaluateAsync<StudioDimensions>(
            "element => { const bounds = element.getBoundingClientRect(); return { left: bounds.left, right: bounds.right, clientWidth: element.clientWidth, scrollWidth: element.scrollWidth }; }");
        Assert.InRange(dimensions.Left, -1, 390);
        Assert.InRange(dimensions.Right, 1, 391);
        Assert.True(dimensions.ScrollWidth <= dimensions.ClientWidth + 1,
            $"Studio content overflows horizontally: {dimensions.ScrollWidth}px inside {dimensions.ClientWidth}px.");

        foreach (var id in new[]
        {
            "theme-select", "theme-primary-color", "theme-density", "theme-reset", "theme-copy",
            "theme-preview-primary", "theme-preview-secondary", "theme-css-output"
        })
        {
            var control = page.Locator($"#{id}");
            await control.ScrollIntoViewIfNeededAsync();
            var bounds = await control.BoundingBoxAsync();
            Assert.NotNull(bounds);
            Assert.True(bounds.X >= dimensions.Left - 1 && bounds.X + bounds.Width <= dimensions.Right + 1,
                $"{id} extends outside the studio at 390px.");
        }
        await page.Locator("#theme-radius-pill").ClickAsync();
        await ExpectRadiusAsync(page, 999);
    }

    private static async Task OpenAsync(IPage page, string initialTheme)
    {
        var theme = System.Text.Json.JsonSerializer.Serialize(initialTheme);
        await page.AddInitScriptAsync($"if (localStorage.getItem('dnet.theme') === null) localStorage.setItem('dnet.theme', JSON.stringify({theme}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await page.GotoAsync($"{BaseUrl}/theming");
        await Assertions.Expect(page.Locator("#theme-studio")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#theme-select")).ToHaveValueAsync(initialTheme);
        await Assertions.Expect(page.Locator("#theme-css-output")).ToContainTextAsync("--dnet-sys-primary:");
    }

    private static async Task SetDensityAsync(IPage page, int value)
    {
        var slider = page.Locator("#theme-density");
        await slider.FocusAsync();
        await slider.PressAsync("Home");
        for (var current = 2; current < value; current++) await slider.PressAsync("ArrowRight");
        await Assertions.Expect(slider).ToHaveValueAsync(value.ToString());
    }

    private static Task ExpectInlinePropertyAsync(IPage page, string name, string value) => page.WaitForFunctionAsync(
        "({ name, value }) => document.documentElement.style.getPropertyValue(name).trim() === value", new { name, value });

    private static Task ExpectRadiusAsync(IPage page, int radius) => page.WaitForFunctionAsync(
        "radius => getComputedStyle(document.getElementById('theme-preview-primary')).borderTopLeftRadius === radius + 'px'", radius);

    private static Task<string> StoredThemeAsync(IPage page) =>
        page.EvaluateAsync<string>("() => JSON.parse(localStorage.getItem('dnet.theme'))");

    private sealed class StudioDimensions
    {
        public double Left { get; set; }
        public double Right { get; set; }
        public double ClientWidth { get; set; }
        public double ScrollWidth { get; set; }
    }
}
