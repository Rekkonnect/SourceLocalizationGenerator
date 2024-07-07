using Microsoft.CodeAnalysis;
using System;

namespace SourceLocalizationGenerator.SourceGenerators;

public static class TypedConstantExtensions
{
    public static T? ValueOrDefault<T>(
        this TypedConstant constant, T? defaultValue = default)
    {
        if (constant.Kind is TypedConstantKind.Array)
        {
            return defaultValue;
        }

        var value = constant.Value;
        if (value is T castValue)
            return castValue;

        return defaultValue;
    }

    public static unsafe TEnum EnumValueOrDefault<TEnum, TUnderlying>(
        this TypedConstant constant, TEnum defaultValue = default)
        where TUnderlying : unmanaged
        where TEnum : unmanaged, Enum
    {
        if (constant.Kind is not TypedConstantKind.Enum)
        {
            return defaultValue;
        }

        var value = constant.Value;
        if (value is TUnderlying castValue)
            return *(TEnum*)&castValue;

        return defaultValue;
    }

    /// <summary>
    /// Returns the type of the constant based on the <see cref="TypedConstant.Type"/>
    /// property, or <see cref="NullTypeSymbol.Instance"/> if the constant
    /// represents a null reference value (<see cref="TypedConstant.IsNull"/> is
    /// <see langword="true"/>).
    /// </summary>
    public static ITypeSymbol? TypeOrNullType(this TypedConstant constant)
    {
        if (constant.IsNull)
            return NullTypeSymbol.Instance;

        return constant.Type;
    }
}
