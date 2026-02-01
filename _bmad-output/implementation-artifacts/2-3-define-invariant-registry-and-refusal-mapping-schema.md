# Story 2.3: Define Invariant Registry and Refusal Mapping Schema

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a product owner,
I want invariants and refusals defined as data,
So that behavior is stable, testable, and versionable.

## Acceptance Criteria

1. **Given** Trust Contract v1 invariants
   **When** I review the invariant registry shape
   **Then** each invariant has a stable identifier and name
   **And** invariants can be referenced by enforcement, tests, and documentation
   **And** this is verified by a test

2. **Given** an invariant violation
   **When** refusal mapping is applied
   **Then** there is a defined mapping to status, invariant_code, and guidance link
   **And** the schema supports stable Problem Details type identifiers
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Task 1: Define InvariantCode constants (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Invariants/InvariantCode.cs`
  - [x] Define stable string constants for each invariant:
    - `ContextInitialized` - tenant context must be initialized
    - `TenantAttributionUnambiguous` - attribution from sources must be unambiguous
    - `TenantScopeRequired` - operation requires explicit tenant scope
    - `BreakGlassExplicitAndAudited` - break-glass must be explicit with actor/reason
    - `DisclosureSafe` - tenant information disclosure must be safe
  - [x] Use string literals (not GUIDs) for stable, human-readable codes
  - [x] Include XML documentation for each invariant

- [x] Task 2: Define InvariantDefinition type (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Invariants/InvariantDefinition.cs`
  - [x] Define immutable record with properties:
    - `string InvariantCode` - stable identifier (e.g., "ContextInitialized")
    - `string Name` - display name (e.g., "Context Initialized")
    - `string Description` - semantic meaning
    - `string Category` - grouping (e.g., "Initialization", "Attribution", "Disclosure")
  - [x] Add validation on construction
  - [x] Implement equality and GetHashCode

- [x] Task 3: Define RefusalMapping type (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Invariants/RefusalMapping.cs`
  - [x] Define immutable record with properties:
    - `string InvariantCode` - links to invariant
    - `int HttpStatusCode` - HTTP status for refusal (e.g., 400, 403, 422)
    - `string ProblemType` - RFC 7807 type identifier (URN or URL)
    - `string Title` - Problem Details title
    - `string GuidanceUri` - link to documentation/remediation
  - [x] Add factory methods for common mappings
  - [x] Validate status code is in valid range (400-599)

- [x] Task 4: Create InvariantRegistry in TrustContractV1 (AC: #1)
  - [x] Update `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs`
  - [x] Add static readonly registry of InvariantDefinition instances
  - [x] Define all baseline invariants (5 from Task 1)
  - [x] Provide lookup methods:
    - `GetInvariant(string code)` - returns InvariantDefinition or throws
    - `TryGetInvariant(string code, out InvariantDefinition)` - safe lookup
  - [x] Make registry immutable and frozen

- [x] Task 5: Create RefusalMappingRegistry in TrustContractV1 (AC: #2)
  - [x] Update `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs`
  - [x] Add static readonly registry of RefusalMapping instances
  - [x] Define mappings for all invariants:
    - ContextInitialized → 400 Bad Request
    - TenantAttributionUnambiguous → 400 Bad Request (or 422 Unprocessable Entity)
    - TenantScopeRequired → 403 Forbidden
    - BreakGlassExplicitAndAudited → 403 Forbidden
    - DisclosureSafe → 500 Internal Server Error
  - [x] Provide lookup methods similar to invariant registry
  - [x] Define stable Problem Details type URNs (e.g., `urn:tenantsaas:error:context-not-initialized`)

- [x] Task 6: Write contract tests for invariant registry (AC: #1)
  - [x] Create `TenantSaas.ContractTests/InvariantRegistryTests.cs`
  - [x] Test all invariants are registered
  - [x] Test invariant codes are stable strings (not GUIDs)
  - [x] Test invariant lookup succeeds for valid codes
  - [x] Test invariant lookup fails for invalid codes
  - [x] Test invariant definitions are immutable
  - [x] Test invariant categories are consistent

- [x] Task 7: Write contract tests for refusal mapping (AC: #2)
  - [x] Update `TenantSaas.ContractTests/InvariantRegistryTests.cs` or create separate file
  - [x] Test refusal mappings exist for all invariants
  - [x] Test Problem Details type identifiers are stable
  - [x] Test HTTP status codes are in valid range
  - [x] Test guidance URIs are well-formed
  - [x] Test refusal mapping lookup succeeds for all invariants
  - [x] Test refusal mapping includes required fields

- [x] Task 8: Update docs/trust-contract.md (AC: #1, #2)
  - [x] Document invariant registry structure
  - [x] List all defined invariants with codes, names, and descriptions
  - [x] Document refusal mapping schema
  - [x] Provide HTTP status code → invariant mapping table
  - [x] Document Problem Details type identifier format
  - [x] Include examples of refusal responses
  - [x] Reference TrustContractV1 version and stability guarantees

## Dev Notes

### Story Context

This is **Story 2.3** of Epic 2 (Trust Contract v1 Foundations). It defines the invariant registry and refusal mapping schema that enables explicit, stable, and testable enforcement behavior.

**Why This Matters:**
- Makes invariants explicit data, not magic strings
- Enables stable API contracts (invariant_code values don't change within major version)
- Provides deterministic refusal behavior (same violation → same HTTP status + Problem Details)
- Supports verification (Epic 5 tests reference these stable codes)
- Makes documentation and error messages consistent

**Dependency Chain:**
- **Depends on Story 2.1**: Uses TrustContractV1 infrastructure
- **Depends on Story 2.2**: References TenantAttributionUnambiguous invariant
- **Blocks Epic 3**: Enforcement needs invariant registry and refusal mappings
- **Blocks Epic 5**: Contract tests need stable invariant codes

### Key Requirements from Epics

**From Epic 2 Introduction:**
> Deliver a complete, readable trust contract that defines scopes, invariants, attribution rules, disclosure policy, and refusal mappings that adopters can rely on immediately.

**From Story 2.3 Acceptance Criteria:**
- Each invariant has stable identifier and name
- Invariants can be referenced by enforcement, tests, and documentation
- Refusal mapping defines status, invariant_code, and guidance link
- Schema supports stable Problem Details type identifiers

**FR Coverage:**
- **FR7**: Operations with missing/ambiguous tenant attribution are refused by default
- **FR8**: Invariants are enumerated, named, finite, and defined in trust contract
- **FR9**: Enforcement applies across execution paths; refusal reasons are explicit
- **FR10**: Refusal reasons are explicit and developer-facing
- **FR20**: Trust contract defines invariants and refusal mappings
- **FR28**: Refusal errors include actionable guidance

**NFR Coverage:**
- **NFR5**: Invariant violations return explicit error codes with stable invariant_code values
- **NFR5a**: invariant_code values and Problem Details types are stable within major version
- **NFR6**: Invariant violations return explicit error messages

### Learnings from Story 2.1 and 2.2

**Established Patterns from 2.1:**
1. **Immutable types**: All contract types are immutable (readonly, sealed, record)
2. **Validation on construction**: Fail fast with ArgumentException
3. **Static registries**: TrustContractV1 holds stable definitions
4. **XML documentation**: All public APIs documented

**Established Patterns from 2.2:**
1. **Discriminated unions**: Use sealed class hierarchies for result types
2. **Factory methods**: Provide static factory methods for common cases
3. **Stable identifiers**: Use string constants (not enums) for extensibility
4. **Test coverage**: Test both valid and invalid scenarios

**File Organization:**
```
TenantSaas.Abstractions/
├── Invariants/
│   ├── InvariantCode.cs (constants - new)
│   ├── InvariantDefinition.cs (new)
│   └── RefusalMapping.cs (new)
└── TrustContract/
    └── TrustContractV1.cs (update - add registries)
```

### Technical Requirements

**InvariantCode Design:**

Use string constants (not enums) for stable, extensible identifiers:

```csharp
namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Defines stable invariant code identifiers for Trust Contract v1.
/// </summary>
/// <remarks>
/// These codes are stable within a major version and used in:
/// - Refusal responses (invariant_code field)
/// - Structured logs
/// - Contract tests
/// - Documentation
/// </remarks>
public static class InvariantCode
{
    /// <summary>Tenant context must be initialized before operations.</summary>
    public const string ContextInitialized = "ContextInitialized";
    
    /// <summary>Tenant attribution from sources must be unambiguous.</summary>
    public const string TenantAttributionUnambiguous = "TenantAttributionUnambiguous";
    
    /// <summary>Operation requires explicit tenant scope.</summary>
    public const string TenantScopeRequired = "TenantScopeRequired";
    
    /// <summary>Break-glass must be explicit with actor identity and reason.</summary>
    public const string BreakGlassExplicitAndAudited = "BreakGlassExplicitAndAudited";
    
    /// <summary>Tenant information disclosure must be safe.</summary>
    public const string DisclosureSafe = "DisclosureSafe";
}
```

**InvariantDefinition Design:**

```csharp
namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Represents an invariant definition in the trust contract.
/// </summary>
public sealed record InvariantDefinition
{
    /// <summary>Gets the stable invariant code identifier.</summary>
    public string InvariantCode { get; }
    
    /// <summary>Gets the display name.</summary>
    public string Name { get; }
    
    /// <summary>Gets the semantic description.</summary>
    public string Description { get; }
    
    /// <summary>Gets the invariant category.</summary>
    public string Category { get; }
    
    public InvariantDefinition(
        string invariantCode,
        string name,
        string description,
        string category)
    {
        if (string.IsNullOrWhiteSpace(invariantCode))
            throw new ArgumentException("InvariantCode cannot be null or empty.", nameof(invariantCode));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));
        
        InvariantCode = invariantCode;
        Name = name;
        Description = description;
        Category = category;
    }
}
```

**RefusalMapping Design:**

```csharp
namespace TenantSaas.Abstractions.Invariants;

/// <summary>
/// Represents the refusal mapping for an invariant violation.
/// </summary>
public sealed record RefusalMapping
{
    /// <summary>Gets the invariant code.</summary>
    public string InvariantCode { get; }
    
    /// <summary>Gets the HTTP status code for refusal.</summary>
    public int HttpStatusCode { get; }
    
    /// <summary>Gets the RFC 7807 Problem Details type identifier.</summary>
    public string ProblemType { get; }
    
    /// <summary>Gets the Problem Details title.</summary>
    public string Title { get; }
    
    /// <summary>Gets the guidance URI for remediation.</summary>
    public string GuidanceUri { get; }
    
    public RefusalMapping(
        string invariantCode,
        int httpStatusCode,
        string problemType,
        string title,
        string guidanceUri)
    {
        if (string.IsNullOrWhiteSpace(invariantCode))
            throw new ArgumentException("InvariantCode cannot be null or empty.", nameof(invariantCode));
        if (httpStatusCode < 400 || httpStatusCode >= 600)
            throw new ArgumentException("HttpStatusCode must be in range 400-599.", nameof(httpStatusCode));
        if (string.IsNullOrWhiteSpace(problemType))
            throw new ArgumentException("ProblemType cannot be null or empty.", nameof(problemType));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(guidanceUri))
            throw new ArgumentException("GuidanceUri cannot be null or empty.", nameof(guidanceUri));
        
        InvariantCode = invariantCode;
        HttpStatusCode = httpStatusCode;
        ProblemType = problemType;
        Title = title;
        GuidanceUri = guidanceUri;
    }
    
    /// <summary>
    /// Creates a refusal mapping for bad request scenarios.
    /// </summary>
    public static RefusalMapping ForBadRequest(
        string invariantCode,
        string title,
        string guidanceUri)
    {
        return new RefusalMapping(
            invariantCode,
            httpStatusCode: 400,
            problemType: $"urn:tenantsaas:error:{ToKebabCase(invariantCode)}",
            title,
            guidanceUri);
    }
    
    /// <summary>
    /// Creates a refusal mapping for forbidden scenarios.
    /// </summary>
    public static RefusalMapping ForForbidden(
        string invariantCode,
        string title,
        string guidanceUri)
    {
        return new RefusalMapping(
            invariantCode,
            httpStatusCode: 403,
            problemType: $"urn:tenantsaas:error:{ToKebabCase(invariantCode)}",
            title,
            guidanceUri);
    }
    
    /// <summary>
    /// Creates a refusal mapping for unprocessable entity scenarios.
    /// </summary>
    public static RefusalMapping ForUnprocessableEntity(
        string invariantCode,
        string title,
        string guidanceUri)
    {
        return new RefusalMapping(
            invariantCode,
            httpStatusCode: 422,
            problemType: $"urn:tenantsaas:error:{ToKebabCase(invariantCode)}",
            title,
            guidanceUri);
    }
    
    private static string ToKebabCase(string value)
    {
        // Convert PascalCase to kebab-case
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLowerInvariant();
    }
}
```

**TrustContractV1 Registries:**

```csharp
namespace TenantSaas.Abstractions.TrustContract;

using TenantSaas.Abstractions.Invariants;
using System.Collections.Frozen;

/// <summary>
/// Trust Contract v1 definitions for invariants and refusal mappings.
/// </summary>
public static class TrustContractV1
{
    /// <summary>Gets the trust contract version.</summary>
    public const string Version = "1.0";
    
    // ... existing content from Story 2.1 ...
    
    /// <summary>Gets the invariant registry.</summary>
    public static readonly FrozenDictionary<string, InvariantDefinition> Invariants = 
        new Dictionary<string, InvariantDefinition>
        {
            [InvariantCode.ContextInitialized] = new InvariantDefinition(
                InvariantCode.ContextInitialized,
                name: "Context Initialized",
                description: "Tenant context must be initialized before operations can proceed.",
                category: "Initialization"),
            
            [InvariantCode.TenantAttributionUnambiguous] = new InvariantDefinition(
                InvariantCode.TenantAttributionUnambiguous,
                name: "Tenant Attribution Unambiguous",
                description: "Tenant attribution from available sources must be unambiguous.",
                category: "Attribution"),
            
            [InvariantCode.TenantScopeRequired] = new InvariantDefinition(
                InvariantCode.TenantScopeRequired,
                name: "Tenant Scope Required",
                description: "Operation requires an explicit tenant scope.",
                category: "Scope"),
            
            [InvariantCode.BreakGlassExplicitAndAudited] = new InvariantDefinition(
                InvariantCode.BreakGlassExplicitAndAudited,
                name: "Break-Glass Explicit and Audited",
                description: "Break-glass must be explicitly declared with actor identity and reason.",
                category: "Authorization"),
            
            [InvariantCode.DisclosureSafe] = new InvariantDefinition(
                InvariantCode.DisclosureSafe,
                name: "Disclosure Safe",
                description: "Tenant information disclosure must follow safe disclosure policy.",
                category: "Disclosure")
        }.ToFrozenDictionary();
    
    /// <summary>Gets the refusal mapping registry.</summary>
    public static readonly FrozenDictionary<string, RefusalMapping> RefusalMappings = 
        new Dictionary<string, RefusalMapping>
        {
            [InvariantCode.ContextInitialized] = RefusalMapping.ForBadRequest(
                InvariantCode.ContextInitialized,
                title: "Tenant context not initialized",
                guidanceUri: "https://docs.tenantsaas.dev/errors/context-not-initialized"),
            
            [InvariantCode.TenantAttributionUnambiguous] = RefusalMapping.ForUnprocessableEntity(
                InvariantCode.TenantAttributionUnambiguous,
                title: "Tenant attribution is ambiguous",
                guidanceUri: "https://docs.tenantsaas.dev/errors/attribution-ambiguous"),
            
            [InvariantCode.TenantScopeRequired] = RefusalMapping.ForForbidden(
                InvariantCode.TenantScopeRequired,
                title: "Tenant scope required",
                guidanceUri: "https://docs.tenantsaas.dev/errors/tenant-scope-required"),
            
            [InvariantCode.BreakGlassExplicitAndAudited] = RefusalMapping.ForForbidden(
                InvariantCode.BreakGlassExplicitAndAudited,
                title: "Break-glass must be explicit",
                guidanceUri: "https://docs.tenantsaas.dev/errors/break-glass-required"),
            
            [InvariantCode.DisclosureSafe] = new RefusalMapping(
                InvariantCode.DisclosureSafe,
                httpStatusCode: 500,
                problemType: "urn:tenantsaas:error:disclosure-unsafe",
                title: "Tenant disclosure policy violation",
                guidanceUri: "https://docs.tenantsaas.dev/errors/disclosure-unsafe")
        }.ToFrozenDictionary();
    
    /// <summary>
    /// Gets an invariant definition by code.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If the invariant code is not registered.</exception>
    public static InvariantDefinition GetInvariant(string code)
    {
        if (!Invariants.TryGetValue(code, out var definition))
            throw new KeyNotFoundException($"Invariant code '{code}' is not registered in Trust Contract v{Version}.");
        return definition;
    }
    
    /// <summary>
    /// Tries to get an invariant definition by code.
    /// </summary>
    public static bool TryGetInvariant(string code, out InvariantDefinition? definition)
    {
        return Invariants.TryGetValue(code, out definition);
    }
    
    /// <summary>
    /// Gets a refusal mapping by invariant code.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If the refusal mapping is not registered.</exception>
    public static RefusalMapping GetRefusalMapping(string invariantCode)
    {
        if (!RefusalMappings.TryGetValue(invariantCode, out var mapping))
            throw new KeyNotFoundException($"Refusal mapping for invariant '{invariantCode}' is not registered in Trust Contract v{Version}.");
        return mapping;
    }
    
    /// <summary>
    /// Tries to get a refusal mapping by invariant code.
    /// </summary>
    public static bool TryGetRefusalMapping(string invariantCode, out RefusalMapping? mapping)
    {
        return RefusalMappings.TryGetValue(invariantCode, out mapping);
    }
}
```

### Architecture Compliance

**From Architecture § Component Boundaries:**
> `TenantSaas.Abstractions` defines contracts and shared types only.  
> No implementation logic; zero dependencies beyond .NET BCL.

**Alignment:**
- Invariant definitions and mappings are contract data, not implementation
- Registries are immutable frozen dictionaries
- Lookup methods are simple accessors

**From Architecture § Error Handling Patterns:**
> Use RFC 7807 Problem Details with fixed fields: `type`, `title`, `status`, `detail`, `instance`.  
> Extensions: `invariant_code`, `trace_id`, `tenant_ref` (only when safe).  
> `type` is stable and machine-meaningful (URN or URL).

**Alignment:**
- RefusalMapping defines `ProblemType` as stable URN: `urn:tenantsaas:error:{kebab-case-code}`
- HTTP status codes mapped deterministically
- Title and guidance URI included
- invariant_code is the stable identifier

**From Architecture § Naming Patterns:**
- PascalCase types/methods: `InvariantCode`, `RefusalMapping`
- camelCase locals/fields: `invariantCode`, `httpStatusCode`
- No underscore prefixes anywhere
- Explicit naming: `InvariantDefinition` not just `Definition`

### Project Context Reference

**From [/home/vlad/Source/TenantSaas/_bmad-output/project-context.md](file:///home/vlad/Source/TenantSaas/_bmad-output/project-context.md):**

> - .NET 10 (all projects target net10.0)
> - Problem Details only for errors; direct resource for success.
> - Problem Details: `type`, `title`, `status`, `detail`, `instance` + `invariant_code`, `trace_id`, `tenant_ref`
> - `type` is stable and machine-meaningful (URN or URL).

**Alignment:**
- RefusalMapping schema matches Problem Details structure
- Stable URN format for type identifiers
- invariant_code is first-class field

### Trust Contract Documentation

**Update [docs/trust-contract.md](file:///home/vlad/Source/TenantSaas/docs/trust-contract.md):**

Add sections documenting:

1. **Invariant Registry**
   - List all invariants with codes, names, descriptions, categories
   - Document lookup methods and usage

2. **Refusal Mapping Schema**
   - HTTP status code → invariant mapping table
   - Problem Details type identifier format
   - Guidance URI pattern

3. **Stability Guarantees**
   - invariant_code values are stable within major version
   - Problem Details type URNs are stable within major version
   - Breaking changes require major version bump

4. **Example Refusal Responses**
   ```json
   {
     "type": "urn:tenantsaas:error:context-not-initialized",
     "title": "Tenant context not initialized",
     "status": 400,
     "detail": "Tenant context must be initialized before operations can proceed.",
     "instance": "/api/tenants/123",
     "invariant_code": "ContextInitialized",
     "trace_id": "trace-abc-123",
     "guidance_uri": "https://docs.tenantsaas.dev/errors/context-not-initialized"
   }
   ```

### Testing Strategy

**Contract Tests in [TenantSaas.ContractTests/InvariantRegistryTests.cs](file:///home/vlad/Source/TenantSaas/TenantSaas.ContractTests/InvariantRegistryTests.cs):**

```csharp
using TenantSaas.Abstractions.Invariants;
using TenantSaas.Abstractions.TrustContract;
using FluentAssertions;
using Xunit;

namespace TenantSaas.ContractTests;

public class InvariantRegistryTests
{
    [Theory]
    [InlineData(InvariantCode.ContextInitialized)]
    [InlineData(InvariantCode.TenantAttributionUnambiguous)]
    [InlineData(InvariantCode.TenantScopeRequired)]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited)]
    [InlineData(InvariantCode.DisclosureSafe)]
    public void InvariantRegistry_ContainsAllInvariants(string code)
    {
        var definition = TrustContractV1.GetInvariant(code);
        
        definition.Should().NotBeNull();
        definition.InvariantCode.Should().Be(code);
        definition.Name.Should().NotBeNullOrWhiteSpace();
        definition.Description.Should().NotBeNullOrWhiteSpace();
        definition.Category.Should().NotBeNullOrWhiteSpace();
    }
    
    [Fact]
    public void InvariantCode_UsesStableStrings()
    {
        var allCodes = new[]
        {
            InvariantCode.ContextInitialized,
            InvariantCode.TenantAttributionUnambiguous,
            InvariantCode.TenantScopeRequired,
            InvariantCode.BreakGlassExplicitAndAudited,
            InvariantCode.DisclosureSafe
        };
        
        foreach (var code in allCodes)
        {
            code.Should().NotBeNullOrWhiteSpace();
            code.Should().NotContain("{"); // Not a GUID
            code.Should().MatchRegex("^[A-Z][a-zA-Z]*$"); // PascalCase
        }
    }
    
    [Fact]
    public void InvariantRegistry_ThrowsForUnknownCode()
    {
        Action act = () => TrustContractV1.GetInvariant("UnknownCode");
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*UnknownCode*");
    }
    
    [Fact]
    public void InvariantRegistry_TryGetReturnsFalseForUnknownCode()
    {
        var found = TrustContractV1.TryGetInvariant("UnknownCode", out var definition);
        
        found.Should().BeFalse();
        definition.Should().BeNull();
    }
    
    [Theory]
    [InlineData(InvariantCode.ContextInitialized)]
    [InlineData(InvariantCode.TenantAttributionUnambiguous)]
    [InlineData(InvariantCode.TenantScopeRequired)]
    [InlineData(InvariantCode.BreakGlassExplicitAndAudited)]
    [InlineData(InvariantCode.DisclosureSafe)]
    public void RefusalMappingRegistry_ContainsAllMappings(string invariantCode)
    {
        var mapping = TrustContractV1.GetRefusalMapping(invariantCode);
        
        mapping.Should().NotBeNull();
        mapping.InvariantCode.Should().Be(invariantCode);
        mapping.HttpStatusCode.Should().BeInRange(400, 599);
        mapping.ProblemType.Should().NotBeNullOrWhiteSpace();
        mapping.Title.Should().NotBeNullOrWhiteSpace();
        mapping.GuidanceUri.Should().NotBeNullOrWhiteSpace();
    }
    
    [Fact]
    public void RefusalMapping_ProblemType_IsStableUrn()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            mapping.ProblemType.Should().StartWith("urn:tenantsaas:error:");
            mapping.ProblemType.Should().MatchRegex("^urn:tenantsaas:error:[a-z-]+$");
        }
    }
    
    [Fact]
    public void RefusalMapping_HttpStatusCodes_AreAppropriate()
    {
        TrustContractV1.GetRefusalMapping(InvariantCode.ContextInitialized).HttpStatusCode.Should().Be(400);
        TrustContractV1.GetRefusalMapping(InvariantCode.TenantAttributionUnambiguous).HttpStatusCode.Should().Be(422);
        TrustContractV1.GetRefusalMapping(InvariantCode.TenantScopeRequired).HttpStatusCode.Should().Be(403);
        TrustContractV1.GetRefusalMapping(InvariantCode.BreakGlassExplicitAndAudited).HttpStatusCode.Should().Be(403);
        TrustContractV1.GetRefusalMapping(InvariantCode.DisclosureSafe).HttpStatusCode.Should().Be(500);
    }
    
    [Fact]
    public void RefusalMapping_GuidanceUri_IsWellFormed()
    {
        foreach (var mapping in TrustContractV1.RefusalMappings.Values)
        {
            Uri.IsWellFormedUriString(mapping.GuidanceUri, UriKind.Absolute).Should().BeTrue();
        }
    }
    
    [Fact]
    public void InvariantDefinition_Immutable()
    {
        var definition = new InvariantDefinition(
            "TestCode",
            "Test Name",
            "Test description",
            "Test");
        
        definition.InvariantCode.Should().Be("TestCode");
        definition.Name.Should().Be("Test Name");
        
        // No setters should exist (record immutability)
        definition.Should().BeAssignableTo<IEquatable<InvariantDefinition>>();
    }
    
    [Fact]
    public void RefusalMapping_Immutable()
    {
        var mapping = new RefusalMapping(
            "TestCode",
            400,
            "urn:test:error",
            "Test Title",
            "https://docs.test.com/errors/test");
        
        mapping.InvariantCode.Should().Be("TestCode");
        mapping.HttpStatusCode.Should().Be(400);
        
        // No setters should exist (record immutability)
        mapping.Should().BeAssignableTo<IEquatable<RefusalMapping>>();
    }
}
```

### Verification Checklist

Before marking story complete, verify:

- [ ] InvariantCode constants defined with stable string values
- [ ] InvariantDefinition type created and immutable
- [ ] RefusalMapping type created with validation
- [ ] TrustContractV1 registries populated with all invariants
- [ ] All contract tests pass
- [ ] [docs/trust-contract.md](file:///home/vlad/Source/TenantSaas/docs/trust-contract.md) updated with invariant registry and refusal mapping
- [ ] Problem Details type URNs follow stable pattern
- [ ] HTTP status codes are appropriate for each invariant
- [ ] Guidance URIs are well-formed

### References

**Source Documents:**
- [PRD](file:///home/vlad/Source/TenantSaas/_bmad-output/planning-artifacts/prd.md) § Functional Requirements FR7, FR8, FR9, FR10, FR20, FR28
- [Architecture](file:///home/vlad/Source/TenantSaas/_bmad-output/planning-artifacts/architecture.md) § Error Handling Patterns, Component Boundaries
- [Epics](file:///home/vlad/Source/TenantSaas/_bmad-output/planning-artifacts/epics.md) § Epic 2 Story 2.3
- [Project Context](file:///home/vlad/Source/TenantSaas/_bmad-output/project-context.md) § Code Quality & Style Rules
- [Trust Contract](file:///home/vlad/Source/TenantSaas/docs/trust-contract.md) (to be updated)

**Related Stories:**
- [Story 2.1](file:///home/vlad/Source/TenantSaas/_bmad-output/implementation-artifacts/2-1-define-context-taxonomy-and-execution-kinds.md) - Established TrustContractV1 infrastructure
- [Story 2.2](file:///home/vlad/Source/TenantSaas/_bmad-output/implementation-artifacts/2-2-define-tenant-attribution-sources-precedence-and-ambiguity-rules.md) - Defined TenantAttributionUnambiguous invariant

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- `dotnet test TenantSaas.sln` (2026-02-01)

### Completion Notes List

- Added invariant codes, invariant definitions, and refusal mappings with validation.
- Added frozen registries and lookup helpers in TrustContractV1.
- Added contract tests for invariants and refusal mappings and updated attribution invariant expectations.
- Updated trust contract documentation with registry and refusal schema details.

### File List

- TenantSaas.Abstractions/Invariants/InvariantCode.cs
- TenantSaas.Abstractions/Invariants/InvariantDefinition.cs
- TenantSaas.Abstractions/Invariants/RefusalMapping.cs
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs
- TenantSaas.ContractTests/InvariantRegistryTests.cs
- TenantSaas.ContractTests/AttributionRulesTests.cs
- docs/trust-contract.md
- _bmad-output/implementation-artifacts/2-3-define-invariant-registry-and-refusal-mapping-schema.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-02-01: Added invariant registry, refusal mappings, contract tests, and documentation updates for Trust Contract v1.
