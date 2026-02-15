using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Tenancy;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for Story 4.5: Execution Kind and Scope in Context for Downstream Use.
/// Verifies AC#1: ExecutionKind and TenantScope are first-class fields aligned with trust contract taxonomy.
/// Verifies AC#2: Missing execution kind or scope results in refusal with Problem Details.
/// Verifies AC#3: Operations fail when execution kind or scope is missing.
/// </summary>
public sealed class ExecutionKindAndScopeTests
{
    #region AC#1 - ExecutionKind and Scope are always present in TenantContext

    [Fact]
    public void RequestContext_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var tenantId = new TenantId("tenant-req-001");
        var scope = TenantScope.ForTenant(tenantId);

        // Act
        var context = TenantContext.ForRequest(scope, "trace-001", "req-001");

        // Assert
        context.ExecutionKind.Should().NotBeNull();
        context.ExecutionKind.Should().Be(ExecutionKind.Request);
        context.Scope.Should().NotBeNull();
        context.Scope.Should().Be(scope);
    }

    [Fact]
    public void BackgroundContext_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var tenantId = new TenantId("tenant-bg-001");
        var scope = TenantScope.ForTenant(tenantId);

        // Act
        var context = TenantContext.ForBackground(scope, "trace-002");

        // Assert
        context.ExecutionKind.Should().NotBeNull();
        context.ExecutionKind.Should().Be(ExecutionKind.Background);
        context.Scope.Should().NotBeNull();
        context.Scope.Should().Be(scope);
    }

    [Fact]
    public void AdminContext_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var tenantId = new TenantId("tenant-admin-001");
        var scope = TenantScope.ForTenant(tenantId);

        // Act
        var context = TenantContext.ForAdmin(scope, "trace-003");

        // Assert
        context.ExecutionKind.Should().NotBeNull();
        context.ExecutionKind.Should().Be(ExecutionKind.Admin);
        context.Scope.Should().NotBeNull();
        context.Scope.Should().Be(scope);
    }

    [Fact]
    public void ScriptedContext_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var tenantId = new TenantId("tenant-script-001");
        var scope = TenantScope.ForTenant(tenantId);

        // Act
        var context = TenantContext.ForScripted(scope, "trace-004");

        // Assert
        context.ExecutionKind.Should().NotBeNull();
        context.ExecutionKind.Should().Be(ExecutionKind.Scripted);
        context.Scope.Should().NotBeNull();
        context.Scope.Should().Be(scope);
    }

    [Fact]
    public void TenantScope_Context_Should_MapToTenantScopeType()
    {
        // Arrange
        var scope = TenantScope.ForTenant(new TenantId("tenant-scope"));

        // Act
        var context = TenantContext.ForRequest(scope, "trace-005", "req-005");

        // Assert
        context.Scope.Should().BeOfType<TenantScope.Tenant>();
    }

    [Fact]
    public void SharedSystemScope_Context_Should_MapToSharedSystemScopeType()
    {
        // Arrange
        var scope = TenantScope.ForSharedSystem();

        // Act
        var context = TenantContext.ForAdmin(scope, "trace-006");

        // Assert
        context.Scope.Should().BeOfType<TenantScope.SharedSystem>();
    }

    [Fact]
    public void NoTenantScope_Context_Should_MapToNoTenantScopeType()
    {
        // Arrange
        var scope = TenantScope.ForNoTenant(NoTenantReason.HealthCheck);

        // Act
        var context = TenantContext.ForRequest(scope, "trace-007", "req-007");

        // Assert
        context.Scope.Should().BeOfType<TenantScope.NoTenant>();
    }

    #endregion

    #region AC#1 - ExecutionKind values align with trust contract taxonomy

    [Fact]
    public void ExecutionKind_Request_Should_MatchTaxonomy()
    {
        // Assert - verify value aligns with trust contract taxonomy
        ExecutionKind.Request.Value.Should().Be(ExecutionKind.RequestValue);
        ExecutionKind.Request.DisplayName.Should().Be("Request");
    }

    [Fact]
    public void ExecutionKind_Background_Should_MatchTaxonomy()
    {
        ExecutionKind.Background.Value.Should().Be(ExecutionKind.BackgroundValue);
        ExecutionKind.Background.DisplayName.Should().Be("Background");
    }

    [Fact]
    public void ExecutionKind_Admin_Should_MatchTaxonomy()
    {
        ExecutionKind.Admin.Value.Should().Be(ExecutionKind.AdminValue);
        ExecutionKind.Admin.DisplayName.Should().Be("Admin");
    }

    [Fact]
    public void ExecutionKind_Scripted_Should_MatchTaxonomy()
    {
        ExecutionKind.Scripted.Value.Should().Be(ExecutionKind.ScriptedValue);
        ExecutionKind.Scripted.DisplayName.Should().Be("Scripted");
    }

    [Fact]
    public void ExecutionKind_All_Should_ContainFourEntries()
    {
        // Trust contract defines exactly 4 execution kinds
        ExecutionKind.All.Should().HaveCount(4);
        ExecutionKind.All.Select(k => k.Value).Should().Contain(
        [
            ExecutionKind.RequestValue,
            ExecutionKind.BackgroundValue,
            ExecutionKind.AdminValue,
            ExecutionKind.ScriptedValue
        ]);
    }

    #endregion

    #region AC#1 - Flow wrappers always set ExecutionKind and Scope

    [Fact]
    public void FlowFactory_BackgroundFlow_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var scope = TenantScope.ForTenant(new TenantId("tenant-flow-bg"));

        // Act
        using var flow = factory.CreateBackgroundFlow(scope, "trace-flow-bg");

        // Assert
        flow.Context.ExecutionKind.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Background);
        flow.Context.Scope.Should().NotBeNull();
        flow.Context.Scope.Should().Be(scope);
    }

    [Fact]
    public void FlowFactory_AdminFlow_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var scope = TenantScope.ForSharedSystem();

        // Act
        using var flow = factory.CreateAdminFlow(scope, "trace-flow-admin");

        // Assert
        flow.Context.ExecutionKind.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Admin);
        flow.Context.Scope.Should().NotBeNull();
        flow.Context.Scope.Should().BeOfType<TenantScope.SharedSystem>();
    }

    [Fact]
    public void FlowFactory_ScriptedFlow_Should_HaveExecutionKindAndScope()
    {
        // Arrange
        var services = CreateServices();
        var factory = services.GetRequiredService<ITenantFlowFactory>();
        var scope = TenantScope.ForTenant(new TenantId("tenant-flow-script"));

        // Act
        using var flow = factory.CreateScriptedFlow(scope, "trace-flow-script");

        // Assert
        flow.Context.ExecutionKind.Should().NotBeNull();
        flow.Context.ExecutionKind.Should().Be(ExecutionKind.Scripted);
        flow.Context.Scope.Should().NotBeNull();
    }

    #endregion

    #region AC#2 and AC#3 - Enforcement refusal when context is null (implicit missing kind/scope)

    [Fact]
    public void BoundaryGuard_RequireContext_NullContext_Should_RefuseWithProblemDetails()
    {
        // Arrange
        var guard = CreateBoundaryGuard();
        TenantContext? nullContext = null;
        var traceId = "trace-refuse-001";

        // Act
        var result = guard.RequireContext(nullContext, traceId);

        // Assert - refusal with invariant_code and trace_id per AC#2
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().Be(traceId);
        result.Detail.Should().Contain("context must be initialized");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_UninitializedAccessor_Should_RefuseWithProblemDetails()
    {
        // Arrange
        var guard = CreateBoundaryGuard();
        var accessor = new AmbientTenantContextAccessor();
        var traceId = "trace-refuse-002";

        // Act
        var result = guard.RequireContext(accessor, traceId);

        // Assert - refusal with invariant_code and trace_id per AC#2
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().Be(traceId);
    }

    [Fact]
    public void EnforcementResult_Failure_Should_ContainInvariantCodeAndTraceId()
    {
        // Arrange & Act
        var result = EnforcementResult.Failure(
            InvariantCode.ContextInitialized,
            "trace-pd-001",
            "Context not initialized");

        // Assert - Problem Details shape per AC#2
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().Be("trace-pd-001");
        result.Detail.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void BoundaryGuard_RequireContext_ExplicitContext_Should_TakePrecedenceOverAmbient()
    {
        // Arrange
        var guard = CreateBoundaryGuard();
        var accessor = new AmbientTenantContextAccessor();

        // Set ambient context with Request kind
        var ambientScope = TenantScope.ForTenant(new TenantId("ambient-tenant"));
        var ambientContext = TenantContext.ForRequest(ambientScope, "trace-ambient", "req-ambient");
        accessor.Set(ambientContext);

        // Create explicit context with Admin kind
        var explicitScope = TenantScope.ForSharedSystem();
        var explicitContext = TenantContext.ForAdmin(explicitScope, "trace-explicit");

        // Act - explicit context should win
        var result = guard.RequireContext(accessor, explicitContext);

        // Assert - explicit context takes precedence
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(explicitContext);
        result.Context!.ExecutionKind.Should().Be(ExecutionKind.Admin);
        result.Context.Scope.Should().BeOfType<TenantScope.SharedSystem>();
    }

    [Fact]
    public void BoundaryGuard_RequireContext_NullExplicit_Should_FallbackToAmbient()
    {
        // Arrange
        var guard = CreateBoundaryGuard();
        var accessor = new AmbientTenantContextAccessor();

        // Set ambient context
        var ambientScope = TenantScope.ForTenant(new TenantId("ambient-tenant"));
        var ambientContext = TenantContext.ForRequest(ambientScope, "trace-ambient", "req-ambient");
        accessor.Set(ambientContext);

        // Act - null explicit should fallback to ambient
        var result = guard.RequireContext(accessor, explicitContext: null);

        // Assert - ambient context used as fallback
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(ambientContext);
        result.Context!.ExecutionKind.Should().Be(ExecutionKind.Request);
    }

    #endregion

    #region AC#2/AC#3 - TenantContext construction enforces valid execution kind and scope

    [Fact]
    public void TenantContext_ForRequest_Should_RejectNullScope()
    {
        // Arrange & Act
        var act = () => TenantContext.ForRequest(null!, "trace-001", "req-001");

        // Assert - cannot create context without scope (AC#3)
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantContext_ForBackground_Should_RejectNullScope()
    {
        // Arrange & Act
        var act = () => TenantContext.ForBackground(null!, "trace-001");

        // Assert - cannot create context without scope (AC#3)
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantContext_ForAdmin_Should_RejectNullScope()
    {
        // Arrange & Act
        var act = () => TenantContext.ForAdmin(null!, "trace-001");

        // Assert - cannot create context without scope (AC#3)
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantContext_ForScripted_Should_RejectNullScope()
    {
        // Arrange & Act
        var act = () => TenantContext.ForScripted(null!, "trace-001");

        // Assert - cannot create context without scope (AC#3)
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantContext_ForRequest_Should_RejectNullTraceId()
    {
        // Arrange
        var scope = TenantScope.ForTenant(new TenantId("tenant-001"));

        // Act
        var act = () => TenantContext.ForRequest(scope, null!, "req-001");

        // Assert - traceId is required for correlation
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TenantContext_ForRequest_Should_RejectNullRequestId()
    {
        // Arrange
        var scope = TenantScope.ForTenant(new TenantId("tenant-001"));

        // Act
        var act = () => TenantContext.ForRequest(scope, "trace-001", null!);

        // Assert - requestId is required for request execution kind
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region AC#1 - Downstream logging includes ExecutionKind and ScopeType

    [Fact]
    public void DefaultLogEnricher_Should_EmitExecutionKindAndScopeType_ForRequestContext()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-001"));
        var context = TenantContext.ForRequest(scope, "trace-log-001", "req-log-001");

        // Act
        var logEvent = enricher.Enrich(context, "TestEvent");

        // Assert - downstream logs must include ExecutionKind and ScopeType per AC#1
        logEvent.ExecutionKind.Should().Be(ExecutionKind.RequestValue);
        logEvent.ScopeType.Should().Be("Tenant");
    }

    [Fact]
    public void DefaultLogEnricher_Should_EmitExecutionKindAndScopeType_ForBackgroundContext()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForTenant(new TenantId("tenant-log-002"));
        var context = TenantContext.ForBackground(scope, "trace-log-002");

        // Act
        var logEvent = enricher.Enrich(context, "TestEvent");

        // Assert
        logEvent.ExecutionKind.Should().Be(ExecutionKind.BackgroundValue);
        logEvent.ScopeType.Should().Be("Tenant");
    }

    [Fact]
    public void DefaultLogEnricher_Should_EmitExecutionKindAndScopeType_ForAdminContext()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForSharedSystem();
        var context = TenantContext.ForAdmin(scope, "trace-log-003");

        // Act
        var logEvent = enricher.Enrich(context, "TestEvent");

        // Assert
        logEvent.ExecutionKind.Should().Be(ExecutionKind.AdminValue);
        logEvent.ScopeType.Should().Be("SharedSystem");
    }

    [Fact]
    public void DefaultLogEnricher_Should_EmitExecutionKindAndScopeType_ForScriptedContext()
    {
        // Arrange
        var enricher = new DefaultLogEnricher();
        var scope = TenantScope.ForNoTenant(NoTenantReason.HealthCheck);
        var context = TenantContext.ForScripted(scope, "trace-log-004");

        // Act
        var logEvent = enricher.Enrich(context, "TestEvent");

        // Assert
        logEvent.ExecutionKind.Should().Be(ExecutionKind.ScriptedValue);
        logEvent.ScopeType.Should().Be("NoTenant");
    }

    [Fact]
    public void StructuredLogEvent_Should_RequireExecutionKindAndScopeType()
    {
        // Verify the contract via reflection: ExecutionKind and ScopeType use 'required' keyword
        var executionKindProp = typeof(StructuredLogEvent).GetProperty(nameof(StructuredLogEvent.ExecutionKind))!;
        var scopeTypeProp = typeof(StructuredLogEvent).GetProperty(nameof(StructuredLogEvent.ScopeType))!;

        // C# 'required' modifier is represented by RequiredMemberAttribute on the type
        // and the property being in the RequiredMembers list
        var requiredAttr = typeof(StructuredLogEvent)
            .GetCustomAttributes(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), false);
        requiredAttr.Should().NotBeEmpty("StructuredLogEvent should have required members");

        // Also verify the properties exist and are not nullable
        executionKindProp.PropertyType.Should().Be(typeof(string));
        scopeTypeProp.PropertyType.Should().Be(typeof(string));

        // Functional test: enricher always populates these fields
        var enricher = new DefaultLogEnricher();
        var context = TenantContext.ForRequest(
            TenantScope.ForTenant(new TenantId("test")),
            "trace-required",
            "req-required");
        var logEvent = enricher.Enrich(context, "TestEvent");

        logEvent.ExecutionKind.Should().NotBeNullOrWhiteSpace();
        logEvent.ScopeType.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Test Helpers

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IMutableTenantContextAccessor, AmbientTenantContextAccessor>();
        services.AddSingleton<ITenantContextAccessor>(sp => sp.GetRequiredService<IMutableTenantContextAccessor>());
        services.AddSingleton<ILogEnricher, DefaultLogEnricher>();
        services.AddSingleton<ITenantContextInitializer, TenantContextInitializer>();
        services.AddSingleton<ITenantFlowFactory, TenantFlowFactory>();
        return services.BuildServiceProvider();
    }

    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }

    #endregion
}
