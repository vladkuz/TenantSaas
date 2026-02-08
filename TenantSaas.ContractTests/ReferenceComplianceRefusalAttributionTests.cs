using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.ContractTestKit;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Reference compliance tests for refusal and attribution rules (Story 5.2).
/// </summary>
public sealed class ReferenceComplianceRefusalAttributionTests : IClassFixture<TrustContractFixture>
{
    private readonly TrustContractFixture fixture;

    public ReferenceComplianceRefusalAttributionTests(TrustContractFixture fixture)
    {
        this.fixture = fixture;
    }

    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }

    [Fact]
    public void MissingContext_OperationIsRefusedByDefault_WithContextInitializedInvariant()
    {
        // Arrange
        var boundaryGuard = CreateBoundaryGuard();
        var accessor = new AmbientTenantContextAccessor();
        const string traceId = "ref-ctx-missing-001";
        const string requestId = "ref-ctx-missing-req-001";

        // Act
        var enforcement = boundaryGuard.RequireContext(accessor, traceId);
        enforcement.TraceId.Should().NotBeNullOrWhiteSpace();
        var refusal = ProblemDetailsFactory.FromInvariantViolation(
            enforcement.InvariantCode!,
            enforcement.TraceId!,
            requestId,
            enforcement.Detail);

        // Assert
        enforcement.IsSuccess.Should().BeFalse();
        enforcement.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        refusal.Status.Should().Be(401);
        refusal.Extensions[InvariantCodeKey].Should().Be(InvariantCode.ContextInitialized);
        refusal.Extensions[TraceId].Should().Be(traceId);
    }

    [Fact]
    public void DisallowedAttributionSource_OperationIsRefusedByDefault_WithTrustContractInvariant()
    {
        // Arrange
        var resolver = new TenantAttributionResolver();
        var boundaryGuard = CreateBoundaryGuard();
        const string traceId = "ref-attrib-disallowed-001";

        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.HeaderValue] = new("tenant-123")
        };

        // Act
        var attribution = resolver.Resolve(
            sources,
            TenantAttributionRules.Default(),
            ExecutionKind.Request);

        var enforcement = boundaryGuard.RequireUnambiguousAttribution(attribution, traceId);
        var refusal = ProblemDetailsFactory.ForTenantAttributionAmbiguous(traceId);

        // Assert
        enforcement.IsSuccess.Should().BeFalse();
        enforcement.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        enforcement.Detail.Should().Contain("not allowed");
        refusal.Status.Should().Be(422);
        refusal.Extensions[InvariantCodeKey].Should().Be(InvariantCode.TenantAttributionUnambiguous);
        refusal.Extensions[TraceId].Should().Be(traceId);
    }

    [Fact]
    public void AttributionSourcesDisagree_TestFailsWithErrorRefusal()
    {
        // Arrange
        fixture.ValidateRefusalMappings();

        var resolver = new TenantAttributionResolver();
        var boundaryGuard = CreateBoundaryGuard();
        const string traceId = "ref-attrib-ambiguous-001";
        const string requestId = "ref-attrib-ambiguous-req-001";
        var sources = new Dictionary<TenantAttributionSource, TenantId>
        {
            [TenantAttributionSource.RouteParameter] = new("tenant-a"),
            [TenantAttributionSource.TokenClaim] = new("tenant-b")
        };

        // Act
        var attribution = resolver.Resolve(
            sources,
            TenantAttributionRules.ForWebApi(),
            ExecutionKind.Request);

        var enforcement = boundaryGuard.RequireUnambiguousAttribution(attribution, traceId);
        var refusal = ProblemDetailsFactory.ForTenantAttributionAmbiguous(
            traceId,
            requestId,
            enforcement.ConflictingSources);

        // Assert
        enforcement.IsSuccess.Should().BeFalse("conflicting sources must refuse the operation");
        enforcement.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        enforcement.Detail.Should().Contain("ambiguous");
        enforcement.ConflictingSources.Should().HaveCount(2);

        refusal.Status.Should().Be(422);
        refusal.Extensions[InvariantCodeKey].Should().Be(InvariantCode.TenantAttributionUnambiguous);
        refusal.Extensions[TraceId].Should().Be(traceId);
        refusal.Extensions.Should().ContainKey(ConflictingSources);
    }
}
