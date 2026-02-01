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

## Ambiguity and Refusal

If two or more allowed sources provide different tenant identifiers, attribution is **ambiguous** and must be refused.
Refusals include a stable invariant code and Problem Details type identifier:

- **Invariant code**: `InvariantCode.TenantAttributionUnambiguous`
- **Problem Details type**: `TrustContractV1.ProblemTypeTenantAttributionUnambiguous`

## Attribution Examples

- **Web API**: allow `RouteParameter` and `TokenClaim`, require `AllMustAgree`.
- **Background/Admin**: allow `ExplicitContext` only, use `FirstMatch`.

## References

- Code contracts: `TenantSaas.Abstractions.Tenancy` and `TenantSaas.Abstractions.Contexts`
- Validation: `TenantSaas.Abstractions.TrustContract.TrustContractV1`
