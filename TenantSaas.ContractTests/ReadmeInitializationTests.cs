using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class ReadmeInitializationTests
{
    [Fact]
    public void ReadmeContainsInitializationCommands()
    {
        var readme = ReadReadme();

        readme.Should().Contain("dotnet new sln -n TenantSaas");
        readme.Should().Contain("dotnet new classlib -n TenantSaas.Core -f net10.0");
        readme.Should().Contain("dotnet new classlib -n TenantSaas.EfCore -f net10.0");
        readme.Should().Contain("dotnet new xunit -n TenantSaas.ContractTests -f net10.0");
        readme.Should().Contain("dotnet new webapi -n TenantSaas.Sample -f net10.0");
        readme.Should().Contain("dotnet sln TenantSaas.sln add");
        readme.Should().Contain("TenantSaas.Core/TenantSaas.Core.csproj");
        readme.Should().Contain("TenantSaas.EfCore/TenantSaas.EfCore.csproj");
        readme.Should().Contain("TenantSaas.ContractTests/TenantSaas.ContractTests.csproj");
        readme.Should().Contain("TenantSaas.Sample/TenantSaas.Sample.csproj");
    }

    [Fact]
    public void InitializationArtifactsExistAndTargetNet10()
    {
        var root = FindRepoRoot();

        File.Exists(Path.Combine(root.FullName, "TenantSaas.sln")).Should().BeTrue();
        File.Exists(Path.Combine(root.FullName, "TenantSaas.Core", "TenantSaas.Core.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(root.FullName, "TenantSaas.EfCore", "TenantSaas.EfCore.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(root.FullName, "TenantSaas.ContractTests", "TenantSaas.ContractTests.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(root.FullName, "TenantSaas.Sample", "TenantSaas.Sample.csproj")).Should().BeTrue();

        var solution = File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.sln"));
        solution.Should().Contain("TenantSaas.Core");
        solution.Should().Contain("TenantSaas.EfCore");
        solution.Should().Contain("TenantSaas.ContractTests");
        solution.Should().Contain("TenantSaas.Sample");

        File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.Core", "TenantSaas.Core.csproj"))
            .Should().Contain("<TargetFramework>net10.0</TargetFramework>");
        File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.EfCore", "TenantSaas.EfCore.csproj"))
            .Should().Contain("<TargetFramework>net10.0</TargetFramework>");
        File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.ContractTests", "TenantSaas.ContractTests.csproj"))
            .Should().Contain("<TargetFramework>net10.0</TargetFramework>");
        File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.Sample", "TenantSaas.Sample.csproj"))
            .Should().Contain("<TargetFramework>net10.0</TargetFramework>");

        var program = File.ReadAllText(Path.Combine(root.FullName, "TenantSaas.Sample", "Program.cs"));
        program.Should().Contain("AddEndpointsApiExplorer");
        program.Should().Contain("MapGet(\"/weatherforecast\"");

        Directory.Exists(Path.Combine(root.FullName, "src")).Should().BeFalse();
        Directory.Exists(Path.Combine(root.FullName, "tests")).Should().BeFalse();
    }

    [Fact]
    public void ReadmeContainsMissingTemplateResolutionGuidance()
    {
        var readme = ReadReadme();

        readme.Should().Contain("Troubleshooting missing templates or dependencies");
        readme.Should().Contain("net10.0 is not a valid value for -f");
        readme.Should().Contain("No templates found matching");
        readme.Should().Contain("dotnet new --install Microsoft.DotNet.Common.ProjectTemplates.10.0");
        readme.Should().Contain("Unable to load the service index for source https://api.nuget.org/v3/index.json");
        readme.Should().Contain("dotnet restore");
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
