using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Logging;

/// <summary>
/// Default implementation of log enricher following disclosure policy from Story 2.5.
/// Resolves safe tenant_ref values and extracts correlation IDs from context.
/// </summary>
public sealed class DefaultLogEnricher : ILogEnricher
{
    /// <summary>
    /// Known successful event names that should be logged at Information level.
    /// Using explicit allow-list to avoid false positives from substring matching.
    /// </summary>
    private static readonly HashSet<string> SuccessEventNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ContextInitialized",
        "AttributionResolved",
        "BreakGlassApproved"
    };

    /// <inheritdoc/>
    public StructuredLogEvent Enrich(
        TenantContext context,
        string eventName,
        string? invariantCode = null,
        string? detail = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        return new StructuredLogEvent
        {
            TenantRef = GetSafeTenantRef(context.Scope),
            TraceId = context.TraceId,
            RequestId = context.RequestId,
            InvariantCode = invariantCode,
            EventName = eventName,
            Severity = DetermineSeverity(eventName, invariantCode),
            ExecutionKind = context.ExecutionKind.Value,
            ScopeType = GetScopeType(context.Scope),
            Detail = detail
        };
    }

    /// <summary>
    /// Resolves disclosure-safe tenant_ref per Story 2.5 policy.
    /// Returns safe-state tokens (unknown, sensitive, cross_tenant) when tenant ID is unsafe to disclose.
    /// Never returns raw tenant IDs that could be reversed or enumerated.
    /// </summary>
    private static string GetSafeTenantRef(TenantScope scope)
    {
        return scope switch
        {
            // NoTenant scope → safe-state token "unknown"
            TenantScope.NoTenant => "unknown",

            // SharedSystem scope → safe-state token "cross_tenant"
            TenantScope.SharedSystem => "cross_tenant",

            // Tenant scope → opaque public ID (for now, using Value; future: hash or public ID mapping)
            TenantScope.Tenant tenant => tenant.Id.Value,

            // Unknown scope type → safe-state token "unknown"
            _ => "unknown"
        };
    }

    /// <summary>
    /// Determines severity based on event type and presence of invariant code.
    /// Uses explicit allow-list for success events to avoid false positives.
    /// </summary>
    private static string DetermineSeverity(string eventName, string? invariantCode)
    {
        // Invariant violations or refusals → Warning severity
        if (invariantCode is not null)
        {
            return "Warning";
        }

        // Explicit success events → Information
        if (SuccessEventNames.Contains(eventName))
        {
            return "Information";
        }

        // Unknown events default to Warning (safer than Information for audit purposes)
        return "Warning";
    }

    /// <summary>
    /// Maps TenantScope to string representation for logging.
    /// </summary>
    private static string GetScopeType(TenantScope scope)
    {
        return scope switch
        {
            TenantScope.Tenant => "Tenant",
            TenantScope.NoTenant => "NoTenant",
            TenantScope.SharedSystem => "SharedSystem",
            _ => "Unknown"
        };
    }
}
