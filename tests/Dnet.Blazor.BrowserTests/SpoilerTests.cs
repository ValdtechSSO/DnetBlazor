using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class SpoilerTests
{
    private static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Fact]
    public async Task Keyboard_expansion_short_content_and_dynamic_updates_stay_synchronized()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Spoiler");

        var root = page.Locator("#spoiler-basic");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow", "true");
        var button = root.GetByRole(AriaRole.Button);
        var viewport = root.Locator(".dnet-spoiler-viewport");
        await Assertions.Expect(viewport).ToHaveCSSAsync("height", "110px");
        await Assertions.Expect(viewport).ToHaveCSSAsync("transition-duration", "0s");
        Assert.Equal(await viewport.GetAttributeAsync("id"), await button.GetAttributeAsync("aria-controls"));
        await viewport.FocusAsync();
        await viewport.PressAsync("Tab");
        await Assertions.Expect(button).ToBeFocusedAsync();
        Assert.True(await button.EvaluateAsync<bool>("element => { const style = getComputedStyle(element); return element.matches(':focus-visible') && (parseFloat(style.outlineWidth) > 0 || style.boxShadow !== 'none'); }"));
        await button.PressAsync("Enter");
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(button).ToHaveTextAsync("Mostrar solo el resumen");
        await page.WaitForFunctionAsync("() => { const element = document.querySelector('#spoiler-basic .dnet-spoiler-viewport'); return element.clientHeight >= element.scrollHeight - 1; }");
        await button.PressAsync("Space");
        await Assertions.Expect(viewport).ToHaveCSSAsync("height", "110px");
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-expanded", "false");

        await Assertions.Expect(page.Locator("#spoiler-short .dnet-spoiler-toggle")).ToBeHiddenAsync();
        var dynamic = page.Locator("#spoiler-dynamic");
        var dynamicButton = dynamic.Locator(".dnet-spoiler-toggle");
        await Assertions.Expect(dynamicButton).ToBeHiddenAsync();
        await page.Locator("#spoiler-change-content").ClickAsync();
        await Assertions.Expect(dynamicButton).ToBeVisibleAsync();
        await dynamicButton.ClickAsync();
        await Assertions.Expect(dynamic).ToHaveAttributeAsync("data-expanded", "true");
        // A content update that hides the focused button must leave focus on the content.
        await page.Locator("#spoiler-change-content").EvaluateAsync("element => element.click()");
        await Assertions.Expect(dynamicButton).ToBeHiddenAsync();
        await Assertions.Expect(dynamic.Locator(".dnet-spoiler-viewport")).ToBeFocusedAsync();
        await page.Locator("#spoiler-change-content").ClickAsync();
        await Assertions.Expect(dynamicButton).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Fact]
    public async Task Clipped_controls_are_skipped_and_focus_is_restored_when_collapsing()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Spoiler");
        var root = page.Locator("#spoiler-controlled");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow", "true");
        var visible = page.Locator("#spoiler-visible-link");
        var clipped = page.Locator("#spoiler-hidden-link");
        var originalInert = page.Locator("#spoiler-original-inert");
        Assert.False(await visible.EvaluateAsync<bool>("element => element.inert"));
        Assert.True(await clipped.EvaluateAsync<bool>("element => element.inert"));
        await visible.FocusAsync();
        await visible.PressAsync("Tab");
        var button = root.Locator(".dnet-spoiler-toggle");
        await Assertions.Expect(button).ToBeFocusedAsync();
        await button.PressAsync("Enter");
        await Assertions.Expect(page.Locator("#spoiler-controlled-state")).ToHaveTextAsync("Estado: expandido");
        await page.WaitForFunctionAsync("() => !document.querySelector('#spoiler-hidden-link').inert");
        Assert.True(await originalInert.EvaluateAsync<bool>("element => element.inert"));
        await clipped.FocusAsync();
        await page.Locator("#spoiler-external-toggle").EvaluateAsync("element => element.click()");
        await Assertions.Expect(button).ToBeFocusedAsync();
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-expanded", "false");

        // Removing an instance releases the inert attributes that it owns.
        var handle = await clipped.ElementHandleAsync();
        await root.EvaluateAsync("element => element.remove()");
        Assert.False(await handle!.EvaluateAsync<bool>("element => element.inert"));
    }

    [Fact]
    public async Task Resize_rtl_and_styling_tokens_work_in_all_scopes()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } });
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Spoiler");
        var root = page.Locator("#spoiler-basic");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow", "true");
        await page.EvaluateAsync("() => document.fonts.ready");
        var button = root.Locator(".dnet-spoiler-toggle");
        await page.AddStyleTagAsync(new() { Content = ":root { --dnet-spoiler-control-color: rgb(90, 20, 130); }" });
        await Assertions.Expect(button).ToHaveCSSAsync("color", "rgb(90, 20, 130)");
        await root.EvaluateAsync("element => element.parentElement.style.setProperty('--dnet-spoiler-control-color', 'rgb(20, 100, 60)')");
        await Assertions.Expect(button).ToHaveCSSAsync("color", "rgb(20, 100, 60)");
        await root.EvaluateAsync("element => element.style.setProperty('--dnet-spoiler-control-color', 'rgb(150, 50, 20)')");
        await Assertions.Expect(button).ToHaveCSSAsync("color", "rgb(150, 50, 20)");
        await page.EvaluateAsync("() => document.documentElement.dataset.dnetTheme = 'dark'");
        await Assertions.Expect(button).ToHaveCSSAsync("color", "rgb(150, 50, 20)");

        await root.EvaluateAsync("element => element.dataset.maxHeight = String(element.querySelector('.dnet-spoiler-body').scrollHeight + 10)");
        await Assertions.Expect(button).ToBeHiddenAsync();
        await page.SetViewportSizeAsync(390, 844);
        await Assertions.Expect(button).ToBeVisibleAsync();
        Assert.True(await root.EvaluateAsync<bool>("element => element.getBoundingClientRect().right <= innerWidth"));
        await page.SetViewportSizeAsync(1440, 1000);
        await Assertions.Expect(button).ToBeHiddenAsync();

        var rtl = page.Locator("#spoiler-rtl");
        var rtlButton = rtl.Locator(".dnet-spoiler-toggle");
        await Assertions.Expect(rtlButton).ToHaveAttributeAsync("aria-expanded", "true");
        Assert.True(await rtlButton.EvaluateAsync<bool>("element => Math.abs(element.getBoundingClientRect().right - element.parentElement.getBoundingClientRect().right) <= 1"));
        await rtlButton.ClickAsync();
        await Assertions.Expect(rtl.Locator(".dnet-spoiler-viewport")).ToHaveCSSAsync("height", "70px");
    }
}
