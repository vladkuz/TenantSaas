---
stepsCompleted:
  - step-01-init
  - step-02-discovery
  - step-03-success
  - step-04-journeys
  - step-05-domain
  - step-06-innovation
  - step-07-project-type
  - step-08-scoping
  - step-09-functional
  - step-10-nonfunctional
  - step-11-polish
  - step-e-01-discovery
  - step-e-02-review
  - step-e-03-edit
inputDocuments:
  - "_bmad-output/planning-artifacts/product-brief-TenantSaas-2026-01-23T15:42:26-08:00.md"
  - "_bmad-output/analysis/brainstorming-session-2026-01-22T23:05:54-08:00.md"
workflowType: 'prd'
workflow: 'edit'
documentCounts:
  productBriefs: 1
  researchDocs: 0
  brainstormingDocs: 1
  projectDocs: 0
classification:
  projectType: developer_tool
  domain: general
  complexity: medium-to-high
  projectContext: greenfield
date: 2026-01-23T16:27:49-08:00
lastEdited: 2026-01-23T23:51:20-08:00
editHistory:
  - date: 2026-01-23T23:51:20-08:00
    changes: Added explicit problem statement, measurable success criteria/NFRs, and clarified FRs per validation findings.
author: Vlad
projectName: TenantSaas
---

# Product Requirements Document - TenantSaas

**Author:** Vlad
**Date:** 2026-01-23T16:27:49-08:00

## Executive Summary

TenantSaas is a trust-first baseline for multi-tenant SaaS systems that enforces tenant-scope invariants at unavoidable integration points. It targets platform engineers, founders, and security reviewers who need explicit, verifiable guarantees rather than framework-driven scaffolding. Differentiation comes from invariant-first design, explicit refusal behavior, and runnable contract tests that prove guarantees hold across execution paths.

**Problem Statement:** Multi-tenant systems routinely allow actions with ambiguous tenant scope or authority, creating latent cross-tenant risks that are only discovered after incidents.

## Success Criteria

### User Success

The primary "aha" is a posture shift: teams stop debating multi-tenant assumptions because the baseline makes them obvious.

Primary signals:
- New services/features fit cleanly into the baseline without redesign.
- Developers hit constraints early and think "good - this would have been messy later."
- The trust contract is treated as the default, not a suggestion.

Secondary signals:
- Dangerous operations are surfaced explicitly instead of silently allowed.
- New engineers can learn tenancy from code, not Slack history.

**Metrics (6 months):**
- >=3 new services integrate TenantSaas within 2 sprints of project start.
- >=80% of new multi-tenant features ship without baseline redesign.
- >=90% of services keep the baseline enabled after initial adoption.
- New engineer onboarding to tenancy concepts takes <=1 day using docs + contract tests.

### Business Success

**~3 months (validation):**
- Integrated into >=3 real (non-demo) projects.
- >=50% of integrations happen at project start (greenfield).
- >=70% of adopters keep it after initial experimentation.
- >=3 feedback items explicitly reference it as a "baseline" or "foundation."
- >=2 internal docs or architecture notes cite it.
Validation: the right users take it seriously and keep it.

**~12 months (traction):**
- Repeat usage by >=2 teams across >=2 projects each.
- Annual retention >=80% among adopters.
- >=3 external or internal mentions in blogs, architecture forums, or platform standards.
- >=5 pull-through requests for examples/extensions/integrations (not core expansion).
- >=2 teams describe it as infrastructure they would not remove without concern.
Validation: it is treated as infrastructure, not a trial tool.

### Technical Success

- Core invariants are enforced through unavoidable integration points.
- Ambiguous or unattributed operations are refused by default.
- Contract/verification tests exist, are runnable by adopters, and are executed in CI.
- The core API surface stays small, stable, and resists ad-hoc extension.
- Behavior is consistent across request paths, background execution, and admin/scripted usage.
Technical success = guarantees hold even as usage patterns expand.

**Metrics (technical):**
- 100% of defined invariants are enforced by contract tests across request, background, and admin paths.
- Contract test suite runs in CI in <=10 minutes on reference projects.
- Zero P0/P1 incidents caused by tenant-scope violations in adopter projects during first 6 months.

### Measurable Outcomes

- Multiple real projects integrate TenantSaas, with at least some greenfield starts.
- Retention after initial experimentation (kept in place).
- Documented references in internal architecture notes.
- Contract tests running in CI across adopters.
- Repeat adoption across projects by the same teams.

**Metrics (measurable outcomes):**
- >=3 real integrations, with >=1 greenfield.
- >=70% retention after initial experimentation.
- >=2 documented references in internal architecture notes.
- >=2 adopters running contract tests in CI.
- >=2 repeat adoptions by the same team/org.

## Product Scope

### MVP - Minimum Viable Product

- Explicit baseline model for multi-tenant SaaS.
- One or more enforced invariants that remove ambiguity early.
- A single unavoidable integration point anchoring the baseline.
- Verification mechanism (tests/assertions) that proves rules hold.
- Documentation that explains the model and contract (not features).
- UX documentation is not required for this project.

### Growth Features (Post-MVP)

- Optional extensions/adapters.
- Reference implementations or example apps.
- Additional verification/reporting hooks.
- Integration patterns for common tooling.
- Better ergonomics around configuration and visibility.

### Vision (Future)

- Recognized baseline for serious multi-tenant systems.
- Core remains small and conservative.
- Innovation primarily in extensions/examples/ecosystem contributions.
- Shapes how teams think about SaaS foundations, not just scaffolding.
- Referenced as a trust kernel, not marketed as a framework.

## User Journeys

### 1) Platform engineer integrating TenantSaas into a new service

**Opening scene:** Maya owns platform standards and is starting a new service in a growing SaaS stack. She is wary of tenant ambiguity because prior teams had to retrofit rules later.
**Rising action:** She evaluates TenantSaas as a baseline, finds a small core package plus contract tests, and identifies the single integration point that every request path must pass through.
**Climax:** When wiring the first endpoint, the baseline refuses an operation without explicit tenant attribution. It forces a decision at design time, not after launch.
**Resolution:** Maya adopts the trust contract as a shared team standard. The new service fits into existing architecture without special cases, and future services reuse the same baseline without re-debate.

### 2) On-call responder using elevated scope during an incident (edge case)

**Opening scene:** Jordan is on call when a cross-tenant data correction is required under time pressure.
**Rising action:** Jordan attempts a privileged operation; the system requires explicit scope elevation with clear intent, not silent admin access.
**Climax:** The system allows the operation only through a constrained path that is visible and auditable, avoiding accidental overreach.
**Resolution:** The fix is applied quickly without bypassing the baseline. The incident is resolved and the audit trail is clean, with no special exceptions introduced.

### 3) Founder / early builder adopting at project start

**Opening scene:** Priya is starting a new SaaS product and wants to avoid fixing multi-tenancy later.
**Rising action:** She adds TenantSaas early because it is small, explicit, and does not prescribe a full framework.
**Climax:** The baseline forces clarity around tenant context at the first integration point, and Priya realizes she does not need to invent her own conventions.
**Resolution:** The team moves fast with confidence, knowing the core assumptions are locked in without slowing momentum.

### 4) Security/compliance stakeholder reviewing guarantees

**Opening scene:** Alex is asked to assess tenant isolation guarantees before an external review.
**Rising action:** Alex inspects the trust contract and verification artifacts rather than chasing tribal knowledge.
**Climax:** The evidence shows invariants enforced at unavoidable choke points, with contract tests that can be executed in CI.
**Resolution:** Alex can approve the baseline as a credible control, because the guarantees are explicit and verifiable, not implied.

### 5) API/integration contributor extending or adapting the core

**Opening scene:** Sam wants to create an adapter for a different stack while keeping the core intact.
**Rising action:** Sam reviews the constraints and sees where extensions are allowed versus where invariants are non-negotiable.
**Climax:** The adapter integrates through the sanctioned boundary and inherits the same guarantees without widening the core API.
**Resolution:** The extension adds reach without fragmenting the baseline, and the core remains intentionally small.

### 6) New engineer onboarding into an existing system (acceptance signal)

**Opening scene:** Riley joins an existing team mid-project.
**Rising action:** Riley reads the baseline code and its contract tests rather than relying on oral history.
**Climax:** A constrained operation makes the rules explicit, and the correct pattern is discoverable in code.
**Resolution:** Onboarding requires fewer explanations, which acts as an acceptance signal that the baseline is doing its job.

### Journey Requirements Summary

- A single, unavoidable integration point for tenant context.
- Explicit tenant attribution required before operations proceed.
- Controlled, auditable elevation paths for privileged actions.
- A clear, inspectable trust contract with runnable verification tests.
- An extension model that preserves invariants and keeps the core small.
- Baseline conventions are discoverable in code, not tribal knowledge.

## Developer Tool Specific Requirements

### Project-Type Overview

TenantSaas is a developer-facing foundation package for .NET-first multi-tenant systems, with a secondary npm distribution. It is embedded into existing systems as a baseline, not a standalone product.

### Technical Architecture Considerations

- The core package must be minimal, invariant-driven, and safe by default.
- A single, unavoidable integration point anchors tenant context for request paths and background execution.
- Verification artifacts (contract tests) are part of the core deliverable and runnable by adopters.

### Language Support (Language Matrix)

- **MVP support:** .NET
- **Secondary distribution:** npm package (for complementary tooling or examples)
- **Out of scope for MVP:** other runtimes/languages

### Installation Methods

- **NuGet** as the primary installation path.
- **npm** for any supporting tooling or integration snippets.
- No IDE/editor extensions in scope for MVP.

### API Surface

- Small, stable API surface focused on tenant context, invariants, and verification hooks.
- Avoid extension points that bypass invariants or widen core behavior.
- Explicit refusal paths for ambiguous attribution or privileged actions.

### Documentation & Guides (Required for MVP)

- Concise conceptual model.
- Explicit trust contract.
- Define `tenant_ref`: a disclosure-safe tenant identifier used in logs/errors; either an opaque public tenant ID or a safe-state token (`unknown`, `sensitive`, `cross_tenant`) when disclosure is unsafe.
- Minimal integration guide.
- Runnable verification/contract tests guide.
- Small API reference (focused on the core surface only).

### Code Examples

- One minimal sample project.
- Copy-paste integration snippets.
- No full reference application for MVP.

### Migration Guide

- MVP: no migration guide required (greenfield-first baseline).
- Future: add a migration guide if adopting into existing systems becomes a primary use case.

### Implementation Considerations

- Baseline must be hard to remove once integrated, without adding operational friction.
- Contract tests should be easily wired into CI with minimal configuration.
- Keep documentation crisp and enforce the mental model over feature descriptions.

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Problem-solving MVP (prove guarantees are real and usable), with platform discipline for extensibility.
**Resource Requirements:** 1 person with strong backend/systems thinking, multi-tenant architecture expertise, API/contract design, test rigor, and clear technical writing.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- Platform engineer integrating into a new service
- Founder/early builder starting a project
- Security/compliance reviewer inspecting guarantees
- Contributor reading/extending the core (design-level)

**Must-Have Capabilities:**
- Explicit baseline model (tenancy, shared system, scope)
- One unavoidable integration point
- Enforced invariants (small, non-negotiable)
- Verification mechanism (contract tests or equivalent)
- Clear non-goals and refusal behavior
- Minimal sample + integration snippets
- Conceptual + contract documentation

### Post-MVP Features

**Phase 2 (Post-MVP):**
- On-call incident workflows (documented, not deeply supported)
- New-hire onboarding narratives
- Advanced ops/support tooling
- Additional adapters or helpers
- Observability hooks
- More ergonomic APIs
- Extended examples (jobs, reporting, admin)

**Phase 3 (Expansion):**
- Reference app
- Ecosystem packages and optional extensions

### Risk Mitigation Strategy

**Technical Risks:** Guarantees too weak to matter or too rigid to use. Mitigate by keeping invariants minimal, validating in at least one real integration, and treating escape hatches as design failures unless explicitly justified.
**Market Risks:** Misinterpreted as just another library or too abstract. Mitigate by leading with problem framing, stating non-goals clearly, and anchoring messaging on baseline/foundation rather than productivity.
**Resource Risks:** Over-engineering and drifting into framework territory. Mitigate by freezing scope, requiring a new invariant for any new capability, and deleting ideas rather than deferring them.

## Functional Requirements

### Tenant Context & Baseline Model

- FR1: Platform engineers can define the canonical tenant identity model used across the system.
- FR2: The system can represent tenant scope and shared-system scope as explicit, distinct contexts.
- FR3: The system can represent a no tenant state only when explicitly justified by the trust contract.
- FR4: Developers can attach tenant context explicitly to operations before execution.
- FR5: The baseline model can be referenced as a single source of truth by services integrating it.

### Invariant Enforcement & Refusal Behavior

- FR6: The system can refuse operations with ambiguous or missing tenant attribution.
- FR7: The system can enforce an explicit invariant set of <=7 invariants defined and enumerated in the trust contract.
- FR8: Invariant enforcement applies uniformly across request, background, administrative, and scripted execution paths.
- FR9: The system can surface refusal reasons explicitly to developers when invariants are violated.
- FR10: Privileged or cross-tenant operations require explicit intent and cannot proceed silently.

### Integration Point & Context Propagation

- FR11: The system provides a single, unavoidable integration point for tenant context initialization.
- FR12: The integration point can propagate tenant context to downstream operations consistently.
- FR13: The integration point can reject execution when required context is absent or inconsistent.
- FR14: Services can integrate the baseline without adopting a full framework or templated scaffold.

### Verification & Contract Tests

- FR15: The system provides runnable verification artifacts that test invariant enforcement.
- FR16: Teams can execute contract tests as part of CI to prove baseline adherence.
- FR17: Contract tests can be run by adopters without specialized tooling beyond the package.
- FR18: Verification artifacts can demonstrate behavior across multiple execution contexts.

### Documentation & Trust Contract

- FR19: The system provides a conceptual model describing tenancy, scope, and shared-system context in <=800 words or <=2 pages.
- FR20: The system provides an explicit trust contract that defines invariants and refusal behavior.
- FR21: The system provides an integration guide with <=6 required steps and a reference setup time <=30 minutes.
- FR22: The system provides a verification guide that explains how to run and interpret contract tests.
- FR23: The system provides an API reference that covers 100% of public surface area and lists all public types/entry points.

### Extensibility & Adapter Boundaries

- FR24: The system defines explicit boundaries where extensions may be built without weakening invariants.
- FR25: Adapters can integrate through sanctioned boundaries while preserving the trust contract.
- FR26: The core surface blocks bypass paths by restricting entry points to sanctioned boundaries, verified by contract tests covering 100% of public entry points.

### Developer Onboarding & Discoverability (Acceptance Signals Only)

- FR27: New engineers can locate the trust contract and run contract tests within 60 minutes using the docs alone.
- FR28: 100% of refusal errors include actionable guidance and a link to the relevant contract rule, verified by contract tests.

## Non-Functional Requirements

### Security

- The system shall reject 100% of operations with missing or ambiguous tenant attribution as measured by contract tests covering request, background, and admin paths.
- Privileged or cross-scope actions shall require explicit scope declaration and justification fields, verified by contract tests for all privileged operations.
- The system shall produce zero silent fallbacks when required context is missing, verified by negative tests in CI.
- Security-relevant behavior shall be covered by contract tests with >=90% branch coverage in the enforcement module, measured in CI.

### Reliability

- Invariant enforcement shall be deterministic across 10 repeated CI runs with identical inputs yielding identical outcomes (0 variance).
- Failure modes shall return explicit error codes/messages for 100% of invariant violations, verified by contract tests.
- Invariant check failures shall return within <=100ms at p95 in reference benchmarks.
- Core guarantees shall pass the same contract tests across local and CI environments with 0 environment-specific skips.

### Integration

- All request paths shall pass through the single integration point in the reference project, verified by tracing tests with 100% coverage.
- Integration shall require no changes to domain/business logic in the reference project (0 files under domain namespaces modified), verified by sample diff.
- The integration guide shall include a full wiring example and complete successfully in <=30 minutes for a new service, measured in onboarding trials.
- If the integration point is removed or bypassed, contract tests shall fail in CI with a specific error within a single test run.

### Performance

- Baseline overhead shall add <=1ms at p95 per request in a reference benchmark of 10,000 requests.
- Enforcement checks shall add <=5% latency when tenant count scales from 1 to 10,000, measured by benchmark.
- The baseline shall start zero background polling loops or timers by default, verified by runtime inspection tests.

### Scalability

- Contract tests shall pass with tenant counts of 1, 100, and 10,000 in load simulations.
- Reference architecture shall demonstrate multi-service and multi-database topology without special casing, verified by documented example and integration test.
- All invariants shall remain enforced under 10x load compared to baseline, verified by load tests.

### Accessibility

- Not applicable. TenantSaas has no end-user UI surface.
