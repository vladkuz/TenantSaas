using FluentAssertions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.Core.Errors;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests.Errors;

/// <summary>
/// Contract tests validating RFC 7807 Problem Details shape compliance.
/// Ensures all Problem Details responses follow consistent structure and stable contracts.
/// </summary>
public class ProblemDetailsShapeTests
{
    [Theory]
    [MemberData(nameof(AllInvariantCodes))]
    public void ProblemDetails_ForAllInvariants_HasRequiredRfc7807Fields(string invariantCode)
    {
        // Arrange
        const string traceId = "test-trace-shape";
        const string requestId = "test-request-shape";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            traceId,
            requestId);

        // Assert - RFC 7807 required fields
        result.Type.Should().NotBeNullOrWhiteSpace("Type is required by RFC 7807");
        result.Title.Should().NotBeNullOrWhiteSpace("Title is required by RFC 7807");
        result.Status.Should().BeGreaterOrEqualTo(400).And.BeLessThan(600, "Status must be 4xx or 5xx");
        result.Detail.Should().NotBeNullOrWhiteSpace("Detail should provide human-readable explanation");
        result.Instance.Should().BeNull("Instance is not used per architecture decision");
    }

    [Theory]
    [MemberData(nameof(AllInvariantCodes))]
    public void ProblemDetails_ForAllInvariants_HasRequiredExtensionFields(string invariantCode)
    {
        // Arrange
        const string traceId = "test-trace-extensions";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            traceId);

        // Assert - Required extensions
        result.Extensions.Should().ContainKey(InvariantCodeKey, "invariant_code is always required");
        result.Extensions[InvariantCodeKey].Should().Be(invariantCode);

        result.Extensions.Should().ContainKey(TraceId, "trace_id is always required");
        result.Extensions[TraceId].Should().Be(traceId);

        result.Extensions.Should().ContainKey(GuidanceLink, "guidance_link is always required");
        result.Extensions[GuidanceLink]?.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [MemberData(nameof(AllInvariantCodes))]
    public void ProblemDetails_TypeUri_FollowsStablePattern(string invariantCode)
    {
        // Arrange
        const string traceId = "test-trace-type";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            traceId);

        // Assert - Type URI pattern
        result.Type.Should().StartWith("urn:tenantsaas:error:", "Type must follow URN pattern");
        result.Type.Should().MatchRegex(@"^urn:tenantsaas:error:[a-z][a-z0-9-]*$",
            "Type must use lowercase kebab-case after prefix");
    }

    [Theory]
    [MemberData(nameof(AllInvariantCodes))]
    public void ProblemDetails_TypeUri_IsStableAcrossInvocations(string invariantCode)
    {
        // Arrange
        const string traceId1 = "test-trace-stable-1";
        const string traceId2 = "test-trace-stable-2";

        // Act - Create same invariant twice with different trace IDs
        var result1 = ProblemDetailsFactory.FromInvariantViolation(invariantCode, traceId1);
        var result2 = ProblemDetailsFactory.FromInvariantViolation(invariantCode, traceId2);

        // Assert - Type URI must be identical
        result1.Type.Should().Be(result2.Type,
            "Type URIs must be stable across invocations within major version");
    }

    [Theory]
    [MemberData(nameof(AllInvariantCodes))]
    public void ProblemDetails_HttpStatus_MatchesRefusalMapping(string invariantCode)
    {
        // Arrange
        const string traceId = "test-trace-status";
        var expectedMapping = TrustContractV1.RefusalMappings[invariantCode];

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            traceId);

        // Assert
        result.Status.Should().Be(expectedMapping.HttpStatusCode,
            "HTTP status must match RefusalMapping");
    }

    [Fact]
    public void ProblemDetails_WithRequestId_IncludesRequestIdExtension()
    {
        // Arrange
        const string traceId = "test-trace-req";
        const string requestId = "test-request-req";

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
    public void ProblemDetails_WithoutRequestId_ExcludesRequestIdExtension()
    {
        // Arrange
        const string traceId = "test-trace-no-req";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId: null);

        // Assert
        result.Extensions.Should().NotContainKey(RequestId,
            "request_id should only be present for request execution kinds");
    }

    [Fact]
    public void ProblemDetails_JsonSerialization_PreservesAllFields()
    {
        // Arrange
        const string traceId = "test-trace-json";
        const string requestId = "test-request-json";
        var problemDetails = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId);

        // Act - Serialize to JSON and back
        var json = System.Text.Json.JsonSerializer.Serialize(problemDetails);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(json);

        // Assert - All fields preserved
        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be(problemDetails.Type);
        deserialized.Title.Should().Be(problemDetails.Title);
        deserialized.Status.Should().Be(problemDetails.Status);
        deserialized.Detail.Should().Be(problemDetails.Detail);
        deserialized.Instance.Should().Be(problemDetails.Instance);

        deserialized.Extensions.Should().ContainKey(InvariantCodeKey);
        deserialized.Extensions.Should().ContainKey(TraceId);
        deserialized.Extensions.Should().ContainKey(RequestId);
        deserialized.Extensions.Should().ContainKey(GuidanceLink);
    }

    [Theory]
    [InlineData(InvariantCode.ContextInitialized, 401)]
    [InlineData(InvariantCode.TenantAttributionUnambiguous, 422)]
    [InlineData(InvariantCode.TenantScopeRequired, 403)]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited, 403)]
    [InlineData(InvariantCode.DisclosureSafe, 500)]
    public void ProblemDetails_ForSpecificInvariants_ReturnsExpectedHttpStatus(
        string invariantCode,
        int expectedStatus)
    {
        // Arrange
        const string traceId = "test-trace-specific";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            invariantCode,
            traceId);

        // Assert
        result.Status.Should().Be(expectedStatus,
            $"{invariantCode} should return HTTP {expectedStatus}");
    }

    [Fact]
    public void ProblemDetails_GuidanceLink_IsValidAbsoluteUri()
    {
        // Arrange
        const string traceId = "test-trace-guidance";

        // Act
        var result = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId);

        // Assert
        var guidanceLink = result.Extensions[GuidanceLink]?.ToString();
        guidanceLink.Should().NotBeNullOrWhiteSpace();
        Uri.IsWellFormedUriString(guidanceLink, UriKind.Absolute)
            .Should().BeTrue("Guidance link must be a valid absolute URI");
    }

    public static IEnumerable<object[]> AllInvariantCodes()
    {
        foreach (var code in InvariantCode.All)
        {
            yield return new object[] { code };
        }
    }
}
