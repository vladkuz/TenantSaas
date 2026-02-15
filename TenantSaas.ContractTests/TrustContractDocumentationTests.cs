using System.Text.RegularExpressions;
using FluentAssertions;
using TenantSaas.Abstractions.TrustContract;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed partial class TrustContractDocumentationTests
{
    [Fact]
    public void TrustContractDoc_ExistsAtCanonicalPath()
    {
        var docPath = GetTrustContractPath();

        File.Exists(docPath).Should().BeTrue("release is blocked when docs/trust-contract.md is missing");
    }

    [Fact]
    public void TrustContractDoc_ListsAllContractInvariants()
    {
        var doc = ReadTrustContractDoc();

        foreach (var invariant in TrustContractV1.Invariants.Values)
        {
            doc.Should().Contain(
                invariant.InvariantCode,
                $"release is blocked when invariant '{invariant.InvariantCode}' is undocumented");
            doc.Should().Contain(
                invariant.Description,
                $"release is blocked when description for invariant '{invariant.InvariantCode}' does not match code");
        }
    }

    [Fact]
    public void TrustContractDoc_ListsAllRefusalMappingsWithStableIdentifiers()
    {
        var doc = ReadTrustContractDoc();
        var section = GetSection(doc, "Refusal Mapping Registry");

        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            section.Should().Contain(
                mapping.InvariantCode,
                $"release is blocked when refusal mapping for '{mapping.InvariantCode}' is missing");
            section.Should().Contain(
                $"| {mapping.HttpStatusCode} |",
                $"release is blocked when HTTP status for '{mapping.InvariantCode}' is missing");
            section.Should().Contain(
                mapping.ProblemType,
                $"release is blocked when problem type for '{mapping.InvariantCode}' is missing");
            section.Should().Contain(
                mapping.Title,
                $"release is blocked when title for '{mapping.InvariantCode}' is missing");
            section.Should().Contain(
                mapping.GuidanceUri,
                $"release is blocked when guidance URI for '{mapping.InvariantCode}' is missing");
        }
    }

    [Fact]
    public void TrustContractDoc_DeclaresDisclosureRulesAndSafeStates()
    {
        var doc = ReadTrustContractDoc();
        var disclosureSection = GetSection(doc, "Disclosure Policy");

        foreach (var safeState in TrustContractV1.RequiredDisclosureSafeStates)
        {
            disclosureSection.Should().Contain(
                safeState,
                $"disclosure safe state '{safeState}' must be documented");
        }

        disclosureSection.Should().Contain("Error Disclosure Rules");
        disclosureSection.Should().Contain("Log Disclosure Rules");
    }

    private static string ReadTrustContractDoc()
    {
        var path = GetTrustContractPath();
        File.Exists(path).Should().BeTrue("release is blocked when docs/trust-contract.md is missing");
        return File.ReadAllText(path);
    }

    private static string GetTrustContractPath()
    {
        var root = FindRepoRoot();
        return Path.Combine(root.FullName, "docs", "trust-contract.md");
    }

    private static string GetSection(string doc, string heading)
    {
        var marker = $"## {heading}";
        var start = doc.IndexOf(marker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0, $"section '{heading}' should exist in docs/trust-contract.md");

        var next = HeadingPattern().Match(doc, start + marker.Length);
        return next.Success ? doc[start..next.Index] : doc[start..];
    }

    [GeneratedRegex("^##\\s+", RegexOptions.Multiline)]
    private static partial Regex HeadingPattern();

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
