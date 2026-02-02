using FluentAssertions;
using TenantSaas.Abstractions.Contexts;
using TenantSaas.Abstractions.Disclosure;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTests;

public sealed class DisclosurePolicyTests
{
    [Fact]
    public void TenantRefSafeState_Constants_Are_Defined()
    {
        TenantRefSafeState.Unknown.Should().Be("unknown");
        TenantRefSafeState.Sensitive.Should().Be("sensitive");
        TenantRefSafeState.CrossTenant.Should().Be("cross_tenant");
        TenantRefSafeState.Opaque.Should().Be("opaque");
    }

    [Fact]
    public void TrustContractV1_RequiredDisclosureSafeStates_Are_Defined()
    {
        TrustContractV1.RequiredDisclosureSafeStates.Should().BeEquivalentTo(
        [
            TrustContractV1.DisclosureSafeStateUnknown,
            TrustContractV1.DisclosureSafeStateSensitive,
            TrustContractV1.DisclosureSafeStateCrossTenant,
            TrustContractV1.DisclosureSafeStateOpaque
        ]);
    }

    [Fact]
    public void TenantRefSafeState_IsSafeState_Returns_True_For_Known_States()
    {
        TenantRefSafeState.IsSafeState(TenantRefSafeState.Unknown).Should().BeTrue();
        TenantRefSafeState.IsSafeState(TenantRefSafeState.Sensitive).Should().BeTrue();
        TenantRefSafeState.IsSafeState(TenantRefSafeState.CrossTenant).Should().BeTrue();
    }

    [Fact]
    public void TenantRefSafeState_IsSafeState_Returns_False_For_Opaque_Marker()
    {
        // Opaque is a marker, not a safe-state token
        TenantRefSafeState.IsSafeState(TenantRefSafeState.Opaque).Should().BeFalse();
    }

    [Fact]
    public void TenantRefSafeState_IsSafeState_Returns_False_For_Opaque_Ids()
    {
        TenantRefSafeState.IsSafeState("tenant-123").Should().BeFalse();
    }

    [Fact]
    public void TenantRef_ForUnknown_Returns_SafeState()
    {
        var tenantRef = TenantRef.ForUnknown();

        tenantRef.Value.Should().Be(TenantRefSafeState.Unknown);
        tenantRef.IsSafeState.Should().BeTrue();
    }

    [Fact]
    public void TenantRef_ForOpaque_Returns_OpaqueId()
    {
        var tenantRef = TenantRef.ForOpaque("tenant-123");

        tenantRef.Value.Should().Be("tenant-123");
        tenantRef.IsSafeState.Should().BeFalse();
    }

    [Fact]
    public void ResolveTenantRef_NoTenantScope_Returns_Unknown()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(scope: TenantScope.ForNoTenant(NoTenantReason.HealthCheck));

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be(TenantRefSafeState.Unknown);
    }

    [Fact]
    public void ResolveTenantRef_SharedSystem_Returns_CrossTenant()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(scope: TenantScope.ForSharedSystem());

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be(TenantRefSafeState.CrossTenant);
    }

    [Fact]
    public void ResolveTenantRef_Unauthenticated_Returns_Unknown()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isAuthenticated: false);

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be(TenantRefSafeState.Unknown);
    }

    [Fact]
    public void ResolveTenantRef_Unauthorized_Returns_Sensitive()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isAuthorizedForTenant: false);

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be(TenantRefSafeState.Sensitive);
    }

    [Fact]
    public void ResolveTenantRef_EnumerationRisk_Returns_Sensitive()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isEnumerationRisk: true);

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be(TenantRefSafeState.Sensitive);
    }

    [Fact]
    public void ResolveTenantRef_AuthorizedAndSafe_Returns_OpaqueId()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(tenantId: "tenant-123");

        var resolved = policy.ResolveTenantRef(context);

        resolved.Should().Be("tenant-123");
    }

    [Fact]
    public void AllowTenantInErrors_Unauthorized_Returns_False()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isAuthorizedForTenant: false);

        policy.AllowTenantInErrors(context).Should().BeFalse();
    }

    [Fact]
    public void AllowTenantInErrors_EnumerationRisk_Returns_False()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isEnumerationRisk: true);

        policy.AllowTenantInErrors(context).Should().BeFalse();
    }

    [Fact]
    public void AllowTenantInErrors_AuthorizedAndSafe_Returns_True()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext();

        policy.AllowTenantInErrors(context).Should().BeTrue();
    }

    [Fact]
    public void AllowTenantInLogs_Always_Returns_True()
    {
        var policy = new DisclosurePolicy();
        var context = CreateContext(isAuthenticated: false, isAuthorizedForTenant: false, isEnumerationRisk: true);

        policy.AllowTenantInLogs(context).Should().BeTrue();
    }

    [Fact]
    public void Validator_RawIdWhenUnsafe_Returns_Violation()
    {
        var context = CreateContext(isAuthorizedForTenant: false);

        var result = DisclosureValidator.Validate(context, "tenant-123");

        result.IsValid.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.DisclosureSafe);
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Validator_EnumerationRisk_With_RawId_Returns_Violation()
    {
        var context = CreateContext(isEnumerationRisk: true);

        var result = DisclosureValidator.Validate(context, "tenant-123");

        result.IsValid.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.DisclosureSafe);
    }

    [Fact]
    public void Validator_EnumerationRisk_With_SafeState_Returns_Valid()
    {
        // Safe-state tokens are always valid because they don't leak tenant info
        var context = CreateContext(isEnumerationRisk: true);

        var result = DisclosureValidator.Validate(context, TenantRefSafeState.Sensitive);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_SafeState_Returns_Valid()
    {
        var context = CreateContext(isAuthorizedForTenant: false);

        var result = DisclosureValidator.Validate(context, TenantRefSafeState.Sensitive);

        result.IsValid.Should().BeTrue();
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public void Validator_OpaqueId_WhenDisclosureSafe_Returns_Valid()
    {
        var context = CreateContext();

        var result = DisclosureValidator.Validate(context, "tenant-123");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Returns_DisclosureSafe_Invariant_Code_On_Violation()
    {
        var context = CreateContext(isAuthenticated: false);

        var result = DisclosureValidator.Validate(context, "tenant-123");

        result.IsValid.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.DisclosureSafe);
    }

    [Fact]
    public void DisclosureContext_Create_ExtractsTenantId_FromTenantScope()
    {
        var tenantContext = TenantContext.ForRequest(
            TenantScope.ForTenant(new TenantId("acme-corp")),
            traceId: "trace-123",
            requestId: "req-456");

        var disclosure = DisclosureContext.Create(tenantContext, isAuthenticated: true, isAuthorizedForTenant: true);

        disclosure.TenantId.Should().Be("acme-corp");
        disclosure.IsAuthenticated.Should().BeTrue();
        disclosure.IsAuthorizedForTenant.Should().BeTrue();
        disclosure.IsEnumerationRisk.Should().BeFalse();
        disclosure.Scope.Should().BeOfType<TenantScope.Tenant>();
    }

    [Fact]
    public void DisclosureContext_Create_SetsNullTenantId_ForNoTenantScope()
    {
        var tenantContext = TenantContext.ForBackground(
            TenantScope.ForNoTenant(NoTenantReason.HealthCheck),
            traceId: "trace-123");

        var disclosure = DisclosureContext.Create(tenantContext, isAuthenticated: false, isAuthorizedForTenant: false);

        disclosure.TenantId.Should().BeNull();
        disclosure.Scope.Should().BeOfType<TenantScope.NoTenant>();
    }

    [Fact]
    public void DisclosureContext_Create_SetsEnumerationRisk_WhenSpecified()
    {
        var tenantContext = TenantContext.ForRequest(
            TenantScope.ForTenant(new TenantId("acme")),
            traceId: "trace-123",
            requestId: "req-456");

        var disclosure = DisclosureContext.Create(
            tenantContext,
            isAuthenticated: true,
            isAuthorizedForTenant: false,
            isEnumerationRisk: true);

        disclosure.IsEnumerationRisk.Should().BeTrue();
    }

    private static DisclosureContext CreateContext(
        TenantScope? scope = null,
        bool isAuthenticated = true,
        bool isAuthorizedForTenant = true,
        bool isEnumerationRisk = false,
        string? tenantId = "tenant-123")
    {
        return new DisclosureContext
        {
            TenantId = tenantId,
            IsAuthenticated = isAuthenticated,
            IsAuthorizedForTenant = isAuthorizedForTenant,
            IsEnumerationRisk = isEnumerationRisk,
            Scope = scope ?? TenantScope.ForTenant(new TenantId(tenantId ?? "tenant-123"))
        };
    }
}
