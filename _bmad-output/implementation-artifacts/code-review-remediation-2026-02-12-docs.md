# Story 0-CR3: Code Review Remediation - Docs Alignment

## Story Info
- **Story ID:** 0-CR3
- **Created:** 2026-02-12
- **Source:** Manual comparison of PRD + Architecture vs repo state
- **Priority:** Medium
- **Scope:** Documentation alignment (planning artifacts)
- **Implements:** Documentation correctness and scope alignment
- **Related NFRs:** NFR7 (Non-invasive integration)
- **Status:** review

## Story

**As a** maintainer of TenantSaas documentation
**I want** the PRD and Architecture documents to match the actual repository state and agreed scope
**So that** planning artifacts remain trustworthy, actionable, and reduce ambiguity for implementation and review

## Acceptance Criteria

**AC#1:** Architecture project structure reflects actual repo layout (modules, folders, key files)
**AC#2:** Architecture component notes reflect current implementation (logging/tenancy/enforcement types)
**AC#3:** PRD requirements that are not yet implemented are explicitly deferred with rationale
**AC#4:** Documentation scope decisions (e.g., npm distribution, missing guides) are consistent across PRD and Architecture

---

## Tasks

### Task 1: Update Architecture Project Structure
- [x] **1.1** Revise `Project Structure & Boundaries` tree in `_bmad-output/planning-artifacts/architecture.md` to match current repo layout
- [x] **1.2** Reflect actual folders in `TenantSaas.Abstractions` (BreakGlass, Contexts, Disclosure, TrustContract) and remove non-existent ones (Errors)
- [x] **1.3** Reflect actual folders/files in `TenantSaas.Core` (Enforcement, Errors, Logging, Tenancy) and current file names (e.g., `DefaultLogEnricher.cs`)
- [x] **1.4** Update `TenantSaas.ContractTests` structure in the architecture doc to match current folder layout
- [x] **1.5** Update `TenantSaas.Sample` structure in the architecture doc to include `HttpCorrelationExtensions.cs` and `TenantSaas.Sample.http`

### Task 2: Reconcile PRD vs Architecture Scope
- [x] **2.1** In `_bmad-output/planning-artifacts/prd.md`, clarify the status of `docs/verification-guide.md` and `docs/api-reference.md` (implement now or explicitly defer)
- [x] **2.2** In `_bmad-output/planning-artifacts/prd.md`, clarify the status of the secondary npm distribution (implement now or explicitly defer)
- [x] **2.3** Ensure `_bmad-output/planning-artifacts/architecture.md` reflects the same scope decisions

### Task 3: Update Validation Sections
- [x] **3.1** Update `Architecture Validation Results` to reflect any scope/structure changes from Tasks 1-2

---

## Test Verification

No automated tests required (documentation-only changes).

---

## File List

### Files to Modify
- `_bmad-output/planning-artifacts/architecture.md`
- `_bmad-output/planning-artifacts/prd.md`

### Updated Files
- `_bmad-output/planning-artifacts/architecture.md`
- `_bmad-output/planning-artifacts/prd.md`

---

## Dev Notes

- Keep PRD and Architecture statements consistent with actual code and agreed scope
- If deferring requirements, document rationale and expected follow-up story/epic

---

## Dev Agent Record

### Implementation Plan

1) Update architecture project structure and component details to reflect current repo state.
2) Reconcile PRD scope items that are currently unmet (verification guide, API reference, npm distribution) by implementing or deferring explicitly.
3) Refresh architecture validation notes to align with updated scope/structure.

### Completion Notes

- Updated architecture project tree to match current repo layout (Abstractions/Core/ContractTests/Sample) and refreshed component notes for enforcement/tenancy/logging.
- Aligned PRD scope with explicit deferrals for verification guide, API reference, and npm distribution; mirrored in architecture.
- Updated Architecture Validation Results date and status entries to reflect current scope/structure.
- Tests: `dotnet test TenantSaas.sln --disable-build-servers -v minimal` (pass). Initial `dotnet test TenantSaas.sln -v minimal` failed due to MSBuild named pipe permission; rerun with build servers disabled succeeded.

### Change Log

- 2026-02-12: Story created for PRD/Architecture alignment with current code state
- 2026-02-13: Aligned architecture/prd scope and structure with repo state; updated validation results and scope deferrals.
