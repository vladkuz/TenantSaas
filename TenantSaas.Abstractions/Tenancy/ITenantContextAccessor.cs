namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Provides read-only access to the current tenant context for the executing flow.
/// </summary>
/// <remarks>
/// <para>
/// This is an extension seam (FR16) that allows different context propagation strategies.
/// Implementations may use ambient propagation (AsyncLocal), explicit context passing,
/// or other mechanisms appropriate to the execution environment.
/// </para>
/// <para>
/// A null <see cref="Current"/> value indicates an invariant violation where context
/// initialization has not occurred. The <see cref="IsInitialized"/> property provides
/// a convenient null-check.
/// </para>
/// <para>
/// For code that needs to set or clear context (middleware, initialization),
/// use <see cref="IMutableTenantContextAccessor"/> instead.
/// </para>
/// </remarks>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant context, or <see langword="null"/> when uninitialized.
    /// </summary>
    /// <remarks>
    /// A null value indicates an invariant violation where context initialization has not occurred.
    /// Implementations must be thread-safe for concurrent reads.
    /// </remarks>
    TenantContext? Current { get; }

    /// <summary>
    /// Gets a value indicating whether the context has been initialized for the current flow.
    /// </summary>
    bool IsInitialized { get; }
}
