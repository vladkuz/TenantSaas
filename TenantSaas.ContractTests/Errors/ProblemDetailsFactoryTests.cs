using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Core.Errors;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests.Errors;

/// <summary>
/// Contract tests for ProblemDetailsFactory.
/// Validates RFC 7807 compliance and stable invariant refusal contracts.
/// </summary>
public class ProblemDetailsFactoryTests
{
    [Fact]
    public void FromInvariantViolation_ContextInitialized_Returns401WithCorrectShape()
    {
        // Arrange
        const string traceId = "test-trace-123";
        const string requestId = "test-request-456";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("urn:tenantsaas:error:context-initialized");
        result.Title.Should().Be("Tenant context not initialized");
        result.Status.Should().Be(401);
        result.Detail.Should().NotBeNullOrWhiteSpace();
        result.Instance.Should().BeNull();

        // Extension fields
        result.Extensions.Should().ContainKey(InvariantCodeKey);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.ContextInitialized);

        result.Extensions.Should().ContainKey(TraceId);
        result.Extensions[TraceId].Should().Be(traceId);

        result.Extensions.Should().ContainKey(RequestId);
        result.Extensions[RequestId].Should().Be(requestId);

        result.Extensions.Should().ContainKey(GuidanceLink);
        result.Extensions[GuidanceLink].Should().Be("https://docs.tenantsaas.dev/errors/context-not-initialized");
    }

    [Fact]
    public void FromInvariantViolation_TenantAttributionUnambiguous_Returns422WithCorrectShape()
    {
        // Arrange
        const string traceId = "test-trace-789";
        const string requestId = "test-request-012";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.TenantAttributionUnambiguous,
            traceId,
            requestId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("urn:tenantsaas:error:tenant-attribution-unambiguous");
        result.Title.Should().Be("Tenant attribution is ambiguous");
        result.Status.Should().Be(422);
        result.Detail.Should().NotBeNullOrWhiteSpace();
        result.Instance.Should().BeNull();

        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.Extensions[TraceId].Should().Be(traceId);
        result.Extensions[RequestId].Should().Be(requestId);
        result.Extensions[GuidanceLink].Should().Be("https://docs.tenantsaas.dev/errors/attribution-ambiguous");
    }

    [Fact]
    public void FromInvariantViolation_UnknownInvariantCode_ThrowsKeyNotFoundException()
    {
        // Arrange
        const string traceId = "test-trace-999";
        const string invalidCode = "NonExistentInvariant";

        // Act & Assert
        var act = () => ProblemDetailsFactory.FromInvariantViolation(
            invalidCode,
            traceId);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void FromInvariantViolation_WithRequestId_IncludesRequestIdInExtensions()
    {
        // Arrange
        const string traceId = "test-trace-abc";
        const string requestId = "test-request-def";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId);

        // Assert
        result.Extensions.Should().ContainKey(RequestId);
        result.Extensions[RequestId].Should().Be(requestId);
    }

    [Fact]
    public void FromInvariantViolation_WithoutRequestId_DoesNotIncludeRequestIdInExtensions()
    {
        // Arrange
        const string traceId = "test-trace-xyz";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId: null);

        // Assert
        result.Extensions.Should().NotContainKey(RequestId);
    }

    [Fact]
    public void FromInvariantViolation_WithCustomDetail_UsesCustomDetail()
    {
        // Arrange
        const string traceId = "test-trace-custom";
        const string customDetail = "Custom error detail message";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            detail: customDetail);

        // Assert
        result.Detail.Should().Be(customDetail);
    }

    [Fact]
    public void FromInvariantViolation_WithoutCustomDetail_UsesRefusalMappingDescription()
    {
        // Arrange
        const string traceId = "test-trace-default";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId);

        // Assert
        result.Detail.Should().Be("Tenant context must be initialized before operations can proceed.");
    }

    [Fact]
    public void FromInvariantViolation_WithAdditionalExtensions_MergesExtensions()
    {
        // Arrange
        const string traceId = "test-trace-merge";
        var additionalExtensions = new Dictionary<string, object?>
        {
            ["custom_field"] = "custom_value",
            ["another_field"] = 42
        };

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            extensions: additionalExtensions);

        // Assert
        result.Extensions.Should().ContainKey("custom_field");
        result.Extensions["custom_field"].Should().Be("custom_value");
        result.Extensions.Should().ContainKey("another_field");
        result.Extensions["another_field"].Should().Be(42);

        // Standard fields still present
        result.Extensions.Should().ContainKey(InvariantCodeKey);
        result.Extensions.Should().ContainKey(TraceId);
    }

    [Fact]
    public void FromInvariantViolation_TenantRefOverload_IncludesTenantRefInExtensions()
    {
        // Arrange
        const string traceId = "test-trace-tenant";
        const string requestId = "test-request-tenant";
        const string tenantRef = "unknown";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId,
            detail: null,
            tenantRef: tenantRef);

        // Assert
        result.Extensions.Should().ContainKey("tenant_ref");
        result.Extensions["tenant_ref"].Should().Be(tenantRef);
    }

    [Fact]
    public void FromInvariantViolation_TenantRefOverloadWithNullTenantRef_DoesNotIncludeTenantRef()
    {
        // Arrange
        const string traceId = "test-trace-no-tenant";
        const string requestId = "test-request-no-tenant";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId,
            detail: null,
            tenantRef: null);

        // Assert
        result.Extensions.Should().NotContainKey("tenant_ref");
    }

    [Fact]
    public void ForContextNotInitialized_ReturnsCorrectProblemDetails()
    {
        // Arrange
        const string traceId = "test-trace-convenience";
        const string requestId = "test-request-convenience";

        // Act
        var result = ProblemDetailsFactory.ForContextNotInitialized(traceId, requestId);

        // Assert
        result.Status.Should().Be(401);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.ContextInitialized);
        result.Extensions[TraceId].Should().Be(traceId);
        result.Extensions[RequestId].Should().Be(requestId);
    }

    [Fact]
    public void ForTenantAttributionAmbiguous_WithoutConflictingSources_ReturnsCorrectProblemDetails()
    {
        // Arrange
        const string traceId = "test-trace-amb";
        const string requestId = "test-request-amb";

        // Act
        var result = ProblemDetailsFactory.ForTenantAttributionAmbiguous(traceId, requestId);

        // Assert
        result.Status.Should().Be(422);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.TenantAttributionUnambiguous);
        result.Extensions[TraceId].Should().Be(traceId);
    }

    [Fact]
    public void ForTenantAttributionAmbiguous_WithConflictingSources_IncludesSourcesInExtensions()
    {
        // Arrange
        const string traceId = "test-trace-sources";
        const string requestId = "test-request-sources";
        var conflictingSources = new List<string> { "route-parameter", "header-value" };

        // Act
        var result = ProblemDetailsFactory.ForTenantAttributionAmbiguous(
            traceId,
            requestId,
            conflictingSources);

        // Assert
        result.Extensions.Should().ContainKey(ConflictingSources);
        result.Extensions[ConflictingSources].Should().BeEquivalentTo(conflictingSources);
    }

    [Fact]
    public void ForTenantScopeRequired_Returns403()
    {
        // Arrange
        const string traceId = "test-trace-scope";

        // Act
        var result = ProblemDetailsFactory.ForTenantScopeRequired(traceId);

        // Assert
        result.Status.Should().Be(403);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.TenantScopeRequired);
    }

    [Fact]
    public void ForSharedSystemOperationNotAllowed_Returns403WithStableExtensions()
    {
        // Arrange
        const string traceId = "test-trace-shared-op";
        const string requestId = "test-request-shared-op";

        // Act
        var result = ProblemDetailsFactory.ForSharedSystemOperationNotAllowed(
            traceId,
            requestId,
            "cross-tenant-maintenance",
            InvariantCode.DisclosureSafe);

        // Assert
        result.Status.Should().Be(403);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.SharedSystemOperationAllowed);
        result.Extensions[TraceId].Should().Be(traceId);
        result.Extensions[RequestId].Should().Be(requestId);
        result.Extensions["operation_name"].Should().Be("cross-tenant-maintenance");
        result.Extensions["attempted_invariant"].Should().Be(InvariantCode.DisclosureSafe);
    }

    [Fact]
    public void ForBreakGlassRequired_Returns403()
    {
        // Arrange
        const string traceId = "test-trace-break";

        // Act
        var result = ProblemDetailsFactory.ForBreakGlassRequired(traceId);

        // Assert
        result.Status.Should().Be(403);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
    }

    [Fact]
    public void ForDisclosureUnsafe_Returns500()
    {
        // Arrange
        const string traceId = "test-trace-disclosure";

        // Act
        var result = ProblemDetailsFactory.ForDisclosureUnsafe(traceId);

        // Assert
        result.Status.Should().Be(500);
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.DisclosureSafe);
    }

    [Fact]
    public void ForDisclosureUnsafe_DoesNotIncludeTenantRef()
    {
        // Arrange
        const string traceId = "test-trace-disclosure-tenant";
        const string requestId = "test-request-disclosure-tenant";

        // Act
        var result = ProblemDetailsFactory.ForDisclosureUnsafe(traceId, requestId);

        // Assert
        result.Extensions.Should().NotContainKey("tenant_ref");
        result.Extensions[InvariantCodeKey].Should().Be(InvariantCode.DisclosureSafe);
        result.Extensions[TraceId].Should().Be(traceId);
        result.Extensions[RequestId].Should().Be(requestId);
    }

    [Fact]
    public void FromInvariantViolation_WithEmptyTraceId_ThrowsArgumentException()
    {
        // Arrange
        const string invariantCode = InvariantCode.ContextInitialized;
        const string emptyTraceId = "";

        // Act & Assert
        var act = () => ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            emptyTraceId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("traceId");
    }

    [Fact]
    public void FromInvariantViolation_WithWhitespaceTraceId_ThrowsArgumentException()
    {
        // Arrange
        const string invariantCode = InvariantCode.ContextInitialized;
        const string whitespaceTraceId = "   ";

        // Act & Assert
        var act = () => ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            whitespaceTraceId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("traceId");
    }

    [Fact]
    public void FromInvariantViolation_WithEmptyInvariantCode_ThrowsArgumentException()
    {
        // Arrange
        const string emptyCode = "";
        const string traceId = "test-trace";

        // Act & Assert
        var act = () => ProblemDetailsFactory.FromInvariantViolation(
            emptyCode,
            traceId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("invariantCode");
    }
}
