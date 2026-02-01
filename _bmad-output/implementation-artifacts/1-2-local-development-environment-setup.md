# Story 1.2: Local Development Environment Setup

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want a minimal local setup process,
so that I can build and run the reference host without tribal knowledge.

## Acceptance Criteria

1. **Given** a new machine with the documented prerequisites
   **When** I follow the setup steps
   **Then** I can build and run the reference host locally
   **And** the reference host starts and responds to a basic health check
   **And** this is verified by a test

2. **Given** a missing prerequisite
   **When** I run the setup steps
   **Then** the failure is explicit and points to the missing dependency
   **And** this is verified by a test

## Tasks / Subtasks

- [x] Update README.md with comprehensive local setup section (AC: #1, #2)
  - [x] Add Prerequisites section (exact .NET SDK version, optional tools)
  - [x] Add Local Setup section with step-by-step commands
  - [x] Add Verification section with health check instructions
  - [x] Add Troubleshooting section with common failures and resolutions
- [x] Add health check endpoint to TenantSaas.Sample (AC: #1)
  - [x] Add `/health` endpoint using Minimal API
  - [x] Return 200 OK with basic status payload
  - [x] Keep it simple - no dependencies or external checks
- [x] Add contract tests for setup verification (AC: #1, #2)
  - [x] Test that README.md includes Prerequisites section
  - [x] Test that README.md includes Local Setup section
  - [x] Test that README.md includes Verification section
  - [x] Test that README.md includes Troubleshooting section with common failures
  - [x] Test that health endpoint returns 200 OK
  - [x] Use xUnit + FluentAssertions

## Dev Notes

### Project Structure Notes

This story extends the initialization from Story 1.1 by adding local development setup documentation and basic health check capability. Key alignment:

- **Minimal API health endpoint** fits the architecture's REST Minimal API pattern
- **Documentation in README.md** follows the architecture's docs-first posture
- **Contract tests** verify documentation completeness per NFR8/NFR11 requirements
- **No new projects** - only updates to existing TenantSaas.Sample and TenantSaas.ContractTests

### Learnings from Story 1.1

Based on the previous story (1.1):

- **Use xUnit + FluentAssertions** for all tests (established pattern)
- **README.md is the primary setup guide** - continue using it for developer docs
- **Tests verify documentation content** - use file reading + assertions on required sections
- **Keep changes minimal** - focus on setup + verification, no domain logic
- **Deterministic verification** - tests must run without external dependencies

### Critical Implementation Notes

**Health Endpoint Requirements:**
- Path: `/health` (lowercase, simple)
- Method: GET
- Response: 200 OK with JSON payload
- Payload format: `{ "status": "healthy" }` (camelCase per architecture)
- No authentication required (public health check)
- No dependencies on database, cache, or external services yet

**README.md Structure:**
The README.md already exists from Story 1.1. This story ADDS new sections without removing existing initialization content:

1. Prerequisites (exact SDK version from global.json)
2. Local Setup (clone, restore, build, run commands)
3. Verification (how to test health endpoint)
4. Troubleshooting (common failures + resolutions)

**Documentation Requirements Per Architecture:**
- Commands must be copy-pastable
- Prerequisites must be explicit (no assumptions)
- Failure guidance must be actionable
- Use standard .NET CLI commands (`dotnet restore`, `dotnet build`, `dotnet run`)

**Testing Strategy:**
- Contract tests verify README sections exist and contain required content
- Contract tests verify health endpoint returns 200 + correct payload
- Tests must NOT require external services or network calls
- Use `File.ReadAllText` + FluentAssertions for doc tests
- Use `HttpClient` + FluentAssertions for health endpoint test (or direct endpoint call)

### Technical Requirements

**Target Framework:**
- All projects: net10.0 (already set in Directory.Build.props from Story 1.1)

**Health Endpoint Pattern:**
```csharp
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
```

**README.md Additions:**
- Must follow existing README structure/tone from Story 1.1
- Add sections AFTER initialization content
- Use clear headings: `## Prerequisites`, `## Local Setup`, etc.
- Keep commands explicit and copy-pastable

**Contract Test Pattern:**
```csharp
[Fact]
public void ReadmeShouldIncludePrerequisitesSection()
{
    var readme = File.ReadAllText("../../../README.md");
    readme.Should().Contain("## Prerequisites");
    readme.Should().Contain(".NET");
}
```

### Architecture Compliance

**Alignment with Architecture Decisions:**

1. **REST Minimal API** (Architecture § API & Communication Patterns)
   - Health endpoint uses Minimal API pattern: `app.MapGet(...)`
   - No controllers introduced
   - Simple JSON response

2. **API Response Format** (Architecture § Format Patterns)
   - Success returns direct payload (no envelope)
   - JSON fields are camelCase: `{ "status": "healthy" }`
   - No Problem Details for success case

3. **Error Handling** (Architecture § Process Patterns)
   - If health check fails later, use Problem Details
   - For this story, health check is always successful (no deps)

4. **Documentation Posture** (Architecture § Infrastructure & Deployment)
   - Docs-first: README.md is primary setup guide
   - Explicit prerequisites and troubleshooting

5. **Testing Standards** (Architecture § Implementation Patterns)
   - xUnit + FluentAssertions
   - Contract tests verify docs AND behavior
   - Deterministic, no external deps

### Library / Framework Requirements

**No New Dependencies:**
- Health endpoint uses built-in ASP.NET Core Minimal API
- No health check libraries (Microsoft.Extensions.Diagnostics.HealthChecks) yet
- Contract tests use existing xUnit + FluentAssertions from Story 1.1

**Existing Dependencies (from Story 1.1):**
- TenantSaas.Sample: ASP.NET Core 10 Minimal API (from template)
- TenantSaas.ContractTests: xUnit + FluentAssertions

**Future Dependencies (NOT this story):**
- Swashbuckle (will be added later, architecture pins 10.1.0)
- OpenTelemetry (optional, pins 1.15.0)

### File Structure Requirements

**Files to Modify:**
1. `README.md` - Add setup sections
2. `TenantSaas.Sample/Program.cs` - Add health endpoint
3. `TenantSaas.ContractTests/ReadmeSetupTests.cs` - NEW test file for README verification
4. `TenantSaas.ContractTests/HealthEndpointTests.cs` - NEW test file for health endpoint verification

**Project Structure (unchanged):**
```
TenantSaas/
├── README.md (update)
├── TenantSaas.Sample/
│   └── Program.cs (update)
├── TenantSaas.ContractTests/
│   ├── ReadmeSetupTests.cs (new)
│   └── HealthEndpointTests.cs (new)
```

### Testing Requirements

**Test Categories:**
1. Documentation tests (verify README.md content)
2. Behavior tests (verify health endpoint)

**Documentation Tests (ReadmeSetupTests.cs):**
- Test that Prerequisites section exists
- Test that Local Setup section exists
- Test that Verification section exists
- Test that Troubleshooting section exists and mentions common failures

**Behavior Tests (HealthEndpointTests.cs):**
- Test that `/health` endpoint returns 200 OK
- Test that response payload matches expected shape
- Use WebApplicationFactory or direct endpoint call

**Testing Best Practices:**
- Tests must run in strict mode (FluentAssertions)
- Tests must be deterministic
- No test subfolders - all tests in TenantSaas.ContractTests root
- Test file names match pattern: `*Tests.cs`

### Latest Tech Information

**Health Check Patterns in ASP.NET Core 10:**
- ASP.NET Core 10 supports native health checks via `Microsoft.Extensions.Diagnostics.HealthChecks`
- For this story, use simple Minimal API endpoint (no library yet)
- Standard health check response: `{ "status": "healthy" }` or similar
- HTTP 200 = healthy, 503 = unhealthy (future consideration)

**Testing Approaches:**
- For Minimal API testing, use `WebApplicationFactory<TEntryPoint>` (standard pattern)
- Alternative: use `HttpClient` against running service (less isolated)
- Prefer factory-based testing for contract tests (no external process needed)

**Documentation Best Practices:**
- Clear prerequisites (exact versions, installation links)
- Step-by-step commands (idempotent where possible)
- Verification step (how to know it worked)
- Troubleshooting (common failures + fixes)

**External References:**
- ASP.NET Core Health Checks: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
- Minimal APIs: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- WebApplicationFactory: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

### References

- [Architecture: API & Communication Patterns](_bmad-output/planning-artifacts/architecture.md#api--communication-patterns)
- [Architecture: Format Patterns](_bmad-output/planning-artifacts/architecture.md#format-patterns)
- [Architecture: Testing Standards](_bmad-output/planning-artifacts/architecture.md#implementation-patterns--consistency-rules)
- [Architecture: Project Structure](_bmad-output/planning-artifacts/architecture.md#project-structure--boundaries)
- [PRD: Developer Tool Requirements](/_bmad-output/planning-artifacts/prd.md#developer-tool-specific-requirements)
- [PRD: NFR8 - Contract tests pass across local and CI](/_bmad-output/planning-artifacts/prd.md#nonfunctional-requirements)
- [PRD: NFR11 - Integration guide completes in <=30 minutes](/_bmad-output/planning-artifacts/prd.md#nonfunctional-requirements)
- [Story 1.1: Project initialization and README.md baseline](1-1-initialize-project-from-the-approved-net-template.md)
- [Epics: Epic 1, Story 1.2 requirements](_bmad-output/planning-artifacts/epics.md#story-12-local-development-environment-setup)

### Project Context Reference

**CRITICAL: Read project-context.md before implementing**

Location: `_bmad-output/project-context.md`

Key rules for this story:
- Use PascalCase for types/methods, camelCase for locals/fields
- No underscore prefixes anywhere
- JSON fields are camelCase
- Problem Details for errors (not needed for health check success)
- Structured logs not required for basic health endpoint yet
- Tests use xUnit + FluentAssertions
- Keep flat repo structure (no test subfolders)

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- dotnet test TenantSaas.sln (required escalated permissions for MSBuild named-pipe access)

### Completion Notes List

- ✅ Added local setup, verification, and troubleshooting sections to README with explicit SDK 10.0.102 guidance.
- ✅ Added `/health` endpoint returning `{ "status": "healthy" }` via Minimal API with test-friendly environment control.
- ✅ Updated README verification to follow HTTPS redirect and consolidated troubleshooting guidance.
- ✅ Health endpoint contract test now starts the host via WebApplicationFactory for a real HTTP response.
- ✅ Added ASP.NET Core testing package to support in-memory host startup.

### File List

- .codex/auth.json
- .codex/history.jsonl
- .codex/log/codex-tui.log
- .codex/models_cache.json
- .codex/rules/default.rules
- .codex/sessions/2026/01/30/rollout-2026-01-30T10-58-49-019c1045-b281-7f51-b41f-7ab695c27b1c.jsonl
- .codex/sessions/2026/01/30/rollout-2026-01-30T12-53-18-019c10ae-860b-7f10-be75-29d11f845b36.jsonl
- README.md
- TenantSaas.Sample/Program.cs
- TenantSaas.ContractTests/ReadmeSetupTests.cs
- TenantSaas.ContractTests/HealthEndpointTests.cs
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj
- _bmad-output/implementation-artifacts/1-2-local-development-environment-setup.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-01-30: Added local setup documentation, health endpoint, and contract tests for setup verification.
- 2026-01-31: Switched health test to WebApplicationFactory, disabled HTTPS redirection in test env, and clarified README verification/troubleshooting.
