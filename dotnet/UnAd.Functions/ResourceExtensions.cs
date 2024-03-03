using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UnAd.Functions;
internal static partial class ResourceExtensions {
    private static readonly Regex ReplaceByNameRegex = ReplaceByName();
    public static string GetStringWithReplacements<T>(this IStringLocalizer<T> localizer, string name, object replacements) {
        var resource = localizer.GetString(name) ?? throw new InvalidOperationException($"Resource not found: {name}");
        var matches = ReplaceByNameRegex.Matches(resource);
        return matches.Aggregate(resource.ToString().Replace("\\n", "\n"),
            (current, match) => current.Replace(match.Value,
                replacements.GetType().GetProperty(match.Groups[1].Value)?.GetValue(replacements)?.ToString() ?? match.Value));
    }

    public static string GetStringWithReplacements<T>(this IStringLocalizer<T> localizer, string name, params object[] replacements) =>
        string.Format(CultureInfo.CurrentCulture, localizer.GetString(name), replacements);

    [GeneratedRegex(@"\{(\w+)\}", RegexOptions.Compiled)]
    private static partial Regex ReplaceByName();
}
