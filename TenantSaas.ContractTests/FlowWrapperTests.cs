using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for flow wrappers (background, admin, scripted execution).
/// Verifies AC#1: Flow wrappers require explicit initialization and set execution kind/scope.
/// Verifies AC#2: Bypassed wrappers result in ContextInitialized refusal.
/// </summary>
public sealed class FlowWrapperTests
{
    #region AC#1 - Flow wrappers set correct execution kind and scope

    [Fact]
    public void BackgroundFlow_Should_SetCorrectExecutionKind()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var tenantId = new TenantId("tenant-123");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-bg-001";

        // Act
        using var flow = factory.CreateBackgroundFlow(scope, traceId);

        // Assert
        flow.Context.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Background);
        flow.Context.Scope.Should().Be(scope);
        flow.Context.TraceId.Should().Be(traceId);
    }

    [Fact]
    public void AdminFlow_Should_SetCorrectExecutionKind()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var tenantId = new TenantId("tenant-456");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-admin-001";

        // Act
        using var flow = factory.CreateAdminFlow(scope, traceId);

        // Assert
        flow.Context.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Admin);
        flow.Context.Scope.Should().Be(scope);
        flow.Context.TraceId.Should().Be(traceId);
    }

    [Fact]
    public void ScriptedFlow_Should_SetCorrectExecutionKind()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var tenantId = new TenantId("tenant-789");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-script-001";

        // Act
        using var flow = factory.CreateScriptedFlow(scope, traceId);

        // Assert
        flow.Context.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Scripted);
        flow.Context.Scope.Should().Be(scope);
        flow.Context.TraceId.Should().Be(traceId);
    }

    [Fact]
    public void FlowWrapper_Should_AcceptAttributionInputs()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var tenantId = new TenantId("tenant-attr");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-attr-001";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        using var flow = factory.CreateBackgroundFlow(scope, traceId, attributionInputs);

        // Assert
        flow.Context.AttributionInputs.Should().Be(attributionInputs);
    }

    #endregion

    #region AC#2 - Bypassed wrapper enforcement refusal

    [Fact]
    public void Enforcement_Should_RefuseWhenFlowWrapperBypassed()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var guard = new BoundaryGuard(
            NullLogger<BoundaryGuard>.Instance,
            new DefaultLogEnricher());

        // Context is NOT initialized (wrapper bypassed)

        // Act
        var result = guard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
    }

    #endregion

    #region Flow disposal clears ambient context

    [Fact]
    public void FlowDisposal_Should_ClearAmbientContext()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var accessor = services.GetRequiredService<ITenantContextAccessor>();
        var tenantId = new TenantId("tenant-dispose");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-dispose-001";

        // Act & Assert - context available during flow
        using (var flow = factory.CreateBackgroundFlow(scope, traceId))
        {
            accessor.IsInitialized.Should().BeTrue();
            accessor.Current.Should().NotBeNull();
        }

        // After disposal, context should be cleared
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public async Task FlowAsyncDisposal_Should_ClearAmbientContext()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var accessor = services.GetRequiredService<ITenantContextAccessor>();
        var tenantId = new TenantId("tenant-async-dispose");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-async-dispose-001";

        // Act & Assert - context available during flow
        await using (var flow = factory.CreateBackgroundFlow(scope, traceId))
        {
            accessor.IsInitialized.Should().BeTrue();
            accessor.Current.Should().NotBeNull();
        }

        // After disposal, context should be cleared
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public void DoubleDisposal_Should_BeIdempotent()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var tenantId = new TenantId("tenant-double");
        var scope = TenantScope.ForTenant(tenantId);
        var traceId = "trace-double-001";

        // Act
        var flow = factory.CreateBackgroundFlow(scope, traceId);
        flow.Dispose();

        // Assert - second disposal should not throw
        var act = () => flow.Dispose();
        act.Should().NotThrow();
    }

    #endregion

    #region Parallel flows remain isolated

    [Fact]
    public async Task ParallelFlows_Should_RemainIsolated()
    {
        // Arrange - shared provider with AsyncLocal isolation
        var tenantId1 = new TenantId("tenant-parallel-1");
        var tenantId2 = new TenantId("tenant-parallel-2");
        var scope1 = TenantScope.ForTenant(tenantId1);
        var scope2 = TenantScope.ForTenant(tenantId2);
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();

        TenantContext? capturedContext1 = null;
        TenantContext? capturedContext2 = null;

        using var ready1 = new ManualResetEventSlim(false);
        using var ready2 = new ManualResetEventSlim(false);
        using var release = new ManualResetEventSlim(false);

        // Act - run two parallel flows
        var task1 = Task.Run(() =>
        {
            using var flow = factory.CreateBackgroundFlow(scope1, "trace-parallel-1");
            ready1.Set();
            release.Wait();
            capturedContext1 = flow.Context;
        });

        var task2 = Task.Run(() =>
        {
            using var flow = factory.CreateAdminFlow(scope2, "trace-parallel-2");
            ready2.Set();
            release.Wait();
            capturedContext2 = flow.Context;
        });

        ready1.Wait();
        ready2.Wait();
        release.Set();

        await Task.WhenAll(task1, task2);

        // Assert - each flow has its own isolated context
        capturedContext1.Should().NotBeNull();
        capturedContext2.Should().NotBeNull();
        capturedContext1!.Scope.Should().Be(scope1);
        capturedContext2!.Scope.Should().Be(scope2);
        capturedContext1.ExecutionKind.Should().Be(ExecutionKind.Background);
        capturedContext2.ExecutionKind.Should().Be(ExecutionKind.Admin);
    }

    [Fact]
    public void FlowDisposal_Should_NotClearNewerContext()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var accessor = services.GetRequiredService<ITenantContextAccessor>();
        var scope1 = TenantScope.ForTenant(new TenantId("tenant-first"));
        var scope2 = TenantScope.ForTenant(new TenantId("tenant-second"));

        // Act
        var flow1 = factory.CreateBackgroundFlow(scope1, "trace-first");
        var flow2 = factory.CreateAdminFlow(scope2, "trace-second");

        // Assert - current should be second flow
        accessor.Current.Should().NotBeNull();
        accessor.Current!.Scope.Should().Be(scope2);

        // Disposing the first flow should not clear the newer context
        flow1.Dispose();
        accessor.Current.Should().NotBeNull();
        accessor.Current!.Scope.Should().Be(scope2);

        // Cleanup
        flow2.Dispose();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public async Task ParallelFlows_Should_NotContaminateEachOther()
    {
        // Arrange - AsyncLocal flows to Task.Run, so nested tasks see the parent context.
        // To create truly independent flows, we need separate service providers per task.
        // This test verifies that when we create independent providers, flows don't contaminate.
        var tenantId1 = new TenantId("tenant-contaminate-1");
        var tenantId2 = new TenantId("tenant-contaminate-2");
        var scope1 = TenantScope.ForTenant(tenantId1);
        var scope2 = TenantScope.ForTenant(tenantId2);

        TenantContext? outerContext = null;
        TenantContext? innerContext = null;
        TenantContext? outerAfterInner = null;

        // Start first flow with its own provider
        var services1 = CreateServices();
        var factory1 = services1.GetRequiredService<ITenantFlowFactory>();
        var accessor1 = services1.GetRequiredService<ITenantContextAccessor>();

        using (var flow1 = factory1.CreateBackgroundFlow(scope1, "trace-contam-1"))
        {
            outerContext = flow1.Context;

            // Start nested parallel task with its own provider (simulating separate flow)
            var task = Task.Run(() =>
            {
                var services2 = CreateServices();
                var factory2 = services2.GetRequiredService<ITenantFlowFactory>();
                using var innerFlow = factory2.CreateAdminFlow(scope2, "trace-contam-2");
                return innerFlow.Context;
            });

            innerContext = await task;
            outerAfterInner = accessor1.Current;
        }

        // Assert - outer context unchanged after inner flow completes
        outerContext.Should().NotBeNull();
        innerContext.Should().NotBeNull();
        outerContext!.Scope.Should().Be(scope1);
        innerContext!.Scope.Should().Be(scope2);
        outerAfterInner.Should().NotBeNull();
        outerAfterInner!.Scope.Should().Be(scope1); // Outer flow was not contaminated
    }

    #endregion

    #region SharedSystem and NoTenant scope support

    [Fact]
    public void BackgroundFlow_Should_SupportSharedSystemScope()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var scope = TenantScope.ForSharedSystem();
        var traceId = "trace-shared-001";

        // Act
        using var flow = factory.CreateBackgroundFlow(scope, traceId);

        // Assert
        flow.Context.Scope.Should().BeOfType<TenantScope.SharedSystem>();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Background);
    }

    [Fact]
    public void AdminFlow_Should_SupportNoTenantScope()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var reason = NoTenantReason.SystemMaintenance;
        var scope = TenantScope.ForNoTenant(reason);
        var traceId = "trace-notenant-001";

        // Act
        using var flow = factory.CreateAdminFlow(scope, traceId);

        // Assert
        flow.Context.Scope.Should().BeOfType<TenantScope.NoTenant>();
        ((TenantScope.NoTenant)flow.Context.Scope).Reason.Should().Be(reason);
    }

    #endregion

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        services.AddSingleton<IMutableTenantContextAccessor, AmbientTenantContextAccessor>();
        services.AddSingleton<ITenantContextAccessor>(sp => sp.GetRequiredService<IMutableTenantContextAccessor>());
        services.AddSingleton<ITenantContextInitializer, TenantContextInitializer>();
        services.AddSingleton<ITenantFlowFactory, TenantFlowFactory>();
        return services.BuildServiceProvider();
    }
}
