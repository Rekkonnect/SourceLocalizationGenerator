using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RoseLynn;
using RoseLynn.Analyzers;
using RoseLynn.Testing;

namespace SourceLocalizationGenerator.SourceGenerators.Tests;

public abstract class BaseParsingCodeGeneratorDiagnosticTests<TAnalyzer>
    : BaseParsingCodeGeneratorDiagnosticTests
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected sealed override TAnalyzer GetNewDiagnosticAnalyzerInstance() => new();
}

public abstract class BaseParsingCodeGeneratorDiagnosticTests : BaseDiagnosticTests
{
    protected ExpectedDiagnostic ExpectedDiagnostic => ExpectedDiagnostic.Create(TestedDiagnosticRule);
    protected sealed override DiagnosticDescriptorStorageBase DiagnosticDescriptorStorage
        => SLGDiagnosticDescriptorStorage.Instance;

    protected override UsingsProviderBase GetNewUsingsProviderInstance()
    {
        return UsingsProviderBase.Default;
    }

    protected override void ValidateCode(string testCode)
    {
        RoslynAssert.Valid(GetNewDiagnosticAnalyzerInstance(), testCode);
    }
    protected override void AssertDiagnostics(string testCode)
    {
        RoslynAssert.Diagnostics(
            GetNewDiagnosticAnalyzerInstance(),
            ExpectedDiagnostic,
            testCode);
    }
    protected void AssertDiagnostics(
        string testCode,
        ExpectedDiagnostic customExpectedDiagnostic)
    {
        RoslynAssert.Diagnostics(
            GetNewDiagnosticAnalyzerInstance(),
            customExpectedDiagnostic,
            testCode);
    }
    protected void AssertDiagnosticsWithMessage(
        string testCode,
        string additionalMessage)
    {
        var id = TestedDiagnosticRule.Id;
        var expectedDiagnostic = ExpectedDiagnostic.Create(id, additionalMessage);
        RoslynAssert.Diagnostics(
            GetNewDiagnosticAnalyzerInstance(),
            expectedDiagnostic,
            testCode);
    }

    [Test]
    public void EmptyCode()
    {
        ValidateCode(string.Empty);
    }
}
