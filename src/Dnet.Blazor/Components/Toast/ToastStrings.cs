using Dnet.Blazor.Localization;

namespace Dnet.Blazor.Components.Toast;

/// <summary>
/// Text used by <see cref="DnetToast"/>. Register an instance in DI for an
/// application-wide default, or set it on <c>ToastConfig.Strings</c>.
/// </summary>
public sealed record ToastStrings
{
    /// <summary>Gets the default English strings.</summary>
    public static ToastStrings Default { get; } = new();

    /// <summary>Gets the accessible label for the dismiss action.</summary>
    public string CloseLabel { get => _closeLabel ?? "Close notification"; init => _closeLabel = value; }

    // Null backing fields distinguish an omitted label from an explicit legacy English override.
    private string? _closeLabel;

    internal static ToastStrings Resolve(ToastStrings? instance, ToastStrings? registered, IDnetLocalizer localizer) => new()
    {
        CloseLabel = instance?._closeLabel ?? registered?._closeLabel ?? localizer["Toast.CloseNotification"],
    };
}
