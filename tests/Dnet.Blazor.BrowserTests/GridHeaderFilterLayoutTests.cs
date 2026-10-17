using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridHeaderFilterLayoutTests
{
    [Theory]
    [InlineData("", 1440, 600)]
    [InlineData("dark", 2294, 800)]
    [InlineData("", 390, 844)]
    [InlineData("dark", 390, 844)]
    public async Task Filter_fields_fit_the_header_and_keep_columns_aligned_when_their_height_changes(
        string theme, int width, int height)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101";
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = height }
        });
        var personsJson = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = personsJson
        }));
        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/BlGrid");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.blg-center-cols-container [data-blg-row-id]').length > 0");
        await page.EvaluateAsync("theme => document.documentElement.dataset.dnetTheme = theme", theme);

        var filters = page.Locator(".blg-filter-field");
        Assert.Equal(2, await filters.CountAsync());
        double initialHeaderHeight = 0, initialBodyTop = 0, initialBodyBottom = 0;

        foreach (var fieldHeight in new[] { 56, 64 })
        {
            await page.EvaluateAsync("height => document.documentElement.style.setProperty('--dnet-form-field-min-height', height + 'px')", fieldHeight);
            for (var index = 0; index < await filters.CountAsync(); index++)
            {
                var filter = filters.Nth(index);
                await BringIntoCanvasAsync(filter);
                var metrics = await MeasureAsync(filter);

                Near(fieldHeight, metrics.ControlHeight, "Control height");
                Near(24, metrics.IconWidth, "Search icon width");
                Near(24, metrics.IconHeight, "Search icon height");
                Assert.InRange(metrics.TopSpace, 8, 10);
                Assert.InRange(metrics.BottomSpace, 8, 10);
                Assert.InRange(metrics.BodyTop - metrics.ControlBottom, 8, double.MaxValue);
                Near(metrics.HeaderBottom, metrics.BodyTop, "Header/body boundary");
                Near(metrics.FilterColumnLeft, metrics.BodyColumnLeft, "Filter/body column left edge");
                Near(metrics.FilterColumnWidth, metrics.BodyColumnWidth, "Filter/body column width");
                foreach (var ancestor in metrics.ClippingAncestors)
                {
                    Assert.True(ancestor.VerticalClearance >= -0.8,
                        $"The field is clipped vertically by {ancestor.Name}: {ancestor.VerticalClearance}px clearance.");
                    Assert.True(ancestor.HorizontalClearance >= -0.8,
                        $"The visible field is clipped horizontally by {ancestor.Name}: {ancestor.HorizontalClearance}px clearance.");
                }
                foreach (var partitionHeight in metrics.PartitionHeights)
                    Near(metrics.PartitionHeights[0], partitionHeight, "Pinned and center header heights");
                Near(0, metrics.PartitionAlignments[0], "Left pinned header/body alignment");
                Near(0, metrics.PartitionAlignments[1], "Center header/body alignment");
                Near(0, metrics.PartitionAlignments[2], "Right pinned header/body alignment");

                if (index != 0) continue;
                if (fieldHeight == 56)
                {
                    initialHeaderHeight = metrics.HeaderHeight;
                    initialBodyTop = metrics.BodyTop;
                    initialBodyBottom = metrics.BodyBottom;
                }
                else
                {
                    Near(8, metrics.HeaderHeight - initialHeaderHeight, "Header expands with the public field-height token");
                    Near(8, metrics.BodyTop - initialBodyTop, "Body moves below the expanded header");
                    Near(initialBodyBottom, metrics.BodyBottom, "Body keeps its lower boundary");
                }
            }
        }

        // Exercise the same real input and clear action after both geometry changes.
        if (width == 1440)
        {
            var nameFilter = filters.First;
            await BringIntoCanvasAsync(nameFilter);
            await nameFilter.Locator("input").FillAsync("__grid_header_no_matching_person__");
            await nameFilter.Locator("input").PressAsync("End");
            await Assertions.Expect(page.Locator(".blg-grid-status")).ToContainTextAsync("No hay resultados");
            await nameFilter.Locator(".dnet-component-icon-wrapper").ClickAsync();
            await Assertions.Expect(nameFilter.Locator("input")).ToHaveValueAsync("");
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.blg-center-cols-container [data-blg-row-id]').length > 0");
            await Assertions.Expect(page.Locator(".blg-grid-status")).ToHaveCountAsync(0);
        }
    }

    private static Task BringIntoCanvasAsync(ILocator filter) => filter.EvaluateAsync(
        """
        async field => {
            const root = field.closest('.blg-root');
            const control = field.querySelector('.dnet-form-field-plain-control-container');
            if (getComputedStyle(root).overflowX === 'auto') {
                root.scrollLeft += control.getBoundingClientRect().left - root.getBoundingClientRect().left - 24;
            } else {
                const viewport = root.querySelector('.blg-header-viewport');
                const scrollbar = root.parentElement.querySelector('.blg-body-horizontal-scroll-viewport');
                scrollbar.scrollLeft += control.getBoundingClientRect().left - viewport.getBoundingClientRect().left - 24;
                scrollbar.dispatchEvent(new Event('scroll'));
            }
            await new Promise(resolve => setTimeout(resolve, 100));
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        }
        """);

    private static Task<LayoutMetrics> MeasureAsync(ILocator filter) => filter.EvaluateAsync<LayoutMetrics>(
        """
        field => {
            const root = field.closest('.blg-root');
            const control = field.querySelector('.dnet-form-field-plain-control-container');
            const controlRect = control.getBoundingClientRect();
            const cell = field.closest('.blg-filter-cell');
            const cellRect = cell.getBoundingClientRect();
            const columnIndex = [...cell.parentElement.querySelectorAll(':scope > .blg-filter-cell')].indexOf(cell);
            const bodyCell = root.querySelector(`.blg-center-cols-container [aria-colindex="${columnIndex}"]`);
            const bodyCellRect = bodyCell.getBoundingClientRect();
            const bodyRect = root.querySelector('.blg-viewport').getBoundingClientRect();
            const headerRect = root.querySelector('.blg-header').getBoundingClientRect();
            const iconRect = field.querySelector('.dnet-component-icon').getBoundingClientRect();
            const clippingAncestors = [];
            const clips = value => ['hidden', 'auto', 'scroll', 'clip'].includes(value);
            for (let ancestor = control.parentElement; ancestor; ancestor = ancestor.parentElement) {
                const style = getComputedStyle(ancestor);
                const rect = ancestor.getBoundingClientRect();
                if (!clips(style.overflowX) && !clips(style.overflowY)) continue;
                clippingAncestors.push({
                    name: ancestor.className || ancestor.tagName,
                    verticalClearance: clips(style.overflowY) ? Math.min(
                        controlRect.top - rect.top - parseFloat(style.borderTopWidth),
                        rect.bottom - parseFloat(style.borderBottomWidth) - controlRect.bottom) : 0,
                    horizontalClearance: clips(style.overflowX) ? Math.min(
                        controlRect.left - rect.left - parseFloat(style.borderLeftWidth),
                        rect.right - parseFloat(style.borderRightWidth) - controlRect.right) : 0
                });
            }
            const partitions = [
                ['.blg-pinned-left-header', '.blg-pinned-left-cols-container'],
                ['.blg-header-container', '.blg-center-cols-container'],
                ['.blg-pinned-right-header', '.blg-pinned-right-cols-container']
            ];
            return {
                controlHeight: controlRect.height, controlBottom: controlRect.bottom,
                iconWidth: iconRect.width, iconHeight: iconRect.height,
                topSpace: controlRect.top - cellRect.top,
                bottomSpace: cellRect.bottom - controlRect.bottom,
                bodyTop: bodyRect.top, bodyBottom: bodyRect.bottom,
                headerHeight: headerRect.height, headerBottom: headerRect.bottom,
                filterColumnLeft: cellRect.left, filterColumnWidth: cellRect.width,
                bodyColumnLeft: bodyCellRect.left, bodyColumnWidth: bodyCellRect.width,
                clippingAncestors,
                partitionHeights: partitions.map(([header]) => root.querySelector(header).getBoundingClientRect().height),
                partitionAlignments: partitions.map(([header, body]) => Math.abs(
                    root.querySelector(header).getBoundingClientRect().left - root.querySelector(body).getBoundingClientRect().left))
            };
        }
        """);

    private static void Near(double expected, double actual, string dimension) =>
        Assert.True(Math.Abs(expected - actual) <= 0.8, $"{dimension}: expected {expected}px, actual {actual}px.");

    private sealed class LayoutMetrics
    {
        public double ControlHeight { get; init; }
        public double ControlBottom { get; init; }
        public double IconWidth { get; init; }
        public double IconHeight { get; init; }
        public double TopSpace { get; init; }
        public double BottomSpace { get; init; }
        public double HeaderHeight { get; init; }
        public double HeaderBottom { get; init; }
        public double BodyTop { get; init; }
        public double BodyBottom { get; init; }
        public double FilterColumnLeft { get; init; }
        public double FilterColumnWidth { get; init; }
        public double BodyColumnLeft { get; init; }
        public double BodyColumnWidth { get; init; }
        public ClippingAncestor[] ClippingAncestors { get; init; } = [];
        public double[] PartitionHeights { get; init; } = [];
        public double[] PartitionAlignments { get; init; } = [];
    }

    private sealed class ClippingAncestor
    {
        public string Name { get; init; } = string.Empty;
        public double VerticalClearance { get; init; }
        public double HorizontalClearance { get; init; }
    }
}
