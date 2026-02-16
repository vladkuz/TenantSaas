using FluentAssertions;
using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.ContractTests.TestUtilities;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Logging;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

public sealed class CrossTenantAdministrativeWorkflowTests
{
    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_MissingBreakGlassDeclaration_RefusesWithInvariant()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _, out _);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-ct-admin-001");

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            TrustContractV1.SharedSystemOperationCrossTenantAdminRead,
            declaration: null,
            traceId: "trace-ct-admin-001",
            requestId: "req-ct-admin-001");

        var problemDetails = ProblemDetailsFactory.ForBreakGlassRequired(
            "trace-ct-admin-001",
            "req-ct-admin-001",
            result.Detail);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        result.Detail.Should().Be("Break-glass declaration is required.");
        problemDetails.Extensions[InvariantCodeKey].Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        problemDetails.Extensions[TraceId].Should().Be("trace-ct-admin-001");
        problemDetails.Extensions[RequestId].Should().Be("req-ct-admin-001");
    }

    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_EmptyOperationName_RefusesWithStableInvariant()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _, out _);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-ct-admin-002");

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Emergency remediation",
            declaredScope: "cross-tenant-admin-read",
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            operationName: string.Empty,
            declaration,
            traceId: "trace-ct-admin-002");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.SharedSystemOperationAllowed);
        result.Detail.Should().Contain("operation scope");
    }

    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_NonSharedSystemScope_RefusesWithTenantScopeRequired()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _, out _);
        var tenantId = new TenantId("tenant-123");
        var scope = TenantScope.ForTenant(tenantId);
        var context = TenantContext.ForAdmin(scope, "trace-ct-admin-003");

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Emergency remediation",
            declaredScope: "cross-tenant-admin-read",
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            TrustContractV1.SharedSystemOperationCrossTenantAdminRead,
            declaration,
            traceId: "trace-ct-admin-003");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantScopeRequired);
        result.Detail.Should().Contain("shared-system scope");
    }

    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_InvalidOperationName_RefusesWithSharedSystemOperationAllowed()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _, out _);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-ct-admin-004");

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Emergency remediation",
            declaredScope: "invalid-operation",
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            operationName: "invalid-operation-not-allowlisted",
            declaration,
            traceId: "trace-ct-admin-004");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.SharedSystemOperationAllowed);
        result.Detail.Should().Contain("not allowlisted");
    }

    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_AuthorizedPath_EmitsCrossTenantAuditAndDisclosureSafeLog()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out var logs, out var auditSink);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-ct-admin-005");

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Incident #787",
            declaredScope: "cross-tenant-admin-write",
            targetTenantRef: "tenant-sensitive-internal-id",
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            TrustContractV1.SharedSystemOperationCrossTenantAdminWrite,
            declaration,
            traceId: "trace-ct-admin-005");

        // Assert
        result.IsSuccess.Should().BeTrue();
        auditSink.Event.Should().NotBeNull();
        auditSink.Event!.TenantRef.Should().Be(TrustContractV1.BreakGlassMarkerCrossTenant);
        auditSink.Event.Scope.Should().Be("cross-tenant-admin-write");
        auditSink.Event.OperationName.Should().Be(TrustContractV1.SharedSystemOperationCrossTenantAdminWrite);

        var logEntry = logs.ToList().Single(e => e.EventId.Id == 1007);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("tenant_ref=cross_tenant");
        logEntry.Message.Should().NotContain("tenant-sensitive-internal-id");
    }

    [Fact]
    public async Task RequireCrossTenantAdministrativeWorkflow_TargetTenantRefDoesNotLeakInAudit_EvenWhenPresent()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _, out var auditSink);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-ct-admin-006");

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Cross-tenant investigation",
            declaredScope: "cross-tenant-admin-read",
            targetTenantRef: "very-sensitive-tenant-id-12345",
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await guard.RequireCrossTenantAdministrativeWorkflowAsync(
            context,
            TrustContractV1.SharedSystemOperationCrossTenantAdminRead,
            declaration,
            traceId: "trace-ct-admin-006");

        // Assert - verify targetTenantRef is overridden to cross_tenant marker
        result.IsSuccess.Should().BeTrue();
        auditSink.Event.Should().NotBeNull();
        auditSink.Event!.TenantRef.Should().Be(TrustContractV1.BreakGlassMarkerCrossTenant);
        auditSink.Event.TenantRef.Should().NotBe("very-sensitive-tenant-id-12345");
    }

    private static BoundaryGuard CreateBoundaryGuard(
        out CapturedLogCollection capturedLogs,
        out CaptureAuditSink auditSink)
    {
        capturedLogs = new CapturedLogCollection();
        var loggerFactory = new TestLoggerFactory(capturedLogs);
        var logger = loggerFactory.CreateLogger<BoundaryGuard>();
        var enricher = new DefaultLogEnricher();
        auditSink = new CaptureAuditSink();

        return new BoundaryGuard(logger, enricher, auditSink);
    }
}
