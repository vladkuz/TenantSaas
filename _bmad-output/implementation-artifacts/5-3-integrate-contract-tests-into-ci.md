# Story 5.3: Integrate Contract Tests into CI

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a team lead,
I want contract tests wired into CI,
so that baseline compliance is enforced before merges.

## Acceptance Criteria

1. **Given** the CI workflow definition
   **When** I open a pull request
   **Then** contract tests run automatically and fail the build on invariant violations
   **And** this is verified by a test
2. **Given** the enforcement boundary is bypassed
   **When** the CI pipeline runs
   **Then** the contract test job fails with a specific, documented error

## Developer Context

- This story makes contract-test enforcement explicit in CI and keeps failures actionable for contributors.
- Existing CI already runs `dotnet test` for the solution; implementation should strengthen this path so contract test failures are unmistakable and documented.
- Prefer extending existing workflow and docs over creating parallel workflows unless there is clear isolation value.
- Ensure CI behavior remains deterministic and reproducible between local and CI runs.

## Technical Requirements

- CI must execute contract tests on pull requests and fail fast on any invariant breach.
- The contract test execution path must remain compatible with `net10.0` and current solution layout.
- The workflow must surface a clear failing command and preserve non-zero exit behavior.
- Add/adjust documentation so contributors know exactly which command validates contract compliance locally.
- Preserve existing workflow scope (`push` on `main`, all `pull_request` branches) unless a broader CI strategy change is explicitly requested.

## Architecture Compliance

- Keep enforcement verification at boundary-level behavior through contract tests; do not add domain-level bypass toggles.
- Preserve refusal-by-default guarantees and invariant validation semantics by keeping CI tied to `TenantSaas.ContractTests` coverage.
- Maintain current repository conventions (flat structure, `.github/workflows/ci.yml`, no new tooling assumptions).
- Avoid introducing background services/timers or non-deterministic CI steps.

## Library & Framework Requirements

- Runtime/test baseline remains `net10.0`.
- Current test stack in repo must remain the default in this story:
  - `xunit` / `xunit.runner.visualstudio`
  - `FluentAssertions`
  - `Moq`
  - `Microsoft.NET.Test.Sdk`
- CI actions should remain pinned and explicit; if upgraded, validate compatibility and keep scope to CI plumbing only.

## File Structure Requirements

- Primary implementation target: `.github/workflows/ci.yml`.
- Optional documentation touchpoints (if needed for AC2 clarity):
  - `README.md`
  - `TenantSaas.ContractTestKit/README.md`
- Do not introduce new top-level workflow directories or duplicate CI definitions.

## Testing Requirements

- Validate CI command path locally using the same/similar command used by workflow.
- Ensure a failing contract-test scenario causes CI failure with a clear, documented error path.
- Confirm `dotnet test` execution remains deterministic with no environment-specific skips for contract tests.
- Keep test runtime practical and avoid broadening suite scope in this story.

## Tasks / Subtasks

- [x] Tighten CI workflow to make contract test enforcement explicit (AC: 1)
  - [x] Update `.github/workflows/ci.yml` to run solution tests in a way that clearly includes `TenantSaas.ContractTests`
  - [x] Ensure failing contract tests produce non-zero pipeline status without masking
  - [x] Keep SDK setup aligned with repository target framework
- [x] Add explicit contract-test verification guidance for contributors (AC: 1)
  - [x] Document canonical local verification command in `README.md` and/or `TenantSaas.ContractTestKit/README.md`
  - [x] Keep guidance concise and consistent with workflow command
- [x] Prove bypass detection behavior is surfaced via CI (AC: 2)
  - [x] Ensure the story implementation includes a documented failure mode when enforcement boundary is bypassed
  - [x] Confirm CI output path points developers to the failing contract-test signal
- [x] Validate end-to-end before handoff (AC: 1, 2)
  - [x] Run `dotnet test TenantSaas.sln --disable-build-servers -v minimal`
  - [x] Capture result summary and include it in completion notes after implementation

## Previous Story Intelligence

- Story 5.2 delivered focused refusal/attribution compliance tests and reinforced reuse of `TrustContractFixture` + `TenantSaas.ContractTestKit` assertions.
- Story 5.2 established that implementation evidence should include concrete passing test output and synchronized sprint-status updates.
- Reuse the existing contract-test style and naming patterns rather than introducing new testing abstractions.

## Git Intelligence Summary

Recent commits show the expected implementation and handoff pattern:

- `5285339`: Story 5.2 completed with new reference compliance tests and explicit validation evidence.
- `db71529`: Contract test kit introduction established reusable assertion surfaces and CI-focused verification intent.
- README-focused commits (`b5131df`, `613fc48`, `d1bdf0f`) show docs are actively curated and should be updated with precise, minimal command guidance when CI behavior changes.

## Latest Technical Information

- .NET 10 is in active support and receives regular servicing updates; keep CI pinned to 10.x and avoid cross-major upgrades in this story.
- GitHub Actions `actions/checkout` and `actions/setup-dotnet` both have v5 releases; current workflow is on v4 and can stay there unless CI-maintenance scope is explicitly included.
- `Microsoft.NET.Test.Sdk` has newer versions available than the repo baseline; treat test package upgrades as out-of-scope unless required to resolve a CI issue discovered during implementation.

### Project Structure Notes

- Keep CI changes concentrated in `.github/workflows/ci.yml`.
- Keep contract-test behavior source-of-truth in existing test projects and trust-contract docs.
- Avoid introducing additional workflow complexity beyond what is needed to enforce AC1/AC2.

## Project Context Reference

- Never bypass tenant context or invariant guards.
- Never allow contract-test failures to be treated as warnings.
- Preserve deterministic behavior and explicit refusal semantics (`invariant_code`, trace correlation).
- Keep changes minimal, auditable, and aligned with current architecture and workflow patterns.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.3: Integrate Contract Tests into CI]
- [Source: _bmad-output/planning-artifacts/prd.md#FR16]
- [Source: _bmad-output/planning-artifacts/prd.md#NFR8]
- [Source: _bmad-output/planning-artifacts/prd.md#NFR12]
- [Source: _bmad-output/planning-artifacts/architecture.md#High Level Architecture]
- [Source: _bmad-output/project-context.md#Technology Stack & Versions]
- [Source: _bmad-output/project-context.md#Testing Rules]
- [Source: .github/workflows/ci.yml]
- [Source: TenantSaas.ContractTests/TenantSaas.ContractTests.csproj]
- [Source: _bmad-output/implementation-artifacts/5-2-provide-reference-compliance-tests-for-refusal-and-attribution-rules.md]
- [External: .NET and .NET Core official support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [External: .NET 10.0.2 release notes](https://github.com/dotnet/core/releases/tag/v10.0.2)
- [External: actions/checkout releases](https://github.com/actions/checkout/releases)
- [External: actions/setup-dotnet releases](https://github.com/actions/setup-dotnet/releases)
- [External: Microsoft.NET.Test.Sdk package versions](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

## Story Completion Status

- Status set to **done**
- Completion note: CI now runs explicit contract tests with hard-fail error signaling and documented local verification commands.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- Story creation workflow execution (YOLO mode)
- Dev execution (YOLO mode): red-green-refactor via `TenantSaas.ContractTests/CiWorkflowTests.cs`
- Validation runs:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal` (Passed: 397, Failed: 0)
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal` (Passed: 397, Failed: 0)

### Implementation Plan

- Enforce explicit contract-test execution in CI before full solution test execution.
- Surface an unambiguous CI error string for enforcement-bypass detection.
- Keep contributor verification docs aligned with workflow commands.
- Verify workflow behavior with contract tests that assert command path and failure message.

### Completion Notes List

- Parsed next backlog story from sprint status and generated a complete implementation-ready story file.
- Incorporated epic requirements, architecture/project-context constraints, previous-story and git learnings, and current ecosystem checks.
- Synchronized sprint tracking status to `in-progress` at start and `review` at completion.
- Added CI contract-test gating step with explicit non-zero failure behavior and documented error:
  - `Contract test failure detected; enforcement boundary may be bypassed`
- Updated CI solution test command to canonical verification path:
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`
- Added/updated CI workflow contract tests to verify:
  - explicit contract test project execution
  - explicit enforcement failure error text
  - explicit non-zero exit behavior (`exit 1`) on contract-test failure
  - canonical solution command presence
- Updated contributor docs in `README.md` and `TenantSaas.ContractTestKit/README.md` with canonical verification and CI signaling guidance.

### File List

- .github/workflows/ci.yml (modified)
- TenantSaas.ContractTests/CiWorkflowTests.cs (modified)
- README.md (modified)
- TenantSaas.ContractTestKit/README.md (modified)
- _bmad-output/implementation-artifacts/sprint-status.yaml (modified)
- _bmad-output/implementation-artifacts/5-3-integrate-contract-tests-into-ci.md (modified)

## Change Log

- 2026-02-08: Implemented AC1/AC2 CI contract-test enforcement, added explicit failure signaling, aligned local verification documentation, and validated with full solution test run.
- 2026-02-08: Senior code review remediation completed: tightened CI failure wording, added explicit non-zero exit assertion coverage, aligned README command consistency, reran contract + solution tests, and moved story to done.

## Senior Developer Review (AI)

### Reviewer

Vlad (AI-assisted review workflow)

### Date

2026-02-08

### Outcome

Approved after remediation

### Findings Resolved

- HIGH: Added explicit CI test coverage for non-zero exit behavior on contract test failures (`CiWorkflowShouldFailWithNonZeroExitWhenContractTestsFail`).
- MEDIUM: Updated failure wording to be specific but not over-attributed (`Contract test failure detected; enforcement boundary may be bypassed`).
- MEDIUM: Aligned README top-level verification command with canonical CI command flags.
- MEDIUM: Confirmed AC behavior with rerun evidence (`397 passed`, `0 failed`) for both contract-project and solution commands.
