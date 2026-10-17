using Bunit;
using Dnet.Blazor.Components.Autocomplete;
using Dnet.Blazor.Components.FloatingDoubleList;
using Dnet.Blazor.Components.FloatingDoubleList.Infrastructure.Models;
using Dnet.Blazor.Components.FloatingDoubleList.Infrastructure.Services;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Grid.BlgAdvancedFilter;
using Dnet.Blazor.Components.Grid.BlgHeader;
using Dnet.Blazor.Components.Grid.BlgRow;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Components.List;
using Dnet.Blazor.Components.Paginator;
using Dnet.Blazor.Components.Select;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Dnet.Blazor.Infrastructure.Models.SearchModels.FilterModels;
using Dnet.Blazor.Infrastructure.Services;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridSelectionLocalizationTests : BunitContext
{
    public GridSelectionLocalizationTests()
    {
        Services.AddDnetBlazor();
        Services.AddSingleton(new HttpClient());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IDnetLocalizer Localizer => Services.GetRequiredService<IDnetLocalizer>();

    [Fact]
    public void Paginator_defaults_to_Spanish_without_library_registration()
    {
        using var context = new BunitContext();
        var cut = context.Render<DnetPaginator>(parameters => parameters
            .Add(component => component.CurrentPage, 2)
            .Add(component => component.PageSize, 5)
            .Add(component => component.TotalItems, 20));

        Assert.Equal("Paginación", cut.Find("nav").GetAttribute("aria-label"));
        Assert.Equal("Página siguiente", cut.Find(".dnet-paginator-navigation-next").GetAttribute("aria-label"));
        Assert.Contains("6 - 10 de 20", cut.Find(".dnet-paginator-range-label").TextContent);
        Assert.Equal("0 de 0", new Paginator().GetRangeLabel(0, 0, 0));
    }

    [Fact]
    public async Task Paginator_updates_language_without_changing_page_or_explicit_labels()
    {
        var cut = Render<DnetPaginator>(parameters => parameters
            .Add(component => component.CurrentPage, 2)
            .Add(component => component.PageSize, 5)
            .Add(component => component.TotalItems, 20)
            .Add(component => component.NextPageLabel, "Next page")
            .Add(component => component.PaginationLabel, ""));
        Assert.Equal("Next page", cut.Find(".dnet-paginator-navigation-next").GetAttribute("aria-label"));
        Assert.Equal("Página anterior", cut.Find(".dnet-paginator-navigation-previous").GetAttribute("aria-label"));

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Previous page", cut.Find(".dnet-paginator-navigation-previous").GetAttribute("aria-label")));
        Assert.Equal("", cut.Find("nav").GetAttribute("aria-label"));
        Assert.Equal("2", cut.Find("input").GetAttribute("value"));
        Assert.Contains("6 - 10 of 20", cut.Find(".dnet-paginator-range-label").TextContent);

        await cut.InvokeAsync(() => Localizer.SetCulture("es-ES"));
        cut.WaitForAssertion(() => Assert.Equal("Página anterior", cut.Find(".dnet-paginator-navigation-previous").GetAttribute("aria-label")));
        Assert.Equal("Next page", cut.Find(".dnet-paginator-navigation-next").GetAttribute("aria-label"));
        Assert.Equal(2, cut.Instance.CurrentPage);
    }

    [Fact]
    public void Paginator_service_uses_its_scope_and_keeps_range_arithmetic()
    {
        var paginator = Services.GetRequiredService<IPaginator>();
        Assert.Equal("6 - 10 de 20", paginator.GetRangeLabel(2, 5, 20));
        Localizer.SetCulture("en-GB");
        Assert.Equal("6 - 10 of 20", paginator.GetRangeLabel(2, 5, 20));
        Assert.Equal(Tuple.Create(5, 5), paginator.GetRangePage(2, 5, 20));
    }

    [Fact]
    public async Task List_language_changes_preserve_the_active_search_and_explicit_heading()
    {
        var cut = Render<DnetList<string>>(parameters => parameters
            .Add(component => component.Items, new List<string> { "Alpha", "Beta" })
            .Add(component => component.ListOptions, ListOptions("People")));
        Assert.Equal("Buscar", cut.FindComponent<DnetInputText>().Instance.AdditionalAttributes!["aria-label"]);
        await cut.InvokeAsync(() => cut.Find("input").InputAsync("Al"));
        await cut.InvokeAsync(() => cut.Find("input").KeyUpAsync(new KeyboardEventArgs { Key = "l" }));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".dnet-list-item")), TimeSpan.FromSeconds(3));

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Search", cut.Instance.Label));
        Assert.Equal("People", cut.Find(".dnet-list-header-text").TextContent.Trim());
        Assert.Equal("Al", cut.Find("input").GetAttribute("value"));
        Assert.Single(cut.FindAll(".dnet-list-item"));
        Assert.Contains("Alpha", cut.Find(".dnet-list-items").TextContent);
        Assert.DoesNotContain("Beta", cut.Find(".dnet-list-items").TextContent);
        Assert.Contains("Count: 1", cut.Find(".dnet-list-footer").TextContent);
    }

    [Fact]
    public async Task List_explicit_search_labels_and_empty_placeholder_override_localized_defaults()
    {
        var cut = Render<DnetList<string>>(parameters => parameters
            .Add(component => component.Items, new List<string> { "Alpha" })
            .Add(component => component.ListOptions, ListOptions())
            .Add(component => component.Label, "Search")
            .Add(component => component.PlaceHolder, ""));
        Assert.Equal("Search", cut.Instance.Label);
        Assert.Equal("", cut.Instance.PlaceHolder);
        Assert.Equal("Elementos", cut.Find(".dnet-list-header-text").TextContent.Trim());
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Items", cut.Find(".dnet-list-header-text").TextContent.Trim()));
        await cut.InvokeAsync(() => Localizer.SetCulture("es-ES"));
        cut.WaitForAssertion(() => Assert.Equal("Elementos", cut.Find(".dnet-list-header-text").TextContent.Trim()));
        Assert.Equal("Search", cut.Instance.Label);
        Assert.Equal("", cut.Instance.PlaceHolder);
    }

    [Fact]
    public async Task Floating_panel_keeps_transfers_while_defaults_switch_and_custom_labels_remain()
    {
        var service = new FloatingDoubleListService<string>();
        TransferredItems<string>? transfer = null;
        service.OnSelectionChange += value => transfer = value;
        var cut = Render<DnetFloatingDoubleListPanel<string>>(parameters => parameters
            .Add(component => component.LeftItems, new List<string> { "Alpha", "Beta" })
            .Add(component => component.RightItems, new List<string> { "Gamma" })
            .Add(component => component.LeftListOptions, ListOptions())
            .Add(component => component.RightListOptions, ListOptions("Assigned"))
            .Add(component => component.FloatingDoubleListService, service));
        Assert.Equal(new[] { "Disponibles", "Assigned" }, Headers(cut));
        await cut.Find(".dnet-list-item-content").ClickAsync();
        cut.WaitForAssertion(() => Assert.NotNull(transfer), TimeSpan.FromSeconds(3));
        Assert.Equal(new[] { "Gamma", "Alpha" }, transfer!.RightItems);

        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal(new[] { "Available", "Assigned" }, Headers(cut)));
        Assert.Equal("Close", cut.Find(".dnet-dialog-icon-wrapper").GetAttribute("aria-label"));
        Assert.Single(cut.FindComponents<DnetList<string>>()[0].FindAll(".dnet-list-item"));
        Assert.Equal(2, cut.FindComponents<DnetList<string>>()[1].FindAll(".dnet-list-item").Count);
        await cut.InvokeAsync(() => Localizer.SetCulture("es-ES"));
        cut.WaitForAssertion(() => Assert.Equal(new[] { "Disponibles", "Assigned" }, Headers(cut)));
        Assert.Equal(2, cut.FindComponents<DnetList<string>>()[1].FindAll(".dnet-list-item").Count);
    }

    [Fact]
    public async Task Select_refreshes_the_selected_display_without_replacing_items_or_value()
    {
        var model = new SelectionModel();
        var items = new List<Choice> { new(1, "Grid.Operator.Contains") };
        var cut = Render<DnetSelect<Choice, int>>(parameters => parameters
            .Add(component => component.Value, model.Value)
            .Add(component => component.ValueExpression, () => model.Value)
            .Add(component => component.Items, items)
            .Add(component => component.ValueConverter, item => item.Value)
            .Add(component => component.DisplayValueConverter, item => Localizer[item.LabelKey]));
        Assert.Contains("Contiene", cut.Find(".dnet-plain-select-value").TextContent);
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Contains("Contains", cut.Find(".dnet-plain-select-value").TextContent));
        Assert.Equal(1, cut.Instance.Value);
        Assert.Same(items, cut.Instance.Items);
        Assert.Equal("Select option", cut.Find("[role=combobox]").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task Select_and_autocomplete_open_content_refreshes_defaults_without_losing_rows_or_input()
    {
        var select = Render<DnetSelectPanel<string>>(parameters => parameters
            .Add(component => component.Items, new List<Dnet.Blazor.Components.Select.RowNode<string>> { new() { RowData = "Alpha" } })
            .Add(component => component.DisplayValueConverter, item => item));
        var autocomplete = Render<DnetAutocompletePanel<string>>(parameters => parameters
            .Add(component => component.Items, new List<string> { "Alpha", "Beta" })
            .Add(component => component.DisplayValueConverter, item => item)
            .Add(component => component.CurrentValue, "Al"));
        Assert.Equal("Opciones", select.Find(".dnet-select-list-header-text").TextContent.Trim());
        Assert.Equal("Sugerencias", autocomplete.Find(".dnet-autocomplete-list-header-text").TextContent.Trim());
        await select.InvokeAsync(() => Localizer.SetCulture("en-US"));
        select.WaitForAssertion(() => Assert.Equal("Options", select.Find(".dnet-select-list-header-text").TextContent.Trim()));
        autocomplete.WaitForAssertion(() => Assert.Equal("Suggestions", autocomplete.Find(".dnet-autocomplete-list-header-text").TextContent.Trim()));
        Assert.Equal("Close", select.Find(".dnet-dialog-icon-wrapper").GetAttribute("aria-label"));
        Assert.Equal("Close", autocomplete.Find(".dnet-dialog-icon-wrapper").GetAttribute("aria-label"));
        Assert.Single(select.FindAll(".dnet-select-list-item"));
        Assert.Equal(2, autocomplete.FindAll(".dnet-autocomplete-list-item").Count);
        Assert.Equal("Al", autocomplete.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Grid_filter_labels_and_selected_operators_change_without_resetting_the_model()
    {
        var model = new AdvancedFilterModel
        {
            Type = CellDataType.Text, Operator = FilterOperator.Contains, Value = "Alpha",
            Condition = FilterCondition.And, AdditionalOperator = FilterOperator.Equals, AdditionalValue = "Beta"
        };
        var cut = Render<BlgAdvancedFilterPanel>(parameters => parameters.Add(component => component.FilterData, model));
        Assert.Contains("Contiene", cut.FindAll(".dnet-plain-select")[0].TextContent);
        Assert.Equal("Restablecer", cut.Find(".blg-adv-filter-buttom-reset").TextContent.Trim());
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Contains("Contains", cut.FindAll(".dnet-plain-select")[0].TextContent));
        Assert.Contains("And", cut.FindAll(".dnet-plain-select")[1].TextContent);
        Assert.Contains("Equal", cut.FindAll(".dnet-plain-select")[2].TextContent);
        Assert.Equal("Reset", cut.Find(".blg-adv-filter-buttom-reset").TextContent.Trim());
        Assert.Equal("Filter", cut.Find(".blg-adv-filter-buttom-ok").TextContent.Trim());
        Assert.Equal("Alpha", model.Value);
        Assert.Equal("Beta", model.AdditionalValue);
        Assert.Equal(FilterCondition.And, model.Condition);
        Assert.False(cut.Find("button[type=submit]").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Grid_boolean_filter_translates_display_without_changing_invariant_values()
    {
        var model = new AdvancedFilterModel { Type = CellDataType.Boolean, Value = "true" };
        var cut = Render<BlgAdvancedFilterPanel>(parameters => parameters.Add(component => component.FilterData, model));
        Assert.Contains("Verdadero", cut.Find(".dnet-plain-select").TextContent);
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Contains("True", cut.Find(".dnet-plain-select").TextContent));
        Assert.Equal("true", model.Value);
        Assert.Equal(FilterOperator.Equals, model.Operator);
    }

    [Fact]
    public async Task Grid_header_refreshes_accessible_actions_even_when_normal_renders_are_suppressed()
    {
        var cut = Render<BlgHeader<GridItem>>(parameters => parameters
            .Add(component => component.GridColumns, new List<GridColumn<GridItem>> { new() { HeaderName = "Name", DataField = "Name", Sortable = true } })
            .Add(component => component.GroupGridColumn, new GridColumn<GridItem>())
            .Add(component => component.GroupByColumns, new List<string>())
            .Add(component => component.GridOptions, new GridOptions<GridItem>())
            .Add(component => component.Pinned, Pinned.None));
        Assert.Equal("Ordenar por Name", cut.Find(".blg-header-sort-button").GetAttribute("aria-label"));
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Equal("Sort by Name", cut.Find(".blg-header-sort-button").GetAttribute("aria-label")));
    }

    [Fact]
    public async Task Grid_row_dates_and_actions_follow_the_scoped_culture_without_mutating_data()
    {
        var row = new Dnet.Blazor.Components.Grid.Infrastructure.Entities.RowNode<GridItem>
        {
            RowNodeId = 1, RowData = new GridItem(new DateTime(2026, 10, 16)),
            RowSpanSkippedCells = new(), RowSpanTargetCells = new(), FirstSpanRow = new(), FirstSpanRowData = new()
        };
        var cut = Render<BlgRow<GridItem>>(parameters => parameters
            .Add(component => component.RowNode, row)
            .Add(component => component.RowId, 1)
            .Add(component => component.GridColumns, new List<GridColumn<GridItem>>
            {
                new() { ColumnId = 1, CellDataType = CellDataType.Date, DateFormat = "dd MMMM yyyy", CellDataFn = item => item.RowData!.Date }
            })
            .Add(component => component.GroupGridColumn, new GridColumn<GridItem>())
            .Add(component => component.GridOptions, new GridOptions<GridItem> { CheckboxSelectionColumn = true })
            .Add(component => component.CheckboxSelectionPinned, true));
        Assert.Contains("16 octubre 2026", cut.Markup);
        await cut.InvokeAsync(() => Localizer.SetCulture("en-US"));
        cut.WaitForAssertion(() => Assert.Contains("16 October 2026", cut.Markup));
        Assert.Equal("Select row 1", cut.Find("input[type=checkbox]").GetAttribute("aria-label"));
        Assert.Equal(new DateTime(2026, 10, 16), row.RowData.Date);
    }

    private static string[] Headers(IRenderedComponent<DnetFloatingDoubleListPanel<string>> cut) => cut.FindAll(".dnet-list-header-text").Select(element => element.TextContent.Trim()).ToArray();
    private static ListOptions<string> ListOptions(string? title = null) => new()
    {
        HeaderText = title, DisplayValueConverter = item => item, SearchValueConverter = item => item,
        EnableSearching = true, ShowHeader = true, ShowFooter = true
    };
    private sealed record Choice(int Value, string LabelKey);
    private sealed record GridItem(DateTime Date);
    private sealed class SelectionModel { public int Value { get; set; } = 1; }
}
