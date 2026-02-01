using TenantSaas.Abstractions.Invariants;

namespace TenantSaas.Abstractions.TrustContract;

/// <summary>
/// Trust Contract v1 definitions for tenant scope, execution kinds, and attribution sources.
/// </summary>
public static class TrustContractV1
{
    /// <summary>
    /// Contract version identifier.
    /// </summary>
    public const string Version = "v1";

    /// <summary>
    /// Tenant-scoped operations.
    /// </summary>
    public const string ScopeTenant = "tenant";

    /// <summary>
    /// Shared system or cross-tenant operations.
    /// </summary>
    public const string ScopeSharedSystem = "shared-system";

    /// <summary>
    /// Explicit no-tenant operations.
    /// </summary>
    public const string ScopeNoTenant = "no-tenant";

    /// <summary>
    /// HTTP or API request flow.
    /// </summary>
    public const string ExecutionRequest = "request";

    /// <summary>
    /// Background job or worker flow.
    /// </summary>
    public const string ExecutionBackground = "background";

    /// <summary>
    /// Administrative operation flow.
    /// </summary>
    public const string ExecutionAdmin = "admin";

    /// <summary>
    /// CLI or script execution flow.
    /// </summary>
    public const string ExecutionScripted = "scripted";

    /// <summary>
    /// Tenant attribution from a URL route parameter.
    /// </summary>
    public const string AttributionSourceRouteParameter = "route-parameter";

    /// <summary>
    /// Tenant attribution from a header value.
    /// </summary>
    public const string AttributionSourceHeaderValue = "header-value";

    /// <summary>
    /// Tenant attribution from the host or domain name.
    /// </summary>
    public const string AttributionSourceHostHeader = "host-header";

    /// <summary>
    /// Tenant attribution from token claims.
    /// </summary>
    public const string AttributionSourceTokenClaim = "token-claim";

    /// <summary>
    /// Tenant attribution explicitly set during context initialization.
    /// </summary>
    public const string AttributionSourceExplicitContext = "explicit-context";

    /// <summary>
    /// Invariant code for unambiguous tenant attribution.
    /// </summary>
    public const string InvariantTenantAttributionUnambiguous = InvariantCode.TenantAttributionUnambiguous;

    /// <summary>
    /// Problem Details type identifier for ambiguous tenant attribution refusals.
    /// </summary>
    public const string ProblemTypeTenantAttributionUnambiguous =
        "urn:tenant-saas:trust-contract:tenant-attribution-unambiguous";

    /// <summary>
    /// Gets the required tenant scopes for the contract.
    /// </summary>
    public static IReadOnlyCollection<string> RequiredScopes { get; } =
    [
        ScopeTenant,
        ScopeSharedSystem,
        ScopeNoTenant
    ];

    /// <summary>
    /// Gets the required execution kinds for the contract.
    /// </summary>
    public static IReadOnlyCollection<string> RequiredExecutionKinds { get; } =
    [
        ExecutionRequest,
        ExecutionBackground,
        ExecutionAdmin,
        ExecutionScripted
    ];

    /// <summary>
    /// Gets the required attribution source identifiers for the contract.
    /// </summary>
    public static IReadOnlyCollection<string> RequiredAttributionSources { get; } =
    [
        AttributionSourceRouteParameter,
        AttributionSourceHeaderValue,
        AttributionSourceHostHeader,
        AttributionSourceTokenClaim,
        AttributionSourceExplicitContext
    ];

    /// <summary>
    /// Validates that a contract definition includes all required scopes and execution kinds.
    /// </summary>
    /// <param name="scopes">Defined scopes for a contract.</param>
    /// <param name="executionKinds">Defined execution kinds for a contract.</param>
    public static TrustContractValidationResult Validate(
        IReadOnlyCollection<string> scopes,
        IReadOnlyCollection<string> executionKinds)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentNullException.ThrowIfNull(executionKinds);

        var scopeSet = new HashSet<string>(scopes, StringComparer.OrdinalIgnoreCase);
        var executionSet = new HashSet<string>(executionKinds, StringComparer.OrdinalIgnoreCase);

        var missingScopes = RequiredScopes.Where(scope => !scopeSet.Contains(scope)).ToArray();
        var missingExecutionKinds = RequiredExecutionKinds
            .Where(kind => !executionSet.Contains(kind))
            .ToArray();

        return new TrustContractValidationResult(missingScopes, missingExecutionKinds);
    }
}

/// <summary>
/// Represents missing definitions in a trust contract review.
/// </summary>
public sealed record TrustContractValidationResult(
    IReadOnlyCollection<string> MissingScopes,
    IReadOnlyCollection<string> MissingExecutionKinds)
{
    /// <summary>
    /// Gets a value indicating whether the contract is complete.
    /// </summary>
    public bool IsValid => MissingScopes.Count == 0 && MissingExecutionKinds.Count == 0;
}
