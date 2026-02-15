using Microsoft.EntityFrameworkCore;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;

namespace TenantSaas.EfCore;

/// <summary>
/// Reference adapter that enforces boundary guard checks before executing EF Core operations.
/// </summary>
public sealed class TenantBoundaryDbContextExecutor(
    IBoundaryGuard boundaryGuard,
    ITenantContextAccessor tenantContextAccessor)
{
    public async Task<TResult> ExecuteAsync<TContext, TResult>(
        TContext dbContext,
        Func<TContext, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        var enforcement = boundaryGuard.RequireContext(tenantContextAccessor);
        if (!enforcement.IsSuccess)
        {
            throw TenantBoundaryViolationException.FromEnforcementResult(enforcement);
        }

        return await operation(dbContext, cancellationToken).ConfigureAwait(false);
    }
}
