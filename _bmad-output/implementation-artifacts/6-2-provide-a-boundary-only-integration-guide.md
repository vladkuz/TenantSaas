# Story 6.2: Provide a Boundary-Only Integration Guide

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a platform engineer,
I want an integration guide that avoids domain rewrites,
so that adoption cost stays low.

## Acceptance Criteria

1. **Given** the integration guide
   **When** I follow it end to end
   **Then** integration steps are focused on boundaries and configuration
   **And** the guide makes no requirement to rewrite domain logic
   **And** this is verified by a test
2. **Given** integration is complete
   **When** I run compliance tests
   **Then** they exercise the configured boundaries successfully
   **And** failures point back to specific contract rules
   **And** this is verified by a test
3. **Given** a boundary is skipped or misconfigured
   **When** I run the integration guide’s verification steps
   **Then** the failure is explicit and references the missing boundary configuration
   **And** this is verified by a test
4. **Given** the integration guide is finalized
   **When** documentation is published
   **Then** the guide is available at `docs/integration-guide.md` and referenced by a documentation test

## Tasks / Subtasks

- [x] Audit `docs/integration-guide.md` for boundary-only steps
  - [x] Ensure steps focus on initialization, boundaries, and configuration (no domain rewrites)
  - [x] Add explicit “no domain logic changes required” statement
- [x] Add a verification section that maps failures to contract rules
  - [x] Include “missing boundary configuration” examples with expected failure messages
- [x] Add documentation tests
  - [x] Verify `docs/integration-guide.md` exists and is referenced by tests
  - [x] Assert required boundary-only language and verification section is present
- [x] Ensure compliance test guidance points to existing contract tests

## Developer Context

- This is a documentation + test verification story.
- The integration guide exists, but must explicitly demonstrate boundary-only adoption and verification steps.
- Any guidance that implies domain rewrites must be removed or corrected.

## Technical Requirements

- Guide must emphasize:
  - Single unavoidable integration point
  - Boundary-only wiring (middleware, initializer, guard)
  - No changes to domain/business logic
- Verification section must:
  - Name the contract tests to run
  - Describe expected failure signals when boundaries are missing
  - Point back to trust contract identifiers for failures

## Architecture Compliance

- Keep Minimal API + Problem Details patterns intact.
- Do not introduce new integration entry points.
- Preserve the “refuse-by-default” posture.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- No new documentation tooling or dependencies.

## File Structure Requirements

- Documentation:
  - `docs/integration-guide.md` (update)
- Tests:
  - `TenantSaas.ContractTests/` (add documentation validation test)

## Testing Requirements

- Add doc tests that assert:
  - Guide exists at `docs/integration-guide.md`
  - Boundary-only language is present
  - Verification section includes boundary-missing failure signals
- Keep tests deterministic and CI-friendly.
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.1 established the canonical extension seams list and documentation baseline.
- Reuse the documentation test pattern introduced for extension seams.

## Git Intelligence Summary

- No git history analysis performed for this story.

## Latest Technical Information

- .NET 10 is LTS with latest patch 10.0.2 (2026-01-13); keep `net10.0` pinned for this story.
- Swashbuckle.AspNetCore latest is 10.1.2 (2026-02-05); keep repo baseline 10.1.0 for this story.

## Project Context Reference

- Never bypass tenant context or invariant guards.
- Keep documentation crisp and aligned with the trust contract.
- No domain rewrites required for integration.

## Story Completion Status

- Status set to **done**
- Completion note: Code review passed — ToC updated, test assertions strengthened for AC2/AC3 invariant codes, trailing newline fixed.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- validation task: _bmad/core/tasks/validate-workflow.xml not found (skipped)
- latest-technology check: .NET support policy + Swashbuckle baseline
- tests: dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal
- tests: dotnet test TenantSaas.sln --disable-build-servers -v minimal

### Completion Notes List

- Extracted Epic 6 story 6.2 requirements and aligned guide expectations with boundary-only adoption.
- Planned doc tests to lock guide content and verification steps.
- Updated docs/integration-guide.md with boundary-only summary and contract-test verification mapping.
- Added IntegrationGuideDocumentationTests to assert guide presence and required boundary-only/verification language.
- Tests passed: dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal; dotnet test TenantSaas.sln --disable-build-servers -v minimal.

### File List

- _bmad-output/implementation-artifacts/6-2-provide-a-boundary-only-integration-guide.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- TenantSaas.ContractTests/IntegrationGuideDocumentationTests.cs
- docs/integration-guide.md

## Change Log

- 2026-02-15: Updated integration guide for boundary-only adoption and added documentation tests.
- 2026-02-14: Story created and marked ready-for-dev.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.2: Provide a Boundary-Only Integration Guide]
- [Source: _bmad-output/planning-artifacts/prd.md#Documentation & Trust Contract]
- [Source: _bmad-output/planning-artifacts/architecture.md#Integration Points]
- [Source: docs/integration-guide.md]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [External: Swashbuckle.AspNetCore package](https://www.nuget.org/packages/Swashbuckle.AspNetCore)
