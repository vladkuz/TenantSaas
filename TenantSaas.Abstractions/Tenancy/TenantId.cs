namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Strongly-typed tenant identifier to avoid stringly-typed mistakes.
/// </summary>
public readonly record struct TenantId
{
    /// <summary>
    /// Creates a new tenant identifier.
    /// </summary>
    /// <param name="value">Tenant identifier value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public TenantId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    /// Gets the tenant identifier value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
