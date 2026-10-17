using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ButtonVariantTests
{
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
        ?? "http://127.0.0.1:5101").TrimEnd('/');

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Filled_and_outlined_actions_have_visible_hierarchy_hover_and_keyboard_focus(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        await OpenAsync(page, "/Buttons", "#button-variant-filled", theme);
        var filled = page.Locator("#button-variant-filled");
        var outlined = page.Locator("#button-variant-outlined");
        await page.Mouse.MoveAsync(0, 0);
        var primary = await MeasureAsync(filled);
        var secondary = await MeasureAsync(outlined);
        Assert.Equal("rgb(91, 92, 224)", primary.Background);
        Assert.Equal("rgb(255, 255, 255)", primary.Color);
        Assert.Equal("rgba(0, 0, 0, 0)", secondary.Background);
        Assert.Equal(secondary.Color, secondary.BorderColor);
        Assert.True(secondary.Contrast >= 4.5,
            $"Outlined text needs at least 4.5:1 contrast against its surface; actual {secondary.Contrast:F2}:1.");
        Assert.True(secondary.BorderWidth >= 1 && secondary.BorderStyle != "none",
            "The secondary action needs a visible outline.");
        Near(40, primary.Height, "Filled medium height");
        Near(40, secondary.Height, "Outlined medium height");
        Near(9999, primary.Radius, "Filled default pill radius");
        Near(9999, secondary.Radius, "Outlined default pill radius");

        foreach (var button in new[] { filled, outlined })
        {
            var before = await MeasureAsync(button);
            await button.HoverAsync();
            await SettleAsync(page);
            var hovered = await MeasureAsync(button);
            Assert.NotEqual(before.Background, hovered.Background);
            if (await button.GetAttributeAsync("id") == "button-variant-outlined")
                Assert.True(hovered.Contrast >= 4.5,
                    $"Outlined hover text contrast was {hovered.Contrast:F2}:1.");
            Near(before.Width, hovered.Width, "Hover keeps width");
            Near(before.Height, hovered.Height, "Hover keeps height");
            await page.Mouse.MoveAsync(0, 0);
            await button.FocusAsync();
            await page.Keyboard.PressAsync("Shift+Tab");
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(button).ToBeFocusedAsync();
            Assert.True(await button.EvaluateAsync<bool>("button => button.matches(':focus-visible')"));
            await SettleAsync(page);
            var focused = await MeasureAsync(button);
            Assert.True(focused.HasVisibleFocus,
                "Keyboard focus needs a visible outline, shadow or state layer in both themes.");
            Near(before.Width, focused.Width, "Focus keeps width");
            Near(before.Height, focused.Height, "Focus keeps height");
        }

        var disabled = page.Locator("#button-disabled-demo");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        var disabledBefore = await MeasureAsync(disabled);
        await disabled.HoverAsync();
        await SettleAsync(page);
        Assert.Equal(disabledBefore.Background, (await MeasureAsync(disabled)).Background);
        Assert.True(await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth <= document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("dark")]
    public async Task Appearance_tokens_override_both_variants_in_root_parent_and_instance_scopes(string theme)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        await OpenAsync(page, "/Buttons", "#button-variant-filled", theme);
        await page.Mouse.MoveAsync(0, 0);

        foreach (var scope in new[] { "root", "parent", "instance" })
        foreach (var currentApi in new[] { true, false })
        {
            await SetTokensAsync(page, scope, currentApi, false);
            foreach (var selector in new[] { "#button-variant-filled", "#button-variant-outlined" })
            {
                var button = page.Locator(selector);
                var style = await MeasureAsync(button);
                Assert.Equal(currentApi ? "rgb(80, 40, 120)" : "rgb(40, 80, 120)", style.Background);
                if (currentApi)
                {
                    Assert.Equal("rgb(250, 240, 230)", style.Color);
                    Assert.Equal("rgb(230, 190, 70)", style.BorderColor);
                }
                await button.HoverAsync();
                await SettleAsync(page);
                Assert.Equal(currentApi ? "rgb(110, 60, 150)" : "rgb(60, 110, 150)",
                    (await MeasureAsync(button)).Background);
                await page.Mouse.MoveAsync(0, 0);
            }
            await SetTokensAsync(page, scope, currentApi, true);
        }
    }

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 390)]
    public async Task Dialog_primary_and_secondary_buttons_keep_their_hierarchy_and_actions(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        await OpenAsync(page, "/Dialog", "#dialog-open", theme);
        var trigger = page.Locator("#dialog-open");
        Assert.Equal("rgb(91, 92, 224)", (await MeasureAsync(trigger)).Background);
        await trigger.ClickAsync();
        var cancel = page.Locator("#showcase-profile-cancel");
        var save = page.Locator("#showcase-profile-save");
        await Assertions.Expect(cancel).ToBeVisibleAsync();
        await Assertions.Expect(save).ToBeVisibleAsync();
        await page.Mouse.MoveAsync(0, 0);
        Assert.Equal("rgb(91, 92, 224)", (await MeasureAsync(save)).Background);
        var secondary = await MeasureAsync(cancel);
        Assert.Equal("rgba(0, 0, 0, 0)", secondary.Background);
        Assert.Equal(secondary.Color, secondary.BorderColor);
        Assert.True(secondary.Contrast >= 4.5,
            $"Dialog secondary text contrast was {secondary.Contrast:F2}:1.");
        await cancel.ClickAsync();
        await Assertions.Expect(cancel).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#dialog-result")).ToContainTextAsync("Editor dismissed");
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await trigger.ClickAsync();
        await page.Locator("#showcase-profile-name").FillAsync("Alex Morgan");
        await page.Locator("#showcase-profile-save").ClickAsync();
        await Assertions.Expect(page.Locator("#dialog-result")).ToContainTextAsync("Saved: Alex Morgan");
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }

    private static async Task OpenAsync(IPage page, string route, string ready, string theme)
    {
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{BaseUrl}{route}");
        await Assertions.Expect(page.Locator(ready)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);
        await page.EvaluateAsync("async () => { await document.fonts.ready; }");
    }

    private static Task SettleAsync(IPage page) => page.EvaluateAsync(
        "async () => { await Promise.all(document.getAnimations().filter(animation => Number.isFinite(animation.effect?.getTiming().iterations)).map(animation => animation.finished.catch(() => {}))); await new Promise(requestAnimationFrame); await new Promise(requestAnimationFrame); }");

    private static Task SetTokensAsync(IPage page, string scope, bool currentApi, bool clear) => page.EvaluateAsync(
        """
        ({ scope, currentApi, clear }) => {
            const buttons = ['button-variant-filled', 'button-variant-outlined'].map(id => document.getElementById(id));
            let parent = buttons[0].parentElement;
            while (!buttons.every(button => parent.contains(button))) parent = parent.parentElement;
            const targets = scope === 'root' ? [document.documentElement] : scope === 'parent' ? [parent] : buttons;
            const legacy = { '--dnet-button-background-color': 'rgb(40, 80, 120)', '--dnet-button-hover-color': 'rgb(60, 110, 150)' };
            const current = { '--dnet-btn-background': 'rgb(80, 40, 120)', '--dnet-btn-background-hover': 'rgb(110, 60, 150)', '--dnet-btn-foreground': 'rgb(250, 240, 230)', '--dnet-btn-border-color': 'rgb(230, 190, 70)' };
            for (const target of targets) for (const [name, value] of Object.entries({ ...legacy, ...current })) {
                if (clear || (!currentApi && name in current)) target.style.removeProperty(name);
                else target.style.setProperty(name, value);
            }
        }
        """, new { scope, currentApi, clear });

    private static Task<ButtonAppearance> MeasureAsync(ILocator button) => button.EvaluateAsync<ButtonAppearance>(
        """
        button => {
            const style = getComputedStyle(button);
            const rect = button.getBoundingClientRect();
            const overlay = button.querySelector('.dnet-button-focus-overlay');
            const overlayStyle = overlay && getComputedStyle(overlay);
            // Canvas resolves CSS colors (including color-mix) to sRGB pixels.
            // Composite ancestor surfaces instead of assuming a theme color.
            const canvas = document.createElement('canvas');
            canvas.width = canvas.height = 1;
            const context = canvas.getContext('2d', { willReadFrequently: true });
            const rgba = color => {
                context.clearRect(0, 0, 1, 1);
                context.fillStyle = color;
                context.fillRect(0, 0, 1, 1);
                return [...context.getImageData(0, 0, 1, 1).data].map((value, index) => index === 3 ? value / 255 : value);
            };
            const composite = (front, back) => {
                const alpha = front[3] + back[3] * (1 - front[3]);
                return [...front.slice(0, 3).map((value, index) =>
                    (value * front[3] + back[index] * back[3] * (1 - front[3])) / alpha), alpha];
            };
            const ancestors = [];
            for (let element = button; element; element = element.parentElement) ancestors.push(element);
            const background = ancestors.reverse().reduce((surface, element) =>
                composite(rgba(getComputedStyle(element).backgroundColor), surface), [255, 255, 255, 1]);
            const foreground = composite(rgba(style.color), background);
            const luminance = color => {
                const linear = color.slice(0, 3).map(channel => {
                    const value = channel / 255;
                    return value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
                });
                return linear[0] * 0.2126 + linear[1] * 0.7152 + linear[2] * 0.0722;
            };
            const textLuminance = luminance(foreground), surfaceLuminance = luminance(background);
            return {
                background: style.backgroundColor, color: style.color, borderColor: style.borderTopColor,
                borderStyle: style.borderTopStyle, borderWidth: parseFloat(style.borderTopWidth),
                width: rect.width, height: rect.height, radius: parseFloat(style.borderTopLeftRadius),
                contrast: (Math.max(textLuminance, surfaceLuminance) + 0.05) / (Math.min(textLuminance, surfaceLuminance) + 0.05),
                hasVisibleFocus: (parseFloat(style.outlineWidth) > 0 && style.outlineStyle !== 'none' && style.outlineColor !== 'rgba(0, 0, 0, 0)')
                    || style.boxShadow !== 'none'
                    || (!!overlayStyle && parseFloat(overlayStyle.opacity) > 0 && overlayStyle.backgroundColor !== 'rgba(0, 0, 0, 0)')
            };
        }
        """);

    private static void Near(double expected, double actual, string dimension) =>
        Assert.True(Math.Abs(expected - actual) <= 0.8, $"{dimension}: expected {expected}px, actual {actual}px.");

    private sealed class ButtonAppearance
    {
        public string Background { get; init; } = "";
        public string Color { get; init; } = "";
        public string BorderColor { get; init; } = "";
        public string BorderStyle { get; init; } = "";
        public double BorderWidth { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public double Radius { get; init; }
        public bool HasVisibleFocus { get; init; }
        public double Contrast { get; init; }
    }
}
