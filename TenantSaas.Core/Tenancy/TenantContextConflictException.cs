namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Raised when tenant context initialization is attempted with conflicting inputs.
/// </summary>
public sealed class TenantContextConflictException : InvalidOperationException
{
    public TenantContextConflictException(
        string message,
        string traceId,
        string? requestId)
        : base(message)
    {
        TraceId = traceId;
        RequestId = requestId;
    }

    /// <summary>
    /// Trace identifier for correlation.
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// Request identifier when available.
    /// </summary>
    public string? RequestId { get; }
}
