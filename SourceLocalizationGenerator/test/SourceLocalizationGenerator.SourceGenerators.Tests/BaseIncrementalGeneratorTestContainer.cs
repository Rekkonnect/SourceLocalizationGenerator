using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using RoseLynn.Testing;
using SourceLocalizationGenerator.SourceGenerators.Tests.Verifiers;
using System.Collections.Generic;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public abstract class BaseIncrementalGeneratorTestContainer<TSourceGenerator>
    : BaseIncrementalGeneratorTestContainer<TSourceGenerator, NUnitVerifier, CSharpIncrementalGeneratorVerifier<TSourceGenerator>.Test>

    where TSourceGenerator : IIncrementalGenerator, new()
{
    protected override IEnumerable<MetadataReference> DefaultMetadataReferences
        => LocalizationCodeGeneratorMetadataReferences.BaseReferences;

    protected override LanguageVersion LanguageVersion => LanguageVersion.CSharp12;
}
