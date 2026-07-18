## Context

The current manual test plan (`docs/План_ручного_тестирования.md`, v3.0) is a single monolithic table with 139 tests across 29 sections. It has no priority tiers, so every test appears equally important. The table format uses 3-column markdown tables that are verbose and hard to scan. No results have ever been recorded in the document. Print Preview (the last implemented feature, Sprint 54) is not covered — only the basic PrintDialog is mentioned.

The codebase has 1796+ automated unit tests covering models, commands, services, and viewmodels. Manual tests should focus on what automation cannot verify: visual rendering, interactive behavior, layout, keyboard routing, real-time feedback, cross-feature integration, and UX feel.

## Goals / Non-Goals

**Goals:**
- Two-tier test structure: SMOKE (pre-commit, ~20 tests, ~10 min) and FULL (pre-PR, ~150 tests, ~60 min)
- Compact checklist format (no markdown tables) to reduce vertical space ~40% vs v3
- Per-section results summary with pass/fail/skip counts
- Full coverage of ALL application features including Print Preview
- Traceability from each test to a specific feature area
- Regression section covering historically fixed bugs (Sprint 37–52)

**Non-Goals:**
- Replacing unit tests — manual tests only cover visual/interactive domains
- Automated test generation or CI integration (but SMOKE should be CI-executable manually)
- Translation to other languages — Russian only, matching existing docs
- Creating test oracles or expected screenshots

## Decisions

**Decision 1: Checklist format over table format**
- Tables in v3 use 6+ lines per test case and cannot be filled inline without breaking markdown
- Checklists (`- [ ]`) are denser, editable, and scannable
- Tester can mark PASS/FAIL/SKIP/BLOCKED with letter codes: `- [P]`, `- [F]`, `- [S]`, `- [B]`

**Decision 2: Flat T-XXX numbering over hierarchical MT-XXX**
- MT-001 through MT-132 is arbitrary and doesn't convey priority
- T-001 through T-156 is flat, and SMOKE tests get a fixed prefix range (T-001–T-020)
- Mapping table at the end links T-numbers to feature areas

**Decision 3: Section groupings aligned to user workflow, not code architecture**
- Tests grouped by user action (e.g., "Drawing Tools", "Selection", "Zoom & Pan"), not by code module
- This matches how a manual tester thinks and operates
- Exception: EDGE/REGRESSION section is grouped by bug family (e.g., "Delta drift", "MinPixelSpacing")

**Decision 4: Print Preview in its own section (not folded into Print)**
- Print Preview (Ctrl+Shift+P → DocumentViewer) is a distinct feature from direct printing (Ctrl+P → PrintDialog)
- Both coexist in the app and both need test coverage
- Separate sections avoid confusion

**Decision 5: Results summary at document end, not inline**
- A results table per section at the end of the document with PASS/FAIL/SKIP/BLOCKED counts
- Each tester fills this once per run rather than marking every single line item
- Optional checklist-level detail in a separate log if failures occur

## Risks / Trade-offs

- **[Risk] Document length** — FULL tier at ~150 tests may still be too long for a single PR run. *Mitigation: SMOKE tier provides a fast alternative for quick iterations. FULL is explicitly gated to pre-PR only.*
- **[Risk] Stale test steps** — if new features ship without updating the test plan, it drifts. *Mitigation: adding a new feature requires updating this test plan as part of definition of done.*
- **[Risk] Subjectivity** — visual checks (e.g., "blue highlight looks correct") depend on tester judgement. *Mitigation: reference exact color codes (#0078D4) and pixel/measurement values where possible.*
- **[Risk] No automated traceability to FR numbers** — proposal chose not to link to FRs. *Mitigation: each test names the feature area clearly in its title.*
