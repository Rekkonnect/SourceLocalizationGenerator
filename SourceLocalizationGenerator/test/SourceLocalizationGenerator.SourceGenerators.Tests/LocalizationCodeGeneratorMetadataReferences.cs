using Microsoft.CodeAnalysis;
using RoseLynn;
using SourceLocalizationGenerator.Core;
using System.Collections.Immutable;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public static class LocalizationCodeGeneratorMetadataReferences
{
    public static readonly ImmutableArray<MetadataReference> BaseReferences;

    static LocalizationCodeGeneratorMetadataReferences()
    {
        BaseReferences = [
            MetadataReferenceFactory.CreateFromType<GeneratedLocalizationStringAttribute>(),
        ];
    }
}
