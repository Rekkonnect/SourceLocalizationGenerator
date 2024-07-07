using Microsoft.CodeAnalysis;

namespace SourceLocalizationGenerator.SourceGenerators;

public sealed record ParameterLocalizationInfo(
    IParameterSymbol Parameter,
    string AttachedPropertyName,
    string Language,
    bool Default
    );
