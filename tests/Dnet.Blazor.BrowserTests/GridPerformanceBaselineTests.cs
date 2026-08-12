using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridPerformanceBaselineTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Complex_grid_keeps_rows_visible_during_render_grouping_and_fast_scroll()
    {
        if (!GridPerformanceTestsEnabled())
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
            ViewportSize = new ViewportSize
            {
                Width = 1_600,
                Height = 900
            }
        });

        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200,
            ContentType = "application/json",
            Body = personsJson
        }));

        await InstallTransientEmptyStateProbeAsync(page);

        var initialRender = Stopwatch.StartNew();
        await page.GotoAsync(
            $"{baseUrl.TrimEnd('/')}/BlGrid",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var grid = page.Locator(".blg-center-cols-container[role=grid]");
        await grid.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        await page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.blg-center-cols-container [data-blg-row-id]').length > 0");
        initialRender.Stop();

        var transientEmptyStates = await page.EvaluateAsync<string[]>(
            "() => globalThis.__dnetGridEmptyStates ?? []");
        Assert.DoesNotContain("No hay datos.", transientEmptyStates);
        Assert.DoesNotContain("No hay resultados para los filtros actuales.", transientEmptyStates);

        var groupDuration = await MeasureButtonTransitionAsync(
            page,
            "Agrupar por Age",
            "Quitar agrupación por Age");
        var ungroupDuration = await MeasureButtonTransitionAsync(
            page,
            "Quitar agrupación por Age",
            "Agrupar por Age");

        var scroll = await MeasureFastVerticalScrollAsync(page);

        output.WriteLine($"Initial render: {initialRender.Elapsed.TotalMilliseconds:F1} ms");
        output.WriteLine($"Group Age: {groupDuration.TotalMilliseconds:F1} ms");
        output.WriteLine($"Ungroup Age: {ungroupDuration.TotalMilliseconds:F1} ms");
        output.WriteLine(
            $"Fast scroll: {scroll.Samples} frames, {scroll.BlankFrames} blank frames, " +
            $"maximum uncovered band {scroll.MaximumGapPixels:F1} px");

        Assert.True(scroll.Samples > 0, "The scroll probe did not collect animation frames.");
        Assert.Equal(0, scroll.BlankFrames);
        Assert.True(
            scroll.MaximumGapPixels <= 1,
            $"Fast vertical scrolling exposed an uncovered band of {scroll.MaximumGapPixels:F1} px.");
    }

    private static bool GridPerformanceTestsEnabled() => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_GRID_PERFORMANCE_TESTS"),
        "true",
        StringComparison.OrdinalIgnoreCase);

    private static async Task InstallTransientEmptyStateProbeAsync(IPage page)
    {
        await page.AddInitScriptAsync(
            """
            globalThis.__dnetGridEmptyStates = [];

            const recordGridEmptyStates = () => {
                for (const status of document.querySelectorAll('.blg-grid-status')) {
                    const text = status.textContent?.trim();
                    if (text && !globalThis.__dnetGridEmptyStates.includes(text)) {
                        globalThis.__dnetGridEmptyStates.push(text);
                    }
                }
            };

            const startGridEmptyStateObserver = () => {
                recordGridEmptyStates();
                new MutationObserver(recordGridEmptyStates).observe(document.documentElement, {
                    childList: true,
                    subtree: true,
                    characterData: true
                });
            };

            if (document.documentElement) {
                startGridEmptyStateObserver();
            } else {
                addEventListener('DOMContentLoaded', startGridEmptyStateObserver, { once: true });
            }
            """);
    }

    private static async Task<TimeSpan> MeasureButtonTransitionAsync(
        IPage page,
        string currentAccessibleName,
        string expectedAccessibleName)
    {
        var currentButton = page.GetByRole(AriaRole.Button, new()
        {
            Name = currentAccessibleName,
            Exact = true
        });
        await currentButton.WaitForAsync();

        var stopwatch = Stopwatch.StartNew();
        await currentButton.ClickAsync();
        await page.GetByRole(AriaRole.Button, new()
        {
            Name = expectedAccessibleName,
            Exact = true
        }).WaitForAsync();
        await page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.blg-center-cols-container [data-blg-row-id]').length > 0");
        await page.EvaluateAsync(
            "() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    private static async Task<ScrollProbeResult> MeasureFastVerticalScrollAsync(IPage page)
    {
        return await page.Locator(".blg-viewport").EvaluateAsync<ScrollProbeResult>(
            """
            async viewport => {
                const result = {
                    samples: 0,
                    blankFrames: 0,
                    maximumGapPixels: 0
                };
                const positions = [0, 0.92, 0.08, 1, 0.18, 0.82, 0.35, 0.68, 0];
                let sampling = true;

                const sample = () => {
                    if (!sampling) {
                        return;
                    }

                    result.samples++;
                    const viewportRect = viewport.getBoundingClientRect();
                    const maximumScrollTop = viewport.scrollHeight - viewport.clientHeight;
                    const rowRects = [...document.querySelectorAll(
                        '.blg-center-cols-container [data-blg-row-id]')]
                        .map(element => element.getBoundingClientRect())
                        .filter(rect => rect.bottom > viewportRect.top && rect.top < viewportRect.bottom);

                    let maximumGap = 0;
                    if (maximumScrollTop > 1) {
                        if (rowRects.length === 0) {
                            maximumGap = viewport.clientHeight;
                        } else {
                            const firstRowTop = Math.min(...rowRects.map(rect => rect.top));
                            const lastRowBottom = Math.max(...rowRects.map(rect => rect.bottom));
                            const topGap = viewport.scrollTop > 1
                                ? Math.max(0, firstRowTop - viewportRect.top)
                                : 0;
                            const bottomGap = viewport.scrollTop < maximumScrollTop - 1
                                ? Math.max(0, viewportRect.bottom - lastRowBottom)
                                : 0;
                            maximumGap = Math.max(topGap, bottomGap);
                        }
                    }

                    if (maximumGap > 1) {
                        result.blankFrames++;
                    }
                    result.maximumGapPixels = Math.max(result.maximumGapPixels, maximumGap);
                    requestAnimationFrame(sample);
                };

                requestAnimationFrame(sample);
                for (const position of positions) {
                    const maximumScrollTop = viewport.scrollHeight - viewport.clientHeight;
                    viewport.scrollTop = maximumScrollTop * position;
                    await new Promise(resolve => setTimeout(resolve, 70));
                }
                await new Promise(resolve => setTimeout(resolve, 250));
                sampling = false;

                return result;
            }
            """);
    }

    private sealed class ScrollProbeResult
    {
        public ScrollProbeResult()
        {
        }

        public int Samples { get; init; }

        public int BlankFrames { get; init; }

        public double MaximumGapPixels { get; init; }
    }
}
