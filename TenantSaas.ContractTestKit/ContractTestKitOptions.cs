namespace TenantSaas.ContractTestKit;

/// <summary>
/// Configuration options for contract test kit assertions.
/// </summary>
public sealed class ContractTestKitOptions
{
    /// <summary>
    /// Gets or sets whether to validate all invariants are registered.
    /// Defaults to true.
    /// </summary>
    public bool ValidateAllInvariantsRegistered { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate all refusal mappings are registered.
    /// Defaults to true.
    /// </summary>
    public bool ValidateAllRefusalMappingsRegistered { get; set; } = true;

    /// <summary>
    /// Gets or sets custom invariant codes to validate beyond the standard set.
    /// </summary>
    public IReadOnlyList<string> AdditionalInvariantCodes { get; set; } = [];

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static ContractTestKitOptions Default { get; } = new();
}
