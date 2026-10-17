using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class FloatingDoubleListShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Assignments_update_the_summary_survive_reopening_and_keep_the_fixed_lead(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        await ExpectCountsAsync(page, 5, 5);
        await ExpectOriginalRosterAsync(page);
        await CheckLayoutAsync(page);

        await OpenPanelAsync(page);
        await ShowListAsync(page, false, width);
        await Row(page, false, "Alex Rivera").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 4, 6);
        await Assertions.Expect(page.Locator("#floating-doublelist-roster")).ToContainTextAsync("Alex Rivera");
        await Assertions.Expect(page.Locator("#floating-doublelist-status")).Not.ToHaveTextAsync(string.Empty);
        await Assertions.Expect(Field(page)).ToContainTextAsync("Alex Rivera");

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(Panel(page)).ToHaveCountAsync(0);
        await OpenPanelAsync(page);
        await ShowListAsync(page, true, width);
        await Assertions.Expect(Row(page, true, "Alex Rivera")).ToBeVisibleAsync();
        var lead = Row(page, true, "Taylor Morgan");
        await Assertions.Expect(lead).ToHaveAttributeAsync("draggable", "false");
        await lead.Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 4, 6);
        await Assertions.Expect(page.Locator("#floating-doublelist-roster")).ToContainTextAsync("Taylor Morgan");

        await Row(page, true, "Alex Rivera").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await Assertions.Expect(page.Locator("#floating-doublelist-roster")).Not.ToContainTextAsync("Alex Rivera");
        await ShowListAsync(page, true, width);
        await Row(page, true, "Riley Parker").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 6, 4);
        await Assertions.Expect(page.Locator("#floating-doublelist-roster")).Not.ToContainTextAsync("Riley Parker");
        await CheckLayoutAsync(page);
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(Panel(page)).ToHaveCountAsync(0);

        var reset = page.Locator("#floating-doublelist-reset");
        await Assertions.Expect(reset).ToHaveClassAsync(new Regex("dnet-button-variant-outlined"));
        await reset.ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await ExpectOriginalRosterAsync(page);
        await OpenPanelAsync(page);
        await ShowListAsync(page, false, width);
        await Assertions.Expect(Row(page, false, "Alex Rivera")).ToBeVisibleAsync();
        await Assertions.Expect(Row(page, false, "Riley Parker")).ToHaveCountAsync(0);
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Search_on_each_side_preserves_membership_and_readonly_cannot_open_the_editor(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        await OpenPanelAsync(page);
        await ShowListAsync(page, false, width);
        await SearchAsync(page, false, "Alex");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(1);
        await Assertions.Expect(Row(page, false, "Alex Rivera")).ToBeVisibleAsync();
        await ExpectCountsAsync(page, 5, 5);
        await SearchAsync(page, false, "no-person-matches-this-query");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(0);
        await ExpectCountsAsync(page, 5, 5);
        await SearchAsync(page, false, string.Empty);
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(5);

        await ShowListAsync(page, true, width);
        await SearchAsync(page, true, "Riley");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(1);
        await Assertions.Expect(Row(page, true, "Riley Parker")).ToBeVisibleAsync();
        await SearchAsync(page, true, "no-person-matches-this-query");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(0);
        await ExpectCountsAsync(page, 5, 5);
        await SearchAsync(page, true, string.Empty);
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(5);
        var currentUrl = page.Url;
        await SearchInput(page, true).PressAsync("Enter");
        await Assertions.Expect(page).ToHaveURLAsync(currentUrl);

        await ShowListAsync(page, false, width);
        await SearchAsync(page, false, "Alex");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(1);
        await Row(page, false, "Alex Rivera").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 4, 6);
        await SearchAsync(page, false, string.Empty);
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(4);
        await ShowListAsync(page, true, width);
        await SearchAsync(page, true, "Alex");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(1);
        await Row(page, true, "Alex Rivera").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await SearchAsync(page, true, string.Empty);
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(5);
        await ExpectOriginalRosterAsync(page);
        await CheckLayoutAsync(page);
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(Panel(page)).ToHaveCountAsync(0);

        var readOnly = page.Locator("#floating-doublelist-readonly .dnet-plain-select");
        await Assertions.Expect(readOnly).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(readOnly).ToHaveAttributeAsync("tabindex", "-1");
        await readOnly.ClickAsync(new() { Force = true });
        await Assertions.Expect(Panel(page)).ToHaveCountAsync(0);
        await readOnly.FocusAsync();
        await readOnly.PressAsync("Enter");
        await Assertions.Expect(Panel(page)).ToHaveCountAsync(0);
        await ExpectCountsAsync(page, 5, 5);
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static ILocator Field(IPage page) => page.Locator("#floating-doublelist-field .dnet-plain-select");
    private static ILocator Panel(IPage page) => page.Locator(".dnet-connected-panel-container-dl");
    private static ILocator List(IPage page, bool assigned) => Panel(page).Locator(".dnet-list-wrapper").Nth(assigned ? 1 : 0);
    private static ILocator Rows(IPage page, bool assigned) => List(page, assigned).Locator(".dnet-list-item");
    private static ILocator Row(IPage page, bool assigned, string name) => Rows(page, assigned).Filter(new() { HasText = name });
    private static ILocator SearchInput(IPage page, bool assigned) => List(page, assigned).Locator(".dnet-list-form input[type=text]");

    private static async Task OpenPanelAsync(IPage page)
    {
        await Field(page).FocusAsync();
        await Field(page).PressAsync("Enter");
        await Assertions.Expect(Panel(page)).ToBeVisibleAsync();
        await Assertions.Expect(Field(page)).ToHaveAttributeAsync("aria-expanded", "true");
    }

    private static async Task ShowListAsync(IPage page, bool assigned, int width)
    {
        if (width <= 550 && !await List(page, assigned).IsVisibleAsync())
            await Panel(page).Locator(".dnet-split-content-btn-dl").ClickAsync();
        await Assertions.Expect(List(page, assigned)).ToBeVisibleAsync();
    }

    private static async Task SearchAsync(IPage page, bool assigned, string value)
    {
        await SearchInput(page, assigned).FillAsync(value);
        await SearchInput(page, assigned).PressAsync("End");
    }

    private static async Task ExpectCountsAsync(IPage page, int available, int assigned)
    {
        await Assertions.Expect(page.Locator("#floating-doublelist-available-count")).ToHaveTextAsync(new Regex($"\\b{available}\\b"));
        await Assertions.Expect(page.Locator("#floating-doublelist-assigned-count")).ToHaveTextAsync(new Regex($"\\b{assigned}\\b"));
    }

    private static async Task ExpectOriginalRosterAsync(IPage page)
    {
        var roster = page.Locator("#floating-doublelist-roster");
        foreach (var name in new[] { "Taylor Morgan", "Riley Parker", "Jordan Lee", "Avery Scott", "Cameron Reed" })
            await Assertions.Expect(roster).ToContainTextAsync(name);
        foreach (var name in new[] { "Alex Rivera", "Sam Chen", "Casey Brooks", "Morgan Patel", "Jamie Torres" })
            await Assertions.Expect(roster).Not.ToContainTextAsync(name);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.EvaluateAsync<string[]>(
            """
            () => {
                const issues = [];
                const root = document.getElementById('demo-floating-doublelist');
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Showcase overflows horizontally');
                const panel = document.querySelector('.dnet-connected-panel-container-dl');
                if (!panel) return issues;
                const bounds = panel.getBoundingClientRect();
                if (bounds.left < -1 || bounds.right > innerWidth + 1) issues.push('Floating panel lies outside the viewport');
                if (panel.scrollWidth > panel.clientWidth + 1) issues.push('Floating panel contents overflow horizontally');
                const contentBounds = element => {
                    const rect = element.getBoundingClientRect(), style = getComputedStyle(element);
                    const left = rect.left + parseFloat(style.borderLeftWidth) + parseFloat(style.paddingLeft);
                    const right = rect.right - parseFloat(style.borderRightWidth) - parseFloat(style.paddingRight);
                    return { left, right, width: right - left };
                };
                const fillsWidth = (element, available, label) => {
                    const rect = element.getBoundingClientRect();
                    if (Math.abs(rect.left - available.left) > 1 || Math.abs(rect.right - available.right) > 1)
                        issues.push(`${label} does not fill the available width (${rect.width.toFixed(1)} of ${available.width.toFixed(1)}px)`);
                };
                const split = panel.querySelector('.dnet-split-content-dl');
                const grid = panel.querySelector('.dnet-split-content-container-dl');
                fillsWidth(split, contentBounds(panel), 'Inner split wrapper');
                fillsWidth(grid, contentBounds(split), 'List grid');
                const available = contentBounds(grid);
                const sides = [...grid.querySelectorAll(':scope > .dnet-split-content-right-dl, :scope > .dnet-split-content-left-dl')];
                const visibleSides = sides.filter(side => side.getClientRects().length > 0 && side.getBoundingClientRect().width > 0);
                if (innerWidth <= 550) {
                    if (visibleSides.length !== 1) issues.push('Mobile panel must show one list at a time');
                    if (visibleSides.length === 1) fillsWidth(visibleSides[0], available, 'Active mobile list');
                    if (sides.some(side => !visibleSides.includes(side) && (side.offsetWidth > 0 || side.offsetHeight > 0)))
                        issues.push('Hidden mobile list still occupies layout space');
                } else if (visibleSides.length !== 2) {
                    issues.push('Desktop panel must show both lists');
                } else {
                    const first = visibleSides[0].getBoundingClientRect(), second = visibleSides[1].getBoundingClientRect();
                    if (Math.abs(first.width - second.width) > 1) issues.push('Desktop list columns have different widths');
                    if (Math.abs(first.left - available.left) > 1 || Math.abs(second.right - available.right) > 1)
                        issues.push('Desktop list columns leave unused space at the grid edges');
                    if (first.right > second.left + 1) issues.push('Desktop list columns overlap');
                }
                for (const side of visibleSides)
                    fillsWidth(side.querySelector('.dnet-list-wrapper'), contentBounds(side), 'List contents');
                for (const control of panel.querySelectorAll('input, button')) {
                    const rect = control.getBoundingClientRect();
                    if (rect.width > 0 && (rect.left < bounds.left - 1 || rect.right > bounds.right + 1))
                        issues.push(`${control.getAttribute('aria-label') || control.textContent.trim()}: control extends outside its panel`);
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }

    private static async Task<(IPage Page, List<string> Errors)> OpenAsync(IBrowser browser, string theme, int width)
    {
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true, ViewportSize = new() { Width = width, Height = 1000 } });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        page.Console += (_, message) =>
        {
            if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal))
                errors.Add(message.Text);
        };
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        await page.GotoAsync($"{baseUrl}/FloatingDoubleList");
        await Assertions.Expect(page.Locator("#demo-floating-doublelist")).ToBeVisibleAsync();
        await Assertions.Expect(Field(page)).ToBeVisibleAsync();
        return (page, errors);
    }
}
