using Dentextist;
using System;

namespace SourceLocalizationGenerator.SourceGenerators;

public static class BadNetStandardVersionExtensions
{
    public static void AppendSingleLineContent(this IndentedStringBuilder indented, string content)
    {
        indented.AppendSingleLineContent(content.AsSpan());
    }
}
