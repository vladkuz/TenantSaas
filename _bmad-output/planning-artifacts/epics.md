---
stepsCompleted:
  - step-01-validate-prerequisites
  - step-02-design-epics
  - step-03-create-stories
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
---

# TenantSaas - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for TenantSaas, decomposing the requirements from the PRD, UX Design if it exists, and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Platform engineers can define the canonical tenant identity model used across the system.
FR2: The system can represent tenant scope and shared-system scope as explicit, distinct contexts.
FR3: The system can represent a no tenant state only when explicitly justified by the trust contract.
FR4: Developers can attach tenant context explicitly to operations before execution.
FR5: The baseline model can be referenced as a single source of truth by services integrating it.
FR6: The system can refuse operations with ambiguous or missing tenant attribution.
FR7: The system can enforce an explicit invariant set of <=7 invariants defined and enumerated in the trust contract.
FR8: Invariant enforcement applies uniformly across request, background, administrative, and scripted execution paths.
FR9: The system can surface refusal reasons explicitly to developers when invariants are violated.
FR10: Privileged or cross-tenant operations require explicit intent and cannot proceed silently.
FR11: The system provides a single, unavoidable integration point for tenant context initialization.
FR12: The integration point can propagate tenant context to downstream operations consistently.
FR13: The integration point can reject execution when required context is absent or inconsistent.
FR14: Services can integrate the baseline without adopting a full framework or templated scaffold.
FR15: The system provides runnable verification artifacts that test invariant enforcement.
FR16: Teams can execute contract tests as part of CI to prove baseline adherence.
FR17: Contract tests can be run by adopters without specialized tooling beyond the package.
FR18: Verification artifacts can demonstrate behavior across multiple execution contexts.
FR19: The system provides a conceptual model describing tenancy, scope, and shared-system context in <=800 words or <=2 pages.
FR20: The system provides an explicit trust contract that defines invariants and refusal behavior.
FR21: The system provides an integration guide with <=6 required steps and a reference setup time <=30 minutes.
FR22: The system provides a verification guide that explains how to run and interpret contract tests.
FR23: The system provides an API reference that covers 100% of public surface area and lists all public types/entry points.
FR24: The system defines explicit boundaries where extensions may be built without weakening invariants.
FR25: Adapters can integrate through sanctioned boundaries while preserving the trust contract.
FR26: The core surface blocks bypass paths by restricting entry points to sanctioned boundaries, verified by contract tests covering 100% of public entry points.
FR27: New engineers can locate the trust contract and run contract tests within 60 minutes using the docs alone.
FR28: 100% of refusal errors include actionable guidance and a link to the relevant contract rule, verified by contract tests.

### NonFunctional Requirements

NFR1: The system shall reject 100% of operations with missing or ambiguous tenant attribution as measured by contract tests covering request, background, and admin paths.
NFR2: Privileged or cross-scope actions shall require explicit scope declaration and justification fields, verified by contract tests for all privileged operations.
NFR3: The system shall produce zero silent fallbacks when required context is missing, verified by negative tests in CI.
NFR4: Security-relevant behavior shall be covered by contract tests with >=90% branch coverage in the enforcement module, measured in CI.
NFR5: Invariant enforcement shall be deterministic across 10 repeated CI runs with identical inputs yielding identical outcomes (0 variance).
NFR6: Failure modes shall return explicit error codes/messages for 100% of invariant violations, verified by contract tests.
NFR7: Invariant check failures shall return within <=100ms at p95 in reference benchmarks.
NFR8: Core guarantees shall pass the same contract tests across local and CI environments with 0 environment-specific skips.
NFR9: All request paths shall pass through the single integration point in the reference project, verified by tracing tests with 100% coverage.
NFR10: Integration shall require no changes to domain/business logic in the reference project (0 files under domain namespaces modified), verified by sample diff.
NFR11: The integration guide shall include a full wiring example and complete successfully in <=30 minutes for a new service, measured in onboarding trials.
NFR12: If the integration point is removed or bypassed, contract tests shall fail in CI with a specific error within a single test run.
NFR13: Baseline overhead shall add <=1ms at p95 per request in a reference benchmark of 10,000 requests.
NFR14: Enforcement checks shall add <=5% latency when tenant count scales from 1 to 10,000, measured by benchmark.
NFR15: The baseline shall start zero background polling loops or timers by default, verified by runtime inspection tests.
NFR16: Contract tests shall pass with tenant counts of 1, 100, and 10,000 in load simulations.
NFR17: Reference architecture shall demonstrate multi-service and multi-database topology without special casing, verified by documented example and integration test.
NFR18: All invariants shall remain enforced under 10x load compared to baseline, verified by load tests.
NFR19: Not applicable. TenantSaas has no end-user UI surface.

### Additional Requirements

- Starter template required: .NET SDK default templates; first implementation story is initializing the monorepo via documented `dotnet new` commands.
- .NET 10 target framework for all projects; monorepo with core, abstractions, EF Core adapter, contract tests, and sample host.
- Core library must be storage-agnostic; EF Core adapter is reference-only.
- REST Minimal APIs for sample host with Swagger docs; errors must be RFC 7807 Problem Details only.
- BYO-auth stance; sample uses API key auth.
- Structured logging required with specified fields; optional OpenTelemetry hooks in sample.
- CI/CD via GitHub Actions build/test/pack; release on tags.
- Dockerfile (and optional docker-compose) for sample host.
- Naming, routing, error, logging, and date/time format conventions must be enforced and tested.

### FR Coverage Map

FR1: Epic 1 - Establish the Trust Baseline in a New Service
FR2: Epic 1 - Establish the Trust Baseline in a New Service
FR3: Epic 1 - Establish the Trust Baseline in a New Service
FR4: Epic 1 - Establish the Trust Baseline in a New Service
FR5: Epic 1 - Establish the Trust Baseline in a New Service
FR6: Epic 1 - Establish the Trust Baseline in a New Service
FR7: Epic 1 - Establish the Trust Baseline in a New Service
FR8: Epic 1 - Establish the Trust Baseline in a New Service
FR9: Epic 1 - Establish the Trust Baseline in a New Service
FR10: Epic 1 - Establish the Trust Baseline in a New Service
FR11: Epic 1 - Establish the Trust Baseline in a New Service
FR12: Epic 1 - Establish the Trust Baseline in a New Service
FR13: Epic 1 - Establish the Trust Baseline in a New Service
FR14: Epic 1 - Establish the Trust Baseline in a New Service
FR15: Epic 2 - Contract Tests That Prove the Guarantees
FR16: Epic 2 - Contract Tests That Prove the Guarantees
FR17: Epic 2 - Contract Tests That Prove the Guarantees
FR18: Epic 2 - Contract Tests That Prove the Guarantees
FR19: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR20: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR21: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR22: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR23: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR24: Epic 4 - Extension Boundaries & Reference Adapter Patterns
FR25: Epic 4 - Extension Boundaries & Reference Adapter Patterns
FR26: Epic 4 - Extension Boundaries & Reference Adapter Patterns
FR27: Epic 3 - Documentation & Trust Contract That Make Adoption Fast
FR28: Epic 3 - Documentation & Trust Contract That Make Adoption Fast

## Epic List

### Epic 1: Establish the Trust Baseline in a New Service
A platform engineer can initialize a new .NET service and enforce tenant context and refusal behavior in real request paths through a single, unavoidable integration point.
**FRs covered:** FR1–FR14

### Epic 2: Contract Tests That Prove the Guarantees
Teams can run verification tests in CI to prove invariants across request, background, admin, and scripted paths.
**FRs covered:** FR15–FR18

### Epic 3: Documentation & Trust Contract That Make Adoption Fast
Engineers can understand the model, integrate quickly, and troubleshoot using explicit guidance, trust contract, and complete API reference.
**FRs covered:** FR19–FR23, FR27–FR28

### Epic 4: Extension Boundaries & Reference Adapter Patterns
Contributors can build adapters and extensions without weakening invariants, with bypass paths blocked and tested.
**FRs covered:** FR24–FR26

<!-- Repeat for each epic in epics_list (N = 1, 2, 3...) -->

## Epic 1: Establish the Trust Baseline in a New Service

A platform engineer can initialize a new .NET service and enforce tenant context and refusal behavior in real request paths through a single, unavoidable integration point.

### Story 1.1: Initialize the Monorepo Baseline

As a platform engineer,
I want a .NET 10 solution scaffolded from the approved starter template,
So that the baseline has the correct structure from day one.

**Acceptance Criteria:**

**Given** the documented `dotnet new` commands
**When** I execute them
**Then** the solution and projects are created as specified in the architecture
**And** all projects target `net10.0` in their `*.csproj` files
**And** the solution builds successfully with `dotnet build`
**And** the monorepo includes core, abstractions, EF Core adapter, contract tests, and sample host projects

**FRs implemented:** FR14

### Story 1.2: Define the Tenant Context Model & Scope Types

As a platform engineer,
I want explicit tenant context and scope types with accessors,
So that operations attach tenant context consistently.

**Acceptance Criteria:**

**Given** the abstractions project
**When** I implement tenant context, scope types, and accessors
**Then** the system can represent tenant scope, shared scope, and explicit “no tenant” states
**And** the API surface is minimal and stable without framework coupling
**And** attempts to create an invalid or ambiguous scope are rejected with a clear invariant code

**FRs implemented:** FR1, FR2, FR3, FR4, FR5

### Story 1.3: Add the Unavoidable Integration Point (Request Middleware)

As a platform engineer,
I want a single request-path integration point that initializes tenant context,
So that every request is guaranteed to pass through it.

**Acceptance Criteria:**

**Given** an HTTP request to the sample host
**When** the tenant context middleware runs
**Then** a tenant context is required and attached before downstream handlers execute
**And** requests without valid tenant context are refused with Problem Details and a stable invariant code

**FRs implemented:** FR11, FR12, FR13

### Story 1.4: Implement Invariant Enforcement + Refusal Reasons

As a platform engineer,
I want invariant enforcement with explicit refusal reasons,
So that ambiguous or cross-tenant operations are blocked and explainable.

**Acceptance Criteria:**

**Given** an operation without required tenant attribution
**When** the invariant enforcer runs
**Then** the operation is refused
**And** the refusal exposes a stable invariant code and clear reason
**And** repeated runs with the same inputs yield identical outcomes

**FRs implemented:** FR6, FR7, FR8, FR9, FR10
**NFRs implemented:** NFR1, NFR3, NFR6

### Story 1.5: Add EF Core Reference Adapter (SaveChanges Guard)

As a platform engineer using EF Core,
I want a reference adapter that enforces tenant scope in persistence,
So that cross-tenant writes are blocked.

**Acceptance Criteria:**

**Given** a DbContext configured with the adapter
**When** SaveChanges is called without valid tenant context
**Then** the operation is refused
**And** the refusal follows the same invariant codes as core enforcement
**And** the core library remains storage-agnostic with no EF Core dependency
**And** valid tenant-scoped operations succeed without additional configuration

**FRs implemented:** FR6, FR8

### Story 1.6: Sample Host Wiring (Minimal API + Problem Details)

As a platform engineer,
I want a runnable sample host that demonstrates the baseline in practice,
So that I can see the integration flow end-to-end.

**Acceptance Criteria:**

**Given** the sample host
**When** I run it and hit example endpoints
**Then** tenant context enforcement is active
**And** the host uses Minimal APIs
**And** errors return RFC 7807 Problem Details only
**And** Problem Details include `trace_id` and `invariant_code` extensions when applicable

**FRs implemented:** FR9, FR14

### Story 1.7: Sample Host Swagger Documentation

As a platform engineer,
I want OpenAPI/Swagger documentation for the sample host,
So that integration points are discoverable and consistent.

**Acceptance Criteria:**

**Given** the sample host running locally
**When** I open the Swagger UI or OpenAPI JSON
**Then** all sample endpoints are documented
**And** the error responses are described as Problem Details
**And** undocumented endpoints are treated as a test failure
**And** missing Problem Details schemas in the OpenAPI spec fail contract tests

**Supporting requirements:** Additional Requirements - REST Minimal APIs with Swagger docs

**FRs implemented:** FR21, FR23

### Story 1.8: Sample Host API Key Authentication (BYO-Auth)

As a platform engineer,
I want the sample host to use a simple API key while documenting BYO-auth,
So that auth is runnable without imposing a framework.

**Acceptance Criteria:**

**Given** a request without an API key
**When** I call a sample endpoint
**Then** the response is 401/403 using Problem Details
**And** a valid API key allows the request to proceed
**And** the docs state the BYO-auth stance and replacement guidance
**And** invalid keys do not reveal whether a tenant exists

**Supporting requirements:** Additional Requirements - BYO-auth stance; sample uses API key auth

**FRs implemented:** FR10, FR14

### Story 1.9: Structured Logging + Optional OpenTelemetry Hooks

As a platform engineer,
I want structured logging with required fields and optional OTel hooks,
So that enforcement events are traceable without logging lock-in.

**Acceptance Criteria:**

**Given** a request processed by the sample host
**When** I inspect log entries emitted by the baseline
**Then** each log line includes required fields: `tenant_ref`, `trace_id`, `request_id`, `invariant_code`, `event_name`, `severity`
**And** the logging helper does not require a specific logging stack beyond `ILogger`
**And** optional OpenTelemetry hooks are demonstrated in the sample host
**And** missing required fields fail a contract test

**Supporting requirements:** Additional Requirements - structured logging fields; optional OpenTelemetry hooks

**FRs implemented:** FR9

### Story 1.10: Docker Assets for Sample Host

As a platform engineer,
I want Docker assets for the sample host,
So that I can run and verify the baseline in containers.

**Acceptance Criteria:**

**Given** the repository root
**When** I build the sample host container
**Then** a Dockerfile produces a runnable image
**And** docker-compose (if provided) starts the sample host successfully
**And** containerized runs emit the same Problem Details format as local runs
**And** container startup failures fail the CI validation step

**Supporting requirements:** Additional Requirements - Dockerfile (optional docker-compose) for sample host

**FRs implemented:** FR14

### Story 1.11: CI Build/Test/Pack and Release on Tags

As a platform engineer,
I want GitHub Actions to build, test, and pack releases,
So that distribution is consistent and automated.

**Acceptance Criteria:**

**Given** a push to main
**When** the CI workflow runs
**Then** it builds and tests all projects
**And** it produces NuGet packages via `dotnet pack`
**And** tagged releases trigger a publish step
**And** missing build artifacts fail the pipeline with a clear error

**Supporting requirements:** Additional Requirements - CI/CD via GitHub Actions build/test/pack; release on tags

**FRs implemented:** FR16, FR17

## Epic 2: Contract Tests That Prove the Guarantees

Teams can run verification tests in CI to prove invariants across request, background, admin, and scripted paths.

### Story 2.1: Contract Test Harness & Fixtures

As a platform engineer,
I want a contract test harness with fixtures,
So that invariants can be executed consistently across contexts.

**Acceptance Criteria:**

**Given** the contract tests project
**When** I implement reusable fixtures for tenant context and test hosts
**Then** contract tests can run deterministically across environments
**And** fixtures cover request and non-request execution setup
**And** fixture failures provide actionable diagnostics

**FRs implemented:** FR15, FR17, FR18
**NFRs implemented:** NFR5

### Story 2.2: Invariant Enforcement Tests (Request Path)

As a platform engineer,
I want contract tests that assert refusal behavior on request paths,
So that ambiguous or missing tenant context is always blocked.

**Acceptance Criteria:**

**Given** an HTTP request without required tenant context
**When** the request is processed through the integration point
**Then** the invariant enforcement refuses the operation
**And** the response returns a stable invariant code
**And** ambiguous tenant attribution is refused with a distinct invariant code

**FRs implemented:** FR15, FR16
**NFRs implemented:** NFR1, NFR2, NFR6

### Story 2.3: Invariant Enforcement Tests (Background/Admin Paths)

As a platform engineer,
I want contract tests that validate background and admin execution paths,
So that invariants are enforced outside HTTP requests.

**Acceptance Criteria:**

**Given** a background or admin operation without valid tenant context
**When** the operation is executed through the invariant enforcer
**Then** the operation is refused consistently
**And** the same invariant codes and refusal reasons are emitted
**And** privileged operations require explicit scope declaration and justification

**FRs implemented:** FR15, FR18
**NFRs implemented:** NFR1, NFR2, NFR3

### Story 2.4: CI Execution of Contract Tests

As a platform engineer,
I want contract tests running in CI with clear pass/fail signals,
So that adoption is verifiable.

**Acceptance Criteria:**

**Given** a CI workflow
**When** the contract test suite runs
**Then** failures are reported clearly in CI output
**And** the full suite completes within the target time budget
**And** enforcement branch coverage meets the >=90% threshold
**And** no environment-specific skips are permitted
**And** failures include the invariant code or test name that triggered them

**FRs implemented:** FR16, FR17
**NFRs implemented:** NFR4, NFR8


### Story 2.5: Performance Benchmarks

As a platform engineer,
I want baseline performance and determinism benchmarks,
So that overhead and timing guarantees are validated.

**Acceptance Criteria:**

**Given** a reference benchmark suite
**When** I run it locally or in CI
**Then** invariant checks return within <=100ms at p95
**And** baseline overhead adds <=1ms at p95 for 10,000 requests
**And** enforcement checks add <=5% latency from 1 to 10,000 tenants
**And** results are published as part of CI artifacts

**NFRs implemented:** NFR7, NFR13, NFR14

**FRs implemented:** FR15

### Story 2.6: Determinism and No Background Loops

As a platform engineer,
I want determinism and no background polling by default,
So that baseline behavior is predictable and low-overhead.

**Acceptance Criteria:**

**Given** repeated CI runs with identical inputs
**When** I execute the contract tests
**Then** invariant enforcement outcomes are identical (0 variance)
**And** no background polling loops or timers start by default
**And** results are published as part of CI artifacts

**NFRs implemented:** NFR5, NFR15

**FRs implemented:** FR15

### Story 2.7: Integration Path Enforcement Tests

As a platform engineer,
I want contract tests to enforce integration and pattern conventions,
So that all request paths are guarded and formats are consistent.

**Acceptance Criteria:**

**Given** the reference project
**When** I run the contract test suite
**Then** every request path is verified to pass through the integration point
**And** bypassing the integration point causes contract tests to fail in CI
**And** the sample integration requires no domain/business logic changes
**And** any deviation reports the specific offending endpoint or log field

**NFRs implemented:** NFR9, NFR10, NFR12

**FRs implemented:** FR15, FR16

### Story 2.8: Scalability Load Validation

As a platform engineer,
I want scalability load validation,
So that invariants hold under 10x load and large tenant counts.

**Acceptance Criteria:**

**Given** load simulations with tenant counts of 1, 100, and 10,000
**When** the contract tests run
**Then** invariants remain enforced under 10x load
**And** failed invariants include the tenant count and scenario in output

**NFRs implemented:** NFR16, NFR18

**FRs implemented:** FR18

### Story 2.9: Topology Validation (Multi-Service + Multi-Database)

As a platform engineer,
I want topology validation,
So that the reference architecture demonstrates multi-service and multi-database layouts without special casing.

**Acceptance Criteria:**

**Given** the reference architecture and integration tests
**When** I run the topology validation suite
**Then** the reference architecture demonstrates multi-service and multi-database topology
**And** any topology-specific invariant failures identify the affected service and database

**NFRs implemented:** NFR17

**FRs implemented:** FR18

### Story 2.10: Naming and Structure Convention Validation

As a platform engineer,
I want automated checks for naming and structure conventions,
So that pattern violations are caught early.

**Acceptance Criteria:**

**Given** the reference solution and EF Core model
**When** convention validation checks run
**Then** API routes are plural and use explicit `{tenantId}` parameters
**And** database tables and columns follow PascalCase without underscores
**And** the project structure matches the flat root layout without `src/` or `tests/` folders
**And** Problem Details responses match the documented shape
**And** required log fields are present in enforcement logs
**And** date/time format conventions are enforced

**Supporting requirements:** Additional Requirements - naming, routing, error, logging, and date/time format conventions

**FRs implemented:** FR14, FR21
## Epic 3: Documentation & Trust Contract That Make Adoption Fast

Engineers can understand the model, integrate quickly, and troubleshoot using explicit guidance, trust contract, and complete API reference.

### Story 3.1: Trust Contract Document

As a platform engineer,
I want an explicit trust contract document that defines invariants and refusal behavior,
So that guarantees are clear and auditable.

**Acceptance Criteria:**

**Given** the documentation set
**When** I open the trust contract
**Then** all invariants are enumerated with explicit refusal behavior
**And** each invariant has a stable code referenced by errors
**And** any missing invariant code is treated as a documentation defect

**FRs implemented:** FR20

### Story 3.2: Conceptual Model (<=2 pages)

As a platform engineer,
I want a concise conceptual model of tenancy and scope,
So that I can understand the baseline quickly.

**Acceptance Criteria:**

**Given** the conceptual model doc
**When** I read it end-to-end
**Then** it fits within two pages or 800 words
**And** it explains tenant, shared, and no-tenant scopes clearly
**And** examples avoid implementation detail beyond the defined model

**FRs implemented:** FR19

### Story 3.3: Integration Guide (<=6 steps, <=30 min)

As a platform engineer,
I want a minimal integration guide with clear steps,
So that I can integrate the baseline quickly.

**Acceptance Criteria:**

**Given** the integration guide
**When** I follow the steps in a new service
**Then** the integration completes in six steps or fewer
**And** a new engineer can finish within 30 minutes
**And** steps that are skipped or out of order fail the contract tests

**FRs implemented:** FR21
**NFRs implemented:** NFR11

### Story 3.4: Verification Guide (Contract Tests)

As a platform engineer,
I want a verification guide that explains contract tests,
So that I can run and interpret the guarantees.

**Acceptance Criteria:**

**Given** the verification guide
**When** I follow the instructions
**Then** I can run the contract tests successfully
**And** I can interpret failures using referenced invariant codes
**And** at least one failing example is documented with remediation guidance

**FRs implemented:** FR22

### Story 3.5: API Reference (100% Surface Area)

As a platform engineer,
I want a complete API reference for public types and entry points,
So that I can integrate without guesswork.

**Acceptance Criteria:**

**Given** the public API surface
**When** I compare it against the API reference
**Then** all public types and entry points are documented
**And** the reference matches current signatures
**And** undocumented public members are treated as a release blocker

**FRs implemented:** FR23

### Story 3.6: Actionable Error Guidance

As a platform engineer,
I want refusal errors to include actionable guidance and links to contract rules,
So that I can fix issues quickly.

**Acceptance Criteria:**

**Given** a refusal error response
**When** I inspect the error details
**Then** it includes a stable invariant code and guidance text
**And** it links to the relevant trust contract rule
**And** missing links are reported in documentation validation

**FRs implemented:** FR27, FR28

## Epic 4: Extension Boundaries & Reference Adapter Patterns

Contributors can build adapters and extensions without weakening invariants, with bypass paths blocked and tested.

### Story 4.1: Define Extension Boundaries & Contracts

As a contributor,
I want explicit extension boundaries and contracts,
So that I can build adapters without weakening invariants.

**Acceptance Criteria:**

**Given** the architecture and documentation
**When** I review extension boundaries
**Then** sanctioned extension points are explicitly defined
**And** bypass paths are explicitly prohibited
**And** prohibited extension attempts are documented with examples

**FRs implemented:** FR24, FR26

### Story 4.2: Adapter Guidance + Examples

As a contributor,
I want guidance and examples for building adapters,
So that I can integrate new stacks safely.

**Acceptance Criteria:**

**Given** the adapter guidance
**When** I follow the example
**Then** the adapter preserves invariant enforcement
**And** it integrates through sanctioned boundaries only
**And** a bypass attempt triggers a documented failure mode

**FRs implemented:** FR25

### Story 4.3: Bypass Prevention Validation

As a platform engineer,
I want validation that bypass paths are blocked,
So that extensions cannot weaken the baseline.

**Acceptance Criteria:**

**Given** a public entry point
**When** I attempt to bypass invariants
**Then** contract tests fail
**And** the failure references the violated invariant code
**And** the failure indicates the exact bypass mechanism used
**FRs implemented:** FR26
