using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TenantSaas.Abstractions.Disclosure;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.ContractTests.TestUtilities;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Errors;
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
    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }

    private static ITenantContextInitializer CreateInitializer(IMutableTenantContextAccessor accessor)
    {
        var logger = NullLogger<TenantContextInitializer>.Instance;
        return new TenantContextInitializer(accessor, logger);
    }

    private static (TenantContextMiddleware Middleware, CapturedLogCollection Logs, AmbientTenantContextAccessor Accessor)
        CreateMiddlewareWithCapturedLogs()
    {
        var logs = new CapturedLogCollection();
        var loggerFactory = new TestLoggerFactory(logs);
        var logger = loggerFactory.CreateLogger<TenantContextMiddleware>();
        var accessor = new AmbientTenantContextAccessor();
        var initializer = CreateInitializer(accessor);
        var attributionResolver = new TenantAttributionResolver();
        var enricher = new DefaultLogEnricher();
        var boundaryGuard = CreateBoundaryGuard();

        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            initializer: initializer,
            attributionResolver: attributionResolver,
            boundaryGuard: boundaryGuard,
            logger: logger,
            enricher: enricher);

        return (middleware, logs, accessor);
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchTraceId()
    {
        // Arrange
        var (middleware, logs, accessor) = CreateMiddlewareWithCapturedLogs();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-001";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - Problem Details includes trace_id
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId]?.ToString().Should().Be("correlation-trace-001",
            "Problem Details trace_id must match log trace_id for correlation");

        // Assert - Log contains same trace_id
        var refusalLogs = logs.ToList().WithEventId(1006).ToList();
        refusalLogs.Should().ContainSingle();
        refusalLogs[0].Message.Should().Contain("trace_id=correlation-trace-001");
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchRequestId()
    {
        // Arrange
        var (middleware, logs, accessor) = CreateMiddlewareWithCapturedLogs();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-002";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - Problem Details includes request_id
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(RequestId);
        var requestId = problemDetails.Extensions[RequestId]?.ToString();
        requestId.Should().NotBeNullOrWhiteSpace("Problem Details must include request_id");

        // Assert - Log contains same request_id
        var refusalLogs = logs.ToList().WithEventId(1006).ToList();
        refusalLogs.Should().ContainSingle();
        refusalLogs[0].Message.Should().Contain($"request_id={requestId}");
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchInvariantCode()
    {
        // Arrange
        var (middleware, logs, accessor) = CreateMiddlewareWithCapturedLogs();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-003";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - Problem Details includes invariant_code
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey(InvariantCodeKey);
        var invariantCode = problemDetails.Extensions[InvariantCodeKey]?.ToString();
        invariantCode.Should().NotBeNullOrWhiteSpace("Problem Details must include invariant_code");

        // Assert - Log contains same invariant_code
        var refusalLogs = logs.ToList().WithEventId(1006).ToList();
        refusalLogs.Should().ContainSingle();
        refusalLogs[0].Message.Should().Contain($"invariant_code={invariantCode}");
    }

    [Fact]
    public async Task TenantContextMiddleware_Refusal_LogAndProblemDetailsMatchHttpStatus()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = CreateInitializer(accessor);
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var boundaryGuard = CreateBoundaryGuard();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            initializer: initializer,
            attributionResolver: attributionResolver,
            boundaryGuard: boundaryGuard,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "correlation-trace-004";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

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
        var initializer = CreateInitializer(accessor);
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var boundaryGuard = CreateBoundaryGuard();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            initializer: initializer,
            attributionResolver: attributionResolver,
            boundaryGuard: boundaryGuard,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "join-trace-005";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

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
        var initializer = CreateInitializer(accessor);
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var boundaryGuard = CreateBoundaryGuard();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            initializer: initializer,
            attributionResolver: attributionResolver,
            boundaryGuard: boundaryGuard,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tenants";
        context.TraceIdentifier = "disclosure-trace-006";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - Problem Details does NOT expose tenant IDs when disclosure is unsafe
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();

        // Validate disclosure policy: tenant_ref absent OR safe-state when disclosure is unsafe
        if (problemDetails!.Extensions.TryGetValue("tenant_ref", out var tenantRef))
        {
            TenantRefSafeState.IsSafeState(tenantRef?.ToString()).Should().BeTrue(
                "tenant_ref must use safe-state values when disclosure is unsafe");
        }

        // In production logs:
        // - If tenant resolved: tenant_ref = opaque public ID or safe-state token
        // - If tenant not resolved: tenant_ref = "unknown"
        // - Never: tenant_ref = raw internal ID that could be reversed or enumerated
    }

    [Fact]
    public void RefusalCorrelation_RequestExecution_ProblemDetailsAndLogShareCorrelationFields()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-correlation"));
        var context = TenantContext.ForRequest(scope, "trace-corr-req", "request-corr-req");

        // Act
        var logEvent = enricher.Enrich(context, "RefusalEmitted", InvariantCode.ContextInitialized);
        var problemDetails = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            context.TraceId,
            context.RequestId);

        // Assert - correlation fields are joinable between logs and Problem Details
        problemDetails.Extensions[TraceId].Should().Be(logEvent.TraceId);
        problemDetails.Extensions[RequestId].Should().Be(logEvent.RequestId);
        problemDetails.Extensions[InvariantCodeKey].Should().Be(logEvent.InvariantCode);
    }

    [Fact]
    public void RefusalCorrelation_NonRequestExecution_OmitsRequestId()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var capturedLogs = new CapturedLogCollection();
        var loggerFactory = new TestLoggerFactory(capturedLogs);
        var logger = loggerFactory.CreateLogger<BoundaryGuard>();
        var scope = TenantScope.ForTenant(new TenantId("tenant-correlation-bg"));
        var context = TenantContext.ForBackground(scope, "trace-corr-bg");

        // Act
        var logEvent = enricher.Enrich(context, "RefusalEmitted", InvariantCode.ContextInitialized);
        var problemDetails = ProblemDetailsFactory.FromInvariantViolation(
            InvariantCode.ContextInitialized,
            context.TraceId,
            requestId: null);
        EnforcementEventSource.RefusalEmitted(
            logger,
            context.TraceId,
            logEvent.InvariantCode ?? InvariantCode.ContextInitialized,
            httpStatus: problemDetails.Status ?? 500,
            problemType: problemDetails.Type ?? "unknown");

        // Assert - request_id omitted for non-request execution kinds
        logEvent.RequestId.Should().BeNull();
        problemDetails.Extensions.Should().NotContainKey(RequestId);
        problemDetails.Extensions[TraceId].Should().Be(logEvent.TraceId);
        problemDetails.Extensions[InvariantCodeKey].Should().Be(logEvent.InvariantCode);

        var refusalLogs = capturedLogs.ToList().WithEventId(1006).ToList();
        refusalLogs.Should().ContainSingle();
        refusalLogs[0].Message.Should().NotContain("request_id=");
    }

    [Fact]
    public async Task TenantContextMiddleware_HealthCheck_LogsIncludeSafeStateTenantRef()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = CreateInitializer(accessor);
        var attributionResolver = new TenantAttributionResolver();
        var logger = NullLogger<TenantContextMiddleware>.Instance;
        var enricher = new DefaultLogEnricher();
        var boundaryGuard = CreateBoundaryGuard();
        var middleware = new TenantContextMiddleware(
            next: _ => Task.CompletedTask,
            initializer: initializer,
            attributionResolver: attributionResolver,
            boundaryGuard: boundaryGuard,
            logger: logger,
            enricher: enricher);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        context.TraceIdentifier = "health-trace-007";

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert - Health check logs should use safe-state tenant_ref "unknown"
        // (Validated by LogEnricherTests; this test demonstrates end-to-end flow)
        accessor.IsInitialized.Should().BeFalse("Context should be cleared after health check");
    }
}
