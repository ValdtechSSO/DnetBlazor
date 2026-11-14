namespace Dnet.Blazor.Components.Form;

public interface IFormEventService
{
    public event Action<bool> OnError;

    public event Action<bool> OnFocus;

    public event Action<string?> OnCurrentValue;

    public event Action OnClearContent;

    public event Action<FormEventData> OnFormEventRaised;

    public event Action OnSufixContentClicked;

    public event Action<bool>? OnDisabled;

    public event Action<Tuple<int, int>>? OnCharCount;

    /// <summary>Raised when an asynchronous validator starts or stops checking the field, or could not check it.</summary>
    public event Action<FieldValidationState>? OnValidationState;

    void RaiseError(bool hasError);

    void RaiseFocus(bool hasFocus);

    void RaiseCurrentValue(string? currentValue);

    void RaiseClearContent();

    void FormRaiseEvent(string error, bool hasFocus, object currentValue);

    void RaiseSufixContentClicked();

    void RaiseDisabledEvent(bool isDisabled);

    void RaiseCharCountEvent(Tuple<int, int> countData);

    /// <summary>Reports the state of the field's asynchronous validation to the form field.</summary>
    void RaiseValidationState(FieldValidationState state);
}
