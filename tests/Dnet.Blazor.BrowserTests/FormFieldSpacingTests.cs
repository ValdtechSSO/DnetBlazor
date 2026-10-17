using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class FormFieldSpacingTests
{
    private static bool Enabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
        "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Filled_fields_follow_the_reference_dimensions_in_every_theme_and_viewport(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Forms");
        await Assertions.Expect(page.Locator("#spacing-counter textarea")).ToHaveValueAsync("Input");
        await page.EvaluateAsync("theme => document.documentElement.dataset.dnetTheme = theme", theme);

        foreach (var id in new[] { "spacing-empty", "spacing-value", "spacing-icons-empty", "spacing-icons-value", "spacing-support", "spacing-counter" })
        {
            var field = page.Locator($"#{id}");
            var control = await BoxAsync(field.Locator(".dnet-form-field-plain-control-container"));
            Near(56, control.Height, $"{id} height");
            Assert.True(control.Width > 180 && control.Width <= width, $"{id} fits the viewport");
        }

        await AssertLabelAsync(page.Locator("#spacing-empty"), 16, 16, 24);
        await AssertLabelAsync(page.Locator("#spacing-value"), 16, 8, 16);
        await AssertInputAsync(page.Locator("#spacing-value"), 16, 16);
        await AssertLabelAsync(page.Locator("#spacing-icons-empty"), 52, 16, 24);
        await AssertLabelAsync(page.Locator("#spacing-icons-value"), 52, 8, 16);
        await AssertInputAsync(page.Locator("#spacing-icons-value"), 52, 52);

        foreach (var id in new[] { "spacing-icons-empty", "spacing-icons-value" })
        {
            var field = page.Locator($"#{id}");
            var control = await BoxAsync(field.Locator(".dnet-form-field-plain-control-container"));
            var prefix = await BoxAsync(field.Locator(".dnet-form-field-plain-prefix"));
            Near(12, prefix.X - control.X, $"{id} leading icon inset");
            Near(16, prefix.Y - control.Y, $"{id} icon vertical inset");
            Near(24, prefix.Width, $"{id} leading icon width");
            Near(24, prefix.Height, $"{id} leading icon height");
        }

        var withIcons = page.Locator("#spacing-icons-value");
        var iconControl = await BoxAsync(withIcons.Locator(".dnet-form-field-plain-control-container"));
        var clear = await BoxAsync(withIcons.Locator(".dnet-component-icon-wrapper"));
        Near(12, iconControl.Right - clear.Right, "Trailing icon inset");
        Near(24, clear.Width, "Trailing icon width");
        Near(24, clear.Height, "Trailing icon height");

        foreach (var id in new[] { "spacing-support", "spacing-counter" })
        {
            var field = page.Locator($"#{id}");
            var control = await BoxAsync(field.Locator(".dnet-form-field-plain-control-container"));
            var hint = await BoxAsync(field.Locator(".dnet-hint"));
            Near(4, hint.Y - control.Bottom, $"{id} supporting text top gap");
            Near(16, hint.X - control.X, $"{id} supporting text start inset");
        }
        var counterField = page.Locator("#spacing-counter");
        var counterControl = await BoxAsync(counterField.Locator(".dnet-form-field-plain-control-container"));
        var counter = await BoxAsync(counterField.Locator(".dnet-form-field-char-counter"));
        Near(4, counter.Y - counterControl.Bottom, "Counter top gap");
        Near(16, counterControl.Right - counter.Right, "Counter end inset");
        await Assertions.Expect(counterField.Locator(".dnet-form-field-plain-info .dnet-form-field-char-counter"))
            .ToHaveTextAsync(new System.Text.RegularExpressions.Regex(@"5\s*/\s*20"));

        // Native date segments must not create a second line via the legacy ::after rule.
        var date = page.Locator("form .dnet-form-field-plain-wrapper")
            .Filter(new() { Has = page.Locator("input[type=date]") });
        var dateControl = await BoxAsync(date.Locator(".dnet-form-field-plain-control-container"));
        Near(56, dateControl.Height, "Native date field height");
        await AssertInputAsync(date, 16, 16);

        // Focusing the real combobox raises its label without moving the arrow down with the text.
        var select = page.Locator("form .dnet-form-field-plain-wrapper")
            .Filter(new() { Has = page.Locator(".dnet-plain-select") });
        await select.Locator(".dnet-plain-select").FocusAsync();
        var selectControl = await BoxAsync(select.Locator(".dnet-form-field-plain-control-container"));
        var arrow = await BoxAsync(select.Locator(".dnet-plain-select-arrow"));
        Near(56, selectControl.Height, "Focused select field height");
        Near(28, arrow.Y + arrow.Height / 2 - selectControl.Y, "Focused select arrow center");
        Near(24, arrow.Height, "Focused select arrow height");
    }

    [Fact]
    public async Task Floating_labels_and_supporting_counts_follow_editing_and_logical_rtl_spacing()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Forms");
        var field = page.Locator("#spacing-empty");
        var input = field.Locator("input");
        await Assertions.Expect(input).ToHaveValueAsync("");
        await input.FocusAsync();
        await WaitForLabelTopAsync(page, "spacing-empty", 8);
        await AssertLabelAsync(field, 16, 8, 16);
        await input.FillAsync("Saved value");
        await input.BlurAsync();
        await AssertLabelAsync(field, 16, 8, 16);
        await input.FillAsync("");
        await input.BlurAsync();
        await WaitForLabelTopAsync(page, "spacing-empty", 16);
        await AssertLabelAsync(field, 16, 16, 24);

        var counterField = page.Locator("#spacing-counter");
        var textarea = counterField.Locator("textarea");
        await textarea.FillAsync("Seven!!");
        await Assertions.Expect(counterField.Locator(".dnet-form-field-char-counter"))
            .ToHaveTextAsync(new System.Text.RegularExpressions.Regex(@"7\s*/\s*20"));
        var icons = page.Locator("#spacing-icons-value");
        await icons.EvaluateAsync("element => element.dir = 'rtl'");
        var control = await BoxAsync(icons.Locator(".dnet-form-field-plain-control-container"));
        var prefix = await BoxAsync(icons.Locator(".dnet-form-field-plain-prefix"));
        var clear = await BoxAsync(icons.Locator(".dnet-component-icon-wrapper"));
        var label = await BoxAsync(icons.Locator(".dnet-form-field-plain-label"));
        var text = await BoxAsync(icons.Locator("input"));
        Near(12, control.Right - prefix.Right, "RTL leading icon inset");
        Near(12, clear.X - control.X, "RTL trailing icon inset");
        Near(52, control.Right - label.Right, "RTL floating label start inset");
        Near(52, control.Right - text.Right, "RTL text start inset");
        Near(52, text.X - control.X, "RTL text end inset");

        await counterField.EvaluateAsync("element => element.dir = 'rtl'");
        var rtlControl = await BoxAsync(counterField.Locator(".dnet-form-field-plain-control-container"));
        var hint = await BoxAsync(counterField.Locator(".dnet-hint"));
        var counter = await BoxAsync(counterField.Locator(".dnet-form-field-char-counter"));
        Near(16, rtlControl.Right - hint.Right, "RTL supporting text start inset");
        Near(16, counter.X - rtlControl.X, "RTL counter end inset");
    }

    [Fact]
    public async Task Geometry_tokens_inherit_from_the_root_container_and_field_instance()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await BrowserReady.OpenAsync(page, $"{BaseUrl.TrimEnd('/')}/Forms");
        await Assertions.Expect(page.Locator("#spacing-counter textarea")).ToHaveValueAsync("Input");

        // Different values at every scope prove both inheritance and the closer override.
        foreach (var geometry in new[]
                 {
                     new Geometry("root", 64, 20, 28, 14, 18, 18),
                     new Geometry("container", 72, 22, 30, 10, 20, 20),
                     new Geometry("instance", 80, 24, 32, 8, 22, 22)
                 })
        {
            await page.EvaluateAsync("""
                geometry => {
                    const fields = ['spacing-value', 'spacing-icons-value', 'spacing-counter']
                        .map(id => document.getElementById(id));
                    const targets = geometry.scope === 'root' ? [document.documentElement]
                        : geometry.scope === 'container' ? [fields[0].parentElement] : fields;
                    for (const target of targets) {
                        target.style.setProperty('--dnet-form-field-min-height', `${geometry.height}px`);
                        target.style.setProperty('--dnet-form-field-padding-inline', `${geometry.padding}px`);
                        target.style.setProperty('--dnet-form-field-icon-size', `${geometry.iconSize}px`);
                        target.style.setProperty('--dnet-form-field-icon-inset', `${geometry.iconInset}px`);
                        target.style.setProperty('--dnet-form-field-icon-gap', `${geometry.iconGap}px`);
                        target.style.setProperty('--dnet-form-field-font-size', `${geometry.fontSize}px`);
                    }
                }
                """, new
            {
                scope = geometry.Scope, height = geometry.Height, padding = geometry.Padding,
                iconSize = geometry.IconSize, iconInset = geometry.IconInset,
                iconGap = geometry.IconGap, fontSize = geometry.FontSize
            });

            var plain = page.Locator("#spacing-value");
            var plainControl = await BoxAsync(plain.Locator(".dnet-form-field-plain-control-container"));
            var plainInput = await BoxAsync(plain.Locator("input"));
            Near(geometry.Height, plainControl.Height, $"{geometry.Scope} min-height");
            Near(geometry.Padding, plainInput.X - plainControl.X, $"{geometry.Scope} plain text start padding");
            Near(geometry.Padding, plainControl.Right - plainInput.Right, $"{geometry.Scope} plain text end padding");
            await Assertions.Expect(plain.Locator("input")).ToHaveCSSAsync("font-size", $"{geometry.FontSize}px");

            var icons = page.Locator("#spacing-icons-value");
            var control = await BoxAsync(icons.Locator(".dnet-form-field-plain-control-container"));
            var prefix = await BoxAsync(icons.Locator(".dnet-form-field-plain-prefix"));
            var clear = await BoxAsync(icons.Locator(".dnet-component-icon-wrapper"));
            var text = await BoxAsync(icons.Locator("input"));
            Near(geometry.Height, control.Height, $"{geometry.Scope} icon field min-height");
            Near(geometry.IconSize, prefix.Width, $"{geometry.Scope} leading icon width");
            Near(geometry.IconSize, prefix.Height, $"{geometry.Scope} leading icon height");
            Near(geometry.IconSize, clear.Width, $"{geometry.Scope} trailing icon width");
            Near(geometry.IconInset, prefix.X - control.X, $"{geometry.Scope} leading icon inset");
            Near(geometry.IconInset, control.Right - clear.Right, $"{geometry.Scope} trailing icon inset");
            Near(geometry.IconGap, text.X - prefix.Right, $"{geometry.Scope} leading icon gap");
            Near(geometry.IconGap, clear.X - text.Right, $"{geometry.Scope} trailing icon gap");
            await Assertions.Expect(icons.Locator("input")).ToHaveCSSAsync("font-size", $"{geometry.FontSize}px");

            var support = page.Locator("#spacing-counter");
            var supportControl = await BoxAsync(support.Locator(".dnet-form-field-plain-control-container"));
            var hint = await BoxAsync(support.Locator(".dnet-hint"));
            var counter = await BoxAsync(support.Locator(".dnet-form-field-char-counter"));
            Near(geometry.Padding, hint.X - supportControl.X, $"{geometry.Scope} supporting text padding");
            Near(geometry.Padding, supportControl.Right - counter.Right, $"{geometry.Scope} counter padding");
        }
    }

    private static Task WaitForLabelTopAsync(IPage page, string id, double top) => page.WaitForFunctionAsync(
        "args => { const field = document.getElementById(args.id); const control = field.querySelector('.dnet-form-field-plain-control-container').getBoundingClientRect(); const label = field.querySelector('.dnet-form-field-plain-label').getBoundingClientRect(); return Math.abs(label.y - control.y - args.top) <= 0.8; }",
        new { id, top });

    private static async Task AssertLabelAsync(ILocator field, double start, double top, double height)
    {
        var control = await BoxAsync(field.Locator(".dnet-form-field-plain-control-container"));
        var label = await BoxAsync(field.Locator(".dnet-form-field-plain-label"));
        Near(start, label.X - control.X, "Label start inset");
        Near(top, label.Y - control.Y, "Label top inset");
        Near(height, label.Height, "Label line height");
    }

    private static async Task AssertInputAsync(ILocator field, double start, double end)
    {
        var control = await BoxAsync(field.Locator(".dnet-form-field-plain-control-container"));
        var input = await BoxAsync(field.Locator("input, textarea"));
        Near(start, input.X - control.X, "Input start inset");
        Near(end, control.Right - input.Right, "Input end inset");
        Near(24, input.Y - control.Y, "Input top inset");
        Near(24, input.Height, "Input line height");
        Near(8, control.Bottom - input.Bottom, "Input bottom inset");
    }

    private static async Task<Box> BoxAsync(ILocator locator)
    {
        var values = await locator.EvaluateAsync<double[]>("element => { const r = element.getBoundingClientRect(); return [r.x, r.y, r.width, r.height]; }");
        return new(values[0], values[1], values[2], values[3]);
    }

    private static void Near(double expected, double actual, string name) =>
        Assert.True(Math.Abs(expected - actual) <= 1, $"{name}: expected {expected:0.##}px, found {actual:0.##}px.");

    private readonly record struct Box(double X, double Y, double Width, double Height)
    {
        public double Right => X + Width;
        public double Bottom => Y + Height;
    }

    private readonly record struct Geometry(string Scope, int Height, int Padding, int IconSize,
        int IconInset, int IconGap, int FontSize);
}
