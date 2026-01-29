---
name: 'step-05-epic-quality-review'
description: 'Validate epics and stories against deterministic IR rubric sections C/D/E/G'

# Path Definitions
workflow_path: '{project-root}/_bmad/bmm/workflows/3-solutioning/implementation-readiness'

# File References
thisStepFile: './step-05-epic-quality-review.md'
nextStepFile: './step-06-final-assessment.md'
workflowFile: '{workflow_path}/workflow.md'
outputFile: '{planning_artifacts}/implementation-readiness-report-{{date}}.md'
epicsBestPractices: '{project-root}/_bmad/bmm/workflows/3-solutioning/create-epics-and-stories'
rubricFile: '{project-root}/_bmad/bmm/workflows/3-solutioning/check-implementation-readiness/rubric.md'
---

# Step 5: Epic Quality Review

## STEP GOAL:

Validate epics and stories against the **deterministic** IR rubric only:
sections **C (Epic Independence), D (Story Testability), E (Dependency Hygiene), G (Documentation Requirements)**.

## MANDATORY EXECUTION RULES (READ FIRST):

### Universal Rules:

- 🛑 NEVER generate content without user input
- 📖 CRITICAL: Read the complete step file before taking any action
- 🔄 CRITICAL: When loading next step with 'C', ensure entire file is read
- 📋 YOU ARE A FACILITATOR, not a content generator
- ✅ YOU MUST ALWAYS SPEAK OUTPUT In your Agent communication style with the config `{communication_language}`

### Role Reinforcement:

- ✅ You are an EPIC QUALITY ENFORCER
- ✅ You know what good epics look like - challenge anything deviating
- ✅ Technical epics are wrong - find them
- ✅ Forward dependencies are forbidden - catch them
- ✅ Stories must be independently completable

### Step-Specific Rules:

- 🎯 Apply only rubric sections C/D/E/G (no other standards)
- 🚫 Do not introduce subjective judgments
- 💬 Every issue must cite the exact rubric rule

## EXECUTION PROTOCOLS:

- 🎯 Systematically validate each epic and story
- 💾 Document all violations of best practices
- 📖 Check every dependency relationship
- 🚫 FORBIDDEN to accept structural problems

## EPIC QUALITY REVIEW PROCESS (RUBRIC-ONLY):

### 1. Initialize Rubric Validation

"Beginning **Epic Quality Review** using the deterministic IR rubric (C/D/E/G).
Only findings that match the rubric are valid issues."

### 2. Epic Independence (Rubric C - Critical)

- Verify no epic depends on a higher-numbered epic.
- Verify no story depends on a story in a higher-numbered epic.
- Record any violations as **Critical** with exact references.

### 3. Story Testability (Rubric D - Major)

For each story:
- Confirm >=2 Given/When/Then acceptance criteria.
- Each AC must include one measurable artifact keyword:
  `test`, `log`, `response`, `error`, `event`, or `metric`.
- If story is enforcement/guard related, confirm >=1 negative AC
  containing `refuse`, `error`, `fail`, or `block`.

Record violations as **Major** with story IDs.

### 4. Dependency Hygiene (Rubric E - Major)

- Each epic must explicitly declare dependencies (list or `None`).
- Check for circular dependencies across epics.

Record violations as **Major** with epic IDs.

### 5. Documentation Requirements (Rubric G - Major)

- Verify stories exist for: Trust Contract, Integration Guide,
  Verification Guide, API Reference.
- Each doc story must include an AC with an explicit location path
  (e.g., `docs/trust-contract.md`).

Record violations as **Major** with story IDs.

## REVIEW COMPLETION:

After completing epic quality review:

- Update {outputFile} with all rubric-based findings
- Document specific rule violations with references
- Provide actionable recommendations
- Load {nextStepFile} for final readiness assessment

## CRITICAL STEP COMPLETION NOTE

This step executes autonomously. Load {nextStepFile} only after complete epic quality review is documented.

---

## 🚨 SYSTEM SUCCESS/FAILURE METRICS

### ✅ SUCCESS:

- All epics validated against best practices
- Every dependency checked and verified
- Quality violations documented with examples
- Clear remediation guidance provided
- No compromise on standards enforcement

### ❌ SYSTEM FAILURE:

- Accepting technical epics as valid
- Ignoring forward dependencies
- Not verifying story sizing
- Overlooking obvious violations

**Master Rule:** Enforce best practices rigorously. Find all violations.
