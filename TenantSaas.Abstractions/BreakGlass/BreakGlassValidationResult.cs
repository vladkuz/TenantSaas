namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Represents the result of break-glass declaration validation.
/// </summary>
public sealed record BreakGlassValidationResult(
    bool IsValid,
    string? InvariantCode,
    string? Reason)
{
    /// <summary>
    /// Creates a valid break-glass validation result.
    /// </summary>
    public static BreakGlassValidationResult Valid() => new(true, null, null);

    /// <summary>
    /// Creates an invalid break-glass validation result.
    /// </summary>
    public static BreakGlassValidationResult Invalid(string invariantCode, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode, nameof(invariantCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));

        return new BreakGlassValidationResult(false, invariantCode, reason);
    }
}
