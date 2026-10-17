using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class DatePickerIconTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Calendar_icons_have_a_hollow_body_visible_edges_and_no_legacy_background(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
            "true", StringComparison.OrdinalIgnoreCase)) return;
        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 900 } });
        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/DatePicker");
        var icons = page.Locator(".dnet-form-field-plain-suffix .dnet-datepicker-icon");
        await Assertions.Expect(icons).ToHaveCountAsync(2);
        await page.EvaluateAsync("theme => document.documentElement.dataset.dnetTheme = theme", theme);

        foreach (var icon in await icons.AllAsync())
        {
            await Assertions.Expect(icon).ToBeVisibleAsync();
            await Assertions.Expect(icon).ToHaveCSSAsync("background-image", "none");
            // Rasterize the actual computed mask: a nonempty URL alone missed the malformed SVG.
            var alpha = await icon.EvaluateAsync<int[]>(
                """
                async element => {
                    const mask = getComputedStyle(element).maskImage;
                    const image = new Image();
                    image.src = JSON.parse(mask.slice(4, -1));
                    await image.decode();
                    const canvas = document.createElement('canvas');
                    canvas.width = canvas.height = 160;
                    const context = canvas.getContext('2d');
                    context.drawImage(image, 0, 0, 160, 160);
                    return [[80, 80], [5, 80], [155, 80], [80, 25]]
                        .map(([x, y]) => context.getImageData(x, y, 1, 1).data[3]);
                }
                """);
            Assert.Equal([0, 255, 255, 255], alpha);
        }

        await icons.First.ClickAsync();
        var single = page.Locator("input[role='combobox']");
        await Assertions.Expect(single).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(single).ToHaveAttributeAsync("aria-expanded", "false");
        await icons.Nth(1).ClickAsync();
        var start = page.Locator(".dnet-date-range-start");
        await Assertions.Expect(start).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(start).ToHaveAttributeAsync("aria-expanded", "false");
    }
}
