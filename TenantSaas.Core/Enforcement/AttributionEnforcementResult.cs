using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Represents the result of tenant attribution enforcement.
/// </summary>
public sealed record AttributionEnforcementResult
{
    private AttributionEnforcementResult(
        bool isSuccess,
        TenantId? resolvedTenantId,
        TenantAttributionSource? resolvedSource,
        string? invariantCode,
        string traceId,
        string? detail,
        IReadOnlyList<string>? conflictingSources)
    {
        IsSuccess = isSuccess;
        ResolvedTenantId = resolvedTenantId;
        ResolvedSource = resolvedSource;
        InvariantCode = invariantCode;
        TraceId = traceId;
        Detail = detail;
        ConflictingSources = conflictingSources;
    }

    /// <summary>
    /// Gets whether enforcement succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the resolved tenant identifier when successful.
    /// </summary>
    public TenantId? ResolvedTenantId { get; }

    /// <summary>
    /// Gets the resolved attribution source when successful.
    /// </summary>
    public TenantAttributionSource? ResolvedSource { get; }

    /// <summary>
    /// Gets the invariant code when enforcement failed.
    /// </summary>
    public string? InvariantCode { get; }

    /// <summary>
    /// Gets the trace identifier for correlation.
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// Gets the human-readable failure reason (disclosure-safe).
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Gets the conflicting source names (no tenant IDs).
    /// </summary>
    public IReadOnlyList<string>? ConflictingSources { get; }

    /// <summary>
    /// Creates a successful attribution enforcement result.
    /// </summary>
    public static AttributionEnforcementResult Success(
        TenantId tenantId,
        TenantAttributionSource source,
        string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return new(
            isSuccess: true,
            resolvedTenantId: tenantId,
            resolvedSource: source,
            invariantCode: null,
            traceId: traceId,
            detail: null,
            conflictingSources: null);
    }

    /// <summary>
    /// Creates an ambiguous attribution failure result.
    /// </summary>
    public static AttributionEnforcementResult Ambiguous(
        IReadOnlyList<string> conflictingSources,
        string traceId)
    {
        ArgumentNullException.ThrowIfNull(conflictingSources);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        if (conflictingSources.Count == 0)
        {
            throw new ArgumentException("Ambiguous result must include conflicting sources.", nameof(conflictingSources));
        }

        var detail = $"Tenant attribution is ambiguous: {conflictingSources.Count} sources provided conflicting tenant identifiers.";

        return new(
            isSuccess: false,
            resolvedTenantId: null,
            resolvedSource: null,
            invariantCode: Abstractions.Invariants.InvariantCode.TenantAttributionUnambiguous,
            traceId: traceId,
            detail: detail,
            conflictingSources: conflictingSources);
    }

    /// <summary>
    /// Creates a not-found attribution failure result.
    /// </summary>
    public static AttributionEnforcementResult NotFound(string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return new(
            isSuccess: false,
            resolvedTenantId: null,
            resolvedSource: null,
            invariantCode: Abstractions.Invariants.InvariantCode.TenantAttributionUnambiguous,
            traceId: traceId,
            detail: "Tenant attribution could not be resolved from any enabled source.",
            conflictingSources: null);
    }

    /// <summary>
    /// Creates a not-allowed attribution failure result.
    /// </summary>
    /// <remarks>
    /// DISCLOSURE SAFETY: Uses TenantAttributionSource.GetDisplayName() which must return a source type name,
    /// never a tenant identifier. Custom attribution source implementations must honor this contract.
    /// </remarks>
    public static AttributionEnforcementResult NotAllowed(
        TenantAttributionSource disallowedSource,
        string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var detail = $"Tenant attribution source '{disallowedSource.GetDisplayName()}' is not allowed for this operation.";

        return new(
            isSuccess: false,
            resolvedTenantId: null,
            resolvedSource: null,
            invariantCode: Abstractions.Invariants.InvariantCode.TenantAttributionUnambiguous,
            traceId: traceId,
            detail: detail,
            conflictingSources: null);
    }
}
