using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class ExtensionSeamsDocumentationTests
{
    [Fact]
    public void ExtensionSeamsDoc_ListsRequiredSeams()
    {
        var doc = ReadExtensionSeams();

        doc.Should().Contain("# Extension Seams");
        doc.Should().Contain("Tenant attribution resolution");
        doc.Should().Contain("ITenantAttributionResolver");
        doc.Should().Contain("Tenant context access");
        doc.Should().Contain("ITenantContextAccessor");
        doc.Should().Contain("IMutableTenantContextAccessor");
        doc.Should().Contain("Invariant evaluation");
        doc.Should().Contain("IBoundaryGuard");
        doc.Should().Contain("Refusal mapping");
        doc.Should().Contain("TrustContractV1");
        doc.Should().Contain("Log enrichment");
        doc.Should().Contain("ILogEnricher");
        doc.Should().Contain("DefaultLogEnricher");
    }

    [Fact]
    public void ExtensionSeamsDoc_DeclaresCustomizableAndInvariantProtectedRules()
    {
        var doc = ReadExtensionSeams();

        AssertSectionHasRules(doc, "Tenant attribution resolution");
        AssertSectionHasRules(doc, "Tenant context access");
        AssertSectionHasRules(doc, "Invariant evaluation");
        AssertSectionHasRules(doc, "Refusal mapping");
        AssertSectionHasRules(doc, "Log enrichment");
    }

    private static string ReadExtensionSeams()
    {
        var root = FindRepoRoot();
        var docPath = Path.Combine(root.FullName, "docs", "extension-seams.md");
        return File.ReadAllText(docPath);
    }

    private static void AssertSectionHasRules(string doc, string heading)
    {
        var section = GetSection(doc, heading);

        section.Should().Contain("Customizable:");
        section.Should().Contain("Invariant-protected:");
        section.Should().Contain("Contract tests:");
    }

    private static string GetSection(string doc, string heading)
    {
        var marker = $"## {heading}";
        var start = doc.IndexOf(marker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0, $"Section '{heading}' should exist");

        var next = doc.IndexOf("## ", start + marker.Length, StringComparison.Ordinal);
        return next < 0 ? doc[start..] : doc[start..next];
    }

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
