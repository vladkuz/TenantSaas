# Story 6.5: Publish the Trust Contract

Status: ready-for-dev

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

- [ ] Audit `docs/trust-contract.md` for completeness
  - [ ] Ensure all invariants in `TrustContractV1` are documented
  - [ ] Ensure refusal mappings are documented with stable identifiers
  - [ ] Ensure disclosure policy rules and safe states are explicitly listed
- [ ] Add documentation tests
  - [ ] Verify `docs/trust-contract.md` exists
  - [ ] Verify all invariants and refusal mappings are present
  - [ ] Fail if any required invariant or refusal mapping is missing

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

- Status set to **ready-for-dev**
- Completion note: Trust contract verification plan prepared; ready for implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy baseline

### Completion Notes List

- Extracted Epic 6 story 6.5 requirements and aligned with `TrustContractV1`.
- Planned doc tests to enforce invariant and refusal mapping completeness.

### File List

- _bmad-output/implementation-artifacts/6-5-publish-the-trust-contract.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.5: Publish the Trust Contract]
- [Source: docs/trust-contract.md]
- [Source: _bmad-output/planning-artifacts/architecture.md#Trust Contract/docs]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
