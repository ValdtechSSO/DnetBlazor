using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class DoubleListShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task People_move_in_both_directions_without_moving_the_fixed_lead(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        await ExpectCountsAsync(page, 5, 5);
        await CheckLayoutAsync(page);

        await SetCheckedAsync(Checkbox(page, false, "Alex Rivera"), true);
        await Checkbox(page, false, "Sam Chen").FocusAsync();
        await Checkbox(page, false, "Sam Chen").PressAsync("Space");
        await Assertions.Expect(page.Locator("#doublelist-selected-count")).ToHaveTextAsync("2 selected");
        var assign = MoveButton(page, true, width);
        await assign.FocusAsync();
        await assign.PressAsync("Enter");
        await ExpectCountsAsync(page, 3, 7);
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Alex Rivera");
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Sam Chen");

        await ShowListAsync(page, true, width);
        await SearchAsync(page, true, "Alex");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(1);
        await SetCheckedAsync(Checkbox(page, true, "Alex Rivera"), true);
        await MoveButton(page, false, width).ClickAsync();
        await ExpectCountsAsync(page, 4, 6);
        await Assertions.Expect(page.Locator("#doublelist-roster")).Not.ToContainTextAsync("Alex Rivera");
        await Assertions.Expect(SearchInput(page, false)).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(SearchInput(page, true)).ToHaveValueAsync(string.Empty);

        await ShowListAsync(page, true, width);
        var fixedLead = Row(page, true, "Taylor Morgan");
        await Assertions.Expect(fixedLead).ToHaveAttributeAsync("draggable", "false");
        await Assertions.Expect(fixedLead.Locator("input[type=checkbox]")).ToHaveCountAsync(0);
        await fixedLead.Locator(".dnet-list-item-content").ClickAsync();
        await SetCheckedAsync(Checkbox(page, true, "Riley Parker"), true);
        await MoveButton(page, false, width).ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Taylor Morgan");
        await Assertions.Expect(page.Locator("#doublelist-roster")).Not.ToContainTextAsync("Riley Parker");

        await page.Locator("#doublelist-reset").ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await ExpectOriginalRosterAsync(page);
        await ShowListAsync(page, false, width);
        await SetCheckedAsync(Panel(page, false).Locator(".dnet-list-header-checkall input[type=checkbox]"), true);
        await Assertions.Expect(page.Locator("#doublelist-selected-count")).ToHaveTextAsync("4 selected");
        await MoveButton(page, true, width).ClickAsync();
        await ExpectCountsAsync(page, 1, 9);
        await ShowListAsync(page, false, width);
        await SetCheckedAsync(Checkbox(page, false, "Jamie Torres"), true);
        await MoveButton(page, true, width).ClickAsync();
        await ExpectCountsAsync(page, 0, 10);
        await ShowListAsync(page, false, width);
        await Assertions.Expect(page.Locator("#doublelist-available-empty")).ToBeVisibleAsync();

        await ShowListAsync(page, true, width);
        await SearchAsync(page, true, "Cameron");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(1);
        await SetCheckedAsync(Checkbox(page, true, "Cameron Reed"), true);
        await MoveButton(page, false, width).ClickAsync();
        await ExpectCountsAsync(page, 1, 9);
        await Assertions.Expect(page.Locator("#doublelist-available-empty")).ToHaveCountAsync(0);
        await page.Locator("#doublelist-reset").ClickAsync();
        await ExpectCountsAsync(page, 5, 5);
        await ExpectOriginalRosterAsync(page);
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Search_preferences_and_reset_keep_data_consistent_and_ignore_an_old_quick_move(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);

        await SearchAsync(page, false, "Alex");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(1);
        await SetCheckedAsync(Checkbox(page, false, "Alex Rivera"), true);
        await SetCheckedAsync(page.Locator("#doublelist-option-search"), false);
        await Assertions.Expect(page.Locator(".doublelist-stage .dnet-list-form")).ToHaveCountAsync(0);
        await ExpectCountsAsync(page, 5, 5);
        await SetCheckedAsync(page.Locator("#doublelist-option-checkboxes"), false);
        await Assertions.Expect(Rows(page, false).Locator("input[type=checkbox]")).ToHaveCountAsync(0);

        await ShowListAsync(page, false, width);
        await Row(page, false, "Alex Rivera").Locator(".dnet-list-item-content").ClickAsync();
        await ExpectCountsAsync(page, 4, 6);
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Alex Rivera");
        await SetCheckedAsync(page.Locator("#doublelist-option-search"), true);
        await SetCheckedAsync(page.Locator("#doublelist-option-checkboxes"), true);

        await SetCheckedAsync(page.Locator("#doublelist-option-header"), false);
        await Assertions.Expect(page.Locator("#doublelist-option-sorting")).Not.ToBeCheckedAsync();
        await Assertions.Expect(page.Locator(".doublelist-stage .dnet-list-header")).ToHaveCountAsync(0);
        await SetCheckedAsync(page.Locator("#doublelist-option-sorting"), true);
        await Assertions.Expect(page.Locator("#doublelist-option-header")).ToBeCheckedAsync();
        await ShowListAsync(page, false, width);
        await Panel(page, false).Locator(".dnet-list-header-content").ClickAsync();
        await Assertions.Expect(Rows(page, false).First.Locator(".dnet-list-item-headline")).ToHaveTextAsync("Casey Brooks");

        await SetCheckedAsync(page.Locator("#doublelist-option-footer"), false);
        await Assertions.Expect(page.Locator("#doublelist-option-pagination")).Not.ToBeCheckedAsync();
        await Assertions.Expect(page.Locator(".doublelist-stage .dnet-list-footer")).ToHaveCountAsync(0);
        await SetCheckedAsync(page.Locator("#doublelist-option-pagination"), true);
        await Assertions.Expect(page.Locator("#doublelist-option-footer")).ToBeCheckedAsync();
        await Assertions.Expect(page.Locator(".doublelist-stage .dnet-paginator")).ToHaveCountAsync(2);

        await ShowListAsync(page, false, width);
        await SearchAsync(page, false, "no-person-matches-this-query");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#doublelist-available-count")).ToHaveTextAsync("4 available");
        await Assertions.Expect(page.Locator("#doublelist-assigned-count")).ToHaveTextAsync("6 assigned");
        await SearchAsync(page, false, "Sam");
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(1);
        await Assertions.Expect(Row(page, false, "Sam Chen")).ToBeVisibleAsync();
        var currentUrl = page.Url;
        await SearchInput(page, false).PressAsync("Enter");
        await Assertions.Expect(page).ToHaveURLAsync(currentUrl);
        await Assertions.Expect(Row(page, false, "Sam Chen")).ToBeVisibleAsync();
        await SearchAsync(page, false, string.Empty);
        await Assertions.Expect(Rows(page, false)).ToHaveCountAsync(4);

        await SetCheckedAsync(page.Locator("#doublelist-option-footer"), false);
        await ShowListAsync(page, false, width);
        var pendingMove = Row(page, false, "Sam Chen");
        await pendingMove.Locator(".dnet-list-item-content").ClickAsync();
        await Assertions.Expect(pendingMove).ToHaveClassAsync(new Regex("dnet-item-fadeout"));
        await page.Locator("#doublelist-reset").EvaluateAsync("element => element.click()");
        await Assertions.Expect(page.Locator("#doublelist-action")).ToContainTextAsync("Original assignment restored");
        // Let the previous editor action finish after reset. Its old callback must be ignored.
        await page.WaitForTimeoutAsync(700);
        await ExpectCountsAsync(page, 5, 5);
        await ExpectOriginalRosterAsync(page);
        await Assertions.Expect(page.Locator("#doublelist-option-footer")).Not.ToBeCheckedAsync();
        await Assertions.Expect(page.Locator("#doublelist-option-pagination")).Not.ToBeCheckedAsync();
        await Assertions.Expect(SearchInput(page, false)).ToHaveValueAsync(string.Empty);
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Theory]
    [InlineData("")]
    [InlineData("dark")]
    public async Task Desktop_drag_moves_one_person_and_a_selection_without_duplicate_assignments(string theme)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, 1440);
        await Row(page, false, "Alex Rivera").DragToAsync(Panel(page, true).Locator(".dnet-list-items"));
        await ExpectCountsAsync(page, 4, 6);
        await Assertions.Expect(page.Locator("#doublelist-roster li").Filter(new() { HasText = "Alex Rivera" })).ToHaveCountAsync(1);
        await Panel(page, true).Locator(".dnet-list-items").EvaluateAsync(
            "element => element.dispatchEvent(new DragEvent('drop', { bubbles: true, dataTransfer: new DataTransfer() }))");
        await ExpectCountsAsync(page, 4, 6);

        await SearchAsync(page, true, "Alex");
        await Assertions.Expect(Rows(page, true)).ToHaveCountAsync(1);
        await Row(page, true, "Alex Rivera").DragToAsync(Panel(page, false).Locator(".dnet-list-items"));
        await ExpectCountsAsync(page, 5, 5);
        await Assertions.Expect(page.Locator("#doublelist-roster")).Not.ToContainTextAsync("Alex Rivera");

        await SetCheckedAsync(Checkbox(page, false, "Sam Chen"), true);
        await SetCheckedAsync(Checkbox(page, false, "Casey Brooks"), true);
        await Row(page, false, "Sam Chen").DragToAsync(Panel(page, true).Locator(".dnet-list-items"));
        await ExpectCountsAsync(page, 3, 7);
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Sam Chen");
        await Assertions.Expect(page.Locator("#doublelist-roster")).ToContainTextAsync("Casey Brooks");
        var roster = await page.Locator("#doublelist-roster li > span:first-child").AllTextContentsAsync();
        Assert.Equal(7, roster.Count);
        Assert.Equal(roster.Count, roster.Distinct(StringComparer.Ordinal).Count());
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
    private static ILocator Panel(IPage page, bool assigned) => page.Locator(assigned ? "#doublelist-assigned" : "#doublelist-available");
    private static ILocator Rows(IPage page, bool assigned) => Panel(page, assigned).Locator(".dnet-list-item");
    private static ILocator Row(IPage page, bool assigned, string name) => Rows(page, assigned).Filter(new() { HasText = name });
    private static ILocator Checkbox(IPage page, bool assigned, string name) => Row(page, assigned, name).Locator("input[type=checkbox]");
    private static ILocator SearchInput(IPage page, bool assigned) => Panel(page, assigned).Locator(".dnet-list-form input[type=text]");
    private static ILocator MoveButton(IPage page, bool assign, int width) => page.Locator(
        $"#doublelist-{(assign ? "assign" : "return")}{(width <= 1180 ? "-mobile" : string.Empty)}");

    private static async Task SetCheckedAsync(ILocator checkbox, bool value)
    {
        if (await checkbox.IsCheckedAsync() != value)
        {
            await checkbox.FocusAsync();
            await checkbox.PressAsync("Space");
        }
        if (value) await Assertions.Expect(checkbox).ToBeCheckedAsync();
        else await Assertions.Expect(checkbox).Not.ToBeCheckedAsync();
    }

    private static async Task ShowListAsync(IPage page, bool assigned, int width)
    {
        if (width <= 1180)
        {
            var tab = page.Locator(assigned ? "#doublelist-show-assigned" : "#doublelist-show-available");
            await tab.ClickAsync();
            await Assertions.Expect(tab).ToHaveAttributeAsync("aria-pressed", "true");
        }
        await Assertions.Expect(Panel(page, assigned)).ToBeVisibleAsync();
    }

    private static async Task SearchAsync(IPage page, bool assigned, string value)
    {
        var input = SearchInput(page, assigned);
        await input.FillAsync(value);
        await input.PressAsync("End");
    }

    private static async Task ExpectCountsAsync(IPage page, int available, int assigned)
    {
        await Assertions.Expect(page.Locator("#doublelist-available-count")).ToHaveTextAsync($"{available} available");
        await Assertions.Expect(page.Locator("#doublelist-assigned-count")).ToHaveTextAsync($"{assigned} assigned");
        await Assertions.Expect(page.Locator("#doublelist-selected-count")).ToHaveTextAsync("0 selected");
    }

    private static async Task ExpectOriginalRosterAsync(IPage page)
    {
        await Assertions.Expect(page.Locator("#doublelist-roster li")).ToHaveCountAsync(5);
        var actual = await page.Locator("#doublelist-roster li > span:first-child").AllTextContentsAsync();
        string[] expected = ["Taylor Morgan", "Riley Parker", "Jordan Lee", "Avery Scott", "Cameron Reed"];
        Assert.Equal(expected.OrderBy(name => name), actual.OrderBy(name => name));
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
        await page.GotoAsync($"{baseUrl}/DoubleList");
        await Assertions.Expect(page.Locator("#demo-doublelist")).ToBeVisibleAsync();
        await Assertions.Expect(Row(page, false, "Alex Rivera")).ToBeVisibleAsync();
        return (page, errors);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-doublelist").EvaluateAsync<string[]>(
            """
            root => {
                const issues = [];
                if (root.scrollWidth > root.clientWidth + 1) issues.push('page overflows horizontally');
                for (const stage of root.querySelectorAll('.doublelist-stage')) {
                    const bounds = stage.getBoundingClientRect();
                    if (bounds.width === 0) continue;
                    if (stage.scrollWidth > stage.clientWidth + 1) issues.push('list contents overflow horizontally');
                    for (const control of stage.querySelectorAll('input, button')) {
                        const rect = control.getBoundingClientRect();
                        if (rect.width > 0 && (rect.left < bounds.left - 1 || rect.right > bounds.right + 1))
                            issues.push(`${control.getAttribute('aria-label') || control.textContent.trim()}: control extends outside its list`);
                    }
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }
}
