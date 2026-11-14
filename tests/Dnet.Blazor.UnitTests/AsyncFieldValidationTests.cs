using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Select;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

// .NET 11 lets a validator check a field asynchronously (EditContext.RegisterAsyncFieldValidator):
// its messages arrive after the change, through OnValidationStateChanged. The field has to show
// them then, not only at the next change.
public sealed class AsyncFieldValidationTests : BunitContext
{
    private const string RejectedMessage = "The server did not accept this value.";

    public AsyncFieldValidationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddSingleton<IOverlayService>(new OverlayService());
    }

    [Fact]
    public void Text_field_shows_an_error_that_an_async_validator_reports_after_typing()
    {
        var model = new Account();
        var context = new EditContext(model);
        var answer = RegisterServerCheck(context);
        var cut = RenderField(context, () => model.Email, (builder, field) => TextInput(builder, model, field));

        cut.Find("input").Input("taken@example.com");
        Assert.True(context.IsValidationPending(context.Field(nameof(Account.Email))));
        Assert.Empty(cut.FindAll(".dnet-error"));

        // While the server checks, the field shows a spinner and says so; the input is busy.
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".dnet-form-field-pending-indicator")));
        Assert.Equal("Comprobando…", cut.Find(".dnet-form-field-validation-status").TextContent.Trim());
        Assert.Equal("status", cut.Find(".dnet-form-field-validation-status").GetAttribute("role"));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-busy"));

        answer.SetResult(true);

        cut.WaitForAssertion(() => Assert.Contains(RejectedMessage, cut.Find(".dnet-error").TextContent));
        Assert.Empty(cut.FindAll(".dnet-form-field-pending-indicator"));
        Assert.Null(cut.Find("input").GetAttribute("aria-busy"));
    }

    [Theory]
    [InlineData(nameof(Account.Email))]
    [InlineData(nameof(Account.Notes))]
    [InlineData(nameof(Account.Age))]
    [InlineData(nameof(Account.Birth))]
    [InlineData(nameof(Account.Team))]
    public async Task Each_field_shows_an_error_that_an_async_validator_reports_after_the_change(string property)
    {
        var model = new Account();
        var context = new EditContext(model);
        var answer = RegisterServerCheck(context);
        var cut = property switch
        {
            nameof(Account.Email) => RenderField(context, () => model.Email, (b, f) => TextInput(b, model, f)),
            nameof(Account.Notes) => RenderField(context, () => model.Notes, (b, f) => TextAreaInput(b, model, f)),
            nameof(Account.Age) => RenderField(context, () => model.Age, (b, f) => NumberInput(b, model, f)),
            nameof(Account.Birth) => RenderField(context, () => model.Birth, (b, f) => DateInput(b, model, f)),
            _ => RenderField(context, () => model.Team, (b, f) => SelectInput(b, model, f)),
        };
        var field = context.Field(property);

        await cut.InvokeAsync(() => context.NotifyFieldChanged(field));
        Assert.True(context.IsValidationPending(field));
        Assert.Empty(cut.FindAll(".dnet-error"));

        await cut.InvokeAsync(() => answer.SetResult(true));

        cut.WaitForAssertion(() => Assert.Contains(RejectedMessage, cut.Find(".dnet-error").TextContent));
    }

    [Fact]
    public async Task An_async_validator_that_accepts_the_value_leaves_the_field_without_an_error()
    {
        var model = new Account();
        var context = new EditContext(model);
        var answer = RegisterServerCheck(context);
        var cut = RenderField(context, () => model.Email, (builder, field) => TextInput(builder, model, field));

        cut.Find("input").Input("free@example.com");
        await cut.InvokeAsync(() => answer.SetResult(false));

        cut.WaitForAssertion(() => Assert.False(context.IsValidationPending(context.Field(nameof(Account.Email)))));
        Assert.Empty(cut.FindAll(".dnet-error"));
    }

    [Fact]
    public async Task A_check_that_fails_says_so_without_marking_the_field_as_wrong()
    {
        var model = new Account();
        var context = new EditContext(model);
        var gate = new TaskCompletionSource();
        context.OnFieldChanged += (_, args) => context.RegisterAsyncFieldValidator(args.FieldIdentifier, async cancellationToken =>
        {
            await gate.Task.WaitAsync(cancellationToken);
            throw new InvalidOperationException("The server is down.");
        });
        var cut = RenderField(context, () => model.Email, (builder, field) => TextInput(builder, model, field));

        cut.Find("input").Input("ana@example.com");
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".dnet-form-field-pending-indicator")));

        await cut.InvokeAsync(() => gate.SetResult());

        cut.WaitForAssertion(() => Assert.Equal("No se pudo comprobar. Inténtalo de nuevo.",
            cut.Find(".dnet-form-field-validation-faulted").TextContent.Trim()));
        Assert.Equal("status", cut.Find(".dnet-form-field-validation-faulted").GetAttribute("role"));
        Assert.Single(cut.FindAll(".dnet-form-field-faulted-icon"));
        Assert.Empty(cut.FindAll(".dnet-error"));
        Assert.Empty(cut.FindAll(".dnet-form-field-pending-indicator"));
        Assert.Null(cut.Find("input").GetAttribute("aria-busy"));
    }

    [Fact]
    public void A_field_without_an_async_validator_renders_as_before()
    {
        var model = new Account { Email = "ana@example.com" };
        var context = new EditContext(model);
        var cut = RenderField(context, () => model.Email, (builder, field) => TextInput(builder, model, field));

        cut.Find("input").Input("ana.lopez@example.com");

        Assert.Empty(cut.FindAll(".dnet-form-field-pending-indicator"));
        Assert.Empty(cut.FindAll(".dnet-form-field-validation-status"));
        Assert.Null(cut.Find("input").GetAttribute("aria-busy"));
    }

    // Each change starts a check whose answer the test releases later; a rejected value adds a
    // message and notifies the form, as a real validator would.
    private static TaskCompletionSource<bool> RegisterServerCheck(EditContext context)
    {
        var messages = new ValidationMessageStore(context);
        var answer = new TaskCompletionSource<bool>();
        context.OnFieldChanged += (_, args) =>
        {
            var field = args.FieldIdentifier;
            context.RegisterAsyncFieldValidator(field, async cancellationToken =>
            {
                messages.Clear(field);
                var rejected = await answer.Task.WaitAsync(cancellationToken);
                if (rejected) messages.Add(field, RejectedMessage);
                context.NotifyValidationStateChanged();
            });
        };
        return answer;
    }

    private IRenderedComponent<EditForm> RenderField<TValue>(EditContext context, Expression<Func<TValue>> field,
        Action<RenderTreeBuilder, Expression<Func<TValue>>> input) =>
        Render<EditForm>(parameters => parameters
            .Add(form => form.EditContext, context)
            .Add(form => form.ChildContent, (RenderFragment<EditContext>)(_ => builder =>
            {
                builder.OpenComponent<DnetFormField>(0);
                builder.AddAttribute(1, nameof(DnetFormField.Label), "Field");
                builder.AddAttribute(2, nameof(DnetFormField.ErrorContent), (RenderFragment)(error =>
                {
                    error.OpenComponent<ValidationMessage<TValue>>(0);
                    error.AddAttribute(1, nameof(ValidationMessage<TValue>.For), field);
                    error.CloseComponent();
                }));
                builder.AddAttribute(3, nameof(DnetFormField.ChildContent), (RenderFragment)(child => input(child, field)));
                builder.CloseComponent();
            })));

    private void TextInput(RenderTreeBuilder builder, Account model, Expression<Func<string?>> field)
    {
        builder.OpenComponent<DnetInputText>(0);
        builder.AddAttribute(1, nameof(DnetInputText.Value), model.Email);
        builder.AddAttribute(2, nameof(DnetInputText.ValueChanged), EventCallback.Factory.Create<string?>(this, value => model.Email = value));
        builder.AddAttribute(3, nameof(DnetInputText.ValueExpression), field);
        builder.CloseComponent();
    }

    private void TextAreaInput(RenderTreeBuilder builder, Account model, Expression<Func<string?>> field)
    {
        builder.OpenComponent<DnetInputTextArea>(0);
        builder.AddAttribute(1, nameof(DnetInputTextArea.Value), model.Notes);
        builder.AddAttribute(2, nameof(DnetInputTextArea.ValueChanged), EventCallback.Factory.Create<string?>(this, value => model.Notes = value));
        builder.AddAttribute(3, nameof(DnetInputTextArea.ValueExpression), field);
        builder.CloseComponent();
    }

    private void NumberInput(RenderTreeBuilder builder, Account model, Expression<Func<int?>> field)
    {
        builder.OpenComponent<DnetInputNumber<int?>>(0);
        builder.AddAttribute(1, nameof(DnetInputNumber<int?>.Value), model.Age);
        builder.AddAttribute(2, nameof(DnetInputNumber<int?>.ValueChanged), EventCallback.Factory.Create<int?>(this, value => model.Age = value));
        builder.AddAttribute(3, nameof(DnetInputNumber<int?>.ValueExpression), field);
        builder.CloseComponent();
    }

    private void DateInput(RenderTreeBuilder builder, Account model, Expression<Func<DateTime?>> field)
    {
        builder.OpenComponent<DnetInputDate<DateTime?>>(0);
        builder.AddAttribute(1, nameof(DnetInputDate<DateTime?>.Value), model.Birth);
        builder.AddAttribute(2, nameof(DnetInputDate<DateTime?>.ValueChanged), EventCallback.Factory.Create<DateTime?>(this, value => model.Birth = value));
        builder.AddAttribute(3, nameof(DnetInputDate<DateTime?>.ValueExpression), field);
        builder.CloseComponent();
    }

    private void SelectInput(RenderTreeBuilder builder, Account model, Expression<Func<string?>> field)
    {
        builder.OpenComponent<DnetSelect<string, string?>>(0);
        builder.AddAttribute(1, nameof(DnetSelect<string, string?>.Value), model.Team);
        builder.AddAttribute(2, nameof(DnetSelect<string, string?>.ValueChanged), EventCallback.Factory.Create<string?>(this, value => model.Team = value));
        builder.AddAttribute(3, nameof(DnetSelect<string, string?>.ValueExpression), field);
        builder.AddAttribute(4, nameof(DnetSelect<string, string?>.Items), new List<string> { "Design", "Sales" });
        builder.AddAttribute(5, nameof(DnetSelect<string, string?>.DisplayValueConverter), (Func<string, string>)(team => team));
        builder.AddAttribute(6, nameof(DnetSelect<string, string?>.ValueConverter), (Expression<Func<string, string?>>)(team => team));
        builder.CloseComponent();
    }

    private sealed class Account
    {
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public int? Age { get; set; }
        public DateTime? Birth { get; set; }
        public string? Team { get; set; }
    }
}
