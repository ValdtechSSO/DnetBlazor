using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class GridAdvancedFilterTests
{
    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Advanced_filter_uses_library_fields_and_nested_selects_without_losing_filter_behavior(
        string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        using var fixture = JsonDocument.Parse(personsJson);
        var names = fixture.RootElement.EnumerateArray()
            .Select(person => person.GetProperty("name").GetString()!).ToArray();
        var firstName = names[0];
        var secondName = names.First(name => name != firstName);
        var prefix = firstName.Split(' ')[0];

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = width, Height = 900 }
        });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.RouteAsync("**/sample-data/person_500.json", route => route.FulfillAsync(new()
        {
            Status = 200, ContentType = "application/json", Body = personsJson
        }));
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        await page.AddInitScriptAsync(
            $"localStorage.setItem('dnet.theme', JSON.stringify({JsonSerializer.Serialize(theme)}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await page.GotoAsync($"{baseUrl}/AdvancedFilteringExample");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);
        await ExpectResultsAsync(page, names);

        var panel = await OpenFilterAsync(page);
        await Assertions.Expect(panel.Locator("select")).ToHaveCountAsync(0);
        await Assertions.Expect(panel.Locator(".dnet-plain-select[role=combobox]")).ToHaveCountAsync(3);
        await Assertions.Expect(panel.Locator("input.mat-input-element")).ToHaveCountAsync(2);
        Assert.True(await panel.Locator("input").EvaluateAllAsync<bool>(
            "inputs => inputs.every(input => !!input.closest('.dnet-form-field-plain-control-container'))"));
        await AssertFieldsFitAsync(panel, width);
        await Assertions.Expect(ApplyButton(panel)).ToBeDisabledAsync();

        // A select is a second portal overlay. Escape must close only this child.
        var operatorSelect = panel.Locator(".blg-adv-filter-operator .dnet-plain-select");
        await Assertions.Expect(operatorSelect).ToHaveAttributeAsync("aria-disabled", "false");
        await Assertions.Expect(panel.Locator(".blg-adv-filter-additional-operator .dnet-plain-select"))
            .ToHaveAttributeAsync("aria-disabled", "true");
        await operatorSelect.ClickAsync();
        var list = page.Locator(".dnet-select-list-wrapper");
        await Assertions.Expect(list).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Listbox)).ToBeVisibleAsync();
        await Assertions.Expect(operatorSelect).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(list).ToBeHiddenAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(operatorSelect).ToHaveAttributeAsync("aria-expanded", "false");

        await ChooseAsync(page, panel, "operator", "Contains");
        await SetValueAsync(panel, "value", prefix);
        await Assertions.Expect(ApplyButton(panel)).ToBeEnabledAsync();
        await ApplyAsync(panel);
        await ExpectResultsAsync(page,
            names.Where(name => name.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToArray());

        // Changing the selected operator must update the model, not just the label.
        panel = await OpenFilterAsync(page);
        await ChooseAsync(page, panel, "operator", "Equal");
        await ApplyAsync(panel);
        await ExpectResultsAsync(page, names.Where(name => name == prefix).ToArray());

        panel = await OpenFilterAsync(page);
        await SetValueAsync(panel, "value", firstName);
        await ApplyAsync(panel);
        await ExpectResultsAsync(page, names.Where(name => name == firstName).ToArray());

        panel = await OpenFilterAsync(page);
        await ChooseAsync(page, panel, "condition", "Or");
        await Assertions.Expect(panel.Locator(".blg-adv-filter-additional-operator .dnet-plain-select"))
            .ToHaveAttributeAsync("aria-disabled", "false");
        await ChooseAsync(page, panel, "additional-operator", "Equal");
        await SetValueAsync(panel, "additional-value", secondName);
        await AssertFieldsFitAsync(panel, width);
        await ApplyAsync(panel);
        await ExpectResultsAsync(page, names.Where(name => name == firstName || name == secondName).ToArray());

        panel = await OpenFilterAsync(page);
        await panel.Locator(".blg-adv-filter-buttom-reset").ClickAsync();
        await Assertions.Expect(panel).ToBeHiddenAsync();
        await ExpectResultsAsync(page, names);
        panel = await OpenFilterAsync(page);
        await Assertions.Expect(panel.Locator(".blg-adv-filter-value input")).ToHaveValueAsync("");
        await Assertions.Expect(panel.Locator(".blg-adv-filter-additional-value input")).ToHaveValueAsync("");
        await Assertions.Expect(ApplyButton(panel)).ToBeDisabledAsync();
        await panel.Locator(".blg-adv-filter-buttom-reset").ClickAsync();
        await Assertions.Expect(panel).ToBeHiddenAsync();
        Assert.Empty(errors);
    }

    private static ILocator ApplyButton(ILocator panel) => panel.Locator(".blg-adv-filter-buttom-ok");

    private static async Task<ILocator> OpenFilterAsync(IPage page)
    {
        var header = page.Locator(".blg-header-container > [role=columnheader][title='Name']");
        await header.EvaluateAsync(
            """
            async header => {
                const root = header.closest('.blg-root');
                const trigger = header.querySelector('.blg-adv-toggle-button-image, .blg-adv-toggle-button-filtered-image');
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
        await header.Locator(".blg-adv-toggle-button-image, .blg-adv-toggle-button-filtered-image").ClickAsync();
        var panel = page.Locator(".blg-advanced-filter-container");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        return panel;
    }

    private static async Task ChooseAsync(IPage page, ILocator panel, string field, string option)
    {
        var trigger = panel.Locator($".blg-adv-filter-{field} .dnet-plain-select");
        await trigger.ClickAsync();
        var list = page.Locator(".dnet-select-list-wrapper");
        await Assertions.Expect(list).ToBeVisibleAsync();
        var choice = list.Locator(".dnet-select-list-item").Filter(new()
        {
            Has = page.GetByText(option, new() { Exact = true })
        });
        if (await choice.CountAsync() == 0)
        {
            await list.Locator(".dnet-select-list-items").EvaluateAsync(
                "items => { items.scrollTop = items.scrollHeight; items.dispatchEvent(new Event('scroll')); }");
        }
        await choice.ClickAsync();
        await Assertions.Expect(list).ToBeHiddenAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToContainTextAsync(option);
    }

    private static async Task SetValueAsync(ILocator panel, string field, string value)
    {
        var input = panel.Locator($".blg-adv-filter-{field} input");
        await input.FillAsync(value);
        await input.PressAsync("Tab");
        await Assertions.Expect(input).ToHaveValueAsync(value);
    }

    private static async Task ApplyAsync(ILocator panel)
    {
        await ApplyButton(panel).ClickAsync();
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    private static async Task ExpectResultsAsync(IPage page, string[] expected)
    {
        var pageNames = expected.Take(25).ToArray();
        var range = expected.Length == 0 ? "0 of 0" : $"1 - {pageNames.Length} of {expected.Length}";
        await Assertions.Expect(page.Locator(".dnet-paginator-range-label")).ToHaveTextAsync(range);
        var grid = page.Locator(".blg-center-cols-container");
        await Assertions.Expect(grid).ToHaveAttributeAsync("aria-rowcount", pageNames.Length.ToString());
        if (pageNames.Length == 0)
        {
            await Assertions.Expect(grid.Locator("[role=gridcell]")).ToHaveCountAsync(0);
            return;
        }
        await Assertions.Expect(grid.Locator("[role=gridcell][aria-colindex='3']").First)
            .ToHaveTextAsync(pageNames[0]);
        // Filtered one/two-result cases must contain exactly those fixture records.
        if (pageNames.Length <= 2)
            await Assertions.Expect(grid.Locator("[role=gridcell][aria-colindex='3']"))
                .ToHaveTextAsync(pageNames);
    }

    private static async Task AssertFieldsFitAsync(ILocator panel, int viewportWidth)
    {
        var metrics = await panel.EvaluateAsync<FieldGeometry[]>(
            """
            panel => {
                const panelRect = panel.getBoundingClientRect();
                return [...panel.querySelectorAll('.dnet-form-field-plain-control-container')].map(field => {
                    const rect = field.getBoundingClientRect();
                    const control = field.querySelector('input, [role=combobox]').getBoundingClientRect();
                    return {
                        height: rect.height, left: rect.left, right: rect.right,
                        panelClearance: Math.min(rect.left - panelRect.left, panelRect.right - rect.right,
                            rect.top - panelRect.top, panelRect.bottom - rect.bottom),
                        controlClearance: Math.min(control.left - rect.left, rect.right - control.right,
                            control.top - rect.top, rect.bottom - control.bottom)
                    };
                });
            }
            """);
        Assert.Equal(5, metrics.Length);
        foreach (var field in metrics)
        {
            Assert.InRange(field.Height, 55.2, 56.8);
            Assert.InRange(field.Left, -0.8, viewportWidth);
            Assert.InRange(field.Right, 0, viewportWidth + 0.8);
            Assert.True(field.PanelClearance >= -0.8, "A library field is clipped by the advanced filter panel.");
            Assert.True(field.ControlClearance >= -0.8, "An input or select extends outside its library field.");
        }
    }

    private sealed class FieldGeometry
    {
        public double Height { get; init; }
        public double Left { get; init; }
        public double Right { get; init; }
        public double PanelClearance { get; init; }
        public double ControlClearance { get; init; }
    }
}
