namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Represents the result of disclosure validation.
/// </summary>
public sealed record DisclosureValidationResult(
    bool IsValid,
    string? InvariantCode,
    string? Reason)
{
    /// <summary>
    /// Creates a valid disclosure validation result.
    /// </summary>
    public static DisclosureValidationResult Valid() => new(true, null, null);

    /// <summary>
    /// Creates an invalid disclosure validation result.
    /// </summary>
    public static DisclosureValidationResult Invalid(string invariantCode, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode, nameof(invariantCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));

        return new DisclosureValidationResult(false, invariantCode, reason);
    }
}
