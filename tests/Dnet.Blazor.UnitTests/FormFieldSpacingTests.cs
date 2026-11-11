using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Infrastructure.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class FormFieldSpacingTests : BunitContext
{
    private readonly FormEventService _events = new();

    public FormFieldSpacingTests() => Services.AddSingleton<IFormEventService>(_events);

    [Fact]
    public async Task Text_fields_reserve_the_label_for_initial_values_focus_and_clearing()
    {
        var value = "Initial";
        RenderFragment content = builder =>
        {
            builder.OpenComponent<DnetInputText>(0);
            builder.AddAttribute(1, nameof(DnetInputText.Value), value);
            builder.AddAttribute(2, nameof(DnetInputText.ValueExpression), (System.Linq.Expressions.Expression<Func<string>>)(() => value));
            builder.AddAttribute(3, nameof(DnetInputText.ValueChanged), EventCallback.Factory.Create<string>(this, next => value = next));
            builder.CloseComponent();
        };
        var cut = Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, "Project")
            .Add(component => component.UseClearButton, true)
            .Add(component => component.ChildContent, content));
        Assert.Equal("Initial", cut.Find("input").GetAttribute("value"));
        AssertRaised(cut, true);

        await cut.Find("input").FocusAsync();
        await cut.Find(".dnet-component-icon-wrapper").ClickAsync(new());
        Assert.True(string.IsNullOrEmpty(value));
        Assert.True(string.IsNullOrEmpty(cut.Find("input").GetAttribute("value")));
        await cut.Find("input").BlurAsync();
        AssertRaised(cut, false);

        await cut.Find("input").FocusAsync();
        AssertRaised(cut, true);
        await cut.Find("input").BlurAsync();
        AssertRaised(cut, false);
    }

    [Theory]
    [InlineData("Notes")]
    [InlineData(null)]
    public async Task Supporting_counter_tracks_input_before_the_debounced_value_announcement(string? label)
    {
        var value = "";
        RenderFragment content = builder =>
        {
            builder.OpenComponent<DnetInputTextArea>(0);
            builder.AddAttribute(1, nameof(DnetInputTextArea.Value), value);
            builder.AddAttribute(2, nameof(DnetInputTextArea.ValueExpression), (System.Linq.Expressions.Expression<Func<string>>)(() => value));
            builder.AddAttribute(3, nameof(DnetInputTextArea.ValueChanged), EventCallback.Factory.Create<string>(this, next => value = next));
            builder.AddAttribute(4, nameof(DnetInputTextArea.MaxCharacters), 20);
            builder.AddAttribute(5, nameof(DnetInputTextArea.DebounceTime), 60_000);
            builder.CloseComponent();
        };
        var cut = Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, label)
            .Add(component => component.HintContent, "Supporting text")
            .Add(component => component.ChildContent, content));
        var announcements = 0;
        _events.OnCurrentValue += _ => announcements++;
        await cut.Find("textarea").InputAsync("Input");
        Assert.Equal("Input", value);
        Assert.Equal(0, announcements);
        Assert.Equal("5 / 20", Normalize(cut.Find(".dnet-form-field-plain-info .dnet-form-field-char-counter").TextContent));
        Assert.Empty(cut.FindAll(".dnet-form-field-header .dnet-form-field-char-counter"));
        Assert.Equal("Supporting text", cut.Find(".dnet-form-field-plain-info .dnet-hint").TextContent.Trim());

        // A programmatic clear must supersede the locally observed text, too.
        await cut.InvokeAsync(() => _events.RaiseCurrentValue(""));
        Assert.Equal("0 / 20", Normalize(cut.Find(".dnet-form-field-char-counter").TextContent));
    }

    [Fact]
    public async Task Switching_appearance_keeps_the_editing_control_and_supporting_state_mounted()
    {
        var value = "Input";
        RenderFragment content = builder =>
        {
            builder.OpenComponent<DnetInputTextArea>(0);
            builder.AddAttribute(1, nameof(DnetInputTextArea.Value), value);
            builder.AddAttribute(2, nameof(DnetInputTextArea.ValueExpression), (System.Linq.Expressions.Expression<Func<string>>)(() => value));
            builder.AddAttribute(3, nameof(DnetInputTextArea.ValueChanged), EventCallback.Factory.Create<string>(this, next => value = next));
            builder.AddAttribute(4, nameof(DnetInputTextArea.MaxCharacters), 20);
            builder.CloseComponent();
        };
        var cut = Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, "Notes")
            .Add(component => component.ChildContent, content));
        var child = cut.FindComponent<DnetInputTextArea>().Instance;
        AssertRaised(cut, true);
        await cut.Find("textarea").InputAsync("Keep this");

        foreach (var appearance in new[] { FormAppearance.Standard, FormAppearance.Legacy, FormAppearance.Fill })
        {
            cut.Render(parameters => parameters.Add(component => component.Appearance, appearance));
            Assert.Same(child, cut.FindComponent<DnetInputTextArea>().Instance);
            Assert.Equal("Keep this", cut.Find("textarea").GetAttribute("value"));
            Assert.Equal("9 / 20", Normalize(cut.Find(".dnet-form-field-plain-info .dnet-form-field-char-counter").TextContent));
        }
    }

    [Fact]
    public Task Text_input_debounces_after_enabling_immediate_response() =>
        AssertDebounceModeAsync<DnetInputText, string>("input", "Text");

    [Fact]
    public Task Textarea_debounces_after_enabling_immediate_response() =>
        AssertDebounceModeAsync<DnetInputTextArea, string>("textarea", "Notes");

    [Fact]
    public Task Number_input_debounces_after_enabling_immediate_response() =>
        AssertDebounceModeAsync<DnetInputNumber<int?>, int?>("input", "42");

    [Fact]
    public Task Date_input_debounces_after_enabling_immediate_response() =>
        AssertDebounceModeAsync<DnetInputDate<DateTime?>, DateTime?>("input", "2026-11-11");

    private async Task AssertDebounceModeAsync<TInput, TValue>(string selector, string text)
        where TInput : DnetInputBase<TValue>
    {
        TValue value = default!;
        var notifications = new List<string>();
        RenderFragment Content(bool immediate, int delay) => builder =>
        {
            builder.OpenComponent<TInput>(0);
            builder.AddAttribute(1, nameof(DnetInputText.ImmediateResponse), immediate);
            builder.AddAttribute(2, nameof(DnetInputText.DebounceTime), delay);
            builder.AddAttribute(3, nameof(DnetInputText.Value), value);
            builder.AddAttribute(4, nameof(DnetInputText.ValueExpression), (System.Linq.Expressions.Expression<Func<TValue>>)(() => value));
            builder.AddAttribute(5, nameof(DnetInputText.ValueChanged), EventCallback.Factory.Create<TValue>(this, next => value = next));
            builder.AddAttribute(6, nameof(DnetInputText.OnStopTyping), EventCallback.Factory.Create<string>(this, notifications.Add));
            builder.CloseComponent();
        };
        var field = Render<DnetFormField>(parameters => parameters
            .Add(component => component.ChildContent, Content(false, 60_000)));
        var input = field.FindComponent<TInput>().Instance;

        field.Render(parameters => parameters.Add(component => component.ChildContent, Content(true, 10)));
        Assert.Same(input, field.FindComponent<TInput>().Instance);
        await field.Find(selector).InputAsync(text);
        await field.Find(selector).KeyUpAsync(new KeyboardEventArgs { Key = "a" }).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(text, Assert.Single(notifications));

        field.Render(parameters => parameters.Add(component => component.ChildContent, Content(true, 60_000)));
        var pending = field.Find(selector).KeyUpAsync(new KeyboardEventArgs { Key = "b" });
        Assert.False(pending.IsCompleted);
        field.Render(parameters => parameters.Add(component => component.ChildContent, Content(false, 60_000)));
        await pending.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Single(notifications);
    }

    private static string Normalize(string text) => string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static void AssertRaised(IRenderedComponent<DnetFormField> cut, bool raised) =>
        Assert.Equal(raised, cut.Find(".dnet-form-field-plain-wrapper").ClassList.Contains("dnet-form-field-label-raised"));
}
