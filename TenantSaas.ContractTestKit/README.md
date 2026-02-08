# TenantSaas.ContractTestKit

Contract test helpers for verifying TenantSaas trust contract compliance in your CI pipeline.

## Installation

Add the package reference to your test project:

```xml
<PackageReference Include="TenantSaas.ContractTestKit" Version="1.0.0" />
```

Or via CLI:

```bash
dotnet add package TenantSaas.ContractTestKit
```

## Quick Start

### Using the Trust Contract Fixture

The simplest way to validate compliance is with `TrustContractFixture`:

```csharp
using TenantSaas.ContractTestKit;
using Xunit;

public class TrustContractComplianceTests : IClassFixture<TrustContractFixture>
{
    private readonly TrustContractFixture _fixture;

    public TrustContractComplianceTests(TrustContractFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TrustContract_IsFullyCompliant()
    {
        _fixture.ValidateAll();
    }
}
```

### Individual Assertions

For more granular control, use the assertion classes directly:

```csharp
using TenantSaas.ContractTestKit.Assertions;
using Xunit;

public class InvariantComplianceTests
{
    [Fact]
    public void AllInvariants_AreRegistered()
    {
        InvariantAssertions.AssertAllInvariantsRegistered();
    }

    [Fact]
    public void AllRefusalMappings_AreConfigured()
    {
        RefusalMappingAssertions.AssertAllRefusalMappingsRegistered();
    }

    [Fact]
    public void DisclosurePolicy_IsCorrect()
    {
        DisclosureAssertions.AssertAllSafeStatesAreDefined();
        DisclosureAssertions.AssertSafeStateTokensAreStable();
    }

    [Fact]
    public void AttributionSources_AreComplete()
    {
        AttributionAssertions.AssertAllAttributionSourcesAreDefined();
    }
}
```

### Validating HTTP Responses

For integration tests that exercise your API:

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using TenantSaas.ContractTestKit.Assertions;
using TenantSaas.ContractTestKit.Extensions;
using Xunit;

public class ApiComplianceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiComplianceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RefusalResponse_ContainsValidProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - make a request that triggers a refusal
        var response = await client.GetAsync("/api/protected-resource");

        // Assert - validate Problem Details compliance
        await ProblemDetailsAssertions.AssertRefusalResponseAsync(
            response,
            expectedInvariantCode: "TenantAttributionUnambiguous",
            expectedStatusCode: 422);
    }

    [Fact]
    public async Task ErrorResponse_HasRequiredExtensions()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/protected-resource");

        var problemDetails = await ProblemDetailsAssertions.AssertProblemDetailsAsync(response);
        ProblemDetailsAssertions.AssertTenantSaasExtensions(problemDetails);
    }
}
```

## Available Assertions

### InvariantAssertions

| Method | Description |
|--------|-------------|
| `AssertAllInvariantsRegistered()` | All required invariants are in the registry |
| `AssertInvariantRegistered(code, category?)` | Specific invariant exists with optional category check |
| `AssertInvariantCodesAreStable()` | Codes use stable PascalCase format |
| `AssertInvariantsContain(codes)` | Registry contains specific codes |

### RefusalMappingAssertions

| Method | Description |
|--------|-------------|
| `AssertAllRefusalMappingsRegistered()` | All invariants have refusal mappings |
| `AssertRefusalMappingRegistered(code, status?)` | Specific mapping exists with optional status check |
| `AssertRefusalMappingIsValid(mapping)` | Mapping has valid structure |
| `AssertProblemTypesAreStableUrns()` | Problem types use `urn:tenantsaas:error:` format |
| `AssertGuidanceUrisAreWellFormed()` | All guidance URIs are valid absolute URIs |
| `AssertHttpStatusCodesAreAppropriate()` | Status codes match semantic expectations |

### ProblemDetailsAssertions

| Method | Description |
|--------|-------------|
| `AssertProblemDetailsAsync(response)` | Response deserializes to valid Problem Details |
| `AssertProblemDetailsStructure(pd)` | Problem Details has required RFC 7807 fields |
| `AssertTenantSaasExtensions(pd, code?)` | Required extensions present (invariantCode, traceId, etc.) |
| `AssertProblemTypeIsValidUrn(pd)` | Type uses stable URN format |
| `AssertRefusalResponseAsync(response, code, status)` | Full refusal response validation |

### DisclosureAssertions

| Method | Description |
|--------|-------------|
| `AssertAllSafeStatesAreDefined()` | Required safe-state tokens exist |
| `AssertSafeStateTokensAreStable()` | Tokens have stable string values |
| `AssertIsSafeState(ref)` | Value is a recognized safe-state |
| `AssertIsOpaqueId(ref)` | Value is an opaque tenant ID |
| `AssertTenantRefSafeStateStatus(ref, expected)` | TenantRef reports correct status |

### AttributionAssertions

| Method | Description |
|--------|-------------|
| `AssertAllAttributionSourcesAreDefined()` | All required sources in enum |
| `AssertAttributionSourceIdentifiersAreStable()` | Identifiers use kebab-case |
| `AssertAttributionSourcesHaveMetadata()` | Sources have names and descriptions |
| `AssertContractAttributionSourcesMatchEnum()` | Contract constants match enum |

## Configuration

Customize validation with `ContractTestKitOptions`:

```csharp
var options = new ContractTestKitOptions
{
    ValidateAllInvariantsRegistered = true,
    ValidateAllRefusalMappingsRegistered = true,
    AdditionalInvariantCodes = ["CustomInvariant"]
};

var fixture = TrustContractFixture.WithOptions(options);
fixture.ValidateAll();
```

## CI Integration

Add to your test project and run in CI:

```yaml
# GitHub Actions example
- name: Run Contract Tests
  run: dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal
```

If this step fails in the reference workflow, CI surfaces:
`Contract test failure detected; enforcement boundary may be bypassed`.

## Versioning

This package follows semantic versioning. Breaking API changes require a major version bump with migration notes. The stability tests in `TenantSaas.ContractTests` verify API compatibility.

## License

MIT
