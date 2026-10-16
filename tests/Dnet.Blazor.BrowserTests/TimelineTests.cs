using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class TimelineTests
{
    private static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Fact]
    public async Task Events_remain_an_ordered_list_and_controls_have_a_visible_keyboard_focus()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Timeline");
        var root = page.Locator("#timeline-default");
        await Assertions.Expect(root).ToBeVisibleAsync();
        Assert.Equal("OL", await root.EvaluateAsync<string>("element => element.tagName"));
        Assert.True(await root.Locator(":scope > li").CountAsync() >= 3);
        await Assertions.Expect(root.Locator("[aria-current='step']")).ToHaveCountAsync(1);
        Assert.True(await root.Locator("time[datetime]").CountAsync() >= 1);
        Assert.True(await root.Locator(".dnet-timeline-bullet[aria-hidden='true']").CountAsync() >= 3);
        var link = page.Locator("#timeline-event-link");
        await link.FocusAsync();
        await link.PressAsync("Shift+Tab");
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(link).ToBeFocusedAsync();
        Assert.True(await link.EvaluateAsync<bool>("element => { const style = getComputedStyle(element); return element.matches(':focus-visible') && (parseFloat(style.outlineWidth) > 0 || style.boxShadow !== 'none'); }"));
    }

    [Fact]
    public async Task Alternate_and_rtl_layouts_preserve_reading_order_and_stack_on_mobile()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } });
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Timeline");
        var alternate = page.Locator("#timeline-alternate");
        await Assertions.Expect(alternate).ToBeVisibleAsync();
        Assert.True(await alternate.EvaluateAsync<bool>("element => { const items = element.querySelectorAll(':scope > li'); const first = items[0].querySelector('.dnet-timeline-content').getBoundingClientRect(); const second = items[1].querySelector('.dnet-timeline-content').getBoundingClientRect(); return first.left > second.right; }"));
        var rtl = page.Locator("#timeline-rtl");
        Assert.True(await rtl.EvaluateAsync<bool>("element => { const item = element.querySelector('li'); return getComputedStyle(element).direction === 'rtl' && item.querySelector('.dnet-timeline-bullet').getBoundingClientRect().left > item.querySelector('.dnet-timeline-content').getBoundingClientRect().right; }"));
        await page.SetViewportSizeAsync(390, 844);
        Assert.True(await alternate.EvaluateAsync<bool>("element => { const items = element.querySelectorAll(':scope > li'); const a = items[0].querySelector('.dnet-timeline-content').getBoundingClientRect(); const b = items[1].querySelector('.dnet-timeline-content').getBoundingClientRect(); return Math.abs(a.left - b.left) < 1 && element.getBoundingClientRect().right <= innerWidth; }"));
        await page.EvaluateAsync("() => document.documentElement.dataset.dnetTheme = 'dark'");
        await page.AddStyleTagAsync(new() { Content = ":root { --dnet-timeline-accent-color: rgb(90, 20, 130); }" });
        var root = page.Locator("#timeline-default");
        var marker = root.Locator(".dnet-timeline-item-completed > .dnet-timeline-bullet").First;
        await Assertions.Expect(marker).ToHaveCSSAsync("background-color", "rgb(90, 20, 130)");
        await root.EvaluateAsync("element => element.parentElement.style.setProperty('--dnet-timeline-accent-color', 'rgb(20, 100, 60)')");
        await Assertions.Expect(marker).ToHaveCSSAsync("background-color", "rgb(20, 100, 60)");
        await root.EvaluateAsync("element => element.style.setProperty('--dnet-timeline-accent-color', 'rgb(150, 50, 20)')");
        await Assertions.Expect(marker).ToHaveCSSAsync("background-color", "rgb(150, 50, 20)");
    }
}
