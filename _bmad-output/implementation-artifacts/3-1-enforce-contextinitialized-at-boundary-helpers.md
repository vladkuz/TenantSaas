# Story 3.1: Enforce ContextInitialized at Boundary Helpers

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a platform engineer,
I want boundary enforcement to refuse when context is not initialized,
So that missing tenant context fails immediately and predictably.

## Acceptance Criteria

1. **Given** an operation enters a sanctioned enforcement boundary
   **When** no context has been initialized for the execution flow
   **Then** the operation is refused by default
   **And** the refusal references the ContextInitialized invariant
   **And** this is verified by a test

2. **Given** a refusal due to missing context
   **When** the error is produced
   **Then** it uses the standardized refusal mapping schema
   **And** it includes invariant_code and trace_id

## Tasks / Subtasks

### Core Boundary Enforcement

- [x] Task 1: Create ITenantContextAccessor interface (AC: #1)
  - [x] Define `TenantSaas.Abstractions/Tenancy/ITenantContextAccessor.cs`
  - [x] Add `TenantContext? Current { get; }` property for retrieving current context
  - [x] Add `bool IsInitialized { get; }` helper property
  - [x] Add XML documentation explaining contract: null context means not initialized
  - [x] This is an extension seam (referenced in FR16)

- [x] Task 2: Create boundary enforcement helper (AC: #1, #2)
  - [x] Create `TenantSaas.Core/Enforcement/BoundaryGuard.cs`
  - [x] Implement `static EnforcementResult RequireContext(ITenantContextAccessor accessor, string? overrideTraceId = null)`
  - [x] Check if `accessor.IsInitialized` is false → return violation with `InvariantCode.ContextInitialized`
  - [x] If context is initialized → return success with context
  - [x] Result type: `EnforcementResult` (success/failure + context or violation details)
  - [x] Use `overrideTraceId` parameter or generate new GUID if context is missing

- [x] Task 3: Create EnforcementResult type (AC: #1, #2)
  - [x] Create `TenantSaas.Core/Enforcement/EnforcementResult.cs`
  - [x] Define sealed record with discriminated union pattern:
    - `bool IsSuccess`
    - `TenantContext? Context` (when success)
    - `string? InvariantCode` (when failure)
    - `string? TraceId` (always present)
    - `string? Detail` (human-readable failure reason)
  - [x] Add static factory methods:
    - `Success(TenantContext context)`
    - `Failure(string invariantCode, string traceId, string detail)`
  - [x] Follow existing pattern from Story 2.5 `DisclosureValidationResult`

- [x] Task 4: Create ambient context accessor implementation (AC: #1)
  - [x] Create `TenantSaas.Core/Tenancy/AmbientTenantContextAccessor.cs`
  - [x] Implement `ITenantContextAccessor` using `AsyncLocal<TenantContext?>` for storage
  - [x] Add `void Set(TenantContext context)` method for initialization
  - [x] Add `void Clear()` method for cleanup
  - [x] Ensure thread-safe and async-safe propagation
  - [x] Document: this is the default accessor; explicit passing is also supported

- [x] Task 5: Create explicit context accessor implementation (AC: #1)
  - [x] Create `TenantSaas.Core/Tenancy/ExplicitTenantContextAccessor.cs`
  - [x] Implement `ITenantContextAccessor` using constructor injection pattern
  - [x] Context is passed explicitly per operation, not ambient
  - [x] Add `WithContext(TenantContext context)` factory method returning new instance
  - [x] Document: this is the explicit-passing alternative to ambient

### Refusal Mapping Integration

- [x] Task 6: Create Problem Details factory for enforcement violations (AC: #2)
  - [x] Create `TenantSaas.Core/Errors/EnforcementProblemDetails.cs`
  - [x] Implement `static ProblemDetails FromEnforcementResult(EnforcementResult result, HttpContext? context = null)`
  - [x] Map `ContextInitialized` invariant → HTTP 401 Unauthorized (unauthenticated)
  - [x] Use `TrustContractV1.RefusalMappings[invariantCode]` for status and type
  - [x] Include `invariant_code`, `trace_id` in extensions
  - [x] Include `request_id` if available from HttpContext
  - [x] Use Problem Details type from refusal mapping
  - [x] Detail message should reference "context not initialized" explicitly

- [x] Task 7: Update RefusalMapping for ContextInitialized (AC: #2)
  - [x] Verify `TrustContractV1.RefusalMappings` already contains `ContextInitialized`
  - [x] If missing, add mapping: HTTP 401, type `urn:tenantsaas:error:context-not-initialized`, guidance link
  - [x] Status should be 401 because context initialization proves identity/authorization
  - [x] Detail: "Tenant context must be initialized before operations can proceed."

### Contract Tests

- [x] Task 8: Write contract tests for boundary guard (AC: #1)
  - [x] Create or update `TenantSaas.ContractTests/Invariants/ContextInitializedTests.cs`
  - [x] Test: `BoundaryGuard.RequireContext` with uninitialized accessor → returns failure
  - [x] Test: Failure result contains `InvariantCode.ContextInitialized`
  - [x] Test: Failure result contains trace_id (either provided or generated)
  - [x] Test: `BoundaryGuard.RequireContext` with initialized context → returns success
  - [x] Test: Success result contains the initialized context

- [x] Task 9: Write contract tests for accessor implementations (AC: #1)
  - [x] Update `TenantSaas.ContractTests/Invariants/ContextInitializedTests.cs`
  - [x] Test: `AmbientTenantContextAccessor` starts with `IsInitialized = false`
  - [x] Test: After `Set(context)`, `Current` returns context and `IsInitialized = true`
  - [x] Test: After `Clear()`, `IsInitialized = false` again
  - [x] Test: Ambient context propagates across `await` boundaries
  - [x] Test: `ExplicitTenantContextAccessor` with context returns `IsInitialized = true`
  - [x] Test: `ExplicitTenantContextAccessor` without context returns `IsInitialized = false`

- [x] Task 10: Write contract tests for Problem Details mapping (AC: #2)
  - [x] Update `TenantSaas.ContractTests/Invariants/ContextInitializedTests.cs`
  - [x] Test: `EnforcementProblemDetails.FromEnforcementResult` with ContextInitialized failure → HTTP 401
  - [x] Test: Problem Details includes `invariant_code = "ContextInitialized"`
  - [x] Test: Problem Details includes `trace_id`
  - [x] Test: Problem Details `type` matches `RefusalMapping` entry
  - [x] Test: Problem Details `detail` references "context not initialized"

### Sample Integration (Demonstration)

- [x] Task 11: Create tenant context middleware for sample host (AC: #1)
  - [x] Create `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs`
  - [x] Middleware checks if context is already initialized (idempotency)
  - [x] If not initialized, resolve tenant scope from request (placeholder logic for now)
  - [x] Call `BoundaryGuard.RequireContext` before proceeding
  - [x] If enforcement fails, return Problem Details via `EnforcementProblemDetails`
  - [x] If enforcement succeeds, set ambient context and call `next()`
  - [x] Use `AmbientTenantContextAccessor.Set(context)` for initialization

- [x] Task 12: Wire middleware into sample host pipeline (AC: #1)
  - [x] Update `TenantSaas.Sample/Program.cs`
  - [x] Add `app.UseMiddleware<TenantContextMiddleware>()` after auth, before endpoints
  - [x] Register `ITenantContextAccessor` as singleton pointing to `AmbientTenantContextAccessor`
  - [x] Document: this demonstrates the single unavoidable integration point

- [x] Task 13: Add health check endpoint as no-tenant example (AC: #1)
  - [x] Update `TenantSaas.Sample/Program.cs` or create health endpoint
  - [x] Health check uses `TenantScope.NoTenant(NoTenantReason.HealthCheck)`
  - [x] Demonstrates that explicit no-tenant initialization is allowed
  - [x] Should NOT fail `RequireContext` - context is initialized, just no-tenant scope

## Dev Notes

### Story Context

This is **Story 3.1**, the first story of Epic 3 (Refuse-by-Default Enforcement). It creates the foundational enforcement boundary that all operations must pass through.

**Why This Matters:**
- Without enforcement, tenant context is optional and mistakes are silent
- This story makes missing context LOUD and IMMEDIATE
- The boundary guard is the single unavoidable choke point (FR11, FR13)
- Establishes the pattern for all other invariant enforcement in Epic 3
- Creates the "refuse-by-default" posture that is core to the trust contract

**Dependency Chain:**
- **Depends on Story 2.1**: Uses `TenantContext`, `TenantScope`, `ExecutionKind`
- **Depends on Story 2.3**: Uses `InvariantCode.ContextInitialized`, `RefusalMapping`
- **Depends on Story 2.5**: Uses disclosure policy for error tenant_ref
- **Blocks Story 3.2**: Tenant attribution enforcement builds on this boundary
- **Blocks Story 3.3**: Problem Details refusal standardization uses this pattern
- **Blocks Story 4.1**: Context initialization primitives call this guard
- **Blocks Epic 5**: Contract tests need this enforcement to validate

### Key Requirements from Epics

**From Story 3.1 Acceptance Criteria (epics.md):**
> Given an operation enters a sanctioned enforcement boundary  
> When no context has been initialized for the execution flow  
> Then the operation is refused by default  
> And the refusal references the ContextInitialized invariant

> Given a refusal due to missing context  
> When the error is produced  
> Then it uses the standardized refusal mapping schema  
> And it includes invariant_code and trace_id

**From PRD (FR11, FR13, FR26, NFR1, NFR3):**
- FR11: The system provides a single, unavoidable integration point for tenant context initialization
- FR13: The integration point can reject execution when required context is absent or inconsistent
- FR26: The core surface blocks bypass paths by restricting entry points to sanctioned boundaries
- NFR1: The system shall reject 100% of operations with missing or ambiguous tenant attribution
- NFR3: The system shall produce zero silent fallbacks when required context is missing

**From Architecture:**
> API Boundaries:
> - External API is only in TenantSaas.Sample (Minimal APIs).
> - Error boundary is enforced via Problem Details middleware.

> Integration Point:
> - Request → API key auth → tenant context middleware → invariant guard → reference adapter

> Errors:
> - Use RFC 7807 Problem Details with fixed fields: type, title, status, detail, instance
> - Extensions: invariant_code, trace_id, tenant_ref (only when safe)

### Learnings from Previous Stories

**From Story 2.1 (Context Taxonomy):**
1. **TenantContext is immutable**: Sealed record with factory methods (ForRequest, ForBackground, etc.)
2. **TraceId is required**: All contexts have trace_id; request execution also has request_id
3. **UTC timestamps**: Use `DateTimeOffset.UtcNow` and ensure UTC offset
4. **Validation in constructor**: Use `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrWhiteSpace`

**From Story 2.3 (Invariant Registry):**
1. **InvariantCode constants**: `ContextInitialized` already defined in `InvariantCode` class
2. **RefusalMapping pattern**: `TrustContractV1.RefusalMappings` is a `FrozenDictionary<string, RefusalMapping>`
3. **HTTP status mapping**: Each invariant maps to specific HTTP status (401/403/422/500)
4. **Type URIs**: Use format `urn:tenantsaas:error:{kebab-case-name}`

**From Story 2.5 (Disclosure Policy):**
1. **Result pattern**: Use sealed record with `IsValid`/`IsSuccess` + discriminated fields
2. **Validation helper**: Static validation methods that return result objects
3. **Safe-state handling**: tenant_ref uses safe states when disclosure is unsafe
4. **Factory methods**: Provide static factories like `ForUnknown()`, `ForSensitive()`

**Code Patterns to Follow:**
```csharp
// Immutable records with private constructor + static factories
public sealed record TenantContext { }
public static TenantContext ForRequest(...) => new(...);

// Result pattern with discriminated union
public sealed record EnforcementResult
{
    public bool IsSuccess { get; init; }
    public TenantContext? Context { get; init; }
    public string? InvariantCode { get; init; }
}

// Validation in constructors
ArgumentNullException.ThrowIfNull(scope);
ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

// AsyncLocal for ambient context
private static readonly AsyncLocal<TenantContext?> _current = new();
```

**File Organization Pattern:**
```
TenantSaas.Abstractions/
├── Tenancy/
│   ├── ITenantContextAccessor.cs (new)
│   ├── TenantContext.cs (exists)
│   └── TenantScope.cs (exists)

TenantSaas.Core/
├── Tenancy/
│   ├── AmbientTenantContextAccessor.cs (new)
│   └── ExplicitTenantContextAccessor.cs (new)
├── Enforcement/
│   ├── BoundaryGuard.cs (new)
│   └── EnforcementResult.cs (new)
└── Errors/
    └── EnforcementProblemDetails.cs (new)

TenantSaas.Sample/
└── Middleware/
    └── TenantContextMiddleware.cs (new)
```

### Technical Requirements

**ITenantContextAccessor Contract:**

```csharp
namespace TenantSaas.Abstractions.Tenancy;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
/// <remarks>
/// This is an extension seam. Implementations may use ambient propagation
/// (AsyncLocal) or explicit context passing. Null context indicates the
/// context has not been initialized for the current execution flow.
/// </remarks>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant context, or null if not initialized.
    /// </summary>
    TenantContext? Current { get; }
    
    /// <summary>
    /// Gets whether a context has been initialized for this flow.
    /// </summary>
    bool IsInitialized => Current is not null;
}
```

**BoundaryGuard Implementation:**

```csharp
namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Enforces tenant context invariants at sanctioned boundaries.
/// </summary>
public static class BoundaryGuard
{
    /// <summary>
    /// Requires that tenant context has been initialized.
    /// </summary>
    /// <param name="accessor">Context accessor to check.</param>
    /// <param name="overrideTraceId">Optional trace ID for correlation when context is missing.</param>
    /// <returns>Success with context, or failure with invariant violation.</returns>
    public static EnforcementResult RequireContext(
        ITenantContextAccessor accessor,
        string? overrideTraceId = null)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        if (!accessor.IsInitialized)
        {
            var traceId = overrideTraceId ?? Guid.NewGuid().ToString("N");
            return EnforcementResult.Failure(
                InvariantCode.ContextInitialized,
                traceId,
                "Tenant context must be initialized before operations can proceed.");
        }

        return EnforcementResult.Success(accessor.Current!);
    }
}
```

**EnforcementResult Pattern:**

```csharp
namespace TenantSaas.Core.Enforcement;

/// <summary>
/// Represents the result of an enforcement check.
/// </summary>
public sealed record EnforcementResult
{
    private EnforcementResult(
        bool isSuccess,
        TenantContext? context,
        string? invariantCode,
        string? traceId,
        string? detail)
    {
        IsSuccess = isSuccess;
        Context = context;
        InvariantCode = invariantCode;
        TraceId = traceId;
        Detail = detail;
    }

    /// <summary>
    /// Gets whether enforcement succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the validated tenant context when successful.
    /// </summary>
    public TenantContext? Context { get; }

    /// <summary>
    /// Gets the invariant code when enforcement failed.
    /// </summary>
    public string? InvariantCode { get; }

    /// <summary>
    /// Gets the trace identifier for correlation.
    /// </summary>
    public string? TraceId { get; }

    /// <summary>
    /// Gets the human-readable detail message when enforcement failed.
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Creates a successful enforcement result.
    /// </summary>
    public static EnforcementResult Success(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(
            isSuccess: true,
            context: context,
            invariantCode: null,
            traceId: context.TraceId,
            detail: null);
    }

    /// <summary>
    /// Creates a failed enforcement result.
    /// </summary>
    public static EnforcementResult Failure(
        string invariantCode,
        string traceId,
        string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invariantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);

        return new(
            isSuccess: false,
            context: null,
            invariantCode: invariantCode,
            traceId: traceId,
            detail: detail);
    }
}
```

**AmbientTenantContextAccessor Implementation:**

```csharp
namespace TenantSaas.Core.Tenancy;

/// <summary>
/// Provides ambient tenant context using AsyncLocal propagation.
/// </summary>
/// <remarks>
/// This is the default accessor implementation. Context flows automatically
/// across async/await boundaries within the same logical execution flow.
/// Each new request/job/admin/script must explicitly initialize its own context.
/// </remarks>
public sealed class AmbientTenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContext?> _current = new();

    /// <inheritdoc />
    public TenantContext? Current => _current.Value;

    /// <summary>
    /// Sets the current tenant context for this execution flow.
    /// </summary>
    public void Set(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _current.Value = context;
    }

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    public void Clear()
    {
        _current.Value = null;
    }
}
```

**Problem Details Mapping:**

```csharp
namespace TenantSaas.Core.Errors;

/// <summary>
/// Creates Problem Details from enforcement results.
/// </summary>
public static class EnforcementProblemDetails
{
    /// <summary>
    /// Converts an enforcement failure to Problem Details.
    /// </summary>
    public static ProblemDetails FromEnforcementResult(
        EnforcementResult result,
        HttpContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        
        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "Cannot create Problem Details from successful enforcement.");
        }

        var mapping = TrustContractV1.RefusalMappings[result.InvariantCode!];
        
        var problemDetails = new ProblemDetails
        {
            Type = mapping.Type,
            Title = mapping.Title,
            Status = mapping.Status,
            Detail = result.Detail,
            Instance = context?.Request.Path
        };

        problemDetails.Extensions["invariant_code"] = result.InvariantCode;
        problemDetails.Extensions["trace_id"] = result.TraceId;
        
        if (context is not null && !string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            problemDetails.Extensions["request_id"] = context.TraceIdentifier;
        }

        return problemDetails;
    }
}
```

### Testing Requirements

**Contract Test Structure:**

```csharp
public class ContextInitializedTests
{
    [Fact]
    public void BoundaryGuard_RequireContext_Uninitialized_ReturnsFailure()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        
        // Act
        var result = BoundaryGuard.RequireContext(accessor);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.InvariantCode.Should().Be(InvariantCode.ContextInitialized);
        result.TraceId.Should().NotBeNullOrWhiteSpace();
        result.Detail.Should().Contain("context must be initialized");
    }

    [Fact]
    public void BoundaryGuard_RequireContext_Initialized_ReturnsSuccess()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        accessor.Set(context);
        
        // Act
        var result = BoundaryGuard.RequireContext(accessor);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Context.Should().Be(context);
        result.InvariantCode.Should().BeNull();
    }

    [Fact]
    public async Task AmbientAccessor_PropagatesAcrossAwait()
    {
        // Arrange
        var accessor = new AmbientTenantContextAccessor();
        var scope = TenantScope.ForTenant(new TenantId("tenant-1"));
        var context = TenantContext.ForRequest(scope, "trace-123", "req-456");
        accessor.Set(context);
        
        // Act
        await Task.Delay(1);
        var retrieved = accessor.Current;
        
        // Assert
        retrieved.Should().Be(context);
    }

    [Fact]
    public void EnforcementProblemDetails_FromFailure_ReturnsHttp401()
    {
        // Arrange
        var result = EnforcementResult.Failure(
            InvariantCode.ContextInitialized,
            "trace-123",
            "Context not initialized");
        
        // Act
        var problemDetails = EnforcementProblemDetails.FromEnforcementResult(result);
        
        // Assert
        problemDetails.Status.Should().Be(401);
        problemDetails.Extensions["invariant_code"].Should().Be("ContextInitialized");
        problemDetails.Extensions["trace_id"].Should().Be("trace-123");
        problemDetails.Type.Should().Contain("context-not-initialized");
    }
}
```

### Architecture Compliance

**From Architecture (Error Handling Patterns):**
- Use RFC 7807 Problem Details with fixed fields: type, title, status, detail, instance
- Extensions: invariant_code, trace_id, tenant_ref (only when safe), errors for validation
- For 500s, use generic title/detail with trace_id + invariant_code
- HTTP mapping discipline: 401/403 auth, 404 not found, 409 conflict, 500+ server faults

**From Architecture (Logging Patterns):**
- Required fields: tenant_ref, trace_id, request_id, invariant_code, event_name, severity
- Prefer invariant_code (string) over GUID-like IDs

**From Architecture (Naming Conventions):**
- PascalCase for types, methods, and constants
- No underscore prefixes anywhere
- camelCase for JSON serialization

**From Architecture (Code Naming Conventions):**
- Standard .NET naming: PascalCase types/methods, camelCase locals/fields
- No underscore prefixes anywhere

### RefusalMapping Update

Need to verify `ContextInitialized` mapping in `TrustContractV1.RefusalMappings`:

```csharp
[InvariantCode.ContextInitialized] = new RefusalMapping(
    Status: 401,
    Type: "urn:tenantsaas:error:context-not-initialized",
    Title: "Context Not Initialized",
    Detail: "Tenant context must be initialized before operations can proceed.",
    GuidanceUri: new Uri("https://docs.tenantsaas.dev/errors/context-not-initialized")
)
```

**Rationale for HTTP 401:**
- Context initialization is the first proof of identity/scope
- Without it, the system cannot authenticate the tenant scope
- 401 Unauthorized is appropriate because we cannot verify who/what is making the request

### Sample Middleware Example

```csharp
namespace TenantSaas.Sample.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantContextAccessor _accessor;

    public TenantContextMiddleware(
        RequestDelegate next,
        ITenantContextAccessor accessor)
    {
        _next = next;
        _accessor = accessor;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Skip if already initialized (idempotency)
        if (_accessor.IsInitialized)
        {
            await _next(httpContext);
            return;
        }

        // Resolve tenant scope (placeholder - will be Story 3.2)
        var scope = ResolveTenantScope(httpContext);
        var traceId = httpContext.TraceIdentifier;
        var requestId = httpContext.TraceIdentifier;
        
        var context = TenantContext.ForRequest(scope, traceId, requestId);
        
        // Initialize ambient context
        if (_accessor is AmbientTenantContextAccessor ambient)
        {
            ambient.Set(context);
        }

        // Enforce boundary
        var result = BoundaryGuard.RequireContext(_accessor);
        
        if (!result.IsSuccess)
        {
            var problemDetails = EnforcementProblemDetails.FromEnforcementResult(
                result,
                httpContext);
            
            httpContext.Response.StatusCode = problemDetails.Status ?? 500;
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await _next(httpContext);
    }

    private TenantScope ResolveTenantScope(HttpContext context)
    {
        // Placeholder: always return no-tenant for health checks
        // Story 3.2 will implement full attribution resolution
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return TenantScope.NoTenant(NoTenantReason.HealthCheck);
        }
        
        // For now, return a dummy tenant
        return TenantScope.ForTenant(new TenantId("placeholder-tenant"));
    }
}
```

### File Structure After Implementation

```
TenantSaas.Abstractions/
├── Tenancy/
│   ├── ITenantContextAccessor.cs (new)
│   ├── TenantContext.cs (exists)
│   ├── TenantScope.cs (exists)
│   └── TenantId.cs (exists)
├── Invariants/
│   └── InvariantCode.cs (exists - has ContextInitialized)
└── TrustContract/
    └── TrustContractV1.cs (update RefusalMappings)

TenantSaas.Core/
├── Tenancy/
│   ├── AmbientTenantContextAccessor.cs (new)
│   └── ExplicitTenantContextAccessor.cs (new)
├── Enforcement/
│   ├── BoundaryGuard.cs (new)
│   └── EnforcementResult.cs (new)
└── Errors/
    └── EnforcementProblemDetails.cs (new)

TenantSaas.Sample/
├── Middleware/
│   └── TenantContextMiddleware.cs (new)
└── Program.cs (update)

TenantSaas.ContractTests/
└── Invariants/
    └── ContextInitializedTests.cs (new)
```

### Integration Points

**From Architecture (Data Flow):**
> Request → API key auth → tenant context middleware → invariant guard → reference adapter

**This Story Implements:**
- Tenant context middleware (initialization point)
- Invariant guard (BoundaryGuard.RequireContext)
- Problem Details on refusal

**Next Story Will Add:**
- Tenant attribution resolution (Story 3.2)
- Full refusal standardization (Story 3.3)

### Example Scenarios

**Scenario 1: Uninitialized context (missing middleware)**
```
Request: GET /tenants
Middleware: Skipped or not wired
BoundaryGuard: accessor.IsInitialized = false
Result: HTTP 401, invariant_code="ContextInitialized"
Response: Problem Details with trace_id for debugging
```

**Scenario 2: Initialized context (normal flow)**
```
Request: GET /tenants
Middleware: Initializes TenantContext.ForRequest(...)
BoundaryGuard: accessor.IsInitialized = true
Result: Success, operation proceeds
```

**Scenario 3: Health check (no-tenant allowed)**
```
Request: GET /health
Middleware: Initializes TenantContext with NoTenant(HealthCheck)
BoundaryGuard: accessor.IsInitialized = true (context exists, just no tenant)
Result: Success, operation proceeds
```

**Scenario 4: Explicit break-glass (future story)**
```
Request: POST /admin/cross-tenant-fix
Middleware: Initializes TenantContext with SharedSystem scope
BoundaryGuard: accessor.IsInitialized = true
BreakGlassGuard: Validates actor + reason (Story 3.5)
Result: Success if break-glass is valid
```

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.1]
- [Source: _bmad-output/planning-artifacts/prd.md#FR11, FR13, FR26, NFR1, NFR3]
- [Source: _bmad-output/planning-artifacts/architecture.md#API Boundaries, Integration Point]
- [Source: TenantSaas.Abstractions/Tenancy/TenantContext.cs]
- [Source: TenantSaas.Abstractions/Invariants/InvariantCode.cs - ContextInitialized constant]
- [Source: TenantSaas.Abstractions/TrustContract/TrustContractV1.cs - RefusalMappings]
- [Source: Story 2.1 - TenantContext, TenantScope, ExecutionKind]
- [Source: Story 2.3 - InvariantCode, RefusalMapping pattern]
- [Source: Story 2.5 - Result pattern, validation helpers]

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5

### Debug Log References

- Followed red-green-refactor cycle for all implementations
- Created failing tests first, then implemented to pass
- Fixed middleware method call: `TenantScope.NoTenant()` → `TenantScope.ForNoTenant()`
- Updated existing test expectation: HTTP 400 → 401 for ContextInitialized invariant per story requirements

### Completion Notes List

**Story 3.1 Complete - All Tasks Implemented and Tested**

**Core Boundary Enforcement (Tasks 1-5):**
- ✅ ITenantContextAccessor interface created with Current and IsInitialized properties
- ✅ BoundaryGuard.RequireContext enforces context initialization at boundaries
- ✅ EnforcementResult type follows discriminated union pattern (success/failure)
- ✅ AmbientTenantContextAccessor implements AsyncLocal propagation
- ✅ ExplicitTenantContextAccessor provides explicit context passing alternative

**Refusal Mapping Integration (Tasks 6-7):**
- ✅ EnforcementProblemDetails converts enforcement failures to RFC 7807 Problem Details
- ✅ ContextInitialized refusal mapping updated to HTTP 401 Unauthorized
- ✅ Added RefusalMapping.ForUnauthorized factory method
- ✅ Problem Details includes invariant_code, trace_id, request_id extensions

**Contract Tests (Tasks 8-10):**
- ✅ 14 comprehensive tests for boundary guard enforcement
- ✅ Tests verify ambient and explicit accessor behavior
- ✅ Tests confirm async context propagation works correctly
- ✅ Tests validate Problem Details mapping to HTTP 401 with correct extensions

**Sample Integration (Tasks 11-13):**
- ✅ TenantContextMiddleware demonstrates single integration point
- ✅ Middleware enforces boundary before request processing
- ✅ Health endpoint uses NoTenant scope (demonstrates valid no-tenant flow)
- ✅ Registered ITenantContextAccessor as singleton in DI container

**Test Results:**
- All 163 tests passing (14 new tests added)
- No regressions in existing test suite
- Full red-green-refactor cycle followed for all implementations

**Key Technical Decisions:**
1. Used HTTP 401 (not 400) for ContextInitialized per story specification - context initialization is authentication/authorization concern
2. Middleware uses placeholder tenant resolution - full attribution logic is Story 3.2
3. EnforcementResult follows same pattern as DisclosureValidationResult from Story 2.5
4. Added FrameworkReference to Microsoft.AspNetCore.App in TenantSaas.Core for ProblemDetails support

**Code Review Fixes Applied (2026-02-01):**
- ✅ Created `IMutableTenantContextAccessor` interface for proper Set/Clear abstraction
- ✅ Middleware now uses try/finally to ensure context cleanup on all exit paths
- ✅ Added extension seam (FR16) documentation to ITenantContextAccessor
- ✅ Documented static AsyncLocal behavior in AmbientTenantContextAccessor
- ✅ Added TODO comment explaining traceId vs requestId placeholder (Story 3.2)
- ✅ Added 13 validation tests for null/invalid inputs (EnforcementResult, BoundaryGuard, accessors)
- ✅ Updated DI registration to provide both ITenantContextAccessor and IMutableTenantContextAccessor
- ✅ Test count: 163 → 176 tests passing

### File List

**New Files:**
- TenantSaas.Abstractions/Tenancy/ITenantContextAccessor.cs
- TenantSaas.Abstractions/Tenancy/IMutableTenantContextAccessor.cs
- TenantSaas.Core/Enforcement/BoundaryGuard.cs
- TenantSaas.Core/Enforcement/EnforcementResult.cs
- TenantSaas.Core/Tenancy/AmbientTenantContextAccessor.cs
- TenantSaas.Core/Tenancy/ExplicitTenantContextAccessor.cs
- TenantSaas.Core/Errors/EnforcementProblemDetails.cs
- TenantSaas.Sample/Middleware/TenantContextMiddleware.cs
- TenantSaas.ContractTests/ContextInitializedTests.cs

**Modified Files:**
- TenantSaas.Abstractions/Invariants/RefusalMapping.cs (added ForUnauthorized factory)
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs (ContextInitialized → HTTP 401)
- TenantSaas.Core/TenantSaas.Core.csproj (added FrameworkReference)
- TenantSaas.Sample/Program.cs (added middleware and DI registration for both interfaces)
- TenantSaas.Sample/TenantSaas.Sample.csproj (added TenantSaas.Core reference)
- TenantSaas.ContractTests/InvariantRegistryTests.cs (updated HTTP status expectation)

**Total Files Changed:** 15 files (9 new, 6 modified)
