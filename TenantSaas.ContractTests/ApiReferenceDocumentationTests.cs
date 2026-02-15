using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class ApiReferenceDocumentationTests
{
    private static readonly string[] RequiredMarkers =
    [
        "## TenantSaas.Abstractions",
        "ExecutionKind",
        "TenantScope",
        "NoTenantReason",
        "TenantId",
        "TenantContext",
        "TenantAttributionSource",
        "TenantAttributionSourceMetadata",
        "AttributionStrategy",
        "TenantAttributionRules",
        "TenantAttributionRuleSet",
        "TenantAttributionInput",
        "TenantAttributionInputs",
        "TenantAttributionResult",
        "AttributionConflict",
        "ITenantAttributionResolver",
        "ITenantContextAccessor",
        "IMutableTenantContextAccessor",
        "ITenantContextInitializer",
        "ITenantFlowFactory",
        "ITenantFlowScope",
        "InvariantCode",
        "InvariantDefinition",
        "RefusalMapping",
        "TrustContractV1",
        "TrustContractValidationResult",
        "DisclosurePolicy",
        "DisclosureContext",
        "DisclosureValidator",
        "DisclosureValidationResult",
        "TenantRef",
        "TenantRefSafeState",
        "IDisclosurePolicyProvider",
        "BreakGlassDeclaration",
        "BreakGlassValidator",
        "BreakGlassValidationResult",
        "BreakGlassAuditEvent",
        "AuditCode",
        "IBreakGlassAuditSink",
        "StructuredLogEvent",
        "ILogEnricher",
        "## TenantSaas.Core",
        "IBoundaryGuard",
        "BoundaryGuard",
        "EnforcementResult",
        "AttributionEnforcementResult",
        "BreakGlassAuditHelper",
        "TenantContextInitializer",
        "TenantAttributionResolver",
        "TenantFlowFactory",
        "AmbientTenantContextAccessor",
        "ExplicitTenantContextAccessor",
        "TenantContextConflictException",
        "ProblemDetailsFactory",
        "ProblemDetailsExtensions",
        "DefaultLogEnricher",
        "EnforcementEventSource",
        "EnforcementEventNames",
        "LoggingDefaults",
        "## TenantSaas.Sample",
        "SampleApp",
        "Program",
        "TenantContextMiddleware",
        "ProblemDetailsExceptionMiddleware",
        "HttpCorrelationExtensions",
        "/health",
        "/tenants/{tenantId}/data",
        "/test/attribution",
        "/weatherforecast",
        "## Extension Seams",
        "ITenantAttributionResolver",
        "ITenantContextAccessor",
        "IMutableTenantContextAccessor",
        "IBoundaryGuard",
        "ILogEnricher",
        "## Trust Contract Identifiers",
        "ContextInitialized",
        "TenantAttributionUnambiguous",
        "TenantScopeRequired",
        "BreakGlassExplicitAndAudited",
        "DisclosureSafe"
    ];

    [Fact]
    public void ApiReference_ExistsAtCanonicalPath()
    {
        var path = GetApiReferencePath();

        File.Exists(path).Should().BeTrue("API reference must exist at docs/api-reference.md");
    }

    [Fact]
    public void ApiReference_DocumentsKnownPublicSurface()
    {
        var doc = ReadApiReference();

        foreach (var marker in RequiredMarkers)
        {
            doc.Should().Contain(
                marker,
                $"release is blocked when API reference omits '{marker}'");
        }
    }

    private static string ReadApiReference()
    {
        var path = GetApiReferencePath();
        File.Exists(path).Should().BeTrue("API reference must exist at docs/api-reference.md");
        return File.ReadAllText(path);
    }

    private static string GetApiReferencePath()
    {
        var root = FindRepoRoot();
        return Path.Combine(root.FullName, "docs", "api-reference.md");
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
