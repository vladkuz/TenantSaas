# Story 1.3: CI Pipeline Skeleton (Build + Smoke Checks)

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a team lead,
I want a CI workflow skeleton that builds and runs basic smoke checks,
so that the baseline can be validated on every pull request without waiting on later epics.

## Acceptance Criteria

1. **Given** the CI workflow definition
   **When** I open a pull request
   **Then** the build and smoke checks run automatically and fail the build on errors
   **And** this is verified by a test

2. **Given** a known failing test or build error is introduced
   **When** the CI pipeline runs
   **Then** the pipeline fails with a specific, documented error

## Tasks / Subtasks

- [x] Create GitHub Actions CI workflow file (AC: #1)
  - [x] Create `.github/workflows/ci.yml` file
  - [x] Configure workflow to trigger on pull requests and pushes to main
  - [x] Set up .NET 10 SDK environment (version from global.json)
  - [x] Add restore step (`dotnet restore`)
  - [x] Add build step (`dotnet build --no-restore`)
  - [x] Add test step (`dotnet test --no-build --verbosity minimal`)
  - [x] Ensure workflow fails on any step failure
- [x] Add contract tests to verify CI workflow structure (AC: #1, #2)
  - [x] Test that `.github/workflows/ci.yml` exists
  - [x] Test that ci.yml contains required steps (restore, build, test)
  - [x] Test that ci.yml triggers on pull_request and push events
  - [x] Test that ci.yml uses correct .NET SDK version
  - [x] Use xUnit + FluentAssertions
- [x] Document CI workflow in README.md (AC: #1, #2)
  - [x] Add CI/CD section to README
  - [x] Explain what the CI workflow does
  - [x] Document expected failure scenarios
  - [x] Add badge showing CI status (optional)

## Dev Notes

### Story Context

This story completes Epic 1 (Bootstrap a Runnable Baseline) by adding continuous integration. This ensures code quality gates are in place before any domain logic is implemented in later epics. The CI pipeline should be minimal but sufficient to catch build breaks and test failures.

### Key Requirements from Epics

From Epic 1, Story 1.3 acceptance criteria:
- CI workflow must run automatically on pull requests
- Build and smoke checks must fail the build on errors
- Failures must be specific and documented

From Architecture:
- CI defined at `.github/workflows/ci.yml`
- Standard GitHub Actions workflow
- Uses .NET SDK build pipeline
- Must be CI-ready for contract tests (NFR requirement)

### Learnings from Previous Stories

**From Story 1.1 (Project Initialization):**
- Use `global.json` for SDK version pinning (currently 10.0.102)
- Solution structure is flat with root-level projects
- All projects target net10.0 via Directory.Build.props
- Contract tests live in TenantSaas.ContractTests
- Use xUnit + FluentAssertions for all tests

**From Story 1.2 (Local Setup):**
- README.md is the primary documentation file
- Tests verify documentation completeness
- Use WebApplicationFactory for integration testing
- Health endpoint exists at `/health` for smoke testing

**Patterns Established:**
1. **Documentation verification pattern**: Read file content and assert required sections exist
2. **Test structure**: All tests in TenantSaas.ContractTests root (no subfolders)
3. **Test naming**: `*Tests.cs` pattern
4. **Assertions**: FluentAssertions with clear error messages

### Critical Implementation Notes

**CI Workflow Requirements:**

1. **File location**: `.github/workflows/ci.yml` (exact path per architecture)

2. **Trigger events**:
   - `pull_request` (any branch)
   - `push` to main branch

3. **Required steps**:
   - Checkout code
   - Setup .NET SDK (version from global.json: 10.0.102)
   - Restore dependencies (`dotnet restore`)
   - Build solution (`dotnet build --no-restore`)
   - Run tests (`dotnet test --no-build --verbosity minimal`)

4. **Failure behavior**:
   - Any step failure should stop the workflow
   - Exit code != 0 should mark the workflow as failed
   - GitHub Actions default behavior provides this

**Testing Strategy:**

The contract tests should verify the CI workflow file structure and content:

1. **File existence test**: Assert `.github/workflows/ci.yml` exists
2. **Trigger verification**: Assert workflow triggers on pull_request and push events
3. **SDK version test**: Assert workflow uses SDK version matching global.json
4. **Step verification**: Assert workflow contains restore, build, and test steps
5. **YAML parsing**: Use YAML parser (YamlDotNet or similar) to read and validate structure

**Documentation Requirements:**

Add a CI/CD section to README.md explaining:
- What the CI workflow does (build, test, validation)
- When it runs (on PRs and main branch commits)
- How to view CI results (GitHub Actions tab)
- Common failure scenarios and how to debug them
- (Optional) Badge showing CI status

### Technical Requirements

**Target Framework:**
- All projects: net10.0 (already set from Story 1.1)

**New Dependencies:**
- **TenantSaas.ContractTests**: YamlDotNet (for parsing ci.yml in tests)
  - Latest stable version (check NuGet for current release)
  - Used only in test project, not production code

**GitHub Actions Configuration:**

```yaml
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ '*' ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity minimal
```

**Contract Test Pattern (YAML Verification):**

```csharp
[Fact]
public void CiWorkflowShouldExist()
{
    var ciPath = ".github/workflows/ci.yml";
    File.Exists(ciPath).Should().BeTrue(
        $"CI workflow must exist at {ciPath}");
}

[Fact]
public void CiWorkflowShouldTriggerOnPullRequest()
{
    var ciYaml = File.ReadAllText(".github/workflows/ci.yml");
    var deserializer = new DeserializerBuilder().Build();
    var workflow = deserializer.Deserialize<Dictionary<string, object>>(ciYaml);
    
    var triggers = workflow["on"] as Dictionary<object, object>;
    triggers.Should().ContainKey("pull_request");
}
```

### Architecture Compliance

**Alignment with Architecture Decisions:**

1. **CI/CD Infrastructure** (Architecture § Infrastructure & Deployment)
   - GitHub Actions for build/test/pack
   - ci.yml at `.github/workflows/ci.yml`
   - Release workflow deferred (release.yml not in this story)

2. **Contract Tests as First-Class Artifacts** (Architecture § Core Decisions)
   - Contract tests must be CI-ready
   - Tests verify CI workflow structure
   - CI must run contract tests to validate compliance

3. **Standard SDK Build Pipeline** (Architecture § Starter Template)
   - Use `dotnet restore`, `dotnet build`, `dotnet test`
   - No custom build tooling
   - Standard CLI workflows

4. **Deterministic Verification** (Architecture § NFR Requirements)
   - NFR8: Contract tests pass across local and CI
   - NFR12: Bypassing integration points fails CI
   - CI workflow validates these requirements

5. **Project Structure** (Architecture § Project Structure)
   - `.github/workflows/` directory for CI definitions
   - Flat repo structure (already established)
   - No changes to project layout

### Library / Framework Requirements

**New Dependencies:**

1. **YamlDotNet** (for contract tests only)
   - Package: YamlDotNet
   - Purpose: Parse and validate ci.yml structure
   - Target: TenantSaas.ContractTests only
   - Latest stable version (13.x as of Jan 2026)
   - NuGet: https://www.nuget.org/packages/YamlDotNet

**Existing Dependencies (from previous stories):**
- xUnit (test framework)
- FluentAssertions (assertion library)
- Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)

**No Production Dependencies:**
- This story adds CI infrastructure only
- No new dependencies to TenantSaas.Core, TenantSaas.Sample, or TenantSaas.EfCore

### File Structure Requirements

**New Files:**
1. `.github/workflows/ci.yml` - CI workflow definition
2. `TenantSaas.ContractTests/CiWorkflowTests.cs` - Contract tests for CI workflow

**Modified Files:**
1. `README.md` - Add CI/CD section
2. `TenantSaas.ContractTests/TenantSaas.ContractTests.csproj` - Add YamlDotNet package

**Project Structure After This Story:**
```
TenantSaas/
├── .github/
│   └── workflows/
│       └── ci.yml (new)
├── README.md (update)
├── TenantSaas.ContractTests/
│   ├── CiWorkflowTests.cs (new)
│   ├── TenantSaas.ContractTests.csproj (update)
│   ├── HealthEndpointTests.cs (existing)
│   ├── ReadmeInitializationTests.cs (existing)
│   └── ReadmeSetupTests.cs (existing)
```

### Testing Requirements

**Test Categories:**
1. CI workflow structure tests (verify ci.yml)
2. Documentation tests (verify README CI section)

**CI Workflow Tests (CiWorkflowTests.cs):**
- Test that `.github/workflows/ci.yml` exists
- Test that workflow triggers on `pull_request`
- Test that workflow triggers on `push` to main
- Test that workflow uses .NET setup action
- Test that workflow contains restore step
- Test that workflow contains build step with --no-restore flag
- Test that workflow contains test step with --no-build flag
- Test that workflow uses ubuntu-latest runner
- (Optional) Test SDK version matches global.json

**README Tests (extend ReadmeSetupTests.cs or new file):**
- Test that README contains "CI/CD" or "Continuous Integration" section
- Test that README explains what CI workflow does
- Test that README mentions GitHub Actions

**Testing Best Practices:**
- All tests in TenantSaas.ContractTests root (no subfolders)
- Use descriptive test names
- Use FluentAssertions for clear failure messages
- Tests must be deterministic (no external dependencies)
- Tests should run fast (file reading + YAML parsing)

**Running Tests:**
- Local: `dotnet test TenantSaas.sln`
- CI: `dotnet test --no-build --configuration Release --verbosity minimal`

### Latest Tech Information

**GitHub Actions Latest Versions (as of Jan 2026):**

1. **actions/checkout@v4**
   - Current stable version
   - Used for checking out repository code
   - Docs: https://github.com/actions/checkout

2. **actions/setup-dotnet@v4**
   - Current stable version for .NET setup
   - Supports .NET 10
   - Docs: https://github.com/actions/setup-dotnet

3. **.NET SDK 10.0.x**
   - Use `10.0.x` pattern to get latest patch (currently 10.0.102)
   - Matches version in global.json from Story 1.1
   - Docs: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

**YamlDotNet Latest Version:**
- Current stable: 13.7.1 (as of Jan 2026)
- NuGet: https://www.nuget.org/packages/YamlDotNet
- Used for parsing YAML in tests
- Mature, stable library with good .NET support

**CI Best Practices (2026):**
- Use workflow-level concurrency control to cancel outdated runs
- Cache NuGet packages to speed up builds (optional for this story)
- Use matrix builds for multi-platform testing (not needed yet)
- Set appropriate permissions (defaults are fine for now)
- Use secrets for sensitive data (not needed in this story)

**Common CI Failures to Document:**
1. SDK version mismatch (wrong .NET version installed)
2. Build errors (compilation failures)
3. Test failures (failing test cases)
4. Restore failures (network issues, package unavailability)
5. Configuration errors (YAML syntax, missing steps)

### Git Intelligence

**Recent Work Patterns (from Stories 1.1 and 1.2):**

Based on previous stories, the following patterns are established:

1. **Root Configuration Files:**
   - `global.json` pins SDK version
   - `Directory.Build.props` sets common properties
   - `.editorconfig` defines code style

2. **Documentation Standards:**
   - README.md is comprehensive and includes all setup instructions
   - Each new capability gets documented in README
   - Troubleshooting sections are required

3. **Testing Approach:**
   - Contract tests validate both code AND documentation
   - Tests use file system operations to verify artifacts
   - WebApplicationFactory for integration tests
   - YAML/text parsing for configuration validation

4. **Commit Patterns:**
   - Focus on incremental, testable changes
   - Each story creates specific, verifiable artifacts
   - Tests are added alongside implementation

**Files Created in Story 1.1:**
- Solution and project files
- Root build configuration
- README with initialization guide
- Initial contract tests

**Files Created in Story 1.2:**
- Health endpoint in TenantSaas.Sample
- Setup documentation in README
- Health endpoint and setup verification tests

**Expected Pattern for This Story:**
- Create `.github/workflows/ci.yml`
- Add CI section to README
- Create CiWorkflowTests.cs
- Update TenantSaas.ContractTests.csproj

### Previous Story Intelligence

**From Story 1.2 (Local Development Environment Setup):**

**Dev Notes Section Insights:**
- README.md is extended incrementally (don't remove existing content)
- Tests verify documentation completeness using file reading + assertions
- Use WebApplicationFactory for integration tests
- Keep changes minimal and focused

**Completion Notes:**
- Health endpoint added successfully
- README updated with setup/verification/troubleshooting
- Contract tests validate both documentation and behavior
- All tests passing in local environment

**Files Modified:**
- README.md (added local setup sections)
- TenantSaas.Sample/Program.cs (added health endpoint)
- TenantSaas.ContractTests/*.cs (added verification tests)

**Testing Approach:**
- File content verification pattern established
- Use FluentAssertions with descriptive messages
- Tests must be deterministic and fast

**Lessons for This Story:**
1. Follow the established pattern: implementation + docs + tests
2. README.md gets extended (add CI/CD section)
3. Use file reading + YAML parsing for CI workflow verification
4. Keep test file names descriptive (CiWorkflowTests.cs)
5. Add new test dependencies carefully (YamlDotNet for YAML parsing)

### References

- [Architecture: Infrastructure & Deployment](_bmad-output/planning-artifacts/architecture.md#infrastructure--deployment)
- [Architecture: Project Structure](_bmad-output/planning-artifacts/architecture.md#project-structure--boundaries)
- [Architecture: CI/CD Requirements](_bmad-output/planning-artifacts/architecture.md#starter-template-evaluation)
- [PRD: FR21 - Developer tool requirements](/_bmad-output/planning-artifacts/prd.md)
- [PRD: NFR8 - Contract tests pass across local and CI](/_bmad-output/planning-artifacts/prd.md#nonfunctional-requirements)
- [PRD: NFR12 - Bypassing integration fails CI](/_bmad-output/planning-artifacts/prd.md#nonfunctional-requirements)
- [Epics: Epic 1, Story 1.3](_bmad-output/planning-artifacts/epics.md#story-13-ci-pipeline-skeleton-build--smoke-checks)
- [Story 1.1: Project initialization](1-1-initialize-project-from-the-approved-net-template.md)
- [Story 1.2: Local setup and health endpoint](1-2-local-development-environment-setup.md)

### Project Context Reference

**CRITICAL: Read project-context.md before implementing**

Location: `_bmad-output/project-context.md`

Key rules for this story:
- Use standard .NET naming (PascalCase types/methods, camelCase locals/fields)
- No underscore prefixes anywhere
- YAML files follow standard conventions
- Use xUnit + FluentAssertions for all tests
- Tests must run in strict mode
- Keep flat repo structure
- No test subfolders (all tests in TenantSaas.ContractTests root)
- Contract tests live in TenantSaas.ContractTests only

**Critical Don't-Miss Rules Relevant to This Story:**
- Never introduce new dependencies without explicit justification (YamlDotNet justified for YAML parsing)
- Never introduce non-deterministic behavior (tests must be repeatable)
- CI workflow must be deterministic and reproducible

## Dev Agent Record

### Agent Model Used

GPT-5 (Codex CLI)

### Debug Log References

- `dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj` (pre-CI file failure captured)
- `dotnet test TenantSaas.sln`

### Implementation Plan

- Add GitHub Actions CI workflow matching global.json SDK and standard restore/build/test steps.
- Add contract tests that parse ci.yml and verify triggers, steps, runner, and SDK version.
- Update README with CI/CD documentation and expected failure scenarios.

### Completion Notes List

- Added `.github/workflows/ci.yml` to run restore/build/test on PRs and main pushes.
- Added CI workflow contract tests with YAML parsing and SDK version match to `global.json`.
- Documented CI/CD workflow behavior and failure scenarios in `README.md`.
- Disabled OpenAPI wiring in Test environment to keep contract tests deterministic.
- Tests: `dotnet test TenantSaas.sln`

### Change Log

- 2026-02-01: Implemented CI workflow, contract tests, and README documentation.
- 2026-02-01: Code review fixes: added --configuration Release to CI, fixed Program.cs indentation, changed SDK to 10.0.x pattern, added AC #2 failure scenario test.

### File List

- .github/workflows/ci.yml
- README.md
- TenantSaas.ContractTests/CiWorkflowTests.cs
- TenantSaas.ContractTests/ReadmeSetupTests.cs
- TenantSaas.ContractTests/TenantSaas.ContractTests.csproj
- TenantSaas.Sample/Program.cs
- _bmad-output/implementation-artifacts/1-3-ci-pipeline-skeleton-build-smoke-checks.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
