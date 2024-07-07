using System.Collections.Immutable;

namespace SourceLocalizationGenerator.Core;

#nullable enable

/// <summary>
/// Denotes that the given attribute class is a localization string attribute.
/// </summary>
public interface ILocalizationStringAttribute
{
    /// <summary>
    /// The languages that the localization string attribute supports, as denoted in its
    /// constructor parameters.
    /// </summary>
    /// <remarks>
    /// Automatically implemented by the generator.
    /// </remarks>
    public ImmutableArray<string> SupportedLanguages { get; }

    /// <summary>
    /// Determines whether the given language is supported by this localization string attribute,
    /// as denoted in its constructor parameters.
    /// </summary>
    /// <remarks>
    /// Automatically implemented by the generator.
    /// </remarks>
    public bool SupportsLanguage(string language);
    /// <summary>
    /// Gets the localization resource value for the given language, if supported by the attribute.
    /// </summary>
    /// <remarks>
    /// Automatically implemented by the generator.
    /// </remarks>
    public string? GetLocalization(string language);
}
