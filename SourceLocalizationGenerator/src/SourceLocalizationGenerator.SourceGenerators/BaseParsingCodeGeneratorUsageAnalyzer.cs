using Microsoft.CodeAnalysis.Diagnostics;
using RoseLynn.Analyzers;
using RoseLynn.CSharp;
using SourceLocalizationGenerator.Core;

namespace SourceLocalizationGenerator.SourceGenerators;

public abstract class BaseParsingCodeGeneratorUsageAnalyzer : CSharpDiagnosticAnalyzer
{
    protected sealed override DiagnosticDescriptorStorageBase DiagnosticDescriptorStorage
        => SLGDiagnosticDescriptorStorage.Instance;

    public sealed override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);

        RegisterNodeActions(context);
    }

    private void RegisterNodeActions(AnalysisContext context)
    {
        context.RegisterTargetAttributeSyntaxNodeAction(
            AnalyzeGenerateParsingCodeElement,
            nameof(GeneratedLocalizationStringAttribute));
    }

    protected abstract void AnalyzeGenerateParsingCodeElement(SyntaxNodeAnalysisContext context);
}
