using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.Sample.Middleware;

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
public class TenantContextMiddleware(
    RequestDelegate next,
    IMutableTenantContextAccessor accessor,
    ITenantAttributionResolver attributionResolver)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Skip if already initialized (idempotency)
        if (accessor.IsInitialized)
        {
            await next(httpContext);
            return;
        }

        // TraceId: correlation ID that spans distributed systems (may come from incoming headers)
        // RequestId: unique identifier for this specific HTTP request
        var traceId = httpContext.TraceIdentifier;
        var requestId = httpContext.TraceIdentifier;

        // Health check bypass - no tenant required
        if (httpContext.Request.Path.StartsWithSegments("/health"))
        {
            var healthContext = TenantContext.ForRequest(
                TenantScope.ForNoTenant(NoTenantReason.HealthCheck),
                traceId,
                requestId);
            accessor.Set(healthContext);
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
            var problemDetails = EnforcementProblemDetails.FromAttributionEnforcementResult(
                enforcementResult,
                httpContext);

            httpContext.Response.StatusCode = problemDetails.Status ?? 422;
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
                var problemDetails = EnforcementProblemDetails.FromEnforcementResult(
                    contextCheck,
                    httpContext);

                httpContext.Response.StatusCode = problemDetails.Status ?? 500;
                await httpContext.Response.WriteAsJsonAsync(problemDetails);
                return;
            }

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
