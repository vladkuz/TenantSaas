using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Logging;
using TenantSaas.Core.Enforcement;
using TenantSaas.Core.Tenancy;
using TenantSaas.Core.Logging;
using TenantSaas.Core.Errors;
using Xunit;
using static TenantSaas.Core.Errors.ProblemDetailsExtensions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Contract tests for ContextInitialized invariant.
/// </summary>
public class ContextInitializedTests
{
    private static IBoundaryGuard CreateBoundaryGuard()
    {
        var logger = NullLogger<BoundaryGuard>.Instance;
        var enricher = new DefaultLogEnricher();
        return new BoundaryGuard(logger, enricher);
    }
    [Fact]
    public void EnforcementResult_Success_ShouldContainContext()
    {
        // Arrange
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");

        // Act
        var result = EnforcementResult.Success(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(context);
        result.InvariantCode.Should().BeNull();
        result.TraceId.Should().Be("trace-123");
        result.Detail.Should().BeNull();
    }

    [Fact]
    public void EnforcementResult_Failure_ShouldContainInvariantCode()
    {
        // Arrange & Act
        var result = EnforcementResult.Failure(
            InvariantCode.ContextInitialized,
            "trace-123",
            "Context not initialized");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Context.Should().BeNull();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().Be("trace-123");
        result.Detail.Should().Be("Context not initialized");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Uninitialized_ReturnsFailure()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().NotBeNullOrWhiteSpace();
        result.Detail.Should().Contain("context must be initialized");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Initialized_ReturnsSuccess()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        accessor.Set(context);
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireContext(accessor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(context);
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public void BoundaryGuard_RequireContext_WithOverrideTraceId_UsesProvidedTraceId()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var overrideTraceId = "custom-trace-456";
        var boundaryGuard = CreateBoundaryGuard();

        // Act
        var result = boundaryGuard.RequireContext(accessor, overrideTraceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.TraceId.Should().Be(overrideTraceId);
    }

    [Fact]
    public void AmbientAccessor_InitiallyUninitialized()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();

        // Act & Assert
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public void AmbientAccessor_AfterSet_IsInitialized()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");

        // Act
        accessor.Set(context);

        // Assert
        accessor.IsInitialized.Should().BeTrue();
        accessor.Current.Should().Be(context);
    }

    [Fact]
    public void AmbientAccessor_AfterClear_IsUninitialized()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        accessor.Set(context);

        // Act
        accessor.Clear();

        // Assert
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public async Task AmbientAccessor_PropagatesAcrossAwait()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        accessor.Set(context);

        // Act
        await Task.Delay(1);
        var retrieved = accessor.Current;

        // Assert
        retrieved.Should().Be(context);
        accessor.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void ExplicitAccessor_WithContext_IsInitialized()
    {
        // Arrange
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        var accessor = ExplicitTenantContextAccessor.WithContext(context);

        // Act & Assert
        accessor.IsInitialized.Should().BeTrue();
        accessor.Current.Should().Be(context);
    }

    [Fact]
    public void ExplicitAccessor_WithoutContext_IsUninitialized()
    {
        // Arrange
        var accessor = new ExplicitTenantContextAccessor();

        // Act & Assert
        accessor.IsInitialized.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public void ProblemDetailsFactory_ContextInitialized_ReturnsHttp401()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForContextNotInitialized("trace-123");

        // Assert
        problemDetails.Status.Should().Be(401);
        problemDetails.Extensions[InvariantCodeKey].Should().Be(Abstractions.Invariants.InvariantCode.ContextInitialized);
        problemDetails.Extensions[TraceId].Should().Be("trace-123");
        problemDetails.Type.Should().Contain("context-");
        problemDetails.Detail.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ProblemDetailsFactory_ContextInitialized_IncludesInvariantCodeAndTraceId()
    {
        // Arrange & Act
        var problemDetails = ProblemDetailsFactory.ForContextNotInitialized("trace-456");

        // Assert
        problemDetails.Extensions.Should().ContainKey(InvariantCodeKey);
        problemDetails.Extensions.Should().ContainKey(TraceId);
        problemDetails.Extensions[TraceId].Should().Be("trace-456");
    }

    [Fact]
    public void ProblemDetailsFactory_WithUnknownInvariantCode_ThrowsKeyNotFoundException()
    {
        // Arrange & Act & Assert
        var act = () => ProblemDetailsFactory.FromInvariantViolation(
            "UnknownInvariant",
            "trace-123");
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void EnforcementResult_Success_ThrowsOnNullContext()
    {
        // Act & Assert
        var act = () => EnforcementResult.Success(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnforcementResult_Failure_ThrowsOnInvalidInvariantCode(string? invalidCode)
    {
        // Act & Assert
        var act = () => EnforcementResult.Failure(invalidCode!, "trace-123", "detail");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("invariantCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnforcementResult_Failure_ThrowsOnInvalidTraceId(string? invalidTraceId)
    {
        // Act & Assert
        var act = () => EnforcementResult.Failure(InvariantCode.ContextInitialized, invalidTraceId!, "detail");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("traceId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnforcementResult_Failure_ThrowsOnInvalidDetail(string? invalidDetail)
    {
        // Act & Assert
        var act = () => EnforcementResult.Failure(InvariantCode.ContextInitialized, "trace-123", invalidDetail!);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("detail");
    }

    [Fact]
    public void AmbientAccessor_Set_ThrowsOnNullContext()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();

        // Act & Assert
        var act = () => accessor.Set(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_ThrowsOnNullAccessor()
    {
        // Arrange
        var boundaryGuard = CreateBoundaryGuard();

        // Act & Assert
        var act = () => boundaryGuard.RequireContext(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("accessor");
    }

    [Fact]
    public void AmbientAccessor_ImplementsIMutableTenantContextAccessor()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();

        // Assert - verifies the interface hierarchy is correct
        accessor.Should().BeAssignableTo<ITenantContextAccessor>();
        accessor.Should().BeAssignableTo<IMutableTenantContextAccessor>();
    }
}
