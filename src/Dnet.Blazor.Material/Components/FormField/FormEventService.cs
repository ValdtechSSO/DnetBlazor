namespace Dnet.Blazor.Material.Components.FormField;

/// <summary>
/// Per-form-field event hub used by Material input components.
/// </summary>
public sealed class FormEventService : IFormEventService
{
    public event Action<bool>? OnError;
    public event Action<bool>? OnFocus;
    public event Action<string?>? OnCurrentValue;
    public event Action? OnClearContent;

    public void RaiseError(bool hasError) => OnError?.Invoke(hasError);

    public void RaiseFocus(bool hasFocus) => OnFocus?.Invoke(hasFocus);

    public void RaiseCurrentValue(string? currentValue) => OnCurrentValue?.Invoke(currentValue);

    public void RaiseClearContent() => OnClearContent?.Invoke();

    public void FormRaiseEvent(string error, bool hasFocus, object currentValue)
    {
        RaiseError(!string.IsNullOrWhiteSpace(error));
        RaiseFocus(hasFocus);
        RaiseCurrentValue(currentValue?.ToString());
    }
}
