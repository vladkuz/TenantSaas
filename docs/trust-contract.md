# Trust Contract v1

## Version and Stability

- Version: v1
- Stability: additive changes only; existing identifiers and semantics are stable within v1
- Source of truth: code contracts in `TenantSaas.Abstractions`

## TenantScope

Tenant scope describes which tenant boundary applies to an execution.

- **Tenant**: scoped to a specific tenant identifier.
- **SharedSystem**: shared or cross-tenant system operations.
- **NoTenant**: explicit no-tenant context with a required reason.

## NoTenantReason

Allowed reasons for operating without tenant scope:

- **Public**: public or unauthenticated operations.
- **Bootstrap**: system initialization and bootstrap workflows.
- **HealthCheck**: health and readiness probes.
- **SystemMaintenance**: maintenance operations.

## ExecutionKind

Execution kind describes how the context was initiated:

- **Request**: HTTP or API request flow.
- **Background**: background job or worker flow.
- **Admin**: administrative operation flow.
- **Scripted**: CLI or script execution flow.

## Tenant Attribution Sources

Allowed sources for tenant attribution:

- **RouteParameter** (`route-parameter`): tenant identifier from a URL route parameter.
- **HeaderValue** (`header-value`): tenant identifier from a trusted HTTP header.
- **HostHeader** (`host-header`): tenant identifier inferred from the host or domain name.
- **TokenClaim** (`token-claim`): tenant identifier from authentication token claims.
- **ExplicitContext** (`explicit-context`): tenant identifier explicitly set during context initialization.

## Tenant Attribution Precedence

Attribution rules define which sources are enabled and how conflicts are resolved:

- **FirstMatch**: the first enabled source in precedence order that supplies a tenant wins.
- **AllMustAgree**: all enabled sources that supply a tenant must agree on the same tenant; otherwise attribution is ambiguous.

Precedence order is explicit and deterministic; rules must reject duplicate or ambiguous precedence definitions.

## Invariant Registry

Invariant definitions are stable, named, and versioned in the trust contract. Each invariant includes:

- **InvariantCode**: stable identifier (PascalCase string).
- **Name**: display name.
- **Description**: semantic meaning.
- **Category**: grouping for documentation and reporting.

Defined invariants for v1:

- **ContextInitialized**: Tenant context must be initialized before operations can proceed. (Category: Initialization)
- **TenantAttributionUnambiguous**: Tenant attribution from available sources must be unambiguous. (Category: Attribution)
- **TenantScopeRequired**: Operation requires an explicit tenant scope. (Category: Scope)
- **BreakGlassExplicitAndAudited**: Break-glass must be explicit with actor identity and reason. (Category: Authorization)
- **DisclosureSafe**: Tenant information disclosure must follow safe disclosure policy. (Category: Disclosure)

Lookups:

- `TrustContractV1.GetInvariant(code)` returns the registered definition or throws.
- `TrustContractV1.TryGetInvariant(code, out definition)` returns false if missing.

## Refusal Mapping Schema

Refusal mappings define how invariant violations become Problem Details responses:

- **invariant_code**: stable invariant identifier (string).
- **status**: HTTP status code (400-599).
- **type**: stable RFC 7807 type identifier (URN or URL).
- **title**: developer-facing summary.
- **guidance_uri**: remediation link.

Lookup:

- `TrustContractV1.GetRefusalMapping(invariantCode)` returns the mapping or throws.
- `TrustContractV1.TryGetRefusalMapping(invariantCode, out mapping)` returns false if missing.

### HTTP Status → Invariant Mapping

| Invariant | Status | Problem Type |
| --- | --- | --- |
| ContextInitialized | 400 | `urn:tenantsaas:error:context-initialized` |
| TenantAttributionUnambiguous | 422 | `urn:tenantsaas:error:tenant-attribution-unambiguous` |
| TenantScopeRequired | 403 | `urn:tenantsaas:error:tenant-scope-required` |
| BreakGlassExplicitAndAudited | 403 | `urn:tenantsaas:error:break-glass-explicit-and-audited` |
| DisclosureSafe | 500 | `urn:tenantsaas:error:disclosure-safe` |

### Problem Details Type Identifier Format

`urn:tenantsaas:error:{kebab-case-invariant-code}`

## Ambiguity and Refusal

If two or more allowed sources provide different tenant identifiers, attribution is **ambiguous** and must be refused.
Refusals include a stable invariant code and Problem Details type identifier:

- **Invariant code**: `InvariantCode.TenantAttributionUnambiguous`
- **Problem Details type**: `TrustContractV1.GetRefusalMapping(InvariantCode.TenantAttributionUnambiguous).ProblemType`

### Example Refusal Response

```json
{
  "type": "urn:tenantsaas:error:context-initialized",
  "title": "Tenant context not initialized",
  "status": 400,
  "detail": "Tenant context must be initialized before operations can proceed.",
  "instance": "/api/tenants/123",
  "invariant_code": "ContextInitialized",
  "trace_id": "trace-abc-123",
  "guidance_uri": "https://docs.tenantsaas.dev/errors/context-not-initialized"
}
```

## Attribution Examples

- **Web API**: allow `RouteParameter` and `TokenClaim`, require `AllMustAgree`.
- **Background/Admin**: allow `ExplicitContext` only, use `FirstMatch`.

## References

- Code contracts: `TenantSaas.Abstractions.Tenancy` and `TenantSaas.Abstractions.Contexts`
- Validation: `TenantSaas.Abstractions.TrustContract.TrustContractV1`
