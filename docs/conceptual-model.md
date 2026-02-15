# TenantSaas Conceptual Model

This conceptual model gives adopters a fast map of how TenantSaas protects tenancy and tenant boundaries. It is intentionally high-level. For normative rules and identifiers, see the [Trust Contract](trust-contract.md) (`docs/trust-contract.md`).

## Core Idea

TenantSaas is an invariant-first trust layer. Application code may vary, but boundary behavior is fixed and testable:

- Context must be initialized before protected operations.
- Tenant attribution must be unambiguous.
- Tenant scope must be explicit.
- Privileged bypasses must be explicit and audited.
- Disclosure must remain safe in errors and logs.

These rules are stable through the trust contract and enforced by boundary guards.

## Scope Model (TenantScope)

`TenantScope` states what boundary applies to an execution:

- `Tenant`: operation is bound to one tenant.
- `SharedSystem`: operation is intentionally cross-tenant or system-level.
- `NoTenant`: operation has no tenant and must carry an explicit reason.

This prevents accidental scope drift. If an operation requires tenant context and scope is missing or invalid, enforcement refuses execution.

## Execution Model (ExecutionKind)

`ExecutionKind` describes how work started, independent of business domain:

- `Request`: HTTP/API flow.
- `Background`: worker/job flow.
- `Admin`: administrative flow.
- `Scripted`: CLI/script flow.

Execution kind is first-class context, not implied metadata. Downstream enforcement, logging, and auditing use it to keep behavior consistent across environments.

## Shared-System Context

Shared-system work is explicit, not a fallback. `TenantScope.SharedSystem` marks operations that intentionally span tenant boundaries, such as platform maintenance or global reporting.

Two constraints apply:

- Shared-system operations still require initialized context and invariant checks.
- Privileged behavior is not automatic; break-glass declarations remain mandatory where required.

This model allows controlled cross-tenant behavior without weakening tenant isolation defaults.

## Invariant-Driven Boundary Model

TenantSaas centers boundary enforcement around named invariants from the trust contract:

- `ContextInitialized`
- `TenantAttributionUnambiguous`
- `TenantScopeRequired`
- `BreakGlassExplicitAndAudited`
- `DisclosureSafe`

When a boundary check fails, the system returns contract-aligned Problem Details with stable `invariant_code` values. This creates a shared language for developers, operators, and adopters:

- Developers get deterministic refusal semantics.
- Integrators get predictable remediation targets.
- Operations and support get correlation fields (`trace_id`, `request_id`) for incident handling.

## Extension and Stability Boundaries

TenantSaas allows customization through named seams (for example, attribution resolution and logging enrichment) while preserving invariant semantics. In short:

- Adapters and wiring are customizable.
- Trust contract invariants are not.

That separation lets adopters integrate with different host architectures while keeping the same safety guarantees.

## Mental Checklist for Adopters

When reviewing an integration, ask:

1. Is context initialized once per flow and cleared appropriately?
2. Is `TenantScope` explicit for every protected operation?
3. Are attribution sources configured to avoid ambiguity?
4. Are privileged paths break-glass gated and audited?
5. Do refusal responses and logs carry contract identifiers?

If these answers are yes, the conceptual model is correctly applied.
