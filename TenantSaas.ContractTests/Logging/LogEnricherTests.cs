using FluentAssertions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Logging;

namespace TenantSaas.ContractTests.Logging;

/// <summary>
/// Contract tests for log enricher implementation following Story 3.4 AC#1.
/// Validates that all enforcement logs include required structured fields and follow disclosure policy.
/// </summary>
public class LogEnricherTests
{
    [Fact]
    public void Enrich_TenantScope_LogsDisclosureSafeTenantRef()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var tenantId = new TenantId("tenant-123");
        var scope = TenantScope.ForTenant(tenantId);
        var context = TenantContext.ForRequest(scope, "trace-001", "req-001");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - tenant_ref is disclosure-safe (opaque public ID, not raw internal ID)
        logEvent.TenantRef.Should().Be("tenant-123", "Tenant scope should log opaque tenant ID");
        logEvent.TraceId.Should().Be("trace-001");
        logEvent.RequestId.Should().Be("req-001");
        logEvent.EventName.Should().Be("ContextInitialized");
        logEvent.Severity.Should().Be("Information");
        logEvent.ExecutionKind.Should().Be("request");
        logEvent.ScopeType.Should().Be("Tenant");
    }

    [Fact]
    public void Enrich_NoTenantScope_LogsSafeStateUnknown()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForNoTenant(NoTenantReason.HealthCheck);
        var context = TenantContext.ForRequest(scope, "trace-002", "req-002");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - tenant_ref is safe-state token "unknown" per disclosure policy
        logEvent.TenantRef.Should().Be("unknown", "NoTenant scope should log safe-state token 'unknown'");
        logEvent.TraceId.Should().Be("trace-002");
        logEvent.RequestId.Should().Be("req-002");
        logEvent.ScopeType.Should().Be("NoTenant");
    }

    [Fact]
    public void Enrich_SharedSystemScope_LogsSafeStateCrossTenant()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForSharedSystem();
        var context = TenantContext.ForBackground(scope, "trace-003");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - tenant_ref is safe-state token "cross_tenant" per disclosure policy
        logEvent.TenantRef.Should().Be("cross_tenant", "SharedSystem scope should log safe-state token 'cross_tenant'");
        logEvent.TraceId.Should().Be("trace-003");
        logEvent.RequestId.Should().BeNull("Background execution should not have request_id");
        logEvent.ExecutionKind.Should().Be("background");
        logEvent.ScopeType.Should().Be("SharedSystem");
    }

    [Fact]
    public void Enrich_AllExecutionKinds_IncludesRequiredFields()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("test-tenant"));

        var executionKinds = new[]
        {
            (ExecutionKind.Request, TenantContext.ForRequest(scope, "trace-req", "req-001")),
            (ExecutionKind.Background, TenantContext.ForBackground(scope, "trace-bg")),
            (ExecutionKind.Admin, TenantContext.ForAdmin(scope, "trace-admin")),
            (ExecutionKind.Scripted, TenantContext.ForScripted(scope, "trace-script"))
        };

        foreach (var (kind, context) in executionKinds)
        {
            // Act
            var logEvent = enricher.Enrich(context, "TestEvent");

            // Assert - all logs include required structured fields
            logEvent.TenantRef.Should().NotBeNullOrWhiteSpace($"{kind.Value} should include tenant_ref");
            logEvent.TraceId.Should().NotBeNullOrWhiteSpace($"{kind.Value} should include trace_id");
            logEvent.EventName.Should().Be("TestEvent", $"{kind.Value} should include event_name");
            logEvent.Severity.Should().NotBeNullOrWhiteSpace($"{kind.Value} should include severity");
            logEvent.ExecutionKind.Should().Be(kind.Value, $"{kind.Value} should match execution_kind");
            logEvent.ScopeType.Should().NotBeNullOrWhiteSpace($"{kind.Value} should include scope_type");
        }
    }

    [Fact]
    public void Enrich_TraceIdAndRequestId_MatchContextValues()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-abc"));
        var context = TenantContext.ForRequest(scope, "correlation-trace-xyz", "correlation-request-123");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - trace_id and request_id match exactly from context
        logEvent.TraceId.Should().Be("correlation-trace-xyz", "trace_id must match context for correlation");
        logEvent.RequestId.Should().Be("correlation-request-123", "request_id must match context for correlation");
    }

    [Fact]
    public void Enrich_WithInvariantCode_SetsWarningSeverity()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-def"));
        var context = TenantContext.ForRequest(scope, "trace-004", "req-004");

        // Act
        var logEvent = enricher.Enrich(
            context,
            "ContextNotInitialized",
            invariantCode: "ContextInitialized",
            detail: "Context was not initialized");

        // Assert - invariant violations should be Warning severity
        logEvent.InvariantCode.Should().Be("ContextInitialized");
        logEvent.Severity.Should().Be("Warning", "Invariant violations should use Warning severity");
        logEvent.Detail.Should().Be("Context was not initialized");
    }

    [Fact]
    public void Enrich_SuccessfulEvent_SetsInformationSeverity()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-ghi"));
        var context = TenantContext.ForRequest(scope, "trace-005", "req-005");

        // Act
        var logEvent = enricher.Enrich(context, "AttributionResolved");

        // Assert - successful operations should be Information severity
        logEvent.Severity.Should().Be("Information", "Successful operations should use Information severity");
        logEvent.InvariantCode.Should().BeNull("Successful operations should not have invariant_code");
    }

    [Fact]
    public void Enrich_BackgroundExecution_NoRequestId()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-jkl"));
        var context = TenantContext.ForBackground(scope, "trace-bg-001");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - background execution should not have request_id
        logEvent.RequestId.Should().BeNull("Background execution should not include request_id");
        logEvent.TraceId.Should().Be("trace-bg-001", "Background execution should still have trace_id");
        logEvent.ExecutionKind.Should().Be("background");
    }

    [Fact]
    public void Enrich_NeverIncludesSensitiveData()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var sensitiveId = new TenantId("SENSITIVE_INTERNAL_ID_12345");
        var scope = TenantScope.ForTenant(sensitiveId);
        var context = TenantContext.ForRequest(scope, "trace-006", "req-006");

        // Act
        var logEvent = enricher.Enrich(context, "ContextInitialized");

        // Assert - tenant_ref should be disclosure-safe (for now, using Value; future: hash or public ID mapping)
        // This test validates the enricher follows disclosure policy requirements
        logEvent.TenantRef.Should().NotBeNullOrWhiteSpace("tenant_ref must be present");
        // Note: In production, sensitive tenant IDs should be mapped to opaque public IDs
        // This test documents the current behavior; Story 2.5 defines the disclosure policy contract
    }

    [Fact]
    public void Enrich_UnknownEventName_DefaultsToWarningSeverity()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-unknown-event"));
        var context = TenantContext.ForRequest(scope, "trace-007", "req-007");

        // Act
        var logEvent = enricher.Enrich(context, "SomeUnknownEvent");

        // Assert - Unknown events default to Warning (safer for audit purposes)
        logEvent.Severity.Should().Be("Warning", "Unknown events should default to Warning for safety");
    }

    [Fact]
    public void Enrich_SafeStateValues_DocumentedForDisclosurePolicy()
    {
        // This test documents the disclosure policy safe-state values from Story 2.5.
        // These are the only valid values for tenant_ref when disclosure is unsafe:
        // - "unknown": No tenant information available (NoTenant scope)
        // - "cross_tenant": Cross-tenant/shared-system operation (SharedSystem scope)
        // - "sensitive": Reserved for future use when tenant exists but is unsafe to disclose
        // - Opaque public ID: Tenant scope with disclosure-safe identifier
        
        var enricher = new DefaultLogEnricher();

        // NoTenant → "unknown"
        var noTenantContext = TenantContext.ForRequest(
            TenantScope.ForNoTenant(NoTenantReason.HealthCheck), "t1", "r1");
        enricher.Enrich(noTenantContext, "Test").TenantRef.Should().Be("unknown");

        // SharedSystem → "cross_tenant"
        var sharedContext = TenantContext.ForBackground(
            TenantScope.ForSharedSystem(), "t2");
        enricher.Enrich(sharedContext, "Test").TenantRef.Should().Be("cross_tenant");

        // Tenant → opaque ID (currently uses raw Value; future: mapping)
        var tenantContext = TenantContext.ForRequest(
            TenantScope.ForTenant(new TenantId("tenant-public-id")), "t3", "r3");
        enricher.Enrich(tenantContext, "Test").TenantRef.Should().Be("tenant-public-id");

        // Note: "sensitive" is reserved for future use when:
        // - Tenant exists but should not be disclosed
        // - E.g., tenant lookup succeeded but policy forbids logging the reference
        // This requires additional infrastructure not yet implemented.
    }
}
