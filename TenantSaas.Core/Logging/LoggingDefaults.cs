namespace TenantSaas.Core.Logging;

/// <summary>
/// Shared fallback values for logging when expected fields cannot be extracted.
/// </summary>
public static class LoggingDefaults
{
    /// <summary>Fallback value when invariant code cannot be extracted.</summary>
    public const string UnknownInvariantCode = "Unknown";

    /// <summary>Fallback value when problem type cannot be extracted.</summary>
    public const string UnknownProblemType = "unknown";

    /// <summary>Fallback value when execution kind cannot be determined.</summary>
    public const string UnknownExecutionKind = "unknown";

    /// <summary>Fallback value when scope type cannot be determined.</summary>
    public const string UnknownScopeType = "Unknown";
}
