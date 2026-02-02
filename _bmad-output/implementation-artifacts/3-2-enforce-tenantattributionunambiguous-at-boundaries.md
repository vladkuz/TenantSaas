# Story 3.2: Enforce TenantAttributionUnambiguous at Boundaries

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want ambiguous tenant attribution to be refused consistently,
So that conflicting signals cannot silently choose a tenant.

## Acceptance Criteria

1. **Given** tenant attribution is present but ambiguous per the trust contract
   **When** enforcement is evaluated
   **Then** the operation is refused
   **And** the refusal references TenantAttributionUnambiguous
   **And** this is verified by a test

2. **Given** an ambiguous attribution refusal
   **When** refusal details are produced
   **Then** they do not leak tenant existence information
   **And** they include actionable guidance via the contract mapping
   **And** this is verified by a test

3. **Given** tenant attribution is ambiguous per the trust contract
   **When** enforcement runs at the boundary
   **Then** the operation must refuse with a standardized error

## Tasks / Subtasks

### Core Attribution Enforcement

- [x] Task 1: Extend BoundaryGuard with RequireUnambiguousAttribution (AC: #1, #3)
  - [x] Add method `static AttributionEnforcementResult RequireUnambiguousAttribution(TenantAttributionResult result, string traceId)` to `TenantSaas.Core/Enforcement/BoundaryGuard.cs`
  - [x] If result is `TenantAttributionResult.Success` → return `AttributionEnforcementResult.Success` with tenant ID and source
  - [x] If result is `TenantAttributionResult.Ambiguous` → return `AttributionEnforcementResult.Ambiguous` with source names
  - [x] If result is `TenantAttributionResult.NotFound` → return `AttributionEnforcementResult.NotFound`
  - [x] If result is `TenantAttributionResult.NotAllowed` → return `AttributionEnforcementResult.NotAllowed`
  - [x] Detail message MUST NOT include actual tenant IDs from conflicts (disclosure safety)
  - [x] Detail message MUST reference the number of conflicting sources without specifics

- [x] Task 2: Create AttributionEnforcementResult type (AC: #1, #2)
  - [x] Create `TenantSaas.Core/Enforcement/AttributionEnforcementResult.cs`
  - [x] Define sealed record with discriminated union pattern:
    - `bool IsSuccess`
    - `TenantId? ResolvedTenantId` (when success)
    - `TenantAttributionSource? ResolvedSource` (when success)
    - `string? InvariantCode` (when failure)
    - `string TraceId` (always present)
    - `string? Detail` (human-readable failure reason - disclosure safe)
    - `IReadOnlyList<string>? ConflictingSources` (source names only, no tenant IDs)
  - [x] Add static factory methods:
    - `Success(TenantId tenantId, TenantAttributionSource source, string traceId)`
    - `Ambiguous(IReadOnlyList<string> conflictingSources, string traceId)`
    - `NotFound(string traceId)`
    - `NotAllowed(TenantAttributionSource disallowedSource, string traceId)`

- [x] Task 3: Add disclosure-safe detail message builder (AC: #2)
  - [x] Integrated into AttributionEnforcementResult factory methods
  - [x] For Ambiguous: "Tenant attribution is ambiguous: {N} sources provided conflicting tenant identifiers."
  - [x] For NotFound: "Tenant attribution could not be resolved from any enabled source."
  - [x] For NotAllowed: "Tenant attribution source '{source.GetDisplayName()}' is not allowed for this operation."
  - [x] NEVER include actual tenant ID values in messages
  - [x] NEVER list specific tenant IDs that conflicted

### Problem Details Integration

- [x] Task 4: Extend EnforcementProblemDetails for attribution failures (AC: #2)
  - [x] Update `TenantSaas.Core/Errors/EnforcementProblemDetails.cs`
  - [x] Added `FromAttributionEnforcementResult` method for attribution-specific conversion
  - [x] For `TenantAttributionUnambiguous` invariant, add `conflicting_sources` extension (source names only)
  - [x] Ensure `guidance_link` from RefusalMapping is included in extensions
  - [x] HTTP status is 422 (UnprocessableEntity) per RefusalMapping
  - [x] Type is `urn:tenantsaas:error:tenant-attribution-unambiguous`

- [x] Task 5: Verify RefusalMapping for TenantAttributionUnambiguous (AC: #2)
  - [x] Confirm `TrustContractV1.RefusalMappings` contains entry for `TenantAttributionUnambiguous`
  - [x] Verify HTTP status is 422 (UnprocessableEntity)
  - [x] Verify Problem Details type is `urn:tenantsaas:error:tenant-attribution-unambiguous`
  - [x] Verify guidance link is present: `https://docs.tenantsaas.dev/errors/attribution-ambiguous`

### Contract Tests

- [x] Task 6: Write contract tests for attribution enforcement (AC: #1, #3)
  - [x] Create `TenantSaas.ContractTests/AttributionEnforcementTests.cs`
  - [x] Test: `RequireUnambiguousAttribution` with successful attribution → returns success
  - [x] Test: `RequireUnambiguousAttribution` with ambiguous result → returns failure with `InvariantCode.TenantAttributionUnambiguous`
  - [x] Test: `RequireUnambiguousAttribution` with not-found result → returns failure with `InvariantCode.TenantAttributionUnambiguous`
  - [x] Test: `RequireUnambiguousAttribution` with not-allowed result → returns failure with `InvariantCode.TenantAttributionUnambiguous`
  - [x] Test: Failure result always contains trace_id

- [x] Task 7: Write contract tests for disclosure safety (AC: #2)
  - [x] Update `TenantSaas.ContractTests/AttributionEnforcementTests.cs`
  - [x] Test: Ambiguous attribution failure detail does NOT contain tenant ID values
  - [x] Test: Ambiguous attribution failure detail DOES contain conflict count
  - [x] Test: Problem Details for attribution failure includes `guidance_link`
  - [x] Test: Problem Details for attribution failure includes `conflicting_sources` with source names only
  - [x] Test: Problem Details `detail` field does not contain any tenant identifier patterns

- [x] Task 8: Write contract tests for Problem Details shape (AC: #2, #3)
  - [x] Update `TenantSaas.ContractTests/AttributionEnforcementTests.cs`
  - [x] Test: `EnforcementProblemDetails.FromAttributionEnforcementResult` with TenantAttributionUnambiguous failure → HTTP 422
  - [x] Test: Problem Details includes `invariant_code = "TenantAttributionUnambiguous"`
  - [x] Test: Problem Details includes `trace_id`
  - [x] Test: Problem Details `type` matches `urn:tenantsaas:error:tenant-attribution-unambiguous`

### Sample Integration

- [x] Task 9: Update TenantContextMiddleware to enforce attribution (AC: #1, #3)
  - [x] Update `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs`
  - [x] After resolving attribution (using `TenantAttributionResolver`), call `BoundaryGuard.RequireUnambiguousAttribution`
  - [x] If enforcement fails, return Problem Details via `EnforcementProblemDetails`
  - [x] If enforcement succeeds, proceed with context initialization
  - [x] Order: ContextInitialized check → Attribution resolution → Attribution enforcement → Context set

- [x] Task 10: Add test endpoint with multiple attribution sources (AC: #1)
  - [x] Update test endpoints in `TenantSaas.Sample` that accept tenant from both header and route
  - [x] Demonstrate that conflicting values result in 422 refusal
  - [x] Demonstrate that matching values result in success
  - [x] Document in endpoint comments

## Dev Notes

### Story Context

This is **Story 3.2**, the second story of Epic 3 (Refuse-by-Default Enforcement). It extends the boundary enforcement from Story 3.1 to handle ambiguous tenant attribution.

**Why This Matters:**
- Without this enforcement, conflicting tenant signals could silently pick a winner
- Ambiguity represents a security risk: which tenant did the caller intend?
- The trust contract requires explicit, unambiguous tenant attribution
- This closes a critical attack vector: injection via secondary attribution sources

**Dependency Chain:**
- **Depends on Story 2.2**: Uses `TenantAttributionResult`, `TenantAttributionSource`, `TenantAttributionResolver`
- **Depends on Story 2.3**: Uses `InvariantCode.TenantAttributionUnambiguous`, `RefusalMapping`
- **Depends on Story 2.5**: Uses disclosure policy for safe error messages
- **Depends on Story 3.1**: Extends `BoundaryGuard` and uses `EnforcementResult` pattern
- **Blocks Story 3.3**: Problem Details standardization builds on this
- **Blocks Story 5.2**: Compliance tests for attribution rules

### Key Requirements from Epics

**From Story 3.2 Acceptance Criteria (epics.md):**
> Given tenant attribution is present but ambiguous per the trust contract  
> When enforcement is evaluated  
> Then the operation is refused  
> And the refusal references TenantAttributionUnambiguous

> Given an ambiguous attribution refusal  
> When refusal details are produced  
> Then they do not leak tenant existence information  
> And they include actionable guidance via the contract mapping

**From PRD (FR6, NFR1, NFR3):**
- FR6: Operations with missing or ambiguous tenant attribution are refused by default
- NFR1: Reject 100% of operations with missing or ambiguous tenant attribution
- NFR3: Zero silent fallbacks when required context is missing

**From Architecture:**
> Refuse-by-default enforcement with strictly constrained break-glass path  
> All refusals include invariant_code + trace_id; request_id is included for request execution

### Learnings from Previous Stories

**From Story 2.2 (Attribution Rules):**
1. **TenantAttributionResult discriminated union**: Success, Ambiguous, NotFound, NotAllowed
2. **AttributionConflict struct**: Contains `Source` and `ProvidedTenantId`
3. **TenantAttributionResolver**: Reference implementation in Core
4. **Precedence-based resolution**: Sources checked in defined order

**From Story 3.1 (ContextInitialized Enforcement):**
1. **BoundaryGuard pattern**: Static methods returning `EnforcementResult`
2. **EnforcementResult pattern**: Success with context, Failure with invariantCode + traceId + detail
3. **EnforcementProblemDetails**: Converts EnforcementResult to ProblemDetails
4. **TenantContextMiddleware**: Single enforcement point in sample host

**Code Patterns to Follow:**
```csharp
// BoundaryGuard static method pattern
public static EnforcementResult RequireUnambiguousAttribution(
    TenantAttributionResult result,
    string traceId)
{
    ArgumentNullException.ThrowIfNull(result);
    ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

    return result switch
    {
        TenantAttributionResult.Success success => 
            EnforcementResult.Success(...),
        TenantAttributionResult.Ambiguous ambiguous =>
            EnforcementResult.Failure(
                InvariantCode.TenantAttributionUnambiguous,
                traceId,
                BuildAmbiguousDetail(ambiguous.Conflicts.Count)),
        TenantAttributionResult.NotFound =>
            EnforcementResult.Failure(...),
        TenantAttributionResult.NotAllowed notAllowed =>
            EnforcementResult.Failure(...),
        _ => throw new InvalidOperationException("Unknown attribution result type")
    };
}

// Disclosure-safe detail message
private static string BuildAmbiguousDetail(int conflictCount)
    => $"Tenant attribution is ambiguous: {conflictCount} sources provided conflicting tenant identifiers.";
```

**File Organization Pattern:**
```
TenantSaas.Core/
├── Enforcement/
│   ├── BoundaryGuard.cs (extend)
│   ├── EnforcementResult.cs (exists)
│   └── AttributionEnforcementResult.cs (new, optional)
└── Errors/
    └── EnforcementProblemDetails.cs (extend)

TenantSaas.ContractTests/
└── AttributionEnforcementTests.cs (new)

TenantSaas.Sample/
└── Middleware/
    └── TenantContextMiddleware.cs (extend)
```

### Technical Requirements

**BoundaryGuard Extension:**
```csharp
/// <summary>
/// Requires that tenant attribution is unambiguous.
/// </summary>
/// <param name="result">Attribution resolution result.</param>
/// <param name="traceId">Trace identifier for correlation.</param>
/// <returns>Success with resolved tenant, or failure with invariant violation.</returns>
public static EnforcementResult RequireUnambiguousAttribution(
    TenantAttributionResult result,
    string traceId)
```

**Disclosure Safety Requirements:**
- Detail messages MUST NOT contain actual tenant ID values
- Detail messages MUST NOT reveal which tenants conflicted
- Detail messages MAY contain:
  - Number of conflicting sources
  - Names of attribution sources (e.g., "Route Parameter", "Header Value")
  - Generic guidance text
- Problem Details extensions MAY include `conflicting_sources` as array of source names

**RefusalMapping Verification:**
```csharp
// Expected mapping in TrustContractV1.RefusalMappings
[InvariantCode.TenantAttributionUnambiguous] = RefusalMapping.ForUnprocessableEntity(
    InvariantCode.TenantAttributionUnambiguous,
    title: "Tenant attribution is ambiguous",
    guidanceUri: "https://docs.tenantsaas.dev/errors/attribution-ambiguous")
```

**Middleware Integration Flow:**
```
Request arrives
    ↓
[ContextInitialized check] - Story 3.1
    ↓
[Extract attribution sources from request]
    ↓
[TenantAttributionResolver.Resolve()]
    ↓
[BoundaryGuard.RequireUnambiguousAttribution()] - Story 3.2 (this story)
    ↓ Failure?
    └── Return Problem Details (422)
    ↓ Success?
[Initialize TenantContext with resolved tenant]
    ↓
[Set ambient context]
    ↓
[Continue to endpoint]
```

### Architecture Compliance

**From Architecture Document:**
- Problem Details type: `urn:tenantsaas:error:tenant-attribution-unambiguous`
- HTTP status for attribution ambiguity: 422 Unprocessable Entity
- Required extensions: `invariant_code`, `trace_id`, `request_id` (when applicable)
- Disclosure policy: never leak tenant identifiers in error messages

**Testing Standards:**
- Use xUnit for all tests
- Use FluentAssertions for assertions
- Tests must assert invariant violations via Problem Details shape + `invariant_code`
- Contract tests live in `TenantSaas.ContractTests` only

### Project Structure Notes

- Existing `BoundaryGuard.cs` in [TenantSaas.Core/Enforcement/BoundaryGuard.cs](TenantSaas.Core/Enforcement/BoundaryGuard.cs) - extend with new method
- Existing `EnforcementProblemDetails.cs` in [TenantSaas.Core/Errors/EnforcementProblemDetails.cs](TenantSaas.Core/Errors/EnforcementProblemDetails.cs) - may need extension for conflicting_sources
- Existing `TenantContextMiddleware.cs` in [TenantSaas.Sample/Middleware/TenantContextMiddleware.cs](TenantSaas.Sample/Middleware/TenantContextMiddleware.cs) - extend for attribution enforcement
- New `AttributionEnforcementTests.cs` in TenantSaas.ContractTests

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.2]
- [Source: _bmad-output/planning-artifacts/architecture.md#Error Handling Patterns]
- [Source: TenantSaas.Abstractions/TrustContract/TrustContractV1.cs#RefusalMappings]
- [Source: TenantSaas.Abstractions/Tenancy/TenantAttributionResult.cs]
- [Source: TenantSaas.Core/Enforcement/BoundaryGuard.cs]
- [Source: TenantSaas.Core/Tenancy/TenantAttributionResolver.cs]
- [Source: _bmad-output/implementation-artifacts/3-1-enforce-contextinitialized-at-boundary-helpers.md]

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5

### Debug Log References

N/A

### Completion Notes List

**Story 3.2 Implementation Complete - 2026-02-01**

✅ **All Acceptance Criteria Satisfied:**
- AC#1: Ambiguous attribution is refused with `TenantAttributionUnambiguous` invariant ✓
- AC#2: Refusal details are disclosure-safe and include guidance ✓
- AC#3: Attribution enforcement runs at boundary with standardized error ✓

**Implementation Summary:**
1. Created `AttributionEnforcementResult` - specialized result type for attribution enforcement with disclosure-safe fields
2. Extended `BoundaryGuard.RequireUnambiguousAttribution()` - validates attribution results and returns enforcement decisions
3. Updated `EnforcementProblemDetails` - added `FromAttributionEnforcementResult()` method with `conflicting_sources` extension
4. Integrated middleware - `TenantContextMiddleware` now resolves attribution, enforces unambiguity, then initializes context
5. Added test endpoints - demonstrate attribution from route parameters and headers

**Key Technical Decisions:**
- Used specialized `AttributionEnforcementResult` instead of generic `EnforcementResult` because attribution stage happens BEFORE context initialization
- Built disclosure safety into factory methods - no tenant IDs ever leak into error messages
- Source names (e.g., "Route Parameter") included in `conflicting_sources` extension for debugging without disclosure risks
- Middleware flow: extract sources → resolve attribution → enforce unambiguity → initialize context → verify context

**Test Coverage:**
- 17 attribution enforcement tests (14 original + 3 added during code review)
- All 193 total tests passing
- Coverage includes: success cases, ambiguous conflicts, not-found, not-allowed, disclosure safety, Problem Details shape, GetDisplayName() contract validation

**Code Review & Quality Improvements (2026-02-01):**
- Removed dead code: unused `BuildAmbiguousDetail()` method in BoundaryGuard
- Added comprehensive endpoint documentation with attribution behavior examples
- Added 3 contract tests validating GetDisplayName() behavior and disclosure safety
- Added XML documentation warning about disclosure safety for custom attribution source implementations
- Added explanatory comments for null-forgiving operator safety and hardcoded sample configuration
- Enhanced middleware documentation explaining reference implementation pattern vs. production configuration

### File List

**New Files:**
- TenantSaas.Core/Enforcement/AttributionEnforcementResult.cs
- TenantSaas.ContractTests/AttributionEnforcementTests.cs

**Modified Files:**
- TenantSaas.Core/Enforcement/BoundaryGuard.cs
- TenantSaas.Core/Errors/EnforcementProblemDetails.cs
- TenantSaas.Sample/Middleware/TenantContextMiddleware.cs
- TenantSaas.Sample/Program.cs

