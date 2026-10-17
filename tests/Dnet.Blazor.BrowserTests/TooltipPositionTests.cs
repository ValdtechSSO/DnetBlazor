using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class TooltipPositionTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 1024)]
    [InlineData("dark", 1024)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Text_and_component_hints_stay_near_their_trigger_and_follow_resize(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{baseUrl}/Tooltip");
        await Assertions.Expect(page.Locator("#demo-tooltip")).ToBeVisibleAsync();

        foreach (var variant in new[] { "basic", "delay", "component" })
        {
            var trigger = page.Locator($"#tooltip-{variant}-trigger");
            await trigger.HoverAsync();
            await ExpectAnchoredAsync(page, variant);
            await page.Mouse.MoveAsync(0, 0);
            await Assertions.Expect(page.Locator(".dnet-tooltip")).ToHaveCountAsync(0);
        }

        // Keyboard focus must give the same anchored hint, even when the trigger
        // lives below the first screen. Keep it open while the viewport changes.
        var richTrigger = page.Locator("#tooltip-component-trigger");
        await richTrigger.FocusAsync();
        await ExpectAnchoredAsync(page, "component");
        var resizedWidth = width switch { 1440 => 1280, 1024 => 900, _ => 430 };
        await page.SetViewportSizeAsync(resizedWidth, 1000);
        await richTrigger.ScrollIntoViewIfNeededAsync();
        await ExpectAnchoredAsync(page, "component");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator(".dnet-tooltip")).ToHaveCountAsync(0);
        await Assertions.Expect(richTrigger).ToBeFocusedAsync();
        Assert.Empty(errors);
    }

    private static async Task ExpectAnchoredAsync(IPage page, string variant)
    {
        await Assertions.Expect(page.Locator(".dnet-tooltip")).ToBeVisibleAsync();
        await page.WaitForFunctionAsync(
            """
            variant => {
                const trigger = document.querySelector(`#tooltip-${variant}-trigger`);
                const tooltip = document.querySelector('.dnet-tooltip');
                if (!trigger || !tooltip) return false;
                const origin = trigger.getBoundingClientRect(), hint = tooltip.getBoundingClientRect();
                const gapX = Math.max(0, origin.left - hint.right, hint.left - origin.right);
                const gapY = Math.max(0, origin.top - hint.bottom, hint.top - origin.bottom);
                return hint.width > 0 && hint.height > 0 && gapX <= 17 && gapY <= 17
                    && hint.left >= -1 && hint.right <= innerWidth + 1
                    && hint.top >= -1 && hint.bottom <= innerHeight + 1;
            }
            """, variant, new() { Timeout = 5000 });
    }
}
