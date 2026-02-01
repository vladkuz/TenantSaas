# Story 2.1: Define Context Taxonomy and Execution Kinds

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a platform engineer,
I want a concrete context taxonomy and execution kind model,
so that every enforcement decision has unambiguous inputs.

## Acceptance Criteria

1. **Given** the Trust Contract v1 scope
   **When** I review the core contracts
   **Then** I can find explicit definitions for tenant scope, shared-system scope, and no-tenant context
   **And** no-tenant context requires a reason/category from an allowed set
   **And** this is verified by a test

2. **Given** the context model
   **When** execution occurs in request, background, admin, or scripted flows
   **Then** the execution kind is represented explicitly in the context
   **And** it is available to enforcement, logging, and refusal mapping
   **And** this is verified by a test

3. **Given** the trust contract is missing a required scope or execution kind definition
   **When** the contract is reviewed
   **Then** the gap is explicitly documented as a blocking issue before implementation proceeds
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Task 1: Create TenantSaas.Abstractions project (AC: #1, #2)
  - [x] Create new classlib project `TenantSaas.Abstractions`
  - [x] Add project reference to solution
  - [x] Configure net10.0 target (via Directory.Build.props)
  - [x] Add XML documentation generation
  - [x] Create project structure (Tenancy/, Invariants/, Contexts/ folders)

- [x] Task 2: Implement TenantScope type (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Tenancy/TenantScope.cs`
  - [x] Define `TenantScope` as a discriminated union or sealed hierarchy:
    - `Tenant(TenantId)` - scoped to a specific tenant
    - `SharedSystem` - shared/cross-tenant operations
    - `NoTenant(NoTenantReason)` - explicit no-tenant context
  - [x] Make `TenantScope` immutable with proper equality
  - [x] Document each variant with XML comments

- [x] Task 3: Implement NoTenantReason type (AC: #1)
  - [x] Create `TenantSaas.Abstractions/Tenancy/NoTenantReason.cs`
  - [x] Define allowed reasons as a closed set:
    - `Public` - public/unauthenticated operations
    - `Bootstrap` - system initialization
    - `HealthCheck` - health/readiness probes
    - `SystemMaintenance` - maintenance operations
  - [x] Make it extensible only through explicit extension point
  - [x] Document the semantic meaning of each reason

- [x] Task 4: Implement ExecutionKind type (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Contexts/ExecutionKind.cs`
  - [x] Define execution kinds:
    - `Request` - HTTP/API request flow
    - `Background` - background job/worker flow
    - `Admin` - administrative operation flow
    - `Scripted` - CLI/script execution flow
  - [x] Include display name and description properties
  - [x] Document when each kind applies

- [x] Task 5: Implement TenantContext type (AC: #1, #2)
  - [x] Create `TenantSaas.Abstractions/Tenancy/TenantContext.cs`
  - [x] Define `TenantContext` with:
    - `Scope` (TenantScope) - required
    - `ExecutionKind` (ExecutionKind) - required
    - `TraceId` (string) - required correlation ID
    - `RequestId` (string?) - optional, present for Request kind
    - `InitializedAt` (DateTimeOffset) - UTC timestamp
  - [x] Factory methods for creating contexts per execution kind
  - [x] Make immutable; no mutation after creation
  - [x] Validate required fields on construction

- [x] Task 6: Implement ITenantContextAccessor interface (AC: #2)
  - [x] Create `TenantSaas.Abstractions/Tenancy/ITenantContextAccessor.cs`
  - [x] Define interface for accessing current context:
    - `TenantContext? Current { get; }`
    - `bool IsInitialized { get; }`
  - [x] Document thread-safety expectations
  - [x] Document that null indicates uninitialized (invariant violation)

- [x] Task 7: Create TrustContract documentation type (AC: #1, #3)
  - [x] Create `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs`
  - [x] Define constants for all scope and execution kind values
  - [x] Include contract version identifier
  - [x] Add static validation method to check contract completeness
  - [x] Document the trust contract version semantics

- [x] Task 8: Write contract tests for context taxonomy (AC: #1, #2, #3)
  - [x] Create `TenantSaas.ContractTests/ContextTaxonomyTests.cs`
  - [x] Test TenantScope variants are complete and documented
  - [x] Test NoTenantReason covers allowed categories
  - [x] Test ExecutionKind covers all flow types
  - [x] Test TenantContext requires scope and execution kind
  - [x] Test TenantContext factory methods produce valid contexts
  - [x] Test trust contract validation identifies gaps

- [x] Task 9: Create docs/trust-contract.md foundation (AC: #1, #3)
  - [x] Create `docs/trust-contract.md` with initial structure
  - [x] Document TenantScope definitions and semantics
  - [x] Document NoTenantReason allowed values
  - [x] Document ExecutionKind definitions
  - [x] Include version identifier and stability guarantees
  - [x] Reference the code contracts as authoritative

## Dev Notes

### Story Context

This is the first story of Epic 2 (Trust Contract v1 Foundations). It establishes the core type system that all enforcement decisions depend on. These types are foundational—every subsequent story in Epics 2-6 depends on this taxonomy being stable and complete.

**Why This Matters:**
- Enforcement (Epic 3) evaluates invariants against context scope and execution kind
- Propagation (Epic 4) carries this context across async boundaries
- Contract tests (Epic 5) assert behavior based on these types
- Extensions (Epic 6) must preserve these semantics

### Key Requirements from Epics

**From Epic 2 Introduction:**
> Deliver a complete, readable trust contract that defines scopes, invariants, attribution rules, disclosure policy, and refusal mappings that adopters can rely on immediately.

**From Story 2.1 Acceptance Criteria:**
- Explicit definitions for tenant scope, shared-system scope, and no-tenant context
- No-tenant context requires a reason/category from an allowed set
- Execution kind is represented explicitly in the context
- Available to enforcement, logging, and refusal mapping
- Gaps must be documented as blocking issues

**FR Coverage:**
- **FR2**: System represents tenant scope, shared-system scope, and no-tenant context
- **FR2a**: Shared-system scope is distinct (not wildcard) with its own operations
- **FR2b**: No-tenant context is explicit state with reason/category
- **FR3**: Developers can attach tenant context explicitly before execution

### Learnings from Epic 1 (Bootstrap)

**From Story 1.1-1.3:**
- Use `global.json` for SDK version pinning (10.0.102)
- Projects at solution root (no src/ or tests/ folders)
- All projects target net10.0 via Directory.Build.props
- Tests use xUnit + FluentAssertions
- Documentation tests verify file content structure
- YAML parsing with YamlDotNet for configuration files

**Established Patterns:**
1. **File naming**: PascalCase for types, lowercase-kebab for markdown
2. **Test location**: All tests in TenantSaas.ContractTests root
3. **Documentation**: XML comments on all public APIs
4. **Immutability**: Value types should be immutable

**Recent Git Activity:**
- `66072b4` - CI workflow implementation
- `8c95786` - Health check and local setup
- `cb15fc9` - Initial project from templates

### Technical Requirements

**Target Framework:**
- net10.0 (all projects via Directory.Build.props)

**New Project: TenantSaas.Abstractions**

This project defines contracts and shared types only—no implementations. It should have zero dependencies beyond the framework.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

**No Additional NuGet Dependencies:**
- Types are pure C# contracts
- No serialization attributes (adopters choose their serializer)
- No framework dependencies

### Architecture Compliance

**From Architecture § Project Structure:**
```
├── TenantSaas.Abstractions/
│   ├── TenantSaas.Abstractions.csproj
│   ├── Tenancy/
│   │   ├── TenantContext.cs
│   │   ├── TenantScope.cs
│   │   └── ITenantContextAccessor.cs
```

**From Architecture § Component Boundaries:**
> `TenantSaas.Abstractions` defines contracts and shared types only.

**From Architecture § Core Decisions:**
> Refuse-by-default enforcement with strictly constrained break-glass path.

This means our types must support:
- Clear distinction between tenant/shared/no-tenant
- Explicit reason tracking for no-tenant operations
- Execution kind for enforcement policy selection

### Type Design Patterns

**TenantScope as Discriminated Union:**

Use a sealed class hierarchy with static factory methods for type-safe scope representation:

```csharp
public abstract class TenantScope : IEquatable<TenantScope>
{
    private TenantScope() { }
    
    public sealed class Tenant : TenantScope
    {
        public TenantId Id { get; }
        internal Tenant(TenantId id) => Id = id;
    }
    
    public sealed class SharedSystem : TenantScope
    {
        internal SharedSystem() { }
    }
    
    public sealed class NoTenant : TenantScope
    {
        public NoTenantReason Reason { get; }
        internal NoTenant(NoTenantReason reason) => Reason = reason;
    }
    
    public static TenantScope ForTenant(TenantId id) => new Tenant(id);
    public static TenantScope ForSharedSystem() => new SharedSystem();
    public static TenantScope ForNoTenant(NoTenantReason reason) => new NoTenant(reason);
}
```

**TenantId Value Object:**

Create a strong type for tenant identifiers to prevent stringly-typed mistakes:

```csharp
public readonly record struct TenantId
{
    public string Value { get; }
    
    public TenantId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }
    
    public override string ToString() => Value;
}
```

**ExecutionKind as Enum with Metadata:**

```csharp
public enum ExecutionKind
{
    /// <summary>HTTP/API request flow.</summary>
    Request = 1,
    
    /// <summary>Background job or worker flow.</summary>
    Background = 2,
    
    /// <summary>Administrative operation flow.</summary>
    Admin = 3,
    
    /// <summary>CLI or script execution flow.</summary>
    Scripted = 4
}
```

**NoTenantReason as Enum:**

```csharp
public enum NoTenantReason
{
    /// <summary>Public/unauthenticated operations.</summary>
    Public = 1,
    
    /// <summary>System initialization/bootstrap.</summary>
    Bootstrap = 2,
    
    /// <summary>Health/readiness probes.</summary>
    HealthCheck = 3,
    
    /// <summary>Maintenance operations.</summary>
    SystemMaintenance = 4
}
```

### Project Structure Notes

**New Files:**
```
TenantSaas.Abstractions/
├── TenantSaas.Abstractions.csproj
├── Tenancy/
│   ├── TenantId.cs
│   ├── TenantScope.cs
│   ├── NoTenantReason.cs
│   ├── TenantContext.cs
│   └── ITenantContextAccessor.cs
├── Contexts/
│   └── ExecutionKind.cs
└── TrustContract/
    └── TrustContractV1.cs

TenantSaas.ContractTests/
└── ContextTaxonomyTests.cs (new)

docs/
└── trust-contract.md (new)
```

**Modified Files:**
- `TenantSaas.sln` - Add TenantSaas.Abstractions project reference
- `TenantSaas.ContractTests.csproj` - Add reference to Abstractions project
- `TenantSaas.Core.csproj` - Add reference to Abstractions project

### Testing Requirements

**Test Categories:**
1. Type completeness tests (all variants defined)
2. Construction validation tests (required fields)
3. Equality and immutability tests
4. Trust contract validation tests
5. Documentation presence tests

**Contract Tests (ContextTaxonomyTests.cs):**

```csharp
public class ContextTaxonomyTests
{
    [Fact]
    public void TenantScope_Should_Have_Three_Variants()
    {
        // Verify Tenant, SharedSystem, NoTenant exist
    }
    
    [Fact]
    public void NoTenantReason_Should_Include_Required_Categories()
    {
        // Verify Public, Bootstrap, HealthCheck, SystemMaintenance exist
    }
    
    [Fact]
    public void ExecutionKind_Should_Include_All_Flow_Types()
    {
        // Verify Request, Background, Admin, Scripted exist
    }
    
    [Fact]
    public void TenantContext_Should_Require_Scope_And_ExecutionKind()
    {
        // Verify construction fails without required fields
    }
    
    [Fact]
    public void TenantContext_ForRequest_Should_Include_RequestId()
    {
        // Verify Request execution kind includes request correlation
    }
    
    [Fact]
    public void TrustContractV1_Validation_Should_Detect_Missing_Definitions()
    {
        // Verify validation catches incomplete contracts
    }
}
```

**Testing Best Practices:**
- Use FluentAssertions for readable assertions
- Test both valid and invalid construction scenarios
- Test equality semantics (important for dictionary keys)
- Test serialization round-trip if applicable
- Document test intent in test names

### File Structure Requirements

**Project Layout After This Story:**
```
TenantSaas/
├── TenantSaas.sln (update)
├── Directory.Build.props (existing)
├── global.json (existing)
├── docs/
│   └── trust-contract.md (new)
├── TenantSaas.Abstractions/ (new)
│   ├── TenantSaas.Abstractions.csproj
│   ├── Tenancy/
│   │   ├── TenantId.cs
│   │   ├── TenantScope.cs
│   │   ├── NoTenantReason.cs
│   │   ├── TenantContext.cs
│   │   └── ITenantContextAccessor.cs
│   ├── Contexts/
│   │   └── ExecutionKind.cs
│   └── TrustContract/
│       └── TrustContractV1.cs
├── TenantSaas.Core/ (existing)
├── TenantSaas.EfCore/ (existing)
├── TenantSaas.Sample/ (existing)
└── TenantSaas.ContractTests/
    ├── ContextTaxonomyTests.cs (new)
    └── ... (existing test files)
```

### Critical Don't-Miss Rules

From [project-context.md](../_bmad-output/project-context.md):

**Must Follow:**
- Use standard .NET naming (PascalCase types/methods, camelCase locals/fields)
- No underscore prefixes anywhere
- Use nullable reference types and explicit nullability
- Use exceptions for truly exceptional control flow; prefer explicit result types for invariant violations
- Use primary constructors where possible

**Must NOT:**
- Never introduce new dependencies without explicit justification
- Never use implicit or ambiguous identifiers
- Never introduce breaking API changes without explicit versioning

### References

- [Architecture](../planning-artifacts/architecture.md) - § Project Structure, § Component Boundaries
- [PRD](../planning-artifacts/prd.md) - FR2, FR2a, FR2b, FR3
- [Epics](../planning-artifacts/epics.md) - Epic 2 Story 2.1
- [Project Context](../project-context.md) - Implementation rules

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- dotnet test TenantSaas.sln
### Completion Notes List

- ✅ Created `TenantSaas.Abstractions` project with contract-only types and XML documentation output.
- ✅ Implemented tenant scope taxonomy, no-tenant reasons, execution kinds, and tenant context factories with validation.
- ✅ Added trust contract constants and validation for missing scope/execution kind definitions.
- ✅ Added contract tests covering taxonomy completeness and validation gaps.
- ✅ Authored `docs/trust-contract.md` with scope, reason, and execution kind definitions.
### File List

- TenantSaas.Abstractions/TenantSaas.Abstractions.csproj
- TenantSaas.Abstractions/Invariants/.gitkeep
- TenantSaas.Abstractions/Tenancy/TenantId.cs
- TenantSaas.Abstractions/Tenancy/TenantScope.cs
- TenantSaas.Abstractions/Tenancy/NoTenantReason.cs
- TenantSaas.Abstractions/Contexts/ExecutionKind.cs
- TenantSaas.Abstractions/Tenancy/TenantContext.cs
- TenantSaas.Abstractions/Tenancy/ITenantContextAccessor.cs
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs
- TenantSaas.sln
- TenantSaas.Core/TenantSaas.Core.csproj
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj
- TenantSaas.ContractTests/ContextTaxonomyTests.cs
- docs/trust-contract.md
- _bmad-output/implementation-artifacts/2-1-define-context-taxonomy-and-execution-kinds.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-02-01: Added Abstractions contracts, trust contract validation, context taxonomy tests, and trust contract documentation.
