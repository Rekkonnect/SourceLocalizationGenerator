using System;

namespace SourceLocalizationGenerator.Core;

/// <summary>
/// Denotes that the annotated class is a localization string resource container, which can be
/// further annotated with other attributes that implement <see cref="ILocalizationStringAttribute"/>
/// to trigger the generation of localization string resources.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LocalizationStringResourceContainerAttribute : Attribute;
