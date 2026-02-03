---
project_name: 'TenantSaas'
user_name: 'Vlad'
date: '2026-01-25T13:34:50-08:00'
sections_completed: ['technology_stack', 'language_rules', 'framework_rules', 'testing_rules', 'quality_rules', 'workflow_rules', 'anti_patterns']
existing_patterns_found: 0
status: 'complete'
rule_count: 56
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

- .NET 10 (all projects target net10.0)
- EF Core 10 (reference adapter)
- SQLite (reference datastore, abstracted)
- Minimal APIs (sample host)
- Swashbuckle.AspNetCore 10.1.0 (Swagger)
- OpenTelemetry.Extensions.Hosting 1.15.0 (optional)
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.0 (optional)
- xUnit (contract tests)
- Problem Details (RFC 7807)

## Critical Implementation Rules

### Language-Specific Rules (C#/.NET)

- Use standard .NET naming (PascalCase types/methods, camelCase locals/fields).
- No underscore prefixes anywhere.
- Prefer `async`/`await` and `Task`-returning APIs; avoid `.Result`/`.Wait()`.
- Always pass `CancellationToken` in async methods.
- Use primary constructors where possible; avoid them only when not viable.
- Use DI extension methods for service registration.
- Business logic lives in services; none in controllers.
- Services call repositories for data access; no data access elsewhere.
- Use nullable reference types and explicit nullability where applicable.
- Use exceptions for truly exceptional control flow; prefer explicit result types for invariant violations.

### Framework-Specific Rules (ASP.NET Core / EF Core)

- Use Minimal APIs for the sample host.
- Controllers (if any) must be thin; all business logic stays in services.
- EF Core access only through repositories; no direct `DbContext` use outside.
- Enforce tenant invariants in `SaveChanges` guard only.

### Testing Rules

- Use xUnit for all tests.
- Use FluentAssertions for assertions.
- Use Moq for mocking where needed.
- Tests must run in strict mode everywhere.
- Contract tests live in `TenantSaas.ContractTests` only (no test subfolders).
- Tests must assert invariant violations via Problem Details shape + `invariant_code`.
- Avoid mocks in contract tests when feasible; prefer real EF Core + SQLite fixtures.

### Code Quality & Style Rules

- JSON fields are `camelCase`; no underscores.
- Date/time is ISO 8601 / RFC 3339 UTC with `Z` only.
- Problem Details only for errors; direct resource for success.
- Structured logs must include required fields (`tenant_ref`, `trace_id`, `request_id`, `invariant_code`, `event_name`, `severity`).
- Use `invariant_code` (string) over GUID-like IDs.

### Development Workflow Rules

- Keep flat repo structure (no `src/`, no `tests/` folders).
- Use `Directory.Build.props/targets` for shared build config.
- CI in `.github/workflows/ci.yml`, release on tags.
- Use `scripts/` for build/test/pack scripts.

### Critical Don’t-Miss Rules

- Never access persistence outside repositories.
- Never execute a query that is not tenant-scoped.
- Never bypass tenant context or invariant guards.
- Never throw or surface raw exceptions across API boundaries.
- Never return non-Problem Details error responses.
- Never use HTTP 200 for error conditions.
- Never emit non-UTC timestamps or timestamps without `Z`.
- Never log without required structured fields.
- Never log secrets, credentials, tokens, or PII.
- Never introduce non-deterministic behavior without explicit justification.
- Never introduce new dependencies without explicit justification.
- Never perform side effects outside designated adapters.
- Never use implicit or ambiguous route parameters (e.g., `{id}`).
- Never introduce breaking API changes without explicit versioning.

---

## Usage Guidelines

**For AI Agents:**
- Read this file before implementing any code.
- Follow ALL rules exactly as documented.
- When in doubt, prefer the more restrictive option.
- Update this file if new patterns emerge.

**For Humans:**
- Keep this file lean and focused on agent needs.
- Update when the technology stack changes.
- Review quarterly for outdated rules.
- Remove rules that become obvious over time.

Last Updated: 2026-01-25T13:34:50-08:00
