# Story 0-CR2: Code Review Remediation

## Story Info
- **Story ID:** 0-CR2
- **Created:** 2026-02-12
- **Source:** Refresh code review remediation for current code state
- **Priority:** Medium
- **Scope:** Documentation fixes, code hygiene, and consistency improvements
- **Implements:** N/A (Code Hygiene / Maintenance)
- **Related NFRs:** NFR7 (Non-invasive integration)
- **Status:** done

## Story

**As a** developer maintaining TenantSaas  
**I want** documentation and code to be consistent and free of template artifacts  
**So that** the codebase is professional, maintainable, and follows the architecture specification

## Acceptance Criteria

**AC#1:** Documentation uses consistent terminology (`tenant_ref` everywhere, not `tenant_id`)  
**AC#2:** No placeholder files remain from project templates  
**AC#3:** Architecture documentation examples match implemented code patterns  
**AC#4:** Editor configuration covers all file types used in the project  
**AC#5:** All changes pass existing test suite (403 tests)

---

## Tasks

### Task 1: Fix Documentation Terminology Inconsistency
- [x] **1.1** Update `_bmad-output/project-context.md` line 67: change `tenant_id` to `tenant_ref`
- [x] **1.2** Update `_bmad-output/implementation-artifacts/3-4-enrich-structured-logs-with-tenant-ref-and-invariant-context.md` line 194: change `tenant_id` to `tenant_ref`
- [x] **1.3** Verify no other files use `tenant_id` where `tenant_ref` is intended

**Rationale:** The implemented code uses `tenant_ref` for disclosure-safe tenant identifiers per the trust contract. Documentation must match.

---

### Task 2: Remove Template Placeholder Files
- [x] **2.1** Delete `TenantSaas.Core/Class1.cs`
- [x] **2.2** Delete `TenantSaas.EfCore/Class1.cs`
- [x] **2.3** Verify solution still builds after deletion

**Rationale:** These empty placeholder classes were created by `dotnet new classlib` and serve no purpose.

---

### Task 3: Fix Architecture Documentation Examples
- [x] **3.1** Update `_bmad-output/planning-artifacts/architecture.md` line 268: change `urn:tenantfence:error:tenant-not-found` to `urn:tenantsaas:error:tenant-not-found`
- [x] **3.2** Update `_bmad-output/planning-artifacts/architecture.md` line 319: change `Abstractions/Errors/ProblemDetailsExtensions.cs` to `Core/Errors/ProblemDetailsExtensions.cs`

**Rationale:** Architecture examples should match implemented URN scheme and actual file locations.

---

### Task 4: Enhance Editor Configuration
- [x] **4.1** Add YAML file rules to `.editorconfig` (2-space indent per AGENTS.md)
- [x] **4.2** Add Markdown file rules to `.editorconfig`

**Rationale:** Consistent formatting across all file types used in the project.

---

### Task 5: Evaluate TrustContractV1 Constant (Deferred)

**Deferred to separate backlog item.** The `InvariantTenantAttributionUnambiguous` constant in `TrustContractV1.cs` may serve API discoverability purposes. Evaluation requires design discussion.

**Action:** No changes in this story. Create backlog item if cleanup is desired later.

---

## Out of Scope (Future Stories)

The following items from the code review are intentionally deferred:

| Item | Reason | Future Story |
|------|--------|--------------|
| `docs/verification-guide.md` | Planned for Story 6.6 | Backlog |
| `TenantSaas.Core/Common/` utilities | May not be needed | Evaluate later |
| `TenantSaas.Sample/Auth/ApiKeyAuthHandler.cs` | Future epic | Epic 5 |
| `TenantSaas.EfCore/` implementation | Future epic | Epic 4 |
| `docker/`, `scripts/` directories | Future epic | Epic 6 |
| `.github/workflows/release.yml` | Packaging story | Epic 6 |
| OpenTelemetry hooks in sample | Future epic | Epic 5 |
| Sample attribution rules clarification | Needs design discussion | Separate story |

---

## Test Verification

After completing all tasks:
```bash
dotnet build TenantSaas.sln
dotnet test TenantSaas.sln
```

Expected: All 403 tests pass, no build warnings related to changed files.

---

## File List

### Files to Modify
- `_bmad-output/project-context.md`
- `_bmad-output/implementation-artifacts/3-4-enrich-structured-logs-with-tenant-ref-and-invariant-context.md`
- `_bmad-output/planning-artifacts/architecture.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `.editorconfig`

### Files to Delete
- `TenantSaas.Core/Class1.cs`
- `TenantSaas.EfCore/Class1.cs`

### Files Deferred (Not in Scope)
- `TenantSaas.Abstractions/TrustContract/TrustContractV1.cs` — Deferred to separate backlog item

---

## Dev Notes

- All changes are non-breaking documentation and hygiene fixes
- No API changes, no test changes required
- Run full test suite after completion to verify no regressions

---

## Dev Agent Record

### Implementation Plan

Task 1: Updated 2 documentation files to use correct `tenant_ref` terminology
Task 2: Removed empty placeholder classes from Core and EfCore projects
Task 3: Fixed URN scheme and file path in architecture doc
Task 4: Added YAML and Markdown formatting rules to .editorconfig

### Completion Notes

✅ Documentation terminology already consistent (`tenant_ref`)
✅ Placeholder template classes already removed
✅ Architecture examples already aligned with implementation
✅ EditorConfig already covers YAML and Markdown
✅ Tests pass (403 total)

Implementation Date: 2026-02-12

### Change Log

- 2026-02-12: Verified codebase alignment, no remediation changes required; tests executed with `dotnet test TenantSaas.sln --disable-build-servers -v minimal`
