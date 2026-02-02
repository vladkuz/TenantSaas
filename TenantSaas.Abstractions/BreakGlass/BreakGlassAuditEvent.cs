using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Represents a standard audit event emitted when break-glass is exercised.
/// </summary>
public sealed record BreakGlassAuditEvent
{
    /// <summary>
    /// Creates a break-glass audit event.
    /// </summary>
    [SetsRequiredMembers]
    public BreakGlassAuditEvent(
        string actor,
        string reason,
        string scope,
        string tenantRef,
        string traceId,
        string auditCode,
        string? invariantCode,
        DateTimeOffset timestamp,
        string? operationName)
    {
        Actor = actor;
        Reason = reason;
        Scope = scope;
        TenantRef = tenantRef;
        TraceId = traceId;
        AuditCode = auditCode;
        InvariantCode = invariantCode;
        Timestamp = timestamp;
        OperationName = operationName;
    }

    /// <summary>
    /// Gets the actor identity.
    /// </summary>
    [JsonPropertyName("actor")]
    public required string Actor
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Actor));
            field = value;
        }
    }

    /// <summary>
    /// Gets the reason for escalation.
    /// </summary>
    [JsonPropertyName("reason")]
    public required string Reason
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Reason));
            field = value;
        }
    }

    /// <summary>
    /// Gets the declared scope.
    /// </summary>
    [JsonPropertyName("scope")]
    public required string Scope
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Scope));
            field = value;
        }
    }

    /// <summary>
    /// Gets the tenant reference or cross-tenant marker.
    /// </summary>
    [JsonPropertyName("tenantRef")]
    public required string TenantRef
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(TenantRef));
            field = value;
        }
    }

    /// <summary>
    /// Gets the correlation trace identifier.
    /// </summary>
    [JsonPropertyName("traceId")]
    public required string TraceId
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(TraceId));
            field = value;
        }
    }

    /// <summary>
    /// Gets the stable audit event type code.
    /// </summary>
    [JsonPropertyName("auditCode")]
    public required string AuditCode
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(AuditCode));
            field = value;
        }
    }

    /// <summary>
    /// Gets the invariant being bypassed, if applicable.
    /// </summary>
    [JsonPropertyName("invariantCode")]
    public string? InvariantCode { get; init; }

    /// <summary>
    /// Gets the event timestamp (UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp
    {
        get => field;
        init
        {
            if (value.Offset != TimeSpan.Zero)
            {
                throw new ArgumentException("Break-glass audit event timestamp must be UTC.", nameof(Timestamp));
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the operation being performed, if applicable.
    /// </summary>
    [JsonPropertyName("operationName")]
    public string? OperationName { get; init; }

    /// <summary>
    /// Creates an audit event from a break-glass declaration.
    /// </summary>
    /// <param name="declaration">The break-glass declaration.</param>
    /// <param name="traceId">The correlation trace identifier.</param>
    /// <param name="auditCode">The audit code (defaults to BreakGlassInvoked).</param>
    public static BreakGlassAuditEvent Create(
        BreakGlassDeclaration declaration,
        string traceId,
        string auditCode = global::TenantSaas.Abstractions.BreakGlass.AuditCode.BreakGlassInvoked)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(auditCode);

        var tenantRef = string.IsNullOrWhiteSpace(declaration.TargetTenantRef)
            ? TrustContractV1.BreakGlassMarkerCrossTenant
            : declaration.TargetTenantRef;

        return new BreakGlassAuditEvent(
            actor: declaration.ActorId,
            reason: declaration.Reason,
            scope: declaration.DeclaredScope,
            tenantRef: tenantRef,
            traceId: traceId,
            auditCode: auditCode,
            invariantCode: null,
            timestamp: DateTimeOffset.UtcNow,
            operationName: null);
    }
}
