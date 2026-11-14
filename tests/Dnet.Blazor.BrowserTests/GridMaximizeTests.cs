using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

// The sample grid enables GridOptions.EnableMaximize: its corner button makes the grid
// fill the window, on desktop and on phones, and the button or Escape brings it back.
public sealed class GridMaximizeTests
{
    [Theory]
    [InlineData("liquid-glass", 1440, 900)]
    [InlineData("", 1440, 900)]
    [InlineData("liquid-glass-dark", 390, 844)]
    public async Task The_corner_button_fills_the_window_and_the_button_or_Escape_brings_the_grid_back(
        string theme, int width, int height)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = height }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = personsJson
        }));
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        await page.AddInitScriptAsync(
            $"localStorage.setItem('dnet.theme', JSON.stringify({JsonSerializer.Serialize(theme)}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{baseUrl}/BlGrid");

        var grid = page.Locator(".blg-arcadia-theme").First;
        var button = grid.Locator(".blg-maximize-button");
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-label", "Maximize table");
        await button.ScrollIntoViewIfNeededAsync();
        var placed = await BoxAsync(grid);
        Assert.True(placed.Width < width || placed.Y > 0, "The grid starts inside the page, not over it.");

        // The quarter disc sits in the grid's top-right corner.
        var corner = await BoxAsync(button);
        Assert.Equal(placed.X + placed.Width, corner.X + corner.Width, 1);
        Assert.Equal(placed.Y, corner.Y, 1);

        await button.ClickAsync();
        await Assertions.Expect(grid).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("blg-grid-maximized"));
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-label", "Restore table");
        var maximized = await BoxAsync(grid);
        Assert.Equal(0, maximized.X, 1);
        Assert.Equal(0, maximized.Y, 1);
        Assert.Equal(width, maximized.Width, 1);
        Assert.Equal(height, maximized.Height, 1);

        // The advanced filter still opens above the maximized grid, and its Escape
        // does not restore the grid.
        await grid.Locator(".blg-adv-toggle-button").First.ClickAsync();
        var filter = page.Locator(".blg-advanced-filter-container");
        await Assertions.Expect(filter).ToBeVisibleAsync();
        Assert.True(await filter.EvaluateAsync<bool>("""
            panel => {
                const box = panel.getBoundingClientRect();
                const top = document.elementFromPoint(box.left + box.width / 2, box.top + box.height / 2);
                return panel.contains(top);
            }
            """), "The filter is drawn above the maximized grid.");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(grid).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("blg-grid-maximized"));
        await page.Locator(".cdk-overlay-backdrop").First.ClickAsync(new() { Position = new() { X = 4, Y = 4 } });
        await Assertions.Expect(filter).ToHaveCountAsync(0);

        // With nothing open, Escape brings the grid back, wherever the focus is.
        await page.EvaluateAsync("() => document.activeElement?.blur()");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(grid).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("blg-grid-maximized"));
        await Assertions.Expect(button).ToHaveAttributeAsync("aria-label", "Maximize table");

        // And the button does too.
        await button.ClickAsync();
        await Assertions.Expect(grid).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("blg-grid-maximized"));
        await button.ClickAsync();
        await Assertions.Expect(grid).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("blg-grid-maximized"));
        var restored = await BoxAsync(grid);
        Assert.Equal(placed.Width, restored.Width, 1);
        Assert.Equal(placed.Height, restored.Height, 1);

        Assert.Empty(errors);
    }

    private static async Task<LocatorBoundingBoxResult> BoxAsync(ILocator locator) =>
        await locator.BoundingBoxAsync() ?? throw new InvalidOperationException("The element has no box.");
}
