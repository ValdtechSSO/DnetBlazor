using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class OverlayBehaviorTests
{
    [Fact]
    public async Task Dialog_uses_modal_semantics_focus_management_and_escape_cleanup()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "https://127.0.0.1:5101";
        var consoleErrors = new List<string>();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                consoleErrors.Add(message.Text);
            }
        };

        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/Dialog", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await page.GetByRole(AriaRole.Button, new() { Name = "Show Dialog", Exact = true }).ClickAsync();

        var dialog = page.Locator("[data-dnet-overlay-id][role=dialog]").Last;
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        Assert.Equal("true", await dialog.GetAttributeAsync("aria-modal"));
        Assert.True(await page.EvaluateAsync<bool>(
            "() => !!document.activeElement?.closest('[data-dnet-overlay-id]')"));

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(dialog).ToBeHiddenAsync();
        Assert.DoesNotContain(consoleErrors, error => error.Contains("DotNetObjectReference", StringComparison.Ordinal));
    }
}
