using Microsoft.Playwright;

namespace Dnet.Blazor.BrowserTests;

internal static class BrowserLanguage
{
    // Existing functional cases deliberately assert English labels. New
    // LocalizationTests and the showcase layout suite exercise Spanish defaults.
    internal static Task UseEnglishAsync(IPage page) => page.AddInitScriptAsync(
        "localStorage.setItem('dnet.language', JSON.stringify('en-US')); ");
}
