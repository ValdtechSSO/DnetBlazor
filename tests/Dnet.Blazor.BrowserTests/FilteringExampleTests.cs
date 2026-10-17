using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class FilteringExampleTests
{
    private static readonly string[] Columns =
        ["IsActive", "Age", "Name", "Gender", "Company", "Email", "Phone", "Address"];

    [Theory]
    [InlineData("", 1440)]
    [InlineData("dark", 1440)]
    [InlineData("", 390)]
    [InlineData("dark", 390)]
    public async Task Column_filters_are_visible_and_filter_the_sample_data(string theme, int width)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"),
                "true", StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL")
            ?? "http://127.0.0.1:5101").TrimEnd('/');
        var personsJson = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "person_500.json"));
        using var fixture = JsonDocument.Parse(personsJson);
        var people = fixture.RootElement.EnumerateArray().Select(person => new Person(
            person.GetProperty("name").GetString()!,
            person.GetProperty("age").ValueKind == JsonValueKind.Null ? null : person.GetProperty("age").GetInt32(),
            person.GetProperty("isActive").GetBoolean())).ToArray();

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
        await page.GotoAsync($"{baseUrl}/FilteringExample");
        await Assertions.Expect(page.Locator("html")).ToHaveAttributeAsync("data-dnet-theme", theme);

        var fields = page.Locator(".blg-header-container > .blg-filter-cell .blg-filter-field");
        await Assertions.Expect(fields).ToHaveCountAsync(Columns.Length);
        var headers = await page.Locator(".blg-header-container > [role=columnheader][title]")
            .EvaluateAllAsync<string[]>("headers => headers.map(header => header.title)");
        Assert.Equal(Columns, headers);
        foreach (var column in Columns)
        {
            var field = Field(page, column);
            await BringIntoCanvasAsync(field);
            await Assertions.Expect(field.Locator("input")).ToBeEditableAsync();
            await Assertions.Expect(field.Locator("input")).ToBeInViewportAsync(new() { Ratio = 1 });
        }
        await ExpectRowsAsync(page, people);

        await SetFilterAsync(page, "Name", "Ana Cruz");
        await ExpectRowsAsync(page, people.Where(person => person.Name.Contains("Ana Cruz", StringComparison.OrdinalIgnoreCase)).ToArray());
        await ClearFilterAsync(page, "Name");
        await ExpectRowsAsync(page, people);

        await SetFilterAsync(page, "Age", "36");
        await ExpectRowsAsync(page, people.Where(person => person.Age == 36).ToArray());
        await SetFilterAsync(page, "IsActive", "true");
        await ExpectRowsAsync(page, people.Where(person => person.Age == 36 && person.IsActive).ToArray());
        await ClearFilterAsync(page, "Age");
        await ExpectRowsAsync(page, people.Where(person => person.IsActive).ToArray());

        await SetFilterAsync(page, "Name", "__no_matching_person__");
        await ExpectRowsAsync(page, []);
        await Assertions.Expect(page.Locator(".blg-grid-status")).ToContainTextAsync("No hay resultados");
        await ClearFilterAsync(page, "Name");
        await ExpectRowsAsync(page, people.Where(person => person.IsActive).ToArray());
        await ClearFilterAsync(page, "IsActive");
        await ExpectRowsAsync(page, people);
        await Assertions.Expect(page.Locator(".blg-grid-status")).ToHaveCountAsync(0);
        Assert.Empty(errors);
    }

    private static ILocator Field(IPage page, string column) => page
        .Locator(".blg-header-container > .blg-filter-cell .blg-filter-field")
        .Nth(Array.IndexOf(Columns, column));

    private static async Task SetFilterAsync(IPage page, string column, string value)
    {
        var field = Field(page, column);
        await BringIntoCanvasAsync(field);
        await field.Locator("input").FillAsync(value);
        // The control debounces keyboard input; FillAsync only raises the input event.
        await field.Locator("input").PressAsync("End");
    }

    private static async Task ClearFilterAsync(IPage page, string column)
    {
        var field = Field(page, column);
        await BringIntoCanvasAsync(field);
        await field.Locator(".dnet-component-icon-wrapper").ClickAsync();
        await Assertions.Expect(field.Locator("input")).ToHaveValueAsync("");
    }

    private static Task BringIntoCanvasAsync(ILocator field) => field.EvaluateAsync(
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

    private static async Task ExpectRowsAsync(IPage page, Person[] matchingPeople)
    {
        // The sample paginates 25 records and virtualizes the current page. Compare actual
        // rows against fixture data, collecting both ends rather than requiring all rows in the DOM.
        var expectedPage = matchingPeople.Take(25).ToArray();
        var range = matchingPeople.Length == 0 ? "0 de 0" : $"1 - {expectedPage.Length} de {matchingPeople.Length}";
        await Assertions.Expect(page.Locator(".dnet-paginator-range-label")).ToHaveTextAsync(range);
        var grid = page.Locator(".blg-center-cols-container");
        await Assertions.Expect(grid).ToHaveAttributeAsync("aria-rowcount", expectedPage.Length.ToString());
        if (expectedPage.Length == 0)
        {
            await Assertions.Expect(grid.Locator("[role=gridcell]")).ToHaveCountAsync(0);
            return;
        }

        var observed = new Dictionary<string, RenderedPerson>();
        await ScrollVerticallyAsync(page, false);
        await Assertions.Expect(grid.Locator("[role=gridcell][aria-colindex='3']").First)
            .ToHaveTextAsync(expectedPage[0].Name);
        foreach (var person in await ReadRowsAsync(grid)) observed[person.Name] = person;
        await ScrollVerticallyAsync(page, true);
        await Assertions.Expect(grid.Locator("[role=gridcell][aria-colindex='3']").Last)
            .ToHaveTextAsync(expectedPage[^1].Name);
        foreach (var person in await ReadRowsAsync(grid)) observed[person.Name] = person;

        Assert.Equal(expectedPage.Select(person => person.Name).Order(), observed.Keys.Order());
        foreach (var person in expectedPage)
        {
            var rendered = observed[person.Name];
            Assert.Equal(person.IsActive.ToString(), rendered.IsActive);
            Assert.Equal(person.Age?.ToString() ?? "", rendered.Age);
        }
    }

    private static Task ScrollVerticallyAsync(IPage page, bool bottom) => page.EvaluateAsync(
        """
        async bottom => {
            const viewport = document.querySelector('.blg-viewport');
            viewport.scrollTop = bottom ? viewport.scrollHeight - viewport.clientHeight : 0;
            viewport.dispatchEvent(new Event('scroll'));
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        }
        """, bottom);

    private static Task<RenderedPerson[]> ReadRowsAsync(ILocator grid) => grid.EvaluateAsync<RenderedPerson[]>(
        """
        grid => [...grid.querySelectorAll('[role=gridcell][aria-colindex="3"]')].map(name => {
            const cells = [...grid.querySelectorAll('[role=gridcell]')]
                .filter(cell => cell.dataset.blgRowId === name.dataset.blgRowId);
            const text = column => cells.find(cell => cell.getAttribute('aria-colindex') === column).textContent.trim();
            return { name: name.textContent.trim(), isActive: text('1'), age: text('2') };
        })
        """);

    private sealed record Person(string Name, int? Age, bool IsActive);

    private sealed class RenderedPerson
    {
        public string Name { get; init; } = "";
        public string IsActive { get; init; } = "";
        public string Age { get; init; } = "";
    }
}
