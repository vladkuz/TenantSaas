using TenantSaas.Core.Enforcement;

namespace TenantSaas.EfCore;

/// <summary>
/// Raised when a boundary guard refusal blocks EF Core adapter execution.
/// </summary>
public sealed class TenantBoundaryViolationException : InvalidOperationException
{
    public TenantBoundaryViolationException(
        string invariantCode,
        string traceId,
        string detail)
        : base(detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);

        InvariantCode = invariantCode;
        TraceId = traceId;
    }

    /// <summary>
    /// Gets the invariant code that caused the boundary violation.
    /// </summary>
    public string InvariantCode { get; }

    /// <summary>
    /// Gets the trace identifier for correlation.
    /// </summary>
    public string TraceId { get; }

    internal static TenantBoundaryViolationException FromEnforcementResult(EnforcementResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new ArgumentException(
                "Cannot create a boundary violation from a successful enforcement result.",
                nameof(result));
        }

        return new TenantBoundaryViolationException(
            result.InvariantCode!,
            result.TraceId!,
            result.Detail!);
    }
}
