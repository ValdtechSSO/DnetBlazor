using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class HeaderPreferencesTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Header_uses_circular_connected_panel_triggers_at_65_pixels_and_preserves_preferences(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true, Locale = "es-ES",
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        page.Console += (_, message) =>
        {
            if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal))
                errors.Add(message.Text);
        };
        var initialTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"if (localStorage.getItem('dnet.theme') === null) localStorage.setItem('dnet.theme', JSON.stringify({initialTheme}));");
        await page.RouteAsync("https://fonts.googleapis.com/**", request => request.AbortAsync());
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        await page.GotoAsync(baseUrl + "/Localization");
        await Assertions.Expect(page.Locator("#demo-localization")).ToBeVisibleAsync();
        var language = BrowserHeaderPreferences.Control(page, "language");
        var themeControl = BrowserHeaderPreferences.Control(page, "theme");
        await Assertions.Expect(page.Locator(".dnet-header select")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator(".dnet-header [role=combobox]")).ToHaveCountAsync(0);
        await Assertions.Expect(language).ToHaveAttributeAsync("aria-label", "Idioma de los componentes");
        await Assertions.Expect(themeControl).ToHaveAttributeAsync("aria-label", "Tema de la aplicación");
        await Assertions.Expect(language).ToContainTextAsync("ES");
        await Assertions.Expect(themeControl).ToHaveAttributeAsync("title", theme == "dark" ? "Tema: Oscuro" : "Tema: Claro");
        foreach (var control in new[] { language, themeControl })
        {
            await Assertions.Expect(control).ToHaveCSSAsync("width", "44px");
            await Assertions.Expect(control).ToHaveCSSAsync("height", "44px");
            Assert.True(await control.EvaluateAsync<bool>("element => parseFloat(getComputedStyle(element).borderRadius) >= 22"));
        }
        await CheckHeaderBoundsAsync(page);
        await CaptureAsync(page, theme, width, "closed-es");

        await language.FocusAsync();
        await language.PressAsync("Enter");
        var panel = page.Locator("#header-language-panel");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(panel).ToHaveAttributeAsync("role", "dialog");
        await Assertions.Expect(language).ToHaveAttributeAsync("aria-controls", "header-language-panel");
        await Assertions.Expect(panel.Locator("[data-preference-value='es']")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(panel.Locator("[data-preference-value='en']")).ToHaveAttributeAsync("aria-pressed", "false");
        await CaptureAsync(page, theme, width, "language-open");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(panel).ToHaveCountAsync(0);
        await Assertions.Expect(language).ToContainTextAsync("ES");

        await BrowserHeaderPreferences.SelectAsync(page, "language", "en");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en-US");
        await Assertions.Expect(language).ToHaveAttributeAsync("aria-label", "Component language");
        await Assertions.Expect(themeControl).ToHaveAttributeAsync("aria-label", "Application theme");
        var expectedTheme = theme == "dark" ? "" : "dark";
        var expectedName = theme == "dark" ? "Light" : "Dark";
        await BrowserHeaderPreferences.SelectAsync(page, "theme", expectedTheme == "" ? "light" : expectedTheme);
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", expectedTheme);
        await Assertions.Expect(language).ToContainTextAsync("EN");
        Assert.Equal(expectedTheme, await page.EvaluateAsync<string>("JSON.parse(localStorage.getItem('dnet.theme'))"));
        Assert.Equal("en-US", await page.EvaluateAsync<string>("JSON.parse(localStorage.getItem('dnet.language'))"));
        await CheckHeaderBoundsAsync(page);

        await themeControl.ClickAsync();
        var themePanel = page.Locator("#header-theme-panel");
        await Assertions.Expect(themePanel).ToBeVisibleAsync();
        foreach (var value in new[] { "light", "dark", "compact", "high-contrast", "modern" })
        {
            var option = themePanel.Locator($"button[data-preference-value='{value}']");
            await Assertions.Expect(option).ToBeVisibleAsync();
            await Assertions.Expect(option).ToHaveAttributeAsync("aria-pressed",
                value == (expectedTheme == "" ? "light" : expectedTheme) ? "true" : "false");
        }
        await CaptureAsync(page, theme, width, "theme-open-en");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(themePanel).ToHaveCountAsync(0);

        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("#demo-localization")).ToBeVisibleAsync();
        await Assertions.Expect(language).ToContainTextAsync("EN");
        await Assertions.Expect(themeControl).ToHaveAttributeAsync("title", $"Theme: {expectedName}");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", expectedTheme);
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en-US");
        await CheckHeaderBoundsAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static async Task CheckHeaderBoundsAsync(IPage page)
    {
        var issues = await page.EvaluateAsync<string[]>(
            """
            () => {
                const issues = [];
                const header = document.querySelector('.dnet-header').getBoundingClientRect();
                if (Math.abs(header.height - 65) > 1) issues.push(`Header is ${header.height}px tall`);
                if (document.documentElement.scrollWidth > innerWidth + 1) issues.push('Document overflows horizontally');
                const controls = ['language', 'theme'].map(name => {
                    const element = document.querySelector(`#dnet-${name}-selector > button`);
                    const bounds = element.getBoundingClientRect();
                    if (bounds.top < header.top - 1 || bounds.bottom > header.bottom + 1)
                        issues.push(`${name} field lies outside the header vertically`);
                    if (bounds.left < -1 || bounds.right > innerWidth + 1)
                        issues.push(`${name} field lies outside the viewport`);
                    if (element.scrollWidth > element.clientWidth + 1) issues.push(`${name} field overflows horizontally`);
                    return bounds;
                });
                if (controls[0].right > controls[1].left + 1) issues.push('Preference fields overlap');
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }

    private static async Task CaptureAsync(IPage page, string theme, int width, string state)
    {
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "artifacts", "header-preferences", "browser"));
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new()
        {
            Path = Path.Combine(directory, $"{(theme == "dark" ? "dark" : "light")}-{width}-{state}.png"), FullPage = true
        });
    }
}
