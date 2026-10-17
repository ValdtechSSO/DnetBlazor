using Microsoft.Playwright;

namespace Dnet.Blazor.BrowserTests;

internal static class BrowserReady
{
    internal static async Task OpenAsync(IPage page, string url, PageGotoOptions? options = null)
    {
        await page.GotoAsync(url, options);
        // A successful document response can still show the WASM loading placeholder.
        // Allow its cold start on CI before the normal interaction assertions begin.
        await page.Locator("main.dnet-desktop-content").WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000
        });
    }
}
