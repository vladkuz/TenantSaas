using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Enforces tenant context invariants at sanctioned boundaries.
/// </summary>
/// <remarks>
/// This is the primary enforcement interface for tenant context invariants.
/// Implementations validate that tenant context is properly initialized
/// and that tenant attribution is unambiguous before operations proceed.
/// </remarks>
public interface IBoundaryGuard
{
    /// <summary>
    /// Requires that tenant context has been initialized.
    /// </summary>
    /// <param name="accessor">Context accessor to check.</param>
    /// <param name="overrideTraceId">Optional trace ID for correlation when context is missing.</param>
    /// <returns>Success with context, or failure with invariant violation.</returns>
    EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        string? overrideTraceId = null);

    /// <summary>
    /// Requires that tenant attribution is unambiguous.
    /// </summary>
    /// <param name="result">Attribution resolution result.</param>
    /// <param name="traceId">Trace identifier for correlation.</param>
    /// <returns>Success with resolved tenant ID and source, or failure with invariant violation.</returns>
    AttributionEnforcementResult RequireUnambiguousAttribution(
        TenantAttributionResult result,
        string traceId);
}
