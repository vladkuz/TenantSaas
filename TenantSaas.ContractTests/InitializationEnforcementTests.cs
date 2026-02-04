using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;
using Xunit;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for initialization primitive enforcement requirements (Story 4.1).
/// </summary>
public class InitializationEnforcementTests
{
    [Fact]
    public void BoundaryGuard_WhenContextNotInitialized_RefusesWithContextInitializedInvariant()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        // Act - invoke boundary guard without initializing context
        var result = guard.RequireContext(accessor);

        // Assert - refusal with ContextInitialized invariant
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.Detail.Should().Contain("context must be initialized");
    }

    [Fact]
    public void BoundaryGuard_WhenContextInitialized_AllowsExecution()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(
            accessor,
            NullLogger<TenantContextInitializer>.Instance);
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = initializer.InitializeRequest(scope, "trace-1", "req-1", TenantAttributionInputs.FromExplicitScope(scope));

        // Act - invoke boundary guard after initialization
        var result = guard.RequireContext(accessor);

        // Assert - success
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(context);
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public void Initializer_IdempotentInitialization_ReturnsSameContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(
            accessor,
            NullLogger<TenantContextInitializer>.Instance);

        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var traceId = "trace-123";
        var requestId = "req-456";

        // Act - initialize twice with identical inputs
        var context1 = initializer.InitializeRequest(scope, traceId, requestId);
        var context2 = initializer.InitializeRequest(
            scope,
            traceId,
            requestId,
            TenantAttributionInputs.FromExplicitScope(scope));

        // Assert - same context returned (idempotent)
        context2.Should().BeSameAs(context1);
        accessor.Current.Should().BeSameAs(context1);
    }

    [Fact]
    public void Initializer_ConflictingInitialization_Throws()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(
            accessor,
            NullLogger<TenantContextInitializer>.Instance);

        var scope1 = TenantScope.ForTenant(new TenantId("tenant-1"));
        var scope2 = TenantScope.ForTenant(new TenantId("tenant-2"));

        // Act - initialize with one scope, then attempt with different scope
        initializer.InitializeRequest(scope1, "trace-1", "req-1", TenantAttributionInputs.FromExplicitScope(scope1));
        var act = () => initializer.InitializeRequest(scope2, "trace-2", "req-2", TenantAttributionInputs.FromExplicitScope(scope2));

        // Assert - throws TenantContextConflictException for conflicting inputs
        act.Should().Throw<TenantContextConflictException>()
            .WithMessage("*already initialized with different inputs*");
    }

    [Fact]
    public void Initializer_AllExecutionKinds_CaptureCorrectMetadata()
    {
        // Test Request flow
        {
            var accessor = new AmbientTenantContextAccessor();
            var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
            var scope = TenantScope.ForTenant(new TenantId("tenant-1"));

            var context = initializer.InitializeRequest(
                scope,
                "trace-r",
                "req-r",
                TenantAttributionInputs.FromExplicitScope(scope));

            context.ExecutionKind.Should().Be(ExecutionKind.Request);
            context.TraceId.Should().Be("trace-r");
            context.RequestId.Should().Be("req-r");
            accessor.Clear();
        }

        // Test Background flow
        {
            var accessor = new AmbientTenantContextAccessor();
            var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
            var scope = TenantScope.ForTenant(new TenantId("tenant-2"));

            var context = initializer.InitializeBackground(
                scope,
                "trace-b",
                TenantAttributionInputs.FromExplicitScope(scope));

            context.ExecutionKind.Should().Be(ExecutionKind.Background);
            context.TraceId.Should().Be("trace-b");
            context.RequestId.Should().BeNull();
            accessor.Clear();
        }

        // Test Admin flow
        {
            var accessor = new AmbientTenantContextAccessor();
            var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
            var scope = TenantScope.ForSharedSystem();

            var context = initializer.InitializeAdmin(
                scope,
                "trace-a",
                TenantAttributionInputs.FromExplicitScope(scope));

            context.ExecutionKind.Should().Be(ExecutionKind.Admin);
            context.TraceId.Should().Be("trace-a");
            context.RequestId.Should().BeNull();
            accessor.Clear();
        }

        // Test Scripted flow
        {
            var accessor = new AmbientTenantContextAccessor();
            var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
            var scope = TenantScope.ForNoTenant(NoTenantReason.SystemMaintenance);

            var context = initializer.InitializeScripted(
                scope,
                "trace-s",
                TenantAttributionInputs.FromExplicitScope(scope));

            context.ExecutionKind.Should().Be(ExecutionKind.Scripted);
            context.TraceId.Should().Be("trace-s");
            context.RequestId.Should().BeNull();
            accessor.Clear();
        }
    }

    [Fact]
    public void Initializer_Clear_RemovesContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(
            accessor,
            NullLogger<TenantContextInitializer>.Instance);

        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        initializer.InitializeRequest(scope, "trace-1", "req-1", TenantAttributionInputs.FromExplicitScope(scope));

        accessor.IsInitialized.Should().BeTrue();

        // Act
        initializer.Clear();

        // Assert
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }
}
