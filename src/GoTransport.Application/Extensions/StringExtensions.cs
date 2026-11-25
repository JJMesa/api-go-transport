using System.Text.RegularExpressions;

namespace GoTransport.Application.Extensions;

public static class StringExtensions
{
    public static string RemoveExtraBlank(this string Value) => Regex.Replace(Value, @"\s+", " ").Trim();

    /// <summary>
    /// A SQL LIKE pattern that finds <paramref name="term"/> anywhere in a value. The term's own LIKE
    /// wildcards (<c>%</c>, <c>_</c>, <c>[</c>) are escaped, so they match literally.
    /// </summary>
    public static string ToContainsPattern(this string term) =>
        $"%{term.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")}%";
}