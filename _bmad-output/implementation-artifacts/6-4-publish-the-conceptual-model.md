# Story 6.4: Publish the Conceptual Model

Status: ready-for-dev

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a concise conceptual model,
so that I can understand tenancy, scope, and shared-system context quickly.

## Acceptance Criteria

1. **Given** the documentation set
   **When** I review it as a new adopter
   **Then** I can find the conceptual model document
   **And** it is <=800 words or <=2 pages and links to the trust contract
   **And** this is verified by a test
2. **Given** the conceptual model exceeds the length constraint
   **When** documentation is reviewed
   **Then** the overage is flagged and corrected before release
   **And** this is verified by a test

## Tasks / Subtasks

- [ ] Create `docs/conceptual-model.md`
  - [ ] Keep length <=800 words (enforceable by test)
  - [ ] Link to `docs/trust-contract.md`
  - [ ] Cover tenancy, scope, shared-system context, and invariants at a high level
- [ ] Add documentation tests
  - [ ] Assert file exists and link to trust contract is present
  - [ ] Enforce word-count limit (<=800 words)
- [ ] Link conceptual model from README or integration guide (if appropriate)

## Developer Context

- The conceptual model is a concise onboarding doc, not an implementation spec.
- It must align with the trust contract and architecture definitions.
- Keep the tone crisp and anchored on invariants and boundaries.

## Technical Requirements

- Document must include:
  - Tenancy and tenant scope
  - Shared-system scope
  - Execution kinds at a high level
  - The invariant-driven boundary model
- Must link directly to `docs/trust-contract.md`.

## Architecture Compliance

- Align terminology with `TenantScope`, `ExecutionKind`, and invariant names.
- Do not introduce new concepts that conflict with the trust contract.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- No new documentation tooling or dependencies.

## File Structure Requirements

- Documentation:
  - `docs/conceptual-model.md` (new)
- Tests:
  - `TenantSaas.ContractTests/` (documentation validation)

## Testing Requirements

- Add doc tests that assert:
  - Conceptual model doc exists
  - Link to `docs/trust-contract.md` is present
  - Word count <=800
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.3 enforces storage-agnostic boundaries; the conceptual model should reinforce that the core is invariant-first and adapter-optional.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.

## Project Context Reference

- Keep documentation crisp and aligned with the trust contract.
- Do not introduce new dependencies or tooling.

## Story Completion Status

- Status set to **ready-for-dev**
- Completion note: Conceptual model document and validation plan prepared; ready for implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy baseline

### Completion Notes List

- Extracted Epic 6 story 6.4 requirements and designed doc + test coverage plan.
- Added explicit length enforcement and trust-contract linkage.

### File List

- _bmad-output/implementation-artifacts/6-4-publish-the-conceptual-model.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.4: Publish the Conceptual Model]
- [Source: _bmad-output/planning-artifacts/prd.md#Documentation & Trust Contract]
- [Source: _bmad-output/planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
