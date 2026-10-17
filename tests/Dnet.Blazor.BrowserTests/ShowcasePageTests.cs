using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ShowcasePageTests
{
    private static readonly string[] Routes =
    [
        "Dialog", "Select", "ConnectedPanel", "FloatingPanel", "DatePicker", "TimePicker",
        "Tabs", "Stepper", "DynamicStepper", "Toast", "Tooltip", "Spinner", "List", "Tree",
        "Expansion", "RadioButton", "CheckBox", "SlideToggle", "Chips", "PickList", "DoubleList", "Timeline", "Spoiler", "ImageEditor"
    ];

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Component_showcases_keep_readable_cards_without_page_overflow(string theme, int width)
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
        var persons = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = persons
        }));
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");

        foreach (var route in Routes)
        {
            await page.GotoAsync($"{baseUrl}/{route}");
            var root = page.Locator("[data-demo-page]");
            await Assertions.Expect(root).ToBeVisibleAsync();
            await Assertions.Expect(root.Locator("h1")).ToHaveCountAsync(1);
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);
            await page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
            var issues = await root.EvaluateAsync<string[]>(
                """
                root => {
                    const issues = [];
                    const bounds = root.getBoundingClientRect();
                    if (document.documentElement.scrollWidth > innerWidth + 1)
                        issues.push('application shell overflows horizontally');
                    if (bounds.left < -1 || bounds.right > innerWidth + 1 || root.scrollWidth > root.clientWidth + 1)
                        issues.push('page overflows horizontally');
                    const cards = [...root.querySelectorAll('[data-demo-card]')];
                    if (cards.length < 2) issues.push('expected at least two example cards');
                    const boxes = cards.map(card => ({ card, box: card.getBoundingClientRect() }));
                    for (const { card, box } of boxes) {
                        const label = card.dataset.demoCard;
                        if (box.left < bounds.left - 1 || box.right > bounds.right + 1)
                            issues.push(`${label}: card extends outside the page`);
                        if (!card.querySelector('h2')?.textContent.trim()) issues.push(`${label}: missing heading`);
                        for (const element of card.querySelectorAll('.demo-card-header, .demo-card-body, .demo-card-footer, .demo-actions')) {
                            const inner = element.getBoundingClientRect();
                            if (inner.left < box.left - 1 || inner.right > box.right + 1 || inner.bottom > box.bottom + 1)
                                issues.push(`${label}: content extends outside its card`);
                        }
                    }
                    for (let i = 0; i < boxes.length; i++) {
                        for (let j = i + 1; j < boxes.length; j++) {
                            const a = boxes[i], b = boxes[j];
                            if (a.card.contains(b.card) || b.card.contains(a.card)) continue;
                            if (Math.min(a.box.right, b.box.right) - Math.max(a.box.left, b.box.left) > 1
                                && Math.min(a.box.bottom, b.box.bottom) - Math.max(a.box.top, b.box.top) > 1)
                                issues.push(`${a.card.dataset.demoCard} overlaps ${b.card.dataset.demoCard}`);
                        }
                    }
                    return issues;
                }
                """);
            Assert.True(issues.Length == 0, $"{route}, {width}px, theme '{theme}': {string.Join("; ", issues)}");
        }

        Assert.Empty(errors);
    }
}
