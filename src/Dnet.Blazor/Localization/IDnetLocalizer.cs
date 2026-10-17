using System.Globalization;

namespace Dnet.Blazor.Localization;

/// <summary>Localizes control-owned text and formats within the current user scope.</summary>
public interface IDnetLocalizer
{
    /// <summary>The selected formatting and UI culture. Spanish is the default.</summary>
    CultureInfo Culture { get; }

    /// <summary>Gets a translated resource, falling back to the Spanish resource.</summary>
    string this[string key] { get; }

    /// <summary>Formats a translated message using the selected culture.</summary>
    string Format(string key, params object?[] arguments);

    /// <summary>Raised when the selected culture changes.</summary>
    event Action? Changed;

    /// <summary>Selects Spanish or English, optionally with a regional variant.</summary>
    void SetCulture(string cultureName);
}
