namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Represents a disclosure-safe tenant reference value.
/// </summary>
public sealed record TenantRef
{
    /// <summary>
    /// Creates a tenant reference with the specified value.
    /// </summary>
    public TenantRef(string value, bool isSafeState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
        IsSafeState = isSafeState;
    }

    /// <summary>
    /// Gets the safe tenant reference value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether this reference is a safe-state token.
    /// </summary>
    public bool IsSafeState { get; }

    /// <summary>
    /// Creates a tenant reference for unknown state.
    /// </summary>
    public static TenantRef ForUnknown()
        => new(TenantRefSafeState.Unknown, isSafeState: true);

    /// <summary>
    /// Creates a tenant reference for sensitive state.
    /// </summary>
    public static TenantRef ForSensitive()
        => new(TenantRefSafeState.Sensitive, isSafeState: true);

    /// <summary>
    /// Creates a tenant reference for cross-tenant operations.
    /// </summary>
    public static TenantRef ForCrossTenant()
        => new(TenantRefSafeState.CrossTenant, isSafeState: true);

    /// <summary>
    /// Creates a tenant reference for an opaque identifier.
    /// </summary>
    public static TenantRef ForOpaque(string opaqueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opaqueId);
        return new TenantRef(opaqueId, isSafeState: false);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
