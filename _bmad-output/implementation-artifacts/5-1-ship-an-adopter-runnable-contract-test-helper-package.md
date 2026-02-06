# Story 5.1: Ship an Adopter-Runnable Contract Test Helper Package

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want contract test helpers I can run in my own CI,
so that compliance is a binary signal rather than a review process.

## Acceptance Criteria

1. **Given** the contract test helper package
   **When** I add it to my test suite
   **Then** I can run black-box assertions without specialized tooling
   **And** the helpers align with the trust contract invariants and refusal mappings
2. **Given** helper APIs are used
   **When** versions change within a major version
   **Then** helper contracts remain stable
   **And** breaking changes require a major version bump with migration notes
   **And** this is verified by a test

## Developer Context

- This story delivers the **adopter-facing contract test helper package**; it must be usable without internal test utilities.
- Helpers must be **black-box** and rely only on public contracts and stable surface area.
- Avoid reinventing existing tests: extract or wrap reusable logic from `TenantSaas.ContractTests` where feasible.
- Preserve deterministic behavior and refusal semantics; never bypass invariants or tenant context rules.

## Technical Requirements

- Target `net10.0` for all new code.
- Use xUnit for tests; FluentAssertions for assertions; Moq only when needed.
- Strict test mode everywhere; failures must be explicit and stable.
- All async APIs must accept `CancellationToken` parameters.

## Architecture Compliance

- Keep the core storage-agnostic; the helper package must not introduce EF Core or sample-host dependencies.
- Use only public contracts in `TenantSaas.Abstractions` (and stable public APIs in `TenantSaas.Core` if explicitly required).
- Do not create new entry points that bypass the enforcement boundary.
- Preserve RFC 7807 Problem Details handling and `invariant_code` semantics in any helper assertions.

## Library & Framework Requirements

- **.NET 10** is required; helper package must not target earlier runtimes.
- **EF Core 10** is not required for this helper; avoid adding EF Core dependencies.
- **Swashbuckle** and **OpenTelemetry** are out of scope for this helper unless explicitly required by contract test assertions.
- No new dependencies without explicit justification and documentation.

## File Structure Requirements

- New helper package should live at repo root (no `src/` or `tests/` folders).
- Add new project to `TenantSaas.sln` and any shared build config as needed.
- All tests remain under `TenantSaas.ContractTests/` (no test subfolders).

## Testing Requirements

- Add tests that prove helper API stability within a major version.
- Add tests that verify helper assertions align with trust contract invariants and refusal mappings.
- Maintain existing test organization and naming conventions.

## Tasks / Subtasks

- [x] Define and document the helper package scope and public surface (AC: 1, 2)
  - [x] Identify reusable assertions/fixtures from existing contract tests and extract into a dedicated package
  - [x] Specify the minimal inputs adopters must provide (e.g., app factory, configuration, allowed execution kinds)
- [x] Create the helper package project and wire it into the solution (AC: 1)
  - [x] New class library project under repo root (no `src/`), target `net10.0`
  - [x] Add to `TenantSaas.sln` and shared build props/targets if needed
- [x] Implement helper APIs for black-box contract assertions (AC: 1)
  - [x] Provide xUnit-friendly fixtures/assertions for refusal behavior, attribution rules, and disclosure policy
  - [x] Ensure no dependency on EF Core or sample host internals
- [x] Add stability/compatibility tests for the helper APIs (AC: 2)
  - [x] Add contract tests that fail on breaking API changes within the same major version
- [x] Update documentation for adopter usage (AC: 1, 2)
  - [x] Document package name, installation, and minimal setup steps
  - [x] Provide a short example of running the helpers in CI

## Dev Notes

- **Primary goal:** ship a reusable helper package adopters can use to run contract tests in their CI without relying on internal test utilities.
- **Do not** introduce new dependencies without explicit justification; prefer existing test stack (xUnit + FluentAssertions + Moq).
- Helpers must be **black-box**: do not reach into internal implementation types; use public contracts only.
- Keep public API surface small, stable, and versioned; breaking changes require a major version bump and migration notes.
- Ensure helper APIs align with trust contract invariants and refusal mappings.

## Latest Technical Information

- .NET 10 is an LTS release. Official support policy lists the original release date as **November 11, 2025**, latest patch **10.0.2 (January 13, 2026)**, with support through **November 14, 2028**.  
- EF Core 10 is an LTS release (November 2025) and **requires** the .NET 10 SDK/runtime; it does **not** run on earlier .NET versions or .NET Framework.  
- Swashbuckle.AspNetCore **v10.1.0** was released **December 12, 2025**; it adds public `SchemaRepository.ReplaceSchemaId` and includes schema-generation fixes.  

### Project Structure Notes

- Keep flat repo structure (no `src/` or `tests/` folders).
- New helper package should live at repo root alongside existing projects (e.g., `TenantSaas.ContractTestKit/`).
- Contract tests remain under `TenantSaas.ContractTests/` only.

## Project Context Reference

- Enforce tenant scoping and invariants through boundaries only; never bypass tenant context or guards.
- Errors must be RFC 7807 Problem Details with `invariant_code` and structured logging fields.
- JSON is `camelCase`; timestamps are UTC with `Z`.
- No new dependencies without explicit justification.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.1: Ship an Adopter-Runnable Contract Test Helper Package]
- [Source: _bmad-output/planning-artifacts/architecture.md#Testing Framework]
- [Source: _bmad-output/planning-artifacts/architecture.md#Project Organization]
- [Source: _bmad-output/planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: _bmad-output/project-context.md#Critical Implementation Rules]
- [Source: _bmad-output/project-context.md#Testing Rules]
- [External: .NET support policy (dotnet.microsoft.com)]
- [External: EF Core 10 What's New (learn.microsoft.com)]
- [External: Swashbuckle.AspNetCore v10.1.0 release (github.com)]

## Story Completion Status

- Status set to **done**
- Completion note: All tasks complete - helper package shipped with full test coverage, code review fixes applied

## Dev Agent Record

### Agent Model Used

Claude Opus 4.5 (via Codex)

### Debug Log References

- Build: 0 warnings, 0 errors
- Tests: 391 passed, 0 failed

### Completion Notes List

- Ultimate context engine analysis completed - comprehensive developer guide created
- Created TenantSaas.ContractTestKit package with black-box assertions for invariants, refusal mappings, disclosure policy, and attribution rules
- Added TrustContractFixture for xUnit integration
- Added ProblemDetailsAssertions for HTTP response validation
- Added HttpResponseExtensions for convenient Problem Details extraction
- Added 17 API stability and functional tests
- All 388 contract tests pass with no regressions
- **Code Review Fixes Applied:**
  - Added API stability tests for TrustContractFixture, ContractTestKitOptions, and HttpResponseExtensions (+3 tests)
  - Removed unused ExpectedContractVersion dead code from ContractTestKitOptions
  - Fixed README example to properly declare WebApplicationFactory fixture

### File List

- TenantSaas.ContractTestKit/TenantSaas.ContractTestKit.csproj (new)
- TenantSaas.ContractTestKit/ContractTestKitOptions.cs (new)
- TenantSaas.ContractTestKit/TrustContractFixture.cs (new)
- TenantSaas.ContractTestKit/README.md (new)
- TenantSaas.ContractTestKit/Assertions/InvariantAssertions.cs (new)
- TenantSaas.ContractTestKit/Assertions/RefusalMappingAssertions.cs (new)
- TenantSaas.ContractTestKit/Assertions/ProblemDetailsAssertions.cs (new)
- TenantSaas.ContractTestKit/Assertions/DisclosureAssertions.cs (new)
- TenantSaas.ContractTestKit/Assertions/AttributionAssertions.cs (new)
- TenantSaas.ContractTestKit/Extensions/HttpResponseExtensions.cs (new)
- TenantSaas.ContractTests/ContractTestKitApiStabilityTests.cs (new)
- TenantSaas.ContractTests/ContractTestKitFunctionalTests.cs (new)
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj (modified - added ContractTestKit reference)
- TenantSaas.sln (modified - added ContractTestKit project)
- _bmad-output/implementation-artifacts/sprint-status.yaml (modified)
- _bmad-output/implementation-artifacts/5-1-ship-an-adopter-runnable-contract-test-helper-package.md (modified)
