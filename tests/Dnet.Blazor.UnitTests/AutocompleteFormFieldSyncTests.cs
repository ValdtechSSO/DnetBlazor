using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.Autocomplete;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class AutocompleteFormFieldSyncTests : BunitContext
{
    private readonly List<FormEventService> _fieldEvents = [];
    private readonly CustomFilter<Contact> _filter = CustomFilter<Contact>.FilterBy(contact => contact.Name);
    private readonly CustomSort<Contact> _sort = CustomSort<Contact>.ByAscending<string>(contact => contact.Name);

    public AutocompleteFormFieldSyncTests()
    {
        Services.AddTransient<IFormEventService>(_ =>
        {
            var events = new FormEventService();
            _fieldEvents.Add(events);
            return events;
        });
        Services.AddSingleton<IOverlayService>(new OverlayService());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Initial contact")]
    public void External_replacement_and_clearing_update_both_labels_and_clear_actions_without_editing_the_model(string initial)
    {
        var model = new Selection { Name = initial };
        var context = new EditContext(model);
        var valueChanges = 0;
        var fieldChanges = 0;
        context.OnFieldChanged += (_, _) => fieldChanges++;
        var cut = RenderSharedFields(model, context, value => { valueChanges++; model.Name = value; });
        var original = cut.FindComponents<DnetAutocomplete<Contact, string>>().Select(component => component.Instance).ToArray();
        AssertState(cut, initial);

        foreach (var value in new[] { "External contact", string.Empty, null, "Replacement contact" })
        {
            model.Name = value;
            cut.Render(parameters => parameters.Add(component => component.EditContext, context));

            AssertState(cut, value);
            var current = cut.FindComponents<DnetAutocomplete<Contact, string>>();
            Assert.Same(original[0], current[0].Instance);
            Assert.Same(original[1], current[1].Instance);
            Assert.Equal(0, valueChanges);
            Assert.Equal(0, fieldChanges);
            Assert.False(context.IsModified());
        }
    }

    [Fact]
    public void Unrelated_parent_renders_do_not_repeat_value_announcements_or_start_a_render_cycle()
    {
        var model = new Selection { Name = "Existing contact" };
        var context = new EditContext(model);
        var cut = RenderSharedFields(model, context, value => model.Name = value);
        var announcements = 0;
        foreach (var events in _fieldEvents) events.OnCurrentValue += _ => announcements++;

        for (var index = 0; index < 3; index++)
        {
            cut.Render(parameters => parameters.Add(component => component.EditContext, context));
            AssertState(cut, "Existing contact");
        }

        Assert.Equal(0, announcements);
    }

    [Fact]
    public async Task Editing_one_shared_field_and_clearing_the_other_keep_their_labels_in_sync()
    {
        var model = new Selection();
        var context = new EditContext(model);
        var cut = RenderSharedFields(model, context, value => model.Name = value);
        var original = cut.FindComponents<DnetAutocomplete<Contact, string>>().Select(component => component.Instance).ToArray();

        await cut.Find("#first-contact input").InputAsync("Typed contact");
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));
        Assert.Equal("Typed contact", model.Name);
        AssertState(cut, model.Name);
        Assert.True(context.IsModified(new FieldIdentifier(model, nameof(Selection.Name))));

        await cut.Find("#second-contact .dnet-component-icon-wrapper").ClickAsync(new());
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));
        Assert.True(string.IsNullOrEmpty(model.Name));
        AssertState(cut, model.Name);
        var current = cut.FindComponents<DnetAutocomplete<Contact, string>>();
        Assert.Same(original[0], current[0].Instance);
        Assert.Same(original[1], current[1].Instance);
    }

    private IRenderedComponent<EditForm> RenderSharedFields(Selection model, EditContext context, Action<string> onChange)
    {
        RenderFragment<EditContext> content = _ => builder =>
        {
            foreach (var id in new[] { "first-contact", "second-contact" })
            {
                builder.OpenRegion(0);
                builder.OpenComponent<DnetFormField>(0);
                builder.AddAttribute(1, nameof(DnetFormField.Label), "Contact name");
                builder.AddAttribute(2, nameof(DnetFormField.UseClearButton), true);
                builder.AddAttribute(3, nameof(DnetFormField.AdditionalAttributes),
                    new Dictionary<string, object> { ["id"] = id });
                builder.AddAttribute(4, nameof(DnetFormField.ChildContent), (RenderFragment)(child =>
                {
                    child.OpenComponent<DnetAutocomplete<Contact, string>>(0);
                    child.AddAttribute(1, nameof(DnetAutocomplete<Contact, string>.Value), model.Name);
                    child.AddAttribute(2, nameof(DnetAutocomplete<Contact, string>.ValueExpression),
                        (Expression<Func<string>>)(() => model.Name!));
                    child.AddAttribute(3, nameof(DnetAutocomplete<Contact, string>.ValueChanged),
                        EventCallback.Factory.Create<string>(this, onChange));
                    child.AddAttribute(4, nameof(DnetAutocomplete<Contact, string>.Items), new List<Contact>());
                    child.AddAttribute(5, nameof(DnetAutocomplete<Contact, string>.DisplayValueConverter),
                        (Func<Contact, string>)(contact => contact.Name));
                    child.AddAttribute(6, nameof(DnetAutocomplete<Contact, string>.ValueConverter),
                        (Expression<Func<Contact, string>>)(contact => contact.Name));
                    child.AddAttribute(7, nameof(DnetAutocomplete<Contact, string>.FilterBy), _filter);
                    child.AddAttribute(8, nameof(DnetAutocomplete<Contact, string>.SortBy), _sort);
                    child.CloseComponent();
                }));
                builder.CloseComponent();
                builder.CloseRegion();
            }
        };
        return Render<EditForm>(parameters => parameters
            .Add(component => component.EditContext, context)
            .Add(component => component.ChildContent, content));
    }

    private static void AssertState(IRenderedComponent<EditForm> cut, string? expected)
    {
        foreach (var id in new[] { "first-contact", "second-contact" })
        {
            var field = cut.Find($"#{id}");
            Assert.Equal(expected ?? string.Empty, field.QuerySelector("input")!.GetAttribute("value") ?? string.Empty);
            var hasValue = !string.IsNullOrEmpty(expected);
            Assert.Equal(hasValue, field.ClassList.Contains("dnet-form-field-label-raised"));
            Assert.Equal(hasValue ? 1 : 0, field.QuerySelectorAll(".dnet-component-clear-button").Length);
        }
    }

    private sealed class Selection
    {
        public string? Name { get; set; } = string.Empty;
    }

    private sealed class Contact
    {
        public string Name { get; set; } = string.Empty;
    }
}
