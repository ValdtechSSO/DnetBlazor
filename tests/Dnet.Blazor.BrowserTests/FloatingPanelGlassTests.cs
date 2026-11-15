using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

// In the Liquid Glass themes the floating panel is the same glass as the dialog.
public sealed class FloatingPanelGlassTests
{
    [Theory]
    [InlineData("liquid-glass")]
    [InlineData("liquid-glass-dark")]
    public async Task The_floating_panel_is_the_dialogs_glass(string theme)
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
            ViewportSize = new() { Width = 1440, Height = 900 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        await page.AddInitScriptAsync(
            $"localStorage.setItem('dnet.theme', JSON.stringify({JsonSerializer.Serialize(theme)}));");
        await BrowserLanguage.UseEnglishAsync(page);

        var dialog = await OpenSurfaceAsync(page, $"{baseUrl}/Dialog", "Show Dialog", ".dnet-dialog-container");
        var panel = await OpenSurfaceAsync(page, $"{baseUrl}/FloatingPanel", "Show Panel", ".dnet-floating-panel-container");

        Assert.Equal(dialog.Background, panel.Background);
        Assert.Equal(dialog.Sheen, panel.Sheen);
        Assert.Equal(dialog.Backdrop, panel.Backdrop);
        Assert.NotEqual("none", panel.Backdrop);
        Assert.Empty(errors);
    }

    private static async Task<Surface> OpenSurfaceAsync(IPage page, string url, string trigger, string selector)
    {
        await BrowserReady.OpenAsync(page, url);
        await page.GetByRole(AriaRole.Button, new() { Name = trigger, Exact = true }).First.ClickAsync();
        var surface = page.Locator(selector).Last;
        await Assertions.Expect(surface).ToBeVisibleAsync();
        return await surface.EvaluateAsync<Surface>(
            "element => { const style = getComputedStyle(element); return { background: style.backgroundColor, sheen: style.backgroundImage, backdrop: style.backdropFilter }; }");
    }

    private sealed class Surface
    {
        public string Background { get; set; } = "";
        public string Sheen { get; set; } = "";
        public string Backdrop { get; set; } = "";
    }
}
