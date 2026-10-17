using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class TimelineShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Order_progress_can_complete_and_reset_without_changing_the_other_stories(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var advance = page.Locator("#timeline-advance");
        var reset = page.Locator("#timeline-reset");

        await ExpectOrderAsync(page, 1, "Preparando tu paquete", "timeline-preparing");
        await Assertions.Expect(reset).ToBeDisabledAsync();
        await Assertions.Expect(advance).ToHaveTextAsync("Enviar paquete");
        await Assertions.Expect(page.Locator("#timeline-shipping time")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#timeline-delivery time")).ToHaveCountAsync(0);
        await ExpectOtherStoriesAsync(page);
        await CheckLayoutAsync(page);

        await advance.FocusAsync();
        await advance.PressAsync("Enter");
        await ExpectOrderAsync(page, 2, "En camino", "timeline-shipping");
        await Assertions.Expect(advance).ToBeFocusedAsync();
        await Assertions.Expect(advance).ToHaveTextAsync("Iniciar reparto");
        await Assertions.Expect(page.Locator("#timeline-shipping time")).ToHaveAttributeAsync("datetime", "2026-10-16T15:30:00+02:00");
        await Assertions.Expect(page.Locator("#timeline-preparing .dnet-timeline-status")).ToHaveTextAsync("Completado");
        await Assertions.Expect(reset).ToBeEnabledAsync();
        await ExpectOtherStoriesAsync(page);

        await reset.FocusAsync();
        await reset.PressAsync("Space");
        await ExpectOrderAsync(page, 1, "Preparando tu paquete", "timeline-preparing");
        await Assertions.Expect(page.Locator("#timeline-shipping time")).ToHaveCountAsync(0);
        await Assertions.Expect(reset).ToBeDisabledAsync();

        await advance.ClickAsync();
        await ExpectOrderAsync(page, 2, "En camino", "timeline-shipping");
        await advance.ClickAsync();
        await ExpectOrderAsync(page, 3, "En reparto", "timeline-delivery");
        await Assertions.Expect(advance).ToHaveTextAsync("Completar entrega");
        await Assertions.Expect(page.Locator("#timeline-shipping .dnet-timeline-status")).ToHaveTextAsync("Completado");
        await Assertions.Expect(page.Locator("#timeline-delivery time")).ToHaveCountAsync(0);
        await advance.ClickAsync();
        await ExpectOrderAsync(page, 4, "Pedido entregado", null);
        await Assertions.Expect(advance).ToHaveTextAsync("Pedido entregado");
        await Assertions.Expect(advance).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("#timeline-delivery time")).ToHaveAttributeAsync("datetime", "2026-10-17T10:15:00+02:00");
        await Assertions.Expect(page.Locator("#timeline-delivery .dnet-timeline-body")).ToContainTextAsync("Pedido entregado");
        await ExpectOtherStoriesAsync(page);
        await CheckLayoutAsync(page);

        await reset.ClickAsync();
        await ExpectOrderAsync(page, 1, "Preparando tu paquete", "timeline-preparing");
        await Assertions.Expect(page.Locator("#timeline-delivery time")).ToHaveCountAsync(0);
        await Assertions.Expect(advance).ToBeEnabledAsync();
        await Assertions.Expect(reset).ToBeDisabledAsync();
        await ExpectOtherStoriesAsync(page);

        var link = page.Locator("#timeline-event-link");
        await Assertions.Expect(link).ToHaveAttributeAsync("href", "/PillTabs");
        await link.FocusAsync();
        await link.PressAsync("Enter");
        await page.WaitForURLAsync("**/PillTabs");
        await page.GoBackAsync();
        await Assertions.Expect(page.Locator("#demo-timeline")).ToBeVisibleAsync();
        await ExpectOrderAsync(page, 1, "Preparando tu paquete", "timeline-preparing");
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static async Task ExpectOrderAsync(IPage page, int completed, string state, string? currentId)
    {
        var order = page.Locator("#timeline-default");
        await Assertions.Expect(order.Locator(":scope > li")).ToHaveCountAsync(4);
        await Assertions.Expect(page.Locator("#timeline-order-state")).ToHaveTextAsync(state);
        await Assertions.Expect(page.Locator("#timeline-order-progress")).ToHaveAttributeAsync("value", completed.ToString());
        await Assertions.Expect(page.Locator("#timeline-order-progress")).ToHaveAttributeAsync("max", "4");
        await Assertions.Expect(page.Locator("#timeline-order-value")).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(page.Locator("#timeline-order-value")).ToHaveTextAsync($"{completed} de 4 pasos completados · {state}");
        await Assertions.Expect(order.Locator(".dnet-timeline-status").Filter(new() { HasTextRegex = new("^Completado$") })).ToHaveCountAsync(completed);
        await Assertions.Expect(order.Locator("[aria-current=step]")).ToHaveCountAsync(currentId is null ? 0 : 1);
        if (currentId is not null)
        {
            await Assertions.Expect(page.Locator($"#{currentId}")).ToHaveAttributeAsync("aria-current", "step");
            await Assertions.Expect(page.Locator($"#{currentId} .dnet-timeline-status")).ToHaveTextAsync("En curso");
        }
        Assert.Equal(
            new[] { "Pedido confirmado", "Preparando tu paquete", "En camino", "Entrega" },
            await order.Locator(".dnet-timeline-title").AllTextContentsAsync());
    }

    private static async Task ExpectOtherStoriesAsync(IPage page)
    {
        await Assertions.Expect(page.Locator("#timeline-alternate > li")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#timeline-alternate [aria-current=step]")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#timeline-alternate-second")).ToHaveAttributeAsync("aria-current", "step");
        Assert.Equal(
            new[] { "La primera idea", "Un espacio para todos", "El siguiente capítulo" },
            await page.Locator("#timeline-alternate .dnet-timeline-title").AllTextContentsAsync());
        await Assertions.Expect(page.Locator("#timeline-alternate .dnet-timeline-opposite")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#timeline-alternate-first time")).ToHaveAttributeAsync("datetime", "2026-01-12");

        await Assertions.Expect(page.Locator("#timeline-end > li")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#timeline-end [aria-current=step]")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#timeline-activity-lucas")).ToHaveAttributeAsync("aria-current", "step");
        Assert.Equal(new[] { "A", "L", "M" }, await page.Locator("#timeline-end .dnet-timeline-bullet").AllTextContentsAsync());
        await Assertions.Expect(page.Locator("#timeline-end .dnet-timeline-bullet[aria-hidden=true]")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#timeline-activity-marina .dnet-timeline-title")).ToHaveTextAsync("Marina preparará el lanzamiento");

        await Assertions.Expect(page.Locator("#timeline-rtl > li")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#timeline-rtl")).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(page.Locator("#timeline-rtl [aria-current=step]")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#timeline-rtl-review")).ToHaveAttributeAsync("aria-current", "step");
        Assert.Equal(
            new[] { "timeline-rtl-received", "timeline-rtl-review", "timeline-rtl-resolution" },
            await page.Locator("#timeline-rtl > li").EvaluateAllAsync<string[]>("items => items.map(item => item.id)"));
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-timeline").EvaluateAsync<string[]>("""
            root => {
                const issues = [];
                const viewport = innerWidth;
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Showcase overflows horizontally');
                for (const id of ['timeline-order-card', 'timeline-project-card', 'timeline-activity-card', 'timeline-direction-card']) {
                    const card = document.getElementById(id), rect = card.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > viewport + 1) issues.push(`${id} is outside viewport`);
                }
                for (const timeline of root.querySelectorAll('.dnet-timeline')) {
                    const rail = timeline.getBoundingClientRect();
                    for (const item of timeline.querySelectorAll(':scope > li')) {
                        for (const child of item.querySelectorAll(':scope > .dnet-timeline-bullet, :scope > .dnet-timeline-content, :scope > .dnet-timeline-opposite')) {
                            const rect = child.getBoundingClientRect();
                            if (rect.left < rail.left - 1 || rect.right > rail.right + 1) issues.push(`${timeline.id} has clipped event content`);
                        }
                    }
                }
                const first = id => document.querySelector(`#${id} > li`);
                const bullet = item => item.querySelector('.dnet-timeline-bullet').getBoundingClientRect();
                const content = item => item.querySelector('.dnet-timeline-content').getBoundingClientRect();
                const start = first('timeline-default'), end = first('timeline-end'), rtl = first('timeline-rtl');
                if (bullet(start).right >= content(start).left) issues.push('Start rail is not before content');
                if (bullet(end).left <= content(end).right) issues.push('End rail is not after content');
                if (getComputedStyle(document.getElementById('timeline-rtl')).direction !== 'rtl' || bullet(rtl).left <= content(rtl).right) issues.push('RTL rail is not mirrored');
                const alternate = document.querySelectorAll('#timeline-alternate > li');
                const a = content(alternate[0]), b = content(alternate[1]);
                if (viewport > 640 && a.left <= b.right) issues.push('Alternate events do not occupy opposite sides');
                if (viewport <= 640 && Math.abs(a.left - b.left) > 1) issues.push('Alternate events do not stack on mobile');
                if (viewport <= 640) {
                    for (const item of alternate) {
                        const metadata = item.querySelector('.dnet-timeline-opposite').getBoundingClientRect();
                        if (metadata.top < content(item).bottom - 1) issues.push('Mobile milestone metadata overlaps its event');
                    }
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join(Environment.NewLine, issues));
    }

    private static async Task<(IPage Page, List<string> Errors)> OpenAsync(IBrowser browser, string theme, int width)
    {
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
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
        await BrowserReady.OpenAsync(page, $"{baseUrl}/Timeline");
        await Assertions.Expect(page.Locator("#demo-timeline")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#timeline-advance")).ToBeEnabledAsync();
        return (page, errors);
    }
}
