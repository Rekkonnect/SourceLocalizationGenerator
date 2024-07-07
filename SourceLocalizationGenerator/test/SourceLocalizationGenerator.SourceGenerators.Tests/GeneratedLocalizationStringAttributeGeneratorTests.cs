using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using RoseLynn.Generators;
using SourceLocalizationGenerator.SourceGenerators.Tests.Verifiers;
using System.Threading;
using System.Threading.Tasks;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public sealed class GeneratedLocalizationStringAttributeGeneratorTests
    : BaseIncrementalGeneratorTestContainer<GeneratedLocalizationStringAttributeGenerator>
{
    [Test]
    public async Task BasicExample()
    {
        const string @namespace = "SourceLocalizationGenerator.SourceGenerators.Tests.Input";
        const string @class = "TestLocalizationAttribute";

        const string source = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable
            
            [GeneratedLocalizationStringAttribute]
            public partial class {{@class}}(
                [LocalizationStringResourceName]
                string resourceName,
                [LocalizationStringResourceValue("ell", Default = true)]
                string greek,
                [LocalizationStringResourceValue("eng")]
                string? english)
                ;
            
            """;

        const string expectedPartial = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed partial class {{@class}}
                : Attribute, ILocalizationStringAttribute
            {
                public string ResourceName { get; } = resourceName;
                public string Greek { get; } = greek;
                public string? English { get; } = english;

                private static readonly ImmutableArray<string> _supportedLanguages
                    = ImmutableArray.Create(["ell", "eng"]);
                public ImmutableArray<string> SupportedLanguages => _supportedLanguages;

                public bool SupportsLanguage(string language)
                {
                    return language switch
                    {
                        "ell" => true,
                        "eng" => true,
                        _ => false,
                    };
                }
                public string? GetLocalization(string language)
                {
                    return language switch
                    {
                        "ell" => Greek,
                        "eng" => English,
                        _ => null,
                    };
                }
            }
            
            """;

        const string expectedResourceType = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable

            public sealed record TestLocalizationResource(
                string Greek,
                string? English)
            {
                // Directly settable from outside the class for improved performance
                public string Current { get; internal set; } = Greek;

                public string? GetResource(string language)
                {
                    return language switch
                    {
                        "ell" => Greek,
                        "eng" => English,
                        _ => null,
                    };
                }
            }
            
            """;

        var mappings = new GeneratedSourceMappings
        {
            { $"{@namespace}.{@class}.LocalizationStringAttributePart.g.cs", expectedPartial },
            { $"{@namespace}.{@class}.LocalizationStringResourceType.g.cs", expectedResourceType },
        };

        await VerifyWithDisabledMarkupAsync(source, mappings);
    }

    private async Task VerifyWithDisabledMarkupAsync(
        string source,
        GeneratedSourceMappings mappings,
        CancellationToken cancellationToken = default)
    {
        var test = CreateTestWithDisabledMarkup();
        await VerifyAsync(source, mappings, test, cancellationToken);
    }

    private static CSharpIncrementalGeneratorVerifier<GeneratedLocalizationStringAttributeGenerator>.Test
        CreateTestWithDisabledMarkup()
    {
        return new()
        {
            MarkupOptions = MarkupOptions.None,
            TestState =
            {
                MarkupHandling = MarkupMode.None,
            }
        };
    }
}
