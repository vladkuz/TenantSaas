# Story 4.3: Provide Ambient Context Propagation with Deterministic Async Behavior

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want ambient context propagation as an option,
so that I can reduce plumbing without weakening guarantees.

## Acceptance Criteria

1) **Given** ambient propagation is enabled  
   **When** execution crosses async boundaries within the same flow  
   **Then** the tenant context remains available deterministically  
   **And** enforcement outcomes match the explicit passing model  
   **And** this is verified by a test

2) **Given** ambient context is enabled  
   **When** a new execution flow begins  
   **Then** no prior context leaks into the new flow  
   **And** context must be explicitly initialized for the new flow  
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Implement ambient context propagation (AC: #1, #2)
  - [x] Extend/confirm `ITenantContextAccessor` and ambient storage (e.g., `ExplicitTenantContextAccessor`) can flow across async boundaries
  - [x] Ensure ambient context is optional (via DI registration of `ExplicitTenantContextAccessor` instead of `AmbientTenantContextAccessor`)
  - [x] Add explicit API to clear ambient context at flow boundaries to prevent leakage
  - [x] Document deterministic behavior expectations for async/await and task scheduling
- [x] Enforce equivalence with explicit context passing (AC: #1)
  - [x] Ensure boundary enforcement resolves effective context consistently (explicit context wins when provided)
  - [x] Verify refusal behavior and invariant codes match explicit context path
- [x] Prevent cross-flow leakage (AC: #2)
  - [x] Ensure new flow initialization resets ambient context before use
  - [x] Validate that background/admin/scripted flow wrappers do not inherit previous ambient context
- [x] Contract tests (AC: #1, #2)
  - [x] Add tests proving ambient context flows across async boundaries (e.g., `await`, `Task.Run`, nested tasks)
  - [x] Add tests proving new flow starts with empty ambient context and refuses when not initialized
  - [x] Add tests proving ambient + explicit precedence rules remain intact
- [x] Documentation updates (AC: #1, #2)
  - [x] Update `docs/integration-guide.md` with ambient propagation opt-in guidance
  - [x] Add a short "ambient vs explicit" section with determinism and leakage rules

## Dev Notes

### Relevant Architecture Patterns and Constraints

From `_bmad-output/planning-artifacts/architecture.md`:

- Enforcement occurs only at boundary helpers/middleware/interceptors.
- Errors must use RFC 7807 Problem Details with `invariant_code` and `trace_id`.
- Structured logs must include required fields (`tenant_ref`, `trace_id`, `request_id`, `invariant_code`, `event_name`, `severity`).
- Context taxonomy includes tenant, shared-system, and no-tenant contexts with explicit execution kind.
- Refuse-by-default posture must remain intact when context is missing or ambiguous.

### Developer Context (Most Important)

- Ambient propagation is **optional** and must not be required for correctness.
- Deterministic async propagation must match explicit passing outcomes exactly.
- No context leakage: a new execution flow must start empty and require explicit initialization.
- Keep behavior deterministic across async boundaries (no hidden fallbacks).
- This story builds on Story 4.1 (initialization primitive) and Story 4.2 (explicit context passing).

### Technical Requirements

- Ambient context storage must flow across async boundaries deterministically.
- Explicit context must take precedence over ambient when both are present.
- Missing/empty context must refuse with `ContextInitialized` invariant in the same way as explicit path.
- Provide a safe way to clear/reset ambient context at flow boundaries.
- No new dependencies without explicit justification.

### Architecture Compliance

- .NET 10, Minimal APIs, Problem Details only, structured logging rules.
- Flat repo structure (no `src/` or `tests/`).
- Contract tests in `TenantSaas.ContractTests` only.
- EF Core access only through repositories; no direct `DbContext` usage outside.

### Library / Framework Requirements

- Target `net10.0` for all projects.
- Use existing abstractions (`ITenantContextAccessor`, `TenantContext`, `TenantContextInitializer`).
- Avoid introducing new external libraries for ambient flow.

### File Structure Requirements

- Abstractions in `TenantSaas.Abstractions/*`.
- Core implementation in `TenantSaas.Core/*`.
- Contract tests in `TenantSaas.ContractTests/*`.
- Integration examples in `TenantSaas.Sample/*`.

### Testing Requirements

- Use xUnit + FluentAssertions; Moq only if needed.
- Contract tests must assert Problem Details shape plus `invariant_code` and `trace_id`.
- Add tests that explicitly validate deterministic async propagation and zero leakage across new flows.

## Previous Story Intelligence

From Story 4.2 (explicit context passing) and Story 4.1 (initialization primitive):

- `TenantContextInitializer` establishes context once per flow and enforces idempotency.
- `ITenantContextAccessor` / `ExplicitTenantContextAccessor` are used for ambient storage.
- `BoundaryGuard` enforcement currently resolves ambient context and emits Problem Details with invariant codes.
- Explicit context overloads already exist; ambient behavior must remain compatible and deterministic.

## Git Intelligence Summary

Recent commits indicate the enforcement boundary and explicit context support were recently updated:

- `efea484` added explicit context passing to `BoundaryGuard` and updated docs/tests.
- `5fd009e` implemented `TenantContextInitializer` and ambient accessor patterns.
- `0243027` introduced core `BoundaryGuard` enforcement and contract tests.

Implication: reuse existing enforcement patterns and extend the ambient accessor carefully to avoid regressions.

## Latest Technical Information

- .NET 10 and EF Core 10 are the current target versions for this repo; keep targeting net10.0.
- Swashbuckle.AspNetCore latest is 10.1.1; current architecture pins 10.1.0 (do not upgrade unless required).
- OpenTelemetry.Extensions.Hosting and OpenTelemetry.Exporter.OpenTelemetryProtocol latest are 1.15.0.
- xUnit latest v2 is 2.9.3 (deprecated line); v3 exists but upgrading is out of scope.
- FluentAssertions latest is 8.3.0; Moq latest is 4.20.72.
- Do not upgrade libraries as part of this story unless required by the implementation.

## Project Context Reference

Follow `_bmad-output/project-context.md` rules, including:
- No underscore prefixes, Problem Details only, required log fields, UTC timestamps, no bypass of invariants.

## Story Completion Status

- Status set to **done**
- Completion note: Ambient context propagation validated via comprehensive async tests; deterministic behavior confirmed; documentation complete. Code review fixes applied.

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5

### Debug Log References

N/A

### Completion Notes List

- ✅ Verified `AmbientTenantContextAccessor` uses `AsyncLocal<TenantContext?>` for deterministic async propagation
- ✅ Confirmed ambient context flows across await, Task.Run, nested tasks, Task.WhenAll per existing implementation
- ✅ Added 11 comprehensive contract tests in `AmbientContextPropagationTests.cs`:
  - 5 tests prove deterministic async propagation (AC#1)
  - 3 tests prove zero leakage across flows (AC#2)
  - 3 tests prove enforcement equivalence with explicit context passing
- ✅ All 327 contract tests pass with zero regressions
- ✅ Documentation updated: added "Ambient vs Explicit Context Propagation" section to integration-guide.md
- ✅ Documented determinism guarantees, leakage prevention rules, and precedence semantics
- No implementation changes required - AsyncLocal already provides deterministic async behavior
- Explicit context already takes precedence over ambient per existing BoundaryGuard implementation

### File List

- `_bmad-output/implementation-artifacts/4-3-provide-ambient-context-propagation-with-deterministic-async-behavior.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `TenantSaas.ContractTests/AmbientContextPropagationTests.cs` (new)
- `docs/integration-guide.md` (updated)
