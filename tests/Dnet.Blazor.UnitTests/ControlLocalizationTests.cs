using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.Badge;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.PickList;
using Dnet.Blazor.Components.Slider;
using Dnet.Blazor.Components.Spoiler;
using Dnet.Blazor.Components.Toast;
using Dnet.Blazor.Components.Toast.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Toast.Infrastructure.Services;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ControlLocalizationTests : BunitContext
{
    public ControlLocalizationTests() => Services.AddTransient<IFormEventService, FormEventService>();

    [Fact]
    public void Presentational_controls_have_Spanish_defaults_without_localization_registration()
    {
        var cut = Render<DnetSpoiler>(parameters => parameters.AddChildContent("Caller content"));
        Assert.Equal("Mostrar más", cut.Find("button").TextContent.Trim());
        Assert.Equal("Show more", cut.Instance.ShowLabel);
    }

    [Fact]
    public async Task PickList_resolves_each_override_and_preserves_search_and_selection_on_culture_change()
    {
        Services.AddDnetLocalization();
        Services.AddSingleton(new PickListStrings { SearchPlaceholder = "DI search", SelectedLabel = "Global selection" });
        var callbacks = 0;
        var selected = new HashSet<int> { 2 };
        var cut = Render<PickList<Person, int>>(parameters => parameters
            .Add(component => component.Items, People)
            .Add(component => component.ItemKey, person => person.Id)
            .Add(component => component.ItemTemplate, person => builder => builder.AddContent(0, person.Name))
            .Add(component => component.SearchTextSelector, person => person.Name)
            .Add(component => component.SelectedKeys, selected)
            .Add(component => component.SelectedKeysChanged,
                EventCallback.Factory.Create<IReadOnlySet<int>>(this, _ => callbacks++))
            .Add(component => component.SearchText, "br")
            .Add(component => component.Strings, new PickListStrings
            {
                // Explicit English text is still an override; equality with the old default is irrelevant.
                SearchPlaceholder = "Search",
                ClearLabel = "My clear"
            }));
        var instance = cut.Instance;
        Assert.Equal("Search", cut.Find("input[role=searchbox]").GetAttribute("placeholder"));
        Assert.Contains("Global selection: 1 / 3", cut.Markup);
        Assert.Contains("Seleccionar página", cut.Markup);
        Assert.Contains("My clear", cut.Markup);
        Assert.Single(cut.FindAll("input[type=checkbox]"));
        Assert.True(cut.Find("input[type=checkbox]").HasAttribute("checked"));

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Contains("Select page", cut.Markup));
        Assert.Equal("Search", cut.Find("input[role=searchbox]").GetAttribute("placeholder"));
        Assert.Equal("br", cut.Find("input[role=searchbox]").GetAttribute("value"));
        Assert.Contains("Global selection: 1 / 3", cut.Markup);
        Assert.Contains("My clear", cut.Markup);
        Assert.Same(instance, cut.Instance);
        Assert.Equal(new[] { 2 }, selected);
        Assert.Equal(0, callbacks);
    }

    [Fact]
    public async Task PickList_culture_change_does_not_reload_provider_data()
    {
        Services.AddDnetLocalization();
        var requests = 0;
        PickListItemsProvider<Person> provider = _ =>
        {
            requests++;
            return ValueTask.FromResult(new PickListItemsProviderResult<Person>(People, 3, 3));
        };
        var cut = Render<PickList<Person, int>>(parameters => parameters
            .Add(component => component.ItemsProvider, provider)
            .Add(component => component.ItemKey, person => person.Id)
            .Add(component => component.ItemTemplate, person => builder => builder.AddContent(0, person.Name))
            .Add(component => component.SelectedKeysChanged,
                EventCallback.Factory.Create<IReadOnlySet<int>>(this, _ => { })));
        Assert.Equal(1, requests);

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en"));

        cut.WaitForAssertion(() => Assert.Equal("Search", cut.Find("input[role=searchbox]").GetAttribute("placeholder")));
        Assert.Equal(1, requests);
        Assert.Equal(3, cut.FindAll("input[type=checkbox]").Count);
    }

    [Fact]
    public async Task Open_toast_updates_only_control_text_and_keeps_caller_content_and_identity()
    {
        Services.AddDnetLocalization();
        Services.AddSingleton<IToastService>(new ToastService(new OverlayService()));
        var cut = Render<DnetToast>(parameters => parameters
            .Add(component => component.Title, "Caller title")
            .Add(component => component.Text, "Caller message")
            .Add(component => component.DurationMilliseconds, (int?)null));
        var instance = cut.Instance;
        Assert.Equal("Cerrar notificación", cut.Find(".dnet-toast__close").GetAttribute("aria-label"));

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en"));

        cut.WaitForAssertion(() => Assert.Equal("Close notification", cut.Find(".dnet-toast__close").GetAttribute("aria-label")));
        Assert.Equal("Caller title", cut.Find(".dnet-toast__title").TextContent);
        Assert.Equal("Caller message", cut.Find(".dnet-toast__message").TextContent);
        Assert.Equal("open", cut.Find(".dnet-toast").GetAttribute("data-state"));
        Assert.Same(instance, cut.Instance);
        Assert.Empty(cut.FindAll(".dnet-toast__progress"));
    }

    [Fact]
    public async Task Toast_instance_English_override_wins_over_DI_and_language_changes()
    {
        Services.AddDnetLocalization();
        Services.AddSingleton<IToastService>(new ToastService(new OverlayService()));
        Services.AddSingleton(new ToastStrings { CloseLabel = "DI dismiss" });
        var cut = Render<DnetToast>(parameters => parameters
            .Add(component => component.Strings, new ToastStrings { CloseLabel = "Close notification" })
            .Add(component => component.DurationMilliseconds, (int?)null));
        Assert.Equal("Close notification", cut.Find("button").GetAttribute("aria-label"));
        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en"));
        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("es"));
        Assert.Equal("Close notification", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task Expanded_spoiler_and_mounted_child_survive_language_changes()
    {
        Services.AddDnetLocalization();
        var cut = Render<DnetSpoiler>(parameters => parameters.AddChildContent<ContentProbe>());
        await cut.Find("button").ClickAsync();
        var child = cut.FindComponent<ContentProbe>().Instance;
        Assert.Equal("Mostrar menos", cut.Find("button").TextContent.Trim());

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en"));

        cut.WaitForAssertion(() => Assert.Equal("Show less", cut.Find("button").TextContent.Trim()));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
        Assert.Same(child, cut.FindComponent<ContentProbe>().Instance);
    }

    [Theory]
    [InlineData(false, "El campo Quantity debe ser un número.", "The Quantity field must be a number.")]
    [InlineData(true, "El campo Departure debe ser una fecha.", "The Departure field must be a date.")]
    public async Task Existing_parse_errors_retranslate_without_model_or_modified_state_changes(
        bool date, string spanish, string english)
    {
        Services.AddDnetLocalization();
        var model = new FormModel();
        var editContext = new EditContext(model);
        var field = new FieldIdentifier(model, date ? nameof(FormModel.Departure) : nameof(FormModel.Quantity));
        var businessMessages = new ValidationMessageStore(editContext);
        businessMessages.Add(field, "Caller validation");
        var fieldChanges = 0;
        var valueChanges = 0;
        editContext.OnFieldChanged += (_, _) => fieldChanges++;
        RenderFragment<EditContext> content = _ => builder =>
        {
            if (date)
            {
                builder.OpenComponent<DnetInputDate<DateTime>>(0);
                builder.AddAttribute(1, "Value", model.Departure);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<DateTime>>)(() => model.Departure));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<DateTime>(this, _ => valueChanges++));
            }
            else
            {
                builder.OpenComponent<DnetInputNumber<int>>(0);
                builder.AddAttribute(1, "Value", model.Quantity);
                builder.AddAttribute(2, "ValueExpression", (Expression<Func<int>>)(() => model.Quantity));
                builder.AddAttribute(3, "ValueChanged", EventCallback.Factory.Create<int>(this, _ => valueChanges++));
            }
            builder.AddAttribute(4, "ImmediateResponse", false);
            builder.CloseComponent();
        };
        var cut = Render<EditForm>(parameters => parameters
            .Add(component => component.EditContext, editContext)
            .Add(component => component.ChildContent, content));
        await cut.Find("input").ChangeAsync("invalid");
        Assert.Contains(spanish, editContext.GetValidationMessages(field));
        Assert.Equal(1, fieldChanges);
        editContext.MarkAsUnmodified(field);

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en"));

        cut.WaitForAssertion(() => Assert.Contains(english, editContext.GetValidationMessages(field)));
        Assert.DoesNotContain(spanish, editContext.GetValidationMessages(field));
        Assert.Contains("Caller validation", editContext.GetValidationMessages(field));
        Assert.Equal("invalid", cut.Find("input").GetAttribute("value"));
        Assert.Equal(1, fieldChanges);
        Assert.Equal(0, valueChanges);
        Assert.False(editContext.IsModified(field));
        Assert.Equal(7, model.Quantity);
        Assert.Equal(new DateTime(2026, 10, 16), model.Departure);
    }

    [Fact]
    public async Task Visible_numeric_formats_follow_service_culture_while_native_values_stay_invariant()
    {
        Services.AddDnetLocalization();
        var ambient = CultureInfo.CurrentCulture;
        var badge = Render<DnetBadge>(parameters => parameters.Add(component => component.Content, 1.5));
        var slider = Render<DnetSlider>(parameters => parameters
            .Add(component => component.Value, 1.5)
            .Add(component => component.Step, 0.1)
            .Add(component => component.Max, 10));
        Assert.Equal("1,5", badge.Find(".dnet-badge-indicator").TextContent);
        Assert.Equal("1,5", slider.Find("input").GetAttribute("aria-valuetext"));
        Assert.Equal("1.5", slider.Find("input").GetAttribute("value"));

        await slider.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US"));

        badge.WaitForAssertion(() => Assert.Equal("1.5", badge.Find(".dnet-badge-indicator").TextContent));
        slider.WaitForAssertion(() => Assert.Equal("1.5", slider.Find("input").GetAttribute("aria-valuetext")));
        Assert.Equal("1.5", slider.Find("input").GetAttribute("value"));
        Assert.Equal(ambient, CultureInfo.CurrentCulture);
    }

    private static readonly Person[] People = [new(1, "Alpha"), new(2, "Bravo"), new(3, "Charlie")];
    private sealed record Person(int Id, string Name);
    private sealed class FormModel
    {
        public int Quantity { get; set; } = 7;
        public DateTime Departure { get; set; } = new(2026, 10, 16);
    }
    public sealed class ContentProbe : ComponentBase { }
}
