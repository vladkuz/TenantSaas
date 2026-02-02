using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Provides explicit tenant context using constructor injection pattern.
/// </summary>
/// <remarks>
/// This is the explicit-passing alternative to ambient context.
/// Context is passed explicitly per operation, not propagated automatically.
/// </remarks>
public sealed class ExplicitTenantContextAccessor : ITenantContextAccessor
{
    private readonly TenantContext? _context;

    /// <summary>
    /// Initializes a new instance with no context.
    /// </summary>
    public ExplicitTenantContextAccessor()
    {
        _context = null;
    }

    private ExplicitTenantContextAccessor(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public TenantContext? Current => _context;

    /// <inheritdoc />
    public bool IsInitialized => Current is not null;

    /// <summary>
    /// Creates a new accessor instance with the specified context.
    /// </summary>
    public static ExplicitTenantContextAccessor WithContext(TenantContext context)
    {
        return new ExplicitTenantContextAccessor(context);
    }
}
