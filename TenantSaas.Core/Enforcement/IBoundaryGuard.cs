using TenantSaas.Abstractions.BreakGlass;
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

    /// <summary>
    /// Requires that break-glass is explicitly declared for privileged operations.
    /// </summary>
    /// <param name="declaration">Break-glass declaration with actor, reason, and scope. Null results in refusal.</param>
    /// <param name="traceId">Trace identifier for correlation.</param>
    /// <param name="requestId">Request identifier for correlation (optional).</param>
    /// <param name="cancellationToken">Cancellation token for audit sink operations.</param>
    /// <returns>
    /// Success if declaration is valid with all required fields (actor, reason, scope).
    /// Failure with BreakGlassExplicitAndAudited if declaration is null, incomplete, or invalid.
    /// </returns>
    /// <remarks>
    /// Break-glass must always be explicit and never implicit or default.
    /// Missing or incomplete declarations (null actor, empty reason, etc.) result in refusal.
    /// When successful, an audit event is emitted and logged for security review.
    /// </remarks>
    Task<EnforcementResult> RequireBreakGlassAsync(
        BreakGlassDeclaration? declaration,
        string traceId,
        string? requestId = null,
        CancellationToken cancellationToken = default);
}
