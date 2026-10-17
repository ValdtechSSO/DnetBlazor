using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class PillTabsShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Inbox_changes_keep_shared_counts_and_preserve_the_other_examples(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var reference = page.Locator("#pill-tabs-reference");
        var tabs = reference.GetByRole(AriaRole.Tab);
        var mirror = page.Locator("#pill-tabs-synchronized").GetByRole(AriaRole.Button);
        var settings = page.Locator("#pill-tabs-settings").GetByRole(AriaRole.Tab);
        var overflow = page.Locator("#pill-tabs-overflow").GetByRole(AriaRole.Button);
        var rtl = page.Locator("#pill-tabs-rtl").GetByRole(AriaRole.Button);

        await Assertions.Expect(reference.GetByRole(AriaRole.Tablist)).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#pill-tabs-synchronized").GetByRole(AriaRole.Group)).ToHaveCountAsync(1);
        await ExpectCountsAsync(page, 1, 2, 3);
        await ExpectSelectionAsync(page, 0, "Pendientes");
        await ExpectRequestsAsync(page, "RQ-1042");
        await Assertions.Expect(Request(page, "RQ-1042")).ToHaveAttributeAsync("data-request-status", "pending");
        await Assertions.Expect(Request(page, "RQ-1042")).ToContainTextAsync("Lucía Marín");
        await CheckLayoutAsync(page);

        await tabs.Nth(0).FocusAsync();
        await tabs.Nth(0).PressAsync("ArrowRight");
        await ExpectSelectionAsync(page, 1, "Aprobados");
        await ExpectRequestsAsync(page, "RQ-1043", "RQ-1044");
        await Assertions.Expect(Request(page, "RQ-1043")).ToContainTextAsync("Diego Torres");
        await Assertions.Expect(Request(page, "RQ-1044")).ToContainTextAsync("Nora Vidal");

        await settings.Nth(0).FocusAsync();
        await settings.Nth(0).PressAsync("ArrowRight");
        await Assertions.Expect(page.Locator("#pill-tabs-setting-value")).ToHaveTextAsync("Notificaciones");
        await Assertions.Expect(settings.Nth(1).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync("0");
        await settings.Nth(1).PressAsync("ArrowRight");
        await Assertions.Expect(page.Locator("#pill-tabs-setting-value")).ToHaveTextAsync("Historial");
        await Assertions.Expect(settings.Nth(2)).ToBeDisabledAsync();
        await overflow.Nth(0).FocusAsync();
        await overflow.Nth(0).PressAsync("End");
        await rtl.Nth(0).FocusAsync();
        await rtl.Nth(0).PressAsync("ArrowLeft");
        await ExpectIndependentSelectionsAsync(page);

        await mirror.Nth(0).FocusAsync();
        await mirror.Nth(0).PressAsync("Enter");
        await ExpectSelectionAsync(page, 0, "Pendientes");
        await page.Locator("#pill-tabs-approve-RQ-1042").FocusAsync();
        await page.Locator("#pill-tabs-approve-RQ-1042").PressAsync("Enter");
        var inboxContent = page.Locator("#pill-tabs-inbox-content");
        await Assertions.Expect(inboxContent).ToBeFocusedAsync();
        Assert.True(await inboxContent.EvaluateAsync<bool>("""
            element => {
                const style = getComputedStyle(element);
                return element.matches(':focus-visible') &&
                    ((style.outlineStyle !== 'none' && parseFloat(style.outlineWidth) > 0) || style.boxShadow !== 'none');
            }
            """), "Keyboard approval must leave a visible focus indicator on the inbox content.");
        await ExpectCountsAsync(page, 0, 3, 3);
        await ExpectSelectionAsync(page, 0, "Pendientes");
        await ExpectRequestsAsync(page);
        await Assertions.Expect(page.Locator("#pill-tabs-inbox-empty")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#pill-tabs-inbox-empty")).ToContainTextAsync("Todo al día");
        await ExpectIndependentSelectionsAsync(page);

        await page.Locator("#pill-tabs-select-all").ClickAsync();
        await ExpectSelectionAsync(page, 2, "Todos");
        await ExpectRequestsAsync(page, "RQ-1042", "RQ-1043", "RQ-1044");
        await Assertions.Expect(Request(page, "RQ-1042")).ToHaveAttributeAsync("data-request-status", "approved");
        await Assertions.Expect(page.Locator("#pill-tabs-approve-RQ-1042")).ToHaveCountAsync(0);
        await page.Locator("#pill-tabs-add-pending").FocusAsync();
        await page.Locator("#pill-tabs-add-pending").PressAsync("Space");
        await ExpectCountsAsync(page, 1, 3, 4);
        await ExpectSelectionAsync(page, 2, "Todos");
        await ExpectRequestsAsync(page, "RQ-1042", "RQ-1043", "RQ-1044", "RQ-1045");
        await Assertions.Expect(Request(page, "RQ-1045")).ToHaveAttributeAsync("data-request-status", "pending");
        await Assertions.Expect(page.Locator("#pill-tabs-approve-RQ-1045")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#pill-tabs-inbox-empty")).ToHaveCountAsync(0);
        await ExpectIndependentSelectionsAsync(page);

        await mirror.Nth(1).ClickAsync();
        await ExpectSelectionAsync(page, 1, "Aprobados");
        await ExpectRequestsAsync(page, "RQ-1042", "RQ-1043", "RQ-1044");
        await Assertions.Expect(Request(page, "RQ-1045")).ToHaveCountAsync(0);
        await page.SetViewportSizeAsync(width == 1440 ? 390 : 1440, 1000);
        await ExpectSelectionAsync(page, 1, "Aprobados");
        await ExpectRequestsAsync(page, "RQ-1042", "RQ-1043", "RQ-1044");
        await ExpectIndependentSelectionsAsync(page);
        await ExpectSelectedVisibleAsync(page, "pill-tabs-settings");
        await CheckLayoutAsync(page);

        await page.Locator("#pill-tabs-reset").ClickAsync();
        await ExpectCountsAsync(page, 1, 2, 3);
        await ExpectSelectionAsync(page, 0, "Pendientes");
        await ExpectRequestsAsync(page, "RQ-1042");
        await Assertions.Expect(Request(page, "RQ-1042")).ToHaveAttributeAsync("data-request-status", "pending");
        await Assertions.Expect(page.Locator("#pill-tabs-approve-RQ-1042")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#pill-tabs-inbox-empty")).ToHaveCountAsync(0);
        await ExpectIndependentSelectionsAsync(page);
        await ExpectSelectedVisibleAsync(page, "pill-tabs-settings");
        await page.SetViewportSizeAsync(width, 1000);
        await ExpectSelectedVisibleAsync(page, "pill-tabs-settings");
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static ILocator Request(IPage page, string id) => page.Locator($"#pill-tabs-reference .pill-request[data-request-id='{id}']");

    private static async Task ExpectRequestsAsync(IPage page, params string[] ids)
    {
        var rows = page.Locator("#pill-tabs-reference .pill-request");
        await Assertions.Expect(rows).ToHaveCountAsync(ids.Length);
        Assert.Equal(ids, await rows.EvaluateAllAsync<string[]>("rows => rows.map(row => row.dataset.requestId)"));
    }

    private static async Task ExpectSelectionAsync(IPage page, int index, string label)
    {
        var reference = page.Locator("#pill-tabs-reference");
        var selected = reference.GetByRole(AriaRole.Tab).Nth(index);
        await Assertions.Expect(selected).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(reference.Locator("[aria-selected=true]")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#pill-tabs-synchronized").GetByRole(AriaRole.Button).Nth(index)).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.Locator("#pill-tabs-selected-status")).ToHaveTextAsync($"Mostrando: {label}");
        await Assertions.Expect(page.Locator("#pill-tabs-shared-value")).ToContainTextAsync($"Selección compartida: {label}");
        var panel = reference.GetByRole(AriaRole.Tabpanel);
        Assert.Equal(await selected.GetAttributeAsync("aria-controls"), await panel.GetAttributeAsync("id"));
        Assert.Equal(await selected.GetAttributeAsync("id"), await panel.GetAttributeAsync("aria-labelledby"));
    }

    private static async Task ExpectCountsAsync(IPage page, int pending, int approved, int total)
    {
        await Assertions.Expect(page.Locator("#pill-tabs-pending-total")).ToHaveTextAsync(pending.ToString());
        await Assertions.Expect(page.Locator("#pill-tabs-approved-total")).ToHaveTextAsync(approved.ToString());
        await Assertions.Expect(page.Locator("#pill-tabs-all-total")).ToHaveTextAsync(total.ToString());
        foreach (var id in new[] { "pill-tabs-reference", "pill-tabs-synchronized", "pill-tabs-overflow", "pill-tabs-rtl" })
        {
            var tabs = page.Locator($"#{id} .dnet-pill-tabs-tab");
            await Assertions.Expect(tabs.Nth(0).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync(pending.ToString());
            await Assertions.Expect(tabs.Nth(1).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync(approved.ToString());
            await Assertions.Expect(tabs.Nth(2).Locator(".dnet-pill-tabs-count")).ToHaveTextAsync(total.ToString());
        }
    }

    private static async Task ExpectIndependentSelectionsAsync(IPage page)
    {
        await Assertions.Expect(page.Locator("#pill-tabs-setting-value")).ToHaveTextAsync("Historial");
        await Assertions.Expect(page.Locator("#pill-tabs-settings").GetByRole(AriaRole.Tab).Nth(3)).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(page.Locator("#pill-tabs-overflow").GetByRole(AriaRole.Button).Nth(2)).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.Locator("#pill-tabs-rtl").GetByRole(AriaRole.Button).Nth(1)).ToHaveAttributeAsync("aria-pressed", "true");
    }

    private static async Task ExpectSelectedVisibleAsync(IPage page, string id)
    {
        await page.WaitForFunctionAsync("""
            id => {
                const list = document.getElementById(id)?.querySelector('.dnet-pill-tabs-list');
                const selected = list?.querySelector('[aria-selected="true"], [aria-pressed="true"]');
                if (!selected) return false;
                const strip = list.getBoundingClientRect(), tab = selected.getBoundingClientRect();
                return tab.left >= strip.left - 1 && tab.right <= strip.right + 1;
            }
            """, id);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-pill-tabs").EvaluateAsync<string[]>("""
            root => {
                const issues = [];
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Showcase overflows horizontally');
                for (const id of ['pill-tabs-inbox-card', 'pill-tabs-settings-card', 'pill-tabs-sync-card', 'pill-tabs-overflow-card', 'pill-tabs-direction-card']) {
                    const card = document.getElementById(id), rect = card.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > innerWidth + 1 || card.scrollWidth > card.clientWidth + 1) issues.push(`${id} overflows`);
                }
                for (const control of root.querySelectorAll('.dnet-pill-tabs')) {
                    const rect = control.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > innerWidth + 1) issues.push(`${control.id} lies outside viewport`);
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
            if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal)) errors.Add(message.Text);
        };
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        await page.GotoAsync($"{baseUrl}/PillTabs");
        await Assertions.Expect(page.Locator("#demo-pill-tabs")).ToBeVisibleAsync();
        return (page, errors);
    }
}
