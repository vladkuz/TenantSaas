# API Reference

This reference is the canonical public API surface for `TenantSaas.Abstractions`, `TenantSaas.Core`, and `TenantSaas.Sample`.

## Scope

This reference covers the three primary packages: `TenantSaas.Abstractions`, `TenantSaas.Core`, and `TenantSaas.Sample`. The reference adapter (`TenantSaas.EfCore`) and the contract test kit (`TenantSaas.ContractTestKit`) are documented separately and are not included here.

## Update Contract

- Public surface area changes in these projects must update this file in the same change.
- Contract identifiers remain stable within major version `v1` (`TrustContractV1` and `InvariantCode` values).
- Documentation tests in `TenantSaas.ContractTests/ApiReferenceDocumentationTests.cs` enforce presence and coverage.

## TenantSaas.Abstractions

### TenantSaas.Abstractions.Contexts

- `ExecutionKind`

### TenantSaas.Abstractions.Tenancy

- `TenantScope`
- `NoTenantReason`
- `TenantId`
- `TenantContext`
- `TenantAttributionSource`
- `TenantAttributionSourceMetadata`
- `AttributionStrategy`
- `TenantAttributionRuleSet`
- `TenantAttributionRules`
- `TenantAttributionInput`
- `TenantAttributionInputs`
- `TenantAttributionResult`
- `AttributionConflict`
- `ITenantAttributionResolver`
- `ITenantContextAccessor`
- `IMutableTenantContextAccessor`
- `ITenantContextInitializer`
- `ITenantFlowFactory`
- `ITenantFlowScope`

### TenantSaas.Abstractions.Invariants

- `InvariantCode`
- `InvariantDefinition`
- `RefusalMapping`

### TenantSaas.Abstractions.TrustContract

- `TrustContractV1`
- `TrustContractValidationResult`

### TenantSaas.Abstractions.Disclosure

- `DisclosurePolicy`
- `DisclosureContext`
- `DisclosureValidator`
- `DisclosureValidationResult`
- `TenantRef`
- `TenantRefSafeState`
- `IDisclosurePolicyProvider`

### TenantSaas.Abstractions.BreakGlass

- `BreakGlassDeclaration`
- `BreakGlassValidator`
- `BreakGlassValidationResult`
- `BreakGlassAuditEvent`
- `AuditCode`
- `IBreakGlassAuditSink`

### TenantSaas.Abstractions.Logging

- `StructuredLogEvent`
- `ILogEnricher`

## TenantSaas.Core

### Enforcement

- `IBoundaryGuard`
- `BoundaryGuard`
- `EnforcementResult`
- `AttributionEnforcementResult`
- `BreakGlassAuditHelper`

### Tenancy

- `TenantContextInitializer`
- `TenantAttributionResolver`
- `TenantFlowFactory`
- `AmbientTenantContextAccessor`
- `ExplicitTenantContextAccessor`
- `TenantContextConflictException`

### Errors

- `ProblemDetailsFactory`
- `ProblemDetailsExtensions`

### Logging

- `DefaultLogEnricher`
- `EnforcementEventSource`
- `EnforcementEventNames`
- `LoggingDefaults`

## TenantSaas.Sample

### Public Types

- `SampleApp`
- `Program`
- `TenantContextMiddleware`
- `ProblemDetailsExceptionMiddleware`
- `HttpCorrelationExtensions`

### Public HTTP Endpoints

| Method | Route | Name | Notes |
| --- | --- | --- | --- |
| GET | `/health` | n/a | Health status endpoint |
| GET | `/tenants/{tenantId}/data` | `GetTenantData` | Route + header attribution enforcement sample |
| GET | `/test/attribution` | `TestAttribution` | Header-only attribution sample |
| GET | `/weatherforecast` | `GetWeatherForecast` | Sample endpoint |

## Extension Seams

- `ITenantAttributionResolver`
- `ITenantContextAccessor`
- `IMutableTenantContextAccessor`
- `IBoundaryGuard`
- `ILogEnricher`

Reference: `docs/extension-seams.md`.

## Trust Contract Identifiers

- `ContextInitialized`
- `TenantAttributionUnambiguous`
- `TenantScopeRequired`
- `BreakGlassExplicitAndAudited`
- `DisclosureSafe`

Reference: `docs/trust-contract.md`.
