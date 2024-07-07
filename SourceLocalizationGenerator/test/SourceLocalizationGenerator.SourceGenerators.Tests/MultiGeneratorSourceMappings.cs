using Microsoft.CodeAnalysis;
using RoseLynn.Generators;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public sealed class MultiGeneratorSourceMappings
    : IEnumerable<KeyValuePair<Type, GeneratedSourceMappings>>
{
    private readonly Dictionary<Type, GeneratedSourceMappings> _mappings = new();

    public IReadOnlyDictionary<Type, GeneratedSourceMappings> Mappings => _mappings;

    public void Add(IIncrementalGenerator generator, GeneratedSourceMappings mappings)
    {
        _mappings.Add(generator.GetType(), mappings);
    }

    public void Add(Type type, GeneratedSourceMappings mappings)
    {
        _mappings.Add(type, mappings);
    }

    public IEnumerator<KeyValuePair<Type, GeneratedSourceMappings>> GetEnumerator()
    {
        return _mappings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
