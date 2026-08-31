namespace Dnet.Blazor.Components.Toast.Infrastructure.Models;

/// <summary>Defines an optional action displayed by a toast.</summary>
public sealed class ToastAction
{
    /// <summary>Gets or sets the action label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the callback invoked when the action is selected.</summary>
    public Func<Task>? OnClick { get; set; }

    /// <summary>Gets or sets whether the action uses the quiet visual treatment.</summary>
    public bool Quiet { get; set; }
}
