using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class FormFieldAppearanceTests
{
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Filled_and_transparent_modes_preserve_values_and_cover_the_entire_control(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Forms");
        var appearance = page.Locator("#form-appearance-select");
        await Assertions.Expect(appearance).ToHaveValueAsync("Fill");
        await page.EvaluateAsync("theme => document.documentElement.dataset.dnetTheme = theme", theme);
        var containers = page.Locator("form .dnet-form-field-plain-control-container");
        Assert.True(await containers.CountAsync() >= 9);
        var first = containers.First;
        var filledColor = await first.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.True(await first.EvaluateAsync<bool>("element => { const c = getComputedStyle(element).backgroundColor; return c !== 'transparent' && c !== 'rgba(0, 0, 0, 0)' && !c.endsWith('/ 0)'); }"));
        await Assertions.Expect(containers.Filter(new() { Has = page.Locator(".dnet-plain-select") })).ToHaveCSSAsync("background-color", filledColor);
        await Assertions.Expect(containers.Filter(new() { Has = page.Locator("textarea") })).ToHaveCSSAsync("background-color", filledColor);
        var input = first.Locator("input");
        await input.FillAsync("");
        var disabled = containers.Filter(new() { Has = page.Locator("input:disabled") });
        await Assertions.Expect(disabled).ToHaveCountAsync(1);
        // Disabled fields retain an opaque, quieter fill.
        Assert.True(await disabled.EvaluateAsync<bool>(
            "element => { const c = getComputedStyle(element).backgroundColor; return c.startsWith('rgb(') || (c.startsWith('color(') && !c.includes('/')); }"));
        var original = await input.ElementHandleAsync();
        await input.FillAsync("Se conserva al cambiar el fondo");
        await appearance.SelectOptionAsync("Standard");
        await Assertions.Expect(first).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
        await Assertions.Expect(containers.Filter(new() { Has = page.Locator(".dnet-plain-select") })).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
        await Assertions.Expect(input).ToHaveValueAsync("Se conserva al cambiar el fondo");
        await appearance.SelectOptionAsync("Fill");
        await Assertions.Expect(first).ToHaveCSSAsync("background-color", filledColor);
        Assert.True(await original!.EvaluateAsync<bool>("element => element === document.querySelector('form input')"));
        await Assertions.Expect(input).ToHaveValueAsync("Se conserva al cambiar el fondo");
    }

    [Fact]
    public async Task Background_tokens_work_in_every_scope_and_nested_themes_remain_independent()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{BaseUrl.TrimEnd('/')}/Forms");
        var field = page.Locator("form .dnet-form-field-plain-wrapper").First;
        var control = field.Locator(".dnet-form-field-plain-control-container");
        await Assertions.Expect(page.Locator("#form-appearance-select")).ToHaveValueAsync("Fill");
        await page.EvaluateAsync("() => document.documentElement.dataset.dnetTheme = ''");
        var light = await control.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await field.EvaluateAsync("element => element.dataset.dnetTheme = 'dark'");
        Assert.NotEqual(light, await control.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        await page.AddStyleTagAsync(new() { Content = ":root { --dnet-form-field-background: rgb(90, 20, 130); }" });
        await Assertions.Expect(control).ToHaveCSSAsync("background-color", "rgb(90, 20, 130)");
        await field.EvaluateAsync("element => element.parentElement.style.setProperty('--dnet-form-field-background', 'rgb(20, 100, 60)')");
        await Assertions.Expect(control).ToHaveCSSAsync("background-color", "rgb(20, 100, 60)");
        await field.EvaluateAsync("element => element.style.setProperty('--dnet-form-field-background', 'rgb(150, 50, 20)')");
        await Assertions.Expect(control).ToHaveCSSAsync("background-color", "rgb(150, 50, 20)");
        await page.Locator("#form-appearance-select").SelectOptionAsync("Standard");
        await Assertions.Expect(control).ToHaveCSSAsync("background-color", "rgb(150, 50, 20)");
    }
}
