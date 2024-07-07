using Garyon.Extensions;
using System.Linq;
using System.Text;

namespace SourceLocalizationGenerator.SourceGenerators;

/// <summary>
/// Provides extensions for handling strings representing names and identifiers.
/// </summary>
public static class NamingExtensions
{
    /// <summary>
    /// Converts the given name into camelCase. This assumes that the
    /// input is in PascalCase.
    /// </summary>
    public static string CamelCase(this string s)
    {
        var builder = new StringBuilder(s.Length);
        builder.Append(s[0].ToLower());
        builder.Append(s, 1);
        return builder.ToString();
    }

    /// <summary>
    /// Converts the given name into PascalCase. This assumes that the
    /// input is in camelCase.
    /// </summary>
    public static string PascalCase(this string s)
    {
        var builder = new StringBuilder(s.Length);
        builder.Append(s[0].ToUpper());
        builder.Append(s, 1);
        return builder.ToString();
    }
}
