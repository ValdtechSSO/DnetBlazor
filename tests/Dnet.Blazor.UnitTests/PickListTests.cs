using Bunit;
using Dnet.Blazor.Components.PickList;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class PickListTests : BunitContext
{
    [Fact]
    public async Task Local_search_and_toggle_use_real_dom_events_without_mutating_the_input_set()
    {
        IReadOnlySet<int>? emitted = null;
        var selected = new HashSet<int> { 1, 4 };
        var cut = RenderLocal(selected, value => emitted = value, pageSize: 2);

        Assert.Contains("Selected: 2 / 5", cut.Markup);
        Assert.Contains("Alicante", cut.Markup);
        Assert.Contains("Bilbao", cut.Markup);
        Assert.NotNull(cut.Find(".dnet-paginator"));
        Assert.Empty(cut.FindAll(".dnet-pick-list-pager"));

        await cut.Find("input[type=search]").InputAsync("burgos");

        Assert.Contains("Burgos", cut.Markup);
        Assert.DoesNotContain("Alicante", cut.Markup);
        await cut.Find("input[type=checkbox]").ChangeAsync(true);

        Assert.NotNull(emitted);
        Assert.Equal(new[] { 1, 3, 4 }, emitted!.OrderBy(value => value));
        Assert.Equal(new[] { 1, 4 }, selected.OrderBy(value => value));
        Assert.NotSame(selected, emitted);
    }

    [Fact]
    public async Task Search_supports_initial_uncontrolled_and_parent_controlled_values()
    {
        var uncontrolled = Render<PickList<Team, int>>(parameters =>
        {
            AddLocalParameters(parameters, Teams, new HashSet<int>(), _ => { });
            parameters.Add(component => component.SearchText, "burgos");
        });
        Assert.Contains("Burgos", uncontrolled.Markup);
        Assert.Single(uncontrolled.FindAll("input[type=checkbox]"));

        string? emittedSearch = null;
        var controlled = Render<PickList<Team, int>>(parameters =>
        {
            AddLocalParameters(parameters, Teams, new HashSet<int>(), _ => { });
            parameters
                .Add(component => component.SearchText, string.Empty)
                .Add(component => component.SearchTextChanged,
                    EventCallback.Factory.Create<string?>(this, value => emittedSearch = value));
        });

        await controlled.Find("input[type=search]").InputAsync("denia");
        Assert.Equal("denia", emittedSearch);
        Assert.Contains("Alicante", controlled.Markup);

        controlled.Render(parameters => parameters.Add(component => component.SearchText, emittedSearch));
        Assert.Contains("Denia", controlled.Markup);
        Assert.Single(controlled.FindAll("input[type=checkbox]"));
    }

    [Fact]
    public async Task Toggle_off_emits_a_new_set_without_the_key()
    {
        IReadOnlySet<int>? emitted = null;
        var selected = new HashSet<int> { 1, 4 };
        var cut = RenderLocal(selected, value => emitted = value);

        await cut.Find("input[type=checkbox]").ChangeAsync(false);

        Assert.NotNull(emitted);
        Assert.Equal(new[] { 4 }, emitted);
        Assert.Equal(new[] { 1, 4 }, selected.OrderBy(value => value));
    }

    [Fact]
    public async Task Select_visible_is_page_scoped_idempotent_and_clear_removes_unloaded_keys()
    {
        IReadOnlySet<int> selected = new HashSet<int> { 99 };
        var cut = RenderLocal(selected, value => selected = value, pageSize: 2);

        await FindButton(cut, PickListStrings.Default.SelectVisibleLabel).ClickAsync();
        Assert.Equal(new[] { 1, 2, 99 }, selected.OrderBy(value => value));

        cut.Render(parameters => parameters.Add(component => component.SelectedKeys, selected));
        await FindButton(cut, PickListStrings.Default.SelectVisibleLabel).ClickAsync();
        Assert.Equal(new[] { 1, 2, 99 }, selected.OrderBy(value => value));

        cut.Render(parameters => parameters.Add(component => component.SelectedKeys, selected));
        await FindButton(cut, PickListStrings.Default.ClearLabel).ClickAsync();
        Assert.Empty(selected);
    }

    [Fact]
    public async Task Selection_survives_page_and_search_changes()
    {
        IReadOnlySet<int> selected = new HashSet<int>();
        var cut = RenderLocal(selected, value => selected = value, pageSize: 2);

        await cut.Find("input[type=checkbox]").ChangeAsync(true);
        cut.Render(parameters => parameters.Add(component => component.SelectedKeys, selected));
        await cut.Find("button[aria-label='Next page']").ClickAsync();
        await cut.Find("input[type=checkbox]").ChangeAsync(true);
        cut.Render(parameters => parameters.Add(component => component.SelectedKeys, selected));
        await cut.Find("input[type=search]").InputAsync("denia");

        Assert.Equal(new[] { 1, 3 }, selected.OrderBy(value => value));
        Assert.Contains("Selected: 2 / 5", cut.Markup);
        Assert.Contains("Denia", cut.Markup);
    }

    [Fact]
    public async Task Provider_requests_counts_only_until_they_are_invalidated()
    {
        var requests = new List<PickListItemsProviderRequest>();
        var cut = RenderProvider(CreateProvider(requests), pageSize: 2);

        Assert.Single(requests);
        Assert.True(requests[0].RequestTotalCount);
        Assert.True(requests[0].RequestFilteredCount);

        await cut.Find("button[aria-label='Next page']").ClickAsync();
        Assert.Equal(2, requests.Count);
        Assert.False(requests[1].RequestTotalCount);
        Assert.False(requests[1].RequestFilteredCount);
        Assert.Equal(1, requests[1].PageIndex);

        await cut.Find("input[type=search]").InputAsync("burgos");
        Assert.Equal(3, requests.Count);
        Assert.False(requests[2].RequestTotalCount);
        Assert.True(requests[2].RequestFilteredCount);
        Assert.Equal(0, requests[2].PageIndex);
        Assert.Equal("burgos", requests[2].SearchText);

        await cut.Instance.RefreshAsync();
        Assert.False(requests[3].RequestTotalCount);
        Assert.True(requests[3].RequestFilteredCount);

        await cut.Instance.RefreshAsync(invalidateTotalCount: true);
        Assert.True(requests[4].RequestTotalCount);
        Assert.True(requests[4].RequestFilteredCount);
    }

    [Fact]
    public void Equivalent_provider_delegate_does_not_reload_on_parent_rerender()
    {
        var holder = new ProviderHolder();
        var first = new PickListItemsProvider<Team>(holder.LoadAsync);
        var second = new PickListItemsProvider<Team>(holder.LoadAsync);
        var cut = RenderProvider(first);

        cut.Render(parameters => parameters.Add(component => component.ItemsProvider, second));

        Assert.Equal(1, holder.RequestCount);
    }

    [Fact]
    public async Task Dynamic_page_size_clamps_the_page_and_reloads_without_counts()
    {
        var requests = new List<PickListItemsProviderRequest>();
        var cut = RenderProvider(CreateProvider(requests), pageSize: 2);

        await cut.Find("button[aria-label='Last page']").ClickAsync();
        Assert.Equal(2, requests[^1].PageIndex);

        cut.Render(parameters => parameters.Add(component => component.PageSize, 3));

        Assert.Equal(1, requests[^1].PageIndex);
        Assert.Equal(3, requests[^1].PageSize);
        Assert.False(requests[^1].RequestTotalCount);
        Assert.False(requests[^1].RequestFilteredCount);
        Assert.Contains("Cádiz", cut.Markup);
    }

    [Fact]
    public async Task A_stale_provider_response_is_cancelled_and_cannot_replace_new_results()
    {
        var slowResult = new TaskCompletionSource<PickListItemsProviderResult<Team>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var slowStarted = new TaskCompletionSource<CancellationToken>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        PickListItemsProvider<Team> provider = request => request.SearchText switch
        {
            "slow" => WaitForSlowAsync(request),
            "fast" => ValueTask.FromResult(Result([new Team(5, "Fast result")], 1, request)),
            _ => ValueTask.FromResult(Result(Teams.Take(2).ToArray(), Teams.Length, request))
        };

        IReadOnlySet<int>? emittedSelection = null;
        var cut = RenderProvider(
            provider,
            selected: new HashSet<int> { 99 },
            onSelectionChanged: value => emittedSelection = value);
        var search = cut.Find("input[type=search]");
        var slowEvent = search.InputAsync("slow");
        var slowToken = await slowStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Contains("Selected: 1 / 5", cut.Markup);
        Assert.True(FindButton(cut, PickListStrings.Default.SelectVisibleLabel).HasAttribute("disabled"));
        Assert.True(cut.Find("input[type=checkbox]").HasAttribute("disabled"));
        await FindButton(cut, PickListStrings.Default.ClearLabel).ClickAsync();
        Assert.NotNull(emittedSelection);
        Assert.Empty(emittedSelection!);

        var fastEvent = search.InputAsync("fast");

        await fastEvent;
        slowResult.SetResult(new PickListItemsProviderResult<Team>(
            [new Team(1, "Stale result")], null, 1));
        await slowEvent;

        Assert.True(slowToken.IsCancellationRequested);
        Assert.Contains("Fast result", cut.Markup);
        Assert.DoesNotContain("Stale result", cut.Markup);

        async ValueTask<PickListItemsProviderResult<Team>> WaitForSlowAsync(
            PickListItemsProviderRequest request)
        {
            slowStarted.TrySetResult(request.CancellationToken);
            return await slowResult.Task;
        }
    }

    [Fact]
    public async Task Refresh_local_rechecks_a_mutated_collection_and_clamps_the_page()
    {
        var mutable = Teams.ToList();
        var cut = Render<PickList<Team, int>>(parameters => AddLocalParameters(
            parameters, mutable, new HashSet<int>(), _ => { }, pageSize: 2));

        await cut.Find("button[aria-label='Last page']").ClickAsync();
        Assert.Contains("Denia", cut.Markup);
        mutable.RemoveRange(2, 3);

        await cut.Instance.RefreshAsync();

        Assert.Contains("Selected: 0 / 2", cut.Markup);
        Assert.Contains("Alicante", cut.Markup);
        Assert.DoesNotContain("Denia", cut.Markup);
    }

    [Fact]
    public void Duplicate_keys_are_normalized_last_wins_within_the_visible_page()
    {
        var duplicateItems = new[]
        {
            new Team(1, "First value"),
            new Team(1, "Last value"),
            new Team(2, "Other value")
        };

#if DEBUG
        var diagnostic = Assert.ThrowsAny<Exception>(() =>
            Render<PickList<Team, int>>(parameters => AddLocalParameters(
                parameters, duplicateItems, new HashSet<int>(), _ => { }, pageSize: 3)));
        Assert.Contains("duplicate ItemKey", diagnostic.Message);
#else
        var cut = Render<PickList<Team, int>>(parameters => AddLocalParameters(
            parameters, duplicateItems, new HashSet<int>(), _ => { }, pageSize: 3));
        Assert.Equal(2, cut.FindAll("input[type=checkbox]").Count);
        Assert.DoesNotContain("First value", cut.Markup);
        Assert.Contains("Last value", cut.Markup);
        Assert.Contains("Selected: 0 / 3", cut.Markup);
#endif
    }

    [Fact]
    public void Empty_and_no_results_are_distinct_and_keep_global_selection_count()
    {
        var empty = Render<PickList<Team, int>>(parameters => AddLocalParameters(
            parameters, Array.Empty<Team>(), new HashSet<int> { 99 }, _ => { }));
        Assert.Contains("No items", empty.Markup);
        Assert.Contains("Selected: 1 / 0", empty.Markup);

        var noResults = RenderLocal(new HashSet<int> { 99 }, _ => { });
        noResults.Find("input[type=search]").Input("not-found");
        Assert.Contains("No results", noResults.Markup);
        Assert.Contains("Selected: 1 / 5", noResults.Markup);
    }

    [Fact]
    public void Localization_supports_DI_and_instance_override()
    {
        Services.AddSingleton(new PickListStrings
        {
            SearchPlaceholder = "Buscar global",
            SelectedLabel = "Elegidos global"
        });
        var fromDi = RenderLocal(new HashSet<int>(), _ => { });
        Assert.NotNull(fromDi.Find("input[aria-label='Buscar global']"));
        Assert.Contains("Elegidos global: 0 / 5", fromDi.Markup);
        var localStrings = new PickListStrings
        {
            SearchPlaceholder = "Buscar local",
            PaginationLabel = "Paginación",
            FirstPageLabel = "Primera",
            NextPageLabel = "Siguiente"
        };
        var overridden = RenderLocal(
            new HashSet<int>(),
            _ => { },
            strings: localStrings);

        Assert.NotNull(overridden.Find("input[aria-label='Buscar local']"));
        Assert.NotNull(overridden.Find("nav[aria-label='Paginación']"));
    }

    [Fact]
    public void Missing_selection_binding_has_an_actionable_error()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Render<PickList<Team, int>>(parameters => parameters
                .Add(component => component.Items, Teams)
                .Add(component => component.ItemKey, team => team.Id)
                .Add(component => component.ItemTemplate, TeamTemplate)
                .Add(component => component.SearchTextSelector, team => team.Name)));

        Assert.Contains("@bind-SelectedKeys", exception.Message);
    }

    [Fact]
    public void Invalid_source_and_page_size_configurations_fail_fast()
    {
        var bothSources = Assert.Throws<InvalidOperationException>(() =>
            Render<PickList<Team, int>>(parameters => AddLocalParameters(
                parameters, Teams, new HashSet<int>(), _ => { }, provider: CreateProvider([]))));
        Assert.Contains("either Items or ItemsProvider", bothSources.Message);

        var invalidPageSize = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderLocal(new HashSet<int>(), _ => { }, pageSize: 0));
        Assert.Equal("PageSize", invalidPageSize.ParamName);
    }

    [Fact]
    public void Provider_must_return_requested_counts_and_non_negative_counts()
    {
        PickListItemsProvider<Team> missingCount = request => ValueTask.FromResult(
            new PickListItemsProviderResult<Team>([], null, request.RequestFilteredCount ? 0 : null));
        var missing = Assert.Throws<InvalidOperationException>(() => RenderProvider(missingCount));
        Assert.Contains("must return TotalCount", missing.Message);

        PickListItemsProvider<Team> negativeCount = request => ValueTask.FromResult(
            new PickListItemsProviderResult<Team>([], -1, 0));
        var negative = Assert.Throws<InvalidOperationException>(() => RenderProvider(negativeCount));
        Assert.Contains("negative TotalCount", negative.Message);
    }

    private IRenderedComponent<PickList<Team, int>> RenderLocal(
        IReadOnlySet<int> selected,
        Action<IReadOnlySet<int>> onSelectionChanged,
        int pageSize = 10,
        PickListStrings? strings = null)
    {
        return Render<PickList<Team, int>>(parameters => AddLocalParameters(
            parameters, Teams, selected, onSelectionChanged, pageSize, strings));
    }

    private IRenderedComponent<PickList<Team, int>> RenderProvider(
        PickListItemsProvider<Team> provider,
        int pageSize = 10,
        IReadOnlySet<int>? selected = null,
        Action<IReadOnlySet<int>>? onSelectionChanged = null)
    {
        return Render<PickList<Team, int>>(parameters => parameters
            .Add(component => component.ItemsProvider, provider)
            .Add(component => component.ItemKey, team => team.Id)
            .Add(component => component.ItemTemplate, TeamTemplate)
            .Add(component => component.SelectedKeys, selected ?? new HashSet<int>())
            .Add(component => component.PageSize, pageSize)
            .Add(component => component.SelectedKeysChanged,
                EventCallback.Factory.Create<IReadOnlySet<int>>(
                    this, onSelectionChanged ?? (_ => { }))));
    }

    private void AddLocalParameters(
        ComponentParameterCollectionBuilder<PickList<Team, int>> parameters,
        IReadOnlyList<Team> items,
        IReadOnlySet<int> selected,
        Action<IReadOnlySet<int>> onSelectionChanged,
        int pageSize = 10,
        PickListStrings? strings = null,
        PickListItemsProvider<Team>? provider = null)
    {
        parameters
            .Add(component => component.Items, items)
            .Add(component => component.ItemsProvider, provider)
            .Add(component => component.ItemKey, team => team.Id)
            .Add(component => component.ItemTemplate, TeamTemplate)
            .Add(component => component.SearchTextSelector, team => team.Name)
            .Add(component => component.SelectedKeys, selected)
            .Add(component => component.PageSize, pageSize)
            .Add(component => component.Strings, strings)
            .Add(component => component.SelectedKeysChanged,
                EventCallback.Factory.Create(this, onSelectionChanged));
    }

    private static AngleSharp.Dom.IElement FindButton(
        IRenderedComponent<PickList<Team, int>> cut,
        string text) =>
        cut.FindAll("button").Single(button => button.TextContent.Trim() == text);

    private static PickListItemsProvider<Team> CreateProvider(
        List<PickListItemsProviderRequest> requests) => request =>
    {
        requests.Add(request);
        var filtered = string.IsNullOrWhiteSpace(request.SearchText)
            ? Teams
            : Teams.Where(team => team.Name.Contains(
                request.SearchText, StringComparison.OrdinalIgnoreCase)).ToArray();
        var page = filtered.Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToArray();
        return ValueTask.FromResult(Result(page, filtered.Length, request, Teams.Length));
    };

    private static PickListItemsProviderResult<Team> Result(
        IReadOnlyList<Team> items,
        int filteredCount,
        PickListItemsProviderRequest request,
        int? totalCount = null) => new(
            items,
            request.RequestTotalCount ? totalCount ?? filteredCount : null,
            request.RequestFilteredCount ? filteredCount : null);

    private static RenderFragment<Team> TeamTemplate =>
        team => builder => builder.AddContent(0, team.Name);

    private static readonly Team[] Teams =
    [
        new(1, "Alicante Thunder"),
        new(2, "Bilbao Titans"),
        new(3, "Burgos Bears"),
        new(4, "Cádiz Sharks"),
        new(5, "Denia Waves")
    ];

    private sealed record Team(int Id, string Name);

    private sealed class ProviderHolder
    {
        public int RequestCount { get; private set; }

        public ValueTask<PickListItemsProviderResult<Team>> LoadAsync(
            PickListItemsProviderRequest request)
        {
            RequestCount++;
            return ValueTask.FromResult(Result(Teams, Teams.Length, request));
        }
    }
}
