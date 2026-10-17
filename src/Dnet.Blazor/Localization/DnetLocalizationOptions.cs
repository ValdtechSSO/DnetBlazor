namespace Dnet.Blazor.Localization;

/// <summary>Application-wide defaults for Dnet control localization.</summary>
public sealed class DnetLocalizationOptions
{
    /// <summary>The initial culture for each user scope. Defaults to Spanish (Spain).</summary>
    public string DefaultCulture { get; set; } = "es-ES";
}
