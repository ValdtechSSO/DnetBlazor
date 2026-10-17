using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class SpoilerShowcaseTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Reading_choices_and_note_survive_other_examples_and_resize(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var article = page.Locator("#spoiler-basic");
        var details = page.Locator("#spoiler-controlled");
        var dynamic = page.Locator("#spoiler-dynamic");
        var rtl = page.Locator("#spoiler-rtl");
        var note = page.Locator("#spoiler-note");
        const string noteText = "Planear un paseo junto al río y reservar una mesa para el sábado.";

        await Assertions.Expect(page.Locator("#spoiler-controlled-state")).ToHaveTextAsync("Estado: contraído");
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync("Nota: Sin anotaciones");
        await Assertions.Expect(page.Locator("#spoiler-content-state")).ToHaveTextAsync("Descripción breve");
        await Assertions.Expect(dynamic.Locator(".dnet-spoiler-toggle")).ToBeHiddenAsync();
        await Assertions.Expect(page.Locator("#spoiler-short .dnet-spoiler-toggle")).ToBeHiddenAsync();
        await Assertions.Expect(rtl).ToHaveAttributeAsync("lang", "es");
        await Assertions.Expect(rtl).ToHaveAttributeAsync("data-expanded", "true");
        await CheckLayoutAsync(page);

        var external = page.Locator("#spoiler-external-toggle");
        await external.FocusAsync();
        await external.PressAsync("Enter");
        await ExpectExpandedAsync(details, true, 90);
        await Assertions.Expect(page.Locator("#spoiler-controlled-state")).ToHaveTextAsync("Estado: expandido");
        await Assertions.Expect(note).ToBeEditableAsync();
        await note.FillAsync(noteText);
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync($"Nota: {noteText}");

        var detailsToggle = details.Locator(".dnet-spoiler-toggle");
        await detailsToggle.FocusAsync();
        await detailsToggle.PressAsync("Space");
        await ExpectExpandedAsync(details, false, 90);
        await Assertions.Expect(note).ToHaveValueAsync(noteText);
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync($"Nota: {noteText}");

        await article.Locator(".dnet-spoiler-toggle").ClickAsync();
        await ExpectExpandedAsync(article, true, 110);
        await page.Locator("#spoiler-change-content").ClickAsync();
        await Assertions.Expect(page.Locator("#spoiler-content-state")).ToHaveTextAsync("Artículo completo disponible");
        await Assertions.Expect(dynamic).ToHaveAttributeAsync("data-overflow", "true");
        await dynamic.Locator(".dnet-spoiler-toggle").ClickAsync();
        await ExpectExpandedAsync(dynamic, true, 100);
        await page.Locator("#spoiler-change-content").ClickAsync();
        await Assertions.Expect(dynamic.Locator(".dnet-spoiler-toggle")).ToBeHiddenAsync();
        await Assertions.Expect(page.Locator("#spoiler-content-state")).ToHaveTextAsync("Descripción breve");
        await Assertions.Expect(article).ToHaveAttributeAsync("data-expanded", "true");
        await Assertions.Expect(details).ToHaveAttributeAsync("data-expanded", "false");
        await Assertions.Expect(rtl).ToHaveAttributeAsync("data-expanded", "true");
        await Assertions.Expect(note).ToHaveValueAsync(noteText);

        await detailsToggle.FocusAsync();
        await detailsToggle.PressAsync("Enter");
        await ExpectExpandedAsync(details, true, 90);
        await Assertions.Expect(note).ToHaveValueAsync(noteText);
        await Assertions.Expect(page.Locator("#spoiler-original-inert")).ToHaveAttributeAsync("inert", string.Empty);
        await page.SetViewportSizeAsync(width == 1440 ? 390 : 1440, 1000);
        await ExpectExpandedAsync(details, true, 90);
        await ExpectExpandedAsync(article, true, 110);
        await Assertions.Expect(dynamic.Locator(".dnet-spoiler-toggle")).ToBeHiddenAsync();
        await Assertions.Expect(page.Locator("#spoiler-short .dnet-spoiler-toggle")).ToBeHiddenAsync();
        await Assertions.Expect(note).ToHaveValueAsync(noteText);
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync($"Nota: {noteText}");
        await CheckLayoutAsync(page);

        await external.ClickAsync();
        await ExpectExpandedAsync(details, false, 90);
        await Assertions.Expect(page.Locator("#spoiler-controlled-state")).ToHaveTextAsync("Estado: contraído");
        await page.SetViewportSizeAsync(width, 1000);
        await page.Locator("#spoiler-change-content").ClickAsync();
        await Assertions.Expect(page.Locator("#spoiler-content-state")).ToHaveTextAsync("Artículo completo disponible");
        await ExpectExpandedAsync(dynamic, true, 100);
        await rtl.Locator(".dnet-spoiler-toggle").ClickAsync();
        await ExpectExpandedAsync(rtl, false, 70);
        await Assertions.Expect(article).ToHaveAttributeAsync("data-expanded", "true");
        await Assertions.Expect(details).ToHaveAttributeAsync("data-expanded", "false");
        await Assertions.Expect(note).ToHaveValueAsync(noteText);
        await CheckLayoutAsync(page);

        await external.ClickAsync();
        await ExpectExpandedAsync(details, true, 90);
        await note.FillAsync(string.Empty);
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync("Nota: Sin anotaciones");
        await note.FillAsync(noteText);
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync($"Nota: {noteText}");
        var themeLink = page.Locator("#spoiler-hidden-link");
        await Assertions.Expect(themeLink).ToHaveAttributeAsync("href", "/theming");
        await themeLink.FocusAsync();
        await themeLink.PressAsync("Enter");
        await page.WaitForURLAsync(new Regex("/theming/?$", RegexOptions.IgnoreCase));
        await page.GoBackAsync();
        await Assertions.Expect(page.Locator("#demo-spoiler")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#spoiler-note-value")).ToHaveTextAsync("Nota: Sin anotaciones");
        await Assertions.Expect(page.Locator("#spoiler-controlled-state")).ToHaveTextAsync("Estado: contraído");
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static async Task ExpectExpandedAsync(ILocator root, bool expanded, int maxHeight)
    {
        await Assertions.Expect(root).ToHaveAttributeAsync("data-expanded", expanded ? "true" : "false");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-overflow", "true");
        await Assertions.Expect(root.Locator(".dnet-spoiler-toggle")).ToHaveAttributeAsync("aria-expanded", expanded ? "true" : "false");
        if (expanded)
        {
            await root.Page.WaitForFunctionAsync("""
                id => {
                    const root = document.getElementById(id);
                    const viewport = root.querySelector('.dnet-spoiler-viewport');
                    const body = root.querySelector('.dnet-spoiler-body');
                    return Math.abs(viewport.clientHeight - Math.max(body.scrollHeight, body.offsetHeight)) <= 1;
                }
                """, await root.GetAttributeAsync("id"));
        }
        else
        {
            await Assertions.Expect(root.Locator(".dnet-spoiler-viewport")).ToHaveCSSAsync("height", $"{maxHeight}px");
        }
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.Locator("#demo-spoiler").EvaluateAsync<string[]>("""
            root => {
                const issues = [];
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Spoiler showcase overflows horizontally');
                for (const id of ['spoiler-article-card', 'spoiler-details-card', 'spoiler-dynamic-card', 'spoiler-short-card', 'spoiler-direction-card']) {
                    const card = document.getElementById(id), rect = card.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > innerWidth + 1) issues.push(`${id} lies outside viewport`);
                    if (card.scrollWidth > card.clientWidth + 1) issues.push(`${id} overflows horizontally`);
                }
                for (const spoiler of root.querySelectorAll('dnet-spoiler')) {
                    const rect = spoiler.getBoundingClientRect();
                    const body = spoiler.querySelector('.dnet-spoiler-body');
                    if (body.scrollWidth > body.clientWidth + 1) issues.push(`${spoiler.id} body overflows`);
                    for (const child of spoiler.querySelectorAll('input, .dnet-spoiler-toggle')) {
                        const bounds = child.getBoundingClientRect();
                        if (bounds.width && (bounds.left < rect.left - 1 || bounds.right > rect.right + 1)) issues.push(`${spoiler.id} has clipped control`);
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
        await BrowserReady.OpenAsync(page, $"{baseUrl}/Spoiler");
        await Assertions.Expect(page.Locator("#demo-spoiler")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#spoiler-basic")).ToHaveAttributeAsync("data-overflow", "true");
        await Assertions.Expect(page.Locator("#spoiler-controlled")).ToHaveAttributeAsync("data-overflow", "true");
        return (page, errors);
    }
}
