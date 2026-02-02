namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Emits break-glass audit events to a durable audit sink.
/// </summary>
/// <remarks>
/// Implementations must be idempotent, non-blocking, and fail-safe.
/// </remarks>
public interface IBreakGlassAuditSink
{
    /// <summary>
    /// Emits a break-glass audit event.
    /// </summary>
    Task EmitAsync(BreakGlassAuditEvent auditEvent, CancellationToken cancellationToken);
}
