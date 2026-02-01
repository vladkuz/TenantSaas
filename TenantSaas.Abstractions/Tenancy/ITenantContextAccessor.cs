namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Provides access to the current tenant context for the executing flow.
/// </summary>
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
