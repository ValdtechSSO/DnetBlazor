using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ButtonSizeTests
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
    public async Task Sizes_keep_their_dimensions_icon_spacing_and_text_layout_for_every_shape(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        await OpenAsync(page, theme);

        foreach (var (size, height, padding, iconSize, gap) in Sizes)
        foreach (var (shape, radius) in Shapes)
        {
            var id = $"button-{size}-{shape}";
            var withIcon = page.Locator($"#{id}");
            var textOnly = page.Locator($"#{id}-text");
            await withIcon.ScrollIntoViewIfNeededAsync();
            var iconMetrics = await MeasureAsync(withIcon);
            var textMetrics = await MeasureAsync(textOnly);

            Near(height, iconMetrics.Height, $"{id} height");
            Near(height, textMetrics.Height, $"{id}-text height");
            Near(padding, iconMetrics.PaddingLeft, $"{id} left padding");
            Near(padding, iconMetrics.PaddingRight, $"{id} right padding");
            Near(padding, textMetrics.PaddingLeft, $"{id}-text left padding");
            Near(padding, textMetrics.PaddingRight, $"{id}-text right padding");
            Near(radius, iconMetrics.Radius, $"{id} radius");
            Near(radius, textMetrics.Radius, $"{id}-text radius");
            Near(iconSize, iconMetrics.IconWidth, $"{id} icon width");
            Near(iconSize, iconMetrics.IconHeight, $"{id} icon height");
            Near(iconSize, iconMetrics.SvgWidth, $"{id} SVG width");
            Near(iconSize, iconMetrics.SvgHeight, $"{id} SVG height");
            Near(gap, iconMetrics.Gap, $"{id} visible gap between icon and text");
            Near(0, iconMetrics.IconCenterOffset, $"{id} icon vertical centering");
            Near(iconSize + gap, iconMetrics.Width - textMetrics.Width,
                $"{id} text-only buttons do not reserve an empty icon slot");
            Assert.Equal("true", await withIcon.Locator(".dnet-button-icon").GetAttributeAsync("aria-hidden"));
            await Assertions.Expect(withIcon).ToHaveAccessibleNameAsync("Common button");
            Assert.Empty(await textOnly.Locator(".dnet-button-icon").AllAsync());
            Assert.InRange(iconMetrics.Left, -1, width);
            Assert.InRange(iconMetrics.Right, 0, width + 1);
        }
    }

    [Fact]
    public async Task Default_medium_buttons_support_pointer_and_keyboard_activation_and_native_disabled_behavior()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        await OpenAsync(page, "dark");
        var demo = page.Locator("#button-click-demo");
        var count = page.Locator("#button-click-count");
        Near(40, (await MeasureAsync(demo)).Height, "Default medium button height");

        await demo.ClickAsync();
        await Assertions.Expect(count).ToHaveTextAsync("1");
        await demo.FocusAsync();
        await demo.PressAsync("Enter");
        await Assertions.Expect(count).ToHaveTextAsync("2");
        await demo.PressAsync("Space");
        await Assertions.Expect(count).ToHaveTextAsync("3");

        var disabled = page.Locator("#button-disabled-demo");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        // HTMLElement.click() uses the browser's native disabled check before dispatching a click.
        await disabled.EvaluateAsync("button => { button.click(); button.focus(); }");
        await Assertions.Expect(disabled).Not.ToBeFocusedAsync();
        await Assertions.Expect(count).ToHaveTextAsync("3");

        var toggle = page.Locator("#button-disable-toggle");
        await toggle.EvaluateAsync("input => input.click()");
        await Assertions.Expect(toggle).ToBeCheckedAsync();
        foreach (var (size, _, _, _, _) in Sizes)
        foreach (var (shape, _) in Shapes)
        {
            await Assertions.Expect(page.Locator($"#button-{size}-{shape}")).ToBeDisabledAsync();
            await Assertions.Expect(page.Locator($"#button-{size}-{shape}-text")).ToBeDisabledAsync();
        }
        await Assertions.Expect(demo).ToBeEnabledAsync();
        await demo.ClickAsync();
        await Assertions.Expect(count).ToHaveTextAsync("4");
    }

    [Fact]
    public async Task Public_and_legacy_size_overrides_win_in_root_parent_and_instance_scopes()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        await OpenAsync(page, "");
        // Remove only the gallery's explicit shape class so inherited radius overrides can apply.
        await page.EvaluateAsync(
            "() => { for (const size of ['small', 'medium', 'large']) document.getElementById(`button-${size}-round`).classList.remove('button-shape-round'); }");

        foreach (var scope in new[] { "root", "parent", "instance" })
        {
            // The newer token must win when both APIs are supplied; aliases remain effective by themselves.
            foreach (var currentApi in new[] { true, false })
            {
                await SetOverridesAsync(page, scope, currentApi, clear: false);
                foreach (var (size, _, _, _, _) in Sizes)
                {
                    var metrics = await MeasureAsync(page.Locator($"#button-{size}-round"));
                    var label = $"{size} in {scope}, {(currentApi ? "current" : "legacy")} API";
                    Near(currentApi ? 44 : 48, metrics.Height, $"{label} height");
                    Near(currentApi ? 27 : 30, metrics.PaddingLeft, $"{label} left padding");
                    Near(currentApi ? 27 : 30, metrics.PaddingRight, $"{label} right padding");
                    Near(currentApi ? 2 : 3, metrics.Radius, $"{label} radius");
                    Near(currentApi ? 240 : 260, metrics.MinWidth, $"{label} minimum width");
                }
                await SetOverridesAsync(page, scope, currentApi, clear: true);
            }
        }
    }

    private static readonly (string Size, int Height, int Padding, int IconSize, int Gap)[] Sizes =
    [
        ("small", 32, 12, 20, 4),
        ("medium", 40, 16, 20, 8),
        ("large", 56, 24, 24, 8)
    ];

    private static readonly (string Shape, int Radius)[] Shapes =
    [
        ("round", 9999), ("soft", 6), ("square", 0)
    ];

    private static async Task OpenAsync(IPage page, string theme)
    {
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        await page.GotoAsync($"{BaseUrl}/Buttons");
        await Assertions.Expect(page.Locator("#button-small-round")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);
        await page.EvaluateAsync("async () => { await document.fonts.ready; }");
    }

    private static Task SetOverridesAsync(IPage page, string scope, bool currentApi, bool clear) => page.EvaluateAsync(
        """
        ({ scope, currentApi, clear }) => {
            const buttons = ['small', 'medium', 'large'].map(size => document.getElementById(`button-${size}-round`));
            let parent = buttons[0].parentElement;
            while (!buttons.every(button => parent.contains(button))) parent = parent.parentElement;
            const targets = scope === 'root' ? [document.documentElement] : scope === 'parent' ? [parent] : buttons;
            const legacy = {
                '--dnet-button-line-height': '48px', '--dnet-button-padding': '0 30px',
                '--dnet-button-border-radius': '3px', '--dnet-button-min-width': '260px'
            };
            const current = {
                '--dnet-btn-height': '44px', '--dnet-btn-padding': '0 27px',
                '--dnet-btn-radius': '2px', '--dnet-btn-min-width': '240px'
            };
            for (const target of targets) {
                for (const [name, value] of Object.entries({ ...legacy, ...current })) {
                    if (clear || (!currentApi && name in current)) target.style.removeProperty(name);
                    else target.style.setProperty(name, value);
                }
            }
        }
        """, new { scope, currentApi, clear });

    private static Task<ButtonMetrics> MeasureAsync(ILocator button) => button.EvaluateAsync<ButtonMetrics>(
        """
        button => {
            const rect = button.getBoundingClientRect();
            const style = getComputedStyle(button);
            const wrapper = button.querySelector('.dnet-button-wrapper');
            const icon = button.querySelector('.dnet-button-icon');
            const svg = icon?.querySelector('svg');
            const iconRect = icon?.getBoundingClientRect();
            const svgRect = svg?.getBoundingClientRect();
            const walker = document.createTreeWalker(wrapper, NodeFilter.SHOW_TEXT);
            let textRect = null;
            for (let node = walker.nextNode(); node; node = walker.nextNode()) {
                const text = node.textContent.trim();
                if (!text || node.parentElement.closest('.dnet-button-icon')) continue;
                const range = document.createRange();
                const start = node.textContent.indexOf(text);
                range.setStart(node, start);
                range.setEnd(node, start + text.length);
                textRect = range.getBoundingClientRect();
                break;
            }
            return {
                left: rect.left, right: rect.right, width: rect.width, height: rect.height,
                paddingLeft: parseFloat(style.paddingLeft), paddingRight: parseFloat(style.paddingRight),
                radius: parseFloat(style.borderTopLeftRadius), minWidth: parseFloat(style.minWidth),
                iconWidth: iconRect?.width || 0, iconHeight: iconRect?.height || 0,
                svgWidth: svgRect?.width || 0, svgHeight: svgRect?.height || 0,
                gap: iconRect && textRect ? textRect.left - iconRect.right : 0,
                iconCenterOffset: iconRect ? (iconRect.top + iconRect.height / 2) - (rect.top + rect.height / 2) : 0
            };
        }
        """);

    private static void Near(double expected, double actual, string dimension) =>
        Assert.True(Math.Abs(expected - actual) <= 0.8,
            $"{dimension}: expected {expected}px, actual {actual}px.");

    private sealed class ButtonMetrics
    {
        public double Left { get; init; }
        public double Right { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public double PaddingLeft { get; init; }
        public double PaddingRight { get; init; }
        public double Radius { get; init; }
        public double MinWidth { get; init; }
        public double IconWidth { get; init; }
        public double IconHeight { get; init; }
        public double SvgWidth { get; init; }
        public double SvgHeight { get; init; }
        public double Gap { get; init; }
        public double IconCenterOffset { get; init; }
    }
}
