using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class VerificationGuideDocumentationTests
{
    [Fact]
    public void VerificationGuide_ExistsAtCanonicalPath()
    {
        var path = GetVerificationGuidePath();

        File.Exists(path).Should().BeTrue("verification guide must exist at docs/verification-guide.md");
    }

    [Fact]
    public void VerificationGuide_ContainsRequiredCommandsAndFailureSignals()
    {
        var doc = ReadVerificationGuide();

        doc.Should().Contain("dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal");
        doc.Should().Contain("dotnet test TenantSaas.sln --disable-build-servers -v minimal");
        doc.Should().Contain("Problem Details");
        doc.Should().Contain("invariant_code");
        doc.Should().Contain("trace_id");
        doc.Should().Contain("contract rule");
    }

    [Fact]
    public void VerificationGuide_ContainsTroubleshootingSection()
    {
        var doc = ReadVerificationGuide();

        doc.Should().Contain("## Troubleshooting");
        doc.Should().Contain("Common failures");
    }

    [Fact]
    public void VerificationGuide_ContainsUpdateGuidance()
    {
        var doc = ReadVerificationGuide();

        doc.Should().Contain("## Updating This Guide When Steps Change");
    }

    [Fact]
    public void VerificationGuide_ContainsTrustContractIdentifiers()
    {
        var doc = ReadVerificationGuide();

        doc.Should().Contain("ContextInitialized");
        doc.Should().Contain("TenantAttributionUnambiguous");
        doc.Should().Contain("TenantScopeRequired");
        doc.Should().Contain("BreakGlassExplicitAndAudited");
        doc.Should().Contain("DisclosureSafe");
    }

    [Fact]
    public void VerificationGuide_IsLinkedFromReadme()
    {
        var root = FindRepoRoot();
        var readmePath = Path.Combine(root.FullName, "README.md");
        var readme = File.ReadAllText(readmePath);

        readme.Should().Contain("docs/verification-guide.md");
    }

    private static string ReadVerificationGuide()
    {
        var path = GetVerificationGuidePath();
        File.Exists(path).Should().BeTrue("verification guide must exist at docs/verification-guide.md");
        return File.ReadAllText(path);
    }

    private static string GetVerificationGuidePath()
    {
        var root = FindRepoRoot();
        return Path.Combine(root.FullName, "docs", "verification-guide.md");
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
