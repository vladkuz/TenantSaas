using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.Invariants;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Enforces tenant context invariants at sanctioned boundaries.
/// </summary>
public static class BoundaryGuard
{
    /// <summary>
    /// Requires that tenant context has been initialized.
    /// </summary>
    /// <param name="accessor">Context accessor to check.</param>
    /// <param name="overrideTraceId">Optional trace ID for correlation when context is missing.</param>
    /// <returns>Success with context, or failure with invariant violation.</returns>
    public static EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        string? overrideTraceId = null)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        if (!accessor.IsInitialized)
        {
            var traceId = overrideTraceId ?? Guid.NewGuid().ToString("N");
            return EnforcementResult.Failure(
                InvariantCode.ContextInitialized,
                traceId,
                "Tenant context must be initialized before operations can proceed.");
        }

        return EnforcementResult.Success(accessor.Current!);
    }

    /// <summary>
    /// Requires that tenant attribution is unambiguous.
    /// </summary>
    /// <param name="result">Attribution resolution result.</param>
    /// <param name="traceId">Trace identifier for correlation.</param>
    /// <returns>Success with resolved tenant ID and source, or failure with invariant violation.</returns>
    public static AttributionEnforcementResult RequireUnambiguousAttribution(
        TenantAttributionResult result,
        string traceId)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return result switch
        {
            TenantAttributionResult.Success success => AttributionEnforcementResult.Success(
                success.TenantId,
                success.Source,
                traceId),
            
            TenantAttributionResult.Ambiguous ambiguous => AttributionEnforcementResult.Ambiguous(
                [.. ambiguous.Conflicts.Select(c => c.Source.GetDisplayName())],
                traceId),
            
            TenantAttributionResult.NotFound => AttributionEnforcementResult.NotFound(traceId),
            
            TenantAttributionResult.NotAllowed notAllowed => AttributionEnforcementResult.NotAllowed(
                notAllowed.Source,
                traceId),
            
            _ => throw new InvalidOperationException("Unknown attribution result type.")
        };
    }
}
