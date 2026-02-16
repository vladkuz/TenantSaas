using FluentAssertions;
using Microsoft.Extensions.Logging;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;
using TenantSaas.ContractTests.TestUtilities;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;

namespace TenantSaas.ContractTests;

public sealed class SharedSystemOperationEnforcementTests
{
    [Fact]
    public void RequireSharedSystemOperation_AllowsOperationMappedToInvariant()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-shared-allow-001");

        // Act
        var result = guard.RequireSharedSystemOperation(
            context,
            TrustContractV1.SharedSystemOperationTenantExistenceCheck,
            InvariantCode.DisclosureSafe);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public void RequireSharedSystemOperation_RefusesTenantScopeForSharedOperation()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out _);
        var context = TenantContext.ForAdmin(
            TenantScope.ForTenant(new TenantId("tenant-regular-001")),
            "trace-shared-refuse-scope-001");

        // Act
        var result = guard.RequireSharedSystemOperation(
            context,
            TrustContractV1.SharedSystemOperationTenantExistenceCheck,
            InvariantCode.DisclosureSafe);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.TenantScopeRequired);
    }

    [Fact]
    public void RequireSharedSystemOperation_RefusesUnknownOperationWithStableInvariant()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out var capturedLogs);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-shared-refuse-op-001");

        // Act
        var result = guard.RequireSharedSystemOperation(
            context,
            "unknown-shared-operation",
            InvariantCode.DisclosureSafe);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.SharedSystemOperationAllowed);
        result.TraceId.Should().Be("trace-shared-refuse-op-001");

        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1005);
        logs.Single(e => e.EventId.Id == 1005).Message.Should().Contain("unknown-shared-operation");
    }

    [Fact]
    public void RequireSharedSystemOperation_RefusesDisallowedInvariantWithStableInvariant()
    {
        // Arrange
        var guard = CreateBoundaryGuard(out var capturedLogs);
        var context = TenantContext.ForAdmin(TenantScope.ForSharedSystem(), "trace-shared-refuse-invariant-001");

        // Act
        var result = guard.RequireSharedSystemOperation(
            context,
            TrustContractV1.SharedSystemOperationTenantExistenceCheck,
            InvariantCode.ContextInitialized);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.SharedSystemOperationAllowed);
        result.TraceId.Should().Be("trace-shared-refuse-invariant-001");

        var logs = capturedLogs.ToList();
        logs.Should().ContainSingle(e => e.EventId.Id == 1005);
        logs.Single(e => e.EventId.Id == 1005).Message.Should().Contain("attempted_invariant=ContextInitialized");
    }

    private static BoundaryGuard CreateBoundaryGuard(out CapturedLogCollection capturedLogs)
    {
        capturedLogs = new CapturedLogCollection();
        var loggerFactory = new TestLoggerFactory(capturedLogs);
        var logger = loggerFactory.CreateLogger<BoundaryGuard>();
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }
}
