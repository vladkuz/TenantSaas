using TenantSaas.Core.Errors;

namespace TenantSaas.Sample.Middleware;

/// <summary>
/// Global exception handler middleware that converts unhandled exceptions to RFC 7807 Problem Details.
/// </summary>
/// <remarks>
/// This middleware provides a final safety net for unexpected errors, ensuring clients always
/// receive consistent Problem Details responses even when something goes wrong outside normal flow.
/// - Never leaks exception details in production
/// - Always includes trace_id for correlation
/// - Uses generic HTTP 500 with stable error format
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">Logger for recording exception details.</param>
public class ProblemDetailsExceptionMiddleware(
    RequestDelegate next,
    ILogger<ProblemDetailsExceptionMiddleware> logger)
{
    /// <summary>
    /// Invariant code for unhandled internal server errors.
    /// </summary>
    /// <remarks>
    /// This is a synthetic invariant code (not in InvariantCode registry) used specifically
    /// for unexpected exceptions. It's distinct from actual invariant violations to avoid
    /// confusion in monitoring and support workflows.
    /// </remarks>
    private const string InternalServerErrorCode = "InternalServerError";

    /// <summary>
    /// Guidance URI for internal server errors.
    /// </summary>
    private const string InternalServerErrorGuidanceUri = "https://docs.tenantsaas.dev/errors/internal-server-error";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var (traceId, requestId) = httpContext.GetCorrelationIds();

            // Log full exception with structured fields for debugging
            logger.LogError(ex,
                "Unhandled exception in request pipeline. TraceId: {TraceId}, RequestId: {RequestId}, Path: {Path}",
                traceId,
                requestId,
                httpContext.Request.Path);

            // Cannot write Problem Details if response has already started (e.g., streaming)
            // In this case, the exception will propagate and be handled by the server
            if (httpContext.Response.HasStarted)
            {
                throw;
            }

            // Create generic Problem Details response
            // DO NOT include exception details - use trace_id for support correlation
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = "urn:tenantsaas:error:internal-server-error",
                Title = "Internal server error",
                Status = 500,
                Detail = "An unexpected error occurred. Please contact support with the trace ID.",
                Instance = null
            };

            problemDetails.Extensions[ProblemDetailsExtensions.InvariantCodeKey] = InternalServerErrorCode;
            problemDetails.Extensions[ProblemDetailsExtensions.TraceId] = traceId;
            problemDetails.Extensions[ProblemDetailsExtensions.RequestId] = requestId;
            problemDetails.Extensions[ProblemDetailsExtensions.GuidanceLink] = InternalServerErrorGuidanceUri;

            // Set response status and write Problem Details
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
