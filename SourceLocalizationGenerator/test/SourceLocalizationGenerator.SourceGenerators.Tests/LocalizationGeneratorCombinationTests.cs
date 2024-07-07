using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;
using RoseLynn.Generators;
using RoseLynn.Testing;
using SourceLocalizationGenerator.SourceGenerators.Tests.Verifiers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public sealed partial class LocalizationGeneratorCombinationTests
    : BaseIncrementalGeneratorTestContainer<LocalizationStringResourceContainerGenerator>
{
    [Test]
    public async Task BasicExample()
    {
        const string @namespace = "SourceLocalizationGenerator.SourceGenerators.Tests.Input";
        const string shortAttributeName = "TestLocalization";
        const string attributeName = $"{shortAttributeName}Attribute";

        const string attributeSource = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable
            
            [GeneratedLocalizationStringAttribute]
            public partial class {{attributeName}}(
                [LocalizationStringResourceName]
                string resourceName,
                [LocalizationStringResourceValue("ell", Default = true)]
                string greek,
                [LocalizationStringResourceValue("eng")]
                string? english)
                : Attribute, ILocalizationStringAttribute
                ;
            
            """;

        const string consumptionSource = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable
            
            [LocalizationStringResourceContainer]
            [{{shortAttributeName}}(nameof(Current), "Ρεύμα", "Current")]
            [{{shortAttributeName}}(nameof(Amperes), "Amperes", null)]
            public sealed partial class AmpereLocalizationResourceContainer;
            
            """;

        const string expectedPartial = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed partial class {{attributeName}}
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

        const string expectedLocalizationContainerPartial = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;
            using System.ComponentModel;
            using SourceLocalizationGenerator.SourceGenerators.Tests.Input;

            namespace {{@namespace}};

            #nullable enable

            sealed partial class AmpereLocalizationResourceContainer
                : INotifyPropertyChanged
            {
                private static readonly PropertyChangedEventArgs _currentChanged = new(nameof(Current));

                private string _currentLanguage = "ell";

                public TestLocalizationResource Current { get; } = new("Ρεύμα", "Current");
                public TestLocalizationResource Amperes { get; } = new("Amperes", null);

                public event PropertyChangedEventHandler? PropertyChanged;

                public void SetLanguage(string language)
                {
                    if (_currentLanguage == language)
                        return;

                    _currentLanguage = language;

                    switch (language)
                    {
                        case "ell":
                            Current.Current = Current.Greek;
                            break;
                        case "eng":
                            Current.Current = Current.English!;
                            break;
                    }

                    NotifyPropertyChanged(_currentChanged);
                }

                private void NotifyPropertyChanged(PropertyChangedEventArgs eventArgs)
                {
                    PropertyChanged?.Invoke(this, eventArgs);
                }
            }
            
            """;

        var mappings = new MultiGeneratorSourceMappings
        {
            {
                typeof(LocalizationStringResourceContainerGenerator),
                new GeneratedSourceMappings()
                {
                    {
                        $"{@namespace}.AmpereLocalizationResourceContainer.LocalizationResourceContainer.Implementation.g.cs",
                        expectedLocalizationContainerPartial },
                }
            },
            {
                typeof(GeneratedLocalizationStringAttributeGenerator),
                new GeneratedSourceMappings()
                {
                    { $"{@namespace}.{attributeName}.LocalizationStringAttributePart.g.cs", expectedPartial },
                    { $"{@namespace}.{attributeName}.LocalizationStringResourceType.g.cs", expectedResourceType },
                }
            },
        };

        await VerifyWithDisabledMarkupAsync(
            [attributeSource, consumptionSource],
            mappings);
    }

    [Test]
    public async Task MultiLanguageExample()
    {
        const string @namespace = "SourceLocalizationGenerator.SourceGenerators.Tests.Input";
        const string shortAttributeName = "TestLocalization";
        const string attributeName = $"{shortAttributeName}Attribute";

        const string attributeSource = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable
            
            [GeneratedLocalizationStringAttribute]
            public partial class {{attributeName}}(
                [LocalizationStringResourceName]
                string resourceName,
                [LocalizationStringResourceValue("ell", Default = true)]
                string greek,
                [LocalizationStringResourceValue("eng")]
                string? english,
                [LocalizationStringResourceValue("fra")]
                string? french)
                : Attribute, ILocalizationStringAttribute
                ;
            
            """;

        const string consumptionSource = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable
            
            [LocalizationStringResourceContainer]
            [{{shortAttributeName}}(nameof(Current), "Ρεύμα", "Current", "Courant")]
            [{{shortAttributeName}}(nameof(Amperes), "Amperes", null, "Ampères")]
            public sealed partial class AmpereLocalizationResourceContainer;
            
            """;

        const string expectedPartial = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;

            namespace {{@namespace}};
            
            #nullable enable

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed partial class {{attributeName}}
                : Attribute, ILocalizationStringAttribute
            {
                public string ResourceName { get; } = resourceName;
                public string Greek { get; } = greek;
                public string? English { get; } = english;
                public string? French { get; } = french;

                private static readonly ImmutableArray<string> _supportedLanguages
                    = ImmutableArray.Create(["ell", "eng", "fra"]);
                public ImmutableArray<string> SupportedLanguages => _supportedLanguages;

                public bool SupportsLanguage(string language)
                {
                    return language switch
                    {
                        "ell" => true,
                        "eng" => true,
                        "fra" => true,
                        _ => false,
                    };
                }
                public string? GetLocalization(string language)
                {
                    return language switch
                    {
                        "ell" => Greek,
                        "eng" => English,
                        "fra" => French,
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
                string? English,
                string? French)
            {
                // Directly settable from outside the class for improved performance
                public string Current { get; internal set; } = Greek;

                public string? GetResource(string language)
                {
                    return language switch
                    {
                        "ell" => Greek,
                        "eng" => English,
                        "fra" => French,
                        _ => null,
                    };
                }
            }
            
            """;

        const string expectedLocalizationContainerPartial = $$"""
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;
            using System.ComponentModel;
            using SourceLocalizationGenerator.SourceGenerators.Tests.Input;

            namespace {{@namespace}};

            #nullable enable

            sealed partial class AmpereLocalizationResourceContainer
                : INotifyPropertyChanged
            {
                private static readonly PropertyChangedEventArgs _currentChanged = new(nameof(Current));
                private static readonly PropertyChangedEventArgs _amperesChanged = new(nameof(Amperes));

                private string _currentLanguage = "ell";

                public TestLocalizationResource Current { get; } = new("Ρεύμα", "Current", "Courant");
                public TestLocalizationResource Amperes { get; } = new("Amperes", null, "Ampères");

                public event PropertyChangedEventHandler? PropertyChanged;

                public void SetLanguage(string language)
                {
                    if (_currentLanguage == language)
                        return;

                    _currentLanguage = language;

                    switch (language)
                    {
                        case "ell":
                            Current.Current = Current.Greek;
                            Amperes.Current = Amperes.Greek;
                            break;
                        case "eng":
                            Current.Current = Current.English!;
                            Amperes.Current = Amperes.Greek;
                            break;
                        case "fra":
                            Current.Current = Current.French!;
                            Amperes.Current = Amperes.French!;
                            break;
                    }

                    NotifyPropertyChanged(_currentChanged);
                    NotifyPropertyChanged(_amperesChanged);
                }

                private void NotifyPropertyChanged(PropertyChangedEventArgs eventArgs)
                {
                    PropertyChanged?.Invoke(this, eventArgs);
                }
            }
            
            """;

        var mappings = new MultiGeneratorSourceMappings
        {
            {
                typeof(LocalizationStringResourceContainerGenerator),
                new GeneratedSourceMappings()
                {
                    {
                        $"{@namespace}.AmpereLocalizationResourceContainer.LocalizationResourceContainer.Implementation.g.cs",
                        expectedLocalizationContainerPartial },
                }
            },
            {
                typeof(GeneratedLocalizationStringAttributeGenerator),
                new GeneratedSourceMappings()
                {
                    { $"{@namespace}.{attributeName}.LocalizationStringAttributePart.g.cs", expectedPartial },
                    { $"{@namespace}.{attributeName}.LocalizationStringResourceType.g.cs", expectedResourceType },
                }
            },
        };

        await VerifyWithDisabledMarkupAsync(
            [attributeSource, consumptionSource],
            mappings);
    }
}

partial class LocalizationGeneratorCombinationTests
{
    private async Task VerifyWithDisabledMarkupAsync(
        IEnumerable<string> sources,
        MultiGeneratorSourceMappings mappings,
        CancellationToken cancellationToken = default)
    {
        var test = CreateTestWithDisabledMarkup();
        await VerifyAsync(sources, mappings, test, cancellationToken);
    }

    private static SourceGeneratorTest<NUnitVerifier>
        CreateTestWithDisabledMarkup()
    {
        return new Test()
        {
            MarkupOptions = MarkupOptions.None,
            TestState =
            {
                MarkupHandling = MarkupMode.None,
            }
        };
    }

    private async Task VerifyAsync(
        IEnumerable<string> sources,
        MultiGeneratorSourceMappings mappings,
        SourceGeneratorTest<NUnitVerifier> test,
        CancellationToken cancellationToken = default)
    {
        test.TestState.Sources.AddRange(sources);
        foreach (var mapping in mappings.Mappings)
        {
            var generatorType = mapping.Key;
            foreach (var sourceMapping in mapping.Value)
            {
                test.TestState.GeneratedSources.Add(
                    (generatorType, sourceMapping.Key, sourceMapping.Value));
            }
        }

        await test.RunAsync(cancellationToken);
    }

    public sealed class Test
        : CSharpIncrementalGeneratorVerifier<LocalizationStringResourceContainerGenerator>.Test
    {
        protected override IEnumerable<ISourceGenerator> GetSourceGenerators()
        {
            return
            [
                new LocalizationStringResourceContainerGenerator()
                    .AsSourceGenerator(),
                new GeneratedLocalizationStringAttributeGenerator()
                    .AsSourceGenerator(),
            ];
        }
    }
}
