using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridResponsiveLayoutTests
{
    [Fact]
    public async Task Mobile_grid_scrolls_as_one_canvas_without_changing_pinned_partitions()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("DNET_BLAZOR_GRID_RESPONSIVE_TESTS"),
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
            ViewportSize = new ViewportSize { Width = 430, Height = 900 }
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
        await page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.blg-center-cols-container [data-blg-row-id]').length > 0");

        var partitionHeaders = await page.EvaluateAsync<PartitionHeaders>(
            """
            () => ({
                left: [...document.querySelectorAll('.blg-pinned-left-header .blg-header-cell-text')]
                    .map(element => element.textContent.trim()),
                center: [...document.querySelectorAll('.blg-header-container .blg-header-cell-text')]
                    .map(element => element.textContent.trim()),
                right: [...document.querySelectorAll('.blg-pinned-right-header .blg-header-cell-text')]
                    .map(element => element.textContent.trim())
            })
            """);

        Assert.Contains("Amount", partitionHeaders.Left);
        Assert.Contains("Sales", partitionHeaders.Left);
        Assert.DoesNotContain("Sales", partitionHeaders.Center);
        Assert.Contains("Balance", partitionHeaders.Right);

        var horizontalLayout = await page.EvaluateAsync<HorizontalLayout>(
            """
            () => {
                const root = document.querySelector('.blg-root');
                const desktopScrollbar = document.querySelector('.blg-body-horizontal-scroll');
                root.scrollLeft = root.scrollWidth - root.clientWidth;
                return new Promise(resolve => requestAnimationFrame(() => {
                    const rootRect = root.getBoundingClientRect();
                    const rightHeader = document.querySelector('.blg-pinned-right-header');
                    const rightCell = document.querySelector('.blg-pinned-right-cols-container [data-blg-row-id]');
                    const rightHeaderRect = rightHeader.getBoundingClientRect();
                    const rightCellRect = rightCell.getBoundingClientRect();
                    resolve({
                        clientWidth: root.clientWidth,
                        scrollWidth: root.scrollWidth,
                        scrollLeft: root.scrollLeft,
                        desktopScrollbarDisplay: getComputedStyle(desktopScrollbar).display,
                        rightHeaderVisible: rightHeaderRect.left < rootRect.right && rightHeaderRect.right > rootRect.left,
                        rightPartitionAlignment: Math.abs(rightHeaderRect.left - rightCellRect.left),
                        rootLeft: rootRect.left,
                        rootRight: rootRect.right,
                        rightHeaderLeft: rightHeaderRect.left,
                        rightHeaderRight: rightHeaderRect.right,
                        headerWidth: document.querySelector('.blg-header').getBoundingClientRect().width,
                        viewportWidth: document.querySelector('.blg-viewport').getBoundingClientRect().width
                    });
                }));
            }
            """);

        Assert.True(horizontalLayout.ScrollWidth > horizontalLayout.ClientWidth);
        Assert.True(horizontalLayout.ScrollLeft > 0);
        Assert.Equal("none", horizontalLayout.DesktopScrollbarDisplay);
        Assert.True(
            horizontalLayout.RightHeaderVisible,
            $"Right header was not visible at scrollLeft {horizontalLayout.ScrollLeft} " +
            $"(clientWidth {horizontalLayout.ClientWidth}, scrollWidth {horizontalLayout.ScrollWidth}, " +
            $"root {horizontalLayout.RootLeft}..{horizontalLayout.RootRight}, " +
            $"right header {horizontalLayout.RightHeaderLeft}..{horizontalLayout.RightHeaderRight}, " +
            $"header width {horizontalLayout.HeaderWidth}, viewport width {horizontalLayout.ViewportWidth}).");
        Assert.True(
            horizontalLayout.RightPartitionAlignment <= 1,
            $"Header/body alignment differed by {horizontalLayout.RightPartitionAlignment}px.");

        var screenshotPath = Environment.GetEnvironmentVariable("DNET_BLAZOR_GRID_SCREENSHOT");
        if (!string.IsNullOrWhiteSpace(screenshotPath))
        {
            await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        }

        var rowsAfterVerticalScroll = await page.EvaluateAsync<PartitionRows>(
            """
            () => {
                const viewport = document.querySelector('.blg-viewport');
                viewport.scrollTop = viewport.scrollHeight - viewport.clientHeight;
                return new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(() => {
                    const rowIds = selector => [...new Set(
                        [...document.querySelectorAll(selector)].map(element => element.dataset.blgRowId))];
                    resolve({
                        left: rowIds('.blg-pinned-left-cols-container [data-blg-row-id]'),
                        center: rowIds('.blg-center-cols-container [data-blg-row-id]'),
                        right: rowIds('.blg-pinned-right-cols-container [data-blg-row-id]')
                    });
                })));
            }
            """);

        Assert.NotEmpty(rowsAfterVerticalScroll.Left);
        Assert.Equal(rowsAfterVerticalScroll.Center, rowsAfterVerticalScroll.Left);
        Assert.Equal(rowsAfterVerticalScroll.Center, rowsAfterVerticalScroll.Right);

        await page.SetViewportSizeAsync(1_600, 900);
        var desktopLayout = await page.EvaluateAsync<DesktopLayout>(
            """
            async () => {
                const root = document.querySelector('.blg-root');
                const desktopScrollbar = document.querySelector('.blg-body-horizontal-scroll');
                const desktopScrollViewport = document.querySelector('.blg-body-horizontal-scroll-viewport');
                const pinnedHeader = document.querySelector('.blg-pinned-left-header');
                const pinnedLeftBefore = pinnedHeader.getBoundingClientRect().left;

                desktopScrollViewport.scrollLeft = desktopScrollViewport.scrollWidth - desktopScrollViewport.clientWidth;
                desktopScrollViewport.dispatchEvent(new Event('scroll'));
                await new Promise(resolve => setTimeout(resolve, 100));
                await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));

                return {
                    desktopScrollbarDisplay: getComputedStyle(desktopScrollbar).display,
                    rootOverflowX: getComputedStyle(root).overflowX,
                    rootScrollLeft: root.scrollLeft,
                    desktopScrollLeft: desktopScrollViewport.scrollLeft,
                    pinnedLeftMovement: Math.abs(pinnedHeader.getBoundingClientRect().left - pinnedLeftBefore),
                    centerAlignment: Math.abs(
                        document.querySelector('.blg-header-container').getBoundingClientRect().left
                        - document.querySelector('.blg-center-cols-container').getBoundingClientRect().left)
                };
            }
            """);

        Assert.Equal("flex", desktopLayout.DesktopScrollbarDisplay);
        Assert.Equal("visible", desktopLayout.RootOverflowX);
        Assert.Equal(0, desktopLayout.RootScrollLeft);
        Assert.True(desktopLayout.DesktopScrollLeft > 0);
        Assert.True(desktopLayout.PinnedLeftMovement <= 1);
        Assert.True(desktopLayout.CenterAlignment <= 1);
    }

    private sealed class PartitionHeaders
    {
        public string[] Left { get; init; } = [];

        public string[] Center { get; init; } = [];

        public string[] Right { get; init; } = [];
    }

    private sealed class HorizontalLayout
    {
        public double ClientWidth { get; init; }

        public double ScrollWidth { get; init; }

        public double ScrollLeft { get; init; }

        public string DesktopScrollbarDisplay { get; init; } = string.Empty;

        public bool RightHeaderVisible { get; init; }

        public double RightPartitionAlignment { get; init; }

        public double RootLeft { get; init; }

        public double RootRight { get; init; }

        public double RightHeaderLeft { get; init; }

        public double RightHeaderRight { get; init; }

        public double HeaderWidth { get; init; }

        public double ViewportWidth { get; init; }
    }

    private sealed class PartitionRows
    {
        public string[] Left { get; init; } = [];

        public string[] Center { get; init; } = [];

        public string[] Right { get; init; } = [];
    }

    private sealed class DesktopLayout
    {
        public string DesktopScrollbarDisplay { get; init; } = string.Empty;

        public string RootOverflowX { get; init; } = string.Empty;

        public double RootScrollLeft { get; init; }

        public double DesktopScrollLeft { get; init; }

        public double PinnedLeftMovement { get; init; }

        public double CenterAlignment { get; init; }
    }
}
