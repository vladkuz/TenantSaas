# Story 3.3: Standardize Problem Details Refusals for Invariant Violations

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want invariant violations to return a stable, machine-meaningful refusal shape,
So that clients, tests, and logs can rely on consistent semantics.

## Acceptance Criteria

1. **Given** any invariant violation occurs at an enforcement boundary
   **When** a refusal is returned
   **Then** it is expressed as RFC 7807 Problem Details
   **And** it uses stable type identifiers and invariant_code values within the major version
   **And** this is verified by a test

2. **Given** a refusal is constructed
   **When** correlation data is available
   **Then** trace_id is always present
   **And** request_id is included for request execution kinds
   **And** this is verified by a test

3. **Given** an invariant violation occurs
   **When** the response is produced
   **Then** the response must return an error with a stable invariant_code

## Tasks / Subtasks

### Unified Problem Details Factory

- [x] Task 1: Create centralized ProblemDetailsFactory (AC: #1, #2, #3)
  - [x] Create `TenantSaas.Core/Errors/ProblemDetailsFactory.cs`
  - [x] Implement `static ProblemDetails FromInvariantViolation(string invariantCode, string traceId, string? requestId = null, string? detail = null, IDictionary<string, object?>? extensions = null)`
  - [x] Load RefusalMapping from `TrustContractV1.RefusalMappings[invariantCode]`
  - [x] Set HTTP status from RefusalMapping
  - [x] Set `type` from RefusalMapping
  - [x] Set `title` from RefusalMapping
  - [x] Set `detail` from parameter (with fallback to RefusalMapping description)
  - [x] Set `instance` to null (per architecture - not using instance URIs)
  - [x] Always include `invariant_code` in extensions
  - [x] Always include `trace_id` in extensions
  - [x] Include `request_id` in extensions when provided
  - [x] Include `guidance_link` from RefusalMapping in extensions
  - [x] Merge any additional extensions from parameter

- [x] Task 2: Add disclosure-safe overload for tenant references (AC: #1, #2)
  - [x] Add overload: `FromInvariantViolation(string invariantCode, string traceId, string? requestId, string? detail, string? tenantRef, IDictionary<string, object?>? extensions = null)`
  - [x] Include `tenant_ref` in extensions only when `tenantRef` parameter is provided
  - [x] Validate that `tenantRef` uses safe-state values when applicable
  - [x] Document: Only include tenant_ref when disclosure is safe per policy

- [x] Task 3: Add convenience methods for common invariant codes (AC: #1, #3)
  - [x] Add `ForContextNotInitialized(string traceId, string? requestId = null)` → HTTP 401
  - [x] Add `ForTenantAttributionAmbiguous(string traceId, string? requestId = null, IReadOnlyList<string>? conflictingSources = null)` → HTTP 422
  - [x] Add `ForTenantScopeRequired(string traceId, string? requestId = null)` → HTTP 403
  - [x] Add `ForBreakGlassRequired(string traceId, string? requestId = null)` → HTTP 403
  - [x] Add `ForDisclosureUnsafe(string traceId, string? requestId = null)` → HTTP 500
  - [x] Each convenience method calls `FromInvariantViolation` with appropriate invariantCode

### Remove Legacy Problem Details Classes

- [x] Task 4: ~~Mark EnforcementProblemDetails as deprecated (AC: #1)~~ **REVISED: Remove entirely**
  - [x] ~~Add `[Obsolete("Use ProblemDetailsFactory instead")]` to `TenantSaas.Core/Errors/EnforcementProblemDetails.cs`~~
  - [x] ~~Update XML documentation to point to ProblemDetailsFactory~~
  - [x] ~~Keep existing functionality for backward compatibility in this story~~
  - [x] ~~Document: Will be removed in next major version~~
  - **Revision Note:** Deprecation is unnecessary in greenfield - no external consumers exist

- [x] Task 4b: Delete EnforcementProblemDetails entirely (AC: #1)
  - [x] Delete `TenantSaas.Core/Errors/EnforcementProblemDetails.cs`
  - [x] Update `ContextInitializedTests.cs` to use `ProblemDetailsFactory` (3 usages)
  - [x] Update `AttributionEnforcementTests.cs` to use `ProblemDetailsFactory` (6 usages)
  - [x] Remove any remaining `using TenantSaas.Core.Errors` referencing EnforcementProblemDetails
  - [x] Verify zero CS0618 (obsolete) warnings on build
  - [x] All 290 tests pass

- [x] Task 5: Update BoundaryGuard to use ProblemDetailsFactory (AC: #1, #3)
  - [x] ~~Update `RequireContext` to return ProblemDetails directly via ProblemDetailsFactory~~
  - [x] ~~Update `RequireUnambiguousAttribution` to return ProblemDetails directly via ProblemDetailsFactory~~
  - [x] ~~Remove dependency on EnforcementProblemDetails~~
  - [x] BoundaryGuard correctly returns domain enforcement results (EnforcementResult/AttributionEnforcementResult), not Problem Details
  - [x] Problem Details conversion happens in middleware layer via ProblemDetailsFactory
  - [x] No changes needed - architecture is correct as-is

### Middleware Integration

- [x] Task 6: Update TenantContextMiddleware to use ProblemDetailsFactory (AC: #1, #2, #3)
  - [x] Update `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs`
  - [x] Replace all EnforcementProblemDetails calls with ProblemDetailsFactory
  - [x] Ensure request_id is passed from HttpContext when available
  - [x] Ensure trace_id is always included
  - [x] Document the standardized refusal pattern in middleware comments

- [x] Task 7: Add Problem Details middleware for global exception handling (AC: #1, #2)
  - [x] Create `TenantSaas.Sample/Middleware/ProblemDetailsExceptionMiddleware.cs`
  - [x] Catch unhandled exceptions and convert to Problem Details
  - [x] Use generic 500 error with trace_id for unexpected exceptions
  - [x] Never leak exception details in production
  - [x] Include invariant_code = "InternalServerError" for unhandled exceptions
  - [x] Log full exception with structured fields

### Contract Tests

- [x] Task 8: Write contract tests for ProblemDetailsFactory (AC: #1, #2, #3)
  - [x] Create `TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs`
  - [x] Test: `FromInvariantViolation` with ContextInitialized → HTTP 401, correct type, invariant_code, trace_id
  - [x] Test: `FromInvariantViolation` with TenantAttributionUnambiguous → HTTP 422, correct type, invariant_code, trace_id
  - [x] Test: `FromInvariantViolation` with unknown invariant code → throws KeyNotFoundException
  - [x] Test: `FromInvariantViolation` with request_id → includes request_id in extensions
  - [x] Test: `FromInvariantViolation` without request_id → does not include request_id in extensions
  - [x] Test: `FromInvariantViolation` with tenant_ref → includes tenant_ref in extensions
  - [x] Test: Convenience methods produce correct HTTP status codes
  - [x] Test: All extension fields are present in correct format

- [x] Task 9: Write contract tests for Problem Details shape validation (AC: #1, #2, #3)
  - [x] Create `TenantSaas.ContractTests/Errors/ProblemDetailsShapeTests.cs`
  - [x] Test: All Problem Details responses have required RFC 7807 fields
  - [x] Test: All Problem Details responses have `invariant_code` extension
  - [x] Test: All Problem Details responses have `trace_id` extension
  - [x] Test: Request-scoped Problem Details have `request_id` extension
  - [x] Test: Problem Details `type` URIs follow urn:tenantsaas:error:{invariant-name} pattern
  - [x] Test: Problem Details `type` values are stable across test runs
  - [x] Test: Problem Details HTTP status matches RefusalMapping

- [x] Task 10: Write end-to-end middleware tests (AC: #1, #2, #3)
  - [x] Create `TenantSaas.ContractTests/MiddlewareProblemDetailsTests.cs`
  - [x] Test: Missing context in middleware → returns standardized Problem Details
  - [x] Test: Ambiguous attribution in middleware → returns standardized Problem Details
  - [x] Test: Unhandled exception in endpoint → returns generic 500 Problem Details
  - [x] Test: All middleware refusals include trace_id and request_id
  - [x] Test: All middleware refusals include guidance_link

### Documentation

- [x] Task 11: Update error catalog documentation (AC: #1, #3)
  - [x] Create or update `docs/error-catalog.md`
  - [x] Document all invariant codes and their HTTP status mappings
  - [x] Document all Problem Details type URIs
  - [x] Document all extension fields (invariant_code, trace_id, request_id, guidance_link, tenant_ref)
  - [x] Provide example Problem Details responses for each invariant
  - [x] Document stability guarantee for type URIs and invariant_code values

- [x] Task 12: Update integration guide with error handling examples (AC: #1)
  - [x] Update `docs/integration-guide.md`
  - [x] Add section on "Handling Invariant Violations"
  - [x] Show example client code parsing Problem Details
  - [x] Show example logging of Problem Details responses
  - [x] Document how to extract invariant_code and trace_id for support tickets

## Dev Notes

### Story Context

This is **Story 3.3**, the third story of Epic 3 (Refuse-by-Default Enforcement). It standardizes the Problem Details refusal format that was established in Stories 3.1 and 3.2.

**Why This Matters:**
- Stories 3.1 and 3.2 created two different Problem Details conversion patterns (EnforcementProblemDetails with two separate methods)
- Without standardization, future stories would continue creating inconsistent error handling
- This story creates a single, authoritative factory that all enforcement boundaries use
- Machine-readable, stable error contracts enable reliable client error handling and test automation
- Consistent refusal shape is a core promise of the trust contract

**Dependency Chain:**
- **Depends on Story 2.3**: Uses `InvariantCode`, `RefusalMapping`, `TrustContractV1`
- **Depends on Story 2.5**: Uses disclosure policy for safe tenant_ref inclusion
- **Depends on Story 3.1**: Consolidates EnforcementProblemDetails pattern
- **Depends on Story 3.2**: Consolidates AttributionEnforcementResult pattern
- **Blocks Story 3.4**: Structured logging needs consistent error format
- **Blocks Story 3.5**: Break-glass enforcement needs consistent refusal format
- **Blocks Epic 5**: Contract tests rely on stable Problem Details shape

### Key Requirements from Epics

**From Story 3.3 Acceptance Criteria (epics.md):**
> Given any invariant violation occurs at an enforcement boundary  
> When a refusal is returned  
> Then it is expressed as RFC 7807 Problem Details  
> And it uses stable type identifiers and invariant_code values within the major version

> Given a refusal is constructed  
> When correlation data is available  
> Then trace_id is always present  
> And request_id is included for request execution kinds

**From PRD (FR9, FR28, NFR5, NFR6):**
- FR9: Invariants are enumerated, named, finite, and defined in the trust contract
- FR28: All refusals include invariant_code + trace_id; request_id is included for request execution
- NFR5: Invariant violations return explicit error codes/messages with stable invariant_code values
- NFR6: invariant_code values and Problem Details type identifiers are stable within a major version

**From Architecture:**
> Errors use RFC 7807 Problem Details with fixed fields: type, title, status, detail, instance  
> Extensions: invariant_code, trace_id, tenant_ref (only when safe)  
> type is stable and machine-meaningful (URN or URL)  
> detail must never leak internals or secrets  
> For 500s, use generic title/detail with trace_id + invariant_code

### Learnings from Previous Stories

**From Story 3.1 (ContextInitialized Enforcement):**
1. **EnforcementProblemDetails pattern**: `FromEnforcementResult` method converts EnforcementResult to ProblemDetails
2. **Extension fields**: `invariant_code`, `trace_id`, `request_id` (when applicable), `guidance_link`
3. **RefusalMapping lookup**: `TrustContractV1.RefusalMappings[invariantCode]` provides status, type, title, guidanceUri
4. **HTTP status mapping**: 401 for ContextInitialized (unauthenticated state)

**From Story 3.2 (Attribution Enforcement):**
1. **AttributionEnforcementResult pattern**: Specialized result type with `conflicting_sources` field
2. **Disclosure safety**: Never include actual tenant IDs in error messages or extensions
3. **Additional extensions**: `conflicting_sources` array for attribution ambiguity
4. **HTTP status mapping**: 422 for TenantAttributionUnambiguous (unprocessable entity)

**From Story 2.3 (Invariant Registry):**
1. **InvariantCode constants**: All codes defined in `InvariantCode` class
2. **RefusalMapping structure**: Status, type (URN), title, guidanceUri
3. **Type URI pattern**: `urn:tenantsaas:error:{kebab-case-invariant-name}`
4. **Stability guarantee**: Type URIs and invariant codes are stable within major version

**From Story 2.5 (Disclosure Policy):**
1. **Safe-state values**: `unknown`, `sensitive`, `cross_tenant`, `opaque`
2. **tenant_ref inclusion rules**: Only when disclosure is safe per policy
3. **Validation pattern**: Validate before including in errors or logs

**Code Patterns to Follow:**
```csharp
// Centralized factory pattern
public static class ProblemDetailsFactory
{
    public static ProblemDetails FromInvariantViolation(
        string invariantCode,
        string traceId,
        string? requestId = null,
        string? detail = null,
        IDictionary<string, object?>? extensions = null)
    {
        var mapping = TrustContractV1.GetRefusalMapping(invariantCode);
        
        var problemDetails = new ProblemDetails
        {
            Type = mapping.Type,
            Title = mapping.Title,
            Status = mapping.Status,
            Detail = detail ?? mapping.Description,
            Instance = null
        };

        problemDetails.Extensions["invariant_code"] = invariantCode;
        problemDetails.Extensions["trace_id"] = traceId;
        
        if (requestId is not null)
        {
            problemDetails.Extensions["request_id"] = requestId;
        }
        
        problemDetails.Extensions["guidance_link"] = mapping.GuidanceUri;
        
        // Merge additional extensions
        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }
        
        return problemDetails;
    }
}

// Convenience method pattern
public static ProblemDetails ForContextNotInitialized(
    string traceId,
    string? requestId = null)
    => FromInvariantViolation(
        InvariantCode.ContextInitialized,
        traceId,
        requestId,
        detail: "Tenant context must be initialized before operations can proceed.");
```

**File Organization Pattern:**
```
TenantSaas.Core/
└── Errors/
    ├── ProblemDetailsFactory.cs (new - primary factory)
    ├── EnforcementProblemDetails.cs (existing - mark obsolete)
    └── InvariantProblemDetails.cs (deprecated name, may not exist)

TenantSaas.Sample/
└── Middleware/
    ├── TenantContextMiddleware.cs (update to use factory)
    └── ProblemDetailsExceptionMiddleware.cs (new)

TenantSaas.ContractTests/
└── Errors/
    ├── ProblemDetailsFactoryTests.cs (new)
    └── ProblemDetailsShapeTests.cs (new)

docs/
├── error-catalog.md (new or update)
└── integration-guide.md (update)
```

### Technical Requirements

**ProblemDetailsFactory Core Contract:**

```csharp
namespace TenantSaas.Core.Errors;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses with invariant violations.
/// </summary>
public static class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a Problem Details response for an invariant violation.
    /// </summary>
    /// <param name="invariantCode">The invariant code from <see cref="InvariantCode"/>.</param>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier (required for request execution).</param>
    /// <param name="detail">Human-readable detail message (falls back to RefusalMapping description).</param>
    /// <param name="extensions">Additional extension fields to include.</param>
    /// <returns>A fully populated Problem Details response.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when invariantCode is not registered.</exception>
    public static ProblemDetails FromInvariantViolation(
        string invariantCode,
        string traceId,
        string? requestId = null,
        string? detail = null,
        IDictionary<string, object?>? extensions = null);

    /// <summary>
    /// Creates a Problem Details response with safe tenant reference inclusion.
    /// </summary>
    /// <param name="invariantCode">The invariant code from <see cref="InvariantCode"/>.</param>
    /// <param name="traceId">End-to-end correlation identifier.</param>
    /// <param name="requestId">Request-scoped correlation identifier.</param>
    /// <param name="detail">Human-readable detail message.</param>
    /// <param name="tenantRef">Disclosure-safe tenant reference (safe-state value or opaque ID).</param>
    /// <param name="extensions">Additional extension fields.</param>
    /// <returns>A fully populated Problem Details response with tenant_ref extension.</returns>
    public static ProblemDetails FromInvariantViolation(
        string invariantCode,
        string traceId,
        string? requestId,
        string? detail,
        string? tenantRef,
        IDictionary<string, object?>? extensions = null);

    /// <summary>
    /// Creates a Problem Details response for missing tenant context.
    /// </summary>
    public static ProblemDetails ForContextNotInitialized(
        string traceId,
        string? requestId = null);

    /// <summary>
    /// Creates a Problem Details response for ambiguous tenant attribution.
    /// </summary>
    public static ProblemDetails ForTenantAttributionAmbiguous(
        string traceId,
        string? requestId = null,
        IReadOnlyList<string>? conflictingSources = null);

    /// <summary>
    /// Creates a Problem Details response for missing tenant scope.
    /// </summary>
    public static ProblemDetails ForTenantScopeRequired(
        string traceId,
        string? requestId = null);

    /// <summary>
    /// Creates a Problem Details response for missing break-glass declaration.
    /// </summary>
    public static ProblemDetails ForBreakGlassRequired(
        string traceId,
        string? requestId = null);

    /// <summary>
    /// Creates a Problem Details response for unsafe disclosure.
    /// </summary>
    public static ProblemDetails ForDisclosureUnsafe(
        string traceId,
        string? requestId = null);
}
```

**Problem Details Extension Fields (Required):**
- `invariant_code` (string): Stable invariant identifier from `InvariantCode`
- `trace_id` (string): End-to-end correlation ID
- `request_id` (string, conditional): Request correlation ID (required for request execution kinds)
- `guidance_link` (string): URI from RefusalMapping for error documentation

**Problem Details Extension Fields (Conditional):**
- `tenant_ref` (string, conditional): Disclosure-safe tenant reference (only when safe)
- `conflicting_sources` (array, conditional): Source names for attribution ambiguity
- `errors` (object, conditional): Validation error details (for ValidationProblemDetails)

**Problem Details Shape Contract:**
```json
{
  "type": "urn:tenantsaas:error:context-not-initialized",
  "title": "Tenant context not initialized",
  "status": 401,
  "detail": "Tenant context must be initialized before operations can proceed.",
  "instance": null,
  "invariant_code": "ContextInitialized",
  "trace_id": "a1b2c3d4e5f6",
  "request_id": "req-12345",
  "guidance_link": "https://docs.tenantsaas.dev/errors/context-not-initialized"
}
```

**Middleware Integration Pattern:**
```csharp
// In TenantContextMiddleware
if (!accessor.IsInitialized)
{
    var traceId = httpContext.TraceIdentifier;
    var requestId = httpContext.Request.Headers["X-Request-ID"].FirstOrDefault();
    
    var problemDetails = ProblemDetailsFactory.ForContextNotInitialized(
        traceId,
        requestId);
    
    httpContext.Response.StatusCode = problemDetails.Status!.Value;
    await httpContext.Response.WriteAsJsonAsync(problemDetails);
    return;
}
```

**Exception Middleware Pattern:**
```csharp
// In ProblemDetailsExceptionMiddleware
try
{
    await next(httpContext);
}
catch (Exception ex)
{
    logger.LogError(ex, "Unhandled exception in request pipeline");
    
    var traceId = httpContext.TraceIdentifier;
    var requestId = httpContext.Request.Headers["X-Request-ID"].FirstOrDefault();
    
    var problemDetails = ProblemDetailsFactory.FromInvariantViolation(
        invariantCode: "InternalServerError", // Generic code for unexpected errors
        traceId: traceId,
        requestId: requestId,
        detail: "An unexpected error occurred. Please contact support with the trace ID.");
    
    httpContext.Response.StatusCode = 500;
    await httpContext.Response.WriteAsJsonAsync(problemDetails);
}
```

### Architecture Compliance

**From Architecture Document:**
- Problem Details is the ONLY error format
- All errors MUST use RFC 7807 structure
- Extensions MUST include: `invariant_code`, `trace_id`
- Extensions SHOULD include: `request_id` (for request execution), `guidance_link`
- Type URIs MUST follow pattern: `urn:tenantsaas:error:{kebab-case-name}`
- Type URIs and invariant_code values MUST be stable within major version
- Detail field MUST NOT leak internals, secrets, or raw exceptions

**Testing Standards:**
- Use xUnit for all tests
- Use FluentAssertions for assertions
- Tests must assert Problem Details shape matches RFC 7807
- Tests must assert extension fields are present and correctly formatted
- Tests must assert type URI stability across runs
- Tests must assert HTTP status matches RefusalMapping

### Project Structure Notes

- Create new `ProblemDetailsFactory.cs` in [TenantSaas.Core/Errors/](TenantSaas.Core/Errors/)
- Mark existing `EnforcementProblemDetails.cs` as obsolete but keep for backward compatibility
- Update `TenantContextMiddleware.cs` to use new factory
- Create new exception middleware in [TenantSaas.Sample/Middleware/](TenantSaas.Sample/Middleware/)
- Add contract tests in new `Errors/` subfolder under TenantSaas.ContractTests

### Critical Implementation Notes

**Deprecation Strategy:**
1. Mark `EnforcementProblemDetails` as obsolete in this story
2. Update all middleware and samples to use `ProblemDetailsFactory`
3. Keep `EnforcementProblemDetails` functional for backward compatibility
4. Document removal plan for next major version

**Stability Guarantee:**
- Type URIs (`urn:tenantsaas:error:*`) are stable within major version
- invariant_code values are stable within major version
- Extension field names are stable within major version
- Breaking changes require major version bump + migration guide

**Error Catalog Requirements:**
- Document all 5 invariant codes and their mappings
- Provide JSON examples for each error type
- Show client-side parsing examples
- Document trace_id and request_id usage for support

**Disclosure Safety:**
- Never include `tenant_ref` unless explicitly passed via parameter
- Validate `tenant_ref` uses safe-state values when applicable
- Document disclosure policy in factory XML comments

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.3]
- [Source: _bmad-output/planning-artifacts/architecture.md#Error Handling Patterns]
- [Source: TenantSaas.Abstractions/TrustContract/TrustContractV1.cs#RefusalMappings]
- [Source: TenantSaas.Abstractions/Invariants/InvariantCode.cs]
- [Source: TenantSaas.Core/Errors/EnforcementProblemDetails.cs]
- [Source: _bmad-output/implementation-artifacts/3-1-enforce-contextinitialized-at-boundary-helpers.md]
- [Source: _bmad-output/implementation-artifacts/3-2-enforce-tenantattributionunambiguous-at-boundaries.md]
- [Source: _bmad-output/implementation-artifacts/2-3-define-invariant-registry-and-refusal-mapping-schema.md]
- [Source: _bmad-output/implementation-artifacts/2-5-define-disclosure-policy-and-tenant-ref-safe-states.md]
- [Source: RFC 7807: https://datatracker.ietf.org/doc/html/rfc7807]

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5 (via GitHub Copilot)

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- ✅ Created centralized `ProblemDetailsFactory` with primary factory method and convenience methods
- ✅ Deleted `EnforcementProblemDetails` (greenfield project - no external consumers, deprecation unnecessary)
- ✅ Updated `TenantContextMiddleware` to use new factory
- ✅ Created `ProblemDetailsExceptionMiddleware` for global exception handling
- ✅ Wrote comprehensive contract tests (66 new tests across 4 test files)
- ✅ Created complete error catalog documentation
- ✅ Created integration guide with client examples and error handling patterns

**Test Coverage:**
- 16 tests in `ProblemDetailsFactoryTests` - factory behavior and extension fields
- 34 tests in `ProblemDetailsShapeTests` - RFC 7807 compliance and stability
- 5 tests in `MiddlewareProblemDetailsTests` - end-to-end middleware integration
- 11 tests in `HttpCorrelationExtensionsTests` - distributed tracing header extraction
- **All 290 tests passing** (full regression suite after code review fixes)

**Architecture Decisions:**
- BoundaryGuard remains unchanged - correctly returns domain enforcement results, not Problem Details
- Problem Details conversion happens at middleware layer for proper separation of concerns
- Synthetic "InternalServerError" code used for unhandled exceptions (not in InvariantCode registry)
- All Problem Details responses follow stable contract within major version
- HttpCorrelationExtensions provides proper separation of trace_id (distributed tracing) vs request_id (per-request)

**Documentation Delivered:**
- `docs/error-catalog.md` - Complete catalog of all 5 invariants with examples and remediation guidance
- `docs/integration-guide.md` - Comprehensive guide with C# and TypeScript client examples

### File List

**New Files:**
- `TenantSaas.Core/Errors/ProblemDetailsFactory.cs`
- `TenantSaas.Sample/Middleware/ProblemDetailsExceptionMiddleware.cs`
- `TenantSaas.Sample/Middleware/HttpCorrelationExtensions.cs`
- `TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs`
- `TenantSaas.ContractTests/Errors/ProblemDetailsShapeTests.cs`
- `TenantSaas.ContractTests/MiddlewareProblemDetailsTests.cs`
- `TenantSaas.ContractTests/HttpCorrelationExtensionsTests.cs`
- `docs/error-catalog.md`
- `docs/integration-guide.md`

**Deleted Files:**
- `TenantSaas.Core/Errors/EnforcementProblemDetails.cs` - Removed (greenfield project, no external consumers - per Task 4b)

**Modified Files:**
- `TenantSaas.ContractTests/ContextInitializedTests.cs` - Migrated from EnforcementProblemDetails to ProblemDetailsFactory
- `TenantSaas.ContractTests/AttributionEnforcementTests.cs` - Migrated from EnforcementProblemDetails to ProblemDetailsFactory
- `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs` - Updated to use ProblemDetailsFactory, HttpCorrelationExtensions, and Content-Type header
- `TenantSaas.Sample/Program.cs` - Registered ProblemDetailsExceptionMiddleware in pipeline
- `_bmad-output/implementation-artifacts/sprint-status.yaml` - Story marked in-progress then review

### Code Review Fixes (2026-02-02)

**Issues Fixed:**
1. **[HIGH] ProblemDetailsExceptionMiddleware not registered** - Added middleware registration in Program.cs as outermost middleware
2. **[MEDIUM] TraceId/RequestId same value** - Created `HttpCorrelationExtensions` utility for proper W3C Trace Context and X-Request-ID header extraction
3. **[MEDIUM] Missing Content-Type header** - Added `application/problem+json` Content-Type to all Problem Details responses in TenantContextMiddleware
4. **[LOW] Missing guidance_link in exception middleware** - Added `guidance_link` extension to ProblemDetailsExceptionMiddleware for consistency

**New Tests Added:**
- 11 tests in `HttpCorrelationExtensionsTests` covering traceparent parsing, header precedence, and fallback behavior
