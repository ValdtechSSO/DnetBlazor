using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class SelectOpensAtSelectionTests
{
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
        ?? "http://127.0.0.1:5101").TrimEnd('/');

    // The theme select on the Theming page lists seven themes, more than its
    // panel shows at once, and its value comes from the address.
    [Theory]
    [InlineData("liquid-glass-dark", "Liquid glass dark")]
    [InlineData("light", "Light")]
    public async Task Panel_opens_scrolled_to_the_selected_option(string theme, string label)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1440, Height = 900 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.RouteAsync("https://fonts.googleapis.com/**", request => request.AbortAsync());
        await BrowserReady.OpenAsync(page, $"{BaseUrl}/theming?theme={theme}&lang=en");
        await Assertions.Expect(page.Locator("#theme-studio[data-settings-ready=true]")).ToBeVisibleAsync();

        var trigger = page.Locator("#theme-select .dnet-plain-select");
        await Assertions.Expect(trigger).ToContainTextAsync(label);
        await trigger.ClickAsync();
        var list = page.Locator(".dnet-select-list-items");
        await Assertions.Expect(list).ToBeVisibleAsync();

        // The selected option sits inside the visible part of the list.
        var option = list.Locator(".dnet-select-list-item").Filter(new()
        {
            Has = page.GetByText(label, new() { Exact = true })
        });
        await Assertions.Expect(option).ToHaveCountAsync(1);
        await page.WaitForFunctionAsync(
            "([list, item]) => { const l = list.getBoundingClientRect(), r = item.getBoundingClientRect();"
            + " return r.top >= l.top - 1 && r.bottom <= l.bottom + 1; }",
            new object[] { await list.ElementHandleAsync(), await option.ElementHandleAsync() },
            new() { Timeout = 5_000 });

        // Without a selection further down, the list keeps opening at the top.
        if (theme == "light")
            Assert.Equal(0, await list.EvaluateAsync<int>("element => Math.round(element.scrollTop)"));
        else
            Assert.True(await list.EvaluateAsync<int>("element => Math.round(element.scrollTop)") > 0);

        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }
}
