using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.Form;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

/// <summary>
/// A value the parent sets from code (a form filled by an assistant, a reset...) reaches the text, text
/// area, number and date inputs without an input event; their field's label still has to rise above it.
/// </summary>
public sealed class InputFormFieldSyncTests : BunitContext
{
    private static readonly string[] Fields = ["text-field", "notes-field", "count-field", "day-field"];

    private readonly List<FormEventService> _fieldEvents = [];

    public InputFormFieldSyncTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddTransient<IFormEventService>(_ =>
        {
            var events = new FormEventService();
            _fieldEvents.Add(events);
            return events;
        });
    }

    [Fact]
    public void Values_set_and_cleared_from_code_raise_and_lower_every_label_without_editing_the_model()
    {
        var model = new Person();
        var context = new EditContext(model);
        var fieldChanges = 0;
        context.OnFieldChanged += (_, _) => fieldChanges++;
        var cut = RenderFields(model, context);
        var inputs = InputInstances(cut);
        AssertRaised(cut, false);

        model.Text = "Adrián";
        model.Notes = "Met at the hearing";
        model.Count = 42;
        model.Day = new DateTime(2026, 11, 22);
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));

        AssertRaised(cut, true);
        Assert.Equal("Adrián", cut.Find("#text-field input").GetAttribute("value"));

        model.Text = null;
        model.Notes = null;
        model.Count = null;
        model.Day = null;
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));

        AssertRaised(cut, false);
        Assert.Equal(inputs, InputInstances(cut));
        Assert.Equal(0, fieldChanges);
        Assert.False(context.IsModified());
    }

    [Fact]
    public void Renders_that_keep_the_value_do_not_announce_it_again()
    {
        var model = new Person { Text = "Adrián", Notes = "Notes", Count = 3, Day = new DateTime(2026, 1, 2) };
        var context = new EditContext(model);
        var cut = RenderFields(model, context);
        var announcements = 0;
        foreach (var events in _fieldEvents) events.OnCurrentValue += _ => announcements++;

        for (var index = 0; index < 3; index++)
        {
            cut.Render(parameters => parameters.Add(component => component.EditContext, context));
            AssertRaised(cut, true);
        }

        Assert.Equal(0, announcements);
    }

    [Fact]
    public async Task A_focused_field_emptied_from_code_keeps_its_label_until_it_loses_the_focus()
    {
        var model = new Person { Text = "Adrián" };
        var context = new EditContext(model);
        var cut = RenderFields(model, context);

        await cut.Find("#text-field input").FocusAsync(new());
        model.Text = null;
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));
        Assert.Contains("dnet-form-field-label-raised", cut.Find("#text-field").ClassList);

        await cut.Find("#text-field input").BlurAsync(new());
        Assert.DoesNotContain("dnet-form-field-label-raised", cut.Find("#text-field").ClassList);
    }

    [Fact]
    public async Task Typing_still_updates_the_model_and_raises_the_label()
    {
        var model = new Person();
        var context = new EditContext(model);
        var cut = RenderFields(model, context);

        await cut.Find("#text-field input").InputAsync("Typed");
        cut.Render(parameters => parameters.Add(component => component.EditContext, context));

        Assert.Equal("Typed", model.Text);
        Assert.Contains("dnet-form-field-label-raised", cut.Find("#text-field").ClassList);
        Assert.True(context.IsModified(new FieldIdentifier(model, nameof(Person.Text))));
    }

    private IRenderedComponent<EditForm> RenderFields(Person model, EditContext context)
    {
        RenderFragment<EditContext> content = _ => builder =>
        {
            Field(builder, 0, "text-field", child =>
                Input<DnetInputText, string?>(child, model.Text, () => model.Text, value => model.Text = value));
            Field(builder, 10, "notes-field", child =>
                Input<DnetInputTextArea, string?>(child, model.Notes, () => model.Notes, value => model.Notes = value));
            Field(builder, 20, "count-field", child =>
                Input<DnetInputNumber<int?>, int?>(child, model.Count, () => model.Count, value => model.Count = value));
            Field(builder, 30, "day-field", child =>
                Input<DnetInputDate<DateTime?>, DateTime?>(child, model.Day, () => model.Day, value => model.Day = value));
        };
        return Render<EditForm>(parameters => parameters
            .Add(component => component.EditContext, context)
            .Add(component => component.ChildContent, content));
    }

    private static void Field(RenderTreeBuilder builder, int sequence, string id, RenderFragment input)
    {
        builder.OpenComponent<DnetFormField>(sequence);
        builder.AddAttribute(sequence + 1, nameof(DnetFormField.Label), id);
        builder.AddAttribute(sequence + 2, nameof(DnetFormField.AdditionalAttributes),
            new Dictionary<string, object> { ["id"] = id });
        builder.AddAttribute(sequence + 3, nameof(DnetFormField.ChildContent), input);
        builder.CloseComponent();
    }

    private void Input<TInput, TValue>(RenderTreeBuilder builder, TValue value, Expression<Func<TValue>> expression,
        Action<TValue> onChange) where TInput : IComponent
    {
        builder.OpenComponent<TInput>(0);
        builder.AddAttribute(1, "Value", value);
        builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create(this, onChange));
        builder.AddAttribute(3, "ValueExpression", expression);
        builder.CloseComponent();
    }

    private static object[] InputInstances(IRenderedComponent<EditForm> cut) =>
    [
        cut.FindComponent<DnetInputText>().Instance,
        cut.FindComponent<DnetInputTextArea>().Instance,
        cut.FindComponent<DnetInputNumber<int?>>().Instance,
        cut.FindComponent<DnetInputDate<DateTime?>>().Instance,
    ];

    private static void AssertRaised(IRenderedComponent<EditForm> cut, bool raised)
    {
        foreach (var id in Fields)
            Assert.True(raised == cut.Find($"#{id}").ClassList.Contains("dnet-form-field-label-raised"),
                $"{id} should {(raised ? "" : "not ")}have its label raised.");
    }

    private sealed class Person
    {
        public string? Text { get; set; }

        public string? Notes { get; set; }

        public int? Count { get; set; }

        public DateTime? Day { get; set; }
    }
}
