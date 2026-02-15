# Story 6.5: Publish the Trust Contract

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want an explicit trust contract document,
so that invariants, refusals, and disclosure rules are unambiguous.

## Acceptance Criteria

1. **Given** the documentation set
   **When** I review it as a new adopter
   **Then** I can find the trust contract document
   **And** it defines invariants, refusal behavior, and disclosure policy with stable identifiers
   **And** this is verified by a test
2. **Given** the trust contract is missing a required invariant or refusal mapping
   **When** documentation is reviewed
   **Then** the issue is flagged as a release blocker
   **And** the missing section is added before release
   **And** this is verified by a test
3. **Given** the trust contract is finalized
   **When** documentation is published
   **Then** the contract is available at `docs/trust-contract.md` and referenced by a documentation test

## Tasks / Subtasks

- [x] Audit `docs/trust-contract.md` for completeness
  - [x] Ensure all invariants in `TrustContractV1` are documented
  - [x] Ensure refusal mappings are documented with stable identifiers
  - [x] Ensure disclosure policy rules and safe states are explicitly listed
- [x] Add documentation tests
  - [x] Verify `docs/trust-contract.md` exists
  - [x] Verify all invariants and refusal mappings are present
  - [x] Fail if any required invariant or refusal mapping is missing

## Developer Context

- The trust contract already exists; this story is about completeness and enforcement via tests.
- Documentation must align with `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs`.

## Technical Requirements

- Document must include:
  - Invariant list with stable identifiers
  - Refusal mapping schema with stable identifiers
  - Disclosure policy rules and safe-state tokens
- Tests must fail if any invariant or refusal mapping is missing from docs.

## Architecture Compliance

- Preserve trust contract identifiers and semantics.
- Do not introduce new invariants outside the contract.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- No new documentation tooling or dependencies.

## File Structure Requirements

- Documentation:
  - `docs/trust-contract.md` (update if needed)
- Tests:
  - `TenantSaas.ContractTests/` (documentation validation)

## Testing Requirements

- Add doc tests that assert:
  - Trust contract doc exists
  - All invariants in `TrustContractV1` appear in the doc
  - All refusal mappings in `TrustContractV1` appear in the doc
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.4 introduced documentation length enforcement for conceptual docs; reuse the same doc test patterns.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.

## Project Context Reference

- Keep documentation crisp and aligned with the trust contract.
- Never introduce new dependencies without explicit justification.

## Story Completion Status

- Status set to **review**
- Completion note: Trust contract documentation completeness and enforcement tests implemented.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy baseline
- dev-story execution (YOLO): added trust contract documentation validation tests
- red phase: `TrustContractDoc_ListsAllRefusalMappingsWithStableIdentifiers` failed due to missing "Refusal Mapping Registry" section
- green phase: updated `docs/trust-contract.md` with explicit refusal mapping registry and guidance URIs
- regression: contract tests and full solution tests passing

### Completion Notes List

- Extracted Epic 6 story 6.5 requirements and aligned with `TrustContractV1`.
- Planned doc tests to enforce invariant and refusal mapping completeness.
- Implemented `TrustContractDocumentationTests` to enforce doc existence, invariant coverage, refusal mapping coverage, and disclosure safe-state coverage.
- Added explicit refusal mapping registry in `docs/trust-contract.md` with invariant code, status, problem type, title, and guidance URI per mapping.
- Verified test commands:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

### File List

- _bmad-output/implementation-artifacts/6-5-publish-the-trust-contract.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- docs/trust-contract.md
- TenantSaas.ContractTests/TrustContractDocumentationTests.cs

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.
- 2026-02-15: Added trust contract documentation enforcement tests and completed refusal mapping registry documentation; story moved to review.
- 2026-02-15: Code review (Claude Opus 4.6): fixed BreakGlassExplicitAndAudited description drift in doc, removed redundant HTTP Status table, added Title+Description assertions to tests. 4 issues found, 4 fixed. All 429 tests pass. Story moved to done.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.5: Publish the Trust Contract]
- [Source: docs/trust-contract.md]
- [Source: _bmad-output/planning-artifacts/architecture.md#Trust Contract/docs]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
