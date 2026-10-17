using System.Globalization;
using System.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dnet.Blazor.Localization;

/// <summary>Registration for shared Dnet control localization.</summary>
public static class DnetLocalization
{
    /// <summary>Registers scoped localization, optionally changing the initial culture.</summary>
    public static IServiceCollection AddDnetLocalization(this IServiceCollection services,
        Action<DnetLocalizationOptions>? configure = null)
    {
        services.AddLogging();
        services.AddLocalization();
        services.AddOptions<DnetLocalizationOptions>();
        if (configure is not null) services.Configure(configure);
        services.TryAddScoped<IDnetLocalizer, DnetLocalizer>();
        return services;
    }

    // Pure presentational components historically required no library DI setup.
    // Keep them usable with an immutable Spanish fallback in that situation.
    internal static IDnetLocalizer Get(IServiceProvider? services) =>
        services?.GetService<IDnetLocalizer>() ?? SpanishFallback.Instance;

    private sealed class SpanishFallback : IDnetLocalizer
    {
        internal static readonly SpanishFallback Instance = new();
        private static readonly ResourceManager Resources = new("Dnet.Blazor.Localization.DnetStrings", typeof(DnetStrings).Assembly);
        public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("es-ES");
        public string this[string key] => Resources.GetString(key, Culture) ?? key;
        public string Format(string key, params object?[] arguments) => string.Format(Culture, this[key], arguments);
        public event Action? Changed { add { } remove { } }
        public void SetCulture(string cultureName) => throw new InvalidOperationException(
            "Register AddDnetBlazor or AddDnetLocalization before changing the control language.");
    }
}
