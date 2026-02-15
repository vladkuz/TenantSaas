using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using Xunit;

namespace TenantSaas.ContractTests;

public sealed class ExtensionSeamsEnforcementTests
{
    [Fact]
    public void BoundaryGuard_DoesNotAllowAccessorBypass_WhenCurrentIsNull()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        var accessor = new BypassAttemptAccessor();

        var result = guard.RequireContext(accessor);

        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
    }

    [Fact]
    public void BoundaryGuard_ExplicitContext_TakesPrecedence_AndStillEnforces()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        var accessor = new UninitializedAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-001"));
        var attribution = TenantAttributionInputs.FromExplicitScope(scope);
        var explicitContext = TenantContext.ForRequest(
            scope,
            traceId: "trace-explicit-001",
            requestId: "req-explicit-001",
            attribution);

        var result = guard.RequireContext(accessor, explicitContext);

        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(explicitContext);
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_RejectsAmbiguous()
    {
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        var conflicts = new[]
        {
            new AttributionConflict(TenantAttributionSource.RouteParameter, new TenantId("tenant-route")),
            new AttributionConflict(TenantAttributionSource.HeaderValue, new TenantId("tenant-header"))
        };
        var attributionResult = new TenantAttributionResult.Ambiguous(conflicts);

        var result = guard.RequireUnambiguousAttribution(attributionResult, "trace-ambiguous-001");

        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.ConflictingSources.Should().Contain(new[]
        {
            TenantAttributionSource.RouteParameter.GetDisplayName(),
            TenantAttributionSource.HeaderValue.GetDisplayName()
        });
    }

    private sealed class BypassAttemptAccessor : ITenantContextAccessor
    {
        public TenantContext? Current => null;

        public bool IsInitialized => true;
    }

    private sealed class UninitializedAccessor : ITenantContextAccessor
    {
        public TenantContext? Current => null;

        public bool IsInitialized => false;
    }

    // --- Refusal mapping seam enforcement ---

    [Fact]
    public void RefusalMappingSeam_AllInvariantCodes_HaveStableMappings()
    {
        // Refusal mapping seam is read-only in v1.
        // Verify every registered invariant has a stable refusal mapping that cannot be bypassed.
        foreach (var code in InvariantCode.All)
        {
            var found = TrustContractV1.TryGetRefusalMapping(code, out var mapping);
            found.Should().BeTrue($"Invariant '{code}' must have a refusal mapping in TrustContractV1");
            mapping.Should().NotBeNull();
            mapping!.InvariantCode.Should().Be(code);
            mapping.HttpStatusCode.Should().BeInRange(400, 599,
                $"Refusal mapping for '{code}' must use an error HTTP status");
            mapping.ProblemType.Should().StartWith("urn:tenantsaas:error:",
                $"Refusal mapping for '{code}' must use the canonical URN prefix");
        }
    }

    [Fact]
    public void RefusalMappingSeam_MappingsAreImmutableAcrossLookups()
    {
        // Seam boundary: refusal mappings must be deterministic and stable across lookups.
        foreach (var code in InvariantCode.All)
        {
            var first = TrustContractV1.GetRefusalMapping(code);
            var second = TrustContractV1.GetRefusalMapping(code);

            first.Should().Be(second,
                $"Refusal mapping for '{code}' must be stable across lookups");
        }
    }

    // --- Log enrichment seam enforcement ---

    [Fact]
    public void LogEnrichmentSeam_AlwaysPopulatesRequiredFields()
    {
        // Extension seam: custom ILogEnricher implementations must produce events with
        // required fields. DefaultLogEnricher is the reference; verify it never omits them.
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-enrich-001"));
        var context = TenantContext.ForRequest(scope, "trace-enrich", "req-enrich");

        var logEvent = enricher.Enrich(context, "TestEvent");

        logEvent.TenantRef.Should().NotBeNullOrWhiteSpace("tenant_ref is required");
        logEvent.TraceId.Should().NotBeNullOrWhiteSpace("trace_id is required");
        logEvent.EventName.Should().NotBeNullOrWhiteSpace("event_name is required");
        logEvent.Severity.Should().NotBeNullOrWhiteSpace("severity is required");
        logEvent.ExecutionKind.Should().NotBeNullOrWhiteSpace("execution_kind is required");
        logEvent.ScopeType.Should().NotBeNullOrWhiteSpace("scope_type is required");
    }

    [Fact]
    public void LogEnrichmentSeam_EnforcesDisclosurePolicy_NoRawIds()
    {
        // Extension seam boundary: enricher must use safe-state tokens for non-tenant scopes.
        var enricher = new DefaultLogEnricher();

        // NoTenant scope → "unknown"
        var noTenantContext = TenantContext.ForBackground(
            TenantScope.ForNoTenant(NoTenantReason.HealthCheck), "trace-no-tenant");
        var noTenantEvent = enricher.Enrich(noTenantContext, "TestEvent");
        noTenantEvent.TenantRef.Should().Be("unknown",
            "NoTenant scope must produce safe-state 'unknown'");

        // SharedSystem scope → "cross_tenant"
        var sharedContext = TenantContext.ForAdmin(
            TenantScope.ForSharedSystem(), "trace-shared");
        var sharedEvent = enricher.Enrich(sharedContext, "TestEvent");
        sharedEvent.TenantRef.Should().Be("cross_tenant",
            "SharedSystem scope must produce safe-state 'cross_tenant'");
    }
}
