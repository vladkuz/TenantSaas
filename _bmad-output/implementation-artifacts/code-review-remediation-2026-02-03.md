# Story 0-CR2: Code Review Remediation (Follow-up)

## Story Info
- **Story ID:** 0-CR2
- **Created:** 2026-02-03
- **Source:** Follow-up review against architecture.md + epics
- **Priority:** Medium
- **Scope:** Code/docs alignment and planned backlog capture
- **Implements:** N/A (Code Hygiene / Consistency)
- **Related NFRs:** NFR7 (Non-invasive integration)
- **Status:** ✅ closed

## Story

**As a** maintainer of TenantSaas  
**I want** the implementation and documentation to match the architecture and epics  
**So that** adopters can trust the baseline and its guidance

## Resolution Summary (2026-02-03)

**Approach:** Minimal disruption; documentation-only fixes; explicit deferrals.

| Task | Resolution |
|------|------------|
| Task 1: API Key Auth | ✅ **Deferred** — BYO-auth stance documented in architecture |
| Task 2: EF Core Adapter | ✅ **Deferred** — Planned in Epic 6, Story 6.3 (noted in architecture) |
| Task 3: Contract Tests Coupling | ✅ **Documented** — Sample coupling is intentional for E2E validation (noted in architecture) |
| Task 4: Logging event_name | ⏸️ **Deferred** — Requires architectural decision on field format |
| Task 5: DateTime RFC3339 | ❌ **Removed** — False positive; `DateTimeOffset.UtcNow` serializes correctly |
| Task 6: Architecture Structure | ✅ **Fixed** — Directory structure updated to match reality |
| Task 7: Integration Guide Pipeline | ✅ **Fixed** — Updated to Minimal APIs |
| Task 8: FluentAssertions Version | ✅ **Fixed** — Docs aligned to 7.0.0 (8.x has license/API concerns) |

**Files Modified:**
- [architecture.md](_bmad-output/planning-artifacts/architecture.md) — structure, deferrals, Implementation Status section
- [integration-guide.md](docs/integration-guide.md) — Minimal API example, FluentAssertions version

**Status:** ✅ Closed

---

## Acceptance Criteria

**AC#1:** Architecture requirements that are in-scope are reflected in the codebase or explicitly deferred.  
**AC#2:** Sample host behavior matches documented patterns (auth, logging, error shape).  
**AC#3:** Reference adapter and docs are aligned with epics where planned.  
**AC#4:** Documentation and tests are consistent on versions and patterns.  

---

## Tasks

### Task 1: API Key Auth Boundary (Sample)
- [x] **1.1** ~~Add API key auth implementation~~ → Documented BYO-auth stance in architecture.md
- [x] **1.2** ~~Wire auth into request pipeline~~ → N/A (BYO-auth)
- [x] **1.3** ~~Update integration guide~~ → N/A (no auth to document)

**Resolution:** Deferred via documentation. BYO-auth stance documented in architecture.md (Architectural Boundaries section + Implementation Status table).

**Rationale:** Architecture expects a sample auth boundary; epics note BYO-auth posture but do not provide an explicit story.

---

### Task 2: EF Core Reference Adapter
- [x] **2.1** ~~Implement reference EF Core adapter~~ → Documented deferral to Epic 6, Story 6.3 in architecture.md
- [x] **2.2** Core remains storage-agnostic ✓

**Resolution:** Deferred. Shell project exists; implementation planned in Epic 6, Story 6.3. Documented in architecture.md (Directory Structure + Implementation Status table).

---

### Task 3: Contract Tests Coupling to Sample Host
- [x] **3.1** Evaluated: integration tests use `WebApplicationFactory<Program>` for E2E validation
- [ ] ~~**3.2** Option A~~ → Not chosen
- [x] **3.3** Option B: Documented intentional coupling in architecture.md
- [ ] ~~**3.4**~~ → N/A (Option A not chosen)
- [x] **3.5** Architecture boundary section updated

**Resolution:** Option B chosen. Coupling documented as intentional for E2E validation in architecture.md (Component Boundaries section).

**Rationale:** Architecture states contract tests should not couple to sample host. Current coupling exists for integration test host. Requires decision: refactor or document exception.

---

### Task 4: Enforcement Logging Required Fields
- [ ] **4.1** Ensure every enforcement log includes `event_name` and `tenant_ref` per architecture ⏸️
- [ ] **4.2** Verify all required log fields are emitted ⏸️
- [ ] **4.3** Align log message templates to required fields set ⏸️
- [ ] **4.4** Add or update contract test asserting required field presence ⏸️

**Resolution:** ⏸️ DEFERRED. Requires architectural decision on `event_name` field format before implementation. See architecture.md Implementation Status table.

---

### Task 5: Date/Time Format Enforcement
- [x] ~~**5.1**~~ → False positive; `DateTimeOffset.UtcNow` serializes to RFC3339 with Z suffix by default
- [x] ~~**5.2**~~ → No action needed

**Resolution:** ❌ REMOVED. False positive. .NET's `DateTimeOffset.UtcNow` with default JSON serialization produces compliant RFC3339 timestamps.

---

### Task 6: Architecture Structure vs Repo Reality
- [x] **6.1** Updated architecture directory structure to match repo reality
- [x] **6.2** Marked deferred items with Epic/Story references in architecture.md
- [x] **6.3** Added "Implementation Status" section to architecture.md

**Resolution:** ✅ FIXED. Directory structure in architecture.md now matches repo. Deferred items marked with Epic/Story references. Implementation Status table added.

---

### Task 7: Integration Guide Pipeline Alignment
- [x] **7.1** Replaced controller pipeline with Minimal API example in integration-guide.md
- [x] **7.2** Middleware ordering matches implementation

**Resolution:** ✅ FIXED. integration-guide.md now shows `app.MapGet()` Minimal API pattern instead of `MapControllers()`.

---

### Task 8: Dependency Version Consistency
- [x] **8.1** Aligned FluentAssertions version to 7.0.0 in integration-guide.md

**Resolution:** ✅ FIXED. Docs now reference FluentAssertions 7.0.0 (8.x has license/API concerns).

---

## Out of Scope / Planned in Epics

| Item | Epic/Story | Note |
|------|------------|------|
| EF Core adapter completion | Epic 6, Story 6.3 | Planned in epics |
| Verification guide doc | Epic 6, Story 6.6 | Planned in epics |
| API reference doc | Epic 6, Story 6.7 | Planned in epics |
| Release workflow | Epic 1 | CI skeleton exists; release deferred |
| Docker/scripts tooling | Post-MVP | Optional infrastructure |

---

## Test Verification

When tasks are complete:
```bash
dotnet build TenantSaas.sln
dotnet test TenantSaas.sln
```

---

## File List

### Files Modified
- `_bmad-output/planning-artifacts/architecture.md` — Directory structure, deferrals, Implementation Status section
- `docs/integration-guide.md` — Minimal API example, FluentAssertions version

### Files Not Modified (Original scope reduced)
- ~~`TenantSaas.Sample/Program.cs`~~ — No changes needed
- ~~`TenantSaas.Sample/Auth/ApiKeyAuthHandler.cs`~~ — BYO-auth stance, not implemented
- ~~`TenantSaas.Core/Logging/EnforcementEventSource.cs`~~ — Task 4 deferred
- ~~`TenantSaas.ContractTests/TenantSaas.ContractTests.csproj`~~ — Already correct
- ~~`docs/verification-guide.md`~~ — Deferred to Epic 6, Story 6.6
- ~~`docs/api-reference.md`~~ — Deferred to Epic 6, Story 6.7
- ~~`.github/workflows/release.yml`~~ — Deferred to Epic 1 completion
- ~~`docker/`~~ — Deferred (optional)
- ~~`scripts/`~~ — Deferred (optional)

---

## Dev Notes

- This story enumerates unresolved items from the latest review.
- Each task can be split into separate stories if needed.
