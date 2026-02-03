using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Tenancy;
using TenantSaas.Core.Logging;
using Microsoft.Extensions.Logging;

namespace TenantSaas.Sample.Middleware;

/// <summary>
/// Constants for logging when values cannot be determined.
/// </summary>
internal static class LoggingDefaults
{
    /// <summary>Fallback value when invariant code cannot be extracted.</summary>
    public const string UnknownInvariantCode = "Unknown";
    
    /// <summary>Fallback value when problem type cannot be extracted.</summary>
    public const string UnknownProblemType = "unknown";
}

/// <summary>
/// Middleware for initializing tenant context at the application boundary.
/// </summary>
/// <remarks>
/// This middleware demonstrates the single unavoidable integration point (FR11).
/// It initializes tenant context before request processing and cleans up afterward
/// to prevent context leakage in pooled execution environments.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="accessor">Mutable tenant context accessor for setting/clearing context.</param>
/// <param name="attributionResolver">Resolver for tenant attribution from request sources.</param>
/// <param name="logger">Logger for structured enforcement events.</param>
/// <param name="enricher">Log enricher for structured field extraction.</param>
public class TenantContextMiddleware(
    RequestDelegate next,
    IMutableTenantContextAccessor accessor,
    ITenantAttributionResolver attributionResolver,
    ILogger<TenantContextMiddleware> logger,
    ILogEnricher enricher)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Skip if already initialized (idempotency)
        if (accessor.IsInitialized)
        {
            await next(httpContext);
            return;
        }

        // Extract correlation IDs using standard distributed tracing patterns
        // TraceId: end-to-end correlation spanning distributed systems (from traceparent, Activity, or headers)
        // RequestId: unique identifier for this specific HTTP request
        var (traceId, requestId) = httpContext.GetCorrelationIds();

        // Health check bypass - no tenant required
        if (httpContext.Request.Path.StartsWithSegments("/health"))
        {
            var healthContext = TenantContext.ForRequest(
                TenantScope.ForNoTenant(NoTenantReason.HealthCheck),
                traceId,
                requestId);
            accessor.Set(healthContext);
            
            // Log health check context initialization
            var healthLogEvent = enricher.Enrich(healthContext, "ContextInitialized");
            EnforcementEventSource.ContextInitialized(
                logger,
                healthLogEvent.TenantRef,
                healthLogEvent.TraceId,
                healthLogEvent.RequestId,
                healthLogEvent.ExecutionKind!,
                healthLogEvent.ScopeType!);
            
            try
            {
                await next(httpContext);
            }
            finally
            {
                accessor.Clear();
            }
            return;
        }

        // Extract attribution sources from request
        var sources = ExtractAttributionSources(httpContext);

        // Resolve tenant attribution with rules
        var rules = TenantAttributionRules.Default();
        var attributionResult = attributionResolver.Resolve(
            sources,
            rules,
            Abstractions.Contexts.ExecutionKind.Request);

        // Enforce unambiguous attribution
        var enforcementResult = BoundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            traceId);

        if (!enforcementResult.IsSuccess)
        {
            // Cannot write Problem Details if response has already started (e.g., streaming)
            if (httpContext.Response.HasStarted)
            {
                return;
            }

            // Convert enforcement failure to standardized RFC 7807 Problem Details
            var problemDetails = ProblemDetailsFactory.ForTenantAttributionAmbiguous(
                traceId,
                requestId,
                enforcementResult.ConflictingSources);

            // Log refusal before returning Problem Details
            var invariantCode = problemDetails.Extensions.TryGetValue(
                ProblemDetailsExtensions.InvariantCodeKey,
                out var code) ? code?.ToString() ?? LoggingDefaults.UnknownInvariantCode : LoggingDefaults.UnknownInvariantCode;
            
            EnforcementEventSource.RefusalEmitted(
                logger,
                traceId,
                requestId,
                invariantCode,
                problemDetails.Status ?? 422,
                problemDetails.Type ?? LoggingDefaults.UnknownProblemType);

            httpContext.Response.StatusCode = problemDetails.Status ?? 422;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        // Initialize context with resolved tenant
        // Safety: ResolvedTenantId is guaranteed non-null when IsSuccess is true
        var scope = TenantScope.ForTenant(enforcementResult.ResolvedTenantId!.Value);
        var context = TenantContext.ForRequest(scope, traceId, requestId);

        // Set context and ensure cleanup on all exit paths
        accessor.Set(context);
        try
        {
            // Verify context initialization (defensive check)
            var contextCheck = BoundaryGuard.RequireContext(accessor);
            if (!contextCheck.IsSuccess)
            {
                // Cannot write Problem Details if response has already started (e.g., streaming)
                if (httpContext.Response.HasStarted)
                {
                    return;
                }

                // Convert enforcement failure to standardized RFC 7807 Problem Details
                var problemDetails = ProblemDetailsFactory.ForContextNotInitialized(
                    traceId,
                    requestId);

                // Log refusal before returning Problem Details
                var contextInvariantCode = problemDetails.Extensions.TryGetValue(
                    ProblemDetailsExtensions.InvariantCodeKey,
                    out var contextCode) ? contextCode?.ToString() ?? LoggingDefaults.UnknownInvariantCode : LoggingDefaults.UnknownInvariantCode;
                
                EnforcementEventSource.RefusalEmitted(
                    logger,
                    traceId,
                    requestId,
                    contextInvariantCode,
                    problemDetails.Status ?? 500,
                    problemDetails.Type ?? LoggingDefaults.UnknownProblemType);

                httpContext.Response.StatusCode = problemDetails.Status ?? 500;
                httpContext.Response.ContentType = "application/problem+json";
                await httpContext.Response.WriteAsJsonAsync(problemDetails);
                return;
            }

            // Log successful context initialization (already logged by BoundaryGuard)
            // No additional logging needed here to avoid duplication

            await next(httpContext);
        }
        finally
        {
            // Always clear context to prevent leakage in pooled environments
            accessor.Clear();
        }
    }

    /// <summary>
    /// Extracts tenant attribution sources from HTTP request.
    /// </summary>
    /// <remarks>
    /// Reference implementation uses hardcoded source names ("tenantId" route, "X-Tenant-Id" header).
    /// Production implementations should inject attribution source configuration from settings.
    /// This sample demonstrates the extraction pattern; adopters customize for their needs.
    /// </remarks>
    private static IReadOnlyDictionary<TenantAttributionSource, TenantId> ExtractAttributionSources(
        HttpContext context)
    {
        var sources = new Dictionary<TenantAttributionSource, TenantId>();

        // Extract from route parameter (e.g., /tenants/{tenantId}/...)
        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeTenantId)
            && routeTenantId is string routeValue
            && !string.IsNullOrWhiteSpace(routeValue))
        {
            sources[TenantAttributionSource.RouteParameter] = new TenantId(routeValue);
        }

        // Extract from X-Tenant-Id header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId)
            && !string.IsNullOrWhiteSpace(headerTenantId.ToString()))
        {
            sources[TenantAttributionSource.HeaderValue] = new TenantId(headerTenantId.ToString());
        }

        return sources;
    }
}
