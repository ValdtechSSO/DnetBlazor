using System.Globalization;
using System.Linq.Expressions;
using Dnet.Blazor.Infrastructure.Services.CssBuilder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Dnet.Blazor.Components.Slider;

/// <summary>
/// An accessible single-value or range slider built on native range inputs.
/// </summary>
public partial class DnetSlider : IDisposable
{
    private const int MaximumRenderedTickMarks = 201;
    private static long _nextId;
    private static readonly IReadOnlyDictionary<string, object> EmptyAttributes =
        new Dictionary<string, object>();
    private static readonly HashSet<string> ProtectedInputAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "class", "type", "min", "max", "step", "value", "name", "disabled", "required",
        "aria-label", "aria-labelledby", "aria-valuetext", "aria-invalid"
    };

    private readonly string _labelId = $"dnet-slider-label-{Interlocked.Increment(ref _nextId)}";
    private ElementReference _singleElement;
    private ElementReference _startElement;
    private ElementReference _endElement;
    private EditContext? _subscribedEditContext;
    private FieldIdentifier _valueField;
    private FieldIdentifier _startField;
    private FieldIdentifier _endField;
    private bool _hasValueField;
    private bool _hasStartField;
    private bool _hasEndField;

    /// <summary>Gets or sets the visible label rendered above the slider.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets the value of a single-value slider.</summary>
    [Parameter]
    public double Value { get; set; }

    /// <summary>Raised continuously when the single slider value changes.</summary>
    [Parameter]
    public EventCallback<double> ValueChanged { get; set; }

    /// <summary>Gets or sets the expression used for EditForm integration.</summary>
    [Parameter]
    public Expression<Func<double>>? ValueExpression { get; set; }

    /// <summary>Gets or sets whether the component renders two range thumbs.</summary>
    [Parameter]
    public bool Range { get; set; }

    /// <summary>Gets or sets the lower value of a range slider.</summary>
    [Parameter]
    public double StartValue { get; set; }

    /// <summary>Raised continuously when the range start value changes.</summary>
    [Parameter]
    public EventCallback<double> StartValueChanged { get; set; }

    /// <summary>Gets or sets the expression used to validate the range start value.</summary>
    [Parameter]
    public Expression<Func<double>>? StartValueExpression { get; set; }

    /// <summary>Gets or sets the upper value of a range slider.</summary>
    [Parameter]
    public double EndValue { get; set; } = 100;

    /// <summary>Raised continuously when the range end value changes.</summary>
    [Parameter]
    public EventCallback<double> EndValueChanged { get; set; }

    /// <summary>Gets or sets the expression used to validate the range end value.</summary>
    [Parameter]
    public Expression<Func<double>>? EndValueExpression { get; set; }

    /// <summary>Gets or sets the minimum permitted value.</summary>
    [Parameter]
    public double Min { get; set; }

    /// <summary>Gets or sets the maximum permitted value.</summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>Gets or sets the interval between permitted values.</summary>
    [Parameter]
    public double Step { get; set; } = 1;

    /// <summary>Gets or sets whether value labels appear while a thumb is active or focused.</summary>
    [Parameter]
    public bool Discrete { get; set; }

    /// <summary>Gets or sets whether tick marks are shown along the track.</summary>
    [Parameter]
    public bool ShowTickMarks { get; set; }

    /// <summary>Gets or sets whether user interaction is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets whether the native range inputs are required.</summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>Gets or sets the slider's semantic color.</summary>
    [Parameter]
    public DnetSliderColor Color { get; set; } = DnetSliderColor.Primary;

    /// <summary>Gets or sets a function that formats value labels and accessible value text.</summary>
    [Parameter]
    public Func<double, string>? DisplayWith { get; set; }

    /// <summary>Gets or sets the accessible label for a single slider.</summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>Gets or sets the accessible label for the start thumb.</summary>
    [Parameter]
    public string? StartAriaLabel { get; set; }

    /// <summary>Gets or sets the accessible label for the end thumb.</summary>
    [Parameter]
    public string? EndAriaLabel { get; set; }

    /// <summary>Gets or sets the form name for a single slider.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>Gets or sets the form name for the range start input.</summary>
    [Parameter]
    public string? StartName { get; set; }

    /// <summary>Gets or sets the form name for the range end input.</summary>
    [Parameter]
    public string? EndName { get; set; }

    /// <summary>Raised continuously while either thumb changes.</summary>
    [Parameter]
    public EventCallback<DnetSliderEventArgs> OnInput { get; set; }

    /// <summary>Raised when the user commits a value change.</summary>
    [Parameter]
    public EventCallback<DnetSliderEventArgs> OnChange { get; set; }

    /// <summary>Raised when pointer dragging starts.</summary>
    [Parameter]
    public EventCallback<DnetSliderEventArgs> OnDragStart { get; set; }

    /// <summary>Raised when pointer dragging ends or is cancelled.</summary>
    [Parameter]
    public EventCallback<DnetSliderEventArgs> OnDragEnd { get; set; }

    /// <summary>Gets or sets extra attributes for the single native input.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    /// <summary>Gets or sets extra attributes for the range start native input.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, object>? StartInputAttributes { get; set; }

    /// <summary>Gets or sets extra attributes for the range end native input.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, object>? EndInputAttributes { get; set; }

    /// <summary>Gets or sets unmatched attributes applied to the component host.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    private double SafeValue => ClampAndSnap(Value);
    private double RawStartValue => ClampAndSnap(StartValue);
    private double RawEndValue => ClampAndSnap(EndValue);
    private double SafeStartValue => Math.Min(RawStartValue, RawEndValue);
    private double SafeEndValue => Math.Max(RawStartValue, RawEndValue);
    private double StartPercent => ToPercent(Range ? SafeStartValue : Min);
    private double EndPercent => ToPercent(Range ? SafeEndValue : SafeValue);

    private IEnumerable<TickMark> TickMarks => BuildTickMarks();

    private IEnumerable<KeyValuePair<string, object>> RootAttributes
        => (AdditionalAttributes ?? EmptyAttributes).Where(attribute =>
            !attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase) &&
            !attribute.Key.Equals("style", StringComparison.OrdinalIgnoreCase));

    private IEnumerable<KeyValuePair<string, object>> FilteredInputAttributes
        => FilterInputAttributes(InputAttributes);

    private IEnumerable<KeyValuePair<string, object>> FilteredStartInputAttributes
        => FilterInputAttributes(StartInputAttributes);

    private IEnumerable<KeyValuePair<string, object>> FilteredEndInputAttributes
        => FilterInputAttributes(EndInputAttributes);

    private string RootClass => new CssBuilder("dnet-slider")
        .AddClass("dnet-slider-range", Range)
        .AddClass("dnet-slider-discrete", Discrete)
        .AddClass("dnet-slider-ticks", ShowTickMarks)
        .AddClass("dnet-slider-disabled", Disabled)
        .AddClass($"dnet-slider-color-{Color.ToString().ToLowerInvariant()}")
        .AddClass(ValidationClass, !string.IsNullOrWhiteSpace(ValidationClass))
        .AddClassFromAttributes(AdditionalAttributes!)
        .Build();

    private string RootStyle
    {
        get
        {
            var style = AdditionalAttributes?.TryGetValue("style", out var customStyle) == true
                ? $"{customStyle?.ToString()?.Trim().TrimEnd(';')}; "
                : string.Empty;

            return $"{style}--_start-percent: {FormatPercent(StartPercent)}%; --_end-percent: {FormatPercent(EndPercent)}%;";
        }
    }

    private string ValidationClass
    {
        get
        {
            if (CascadedEditContext is null)
            {
                return string.Empty;
            }

            var classes = new List<string>();
            if (Range)
            {
                if (_hasStartField)
                {
                    classes.Add(CascadedEditContext.FieldCssClass(_startField));
                }
                if (_hasEndField)
                {
                    classes.Add(CascadedEditContext.FieldCssClass(_endField));
                }
            }
            else if (_hasValueField)
            {
                classes.Add(CascadedEditContext.FieldCssClass(_valueField));
            }

            return string.Join(" ", classes.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct());
        }
    }

    /// <summary>Moves keyboard focus to the requested slider thumb.</summary>
    public ValueTask FocusAsync(DnetSliderThumb thumb = DnetSliderThumb.Single)
        => thumb switch
        {
            DnetSliderThumb.Start when Range => _startElement.FocusAsync(),
            DnetSliderThumb.End when Range => _endElement.FocusAsync(),
            _ => _singleElement.FocusAsync()
        };

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (!double.IsFinite(Min) || !double.IsFinite(Max) || Min >= Max)
        {
            throw new InvalidOperationException($"{nameof(Min)} must be finite and lower than {nameof(Max)}.");
        }

        if (!double.IsFinite(Step) || Step <= 0)
        {
            throw new InvalidOperationException($"{nameof(Step)} must be a finite number greater than zero.");
        }

        ConfigureEditContext();
    }

    private void ConfigureEditContext()
    {
        if (!ReferenceEquals(_subscribedEditContext, CascadedEditContext))
        {
            if (_subscribedEditContext is not null)
            {
                _subscribedEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }

            _subscribedEditContext = CascadedEditContext;
            if (_subscribedEditContext is not null)
            {
                _subscribedEditContext.OnValidationStateChanged += HandleValidationStateChanged;
            }
        }

        _hasValueField = ValueExpression is not null;
        _hasStartField = StartValueExpression is not null;
        _hasEndField = EndValueExpression is not null;
        if (_hasValueField)
        {
            _valueField = FieldIdentifier.Create(ValueExpression!);
        }
        if (_hasStartField)
        {
            _startField = FieldIdentifier.Create(StartValueExpression!);
        }
        if (_hasEndField)
        {
            _endField = FieldIdentifier.Create(EndValueExpression!);
        }
    }

    private async Task HandleInputAsync(ChangeEventArgs eventArgs, DnetSliderThumb thumb)
    {
        if (Disabled || !TryParse(eventArgs.Value, out var parsed))
        {
            return;
        }

        var value = NormalizeForThumb(parsed, thumb);
        await SetValueAsync(thumb, value);
        await OnInput.InvokeAsync(new DnetSliderEventArgs(thumb, value));
    }

    private async Task HandleChangeAsync(ChangeEventArgs eventArgs, DnetSliderThumb thumb)
    {
        if (Disabled || !TryParse(eventArgs.Value, out var parsed))
        {
            return;
        }

        var value = NormalizeForThumb(parsed, thumb);
        await SetValueAsync(thumb, value);
        await OnChange.InvokeAsync(new DnetSliderEventArgs(thumb, value));
    }

    private Task HandleDragStartAsync(DnetSliderThumb thumb)
        => Disabled
            ? Task.CompletedTask
            : OnDragStart.InvokeAsync(new DnetSliderEventArgs(thumb, GetValue(thumb)));

    private Task HandleDragEndAsync(DnetSliderThumb thumb)
        => Disabled
            ? Task.CompletedTask
            : OnDragEnd.InvokeAsync(new DnetSliderEventArgs(thumb, GetValue(thumb)));

    private async Task SetValueAsync(DnetSliderThumb thumb, double value)
    {
        switch (thumb)
        {
            case DnetSliderThumb.Start:
                StartValue = value;
                await StartValueChanged.InvokeAsync(value);
                if (_hasStartField)
                {
                    CascadedEditContext?.NotifyFieldChanged(_startField);
                }
                break;
            case DnetSliderThumb.End:
                EndValue = value;
                await EndValueChanged.InvokeAsync(value);
                if (_hasEndField)
                {
                    CascadedEditContext?.NotifyFieldChanged(_endField);
                }
                break;
            default:
                Value = value;
                await ValueChanged.InvokeAsync(value);
                if (_hasValueField)
                {
                    CascadedEditContext?.NotifyFieldChanged(_valueField);
                }
                break;
        }
    }

    private double GetValue(DnetSliderThumb thumb)
        => thumb switch
        {
            DnetSliderThumb.Start => SafeStartValue,
            DnetSliderThumb.End => SafeEndValue,
            _ => SafeValue
        };

    private double NormalizeForThumb(double value, DnetSliderThumb thumb)
        => thumb switch
        {
            DnetSliderThumb.Start => Math.Min(ClampAndSnap(value), SafeEndValue),
            DnetSliderThumb.End => Math.Max(ClampAndSnap(value), SafeStartValue),
            _ => ClampAndSnap(value)
        };

    private double ClampAndSnap(double value)
    {
        var finiteValue = double.IsFinite(value) ? value : Min;
        var clamped = Math.Clamp(finiteValue, Min, Max);
        var steps = Math.Round((clamped - Min) / Step, MidpointRounding.AwayFromZero);
        return Math.Clamp(Min + (steps * Step), Min, Max);
    }

    private double ToPercent(double value) => ((value - Min) / (Max - Min)) * 100;

    private IEnumerable<TickMark> BuildTickMarks()
    {
        var intervalCount = Math.Max(1L, (long)Math.Floor((Max - Min) / Step));
        var stride = Math.Max(1L, (long)Math.Ceiling((intervalCount + 1d) / MaximumRenderedTickMarks));

        for (long index = 0; index <= intervalCount; index += stride)
        {
            var value = Math.Min(Max, Min + (index * Step));
            yield return new TickMark(ToPercent(value), IsTickActive(value));
        }

        if (intervalCount % stride != 0)
        {
            yield return new TickMark(100, IsTickActive(Max));
        }
    }

    private bool IsTickActive(double value)
        => Range
            ? value >= SafeStartValue && value <= SafeEndValue
            : value <= SafeValue;

    private string TickClass(TickMark tick)
        => tick.Active ? "dnet-slider-tick dnet-slider-tick-active" : "dnet-slider-tick";

    private string InputClass(DnetSliderThumb thumb, IReadOnlyDictionary<string, object>? attributes)
        => new CssBuilder("dnet-slider-input")
            .AddClass(thumb switch
            {
                DnetSliderThumb.Start => "dnet-slider-input-start",
                DnetSliderThumb.End => "dnet-slider-input-end",
                _ => "dnet-slider-input-single"
            })
            .AddClassFromAttributes(attributes!)
            .Build();

    private string? ResolveLabelledBy(
        string? explicitAriaLabel,
        IReadOnlyDictionary<string, object>? attributes)
    {
        if (!string.IsNullOrWhiteSpace(explicitAriaLabel) || attributes?.ContainsKey("aria-label") == true)
        {
            return null;
        }

        if (attributes?.TryGetValue("aria-labelledby", out var labelledBy) == true)
        {
            return labelledBy?.ToString();
        }

        return ChildContent is null ? null : _labelId;
    }

    private static string? ResolveAriaLabel(
        string? explicitAriaLabel,
        IReadOnlyDictionary<string, object>? attributes)
    {
        if (!string.IsNullOrWhiteSpace(explicitAriaLabel))
        {
            return explicitAriaLabel;
        }

        return attributes?.TryGetValue("aria-label", out var ariaLabel) == true
            ? ariaLabel?.ToString()
            : null;
    }

    private string? AriaInvalid(DnetSliderThumb thumb)
    {
        if (CascadedEditContext is null)
        {
            return null;
        }

        var hasErrors = thumb switch
        {
            DnetSliderThumb.Start when _hasStartField => CascadedEditContext.GetValidationMessages(_startField).Any(),
            DnetSliderThumb.End when _hasEndField => CascadedEditContext.GetValidationMessages(_endField).Any(),
            DnetSliderThumb.Single when _hasValueField => CascadedEditContext.GetValidationMessages(_valueField).Any(),
            _ => false
        };

        return hasErrors ? "true" : null;
    }

    private string FormatValue(double value)
        => DisplayWith?.Invoke(value) ?? value.ToString("0.########", CultureInfo.CurrentCulture);

    private static string FormatNumber(double value)
        => value.ToString("0.################", CultureInfo.InvariantCulture);

    private static string FormatPercent(double value)
        => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static bool TryParse(object? value, out double result)
        => double.TryParse(
            Convert.ToString(value, CultureInfo.InvariantCulture),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);

    private static IEnumerable<KeyValuePair<string, object>> FilterInputAttributes(
        IReadOnlyDictionary<string, object>? attributes)
        => (attributes ?? EmptyAttributes).Where(attribute => !ProtectedInputAttributes.Contains(attribute.Key));

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs eventArgs)
        => InvokeAsync(StateHasChanged);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_subscribedEditContext is not null)
        {
            _subscribedEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }

    private readonly record struct TickMark(double Percent, bool Active);
}
