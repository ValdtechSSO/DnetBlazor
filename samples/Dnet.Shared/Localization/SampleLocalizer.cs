using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Dnet.Blazor.Localization;

namespace Dnet.App.Shared.Localization;

/// <summary>Application-owned copy, following the library's scoped language.</summary>
public sealed class SampleLocalizer
{
    private static readonly ResourceManager Spanish = new("Dnet.App.Shared.Localization.SampleStrings", typeof(SampleLocalizer).Assembly);
    private static readonly ResourceManager English = new("Dnet.App.Shared.Localization.SampleEnglishStrings", typeof(SampleLocalizer).Assembly);
    private static readonly Lazy<IReadOnlyDictionary<string, string>> SourceKeys = new(BuildSourceKeys);
    private static readonly Lazy<IReadOnlyList<MessagePattern>> MessagePatterns = new(BuildMessagePatterns);
    private readonly IDnetLocalizer _language;

    public SampleLocalizer(IDnetLocalizer language) => _language = language;
    public CultureInfo Culture => _language.Culture;
    public string this[string key] =>
        (Culture.TwoLetterISOLanguageName == "en" ? English.GetString(key, CultureInfo.InvariantCulture) : null)
        ?? Spanish.GetString(key, CultureInfo.InvariantCulture) ?? key;

    public string Format(string key, params object[] arguments) => string.Format(Culture, this[key], arguments);
    public string Display(object value) => Message(Convert.ToString(value, Culture) ?? string.Empty);

    /// <summary>Translates display labels from sample data, keeping stored values and user input stable.</summary>
    public string Text(string value) => value is not null && SourceKeys.Value.TryGetValue(value, out var key) ? this[key] : value ?? string.Empty;

    /// <summary>Resolves an existing result without replacing its underlying selection or payload.</summary>
    public string Message(string value)
    {
        var translated = Text(value);
        if (translated != value || string.IsNullOrEmpty(value) || value.Length > 4096) return translated;
        foreach (var template in MessagePatterns.Value)
        {
            var match = template.Pattern.Match(value);
            if (!match.Success) continue;
            var arguments = new object[template.Positions.Max() + 1];
            for (var i = 0; i < template.Positions.Length; i++)
                arguments[template.Positions[i]] = template.LocalizeArguments ? Text(match.Groups[i + 1].Value) : match.Groups[i + 1].Value;
            return Format(template.Key, arguments);
        }
        return value;
    }

    private static IReadOnlyList<MessagePattern> BuildMessagePatterns()
    {
        var patterns = new List<MessagePattern>();
        foreach (var manager in new[] { English, Spanish })
        {
            var resources = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            if (resources is null) continue;
            foreach (System.Collections.DictionaryEntry entry in resources)
            {
                if (entry.Key is not string key || entry.Value is not string value) continue;
                var placeholders = Regex.Matches(value, @"\{(\d+)\}");
                if (placeholders.Count == 0) continue;
                var expression = new StringBuilder("^");
                var positions = new List<int>();
                var offset = 0;
                foreach (Match placeholder in placeholders)
                {
                    expression.Append(Regex.Escape(value.Substring(offset, placeholder.Index - offset)));
                    expression.Append("(.*?)");
                    positions.Add(int.Parse(placeholder.Groups[1].Value, CultureInfo.InvariantCulture));
                    offset = placeholder.Index + placeholder.Length;
                }
                expression.Append(Regex.Escape(value.Substring(offset))).Append('$');
                var original = English.GetString(key, CultureInfo.InvariantCulture) ?? value;
                var localizeArguments = !original.StartsWith("Saved:", StringComparison.Ordinal)
                    && !original.EndsWith("removed from the project.", StringComparison.Ordinal);
                var specificity = value.Length - placeholders.Sum(match => match.Length);
                patterns.Add(new(key, new Regex(expression.ToString(), RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50)), positions.ToArray(), localizeArguments, specificity));
            }
        }
        // Prefer a complete phrase over a broader template such as "{0} {1} selected".
        return patterns.OrderByDescending(pattern => pattern.Specificity).ToArray();
    }

    private sealed record MessagePattern(string Key, Regex Pattern, int[] Positions, bool LocalizeArguments, int Specificity);

    private static IReadOnlyDictionary<string, string> BuildSourceKeys()
    {
        var keys = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var manager in new[] { English, Spanish })
        {
            var resources = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            if (resources is null) continue;
            foreach (System.Collections.DictionaryEntry entry in resources)
                if (entry.Key is string key && entry.Value is string value) keys.TryAdd(value, key);
        }
        return keys;
    }
}
