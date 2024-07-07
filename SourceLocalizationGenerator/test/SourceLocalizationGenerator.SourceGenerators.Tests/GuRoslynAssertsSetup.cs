using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public class GuRoslynAssertsSetup
{
    [ModuleInitializer]
    public static void Setup()
    {
        SetupDefaultSettings<GuRoslynAssertsSetup>();
    }

    public static void SetupDefaultSettings<TSetup>()
    {
        Settings.Default = Settings.Default
            .WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors)
            .WithMetadataReferences(MetadataReferences.Transitive(typeof(TSetup)))
            .WithCompilationOptions(
                CodeFactory.DefaultCompilationOptions([
                    // Unnecessary using directive
                    new("CS8019", ReportDiagnostic.Suppress)
                ]))
            ;
    }
}
