using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.Core.Enforcement;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.Core.Errors;

/// <summary>
/// Creates Problem Details from enforcement results.
/// </summary>
public static class EnforcementProblemDetails
{
    /// <summary>
    /// Converts an enforcement failure to Problem Details.
    /// </summary>
    public static ProblemDetails FromEnforcementResult(
        EnforcementResult result,
        HttpContext? context = null,
        IReadOnlyList<string>? conflictingSources = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot create Problem Details from successful enforcement.");
        }

        var mapping = TrustContractV1.RefusalMappings[result.InvariantCode!];

        var problemDetails = new ProblemDetails
        {
            Type = mapping.ProblemType,
            Title = mapping.Title,
            Status = mapping.HttpStatusCode,
            Detail = result.Detail,
            Instance = context?.Request.Path
        };

        problemDetails.Extensions[InvariantCodeKey] = result.InvariantCode;
        problemDetails.Extensions[TraceId] = result.TraceId;

        if (context is not null && !string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            problemDetails.Extensions[RequestId] = context.TraceIdentifier;
        }

        if (mapping.GuidanceUri is not null)
        {
            problemDetails.Extensions[GuidanceLink] = mapping.GuidanceUri;
        }

        if (result.InvariantCode == Abstractions.Invariants.InvariantCode.TenantAttributionUnambiguous
            && conflictingSources is { Count: > 0 })
        {
            problemDetails.Extensions[ConflictingSources] = conflictingSources;
        }

        return problemDetails;
    }

    /// <summary>
    /// Converts an attribution enforcement failure to Problem Details.
    /// </summary>
    public static ProblemDetails FromAttributionEnforcementResult(
        AttributionEnforcementResult result,
        HttpContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot create Problem Details from successful enforcement.");
        }

        var mapping = TrustContractV1.RefusalMappings[result.InvariantCode!];

        var problemDetails = new ProblemDetails
        {
            Type = mapping.ProblemType,
            Title = mapping.Title,
            Status = mapping.HttpStatusCode,
            Detail = result.Detail,
            Instance = context?.Request.Path
        };

        problemDetails.Extensions[InvariantCodeKey] = result.InvariantCode;
        problemDetails.Extensions[TraceId] = result.TraceId;

        if (context is not null && !string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            problemDetails.Extensions[RequestId] = context.TraceIdentifier;
        }

        if (mapping.GuidanceUri is not null)
        {
            problemDetails.Extensions[GuidanceLink] = mapping.GuidanceUri;
        }

        if (result.ConflictingSources is { Count: > 0 })
        {
            problemDetails.Extensions[ConflictingSources] = result.ConflictingSources;
        }

        return problemDetails;
    }
}
