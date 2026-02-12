using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.ContractTestKit;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Logging;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Reference compliance tests for break-glass refusal and audit emission (Story 5.4).
/// </summary>
public sealed class ReferenceComplianceBreakGlassTests(TrustContractFixture fixture) : IClassFixture<TrustContractFixture>
{
    private static BoundaryGuard CreateBoundaryGuard(IBreakGlassAuditSink? auditSink = null)
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher, auditSink);
    }

    [Fact]
    public async Task MissingBreakGlassDeclaration_IsRefused_WithInvariantAndProblemDetails()
    {
        // Arrange
        fixture.ValidateRefusalMappings();
        var boundaryGuard = CreateBoundaryGuard();
        const string traceId = "ref-bg-denied-001";
        const string requestId = "ref-bg-denied-req-001";

        // Act
        var enforcement = await boundaryGuard.RequireBreakGlassAsync(
            declaration: null,
            traceId: traceId,
            requestId: requestId);
        var refusal = ProblemDetailsFactory.ForBreakGlassRequired(
            traceId,
            requestId,
            enforcement.Detail);
        var mapping = TrustContractV1.RefusalMappings[InvariantCode.BreakGlassExplicitAndAudited];

        // Assert
        enforcement.IsSuccess.Should().BeFalse();
        enforcement.InvariantCode.Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        enforcement.Detail.Should().Be("Break-glass declaration is required.");
        refusal.Type.Should().Be(mapping.ProblemType);
        refusal.Title.Should().Be(mapping.Title);
        refusal.Status.Should().Be(mapping.HttpStatusCode);
        refusal.Detail.Should().Be(enforcement.Detail);
        refusal.Instance.Should().BeNull();
        refusal.Extensions[InvariantCodeKey].Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        refusal.Extensions[TraceId].Should().Be(traceId);
        refusal.Extensions[RequestId].Should().Be(requestId);
        refusal.Extensions[GuidanceLink].Should().Be(mapping.GuidanceUri);
    }

    [Fact]
    public async Task ValidBreakGlassDeclaration_EmitsAuditEvent_WithRequiredFields()
    {
        // Arrange
        var auditSink = new CaptureAuditSink();
        var boundaryGuard = CreateBoundaryGuard(auditSink);
        const string traceId = "ref-bg-allowed-001";

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Production incident #321",
            declaredScope: "Cross-tenant data validation",
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: declaration,
            traceId: traceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        auditSink.EmitCount.Should().Be(1);
        auditSink.Event.Should().NotBeNull();
        auditSink.Event!.Actor.Should().Be(declaration.ActorId);
        auditSink.Event.Reason.Should().Be(declaration.Reason);
        auditSink.Event.Scope.Should().Be(declaration.DeclaredScope);
        auditSink.Event.TenantRef.Should().Be(TrustContractV1.BreakGlassMarkerCrossTenant);
        auditSink.Event.TraceId.Should().Be(traceId);
        auditSink.Event.AuditCode.Should().Be(AuditCode.BreakGlassInvoked);
        auditSink.Event.InvariantCode.Should().BeNull();
        auditSink.Event.Timestamp.Offset.Should().Be(TimeSpan.Zero);
    }

    private sealed class CaptureAuditSink : IBreakGlassAuditSink
    {
        public BreakGlassAuditEvent? Event { get; private set; }

        public int EmitCount { get; private set; }

        public Task EmitAsync(BreakGlassAuditEvent auditEvent, CancellationToken cancellationToken)
        {
            EmitCount++;
            Event = auditEvent;
            return Task.CompletedTask;
        }
    }
}
