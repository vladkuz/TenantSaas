namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Represents an invariant definition in the trust contract.
/// </summary>
public sealed record InvariantDefinition
{
    /// <summary>
    /// Gets the stable invariant code identifier.
    /// </summary>
    public string InvariantCode { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the semantic description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the invariant category.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Creates a new invariant definition.
    /// </summary>
    /// <param name="invariantCode">Stable invariant code identifier.</param>
    /// <param name="name">Display name.</param>
    /// <param name="description">Semantic description.</param>
    /// <param name="category">Invariant category.</param>
    /// <exception cref="ArgumentException">Thrown when any argument is null or whitespace.</exception>
    public InvariantDefinition(
        string invariantCode,
        string name,
        string description,
        string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        InvariantCode = invariantCode;
        Name = name;
        Description = description;
        Category = category;
    }
}
