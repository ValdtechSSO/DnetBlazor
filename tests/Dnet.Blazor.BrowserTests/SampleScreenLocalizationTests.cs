using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class SampleScreenLocalizationTests
{
    private static readonly string[] Routes =
    [
        "Slider", "SlideToggle", "ConnectedPanel", "Tree", "ImageEditor", "List", "Select",
        "Expansion", "RadioButton", "PickList", "Skeleton", "Toast", "PickList/controlled-rejection",
        "Chips", "Dialog", "CheckBox", "Badge", "Stepper", "FormValidation", "Theming",
        "Autocomplete", "DoubleList", "DatePicker", "Localization", "Tooltip", "Buttons", "TimePicker",
        "InputText", "FloatingDoubleList", "Forms", "DynamicStepper", "Tabs", "Spoiler", "FloatingPanel",
        "Spinner", "PillTabs", "Timeline", "RowSelectionExample", "CellTemplatesExample",
        "AdvancedFilteringExample", "BlGrid", "ServerSideOperationsExample", "FilteringExample",
        "SortingExample", "GroupingExample", "MultipleRowSelectionExample"
    ];

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Every_screen_switches_both_ways_keeps_menu_and_has_no_resource_keys_or_overflow(string theme, int width)
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { Locale = "es-ES", ViewportSize = new() { Width = width, Height = 1000 } });
        var errors = new List<string>();
        page.PageError += (_, message) => errors.Add(message);
        page.Console += (_, message) => { if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components")) errors.Add(message.Text); };
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        var persons = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new() { ContentType = "application/json", Body = persons }));
        await page.RouteAsync("**/api/person/searchpersons", route => route.FulfillAsync(new() { ContentType = "application/json", Body = "{\"persons\":" + persons + ",\"totalItems\":500}" }));
        var initialTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({initialTheme})); if (!localStorage.getItem('dnet.language')) localStorage.setItem('dnet.language', JSON.stringify('es-ES'));");
        foreach (var route in Routes)
        {
            await BrowserReady.OpenAsync(page, BaseUrl + "/" + route, new() { WaitUntil = WaitUntilState.NetworkIdle });
            var content = page.Locator("main.dnet-desktop-content");
            await Assertions.Expect(content).ToBeVisibleAsync();
            if (route == "PickList")
                await Assertions.Expect(page.Locator("#picklist-provider .dnet-paginator-range-label")).ToContainTextAsync("de 7");
            var spanish = await content.InnerTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(spanish), route);
            var menu = await page.Locator(".dnet-left-column").InnerTextAsync();
            await BrowserHeaderPreferences.SelectAsync(page, "language", "en");
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en-US");
            if (route == "FilteringExample")
                await Assertions.Expect(page.Locator(".blg-header-cell[title='Name']")).ToHaveCountAsync(1);
            var english = await content.InnerTextAsync();
            Assert.NotEqual(spanish, english);
            Assert.Equal(menu, await page.Locator(".dnet-left-column").InnerTextAsync());
            Assert.DoesNotMatch(@"\b(?:Common|" + route.Split('/')[0] + @")\.[A-Z][A-Za-z0-9]+", english);
            if (!route.Contains("Example") && route != "BlGrid" && route != "InputText")
            {
                var overflow = await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth + 1");
                Assert.False(overflow, $"{route}/{theme}/{width}: horizontal overflow in English");
            }
            await BrowserHeaderPreferences.SelectAsync(page, "language", "es");
            await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "es-ES");
            if (route == "FilteringExample")
                await Assertions.Expect(page.Locator(".blg-header-cell[title='Nombre']")).ToHaveCountAsync(1);
            Assert.Equal(menu, await page.Locator(".dnet-left-column").InnerTextAsync());
            await Assertions.Expect(content).ToHaveTextAsync(spanish, new() { UseInnerText = true });
            Assert.True(errors.Count == 0, route + ": " + string.Join("\n", errors));
        }
    }

    [Fact]
    public async Task Validation_errors_and_open_dialog_and_toast_copy_follow_language_without_losing_work()
    {
        if (!Enabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { Locale = "es-ES", ViewportSize = new() { Width = 1440, Height = 1000 } });
        await BrowserReady.OpenAsync(page, BaseUrl + "/FormValidation");
        var name = page.Locator("#validation-first-name");
        await name.FillAsync("");
        await name.PressAsync("Tab");
        await Assertions.Expect(page.Locator("#validation-first-name-help")).ToContainTextAsync("El nombre es obligatorio.");
        await BrowserHeaderPreferences.SelectAsync(page, "language", "en");
        await Assertions.Expect(page.Locator("#validation-first-name-help")).ToContainTextAsync("First name is required.");
        await Assertions.Expect(name).ToHaveValueAsync("");
        await BrowserReady.OpenAsync(page, BaseUrl + "/Dialog");
        await page.Locator("#dialog-open").ClickAsync();
        await page.Locator("#showcase-profile-name").FillAsync("Unchanged user input");
        await BrowserHeaderPreferences.SelectAsync(page, "language", "es", keepPointerPosition: true);
        await Assertions.Expect(page.Locator(".dnet-dialog-title")).ToHaveTextAsync("Editar perfil");
        await Assertions.Expect(page.Locator("#showcase-profile-name")).ToHaveValueAsync("Unchanged user input");
        await page.Locator("#showcase-profile-cancel").ClickAsync();
        await BrowserReady.OpenAsync(page, BaseUrl + "/Toast");
        await page.Locator("#toast-show").ClickAsync();
        var toast = page.Locator(".dnet-toast");
        await toast.HoverAsync();
        await Assertions.Expect(toast.Locator(".dnet-toast__title")).ToHaveTextAsync("Tu informe está listo");
        await BrowserHeaderPreferences.SelectAsync(page, "language", "en", keepPointerPosition: true);
        await Assertions.Expect(toast.Locator(".dnet-toast__title")).ToHaveTextAsync("Your report is ready");
        await Assertions.Expect(toast.Locator(".dnet-toast__action").First).ToHaveTextAsync("View report");
    }

    private static bool Enabled => Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS") == "true";
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
}
