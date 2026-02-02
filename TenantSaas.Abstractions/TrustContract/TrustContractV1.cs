using System.Collections.Frozen;
using TenantSaas.Abstractions.BreakGlass;
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
    /// Cross-tenant marker for break-glass audit events.
    /// </summary>
    public const string BreakGlassMarkerCrossTenant = "cross_tenant";

    /// <summary>
    /// Privileged operation marker for break-glass declarations.
    /// </summary>
    public const string BreakGlassMarkerPrivileged = "privileged";

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
        "urn:tenantsaas:error:tenant-attribution-unambiguous";

    /// <summary>
    /// Gets the invariant registry.
    /// </summary>
    public static readonly FrozenDictionary<string, InvariantDefinition> Invariants =
        new Dictionary<string, InvariantDefinition>(StringComparer.Ordinal)
        {
            [InvariantCode.ContextInitialized] = new InvariantDefinition(
                InvariantCode.ContextInitialized,
                name: "Context Initialized",
                description: "Tenant context must be initialized before operations can proceed.",
                category: "Initialization"),
            [InvariantCode.TenantAttributionUnambiguous] = new InvariantDefinition(
                InvariantCode.TenantAttributionUnambiguous,
                name: "Tenant Attribution Unambiguous",
                description: "Tenant attribution from available sources must be unambiguous.",
                category: "Attribution"),
            [InvariantCode.TenantScopeRequired] = new InvariantDefinition(
                InvariantCode.TenantScopeRequired,
                name: "Tenant Scope Required",
                description: "Operation requires an explicit tenant scope.",
                category: "Scope"),
            [InvariantCode.BreakGlassExplicitAndAudited] = new InvariantDefinition(
                InvariantCode.BreakGlassExplicitAndAudited,
                name: "Break-Glass Explicit and Audited",
                description: "Break-glass must be explicitly declared with actor identity and reason.",
                category: "Authorization"),
            [InvariantCode.DisclosureSafe] = new InvariantDefinition(
                InvariantCode.DisclosureSafe,
                name: "Disclosure Safe",
                description: "Tenant information disclosure must follow safe disclosure policy.",
                category: "Disclosure")
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Gets the refusal mapping registry.
    /// </summary>
    public static readonly FrozenDictionary<string, RefusalMapping> RefusalMappings =
        new Dictionary<string, RefusalMapping>(StringComparer.Ordinal)
        {
            [InvariantCode.ContextInitialized] = RefusalMapping.ForBadRequest(
                InvariantCode.ContextInitialized,
                title: "Tenant context not initialized",
                guidanceUri: "https://docs.tenantsaas.dev/errors/context-not-initialized"),
            [InvariantCode.TenantAttributionUnambiguous] = RefusalMapping.ForUnprocessableEntity(
                InvariantCode.TenantAttributionUnambiguous,
                title: "Tenant attribution is ambiguous",
                guidanceUri: "https://docs.tenantsaas.dev/errors/attribution-ambiguous"),
            [InvariantCode.TenantScopeRequired] = RefusalMapping.ForForbidden(
                InvariantCode.TenantScopeRequired,
                title: "Tenant scope required",
                guidanceUri: "https://docs.tenantsaas.dev/errors/tenant-scope-required"),
            [InvariantCode.BreakGlassExplicitAndAudited] = RefusalMapping.ForForbidden(
                InvariantCode.BreakGlassExplicitAndAudited,
                title: "Break-glass must be explicit",
                guidanceUri: "https://docs.tenantsaas.dev/errors/break-glass-required"),
            [InvariantCode.DisclosureSafe] = RefusalMapping.ForInternalServerError(
                InvariantCode.DisclosureSafe,
                title: "Tenant disclosure policy violation",
                guidanceUri: "https://docs.tenantsaas.dev/errors/disclosure-unsafe")
        }.ToFrozenDictionary(StringComparer.Ordinal);

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

    /// <summary>
    /// Validates a break-glass declaration for privileged operations.
    /// </summary>
    /// <remarks>
    /// Break-glass is never implicit or default; privileged flows must provide a declaration.
    /// </remarks>
    public static BreakGlassValidationResult ValidateBreakGlassDeclaration(BreakGlassDeclaration? declaration)
        => BreakGlassValidator.Validate(declaration);

    /// <summary>
    /// Gets an invariant definition by code.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the invariant code is not registered.</exception>
    public static InvariantDefinition GetInvariant(string code)
    {
        if (!Invariants.TryGetValue(code, out var definition))
        {
            throw new KeyNotFoundException(
                $"Invariant code '{code}' is not registered in Trust Contract {Version}.");
        }

        return definition;
    }

    /// <summary>
    /// Tries to get an invariant definition by code.
    /// </summary>
    public static bool TryGetInvariant(string code, out InvariantDefinition? definition)
        => Invariants.TryGetValue(code, out definition);

    /// <summary>
    /// Gets a refusal mapping by invariant code.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the refusal mapping is not registered.</exception>
    public static RefusalMapping GetRefusalMapping(string invariantCode)
    {
        if (!RefusalMappings.TryGetValue(invariantCode, out var mapping))
        {
            throw new KeyNotFoundException(
                $"Refusal mapping for invariant '{invariantCode}' is not registered in Trust Contract {Version}.");
        }

        return mapping;
    }

    /// <summary>
    /// Tries to get a refusal mapping by invariant code.
    /// </summary>
    public static bool TryGetRefusalMapping(string invariantCode, out RefusalMapping? mapping)
        => RefusalMappings.TryGetValue(invariantCode, out mapping);
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
