# Story 6.6: Publish the Verification Guide

Status: ready-for-dev

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a verification guide,
so that I can run and interpret contract tests quickly.

## Acceptance Criteria

1. **Given** the documentation set
   **When** I review it as a new adopter
   **Then** I can find the verification guide
   **And** it explains how to run and interpret contract tests end to end
   **And** this is verified by a test
2. **Given** a verification step fails or is out of date
   **When** the guide is followed
   **Then** the failure is reproducible
   **And** the guide is updated with the corrected steps
   **And** this is verified by a test
3. **Given** the verification guide is finalized
   **When** documentation is published
   **Then** the guide is available at `docs/verification-guide.md` and referenced by a documentation test

## Tasks / Subtasks

- [ ] Create `docs/verification-guide.md`
  - [ ] Include exact commands for contract tests and solution tests
  - [ ] Explain how to interpret failures (Problem Details + invariant codes)
  - [ ] Include troubleshooting for common failures
- [ ] Add documentation tests
  - [ ] Verify `docs/verification-guide.md` exists
  - [ ] Assert required sections/commands appear
- [ ] Link the verification guide from README and/or integration guide

## Developer Context

- The guide must be runnable by adopters with only the repo and .NET SDK installed.
- The goal is to reduce time-to-proof by providing deterministic steps and expected signals.

## Technical Requirements

- Include commands:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`
- Explain expected signals:
  - Problem Details with invariant_code and trace_id
  - Contract-test failure messages referencing contract rules

## Architecture Compliance

- Keep refuse-by-default and Problem Details patterns intact.
- Do not introduce new tooling dependencies or test frameworks.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- No new documentation tooling or dependencies.

## File Structure Requirements

- Documentation:
  - `docs/verification-guide.md` (new)
- Tests:
  - `TenantSaas.ContractTests/` (documentation validation)

## Testing Requirements

- Add doc tests that assert:
  - Verification guide exists at `docs/verification-guide.md`
  - Required commands are present
  - Failure-interpretation guidance is present
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.5 enforced trust-contract completeness; the verification guide should reference trust contract identifiers when interpreting failures.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.

## Project Context Reference

- Keep documentation crisp and aligned with the trust contract.
- No new dependencies or tools.

## Story Completion Status

- Status set to **ready-for-dev**
- Completion note: Verification guide doc and test plan prepared; ready for implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy baseline

### Completion Notes List

- Extracted Epic 6 story 6.6 requirements and defined guide structure and validation checks.
- Included deterministic test commands and failure interpretation signals.

### File List

- _bmad-output/implementation-artifacts/6-6-publish-the-verification-guide.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-14: Story created and marked ready-for-dev.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.6: Publish the Verification Guide]
- [Source: _bmad-output/planning-artifacts/prd.md#Verification & Contract Tests]
- [Source: _bmad-output/project-context.md#Testing Rules]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
