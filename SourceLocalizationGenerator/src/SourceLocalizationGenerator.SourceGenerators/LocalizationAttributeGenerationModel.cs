using Microsoft.CodeAnalysis;
using SourceLocalizationGenerator.Core;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace SourceLocalizationGenerator.SourceGenerators;

public sealed record LocalizationAttributeGenerationModel(
    INamedTypeSymbol ClassType,
    string AssociatedResourceTypeName,
    ParameterLocalizationInfo ResourceNameParameter,
    ParameterLocalizationInfo? DefaultLocalizationParameter,
    ImmutableArray<ParameterLocalizationInfo> LocalizationParameters
    )
{
    public IMethodSymbol ConstructorSymbol { get; }
        = (ResourceNameParameter.Parameter.ContainingSymbol as IMethodSymbol)!;

    public ImmutableArray<string> Languages { get; } = LocalizationParameters
        .Select(s => s.Language)
        .ToImmutableArray();

    public static LocalizationAttributeGenerationModel? RetrieveForType(
        INamedTypeSymbol type)
    {
        var constructors = type.InstanceConstructors;
        if (constructors.Length is not 1)
            return null;

        var constructor = constructors.First();

        var parameterLocalizations = GetParameterLocalizations(constructor);
        if (parameterLocalizations.IsEmpty)
            return null;

        var defaultLocalization = parameterLocalizations.FirstOrDefault(
            p => p.Default);

        var resourceNameParameter = GetResourceNameParameter(constructor);
        if (resourceNameParameter is null)
            return null;

        var resourceNameParameterInfo = new ParameterLocalizationInfo(
            resourceNameParameter,
            resourceNameParameter.Name.PascalCase(),
            string.Empty,
            false);

        var resourceTypeName = GenerateLocalizationResourceTypeName(type.Name);

        var model = new LocalizationAttributeGenerationModel(
            type,
            resourceTypeName,
            resourceNameParameterInfo,
            defaultLocalization,
            parameterLocalizations);
        return model;
    }

    private static string GenerateLocalizationResourceTypeName(string attributeName)
    {
        const string attributeSuffix = "Attribute";
        if (attributeName.EndsWith(attributeSuffix))
        {
            attributeName = attributeName[..^attributeSuffix.Length];
        }

        return $"{attributeName}Resource";
    }

    private static ImmutableArray<ParameterLocalizationInfo> GetParameterLocalizations(
        IMethodSymbol constructor)
    {
        Debug.Assert(constructor.MethodKind is MethodKind.Constructor);

        var parameters = constructor.Parameters;
        var mappings = ImmutableArray.CreateBuilder<ParameterLocalizationInfo>(
            parameters.Length);
        foreach (var parameter in parameters)
        {
            var localizationInfo = GetLocalizationInfo(parameter);
            if (localizationInfo is null)
                continue;

            mappings.Add(localizationInfo);
        }

        return mappings.ToImmutable();
    }

    private static ParameterLocalizationInfo? GetLocalizationInfo(IParameterSymbol parameter)
    {
        var attribute = GetLocalizationStringResourceValueAttribute(parameter);
        if (attribute is null)
            return null;

        var languageArgument = attribute.ConstructorArguments.FirstOrDefault();
        var language = languageArgument.ValueOrDefault<string>();

        if (language is null)
            return null;

        var defaultArgument = attribute.NamedArguments.FirstOrDefault(
            a => a.Key is nameof(LocalizationStringResourceValueAttribute.Default));
        var isDefault = defaultArgument.Value.ValueOrDefault<bool>();

        var propertyName = parameter.Name.PascalCase();
        return new(parameter, propertyName, language, isDefault);
    }

    public static AttributeData? GetLocalizationStringResourceValueAttribute(
        IParameterSymbol parameter)
    {
        return parameter
            .GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString()
                is WellKnownNames.Metadata.LocalizationStringResourceValueAttribute)
            .FirstOrDefault();
    }

    private static IParameterSymbol? GetResourceNameParameter(IMethodSymbol constructor)
    {
        return constructor.Parameters.FirstOrDefault(HasRequiredAttribute);

        static bool HasRequiredAttribute(IParameterSymbol symbol)
        {
            return HasAttributeNamed(
                symbol,
                nameof(LocalizationStringResourceNameAttribute));
        }
    }

    private static bool HasAttributeNamed(ISymbol symbol, string name)
    {
        var attributes = symbol.GetAttributes();
        return attributes.Any(a => a.AttributeClass?.Name == name);
    }
}
