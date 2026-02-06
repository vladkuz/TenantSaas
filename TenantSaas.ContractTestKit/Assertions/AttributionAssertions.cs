using FluentAssertions;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTestKit.Assertions;

/// <summary>
/// Assertions for tenant attribution rules compliance.
/// </summary>
public static class AttributionAssertions
{
    /// <summary>
    /// Asserts that all required attribution sources are defined.
    /// </summary>
    public static void AssertAllAttributionSourcesAreDefined()
    {
        var sources = Enum.GetValues<TenantAttributionSource>();

        sources.Should().Contain(TenantAttributionSource.RouteParameter);
        sources.Should().Contain(TenantAttributionSource.HeaderValue);
        sources.Should().Contain(TenantAttributionSource.HostHeader);
        sources.Should().Contain(TenantAttributionSource.TokenClaim);
        sources.Should().Contain(TenantAttributionSource.ExplicitContext);
    }

    /// <summary>
    /// Asserts that attribution sources have stable identifiers for serialization.
    /// </summary>
    public static void AssertAttributionSourceIdentifiersAreStable()
    {
        TenantAttributionSource.RouteParameter.GetIdentifier().Should().Be("route-parameter");
        TenantAttributionSource.HeaderValue.GetIdentifier().Should().Be("header-value");
        TenantAttributionSource.HostHeader.GetIdentifier().Should().Be("host-header");
        TenantAttributionSource.TokenClaim.GetIdentifier().Should().Be("token-claim");
        TenantAttributionSource.ExplicitContext.GetIdentifier().Should().Be("explicit-context");
    }

    /// <summary>
    /// Asserts that attribution sources have display names and descriptions.
    /// </summary>
    public static void AssertAttributionSourcesHaveMetadata()
    {
        var sources = Enum.GetValues<TenantAttributionSource>();

        foreach (var source in sources)
        {
            source.GetDisplayName().Should().NotBeNullOrWhiteSpace(
                $"Attribution source '{source}' must have a display name");
            source.GetIdentifier().Should().NotBeNullOrWhiteSpace(
                $"Attribution source '{source}' must have an identifier");
            source.GetDescription().Should().NotBeNullOrWhiteSpace(
                $"Attribution source '{source}' must have a description");
        }
    }

    /// <summary>
    /// Asserts that TrustContractV1 required attribution sources match the enum values.
    /// </summary>
    public static void AssertContractAttributionSourcesMatchEnum()
    {
        var enumSources = Enum.GetValues<TenantAttributionSource>()
            .Select(s => s.GetIdentifier())
            .OrderBy(s => s)
            .ToArray();

        var contractSources = TrustContractV1.RequiredAttributionSources
            .OrderBy(s => s)
            .ToArray();

        enumSources.Should().BeEquivalentTo(contractSources,
            "Trust contract attribution sources must match enum definitions");
    }
}
