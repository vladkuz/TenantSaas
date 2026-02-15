using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.Disclosure;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Core.Logging;

/// <summary>
/// Default implementation of log enricher following disclosure policy from Story 2.5.
/// Resolves safe tenant_ref values and extracts correlation IDs from context.
/// </summary>
public sealed class DefaultLogEnricher : ILogEnricher
{
    private const string UnknownScopeType = "Unknown";
    /// <summary>
    /// Known successful event names that should be logged at Information level.
    /// Using explicit allow-list to avoid false positives from substring matching.
    /// </summary>
    private static readonly HashSet<string> SuccessEventNames = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(EnforcementEventSource.ContextInitialized),
        nameof(EnforcementEventSource.AttributionResolved),
        EnforcementEventNames.BreakGlassApproved
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
    /// For tenant scope, returns an opaque, non-reversible identifier.
    /// </summary>
    private static string GetSafeTenantRef(TenantScope scope)
    {
        return scope switch
        {
            // NoTenant scope → safe-state token "unknown"
            TenantScope.NoTenant => TenantRefSafeState.Unknown,

            // SharedSystem scope → safe-state token "cross_tenant"
            TenantScope.SharedSystem => TenantRefSafeState.CrossTenant,

            // Tenant scope → opaque public ID (non-reversible hash of tenant identifier)
            TenantScope.Tenant tenant => ToOpaqueTenantRef(tenant.Id.Value),

            // Unknown scope type → safe-state token "unknown"
            _ => TenantRefSafeState.Unknown
        };
    }

    internal static string ToOpaqueTenantRef(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var bytes = Encoding.UTF8.GetBytes(tenantId);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        return $"{TenantRefSafeState.Opaque}:{hash}";
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
            return LogLevel.Warning.ToString();
        }

        // Explicit success events → Information
        if (SuccessEventNames.Contains(eventName))
        {
            return LogLevel.Information.ToString();
        }

        // Unknown events default to Warning (safer than Information for audit purposes)
        return LogLevel.Warning.ToString();
    }

    /// <summary>
    /// Maps TenantScope to string representation for logging.
    /// </summary>
    private static string GetScopeType(TenantScope scope)
    {
        return scope switch
        {
            TenantScope.Tenant => nameof(TenantScope.Tenant),
            TenantScope.NoTenant => nameof(TenantScope.NoTenant),
            TenantScope.SharedSystem => nameof(TenantScope.SharedSystem),
            _ => UnknownScopeType
        };
    }
}
