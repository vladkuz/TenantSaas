using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Implements the default disclosure policy for tenant references.
/// </summary>
public sealed class DisclosurePolicy
{
    /// <summary>
    /// Resolves the safe tenant reference based on disclosure context.
    /// </summary>
    public string ResolveTenantRef(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Scope is TenantScope.NoTenant)
        {
            return TenantRefSafeState.Unknown;
        }

        if (context.Scope is TenantScope.SharedSystem)
        {
            return TenantRefSafeState.CrossTenant;
        }

        if (!context.IsAuthenticated)
        {
            return TenantRefSafeState.Unknown;
        }

        if (!context.IsAuthorizedForTenant)
        {
            return TenantRefSafeState.Sensitive;
        }

        if (context.IsEnumerationRisk)
        {
            return TenantRefSafeState.Sensitive;
        }

        return context.TenantId ?? TenantRefSafeState.Opaque;
    }

    /// <summary>
    /// Returns true if tenant information may appear in error responses.
    /// </summary>
    public bool AllowTenantInErrors(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.IsAuthenticated
            && context.IsAuthorizedForTenant
            && !context.IsEnumerationRisk;
    }

    /// <summary>
    /// Returns true if tenant information may appear in logs.
    /// Logs always use safe states.
    /// </summary>
    public bool AllowTenantInLogs(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return true;
    }
}
