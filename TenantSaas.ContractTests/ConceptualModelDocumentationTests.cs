using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed partial class ConceptualModelDocumentationTests
{
    [Fact]
    public void ConceptualModel_Exists_AndLinksTrustContract()
    {
        var root = FindRepoRoot();
        var docPath = Path.Combine(root.FullName, "docs", "conceptual-model.md");

        File.Exists(docPath).Should().BeTrue("conceptual model must exist at docs/conceptual-model.md");

        var doc = File.ReadAllText(docPath);
        doc.Should().Contain("docs/trust-contract.md");
    }

    [Fact]
    public void ConceptualModel_StaysWithinWordLimit()
    {
        var doc = ReadConceptualModel();
        var wordCount = CountWords(doc);

        wordCount.Should().BeLessThanOrEqualTo(800, "conceptual model must stay within onboarding length limits");
    }

    [Fact]
    public void ConceptualModel_CoversCoreBoundaryConcepts()
    {
        var doc = ReadConceptualModel();
        var lowered = doc.ToLowerInvariant();

        lowered.Should().Contain("tenancy");
        doc.Should().Contain("TenantScope");
        doc.Should().Contain("ExecutionKind");
        lowered.Should().Contain("shared-system");
        lowered.Should().Contain("invariant");
    }

    [Fact]
    public void Readme_LinksToConceptualModel()
    {
        var root = FindRepoRoot();
        var readmePath = Path.Combine(root.FullName, "README.md");
        var readme = File.ReadAllText(readmePath);

        readme.Should().Contain("docs/conceptual-model.md");
    }

    private static string ReadConceptualModel()
    {
        var root = FindRepoRoot();
        var docPath = Path.Combine(root.FullName, "docs", "conceptual-model.md");
        File.Exists(docPath).Should().BeTrue("conceptual model must exist at docs/conceptual-model.md");
        return File.ReadAllText(docPath);
    }

    private static int CountWords(string content) =>
        WordPattern().Matches(content).Count;

    [GeneratedRegex(@"\b[\p{L}\p{N}][\p{L}\p{N}'-]*\b")]
    private static partial Regex WordPattern();

    private static DirectoryInfo FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var readmePath = Path.Combine(current.FullName, "README.md");
            if (File.Exists(readmePath))
            {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("README.md not found from test execution directory.");
    }
}
