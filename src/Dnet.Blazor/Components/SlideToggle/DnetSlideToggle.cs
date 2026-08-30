using System.Diagnostics.CodeAnalysis;
using Dnet.Blazor.Infrastructure.Forms;
using Dnet.Blazor.Infrastructure.Services.CssBuilder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Dnet.Blazor.Components.SlideToggle;

/// <summary>
/// An accessible on/off switch for editing <see cref="bool"/> values.
/// </summary>
public class DnetSlideToggle : DnetInputBase<bool>
{
    /// <summary>Gets the underlying checkbox element.</summary>
    [DisallowNull]
    public ElementReference? Element { get; protected set; }

    /// <summary>Gets or sets the label rendered beside the switch.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets whether user interaction with the switch is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets whether the label is rendered before the switch.</summary>
    [Parameter]
    public bool TextPlacedBefore { get; set; }

    /// <summary>Gets or sets whether the switch and label fill the available width.</summary>
    [Parameter]
    public bool FullWidth { get; set; }

    /// <summary>Gets or sets whether the underlying form control is required.</summary>
    [Parameter]
    public bool Required { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var sequence = 0;

        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", RootClass);
        if (AdditionalAttributes?.TryGetValue("style", out var style) == true)
        {
            // Mirror instance-level token overrides onto the visual host. The
            // same attribute is still forwarded to the native input below.
            builder.AddAttribute(sequence++, "style", style);
        }

        builder.OpenElement(sequence++, "label");
        builder.AddAttribute(sequence++, "class", LabelClass);

        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-control");

        builder.OpenElement(sequence++, "input");
        builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
        builder.AddAttribute(sequence++, "type", "checkbox");
        builder.AddAttribute(sequence++, "role", "switch");
        builder.AddAttribute(sequence++, "class", $"{CssClass} dnet-slide-toggle-input");
        builder.AddAttribute(sequence++, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddAttribute(sequence++, "aria-checked", CurrentValue == true ? "true" : "false");
        builder.AddAttribute(sequence++, "disabled", Disabled);
        builder.AddAttribute(sequence++, "required", Required);
        builder.AddAttribute(sequence++, "aria-required", Required ? "true" : null);
        builder.AddAttribute(
            sequence++,
            "onchange",
            EventCallback.Factory.CreateBinder<bool>(this, value => CurrentValue = value, CurrentValue));
        builder.AddElementReferenceCapture(sequence++, reference => Element = reference);
        builder.CloseElement();

        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-track");
        builder.AddAttribute(sequence++, "aria-hidden", "true");
        builder.CloseElement();

        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-handle");
        builder.AddAttribute(sequence++, "aria-hidden", "true");

        builder.OpenElement(sequence++, "svg");
        builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-icon dnet-slide-toggle-icon-on");
        builder.AddAttribute(sequence++, "viewBox", "0 0 24 24");
        builder.AddAttribute(sequence++, "focusable", "false");
        builder.OpenElement(sequence++, "path");
        builder.AddAttribute(sequence++, "d", "M19.69,5.23 8.96,15.96 4.73,11.73 2.96,13.5 8.96,19.5 21.46,7Z");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(sequence++, "svg");
        builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-icon dnet-slide-toggle-icon-off");
        builder.AddAttribute(sequence++, "viewBox", "0 0 24 24");
        builder.AddAttribute(sequence++, "focusable", "false");
        builder.OpenElement(sequence++, "path");
        builder.AddAttribute(sequence++, "d", "M20 13H4V11H20Z");
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement();
        builder.CloseElement();

        if (ChildContent is not null)
        {
            builder.OpenElement(sequence++, "span");
            builder.AddAttribute(sequence++, "class", "dnet-slide-toggle-label");
            builder.AddContent(sequence++, ChildContent);
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }

    private string RootClass => new CssBuilder("dnet-slide-toggle")
        .AddClass("dnet-slide-toggle-checked", CurrentValue == true)
        .AddClass("dnet-slide-toggle-disabled", Disabled)
        .AddClass("dnet-slide-toggle-full-width", FullWidth)
        .AddClassFromAttributes(AdditionalAttributes!)
        .Build();

    private string LabelClass => new CssBuilder("dnet-slide-toggle-layout")
        .AddClass("text-before", TextPlacedBefore)
        .Build();

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        out bool result,
        [NotNullWhen(false)] out string? validationErrorMessage)
        => throw new NotSupportedException(
            $"This component does not parse string inputs. Bind to the '{nameof(CurrentValue)}' property, not '{nameof(CurrentValueAsString)}'.");
}
