# Story 5.4: Assert Break-Glass Constraints and Audit Event Emission

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a security stakeholder,
I want break-glass behavior proven by tests,
so that escalations cannot bypass the contract silently.

## Acceptance Criteria

1. **Given** break-glass is not explicitly declared
   **When** privileged or cross-tenant operations are attempted
   **Then** compliance tests fail with a refusal
   **And** the refusal references `BreakGlassExplicitAndAudited`
   **And** this is verified by a test
2. **Given** break-glass is explicitly declared and allowed
   **When** the operation proceeds in reference tests
   **Then** a standard audit event is emitted
   **And** the audit event includes the contract-required fields

## Developer Context

- Story 5.4 is a contract-test proof story, not a core feature rewrite.
- Break-glass enforcement and audit event primitives already exist (from Story 3.5); implementation should add/align reference compliance coverage instead of duplicating enforcement logic.
- The acceptance signal for this story is explicit, stable test evidence that maps directly to trust-contract behavior for denied and allowed break-glass paths.
- Keep implementation deterministic and CI-friendly to match Epic 5 objectives.

## Technical Requirements

- Validate refusal behavior for missing/invalid break-glass declarations through compliance-oriented tests that assert:
  - invariant code `BreakGlassExplicitAndAudited`
  - refusal mapping remains HTTP 403
  - deterministic error shape (Problem Details extensions)
- Validate successful break-glass path includes contract-required audit event fields:
  - actor
  - reason
  - scope
  - target tenant reference or `cross_tenant` marker
  - trace identifier
  - invariant/audit code semantics
- Reuse existing break-glass contracts (`BreakGlassDeclaration`, `BreakGlassValidator`, `BreakGlassAuditEvent`, `IBoundaryGuard.RequireBreakGlassAsync`) rather than introducing alternate schemas.
- Keep existing behavior for optional audit sink non-blocking/fail-safe.

## Architecture Compliance

- Preserve refuse-by-default boundaries and do not add bypass toggles.
- Keep trust-contract identifiers stable (especially `BreakGlassExplicitAndAudited` and audit code names).
- Maintain Problem Details + invariant-code discipline for refusals.
- Keep changes scoped to test/compliance surfaces unless a failing AC proves a minimal production fix is required.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- Continue using current test stack in `TenantSaas.ContractTests`:
  - `xunit`
  - `FluentAssertions`
  - `Moq`
  - `Microsoft.NET.Test.Sdk`
- Treat framework/tooling upgrades as out-of-scope unless needed to resolve a direct compatibility issue.

## File Structure Requirements

- Primary implementation target(s):
  - `TenantSaas.ContractTests/`
- Most likely files to extend (preferred over creating redundant suites):
  - `TenantSaas.ContractTests/Enforcement/BreakGlassEnforcementTests.cs`
  - `TenantSaas.ContractTests/BreakGlassContractTests.cs`
  - `TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs` (or new reference-compliance file if cleaner separation is needed)
- Do not move break-glass contracts out of existing abstractions/core locations.

## Testing Requirements

- Add/adjust tests that explicitly prove AC1 and AC2 from Story 5.4.
- Ensure denied path asserts both semantic failure and stable contract identifiers.
- Ensure allowed path asserts audit-event schema fields and `cross_tenant` behavior when tenant target is null.
- Keep tests deterministic (no timing-sensitive assertions, no environment-specific assumptions).
- Validate via:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Tasks / Subtasks

- [x] Add or refine reference compliance tests for denied break-glass behavior (AC: 1)
  - [x] Assert refusal invariant code remains `BreakGlassExplicitAndAudited`
  - [x] Assert refusal mapping remains HTTP 403 and includes trace correlation fields
- [x] Add or refine reference compliance tests for allowed break-glass behavior (AC: 2)
  - [x] Assert audit event includes actor, reason, scope, tenant marker/reference, trace id, and audit code
  - [x] Assert `targetTenantRef: null` maps to `cross_tenant`
- [x] Keep implementation aligned with existing break-glass contracts and avoid duplicate enforcement paths
- [x] Execute contract tests and full solution tests before handoff

## Previous Story Intelligence

- Story 5.3 established the CI pattern: contract-test failures must be explicit, non-zero, and easy to diagnose.
- Story 5.3 reinforced keeping changes narrow and evidence-driven (tests + clear output paths).
- For Story 5.4, prefer adding precise assertions over broad test-suite restructuring.

## Git Intelligence Summary

Recent patterns indicate incremental, test-first compliance delivery:

- `932c936`: integrated contract tests into CI with explicit failure signaling and canonical commands.
- `5285339`: introduced reference compliance tests for refusal/attribution, establishing expected style for Epic 5 proof stories.
- Recent README-focused commits indicate documentation is actively curated; only update docs here if Story 5.4 introduces materially new verification steps.

## Latest Technical Information

- .NET support policy currently lists .NET 10 as STS with end-of-support on **November 10, 2026**; keep CI pinned to `10.0.x` for this story.
- `xunit` package feed shows newer 2.x releases beyond current repo baseline (`2.5.3`), but upgrading test framework versions is out-of-scope for Story 5.4 unless a concrete compatibility issue appears.
- `FluentAssertions` package feed indicates newer major versions are available; retain current repo version to avoid incidental assertion-behavior drift during compliance proof work.
- GitHub Actions official actions have newer major releases available (`actions/checkout@v5`, `actions/setup-dotnet@v5`), but Story 5.4 should not include CI-action upgrades because this story is break-glass compliance focused.

### Project Structure Notes

- Keep Epic 5 compliance work concentrated in `TenantSaas.ContractTests`.
- Reuse existing enforcement and abstraction layers instead of introducing parallel break-glass pathways.
- Preserve current repo conventions: flat structure, deterministic tests, explicit invariant references.

## Project Context Reference

- Never bypass tenant context or invariant guards.
- Never weaken refusal semantics or invariant naming stability.
- Never introduce ambiguous/implicit break-glass behavior.
- Never emit non-UTC timestamps for audit-event payloads.
- Keep changes minimal, auditable, and strongly correlated to acceptance criteria.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.4: Assert Break-Glass Constraints and Audit Event Emission]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.4: Define Break-Glass Contract and Standard Audit Event Schema]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.5: Require Explicit Break-Glass with Audit Event Emission]
- [Source: _bmad-output/planning-artifacts/architecture.md#Core Architectural Decisions]
- [Source: _bmad-output/project-context.md#Critical Don’t-Miss Rules]
- [Source: TenantSaas.Core/Enforcement/BoundaryGuard.cs]
- [Source: TenantSaas.Abstractions/BreakGlass/BreakGlassAuditEvent.cs]
- [Source: TenantSaas.Abstractions/BreakGlass/IBreakGlassAuditSink.cs]
- [Source: TenantSaas.ContractTests/Enforcement/BreakGlassEnforcementTests.cs]
- [Source: TenantSaas.ContractTests/BreakGlassContractTests.cs]
- [Source: TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs]
- [Source: _bmad-output/implementation-artifacts/5-3-integrate-contract-tests-into-ci.md]
- [External: .NET and .NET Core official support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [External: xunit package versions](https://www.nuget.org/packages/xunit)
- [External: FluentAssertions package versions](https://www.nuget.org/packages/FluentAssertions)
- [External: actions/setup-dotnet repository](https://github.com/actions/setup-dotnet)
- [External: actions/checkout repository](https://github.com/actions/checkout)

## Story Completion Status

- Status set to **done**
- Completion note: Code review remediation completed; compliance tests executed on 2026-02-12.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow execution (YOLO mode)
- artifact discovery: epics, PRD, architecture, project-context, previous story, git history
- latest-technology check: .NET support policy, package feeds, GitHub Actions repositories

### Implementation Plan

- Add reference compliance tests covering break-glass refusal Problem Details invariants and trace correlation fields.
- Add audit emission tests verifying required fields and cross-tenant marker behavior via audit sink capture.
- Run contract tests and full solution tests to validate compliance.

### Completion Notes List

- Parsed first backlog story from sprint tracking (`5-4-assert-break-glass-constraints-and-audit-event-emission`).
- Consolidated epic intent, architecture constraints, and project-context rules into implementation guardrails.
- Added explicit anti-regression guidance to prevent duplicate break-glass enforcement implementations.
- Added concrete test execution commands aligned with CI enforcement path.
- Prepared story for direct handoff to `dev-story` with status `ready-for-dev`.
- Added reference compliance tests for break-glass refusal Problem Details and audit event emission.
- Verified cross-tenant marker behavior and audit field requirements via captured audit sink events.
- Strengthened break-glass refusal assertions to validate refusal mappings and deterministic Problem Details shape.
- Replaced mock audit sink with a capture sink to align with contract-test guidance.
- Documented tool-state changes in `.codex/*` as out-of-scope for story review.
- Tests run (2026-02-12): `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`; `dotnet test TenantSaas.sln --disable-build-servers -v minimal`.

### File List

- TenantSaas.ContractTests/ReferenceComplianceBreakGlassTests.cs
- _bmad-output/implementation-artifacts/5-4-assert-break-glass-constraints-and-audit-event-emission.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

## Change Log

- 2026-02-11: Added break-glass reference compliance tests for refusals and audit emission; updated story status.
- 2026-02-12: Strengthened break-glass compliance assertions, replaced mock audit sink, ran contract/solution tests, marked story done.
