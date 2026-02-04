using TenantSaas.Abstractions.Contexts;

namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Captures tenant scope and execution metadata for enforcement decisions.
/// </summary>
public sealed record TenantContext
{
    private TenantContext(
        TenantScope scope,
        ExecutionKind executionKind,
        string traceId,
        string? requestId,
        TenantAttributionInputs attributionInputs,
        DateTimeOffset initializedAt)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(executionKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentNullException.ThrowIfNull(attributionInputs);

        if (executionKind == ExecutionKind.Request)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        }

        Scope = scope;
        ExecutionKind = executionKind;
        TraceId = traceId;
        RequestId = requestId;
        AttributionInputs = attributionInputs;
        InitializedAt = EnsureUtc(initializedAt);
    }

    /// <summary>
    /// Gets the tenant scope for the execution.
    /// </summary>
    public TenantScope Scope { get; }

    /// <summary>
    /// Gets the execution kind for the flow.
    /// </summary>
    public ExecutionKind ExecutionKind { get; }

    /// <summary>
    /// Gets the correlation trace identifier.
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// Gets the request identifier when execution was initiated by a request.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Gets the attribution inputs captured during initialization.
    /// </summary>
    public TenantAttributionInputs AttributionInputs { get; }

    /// <summary>
    /// Gets the UTC timestamp when the context was initialized.
    /// </summary>
    public DateTimeOffset InitializedAt { get; }

    /// <summary>
    /// Creates a context for an HTTP or API request flow.
    /// </summary>
    public static TenantContext ForRequest(TenantScope scope, string traceId, string requestId)
        => new(
            scope,
            ExecutionKind.Request,
            traceId,
            requestId,
            TenantAttributionInputs.None(),
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for an HTTP or API request flow with attribution inputs.
    /// </summary>
    public static TenantContext ForRequest(
        TenantScope scope,
        string traceId,
        string requestId,
        TenantAttributionInputs attributionInputs)
        => new(
            scope,
            ExecutionKind.Request,
            traceId,
            requestId,
            attributionInputs,
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for a background job or worker flow.
    /// </summary>
    public static TenantContext ForBackground(TenantScope scope, string traceId)
        => new(
            scope,
            ExecutionKind.Background,
            traceId,
            requestId: null,
            TenantAttributionInputs.None(),
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for a background job or worker flow with attribution inputs.
    /// </summary>
    public static TenantContext ForBackground(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs attributionInputs)
        => new(
            scope,
            ExecutionKind.Background,
            traceId,
            requestId: null,
            attributionInputs,
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for an administrative operation flow.
    /// </summary>
    public static TenantContext ForAdmin(TenantScope scope, string traceId)
        => new(
            scope,
            ExecutionKind.Admin,
            traceId,
            requestId: null,
            TenantAttributionInputs.None(),
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for an administrative operation flow with attribution inputs.
    /// </summary>
    public static TenantContext ForAdmin(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs attributionInputs)
        => new(
            scope,
            ExecutionKind.Admin,
            traceId,
            requestId: null,
            attributionInputs,
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for a CLI or script execution flow.
    /// </summary>
    public static TenantContext ForScripted(TenantScope scope, string traceId)
        => new(
            scope,
            ExecutionKind.Scripted,
            traceId,
            requestId: null,
            TenantAttributionInputs.None(),
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a context for a CLI or script execution flow with attribution inputs.
    /// </summary>
    public static TenantContext ForScripted(
        TenantScope scope,
        string traceId,
        TenantAttributionInputs attributionInputs)
        => new(
            scope,
            ExecutionKind.Scripted,
            traceId,
            requestId: null,
            attributionInputs,
            DateTimeOffset.UtcNow);

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        if (timestamp.Offset != TimeSpan.Zero)
        {
            return timestamp.ToUniversalTime();
        }

        return timestamp;
    }
}
