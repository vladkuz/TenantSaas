using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace TenantSaas.ContractTestKit.Assertions;

/// <summary>
/// Assertions for RFC 7807 Problem Details compliance in HTTP responses.
/// </summary>
public static class ProblemDetailsAssertions
{
    /// <summary>
    /// Standard Problem Details extension key for invariant code.
    /// </summary>
    public const string InvariantCodeKey = "invariantCode";

    /// <summary>
    /// Standard Problem Details extension key for trace ID.
    /// </summary>
    public const string TraceIdKey = "traceId";

    /// <summary>
    /// Standard Problem Details extension key for request ID.
    /// </summary>
    public const string RequestIdKey = "requestId";

    /// <summary>
    /// Standard Problem Details extension key for guidance link.
    /// </summary>
    public const string GuidanceLinkKey = "guidanceLink";

    /// <summary>
    /// Asserts that an HTTP response contains valid RFC 7807 Problem Details.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed Problem Details for further assertions.</returns>
    public static async Task<ProblemDetails> AssertProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Content.Headers.ContentType?.MediaType
            .Should().BeOneOf("application/problem+json", "application/json",
                "Response content type must be Problem Details or JSON");

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        problemDetails.Should().NotBeNull("Response must deserialize to Problem Details");

        AssertProblemDetailsStructure(problemDetails!);

        return problemDetails!;
    }

    /// <summary>
    /// Asserts that Problem Details has valid RFC 7807 structure.
    /// </summary>
    /// <param name="problemDetails">The Problem Details to validate.</param>
    public static void AssertProblemDetailsStructure(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Type.Should().NotBeNullOrWhiteSpace("Problem Details must have a type");
        problemDetails.Title.Should().NotBeNullOrWhiteSpace("Problem Details must have a title");
        problemDetails.Status.Should().BeInRange(400, 599, "Problem Details status must be an error code");
    }

    /// <summary>
    /// Asserts that Problem Details contains the required TenantSaas extensions.
    /// </summary>
    /// <param name="problemDetails">The Problem Details to validate.</param>
    /// <param name="expectedInvariantCode">Optional expected invariant code.</param>
    public static void AssertTenantSaasExtensions(ProblemDetails problemDetails, string? expectedInvariantCode = null)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Extensions.Should().ContainKey(InvariantCodeKey,
            "Problem Details must contain invariant_code extension");
        problemDetails.Extensions.Should().ContainKey(TraceIdKey,
            "Problem Details must contain trace_id extension");
        problemDetails.Extensions.Should().ContainKey(RequestIdKey,
            "Problem Details must contain request_id extension");
        problemDetails.Extensions.Should().ContainKey(GuidanceLinkKey,
            "Problem Details must contain guidance_link extension");

        if (expectedInvariantCode is not null)
        {
            var invariantCode = GetExtensionValue(problemDetails, InvariantCodeKey);
            invariantCode.Should().Be(expectedInvariantCode);
        }
    }

    /// <summary>
    /// Asserts that Problem Details type is a valid TenantSaas URN.
    /// </summary>
    /// <param name="problemDetails">The Problem Details to validate.</param>
    public static void AssertProblemTypeIsValidUrn(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Type.Should().StartWith("urn:tenantsaas:error:",
            "Problem type must use TenantSaas URN format");
        problemDetails.Type.Should().MatchRegex("^urn:tenantsaas:error:[a-z-]+$",
            "Problem type must use kebab-case identifier");
    }

    /// <summary>
    /// Asserts that an HTTP response is a refusal for a specific invariant violation.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="expectedInvariantCode">The expected invariant code.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task AssertRefusalResponseAsync(
        HttpResponseMessage response,
        string expectedInvariantCode,
        int expectedStatusCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedInvariantCode);

        ((int)response.StatusCode).Should().Be(expectedStatusCode,
            $"Response status code should be {expectedStatusCode} for {expectedInvariantCode} violation");

        var problemDetails = await AssertProblemDetailsAsync(response, cancellationToken);
        AssertTenantSaasExtensions(problemDetails, expectedInvariantCode);
        AssertProblemTypeIsValidUrn(problemDetails);
    }

    private static string? GetExtensionValue(ProblemDetails problemDetails, string key)
    {
        if (!problemDetails.Extensions.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
            _ => value?.ToString()
        };
    }
}
