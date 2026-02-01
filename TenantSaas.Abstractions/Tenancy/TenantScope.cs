namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Represents the tenant scoping of an execution context.
/// </summary>
public abstract record TenantScope
{
    private TenantScope()
    {
    }

    /// <summary>
    /// Scoped to a specific tenant identifier.
    /// </summary>
    /// <param name="Id">Tenant identifier.</param>
    public sealed record Tenant(TenantId Id) : TenantScope;

    /// <summary>
    /// Shared or cross-tenant system operations.
    /// </summary>
    public sealed record SharedSystem : TenantScope;

    /// <summary>
    /// Explicit no-tenant context with an allowed reason.
    /// </summary>
    /// <param name="Reason">Reason for operating without tenant scope.</param>
    public sealed record NoTenant(NoTenantReason Reason) : TenantScope;

    /// <summary>
    /// Creates a tenant-scoped instance.
    /// </summary>
    /// <param name="id">Tenant identifier.</param>
    public static TenantScope ForTenant(TenantId id) => new Tenant(id);

    /// <summary>
    /// Creates a shared-system scope.
    /// </summary>
    public static TenantScope ForSharedSystem() => new SharedSystem();

    /// <summary>
    /// Creates a no-tenant scope with an allowed reason.
    /// </summary>
    /// <param name="reason">Reason for operating without tenant scope.</param>
    public static TenantScope ForNoTenant(NoTenantReason reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        return new NoTenant(reason);
    }
}
