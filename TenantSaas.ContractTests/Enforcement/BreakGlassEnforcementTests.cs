using FluentAssertions;
using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.ContractTests.TestUtilities;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;

namespace TenantSaas.ContractTests.Enforcement;

/// <summary>
/// Contract tests for break-glass enforcement (Story 3.5 AC #1).
/// Verifies that missing break-glass declarations are refused with proper invariant.
/// </summary>
public class BreakGlassEnforcementTests
{
    private static IBoundaryGuard CreateBoundaryGuard(CapturedLogCollection logs)
    {
        var loggerFactory = new TestLoggerFactory(logs);
        var logger = loggerFactory.CreateLogger<BoundaryGuard>();
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher, auditSink: null);
    }

    [Fact]
    public async Task RequireBreakGlass_NullDeclaration_ReturnsFailure()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-001";

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: null,
            traceId: traceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        result.Detail.Should().Be("Break-glass declaration is required.");
    }

    [Fact]
    public async Task RequireBreakGlass_NullDeclaration_ReturnsInvariantCode()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-002";

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: null,
            traceId: traceId);

        // Assert
        result.InvariantCode.Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
    }

    [Fact]
    public async Task RequireBreakGlass_NullDeclaration_LogsBreakGlassAttemptDenied()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-003";

        // Act
        await boundaryGuard.RequireBreakGlassAsync(
            declaration: null,
            traceId: traceId);

        // Assert - verify BreakGlassAttemptDenied was logged (EventId 1010)
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1010, "BreakGlassAttemptDenied event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1010);
        logEntry.LogLevel.Should().Be(LogLevel.Error);
        logEntry.Message.Should().Contain("trace-bg-003");
        logEntry.Message.Should().Contain("BreakGlassExplicitAndAudited");
    }

    [Fact]
    public async Task RequireBreakGlass_NullDeclaration_MapsToHttp403()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-004";

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: null,
            traceId: traceId);

        // Act
        var problemDetails = Core.Errors.ProblemDetailsFactory.ForBreakGlassRequired(
            traceId,
            requestId: null,
            reason: result.Detail);

        // Assert
        problemDetails.Status.Should().Be(403);
    }

    // Task 9: Tests for valid break-glass (AC #2)

    [Fact]
    public async Task RequireBreakGlass_ValidDeclaration_ReturnsSuccess()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-valid-001";

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Production incident #12345",
            declaredScope: "Debug customer_id=cust-999",
            targetTenantRef: "tenant-alpha",
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: declaration,
            traceId: traceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public async Task RequireBreakGlass_ValidDeclaration_LogsBreakGlassInvoked()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-valid-002";

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Production incident #12345",
            declaredScope: "Debug customer_id=cust-999",
            targetTenantRef: "tenant-alpha",
            timestamp: DateTimeOffset.UtcNow);

        // Act
        await boundaryGuard.RequireBreakGlassAsync(
            declaration: declaration,
            traceId: traceId);

        // Assert - verify BreakGlassInvoked was logged (EventId 1007)
        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1007, "BreakGlassInvoked event should be logged");
        var logEntry = logs.Single(e => e.EventId.Id == 1007);
        logEntry.LogLevel.Should().Be(LogLevel.Warning);
        logEntry.Message.Should().Contain("trace-bg-valid-002");
        logEntry.Message.Should().Contain("on-call@example.com");
        logEntry.Message.Should().Contain("Production incident #12345");
    }

    [Fact]
    public async Task RequireBreakGlass_ValidDeclaration_NullTargetTenantRef_UsesCrossTenantMarker()
    {
        // Arrange
        var capturedLogs = new CapturedLogCollection();
        var boundaryGuard = CreateBoundaryGuard(capturedLogs);
        var traceId = "trace-bg-valid-003";

        var declaration = new BreakGlassDeclaration(
            actorId: "on-call@example.com",
            reason: "Production incident #12345",
            declaredScope: "Query cross-tenant metrics",
            targetTenantRef: null,  // Cross-tenant operation
            timestamp: DateTimeOffset.UtcNow);

        // Act
        var result = await boundaryGuard.RequireBreakGlassAsync(
            declaration: declaration,
            traceId: traceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify log uses cross-tenant marker
        var logs = capturedLogs.ToList();
        var logEntry = logs.Single(e => e.EventId.Id == 1007);
        logEntry.Message.Should().Contain("cross_tenant");
    }
}
