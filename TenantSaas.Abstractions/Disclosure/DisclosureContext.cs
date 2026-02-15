using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Represents the context for evaluating tenant disclosure safety.
/// </summary>
public sealed record DisclosureContext
{
    /// <summary>
    /// Gets the actual tenant identifier (may be null).
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets whether the caller is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// Gets whether the caller is authorized for the tenant.
    /// </summary>
    public bool IsAuthorizedForTenant { get; init; }

    /// <summary>
    /// Gets whether disclosure risks enumeration attack.
    /// </summary>
    public bool IsEnumerationRisk { get; init; }

    /// <summary>
    /// Gets the current tenant scope.
    /// </summary>
    public TenantScope? Scope { get; init; }

    /// <summary>
    /// Creates a disclosure context from an initialized tenant context.
    /// </summary>
    /// <param name="context">The tenant context.</param>
    /// <param name="isAuthenticated">Whether the caller is authenticated.</param>
    /// <param name="isAuthorizedForTenant">Whether the caller is authorized for the tenant.</param>
    /// <param name="isEnumerationRisk">Whether disclosure risks enumeration attack. Defaults to false.</param>
    public static DisclosureContext Create(
        TenantContext context,
        bool isAuthenticated,
        bool isAuthorizedForTenant,
        bool isEnumerationRisk = false)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tenantId = context.Scope switch
        {
            TenantScope.Tenant tenantScope => tenantScope.Id.Value,
            _ => null
        };

        return new DisclosureContext
        {
            TenantId = tenantId,
            IsAuthenticated = isAuthenticated,
            IsAuthorizedForTenant = isAuthorizedForTenant,
            IsEnumerationRisk = isEnumerationRisk,
            Scope = context.Scope
        };
    }
}
