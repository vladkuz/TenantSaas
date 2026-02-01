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

## References

- Code contracts: `TenantSaas.Abstractions.Tenancy` and `TenantSaas.Abstractions.Contexts`
- Validation: `TenantSaas.Abstractions.TrustContract.TrustContractV1`
