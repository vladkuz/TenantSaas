using Microsoft.Extensions.Logging;

namespace TenantSaas.Core.Logging;

/// <summary>
/// High-performance enforcement event logging using LoggerMessage source generators.
/// Provides zero-allocation structured logging for all enforcement decisions.
/// </summary>
public static partial class EnforcementEventSource
{
    /// <summary>
    /// Logs successful tenant context initialization.
    /// </summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Tenant context initialized: tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}")]
    public static partial void ContextInitialized(
        ILogger logger,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType);

    /// <summary>
    /// Logs failed tenant context initialization (refusal).
    /// </summary>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Tenant context not initialized: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}")]
    public static partial void ContextNotInitialized(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode);

    /// <summary>
    /// Logs successful tenant attribution resolution.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Tenant attribution resolved: tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, source={Source}")]
    public static partial void AttributionResolved(
        ILogger logger,
        string tenantRef,
        string traceId,
        string? requestId,
        string source);

    /// <summary>
    /// Logs ambiguous tenant attribution (refusal).
    /// </summary>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Tenant attribution ambiguous: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}, conflicting_sources={ConflictingSources}")]
    public static partial void AttributionAmbiguous(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode,
        string conflictingSources);

    /// <summary>
    /// Logs tenant attribution not found (no attribution sources provided).
    /// </summary>
    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Tenant attribution not found: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}")]
    public static partial void AttributionNotFound(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode);

    /// <summary>
    /// Logs tenant attribution source not allowed (source is disabled or not permitted).
    /// </summary>
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Warning,
        Message = "Tenant attribution source not allowed: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}, source={Source}")]
    public static partial void AttributionNotAllowed(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode,
        string source);

    /// <summary>
    /// Logs invariant violation.
    /// </summary>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Invariant violated: tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}, detail={Detail}")]
    public static partial void InvariantViolated(
        ILogger logger,
        string tenantRef,
        string traceId,
        string? requestId,
        string invariantCode,
        string? detail);

    /// <summary>
    /// Logs refusal emission (correlation with Problem Details).
    /// </summary>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Refusal emitted: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}, http_status={HttpStatus}, problem_type={ProblemType}")]
    public static partial void RefusalEmitted(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode,
        int httpStatus,
        string problemType);

    /// <summary>
    /// Logs refusal emission for non-request execution kinds (no request_id).
    /// </summary>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Refusal emitted: trace_id={TraceId}, invariant_code={InvariantCode}, http_status={HttpStatus}, problem_type={ProblemType}")]
    public static partial void RefusalEmitted(
        ILogger logger,
        string traceId,
        string invariantCode,
        int httpStatus,
        string problemType);

    /// <summary>
    /// Logs successful break-glass invocation with audit event.
    /// </summary>
    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "Break-glass invoked: actor={Actor}, reason={Reason}, scope={Scope}, tenant_ref={TenantRef}, trace_id={TraceId}, audit_code={AuditCode}")]
    public static partial void BreakGlassInvoked(
        ILogger logger,
        string actor,
        string reason,
        string scope,
        string tenantRef,
        string traceId,
        string auditCode);

    /// <summary>
    /// Logs denied break-glass attempt (security violation).
    /// </summary>
    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "Break-glass attempt denied: trace_id={TraceId}, request_id={RequestId}, invariant_code={InvariantCode}, reason={Reason}")]
    public static partial void BreakGlassAttemptDenied(
        ILogger logger,
        string traceId,
        string? requestId,
        string invariantCode,
        string reason);
}
