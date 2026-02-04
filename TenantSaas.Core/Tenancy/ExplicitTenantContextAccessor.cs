using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Provides explicit tenant context using constructor injection pattern.
/// </summary>
/// <remarks>
/// This is the explicit-passing alternative to ambient context.
/// Context is passed explicitly per operation, not propagated automatically.
/// </remarks>
public sealed class ExplicitTenantContextAccessor : IMutableTenantContextAccessor
{
    private TenantContext? context;

    /// <summary>
    /// Initializes a new instance with no context.
    /// </summary>
    public ExplicitTenantContextAccessor()
    {
        context = null;
    }

    private ExplicitTenantContextAccessor(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.context = context;
    }

    /// <inheritdoc />
    public TenantContext? Current => context;

    /// <inheritdoc />
    public bool IsInitialized => Current is not null;

    /// <summary>
    /// Sets the tenant context for the current execution flow.
    /// </summary>
    public void Set(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.context = context;
    }

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    public void Clear()
    {
        context = null;
    }

    /// <summary>
    /// Creates a new accessor instance with the specified context.
    /// </summary>
    public static ExplicitTenantContextAccessor WithContext(TenantContext context)
    {
        return new ExplicitTenantContextAccessor(context);
    }
}
