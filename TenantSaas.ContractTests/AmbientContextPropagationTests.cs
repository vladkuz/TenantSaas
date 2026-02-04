using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for ambient context propagation with deterministic async behavior.
/// </summary>
/// <remarks>
/// AC#1: Ambient context propagates deterministically across async boundaries within same flow.
/// AC#2: New execution flows start empty with no leakage from previous flows.
/// </remarks>
public class AmbientContextPropagationTests
{
    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }
    // AC#1: Deterministic async propagation tests

    [Fact]
    public async Task AmbientContext_PropagatesAcrossSimpleAwait()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-1", "req-1");
        accessor.Set(context);

        // Act
        await Task.Delay(1);
        var retrieved = accessor.Current;

        // Assert - context should flow across await
        retrieved.Should().Be(context);
        accessor.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task AmbientContext_PropagatesAcrossTaskRun()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-2"));
        var context = TenantContext.ForRequest(scope, "trace-2", "req-2");
        accessor.Set(context);

        // Act - Task.Run creates new execution context but AsyncLocal should flow
        var retrieved = await Task.Run(() => accessor.Current);

        // Assert - context should propagate to Task.Run
        retrieved.Should().Be(context);
    }

    [Fact]
    public async Task AmbientContext_PropagatesAcrossNestedAsyncCalls()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-3"));
        var context = TenantContext.ForRequest(scope, "trace-3", "req-3");
        accessor.Set(context);

        // Act - multiple async layers
        var retrieved = await Level1Async(accessor);

        // Assert - context should flow through nested calls
        retrieved.Should().Be(context);

        static async Task<TenantContext?> Level1Async(AmbientTenantContextAccessor acc)
        {
            await Task.Delay(1);
            return await Level2Async(acc);
        }

        static async Task<TenantContext?> Level2Async(AmbientTenantContextAccessor acc)
        {
            await Task.Delay(1);
            return acc.Current;
        }
    }

    [Fact]
    public async Task AmbientContext_PropagatesAcrossTaskWhenAll()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-4"));
        var context = TenantContext.ForRequest(scope, "trace-4", "req-4");
        accessor.Set(context);

        // Act - parallel tasks should all see same context
        var tasks = new[]
        {
            Task.Run(() => accessor.Current),
            Task.Run(() => accessor.Current),
            Task.Run(() => accessor.Current)
        };
        var results = await Task.WhenAll(tasks);

        // Assert - all parallel tasks see same context
        results.Should().OnlyContain(c => c == context);
    }

    [Fact]
    public async Task AmbientContext_PropagatesAcrossAsyncDelayBoundary()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-5"));
        var context = TenantContext.ForRequest(scope, "trace-5", "req-5");
        accessor.Set(context);

        // Act - AsyncLocal flows across async boundaries regardless of synchronization context
        await Task.Delay(1);
        var retrieved = accessor.Current;

        // Assert - context survives async boundary
        retrieved.Should().Be(context);
    }

    // AC#2: Zero leakage across new flows tests

    [Fact]
    public void AmbientContext_NewFlowStartsEmpty()
    {
        // Arrange - simulate previous flow cleanup
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-old"));
        var oldContext = TenantContext.ForRequest(scope, "trace-old", "req-old");
        accessor.Set(oldContext);
        accessor.Clear(); // Explicit cleanup as middleware would do

        // Act - new flow starts
        var retrieved = accessor.Current;

        // Assert - no leakage from previous flow
        retrieved.Should().BeNull();
        accessor.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task AmbientContext_ChildTasksInheritContext_ButNewFlowsAfterClearDoNot()
    {
        // Arrange - set context in parent flow
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-6"));
        var context = TenantContext.ForRequest(scope, "trace-6", "req-6");
        accessor.Set(context);

        // Act 1 - child task DOES inherit context (this is expected AsyncLocal behavior)
        var retrievedInChildTask = await Task.Run(() => accessor.Current);

        // Act 2 - simulate request completion with cleanup
        accessor.Clear();

        // Act 3 - simulate new request starting (new logical flow)
        await Task.Delay(1); // async gap simulating time between requests
        var retrievedInNewFlow = accessor.Current;

        // Assert
        retrievedInChildTask.Should().Be(context, "child tasks inherit parent context via AsyncLocal");
        retrievedInNewFlow.Should().BeNull("after Clear(), new flow must start without context");
    }

    [Fact]
    public async Task AmbientContext_ClearPreventsLeakageToNextRequest()
    {
        // Arrange - simulate request 1
        var accessor = new AmbientTenantContextAccessor();
        var scope1 = TenantScope.ForTenant(new TenantId("tenant-req1"));
        var context1 = TenantContext.ForRequest(scope1, "trace-req1", "req-1");
        accessor.Set(context1);

        // Simulate request 1 completion with cleanup
        accessor.Clear();

        // Act - simulate request 2 starting
        await Task.Delay(1); // Simulate async gap
        var leakedContext = accessor.Current;

        // Initialize request 2
        var scope2 = TenantScope.ForTenant(new TenantId("tenant-req2"));
        var context2 = TenantContext.ForRequest(scope2, "trace-req2", "req-2");
        accessor.Set(context2);
        var request2Context = accessor.Current;

        // Assert - no leakage between requests
        leakedContext.Should().BeNull("context must not leak after Clear()");
        request2Context.Should().Be(context2);
    }

    // AC#1: Enforcement equivalence with explicit context passing

    [Fact]
    public async Task AmbientEnforcement_MatchesExplicitPassing_WhenContextPresent()
    {
        // Arrange
        var ambientAccessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-7"));
        var context = TenantContext.ForRequest(scope, "trace-7", "req-7");
        ambientAccessor.Set(context);

        var boundaryGuard = CreateBoundaryGuard();

        // Act - test both ambient and explicit paths
        var ambientResult = await Task.Run(() => boundaryGuard.RequireContext(ambientAccessor));
        var explicitResult = await Task.Run(() => boundaryGuard.RequireContext(context));

        // Assert - both should succeed with same context
        ambientResult.IsSuccess.Should().BeTrue();
        explicitResult.IsSuccess.Should().BeTrue();
        ambientResult.Context.Should().Be(context);
        explicitResult.Context.Should().Be(context);
    }

    [Fact]
    public async Task AmbientEnforcement_MatchesExplicitPassing_WhenContextMissing()
    {
        // Arrange
        var ambientAccessor = new AmbientTenantContextAccessor();
        var boundaryGuard = CreateBoundaryGuard();

        // Act - test both ambient and explicit paths without context
        var ambientResult = await Task.Run(() => boundaryGuard.RequireContext(ambientAccessor));
        var explicitResult = await Task.Run(() => boundaryGuard.RequireContext((TenantContext?)null));

        // Assert - both should fail with ContextInitialized invariant
        ambientResult.IsSuccess.Should().BeFalse();
        explicitResult.IsSuccess.Should().BeFalse();
        ambientResult.InvariantCode.Should().Be("ContextInitialized");
        explicitResult.InvariantCode.Should().Be("ContextInitialized");
    }

    [Fact]
    public async Task AmbientEnforcement_ExplicitContextTakesPrecedence()
    {
        // Arrange
        var ambientAccessor = new AmbientTenantContextAccessor();
        var ambientScope = TenantScope.ForTenant(new TenantId("ambient-tenant"));
        var ambientContext = TenantContext.ForRequest(ambientScope, "trace-ambient", "req-ambient");
        ambientAccessor.Set(ambientContext);

        var explicitScope = TenantScope.ForTenant(new TenantId("explicit-tenant"));
        var explicitContext = TenantContext.ForRequest(explicitScope, "trace-explicit", "req-explicit");

        var boundaryGuard = CreateBoundaryGuard();

        // Act - provide both ambient and explicit context
        var result = await Task.Run(() => boundaryGuard.RequireContext(ambientAccessor, explicitContext));

        // Assert - explicit should win
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(explicitContext, "explicit context must take precedence over ambient");
    }
}
