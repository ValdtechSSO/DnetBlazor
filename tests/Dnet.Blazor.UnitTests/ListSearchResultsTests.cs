using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.List;
using Dnet.Blazor.Components.Paginator;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ListSearchResultsTests : BunitContext
{
    public ListSearchResultsTests()
    {
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddScoped(typeof(DnetListDragAndDropService<>), typeof(DnetListDragAndDropService<>));
        Services.AddScoped<IPaginator, Paginator>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_search_with_no_matches_stays_empty_until_the_search_is_cleared(bool paginated)
    {
        var cut = RenderList(paginated);
        AssertInitialRows(cut, paginated);

        if (paginated)
        {
            await cut.Find("button[aria-label='Next page']").ClickAsync();
            AssertRows(cut, "Alina", "Marco");
        }

        await SearchAsync(cut, "ALI", "Alice", "Alina");
        if (paginated)
        {
            Assert.Equal(1, cut.FindComponent<DnetPaginator>().Instance.CurrentPage);
            Assert.Equal(2, cut.FindComponent<DnetPaginator>().Instance.TotalItems);
        }

        await SearchAsync(cut, "no-matching-person");
        if (paginated)
        {
            Assert.Equal(0, cut.FindComponent<DnetPaginator>().Instance.TotalItems);
            Assert.Contains("0 of 0", cut.Find(".dnet-paginator-range-label").TextContent);
            Assert.True(cut.Find("button[aria-label='Next page']").HasAttribute("disabled"));
        }
        else
        {
            Assert.Contains("Count: 0", cut.Find(".dnet-list-footer").TextContent);
        }

        await ClearSearchAsync(cut, paginated);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Sorting_an_empty_search_does_not_restore_or_reorder_the_unfiltered_items(bool paginated)
    {
        var cut = RenderList(paginated);
        await SearchAsync(cut, "no-matching-person");

        await cut.Find(".dnet-list-header-content").ClickAsync();
        AssertRows(cut);
        Assert.NotNull(cut.Find(".dnet-icon-asc"));

        await cut.Find(".dnet-list-header-content").ClickAsync();
        AssertRows(cut);
        Assert.NotNull(cut.Find(".dnet-icon-desc"));

        await SearchAsync(cut, "ali", "Alina", "Alice");
        await SearchAsync(cut, "no-matching-person");
        await ClearSearchAsync(cut, paginated);
    }

    private IRenderedComponent<DnetList<Person>> RenderList(bool paginated)
        => Render<DnetList<Person>>(parameters => parameters
            .Add(component => component.Items, new List<Person>
            {
                new("Zoe"), new("Alice"), new("Alina"), new("Marco")
            })
            .Add(component => component.ListOptions, new ListOptions<Person>
            {
                DisplayValueConverter = person => person.Name,
                SearchValueConverter = person => person.Name,
                SortedData = person => person.Name,
                EnableSearching = true,
                EnableSorting = true,
                EnablePagingination = paginated,
                PageSize = paginated ? 2 : 100,
                ShowHeader = true,
                ShowFooter = true,
                HeaderText = "People"
            }));

    private static async Task SearchAsync(
        IRenderedComponent<DnetList<Person>> cut,
        string query,
        params string[] expected)
    {
        // The debounce can finish a render between a DOM lookup and dispatch.
        // Perform both on the renderer thread so the handler cannot go stale.
        await cut.InvokeAsync(() => cut.Find(".dnet-list-form input").InputAsync(query));
        await cut.InvokeAsync(() => cut.Find(".dnet-list-form input").KeyUpAsync(new KeyboardEventArgs { Key = "a" }));
        cut.WaitForAssertion(() => AssertRows(cut, expected), TimeSpan.FromSeconds(3));
    }

    private static async Task ClearSearchAsync(IRenderedComponent<DnetList<Person>> cut, bool paginated)
    {
        await cut.InvokeAsync(() => cut.Find(".dnet-component-clear-button").ParentElement!.ClickAsync());
        cut.WaitForAssertion(() => AssertInitialRows(cut, paginated), TimeSpan.FromSeconds(3));
        Assert.True(string.IsNullOrEmpty(cut.Find(".dnet-list-form input").GetAttribute("value")));
        if (paginated)
        {
            Assert.Equal(1, cut.FindComponent<DnetPaginator>().Instance.CurrentPage);
            Assert.Equal(4, cut.FindComponent<DnetPaginator>().Instance.TotalItems);
        }
    }

    private static void AssertInitialRows(IRenderedComponent<DnetList<Person>> cut, bool paginated)
        => AssertRows(cut, paginated ? ["Zoe", "Alice"] : ["Zoe", "Alice", "Alina", "Marco"]);

    private static void AssertRows(IRenderedComponent<DnetList<Person>> cut, params string[] expected)
        => Assert.Equal(expected, cut.FindAll(".dnet-list-item-headline")
            .Select(element => element.TextContent.Trim()).ToArray());

    private sealed record Person(string Name);
}
