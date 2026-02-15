using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Scoped execution flow that automatically clears tenant context on disposal.
/// </summary>
/// <remarks>
/// This implementation wraps the <see cref="ITenantContextInitializer"/> to provide
/// automatic cleanup via the disposable pattern, similar to <c>IServiceScope</c> in .NET DI.
/// </remarks>
internal sealed class TenantFlowScope : ITenantFlowScope
{
    private readonly IMutableTenantContextAccessor accessor;
    private bool disposed;

    /// <summary>
    /// Initializes a new tenant flow scope with the given context.
    /// </summary>
    /// <param name="context">The tenant context for this flow.</param>
    /// <param name="accessor">The accessor used to clear context on disposal.</param>
    internal TenantFlowScope(TenantContext context, IMutableTenantContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(accessor);

        Context = context;
        this.accessor = accessor;
    }

    /// <inheritdoc />
    public TenantContext Context { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        if (ReferenceEquals(accessor.Current, Context))
        {
            accessor.Clear();
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
