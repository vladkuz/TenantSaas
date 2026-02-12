# Story 5.5: Assert Disclosure Policy and Error/Log Correlation Rules

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an auditor,
I want disclosure safety and correlation proven by contract tests,
so that logs and errors are safe and diagnosable together.

## Acceptance Criteria

1. **Given** a refusal occurs under sensitive disclosure conditions
   **When** compliance tests inspect Problem Details and logs
   **Then** tenant information is withheld or redacted per policy
   **And** tenant_ref uses safe-state values
   **And** this is verified by a test
2. **Given** any invariant refusal occurs
   **When** compliance tests inspect output
   **Then** invariant_code and trace_id appear in both errors and logs
   **And** request_id appears for request execution kinds
   **And** this is verified by a test
3. **Given** disclosure is unsafe
   **When** compliance tests validate outputs
   **Then** an error must refuse to expose tenant identifiers

## Tasks / Subtasks

- [x] Add or extend disclosure-policy contract tests that prove unsafe disclosure never exposes tenant identifiers in Problem Details
- [x] Add or extend log/error correlation tests that prove trace_id, request_id (request-only), and invariant_code are present and joinable
- [x] Add or extend tests ensuring tenant_ref uses safe-state values when disclosure is unsafe
- [x] Keep tests deterministic and CI-friendly (no timing dependence, no environment coupling)
- [x] Run contract tests and solution tests before handoff

## Developer Context

- This is a contract-test proof story, not a core feature rewrite.
- Disclosure policy and correlation behavior already exist; the goal is to assert and lock them via compliance tests.
- Avoid duplicating enforcement logic. Prefer test coverage that validates existing behavior.
- Explicitly prove the joinability of logs and Problem Details via trace_id, request_id (request only), and invariant_code.

## Technical Requirements

- Validate disclosure safety for Problem Details when disclosure is unsafe:
  - tenant_ref must be absent or safe-state token
  - tenant identifiers must never be leaked
- Validate log/error correlation rules:
  - trace_id and invariant_code exist in Problem Details and logs
  - request_id exists for request execution kinds only
- Assert that any refusal in request execution kinds includes request_id in Problem Details extensions.
- Keep Problem Details RFC 7807 shape and extension key names stable.

## Architecture Compliance

- Preserve refuse-by-default enforcement and trust contract identifiers.
- Keep Problem Details as the sole error shape for refusals.
- Preserve disclosure policy semantics (safe-state tokens and tenant_ref rules).
- Do not introduce new dependencies or change public contracts.

## Library & Framework Requirements

- Keep net10.0 baseline unchanged.
- Continue using the existing test stack in TenantSaas.ContractTests:
  - xUnit
  - FluentAssertions
  - Moq (only if needed)
  - Microsoft.NET.Test.Sdk
- Treat library upgrades as out-of-scope unless required for compatibility.

## File Structure Requirements

- Primary implementation targets:
  - TenantSaas.ContractTests/
- Preferred files to extend (avoid redundant suites):
  - TenantSaas.ContractTests/Logging/RefusalCorrelationTests.cs
  - TenantSaas.ContractTests/Logging/LogEnricherTests.cs
  - TenantSaas.ContractTests/Errors/ProblemDetailsShapeTests.cs
  - TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs
  - TenantSaas.ContractTests/DisclosurePolicyTests.cs
- Avoid creating new test projects or moving existing contracts.

## Testing Requirements

- Add/adjust contract tests that directly prove AC1–AC3 for Story 5.5.
- Ensure denied disclosure cases do not expose tenant_ref or raw tenant IDs in Problem Details.
- Ensure correlation fields (trace_id, request_id, invariant_code) match between log entries and Problem Details.
- Validate request_id is omitted for non-request execution kinds.
- Validate deterministic outputs by using fixed trace/request IDs in tests.
- Validate via:
  - dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal
  - dotnet test TenantSaas.sln --disable-build-servers -v minimal

## Previous Story Intelligence

- Story 5.4 reinforced test-first compliance delivery and minimizing scope.
- Reuse existing break-glass and refusal test patterns instead of creating parallel logic.
- Keep changes narrow and evidence-driven to align with Epic 5 objectives.

## Git Intelligence Summary

Recent patterns indicate incremental, test-first compliance delivery:

- 8a52502: added reference compliance tests for break-glass refusal/audit emission and updated sprint status
- 932c936: integrated contract tests into CI workflow
- 5285339: added refusal/attribution reference compliance tests (establishes expected style)

## Latest Technical Information

- .NET 10 is listed as STS with support ending on 2026-11-10; keep CI pinned to 10.0.x for this story.
- Latest known package versions (do not upgrade unless required for compatibility):
  - xUnit: 2.9.3
  - xunit.runner.visualstudio: 3.1.5
  - FluentAssertions: 8.8.0
- GitHub Actions releases available (do not upgrade in this story):
  - actions/checkout: v6.0.2
  - actions/setup-dotnet: v5.1.0

## Project Context Reference

- Never bypass tenant context or invariant guards.
- Never emit non-UTC timestamps or timestamps without Z.
- Never log without required structured fields.
- Never return non-Problem Details error responses.
- Never expose tenant identifiers when disclosure is unsafe.
- Never introduce new dependencies without explicit justification.

## Story Completion Status

- Status set to **review**
- Completion note: Contract tests added for disclosure safety and correlation; compliance tests executed on 2026-02-12.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow execution (YOLO mode)
- artifact discovery: epics, PRD, architecture, project-context, previous story, git history
- latest-technology check: .NET support policy, NuGet package feeds, GitHub Actions releases
- validation task: validate-workflow.xml not found (skipped)
- dev-story workflow execution (YOLO mode)

### Completion Notes List

- Parsed next backlog story from sprint tracking (5-5-assert-disclosure-policy-and-error-log-correlation-rules).
- Consolidated epic intent, architecture constraints, and project-context rules into implementation guardrails.
- Identified existing disclosure and correlation test files to extend instead of creating new suites.
- Added explicit test and correlation requirements for tenant_ref safe-state handling.
- Validation step skipped because _bmad/core/tasks/validate-workflow.xml is missing in repo.
- Added correlation tests asserting trace_id, request_id (request-only), and invariant_code joinability between logs and Problem Details.
- Added disclosure-safety tests ensuring unsafe disclosure does not expose tenant identifiers in errors.
- Added ProblemDetails disclosure-unsafe contract test to ensure tenant_ref is omitted and invariant/trace/request fields remain stable.
- Captured RefusalEmitted logs in middleware tests and asserted correlation fields match Problem Details.
- Added non-request refusal log overload to omit request_id and asserted omission in tests.
- Expanded disclosure-unsafe assertions to scan Problem Details fields and serialized output for tenant identifiers.
- Tests run (2026-02-12): `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`; `dotnet test TenantSaas.sln --disable-build-servers -v minimal`.

### File List

- .codex/auth.json
- .codex/config.toml
- .codex/history.jsonl
- .codex/log/codex-tui.log
- .codex/models_cache.json
- .codex/version.json
- _bmad-output/implementation-artifacts/5-5-assert-disclosure-policy-and-error-log-correlation-rules.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- TenantSaas.ContractTests/DisclosurePolicyTests.cs
- TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs
- TenantSaas.ContractTests/Logging/RefusalCorrelationTests.cs
- TenantSaas.Core/Logging/EnforcementEventSource.cs

## Change Log

- 2026-02-12: Added disclosure safety and correlation contract tests; ran contract/solution tests; marked story review-ready.
- 2026-02-12: Captured refusal log correlation in middleware tests; ensured non-request refusals omit request_id; expanded disclosure-unsafe leakage checks.
- 2026-02-12: Tests re-run and green; status set to done.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.5: Assert Disclosure Policy and Error/Log Correlation Rules]
- [Source: _bmad-output/planning-artifacts/architecture.md#Error Handling Patterns]
- [Source: _bmad-output/planning-artifacts/architecture.md#Logging Patterns]
- [Source: _bmad-output/project-context.md#Critical Don’t-Miss Rules]
- [Source: TenantSaas.ContractTests/DisclosurePolicyTests.cs]
- [Source: TenantSaas.ContractTests/Logging/RefusalCorrelationTests.cs]
- [Source: TenantSaas.ContractTests/Logging/LogEnricherTests.cs]
- [Source: TenantSaas.ContractTests/Errors/ProblemDetailsShapeTests.cs]
- [Source: TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs]
- [Source: _bmad-output/implementation-artifacts/5-4-assert-break-glass-constraints-and-audit-event-emission.md]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [External: xunit package versions](https://www.nuget.org/profiles/xunit)
- [External: FluentAssertions package](https://www.nuget.org/packages/FluentAssertions)
- [External: actions/checkout releases](https://github.com/actions/checkout/releases)
- [External: actions/setup-dotnet releases](https://github.com/actions/setup-dotnet/releases)
