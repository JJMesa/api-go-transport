using System.Text.RegularExpressions;

namespace GoTransport.Application.Extensions;

public static class StringExtensions
{
    public static string RemoveExtraBlank(this string Value) => Regex.Replace(Value, @"\s+", " ").Trim();
}