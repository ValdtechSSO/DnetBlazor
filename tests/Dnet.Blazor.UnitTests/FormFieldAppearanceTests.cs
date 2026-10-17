using Bunit;
using Dnet.Blazor.Components.Form;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class FormFieldAppearanceTests : BunitContext
{
    public FormFieldAppearanceTests() => Services.AddTransient<IFormEventService, FormEventService>();

    [Fact]
    public void Fields_are_filled_by_default_and_forward_consumer_styles_and_attributes()
    {
        var cut = Render<DnetFormField>(parameters => parameters
            .Add(component => component.Label, "Project")
            .AddUnmatched("id", "project-field")
            .AddUnmatched("class", "custom-field")
            .AddUnmatched("style", "--dnet-form-field-background: var(--dnet-sys-surface);")
            .AddChildContent("<input value='Initial' />"));
        var root = cut.Find("#project-field");
        Assert.Equal(FormAppearance.Fill, cut.Instance.Appearance);
        Assert.Contains("dnet-form-field-plain-wrapper", root.ClassList);
        Assert.Contains("dnet-form-field-appearance-fill", root.ClassList);
        Assert.Contains("custom-field", root.ClassList);
        Assert.Equal("--dnet-form-field-background: var(--dnet-sys-surface);", root.GetAttribute("style"));
        Assert.Equal("Project", cut.Find("label").TextContent);
        Assert.Single(cut.FindAll(".dnet-form-field-plain-control-container"));
    }

    [Theory]
    [InlineData(FormAppearance.Standard, "standard")]
    [InlineData(FormAppearance.Legacy, "legacy")]
    public async Task Switching_appearance_preserves_the_mounted_child_and_its_state(FormAppearance alternative, string cssName)
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<StatefulInput>(0);
            builder.CloseComponent();
        };
        var cut = Render<DnetFormField>(parameters => parameters.Add(component => component.ChildContent, content));
        var child = cut.FindComponent<StatefulInput>().Instance;
        await cut.Find("input").InputAsync("Keep this value");
        cut.Render(parameters => parameters.Add(component => component.Appearance, alternative));
        Assert.Contains($"dnet-form-field-appearance-{cssName}", cut.Find(".dnet-form-field-plain-wrapper").ClassList);
        Assert.Same(child, cut.FindComponent<StatefulInput>().Instance);
        Assert.Equal("Keep this value", cut.Find("input").GetAttribute("value"));
        cut.Render(parameters => parameters.Add(component => component.Appearance, FormAppearance.Fill));
        Assert.Same(child, cut.FindComponent<StatefulInput>().Instance);
        Assert.Equal("Keep this value", cut.Find("input").GetAttribute("value"));
        Assert.False(child.Disposed);
    }

    [Fact]
    public void Unknown_appearances_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Render<DnetFormField>(parameters => parameters
            .Add(component => component.Appearance, (FormAppearance)42)));
    }

    public sealed class StatefulInput : ComponentBase, IDisposable
    {
        private string _value = "Initial";
        public bool Disposed { get; private set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "value", _value);
            builder.AddAttribute(2, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this,
                args => _value = args.Value?.ToString() ?? ""));
            builder.CloseElement();
        }

        public void Dispose() => Disposed = true;
    }
}
