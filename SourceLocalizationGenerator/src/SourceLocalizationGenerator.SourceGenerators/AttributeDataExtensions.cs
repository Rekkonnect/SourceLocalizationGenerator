using Microsoft.CodeAnalysis;
using System.Linq;

namespace SourceLocalizationGenerator.SourceGenerators;

public static class AttributeDataExtensions
{
    public static TypedConstant ConstructorArgumentAtOrDefault(
        this AttributeData attribute, int index)
    {
        var args = attribute.ConstructorArguments;
        if (args.Length <= index)
            return default;

        return args[index];
    }

    public static TypedConstant GetNamedArgumentValue(
        this AttributeData attribute,
        string name)
    {
        return attribute.NamedArguments
            .FirstOrDefault(s => s.Key == name)
            .Value;
    }
}
