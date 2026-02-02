namespace TenantSaas.Core.Errors;

/// <summary>
/// Standard Problem Details extension keys per RFC 7807.
/// </summary>
/// <remarks>
/// These extension keys are stable within a major version and form part of the API contract.
/// Use these constants when reading or writing Problem Details extensions to ensure consistency.
/// </remarks>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Problem Details extension key for invariant code.
    /// </summary>
    /// <remarks>
    /// Value type: string (matches InvariantCode constants).
    /// Always present in enforcement failures.
    /// </remarks>
    public const string InvariantCodeKey = "invariant_code";

    /// <summary>
    /// Problem Details extension key for trace identifier.
    /// </summary>
    /// <remarks>
    /// Value type: string.
    /// Always present in enforcement failures for correlation across distributed systems.
    /// </remarks>
    public const string TraceId = "trace_id";

    /// <summary>
    /// Problem Details extension key for request identifier.
    /// </summary>
    /// <remarks>
    /// Value type: string.
    /// Present in enforcement failures during request execution (not background/admin/scripted flows).
    /// </remarks>
    public const string RequestId = "request_id";

    /// <summary>
    /// Problem Details extension key for guidance URI.
    /// </summary>
    /// <remarks>
    /// Value type: string (URI).
    /// Present when RefusalMapping defines a guidance link for the invariant violation.
    /// </remarks>
    public const string GuidanceLink = "guidance_link";

    /// <summary>
    /// Problem Details extension key for conflicting attribution sources.
    /// </summary>
    /// <remarks>
    /// Value type: IReadOnlyList&lt;string&gt; (source display names).
    /// Present only for TenantAttributionUnambiguous failures with multiple conflicting sources.
    /// </remarks>
    public const string ConflictingSources = "conflicting_sources";
}
