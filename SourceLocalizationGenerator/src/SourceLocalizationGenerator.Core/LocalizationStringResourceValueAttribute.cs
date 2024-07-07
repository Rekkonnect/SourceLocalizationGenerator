using System;

namespace SourceLocalizationGenerator.Core;

/// <summary>
/// Denotes that the annotated attribute constructor parameter reflects a localization string
/// resource value for a specific language, optionally marking it as the default language to use
/// when another language does not provide a localization resource string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class LocalizationStringResourceValueAttribute(string language) : Attribute
{
    /// <summary>
    /// Determines whether the language denotes the default language for the localization resource.
    /// </summary>
    public bool Default { get; init; }

    /// <summary>
    /// Denotes the language for this localization resource.
    /// </summary>
    public string Language { get; } = language;
}
