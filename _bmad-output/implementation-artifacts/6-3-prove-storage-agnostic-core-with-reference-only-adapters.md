# Story 6.3: Prove Storage-Agnostic Core with Reference-Only Adapters

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a technical lead,
I want confidence that the core stays storage-agnostic,
so that the baseline remains portable across data layers.

## Acceptance Criteria

1. **Given** the core packages
   **When** dependencies are reviewed
   **Then** storage-specific dependencies are not required by the core
   **And** reference adapters are isolated as optional components
   **And** this is verified by a test
2. **Given** the reference adapter is used
   **When** it integrates with the core
   **Then** it uses sanctioned seams and boundaries
   **And** the core invariants remain enforced
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Implement the EF Core reference adapter in `TenantSaas.EfCore`
  - [x] Add adapter implementation that uses sanctioned seams (no bypasses)
  - [x] Keep EF Core references isolated to `TenantSaas.EfCore`
- [x] Add tests that prove the core is storage-agnostic
  - [x] Assert `TenantSaas.Core` and `TenantSaas.Abstractions` have no EF Core package references
  - [x] Add a contract test proving the EF Core adapter enforces invariants via the same guard/boundary
- [x] Update docs (if needed) to explain reference adapter role and optionality

## Developer Context

- `TenantSaas.EfCore` is present as a shell; this story makes it real without leaking EF Core into core packages.
- Storage agnostic means core and abstractions must not reference EF Core assemblies.
- Adapter must respect boundary guard and context rules already defined.

## Technical Requirements

- EF Core adapter must:
  - Live entirely under `TenantSaas.EfCore`
  - Use `TenantSaas.Abstractions` and `TenantSaas.Core` contracts
  - Enforce invariants via existing guard/tenancy APIs
- Core packages (`TenantSaas.Core`, `TenantSaas.Abstractions`) must not reference EF Core assemblies.
- Add tests that fail if EF Core references appear in core packages.

## Architecture Compliance

- Preserve the storage-agnostic core architecture.
- Keep adapter as optional reference implementation.
- Preserve refuse-by-default enforcement, Problem Details, and logging requirements.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- Use EF Core 10.0.0 for the reference adapter (no upgrades in this story).

## File Structure Requirements

- Implementation:
  - `TenantSaas.EfCore/` (new adapter classes)
- Tests:
  - `TenantSaas.ContractTests/` (add tests for package references and adapter enforcement)

## Testing Requirements

- Add tests that assert no EF Core references in `TenantSaas.Core` and `TenantSaas.Abstractions`.
- Add tests that exercise EF Core adapter and verify invariants are enforced through the guard.
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.2 locked the boundary-only integration guide; the adapter must align with those boundaries.
- Reuse extension seams defined in Story 6.1; do not create new seams.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.
- EF Core latest patch is 10.0.3 (2026-02-10); keep repo baseline `10.0.x` unless compatibility forces an update.

## Project Context Reference

- Never access persistence outside repositories.
- Never bypass tenant context or invariant guards.
- Never introduce new dependencies without explicit justification.

## Story Completion Status

- Status set to **ready-for-dev**
- Completion note: Adapter implementation and storage-agnostic test plan prepared; ready for implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy + EF Core baseline
- red phase: added `EfCoreReferenceAdapterContractTests` and confirmed initial compile failure (missing `TenantSaas.EfCore` adapter API)
- green phase: implemented `TenantBoundaryDbContextExecutor` + `TenantBoundaryViolationException` in `TenantSaas.EfCore`
- verification: `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal` (pass)
- verification: `dotnet test TenantSaas.sln --disable-build-servers -v minimal` (pass)

### Completion Notes List

- Extracted Epic 6 story 6.3 requirements and aligned adapter scope to `TenantSaas.EfCore`.
- Identified tests to prevent EF Core dependency leakage into core packages.
- Implemented an optional EF Core reference adapter that enforces `IBoundaryGuard.RequireContext` before executing EF operations.
- Added deterministic contract tests proving both storage-agnostic package boundaries and guard-based adapter enforcement.
- Updated README to document `TenantSaas.EfCore` as an optional reference adapter instead of a planned placeholder.

### File List

- README.md
- TenantSaas.ContractTests/EfCoreReferenceAdapterContractTests.cs
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj
- TenantSaas.EfCore/TenantBoundaryDbContextExecutor.cs
- TenantSaas.EfCore/TenantBoundaryViolationException.cs
- TenantSaas.EfCore/TenantSaas.EfCore.csproj
- docs/extension-seams.md
- docs/integration-guide.md
- _bmad-output/implementation-artifacts/6-3-prove-storage-agnostic-core-with-reference-only-adapters.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.
- 2026-02-15: Implemented EF Core reference adapter, added storage-agnostic and adapter-boundary contract tests, and updated docs; status moved to review.
- 2026-02-15: Code review fixes — added failure guard to FromEnforcementResult, added null-argument tests for ExecuteAsync, added storage adapter section to extension-seams.md and integration-guide.md, added XML doc comments on exception properties; status moved to done.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.3: Prove Storage-Agnostic Core with Reference-Only Adapters]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data Architecture]
- [Source: _bmad-output/planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [External: Microsoft.EntityFrameworkCore package](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)
