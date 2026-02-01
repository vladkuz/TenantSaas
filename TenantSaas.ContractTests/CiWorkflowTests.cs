using FluentAssertions;
using System.Text.Json;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace TenantSaas.ContractTests;

public sealed class CiWorkflowTests
{
    [Fact]
    public void CiWorkflowFileShouldExist()
    {
        var root = FindRepoRoot();
        var ciPath = Path.Combine(root.FullName, ".github", "workflows", "ci.yml");

        File.Exists(ciPath).Should().BeTrue($"CI workflow must exist at {ciPath}");
    }

    [Fact]
    public void CiWorkflowShouldTriggerOnPullRequestAndPushToMain()
    {
        var root = FindRepoRoot();
        var workflow = LoadWorkflow(root);

        var onNode = GetMapping(workflow, "on");

        onNode.Children.Should().ContainKey(new YamlScalarNode("pull_request"));
        onNode.Children.Should().ContainKey(new YamlScalarNode("push"));

        var pushNode = GetMapping(onNode, "push");
        var branches = GetSequence(pushNode, "branches");
        branches.Select(node => ((YamlScalarNode)node).Value).Should().Contain("main");
    }

    [Fact]
    public void CiWorkflowShouldUseDotNetSdkVersionFromGlobalJson()
    {
        var root = FindRepoRoot();
        var workflow = LoadWorkflow(root);

        var dotnetVersion = ReadDotNetSdkVersion(root);
        var majorMinor = string.Join(".", dotnetVersion.Split('.').Take(2)) + ".x";
        var setupDotnetStep = FindStepByUses(workflow, "actions/setup-dotnet@v4");
        var withNode = GetMapping(setupDotnetStep, "with");
        var versionNode = GetScalar(withNode, "dotnet-version");

        versionNode.Value.Should().Be(majorMinor);
    }

    [Fact]
    public void CiWorkflowShouldContainRestoreBuildAndTestSteps()
    {
        var root = FindRepoRoot();
        var workflow = LoadWorkflow(root);
        var steps = GetSteps(workflow);

        steps.Should().Contain(step => StepRunContains(step, "dotnet restore"));
        steps.Should().Contain(step => StepRunContains(step, "dotnet build --no-restore --configuration Release"));
        steps.Should().Contain(step => StepRunContains(step, "dotnet test --no-build --configuration Release --verbosity minimal"));
    }

    [Fact]
    public void CiWorkflowShouldUseUbuntuLatestRunner()
    {
        var root = FindRepoRoot();
        var workflow = LoadWorkflow(root);
        var jobsNode = GetMapping(workflow, "jobs");
        var buildNode = GetMapping(jobsNode, "build");
        var runnerNode = GetScalar(buildNode, "runs-on");

        runnerNode.Value.Should().Be("ubuntu-latest");
    }

    private static YamlMappingNode LoadWorkflow(DirectoryInfo root)
    {
        var ciPath = Path.Combine(root.FullName, ".github", "workflows", "ci.yml");
        var yaml = File.ReadAllText(ciPath);
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        return (YamlMappingNode)stream.Documents[0].RootNode;
    }

    private static IReadOnlyList<YamlMappingNode> GetSteps(YamlMappingNode workflow)
    {
        var jobsNode = GetMapping(workflow, "jobs");
        var buildNode = GetMapping(jobsNode, "build");
        var stepsNode = GetSequence(buildNode, "steps");

        return stepsNode.Select(node => (YamlMappingNode)node).ToList();
    }

    private static YamlMappingNode FindStepByUses(YamlMappingNode workflow, string uses)
    {
        var steps = GetSteps(workflow);
        var match = steps.FirstOrDefault(step => GetScalar(step, "uses").Value == uses);

        match.Should().NotBeNull($"workflow should include a step that uses {uses}");
        return match!;
    }

    private static bool StepRunContains(YamlMappingNode step, string snippet)
    {
        if (!step.Children.TryGetValue(new YamlScalarNode("run"), out var runNode))
        {
            return false;
        }

        var runScalar = runNode as YamlScalarNode;
        return runScalar?.Value?.Contains(snippet, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string ReadDotNetSdkVersion(DirectoryInfo root)
    {
        var globalJsonPath = Path.Combine(root.FullName, "global.json");
        var json = File.ReadAllText(globalJsonPath);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sdk").GetProperty("version").GetString() ?? string.Empty;
    }

    private static YamlMappingNode GetMapping(YamlMappingNode node, string key)
    {
        node.Children.Should().ContainKey(new YamlScalarNode(key));
        return (YamlMappingNode)node.Children[new YamlScalarNode(key)];
    }

    private static YamlSequenceNode GetSequence(YamlMappingNode node, string key)
    {
        node.Children.Should().ContainKey(new YamlScalarNode(key));
        return (YamlSequenceNode)node.Children[new YamlScalarNode(key)];
    }

    private static YamlScalarNode GetScalar(YamlMappingNode node, string key)
    {
        node.Children.Should().ContainKey(new YamlScalarNode(key));
        return (YamlScalarNode)node.Children[new YamlScalarNode(key)];
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
