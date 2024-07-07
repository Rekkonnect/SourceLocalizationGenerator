using RoseLynn.Analyzers;
using System.Resources;

namespace SourceLocalizationGenerator.SourceGenerators;

public class SLGDiagnosticDescriptorStorage : DiagnosticDescriptorStorageBase
{
    public static readonly SLGDiagnosticDescriptorStorage Instance = new();

    protected override string BaseRuleDocsURI => "https://github.com/Rekkonnect/SourceLocalizationGenerator/tree/master/SourceLocalizationGenerator/docs/rules";
    protected override string DiagnosticIDPrefix => "SLG";
    protected override ResourceManager ResourceManager => AnalyzerResources.ResourceManager;

    #region Category Constants
    public const string APIRestrictionsCategory = "API Restrictions";
    public const string BrevityCategory = "Brevity";
    public const string DesignCategory = "Design";
    public const string InformationCategory = "Information";
    public const string ValidityCategory = "Validity";
    #endregion

    #region Rules
    private SLGDiagnosticDescriptorStorage()
    {
    }
    #endregion
}
