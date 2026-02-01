namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Stable invariant codes for trust contract enforcement.
/// </summary>
public static class InvariantCode
{
    /// <summary>
    /// Tenant context must be initialized before operations.
    /// </summary>
    public const string ContextInitialized = "ContextInitialized";

    /// <summary>
    /// Tenant attribution must be unambiguous across enabled sources.
    /// </summary>
    public const string TenantAttributionUnambiguous = "TenantAttributionUnambiguous";

    /// <summary>
    /// Operation requires explicit tenant scope.
    /// </summary>
    public const string TenantScopeRequired = "TenantScopeRequired";

    /// <summary>
    /// Break-glass must be explicit with actor identity and reason.
    /// </summary>
    public const string BreakGlassExplicitAndAudited = "BreakGlassExplicitAndAudited";

    /// <summary>
    /// Tenant information disclosure must be safe.
    /// </summary>
    public const string DisclosureSafe = "DisclosureSafe";

    /// <summary>
    /// Gets all defined invariant codes.
    /// </summary>
    /// <remarks>
    /// Use this collection to ensure tests and registries stay in sync when new invariants are added.
    /// </remarks>
    public static IReadOnlyList<string> All { get; } =
    [
        ContextInitialized,
        TenantAttributionUnambiguous,
        TenantScopeRequired,
        BreakGlassExplicitAndAudited,
        DisclosureSafe
    ];
}
