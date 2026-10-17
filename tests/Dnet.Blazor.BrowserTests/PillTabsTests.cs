using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class PillTabsTests
{
    private static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Fact]
    public async Task Selection_keyboard_panels_and_live_counts_stay_synchronized()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/PillTabs");

        var reference = page.Locator("#pill-tabs-reference");
        var tabs = reference.GetByRole(AriaRole.Tab);
        await Assertions.Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");
        await tabs.Nth(0).FocusAsync();
        await tabs.Nth(0).PressAsync("ArrowRight");
        await Assertions.Expect(tabs.Nth(1)).ToBeFocusedAsync();
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(reference.GetByRole(AriaRole.Tabpanel)).ToContainTextAsync("Aprobados");

        await tabs.Nth(1).PressAsync("End");
        await Assertions.Expect(tabs.Nth(2)).ToHaveAttributeAsync("aria-selected", "true");
        await tabs.Nth(2).PressAsync("Home");
        await Assertions.Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");
        await tabs.Nth(0).PressAsync("ArrowLeft");
        await Assertions.Expect(tabs.Nth(2)).ToHaveAttributeAsync("aria-selected", "true");
        await tabs.Nth(2).PressAsync("Tab");
        await Assertions.Expect(reference.GetByRole(AriaRole.Tabpanel)).ToBeFocusedAsync();

        var synchronized = page.Locator("#pill-tabs-synchronized").GetByRole(AriaRole.Button);
        await Assertions.Expect(synchronized.Nth(2)).ToHaveAttributeAsync("aria-pressed", "true");
        await synchronized.Nth(0).ClickAsync();
        await Assertions.Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");
        await page.Locator("#pill-tabs-select-all").ClickAsync();
        await Assertions.Expect(tabs.Nth(2)).ToHaveAttributeAsync("aria-selected", "true");
        await page.Locator("#pill-tabs-add-pending").ClickAsync();
        await Assertions.Expect(tabs.Nth(0).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync("2");
        await Assertions.Expect(tabs.Nth(2).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync("4");

        var settings = page.Locator("#pill-tabs-settings").GetByRole(AriaRole.Tab);
        await settings.Nth(0).FocusAsync();
        await settings.Nth(0).PressAsync("ArrowRight");
        await Assertions.Expect(settings.Nth(1).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync("0");
        await settings.Nth(1).PressAsync("ArrowRight");
        await Assertions.Expect(settings.Nth(3)).ToBeFocusedAsync();
        await Assertions.Expect(settings.Nth(2)).ToBeDisabledAsync();

        var rtl = page.Locator("#pill-tabs-rtl").GetByRole(AriaRole.Button);
        await rtl.Nth(0).FocusAsync();
        await rtl.Nth(0).PressAsync("ArrowLeft");
        await Assertions.Expect(rtl.Nth(1)).ToBeFocusedAsync();
        await Assertions.Expect(rtl.Nth(1)).ToHaveAttributeAsync("aria-pressed", "true");
    }

    [Fact]
    public async Task Overflow_fades_follow_scrolling_resize_and_content_changes_in_both_directions()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/PillTabs");
        await page.Locator("#pill-tabs-overflow").WaitForAsync();
        await page.EvaluateAsync("() => document.fonts.ready");

        foreach (var (id, rtl) in new[] { ("pill-tabs-overflow", false), ("pill-tabs-rtl", true) })
        {
            var list = page.Locator($"#{id} .dnet-pill-tabs-list");
            await Assertions.Expect(list).ToHaveCSSAsync("scrollbar-width", "none");
            Assert.Equal("none", await list.EvaluateAsync<string>(
                "element => getComputedStyle(element, '::-webkit-scrollbar').display"));
            await ExpectFadesAsync(page, id, left: rtl, right: !rtl);

            await list.HoverAsync();
            await page.Mouse.WheelAsync(rtl ? -40 : 40, 0);
            await ExpectFadesAsync(page, id, left: true, right: true);
            await list.EvaluateAsync("(element, rtl) => element.scrollLeft = rtl ? -element.scrollWidth : element.scrollWidth", rtl);
            await ExpectFadesAsync(page, id, left: !rtl, right: rtl);

            // Native keyboard focus must reveal the chosen option inside the scroller.
            var tabs = list.GetByRole(AriaRole.Button);
            await tabs.Nth(0).FocusAsync();
            await tabs.Nth(0).PressAsync("End");
            await Assertions.Expect(tabs.Nth(2)).ToBeFocusedAsync();
            await Assertions.Expect(tabs.Nth(2)).ToHaveAttributeAsync("aria-pressed", "true");
            Assert.True(await tabs.Nth(2).EvaluateAsync<bool>("element => { const tab = element.getBoundingClientRect(); const list = element.parentElement.getBoundingClientRect(); return tab.left >= list.left - 1 && tab.right <= list.right + 1; }"));

            await list.EvaluateAsync("element => element.parentElement.parentElement.parentElement.style.maxWidth = '600px'");
            await ExpectFadesAsync(page, id, left: false, right: false);
            await list.EvaluateAsync("element => { element.parentElement.parentElement.parentElement.style.maxWidth = '260px'; element.scrollLeft = 0; }");
            await ExpectFadesAsync(page, id, left: rtl, right: !rtl);

            // Intrinsic label/count changes can change overflow without resizing the list.
            await list.EvaluateAsync("element => element.parentElement.parentElement.parentElement.style.maxWidth = '600px'");
            await ExpectFadesAsync(page, id, left: false, right: false);
            await list.EvaluateAsync("element => element.querySelector('.dnet-pill-tabs-label').textContent = 'Pendientes de revisión y validación del equipo '.repeat(4)");
            await ExpectFadesAsync(page, id, left: rtl, right: !rtl);
            await list.EvaluateAsync("element => element.replaceChildren(element.lastElementChild)");
            await ExpectFadesAsync(page, id, left: false, right: false);
        }

        await ExpectFadesAsync(page, "pill-tabs-reference", left: false, right: false);
    }

    private static async Task ExpectFadesAsync(IPage page, string id, bool left, bool right)
    {
        await page.WaitForFunctionAsync("({ id, left, right }) => { const viewport = document.querySelector(`#${id} .dnet-pill-tabs-viewport`); return viewport?.classList.contains('dnet-pill-tabs-overflow-left') === left && viewport.classList.contains('dnet-pill-tabs-overflow-right') === right; }",
            new { id, left, right });
    }

    [Fact]
    public async Task Public_tokens_inherit_at_all_scopes_and_mobile_overflow_stays_inside_the_list()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 390, Height = 844 } });
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/PillTabs");
        var root = page.Locator("#pill-tabs-settings");
        await root.WaitForAsync();
        var selected = root.Locator("[aria-selected=true]");

        await page.AddStyleTagAsync(new() { Content = ":root { --dnet-pill-tabs-selected-background: rgb(90, 20, 130); }" });
        await Assertions.Expect(selected).ToHaveCSSAsync("background-color", "rgb(90, 20, 130)");
        await root.EvaluateAsync("element => element.parentElement.style.setProperty('--dnet-pill-tabs-selected-background', 'rgb(20, 100, 60)')");
        await Assertions.Expect(selected).ToHaveCSSAsync("background-color", "rgb(20, 100, 60)");
        await root.EvaluateAsync("element => element.style.setProperty('--dnet-pill-tabs-selected-background', 'rgb(150, 50, 20)')");
        await Assertions.Expect(selected).ToHaveCSSAsync("background-color", "rgb(150, 50, 20)");

        await page.EvaluateAsync("() => document.documentElement.dataset.dnetTheme = 'dark'");
        await Assertions.Expect(root.Locator(".dnet-pill-tabs-list")).ToHaveCSSAsync("background-color", "rgb(30, 31, 34)");
        var overflow = page.Locator("#pill-tabs-rtl .dnet-pill-tabs-list");
        Assert.True(await overflow.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth"));
        Assert.True(await overflow.EvaluateAsync<bool>("element => element.getBoundingClientRect().right <= innerWidth"));
        Assert.True(await page.Locator(".dnet-pill-tabs").EvaluateAllAsync<bool>(
            "elements => elements.every(element => element.getBoundingClientRect().right <= innerWidth)"));
    }
}
