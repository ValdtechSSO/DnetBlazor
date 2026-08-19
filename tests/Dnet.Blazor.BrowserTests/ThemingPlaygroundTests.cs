using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ThemingPlaygroundTests
{
    [Fact]
    public async Task Playground_applies_global_theme_and_instance_override()
    {
        if (!BrowserTestsEnabled())
        {
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "https://127.0.0.1:5101";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/theming", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.Locator("select").SelectOptionAsync("dark");
        Assert.Equal("dark", await page.Locator("html").GetAttributeAsync("data-dnet-theme"));

        await page.Locator("input[type=color]").FillAsync("#0f6cbd");
        await page.Locator("input[type=number]").FillAsync("6");
        await page.Locator("input[type=number]").PressAsync("Tab");
        Assert.Equal("#0f6cbd", await page.EvaluateAsync<string>(
            "() => document.documentElement.style.getPropertyValue('--dnet-sys-primary')"));
        Assert.Equal("6px", await page.EvaluateAsync<string>(
            "() => document.documentElement.style.getPropertyValue('--dnet-sys-space-unit')"));

        var instanceButton = page.GetByRole(AriaRole.Button, new()
        {
            Name = "Instance override",
            Exact = true
        });
        await instanceButton.WaitForAsync();
        Assert.Equal("rgb(166, 54, 54)", await instanceButton.EvaluateAsync<string>(
            "element => getComputedStyle(element).backgroundColor"));
    }

    private static bool BrowserTestsEnabled() => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true",
        StringComparison.OrdinalIgnoreCase);
}