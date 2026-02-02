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

## Break-Glass Contract

Break-glass declarations are explicit and never implicit or default. Privileged or cross-tenant operations require a declaration with:

- **actorId**: identity of the actor invoking break-glass (required).
- **reason**: justification for the escalation (required).
- **declaredScope**: scope being claimed, e.g., `tenant`, `cross-tenant`, `shared-system` (required).
- **targetTenantRef**: target tenant reference; `null` indicates cross-tenant.
- **timestamp**: UTC timestamp for the declaration.

Markers:

- **cross_tenant**: marker for cross-tenant audit events (`TrustContractV1.BreakGlassMarkerCrossTenant`).
- **privileged**: marker for privileged operations (`TrustContractV1.BreakGlassMarkerPrivileged`).

## Break-Glass Audit Event Schema

Break-glass audit events are stable and include the following fields:

- **actor**: actor identity from declaration.
- **reason**: reason from declaration.
- **scope**: declared scope.
- **tenantRef**: target tenant reference or `cross_tenant` marker.
- **traceId**: correlation trace identifier.
- **auditCode**: stable audit event type code.
- **invariantCode**: invariant being bypassed (optional).
- **timestamp**: UTC timestamp for the event.
- **operationName**: operation being performed (optional).

Audit codes:

- **BreakGlassInvoked**
- **BreakGlassAttemptDenied**
- **CrossTenantAccess**
- **PrivilegedEscalation**

### Example Break-Glass Audit Event

```json
{
  "actor": "admin@example.com",
  "reason": "Emergency data correction for incident INC-12345",
  "scope": "cross-tenant",
  "tenantRef": "cross_tenant",
  "traceId": "abc123-def456-ghi789",
  "auditCode": "BreakGlassInvoked",
  "invariantCode": "TenantScopeRequired",
  "timestamp": "2026-02-01T10:30:00Z",
  "operationName": "UpdateUserData"
}
```

## Disclosure Policy

Tenant disclosure policy prevents tenant existence oracles by defining safe tenant reference states and when tenant information may appear in errors or logs.

### Tenant Reference Safe States

`tenant_ref` MUST be one of:

- **opaque tenant id**: a stable public identifier safe to disclose.
- **unknown**: tenant is unresolved or attribution failed.
- **sensitive**: tenant resolved but unsafe to disclose.
- **cross_tenant**: admin or global cross-tenant operations.
- **opaque**: marker indicating the value is an opaque public tenant identifier.

### Error Disclosure Rules

Tenant information MAY appear in Problem Details only when all conditions are met:

- Caller is authenticated.
- Caller is authorized for the tenant.
- Disclosure does not create an enumeration risk.

Otherwise, tenant information must be omitted.

### Log Disclosure Rules

Structured logs MUST always include `tenant_ref`, but only as safe states or an opaque tenant id. Logs must never contain raw internal tenant identifiers.

### Examples

- **Unauthorized caller probes tenant** → `tenant_ref="sensitive"` in logs, omitted in errors.
- **Authorized caller** → `tenant_ref="<opaque id>"` in logs and errors.
- **Health check / NoTenant** → `tenant_ref="unknown"` in logs and errors.
- **SharedSystem** → `tenant_ref="cross_tenant"` in logs and errors.

### References

- Code: `TenantSaas.Abstractions.Disclosure`
- Contract tests: `TenantSaas.ContractTests.DisclosurePolicyTests`

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

- Code contracts: `TenantSaas.Abstractions.Tenancy`, `TenantSaas.Abstractions.Contexts`, `TenantSaas.Abstractions.BreakGlass`
- Validation: `TenantSaas.Abstractions.TrustContract.TrustContractV1`
