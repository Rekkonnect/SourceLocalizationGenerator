using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading;

namespace SourceLocalizationGenerator.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed partial class GeneratedLocalizationStringAttributeGenerator : IIncrementalGenerator
{
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributePipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            WellKnownNames.Metadata.GeneratedLocalizationStringAttribute,
            FilterSyntaxNode,
            RetrieveModelFromContext);

        context.RegisterSourceOutput(attributePipeline, GenerateFromModel);
    }

    private static bool FilterSyntaxNode(SyntaxNode node, CancellationToken token)
    {
        return true;
    }

    private static void GenerateFromModel(
        SourceProductionContext context, LocalizationAttributeGenerationModel? model)
    {
        if (model is null)
            return;

        GenerateLocalizationAttributePart(context, model);
        GenerateLocalizationResourceType(context, model);
    }

    private static void GenerateLocalizationAttributePart(
        SourceProductionContext context, LocalizationAttributeGenerationModel model)
    {
        var @class = model.ClassType.Name;
        var @namespace = model.ClassType.ContainingNamespace.ToDisplayString();
        var fullDisplayName = model.ClassType.ToDisplayString();

        var args = new AttributeGenerationArgs(model, @class);
        var supportsLanguageArms = new LocalizationAttributeCodeBuilder(args)
            .GenerateSupportsLanguageArms();
        var localizationArms = new LocalizationAttributeCodeBuilder(args)
            .GenerateLocalizationArms();
        var localizationProperties = new LocalizationAttributeCodeBuilder(args)
            .GenerateLocalizationProperties();
        var supportedLanguageList = new LocalizationAttributeCodeBuilder(args)
            .GenerateSupportedLanguageList();

        const string usings = """
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;
            
            """;

        string namespaceCode = $"""

            namespace {@namespace};

            """;

        string code = $$"""
            
            #nullable enable

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public sealed partial class {{@class}}
                : Attribute, ILocalizationStringAttribute
            {
                public string {{model.ResourceNameParameter.AttachedPropertyName}} { get; } = {{model.ResourceNameParameter.Parameter.Name}};
            {{localizationProperties}}

                private static readonly ImmutableArray<string> _supportedLanguages
                    = ImmutableArray.Create([{{supportedLanguageList}}]);
                public ImmutableArray<string> SupportedLanguages => _supportedLanguages;

                public bool SupportsLanguage(string language)
                {
                    return language switch
                    {
            {{supportsLanguageArms}}
                        _ => false,
                    };
                }
                public string? GetLocalization(string language)
                {
                    return language switch
                    {
            {{localizationArms}}
                        _ => null,
                    };
                }
            }

            """;

        var fullSource = usings;
        var topLevelType = model.ClassType.IsTopLevelType();
        if (!topLevelType)
        {
            fullSource += namespaceCode;
        }
        fullSource += code;

        var sourceText = SourceText.From(fullSource, Encoding.UTF8);
        context.AddSource($"{fullDisplayName}.LocalizationStringAttributePart.g.cs", sourceText);
    }

    private static void GenerateLocalizationResourceType(
        SourceProductionContext context, LocalizationAttributeGenerationModel model)
    {
        var @class = model.ClassType.Name;
        var @namespace = model.ClassType.ContainingNamespace.ToDisplayString();
        var fullDisplayName = model.ClassType.ToDisplayString();

        var resourceTypeName = model.AssociatedResourceTypeName;

        var args = new AttributeGenerationArgs(model, @class);
        var localizationArms = new LocalizationAttributeCodeBuilder(args)
            .GenerateLocalizationArms();
        var constructorParameters = new LocalizationAttributeCodeBuilder(args)
            .GenerateLocalizationResourceRecordConstructorParameters();

        var initCurrentValue = model.DefaultLocalizationParameter?.AttachedPropertyName
            ?? "string.Empty";

        const string usings = """
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;
            
            """;

        string namespaceCode = $"""

            namespace {@namespace};

            """;

        string code = $$"""
            
            #nullable enable
            
            public sealed record {{resourceTypeName}}(
            {{constructorParameters}})
            {
                // Directly settable from outside the class for improved performance
                public string Current { get; internal set; } = {{initCurrentValue}};

                public string? GetResource(string language)
                {
                    return language switch
                    {
            {{localizationArms}}
                        _ => null,
                    };
                }
            }

            """;

        var fullSource = usings;
        var topLevelType = model.ClassType.IsTopLevelType();
        if (!topLevelType)
        {
            fullSource += namespaceCode;
        }
        fullSource += code;

        var sourceText = SourceText.From(fullSource, Encoding.UTF8);
        context.AddSource($"{fullDisplayName}.LocalizationStringResourceType.g.cs", sourceText);
    }

    private static LocalizationAttributeGenerationModel? RetrieveModelFromContext(
        GeneratorAttributeSyntaxContext sourceContext,
        CancellationToken cancellationToken)
    {
        var type = sourceContext.TargetSymbol as INamedTypeSymbol;
        if (type is null)
            return null;

        return LocalizationAttributeGenerationModel.RetrieveForType(type);
    }

    private sealed record AttributeGenerationArgs(
        LocalizationAttributeGenerationModel Model,
        string ClassName
        );
}
