using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class SkeletonTests
{
    private static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Fact]
    public async Task Loading_hides_interactive_content_and_preserves_input_and_child_state()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Skeleton");
        var root = page.Locator("#skeleton-wrapped");
        var content = root.Locator(".dnet-skeleton-content");
        var input = page.Locator("#skeleton-state-input");
        var toggle = page.Locator("#skeleton-loading-toggle");
        await Assertions.Expect(root).ToHaveAttributeAsync("aria-busy", "true");
        Assert.True(await content.EvaluateAsync<bool>("element => element.inert"));
        await Assertions.Expect(content).ToHaveAttributeAsync("aria-hidden", "true");
        await Assertions.Expect(input).ToBeHiddenAsync();
        await input.EvaluateAsync("element => element.focus()");
        Assert.False(await input.EvaluateAsync<bool>("element => element === document.activeElement"));
        await toggle.FocusAsync();
        await toggle.PressAsync("Enter");
        await Assertions.Expect(root).ToHaveAttributeAsync("aria-busy", "false");
        Assert.False(await content.EvaluateAsync<bool>("element => element.inert"));
        await Assertions.Expect(input).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#skeleton-circle-wrapped")).ToHaveCSSAsync("width", "64px");
        await Assertions.Expect(page.Locator("#skeleton-circle-wrapped")).ToHaveCSSAsync("height", "64px");
        await input.FillAsync("Una idea que sigue aquí");
        var original = await input.ElementHandleAsync();
        await page.Locator("#skeleton-content-action").ClickAsync();
        await Assertions.Expect(page.Locator("#skeleton-action-count")).ToContainTextAsync("1");
        await toggle.ClickAsync();
        await Assertions.Expect(input).ToBeHiddenAsync();
        await toggle.ClickAsync();
        await Assertions.Expect(input).ToHaveValueAsync("Una idea que sigue aquí");
        Assert.True(await original!.EvaluateAsync<bool>("element => element === document.querySelector('#skeleton-state-input')"));
        await Assertions.Expect(page.Locator("#skeleton-action-count")).ToContainTextAsync("1");
        await toggle.FocusAsync();
        await toggle.PressAsync("Tab");
        await Assertions.Expect(input).ToBeFocusedAsync();
    }

    [Fact]
    public async Task Shapes_motion_and_inherited_tokens_work_in_dark_and_mobile_layouts()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } });
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Skeleton");
        var circle = page.Locator("#skeleton-circle");
        await Assertions.Expect(circle).ToHaveCSSAsync("width", "64px");
        await Assertions.Expect(circle).ToHaveCSSAsync("height", "64px");
        await Assertions.Expect(circle).ToHaveCSSAsync("border-radius", "50%");
        await Assertions.Expect(circle).ToHaveAttributeAsync("aria-hidden", "true");
        await Assertions.Expect(page.Locator("#skeleton-circle-wrapped")).ToHaveCSSAsync("width", "64px");
        await Assertions.Expect(page.Locator("#skeleton-circle-wrapped")).ToHaveCSSAsync("height", "64px");
        var placeholder = page.Locator("#skeleton-placeholder");
        Assert.Equal("dnet-skeleton-pulse", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').animationName"));
        Assert.Equal("none", await page.Locator("#skeleton-static").EvaluateAsync<string>("element => getComputedStyle(element, '::before').animationName"));
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        Assert.Equal("none", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').animationName"));

        await page.AddStyleTagAsync(new() { Content = ":root { --dnet-skeleton-background: rgb(90, 20, 130); }" });
        Assert.Equal("rgb(90, 20, 130)", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').backgroundColor"));
        await placeholder.EvaluateAsync("element => element.parentElement.style.setProperty('--dnet-skeleton-background', 'rgb(20, 100, 60)')");
        Assert.Equal("rgb(20, 100, 60)", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').backgroundColor"));
        await placeholder.EvaluateAsync("element => element.style.setProperty('--dnet-skeleton-background', 'rgb(150, 50, 20)')");
        Assert.Equal("rgb(150, 50, 20)", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').backgroundColor"));
        await page.EvaluateAsync("() => document.documentElement.dataset.dnetTheme = 'dark'");
        Assert.Equal("rgb(150, 50, 20)", await placeholder.EvaluateAsync<string>("element => getComputedStyle(element, '::before').backgroundColor"));
        await page.SetViewportSizeAsync(390, 844);
        Assert.True(await page.Locator("#skeleton-wrapped").EvaluateAsync<bool>("element => element.getBoundingClientRect().right <= innerWidth"));
        await Assertions.Expect(circle).ToHaveCSSAsync("width", "64px");
    }
}
