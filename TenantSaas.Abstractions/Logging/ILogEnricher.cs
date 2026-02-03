using TenantSaas.Abstractions.Tenancy;

namespace TenantSaas.Abstractions.Logging;

/// <summary>
/// Extension seam for enriching log events with structured context fields.
/// Provides a hook for custom correlation ID extraction and safe tenant ref resolution.
/// </summary>
public interface ILogEnricher
{
    /// <summary>
    /// Enriches a log event with structured fields from tenant context.
    /// </summary>
    /// <param name="context">Tenant context containing trace_id, request_id, scope details.</param>
    /// <param name="eventName">Event type identifier (e.g., "ContextInitialized").</param>
    /// <param name="invariantCode">Optional invariant code for violations/refusals.</param>
    /// <param name="detail">Optional human-readable detail message.</param>
    /// <returns>Structured log event with required and optional fields populated.</returns>
    /// <remarks>
    /// Implementation must follow disclosure policy (Story 2.5) when resolving tenant_ref.
    /// tenant_ref must be disclosure-safe: opaque public ID or safe-state token (unknown, sensitive, cross_tenant).
    /// Never log raw tenant IDs that could be reversed or enumerated.
    /// </remarks>
    StructuredLogEvent Enrich(
        TenantContext context,
        string eventName,
        string? invariantCode = null,
        string? detail = null);
}
