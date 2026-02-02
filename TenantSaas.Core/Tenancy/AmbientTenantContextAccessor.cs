using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Provides ambient tenant context using AsyncLocal propagation.
/// </summary>
/// <remarks>
/// <para>
/// This is the default accessor implementation. Context flows automatically
/// across async/await boundaries within the same logical execution flow.
/// Each new request/job/admin/script must explicitly initialize its own context.
/// </para>
/// <para>
/// <strong>Important:</strong> This class uses a static <see cref="AsyncLocal{T}"/> for storage,
/// meaning all instances share the same backing context. This is intentional for ambient propagation.
/// Register as a singleton in DI to avoid confusion about instance semantics.
/// </para>
/// <para>
/// Middleware must call <see cref="Clear"/> in a finally block after request processing
/// to prevent context leakage in pooled execution environments.
/// </para>
/// </remarks>
public sealed class AmbientTenantContextAccessor : IMutableTenantContextAccessor
{
    private static readonly AsyncLocal<TenantContext?> _current = new();

    /// <inheritdoc />
    public TenantContext? Current => _current.Value;

    /// <inheritdoc />
    public bool IsInitialized => Current is not null;

    /// <summary>
    /// Sets the current tenant context for this execution flow.
    /// </summary>
    public void Set(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _current.Value = context;
    }

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    public void Clear()
    {
        _current.Value = null;
    }
}
