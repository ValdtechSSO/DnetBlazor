using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.List;
using Dnet.Blazor.Components.Paginator;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Dnet.Blazor.Localization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ListSearchResultsTests : BunitContext
{
    public ListSearchResultsTests()
    {
        Services.AddDnetLocalization(options => options.DefaultCulture = "en-US");
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddScoped(typeof(DnetListDragAndDropService<>), typeof(DnetListDragAndDropService<>));
        Services.AddScoped<IPaginator, Paginator>();
    }

    [Fact]
    public async Task UsePlainControl_switches_the_search_background_and_preserves_the_active_query()
    {
        var cut = RenderList(paginated: false);
        var input = cut.FindComponent<DnetInputText>().Instance;
        Assert.Equal(FormAppearance.Fill, cut.FindComponent<DnetFormField>().Instance.Appearance);

        await SearchAsync(cut, "ali", "Alice", "Alina");
        cut.Render(parameters => parameters.Add(list => list.UsePlainControl, false));

        Assert.Equal(FormAppearance.Standard, cut.FindComponent<DnetFormField>().Instance.Appearance);
        Assert.Same(input, cut.FindComponent<DnetInputText>().Instance);
        Assert.Equal("ali", cut.Find(".dnet-list-form input").GetAttribute("value"));
        AssertRows(cut, "Alice", "Alina");
        await ClearSearchAsync(cut, paginated: false);

        cut.Render(parameters => parameters.Add(list => list.UsePlainControl, true));
        Assert.Equal(FormAppearance.Fill, cut.FindComponent<DnetFormField>().Instance.Appearance);
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

    [Fact]
    public async Task Server_pagination_waits_for_the_requested_page_to_load()
    {
        var requestedPage = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowLoad = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = Render<DnetList<Person>>(parameters => parameters
            .Add(list => list.Items, new List<Person> { new("First") })
            .Add(list => list.ListOptions, new ListOptions<Person>
            {
                DisplayValueConverter = person => person.Name,
                EnablePagingination = true,
                EnableServerSide = true,
                PageSize = 1,
                ShowFooter = true
            })
            .Add(list => list.OnPaginationChanged, async request =>
            {
                requestedPage.SetResult(request.PaginationModel.CurrentPage);
                await allowLoad.Task;
            }));
        await cut.InvokeAsync(() => cut.Instance.SetRowData([new("First")], totalItems: 2, currentPage: 1));

        var navigation = cut.Find("button[aria-label='Next page']").ClickAsync();
        try
        {
            Assert.Equal(2, await requestedPage.Task.WaitAsync(TimeSpan.FromSeconds(3)));
            await cut.InvokeAsync(() => { });
            Assert.False(navigation.IsCompleted);
        }
        finally
        {
            allowLoad.SetResult();
        }

        await navigation.WaitAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Concurrent_server_page_requests_keep_their_own_pagination_sort_and_filters()
    {
        var firstRequest = new TaskCompletionSource<SearchModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondRequest = new TaskCompletionSource<SearchModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timeout = TimeSpan.FromSeconds(3);
        var pageReadAfterAwait = 0;
        var cut = Render<DnetList<Person>>(parameters => parameters
            .Add(list => list.Items, new List<Person> { new("First") })
            .Add(list => list.ListOptions, new ListOptions<Person>
            {
                DisplayValueConverter = person => person.Name,
                EnablePagingination = true,
                EnableServerSide = true,
                EnableSearching = true,
                ServerSideSearchColumns = [nameof(Person.Name)],
                ServerSideSortColumn = nameof(Person.Name),
                PageSize = 1,
                ShowFooter = true
            })
            .Add(list => list.OnPaginationChanged, async request =>
            {
                if (request.PaginationModel.CurrentPage == 2)
                {
                    firstRequest.SetResult(request);
                    await releaseFirst.Task;
                    pageReadAfterAwait = request.PaginationModel.CurrentPage;
                }
                else
                {
                    secondRequest.SetResult(request);
                }
            }));
        await cut.Find(".dnet-list-form input").InputAsync("First").WaitAsync(timeout);
        await cut.InvokeAsync(() => cut.Instance.SetRowData([new("First")], totalItems: 3, currentPage: 1));

        var firstNavigation = cut.Find("button[aria-label='Next page']").ClickAsync();
        try
        {
            await firstRequest.Task.WaitAsync(timeout);
            await cut.Find("button[aria-label='Next page']").ClickAsync().WaitAsync(timeout);
        }
        finally
        {
            releaseFirst.SetResult();
        }

        await firstNavigation.WaitAsync(timeout);
        var first = await firstRequest.Task.WaitAsync(timeout);
        var second = await secondRequest.Task.WaitAsync(timeout);
        Assert.Equal(2, pageReadAfterAwait);
        Assert.Equal(3, second.PaginationModel.CurrentPage);
        Assert.Equal(1, second.PaginationModel.PageSize);
        Assert.Equal(3, second.PaginationModel.ItemsCount);
        Assert.NotSame(first.PaginationModel, second.PaginationModel);
        Assert.NotSame(first.SortModel, second.SortModel);
        Assert.Equal(nameof(Person.Name), second.SortModel.ColumnName);
        var secondFilter = Assert.Single(second.AdvancedFilterModels);
        Assert.NotSame(Assert.Single(first.AdvancedFilterModels), secondFilter);
        Assert.Equal("First", secondFilter.Value);
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
        await cut.Find(".dnet-list-form input").InputAsync(query).WaitAsync(TimeSpan.FromSeconds(3));
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
