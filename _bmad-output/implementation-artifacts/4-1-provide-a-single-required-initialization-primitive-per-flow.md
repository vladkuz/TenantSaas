# Story 4.1: Provide a Single Required Initialization Primitive per Flow

Status: in-progress

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As an adopter,
I want a clear initialization primitive,
so that I know exactly where tenancy is established for each execution flow.

## Acceptance Criteria

1) **Given** a request, background job, admin task, or scripted flow  
   **When** I integrate the baseline  
   **Then** there is one required initialization primitive for that flow  
   **And** it produces a context object with scope, execution kind, and attribution inputs  
   **And** this is verified by a test

2) **Given** initialization is attempted multiple times in the same flow  
   **When** initialization runs  
   **Then** it behaves idempotently  
   **And** it does not create conflicting context state  
   **And** this is verified by a test

3) **Given** an execution flow without initialization  
   **When** a boundary is invoked  
   **Then** enforcement refuses with the ContextInitialized invariant  
   **And** the refusal is testable via contract tests  
   **And** this is verified by a test

4) **Given** a flow attempts boundary execution without initialization  
   **When** enforcement evaluates the request  
   **Then** the operation must refuse with an error

## Tasks / Subtasks

- [x] Define the single initialization primitive contract per execution flow
  - [x] Confirm or add a single entry point in `TenantSaas.Abstractions/Tenancy` for initialization inputs
  - [x] Ensure initialization captures: scope, execution kind, attribution inputs
  - [x] Ensure explicit-only usage works without ambient dependency
- [x] Implement idempotent initialization behavior in `TenantSaas.Core/Tenancy`
  - [x] Second initialization in the same flow must not mutate existing context
  - [x] Emit explicit refusal if conflicting inputs are attempted
- [x] Wire initialization at the boundary integration point in `TenantSaas.Sample/Middleware`
  - [x] Ensure all request paths initialize before enforcement
  - [x] Ensure background/admin/scripted flows have an explicit wrapper/entry point for initialization
- [x] Enforce ContextInitialized invariant at boundary helpers
  - [x] Refuse when no initialization is present
  - [x] Use Problem Details shape with `invariant_code`
- [x] Contract tests
  - [x] Add/extend tests in `TenantSaas.ContractTests` for initialization presence, idempotency, and refusal
  - [x] Verify refusal uses the correct invariant code and Problem Details shape
- [x] Documentation (minimal)
  - [x] Update trust-contract or integration guide only if the initialization primitive name/shape changes

## Developer Context

- This story is the first in Epic 4; it establishes the required initialization primitive used across all flows.
- Do not add new invariants or expand scope without updating the trust contract and references.
- Use existing patterns for refusal and Problem Details; avoid new error envelopes.

## Technical Requirements

- Initialization must be a single, required entry point per flow: request, background, admin, scripted.
- Initialization is idempotent within a flow; repeat calls must not create conflicting context state.
- Context includes scope, execution kind, and attribution inputs at initialization time.
- If enforcement runs without prior initialization, refusal must use the ContextInitialized invariant.

## Architecture Compliance

- Follow the .NET 10 stack and patterns; no new dependencies without explicit justification.
- Minimal APIs remain in `TenantSaas.Sample`; no controllers or MVC additions.
- Enforcement occurs only at boundary helpers/middleware/interceptors.
- Use Problem Details (RFC 7807) as the only error format.

## Library / Framework Requirements

- .NET target net10.0 across projects.
- EF Core is reference-only; no direct `DbContext` usage outside repositories.
- Swashbuckle and OpenTelemetry remain optional and sample-only; do not add new observability deps here.

## File Structure Requirements

- Keep flat repo structure (no `src/` or `tests/` folders).
- Abstractions live under `TenantSaas.Abstractions`; core logic under `TenantSaas.Core`.
- Contract tests live only in `TenantSaas.ContractTests`.

## Testing Requirements

- Use xUnit + FluentAssertions; Moq only where needed.
- Contract tests must assert refusal Problem Details + `invariant_code`.
- Avoid mocks in contract tests when feasible; prefer real EF Core + SQLite fixtures.

## Latest Technical Information

- .NET 10 is the current .NET release line and is the target stack for this repo. Stick to net10.0 and avoid version drift unless a deliberate upgrade is approved.
- EF Core 10 is the current stable major line for EF Core; keep EF Core 10.x alignment with .NET 10.
- Swashbuckle.AspNetCore latest stable is 10.1.1; project currently pins 10.1.0, so only upgrade if explicitly approved.
- OpenTelemetry.Extensions.Hosting and OpenTelemetry.Exporter.OpenTelemetryProtocol latest stable are 1.15.0; the architecture already specifies these versions.

## Project Context Reference

- Must follow `_bmad-output/project-context.md` rules (naming, async patterns, logging fields, Problem Details, tenant-scoping invariants).
- Never bypass tenant context or invariant guards; never use non-UTC timestamps.

## Story Completion Status

- Status set to **in-progress** (review remediation applied).
- Completion note: Review remediation applied; tests not re-run.

## Dev Notes

- Relevant architecture patterns and constraints
  - Use explicit route parameters and Problem Details for any request-path refusal.
  - Initialization is idempotent and side-effect-free (no persistence, no network calls).
  - Enforce tenant-scoped access; no operation without an initialized context.
- Source tree components to touch
  - `TenantSaas.Abstractions/Tenancy` for initialization contracts and context types.
  - `TenantSaas.Core/Tenancy` for initialization logic and context storage/access.
  - `TenantSaas.Core/Enforcement` for ContextInitialized invariant checks.
  - `TenantSaas.Sample/Middleware` to ensure the integration point invokes initialization.
  - `TenantSaas.ContractTests/Invariants` for contract test coverage.
- Testing standards summary
  - xUnit + FluentAssertions; contract tests prefer real fixtures over mocks.
  - Assertions must validate Problem Details + `invariant_code`.

### Project Structure Notes

- Alignment with unified project structure (paths, modules, naming)
  - Keep flat repo layout; no new `src/` or `tests/` directories.
  - No underscore prefixes in identifiers; standard .NET naming only.
  - Keep Sample host minimal; do not add controllers or new frameworks.
- Detected conflicts or variances (with rationale)
  - None expected; avoid introducing new dependencies for this story.

### References

- Epic 4, Story 4.1: `_bmad-output/planning-artifacts/epics.md` (Epic 4 / Story 4.1)
- Architecture patterns: `_bmad-output/planning-artifacts/architecture.md` (Implementation Patterns, Project Structure, Enforcement Guidelines)
- Project rules: `_bmad-output/project-context.md`

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5 (GitHub Copilot in VS Code)

### Debug Log References

### Completion Notes List

- Story created from epics + architecture + project context.
- Sprint status updated to in-progress for Epic 4 and ready-for-dev for Story 4.1.
- **Implementation complete:**
  - Created `ITenantContextInitializer` interface in TenantSaas.Abstractions/Tenancy
  - Implemented `TenantContextInitializer` with idempotent initialization in TenantSaas.Core/Tenancy
  - Updated `TenantContextMiddleware` to use ITenantContextInitializer instead of direct accessor manipulation
  - Registered ITenantContextInitializer as scoped service in Program.cs
  - Added comprehensive contract tests: TenantContextInitializerTests, InitializationEnforcementTests
  - Updated all existing middleware tests to use new signature (MiddlewareProblemDetailsTests, RefusalCorrelationTests)
  - Updated integration guide with Context Initialization section and initialization primitive documentation
  - Previously all 310 tests passing
  - Review remediation: capture attribution inputs in context, explicit accessor support, conflict refusal mapping, and middleware single-entry enforcement
  - Tests not re-run after review fixes

### File List

- `_bmad-output/implementation-artifacts/4-1-provide-a-single-required-initialization-primitive-per-flow.md`
- `TenantSaas.Abstractions/Tenancy/ITenantContextInitializer.cs` (created)
- `TenantSaas.Core/Tenancy/TenantContextInitializer.cs` (created)
- `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs` (modified)
- `TenantSaas.Sample/Program.cs` (modified)
- `TenantSaas.ContractTests/TenantContextInitializerTests.cs` (created)
- `TenantSaas.ContractTests/InitializationEnforcementTests.cs` (created)
- `TenantSaas.ContractTests/MiddlewareProblemDetailsTests.cs` (modified)
- `TenantSaas.ContractTests/Logging/RefusalCorrelationTests.cs` (modified)
- `docs/integration-guide.md` (modified)
- `TenantSaas.Abstractions/Tenancy/TenantAttributionInput.cs` (created)
- `TenantSaas.Abstractions/Tenancy/TenantAttributionInputs.cs` (created)
- `TenantSaas.Abstractions/Tenancy/ITenantContextInitializer.cs` (modified)
- `TenantSaas.Abstractions/Tenancy/TenantContext.cs` (modified)
- `TenantSaas.Core/Errors/ProblemDetailsFactory.cs` (modified)
- `TenantSaas.Core/Tenancy/ExplicitTenantContextAccessor.cs` (modified)
- `TenantSaas.Core/Tenancy/TenantContextConflictException.cs` (created)
- `TenantSaas.Core/Tenancy/TenantContextInitializer.cs` (modified)
- `TenantSaas.Sample/Middleware/TenantContextMiddleware.cs` (modified)
- `TenantSaas.ContractTests/InitializationEnforcementTests.cs` (modified)
- `TenantSaas.ContractTests/TenantContextInitializerTests.cs` (modified)
- `TenantSaas.ContractTests/MiddlewareProblemDetailsTests.cs` (modified)
