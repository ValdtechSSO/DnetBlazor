using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Bunit;
using Dnet.App.Shared.Localization;
using Dnet.Blazor.Components.Dialog;
using Dnet.Blazor.Components.Toast;
using Dnet.Blazor.Infrastructure.Services;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class SampleLocalizationTests
{
    [Fact]
    public void Application_catalog_follows_the_library_scope_and_keeps_other_users_independent()
    {
        var services = new ServiceCollection();
        services.AddDnetBlazor();
        services.AddScoped<SampleLocalizer>();
        using var provider = services.BuildServiceProvider();
        using var one = provider.CreateScope();
        using var two = provider.CreateScope();
        var first = one.ServiceProvider.GetRequiredService<SampleLocalizer>();
        var second = two.ServiceProvider.GetRequiredService<SampleLocalizer>();
        Assert.Equal("Controles de formulario", first["Forms.FormControls"]);
        one.ServiceProvider.GetRequiredService<IDnetLocalizer>().SetCulture("en-GB");
        Assert.Equal("Form controls", first["Forms.FormControls"]);
        Assert.Equal("Controles de formulario", second["Forms.FormControls"]);
        Assert.Equal("unknown-key", first["unknown-key"]);
    }

    [Fact]
    public void Both_catalogs_have_identical_keys_and_format_arguments()
    {
        var assembly = typeof(SampleLocalizer).Assembly;
        var es = new ResourceManager("Dnet.App.Shared.Localization.SampleStrings", assembly).GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var en = new ResourceManager("Dnet.App.Shared.Localization.SampleEnglishStrings", assembly).GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var keys = es.Cast<System.Collections.DictionaryEntry>().Select(x => (string)x.Key).Order().ToArray();
        Assert.True(keys.Length > 1000);
        Assert.Equal(keys, en.Cast<System.Collections.DictionaryEntry>().Select(x => (string)x.Key).Order());
        foreach (var key in keys)
        {
            var spanish = es.GetString(key)!;
            var english = en.GetString(key)!;
            Assert.False(string.IsNullOrWhiteSpace(spanish), key);
            Assert.False(string.IsNullOrWhiteSpace(english), key);
            Assert.Equal(Arguments(english), Arguments(spanish));
        }
        static string[] Arguments(string value) => Regex.Matches(value, @"\{\d+\}").Select(x => x.Value).Order().ToArray();
    }

    [Fact]
    public void Existing_result_messages_translate_both_ways_and_preserve_user_payloads()
    {
        using var context = CreateContext();
        var text = context.Services.GetRequiredService<SampleLocalizer>();
        var language = context.Services.GetRequiredService<IDnetLocalizer>();
        Assert.Equal("Guardado: Design", text.Message("Saved: Design"));
        Assert.Equal("Acción completada: Deshacer", text.Message("Action completed: Undo"));
        Assert.Equal("2 personas asignadas al proyecto.", text.Message("2 people assigned to the project."));
        Assert.Equal("2 equipos seleccionados", text.Message("2 teams selected"));
        language.SetCulture("en-US");
        Assert.Equal("Saved: Design", text.Message("Guardado: Design"));
        Assert.Equal("Assigned", text.Text("Asignados"));
    }

    [Fact]
    public void Visible_validation_changes_language_without_revalidating_or_losing_modified_state()
    {
        using var context = CreateContext();
        var model = new Profile { Name = "edited" };
        var edit = new EditContext(model);
        var field = new FieldIdentifier(model, nameof(model.Name));
        edit.NotifyFieldChanged(field);
        var errors = new ValidationMessageStore(edit);
        errors.Add(field, "First name is required.");
        var validations = 0;
        edit.OnValidationRequested += (_, _) => validations++;
        var cut = context.Render<SampleValidationMessage<string>>(p => p.AddCascadingValue(edit).Add(x => x.For, () => model.Name));
        Assert.Contains("El nombre es obligatorio.", cut.Markup);
        context.Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US");
        cut.WaitForAssertion(() => Assert.Contains("First name is required.", cut.Markup));
        Assert.True(edit.IsModified(field));
        Assert.Equal("edited", model.Name);
        Assert.Equal(0, validations);
        Assert.Single(edit.GetValidationMessages(field));
    }

    [Fact]
    public void Open_dialog_title_resolves_application_copy_without_recreating_its_content()
    {
        using var context = CreateContext();
        var texts = context.Services.GetRequiredService<SampleLocalizer>();
        var cut = context.Render<DnetDialog>(p => p.Add(x => x.Title, "Edit profile").Add(x => x.TextLocalizer, texts.Message).Add(x => x.ComponentType, typeof(EmptyContent)));
        var child = cut.FindComponent<EmptyContent>().Instance;
        Assert.Equal("Editar perfil", cut.Find("h1").TextContent);
        context.Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US");
        cut.WaitForAssertion(() => Assert.Equal("Edit profile", cut.Find("h1").TextContent));
        Assert.Same(child, cut.FindComponent<EmptyContent>().Instance);
    }

    [Fact]
    public void Toast_title_message_and_actions_follow_language_while_callbacks_remain_intact()
    {
        using var context = CreateContext();
        var texts = context.Services.GetRequiredService<SampleLocalizer>();
        var calls = 0;
        var action = new Components.Toast.Infrastructure.Models.ToastAction { Label = "Undo", OnClick = () => { calls++; return Task.CompletedTask; } };
        var cut = context.Render<DnetToast>(p => p.Add(x => x.Title, "Settings saved").Add(x => x.Text, "Your workspace preferences are up to date.").Add(x => x.TextLocalizer, texts.Message).Add(x => x.Actions, [action]));
        Assert.Contains("Ajustes guardados", cut.Markup);
        Assert.Contains("Deshacer", cut.Markup);
        context.Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US");
        cut.WaitForAssertion(() => Assert.Equal("Undo", cut.Find(".dnet-toast__action").TextContent.Trim()));
        cut.Find(".dnet-toast__action").Click();
        Assert.Equal(1, calls);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddDnetBlazor();
        context.Services.AddScoped<SampleLocalizer>();
        return context;
    }
    public sealed class EmptyContent : ComponentBase { }
    private sealed class Profile { public string Name { get; set; } = ""; }
}
