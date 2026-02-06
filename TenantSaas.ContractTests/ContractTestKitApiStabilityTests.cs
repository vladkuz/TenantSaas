using FluentAssertions;
using TenantSaas.ContractTestKit;
using TenantSaas.ContractTestKit.Assertions;

namespace TenantSaas.ContractTests;

/// <summary>
/// Tests that verify the ContractTestKit public API remains stable within a major version.
/// These tests fail on breaking API changes, ensuring adopters have a migration path.
/// </summary>
public sealed class ContractTestKitApiStabilityTests
{
    [Fact]
    public void InvariantAssertions_PublicMethods_AreStable()
    {
        // Document expected public methods - test fails if any are removed
        var type = typeof(InvariantAssertions);

        type.GetMethod(nameof(InvariantAssertions.AssertAllInvariantsRegistered))
            .Should().NotBeNull("AssertAllInvariantsRegistered must remain public");

        type.GetMethod(nameof(InvariantAssertions.AssertInvariantRegistered))
            .Should().NotBeNull("AssertInvariantRegistered must remain public");

        type.GetMethod(nameof(InvariantAssertions.AssertInvariantCodesAreStable))
            .Should().NotBeNull("AssertInvariantCodesAreStable must remain public");

        type.GetMethod(nameof(InvariantAssertions.AssertInvariantsContain))
            .Should().NotBeNull("AssertInvariantsContain must remain public");
    }

    [Fact]
    public void RefusalMappingAssertions_PublicMethods_AreStable()
    {
        var type = typeof(RefusalMappingAssertions);

        type.GetMethod(nameof(RefusalMappingAssertions.AssertAllRefusalMappingsRegistered))
            .Should().NotBeNull("AssertAllRefusalMappingsRegistered must remain public");

        type.GetMethod(nameof(RefusalMappingAssertions.AssertRefusalMappingRegistered))
            .Should().NotBeNull("AssertRefusalMappingRegistered must remain public");

        type.GetMethod(nameof(RefusalMappingAssertions.AssertRefusalMappingIsValid))
            .Should().NotBeNull("AssertRefusalMappingIsValid must remain public");

        type.GetMethod(nameof(RefusalMappingAssertions.AssertProblemTypesAreStableUrns))
            .Should().NotBeNull("AssertProblemTypesAreStableUrns must remain public");

        type.GetMethod(nameof(RefusalMappingAssertions.AssertGuidanceUrisAreWellFormed))
            .Should().NotBeNull("AssertGuidanceUrisAreWellFormed must remain public");

        type.GetMethod(nameof(RefusalMappingAssertions.AssertHttpStatusCodesAreAppropriate))
            .Should().NotBeNull("AssertHttpStatusCodesAreAppropriate must remain public");
    }

    [Fact]
    public void ProblemDetailsAssertions_PublicMethods_AreStable()
    {
        var type = typeof(ProblemDetailsAssertions);

        type.GetMethod(nameof(ProblemDetailsAssertions.AssertProblemDetailsAsync))
            .Should().NotBeNull("AssertProblemDetailsAsync must remain public");

        type.GetMethod(nameof(ProblemDetailsAssertions.AssertProblemDetailsStructure))
            .Should().NotBeNull("AssertProblemDetailsStructure must remain public");

        type.GetMethod(nameof(ProblemDetailsAssertions.AssertTenantSaasExtensions))
            .Should().NotBeNull("AssertTenantSaasExtensions must remain public");

        type.GetMethod(nameof(ProblemDetailsAssertions.AssertProblemTypeIsValidUrn))
            .Should().NotBeNull("AssertProblemTypeIsValidUrn must remain public");

        type.GetMethod(nameof(ProblemDetailsAssertions.AssertRefusalResponseAsync))
            .Should().NotBeNull("AssertRefusalResponseAsync must remain public");
    }

    [Fact]
    public void ProblemDetailsAssertions_ExtensionKeys_AreStable()
    {
        // These constants are part of the public API contract
        ProblemDetailsAssertions.InvariantCodeKey.Should().Be("invariantCode");
        ProblemDetailsAssertions.TraceIdKey.Should().Be("traceId");
        ProblemDetailsAssertions.RequestIdKey.Should().Be("requestId");
        ProblemDetailsAssertions.GuidanceLinkKey.Should().Be("guidanceLink");
    }

    [Fact]
    public void DisclosureAssertions_PublicMethods_AreStable()
    {
        var type = typeof(DisclosureAssertions);

        type.GetMethod(nameof(DisclosureAssertions.AssertAllSafeStatesAreDefined))
            .Should().NotBeNull("AssertAllSafeStatesAreDefined must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertSafeStateTokensAreStable))
            .Should().NotBeNull("AssertSafeStateTokensAreStable must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertIsSafeState))
            .Should().NotBeNull("AssertIsSafeState must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertIsOpaqueId))
            .Should().NotBeNull("AssertIsOpaqueId must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertTenantRefSafeStateStatus))
            .Should().NotBeNull("AssertTenantRefSafeStateStatus must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertForUnknownProducesSafeState))
            .Should().NotBeNull("AssertForUnknownProducesSafeState must remain public");

        type.GetMethod(nameof(DisclosureAssertions.AssertForOpaqueProducesOpaqueRef))
            .Should().NotBeNull("AssertForOpaqueProducesOpaqueRef must remain public");
    }

    [Fact]
    public void AttributionAssertions_PublicMethods_AreStable()
    {
        var type = typeof(AttributionAssertions);

        type.GetMethod(nameof(AttributionAssertions.AssertAllAttributionSourcesAreDefined))
            .Should().NotBeNull("AssertAllAttributionSourcesAreDefined must remain public");

        type.GetMethod(nameof(AttributionAssertions.AssertAttributionSourceIdentifiersAreStable))
            .Should().NotBeNull("AssertAttributionSourceIdentifiersAreStable must remain public");

        type.GetMethod(nameof(AttributionAssertions.AssertAttributionSourcesHaveMetadata))
            .Should().NotBeNull("AssertAttributionSourcesHaveMetadata must remain public");

        type.GetMethod(nameof(AttributionAssertions.AssertContractAttributionSourcesMatchEnum))
            .Should().NotBeNull("AssertContractAttributionSourcesMatchEnum must remain public");
    }

    [Fact]
    public void TrustContractFixture_PublicMethods_AreStable()
    {
        var type = typeof(TrustContractFixture);

        type.GetMethod(nameof(TrustContractFixture.ValidateAll))
            .Should().NotBeNull("ValidateAll must remain public");

        type.GetMethod(nameof(TrustContractFixture.ValidateInvariants))
            .Should().NotBeNull("ValidateInvariants must remain public");

        type.GetMethod(nameof(TrustContractFixture.ValidateRefusalMappings))
            .Should().NotBeNull("ValidateRefusalMappings must remain public");

        type.GetMethod(nameof(TrustContractFixture.ValidateDisclosurePolicy))
            .Should().NotBeNull("ValidateDisclosurePolicy must remain public");

        type.GetMethod(nameof(TrustContractFixture.ValidateAttributionSources))
            .Should().NotBeNull("ValidateAttributionSources must remain public");

        type.GetMethod(nameof(TrustContractFixture.WithOptions))
            .Should().NotBeNull("WithOptions must remain public");
    }

    [Fact]
    public void ContractTestKitOptions_PublicProperties_AreStable()
    {
        var type = typeof(ContractTestKitOptions);

        type.GetProperty(nameof(ContractTestKitOptions.ValidateAllInvariantsRegistered))
            .Should().NotBeNull("ValidateAllInvariantsRegistered must remain public");

        type.GetProperty(nameof(ContractTestKitOptions.ValidateAllRefusalMappingsRegistered))
            .Should().NotBeNull("ValidateAllRefusalMappingsRegistered must remain public");

        type.GetProperty(nameof(ContractTestKitOptions.AdditionalInvariantCodes))
            .Should().NotBeNull("AdditionalInvariantCodes must remain public");

        type.GetProperty(nameof(ContractTestKitOptions.Default))
            .Should().NotBeNull("Default must remain public");
    }

    [Fact]
    public void HttpResponseExtensions_PublicMethods_AreStable()
    {
        var type = typeof(TenantSaas.ContractTestKit.Extensions.HttpResponseExtensions);

        type.GetMethod("ReadProblemDetailsAsync")
            .Should().NotBeNull("ReadProblemDetailsAsync must remain public");

        type.GetMethod("IsProblemDetails")
            .Should().NotBeNull("IsProblemDetails must remain public");

        type.GetMethod("GetInvariantCodeAsync")
            .Should().NotBeNull("GetInvariantCodeAsync must remain public");
    }
}
