# TenantSaas

> ⚠️ **Work in Progress** — Under active development. APIs and documentation may change.

Tenant isolation enforcement for multi-tenant .NET apps. Operations without explicit tenant scope are refused — not silently allowed.

**[Quick Start](#quick-start)** · **[Conceptual Model](docs/conceptual-model.md)** · **[Integration Guide](docs/integration-guide.md)** · **[Trust Contract](docs/trust-contract.md)** · **[Verification Guide](docs/verification-guide.md)** · **[Error Catalog](docs/error-catalog.md)**

```bash
dotnet restore && dotnet test TenantSaas.sln --disable-build-servers -v minimal   # prove invariants hold
```

---

## Why TenantSaas?

| Pain Point | Without TenantSaas | With TenantSaas |
|-----------|--------------------|-----------------:|
| **Ambiguous tenant context** | Discovered after a cross-tenant incident | Refused at the boundary before execution |
| **Missing context in background jobs** | Silent — job runs in wrong scope | Fails immediately with actionable error |
| **Conflicting attribution sources** | Last-write-wins or random precedence | Explicit refusal with RFC 7807 error |
| **Privileged operations** | Admin endpoints with implicit god-mode | Break-glass requires actor, reason, and audit trail |
| **Tenant IDs in logs** | Raw IDs leak into shared observability | Disclosure-safe values (`unknown`, `sensitive`, `cross_tenant`, `opaque:<hash>`) |
| **Onboarding new engineers** | Tribal knowledge in Slack threads | Trust contract and contract tests *are* the documentation |
| **Proving compliance** | "Trust us, we tested it" | Runnable contract tests in CI prove invariants hold |

## Quick Start

Requires **.NET SDK 10.0.102** ([`global.json`](global.json)). Verify: `dotnet --version`

```bash
git clone <repo-url> && cd TenantSaas
dotnet build TenantSaas.sln
dotnet test  TenantSaas.sln          # contract tests across all execution paths
dotnet run --project TenantSaas.Sample/TenantSaas.Sample.csproj
curl http://localhost:<port>/health   # → {"status":"healthy"}
```

Integrate into your own service in ≤30 minutes → [integration guide](docs/integration-guide.md).

## Prerequisites

- .NET SDK 10.0.102 (`dotnet --list-sdks`)

## Local Setup

```bash
dotnet restore
dotnet build TenantSaas.sln
dotnet run --project TenantSaas.Sample/TenantSaas.Sample.csproj
```

## Verification

Use the same command enforced in CI:

```bash
dotnet test TenantSaas.sln --disable-build-servers -v minimal
```

Contract-test bypass detection is surfaced as a hard CI error:
`Contract test failure detected; enforcement boundary may be bypassed`.

Health-check validation:

```bash
curl http://localhost:<port>/health
# {"status":"healthy"}
```

## CI/CD

GitHub Actions runs on every pull request and on pushes to `main`.

Failure scenarios are explicit and actionable:

- SDK version mismatch
- Restore failures
- Build errors
- Test failures

### Bootstrap Commands (Reference)

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

## Contract Tests — Ship Proof, Not Promises

TenantSaas ships `TenantSaas.ContractTestKit` so adopters can verify compliance in their own CI:

```csharp
public class TrustContractComplianceTests : IClassFixture<TrustContractFixture>
{
    private readonly TrustContractFixture _fixture;
    public TrustContractComplianceTests(TrustContractFixture fixture) => _fixture = fixture;

    [Fact]
    public void TrustContract_IsFullyCompliant() => _fixture.ValidateAll();
}
```

Five invariants are tested across request, background, admin, and scripted paths. CI fails if any invariant is violated. See the [ContractTestKit README](TenantSaas.ContractTestKit/README.md) for granular assertions.

## How It Works

A single integration point — `ITenantContextInitializer` — establishes tenant context for every execution path. If context is missing or inconsistent, the operation is refused with an [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) Problem Details response.

Five enforced invariants:

| Invariant | Rule |
|-----------|------|
| `ContextInitialized` | Tenant context must exist before any operation |
| `TenantAttributionUnambiguous` | Multiple attribution sources must agree |
| `TenantScopeRequired` | Tenant-scoped operations cannot proceed without a tenant |
| `BreakGlassExplicitAndAudited` | Privileged actions need actor + reason + audit trail |
| `DisclosureSafe` | Logs and errors use disclosure-safe tenant references |

Full details: [trust contract](docs/trust-contract.md) · [verification guide](docs/verification-guide.md) · [error catalog](docs/error-catalog.md)

## How It's Different

Most multi-tenant libraries give you scaffolding — tenant resolution, database helpers, DI wiring. They help you build faster. TenantSaas helps you not break things.

| | Typical SaaS Template | TenantSaas |
|-|----------------------|------------|
| **Goal** | Productivity | Safety |
| **Tenant context** | Convention | Enforced invariant |
| **Missing context** | Silent fallback | Explicit refusal |
| **Ambiguous attribution** | Last source wins | Refused |
| **Privileged operations** | Admin role = full access | Break-glass with audit |
| **Verification** | Tests you write yourself | Contract tests shipped as a package |
| **Scope** | Full framework | Thin trust layer |

TenantSaas is not a replacement for your SaaS template. It's the trust layer underneath — it guarantees tenant boundaries hold regardless of what you build on top.

## What Exists Today vs. What's Planned

### ✅ Available Now

- Trust contract v1 with five enforced invariants
- Single integration point for all execution paths
- Boundary guard enforcement engine
- Break-glass subsystem with audit trail
- Disclosure-safe tenant references
- RFC 7807 error responses with stable codes
- Contract test kit for adopter CI
- Sample ASP.NET Core host
- Optional EF Core reference adapter that enforces `IBoundaryGuard` before operations
- [Trust contract](docs/trust-contract.md), [integration guide](docs/integration-guide.md), [verification guide](docs/verification-guide.md), [error catalog](docs/error-catalog.md)

### 🔜 Planned

- EF Core tenant-scoped query filters and database-per-tenant guidance beyond the baseline reference adapter
- Flow wrappers for hosted services and message consumers
- Observability hooks (metrics, tracing enrichment)
- Additional attribution adapters (JWT claims, custom resolvers)
- Reference application with real-world patterns
- Migration guide for retrofitting existing services

## Project Structure

```
TenantSaas.Abstractions/   # Contracts, interfaces, trust contract definitions
TenantSaas.Core/            # Enforcement engine, context propagation, logging
TenantSaas.EfCore/          # Optional EF Core reference adapter (storage adapter only)
TenantSaas.ContractTestKit/ # Test helpers for adopters to verify compliance
TenantSaas.ContractTests/   # First-party contract test suite
TenantSaas.Sample/          # Minimal reference host
docs/                       # Trust contract, integration guide, verification guide, error catalog
```

## Non-Goals

- Not a full application framework or scaffold generator
- Does not manage tenant provisioning, billing, or authentication
- Does not prescribe your domain model, database strategy, or deployment topology

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `SDK version [10.0.102] could not be found` | Install .NET SDK 10.0.102 — `dotnet --list-sdks` |
| NuGet restore fails | Check network/proxy, rerun `dotnet restore` |
| HTTPS/TLS errors | `dotnet dev-certs https --trust` or use HTTP URL |

### Troubleshooting missing templates or dependencies

- `net10.0 is not a valid value for -f`: install the .NET 10 SDK.
- `No templates found matching`: install templates and retry.
- `dotnet new --install Microsoft.DotNet.Common.ProjectTemplates.10.0`
- `Unable to load the service index for source https://api.nuget.org/v3/index.json`: check network/proxy and rerun `dotnet restore`.

## License

See [LICENSE](LICENSE) for details.
