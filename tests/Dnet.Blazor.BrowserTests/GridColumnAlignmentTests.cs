using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridColumnAlignmentTests
{
    [Theory]
    [InlineData(1440, "auto")]
    [InlineData(1440, "thin")]
    [InlineData(1440, "none")]
    [InlineData(2294, "auto")]
    [InlineData(2294, "thin")]
    [InlineData(2294, "none")]
    [InlineData(390, "auto")]
    [InlineData(390, "thin")]
    [InlineData(390, "none")]
    public async Task Header_and_body_columns_align_through_the_end_of_horizontal_scrolling(
        int width, string scrollbarWidth)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            // Chromium's headless default hides native scrollbars and masks
            // regressions when a classic scrollbar consumes the viewport height.
            IgnoreDefaultArgs = new[] { "--hide-scrollbars" }
        });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 900 }
        });
        var pageErrors = new List<string>();
        page.PageError += (_, error) => pageErrors.Add(error);
        var persons = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = persons
        }));
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{baseUrl}/BlGrid");
        await page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.blg-center-cols-container [role=gridcell]').length > 0");

        // Exercise native scrollbar preferences, including browsers with no reserved gutter.
        // These preferences change scrollbar geometry without adding layout compensation.
        await page.EvaluateAsync(
            """
            value => {
                for (const element of document.querySelectorAll(
                    '.blg-root, .blg-header, .blg-viewport, .blg-body-horizontal-scroll, .blg-body-horizontal-scroll-viewport')) {
                    element.style.scrollbarWidth = value;
                }
            }
            """, scrollbarWidth);

        foreach (var atEnd in new[] { false, true })
        {
            await ScrollAsync(page, atEnd);
            var metrics = await MeasureAsync(page);
            var state = $"{width}px, scrollbar-width:{scrollbarWidth}, {(atEnd ? "end" : "start")}";

            Near(metrics.BalanceHeader.Left, metrics.BalanceBody.Left, $"Balance left ({state})");
            Near(metrics.BalanceHeader.Right, metrics.BalanceBody.Right, $"Balance right ({state})");
            Near(metrics.BalanceHeader.Width, metrics.BalanceBody.Width, $"Balance width ({state})");
            Near(metrics.FirstCenterHeader.Left, metrics.FirstCenterBody.Left, $"First center column left ({state})");
            Near(metrics.FirstCenterHeader.Width, metrics.FirstCenterBody.Width, $"First center column width ({state})");
            Near(metrics.LastCenterHeader.Left, metrics.LastCenterBody.Left, $"Last center column left ({state})");
            Near(metrics.LastCenterHeader.Right, metrics.LastCenterBody.Right, $"Last center column right ({state})");

            if (!atEnd) continue;
            Assert.True(metrics.ScrollLeft > 0, $"The grid must exercise horizontal scrolling ({state}).");
            Near(metrics.MaxScrollLeft, metrics.ScrollLeft, $"Horizontal scrollbar reaches its end ({state})");
            Near(metrics.BalanceHeader.Left, metrics.LastCenterHeader.Right,
                $"The final center header is fully exposed before the pinned column ({state})");
            Near(metrics.BalanceBody.Left, metrics.LastCenterBody.Right,
                $"The final center body cell is fully exposed before the pinned column ({state})");
        }

        Assert.Empty(pageErrors);
    }

    private static Task ScrollAsync(IPage page, bool atEnd) => page.EvaluateAsync(
        """
        async atEnd => {
            const root = document.querySelector('.blg-root');
            const scrollViewport = getComputedStyle(root).overflowX === 'auto'
                ? root : root.querySelector('.blg-body-horizontal-scroll-viewport');
            scrollViewport.scrollLeft = atEnd ? scrollViewport.scrollWidth - scrollViewport.clientWidth : 0;
            scrollViewport.dispatchEvent(new Event('scroll'));
            await new Promise(resolve => setTimeout(resolve, 100));
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        }
        """, atEnd);

    private static Task<LayoutMetrics> MeasureAsync(IPage page) => page.EvaluateAsync<LayoutMetrics>(
        """
        () => {
            const root = document.querySelector('.blg-root');
            const centerHeaders = [...root.querySelectorAll('.blg-header-container > [role=columnheader][title]')];
            const firstBodyCell = root.querySelector('.blg-center-cols-container [role=gridcell]');
            const rowId = firstBodyCell.dataset.blgRowId;
            const centerBodyCells = [...root.querySelectorAll(
                `.blg-center-cols-container [role=gridcell][data-blg-row-id="${rowId}"]`)];
            const scrollViewport = getComputedStyle(root).overflowX === 'auto'
                ? root : root.querySelector('.blg-body-horizontal-scroll-viewport');
            const edges = element => {
                const rect = element.getBoundingClientRect();
                return { left: rect.left, right: rect.right, width: rect.width };
            };
            return {
                balanceHeader: edges(root.querySelector('.blg-pinned-right-header [role=columnheader][title="Balance"]')),
                balanceBody: edges(root.querySelector('.blg-pinned-right-cols-container [role=gridcell]')),
                firstCenterHeader: edges(centerHeaders[0]),
                firstCenterBody: edges(firstBodyCell),
                lastCenterHeader: edges(centerHeaders.at(-1)),
                lastCenterBody: edges(centerBodyCells.at(-1)),
                scrollLeft: scrollViewport.scrollLeft,
                maxScrollLeft: scrollViewport.scrollWidth - scrollViewport.clientWidth
            };
        }
        """);

    private static void Near(double expected, double actual, string dimension) =>
        Assert.True(Math.Abs(expected - actual) <= 0.8,
            $"{dimension}: expected {expected}px, actual {actual}px.");

    private sealed class LayoutMetrics
    {
        public ColumnEdges BalanceHeader { get; init; } = new();
        public ColumnEdges BalanceBody { get; init; } = new();
        public ColumnEdges FirstCenterHeader { get; init; } = new();
        public ColumnEdges FirstCenterBody { get; init; } = new();
        public ColumnEdges LastCenterHeader { get; init; } = new();
        public ColumnEdges LastCenterBody { get; init; } = new();
        public double ScrollLeft { get; init; }
        public double MaxScrollLeft { get; init; }
    }

    private sealed class ColumnEdges
    {
        public double Left { get; init; }
        public double Right { get; init; }
        public double Width { get; init; }
    }
}
