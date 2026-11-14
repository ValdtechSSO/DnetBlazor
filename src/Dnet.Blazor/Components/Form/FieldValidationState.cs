namespace Dnet.Blazor.Components.Form;

/// <summary>
/// The state of the asynchronous validation of a field, which a form field shows under it.
/// </summary>
public enum FieldValidationState
{
    /// <summary>No asynchronous validator is checking the field, and the last one finished.</summary>
    None,

    /// <summary>An asynchronous validator is checking the field.</summary>
    Pending,

    /// <summary>The last asynchronous validation of the field could not finish: it threw.</summary>
    Faulted,
}
