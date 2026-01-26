---
stepsCompleted: [1, 2, 3, 4, 5]
inputDocuments:
  - "_bmad-output/analysis/brainstorming-session-2026-01-22T23:05:54-08:00.md"
date: 2026-01-23T15:42:26-08:00
author: Vlad
---

# Product Brief: TenantSaas

<!-- Content will be appended sequentially through collaborative workflow steps -->

## Executive Summary

TenantSaas is a trust-first foundation for multi-tenant SaaS systems. It addresses a systemic safety problem: teams routinely perform shared-system actions with ambiguous ownership, scope, or authority, which leads to irreversible trust failures. The product encodes trust as explicit invariants, enforced at unavoidable choke points, so unsafe actions fail immediately and clearly. The goal is to make it hard to build multi-tenant systems incorrectly-and impossible to do so quietly.

---

## Core Vision

### Problem Statement

Multi-tenant SaaS systems regularly allow actions whose tenant ownership, scope, or authority is implicit or ambiguous. This ambiguity produces latent, catastrophic failures-cross-tenant exposure, corrupted state, and unverifiable incident response-because enforcement depends on human discipline rather than mechanical guarantees.

### Problem Impact

When the first cross-tenant violation occurs, trust is permanently damaged. Teams face legal and compliance exposure, velocity drops under fear, and systems accrete defensive complexity instead of clarity. On-call engineers, senior platform owners, founders, and security reviewers bear the earliest and most intense consequences because they are closest to the blast radius.

### Why Existing Solutions Fall Short

Most frameworks and libraries optimize for multi-tenancy capabilities, not guarantees. Enforcement is partial and contextual (HTTP middleware but not jobs), tenant attribution is often optional or implicit, privilege is treated as a role rather than a scoped event, and proof is absent. This creates safety gaps exactly where incidents occur and produces false confidence that persists until failure.

### Proposed Solution

A trust-first foundation that requires explicit attribution, refuses ambiguous actions, enforces invariants at a small set of unavoidable choke points, and makes exceptional power explicit and reviewable. The system shifts multi-tenant safety from "best practices" to "mechanically enforced guarantees."

### Key Differentiators

- Guarantees over capabilities: define what must never happen, then enforce it.
- Invariant-first design with unavoidable enforcement boundaries.
- Formal scope and privilege semantics; exceptional power is explicit and auditable.
- Executable proof of correctness (e.g., contract tests), not narrative assurance.
- Deliberately narrow surface area to preserve trust under stress.

## Target Users

### Primary Users

**Senior / Platform Engineers**  
They own system integrity over time and are accountable for failures caused by entropy, team growth, and inconsistent enforcement. They need mechanical guarantees, a shared mental model that scales across teams, and fewer ad-hoc exceptions to police. TenantSaas becomes a trust kernel and policy anchor they can standardize across services.

**Founders & Early SaaS Builders**  
They move fast and cannot afford one existential trust failure. They need protection without cognitive overhead, early guardrails that don’t slow iteration, and confidence they didn’t miss something fundamental. TenantSaas lets them ship while eliminating silent risk.

### Secondary Users

**On-Call / Incident Responders**  
They act under pressure with elevated privileges. They need clear blast-radius boundaries, safe inspection/repair paths, and guardrails that prevent accidental escalation.

**Security, Compliance, and Audit Stakeholders**  
They must validate guarantees that usually exist only as intent. They need inspectable controls, repeatable evidence, and clear answers to “how is this enforced?”

**New Engineers / Maintainers**  
They inherit implicit rules. They need a system that teaches correct behavior through constraints and provides fast feedback when actions are unsafe.

### User Journey

**Founders / Early SaaS Builders**  
Discovery arises during multi-tenant design, near-misses, or enterprise readiness. Onboarding is fast and opinionated: explicit attribution is required, unsafe defaults fail loudly. Day-to-day, it runs mostly invisibly, with occasional explicit scoping or privilege justification. Success is often negative-a dangerous mistake is prevented. Long-term, it becomes baseline infrastructure and fades into the background.

**Senior / Platform Engineers**  
Discovery follows incidents, inconsistent tenant handling, or org-wide standardization needs. Onboarding is deliberate: they inspect the trust contract, enforcement points, and contract tests, then adopt in high-risk areas first. Day-to-day, it serves as a policy anchor that constrains patterns and standardizes privilege handling. Success is governance that emerges without meetings-safe integration, contained incidents, and evidence-based audits. Long-term, it becomes part of platform standards and shapes new services by default.

## Success Metrics

User success is felt when TenantSaas fades into the background while making progress more reliable. Teams can model tenants, roles, and shared infrastructure without inventing new mental frameworks; early architectural decisions hold; growth is incremental rather than destabilizing; and the system remains understandable months later. The “worth it” moment is when teams realize they didn’t have to rethink multi-tenancy, rewrite core assumptions, or teach tribal rules to new engineers.

### Business Objectives

**~3 months (validation):**  
Validate resonance with serious builders (platform engineers, committed founders), prove TenantSaas is understood as a baseline/substrate, build credibility through depth and restraint, and confirm adoption without immediate forking. Success = “the right people are paying attention and taking it seriously.”

**~12 months (traction & positioning):**  
Establish TenantSaas as a reference baseline for multi-tenant SaaS architecture, keep it in place as systems grow, anchor governance and structure discussions, and pull follow-on extensions organically. Success = “this shaped how teams think about multi-tenant SaaS, not just how they scaffold it.”

### Key Performance Indicators

**Leading indicators (early, high-signal):**
- Projects integrating TenantSaas into non-toy codebases
- Retention after initial experimentation
- Frequency of users extending it rather than bypassing it
- Quality of inbound feedback (long-form issues, design discussions, PRs)
- References in blog posts, talks, or internal docs

**Adoption & engagement:**
- Active projects using TenantSaas over time
- Repeat usage across multiple projects by the same team/org
- Time from discovery to first meaningful integration
- Usage in CI or baseline tests

**Ecosystem & pull-through:**
- Requests for examples/patterns/extensions (not feature breadth)
- Community contributions that respect the core contract
- External tools or docs referencing TenantSaas concepts
- Mentions in discussions about SaaS architecture vs tooling

**Not optimized for:**
- Raw downloads or stars
- One-click onboarding metrics
- Feature requests that dilute the core
- Broad appeal to all .NET developers

## MVP Scope

### Core Features

The MVP is the smallest baseline that prevents early architectural regret. It delivers:

- A clear, explicit model for multi-tenant SaaS (tenant, shared infra, scope/context)
- A small set of enforced invariants (no ambiguous attribution; explicit exceptional behavior)
- A single unavoidable integration point that anchors the baseline early
- A proof mechanism (executable tests/assertions/verification hooks)

The MVP includes a minimal but opinionated core, a precise trust/baseline contract, and documentation that explains the model (not feature breadth).

### Out of Scope for MVP

Explicitly out of scope:

- Billing, subscriptions, pricing, metering
- Authentication / identity providers
- Authorization frameworks or RBAC systems
- UI scaffolding or admin dashboards
- Feature flags or plan-based behavior
- Multi-region or data residency support
- Database sharding or DB-per-tenant orchestration
- Tenant lifecycle workflows (provisioning/activation)
- Background job frameworks
- Observability platforms (metrics, dashboards, tracing)
- Enterprise compliance tooling (SOC2 automation, GDPR exports)
- Opinionated frontend stacks
- Turn-key SaaS templates

Anything that expands surface area, introduces product-specific policy, or can only be validated at scale is deferred.

### MVP Success Criteria

**Adoption & usage:**
- Integrated into non-toy projects
- Adopted early in the lifecycle
- Retained after initial experimentation
- Core model/API remains stable across projects

**Behavioral signals:**
- Teams stop inventing bespoke baselines
- Architectural debates about multi-tenancy shrink or disappear
- New engineers onboard without long verbal downloads
- Users extend around the baseline rather than bypass it

**Proof & credibility:**
- Concrete baseline artifacts exist and are referenced
- Verification hooks/contract tests are actually run
- Ambiguous actions are blocked or clarified early
- External reviewers quickly understand the role

**Negative signals that still indicate success:**
- Few requests for “everything else”
- Some users opt out because it’s opinionated
- Minimal churn due to missing conveniences

If removing it would feel risky, the MVP succeeded.

### Future Vision

TenantSaas becomes a de facto baseline and trust kernel for serious multi-tenant SaaS systems. The core stays small and conservative while an ecosystem of extensions, examples, integrations, and policy layers grows around it. It becomes the reference point for tenant modeling, scope handling, and shared-system governance-shaping how teams think about SaaS architecture, not just what they scaffold.
