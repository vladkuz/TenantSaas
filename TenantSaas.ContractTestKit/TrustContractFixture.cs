using TenantSaas.ContractTestKit.Assertions;

namespace TenantSaas.ContractTestKit;

/// <summary>
/// xUnit fixture for running trust contract compliance validations.
/// Use this fixture in test classes to validate that the TenantSaas trust contract is correctly implemented.
/// </summary>
/// <example>
/// <code>
/// public class MyContractTests : IClassFixture&lt;TrustContractFixture&gt;
/// {
///     private readonly TrustContractFixture _fixture;
///
///     public MyContractTests(TrustContractFixture fixture)
///     {
///         _fixture = fixture;
///     }
///
///     [Fact]
///     public void TrustContract_IsValid() => _fixture.ValidateAll();
/// }
/// </code>
/// </example>
public sealed class TrustContractFixture
{
    private readonly ContractTestKitOptions options;

    /// <summary>
    /// Creates a new trust contract fixture with default options.
    /// </summary>
    public TrustContractFixture() : this(ContractTestKitOptions.Default)
    {
    }

    /// <summary>
    /// Creates a new trust contract fixture with custom options.
    /// This constructor is private; use <see cref="WithOptions"/> to create a configured fixture.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    private TrustContractFixture(ContractTestKitOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options;
    }

    /// <summary>
    /// Creates a new fixture with custom options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <returns>A new configured fixture.</returns>
    public static TrustContractFixture WithOptions(ContractTestKitOptions options)
        => new(options);

    /// <summary>
    /// Validates all trust contract invariants, refusal mappings, and stability requirements.
    /// </summary>
    public void ValidateAll()
    {
        ValidateInvariants();
        ValidateRefusalMappings();
        ValidateDisclosurePolicy();
        ValidateAttributionSources();
    }

    /// <summary>
    /// Validates invariant registry completeness and stability.
    /// </summary>
    public void ValidateInvariants()
    {
        if (options.ValidateAllInvariantsRegistered)
        {
            InvariantAssertions.AssertAllInvariantsRegistered();
        }

        InvariantAssertions.AssertInvariantCodesAreStable();

        foreach (var code in options.AdditionalInvariantCodes)
        {
            InvariantAssertions.AssertInvariantRegistered(code);
        }
    }

    /// <summary>
    /// Validates refusal mapping registry completeness and format.
    /// </summary>
    public void ValidateRefusalMappings()
    {
        if (options.ValidateAllRefusalMappingsRegistered)
        {
            RefusalMappingAssertions.AssertAllRefusalMappingsRegistered();
        }

        RefusalMappingAssertions.AssertProblemTypesAreStableUrns();
        RefusalMappingAssertions.AssertGuidanceUrisAreWellFormed();
        RefusalMappingAssertions.AssertHttpStatusCodesAreAppropriate();
    }

    /// <summary>
    /// Validates disclosure policy safe-state definitions.
    /// </summary>
    public void ValidateDisclosurePolicy()
    {
        DisclosureAssertions.AssertAllSafeStatesAreDefined();
        DisclosureAssertions.AssertSafeStateTokensAreStable();
        DisclosureAssertions.AssertForUnknownProducesSafeState();
    }

    /// <summary>
    /// Validates attribution source definitions and metadata.
    /// </summary>
    public void ValidateAttributionSources()
    {
        AttributionAssertions.AssertAllAttributionSourcesAreDefined();
        AttributionAssertions.AssertAttributionSourceIdentifiersAreStable();
        AttributionAssertions.AssertAttributionSourcesHaveMetadata();
        AttributionAssertions.AssertContractAttributionSourcesMatchEnum();
    }
}
