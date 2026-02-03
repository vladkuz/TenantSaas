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
public class EnforcementLoggingTests : IDisposable
{
    private readonly List<CapturedLogEntry> capturedLogs = [];

    public EnforcementLoggingTests()
    {
        // Reset BoundaryGuard for each test to allow reconfiguration
        BoundaryGuard.ResetForTesting();
    }

    public void Dispose()
    {
        // Clean up after each test
        BoundaryGuard.ResetForTesting();
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Success_LogsContextInitialized()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-001"));
        var context = TenantContext.ForRequest(scope, "trace-log-001", "req-log-001");
        accessor.Set(context);

        // Act
        var result = BoundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeTrue("Context is initialized");

        // Verify log was emitted with all required fields
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1001, "ContextInitialized event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1001);
        logEntry.LogLevel.Should().Be(LogLevel.Information);
        logEntry.Message.Should().Contain("tenant-log-001");
        logEntry.Message.Should().Contain("trace-log-001");
        logEntry.Message.Should().Contain("req-log-001");
        logEntry.Message.Should().Contain("request");
        logEntry.Message.Should().Contain("Tenant");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Failure_LogsContextNotInitialized()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var accessor = new AmbientTenantContextAccessor();
        // Context not initialized

        // Act
        var result = BoundaryGuard.RequireContext(accessor, overrideTraceId: "trace-failure-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Context is not initialized");
        result.InvariantCode.Should().Be("ContextInitialized");

        // Verify log was emitted with invariant_code
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1002, "ContextNotInitialized event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1002);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("trace-failure-001");
        logEntry.Message.Should().Contain("ContextInitialized");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_Success_LogsAttributionResolved()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var tenantId = new TenantId("tenant-log-002");
        var source = TenantAttributionSource.RouteParameter;
        var attributionResult = new TenantAttributionResult.Success(tenantId, source);

        // Act
        var result = BoundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-log-002");

        // Assert
        result.IsSuccess.Should().BeTrue("Attribution is unambiguous");

        // Verify log was emitted
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1003, "AttributionResolved event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1003);
        logEntry.LogLevel.Should().Be(LogLevel.Information);
        logEntry.Message.Should().Contain("tenant-log-002");
        logEntry.Message.Should().Contain("trace-log-002");
        logEntry.Message.Should().Contain("Route Parameter");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_Ambiguous_LogsAttributionAmbiguous()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var tenantId1 = new TenantId("tenant-route");
        var tenantId2 = new TenantId("tenant-header");
        var conflicts = new[]
        {
            new AttributionConflict(TenantAttributionSource.RouteParameter, tenantId1),
            new AttributionConflict(TenantAttributionSource.HeaderValue, tenantId2)
        };
        var attributionResult = new TenantAttributionResult.Ambiguous(conflicts);

        // Act
        var result = BoundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-log-003");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution is ambiguous");

        // Verify log was emitted
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1004, "AttributionAmbiguous event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1004);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("trace-log-003");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
        logEntry.Message.Should().Contain("Route Parameter");
        logEntry.Message.Should().Contain("Header Value");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_NotFound_LogsAttributionNotFound()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var attributionResult = TenantAttributionResult.WasNotFound();

        // Act
        var result = BoundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-notfound-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution not found");

        // Verify log was emitted
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1008, "AttributionNotFound event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1008);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("trace-notfound-001");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
    }

    [Fact]
    public void BoundaryGuard_RequireUnambiguousAttribution_NotAllowed_LogsAttributionNotAllowed()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var attributionResult = new TenantAttributionResult.NotAllowed(TenantAttributionSource.TokenClaim);

        // Act
        var result = BoundaryGuard.RequireUnambiguousAttribution(
            attributionResult,
            "trace-notallowed-001");

        // Assert
        result.IsSuccess.Should().BeFalse("Attribution source not allowed");

        // Verify log was emitted
        capturedLogs.Should().ContainSingle(e => e.EventId.Id == 1009, "AttributionNotAllowed event should be logged");
        var logEntry = capturedLogs.Single(e => e.EventId.Id == 1009);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("trace-notallowed-001");
        logEntry.Message.Should().Contain("TenantAttributionUnambiguous");
        logEntry.Message.Should().Contain("Token Claim");
    }

    [Fact]
    public void EnforcementLogs_AllEventsIncludeTraceId()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-004"));
        var context = TenantContext.ForRequest(scope, "trace-correlation-test", "req-correlation-test");
        accessor.Set(context);

        // Act - Success path
        var successResult = BoundaryGuard.RequireContext(accessor);
        accessor.Clear();

        // Act - Failure path
        var failureResult = BoundaryGuard.RequireContext(accessor, overrideTraceId: "trace-failure-test");

        // Assert
        successResult.IsSuccess.Should().BeTrue();
        failureResult.IsSuccess.Should().BeFalse();
        failureResult.TraceId.Should().Be("trace-failure-test");

        // Verify all logs contain trace_id
        capturedLogs.Should().HaveCount(2);
        capturedLogs.Should().AllSatisfy(e => e.Message.Should().Contain("trace_id="));
    }

    [Fact]
    public void EnforcementLogs_FailuresIncludeInvariantCode()
    {
        // Arrange
        var testLogger = new TestLoggerFactory(capturedLogs).CreateLogger("BoundaryGuard");
        var enricher = new DefaultLogEnricher();
        BoundaryGuard.Configure(testLogger, enricher);

        var accessor = new AmbientTenantContextAccessor();

        // Act
        var result = BoundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be("ContextInitialized");

        // Verify log includes invariant_code
        var logEntry = capturedLogs.Single();
        logEntry.Message.Should().Contain("invariant_code=ContextInitialized");
    }
}
