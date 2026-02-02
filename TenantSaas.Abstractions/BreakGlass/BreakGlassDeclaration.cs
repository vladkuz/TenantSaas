using System.Diagnostics.CodeAnalysis;

namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Represents an explicit break-glass declaration for privileged operations.
/// </summary>
/// <remarks>
/// Break-glass must always be explicit and never implicit or default.
/// Missing or incomplete declarations result in refusal.
/// </remarks>
public sealed record BreakGlassDeclaration
{
    /// <summary>
    /// Creates a break-glass declaration with required details.
    /// </summary>
    [SetsRequiredMembers]
    public BreakGlassDeclaration(
        string actorId,
        string reason,
        string declaredScope,
        string? targetTenantRef,
        DateTimeOffset timestamp)
    {
        ActorId = actorId;
        Reason = reason;
        DeclaredScope = declaredScope;
        TargetTenantRef = targetTenantRef;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the actor identity invoking break-glass.
    /// </summary>
    public required string ActorId
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(ActorId));
            field = value;
        }
    }

    /// <summary>
    /// Gets the justification for the escalation.
    /// </summary>
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
    /// Gets the scope being claimed.
    /// </summary>
    public required string DeclaredScope
    {
        get => field;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(DeclaredScope));
            field = value;
        }
    }

    /// <summary>
    /// Gets the target tenant reference, or null for cross-tenant.
    /// </summary>
    public string? TargetTenantRef { get; init; }

    /// <summary>
    /// Gets the timestamp when break-glass was declared (UTC).
    /// </summary>
    public required DateTimeOffset Timestamp
    {
        get => field;
        init
        {
            if (value.Offset != TimeSpan.Zero)
            {
                throw new ArgumentException("Break-glass timestamp must be UTC.", nameof(Timestamp));
            }

            field = value;
        }
    }
}
