using System.Collections;
using System.Reflection;
using Bunit;
using Dnet.Blazor.Components.DatePicker;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Grid.BlgAdvancedFilter;
using Dnet.Blazor.Components.Overlay.Infrastructure.Enums;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Select;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Dnet.Blazor.Infrastructure.Models.SearchModels.FilterModels;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class GridAdvancedFilterPanelTests : BunitContext
{
    private readonly OverlayService _overlay = new();
    private readonly List<OverlayResult> _results = [];

    public GridAdvancedFilterPanelTests()
    {
        Services.AddDnetLocalization(options => options.DefaultCulture = "en-US");
        Services.AddSingleton<IOverlayService>(_overlay);
        Services.AddTransient<IFormEventService, FormEventService>();
        JSInterop.Mode = JSRuntimeMode.Loose;
        _overlay.OnDetach += _results.Add;
    }

    [Theory]
    [InlineData(CellDataType.Text, 5, 3)]
    [InlineData(CellDataType.Number, 5, 3)]
    [InlineData(CellDataType.Date, 5, 3)]
    [InlineData(CellDataType.Boolean, 1, 1)]
    public void Each_data_type_uses_library_fields_instead_of_native_selects(
        CellDataType type, int fieldCount, int selectCount)
    {
        var model = Model(type);
        var cut = RenderPanel(model);

        Assert.Equal(fieldCount, cut.FindComponents<DnetFormField>().Count);
        Assert.Equal(selectCount, Selects(cut).Count);
        Assert.Empty(cut.FindAll("select"));
        Assert.Empty(cut.FindAll(".form-control-sm"));
        switch (type)
        {
            case CellDataType.Text:
                Assert.Equal(2, cut.FindComponents<DnetInputText>().Count);
                break;
            case CellDataType.Number:
                Assert.Equal(2, cut.FindComponents<DnetInputNumber<int?>>().Count);
                break;
            case CellDataType.Date:
                Assert.Equal(2, cut.FindComponents<DnetDatePicker>().Count);
                Assert.All(cut.FindComponents<DnetDatePicker>(), picker =>
                    Assert.False(string.IsNullOrWhiteSpace(picker.Instance.Format)));
                break;
        }
    }

    [Theory]
    [InlineData(CellDataType.Text, "female")]
    [InlineData(CellDataType.Number, "42")]
    [InlineData(CellDataType.Date, "2026-10-16")]
    public async Task Editing_a_library_input_applies_the_typed_filter(
        CellDataType type, string value)
    {
        var model = Model(type);
        var cut = RenderPanel(model);
        Assert.True(Submit(cut).HasAttribute("disabled"));

        await cut.Find("input").InputAsync(value);

        Assert.Equal(value, model.Value);
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);

        var result = Assert.Single(_results);
        Assert.Equal(CloseReason.Ok, result.CloseReason);
        Assert.Same(model, result.ComponentData);
        Assert.NotEqual(0, result.OverlayReferenceId);
    }

    [Fact]
    public async Task Boolean_selection_commits_an_equality_filter()
    {
        var model = Model(CellDataType.Boolean);
        model.Operator = FilterOperator.None;
        var cut = RenderPanel(model);

        await SelectValueAsync(cut, 0, "true");

        Assert.Equal("true", model.Value);
        Assert.Equal(FilterOperator.Equals, model.Operator);
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Equal(CloseReason.Ok, Assert.Single(_results).CloseReason);
    }

    [Fact]
    public async Task Additional_controls_only_participate_when_a_condition_is_selected()
    {
        var model = Model(CellDataType.Text);
        model.Value = "Ada";
        model.AdditionalOperator = FilterOperator.Equals;
        model.AdditionalValue = "Grace";
        var cut = RenderPanel(model);

        Assert.True(cut.FindAll("input")[1].HasAttribute("disabled"));
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await SelectValueAsync(cut, 1, (int)FilterCondition.And);
        Assert.Equal(FilterCondition.And, model.Condition);
        Assert.False(cut.FindAll("input")[1].HasAttribute("disabled"));

        await cut.FindAll("input")[1].InputAsync(string.Empty);
        Assert.True(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Empty(_results);

        await SelectValueAsync(cut, 1, (int)FilterCondition.None);
        Assert.True(cut.FindAll("input")[1].HasAttribute("disabled"));
        Assert.False(Submit(cut).HasAttribute("disabled"));
        Assert.Equal(FilterOperator.Equals, model.AdditionalOperator);
        Assert.Equal(string.Empty, model.AdditionalValue);
    }

    [Theory]
    [InlineData(CellDataType.Number, "42", "not a number", "43")]
    [InlineData(CellDataType.Date, "2026-10-16", "not a date", "2026-10-17")]
    public async Task Parse_errors_block_submission_until_the_active_value_is_corrected(
        CellDataType type, string original, string invalid, string corrected)
    {
        var model = Model(type);
        model.Value = original;
        var cut = RenderPanel(model);
        var context = cut.FindComponent<EditForm>().Instance.EditContext!;

        await cut.Find("input").InputAsync(invalid);

        Assert.Equal(original, model.Value);
        Assert.NotEmpty(context.GetValidationMessages());
        Assert.True(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Empty(_results);

        await cut.Find("input").InputAsync(corrected);

        Assert.Equal(corrected, model.Value);
        Assert.Empty(context.GetValidationMessages());
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Equal(CloseReason.Ok, Assert.Single(_results).CloseReason);
    }

    [Theory]
    [InlineData(CellDataType.Number, "42", "43")]
    [InlineData(CellDataType.Date, "2026-10-16", "2026-10-17")]
    public async Task Disabling_a_secondary_rule_stops_its_parse_error_from_blocking_the_primary_rule(
        CellDataType type, string primary, string secondary)
    {
        var model = Model(type);
        model.Value = primary;
        model.AdditionalOperator = FilterOperator.Equals;
        model.AdditionalValue = secondary;
        model.Condition = FilterCondition.And;
        var cut = RenderPanel(model);

        await cut.FindAll("input")[1].InputAsync("invalid");
        Assert.True(Submit(cut).HasAttribute("disabled"));
        await SelectValueAsync(cut, 1, (int)FilterCondition.None);

        Assert.True(cut.FindAll("input")[1].HasAttribute("disabled"));
        Assert.Equal(secondary, model.AdditionalValue);
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Equal(CloseReason.Ok, Assert.Single(_results).CloseReason);
    }

    [Theory]
    [InlineData(CellDataType.Text, "female")]
    [InlineData(CellDataType.Number, "36")]
    [InlineData(CellDataType.Date, "2026-10-16")]
    [InlineData(CellDataType.Boolean, "false")]
    public async Task An_existing_valid_filter_can_be_submitted_immediately_after_reopening(
        CellDataType type, string value)
    {
        var model = Model(type);
        model.Value = value;
        var cut = RenderPanel(model);

        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);

        Assert.Equal(CloseReason.Ok, Assert.Single(_results).CloseReason);
        Assert.Equal(value, model.Value);
    }

    [Fact]
    public async Task Replacing_the_model_recreates_its_edit_context_and_inputs_before_submission()
    {
        var original = Model(CellDataType.Text);
        original.Value = "Ada";
        var cut = RenderPanel(original);
        var oldContext = cut.FindComponent<EditForm>().Instance.EditContext;
        var oldInput = cut.FindComponents<DnetInputText>()[0].Instance;
        var replacement = Model(CellDataType.Text);
        replacement.Value = "Grace";

        cut.FindComponent<BlgAdvancedFilterPanel>().Render(parameters =>
            parameters.Add(component => component.FilterData, replacement));

        Assert.NotSame(oldContext, cut.FindComponent<EditForm>().Instance.EditContext);
        Assert.NotSame(oldInput, cut.FindComponents<DnetInputText>()[0].Instance);
        Assert.Equal("Grace", cut.Find("input").GetAttribute("value"));
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Same(replacement, Assert.Single(_results).ComponentData);
        Assert.Equal("Ada", original.Value);
    }

    [Fact]
    public async Task A_custom_date_format_displays_local_dates_and_commits_invariant_values_for_filtering()
    {
        var model = Model(CellDataType.Date);
        model.DateFormat = "dd/MM/yyyy";
        var cut = RenderPanel(model);

        await cut.Find("input").InputAsync("31/10/2026");

        Assert.Equal("2026-10-31", model.Value);
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Same(model, Assert.Single(_results).ComponentData);
        cut.Dispose();

        var reopened = RenderPanel(model);
        Assert.Equal("31/10/2026", reopened.Find("input").GetAttribute("value"));
        Assert.False(Submit(reopened).HasAttribute("disabled"));
    }

    [Fact]
    public async Task Both_date_rules_respect_the_explicit_format_and_preserve_invariant_model_values()
    {
        var model = Model(CellDataType.Date);
        model.Value = "2026-10-16";
        model.AdditionalOperator = FilterOperator.Equals;
        model.AdditionalValue = "2026-10-18";
        model.Condition = FilterCondition.And;
        var cut = RenderPanel(model, "dd/MM/yyyy");

        Assert.Equal("16/10/2026", cut.FindAll("input")[0].GetAttribute("value"));
        Assert.Equal("18/10/2026", cut.FindAll("input")[1].GetAttribute("value"));
        await cut.FindAll("input")[1].InputAsync("31/10/2026");

        Assert.Equal("2026-10-16", model.Value);
        Assert.Equal("2026-10-31", model.AdditionalValue);
        Assert.False(Submit(cut).HasAttribute("disabled"));
        await cut.Find("form").SubmitAsync(EventArgs.Empty);
        Assert.Equal(CloseReason.Ok, Assert.Single(_results).CloseReason);
    }

    [Fact]
    public async Task Reset_preserves_the_existing_overlay_reset_contract()
    {
        var model = Model(CellDataType.Text);
        model.Value = "female";
        var cut = RenderPanel(model);

        await cut.Find(".blg-adv-filter-buttom-reset").ClickAsync(new());

        var result = Assert.Single(_results);
        Assert.Equal(CloseReason.Cancel, result.CloseReason);
        Assert.True(Assert.IsType<bool>(result.ComponentData));
        Assert.NotEqual(0, result.OverlayReferenceId);
    }

    private IRenderedComponent<CascadingValue<int>> RenderPanel(AdvancedFilterModel model, string? dateFormat = null)
    {
        var reference = _overlay.Attach(static _ => { }, new OverlayConfig());
        RenderFragment content = builder =>
        {
            builder.OpenComponent<BlgAdvancedFilterPanel>(0);
            builder.AddAttribute(1, nameof(BlgAdvancedFilterPanel.FilterData), model);
            builder.AddAttribute(2, nameof(BlgAdvancedFilterPanel.DateFormat), dateFormat);
            builder.CloseComponent();
        };
        return Render<CascadingValue<int>>(parameters => parameters
            .Add(component => component.Value, reference.GetOverlayReferenceId())
            .Add(component => component.ChildContent, content));
    }

    private static AdvancedFilterModel Model(CellDataType type) => new()
    {
        Column = "TestColumn",
        Type = type,
        Operator = type == CellDataType.Text ? FilterOperator.Contains : FilterOperator.Equals,
        Condition = FilterCondition.None,
        AdditionalOperator = FilterOperator.None,
        Value = string.Empty,
        AdditionalValue = string.Empty
    };

    private static AngleSharp.Dom.IElement Submit(IRenderedComponent<IComponent> cut) =>
        cut.Find("button[type=submit]");

    private static IReadOnlyList<IRenderedComponent<ComponentBase>> Selects(
        IRenderedComponent<IComponent> cut) => cut.FindComponents<ComponentBase>()
        .Where(component => component.Instance.GetType().IsGenericType &&
            component.Instance.GetType().GetGenericTypeDefinition() == typeof(DnetSelect<,>)).ToArray();

    private static async Task SelectValueAsync(
        IRenderedComponent<IComponent> cut, int index, object value)
    {
        var select = Selects(cut)[index].Instance;
        var selectType = select.GetType();
        var items = Assert.IsAssignableFrom<IEnumerable>(selectType.GetProperty("Items")!.GetValue(select));
        var item = items.Cast<object>().Single(candidate =>
            Equals(candidate.GetType().GetProperty("Value")?.GetValue(candidate) ?? candidate, value));
        // The option DTO is intentionally private. Invoke the control's existing
        // selection path rather than adding a public API only for these tests.
        var method = selectType.GetMethod("ItemSelectedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await cut.InvokeAsync(() => (Task)method.Invoke(select, [item])!);
    }
}
