using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Dnet.Blazor.Components.Form;
using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Select;
using Dnet.Blazor.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

// Messages and accessible names use the name the model gives a member with [Display] or
// [DisplayName], as the framework's inputs do since .NET 11, instead of the property name.
public sealed class ModelDisplayNameTests : BunitContext
{
    public ModelDisplayNameTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddDnetLocalization();
        Services.AddTransient<IFormEventService, FormEventService>();
        Services.AddSingleton<IOverlayService>(new OverlayService());
    }

    [Fact]
    public void A_parsing_error_names_the_field_as_the_model_does()
    {
        var model = new Profile();
        var (cut, context) = RenderNumber(model, () => model.Age);

        cut.Find("input").Change("abc");

        Assert.Equal("El campo Edad debe ser un número.", Assert.Single(context.GetValidationMessages(() => model.Age)));
    }

    [Fact]
    public void A_DisplayName_attribute_names_the_field_too()
    {
        var model = new Profile();
        var (cut, context) = RenderNumber(model, () => model.Height);

        cut.Find("input").Change("tall");

        Assert.Equal("El campo Altura (cm) debe ser un número.", Assert.Single(context.GetValidationMessages(() => model.Height)));
    }

    [Fact]
    public void The_DisplayName_parameter_still_wins_over_the_model()
    {
        var model = new Profile();
        var (cut, context) = RenderNumber(model, () => model.Age, displayName: "Años cumplidos");

        cut.Find("input").Change("abc");

        Assert.Equal("El campo Años cumplidos debe ser un número.", Assert.Single(context.GetValidationMessages(() => model.Age)));
    }

    [Fact]
    public void A_member_without_a_display_name_keeps_its_property_name()
    {
        var model = new Profile();
        var (cut, context) = RenderNumber(model, () => model.Score);

        cut.Find("input").Change("abc");

        Assert.Equal("El campo Score debe ser un número.", Assert.Single(context.GetValidationMessages(() => model.Score)));
    }

    [Fact]
    public async Task A_name_from_resources_follows_the_language_of_the_library()
    {
        var model = new Profile();
        var ambient = CultureInfo.CurrentUICulture.Name;
        var (cut, context) = RenderNumber(model, () => model.Weight);

        cut.Find("input").Change("heavy");
        Assert.Equal("El campo Peso debe ser un número.", Assert.Single(context.GetValidationMessages(() => model.Weight)));

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US"));
        cut.Find("input").Change("very heavy");

        Assert.Contains("Weight (kg)", Assert.Single(context.GetValidationMessages(() => model.Weight)));
        // The name is read in the library's language without changing the thread's.
        Assert.Equal(ambient, CultureInfo.CurrentUICulture.Name);
    }

    [Fact]
    public void A_select_without_a_label_takes_the_model_name_as_its_accessible_name()
    {
        var model = new Profile();
        var context = new EditContext(model);
        var cut = Render<EditForm>(parameters => parameters
            .Add(form => form.EditContext, context)
            .Add(form => form.ChildContent, (RenderFragment<EditContext>)(_ => builder =>
            {
                builder.OpenComponent<DnetSelect<string, string?>>(0);
                builder.AddAttribute(1, nameof(DnetSelect<string, string?>.Value), model.Team);
                builder.AddAttribute(2, nameof(DnetSelect<string, string?>.ValueChanged),
                    EventCallback.Factory.Create<string?>(this, value => model.Team = value));
                builder.AddAttribute(3, nameof(DnetSelect<string, string?>.ValueExpression),
                    (Expression<Func<string?>>)(() => model.Team));
                builder.AddAttribute(4, nameof(DnetSelect<string, string?>.Items), new List<string> { "Design", "Sales" });
                builder.AddAttribute(5, nameof(DnetSelect<string, string?>.DisplayValueConverter), (Func<string, string>)(team => team));
                builder.AddAttribute(6, nameof(DnetSelect<string, string?>.ValueConverter), (Expression<Func<string, string?>>)(team => team));
                builder.CloseComponent();
            })));

        Assert.Equal("Equipo", cut.Find("[aria-haspopup=listbox]").GetAttribute("aria-label"));
    }

    private (IRenderedComponent<EditForm> Cut, EditContext Context) RenderNumber(Profile model,
        Expression<Func<int?>> field, string? displayName = null)
    {
        var context = new EditContext(model);
        var accessor = field.Compile();
        var cut = Render<EditForm>(parameters => parameters
            .Add(form => form.EditContext, context)
            .Add(form => form.ChildContent, (RenderFragment<EditContext>)(_ => builder =>
            {
                builder.OpenComponent<DnetInputNumber<int?>>(0);
                builder.AddAttribute(1, nameof(DnetInputNumber<int?>.Value), accessor());
                builder.AddAttribute(2, nameof(DnetInputNumber<int?>.ValueExpression), field);
                if (displayName is not null) builder.AddAttribute(3, nameof(DnetInputNumber<int?>.DisplayName), displayName);
                builder.CloseComponent();
            })));
        return (cut, context);
    }

    private sealed class Profile
    {
        [Display(Name = "Edad")]
        public int? Age { get; set; }

        [DisplayName("Altura (cm)")]
        public int? Height { get; set; }

        [Display(Name = nameof(ProfileNames.Weight), ResourceType = typeof(ProfileNames))]
        public int? Weight { get; set; }

        public int? Score { get; set; }

        [Display(Name = "Equipo")]
        public string? Team { get; set; }
    }

    // A resource class as the designer generates it: each name follows the UI culture.
    public static class ProfileNames
    {
        public static string Weight => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "Weight (kg)" : "Peso";
    }
}
