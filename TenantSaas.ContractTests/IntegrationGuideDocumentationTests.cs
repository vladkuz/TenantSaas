using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class IntegrationGuideDocumentationTests
{
    [Fact]
    public void IntegrationGuide_ExistsAndIsReferenced()
    {
        var root = FindRepoRoot();
        var docPath = Path.Combine(root.FullName, "docs", "integration-guide.md");

        File.Exists(docPath).Should().BeTrue("integration guide must exist at docs/integration-guide.md");
        File.ReadAllText(docPath).Should().Contain("# TenantSaas Integration Guide");
    }

    [Fact]
    public void IntegrationGuide_StatesBoundaryOnlyAdoption()
    {
        var doc = ReadIntegrationGuide();

        doc.Should().Contain("Boundary-Only Integration Summary");
        doc.Should().Contain("No domain logic changes required");
        doc.Should().Contain("Boundary-only wiring");
    }

    [Fact]
    public void IntegrationGuide_ContainsBoundaryVerificationSection()
    {
        var doc = ReadIntegrationGuide();

        doc.Should().Contain("Boundary Verification (Contract Tests)");
        doc.Should().Contain("Missing boundary configuration examples");
        doc.Should().Contain("Trust contract identifier");

        // AC2: failures point back to specific contract rules via invariant_code
        doc.Should().Contain("invariant_code");

        // AC3: explicit failure signals referencing missing boundary configuration
        doc.Should().Contain("ContextInitialized");
        doc.Should().Contain("TenantAttributionUnambiguous");
        doc.Should().Contain("TenantScopeRequired");
    }

    private static string ReadIntegrationGuide()
    {
        var root = FindRepoRoot();
        var docPath = Path.Combine(root.FullName, "docs", "integration-guide.md");
        return File.ReadAllText(docPath);
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
