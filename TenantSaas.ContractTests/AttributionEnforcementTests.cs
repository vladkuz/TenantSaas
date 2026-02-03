using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Logging;
using Xunit;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for attribution enforcement at boundaries.
/// </summary>
public class AttributionEnforcementTests
{
    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }
    [Fact]
    public void RequireUnambiguousAttribution_WithSuccessfulAttribution_ReturnsSuccess()
    {
        // Arrange
        var tenantId = new TenantId("tenant-123");
        var attributionResult = TenantAttributionResult.Succeeded(tenantId, TenantAttributionSource.RouteParameter);
        var traceId = "trace-001";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ResolvedTenantId.Should().Be(tenantId);
        result.ResolvedSource.Should().Be(TenantAttributionSource.RouteParameter);
        result.TraceId.Should().Be(traceId);
        result.InvariantCode.Should().BeNull();
        result.Detail.Should().BeNull();
        result.ConflictingSources.Should().BeNull();
    }

    [Fact]
    public void RequireUnambiguousAttribution_WithAmbiguousResult_ReturnsFailure()
    {
        // Arrange
        var conflicts = new List<AttributionConflict>
        {
            new(TenantAttributionSource.RouteParameter, new TenantId("tenant-123")),
            new(TenantAttributionSource.HeaderValue, new TenantId("tenant-456"))
        };
        var attributionResult = TenantAttributionResult.IsAmbiguous(conflicts);
        var traceId = "trace-002";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.TraceId.Should().Be(traceId);
        result.Detail.Should().Contain("ambiguous");
        result.Detail.Should().Contain("2 sources");
        result.ResolvedTenantId.Should().BeNull();
        result.ConflictingSources.Should().NotBeNull();
        result.ConflictingSources.Should().HaveCount(2);
    }

    [Fact]
    public void RequireUnambiguousAttribution_WithNotFoundResult_ReturnsFailure()
    {
        // Arrange
        var attributionResult = TenantAttributionResult.WasNotFound();
        var traceId = "trace-003";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.TraceId.Should().Be(traceId);
        result.Detail.Should().Contain("could not be resolved");
        result.ResolvedTenantId.Should().BeNull();
    }

    [Fact]
    public void RequireUnambiguousAttribution_WithNotAllowedResult_ReturnsFailure()
    {
        // Arrange
        var attributionResult = TenantAttributionResult.IsNotAllowed(TenantAttributionSource.HostHeader);
        var traceId = "trace-004";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.TraceId.Should().Be(traceId);
        result.Detail.Should().Contain("not allowed");
        result.Detail.Should().Contain("Host Header");
        result.ResolvedTenantId.Should().BeNull();
    }

    [Fact]
    public void RequireUnambiguousAttribution_FailureResult_AlwaysContainsTraceId()
    {
        // Arrange
        var attributionResult = TenantAttributionResult.WasNotFound();
        var traceId = "trace-005";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert
        result.TraceId.Should().Be(traceId);
        result.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RequireUnambiguousAttribution_AmbiguousFailure_DoesNotContainTenantIds()
    {
        // Arrange
        var conflicts = new List<AttributionConflict>
        {
            new(TenantAttributionSource.RouteParameter, new TenantId("tenant-123")),
            new(TenantAttributionSource.HeaderValue, new TenantId("tenant-456"))
        };
        var attributionResult = TenantAttributionResult.IsAmbiguous(conflicts);
        var traceId = "trace-006";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert - Detail must NOT contain actual tenant IDs
        result.Detail.Should().NotContain("tenant-123");
        result.Detail.Should().NotContain("tenant-456");
    }

    [Fact]
    public void RequireUnambiguousAttribution_AmbiguousFailure_ContainsConflictCount()
    {
        // Arrange
        var conflicts = new List<AttributionConflict>
        {
            new(TenantAttributionSource.RouteParameter, new TenantId("tenant-123")),
            new(TenantAttributionSource.HeaderValue, new TenantId("tenant-456")),
            new(TenantAttributionSource.TokenClaim, new TenantId("tenant-789"))
        };
        var attributionResult = TenantAttributionResult.IsAmbiguous(conflicts);
        var traceId = "trace-007";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert - Detail should contain the count of conflicts
        result.Detail.Should().Contain("3 sources");
        result.ConflictingSources.Should().HaveCount(3);
    }

    [Fact]
    public void RequireUnambiguousAttribution_AmbiguousFailure_ConflictingSourcesContainsOnlyNames()
    {
        // Arrange
        var conflicts = new List<AttributionConflict>
        {
            new(TenantAttributionSource.RouteParameter, new TenantId("tenant-123")),
            new(TenantAttributionSource.HeaderValue, new TenantId("tenant-456"))
        };
        var attributionResult = TenantAttributionResult.IsAmbiguous(conflicts);
        var traceId = "trace-008";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(attributionResult, traceId);

        // Assert - ConflictingSources should contain display names, not tenant IDs
        result.ConflictingSources.Should().Contain("Route Parameter");
        result.ConflictingSources.Should().Contain("Header Value");
        result.ConflictingSources.Should().NotContain("tenant-123");
        result.ConflictingSources.Should().NotContain("tenant-456");
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_ReturnsHttp422()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous("trace-009");

        // Assert
        problemDetails.Status.Should().Be(422); // UnprocessableEntity
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_IncludesInvariantCode()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous("trace-010");

        // Assert
        problemDetails.Extensions.Should().ContainKey(InvariantCodeKey);
        problemDetails.Extensions[InvariantCodeKey].Should().Be(Abstractions.Invariants.InvariantCode.TenantAttributionUnambiguous);
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_IncludesTraceId()
    {
        // Arrange
        var traceId = "trace-011";

        // Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous(traceId);

        // Assert
        problemDetails.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId].Should().Be(traceId);
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_HasCorrectType()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous("trace-012");

        // Assert
        problemDetails.Type.Should().Be("urn:tenantsaas:error:tenant-attribution-unambiguous");
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_IncludesGuidanceLink()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous("trace-013");

        // Assert
        problemDetails.Extensions.Should().ContainKey(GuidanceLink);
        problemDetails.Extensions[GuidanceLink].Should().Be("https://docs.tenantsaas.dev/errors/attribution-ambiguous");
    }

    [Fact]
    public void ProblemDetailsFactory_TenantAttributionUnambiguous_IncludesConflictingSources()
    {
        // Arrange
        var conflictingSources = new List<string> { "Route Parameter", "Header Value" };
        var extensions = new Dictionary<string, object?>
        {
            [ConflictingSources] = conflictingSources
        };

        // Act
        var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous(
            "trace-014",
            conflictingSources: conflictingSources);

        // Assert
        problemDetails.Extensions.Should().ContainKey(ConflictingSources);
        var sources = problemDetails.Extensions[ConflictingSources] as IReadOnlyList<string>;
        sources.Should().NotBeNull();
        sources.Should().HaveCount(2);
        sources.Should().Contain("Route Parameter");
        sources.Should().Contain("Header Value");
    }

    [Fact]
    public void TenantAttributionSource_GetDisplayName_ReturnsExpectedNames()
    {
        // Arrange & Act & Assert - Verify display names match expected values used in error messages
        TenantAttributionSource.RouteParameter.GetDisplayName().Should().Be("Route Parameter");
        TenantAttributionSource.HeaderValue.GetDisplayName().Should().Be("Header Value");
        TenantAttributionSource.HostHeader.GetDisplayName().Should().Be("Host Header");
        TenantAttributionSource.TokenClaim.GetDisplayName().Should().Be("Token Claim");
        TenantAttributionSource.ExplicitContext.GetDisplayName().Should().Be("Explicit Context");
    }

    [Fact]
    public void TenantAttributionSource_GetDisplayName_NeverReturnsNull()
    {
        // Arrange - All attribution sources
        var allSources = new[]
        {
            TenantAttributionSource.RouteParameter,
            TenantAttributionSource.HeaderValue,
            TenantAttributionSource.HostHeader,
            TenantAttributionSource.TokenClaim,
            TenantAttributionSource.ExplicitContext
        };

        // Act & Assert - Display names must never be null or empty (disclosure safety requirement)
        foreach (var source in allSources)
        {
            source.GetDisplayName().Should().NotBeNullOrWhiteSpace(
                because: "display names are used in error messages and must be disclosure-safe");
        }
    }

    [Fact]
    public void TenantAttributionSource_GetDisplayName_NeverContainsTenantIdentifiers()
    {
        // Arrange - All attribution sources
        var allSources = new[]
        {
            TenantAttributionSource.RouteParameter,
            TenantAttributionSource.HeaderValue,
            TenantAttributionSource.HostHeader,
            TenantAttributionSource.TokenClaim,
            TenantAttributionSource.ExplicitContext
        };

        // Act & Assert - Display names must describe source types, never contain tenant IDs
        foreach (var source in allSources)
        {
            var displayName = source.GetDisplayName();
            
            // Display names should be human-readable source type descriptions
            displayName.Should().NotMatchRegex(@"tenant-\d+", 
                because: "display names must not leak tenant identifiers");
            displayName.Should().NotMatchRegex(@"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
                because: "display names must not contain GUIDs or tenant IDs");
        }
    }
}
