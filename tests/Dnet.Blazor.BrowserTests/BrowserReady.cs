using Microsoft.Playwright;

namespace Dnet.Blazor.BrowserTests;

internal static class BrowserReady
{
    internal static async Task OpenAsync(IPage page, string url, PageGotoOptions? options = null)
    {
        await page.GotoAsync(url, options);
        await WaitForAppAsync(page);
    }

    /// <summary>
    /// Reloads the page and waits for the application again. A reload restarts the
    /// WASM runtime, so assertions made right after it would otherwise spend their
    /// 5 s on its cold start and fail on a slow CI runner.
    /// </summary>
    internal static async Task ReloadAsync(IPage page)
    {
        await page.ReloadAsync();
        await WaitForAppAsync(page);
    }

    // A successful document response can still show the WASM loading placeholder.
    // Allow its cold start on CI before the normal interaction assertions begin.
    private static Task WaitForAppAsync(IPage page) =>
        page.Locator("main.dnet-desktop-content").WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000
        });
}
