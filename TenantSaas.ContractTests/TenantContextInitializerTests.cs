using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Tenancy;
using Xunit;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for TenantContextInitializer - the single initialization primitive.
/// </summary>
public class TenantContextInitializerTests
{
    [Fact]
    public void InitializeRequest_WithValidInputs_ShouldInitializeContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var traceId = "trace-123";
        var requestId = "req-456";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        var result = initializer.InitializeRequest(scope, traceId, requestId, attributionInputs);

        // Assert
        result.Should().NotBeNull();
        result.Scope.Should().Be(scope);
        result.ExecutionKind.Should().Be(ExecutionKind.Request);
        result.TraceId.Should().Be(traceId);
        result.RequestId.Should().Be(requestId);
        result.AttributionInputs.Should().Be(attributionInputs);
        accessor.Current.Should().Be(result);
    }

    [Fact]
    public void InitializeBackground_WithValidInputs_ShouldInitializeContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForTenant(new TenantId("tenant-bg-1"));
        var traceId = "trace-bg-789";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        var result = initializer.InitializeBackground(scope, traceId, attributionInputs);

        // Assert
        result.Should().NotBeNull();
        result.Scope.Should().Be(scope);
        result.ExecutionKind.Should().Be(ExecutionKind.Background);
        result.TraceId.Should().Be(traceId);
        result.RequestId.Should().BeNull();
        result.AttributionInputs.Should().Be(attributionInputs);
        accessor.Current.Should().Be(result);
    }

    [Fact]
    public void InitializeAdmin_WithValidInputs_ShouldInitializeContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForSharedSystem();
        var traceId = "trace-admin-999";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        var result = initializer.InitializeAdmin(scope, traceId, attributionInputs);

        // Assert
        result.Should().NotBeNull();
        result.Scope.Should().Be(scope);
        result.ExecutionKind.Should().Be(ExecutionKind.Admin);
        result.TraceId.Should().Be(traceId);
        result.RequestId.Should().BeNull();
        result.AttributionInputs.Should().Be(attributionInputs);
        accessor.Current.Should().Be(result);
    }

    [Fact]
    public void InitializeScripted_WithValidInputs_ShouldInitializeContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForNoTenant(NoTenantReason.SystemMaintenance);
        var traceId = "trace-script-321";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        var result = initializer.InitializeScripted(scope, traceId, attributionInputs);

        // Assert
        result.Should().NotBeNull();
        result.Scope.Should().Be(scope);
        result.ExecutionKind.Should().Be(ExecutionKind.Scripted);
        result.TraceId.Should().Be(traceId);
        result.RequestId.Should().BeNull();
        result.AttributionInputs.Should().Be(attributionInputs);
        accessor.Current.Should().Be(result);
    }

    [Fact]
    public void InitializeRequest_WhenAlreadyInitialized_ShouldBeIdempotentAndReturnExisting()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope1 = TenantScope.ForTenant(new TenantId("tenant-1"));
        var traceId1 = "trace-first";
        var requestId1 = "req-first";
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope1);

        // Act - first initialization
        var firstContext = initializer.InitializeRequest(scope1, traceId1, requestId1, attributionInputs);

        // Act - second initialization with same inputs
        var secondContext = initializer.InitializeRequest(scope1, traceId1, requestId1, attributionInputs);

        // Assert - returns the same context (idempotent)
        secondContext.Should().BeSameAs(firstContext);
        accessor.Current.Should().Be(firstContext);
    }

    [Fact]
    public void InitializeRequest_WhenAlreadyInitializedWithDifferentInputs_ShouldThrow()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope1 = TenantScope.ForTenant(new TenantId("tenant-1"));
        var scope2 = TenantScope.ForTenant(new TenantId("tenant-2"));
        var traceId = "trace-123";
        var requestId = "req-456";

        // Act - first initialization
        initializer.InitializeRequest(scope1, traceId, requestId, TenantAttributionInputs.FromExplicitScope(scope1));

        // Act & Assert - second initialization with different scope should throw
        var act = () => initializer.InitializeRequest(scope2, traceId, requestId, TenantAttributionInputs.FromExplicitScope(scope2));
        act.Should().Throw<TenantContextConflictException>()
            .WithMessage("*already initialized*");
    }

    [Fact]
    public void Clear_ShouldRemoveCurrentContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        initializer.InitializeRequest(scope, "trace-123", "req-456", TenantAttributionInputs.FromExplicitScope(scope));

        // Act
        initializer.Clear();

        // Assert
        accessor.Current.Should().BeNull();
        accessor.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void InitializeRequest_WithExplicitAccessor_ShouldInitializeContext()
    {
        // Arrange
        var accessor = new ExplicitTenantContextAccessor();
        var initializer = new TenantContextInitializer(accessor, NullLogger<TenantContextInitializer>.Instance);
        var scope = TenantScope.ForTenant(new TenantId("tenant-explicit-1"));
        var attributionInputs = TenantAttributionInputs.FromExplicitScope(scope);

        // Act
        var result = initializer.InitializeRequest(scope, "trace-explicit", "req-explicit", attributionInputs);

        // Assert
        result.Should().NotBeNull();
        result.AttributionInputs.Should().Be(attributionInputs);
        accessor.Current.Should().Be(result);
    }
}
