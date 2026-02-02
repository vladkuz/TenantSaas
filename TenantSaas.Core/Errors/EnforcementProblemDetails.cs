using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.Core.Enforcement;

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

        problemDetails.Extensions["invariant_code"] = result.InvariantCode;
        problemDetails.Extensions["trace_id"] = result.TraceId;

        if (context is not null && !string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            problemDetails.Extensions["request_id"] = context.TraceIdentifier;
        }

        return problemDetails;
    }
}
