using TenantSaas.Abstractions.BreakGlass;

namespace TenantSaas.ContractTests.TestUtilities;

/// <summary>
/// Test helper that captures break-glass audit events for verification.
/// </summary>
public sealed class CaptureAuditSink : IBreakGlassAuditSink
{
    /// <summary>
    /// The most recently emitted audit event.
    /// </summary>
    public BreakGlassAuditEvent? Event { get; private set; }

    /// <summary>
    /// Total number of audit events emitted.
    /// </summary>
    public int EmitCount { get; private set; }

    public Task EmitAsync(BreakGlassAuditEvent auditEvent, CancellationToken cancellationToken)
    {
        EmitCount++;
        Event = auditEvent;
        return Task.CompletedTask;
    }
}
