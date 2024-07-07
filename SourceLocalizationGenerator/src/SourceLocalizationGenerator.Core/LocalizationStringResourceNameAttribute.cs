using System;

namespace SourceLocalizationGenerator.Core;

/// <summary>
/// Denotes that the annotated parameter represents a localization string resource name, i.e.
/// the name of the property that contains the localization resource values.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class LocalizationStringResourceNameAttribute : Attribute;
