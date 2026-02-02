# Story 2.5: Define Disclosure Policy and tenant_ref Safe States

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a precise disclosure policy,
So that logs and errors do not become tenant existence oracles.

## Acceptance Criteria

1. **Given** tenant disclosure is evaluated
   **When** I review the disclosure policy contract
   **Then** I see defined `tenant_ref` safe states (e.g., `unknown`, `sensitive`, `cross_tenant`, opaque id)
   **And** the policy specifies when tenant information may appear in errors
   **And** this is verified by a test

2. **Given** a refusal occurs
   **When** Problem Details are constructed
   **Then** tenant information is included only when disclosure is safe
   **And** the policy is available to refusal mapping and logging enrichment
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Task 1: Define TenantRefSafeState constants (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Disclosure/TenantRefSafeState.cs`
  - [x] Define stable string constants:
    - `Unknown = "unknown"` - tenant is unresolved or attribution failed
    - `Sensitive = "sensitive"` - tenant resolved but unsafe to disclose
    - `CrossTenant = "cross_tenant"` - admin/global cross-tenant operations
    - `Opaque = "opaque"` - opaque public tenant identifier (safe to disclose)
  - [x] Add XML documentation describing each safe state semantics
  - [x] Add static `IsSafeState(string value)` helper method

- [x] Task 2: Define DisclosureContext type (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Disclosure/DisclosureContext.cs`
  - [x] Define immutable record with:
    - `string? TenantId` - the actual tenant identifier (may be null)
    - `bool IsAuthenticated` - whether caller is authenticated
    - `bool IsAuthorizedForTenant` - whether caller is authorized for the tenant
    - `bool IsEnumerationRisk` - whether disclosure risks enumeration attack
    - `TenantScope? Scope` - current tenant scope (from context)
  - [x] Use sealed record with init properties
  - [x] Add static factory method `Create(TenantContext context, bool isAuthenticated, bool isAuthorizedForTenant)`

- [x] Task 3: Define DisclosurePolicy type (AC: #1, #2)
  - [x] Create `TenantSaas.Abstractions/Disclosure/DisclosurePolicy.cs`
  - [x] Define sealed class with:
    - `string ResolveTenantRef(DisclosureContext context)` - returns safe state or opaque id
    - `bool AllowTenantInErrors(DisclosureContext context)` - returns true if safe to include in errors
    - `bool AllowTenantInLogs(DisclosureContext context)` - returns true if safe to include in logs
  - [x] Implement default policy logic:
    - If scope is NoTenant → return `unknown`
    - If scope is SharedSystem or cross-tenant operation → return `cross_tenant`
    - If not authenticated → return `unknown`
    - If authenticated but not authorized for tenant → return `sensitive`
    - If enumeration risk → return `sensitive`
    - If authenticated and authorized → return opaque tenant id or `opaque` marker
  - [x] AllowTenantInErrors: only when authenticated AND authorized AND no enumeration risk
  - [x] AllowTenantInLogs: always allowed with safe state (never raw internal id)

- [x] Task 4: Define IDisclosurePolicyProvider interface (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Disclosure/IDisclosurePolicyProvider.cs`
  - [x] Define interface with:
    - `DisclosurePolicy GetPolicy()` - returns the active disclosure policy
  - [x] Document contract: implementations may vary by environment or configuration
  - [x] This is an extension seam (referenced in FR16)

- [x] Task 5: Add DisclosureSafe invariant validation helper (AC: #1, #2)
  - [x] Create `TenantSaas.Abstractions/Disclosure/DisclosureValidator.cs`
  - [x] Implement static validation method:
    - `DisclosureValidationResult Validate(DisclosureContext context, string? disclosedTenantRef)`
  - [x] Return result indicating:
    - If `disclosedTenantRef` is a raw internal id when context says disclosure is unsafe → violation with DisclosureSafe
    - If disclosed when enumeration risk → violation with DisclosureSafe
    - Otherwise → valid
  - [x] Result type includes `IsValid`, `InvariantCode`, and `Reason`

- [x] Task 6: Define TenantRef helper type (AC: #1, #2)
  - [x] Create `TenantSaas.Abstractions/Disclosure/TenantRef.cs`
  - [x] Define sealed record with:
    - `string Value` - the safe tenant reference value
    - `bool IsSafeState` - whether this is a safe-state token vs opaque id
  - [x] Add static factory methods:
    - `ForUnknown()` → `new TenantRef(TenantRefSafeState.Unknown, isSafeState: true)`
    - `ForSensitive()` → `new TenantRef(TenantRefSafeState.Sensitive, isSafeState: true)`
    - `ForCrossTenant()` → `new TenantRef(TenantRefSafeState.CrossTenant, isSafeState: true)`
    - `ForOpaque(string opaqueId)` → `new TenantRef(opaqueId, isSafeState: false)`
  - [x] Override `ToString()` to return `Value`

- [x] Task 7: Update TrustContractV1 with disclosure constants (AC: #1)
  - [x] Add constants in `TrustContractV1.cs`:
    - `DisclosureSafeStateUnknown = "unknown"`
    - `DisclosureSafeStateSensitive = "sensitive"`
    - `DisclosureSafeStateCrossTenant = "cross_tenant"`
    - `DisclosureSafeStateOpaque = "opaque"`
  - [x] Add `RequiredDisclosureSafeStates` collection
  - [x] Document the disclosure policy semantics in XML comments

- [x] Task 8: Write contract tests for disclosure safe states (AC: #1)
  - [x] Create `TenantSaas.ContractTests/DisclosurePolicyTests.cs`
  - [x] Test all safe state constants are defined
  - [x] Test TenantRefSafeState.IsSafeState helper
  - [x] Test TenantRef factory methods produce correct values
  - [x] Test TenantRef.IsSafeState property

- [x] Task 9: Write contract tests for disclosure policy (AC: #1, #2)
  - [x] Update `TenantSaas.ContractTests/DisclosurePolicyTests.cs`
  - [x] Test policy returns `unknown` for NoTenant scope
  - [x] Test policy returns `cross_tenant` for SharedSystem scope
  - [x] Test policy returns `unknown` for unauthenticated caller
  - [x] Test policy returns `sensitive` for unauthorized caller
  - [x] Test policy returns `sensitive` for enumeration risk
  - [x] Test policy returns opaque id for authorized caller
  - [x] Test AllowTenantInErrors returns false when unauthorized
  - [x] Test AllowTenantInErrors returns false for enumeration risk
  - [x] Test AllowTenantInErrors returns true for authorized + no risk
  - [x] Test AllowTenantInLogs always returns true (with safe state)

- [x] Task 10: Write contract tests for disclosure validation (AC: #2)
  - [x] Update `TenantSaas.ContractTests/DisclosurePolicyTests.cs`
  - [x] Test validator rejects raw tenant id when disclosure is unsafe
  - [x] Test validator rejects disclosure when enumeration risk
  - [x] Test validator accepts safe-state tokens always
  - [x] Test validator accepts opaque id when disclosure is safe
  - [x] Test validator returns DisclosureSafe invariant code on violation

- [x] Task 11: Update docs/trust-contract.md (AC: #1, #2)
  - [x] Add Disclosure Policy section
  - [x] Document tenant_ref safe states with definitions
  - [x] Document when tenant information may appear in errors
  - [x] Document when tenant information appears in logs
  - [x] Add examples of safe vs unsafe disclosure scenarios
  - [x] Reference code contracts and test coverage

## Dev Notes

### Story Context

This is **Story 2.5**, the final story of Epic 2 (Trust Contract v1 Foundations). It defines the disclosure policy that prevents tenant existence oracle attacks.

**Why This Matters:**
- Without disclosure policy, error messages become tenant existence oracles
- Attackers can enumerate tenants by probing for different error messages
- Safe states provide consistent, non-revealing tenant references
- Logs need tenant context for debugging but must not leak sensitive identifiers
- This completes the Trust Contract v1 foundation

**Dependency Chain:**
- **Depends on Story 2.1**: Uses TenantScope and TenantContext types
- **Depends on Story 2.3**: References DisclosureSafe invariant
- **Depends on Story 2.4**: Uses cross_tenant marker semantics
- **Blocks Story 3.3**: Problem Details construction needs disclosure policy
- **Blocks Story 3.4**: Log enrichment needs tenant_ref safe states
- **Blocks Epic 5 Story 5.5**: Contract tests need disclosure policy assertions

### Key Requirements from Epics

**From Story 2.5 Acceptance Criteria (epics.md):**
> Given tenant disclosure is evaluated  
> When I review the disclosure policy contract  
> Then I see defined tenant_ref safe states (e.g., unknown, sensitive, cross_tenant, opaque id)  
> And the policy specifies when tenant information may appear in errors

> Given a refusal occurs  
> When Problem Details are constructed  
> Then tenant information is included only when disclosure is safe  
> And the policy is available to refusal mapping and logging enrichment

**From PRD (FR19, FR20, FR28):**
- FR19: Tenant disclosure policy is explicit: tenant_ref is always logged using safe states; errors include tenant info only when safe to disclose
- FR20: Trust contract defines disclosure policy
- FR28: All refusals include invariant_code + trace_id; request_id is included for request execution

**From Architecture (Logging Patterns):**
> Tenant reference states for logs:
> - `tenant_ref = <opaque tenant id>` when safe to disclose.
> - `tenant_ref = "unknown"` when unresolved.
> - `tenant_ref = "sensitive"` when resolved but unsafe to disclose.
> - `tenant_ref = "cross_tenant"` for admin/global operations.

### Learnings from Previous Stories

**From Story 2.4 (Break-Glass Contract):**
1. **cross_tenant marker**: Already defined as `TrustContractV1.BreakGlassMarkerCrossTenant = "cross_tenant"` - reuse this constant
2. **Immutable types**: All contract types are immutable sealed records
3. **Factory methods**: Provide static factory methods for common cases
4. **Validation results**: Return result types with `IsValid`, `InvariantCode`, and `Reason`

**From Story 2.3 (Invariant Registry):**
1. **DisclosureSafe invariant**: Already defined in `InvariantCode.DisclosureSafe`
2. **RefusalMapping exists**: `TrustContractV1.RefusalMappings[InvariantCode.DisclosureSafe]` returns HTTP 500
3. **FrozenDictionary**: Use for immutable registries

**From Story 2.1 (Context Taxonomy):**
1. **TenantScope types**: `TenantScope.Tenant`, `TenantScope.SharedSystem`, `TenantScope.NoTenant`
2. **NoTenantReason**: Has `Value` property for serialization
3. **TenantId**: Has `Value` property for the identifier

**File Organization:**
```
TenantSaas.Abstractions/
├── Disclosure/                    (new folder)
│   ├── DisclosureContext.cs       (new)
│   ├── DisclosurePolicy.cs        (new)
│   ├── DisclosureValidator.cs     (new)
│   ├── IDisclosurePolicyProvider.cs (new)
│   ├── TenantRef.cs               (new)
│   └── TenantRefSafeState.cs      (new)
└── TrustContract/
    └── TrustContractV1.cs         (update)
```

### Technical Requirements

**TenantRefSafeState Design:**

```csharp
namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Defines safe-state tokens for tenant reference disclosure.
/// </summary>
public static class TenantRefSafeState
{
    /// <summary>Tenant is unresolved or attribution failed.</summary>
    public const string Unknown = "unknown";
    
    /// <summary>Tenant resolved but unsafe to disclose.</summary>
    public const string Sensitive = "sensitive";
    
    /// <summary>Admin/global cross-tenant operations.</summary>
    public const string CrossTenant = "cross_tenant";
    
    /// <summary>Marker indicating value is an opaque public identifier.</summary>
    public const string Opaque = "opaque";

    /// <summary>Returns true if the value is a recognized safe-state token.</summary>
    public static bool IsSafeState(string? value)
        => value is Unknown or Sensitive or CrossTenant;
}
```

**DisclosureContext Design:**

```csharp
namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Represents the context for evaluating tenant disclosure safety.
/// </summary>
public sealed record DisclosureContext
{
    /// <summary>Gets the actual tenant identifier (may be null).</summary>
    public string? TenantId { get; init; }
    
    /// <summary>Gets whether the caller is authenticated.</summary>
    public bool IsAuthenticated { get; init; }
    
    /// <summary>Gets whether the caller is authorized for the tenant.</summary>
    public bool IsAuthorizedForTenant { get; init; }
    
    /// <summary>Gets whether disclosure risks enumeration attack.</summary>
    public bool IsEnumerationRisk { get; init; }
    
    /// <summary>Gets the current tenant scope.</summary>
    public TenantScope? Scope { get; init; }
}
```

**DisclosurePolicy Logic:**

```csharp
namespace TenantSaas.Abstractions.Disclosure;

/// <summary>
/// Implements the default disclosure policy for tenant references.
/// </summary>
public class DisclosurePolicy
{
    /// <summary>
    /// Resolves the safe tenant reference based on disclosure context.
    /// </summary>
    public virtual TenantRef ResolveTenantRef(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // NoTenant scope → unknown
        if (context.Scope is TenantScope.NoTenant)
            return TenantRef.ForUnknown();

        // SharedSystem or cross-tenant → cross_tenant
        if (context.Scope is TenantScope.SharedSystem)
            return TenantRef.ForCrossTenant();

        // Not authenticated → unknown
        if (!context.IsAuthenticated)
            return TenantRef.ForUnknown();

        // Authenticated but not authorized → sensitive
        if (!context.IsAuthorizedForTenant)
            return TenantRef.ForSensitive();

        // Enumeration risk → sensitive
        if (context.IsEnumerationRisk)
            return TenantRef.ForSensitive();

        // Safe to disclose opaque identifier
        return context.TenantId is not null
            ? TenantRef.ForOpaque(context.TenantId)
            : TenantRef.ForUnknown();
    }

    /// <summary>
    /// Returns true if tenant information may appear in error responses.
    /// </summary>
    public virtual bool AllowTenantInErrors(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.IsAuthenticated 
            && context.IsAuthorizedForTenant 
            && !context.IsEnumerationRisk;
    }

    /// <summary>
    /// Returns true if tenant information may appear in logs.
    /// Always true - logs use safe states.
    /// </summary>
    public virtual bool AllowTenantInLogs(DisclosureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return true; // Logs always get safe states, never raw identifiers
    }
}
```

### Testing Requirements

**Contract Tests Structure:**

```csharp
public class DisclosurePolicyTests
{
    // Safe state tests
    [Fact]
    public void TenantRefSafeState_Unknown_IsDefined()
    
    [Fact]
    public void TenantRefSafeState_IsSafeState_ReturnsTrue_ForKnownStates()
    
    [Fact]
    public void TenantRefSafeState_IsSafeState_ReturnsFalse_ForOpaqueIds()
    
    // TenantRef tests
    [Fact]
    public void TenantRef_ForUnknown_ReturnsSafeState()
    
    [Fact]
    public void TenantRef_ForOpaque_ReturnsOpaqueId()
    
    // Policy resolution tests
    [Fact]
    public void ResolveTenantRef_NoTenantScope_ReturnsUnknown()
    
    [Fact]
    public void ResolveTenantRef_SharedSystem_ReturnsCrossTenant()
    
    [Fact]
    public void ResolveTenantRef_Unauthenticated_ReturnsUnknown()
    
    [Fact]
    public void ResolveTenantRef_Unauthorized_ReturnsSensitive()
    
    [Fact]
    public void ResolveTenantRef_EnumerationRisk_ReturnsSensitive()
    
    [Fact]
    public void ResolveTenantRef_AuthorizedAndSafe_ReturnsOpaqueId()
    
    // Error disclosure tests
    [Fact]
    public void AllowTenantInErrors_Unauthorized_ReturnsFalse()
    
    [Fact]
    public void AllowTenantInErrors_EnumerationRisk_ReturnsFalse()
    
    [Fact]
    public void AllowTenantInErrors_AuthorizedAndSafe_ReturnsTrue()
    
    // Validation tests
    [Fact]
    public void Validator_RawIdWhenUnsafe_ReturnsViolation()
    
    [Fact]
    public void Validator_SafeState_ReturnsValid()
}
```

### Architecture Compliance

**From Architecture (Error Handling Patterns):**
- Tenant disclosure policy: include `tenant_ref` in Problem Details only when the caller is authenticated/authorized for that tenant and disclosure is not an enumeration risk; otherwise omit it
- `tenant_ref` is the disclosure-safe tenant identifier used in logs/errors. It is either an opaque public tenant ID or a safe-state token (`unknown`, `sensitive`, `cross_tenant`) when disclosure is unsafe; it must not be a raw internal ID or a reversible identifier

**Naming Conventions:**
- PascalCase for types, properties, and constants
- No underscore prefixes
- camelCase for JSON serialization

### Example Disclosure Scenarios

**Scenario 1: Unauthorized caller probes tenant**
```
Context: IsAuthenticated=true, IsAuthorizedForTenant=false
Result: tenant_ref="sensitive", AllowTenantInErrors=false
Error Response: { "type": "...", "tenant_ref": null } // omitted
Log: { "tenant_ref": "sensitive", ... }
```

**Scenario 2: Authorized caller normal operation**
```
Context: IsAuthenticated=true, IsAuthorizedForTenant=true, TenantId="acme-corp"
Result: tenant_ref="acme-corp", AllowTenantInErrors=true
Error Response: { "type": "...", "tenant_ref": "acme-corp" }
Log: { "tenant_ref": "acme-corp", ... }
```

**Scenario 3: Health check (no tenant)**
```
Context: Scope=NoTenant(HealthCheck)
Result: tenant_ref="unknown", AllowTenantInErrors=true
Error Response: { "type": "...", "tenant_ref": "unknown" }
Log: { "tenant_ref": "unknown", ... }
```

**Scenario 4: Admin cross-tenant operation**
```
Context: Scope=SharedSystem
Result: tenant_ref="cross_tenant", AllowTenantInErrors=true
Error Response: { "type": "...", "tenant_ref": "cross_tenant" }
Log: { "tenant_ref": "cross_tenant", ... }
```

### File Structure After Implementation

```
TenantSaas.Abstractions/
├── BreakGlass/
│   └── ...
├── Contexts/
│   └── ExecutionKind.cs
├── Disclosure/                    (new)
│   ├── DisclosureContext.cs
│   ├── DisclosurePolicy.cs
│   ├── DisclosureValidator.cs
│   ├── DisclosureValidationResult.cs
│   ├── IDisclosurePolicyProvider.cs
│   ├── TenantRef.cs
│   └── TenantRefSafeState.cs
├── Invariants/
│   ├── InvariantCode.cs
│   ├── InvariantDefinition.cs
│   └── RefusalMapping.cs
├── Tenancy/
│   └── ...
└── TrustContract/
    └── TrustContractV1.cs (updated)

TenantSaas.ContractTests/
├── DisclosurePolicyTests.cs (new)
├── BreakGlassContractTests.cs
├── InvariantRegistryTests.cs
└── ...

docs/
└── trust-contract.md (updated)
```

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR19, FR20, FR28]
- [Source: _bmad-output/planning-artifacts/architecture.md#Logging Patterns]
- [Source: docs/trust-contract.md]
- [Source: TenantSaas.Abstractions/Invariants/InvariantCode.cs - DisclosureSafe]
- [Source: TenantSaas.Abstractions/TrustContract/TrustContractV1.cs - RefusalMappings]
- [Source: TenantSaas.Abstractions/BreakGlass/AuditCode.cs - cross_tenant marker]

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- 2026-02-02: `dotnet test /home/vlad/Source/TenantSaas/TenantSaas.sln`

### Completion Notes List

- Implemented disclosure contracts (safe states, context, policy, validator, tenant ref, provider).
- Updated trust contract with disclosure safe-state constants and required collection.
- Added contract tests covering disclosure safe states, policy behavior, and validation.
- Documented disclosure policy and examples in trust contract docs.
- [Code Review Fix] H1: Removed `Opaque` from `TenantRefSafeState.IsSafeState()` — Opaque is a marker, not a safe-state token.
- [Code Review Fix] H3: Fixed `DisclosureValidator` to allow safe-state tokens during enumeration risk — safe states never leak info.
- [Code Review Fix] M1: Added `isEnumerationRisk` optional parameter to `DisclosureContext.Create()` factory.
- [Code Review Fix] M3: Added tests for `DisclosureContext.Create()` factory method.

### File List

- TenantSaas.Abstractions/Disclosure/DisclosureContext.cs
- TenantSaas.Abstractions/Disclosure/DisclosurePolicy.cs
- TenantSaas.Abstractions/Disclosure/DisclosureValidationResult.cs
- TenantSaas.Abstractions/Disclosure/DisclosureValidator.cs
- TenantSaas.Abstractions/Disclosure/IDisclosurePolicyProvider.cs
- TenantSaas.Abstractions/Disclosure/TenantRef.cs
- TenantSaas.Abstractions/Disclosure/TenantRefSafeState.cs
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs
- TenantSaas.ContractTests/DisclosurePolicyTests.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- docs/trust-contract.md
