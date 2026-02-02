using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;

namespace TenantSaas.Sample.Middleware;

/// <summary>
/// Middleware for initializing tenant context at the application boundary.
/// </summary>
/// <remarks>
/// This middleware demonstrates the single unavoidable integration point (FR11).
/// It initializes tenant context before request processing and cleans up afterward
/// to prevent context leakage in pooled execution environments.
/// </remarks>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMutableTenantContextAccessor _accessor;

    public TenantContextMiddleware(
        RequestDelegate next,
        IMutableTenantContextAccessor accessor)
    {
        _next = next;
        _accessor = accessor;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Skip if already initialized (idempotency)
        if (_accessor.IsInitialized)
        {
            await _next(httpContext);
            return;
        }

        // Resolve tenant scope (placeholder - will be Story 3.2)
        var scope = ResolveTenantScope(httpContext);

        // TraceId: correlation ID that spans distributed systems (may come from incoming headers)
        // RequestId: unique identifier for this specific HTTP request
        // TODO: Story 3.2 will extract distributed trace ID from headers (e.g., traceparent)
        //       For now, both use TraceIdentifier as placeholder
        var traceId = httpContext.TraceIdentifier;
        var requestId = httpContext.TraceIdentifier;

        var context = TenantContext.ForRequest(scope, traceId, requestId);

        // Initialize context and ensure cleanup on all exit paths
        _accessor.Set(context);
        try
        {
            // Enforce boundary - verify context is properly initialized
            var result = BoundaryGuard.RequireContext(_accessor);

            if (!result.IsSuccess)
            {
                var problemDetails = EnforcementProblemDetails.FromEnforcementResult(
                    result,
                    httpContext);

                httpContext.Response.StatusCode = problemDetails.Status ?? 500;
                await httpContext.Response.WriteAsJsonAsync(problemDetails);
                return;
            }

            await _next(httpContext);
        }
        finally
        {
            // Always clear context to prevent leakage in pooled environments
            _accessor.Clear();
        }
    }

    private static TenantScope ResolveTenantScope(HttpContext context)
    {
        // Placeholder: always return no-tenant for health checks
        // Story 3.2 will implement full attribution resolution
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return TenantScope.ForNoTenant(NoTenantReason.HealthCheck);
        }

        // For now, return a dummy tenant
        return TenantScope.ForTenant(new TenantId("placeholder-tenant"));
    }
}
