using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class ReadmeSetupTests
{
    [Fact]
    public void ReadmeContainsPrerequisitesSection()
    {
        var readme = ReadReadme();

        readme.Should().Contain("## Prerequisites");
        readme.Should().Contain(".NET SDK 10.0.102");
    }

    [Fact]
    public void ReadmeContainsLocalSetupSection()
    {
        var readme = ReadReadme();

        readme.Should().Contain("## Local Setup");
        readme.Should().Contain("dotnet restore");
        readme.Should().Contain("dotnet build TenantSaas.sln");
        readme.Should().Contain("dotnet run --project TenantSaas.Sample/TenantSaas.Sample.csproj");
    }

    [Fact]
    public void ReadmeContainsVerificationSection()
    {
        var readme = ReadReadme();

        readme.Should().Contain("## Verification");
        readme.Should().Contain("/health");
        readme.Should().Contain("\"status\":\"healthy\"");
    }

    [Fact]
    public void ReadmeContainsTroubleshootingSectionWithCommonFailures()
    {
        var readme = ReadReadme();

        readme.Should().Contain("## Troubleshooting");
        readme.Should().Contain("dotnet --list-sdks");
        readme.Should().Contain("dotnet dev-certs https --trust");
        readme.Should().Contain("Unable to load the service index for source https://api.nuget.org/v3/index.json");
    }

    private static string ReadReadme()
    {
        var root = FindRepoRoot();
        var readmePath = Path.Combine(root.FullName, "README.md");
        return File.ReadAllText(readmePath);
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
