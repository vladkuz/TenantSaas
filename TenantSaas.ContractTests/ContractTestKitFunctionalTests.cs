using FluentAssertions;
using TenantSaas.ContractTestKit;
using TenantSaas.ContractTestKit.Assertions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Tests that verify the ContractTestKit fixtures and assertions work correctly
/// when used against the TenantSaas trust contract implementation.
/// </summary>
public sealed class ContractTestKitFunctionalTests : IClassFixture<TrustContractFixture>
{
    private readonly TrustContractFixture fixture;

    public ContractTestKitFunctionalTests(TrustContractFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TrustContractFixture_ValidateAll_Succeeds()
    {
        // This validates the entire trust contract using the helper kit
        fixture.ValidateAll();
    }

    [Fact]
    public void TrustContractFixture_ValidateInvariants_Succeeds()
    {
        fixture.ValidateInvariants();
    }

    [Fact]
    public void TrustContractFixture_ValidateRefusalMappings_Succeeds()
    {
        fixture.ValidateRefusalMappings();
    }

    [Fact]
    public void TrustContractFixture_ValidateDisclosurePolicy_Succeeds()
    {
        fixture.ValidateDisclosurePolicy();
    }

    [Fact]
    public void TrustContractFixture_ValidateAttributionSources_Succeeds()
    {
        fixture.ValidateAttributionSources();
    }

    [Fact]
    public void InvariantAssertions_AssertAllInvariantsRegistered_Succeeds()
    {
        InvariantAssertions.AssertAllInvariantsRegistered();
    }

    [Fact]
    public void RefusalMappingAssertions_AssertAllRefusalMappingsRegistered_Succeeds()
    {
        RefusalMappingAssertions.AssertAllRefusalMappingsRegistered();
    }

    [Fact]
    public void DisclosureAssertions_AssertAllSafeStatesAreDefined_Succeeds()
    {
        DisclosureAssertions.AssertAllSafeStatesAreDefined();
    }

    [Fact]
    public void AttributionAssertions_AssertAllAttributionSourcesAreDefined_Succeeds()
    {
        AttributionAssertions.AssertAllAttributionSourcesAreDefined();
    }

    [Fact]
    public void ContractTestKitOptions_DefaultOptions_AreCorrect()
    {
        var options = ContractTestKitOptions.Default;

        options.ValidateAllInvariantsRegistered.Should().BeTrue();
        options.ValidateAllRefusalMappingsRegistered.Should().BeTrue();
        options.AdditionalInvariantCodes.Should().BeEmpty();
    }

    [Fact]
    public void TrustContractFixture_WithCustomOptions_Succeeds()
    {
        var customOptions = new ContractTestKitOptions
        {
            ValidateAllInvariantsRegistered = true,
            ValidateAllRefusalMappingsRegistered = true
        };

        var customFixture = TrustContractFixture.WithOptions(customOptions);
        customFixture.ValidateAll();
    }
}
