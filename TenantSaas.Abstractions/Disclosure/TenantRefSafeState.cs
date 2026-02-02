namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Defines safe-state tokens for tenant reference disclosure.
/// </summary>
public static class TenantRefSafeState
{
    /// <summary>
    /// Tenant is unresolved or attribution failed.
    /// </summary>
    public const string Unknown = "unknown";

    /// <summary>
    /// Tenant resolved but unsafe to disclose.
    /// </summary>
    public const string Sensitive = "sensitive";

    /// <summary>
    /// Admin/global cross-tenant operations.
    /// </summary>
    public const string CrossTenant = "cross_tenant";

    /// <summary>
    /// Marker indicating value is an opaque public tenant identifier.
    /// </summary>
    public const string Opaque = "opaque";

    /// <summary>
    /// Returns true if the value is a recognized safe-state token.
    /// Safe-state tokens are: unknown, sensitive, cross_tenant.
    /// Note: Opaque is NOT a safe-state token; it's a marker indicating the value is an opaque identifier.
    /// </summary>
    public static bool IsSafeState(string? value)
        => value is Unknown or Sensitive or CrossTenant;
}
