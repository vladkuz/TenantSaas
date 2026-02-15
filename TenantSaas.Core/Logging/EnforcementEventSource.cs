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
        Message = "Tenant context initialized: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}")]
    public static partial void ContextInitialized(
        ILogger logger,
        string eventName,
        string severity,
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
        Message = "Tenant context not initialized: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}")]
    public static partial void ContextNotInitialized(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode);

    /// <summary>
    /// Logs successful tenant attribution resolution.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Tenant attribution resolved: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, source={Source}")]
    public static partial void AttributionResolved(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string source);

    /// <summary>
    /// Logs ambiguous tenant attribution (refusal).
    /// </summary>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Tenant attribution ambiguous: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, conflicting_sources={ConflictingSources}")]
    public static partial void AttributionAmbiguous(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode,
        string conflictingSources);

    /// <summary>
    /// Logs tenant attribution not found (no attribution sources provided).
    /// </summary>
    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Tenant attribution not found: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}")]
    public static partial void AttributionNotFound(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode);

    /// <summary>
    /// Logs tenant attribution source not allowed (source is disabled or not permitted).
    /// </summary>
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Warning,
        Message = "Tenant attribution source not allowed: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, source={Source}")]
    public static partial void AttributionNotAllowed(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode,
        string source);

    /// <summary>
    /// Logs invariant violation.
    /// </summary>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Invariant violated: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, detail={Detail}")]
    public static partial void InvariantViolated(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode,
        string? detail);

    /// <summary>
    /// Logs refusal emission (correlation with Problem Details).
    /// </summary>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Refusal emitted: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, http_status={HttpStatus}, problem_type={ProblemType}")]
    public static partial void RefusalEmitted(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode,
        int httpStatus,
        string problemType);

    /// <summary>
    /// Logs refusal emission for non-request execution kinds (no request_id).
    /// </summary>
    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Warning,
        Message = "Refusal emitted: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, http_status={HttpStatus}, problem_type={ProblemType}")]
    public static partial void RefusalEmitted(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string executionKind,
        string scopeType,
        string invariantCode,
        int httpStatus,
        string problemType);

    /// <summary>
    /// Logs successful break-glass invocation with audit event.
    /// </summary>
    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "Break-glass invoked: event_name={EventName}, severity={Severity}, actor={Actor}, reason={Reason}, scope={Scope}, tenant_ref={TenantRef}, trace_id={TraceId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, audit_code={AuditCode}")]
    public static partial void BreakGlassInvoked(
        ILogger logger,
        string eventName,
        string severity,
        string actor,
        string reason,
        string scope,
        string tenantRef,
        string traceId,
        string executionKind,
        string scopeType,
        string auditCode);

    /// <summary>
    /// Logs denied break-glass attempt (security violation).
    /// </summary>
    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "Break-glass attempt denied: event_name={EventName}, severity={Severity}, tenant_ref={TenantRef}, trace_id={TraceId}, request_id={RequestId}, execution_kind={ExecutionKind}, scope_type={ScopeType}, invariant_code={InvariantCode}, reason={Reason}")]
    public static partial void BreakGlassAttemptDenied(
        ILogger logger,
        string eventName,
        string severity,
        string tenantRef,
        string traceId,
        string? requestId,
        string executionKind,
        string scopeType,
        string invariantCode,
        string reason);
}
