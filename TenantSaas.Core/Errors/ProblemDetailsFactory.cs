using Microsoft.AspNetCore.Mvc;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.Core.Errors;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses with invariant violations.
/// </summary>
/// <remarks>
/// This factory provides a centralized, stable contract for all error responses in the system.
/// All Problem Details responses include standard extension fields (invariant_code, trace_id)
/// and follow RFC 7807 structure with stable type URIs.
/// </remarks>
public static class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a Problem Details response for an invariant violation.
    /// </summary>
    /// <param name="invariantCode">The invariant code from <see cref="InvariantCode"/>.</param>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier (required for request execution).</param>
    /// <param name="detail">Human-readable detail message (falls back to RefusalMapping description).</param>
    /// <param name="extensions">Additional extension fields to include.</param>
    /// <returns>A fully populated Problem Details response.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when invariantCode is not registered in RefusalMappings.</exception>
    public static ProblemDetails FromInvariantViolation(
        string invariantCode,
        string traceId,
        string? requestId = null,
        string? detail = null,
        IDictionary<string, object?>? extensions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var mapping = TrustContractV1.RefusalMappings[invariantCode];
        var invariantDefinition = TrustContractV1.Invariants[invariantCode];

        var problemDetails = new ProblemDetails
        {
            Type = mapping.ProblemType,
            Title = mapping.Title,
            Status = mapping.HttpStatusCode,
            Detail = detail ?? invariantDefinition.Description,
            Instance = null
        };

        problemDetails.Extensions[InvariantCodeKey] = invariantCode;
        problemDetails.Extensions[TraceId] = traceId;

        if (requestId is not null)
        {
            problemDetails.Extensions[RequestId] = requestId;
        }

        if (mapping.GuidanceUri is not null)
        {
            problemDetails.Extensions[GuidanceLink] = mapping.GuidanceUri;
        }

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }

    /// <summary>
    /// Creates a Problem Details response with safe tenant reference inclusion.
    /// </summary>
    /// <param name="invariantCode">The invariant code from <see cref="InvariantCode"/>.</param>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <param name="detail">Human-readable detail message.</param>
    /// <param name="tenantRef">Disclosure-safe tenant reference (safe-state value or opaque ID).</param>
    /// <param name="extensions">Additional extension fields.</param>
    /// <returns>A fully populated Problem Details response with tenant_ref extension.</returns>
    /// <remarks>
    /// Only include tenant_ref when disclosure is safe per policy (safe-state values or opaque IDs).
    /// Never include actual tenant IDs that could leak sensitive information.
    /// </remarks>
    public static ProblemDetails FromInvariantViolation(
        string invariantCode,
        string traceId,
        string? requestId,
        string? detail,
        string? tenantRef,
        IDictionary<string, object?>? extensions = null)
    {
        var baseExtensions = new Dictionary<string, object?>();

        if (tenantRef is not null)
        {
            baseExtensions["tenant_ref"] = tenantRef;
        }

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                baseExtensions[key] = value;
            }
        }

        return FromInvariantViolation(
            invariantCode,
            traceId,
            requestId,
            detail,
            baseExtensions);
    }

    /// <summary>
    /// Creates a Problem Details response for missing tenant context.
    /// </summary>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <returns>HTTP 401 Problem Details response.</returns>
    public static ProblemDetails ForContextNotInitialized(
        string traceId,
        string? requestId = null)
        => FromInvariantViolation(
            InvariantCode.ContextInitialized,
            traceId,
            requestId,
            detail: "Tenant context must be initialized before operations can proceed.");

    /// <summary>
    /// Creates a Problem Details response for ambiguous tenant attribution.
    /// </summary>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <param name="conflictingSources">List of conflicting attribution sources.</param>
    /// <returns>HTTP 422 Problem Details response.</returns>
    public static ProblemDetails ForTenantAttributionAmbiguous(
        string traceId,
        string? requestId = null,
        IReadOnlyList<string>? conflictingSources = null)
    {
        var extensions = conflictingSources is { Count: > 0 }
            ? new Dictionary<string, object?> { [ConflictingSources] = conflictingSources }
            : null;

        return FromInvariantViolation(
            InvariantCode.TenantAttributionUnambiguous,
            traceId,
            requestId,
            detail: "Tenant attribution from available sources is ambiguous.",
            extensions: extensions);
    }

    /// <summary>
    /// Creates a Problem Details response for missing tenant scope.
    /// </summary>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <returns>HTTP 403 Problem Details response.</returns>
    public static ProblemDetails ForTenantScopeRequired(
        string traceId,
        string? requestId = null)
        => FromInvariantViolation(
            InvariantCode.TenantScopeRequired,
            traceId,
            requestId,
            detail: "Operation requires an explicit tenant scope.");

    /// <summary>
    /// Creates a Problem Details response for missing or invalid break-glass declaration.
    /// </summary>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <param name="reason">Specific reason why break-glass is required or why validation failed.</param>
    /// <returns>HTTP 403 Problem Details response.</returns>
    public static ProblemDetails ForBreakGlassRequired(
        string traceId,
        string? requestId = null,
        string? reason = null)
        => FromInvariantViolation(
            InvariantCode.BreakGlassExplicitAndAudited,
            traceId,
            requestId,
            detail: reason ?? "Break-glass must be explicitly declared with actor identity and reason.");

    /// <summary>
    /// Creates a Problem Details response for unsafe disclosure.
    /// </summary>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <returns>HTTP 500 Problem Details response.</returns>
    public static ProblemDetails ForDisclosureUnsafe(
        string traceId,
        string? requestId = null)
        => FromInvariantViolation(
            InvariantCode.DisclosureSafe,
            traceId,
            requestId,
            detail: "Tenant information disclosure must follow safe disclosure policy.");
}
