using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Represents the result of an enforcement check.
/// </summary>
public sealed record EnforcementResult
{
    private EnforcementResult(
        bool isSuccess,
        TenantContext? context,
        string? invariantCode,
        string? traceId,
        string? detail)
    {
        IsSuccess = isSuccess;
        Context = context;
        InvariantCode = invariantCode;
        TraceId = traceId;
        Detail = detail;
    }

    /// <summary>
    /// Gets whether enforcement succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the validated tenant context when successful.
    /// </summary>
    public TenantContext? Context { get; }

    /// <summary>
    /// Gets the invariant code when enforcement failed.
    /// </summary>
    public string? InvariantCode { get; }

    /// <summary>
    /// Gets the trace identifier for correlation.
    /// </summary>
    public string? TraceId { get; }

    /// <summary>
    /// Gets the human-readable detail message when enforcement failed.
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Creates a successful enforcement result.
    /// </summary>
    public static EnforcementResult Success(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(
            isSuccess: true,
            context: context,
            invariantCode: null,
            traceId: context.TraceId,
            detail: null);
    }

    /// <summary>
    /// Creates a failed enforcement result.
    /// </summary>
    public static EnforcementResult Failure(
        string invariantCode,
        string traceId,
        string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);

        return new(
            isSuccess: false,
            context: null,
            invariantCode: invariantCode,
            traceId: traceId,
            detail: detail);
    }
}
