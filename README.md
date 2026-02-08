# TenantSaas

> ⚠️ **Work in Progress** — TenantSaas is under active development. APIs, invariants, and documentation may change.

A trust-first baseline for multi-tenant SaaS systems that enforces tenant-scope invariants at unavoidable integration points.

TenantSaas targets platform engineers, founders, and security reviewers who need explicit, verifiable guarantees — not framework-driven scaffolding. Differentiation comes from invariant-first design, explicit refusal behavior, and runnable contract tests that prove guarantees hold across all execution paths.

## The Problem

Multi-tenant systems routinely allow actions with ambiguous tenant scope or authority, creating latent cross-tenant risks that are only discovered after incidents. TenantSaas eliminates this class of problems by making tenant context explicit, enforced, and verifiable.

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
