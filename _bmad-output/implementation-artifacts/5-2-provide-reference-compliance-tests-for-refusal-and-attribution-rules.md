# Story 5.2: Provide Reference Compliance Tests for Refusal and Attribution Rules

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a platform engineer,
I want reference tests that prove the guardrails work,
so that I can see failures when invariants are violated.

## Acceptance Criteria

1. **Given** reference compliance tests
   **When** context is missing, ambiguous, or disallowed
   **Then** operations are refused by default
   **And** refusals reference the appropriate invariants
   **And** this is verified by a test
2. **Given** attribution sources disagree
   **When** compliance tests run
   **Then** attribution is treated as ambiguous
   **And** refusal behavior is consistent with the trust contract
   **And** this is verified by a test
3. **Given** attribution sources disagree
   **When** compliance tests execute
   **Then** the test must fail and report an error refusal

## Developer Context

- This story should add/adapt **reference compliance tests** that prove refusal and attribution behavior as black-box guardrails.
- Prefer extending existing contract-test surfaces before creating new abstractions:
  - `TenantSaas.ContractTests/AttributionEnforcementTests.cs`
  - `TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs`
  - `TenantSaas.ContractTests/Logging/RefusalCorrelationTests.cs`
  - `TenantSaas.ContractTests/MiddlewareProblemDetailsTests.cs`
- Reuse helper assertions added in Story 5.1 (`TenantSaas.ContractTestKit/Assertions/*`) to avoid reinventing validation logic.
- Ensure tests prove both enforcement outcome and refusal contract shape (`invariant_code`, status mapping, trace correlation).

## Technical Requirements

- Target `net10.0` for all test code.
- Use xUnit + FluentAssertions for assertions; Moq only when unavoidable.
- Tests must remain deterministic and CI-stable.
- Validate refusal mappings for missing, ambiguous, and disallowed attribution using trust-contract invariant identifiers.
- Include trace correlation assertions when validating refusal payload/log behavior.

## Architecture Compliance

- Enforce via sanctioned boundary helpers only; do not bypass guards or create alternate enforcement paths.
- Refusal checks must align with RFC 7807 Problem Details and stable `invariant_code` semantics.
- Tenant disclosure policy must remain intact in failure tests (no unsafe tenant identifier leakage).
- Keep contract-test coverage non-invasive to domain logic and focused on boundary behavior.

## Library & Framework Requirements

- .NET 10 remains required for this project.
- Existing stack in repo should remain the baseline unless explicitly upgraded in a separate dependency update story:
  - `xunit` / `xunit.runner.visualstudio`
  - `FluentAssertions`
  - `Moq`
  - `Microsoft.NET.Test.Sdk`
- Do not add new test frameworks.

## File Structure Requirements

- Keep all new tests under `TenantSaas.ContractTests/` (no `src/` or `tests/` folder introduction).
- Prefer colocating new tests by concern:
  - attribution behavior in existing attribution/enforcement suites
  - refusal payload shape under `TenantSaas.ContractTests/Errors/`
  - correlation behavior under `TenantSaas.ContractTests/Logging/`
- If a new dedicated reference suite file is needed, place it at `TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs`.

## Testing Requirements

- Add or strengthen tests to cover:
  - missing context refusal
  - ambiguous attribution refusal
  - disallowed attribution source refusal
  - invariant-to-status mapping consistency
  - refusal payload consistency (`invariant_code`, `trace_id`, guidance/type)
- Ensure tests fail loudly with actionable assertions when attribution disagreement occurs.
- Keep CI runtime bounded by reusing existing fixtures/patterns.

## Tasks / Subtasks

- [x] Add story-scoped reference compliance tests for refusal defaults (AC: 1)
  - [x] Verify missing context is refused and mapped to `InvariantCode.ContextInitialized`
  - [x] Verify disallowed attribution source is refused with trust-contract invariant semantics
  - [x] Verify refusal payload includes `invariant_code` and `trace_id`
- [x] Add explicit disagreement-path test for attribution ambiguity (AC: 2, 3)
  - [x] Verify disagreeing sources resolve to ambiguous attribution
  - [x] Verify operation fails with error refusal (`422` + `TenantAttributionUnambiguous`)
  - [x] Verify conflicting source metadata is included in refusal payload
- [x] Validate trust-contract refusal mapping consistency (AC: 1, 2)
  - [x] Reuse `TrustContractFixture` to validate refusal mapping invariants in story-scoped suite
- [x] Run contract tests and capture evidence (AC: 1, 2, 3)
  - [x] Run `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - [x] Confirm all tests pass without failures

## Previous Story Intelligence

- Story 5.1 delivered `TenantSaas.ContractTestKit` with reusable assertions for invariants, refusal mappings, attribution, disclosure, and Problem Details.
- Story 5.1 added API stability tests and established the pattern of validating contracts through public surfaces only.
- Reuse `TrustContractFixture` and assertion helpers to avoid duplicated assertion code and drift.
- Preserve current quality bar from Story 5.1 completion: full suite passing and no regression in existing contract tests.

## Git Intelligence Summary

Recent commits indicate established implementation patterns for this story:

- `db71529`: introduced `TenantSaas.ContractTestKit` plus contract-test integration and stability tests.
- `c372a5a`, `47a8ccf`, `35dbebb`, `efea484`: added focused contract-test files per story AC and updated implementation artifacts/sprint tracking in lockstep.
- Pattern to follow:
  - Implement story-specific tests in narrowly scoped files.
  - Keep assertions explicit and linked to invariant names.
  - Preserve story + sprint-status updates as part of workflow hygiene.

## Latest Technical Information

- .NET 10 is current LTS; keep `net10.0` target for compatibility with the repository baseline and existing package references.
- xUnit ecosystem has active v3 support available; current repo remains on xUnit v2 packages, so story implementation should stay on existing versions unless a dedicated upgrade story is approved.
- FluentAssertions and Microsoft.NET.Test.Sdk have newer releases available; avoid opportunistic upgrades in this story to prevent scope creep and regression risk.
- Moq version in repo (`4.20.72`) is current and sufficient for this story scope.

### Project Structure Notes

- Maintain flat repository conventions.
- Keep refusal and attribution compliance assertions close to existing enforcement/error/logging suites.
- Prefer extending existing files when the scenario aligns to avoid fragmentation.

## Project Context Reference

- Never bypass tenant context or invariant guards.
- Never return non-Problem Details error responses in tested API behavior.
- Ensure `invariant_code`, `trace_id`, and disclosure-safe tenant semantics remain consistent.
- Keep behavior deterministic and avoid introducing non-deterministic test setup.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.2: Provide Reference Compliance Tests for Refusal and Attribution Rules]
- [Source: _bmad-output/planning-artifacts/epics.md#Epic 5: Teams Can Prove Compliance in CI]
- [Source: _bmad-output/planning-artifacts/architecture.md#Error Handling Patterns]
- [Source: _bmad-output/planning-artifacts/architecture.md#Logging Patterns]
- [Source: _bmad-output/project-context.md#Critical Implementation Rules]
- [Source: _bmad-output/project-context.md#Testing Rules]
- [Source: _bmad-output/implementation-artifacts/5-1-ship-an-adopter-runnable-contract-test-helper-package.md]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [External: xUnit v3 docs](https://xunit.net/docs/getting-started/v3/getting-started)
- [External: xUnit package versions](https://www.nuget.org/packages/xunit)
- [External: FluentAssertions package versions](https://www.nuget.org/packages/FluentAssertions)
- [External: Microsoft.NET.Test.Sdk package versions](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
- [External: Moq package versions](https://www.nuget.org/packages/Moq)

## Story Completion Status

- Status set to **done**
- Completion note: Story-scoped reference compliance tests implemented and validated against refusal/attribution ACs

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
- Results: 394 passed, 0 failed, 0 skipped
- Sprint status synchronized to `done`

### Completion Notes List

- Added `TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs` with explicit AC coverage for:
  - missing context refusal
  - disallowed attribution source refusal
  - ambiguous attribution disagreement refusal
- Reused `TrustContractFixture` to validate refusal mapping consistency with trust-contract expectations
- Verified refusal payload contract fields (`invariant_code`, `trace_id`) and status mapping behavior
- Executed contract test project successfully (394/394 passing)

### File List

- TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs (new)
- _bmad-output/implementation-artifacts/5-2-provide-reference-compliance-tests-for-refusal-and-attribution-rules.md (modified)
- _bmad-output/implementation-artifacts/sprint-status.yaml (modified)

## Senior Developer Review (AI)

### Reviewer

- Reviewer: Vlad
- Date (UTC): 2026-02-07T00:44:44Z

### Outcome

- Decision: **Approved**
- Rationale: Story now contains a story-scoped implementation delta with tests that directly satisfy AC1-AC3, and contract tests are green.

### Findings Resolved

- Updated story status from `ready-for-dev` to `done` after implementation/test completion.
- Added missing tasks/subtasks section and completion evidence.
- Added explicit story-scoped test coverage (`ReferenceComplianceRefusalAttributionTests`) to eliminate implementation ambiguity.
- Synced story metadata and file list with actual repository changes.

## Change Log

- 2026-02-07: Added story-scoped refusal/attribution compliance tests, updated story record, and synchronized sprint status to done.
