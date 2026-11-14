using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Dnet.Blazor.Infrastructure.Forms
{
    /// <summary>
    /// Reads the name a model gives a member with <see cref="DisplayAttribute"/> or
    /// <see cref="DisplayNameAttribute"/>, as the framework's inputs do since .NET 11.
    /// </summary>
    internal static class DnetDisplayNames
    {
        // Keyed by member and culture, never by expression: expressions are rebuilt on every render.
        private static readonly ConcurrentDictionary<(MemberInfo Member, string Culture), string?> Cache = new();

        /// <summary>
        /// Returns the display name of the member the <paramref name="accessor"/> reads, or
        /// <see langword="null"/> when it has none. A name taken from resources is read in
        /// <paramref name="culture"/>, the culture the library shows its text in.
        /// </summary>
        public static string? For(LambdaExpression? accessor, CultureInfo culture)
        {
            var member = MemberOf(accessor?.Body);
            return member is null ? null : For(member, culture);
        }

        public static string? For(MemberInfo member, CultureInfo culture) =>
            Cache.GetOrAdd((member, culture.Name), static (key, culture) => Read(key.Member, culture), culture);

        private static string? Read(MemberInfo member, CultureInfo culture)
        {
            var display = member.GetCustomAttribute<DisplayAttribute>(inherit: true);
            if (display is not null)
            {
                // DisplayAttribute resolves resource names with the ambient UI culture. Scope it to
                // the library's culture for the lookup, as DnetLocalizer does for its own text.
                var original = CultureInfo.CurrentUICulture;
                try
                {
                    CultureInfo.CurrentUICulture = culture;
                    var name = display.GetName();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
                catch (InvalidOperationException)
                {
                    // A resource that cannot be found leaves the member without a display name.
                }
                finally
                {
                    CultureInfo.CurrentUICulture = original;
                }
            }

            var displayName = member.GetCustomAttribute<DisplayNameAttribute>(inherit: true)?.DisplayName;
            return string.IsNullOrEmpty(displayName) ? null : displayName;
        }

        private static MemberInfo? MemberOf(Expression? body) => body switch
        {
            MemberExpression member => member.Member,
            UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary => MemberOf(unary.Operand),
            _ => null,
        };
    }
}
