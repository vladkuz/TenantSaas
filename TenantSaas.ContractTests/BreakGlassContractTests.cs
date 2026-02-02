using System.Reflection;
using FluentAssertions;
using TenantSaas.Abstractions.BreakGlass;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTests;

public sealed class BreakGlassContractTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Declaration_Requires_ActorId(string? actorId)
    {
        Action act = () => new BreakGlassDeclaration(
            actorId!,
            reason: "Emergency access",
            declaredScope: TrustContractV1.ScopeSharedSystem,
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("ActorId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Declaration_Requires_Reason(string? reason)
    {
        Action act = () => new BreakGlassDeclaration(
            actorId: "admin@example.com",
            reason: reason!,
            declaredScope: TrustContractV1.ScopeSharedSystem,
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("Reason");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Declaration_Requires_DeclaredScope(string? declaredScope)
    {
        Action act = () => new BreakGlassDeclaration(
            actorId: "admin@example.com",
            reason: "Emergency access",
            declaredScope: declaredScope!,
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("DeclaredScope");
    }

    [Fact]
    public void Validator_NullDeclaration_Returns_Invalid()
    {
        var result = BreakGlassValidator.Validate(null);

        result.IsValid.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.BreakGlassExplicitAndAudited);
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Validator_ValidDeclaration_Returns_Valid()
    {
        var declaration = new BreakGlassDeclaration(
            actorId: "admin@example.com",
            reason: "Emergency access",
            declaredScope: TrustContractV1.ScopeSharedSystem,
            targetTenantRef: null,
            timestamp: DateTimeOffset.UtcNow);

        var result = BreakGlassValidator.Validate(declaration);

        result.IsValid.Should().BeTrue();
        result.InvariantCode.Should().BeNull();
        result.Reason.Should().BeNull();
    }

    [Fact]
    public void AuditEvent_Includes_Actor_From_Declaration()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.Actor.Should().Be(declaration.ActorId);
    }

    [Fact]
    public void AuditEvent_Includes_Reason_From_Declaration()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.Reason.Should().Be(declaration.Reason);
    }

    [Fact]
    public void AuditEvent_Includes_Scope_From_Declaration()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.Scope.Should().Be(declaration.DeclaredScope);
    }

    [Fact]
    public void AuditEvent_Uses_CrossTenant_Marker_When_TargetTenantRef_Is_Null()
    {
        var declaration = CreateDeclaration(targetTenantRef: null);

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.TenantRef.Should().Be(TrustContractV1.BreakGlassMarkerCrossTenant);
    }

    [Fact]
    public void AuditEvent_Uses_TargetTenantRef_When_Provided()
    {
        var declaration = CreateDeclaration(targetTenantRef: "tenant-456");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.TenantRef.Should().Be("tenant-456");
    }

    [Fact]
    public void AuditEvent_Includes_TraceId()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.TraceId.Should().Be("trace-123");
    }

    [Fact]
    public void AuditEvent_Includes_AuditCode()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.AuditCode.Should().Be(AuditCode.BreakGlassInvoked);
    }

    [Fact]
    public void AuditEvent_Create_Accepts_Custom_AuditCode()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(
            declaration,
            traceId: "trace-123",
            auditCode: AuditCode.CrossTenantAccess);

        auditEvent.AuditCode.Should().Be(AuditCode.CrossTenantAccess);
    }

    [Fact]
    public void AuditEvent_Timestamp_Is_Utc()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.Timestamp.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void AuditEvent_Schema_Is_Stable()
    {
        string[] properties =
        [
            .. typeof(BreakGlassAuditEvent)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .OrderBy(name => name)
        ];

        properties.Should().Equal(
            "Actor",
            "AuditCode",
            "InvariantCode",
            "OperationName",
            "Reason",
            "Scope",
            "TenantRef",
            "Timestamp",
            "TraceId");
    }

    [Fact]
    public void AuditEvent_Create_FromDeclaration_Sets_All_Fields()
    {
        var declaration = CreateDeclaration("tenant-123");

        var auditEvent = BreakGlassAuditEvent.Create(declaration, traceId: "trace-123");

        auditEvent.Actor.Should().Be(declaration.ActorId);
        auditEvent.Reason.Should().Be(declaration.Reason);
        auditEvent.Scope.Should().Be(declaration.DeclaredScope);
        auditEvent.TenantRef.Should().Be(declaration.TargetTenantRef);
        auditEvent.TraceId.Should().Be("trace-123");
        auditEvent.AuditCode.Should().Be(AuditCode.BreakGlassInvoked);
    }

    private static BreakGlassDeclaration CreateDeclaration(string? targetTenantRef)
        => new(
            actorId: "admin@example.com",
            reason: "Emergency access",
            declaredScope: TrustContractV1.ScopeSharedSystem,
            targetTenantRef: targetTenantRef,
            timestamp: DateTimeOffset.UtcNow);
}
