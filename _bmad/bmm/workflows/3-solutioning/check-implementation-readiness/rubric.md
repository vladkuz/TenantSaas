---
name: 'implementation-readiness-rubric'
description: 'Deterministic checklist for Implementation Readiness (IR) workflow'
---

# Implementation Readiness Checklist (Tight / Binary)

**Rule:** Only findings that match this checklist are valid issues. Anything else is an observation.

## A) Inputs
- Exactly 1 PRD file (whole OR sharded). If both: **Critical**.
- Exactly 1 Architecture file (whole OR sharded). If both: **Critical**.
- Exactly 1 Epics/Stories file (whole OR sharded). If both: **Critical**.
- UX doc: must be present OR PRD contains the literal statement
  `UX documentation is not required for this project.` If neither: **Major**.

## B) FR Coverage (Critical)
- Every PRD FR is mapped to >=1 Epic/Story ID.
- Every mapped FR references a real Epic/Story ID that exists.
- No unmapped PRD FRs (0 allowed).

## C) Epic Independence (Critical)
- No epic depends on a higher-numbered epic.
- No story depends on a story in a higher-numbered epic.

## D) Story Testability (Major)
- Every story has >=2 Given/When/Then acceptance criteria.
- Each AC contains a measurable artifact keyword:
  `test`, `log`, `response`, `error`, `event`, or `metric`.
- Every enforcement/guard story has >=1 negative AC:
  `refuse`, `error`, `fail`, or `block`.

## E) Dependency Hygiene (Major)
- Each epic declares dependencies explicitly (list or `None`).
- No circular dependencies.

## F) NFR Coverage (Major)
- Every PRD NFR is mapped to >=1 Epic/Story OR a named test artifact.
- “Reference project” used for benchmarks/tests is explicitly named.

## G) Documentation Requirements (Major)
- Stories exist for: Trust Contract, Integration Guide, Verification Guide, API Reference.
- Each doc story includes an acceptance criterion that states a location path
  (e.g., `docs/trust-contract.md`).

## H) Compliance/Regulatory (Minor)
- PRD explicitly states `No compliance requirements apply` OR lists named standards
  (e.g., SOC2, GDPR). If missing: **Minor**.

## Readiness Gate
- **Critical = 0** to proceed.
- **Major = 0** to proceed unless you explicitly accept risk in writing.
