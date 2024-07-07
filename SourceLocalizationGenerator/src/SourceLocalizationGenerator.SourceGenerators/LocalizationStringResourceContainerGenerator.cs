using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoseLynn;
using RoseLynn.CSharp.Syntax;
using SourceLocalizationGenerator.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace SourceLocalizationGenerator.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed partial class LocalizationStringResourceContainerGenerator
    : IIncrementalGenerator
{
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributePipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            WellKnownNames.Metadata.LocalizationStringResourceContainerAttribute,
            FilterSyntaxNode,
            RetrieveModelFromContext);

        context.RegisterSourceOutput(attributePipeline, GenerateFromModel);
    }

    private static bool FilterSyntaxNode(SyntaxNode node, CancellationToken token)
    {
        return true;
    }

    private static void GenerateFromModel(
        SourceProductionContext context, AttributeGenerationModel? model)
    {
        if (model is null)
            return;

        GenerateLocalizationContainerPart(context, model);
    }

    private static void GenerateLocalizationContainerPart(
        SourceProductionContext context, AttributeGenerationModel model)
    {
        var @class = model.ClassType.Name;
        var @namespace = model.ClassType.ContainingNamespace.ToDisplayString();
        var fullDisplayName = model.ClassType.ToDisplayString();

        var args = new AttributeGenerationArgs(model, @class);
        var propertyChangedFields = new LocalizationContainerCodeBuilder(args)
            .GeneratePropertyChangedFields();
        var localizationResourceFields = new LocalizationContainerCodeBuilder(args)
            .GenerateResourceFields();
        var localizationCases = new LocalizationContainerCodeBuilder(args)
            .GenerateLocalizationCases();
        var notifyPropertyLines = new LocalizationContainerCodeBuilder(args)
            .GenerateNotifyPropertyLines();

        var resourceAttributeTypes = model.LocalizationModels
            .Select(s => s.AttributeModel.ClassType)
            .ToImmutableArray();
        var provider = NamespaceUsingsProviderForTypes(resourceAttributeTypes);

        const string usings = """
            using SourceLocalizationGenerator.Core;
            using System;
            using System.Collections.Immutable;
            using System.ComponentModel;

            """;

        string namespaceCode = $"""

            namespace {@namespace};

            """;

        string code = $$"""

            #nullable enable
            
            sealed partial class {{@class}}
                : INotifyPropertyChanged
            {
            {{propertyChangedFields}}

                private string _currentLanguage = "{{model.DefaultLanguage}}";
            
            {{localizationResourceFields}}

                public event PropertyChangedEventHandler? PropertyChanged;

                public void SetLanguage(string language)
                {
                    if (_currentLanguage == language)
                        return;

                    _currentLanguage = language;

                    switch (language)
                    {
            {{localizationCases}}
                    }

            {{notifyPropertyLines}}
                }

                private void NotifyPropertyChanged(PropertyChangedEventArgs eventArgs)
                {
                    PropertyChanged?.Invoke(this, eventArgs);
                }
            }

            """;

        var fullSource = usings;
        fullSource += provider.DefaultNecessaryUsings;
        fullSource += "\r\n";
        var topLevelType = model.ClassType.IsTopLevelType();
        if (!topLevelType)
        {
            fullSource += namespaceCode;
        }
        fullSource += code;

        var sourceText = SourceText.From(fullSource, Encoding.UTF8);
        context.AddSource(
            $"{fullDisplayName}.LocalizationResourceContainer.Implementation.g.cs",
            sourceText);
    }

    private static UsingsProviderBase NamespaceUsingsProviderForTypes(
        IEnumerable<ITypeSymbol> types)
    {
        var namespaces = DistinctNamespacesForTypes(types);
        var qualifiedNames = QualifiedNamespaceStrings(namespaces);
        return UsingsProviderBase.ForUsings(
            UsingDirectiveKind.Using,
            false,
            [.. qualifiedNames]);
    }

    private static ImmutableArray<string> QualifiedNamespaceStrings(
        IEnumerable<INamespaceSymbol> namespaces)
    {
        var qualifiedNames = ImmutableArray.CreateBuilder<string>();

        foreach (var @namespace in namespaces)
        {
            if (@namespace.IsGlobalNamespace)
                continue;

            qualifiedNames.Add(@namespace.ToDisplayString());
        }

        return qualifiedNames.ToImmutable();
    }

    private static IReadOnlyCollection<INamespaceSymbol> DistinctNamespacesForTypes(
        IEnumerable<ITypeSymbol> types)
    {
        var distinctTypes = types.Distinct(SymbolEqualityComparer.Default);
        var namespaces = new HashSet<INamespaceSymbol>(SymbolEqualityComparer.Default);

        foreach (var type in types)
        {
            var containingNamespace = type.ContainingNamespace;
            namespaces.Add(containingNamespace);
        }

        return namespaces;
    }

    private static AttributeGenerationModel? RetrieveModelFromContext(
        GeneratorAttributeSyntaxContext sourceContext,
        CancellationToken cancellationToken)
    {
        var type = sourceContext.TargetSymbol as INamedTypeSymbol;
        if (type is null)
            return null;

        var attributes = type.GetAttributes()
            .Where(IsLocalizationAttribute)
            .ToImmutableArray()
            ;

        var attributeModelDictionary = new LocalizationAttributeTypeDictionary();
        var localizationAttributes = ImmutableArray
            .CreateBuilder<LocalizationResourceAttributeInstanceModel>();

        foreach (var attribute in attributes)
        {
            var localizationAttribute = LocalizationResourceAttributeInstanceModel
                .RetrieveForAttribute(attributeModelDictionary, attribute);

            if (localizationAttribute is null)
                continue;

            localizationAttributes.Add(localizationAttribute);
        }

        return new AttributeGenerationModel(
            type,
            attributeModelDictionary,
            localizationAttributes.ToImmutable());

        static bool IsLocalizationAttribute(AttributeData attribute)
        {
            return attribute.AttributeClass!.Interfaces
                .Any(s => s.Name is nameof(ILocalizationStringAttribute));
        }
    }

    private sealed record AttributeGenerationArgs(
        AttributeGenerationModel Model,
        string ClassName
        );

    private sealed record AttributeGenerationModel(
        INamedTypeSymbol ClassType,
        LocalizationAttributeTypeDictionary LocalizationAttributeTypes,
        ImmutableArray<LocalizationResourceAttributeInstanceModel> LocalizationModels
        )
    {
        public string? DefaultLanguage { get; }
            = LocalizationModels
                .FirstOrDefault()
                ?.AttributeModel
                .DefaultLocalizationParameter
                ?.Language
                ;

        public ImmutableArray<string> Languages { get; }
            = LocalizationModels
                .SelectMany(s => s.Values.Select(s => s.Language))
                .Distinct()
                .ToImmutableArray()
                ;
    }

    private sealed record LocalizationResourceAttributeInstanceModel(
        AttributeData Attribute,
        LocalizationAttributeGenerationModel AttributeModel,
        string ResourceName,
        ImmutableArray<LocalizationResourceEntry> Values
        )
    {
        public bool HasNullNonDefaultResources { get; } = CheckHasNullNonDefaultResources(
            AttributeModel.DefaultLocalizationParameter?.Language,
            Values);

        private static bool CheckHasNullNonDefaultResources(
            string? defaultLanguage,
            ImmutableArray<LocalizationResourceEntry> values)
        {
            foreach (var value in values)
            {
                if (value.Language != defaultLanguage && value.Value is not null)
                    return false;
            }

            return true;
        }

        public static LocalizationResourceAttributeInstanceModel? RetrieveForAttribute(
            LocalizationAttributeTypeDictionary localizationAttributeTypeDictionary,
            AttributeData attribute)
        {
            var attributeModel = localizationAttributeTypeDictionary
                .ModelFor(attribute.AttributeClass!);

            if (attributeModel is null)
                return null;

            var resourceNameOrdinal = attributeModel.ResourceNameParameter.Parameter.Ordinal;

            var resourceNameArgument = attribute.ConstructorArgumentAtOrDefault(resourceNameOrdinal);
            var validResourceName = resourceNameArgument.Kind is TypedConstantKind.Primitive
                && resourceNameArgument.Type?.SpecialType is SpecialType.System_String
                ;

            if (!validResourceName)
                return null;

            var resourceName = resourceNameArgument.Value as string;
            if (resourceName is null)
            {
                return null;
            }

            var localizationValues = ImmutableArray.CreateBuilder<LocalizationResourceEntry>();

            foreach (var parameter in attributeModel.LocalizationParameters)
            {
                var ordinal = parameter.Parameter.Ordinal;
                var value = attribute.ConstructorArgumentAtOrDefault(ordinal);

                var validValue = value.Kind is TypedConstantKind.Primitive
                    && value.Type?.SpecialType is SpecialType.System_String
                    ;

                if (!validValue)
                    continue;

                var valueString = value.Value as string;

                localizationValues.Add(new(parameter.Language, valueString));
            }

            return new LocalizationResourceAttributeInstanceModel(
                attribute,
                attributeModel,
                resourceName,
                localizationValues.ToImmutable());
        }
    }

    private sealed class LocalizationAttributeTypeDictionary
    {
        private readonly Dictionary<INamedTypeSymbol, LocalizationAttributeGenerationModel>
            _dictionary = new(SymbolEqualityComparer.Default);

        public LocalizationAttributeGenerationModel? ModelFor(INamedTypeSymbol type)
        {
            if (!_dictionary.TryGetValue(type, out var model))
            {
                model = LocalizationAttributeGenerationModel.RetrieveForType(type);
                if (model is null)
                    return null;

                _dictionary.Add(type, model);
            }

            return model;
        }
    }

    private readonly record struct LocalizationResourceEntry(string Language, string? Value);
}
