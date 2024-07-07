using System;

namespace SourceLocalizationGenerator.Core;

/// <summary>
/// Denotes that the annotated attribute is a localization string resource template
/// attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GeneratedLocalizationStringAttribute : Attribute;
