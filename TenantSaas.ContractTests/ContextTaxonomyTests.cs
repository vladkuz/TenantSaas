using FluentAssertions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTests;

public sealed class ContextTaxonomyTests
{
    [Fact]
    public void TenantScope_Should_Have_Three_Variants()
    {
        var tenant = TenantScope.ForTenant(new TenantId("tenant-1"));
        var shared = TenantScope.ForSharedSystem();
        var noTenant = TenantScope.ForNoTenant(NoTenantReason.Public);

        tenant.Should().BeOfType<TenantScope.Tenant>();
        shared.Should().BeOfType<TenantScope.SharedSystem>();
        noTenant.Should().BeOfType<TenantScope.NoTenant>();
    }

    [Fact]
    public void NoTenantReason_Should_Include_Required_Categories()
    {
        var values = NoTenantReason.All.Select(reason => reason.Value).ToArray();

        values.Should().BeEquivalentTo(
            ["public", "bootstrap", "health-check", "system-maintenance"],
            options => options.WithStrictOrdering());

        NoTenantReason.All.Should().OnlyContain(reason => !string.IsNullOrWhiteSpace(reason.Description));
    }

    [Fact]
    public void ExecutionKind_Should_Include_All_Flow_Types()
    {
        var values = ExecutionKind.All.Select(kind => kind.Value).ToArray();

        values.Should().BeEquivalentTo(
            ["request", "background", "admin", "scripted"],
            options => options.WithStrictOrdering());

        ExecutionKind.All.Should().OnlyContain(kind => !string.IsNullOrWhiteSpace(kind.Description));
    }

    [Fact]
    public void TenantContext_Should_Require_Scope_And_ExecutionKind()
    {
        var act = () => TenantContext.ForBackground(null!, "trace-1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantContext_Factories_Should_Produce_Valid_Contexts()
    {
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));

        var request = TenantContext.ForRequest(scope, "trace-1", "request-1");
        request.ExecutionKind.Should().Be(ExecutionKind.Request);
        request.RequestId.Should().Be("request-1");

        var background = TenantContext.ForBackground(scope, "trace-2");
        background.ExecutionKind.Should().Be(ExecutionKind.Background);
        background.RequestId.Should().BeNull();
    }

    [Fact]
    public void TrustContractV1_Validation_Should_Detect_Missing_Definitions()
    {
        var result = TrustContractV1.Validate(
            scopes: [TrustContractV1.ScopeTenant],
            executionKinds: [TrustContractV1.ExecutionRequest]);

        result.IsValid.Should().BeFalse();
        result.MissingScopes.Should().Contain(TrustContractV1.ScopeSharedSystem);
        result.MissingExecutionKinds.Should().Contain(TrustContractV1.ExecutionBackground);
    }
    [Fact]
    public void TenantContext_ForRequest_Should_Require_RequestId()
    {
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));

        var actNull = () => TenantContext.ForRequest(scope, "trace-1", null!);
        var actEmpty = () => TenantContext.ForRequest(scope, "trace-1", "");
        var actWhitespace = () => TenantContext.ForRequest(scope, "trace-1", "   ");

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
        actWhitespace.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TenantId_Should_Have_Value_Equality()
    {
        var id1 = new TenantId("tenant-1");
        var id2 = new TenantId("tenant-1");
        var id3 = new TenantId("tenant-2");

        id1.Should().Be(id2);
        id1.GetHashCode().Should().Be(id2.GetHashCode());
        id1.Should().NotBe(id3);
    }}
