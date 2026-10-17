using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class LocalizationTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Language_changes_update_open_controls_preserve_state_and_keep_explicit_wording(string theme, int width)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        await ExpectLanguageAsync(page, "es");
        await CaptureAsync(page, theme, width, "default-es");
        var picklist = page.Locator("#localization-picklist");
        var search = picklist.Locator("input[role=searchbox]");
        await Toggle(picklist, "Alex Rivera").CheckAsync();
        await search.FillAsync("Riley");
        await Assertions.Expect(picklist.Locator(".dnet-pick-list-item")).ToHaveCountAsync(1);
        await Toggle(picklist, "Riley Parker").CheckAsync();
        await Assertions.Expect(picklist.Locator(".dnet-pick-list-counter")).ToHaveTextAsync("Seleccionados: 3 / 6");
        var summary = page.Locator("#localization-selection-value");
        await Assertions.Expect(summary).ToContainTextAsync("Alex Rivera");
        await Assertions.Expect(summary).ToContainTextAsync("Riley Parker");
        await Assertions.Expect(summary).ToContainTextAsync("Taylor Morgan");
        var date = page.Locator("#localization-date input");
        var time = page.Locator("#localization-time input");
        var originalDate = await date.InputValueAsync();
        var originalTime = await time.InputValueAsync();

        await ChangeLanguageAsync(page, "en");
        await Assertions.Expect(search).ToHaveValueAsync("Riley");
        await Assertions.Expect(search).ToHaveAttributeAsync("aria-label", "Search");
        await Assertions.Expect(Toggle(picklist, "Riley Parker")).ToBeCheckedAsync();
        await Assertions.Expect(picklist.Locator(".dnet-pick-list-counter")).ToHaveTextAsync("Selected: 3 / 6");
        var custom = page.Locator("#localization-custom-picklist");
        await Assertions.Expect(custom.GetByRole(AriaRole.Button, new() { Name = "Vaciar mi selección", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(custom.GetByRole(AriaRole.Button, new() { Name = "Select page", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(date).ToHaveValueAsync(originalDate);
        await Assertions.Expect(time).ToHaveValueAsync(originalTime);
        await Assertions.Expect(page.Locator("#localization-format-value")).ToContainTextAsync("October");
        await Assertions.Expect(page.Locator("#localization-format-value")).ToContainTextAsync("1,234.56");

        await search.FillAsync(string.Empty);
        await picklist.GetByRole(AriaRole.Button, new() { Name = "Next page", Exact = true }).ClickAsync();
        await Assertions.Expect(picklist.Locator(".dnet-paginator-page-input")).ToHaveValueAsync("2");
        var spoiler = page.Locator("#localization-spoiler .dnet-spoiler-toggle");
        await Assertions.Expect(spoiler).ToHaveTextAsync("Show more");
        await spoiler.ClickAsync();
        await Assertions.Expect(spoiler).ToHaveAttributeAsync("aria-expanded", "true");
        await ChangeLanguageAsync(page, "es");
        await Assertions.Expect(picklist.Locator(".dnet-paginator-page-input")).ToHaveValueAsync("2");
        await Assertions.Expect(Toggle(picklist, "Riley Parker")).ToBeCheckedAsync();
        await Assertions.Expect(spoiler).ToHaveTextAsync("Mostrar menos");
        await Assertions.Expect(spoiler).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(page.Locator("#localization-format-value")).ToContainTextAsync("octubre");
        await Assertions.Expect(page.Locator("#localization-format-value")).ToContainTextAsync("1.234,56");

        await date.FocusAsync();
        await date.PressAsync("ArrowDown");
        var calendar = page.Locator(".dnet-datepicker-content");
        await Assertions.Expect(calendar).ToBeVisibleAsync();
        await Assertions.Expect(calendar.Locator(".dnet-calendar-previous-button")).ToHaveAttributeAsync("aria-label", "Periodo anterior");
        var calendarPane = calendar.Locator("xpath=ancestor::*[@data-dnet-overlay-id][1]");
        var calendarId = await calendarPane.GetAttributeAsync("id");
        await Assertions.Expect(date).ToHaveAttributeAsync("aria-controls", calendarId!);
        await ChangeLanguageAsync(page, "en", whileOverlayOpen: true);
        await Assertions.Expect(calendar).ToBeVisibleAsync();
        await Assertions.Expect(calendarPane).ToHaveAttributeAsync("aria-label", "Calendar");
        await Assertions.Expect(calendar.Locator(".dnet-calendar-previous-button")).ToHaveAttributeAsync("aria-label", "Previous period");
        await Assertions.Expect(calendar.Locator(".dnet-calendar-period-button")).ToContainTextAsync("October");
        await Assertions.Expect(calendar.Locator(".dnet-calendar-day-selected")).ToHaveTextAsync("16");
        await Assertions.Expect(date).ToHaveValueAsync(originalDate);
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(calendar).ToHaveCountAsync(0);

        await time.FocusAsync();
        await time.PressAsync("ArrowDown");
        var timePanel = page.Locator(".dnet-timepicker-panel");
        await Assertions.Expect(timePanel).ToBeVisibleAsync();
        await Assertions.Expect(timePanel.Locator(".dnet-timepicker-panel-header")).ToHaveTextAsync("Choose a time");
        var timePane = timePanel.Locator("xpath=ancestor::*[@data-dnet-overlay-id][1]");
        await ChangeLanguageAsync(page, "es", whileOverlayOpen: true);
        await Assertions.Expect(timePanel.Locator(".dnet-timepicker-panel-header")).ToHaveTextAsync("Elegir hora");
        await Assertions.Expect(timePane).ToHaveAttributeAsync("aria-label", "Elegir hora");
        await Assertions.Expect(timePanel.Locator(".dnet-timepicker-action")).ToHaveTextAsync("Cancelar");
        await Assertions.Expect(timePanel.Locator("[aria-selected=true] span")).ToHaveTextAsync(originalTime);
        await Assertions.Expect(time).ToHaveValueAsync(originalTime);
        await CaptureAsync(page, theme, width, "open-time-es");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(timePanel).ToHaveCountAsync(0);

        await page.Locator("#localization-show-toast").ClickAsync();
        var toastClose = page.Locator(".dnet-toast__close");
        await Assertions.Expect(toastClose).ToHaveAttributeAsync("aria-label", "Cerrar notificación");
        await ChangeLanguageAsync(page, "en");
        await Assertions.Expect(toastClose).ToHaveAttributeAsync("aria-label", "Close notification");
        await toastClose.ClickAsync();
        await Assertions.Expect(toastClose).ToHaveCountAsync(0);
        Assert.Equal("en-US", await page.EvaluateAsync<string>("JSON.parse(localStorage.getItem('dnet.language'))"));
        await CheckLayoutAsync(page);
        await CaptureAsync(page, theme, width, "proof-en");

        await page.ReloadAsync();
        await ExpectLanguageAsync(page, "en");
        await Assertions.Expect(page.Locator("#localization-picklist input[role=searchbox]")).ToHaveAttributeAsync("aria-label", "Search");
        await Assertions.Expect(page.Locator("#localization-custom-picklist").GetByRole(AriaRole.Button,
            new() { Name = "Vaciar mi selección", Exact = true })).ToBeVisibleAsync();
        await CheckLayoutAsync(page);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task Two_browser_contexts_keep_their_language_preferences_independent()
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (first, firstErrors) = await OpenAsync(browser, "", 1440);
        var (second, secondErrors) = await OpenAsync(browser, "", 1440);
        await ChangeLanguageAsync(first, "en");
        await ExpectLanguageAsync(second, "es");
        await second.ReloadAsync();
        await ExpectLanguageAsync(second, "es");
        await first.ReloadAsync();
        await ExpectLanguageAsync(first, "en");
        Assert.True(firstErrors.Count == 0, string.Join(Environment.NewLine, firstErrors));
        Assert.True(secondErrors.Count == 0, string.Join(Environment.NewLine, secondErrors));
    }

    [Fact]
    public async Task Nested_advanced_filter_operator_updates_while_open_and_keeps_the_typed_filter()
    {
        if (!BrowserTestsEnabled) return;
        var persons = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        using var fixture = System.Text.Json.JsonDocument.Parse(persons);
        var prefix = fixture.RootElement[0].GetProperty("name").GetString()!.Split(' ')[0];
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, "dark", 1440, "/AdvancedFilteringExample", persons);
        await Assertions.Expect(page.Locator(".blg-center-cols-container [data-blg-row-id]").First).ToBeVisibleAsync();
        var header = page.Locator(".blg-header-container > [role=columnheader]:is([title='Name'], [title='Nombre'])");
        await header.EvaluateAsync(
            """
            async header => {
                const root = header.closest('.blg-root');
                const trigger = header.querySelector('.blg-adv-toggle-button-image');
                if (getComputedStyle(root).overflowX === 'auto') {
                    root.scrollLeft += trigger.getBoundingClientRect().left - root.getBoundingClientRect().left - 24;
                } else {
                    const viewport = root.querySelector('.blg-header-viewport');
                    const scrollbar = root.parentElement.querySelector('.blg-body-horizontal-scroll-viewport');
                    scrollbar.scrollLeft += trigger.getBoundingClientRect().left - viewport.getBoundingClientRect().left - 24;
                    scrollbar.dispatchEvent(new Event('scroll'));
                }
                await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            }
            """);
        await header.Locator(".blg-adv-toggle-button-image").ClickAsync();
        var filter = page.Locator(".blg-advanced-filter-container");
        await Assertions.Expect(filter).ToBeVisibleAsync();
        var input = filter.Locator(".blg-adv-filter-value input");
        await input.FillAsync(prefix);
        await input.PressAsync("Tab");
        var field = filter.Locator(".blg-adv-filter-operator");
        await Assertions.Expect(field.Locator("label")).ToContainTextAsync("Operador");
        await field.Locator(".dnet-plain-select").ClickAsync();
        var options = page.Locator(".dnet-select-list-wrapper");
        await Assertions.Expect(options).ToBeVisibleAsync();
        await Assertions.Expect(options.Locator(".dnet-select-list-header-text")).ToHaveTextAsync("Operador");
        await Assertions.Expect(options.GetByText("Contiene", new() { Exact = true })).ToBeVisibleAsync();

        await DispatchHeaderLanguageAsync(page, "en");

        await Assertions.Expect(filter).ToBeVisibleAsync();
        await Assertions.Expect(options).ToBeVisibleAsync();
        await Assertions.Expect(field.Locator("label")).ToContainTextAsync("Operator");
        await Assertions.Expect(options.Locator(".dnet-select-list-header-text")).ToHaveTextAsync("Operator");
        await Assertions.Expect(options.GetByText("Contains", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(input).ToHaveValueAsync(prefix);
        await CaptureAsync(page, "dark", 1440, "nested-filter-en");
        await options.GetByText("Contains", new() { Exact = true }).ClickAsync();
        await Assertions.Expect(options).ToHaveCountAsync(0);
        await Assertions.Expect(field.Locator(".dnet-plain-select")).ToContainTextAsync("Contains");
        await Assertions.Expect(input).ToHaveValueAsync(prefix);
        await Assertions.Expect(filter.Locator(".blg-adv-filter-buttom-ok")).ToBeEnabledAsync();
        await filter.Locator(".blg-adv-filter-buttom-reset").ClickAsync();
        await Assertions.Expect(filter).ToHaveCountAsync(0);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task Open_image_editor_and_tooltip_translate_without_replacing_or_clearing_the_canvas()
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, "dark", 1440, "/ImageEditor");
        await page.Locator("#image-editor-demo").ClickAsync();
        var editor = page.Locator(".dnet-image-editor");
        await Assertions.Expect(editor).ToBeVisibleAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Aplicar", Exact = true })).ToBeEnabledAsync();
        await Assertions.Expect(page.Locator(".dnet-dialog-title")).ToHaveTextAsync("Editor de imágenes");
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Recortar", Exact = true })).ToBeVisibleAsync();
        var canvas = editor.Locator(".dnet-editing-image-canvas");
        await page.WaitForFunctionAsync("() => document.querySelector('.dnet-editing-image-canvas')?.width > 1");
        await canvas.EvaluateAsync("canvas => canvas.dataset.localizationIdentity = 'preserved-canvas'");
        var digest = await CanvasDigestAsync(canvas);
        var selection = await editor.Locator(".dnet-crop-box-size").TextContentAsync();
        // This fixed-size icon stays in place when the adjacent toolbar text changes width.
        await editor.GetByRole(AriaRole.Button, new() { Name = "Alejar", Exact = true }).HoverAsync();
        var tooltip = page.Locator(".dnet-tooltip[role=tooltip]");
        await Assertions.Expect(tooltip).ToHaveTextAsync("Alejar");

        await DispatchHeaderLanguageAsync(page, "en");

        await Assertions.Expect(page.Locator(".dnet-dialog-title")).ToHaveTextAsync("Image Editor");
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Crop", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Reset", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true })).ToBeEnabledAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Rotate left", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(tooltip).ToBeVisibleAsync();
        await Assertions.Expect(tooltip).ToHaveTextAsync("Zoom out");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-localization-identity", "preserved-canvas");
        Assert.Equal(digest, await CanvasDigestAsync(canvas));
        Assert.Equal(selection, await editor.Locator(".dnet-crop-box-size").TextContentAsync());
        await CaptureAsync(page, "dark", 1440, "editor-tooltip-en");
        await editor.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Assertions.Expect(editor).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#image-editor-result-empty")).ToBeVisibleAsync();
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static ILocator Toggle(ILocator parent, string name) => parent.Locator(".dnet-pick-list-item")
        .Filter(new() { HasText = name }).Locator("input[type=checkbox]");

    private static Task<string> CanvasDigestAsync(ILocator canvas) => canvas.EvaluateAsync<string>(
        """
        canvas => {
            const pixels = canvas.getContext('2d').getImageData(0, 0, canvas.width, canvas.height).data;
            let hash = 2166136261;
            let visible = false;
            for (let index = 0; index < pixels.length; index++) {
                hash = Math.imul(hash ^ pixels[index], 16777619) >>> 0;
                if (index % 4 === 3 && pixels[index] > 0) visible = true;
            }
            if (!visible) throw new Error('The editor canvas has no visible image pixels');
            return `${canvas.width}x${canvas.height}:${hash}`;
        }
        """);

    private static async Task DispatchHeaderLanguageAsync(IPage page, string language)
    {
        await BrowserHeaderPreferences.SelectAsync(page, "language", language,
            keepPointerPosition: true);
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", language == "en" ? "en-US" : "es-ES");
    }

    private static async Task ChangeLanguageAsync(IPage page, string language, bool whileOverlayOpen = false)
    {
        await BrowserHeaderPreferences.SelectAsync(page, "language", language,
            keepPointerPosition: whileOverlayOpen);
        await ExpectLanguageAsync(page, language);
    }

    private static async Task ExpectLanguageAsync(IPage page, string language)
    {
        await Assertions.Expect(BrowserHeaderPreferences.Control(page, "language"))
            .ToContainTextAsync(language.ToUpperInvariant());
        await Assertions.Expect(page.Locator("#demo-localization h1")).ToHaveTextAsync(language == "en"
            ? "One language for every control" : "Un idioma para todos los controles");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("lang", language == "en" ? "en-US" : "es-ES");
    }

    private static async Task<(IPage Page, List<string> Errors)> OpenAsync(IBrowser browser, string theme, int width,
        string route = "/Localization", string? personsFixture = null)
    {
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            Locale = "es-ES",
            ViewportSize = new() { Width = width, Height = 1000 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        page.Console += (_, message) =>
        {
            if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal)) errors.Add(message.Text);
        };
        await page.RouteAsync("https://fonts.googleapis.com/**", request => request.AbortAsync());
        if (personsFixture is not null)
            await page.RouteAsync("**/sample-data/person_500.json", request => request.FulfillAsync(new()
            {
                Status = 200, ContentType = "application/json", Body = personsFixture
            }));
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        await BrowserReady.OpenAsync(page, baseUrl + route);
        if (route == "/Localization") await Assertions.Expect(page.Locator("#demo-localization")).ToBeVisibleAsync();
        else await Assertions.Expect(page.Locator("#dnet-language-selector")).ToBeVisibleAsync();
        return (page, errors);
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.EvaluateAsync<string[]>(
            """
            () => {
                const issues = [];
                const root = document.getElementById('demo-localization');
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Showcase overflows horizontally');
                if (document.documentElement.scrollWidth > innerWidth + 1) issues.push('Document overflows horizontally');
                for (const list of root.querySelectorAll('.dnet-pick-list')) {
                    if (list.scrollWidth > list.clientWidth + 1) issues.push('Translated PickList overflows horizontally');
                }
                const language = document.getElementById('dnet-language-selector').getBoundingClientRect();
                if (language.left < -1 || language.right > innerWidth + 1) issues.push('Language selector lies outside the viewport');
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join("; ", issues));
    }

    private static async Task CaptureAsync(IPage page, string theme, int width, string state)
    {
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "artifacts", "localization", "browser"));
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new() { Path = Path.Combine(directory, $"{(theme == "dark" ? "dark" : "light")}-{width}-{state}.png"), FullPage = true });
    }
}
