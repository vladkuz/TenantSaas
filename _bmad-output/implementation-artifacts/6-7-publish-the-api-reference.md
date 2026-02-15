# Story 6.7: Publish the API Reference

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a complete API reference,
so that I can integrate with confidence and avoid hidden surface area.

## Acceptance Criteria

1. **Given** the API reference
   **When** public surface area changes
   **Then** the reference updates in the same change
   **And** contract identifiers remain stable within a major version
   **And** this is verified by a test
2. **Given** a public type or endpoint is undocumented
   **When** the API reference is reviewed
   **Then** the gap is flagged before release
   **And** the missing entry is added
   **And** this is verified by a test
3. **Given** the API reference is finalized
   **When** documentation is published
   **Then** the reference is available at `docs/api-reference.md` and referenced by a documentation test

## Tasks / Subtasks

- [x] Create `docs/api-reference.md`
  - [x] Enumerate all public types and entry points across `TenantSaas.Abstractions`, `TenantSaas.Core`, and `TenantSaas.Sample` API surface
  - [x] Include extension seams (from Story 6.1) and trust contract identifiers
  - [x] Provide a stable structure for future updates
- [x] Add documentation tests
  - [x] Verify `docs/api-reference.md` exists
  - [x] Assert key public namespaces/types are documented
  - [x] Fail if any known public surface area is missing

## Developer Context

- This is a documentation + enforcement story.
- The API reference must reflect actual public surface area and be kept in sync with code changes.
- Prefer a clear, enumerated list over narrative prose.

## Technical Requirements

- API reference must include (at minimum):
  - `TenantSaas.Abstractions` public interfaces and types (tenancy, invariants, disclosure, logging, break-glass)
  - `TenantSaas.Core` public entry points (guard, context initialization, flow factory, enforcement)
  - `TenantSaas.Sample` public HTTP endpoints (if any are intended as reference)
- Include references to trust contract identifiers and invariant codes where applicable.

## Architecture Compliance

- Preserve stable identifiers and do not rename public contracts in documentation.
- Align with the flat repo structure and project boundaries.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- No new documentation tooling or dependencies.

## File Structure Requirements

- Documentation:
  - `docs/api-reference.md` (new)
- Tests:
  - `TenantSaas.ContractTests/` (documentation validation)

## Testing Requirements

- Add doc tests that assert:
  - API reference exists at `docs/api-reference.md`
  - Key public namespaces/types are documented
  - Extension seams and trust contract identifiers are referenced
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.6 introduced verification-guide doc tests; reuse that test pattern for API reference enforcement.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.

## Project Context Reference

- Keep documentation crisp and aligned with the trust contract.
- Never introduce new dependencies without explicit justification.

## Story Completion Status

- Status set to **ready-for-dev**
- Completion note: API reference doc and validation plan prepared; ready for implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy baseline
- dev-story workflow executed in YOLO mode with explicit story path input
- red phase: `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal --filter ApiReferenceDocumentationTests` (failed as expected before doc creation)
- green/refactor validation: `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal --filter ApiReferenceDocumentationTests`
- regression validation: `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
- regression validation: `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

### Completion Notes List

- Extracted Epic 6 story 6.7 requirements and defined API reference scope and validation checks.
- Ensured alignment with extension seams and trust contract identifiers.
- Added `docs/api-reference.md` with stable sections for `TenantSaas.Abstractions`, `TenantSaas.Core`, and `TenantSaas.Sample`.
- Added strict documentation enforcement tests in `TenantSaas.ContractTests/ApiReferenceDocumentationTests.cs`.
- Verified API reference includes extension seams and trust contract identifiers required by ACs.
- Full contract-tests suite and solution tests passed after implementation.

### File List

- _bmad-output/implementation-artifacts/6-7-publish-the-api-reference.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- docs/api-reference.md
- TenantSaas.ContractTests/ApiReferenceDocumentationTests.cs

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.
- 2026-02-15: Implemented API reference documentation and enforcement tests; story moved to review.
- 2026-02-15: Code review fixed 4 issues (H1 missing test markers, M1 type categorization, M2 scope note, L1 substring ambiguity); all 437 tests pass; story moved to done.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.7: Publish the API Reference]
- [Source: _bmad-output/planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
