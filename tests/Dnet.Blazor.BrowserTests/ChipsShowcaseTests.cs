using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ChipsShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Choices_survive_page_renders_and_removed_teammates_can_be_restored(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 1000 }
        });
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
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{baseUrl}/Chips");
        await Assertions.Expect(page.Locator("#demo-chips")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#chips-clear-topics")).ToBeDisabledAsync();
        await CheckLayoutAsync(page);

        ILocator Topic(string name) => page.Locator("#chips-topics")
            .GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
        ILocator Priority(string name) => page.Locator("#chips-priority")
            .GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

        await Topic("Design").ClickAsync();
        await Topic("Engineering").FocusAsync();
        await Topic("Engineering").PressAsync("Enter");
        await Assertions.Expect(page.Locator("#chips-topics-value")).ToHaveTextAsync("2 selected · Design, Engineering");

        // Updating another card re-renders every chip. Both selections must remain.
        await Priority("Normal").ClickAsync();
        await Priority("High").FocusAsync();
        await Priority("High").PressAsync("Space");
        await Assertions.Expect(Priority("High")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(Priority("Normal")).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(Topic("Design")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(Topic("Engineering")).ToHaveAttributeAsync("aria-pressed", "true");
        await Priority("High").PressAsync("Enter");
        await Assertions.Expect(page.Locator("#chips-priority-value")).ToHaveTextAsync("No priority selected");

        // Clearing can leave older component references inside the list's API.
        // A subsequent selection must contain only the choices currently pressed.
        await page.Locator("#chips-clear-topics").ClickAsync();
        await Assertions.Expect(Topic("Design")).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(Topic("Engineering")).ToHaveAttributeAsync("aria-pressed", "false");
        await Topic("Operations").ClickAsync();
        await Assertions.Expect(page.Locator("#chips-topics-value")).ToHaveTextAsync("1 selected · Operations");
        await Topic("Design").FocusAsync();
        await Topic("Design").PressAsync("Space");
        await Topic("Operations").ClickAsync();
        await Assertions.Expect(page.Locator("#chips-topics-value")).ToHaveTextAsync("1 selected · Design");

        var people = page.Locator("#chips-people .dnet-chip");
        await Assertions.Expect(people).ToHaveCountAsync(3);
        foreach (var name in new[] { "Taylor Morgan", "Sam Chen", "Alex Rivera" })
            await people.Filter(new() { HasText = name }).Locator(".dnet-chip-remove").ClickAsync();
        await Assertions.Expect(people).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#chips-people-empty")).ToBeVisibleAsync();
        await page.Locator("#chips-restore-team").ClickAsync();
        await Assertions.Expect(people).ToHaveCountAsync(3);
        await people.Filter(new() { HasText = "Taylor Morgan" }).Locator(".dnet-chip-remove").ClickAsync();
        await Assertions.Expect(people).ToHaveCountAsync(2);
        await Assertions.Expect(page.Locator("#chips-people-value")).ToHaveTextAsync("2 teammates in this project");
        await Assertions.Expect(Topic("Design")).ToHaveAttributeAsync("aria-pressed", "true");

        var disabled = page.Locator("#chips-appearance .dnet-chip").Filter(new() { HasText = "Managed label" });
        await Assertions.Expect(disabled.Locator(".dnet-chip-remove")).ToBeDisabledAsync();
        await disabled.ClickAsync(new() { Force = true });
        await Assertions.Expect(disabled).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(disabled).ToHaveAttributeAsync("tabindex", "-1");
        await Assertions.Expect(page.Locator("#chips-sizes .dnet-chip")).ToHaveCountAsync(4);
        await CheckLayoutAsync(page);
        Assert.Empty(errors);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-chips").EvaluateAsync<string[]>(
            """
            root => {
                const issues = [];
                if (document.documentElement.scrollWidth > innerWidth + 1 || root.scrollWidth > root.clientWidth + 1)
                    issues.push('page overflows horizontally');
                for (const card of root.querySelectorAll('[data-demo-card]')) {
                    const outer = card.getBoundingClientRect();
                    for (const chip of card.querySelectorAll('.dnet-chip')) {
                        const rect = chip.getBoundingClientRect();
                        if (rect.left < outer.left - 1 || rect.right > outer.right + 1)
                            issues.push(`${chip.textContent.trim()}: chip extends outside its card`);
                    }
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }
}
