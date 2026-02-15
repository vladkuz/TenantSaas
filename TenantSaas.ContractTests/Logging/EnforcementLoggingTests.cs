using FluentAssertions;
using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.ContractTests.TestUtilities;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.ContractTests.Logging;

/// <summary>
/// Contract tests for enforcement logging integration following Story 3.4 AC#1.
/// Validates that enforcement decisions emit structured logs with required fields.
/// </summary>
/// <remarks>
/// Each test creates its own BoundaryGuard instance, ensuring full test isolation.
/// </remarks>
public class EnforcementLoggingTests
{
    private static IBoundaryGuard CreateBoundaryGuard(CapturedLogCollection logs)
    {
        var loggerFactory = new TestLoggerFactory(logs);
        var logger = loggerFactory.CreateLogger<BoundaryGuard>();
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Success_LogsContextInitialized()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-001"));
        var context = TenantContext.ForRequest(scope, "trace-log-001", "req-log-001");
        accessor.Set(context);

        // Act
        var result = boundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeTrue("Context is initialized");

        // Verify log was emitted with all required fields
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1001, "ContextInitialized event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1001);
        logEntry.LogLevel.Should().Be(LogLevel.Information);
        logEntry.Message.Should().Contain("event_name=ContextInitialized");
        logEntry.Message.Should().Contain("severity=Information");
        logEntry.Message.Should().Contain("tenant_ref=opaque:");
        logEntry.Message.Should().Contain("trace-log-001");
        logEntry.Message.Should().Contain("req-log-001");
        logEntry.Message.Should().Contain("execution_kind=request");
        logEntry.Message.Should().Contain("scope_type=Tenant");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Failure_LogsContextNotInitialized()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var accessor = new AmbientTenantContextAccessor();
        // Context not initialized

        // Act
        var result = boundaryGuard.RequireContext(accessor, overrideTraceId: "trace-failure-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Context is not initialized");
        result.InvariantCode.Should().Be("ContextInitialized");

        // Verify log was emitted with invariant_code
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1002, "ContextNotInitialized event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1002);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("event_name=ContextNotInitialized");
        logEntry.Message.Should().Contain("severity=Warning");
        logEntry.Message.Should().Contain("tenant_ref=unknown");
        logEntry.Message.Should().Contain("trace-failure-001");
        logEntry.Message.Should().Contain("execution_kind=unknown");
        logEntry.Message.Should().Contain("scope_type=Unknown");
        logEntry.Message.Should().Contain("ContextInitialized");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_Success_LogsAttributionResolved()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var tenantId = new TenantId("tenant-log-002");
        var source = TenantAttributionSource.RouteParameter;
        var attributionResult = new TenantAttributionResult.Success(tenantId, source);

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-log-002");

        // Assert
        result.IsSuccess.Should().BeTrue("Attribution is unambiguous");

        // Verify log was emitted
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1003, "AttributionResolved event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1003);
        logEntry.LogLevel.Should().Be(LogLevel.Information);
        logEntry.Message.Should().Contain("event_name=AttributionResolved");
        logEntry.Message.Should().Contain("severity=Information");
        logEntry.Message.Should().Contain("tenant_ref=opaque:");
        logEntry.Message.Should().Contain("trace-log-002");
        logEntry.Message.Should().Contain("Route Parameter");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_Ambiguous_LogsAttributionAmbiguous()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var tenantId1 = new TenantId("tenant-route");
        var tenantId2 = new TenantId("tenant-header");
        var conflicts = new[]
        {
            new AttributionConflict(TenantAttributionSource.RouteParameter, tenantId1),
            new AttributionConflict(TenantAttributionSource.HeaderValue, tenantId2)
        };
        var attributionResult = new TenantAttributionResult.Ambiguous(conflicts);

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-log-003");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution is ambiguous");

        // Verify log was emitted
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1004, "AttributionAmbiguous event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1004);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("event_name=AttributionAmbiguous");
        logEntry.Message.Should().Contain("severity=Warning");
        logEntry.Message.Should().Contain("trace-log-003");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
        logEntry.Message.Should().Contain("Route Parameter");
        logEntry.Message.Should().Contain("Header Value");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_NotFound_LogsAttributionNotFound()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var attributionResult = TenantAttributionResult.WasNotFound();

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-notfound-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution not found");

        // Verify log was emitted
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1008, "AttributionNotFound event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1008);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("event_name=AttributionNotFound");
        logEntry.Message.Should().Contain("severity=Warning");
        logEntry.Message.Should().Contain("trace-notfound-001");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_NotAllowed_LogsAttributionNotAllowed()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var attributionResult = new TenantAttributionResult.NotAllowed(TenantAttributionSource.TokenClaim);

        // Act
        var result = boundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-notallowed-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution source not allowed");

        // Verify log was emitted
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1009, "AttributionNotAllowed event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1009);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("event_name=AttributionNotAllowed");
        logEntry.Message.Should().Contain("severity=Warning");
        logEntry.Message.Should().Contain("trace-notallowed-001");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
        logEntry.Message.Should().Contain("Token Claim");
    }

    [Fact]
    public void EnforcementLogs_AllEventsIncludeTraceId()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-004"));
        var context = TenantContext.ForRequest(scope, "trace-correlation-test", "req-correlation-test");
        accessor.Set(context);

        // Act - Success path
        var successResult = boundaryGuard.RequireContext(accessor);
        accessor.Clear();

        // Act - Failure path
        var failureResult = boundaryGuard.RequireContext(accessor, overrideTraceId: "trace-failure-test");

        // Assert
        successResult.IsSuccess.Should().BeTrue();
        failureResult.IsSuccess.Should().BeFalse();
        failureResult.TraceId.Should().Be("trace-failure-test");

        // Verify all logs contain trace_id
        var logs = capturedLogs.ToList();
        logs.Should().HaveCount(2);
        logs.Should().AllSatisfy(e => e.Message.Should().Contain("trace_id="));
    }

    [Fact]
    public void EnforcementLogs_FailuresIncludeInvariantCode()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);

        var accessor = new AmbientTenantContextAccessor();

        // Act
        var result = boundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be("ContextInitialized");

        // Verify log includes invariant_code
        var logs = capturedLogs.ToList();
        var logEntry = logs.Single();
        logEntry.Message.Should().Contain("invariant_code=ContextInitialized");
    }
}
