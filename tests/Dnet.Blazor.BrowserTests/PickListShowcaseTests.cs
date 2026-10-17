using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class PickListShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Local_choices_survive_search_and_pages_and_clear_removes_hidden_choices(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var local = page.Locator("#picklist-local");
        var counter = local.Locator(".dnet-pick-list-counter");
        var summary = page.Locator("#picklist-local-value .demo-value");
        var search = local.GetByRole(AriaRole.Searchbox, new() { Name = "Search", Exact = true });

        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 2 / 7");
        await Assertions.Expect(Toggle(local, "Alicante Thunder")).ToBeCheckedAsync();
        await Assertions.Expect(Toggle(local, "Cádiz Sharks")).ToBeCheckedAsync();
        await CheckLayoutAsync(page);

        await local.GetByRole(AriaRole.Button, new() { Name = "Select page", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 4 / 7");
        await local.GetByRole(AriaRole.Button, new() { Name = "Select page", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 4 / 7");

        var next = local.GetByRole(AriaRole.Button, new() { Name = "Next page", Exact = true });
        await next.FocusAsync();
        await next.PressAsync("Enter");
        await Assertions.Expect(local.Locator(".dnet-pick-list-item")).ToHaveCountAsync(3);
        await Toggle(local, "Ebro Wolves").FocusAsync();
        await Toggle(local, "Ebro Wolves").PressAsync("Space");
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 5 / 7");

        await search.FillAsync("Zaragoza");
        await Assertions.Expect(local.Locator(".dnet-pick-list-item")).ToHaveCountAsync(1);
        await Assertions.Expect(Toggle(local, "Ebro Wolves")).ToBeCheckedAsync();
        await Assertions.Expect(summary).ToContainTextAsync("Alicante Thunder");
        await Assertions.Expect(summary).ToContainTextAsync("Cádiz Sharks");
        await Assertions.Expect(summary).ToContainTextAsync("Ebro Wolves");

        await search.FillAsync("no-team-matches-this-query");
        await Assertions.Expect(local.Locator(".dnet-pick-list-status")).ToHaveTextAsync("No results");
        await Assertions.Expect(local.Locator("input[type=checkbox]")).ToHaveCountAsync(0);
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 5 / 7");
        await Assertions.Expect(local.GetByRole(AriaRole.Button, new() { Name = "Select page", Exact = true })).ToBeDisabledAsync();

        await local.GetByRole(AriaRole.Button, new() { Name = "Clear", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 0 / 7");
        await Assertions.Expect(summary).ToHaveTextAsync("No teams selected");
        await Assertions.Expect(local.GetByRole(AriaRole.Button, new() { Name = "Clear", Exact = true })).ToBeDisabledAsync();
        await search.FillAsync(string.Empty);
        await Assertions.Expect(Toggle(local, "Alicante Thunder")).Not.ToBeCheckedAsync();
        await Assertions.Expect(Toggle(local, "Cádiz Sharks")).Not.ToBeCheckedAsync();
        await next.ClickAsync();
        await Assertions.Expect(Toggle(local, "Ebro Wolves")).Not.ToBeCheckedAsync();
        await CheckLayoutAsync(page);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Provider_keeps_unloaded_choices_and_page_position_when_another_example_changes(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var provider = page.Locator("#picklist-provider");
        var counter = provider.Locator(".dnet-pick-list-counter");
        var summary = page.Locator("#picklist-provider-value .demo-value");
        var search = provider.GetByRole(AriaRole.Searchbox, new() { Name = "Search", Exact = true });

        await Assertions.Expect(provider.Locator(".dnet-pick-list-item")).ToHaveCountAsync(3);
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 2 / 7");
        await Assertions.Expect(Toggle(provider, "Bilbao Titans")).ToBeCheckedAsync();
        await Assertions.Expect(summary).ToHaveTextAsync("Bilbao Titans, Archived team");

        await search.FillAsync("Zaragoza");
        await Assertions.Expect(provider.Locator(".dnet-pick-list-item")).ToHaveCountAsync(1);
        await Assertions.Expect(Toggle(provider, "Ebro Wolves")).Not.ToBeCheckedAsync();
        var selectPage = provider.GetByRole(AriaRole.Button, new() { Name = "Select page", Exact = true });
        await selectPage.FocusAsync();
        await selectPage.PressAsync("Enter");
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 3 / 7");
        await Assertions.Expect(summary).ToHaveTextAsync("Bilbao Titans, Ebro Wolves, Archived team");

        await search.FillAsync(string.Empty);
        await Assertions.Expect(provider.Locator(".dnet-pick-list-item")).ToHaveCountAsync(3);
        await provider.GetByRole(AriaRole.Button, new() { Name = "Next page", Exact = true }).ClickAsync();
        await Assertions.Expect(Toggle(provider, "Ebro Wolves")).ToBeCheckedAsync();
        await provider.GetByRole(AriaRole.Button, new() { Name = "Last page", Exact = true }).ClickAsync();
        await Assertions.Expect(provider.Locator(".dnet-pick-list-item")).ToHaveCountAsync(1);
        await Assertions.Expect(Toggle(provider, "Faro Eagles")).Not.ToBeCheckedAsync();

        // A parent render from another card must keep this provider's window and shortlist.
        await Toggle(page.Locator("#picklist-local"), "Alicante Thunder").UncheckAsync();
        await Assertions.Expect(page.Locator("#picklist-local-value .demo-value")).ToHaveTextAsync("Cádiz Sharks");
        await Assertions.Expect(provider.Locator(".dnet-paginator-page-input")).ToHaveValueAsync("3");
        await Assertions.Expect(Toggle(provider, "Faro Eagles")).ToBeVisibleAsync();
        await Assertions.Expect(summary).ToHaveTextAsync("Bilbao Titans, Ebro Wolves, Archived team");

        await search.FillAsync("no-team-matches-this-query");
        await Assertions.Expect(provider.Locator(".dnet-pick-list-status")).ToHaveTextAsync("No results");
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 3 / 7");
        await provider.GetByRole(AriaRole.Button, new() { Name = "Clear", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Selected: 0 / 7");
        await Assertions.Expect(summary).ToHaveTextAsync("No teams selected");
        await Assertions.Expect(page.Locator("#picklist-provider-note")).Not.ToContainTextAsync("archived");
        await search.FillAsync(string.Empty);
        await Assertions.Expect(Toggle(provider, "Bilbao Titans")).Not.ToBeCheckedAsync();
        await CheckLayoutAsync(page);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Localized_actions_and_keyboard_keep_the_protected_team_selected(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var localized = page.Locator("#picklist-localized");
        var counter = localized.Locator(".dnet-pick-list-counter");
        var search = localized.GetByRole(AriaRole.Searchbox, new() { Name = "Buscar equipos", Exact = true });
        var protectedTeam = Toggle(localized, "Alicante Thunder");

        await Assertions.Expect(protectedTeam).ToBeCheckedAsync();
        await Assertions.Expect(protectedTeam).ToBeDisabledAsync();
        await Assertions.Expect(Row(localized, "Alicante Thunder")).ToContainTextAsync("Fijo");
        await Assertions.Expect(localized.GetByRole(AriaRole.Navigation, new() { Name = "Paginación de equipos", Exact = true })).ToBeVisibleAsync();
        await protectedTeam.EvaluateAsync("element => element.click()");
        await Toggle(localized, "Bilbao Titans").FocusAsync();
        await Toggle(localized, "Bilbao Titans").PressAsync("Space");
        await Assertions.Expect(counter).ToHaveTextAsync("Seleccionados: 2 / 7");

        await localized.GetByRole(AriaRole.Button, new() { Name = "Seleccionar página", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Seleccionados: 4 / 7");
        var next = localized.GetByRole(AriaRole.Button, new() { Name = "Página siguiente", Exact = true });
        await next.FocusAsync();
        await next.PressAsync("Enter");
        await Assertions.Expect(localized.Locator(".dnet-pick-list-item")).ToHaveCountAsync(3);
        await localized.GetByRole(AriaRole.Button, new() { Name = "Seleccionar página", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Seleccionados: 7 / 7");

        await search.FillAsync("Huelva");
        await Assertions.Expect(localized.Locator(".dnet-pick-list-item")).ToHaveCountAsync(1);
        await Assertions.Expect(Toggle(localized, "Faro Eagles")).ToBeCheckedAsync();
        await localized.GetByRole(AriaRole.Button, new() { Name = "Limpiar", Exact = true }).ClickAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Seleccionados: 1 / 7");
        await Assertions.Expect(page.Locator("#picklist-localized-value .demo-value")).ToHaveTextAsync("Alicante Thunder");
        await Assertions.Expect(Toggle(localized, "Faro Eagles")).Not.ToBeCheckedAsync();

        await search.FillAsync("no-team-matches-this-query");
        await Assertions.Expect(localized.Locator(".dnet-pick-list-status")).ToHaveTextAsync("No se encontraron equipos");
        await Assertions.Expect(localized.GetByRole(AriaRole.Button, new() { Name = "Seleccionar página", Exact = true })).ToBeDisabledAsync();
        await Assertions.Expect(counter).ToHaveTextAsync("Seleccionados: 1 / 7");
        await search.FillAsync(string.Empty);
        await Assertions.Expect(search).ToBeFocusedAsync();
        await Assertions.Expect(protectedTeam).ToBeCheckedAsync();
        await Assertions.Expect(protectedTeam).ToBeDisabledAsync();
        await Assertions.Expect(Toggle(localized, "Bilbao Titans")).Not.ToBeCheckedAsync();
        await CheckLayoutAsync(page);
        Assert.Empty(errors);
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static ILocator Row(ILocator card, string name) => card.Locator(".dnet-pick-list-item")
        .Filter(new() { HasText = name });

    private static ILocator Toggle(ILocator card, string name) => Row(card, name).Locator("input[type=checkbox]");

    private static async Task<(IPage Page, List<string> Errors)> OpenAsync(IBrowser browser, string theme, int width)
    {
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
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        await BrowserLanguage.UseEnglishAsync(page);
        await page.GotoAsync($"{baseUrl}/PickList");
        await Assertions.Expect(page.Locator("#demo-picklist")).ToBeVisibleAsync();
        return (page, errors);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-picklist").EvaluateAsync<string[]>(
            """
            root => {
                const issues = [];
                if (root.scrollWidth > root.clientWidth + 1)
                    issues.push('page overflows horizontally');
                for (const list of root.querySelectorAll('.dnet-pick-list')) {
                    const outer = list.getBoundingClientRect();
                    if (list.scrollWidth > list.clientWidth + 1)
                        issues.push('list contents overflow horizontally');
                    for (const control of list.querySelectorAll('input, button')) {
                        const rect = control.getBoundingClientRect();
                        if (rect.width > 0 && (rect.left < outer.left - 1 || rect.right > outer.right + 1))
                            issues.push(`${control.getAttribute('aria-label') || control.textContent.trim()}: control extends outside its list`);
                    }
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }
}
