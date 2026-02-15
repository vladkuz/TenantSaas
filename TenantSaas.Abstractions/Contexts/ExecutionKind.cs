namespace TenantSaas.Abstractions.Contexts;

/// <summary>
/// Represents how execution was initiated for a tenant context.
/// </summary>
public sealed record ExecutionKind
{
    /// <summary>
    /// Canonical value for request execution kind.
    /// </summary>
    public const string RequestValue = "request";
    /// <summary>
    /// Canonical value for background execution kind.
    /// </summary>
    public const string BackgroundValue = "background";
    /// <summary>
    /// Canonical value for admin execution kind.
    /// </summary>
    public const string AdminValue = "admin";
    /// <summary>
    /// Canonical value for scripted execution kind.
    /// </summary>
    public const string ScriptedValue = "scripted";

    private static readonly List<ExecutionKind> Registry = [];
    private static readonly StringComparer ValueComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly Lock Sync = new();

    static ExecutionKind()
    {
        Registry.AddRange(
        [
            new ExecutionKind(RequestValue, "Request", "HTTP or API request flow."),
            new ExecutionKind(BackgroundValue, "Background", "Background job or worker flow."),
            new ExecutionKind(AdminValue, "Admin", "Administrative operation flow."),
            new ExecutionKind(ScriptedValue, "Scripted", "CLI or script execution flow.")
        ]);
    }

    private ExecutionKind(string value, string displayName, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Value = value;
        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets the canonical value for the execution kind.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the display name for the execution kind.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description for the execution kind.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// HTTP or API request flow.
    /// </summary>
    public static ExecutionKind Request => FindByValue(RequestValue);

    /// <summary>
    /// Background job or worker flow.
    /// </summary>
    public static ExecutionKind Background => FindByValue(BackgroundValue);

    /// <summary>
    /// Administrative operation flow.
    /// </summary>
    public static ExecutionKind Admin => FindByValue(AdminValue);

    /// <summary>
    /// CLI or script execution flow.
    /// </summary>
    public static ExecutionKind Scripted => FindByValue(ScriptedValue);

    /// <summary>
    /// Gets the full list of execution kinds.
    /// </summary>
    public static IReadOnlyList<ExecutionKind> All
    {
        get
        {
            lock (Sync)
            {
                return [.. Registry];
            }
        }
    }

    private static ExecutionKind FindByValue(string value)
    {
        lock (Sync)
        {
            return Registry.First(kind => ValueComparer.Equals(kind.Value, value));
        }
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
