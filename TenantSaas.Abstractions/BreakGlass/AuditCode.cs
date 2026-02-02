namespace TenantSaas.Abstractions.BreakGlass;

/// <summary>
/// Defines stable audit event codes for break-glass operations.
/// </summary>
public static class AuditCode
{
    /// <summary>
    /// Break-glass was successfully invoked.
    /// </summary>
    public const string BreakGlassInvoked = "BreakGlassInvoked";

    /// <summary>
    /// Break-glass attempt was rejected.
    /// </summary>
    public const string BreakGlassAttemptDenied = "BreakGlassAttemptDenied";

    /// <summary>
    /// Cross-tenant operation was performed.
    /// </summary>
    public const string CrossTenantAccess = "CrossTenantAccess";

    /// <summary>
    /// Privilege escalation occurred.
    /// </summary>
    public const string PrivilegedEscalation = "PrivilegedEscalation";
}
