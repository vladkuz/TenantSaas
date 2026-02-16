# Story 7.2: Support Safe Cross-Tenant Administrative Workflows

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an on-call responder,
I want cross-tenant workflows to be explicit and constrained,
so that urgent fixes do not weaken tenant isolation guarantees.

## Acceptance Criteria

1. Given a cross-tenant administrative operation is required
   When it is executed without explicit break-glass and scope declaration
   Then enforcement refuses the operation
   And the refusal references the appropriate invariants
   And this is verified by a test
2. Given a cross-tenant operation is explicitly authorized
   When it proceeds
   Then audit events capture cross-tenant markers and scope details
   And disclosure policy is enforced for errors and logs
   And this is verified by a test
3. Given cross-tenant admin workflows are attempted without break-glass
   When enforcement evaluates the operation
   Then the operation must refuse with an error

## Tasks / Subtasks

- [x] Define cross-tenant administrative workflow contract with explicit preconditions (AC: 1)
  - [x] Require declared scope + break-glass declaration before workflow execution
  - [x] Ensure default path refuses when declaration is missing or incomplete
- [x] Implement/extend enforcement + audit behavior for authorized cross-tenant flows (AC: 2)
  - [x] Capture cross-tenant marker and scope details in audit events
  - [x] Apply disclosure policy consistently in both errors and logs
- [x] Add compliance tests for unauthorized and authorized cross-tenant workflows (AC: 1, 2, 3)
  - [x] Validate refusal invariant on missing break-glass
  - [x] Validate emitted audit payload and disclosure-safe logging on authorized path

## Developer Context

- This is a post-MVP hardening story extending existing trust-contract guardrails.
- Reuse existing abstractions/core enforcement modules; do not introduce parallel or bypass code paths.
- Keep refusal behavior machine-stable (`ProblemDetails` + `invariant_code` + trace correlation).

## Technical Requirements

- Maintain strict boundary-only enforcement model from previous epics.
- Preserve disclosure-safe `tenant_ref` semantics (`unknown`, `sensitive`, `cross_tenant`, opaque tenant id).
- Enforce deterministic outcomes across execution kinds and repeated runs.
- Do not relax break-glass requirements (explicit actor, reason, scope, auditable event).

## Architecture Compliance

- Keep core storage-agnostic; adapter behavior remains in sanctioned adapter seams only.
- Respect current module boundaries: `TenantSaas.Abstractions`, `TenantSaas.Core`, `TenantSaas.ContractTests`, `TenantSaas.Sample`.
- Keep API/error/logging conventions unchanged unless explicitly required by ACs.

## Library & Framework Requirements

- Keep `net10.0` baseline unchanged.
- Continue xUnit-based contract tests; ensure compatibility with current repo tooling.
- No new dependency introduction unless technically required and justified in story implementation notes.

## File Structure Requirements

- Core contracts/types: `TenantSaas.Abstractions/*`
- Enforcement/runtime behavior: `TenantSaas.Core/*`
- Compliance tests: `TenantSaas.ContractTests/*` (flat test structure, no nested test folders)
- Documentation updates (if needed by AC traceability): `docs/trust-contract.md`, `docs/error-catalog.md`, or related docs

## Testing Requirements

- Add focused contract tests proving both positive and refusal scenarios per ACs.
- Assert full Problem Details shape + stable `invariant_code` + `trace_id` (+ `request_id` for request flows).
- Assert required structured log fields including disclosure-safe `tenant_ref` values.
- Validate with:
  - `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal`
  - `dotnet test TenantSaas.sln --disable-build-servers -v minimal`

## Previous Story Intelligence

- Story 6.7 reinforced documentation-as-contract and strict validation tests; apply the same “tests enforce docs/contract semantics” pattern here.
- Recent Epic 6 flow consistently used contract tests first, then docs alignment; preserve that sequencing.

## Git Intelligence Summary

- Recent commits show emphasis on docs + contract-test enforcement (`feat: Implement API reference documentation...`, `feat: Publish verification guide...`).
- Preserve established naming, module boundaries, and CI-first validation approach used in latest commits.

## Latest Technical Information

- .NET 10 is active LTS with latest patch `10.0.2` (patch date: 2026-01-13); keep `net10.0` pinned for this story set.
- Use currently adopted xUnit line in-repo; if upgrading, validate adapter/framework compatibility with .NET 10 before changing package versions.

## Project Context Reference

- Follow `_bmad-output/project-context.md` rules: no persistence outside repositories, no non-ProblemDetails errors, required structured logging fields, no ambiguous route params, no implicit fallback behavior.

## Story Completion Status

- Status set to **review**
- Completion note: Story 7.2 implementation completed with cross-tenant workflow enforcement and contract tests.

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex)

### Debug Log References

- create-story workflow (YOLO)
- artifact discovery: epics, prd, architecture, project-context
- git intelligence: recent commit analysis
- latest-technology check: .NET support policy and xUnit release notes
- dev-story workflow (YOLO)
- implementation: cross-tenant admin workflow guard + audit marker enforcement
- validation:
  - dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal
  - dotnet test TenantSaas.sln --disable-build-servers -v minimal

### Implementation Plan

- Added `RequireCrossTenantAdministrativeWorkflowAsync` to `IBoundaryGuard` for explicit operation scope + break-glass preconditions.
- Enforced cross-tenant workflow preconditions in `BoundaryGuard`:
  - refuse missing scope declaration with `SharedSystemOperationAllowed`
  - require allowlisted shared-system operation bound to `BreakGlassExplicitAndAudited`
  - require valid break-glass declaration before workflow success
- Extended break-glass internals to force disclosure-safe `tenant_ref=cross_tenant` for cross-tenant workflow logs and audit events.
- Extended `BreakGlassAuditHelper` to support `operationName` and `tenantRef` override propagation for audit payload fidelity.
- Added contract tests for refusal path and authorized path payload/log assertions.

### Completion Notes List

- Implemented explicit cross-tenant administrative workflow contract in `IBoundaryGuard`/`BoundaryGuard`.
- Added enforcement precondition that workflow scope declaration (`operationName`) is mandatory and allowlisted.
- Enforced break-glass validation on cross-tenant workflow path with stable invariant refusals.
- Ensured authorized cross-tenant workflow audit/log path always emits disclosure-safe `tenant_ref=cross_tenant`.
- Captured workflow scope in audit payload via `operationName`.
- Added compliance tests for unauthorized and authorized cross-tenant workflows.
- Verified full contract test suite and full solution test suite pass.

### File List

- _bmad-output/implementation-artifacts/7-2-support-safe-cross-tenant-administrative-workflows.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- TenantSaas.Core/Enforcement/IBoundaryGuard.cs
- TenantSaas.Core/Enforcement/BoundaryGuard.cs
- TenantSaas.Core/Enforcement/BreakGlassAuditHelper.cs
- TenantSaas.ContractTests/CrossTenantAdministrativeWorkflowTests.cs (new file)
- TenantSaas.ContractTests/TestUtilities/CaptureAuditSink.cs (new file, extracted shared test utility)
- TenantSaas.ContractTests/ReferenceComplianceBreakGlassTests.cs (updated to use shared CaptureAuditSink)
- docs/trust-contract.md (updated with cross-tenant admin operations documentation)

- 2026-02-16: Code review completed with 10 findings (3 HIGH, 4 MEDIUM, 3 LOW); all issues remediated automatically; story set to done.

### Code Review Remediation (2026-02-16)

**Findings Addressed:**

1. ✅ **HIGH: Code Duplication** - Extracted `CaptureAuditSink` to `TestUtilities/CaptureAuditSink.cs` for reuse across test files.

2. ✅ **HIGH: Test Coverage Gaps** - Added 3 new tests:
   - `RequireCrossTenantAdministrativeWorkflow_NonSharedSystemScope_RefusesWithTenantScopeRequired`
   - `RequireCrossTenantAdministrativeWorkflow_InvalidOperationName_RefusesWithSharedSystemOperationAllowed`
   - `RequireCrossTenantAdministrativeWorkflow_TargetTenantRefDoesNotLeakInAudit_EvenWhenPresent`

3. ✅ **HIGH: Documentation Gap** - Updated `docs/trust-contract.md` with complete cross-tenant administrative workflow documentation including:
   - Cross-tenant workflow enforcement rules
   - Allowlisted operation names (`cross-tenant-admin-read`, `cross-tenant-admin-write`)
   - Usage examples and disclosure-safe audit trail requirements

4. ✅ **MEDIUM: Git Status Mismatch** - Staged new test files properly.

5. ✅ **MEDIUM: Error Message Clarity** - Improved error message in `BoundaryGuard.cs` to mention both operation scope and break-glass requirements.

6. ✅ **MEDIUM: Inline Test Utilities** - Moved test helpers to shared `TestUtilities/` directory.

7. ✅ **MEDIUM: DRY Violation** - Removed duplicate `CaptureAuditSink` implementation from `ReferenceComplianceBreakGlassTests.cs`.

8. ✅ **LOW: Test Name Ambiguity** - Renamed test to `RequireCrossTenantAdministrativeWorkflow_EmptyOperationName_RefusesWithStableInvariant` for clarity.

9. ✅ **LOW: Untested Declaration Scope** - Added explicit test for `targetTenantRef` disclosure safety.

10. ✅ **LOW: Edge Case Coverage** - Enhanced test assertions for disclosure policy enforcement.

**Test Results:**
- All 456 tests passing (3 new tests added)
- Full solution test suite verified
- All code quality issues resolved
### Change Log

- 2026-02-16: Implemented Story 7.2 cross-tenant administrative workflow contract, enforcement/audit behavior, and compliance tests; set story to review.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.2: Support Safe Cross-Tenant Administrative Workflows]
- [Source: _bmad-output/planning-artifacts/architecture.md#Implementation Patterns & Consistency Rules]
- [Source: _bmad-output/project-context.md#Critical Implementation Rules]
- [External: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [External: xUnit releases](https://xunit.net/releases/v3/3.1.0)
