using FluentAssertions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTestKit.Assertions;

/// <summary>
/// Assertions for trust contract refusal mapping compliance.
/// </summary>
public static class RefusalMappingAssertions
{
    /// <summary>
    /// Asserts that all invariants have corresponding refusal mappings.
    /// </summary>
    public static void AssertAllRefusalMappingsRegistered()
    {
        foreach (var code in InvariantCode.All)
        {
            TrustContractV1.TryGetRefusalMapping(code, out var mapping)
                .Should().BeTrue($"Refusal mapping for '{code}' must be registered");

            mapping.Should().NotBeNull();
            AssertRefusalMappingIsValid(mapping!);
        }
    }

    /// <summary>
    /// Asserts that a specific refusal mapping is registered with expected properties.
    /// </summary>
    /// <param name="invariantCode">The invariant code to check.</param>
    /// <param name="expectedHttpStatusCode">Optional expected HTTP status code.</param>
    public static void AssertRefusalMappingRegistered(string invariantCode, int? expectedHttpStatusCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);

        TrustContractV1.TryGetRefusalMapping(invariantCode, out var mapping)
            .Should().BeTrue($"Refusal mapping for '{invariantCode}' must be registered");

        mapping.Should().NotBeNull();
        AssertRefusalMappingIsValid(mapping!);

        if (expectedHttpStatusCode.HasValue)
        {
            mapping!.HttpStatusCode.Should().Be(expectedHttpStatusCode.Value);
        }
    }

    /// <summary>
    /// Asserts that a refusal mapping has valid structure.
    /// </summary>
    /// <param name="mapping">The refusal mapping to validate.</param>
    public static void AssertRefusalMappingIsValid(RefusalMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        mapping.InvariantCode.Should().NotBeNullOrWhiteSpace();
        mapping.HttpStatusCode.Should().BeInRange(400, 599, "HTTP status must be a client or server error");
        mapping.ProblemType.Should().NotBeNullOrWhiteSpace();
        mapping.Title.Should().NotBeNullOrWhiteSpace();
        mapping.GuidanceUri.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Asserts that all refusal mapping Problem Types use stable URN format.
    /// </summary>
    public static void AssertProblemTypesAreStableUrns()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            mapping.ProblemType.Should().StartWith("urn:tenantsaas:error:",
                $"Problem type for '{mapping.InvariantCode}' must use stable URN format");

            mapping.ProblemType.Should().MatchRegex("^urn:tenantsaas:error:[a-z-]+$",
                $"Problem type for '{mapping.InvariantCode}' must use kebab-case identifier");
        }
    }

    /// <summary>
    /// Asserts that all refusal mapping guidance URIs are well-formed absolute URIs.
    /// </summary>
    public static void AssertGuidanceUrisAreWellFormed()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            Uri.IsWellFormedUriString(mapping.GuidanceUri, UriKind.Absolute)
                .Should().BeTrue($"Guidance URI for '{mapping.InvariantCode}' must be a valid absolute URI");
        }
    }

    /// <summary>
    /// Asserts that HTTP status codes for refusal mappings are semantically appropriate.
    /// </summary>
    public static void AssertHttpStatusCodesAreAppropriate()
    {
        // ContextInitialized → 401 Unauthorized (not authenticated in tenant context)
        AssertRefusalMappingRegistered(InvariantCode.ContextInitialized, expectedHttpStatusCode: 401);

        // TenantAttributionUnambiguous → 422 Unprocessable Entity (cannot determine tenant)
        AssertRefusalMappingRegistered(InvariantCode.TenantAttributionUnambiguous, expectedHttpStatusCode: 422);

        // TenantScopeRequired → 403 Forbidden (operation requires tenant scope)
        AssertRefusalMappingRegistered(InvariantCode.TenantScopeRequired, expectedHttpStatusCode: 403);

        // BreakGlassExplicitAndAudited → 403 Forbidden (privileged operation denied)
        AssertRefusalMappingRegistered(InvariantCode.BreakGlassExplicitAndAudited, expectedHttpStatusCode: 403);

        // DisclosureSafe → 500 Internal Server Error (disclosure policy violated internally)
        AssertRefusalMappingRegistered(InvariantCode.DisclosureSafe, expectedHttpStatusCode: 500);
    }
}
