# Story 4.4: Provide Flow Wrappers for Background, Admin, and Scripted Execution

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want clear wrappers for non-request execution kinds,
so that tenancy is established consistently outside HTTP.

## Acceptance Criteria

1) **Given** a background, admin, or scripted operation  
   **When** I use the provided flow wrapper  
   **Then** the wrapper requires explicit initialization inputs  
   **And** it sets execution kind and scope in the context  
   **And** this is verified by a test

2) **Given** a flow wrapper is bypassed  
   **When** enforcement is evaluated downstream  
   **Then** missing context is refused by default  
   **And** the refusal references the ContextInitialized invariant  
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Implement flow wrapper abstractions (AC: #1)
  - [x] Create `ITenantFlowScope` interface as the result of starting a flow
  - [x] Create `ITenantFlowFactory` interface for creating flow scopes
  - [x] Define wrapper result contract that provides context and disposal semantics
- [x] Implement concrete flow wrappers (AC: #1)
  - [x] Implement `TenantFlowFactory` that creates flow scopes using `ITenantContextInitializer`
  - [x] Implement `BackgroundFlow`, `AdminFlow`, `ScriptedFlow` scope types
  - [x] Ensure wrappers require explicit inputs (scope, traceId, attributionInputs)
  - [x] Ensure automatic cleanup via `IDisposable`/`IAsyncDisposable` pattern
- [x] Prevent context leakage across flows (AC: #2)
  - [x] Ensure flow wrapper clears ambient context on disposal
  - [x] Validate that new flow starts with clean context state
  - [x] Ensure parallel flows do not interfere with each other (AsyncLocal isolation)
- [x] Contract tests (AC: #1, #2)
  - [x] Add tests proving flow wrappers set correct execution kind and scope
  - [x] Add tests proving bypassed wrappers result in enforcement refusal with `ContextInitialized`
  - [x] Add tests proving flow disposal clears ambient context
  - [x] Add tests proving parallel flows remain isolated
- [x] Documentation updates (AC: #1, #2)
  - [x] Update `docs/integration-guide.md` with flow wrapper usage examples
  - [x] Document background job, admin task, and scripted CLI patterns

## Dev Notes

### Relevant Architecture Patterns and Constraints

From `_bmad-output/planning-artifacts/architecture.md`:

- Enforcement occurs only at boundary helpers/middleware/interceptors.
- Errors must use RFC 7807 Problem Details with `invariant_code` and `trace_id`.
- Structured logs must include required fields (`tenant_ref`, `trace_id`, `request_id`, `invariant_code`, `event_name`, `severity`).
- Context taxonomy includes tenant, shared-system, and no-tenant contexts with explicit execution kind.
- Refuse-by-default posture must remain intact when context is missing or ambiguous.
- Abstractions go in `TenantSaas.Abstractions/*`; implementations in `TenantSaas.Core/*`.

### Developer Context (Most Important)

- **Current state**: `TenantContextInitializer` provides raw `InitializeBackground`, `InitializeAdmin`, `InitializeScripted` methods but no structured wrapper pattern. Users must manually call `Clear()` in finally blocks.
- **Goal**: Provide ergonomic flow wrappers that handle the try/finally pattern automatically via `IDisposable`/`IAsyncDisposable`.
- **Key insight**: Flow wrappers should use the existing `TenantContextInitializer` internally, not duplicate its logic.
- **Existing infrastructure**:
  - `ITenantContextInitializer` with `InitializeBackground`, `InitializeAdmin`, `InitializeScripted`, and `Clear()` methods
  - `IMutableTenantContextAccessor` for setting/clearing ambient context
  - `AmbientTenantContextAccessor` uses `AsyncLocal<TenantContext?>` for flow isolation
  - `ExecutionKind` sealed record with `Background`, `Admin`, `Scripted` static properties
  - `TenantContext.ForBackground()`, `TenantContext.ForAdmin()`, `TenantContext.ForScripted()` factory methods
- **Pattern to follow**: Similar to `IServiceScope` / `using var scope = ...` pattern in .NET DI.
- **Enforcement validation**: `BoundaryGuard.EnforceContextInitialized()` already refuses with `InvariantCode.ContextInitialized` when context is missing.

### Technical Requirements

- Flow wrappers must be `IDisposable` and optionally `IAsyncDisposable` for proper cleanup.
- Wrappers must require explicit inputs: `TenantScope`, `traceId`, and optionally `TenantAttributionInputs`.
- Wrappers must automatically call `ITenantContextInitializer.Clear()` on disposal.
- Flow wrappers must not introduce new dependencies beyond existing abstractions.
- Wrappers must work with both ambient (`AmbientTenantContextAccessor`) and explicit (`ExplicitTenantContextAccessor`) modes.
- Disposal must be idempotent (safe to call multiple times).

### Architecture Compliance

- .NET 10, Minimal APIs, Problem Details only, structured logging rules.
- Flat repo structure (no `src/` or `tests/` folders).
- Contract tests in `TenantSaas.ContractTests` only.
- Use existing patterns from `TenantContextInitializer` implementation.

### Library / Framework Requirements

- Target `net10.0` for all projects.
- Use existing abstractions (`ITenantContextInitializer`, `TenantContext`, `ExecutionKind`, `TenantScope`).
- Avoid introducing new external libraries.
- Follow existing naming: `TenantSaas.Abstractions.Tenancy` for interfaces, `TenantSaas.Core.Tenancy` for implementations.

### File Structure Requirements

- **Abstractions**:
  - `TenantSaas.Abstractions/Tenancy/ITenantFlowScope.cs` - flow scope interface
  - `TenantSaas.Abstractions/Tenancy/ITenantFlowFactory.cs` - factory interface
- **Core implementation**:
  - `TenantSaas.Core/Tenancy/TenantFlowFactory.cs` - factory implementation
  - `TenantSaas.Core/Tenancy/TenantFlowScope.cs` - flow scope implementation
- **Contract tests**:
  - `TenantSaas.ContractTests/FlowWrapperTests.cs` - new test file
- **Documentation**:
  - `docs/integration-guide.md` - update with flow wrapper patterns

### Testing Requirements

- Use xUnit + FluentAssertions; Moq only if needed.
- Contract tests must assert Problem Details shape plus `invariant_code` and `trace_id`.
- Test scenarios:
  1. `BackgroundFlow` wrapper sets `ExecutionKind.Background` and provides correct scope
  2. `AdminFlow` wrapper sets `ExecutionKind.Admin` and provides correct scope
  3. `ScriptedFlow` wrapper sets `ExecutionKind.Scripted` and provides correct scope
  4. Wrapper disposal clears ambient context
  5. Missing wrapper results in `ContextInitialized` refusal from `BoundaryGuard`
  6. Parallel flows remain isolated (no cross-contamination)
  7. Double-disposal is safe (idempotent)

### Suggested API Design

```csharp
// Interface for flow scope result
public interface ITenantFlowScope : IDisposable, IAsyncDisposable
{
    TenantContext Context { get; }
}

// Factory interface
public interface ITenantFlowFactory
{
    ITenantFlowScope CreateBackgroundFlow(TenantScope scope, string traceId, TenantAttributionInputs? attributionInputs = null);
    ITenantFlowScope CreateAdminFlow(TenantScope scope, string traceId, TenantAttributionInputs? attributionInputs = null);
    ITenantFlowScope CreateScriptedFlow(TenantScope scope, string traceId, TenantAttributionInputs? attributionInputs = null);
}

// Usage pattern
using var flow = flowFactory.CreateBackgroundFlow(scope, traceId);
// flow.Context is available
// await DoWork();
// Disposal automatically clears context
```

## Previous Story Intelligence

From Story 4.3 (ambient context propagation):

- `AmbientTenantContextAccessor` uses `AsyncLocal<TenantContext?>` which provides automatic flow isolation across async boundaries.
- `TenantContextInitializer.Clear()` sets the accessor's context to `null`.
- Enforcement via `BoundaryGuard.EnforceContextInitialized()` refuses with `InvariantCode.ContextInitialized` when no context is set.
- Existing tests in `AmbientContextPropagationTests.cs` prove async propagation and flow isolation.
- `TenantContextInitializer` is idempotent - repeated initialization with identical inputs is safe.

From Story 4.2 (explicit context passing):

- `BoundaryGuard` supports hybrid mode: `EnforceContextInitialized(explicitContext)` allows explicit context to override ambient.
- Flow wrappers should work with both ambient and explicit patterns.

From Story 4.1 (initialization primitive):

- `TenantContextInitializer` provides `InitializeBackground`, `InitializeAdmin`, `InitializeScripted` methods.
- Each returns a `TenantContext` with the appropriate `ExecutionKind`.
- `Clear()` method resets the ambient context.

## Git Intelligence Summary

Recent commits indicate the context initialization and ambient propagation were recently completed:

- `35dbebb` added ambient context propagation tests and documentation
- `efea484` added explicit context passing support to `BoundaryGuard`
- `5fd009e` implemented `TenantContextInitializer` with all four flow types

Implication: Build on the existing `TenantContextInitializer` - wrap its methods, don't replace them.

## Latest Technical Information

- .NET 10 SDK 10.0.102 is installed and all projects target `net10.0`.
- All 327 existing contract tests pass.
- `AsyncLocal<T>` in .NET 10 provides deterministic async propagation without changes.
- No library upgrades needed for this story.

## Project Context Reference

Follow `_bmad-output/project-context.md` rules, including:
- No underscore prefixes, Problem Details only, required log fields, UTC timestamps, no bypass of invariants.
- Use nullable reference types and explicit nullability where applicable.
- Prefer `async`/`await` and `Task`-returning APIs; avoid `.Result`/`.Wait()`.
- Always pass `CancellationToken` in async methods.
- Use primary constructors where possible.

## Story Completion Status

- Status set to **review**
- Completion note: All acceptance criteria met, flow wrappers implemented and tested

## Dev Agent Record

### Agent Model Used

Claude Opus 4.5

### Debug Log References

N/A

### Completion Notes List

- ✅ Created `ITenantFlowScope` interface in `TenantSaas.Abstractions/Tenancy/ITenantFlowScope.cs`
- ✅ Created `ITenantFlowFactory` interface in `TenantSaas.Abstractions/Tenancy/ITenantFlowFactory.cs`
- ✅ Implemented `TenantFlowScope` class with `IDisposable`/`IAsyncDisposable` pattern
- ✅ Implemented `TenantFlowFactory` that uses `ITenantContextInitializer` internally
- ✅ Flow wrappers clear ambient context before initialization (prevents cross-flow leakage)
- ✅ Flow wrappers clear context automatically on disposal (idempotent)
- ✅ All 12 new contract tests in `FlowWrapperTests.cs` verify:
  - BackgroundFlow sets ExecutionKind.Background
  - AdminFlow sets ExecutionKind.Admin  
  - ScriptedFlow sets ExecutionKind.Scripted
  - Attribution inputs are properly propagated
  - Bypassed wrappers result in ContextInitialized refusal
  - Disposal clears ambient context (sync and async)
  - Double disposal is idempotent
  - Parallel flows remain isolated via AsyncLocal
  - SharedSystem and NoTenant scopes work correctly
- ✅ All 339 contract tests pass with zero regressions
- ✅ Documentation updated: added "Flow Wrappers for Non-Request Execution" section to integration-guide.md
- ✅ Review fixes applied: flow disposal only clears when current context matches, parallel tests deterministic, DI guidance clarified, CLI example passes CancellationToken, XML docs corrected
- ✅ Tests passed (user confirmed)

### File List

- `TenantSaas.Abstractions/Tenancy/ITenantFlowScope.cs` (new)
- `TenantSaas.Abstractions/Tenancy/ITenantFlowFactory.cs` (new)
- `TenantSaas.Core/Tenancy/TenantFlowScope.cs` (new)
- `TenantSaas.Core/Tenancy/TenantFlowFactory.cs` (new)
- `TenantSaas.ContractTests/FlowWrapperTests.cs` (new)
- `docs/integration-guide.md` (updated)
- `.codex/history.jsonl` (updated)
- `.codex/models_cache.json` (updated)
- `_bmad-output/implementation-artifacts/4-4-provide-flow-wrappers-for-background-admin-and-scripted-execution.md` (updated)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (updated)
