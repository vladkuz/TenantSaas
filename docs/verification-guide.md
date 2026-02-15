# TenantSaas Verification Guide

Use this guide to prove TenantSaas contract compliance in a fresh clone with only the .NET SDK installed.

## Prerequisites

- .NET SDK 10.0.102+ (same feature band; see [`global.json`](../global.json) for `rollForward: latestPatch`)
- Repository cloned locally

## Verification Steps

### 1. Run contract tests

```bash
dotnet test TenantSaas.ContractTests/TenantSaas.ContractTests.csproj --disable-build-servers -v minimal
```

Expected signal: all contract tests pass. If a test fails, read the failure message for the violated contract rule and the invariant identifier.

### 2. Run solution tests

```bash
dotnet test TenantSaas.sln --disable-build-servers -v minimal
```

Expected signal: full solution test run completes without failures.

## How To Interpret Failures

When verification fails, use these signals first:

- `Problem Details` payload indicates refusal shape and HTTP status.
- `invariant_code` identifies the exact trust-contract rule that failed.
- `trace_id` links the refusal to logs and diagnostics.
- Contract test failure text points to the specific contract rule or assertion.

### Trust Contract Identifiers

| `invariant_code` | Meaning |
|---|---|
| `ContextInitialized` | Tenant context must exist before any operation |
| `TenantAttributionUnambiguous` | Multiple attribution sources must agree |
| `TenantScopeRequired` | Tenant-scoped operations cannot proceed without a tenant |
| `BreakGlassExplicitAndAudited` | Privileged actions need actor, reason, and audit trail |
| `DisclosureSafe` | Logs and errors use disclosure-safe tenant references |

Full definitions: [trust contract](trust-contract.md) · [error catalog](error-catalog.md) · [integration guide](integration-guide.md)

If you see a mismatch between behavior and this guide, reproduce with the same commands above and update the guide with corrected steps.

## Troubleshooting

Common failures and actions:

- SDK mismatch: install required .NET 10 SDK and rerun.
- Restore/build issues: run `dotnet restore` and verify network/package feed access.
- Contract assertion failure: inspect reported `invariant_code`, then compare behavior against `docs/trust-contract.md`.
- Correlation troubleshooting: capture `trace_id` from Problem Details and match it with logs.

## Updating This Guide When Steps Change

1. Reproduce the failure with the commands in this guide.
2. Correct commands/expectations in `docs/verification-guide.md`.
3. Rerun verification tests until green.
4. Keep references aligned with `docs/trust-contract.md` identifiers.
