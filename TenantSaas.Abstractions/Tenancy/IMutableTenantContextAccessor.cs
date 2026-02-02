namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Extends <see cref="ITenantContextAccessor"/> with mutation capabilities for middleware
/// and initialization code that needs to set or clear the tenant context.
/// </summary>
/// <remarks>
/// <para>
/// This interface is an extension seam (FR16) for the tenant context lifecycle.
/// Implementations control how context is stored (ambient via AsyncLocal, explicit via DI, etc.).
/// </para>
/// <para>
/// Middleware and initialization code should depend on this interface when they need to
/// set context. Business logic and enforcement code should depend on the read-only
/// <see cref="ITenantContextAccessor"/> interface.
/// </para>
/// </remarks>
public interface IMutableTenantContextAccessor : ITenantContextAccessor
{
    /// <summary>
    /// Sets the tenant context for the current execution flow.
    /// </summary>
    /// <param name="context">The tenant context to set. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    void Set(TenantContext context);

    /// <summary>
    /// Clears the tenant context for the current execution flow.
    /// </summary>
    /// <remarks>
    /// This should be called in a finally block after request processing completes
    /// to prevent context leakage in pooled execution environments.
    /// </remarks>
    void Clear();
}
