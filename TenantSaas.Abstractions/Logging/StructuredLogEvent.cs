namespace TenantSaas.Abstractions.Logging;

/// <summary>
/// Structured log event model containing required fields for audit trail and correlation.
/// All enforcement decisions and refusals must emit events with this structure.
/// </summary>
public sealed record StructuredLogEvent
{
    /// <summary>
    /// Disclosure-safe tenant identifier. Either an opaque public ID or a safe-state token
    /// (unknown, sensitive, cross_tenant) per disclosure policy from Story 2.5.
    /// Never log raw tenant IDs that could be reversed or enumerated.
    /// </summary>
    public required string TenantRef { get; init; }

    /// <summary>
    /// End-to-end correlation ID. Flows across service boundaries.
    /// Used to join logs with Problem Details responses.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Request-scoped correlation ID. Only present for request execution kind.
    /// Used to join logs within a single HTTP request scope.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Invariant code from InvariantCode registry. Present for violations and refusals.
    /// Enables correlation with Problem Details and error catalog.
    /// </summary>
    public string? InvariantCode { get; init; }

    /// <summary>
    /// Event type identifier (e.g., "ContextInitialized", "RefusalEmitted").
    /// Used for filtering and aggregation in log analysis.
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Log severity level: Information, Warning, Error.
    /// Determines alerting and escalation behavior.
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Execution kind: request, background, admin, scripted.
    /// Indicates the operational context of the event. Always present for enforcement logs.
    /// </summary>
    public required string ExecutionKind { get; init; }

    /// <summary>
    /// Scope type: Tenant, NoTenant, SharedSystem.
    /// Indicates the tenant scope context of the event. Always present for enforcement logs.
    /// </summary>
    public required string ScopeType { get; init; }

    /// <summary>
    /// Human-readable detail message. Optional context for operators.
    /// Must not contain PII, secrets, or sensitive data.
    /// </summary>
    public string? Detail { get; init; }
}
