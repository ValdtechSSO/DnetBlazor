using Microsoft.Playwright;

namespace Dnet.Blazor.BrowserTests;

internal static class BrowserHeaderPreferences
{
    internal static ILocator Control(IPage page, string preference) =>
        page.Locator($"#dnet-{preference}-selector > button");

    internal static async Task SelectAsync(IPage page, string preference, string value,
        bool keepPointerPosition = false)
    {
        var trigger = Control(page, preference);
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        // Open-portal cases dispatch the real component click without moving the pointer:
        // a modal backdrop blocks the header, and leaving an icon dismisses its tooltip.
        if (keepPointerPosition) await trigger.DispatchEventAsync("click");
        else await trigger.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        var panel = page.Locator($"#header-{preference}-panel");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(panel).ToHaveAttributeAsync("role", "dialog");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-controls", $"header-{preference}-panel");
        var option = panel.Locator($"button[data-preference-value='{value}']");
        await Assertions.Expect(option).ToBeVisibleAsync();
        if (keepPointerPosition) await option.DispatchEventAsync("click");
        else await option.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(panel).ToHaveCountAsync(0);
        if (preference == "language") await Assertions.Expect(trigger).ToContainTextAsync(value.ToUpperInvariant());
    }
}
