---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - "_bmad-output/planning-artifacts/product-brief-TenantSaas-2026-01-23T15:42:26-08:00.md"
  - "_bmad-output/planning-artifacts/prd.md"
workflowType: 'architecture'
project_name: 'TenantSaas'
user_name: 'Vlad'
date: '2026-01-24T10:08:22-08:00'
lastStep: 8
status: 'complete'
completedAt: '2026-01-25T13:32:41-08:00'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
The FRs define a strict tenant-context model with explicit attribution, a small invariant set (<=7), and uniform enforcement across request, background, admin, and scripted paths. A single unavoidable integration point must initialize and propagate context. The baseline must refuse ambiguous actions and surface explicit refusal reasons. Verification artifacts (contract tests) are first-class deliverables and must be runnable by adopters in CI. Documentation is part of the core contract: a concise conceptual model, explicit trust contract, integration guide, and full API reference. Extension points are allowed only through sanctioned boundaries that preserve invariants; bypass paths are explicitly blocked and tested. Onboarding requirements emphasize discoverability and actionable errors.

**Non-Functional Requirements:**
Security and correctness dominate: 100% rejection of ambiguous attribution, explicit privileged action gating, deterministic enforcement, and zero silent fallbacks. Reliability requires consistent behavior across execution contexts and repeated runs. Integration must be unavoidable and non-invasive to domain logic, with contract tests catching bypasses. Performance and scalability targets are tight (low latency overhead, no default background loops, invariants hold under scale).

**Scale & Complexity:**
Complexity is medium-to-high due to safety guarantees, cross-context enforcement, and proof requirements.

- Primary domain: backend/platform developer tool (multi-tenant SaaS baseline)
- Complexity level: medium-to-high
- Estimated architectural components: 6-8 (core context model, enforcement engine, integration boundary, context propagation, contract tests, docs/contract, sample integration, packaging/distribution)

### Technical Constraints & Dependencies

- MVP language: .NET; secondary npm distribution deferred post-MVP for tooling/examples.
- No end-user UI; UX work limited to developer ergonomics, docs flow, and error messaging.
- Small, stable API surface; invariants must be non-bypassable.
- Single unavoidable integration point for all execution paths.
- Contract tests are mandatory and must be CI-ready.

### Cross-Cutting Concerns Identified

- Tenant attribution as a hard requirement across all paths.
- Explicit refusal behavior and error guidance.
- Privilege elevation with explicit intent and auditability.
- Deterministic enforcement and proof via contract tests.
- Performance overhead limits and predictable behavior under scale.

## Starter Template Evaluation

### Primary Technology Domain

Backend/platform developer tool (multi-tenant SaaS baseline) based on the project requirements.

### Starter Options Considered

**.NET SDK default templates (`dotnet new`)**  
Verified on Microsoft Learn as the current, maintained templates that ship with the .NET SDK (solution, class library, xUnit, web API).

Rationale: Minimal, unopinionated scaffolding that fits a trust-kernel library and avoids unnecessary framework assumptions.

### Selected Starter: .NET SDK default templates (monorepo baseline)

**Rationale for Selection:**
- Aligns with .NET-first MVP and a minimal, invariant-focused core.
- Keeps surface area small and avoids architectural bias.
- Provides contract-test scaffolding (xUnit) and a reference host (web API) without dictating runtime architecture.

**Initialization Command:**

```bash
dotnet new sln -n TenantSaas

dotnet new classlib -n TenantSaas.Core -f net10.0
dotnet new classlib -n TenantSaas.EfCore -f net10.0
dotnet new xunit -n TenantSaas.ContractTests -f net10.0
dotnet new webapi -n TenantSaas.Sample -f net10.0

dotnet sln TenantSaas.sln add \
  TenantSaas.Core/TenantSaas.Core.csproj \
  TenantSaas.EfCore/TenantSaas.EfCore.csproj \
  TenantSaas.ContractTests/TenantSaas.ContractTests.csproj \
  TenantSaas.Sample/TenantSaas.Sample.csproj
```

**Architectural Decisions Provided by Starter:**

**Language & Runtime:**
- .NET 10 target framework for all projects.

**Build Tooling:**
- Standard .NET SDK build pipeline and solution structure.

**Testing Framework:**
- xUnit scaffold for contract tests.

**Code Organization:**
- Monorepo with separate projects for core, contract tests, and sample host.

**Development Experience:**
- Standard SDK CLI workflows; no added tooling assumptions.

**Note:** Project initialization using this command should be the first implementation story.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- .NET 10 monorepo baseline with storage-agnostic core and reference adapters.
- Tenant context + invariant enforcement in core; EF Core and TenantFence-style patterns are reference implementations only.
- Refuse-by-default enforcement with strictly constrained break-glass path.
- Contract tests as first-class verification artifacts.

**Important Decisions (Shape Architecture):**
- REST Minimal APIs for sample host.
- Swagger endpoint docs + minimal handwritten guidance.
- Problem Details (RFC 7807) as the sole error format.
- BYO-auth stance with runnable API key sample.
- Structured logging with optional OpenTelemetry hooks.

**Deferred Decisions (Post-MVP):**
- Packaging strategy (NuGet layout, multi-package split).
- Distributed caching (Redis, etc.).
- Auth provider integrations (OIDC/other).
- Rate limiting implementation.
- Production infrastructure templates.

### Data Architecture

- **Core library:** Storage-agnostic; no EF Core dependency.
- **Datastore (reference/sample):** SQLite (abstracted).
- **Reference persistence adapter:** EF Core (current 10.0) in sample/reference only.
- **Modeling approach (reference/sample):** `CurrentTenant` scope + `TenantDbContext` SaveChanges guard (TenantFence-style).
- **Caching:** In-memory only, abstracted behind interface.

### Authentication & Security

- **Auth in sample:** Simple API key (runnable) + official BYO-auth stance.
- **Authorization pattern:** Thin, tenant-scoped guard/policy module portable across HTTP/jobs/scripts.
- **Middleware posture:** Refuse-by-default with optional, strictly constrained break-glass path.

### API & Communication Patterns

- **API style:** REST Minimal APIs.
- **API docs:** Swagger (Swashbuckle.AspNetCore 10.1.0) + minimal handwritten guidance (non-duplicative).
- **Errors:** Problem Details (RFC 7807) only.
- **Rate limiting:** Documented recommendation only (no implementation).

### Frontend Architecture

- Not applicable (no UI).

### Infrastructure & Deployment

- **Sample hosting:** `dotnet run` + Dockerfile (optional docker-compose).
- **CI/CD:** GitHub Actions build/test/pack; release on tags.
- **Observability:** Core defines logging interfaces and enrichers; optional OpenTelemetry hooks (OpenTelemetry.Extensions.Hosting 1.15.0, OTLP exporter 1.15.0) demonstrated in the sample host.
- **Scaling:** Docs-first with “known failure modes” and recommended patterns; no infra implementation.

### Decision Impact Analysis

**Implementation Sequence:**
1) Define tenant context + policy guard contract (core).
2) Implement EF Core `TenantDbContext` SaveChanges guard (reference adapter).
3) Add contract tests (xUnit) for invariants and refusal behavior.
4) Add sample host (Minimal API) with API key auth and Problem Details.
5) Add Swagger docs + minimal guidance.
6) Add logging hooks; optional OTel example.

**Cross-Component Dependencies:**
- Contract tests depend on core invariants and EF Core adapter.
- Sample host depends on policy guard and error handling standard.
- Logging/observability hooks must wrap guard/tenant context to preserve traceability.

## Implementation Patterns & Consistency Rules

### Pattern Categories Defined

**Critical Conflict Points Identified:**
Naming, structure, API formats, error handling, logging fields, and date/time formatting.

### Naming Patterns

**Database Naming Conventions:**
- Tables and columns use `PascalCase` to align with EF Core defaults (e.g., `TenantId`, `CreatedAt`).
- No underscore prefixes anywhere.

**API Naming Conventions:**
- REST endpoints are plural nouns: `/tenants`, `/invariants`, `/contexts`.
- Route parameters are explicit: `/tenants/{tenantId}` (never `{id}`).
- Query parameters are `camelCase` (e.g., `includeDeleted`).

**Code Naming Conventions:**
- Standard .NET naming: PascalCase types/methods, camelCase locals/fields.
- No underscore prefixes anywhere.

### Structure Patterns

**Project Organization:**
- Flat root structure (no `src/` or `tests/` folders).
- Per-project `*.Tests` projects only (no tests subfolder).

**File Structure Patterns:**
- Shared utilities live in `TenantSaas.Core` (no `Common/` folder currently).
- Samples live under dedicated `TenantSaas.Sample` project.

### Format Patterns

**API Response Formats:**
- Success responses return direct resource payloads (no `{data, error}` envelope).
- Errors use Problem Details only.

**Data Exchange Formats:**
- JSON fields are `camelCase`.
- Date/time values are ISO 8601 / RFC 3339 UTC strings with `Z` suffix only.

### Communication Patterns

**Event Naming (if applicable):**
- `event_name` is lowercase, dot-separated (e.g., `tenant.resolve_failed`).

### Process Patterns

**Error Handling Patterns:**
- Use RFC 7807 Problem Details with fixed fields: `type`, `title`, `status`, `detail`, `instance`.
- Extensions: `invariant_code`, `trace_id`, `tenant_ref` (only when safe), and `errors` for validation.
- Tenant disclosure policy: include `tenant_ref` in Problem Details only when the caller is authenticated/authorized for that tenant and disclosure is not an enumeration risk; otherwise omit it.
- Definition: `tenant_ref` is the disclosure-safe tenant identifier used in logs/errors. It is either an opaque public tenant ID or a safe-state token (`unknown`, `sensitive`, `cross_tenant`) when disclosure is unsafe; it must not be a raw internal ID or a reversible identifier.
- `type` is stable and machine-meaningful (URN or URL).
- `detail` must never leak internals or secrets.
- For 500s, use generic title/detail with `trace_id` + `invariant_code`.
- HTTP mapping discipline: 401/403 auth, 404 not found, 409 conflict, 422 semantic validation (if used), 429 rate limit, 500+ server faults.
- For validation, use `ValidationProblemDetails` and include `invariant_code`.

**Logging Patterns:**
- Structured logs with a tight, consistent field set.
- Required fields: `tenant_ref`, `trace_id`, `request_id`, `invariant_code`, `event_name`, `severity`.
- Optional fields: `span_id`, `user_id`, `route`, `endpoint`, `http_method`, `status_code`, `duration_ms`, `client_id`.
- Prefer `invariant_code` (string) over GUID-like IDs.
- Provide a minimal log enricher helper (e.g., `TenantLogEnricher`) and/or middleware.
- Do not hard-depend on a specific logging stack beyond `ILogger`.
- Tenant reference states for logs:
  - `tenant_ref = <opaque tenant id>` when safe to disclose.
  - `tenant_ref = "unknown"` when unresolved.
  - `tenant_ref = "sensitive"` when resolved but unsafe to disclose.
  - `tenant_ref = "cross_tenant"` for admin/global operations.

### Enforcement Guidelines

**All AI Agents MUST:**
- Use explicit route parameters (e.g., `{tenantId}`) and plural endpoints.
- Emit Problem Details for all errors with `invariant_code`.
- Log required fields on every request-related log line, using `tenant_ref` semantics.

**Pattern Enforcement:**
- Contract tests validate error shape, required log fields, and date/time formats.
- Pattern violations are documented in the architecture decision doc and corrected in PRs.
- Updates to patterns require updating this section and tests.

### Pattern Examples

**Good Examples:**
- Route: `GET /tenants/{tenantId}`
- Problem Details `type`: `urn:tenantsaas:error:tenant-not-found`
- Log: `event_name=tenant.resolve_failed`, `tenant_ref=...`, `trace_id=...`

**Anti-Patterns:**
- Route: `/tenant/{id}` or `/Tenant`
- Error envelope: `{ data, error }`
- Dates: local time strings or missing `Z` suffix
- Logs without `trace_id` or `invariant_code`

## Project Structure & Boundaries

### Complete Project Directory Structure
```
TenantSaas/
├── README.md
├── docs/
│   ├── error-catalog.md
│   ├── integration-guide.md
│   └── trust-contract.md
├── TenantSaas.sln
├── .gitignore
├── Directory.Build.props
├── Directory.Build.targets
├── global.json
├── .editorconfig
├── .github/
│   └── workflows/
│       └── ci.yml                          # release.yml deferred to Epic 1 completion
├── TenantSaas.Abstractions/
│   ├── TenantSaas.Abstractions.csproj
│   ├── BreakGlass/
│   │   ├── AuditCode.cs
│   │   ├── BreakGlassAuditEvent.cs
│   │   ├── BreakGlassDeclaration.cs
│   │   ├── BreakGlassValidationResult.cs
│   │   ├── BreakGlassValidator.cs
│   │   └── IBreakGlassAuditSink.cs
│   ├── Contexts/
│   │   └── ExecutionKind.cs
│   ├── Disclosure/
│   │   ├── DisclosureContext.cs
│   │   ├── DisclosurePolicy.cs
│   │   ├── DisclosureValidationResult.cs
│   │   ├── DisclosureValidator.cs
│   │   ├── IDisclosurePolicyProvider.cs
│   │   ├── TenantRef.cs
│   │   └── TenantRefSafeState.cs
│   ├── Tenancy/
│   │   ├── IMutableTenantContextAccessor.cs
│   │   ├── ITenantAttributionResolver.cs
│   │   ├── ITenantContextAccessor.cs
│   │   ├── ITenantContextInitializer.cs
│   │   ├── ITenantFlowFactory.cs
│   │   ├── ITenantFlowScope.cs
│   │   ├── NoTenantReason.cs
│   │   ├── TenantAttributionInput.cs
│   │   ├── TenantAttributionInputs.cs
│   │   ├── TenantAttributionResult.cs
│   │   ├── TenantAttributionRules.cs
│   │   ├── TenantAttributionSource.cs
│   │   ├── TenantContext.cs
│   │   ├── TenantId.cs
│   │   └── TenantScope.cs
│   ├── Invariants/
│   │   ├── InvariantCode.cs
│   │   ├── InvariantDefinition.cs
│   │   └── RefusalMapping.cs
│   ├── Logging/
│   │   ├── ILogEnricher.cs
│   │   └── StructuredLogEvent.cs
│   └── TrustContract/
│       └── TrustContractV1.cs
├── TenantSaas.Core/
│   ├── TenantSaas.Core.csproj
│   ├── Tenancy/
│   │   ├── AmbientTenantContextAccessor.cs
│   │   ├── ExplicitTenantContextAccessor.cs
│   │   ├── TenantAttributionResolver.cs
│   │   ├── TenantContextConflictException.cs
│   │   ├── TenantContextInitializer.cs
│   │   ├── TenantFlowFactory.cs
│   │   └── TenantFlowScope.cs
│   ├── Enforcement/
│   │   ├── AttributionEnforcementResult.cs
│   │   ├── BoundaryGuard.cs
│   │   ├── BreakGlassAuditHelper.cs
│   │   ├── EnforcementResult.cs
│   │   └── IBoundaryGuard.cs
│   ├── Logging/
│   │   ├── DefaultLogEnricher.cs
│   │   └── EnforcementEventSource.cs
│   └── Errors/
│       ├── ProblemDetailsExtensions.cs
│       └── ProblemDetailsFactory.cs
├── TenantSaas.EfCore/                      # Planned: Epic 6, Story 6.3
│   └── TenantSaas.EfCore.csproj            # Shell only; adapter not yet implemented
├── TenantSaas.ContractTests/
│   ├── TenantSaas.ContractTests.csproj
│   ├── Enforcement/
│   │   └── BreakGlassEnforcementTests.cs
│   ├── Errors/
│   │   ├── ProblemDetailsFactoryTests.cs
│   │   └── ProblemDetailsShapeTests.cs
│   ├── Logging/
│   │   ├── EnforcementLoggingTests.cs
│   │   ├── LogEnricherTests.cs
│   │   └── RefusalCorrelationTests.cs
│   ├── TestUtilities/
│   │   └── TestLogCapture.cs
│   ├── AmbientContextPropagationTests.cs
│   ├── AttributionEnforcementTests.cs
│   ├── AttributionRulesTests.cs
│   ├── BreakGlassContractTests.cs
│   ├── CiWorkflowTests.cs
│   ├── ContextInitializedTests.cs
│   ├── ContextTaxonomyTests.cs
│   ├── ContractTestKitApiStabilityTests.cs
│   ├── ContractTestKitFunctionalTests.cs
│   ├── DisclosurePolicyTests.cs
│   ├── ExecutionKindAndScopeTests.cs
│   ├── FlowWrapperTests.cs
│   ├── HealthEndpointTests.cs
│   ├── HttpCorrelationExtensionsTests.cs
│   ├── InitializationEnforcementTests.cs
│   ├── InvariantRegistryTests.cs
│   ├── MiddlewareProblemDetailsTests.cs
│   ├── ReadmeInitializationTests.cs
│   ├── ReadmeSetupTests.cs
│   ├── ReferenceComplianceBreakGlassTests.cs
│   ├── ReferenceComplianceRefusalAttributionTests.cs
│   └── TenantContextInitializerTests.cs
├── TenantSaas.Sample/
│   ├── TenantSaas.Sample.csproj
│   ├── Program.cs                         # Minimal API routes defined inline
│   ├── Middleware/
│   │   ├── HttpCorrelationExtensions.cs
│   │   ├── TenantContextMiddleware.cs
│   │   └── ProblemDetailsExceptionMiddleware.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── TenantSaas.Sample.http
```

**Deferred Docs:** `docs/verification-guide.md` (Epic 6, Story 6.6) and `docs/api-reference.md` (Epic 6, Story 6.7).
**Deferred Infrastructure:** `docker/` and `scripts/` are not present in the repo (planned optional infrastructure).

### Architectural Boundaries

**API Boundaries:**
- External API is only in `TenantSaas.Sample` (Minimal APIs).
- Authentication: BYO-auth stance; sample does not implement auth (adopters bring their own). Deferred to adopter integration.
- Error boundary is enforced via Problem Details middleware.

**Component Boundaries:**
- `TenantSaas.Abstractions` defines contracts and shared types only.
- `TenantSaas.Core` implements invariant enforcement and logging helpers.
- `TenantSaas.EfCore` provides the reference EF Core adapter only (shell; implementation deferred to Epic 6, Story 6.3).
- Contract tests target abstractions, core behavior, and the sample host for E2E integration verification. Note: Some tests use `WebApplicationFactory<Program>` for integration testing; this coupling is intentional for E2E validation.

**Service Boundaries:**
- No internal services; the library remains a single trust-kernel.
- Sample host is a thin integration example only.

**Data Boundaries:**
- EF Core integration contained in `TenantSaas.EfCore` (shell only; implementation deferred).
- Tenant context resolution is in `TenantSaas.Core/Tenancy`, accessed via abstractions.

### Requirements to Structure Mapping

**FR Categories → Locations:**
- Tenant context model → `TenantSaas.Abstractions/Tenancy` + `TenantSaas.Core/Tenancy`
- Invariant enforcement/refusal → `TenantSaas.Core/Enforcement`
- Integration point → `TenantSaas.Sample/Middleware` + `TenantSaas.Core/Tenancy`
- Verification artifacts → `TenantSaas.ContractTests`
- Trust contract/docs → `docs/`

**Cross-Cutting Concerns:**
- Error handling → `TenantSaas.Core/Errors` + `TenantSaas.Sample/Middleware`
- Logging → `TenantSaas.Abstractions/Logging` + `TenantSaas.Core/Logging`
- Break-glass → `TenantSaas.Abstractions/BreakGlass` + `TenantSaas.Core/Enforcement/BreakGlassAuditHelper.cs`

### Integration Points

**Internal Communication:**
- Sample host consumes `TenantSaas.Core` + `TenantSaas.Abstractions`.
- Contract tests reference `TenantSaas.Core` and `TenantSaas.Abstractions`.

**External Integrations:**
- API key auth (sample only) is replaceable; BYO-auth stance in docs.

**Data Flow:**
- Request → API key auth → tenant context middleware → invariant guard → reference adapter (EF Core in sample).

### File Organization Patterns

**Configuration Files:**
- Root-level `Directory.Build.props/targets`, `global.json`, `.editorconfig`.
- Sample config in `TenantSaas.Sample/appsettings*.json`.

**Source Organization:**
- Abstractions contain contracts only; Core contains invariant enforcement and shared logic; reference adapters live in separate packages (e.g., `TenantSaas.EfCore`).

**Test Organization:**
- Contract tests in `TenantSaas.ContractTests` only (no test subfolders).

**Asset Organization:**
- Docker assets under `docker/`.
- Docs under `docs/`.

### Development Workflow Integration

**Development Server Structure:**
- `dotnet run` from `TenantSaas.Sample` or Docker via `docker/sample.Dockerfile`.

**Build Process Structure:**
- Solution-wide build via `dotnet build TenantSaas.sln`.

**Deployment Structure:**
- Sample API containerized for demo; library shipped via NuGet (packaging deferred).

## Architecture Validation Results

### Implementation Status

> **Last Updated:** 2026-02-13

| Component | Status | Notes |
|-----------|--------|-------|
| TenantSaas.Abstractions | ✅ Implemented | Contracts: Tenancy (`ITenantContextAccessor`, `TenantScope`), Logging (`ILogEnricher`, `StructuredLogEvent`), BreakGlass/Disclosure/TrustContract |
| TenantSaas.Core | ✅ Implemented | Enforcement (`BoundaryGuard`, `EnforcementResult`), Tenancy (`TenantContextInitializer`, `TenantFlowFactory`), Logging (`DefaultLogEnricher`) |
| TenantSaas.EfCore | 🔜 Deferred | Shell only; adapter planned in Epic 6, Story 6.3 |
| TenantSaas.Sample | ✅ Implemented | Minimal API host with middleware; BYO-auth stance |
| TenantSaas.ContractTests | ✅ Implemented | Contract + E2E tests; intentional Sample coupling for integration verification |
| docs/verification-guide.md | 🔜 Deferred | Planned in Epic 6, Story 6.6 |
| docs/api-reference.md | 🔜 Deferred | Planned in Epic 6, Story 6.7 |
| npm distribution | 🔜 Deferred | Post-MVP secondary tooling/examples distribution |
| .github/workflows/release.yml | 🔜 Deferred | Planned post-Epic 1 completion |
| docker/, scripts/ | 🔜 Deferred | Optional infrastructure |
| Sample Auth (ApiKeyAuthHandler) | ⏭️ Not Planned | BYO-auth stance; adopters bring their own |
| Logging `event_name` field | ⏸️ Pending | Requires architectural decision on field format before implementation |

### Coherence Validation ✅

**Decision Compatibility:**
All technology choices are compatible: .NET 10 + EF Core 10, Minimal APIs, Problem Details, and Swashbuckle align without conflict.

**Pattern Consistency:**
Naming, format, and logging patterns align with the stack and enforceable invariants.

**Structure Alignment:**
Project structure cleanly separates abstractions, core enforcement, samples, and tests.

### Requirements Coverage Validation ✅

**Epic/Feature Coverage:**
No epics provided; FR categories are mapped to modules.

**Functional Requirements Coverage:**
All FR categories (tenant context, invariants, integration point, verification, docs, extensibility, onboarding) are supported by specific components and patterns.

**Non-Functional Requirements Coverage:**
Security, determinism, explicit error handling, performance overhead, and integration safeguards are addressed via enforcement design and contract tests.

### Implementation Readiness Validation ✅

**Decision Completeness:**
Critical decisions documented with versions and rationale.

**Structure Completeness:**
Full project tree defined with boundaries and integration points.

**Pattern Completeness:**
Patterns specified for naming, errors, logging, dates, and routes.

### Gap Analysis Results

**Critical Gaps:** None  
**Important Gaps:** None  
**Nice-to-Have Gaps:** Packaging strategy deferred (intentional), background job sample planned.

### Validation Issues Addressed

- Packaging deferred to post-MVP (documented).
- Background job sample acknowledged as planned.

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**✅ Architectural Decisions**
- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance considerations addressed

**✅ Implementation Patterns**
- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Communication patterns specified
- [x] Process patterns documented

**✅ Project Structure**
- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION AFTER EPICS  
**Confidence Level:** High

**Key Strengths:**
- Clear invariant enforcement boundaries
- Strong consistency rules for multi-agent implementation
- Full traceability from requirements to structure

**Areas for Future Enhancement:**
- Packaging strategy definition
- Additional sample (background jobs)

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented
- Use implementation patterns consistently across all components
- Respect project structure and boundaries
- Refer to this document for all architectural questions

**First Implementation Priority:**
Initialize the monorepo with the chosen `dotnet new` commands and wire the core invariant enforcement + contract tests.

## Architecture Completion Summary

### Workflow Completion

**Architecture Decision Workflow:** COMPLETED ✅
**Total Steps Completed:** 8
**Date Completed:** 2026-01-25T13:32:41-08:00
**Document Location:** _bmad-output/planning-artifacts/architecture.md

### Final Architecture Deliverables

**📋 Complete Architecture Document**

- All architectural decisions documented with specific versions
- Implementation patterns ensuring AI agent consistency
- Complete project structure with all files and directories
- Requirements to architecture mapping
- Validation confirming coherence and completeness

**🏗️ Implementation Ready Foundation**

- 20+ architectural decisions made
- 12+ implementation patterns defined
- 4 primary architectural components specified
- All requirements fully supported

**📚 AI Agent Implementation Guide**

- Technology stack with verified versions
- Consistency rules that prevent implementation conflicts
- Project structure with clear boundaries
- Integration patterns and communication standards

### Implementation Handoff

**For AI Agents:**
This architecture document is your complete guide for implementing TenantSaas. Follow all decisions, patterns, and structures exactly as documented.

**First Implementation Priority:**
Initialize the monorepo with the chosen `dotnet new` commands and wire the core invariant enforcement + contract tests.

**Development Sequence:**

1. Initialize project using documented starter template
2. Set up development environment per architecture
3. Implement core architectural foundations
4. Build features following established patterns
5. Maintain consistency with documented rules

### Quality Assurance Checklist

**✅ Architecture Coherence**

- [x] All decisions work together without conflicts
- [x] Technology choices are compatible
- [x] Patterns support the architectural decisions
- [x] Structure aligns with all choices

**✅ Requirements Coverage**

- [x] All functional requirements are supported
- [x] All non-functional requirements are addressed
- [x] Cross-cutting concerns are handled
- [x] Integration points are defined

**✅ Implementation Readiness**

- [x] Decisions are specific and actionable
- [x] Patterns prevent agent conflicts
- [x] Structure is complete and unambiguous
- [x] Examples are provided for clarity

### Project Success Factors

**🎯 Clear Decision Framework**
Every technology choice was made collaboratively with clear rationale, ensuring all stakeholders understand the architectural direction.

**🔧 Consistency Guarantee**
Implementation patterns and rules ensure that multiple AI agents will produce compatible, consistent code that works together seamlessly.

**📋 Complete Coverage**
All project requirements are architecturally supported, with clear mapping from business needs to technical implementation.

**🏗️ Solid Foundation**
The chosen starter template and architectural patterns provide a production-ready foundation following current best practices.

---

## Glossary

- **break-glass:** Explicit, audited override path for privileged or cross-tenant actions; never implicit or default.
- **context taxonomy:** The defined set of tenancy scopes and states (tenant, shared-system, no-tenant + reasons) used across the system.
- **contract tests:** Adopter-runnable tests that verify invariants, refusal behavior, and disclosure rules in CI.
- **cross_tenant marker:** Log/error value indicating a cross-tenant administrative operation.
- **disclosure policy:** Rules that determine when tenant-related information may be shown in logs/errors.
- **execution kind:** Labeled execution flow type (request, background, admin, scripted) captured in context for enforcement.
- **invariant_code:** Stable identifier for an invariant or refusal, used in errors/logs and tests.
- **Problem Details:** RFC 7807 error format used as the sole error response shape.
- **refusal mapping:** Schema that maps an invariant violation to status, invariant_code, and guidance link.
- **request_id:** Correlation ID for request-scoped operations; included in logs/errors when applicable.
- **sanctioned boundaries:** Approved integration points where enforcement occurs; bypass paths are disallowed.
- **safe-state values:** `tenant_ref` values like `unknown`, `sensitive`, `cross_tenant` used when disclosure is unsafe.
- **target_tenant_ref:** Disclosure-safe tenant identifier for the target of a privileged/cross-tenant operation.
- **tenant attribution:** Rules and sources used to resolve tenant identity; ambiguity must refuse.
- **trace_id:** End-to-end correlation ID present in refusals and logs.
- **trust contract:** Canonical definition of invariants, scopes, attribution rules, refusal behavior, and disclosure policy.

**Architecture Status:** READY FOR IMPLEMENTATION ✅

**Next Phase:** Begin implementation using the architectural decisions and patterns documented herein.

**Document Maintenance:** Update this architecture when major technical decisions are made during implementation.
