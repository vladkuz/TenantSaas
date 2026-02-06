using FluentAssertions;
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;

namespace TenantSaas.ContractTestKit.Assertions;

/// <summary>
/// Assertions for trust contract invariant registry compliance.
/// </summary>
public static class InvariantAssertions
{
    /// <summary>
    /// Asserts that all required invariants are registered in the trust contract.
    /// </summary>
    public static void AssertAllInvariantsRegistered()
    {
        var allCodes = InvariantCode.All;

        foreach (var code in allCodes)
        {
            TrustContractV1.TryGetInvariant(code, out var definition)
                .Should().BeTrue($"Invariant '{code}' must be registered");

            definition.Should().NotBeNull();
            definition!.InvariantCode.Should().Be(code);
            definition.Name.Should().NotBeNullOrWhiteSpace($"Invariant '{code}' must have a name");
            definition.Description.Should().NotBeNullOrWhiteSpace($"Invariant '{code}' must have a description");
            definition.Category.Should().NotBeNullOrWhiteSpace($"Invariant '{code}' must have a category");
        }
    }

    /// <summary>
    /// Asserts that a specific invariant is registered with expected properties.
    /// </summary>
    /// <param name="invariantCode">The invariant code to check.</param>
    /// <param name="expectedCategory">Optional expected category.</param>
    public static void AssertInvariantRegistered(string invariantCode, string? expectedCategory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);

        TrustContractV1.TryGetInvariant(invariantCode, out var definition)
            .Should().BeTrue($"Invariant '{invariantCode}' must be registered");

        definition.Should().NotBeNull();
        definition!.InvariantCode.Should().Be(invariantCode);
        definition.Name.Should().NotBeNullOrWhiteSpace();
        definition.Description.Should().NotBeNullOrWhiteSpace();

        if (expectedCategory is not null)
        {
            definition.Category.Should().Be(expectedCategory);
        }
    }

    /// <summary>
    /// Asserts that invariant codes use stable string format (PascalCase, no special characters).
    /// </summary>
    public static void AssertInvariantCodesAreStable()
    {
        foreach (var code in InvariantCode.All)
        {
            code.Should().NotBeNullOrWhiteSpace();
            code.Should().NotContain("{", "Invariant codes must not contain template markers");
            code.Should().NotContain("}", "Invariant codes must not contain template markers");
            code.Should().MatchRegex("^[A-Z][a-zA-Z]*$", "Invariant codes must be PascalCase");
        }
    }

    /// <summary>
    /// Asserts that the invariant registry contains a specific set of required codes.
    /// </summary>
    /// <param name="requiredCodes">The required invariant codes.</param>
    public static void AssertInvariantsContain(params string[] requiredCodes)
    {
        foreach (var code in requiredCodes)
        {
            TrustContractV1.TryGetInvariant(code, out _)
                .Should().BeTrue($"Invariant '{code}' must be registered");
        }
    }
}
