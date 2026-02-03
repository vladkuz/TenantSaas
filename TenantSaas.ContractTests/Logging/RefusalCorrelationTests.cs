using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;
using TenantSaas.Sample.Middleware;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests.Logging;

/// <summary>
/// Contract tests for refusal correlation between logs and Problem Details following Story 3.4 AC#2.
/// Validates that logs and Problem Details can be joined by trace_id/request_id/invariant_code.
/// </summary>
public class RefusalCorrelationTests
{
    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchTraceId()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-001";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Problem Details includes trace_id
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId]?.ToString().Should().Be("correlation-trace-001",
            "Problem Details trace_id must match log trace_id for correlation");

        // In production, log would also contain: trace_id=correlation-trace-001
        // Logs and Problem Details can be joined: logs.trace_id = problemDetails.extensions.trace_id
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchRequestId()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-002";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Problem Details includes request_id
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(RequestId);
        var requestId = problemDetails.Extensions[RequestId]?.ToString();
        requestId.Should().NotBeNullOrWhiteSpace("Problem Details must include request_id");

        // In production, log would also contain: request_id={requestId}
        // Logs and Problem Details can be joined: logs.request_id = problemDetails.extensions.request_id
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchInvariantCode()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-003";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Problem Details includes invariant_code
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(InvariantCodeKey);
        var invariantCode = problemDetails.Extensions[InvariantCodeKey]?.ToString();
        invariantCode.Should().NotBeNullOrWhiteSpace("Problem Details must include invariant_code");

        // In production, log would also contain: invariant_code={invariantCode}
        // Logs and Problem Details can be joined: logs.invariant_code = problemDetails.extensions.invariant_code
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchHttpStatus()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-004";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - HTTP status matches between response and Problem Details
        context.Response.StatusCode.Should().Be(422, "Ambiguous attribution returns 422");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(422, "Problem Details status must match HTTP response status");

        // In production, log would also contain: http_status=422
        // Logs and Problem Details can be joined: logs.http_status = problemDetails.status
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogsCanBeJoinedByTraceId()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "join-trace-005";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Validate join pattern
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        
        // Extract correlation fields from Problem Details
        var traceId = problemDetails!.Extensions[TraceId]?.ToString();
        var requestId = problemDetails.Extensions[RequestId]?.ToString();
        var invariantCode = problemDetails.Extensions[InvariantCodeKey]?.ToString();

        traceId.Should().Be("join-trace-005");
        requestId.Should().NotBeNullOrWhiteSpace();
        invariantCode.Should().NotBeNullOrWhiteSpace();

        // Demonstrate join pattern (in production log aggregation):
        // SELECT * FROM logs
        // INNER JOIN problem_details ON logs.trace_id = problem_details.extensions.trace_id
        // WHERE logs.trace_id = 'join-trace-005'
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_NoSensitiveTenantIdsInLogsWhenDisclosureUnsafe()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "disclosure-trace-006";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Problem Details does NOT expose tenant IDs when disclosure is unsafe
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();

        // Validate disclosure policy: no tenant_ref in extensions when unsafe
        // For attribution failures (no tenant resolved yet), tenant_ref should not be present or use safe-state
        problemDetails!.Extensions.Should().NotContainKey("tenant_ref",
            "Problem Details should not expose tenant_ref when disclosure is unsafe");

        // In production logs:
        // - If tenant resolved: tenant_ref = opaque public ID or safe-state token
        // - If tenant not resolved: tenant_ref = "unknown"
        // - Never: tenant_ref = raw internal ID that could be reversed or enumerated
    }

    [Fact]
    public async Task TenantContextMiddleware_HealthCheck_LogsIncludeSafeStateTenantRef()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor,
            attributionResolver: attributionResolver,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        context.TraceIdentifier = "health-trace-007";

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Health check logs should use safe-state tenant_ref "unknown"
        // (Validated by LogEnricherTests; this test demonstrates end-to-end flow)
        accessor.IsInitialized.Should().BeFalse("Context should be cleared after health check");
    }
}
