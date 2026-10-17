using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Dnet.Blazor.Localization;

internal sealed class DnetLocalizer : IDnetLocalizer
{
    private readonly IStringLocalizer<DnetStrings> _resources;
    private readonly IStringLocalizer<DnetEnglishStrings> _englishResources;
    public CultureInfo Culture { get; private set; }
    public event Action? Changed;

    public DnetLocalizer(IStringLocalizer<DnetStrings> resources,
        IStringLocalizer<DnetEnglishStrings> englishResources, IOptions<DnetLocalizationOptions> options)
    {
        _resources = resources;
        _englishResources = englishResources;
        Culture = ResolveCulture(options.Value.DefaultCulture);
    }

    public string this[string key]
    {
        get
        {
            // IStringLocalizer uses the ambient UI culture. Scope the lookup to
            // this user without changing process-wide or circuit-wide culture.
            var original = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = Culture;
                if (Culture.TwoLetterISOLanguageName == "en")
                {
                    var translated = _englishResources[key];
                    if (!translated.ResourceNotFound) return translated.Value;
                }
                return _resources[key].Value;
            }
            finally
            {
                CultureInfo.CurrentUICulture = original;
            }
        }
    }

    public string Format(string key, params object?[] arguments) =>
        string.Format(Culture, this[key], arguments);

    public void SetCulture(string cultureName)
    {
        var culture = ResolveCulture(cultureName);
        if (Culture.Name == culture.Name) return;
        Culture = culture;
        Changed?.Invoke();
    }

    internal static CultureInfo ResolveCulture(string cultureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);
        var culture = CultureInfo.GetCultureInfo(cultureName);
        if (culture.TwoLetterISOLanguageName is not ("es" or "en"))
            throw new ArgumentException("Dnet supports Spanish (es) and English (en).", nameof(cultureName));
        return culture;
    }
}
