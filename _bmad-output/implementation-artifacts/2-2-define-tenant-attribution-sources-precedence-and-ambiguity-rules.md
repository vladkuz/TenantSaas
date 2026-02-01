# Story 2.2: Define Tenant Attribution Sources, Precedence, and Ambiguity Rules

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a declared tenant attribution contract,
so that tenant resolution is deterministic and disagreements are refused.

## Acceptance Criteria

1. **Given** tenant attribution is required
   **When** I inspect the tenant resolution contract
   **Then** I see an allowed set of attribution sources
   **And** I see a clear precedence order across those sources
   **And** this is verified by a test

2. **Given** two allowed sources disagree on tenant identity
   **When** tenant attribution is evaluated
   **Then** the attribution is classified as ambiguous
   **And** enforcement can refuse with a stable invariant_code
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Task 1: Define TenantAttributionSource type (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Tenancy/TenantAttributionSource.cs`
  - [x] Define allowed attribution sources as enum or sealed type:
    - `RouteParameter` - tenant from URL route parameter
    - `HeaderValue` - tenant from HTTP header
    - `HostHeader` - tenant from host/domain
    - `TokenClaim` - tenant from auth token claims
    - `ExplicitContext` - explicitly set by initialization
  - [x] Make each source have a stable identifier and display name
  - [x] Document semantic meaning of each source

- [x] Task 2: Define TenantAttributionRules and precedence (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Tenancy/TenantAttributionRules.cs`
  - [x] Define precedence configuration:
    - Allow specification of which sources are enabled
    - Define order of precedence (first match wins OR all must agree)
    - Support per-execution-kind or per-endpoint customization
  - [x] Factory methods for common configurations
  - [x] Validation on construction (no duplicate precedence)

- [x] Task 3: Create TenantAttributionResult type (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Tenancy/TenantAttributionResult.cs`
  - [x] Discriminated union for result states:
    - `Success(TenantId, AttributionSource)` - resolved from single source
    - `Ambiguous(conflicts)` - multiple sources disagree
    - `NotFound` - no sources provided tenant ID
    - `NotAllowed(source)` - source not allowed for this operation
  - [x] Include detailed conflict information for ambiguous cases
  - [x] Make immutable with proper equality

- [x] Task 4: Define ITenantAttributionResolver interface (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Tenancy/ITenantAttributionResolver.cs`
  - [x] Define resolution contract:
    - `TenantAttributionResult Resolve(TenantContext, AttributionRules)`
    - Accept context with available attribution sources
    - Apply rules to determine single tenant or refusal
  - [x] Document determinism requirements
  - [x] Document thread-safety expectations

- [x] Task 5: Implement reference resolver in Core (AC: #1, #2)
  - [x] Create `TenantSaas.Core/Tenancy/TenantAttributionResolver.cs`
  - [x] Implement ITenantAttributionResolver
  - [x] Apply precedence rules:
    - If single source: return success
    - If multiple enabled sources: check all, refuse if disagree
    - If no sources: return NotFound
    - If source not allowed: return NotAllowed
  - [x] Include detailed conflict tracking for debugging
  - [x] Make side-effect-free and deterministic

- [x] Task 6: Add TenantAttributionUnambiguous invariant (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Invariants/InvariantCode.cs` if not exists
  - [x] Add `TenantAttributionUnambiguous` constant
  - [x] Document invariant semantics in TrustContractV1
  - [x] Map to stable Problem Details type identifier

- [x] Task 7: Write contract tests for attribution rules (AC: #1, #2)
  - [x] Create `TenantSaas.ContractTests/AttributionRulesTests.cs`
  - [x] Test single source resolution succeeds
  - [x] Test conflicting sources produce Ambiguous result
  - [x] Test disallowed source produces NotAllowed result
  - [x] Test precedence order is respected
  - [x] Test per-execution-kind customization
  - [x] Test ambiguous attribution includes conflict details

- [x] Task 8: Update docs/trust-contract.md (AC: #1, #2)
  - [x] Document attribution source taxonomy
  - [x] Document precedence semantics (first-match vs all-agree)
  - [x] Document ambiguity detection and refusal behavior
  - [x] Include examples for common configurations
  - [x] Reference InvariantCode.TenantAttributionUnambiguous

## Dev Notes

### Story Context

This is Story 2.2 of Epic 2 (Trust Contract v1 Foundations), building on Story 2.1's context taxonomy. It defines the **attribution contract** - how tenant identity is resolved from available sources, what happens when sources conflict, and when to refuse.

**Why This Matters:**
- Prevents cross-tenant data leaks from ambiguous tenant resolution
- Makes tenant attribution failures explicit and debuggable
- Enables enforcement (Epic 3) to refuse unsafe operations deterministically
- Provides verification (Epic 5) with testable attribution behaviors

**Dependency Chain:**
- **Depends on Story 2.1**: Uses TenantId, TenantContext, ExecutionKind from 2.1
- **Blocks Story 2.3**: Invariant registry needs TenantAttributionUnambiguous
- **Blocks Epic 3**: Enforcement needs attribution rules to evaluate safety

### Key Requirements from Epics

**From Epic 2 Introduction:**
> Deliver a complete, readable trust contract that defines scopes, invariants, attribution rules, disclosure policy, and refusal mappings that adopters can rely on immediately.

**From Story 2.2 Acceptance Criteria:**
- Declared set of allowed attribution sources
- Clear precedence order across sources
- Ambiguous attribution when sources disagree → classified and refused
- Stable invariant_code for refusals

**FR Coverage:**
- **FR6**: Operations with missing or ambiguous tenant attribution are refused by default
- **FR7a**: Tenant attribution uses declared set of allowed sources with explicit precedence
- **FR7b**: If multiple sources disagree, attribution is ambiguous and must refuse
- **FR7c**: If source not allowed for endpoint/execution kind, must refuse
- **FR20**: Trust contract defines attribution sources, precedence, and ambiguity semantics

**NFR Coverage:**
- **NFR1**: Reject 100% of operations with missing or ambiguous tenant attribution
- **NFR3**: Zero silent fallbacks when required context is missing

### Learnings from Story 2.1

**Established Patterns from 2.1:**
1. **Discriminated union pattern**: TenantScope as sealed class hierarchy
   - Use same pattern for TenantAttributionResult states
2. **Strong typing**: TenantId as readonly record struct
   - Create TenantAttributionSource as enum or sealed type
3. **Immutability**: All context types immutable
   - Attribution results should be immutable
4. **Validation on construction**: TenantContext validates required fields
   - Attribution rules should validate precedence config
5. **Trust contract constants**: TrustContractV1 holds stable identifiers
   - Add attribution source identifiers there

**File Organization from 2.1:**
```
TenantSaas.Abstractions/
├── Tenancy/
│   ├── TenantId.cs (exists)
│   ├── TenantScope.cs (exists)
│   ├── TenantContext.cs (exists)
│   ├── ITenantContextAccessor.cs (exists)
│   ├── TenantAttributionSource.cs (new)
│   ├── TenantAttributionRules.cs (new)
│   ├── TenantAttributionResult.cs (new)
│   └── ITenantAttributionResolver.cs (new)
├── Invariants/
│   └── InvariantCode.cs (new)
└── TrustContract/
    └── TrustContractV1.cs (update)
```

**Testing Patterns from 2.1:**
- Tests in `TenantSaas.ContractTests` root (no subfolders)
- Use FluentAssertions for readable assertions
- Test both valid and invalid scenarios
- Document test intent clearly

### Technical Requirements

**Attribution Source Design:**

Use an enum for stable, well-known sources:

```csharp
/// <summary>
/// Defines the allowed sources for tenant attribution.
/// </summary>
public enum TenantAttributionSource
{
    /// <summary>Tenant from URL route parameter (e.g., /tenants/{tenantId}).</summary>
    RouteParameter = 1,
    
    /// <summary>Tenant from HTTP header (e.g., X-Tenant-Id).</summary>
    HeaderValue = 2,
    
    /// <summary>Tenant from host/domain (e.g., tenant1.api.example.com).</summary>
    HostHeader = 3,
    
    /// <summary>Tenant from authentication token claims.</summary>
    TokenClaim = 4,
    
    /// <summary>Explicitly set during context initialization.</summary>
    ExplicitContext = 5
}
```

**Attribution Rules Design:**

```csharp
public sealed class TenantAttributionRules
{
    public IReadOnlyList<TenantAttributionSource> AllowedSources { get; }
    public AttributionStrategy Strategy { get; }
    
    // Strategy: FirstMatch (first enabled source wins) 
    //        or AllMustAgree (all enabled sources must provide same tenant)
    
    public static TenantAttributionRules Default()
    {
        // Safe default: ExplicitContext only
        return new TenantAttributionRules(
            allowedSources: [TenantAttributionSource.ExplicitContext],
            strategy: AttributionStrategy.FirstMatch
        );
    }
    
    public static TenantAttributionRules ForWebApi()
    {
        // Web API: route parameter preferred, token claim as fallback
        return new TenantAttributionRules(
            allowedSources: [
                TenantAttributionSource.RouteParameter,
                TenantAttributionSource.TokenClaim
            ],
            strategy: AttributionStrategy.AllMustAgree
        );
    }
}

public enum AttributionStrategy
{
    /// <summary>First enabled source that provides a tenant wins.</summary>
    FirstMatch = 1,
    
    /// <summary>All enabled sources must agree on the same tenant.</summary>
    AllMustAgree = 2
}
```

**Attribution Result Design:**

Use sealed class hierarchy (discriminated union pattern from 2.1):

```csharp
public abstract class TenantAttributionResult : IEquatable<TenantAttributionResult>
{
    private TenantAttributionResult() { }
    
    public sealed class Success : TenantAttributionResult
    {
        public TenantId TenantId { get; }
        public TenantAttributionSource Source { get; }
        internal Success(TenantId tenantId, TenantAttributionSource source)
        {
            TenantId = tenantId;
            Source = source;
        }
    }
    
    public sealed class Ambiguous : TenantAttributionResult
    {
        public IReadOnlyList<AttributionConflict> Conflicts { get; }
        internal Ambiguous(IReadOnlyList<AttributionConflict> conflicts)
        {
            Conflicts = conflicts;
        }
    }
    
    public sealed class NotFound : TenantAttributionResult
    {
        internal NotFound() { }
    }
    
    public sealed class NotAllowed : TenantAttributionResult
    {
        public TenantAttributionSource Source { get; }
        internal NotAllowed(TenantAttributionSource source)
        {
            Source = source;
        }
    }
    
    // Factory methods
    public static TenantAttributionResult Succeeded(TenantId id, TenantAttributionSource source)
        => new Success(id, source);
    
    public static TenantAttributionResult IsAmbiguous(IReadOnlyList<AttributionConflict> conflicts)
        => new Ambiguous(conflicts);
    
    public static TenantAttributionResult WasNotFound()
        => new NotFound();
    
    public static TenantAttributionResult IsNotAllowed(TenantAttributionSource source)
        => new NotAllowed(source);
}

public readonly record struct AttributionConflict(
    TenantAttributionSource Source,
    TenantId ProvidedTenantId
);
```

**Resolver Interface:**

```csharp
/// <summary>
/// Resolves tenant identity from available attribution sources.
/// </summary>
public interface ITenantAttributionResolver
{
    /// <summary>
    /// Resolves tenant attribution given available sources and rules.
    /// Must be deterministic and side-effect-free.
    /// </summary>
    TenantAttributionResult Resolve(
        IReadOnlyDictionary<TenantAttributionSource, TenantId> availableSources,
        TenantAttributionRules rules
    );
}
```

### Architecture Compliance

**From Architecture § Component Boundaries:**
> `TenantSaas.Abstractions` defines contracts and shared types only.  
> `TenantSaas.Core` implements invariant enforcement and logging helpers.

**Alignment:**
- Attribution source types, rules, result types → Abstractions
- Resolution implementation → Core
- Invariant code constants → Abstractions
- Contract tests → ContractTests

**From Architecture § Error Handling Patterns:**
> Use RFC 7807 Problem Details with fixed fields: `type`, `title`, `status`, `detail`, `instance`.  
> Extensions: `invariant_code`, `trace_id`, `tenant_ref` (only when safe).

**This Story:**
- Define `InvariantCode.TenantAttributionUnambiguous` constant
- Map to stable Problem Details type identifier in TrustContractV1
- Refusal implementation happens in Epic 3 (Story 3.2)

**From Architecture § Naming Patterns:**
- PascalCase types/methods, camelCase locals/fields
- No underscore prefixes anywhere
- Explicit naming (TenantAttributionSource, not just Source)

### Previous Story Intelligence (Story 2.1)

**Implementation Patterns Established:**
1. **Project structure**: TenantSaas.Abstractions is contract-only, zero dependencies
2. **Type design**: Sealed class hierarchies for discriminated unions
3. **Validation**: Validate on construction, fail fast with ArgumentException
4. **Testing**: Completeness tests + validation tests + equality tests
5. **Documentation**: XML comments on all public APIs

**Files Created in 2.1:**
- TenantSaas.Abstractions project with XML documentation enabled
- Tenancy/ folder structure established
- TrustContractV1.cs with version and validation infrastructure
- ContextTaxonomyTests.cs pattern for contract tests

**Git Context from 2.1:**
```
Commit 6fdde76: feat: Add TenantSaas.Abstractions project with context taxonomy and execution kinds
- Added TenantId, TenantScope, NoTenantReason, ExecutionKind, TenantContext
- Created ITenantContextAccessor, TrustContractV1
- Added ContextTaxonomyTests
- Updated docs/trust-contract.md
```

**Dev Notes from 2.1:**
> - Use primary constructors where possible
> - Use nullable reference types and explicit nullability
> - Tests use xUnit + FluentAssertions
> - Documentation tests verify file content structure

### Project Structure Notes

**New Files This Story:**
```
TenantSaas.Abstractions/
├── Tenancy/
│   ├── TenantAttributionSource.cs (new)
│   ├── TenantAttributionRules.cs (new)
│   ├── TenantAttributionResult.cs (new)
│   └── ITenantAttributionResolver.cs (new)
├── Invariants/
│   └── InvariantCode.cs (new - holds all invariant constants)
└── TrustContract/
    └── TrustContractV1.cs (update - add attribution constants)

TenantSaas.Core/
└── Tenancy/
    └── TenantAttributionResolver.cs (new - reference implementation)

TenantSaas.ContractTests/
└── AttributionRulesTests.cs (new)

docs/
└── trust-contract.md (update - add attribution section)
```

**No Modified Files** (except updates noted above)

### Testing Requirements

**Test Categories:**
1. **Attribution source completeness** (all enum values documented)
2. **Rules validation** (no duplicates, valid precedence)
3. **Single source resolution** (success cases)
4. **Ambiguity detection** (conflicting sources)
5. **Not allowed detection** (disabled sources)
6. **Precedence order** (first match vs all agree)
7. **Execution kind customization** (if implemented)

**Contract Tests (AttributionRulesTests.cs):**

```csharp
public class AttributionRulesTests
{
    [Fact]
    public void Attribution_Sources_Should_Be_Complete_And_Documented()
    {
        // Verify RouteParameter, HeaderValue, HostHeader, TokenClaim, ExplicitContext exist
        // Verify all have XML documentation
    }
    
    [Fact]
    public void Single_Source_Resolution_Should_Succeed()
    {
        // Given: one source provides tenant ID, rules allow that source
        // When: resolver evaluates
        // Then: Success result with tenant ID and source
    }
    
    [Fact]
    public void Conflicting_Sources_Should_Produce_Ambiguous_Result()
    {
        // Given: two enabled sources provide different tenant IDs
        // When: resolver evaluates with AllMustAgree strategy
        // Then: Ambiguous result with conflict details
    }
    
    [Fact]
    public void Disallowed_Source_Should_Produce_NotAllowed_Result()
    {
        // Given: source provides tenant ID but is not in allowed sources
        // When: resolver evaluates
        // Then: NotAllowed result with source identifier
    }
    
    [Fact]
    public void FirstMatch_Strategy_Should_Use_Precedence_Order()
    {
        // Given: multiple sources enabled with FirstMatch strategy
        // When: first source in precedence provides ID
        // Then: Success with that source (later sources ignored)
    }
    
    [Fact]
    public void AllMustAgree_Strategy_Should_Require_Agreement()
    {
        // Given: multiple sources provide same tenant ID with AllMustAgree
        // When: resolver evaluates
        // Then: Success result
        
        // Given: multiple sources provide different IDs with AllMustAgree
        // When: resolver evaluates
        // Then: Ambiguous result
    }
    
    [Fact]
    public void Ambiguous_Result_Should_Include_Conflict_Details()
    {
        // Given: conflicting sources
        // When: Ambiguous result created
        // Then: result includes all conflicting sources and their tenant IDs
    }
    
    [Fact]
    public void No_Sources_Should_Produce_NotFound_Result()
    {
        // Given: no sources provide tenant ID
        // When: resolver evaluates
        // Then: NotFound result
    }
}
```

### Critical Don't-Miss Rules

From [project-context.md](../_bmad-output/project-context.md):

**Must Follow:**
- Use standard .NET naming (PascalCase types/methods, camelCase locals/fields)
- No underscore prefixes anywhere
- Use nullable reference types and explicit nullability
- Use primary constructors where possible
- Business logic lives in services; none in controllers
- Use exceptions for truly exceptional control flow; prefer explicit result types for invariant violations

**Must NOT:**
- Never introduce new dependencies without explicit justification
- Never use implicit or ambiguous route parameters
- Never perform side effects outside designated adapters
- Never introduce non-deterministic behavior without explicit justification

**Attribution-Specific Rules:**
- **Never** silently default to a tenant when attribution is ambiguous
- **Never** allow bypass of attribution rules
- **Always** provide detailed conflict information for debugging
- **Always** make attribution deterministic for same inputs

### Latest Technical Information

**C# 13 / .NET 10 Features to Use:**
- **Primary constructors** for simple types
- **Collection expressions** `[item1, item2]` for arrays/lists
- **Required members** for mandatory properties
- **File-scoped types** if needed for internal helpers

**Pattern Matching Best Practices:**
```csharp
// Use pattern matching for result handling
var result = resolver.Resolve(sources, rules);
return result switch
{
    TenantAttributionResult.Success success => HandleSuccess(success),
    TenantAttributionResult.Ambiguous ambiguous => RefuseAmbiguous(ambiguous),
    TenantAttributionResult.NotFound => RefuseNotFound(),
    TenantAttributionResult.NotAllowed notAllowed => RefuseNotAllowed(notAllowed),
    _ => throw new InvalidOperationException("Unknown result type")
};
```

**Determinism Requirements:**
- No DateTime.Now (use injected IClock if needed)
- No Random or Guid.NewGuid() in resolution logic
- Same inputs MUST produce same outputs
- No network calls, no database reads, no async

### File Structure Requirements

**Expected Structure After This Story:**
```
TenantSaas/
├── TenantSaas.Abstractions/
│   ├── Tenancy/
│   │   ├── TenantId.cs (existing from 2.1)
│   │   ├── TenantScope.cs (existing from 2.1)
│   │   ├── NoTenantReason.cs (existing from 2.1)
│   │   ├── TenantContext.cs (existing from 2.1)
│   │   ├── ITenantContextAccessor.cs (existing from 2.1)
│   │   ├── TenantAttributionSource.cs (NEW)
│   │   ├── TenantAttributionRules.cs (NEW)
│   │   ├── TenantAttributionResult.cs (NEW)
│   │   └── ITenantAttributionResolver.cs (NEW)
│   ├── Contexts/
│   │   └── ExecutionKind.cs (existing from 2.1)
│   ├── Invariants/
│   │   └── InvariantCode.cs (NEW - all invariant constants)
│   └── TrustContract/
│       └── TrustContractV1.cs (UPDATE - add attribution constants)
├── TenantSaas.Core/
│   └── Tenancy/
│       └── TenantAttributionResolver.cs (NEW - implementation)
├── TenantSaas.ContractTests/
│   ├── ContextTaxonomyTests.cs (existing from 2.1)
│   └── AttributionRulesTests.cs (NEW)
└── docs/
    └── trust-contract.md (UPDATE - add attribution rules section)
```

### References

- [Architecture](../planning-artifacts/architecture.md#data-architecture) - § Tenant Attribution
- [Architecture](../planning-artifacts/architecture.md#error-handling-patterns) - § Problem Details
- [PRD](../planning-artifacts/prd.md) - FR6, FR7a, FR7b, FR7c, FR20
- [Epics](../planning-artifacts/epics.md#story-22-define-tenant-attribution-sources-precedence-and-ambiguity-rules) - Epic 2 Story 2.2
- [Project Context](../project-context.md) - Implementation rules
- [Story 2.1](./2-1-define-context-taxonomy-and-execution-kinds.md) - Context taxonomy foundation

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- `dotnet test TenantSaas.sln` (2026-02-01)

### Completion Notes List

- Added tenant attribution contracts (sources, rules, results, resolver interface) and invariant codes.
- Implemented deterministic attribution resolver with precedence and ambiguity handling.
- Added contract tests and updated trust contract documentation and constants.
- **Code Review 2026-02-01**: Fixed execution kind override resolution, enhanced interface with execution context parameters, added invariant/Problem Details validation tests, improved documentation.

### File List

- TenantSaas.Abstractions/Invariants/InvariantCode.cs
- TenantSaas.Abstractions/Tenancy/TenantAttributionSource.cs
- TenantSaas.Abstractions/Tenancy/TenantAttributionRules.cs
- TenantSaas.Abstractions/Tenancy/TenantAttributionResult.cs
- TenantSaas.Abstractions/Tenancy/ITenantAttributionResolver.cs
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs
- TenantSaas.Core/Tenancy/TenantAttributionResolver.cs
- TenantSaas.ContractTests/AttributionRulesTests.cs
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj
- docs/trust-contract.md
- _bmad-output/implementation-artifacts/2-2-define-tenant-attribution-sources-precedence-and-ambiguity-rules.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-02-01: Defined tenant attribution sources, rules, results, resolver, invariant code, tests, and trust contract updates.
- 2026-02-01 (Code Review): Fixed ITenantAttributionResolver to accept ExecutionKind and endpointKey parameters, updated TenantAttributionResolver to properly resolve execution kind overrides, added InvariantCode and ProblemDetails type validation tests, enhanced factory method documentation, added conflict ordering documentation.
