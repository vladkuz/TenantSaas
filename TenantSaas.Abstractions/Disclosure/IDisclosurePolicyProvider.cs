namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Provides the active disclosure policy.
/// </summary>
public interface IDisclosurePolicyProvider
{
    /// <summary>
    /// Gets the active disclosure policy.
    /// </summary>
    /// <remarks>
    /// Implementations may vary by environment or configuration.
    /// </remarks>
    DisclosurePolicy GetPolicy();
}
