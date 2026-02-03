using System.Diagnostics;

namespace TenantSaas.Sample.Middleware;

/// <summary>
/// Extracts correlation IDs from HTTP requests following distributed tracing standards.
/// </summary>
/// <remarks>
/// This utility implements proper separation between trace_id (distributed tracing correlation)
/// and request_id (per-request identifier) as required by the trust contract.
/// 
/// Trace ID sources (in order of precedence):
/// 1. W3C Trace Context (traceparent header) - standard for distributed tracing
/// 2. Activity.Current.TraceId (OpenTelemetry/DiagnosticSource integration)
/// 3. X-Trace-Id header (legacy/custom implementations)
/// 4. HttpContext.TraceIdentifier (fallback)
/// 
/// Request ID sources (in order of precedence):
/// 1. X-Request-ID header (client-provided)
/// 2. HttpContext.TraceIdentifier (ASP.NET Core generated)
/// </remarks>
public static class HttpCorrelationExtensions
{
    /// <summary>
    /// W3C Trace Context header name (standard for distributed tracing).
    /// </summary>
    public const string TraceparentHeader = "traceparent";

    /// <summary>
    /// Custom trace ID header (legacy/custom implementations).
    /// </summary>
    public const string XTraceIdHeader = "X-Trace-Id";

    /// <summary>
    /// Request ID header (client-provided for request correlation).
    /// </summary>
    public const string XRequestIdHeader = "X-Request-ID";

    /// <summary>
    /// Extracts the distributed trace ID from the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The trace ID for end-to-end distributed tracing correlation.</returns>
    public static string GetTraceId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. W3C Trace Context (traceparent header)
        // Format: {version}-{trace-id}-{parent-id}-{trace-flags}
        // Example: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01
        if (context.Request.Headers.TryGetValue(TraceparentHeader, out var traceparent)
            && !string.IsNullOrWhiteSpace(traceparent))
        {
            var parts = traceparent.ToString().Split('-');
            if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                return parts[1];
            }
        }

        // 2. Activity.Current (OpenTelemetry/DiagnosticSource integration)
        if (Activity.Current?.TraceId is { } activityTraceId)
        {
            var traceIdString = activityTraceId.ToString();
            // Ensure it's not the default empty trace ID
            if (!string.IsNullOrWhiteSpace(traceIdString)
                && traceIdString != "00000000000000000000000000000000")
            {
                return traceIdString;
            }
        }

        // 3. X-Trace-Id header (legacy/custom implementations)
        if (context.Request.Headers.TryGetValue(XTraceIdHeader, out var xTraceId)
            && !string.IsNullOrWhiteSpace(xTraceId))
        {
            return xTraceId.ToString();
        }

        // 4. Fallback to ASP.NET Core's TraceIdentifier
        return context.TraceIdentifier;
    }

    /// <summary>
    /// Extracts the request-specific ID from the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The request ID unique to this specific HTTP request.</returns>
    public static string GetRequestId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. X-Request-ID header (client-provided for correlation)
        if (context.Request.Headers.TryGetValue(XRequestIdHeader, out var requestId)
            && !string.IsNullOrWhiteSpace(requestId))
        {
            return requestId.ToString();
        }

        // 2. ASP.NET Core's TraceIdentifier (unique per request)
        return context.TraceIdentifier;
    }

    /// <summary>
    /// Extracts both correlation IDs from the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A tuple containing (traceId, requestId).</returns>
    public static (string TraceId, string RequestId) GetCorrelationIds(this HttpContext context)
    {
        return (context.GetTraceId(), context.GetRequestId());
    }
}
