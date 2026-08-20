using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridRowSpanHoverTests
{
    [Fact]
    public async Task Hovering_either_covered_row_highlights_the_rowspan_cell_and_only_that_visual_row()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("DNET_BLAZOR_GRID_ROWSPAN_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "https://127.0.0.1:5101";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1_600, Height = 900 }
        });

        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200,
            ContentType = "application/json",
            Body = personsJson
        }));

        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/BlGrid", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000
        });

        var spanningCell = page.Locator(
            ".blg-center-cols-container [data-blg-row-span=\"2\"]").First;
        await spanningCell.WaitForAsync(new() { Timeout = 30_000 });

        var upperRowIndex = int.Parse(
            await spanningCell.GetAttributeAsync("data-blg-row-index")
            ?? throw new InvalidOperationException("The spanning cell has no row index."));
        var lowerRowIndex = upperRowIndex + 1;

        var upperRowCell = page.Locator(
            $".blg-center-cols-container [data-blg-row-index=\"{upperRowIndex}\"][data-blg-row-span=\"1\"]").First;
        var lowerRowCell = page.Locator(
            $".blg-center-cols-container [data-blg-row-index=\"{lowerRowIndex}\"][data-blg-row-span=\"1\"]").First;

        await lowerRowCell.HoverAsync();

        await Assertions.Expect(spanningCell).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));
        await Assertions.Expect(lowerRowCell).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));
        await Assertions.Expect(upperRowCell).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));

        await upperRowCell.HoverAsync();

        await Assertions.Expect(spanningCell).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));
        await Assertions.Expect(upperRowCell).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));
        await Assertions.Expect(lowerRowCell).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\bblg-hover-class\b"));

        var screenshotPath = Environment.GetEnvironmentVariable("DNET_BLAZOR_GRID_ROWSPAN_SCREENSHOT");
        if (!string.IsNullOrWhiteSpace(screenshotPath))
        {
            await spanningCell.ScrollIntoViewIfNeededAsync();
            await upperRowCell.DispatchEventAsync("mouseover");
            await page.WaitForTimeoutAsync(100);
            await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        }
    }
}
