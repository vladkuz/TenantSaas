# TenantSaas

> ⚠️ **Work in Progress** — TenantSaas is under active development. APIs, invariants, and documentation may change.

A trust-first baseline for multi-tenant SaaS systems that enforces tenant-scope invariants at unavoidable integration points.

---

## What Problem Does TenantSaas Solve?

Multi-tenant SaaS systems share infrastructure across tenants. Every request, background job, and admin operation must know *which tenant* it belongs to — and what happens when that answer is missing or ambiguous. Most systems get this wrong silently:

- A background job runs without tenant context and writes data to the wrong partition.
- Two attribution sources (header vs. route) disagree, and the system picks one without telling anyone.
- An admin script touches all tenants because nobody enforced scope.
- Logs leak real tenant identifiers into shared monitoring pipelines.

These are not edge cases. They are **the default failure mode** of multi-tenant systems that treat tenancy as a convention instead of an invariant. TenantSaas makes these failures impossible by refusing to proceed when the rules are broken.

## What Pain Does It Remove?

| Pain Point | Without TenantSaas | With TenantSaas |
|-----------|--------------------|-----------------|
| **Ambiguous tenant context** | Discovered after a cross-tenant incident | Refused at the boundary before execution |
| **Missing context in background jobs** | Silent — job runs in wrong scope | Fails immediately with actionable error |
| **Conflicting attribution sources** | Last-write-wins or random precedence | Explicit refusal with RFC 7807 Problem Details |
| **Privileged operations** | Admin endpoints with implicit god-mode | Break-glass requires actor, reason, and audit trail |
| **Tenant IDs in logs** | Raw IDs leak into shared observability | Disclosure-safe tokens (`unknown`, `sensitive`, `opaque`) |
| **Onboarding new engineers** | Tribal knowledge in Slack threads | Trust contract and contract tests are the documentation |
| **Proving compliance** | "Trust us, we tested it" | Runnable contract tests in CI that prove invariants hold |

## What Exists Today vs. What Is Planned

### ✅ Available Now (MVP)

- **Trust contract v1** — five enforced invariants covering context, attribution, scope, break-glass, and disclosure
- **Single integration point** — `ITenantContextInitializer` for all execution paths (request, background, admin, scripted)
- **Enforcement engine** — boundary guard that refuses ambiguous or unattributed operations
- **Break-glass subsystem** — explicit, auditable elevation for privileged cross-tenant operations
- **Disclosure policy** — safe-state tokens prevent tenant ID leaks in logs and errors
- **RFC 7807 error responses** — stable error codes, actionable guidance, and contract rule links
- **Contract test kit** — `TenantSaas.ContractTestKit` for adopters to verify compliance in their own CI
- **First-party contract tests** — full coverage across all execution paths
- **Sample host** — minimal ASP.NET Core app demonstrating end-to-end integration
- **Documentation** — trust contract, integration guide (≤30 min setup), error catalog

### 🔜 Planned (Post-MVP)

- EF Core tenant-scoped query filters and database-per-tenant patterns
- Flow wrappers for hosted services, message consumers, and scheduled jobs
- Observability hooks (metrics, distributed tracing enrichment)
- Additional attribution adapters (JWT claims, custom resolvers)
- Reference application with real-world patterns
- On-call incident workflow documentation
- Migration guide for retrofitting existing services
- Ecosystem extensions and community adapters

## Why Is This Different from Other SaaS Templates?

Most multi-tenant libraries and templates give you **scaffolding** — tenant resolution middleware, database-per-tenant helpers, maybe some DI wiring. They help you *build faster*. TenantSaas helps you **not break things**.

| | Typical SaaS Template | TenantSaas |
|-|----------------------|------------|
| **Philosophy** | Productivity — get started quickly | Safety — make wrong things impossible |
| **Tenant context** | Convention (hope you remembered) | Enforced invariant (system refuses otherwise) |
| **Missing context** | Silent fallback or null | Explicit refusal with error details |
| **Ambiguous attribution** | Last source wins | Refused — conflicting sources are an error |
| **Privileged operations** | Admin role = do anything | Break-glass with actor, reason, audit |
| **Verification** | Unit tests you write yourself | Contract tests shipped as a package, runnable in your CI |
| **Scope** | Full framework (auth, billing, UI) | Thin baseline — just the trust kernel |
| **Extension model** | Override anything | Invariants are non-negotiable; extend only at sanctioned boundaries |

TenantSaas is not trying to replace your SaaS template. It is the **trust layer underneath it** — the part that makes sure tenant boundaries are never silently violated, regardless of what you build on top.

---

## Core Concepts

### Trust Contract

The [trust contract](docs/trust-contract.md) is the single source of truth for tenancy rules. It defines:

- **Tenant Scopes** — `Tenant`, `SharedSystem`, and `NoTenant` (with required reason)
- **Execution Kinds** — `Request`, `Background`, `Admin`, `Scripted`
- **Break-Glass** — explicit, auditable elevation for privileged operations
- **Disclosure Policy** — safe-state tokens (`unknown`, `sensitive`, `cross_tenant`, `opaque`) for logs and errors

### Invariants

Five non-negotiable invariants enforced at boundaries:

| Invariant | What It Enforces |
|-----------|-----------------|
| `ContextInitialized` | Tenant context must be established before any operation |
| `TenantAttributionUnambiguous` | Attribution from multiple sources must agree or be refused |
| `TenantScopeRequired` | Operations requiring a tenant cannot proceed without one |
| `BreakGlassExplicitAndAudited` | Privileged actions require explicit declaration with actor, reason, and scope |
| `DisclosureSafe` | Tenant identifiers in logs/errors use disclosure-safe references |

Every invariant violation returns an [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) response with a stable error code, actionable guidance, and a link to the relevant contract rule. See the [error catalog](docs/error-catalog.md) for the full reference.

### Single Integration Point

All execution paths (HTTP requests, background jobs, admin operations, scripts) pass through `ITenantContextInitializer` — the single, unavoidable entry point for establishing tenant context. If context is missing or inconsistent, execution is refused.

## Project Structure

```
TenantSaas.Abstractions/   # Contracts, interfaces, trust contract definitions
TenantSaas.Core/            # Enforcement engine, context propagation, logging
TenantSaas.EfCore/          # Entity Framework Core tenant-scoped extensions
TenantSaas.ContractTestKit/ # Reusable test helpers for adopters to verify compliance
TenantSaas.ContractTests/   # First-party contract test suite
TenantSaas.Sample/          # Minimal reference host demonstrating integration
docs/                       # Trust contract, integration guide, error catalog
```

## Prerequisites

- **.NET SDK 10.0.102** (pinned in `global.json`)
  - Verify: `dotnet --version`
- Git, curl (optional, for cloning and health checks)
- IDE: Visual Studio, Rider, or VS Code

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd TenantSaas
dotnet restore
```

### 2. Build

```bash
dotnet build TenantSaas.sln
```

### 3. Run contract tests

```bash
dotnet test TenantSaas.sln
```

Contract tests verify that all five invariants hold across request, background, admin, and scripted execution paths. They are designed to run in CI in under 10 minutes.

### 4. Run the sample host

```bash
dotnet run --project TenantSaas.Sample/TenantSaas.Sample.csproj
```

The host prints a listening URL in the console. Verify with:

```bash
curl -L http://localhost:<port>/health
# → {"status":"healthy"}
```

## Integration

Integrating TenantSaas into a new service takes ≤30 minutes and ≤6 steps. See the full [integration guide](docs/integration-guide.md).

The short version:

1. **Add package references** to `TenantSaas.Abstractions` and `TenantSaas.Core`
2. **Register services** — context accessor, initializer, attribution resolver, boundary guard
3. **Wire middleware** — `ProblemDetailsExceptionMiddleware` (outermost) → `TenantContextMiddleware`
4. **Configure attribution sources** — route parameters, headers, host, or token claims
5. **Add contract tests** using `TenantSaas.ContractTestKit`
6. **Run in CI** — contract tests validate compliance on every build

```csharp
// Minimal service registration
var accessor = new AmbientTenantContextAccessor();
builder.Services.AddSingleton<ITenantContextAccessor>(accessor);
builder.Services.AddSingleton<IMutableTenantContextAccessor>(accessor);
builder.Services.AddScoped<ITenantContextInitializer, TenantContextInitializer>();
builder.Services.AddSingleton<ITenantAttributionResolver, TenantAttributionResolver>();
builder.Services.AddSingleton<IBoundaryGuard, BoundaryGuard>();
```

## Verification with Contract Tests

Adopters verify trust contract compliance using `TenantSaas.ContractTestKit`:

```csharp
using TenantSaas.ContractTestKit;
using Xunit;

public class TrustContractComplianceTests : IClassFixture<TrustContractFixture>
{
    private readonly TrustContractFixture _fixture;

    public TrustContractComplianceTests(TrustContractFixture fixture)
        => _fixture = fixture;

    [Fact]
    public void TrustContract_IsFullyCompliant()
        => _fixture.ValidateAll();
}
```

See the [ContractTestKit README](TenantSaas.ContractTestKit/README.md) for granular assertions and advanced usage.

## CI/CD

GitHub Actions runs on every pull request and push to `main`:

1. Restore → Build → Test
2. Contract tests validate all invariants across execution paths
3. CI fails on any invariant violation, build error, or test failure

## Key Design Principles

- **Invariant-first** — guarantees are enforced, not suggested
- **Explicit refusal** — ambiguous or unattributed operations are refused by default
- **Small core** — the API surface stays minimal and resists ad-hoc extension
- **Verifiable** — contract tests prove rules hold; they are artifacts, not afterthoughts
- **Not a framework** — TenantSaas is a baseline that fits into your architecture, not the other way around

## Documentation

| Document | Description |
|----------|-------------|
| [Trust Contract](docs/trust-contract.md) | Invariants, scopes, break-glass rules, disclosure policy |
| [Integration Guide](docs/integration-guide.md) | Step-by-step wiring for new services |
| [Error Catalog](docs/error-catalog.md) | All invariant violations and Problem Details responses |
| [ContractTestKit](TenantSaas.ContractTestKit/README.md) | Test helpers for adopter CI pipelines |

## Non-Goals

- TenantSaas is **not** a full application framework or scaffold generator
- It does **not** manage tenant provisioning, billing, or user authentication
- It does **not** prescribe your domain model, database strategy, or deployment topology
- It **will not** silently fall back when required context is missing

## Troubleshooting

### .NET SDK not found

- **Symptom:** `The specified SDK version [10.0.102] could not be found.`
- **Fix:** Install .NET SDK 10.0.102 and confirm with `dotnet --list-sdks`

### NuGet restore failures

- **Symptom:** `Unable to load the service index for source https://api.nuget.org/v3/index.json`
- **Fix:** Check network/proxy access, then rerun `dotnet restore`

### HTTPS certificate errors

- **Symptom:** TLS errors on the HTTPS URL
- **Fix:** Run `dotnet dev-certs https --trust` or use the HTTP URL printed on startup

## License

See [LICENSE](LICENSE) for details.
