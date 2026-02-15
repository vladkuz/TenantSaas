# Extension Seams (Trust Contract v1)

This document names the supported extension seams and the invariant boundaries they must respect.
These seams are customization points; they are not alternate enforcement paths.

## Seam Summary

| Seam | Contract / Type | Owning Project |
| --- | --- | --- |
| Tenant attribution resolution | `ITenantAttributionResolver` | `TenantSaas.Abstractions` |
| Tenant context access | `ITenantContextAccessor`, `IMutableTenantContextAccessor` | `TenantSaas.Abstractions` |
| Invariant evaluation | `IBoundaryGuard` | `TenantSaas.Core` |
| Refusal mapping | `TrustContractV1.RefusalMappings` | `TenantSaas.Abstractions` |
| Log enrichment | `ILogEnricher`, `DefaultLogEnricher` | `TenantSaas.Abstractions`, `TenantSaas.Core` |

---

## Tenant attribution resolution

- Owning interface: `TenantSaas.Abstractions/Tenancy/ITenantAttributionResolver.cs`
- Customizable:
  - How tenant identifiers are resolved from the provided sources.
  - Rules interpretation for execution kind and endpoint overrides.
  - Deterministic resolution strategy (must remain side-effect-free).
- Invariant-protected:
  - Attribution ambiguity must be refused at boundaries.
  - Attribution results must still be enforced by `IBoundaryGuard.RequireUnambiguousAttribution`.
- Contract tests:
  - `TenantSaas.ContractTests/AttributionEnforcementTests.cs`
  - `TenantSaas.ContractTests/ReferenceComplianceRefusalAttributionTests.cs`
  - `TenantSaas.ContractTests/AttributionRulesTests.cs`

## Tenant context access

- Owning interfaces:
  - `TenantSaas.Abstractions/Tenancy/ITenantContextAccessor.cs`
  - `TenantSaas.Abstractions/Tenancy/IMutableTenantContextAccessor.cs`
- Customizable:
  - Context propagation strategy (ambient `AsyncLocal`, explicit passing, or other deterministic mechanisms).
  - Storage lifecycle behavior for set/clear in middleware or flow wrappers.
- Invariant-protected:
  - Uninitialized context must be refused at boundaries.
  - Accessors must not enable bypassing `ContextInitialized` enforcement.
- Contract tests:
  - `TenantSaas.ContractTests/ContextInitializedTests.cs`
  - `TenantSaas.ContractTests/InitializationEnforcementTests.cs`
  - `TenantSaas.ContractTests/AmbientContextPropagationTests.cs`
  - `TenantSaas.ContractTests/ExtensionSeamsEnforcementTests.cs`

## Invariant evaluation

- Owning interface: `TenantSaas.Core/Enforcement/IBoundaryGuard.cs`
- Customizable:
  - Implementations may emit logs or integrate with audit sinks.
  - Boundary guard wiring and composition within host applications.
- Invariant-protected:
  - Context initialization, attribution ambiguity, and break-glass rules must be enforced.
  - No alternate entry points may bypass guard checks.
- Contract tests:
  - `TenantSaas.ContractTests/ContextInitializedTests.cs`
  - `TenantSaas.ContractTests/InitializationEnforcementTests.cs`
  - `TenantSaas.ContractTests/Enforcement/BreakGlassEnforcementTests.cs`

## Refusal mapping

- Owning type: `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs`
- Customizable:
  - None in v1; consumers may read mappings but must not alter invariant codes, HTTP status, or problem types.
- Invariant-protected:
  - Refusal mappings must remain stable for v1 invariants.
  - All refusal mapping lookups must use `TrustContractV1` definitions.
- Contract tests:
  - `TenantSaas.ContractTests/InvariantRegistryTests.cs`
  - `TenantSaas.ContractTests/Errors/ProblemDetailsFactoryTests.cs`
  - `TenantSaas.ContractTests/Errors/ProblemDetailsShapeTests.cs`

## Log enrichment

- Owning interfaces/types:
  - `TenantSaas.Abstractions/Logging/ILogEnricher.cs`
  - `TenantSaas.Core/Logging/DefaultLogEnricher.cs`
- Customizable:
  - Structured event shaping and correlation ID extraction.
  - Optional integration with external logging pipelines.
- Invariant-protected:
  - Disclosure policy must be enforced (`tenant_ref` must be safe-state or opaque public ID).
  - Required structured fields must always be present.
- Contract tests:
  - `TenantSaas.ContractTests/ExecutionKindAndScopeTests.cs`
  - `TenantSaas.ContractTests/Logging/EnforcementLoggingTests.cs`

---

## Required Doc Test

The following test asserts this document exists and lists all required seams:

- `TenantSaas.ContractTests/ExtensionSeamsDocumentationTests.cs`
