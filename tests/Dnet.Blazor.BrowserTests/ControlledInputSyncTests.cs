using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ControlledInputSyncTests
{
    [Fact]
    public async Task Controlled_inputs_restore_rejected_values_and_keep_focus_when_accepted()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            // The normal solution test remains self-contained. CI and local browser
            // verification opt in after starting the sample application.
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/PickList/controlled-rejection");

        var rejected = page.Locator("#rejected-controlled-inputs");
        var checkbox = rejected.Locator("input[type=checkbox]");
        await Assertions.Expect(checkbox).Not.ToBeCheckedAsync();
        await checkbox.ClickAsync();
        await Assertions.Expect(checkbox).Not.ToBeCheckedAsync();
        await checkbox.PressAsync("Space");
        await Assertions.Expect(checkbox).Not.ToBeCheckedAsync();

        var search = rejected.Locator("input[type=search]");
        await search.FillAsync("rejected value");
        await Assertions.Expect(search).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.Locator("#controlled-probe-state"))
            .ToContainTextAsync("Selected: 0; Search:");

        await AssertTypingKeepsFocusAsync(
            page.Locator("#accepted-controlled-search input[type=search]"));
        await AssertTypingKeepsFocusAsync(
            page.Locator("#uncontrolled-search input[type=search]"));
    }

    private static async Task AssertTypingKeepsFocusAsync(ILocator search)
    {
        await search.ClickAsync();
        foreach (var character in "acc")
        {
            await search.PressAsync(character.ToString());
            Assert.True(await search.EvaluateAsync<bool>(
                "element => element === document.activeElement"));
        }

        await Assertions.Expect(search).ToHaveValueAsync("acc");
    }
}
