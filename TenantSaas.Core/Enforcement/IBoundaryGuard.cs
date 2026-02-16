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
    /// Requires that tenant context has been initialized using explicit context passing.
    /// </summary>
    /// <param name="context">Explicit tenant context to validate. Null is treated as uninitialized.</param>
    /// <param name="overrideTraceId">Optional trace ID for correlation when context is null. If not provided, a new GUID is generated.</param>
    /// <returns>Success with context, or failure with ContextInitialized invariant.</returns>
    /// <remarks>
    /// This overload enables enforcement without ambient context dependency (ITenantContextAccessor).
    /// Null context is refused with ContextInitialized invariant.
    /// For pure explicit mode without accessor dependency, use this overload.
    /// </remarks>
    EnforcementResult RequireContext(
        TenantContext? context,
        string? overrideTraceId = null);

    /// <summary>
    /// Requires that tenant context has been initialized, with explicit context taking precedence over ambient.
    /// </summary>
    /// <param name="accessor">Ambient context accessor (fallback). Required even when explicit context is provided.</param>
    /// <param name="explicitContext">Explicit context (priority). If non-null, takes precedence over accessor.</param>
    /// <returns>Success with context (explicit or ambient), or failure with invariant violation.</returns>
    /// <remarks>
    /// When both explicit context and ambient accessor are provided, explicit context takes precedence.
    /// This allows hybrid usage where explicit context can override ambient for specific operations.
    /// For pure explicit mode without accessor dependency, use the <see cref="RequireContext(TenantContext?, string?)"/> overload instead.
    /// </remarks>
    EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        TenantContext? explicitContext);

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
    /// Requires that a shared-system operation is explicitly allowlisted and mapped to the evaluated invariant.
    /// </summary>
    /// <param name="context">Initialized tenant context for the current operation.</param>
    /// <param name="operationName">Declared shared-system operation identifier.</param>
    /// <param name="invariantCode">Invariant being enforced for this operation.</param>
    /// <returns>
    /// Success when the context is shared-system scope and operation/invariant mapping is allowed.
    /// Failure with TenantScopeRequired or SharedSystemOperationAllowed when mapping rules are violated.
    /// </returns>
    EnforcementResult RequireSharedSystemOperation(
        TenantContext context,
        string operationName,
        string invariantCode);

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
