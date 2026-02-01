namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Allowed categories for operating without tenant scope.
/// </summary>
public sealed record NoTenantReason
{
    private static readonly List<NoTenantReason> Registry = [];
    private static readonly StringComparer ValueComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly Lock Sync = new();

    static NoTenantReason()
    {
        Registry.AddRange(
        [
            new NoTenantReason("public", "Public", "Public or unauthenticated operations."),
            new NoTenantReason("bootstrap", "Bootstrap", "System initialization and bootstrap workflows."),
            new NoTenantReason("health-check", "HealthCheck", "Health and readiness probes."),
            new NoTenantReason("system-maintenance", "SystemMaintenance", "Maintenance operations.")
        ]);
    }

    private NoTenantReason(string value, string displayName, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Value = value;
        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets the canonical value for the reason.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the display name for the reason.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description for the reason.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Public or unauthenticated operations.
    /// </summary>
    public static NoTenantReason Public => FindByValue("public");

    /// <summary>
    /// System initialization and bootstrap workflows.
    /// </summary>
    public static NoTenantReason Bootstrap => FindByValue("bootstrap");

    /// <summary>
    /// Health and readiness probes.
    /// </summary>
    public static NoTenantReason HealthCheck => FindByValue("health-check");

    /// <summary>
    /// Maintenance operations.
    /// </summary>
    public static NoTenantReason SystemMaintenance => FindByValue("system-maintenance");

    /// <summary>
    /// Gets the full list of allowed reasons.
    /// </summary>
    public static IReadOnlyList<NoTenantReason> All
    {
        get
        {
            lock (Sync)
            {
                return [.. Registry];
            }
        }
    }

    /// <summary>
    /// Adds an explicit extension reason to the allowed set.
    /// </summary>
    /// <param name="value">Canonical value for the extension.</param>
    /// <param name="displayName">Display name for the extension.</param>
    /// <param name="description">Description of the extension.</param>
    /// <exception cref="InvalidOperationException">Thrown when a reason with the same value already exists.</exception>
    public static NoTenantReason RegisterExtension(string value, string displayName, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        lock (Sync)
        {
            if (Registry.Any(reason => ValueComparer.Equals(reason.Value, value)))
            {
                throw new InvalidOperationException($"No-tenant reason '{value}' is already registered.");
            }

            var extension = new NoTenantReason(value, displayName, description);
            Registry.Add(extension);
            return extension;
        }
    }

    private static NoTenantReason FindByValue(string value)
    {
        lock (Sync)
        {
            return Registry.First(reason => ValueComparer.Equals(reason.Value, value));
        }
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
