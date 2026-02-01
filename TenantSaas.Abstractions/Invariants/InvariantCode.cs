namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Stable invariant codes for trust contract enforcement.
/// </summary>
public static class InvariantCode
{
    /// <summary>
    /// Tenant attribution must be unambiguous across enabled sources.
    /// </summary>
    public const string TenantAttributionUnambiguous = "tenant-attribution-unambiguous";
}
