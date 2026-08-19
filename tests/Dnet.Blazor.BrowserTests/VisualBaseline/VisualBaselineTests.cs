using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests.VisualBaseline;

/// <summary>
/// Shared Playwright browser for the visual baseline suite. Browser launches
/// are expensive, so one Chromium instance serves every scenario; each test
/// opens its own page at its own viewport.
/// </summary>
public sealed class VisualBaselineFixture : IAsyncLifetime
{
    public const string EnabledVariable = "DNET_BLAZOR_VISUAL_TESTS";

    private IPlaywright _playwright = null!;

    private IBrowser _browser = null!;

    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "https://127.0.0.1:5101";

    public string TestProjectRoot { get; }

    public VisualBaselineFixture()
    {
        // bin/Debug/net10.0 -> project root.
        TestProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    }

    public static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable(EnabledVariable),
        "true",
        StringComparison.OrdinalIgnoreCase);

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    public async Task<IPage> NewPageAsync(int width, int height)
    {
        var page = await _browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = width, Height = height },
        });

        // Deterministic rendering: reduced motion and no transitions/animations.
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await page.AddInitScriptAsync(
            """
            (function () {
                const inject = () => {
                    const style = document.createElement('style');
                    style.textContent = '*{animation:none !important;transition:none !important;animation-delay:0s !important;transition-delay:0s !important;}';
                    document.documentElement.appendChild(style);
                };
                if (document.documentElement) { inject(); } else { document.addEventListener('DOMContentLoaded', inject, { once: true }); }
            })();
            """);;

        // Block webfont downloads: Roboto arrives from Google Fonts over the
        // network and its availability varies run to run, which shifts every
        // text glyph and makes goldens flaky. The fallback font stack is
        // deterministic per platform, which is what the per-platform goldens
        // already assume.
        await page.RouteAsync("**://fonts.googleapis.com/**", route => route.AbortAsync());
        await page.RouteAsync("**://fonts.gstatic.com/**", route => route.AbortAsync());;

        // Data-driven pages load sample-data/person_500.json from the WebHostURL
        // client (an external origin). Resolve it from the local fixture so
        // goldens never depend on the network or remote content.
        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200,
            ContentType = "application/json",
            Body = personsJson,
        }));

        return page;
    }
}

public sealed class VisualBaselineTests : IClassFixture<VisualBaselineFixture>
{
    private static readonly (string Name, int Width, int Height)[] Viewports =
    [
        ("desktop", 1440, 900),
        ("mobile", 390, 844),
    ];

    private readonly VisualBaselineFixture _fixture;

    public VisualBaselineTests(VisualBaselineFixture fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> Cases() =>
        ComponentScenarios.All
            .SelectMany(scenario => Viewports.Select(viewport => new object[] { scenario, viewport.Name, viewport.Width, viewport.Height }));

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Visual_baseline(ComponentScenario scenario, string viewport, int width, int height)
    {
        if (!VisualBaselineFixture.Enabled)
        {
            // Opt-in suite: the sample application must be running and the
            // goldens must exist for the current platform. See README.md.
            return;
        }

        if (viewport == "mobile" && scenario.SkipMobile)
        {
            // Deliberately not captured: the sample page has no stable mobile
            // instance for this component. See VisualBaseline/README.md.
            return;
        }

        var page = await _fixture.NewPageAsync(width, height);
        try
        {
            await page.GotoAsync($"{_fixture.BaseUrl.TrimEnd('/')}{scenario.Route}", new()
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60_000,
            });

            var root = page.Locator(scenario.RootSelector).First;
            await root.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 60_000,
            });

            if (scenario.ReadyFunction is not null)
            {
                await page.WaitForFunctionAsync(scenario.ReadyFunction, null, new()
                {
                    Timeout = 30_000,
                });
            }

            await SettleAsync(page);

            foreach (var state in scenario.States)
            {
                await CaptureStateAsync(page, scenario, state, viewport);
            }

            foreach (var variant in scenario.Variants)
            {
                await CaptureVariantAsync(page, scenario, variant, viewport);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private async Task CaptureStateAsync(IPage page, ComponentScenario scenario, VisualState state, string viewport)
    {
        var capturePage = scenario.PageCaptureStates.Contains(state);

        switch (state)
        {
            case VisualState.Hover:
                await page.Locator(scenario.HoverSelector ?? scenario.RootSelector).First.HoverAsync();
                break;
            case VisualState.Focus:
                await FocusViaKeyboardAsync(page, scenario.FocusSelector ?? scenario.RootSelector);
                break;
            case VisualState.Selected:
                await page.Locator(scenario.SelectedSelector ?? scenario.RootSelector).First.ClickAsync();
                break;
            case VisualState.Open:
                await page.Locator(scenario.OpenSelector ?? scenario.RootSelector).First.ClickAsync();
                break;
        }

        await SettleAsync(page);

        var bytes = capturePage
            ? await page.ScreenshotAsync()
            : await page.Locator(scenario.RootSelector).First.ScreenshotAsync();

        await CompareOrUpdateAsync(scenario, $"{StateName(state)}", viewport, bytes);
    }

    private async Task CaptureVariantAsync(IPage page, ComponentScenario scenario, ComponentVariant variant, string viewport)
    {
        // Variants are captured on a fresh page so earlier states (selection,
        // open panels) cannot leak into them.
        await page.GotoAsync($"{_fixture.BaseUrl.TrimEnd('/')}{scenario.Route}", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000,
        });
        var locator = page.Locator(variant.Selector).First;
        await locator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 60_000 });
        await SettleAsync(page);

        var bytes = await locator.ScreenshotAsync();
        await CompareOrUpdateAsync(scenario, $"variant-{variant.Label}", viewport, bytes);
    }

    private async Task CompareOrUpdateAsync(ComponentScenario scenario, string state, string viewport, byte[] actual)
    {
        var goldenPath = VisualGoldenComparer.GoldenPath(_fixture.TestProjectRoot, scenario.Name, state, viewport);

        if (VisualGoldenComparer.ShouldUpdate)
        {
            VisualGoldenComparer.Update(goldenPath, actual);
            return;
        }

        if (!File.Exists(goldenPath))
        {
            throw new Xunit.Sdk.XunitException(
                $"Missing golden '{goldenPath}'. Review the state, then freeze it with DNET_BLAZOR_UPDATE_GOLDENS=true.");
        }

        var result = VisualGoldenComparer.Compare(actual, await File.ReadAllBytesAsync(goldenPath), scenario.MaxDiffPixels, scenario.ChannelThreshold);
        if (!result.Passed)
        {
            // Keep the offending capture next to the golden for review.
            File.WriteAllBytes($"{goldenPath}.actual.png", actual);
        }

        Assert.True(result.Passed, $"{scenario.Name}/{state}/{viewport}: {result.Message}");
    }

    private static async Task FocusViaKeyboardAsync(IPage page, string selector)
    {
        var reached = false;
        for (var i = 0; i < 80 && !reached; i++)
        {
            reached = await page.EvaluateAsync<bool>(
                "selector => { const element = document.querySelector(selector); return !!element && document.activeElement === element; }",
                selector);
            if (!reached)
            {
                await page.Keyboard.PressAsync("Tab");
            }
        }

        if (!reached)
        {
            throw new Xunit.Sdk.XunitException($"Could not focus '{selector}' via keyboard (80 Tab presses).");
        }
    }

    private static async Task SettleAsync(IPage page)
    {
        // Fonts are the main source of text-rendering jitter between runs:
        // wait until every webfont is loaded before capturing.
        await page.EvaluateAsync("document.fonts ? document.fonts.ready : Promise.resolve()");
        await page.WaitForFunctionAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
        await Task.Delay(250);
    }

    private static string StateName(VisualState state) => state.ToString().ToLowerInvariant();
}
