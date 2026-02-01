using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Defines the allowed sources for tenant attribution.
/// </summary>
public enum TenantAttributionSource
{
    /// <summary>
    /// Tenant from a URL route parameter (for example, /tenants/{tenantId}).
    /// </summary>
    RouteParameter = 1,

    /// <summary>
    /// Tenant from an HTTP header (for example, X-Tenant-Id).
    /// </summary>
    HeaderValue = 2,

    /// <summary>
    /// Tenant from the host or domain name (for example, tenant1.api.example.com).
    /// </summary>
    HostHeader = 3,

    /// <summary>
    /// Tenant from authentication token claims.
    /// </summary>
    TokenClaim = 4,

    /// <summary>
    /// Tenant explicitly set during context initialization.
    /// </summary>
    ExplicitContext = 5
}

/// <summary>
/// Provides stable identifiers and display metadata for attribution sources.
/// </summary>
public static class TenantAttributionSourceMetadata
{
    /// <summary>
    /// Gets the stable identifier for the attribution source.
    /// </summary>
    public static string GetIdentifier(this TenantAttributionSource source) => source switch
    {
        TenantAttributionSource.RouteParameter => TrustContractV1.AttributionSourceRouteParameter,
        TenantAttributionSource.HeaderValue => TrustContractV1.AttributionSourceHeaderValue,
        TenantAttributionSource.HostHeader => TrustContractV1.AttributionSourceHostHeader,
        TenantAttributionSource.TokenClaim => TrustContractV1.AttributionSourceTokenClaim,
        TenantAttributionSource.ExplicitContext => TrustContractV1.AttributionSourceExplicitContext,
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown attribution source.")
    };

    /// <summary>
    /// Gets the display name for the attribution source.
    /// </summary>
    public static string GetDisplayName(this TenantAttributionSource source) => source switch
    {
        TenantAttributionSource.RouteParameter => "Route Parameter",
        TenantAttributionSource.HeaderValue => "Header Value",
        TenantAttributionSource.HostHeader => "Host Header",
        TenantAttributionSource.TokenClaim => "Token Claim",
        TenantAttributionSource.ExplicitContext => "Explicit Context",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown attribution source.")
    };

    /// <summary>
    /// Gets a detailed description for the attribution source.
    /// </summary>
    public static string GetDescription(this TenantAttributionSource source) => source switch
    {
        TenantAttributionSource.RouteParameter => "Tenant identifier supplied as an explicit route parameter.",
        TenantAttributionSource.HeaderValue => "Tenant identifier supplied in a trusted request header.",
        TenantAttributionSource.HostHeader => "Tenant identifier inferred from the host or domain name.",
        TenantAttributionSource.TokenClaim => "Tenant identifier asserted in authentication token claims.",
        TenantAttributionSource.ExplicitContext => "Tenant identifier explicitly set during context initialization.",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown attribution source.")
    };
}
