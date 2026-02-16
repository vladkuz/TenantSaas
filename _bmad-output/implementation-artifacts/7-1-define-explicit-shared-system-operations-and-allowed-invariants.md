# Story 7.1: Define Explicit Shared-System Operations and Allowed Invariants

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an architect,
I want shared-system scope to be explicit and bounded,
so that it cannot become a hidden wildcard.

## Acceptance Criteria

1. Given shared-system scope is used
   When its allowed operations are reviewed
   Then the allowed operations are explicit
   And they are mapped to specific invariants and refusal rules
   And this is verified by a test
2. Given a shared-system operation violates scope rules
   When enforcement is evaluated
   Then the operation is refused
   And the refusal is standardized and auditable
   And this is verified by a test
3. Given shared-system operations violate allowed invariants
   When enforcement runs
   Then the operation must refuse with an error

## Tasks / Subtasks

- [x] Define and codify explicit shared-system operation taxonomy (AC: 1)
  - [x] Add/extend contract types for allowed shared-system operations with explicit mapping to invariant codes
  - [x] Ensure shared-system scope remains distinct from tenant scope and never behaves as wildcard
- [x] Enforce refusal path for out-of-scope shared-system operations (AC: 2, 3)
  - [x] Update `BoundaryGuard`/enforcement pipeline to reject shared-system operations that violate mapped invariants
  - [x] Emit standardized Problem Details with stable `invariant_code` and correlation fields
- [x] Add compliance tests for allowed/forbidden shared-system operations (AC: 1, 2, 3)
  - [x] Add positive tests for explicitly allowed operations
  - [x] Add negative tests proving refusal + auditability for violations

## Developer Context

- This is a post-MVP hardening story extending existing trust-contract guardrails.
- Reuse existing abstractions/core enforcement modules; do not introduce parallel or bypass code paths.
- Keep refusal behavior machine-stable (`ProblemDetails` + `invariant_code` + trace correlation).

## Technical Requirements

- Maintain strict boundary-only enforcement model from previous epics.
- Preserve disclosure-safe `tenant_ref` semantics (`unknown`, `sensitive`, `cross_tenant`, opaque tenant id).
- Enforce deterministic outcomes across execution kinds and repeated runs.
- Do not relax break-glass requirements (explicit actor, reason, scope, auditable event).

## Architecture Compliance

- Keep core storage-agnostic; adapter behavior remains in sanctioned adapter seams only.
- Respect current module boundaries: `TenantSaas.Abstractions`, `TenantSaas.Core`, `TenantSaas.ContractTests`, `TenantSaas.Sample`.
- Keep API/error/logging conventions unchanged unless explicitly required by ACs.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- Continue xUnit-based contract tests; ensure compatibility with current repo tooling.
- No new dependency introduction unless technically required and justified in story implementation notes.

## File Structure Requirements

- Core contracts/types: `TenantSaas.Abstractions/*`
- Enforcement/runtime behavior: `TenantSaas.Core/*`
- Compliance tests: `TenantSaas.ContractTests/*` (flat test structure, no nested test folders)
- Documentation updates (if needed by AC traceability): `docs/trust-contract.md`, `docs/error-catalog.md`, or related docs

## Testing Requirements

- Add focused contract tests proving both positive and refusal scenarios per ACs.
- Assert full Problem Details shape + stable `invariant_code` + `trace_id` (+ `request_id` for request flows).
- Assert required structured log fields including disclosure-safe `tenant_ref` values.
- Validate with:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.7 reinforced documentation-as-contract and strict validation tests; apply the same “tests enforce docs/contract semantics” pattern here.
- Recent Epic 6 flow consistently used contract tests first, then docs alignment; preserve that sequencing.

## Git Intelligence Summary

- Recent commits show emphasis on docs + contract-test enforcement (`feat: Implement API reference documentation...`, `feat: Publish verification guide...`).
- Preserve established naming, module boundaries, and CI-first validation approach used in latest commits.

## Latest Technical Information

- .NET 10 is active LTS with latest patch `10.0.2` (patch date: 2026-01-13); keep `net10.0` pinned for this story set.
- Use currently adopted xUnit line in-repo; if upgrading, validate adapter/framework compatibility with .NET 10 before changing package versions.

## Project Context Reference

- Follow `_bmad-output/project-context.md` rules: no persistence outside repositories, no non-ProblemDetails errors, required structured logging fields, no ambiguous route params, no implicit fallback behavior.

## Story Completion Status

- Status set to **review**
- Completion note: Explicit shared-system operation taxonomy and invariant allowlist enforcement implemented with standardized refusals and compliance tests.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- git intelligence: recent commit analysis
- latest-technology check: .NET support policy and xUnit release notes
- dev-story workflow (YOLO)
- contract-first implementation: added failing tests, then implementation, then full regression validation

### Implementation Plan

- Added a stable invariant `SharedSystemOperationAllowed` and trust-contract refusal mapping.
- Added explicit shared-system operation taxonomy/allowlist in `TrustContractV1` with invariant mapping.
- Added `IBoundaryGuard.RequireSharedSystemOperation(...)` and `BoundaryGuard` enforcement with refusal logging for auditable denials.
- Added standardized Problem Details factory method for disallowed shared-system operations with stable extension fields.
- Updated trust-contract documentation and added focused compliance tests for allow/refuse behavior and auditability.

### Completion Notes List

- Extracted Epic 7 story requirements and acceptance criteria from epics.md.
- Added implementation guardrails aligned with trust contract and enforcement boundaries.
- Added concrete testing requirements and validation commands for contract-test regression safety.
- Implemented explicit shared-system operation allowlist with stable operation identifiers and invariant mapping.
- Enforced shared-system scope/non-wildcard behavior in `BoundaryGuard.RequireSharedSystemOperation(...)`.
- Added standardized refusal path (`SharedSystemOperationAllowed`) and `ProblemDetailsFactory.ForSharedSystemOperationNotAllowed(...)`.
- Added compliance tests for positive allowlist path and negative refusal/auditability paths.
- Validated with:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

### File List

- TenantSaas.Abstractions/Invariants/InvariantCode.cs
- TenantSaas.Abstractions/TrustContract/TrustContractV1.cs
- TenantSaas.Core/Enforcement/IBoundaryGuard.cs
- TenantSaas.Core/Enforcement/BoundaryGuard.cs
- TenantSaas.Core/Errors/ProblemDetailsFactory.cs
- TenantSaas.ContractTests/InvariantRegistryTests.cs
- TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs
- TenantSaas.ContractTests/SharedSystemOperationEnforcementTests.cs
- docs/trust-contract.md
- _bmad-output/implementation-artifacts/7-1-define-explicit-shared-system-operations-and-allowed-invariants.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-16: Implemented explicit shared-system operation taxonomy, enforcement, standardized refusals, documentation alignment, and compliance tests (Story 7.1).

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.1: Define Explicit Shared-System Operations and Allowed Invariants]
- [Source: _bmad-output/planning-artifacts/architecture.md#Implementation Patterns & Consistency Rules]
- [Source: _bmad-output/project-context.md#Critical Implementation Rules]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [External: xUnit releases](https://xunit.net/releases/v3/3.1.0)
