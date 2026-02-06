using FluentAssertions;
using TenantSaas.Abstractions.Disclosure;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTestKit.Assertions;

/// <summary>
/// Assertions for tenant disclosure policy compliance.
/// </summary>
public static class DisclosureAssertions
{
    /// <summary>
    /// Asserts that all required disclosure safe-state tokens are defined.
    /// </summary>
    public static void AssertAllSafeStatesAreDefined()
    {
        TrustContractV1.RequiredDisclosureSafeStates.Should().Contain(TenantRefSafeState.Unknown);
        TrustContractV1.RequiredDisclosureSafeStates.Should().Contain(TenantRefSafeState.Sensitive);
        TrustContractV1.RequiredDisclosureSafeStates.Should().Contain(TenantRefSafeState.CrossTenant);
        TrustContractV1.RequiredDisclosureSafeStates.Should().Contain(TenantRefSafeState.Opaque);
    }

    /// <summary>
    /// Asserts that TenantRefSafeState constants have stable string values.
    /// </summary>
    public static void AssertSafeStateTokensAreStable()
    {
        TenantRefSafeState.Unknown.Should().Be("unknown");
        TenantRefSafeState.Sensitive.Should().Be("sensitive");
        TenantRefSafeState.CrossTenant.Should().Be("cross_tenant");
        TenantRefSafeState.Opaque.Should().Be("opaque");
    }

    /// <summary>
    /// Asserts that a tenant reference value is a recognized safe-state token.
    /// </summary>
    /// <param name="tenantRef">The tenant reference value to check.</param>
    public static void AssertIsSafeState(string tenantRef)
    {
        TenantRefSafeState.IsSafeState(tenantRef)
            .Should().BeTrue($"'{tenantRef}' should be a recognized safe-state token");
    }

    /// <summary>
    /// Asserts that a tenant reference value is NOT a safe-state token (i.e., is an opaque ID).
    /// </summary>
    /// <param name="tenantRef">The tenant reference value to check.</param>
    public static void AssertIsOpaqueId(string tenantRef)
    {
        TenantRefSafeState.IsSafeState(tenantRef)
            .Should().BeFalse($"'{tenantRef}' should be an opaque tenant ID, not a safe-state token");
    }

    /// <summary>
    /// Asserts that a TenantRef instance correctly reports its safe-state status.
    /// </summary>
    /// <param name="tenantRef">The TenantRef to validate.</param>
    /// <param name="expectedIsSafeState">Whether it should be a safe-state.</param>
    public static void AssertTenantRefSafeStateStatus(TenantRef tenantRef, bool expectedIsSafeState)
    {
        tenantRef.IsSafeState.Should().Be(expectedIsSafeState);

        if (expectedIsSafeState)
        {
            TenantRefSafeState.IsSafeState(tenantRef.Value).Should().BeTrue();
        }
        else
        {
            TenantRefSafeState.IsSafeState(tenantRef.Value).Should().BeFalse();
        }
    }

    /// <summary>
    /// Asserts that TenantRef.ForUnknown() produces correct safe-state.
    /// </summary>
    public static void AssertForUnknownProducesSafeState()
    {
        var tenantRef = TenantRef.ForUnknown();

        tenantRef.Value.Should().Be(TenantRefSafeState.Unknown);
        tenantRef.IsSafeState.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that TenantRef.ForOpaque() produces correct opaque reference.
    /// </summary>
    /// <param name="tenantId">The tenant ID to wrap.</param>
    public static void AssertForOpaqueProducesOpaqueRef(string tenantId)
    {
        var tenantRef = TenantRef.ForOpaque(tenantId);

        tenantRef.Value.Should().Be(tenantId);
        tenantRef.IsSafeState.Should().BeFalse();
    }
}
