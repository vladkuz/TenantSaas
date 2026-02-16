using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Helper for creating break-glass audit events from declarations.
/// </summary>
public static class BreakGlassAuditHelper
{
    /// <summary>
    /// Creates a break-glass audit event from a declaration.
    /// </summary>
    /// <param name="declaration">Break-glass declaration with actor, reason, and scope.</param>
    /// <param name="traceId">Correlation trace identifier.</param>
    /// <param name="invariantCode">Optional invariant being bypassed.</param>
    /// <param name="operationName">Optional operation being performed.</param>
    /// <returns>Audit event ready for emission.</returns>
    /// <exception cref="ArgumentNullException">Thrown when declaration or traceId is null/empty.</exception>
    public static BreakGlassAuditEvent CreateAuditEvent(
        BreakGlassDeclaration declaration,
        string traceId,
        string? invariantCode = null,
        string? operationName = null,
        string? tenantRefOverride = null)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var tenantRef = tenantRefOverride
            ?? declaration.TargetTenantRef
            ?? TrustContractV1.BreakGlassMarkerCrossTenant;

        return new BreakGlassAuditEvent(
            actor: declaration.ActorId,
            reason: declaration.Reason,
            scope: declaration.DeclaredScope,
            tenantRef: tenantRef,
            traceId: traceId,
            auditCode: AuditCode.BreakGlassInvoked,
            invariantCode: invariantCode,
            timestamp: DateTimeOffset.UtcNow,
            operationName: operationName);
    }
}
