using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class FormValidationPageTests
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
    public async Task Nested_address_validation_controls_submission_and_reset_without_overlapping_fields(
        string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        var pageErrors = new List<string>();
        page.PageError += (_, error) => pageErrors.Add(error);
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{BaseUrl}/FormValidation");
        await Assertions.Expect(page.Locator("#validation-page")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);

        var form = page.Locator("#validation-form");
        var save = page.Locator("#validation-save");
        var errors = form.Locator(".validation-message");
        var firstName = page.Locator("#validation-first-name");
        var postal0 = page.Locator("#validation-address-0-postal");
        var postal1 = page.Locator("#validation-address-1-postal");
        await Assertions.Expect(save).ToHaveAttributeAsync("type", "submit");
        await Assertions.Expect(save).ToBeDisabledAsync();
        await Assertions.Expect(firstName).ToHaveValueAsync("John");
        await Assertions.Expect(page.Locator("#validation-last-name")).ToHaveValueAsync("Doe");
        await Assertions.Expect(page.Locator("#validation-job-title")).ToHaveValueAsync("Software developer");
        await Assertions.Expect(errors).ToHaveCountAsync(2);
        await ExpectRequiredErrorAsync(page, "validation-address-0-postal");
        await ExpectRequiredErrorAsync(page, "validation-address-1-postal");
        await Assertions.Expect(postal0).ToHaveAttributeAsync("aria-describedby", "validation-address-0-postal-help");
        await Assertions.Expect(postal1).ToHaveAttributeAsync("aria-describedby", "validation-address-1-postal-help");
        await Assertions.Expect(page.Locator("#validation-summary")).ToBeVisibleAsync();
        await AssertLayoutAsync(page, width);

        // Error links must stay on this route and focus the field they describe.
        await page.Locator("#validation-summary a[href$='#validation-address-0-postal']").ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync($"{BaseUrl}/FormValidation#validation-address-0-postal");
        await Assertions.Expect(postal0).ToBeFocusedAsync();
        await Assertions.Expect(firstName).ToHaveValueAsync("John");
        await Assertions.Expect(errors).ToHaveCountAsync(2);
        await Assertions.Expect(save).ToBeDisabledAsync();

        // Each address has its own FieldIdentifier: fixing one must leave the other invalid.
        await postal0.FillAsync("28001");
        await postal0.BlurAsync();
        await Assertions.Expect(errors).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#validation-address-0-postal-help .validation-message")).ToHaveCountAsync(0);
        await ExpectRequiredErrorAsync(page, "validation-address-1-postal");
        await Assertions.Expect(save).ToBeDisabledAsync();

        await postal1.FillAsync("08001");
        await postal1.BlurAsync();
        await Assertions.Expect(errors).ToHaveCountAsync(0);
        await Assertions.Expect(save).ToBeEnabledAsync();

        await firstName.FillAsync("");
        await firstName.BlurAsync();
        await Assertions.Expect(errors).ToHaveCountAsync(1);
        await ExpectRequiredErrorAsync(page, "validation-first-name");
        await Assertions.Expect(save).ToBeDisabledAsync();
        await AssertLayoutAsync(page, width);

        await firstName.FillAsync("Jane");
        await firstName.BlurAsync();
        await Assertions.Expect(errors).ToHaveCountAsync(0);
        await Assertions.Expect(save).ToBeEnabledAsync();
        await save.ClickAsync();
        var status = page.Locator("#validation-status");
        await Assertions.Expect(status).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(status).ToContainTextAsync(new Regex("Profile saved", RegexOptions.IgnoreCase));
        await Assertions.Expect(firstName).ToHaveValueAsync("Jane");
        await Assertions.Expect(postal0).ToHaveValueAsync("28001");
        await Assertions.Expect(postal1).ToHaveValueAsync("08001");
        await AssertLayoutAsync(page, width);

        await page.Locator("#validation-reset").ClickAsync();
        await Assertions.Expect(firstName).ToHaveValueAsync("John");
        await Assertions.Expect(postal0).ToHaveValueAsync("");
        await Assertions.Expect(postal1).ToHaveValueAsync("");
        await Assertions.Expect(errors).ToHaveCountAsync(2);
        await ExpectRequiredErrorAsync(page, "validation-address-0-postal");
        await ExpectRequiredErrorAsync(page, "validation-address-1-postal");
        await Assertions.Expect(save).ToBeDisabledAsync();
        await page.WaitForFunctionAsync(
            "() => !document.getElementById('validation-status')?.textContent?.trim()");
        Assert.Empty(pageErrors);
    }

    private static Task ExpectRequiredErrorAsync(IPage page, string inputId) =>
        Assertions.Expect(page.Locator($"#{inputId}-help .validation-message"))
            .ToContainTextAsync(new Regex("required", RegexOptions.IgnoreCase));

    private static async Task AssertLayoutAsync(IPage page, int width)
    {
        var layout = await page.EvaluateAsync<LayoutAudit>(
            """
            () => {
                const root = document.getElementById('validation-page');
                const form = document.getElementById('validation-form');
                const rootRect = root.getBoundingClientRect();
                const ids = [
                    'validation-first-name', 'validation-last-name', 'validation-job-title',
                    'validation-address-0-city', 'validation-address-0-town', 'validation-address-0-postal',
                    'validation-address-1-city', 'validation-address-1-town', 'validation-address-1-postal'
                ];
                const issues = [];
                const boxes = [];
                const within = (inner, outer) => inner.left >= outer.left - 0.8
                    && inner.right <= outer.right + 0.8 && inner.top >= outer.top - 0.8
                    && inner.bottom <= outer.bottom + 0.8;
                for (const id of ids) {
                    const input = document.getElementById(id);
                    if (!input) { issues.push(`${id}: input missing`); continue; }
                    const field = input.closest('.dnet-form-field-plain-wrapper');
                    const control = field?.querySelector('.dnet-form-field-plain-control-container');
                    if (!field || !control) { issues.push(`${id}: form field missing`); continue; }
                    const fieldRect = field.getBoundingClientRect();
                    const controlRect = control.getBoundingClientRect();
                    const inputRect = input.getBoundingClientRect();
                    const fieldBlock = { left: fieldRect.left, right: fieldRect.right, top: fieldRect.top, bottom: fieldRect.bottom };
                    if (!within(controlRect, fieldRect)) issues.push(`${id}: control extends outside its field`);
                    if (!within(inputRect, controlRect)) issues.push(`${id}: input extends outside its control`);
                    if (fieldRect.left < rootRect.left - 0.8 || fieldRect.right > rootRect.right + 0.8)
                        issues.push(`${id}: field extends outside the page`);
                    const help = document.getElementById(`${id}-help`);
                    if (help?.textContent.trim()) {
                        const helpRect = help.getBoundingClientRect();
                        if (helpRect.height > 0 && helpRect.top < controlRect.bottom - 0.8)
                            issues.push(`${id}: error overlaps the control`);
                        if (helpRect.height > 0) {
                            fieldBlock.left = Math.min(fieldBlock.left, helpRect.left);
                            fieldBlock.right = Math.max(fieldBlock.right, helpRect.right);
                            fieldBlock.bottom = Math.max(fieldBlock.bottom, helpRect.bottom);
                            if (helpRect.left < rootRect.left - 0.8 || helpRect.right > rootRect.right + 0.8)
                                issues.push(`${id}: error extends outside the page`);
                        }
                    }
                    boxes.push({ id, rect: fieldBlock, element: field });
                }
                for (const id of ['validation-save', 'validation-reset', 'validation-summary', 'validation-status']) {
                    const element = document.getElementById(id);
                    if (!element || getComputedStyle(element).display === 'none') continue;
                    const rect = element.getBoundingClientRect();
                    if (rect.width <= 0 || rect.height <= 0) continue;
                    boxes.push({ id, rect, element });
                    if (rect.left < rootRect.left - 0.8 || rect.right > rootRect.right + 0.8)
                        issues.push(`${id}: element extends outside the page`);
                }
                const overview = document.getElementById('validation-overview');
                if (!overview) issues.push('validation-overview: card missing');
                else {
                    const overviewRect = overview.getBoundingClientRect();
                    for (const child of overview.querySelectorAll('.validation-checklist li, #validation-summary')) {
                        const style = getComputedStyle(child);
                        if (style.display === 'none' || style.visibility === 'hidden') continue;
                        const rect = child.getBoundingClientRect();
                        if (rect.width > 0 && rect.height > 0 && !within(rect, overviewRect))
                            issues.push(`${child.id || child.textContent.trim()}: extends outside the validation overview card`);
                    }
                }
                for (let i = 0; i < boxes.length; i++) {
                    for (let j = i + 1; j < boxes.length; j++) {
                        const a = boxes[i], b = boxes[j];
                        if (a.element.contains(b.element) || b.element.contains(a.element)) continue;
                        const overlapX = Math.min(a.rect.right, b.rect.right) - Math.max(a.rect.left, b.rect.left);
                        const overlapY = Math.min(a.rect.bottom, b.rect.bottom) - Math.max(a.rect.top, b.rect.top);
                        if (overlapX > 0.8 && overlapY > 0.8) issues.push(`${a.id} overlaps ${b.id}`);
                    }
                }
                return {
                    left: rootRect.left, right: rootRect.right,
                    clientWidth: root.clientWidth, scrollWidth: root.scrollWidth,
                    formClientWidth: form.clientWidth, formScrollWidth: form.scrollWidth,
                    issues
                };
            }
            """);

        Assert.InRange(layout.Left, -1, width);
        Assert.InRange(layout.Right, 1, width + 1);
        Assert.True(layout.ScrollWidth <= layout.ClientWidth + 1,
            $"Validation page overflows horizontally: {layout.ScrollWidth}px inside {layout.ClientWidth}px.");
        Assert.True(layout.FormScrollWidth <= layout.FormClientWidth + 1,
            $"Validation form overflows horizontally: {layout.FormScrollWidth}px inside {layout.FormClientWidth}px.");
        Assert.True(layout.Issues.Length == 0, string.Join(Environment.NewLine, layout.Issues));
    }

    private sealed class LayoutAudit
    {
        public double Left { get; init; }
        public double Right { get; init; }
        public double ClientWidth { get; init; }
        public double ScrollWidth { get; init; }
        public double FormClientWidth { get; init; }
        public double FormScrollWidth { get; init; }
        public string[] Issues { get; init; } = [];
    }
}
