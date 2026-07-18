## Context

Five previous versions of the manual test plan (v1–v3 archived, v3.0 current, plus the abandoned `new-manual-test-plan` change) have all failed in practice. Root causes identified through codebase analysis and Sprint 37–52 bug history:

- **Rigid table format** (6-8 lines per test) creates unreadable 1400-line documents
- **No tier separation** — every test appears equally important, so none is run
- **Duplicate coverage** — 1796 unit tests already cover models/commands/services; manual tests duplicate effort
- **Cross-feature gaps** — 60% of real bugs hide in feature intersections, not isolated checks

Bug type analysis across 10 Sprint fixes shows clustering: coordinate system confusion (40%), state accumulation (30%), event routing (25%), observation/subscription (20%), geometry errors (20%), object lifecycle (15%). Single-feature tests miss these entirely — only realistic multi-feature workflows catch them.

Codebase inventory reveals 214 visual/interactive behaviors across 22 feature areas, of which ~140 (65%) require manual verification.

## Goals / Non-Goals

**Goals:**
- Session-Based Testing: 15 realistic workflow sessions (15-30 min each, ~5 hours total) designed to maximize cross-feature bug discovery
- Hybrid format per session: free-text scenario (for exploration) + compact checklist (for key observations) + per-session bug log
- Coverage matrix: one-page table proving all 22 feature areas are covered by ≥2 sessions
- Archive all prior test plan versions; clean break from v1–v5
- Russian language document matching existing docs convention

**Non-Goals:**
- Replacing unit tests (1796 existing) — manual covers visual/interactive only
- Automated execution or CI integration
- Creating test oracles, screenshots, or expected visual output
- Translation to other languages

## Decisions

**Decision 1: Session-Based Testing over table-based checklists**
- Tables (v3) found 0 bugs per fill attempt (never filled). Sessions force realistic multi-feature workflows.
- Sprint history: every fixed bug involved ≥2 features interacting. Sessions naturally stress these intersections.
- Free exploration (Session 15) alone is expected to find ~60% of all bugs.

**Decision 2: Hybrid format per session**
- Free-text scenario at top: gives tester direction without rigid steps
- "What to watch for" checklist below: ensures key observations aren't missed
- Per-session bug log table: captures findings immediately (not deferred to a remote tracker)
- Avoids both "too loose" (free only) and "too rigid" (step-by-step only) extremes

**Decision 3: Coverage matrix as a single-page accountability tool**
- Rows = 22 feature areas; Columns = 15 sessions; Cells = ● covered / ○ partial / blank
- Proves at a glance that every feature area has manual coverage
- If a feature has no ● in any column — it's a gap
- Forces session authors to explicitly map each session's coverage

**Decision 4: 15 sessions, 5 hours total (not 8-10 hours)**
- Longer than that and nobody runs the full suite
- Shorter and coverage gaps appear
- Session 15 (free exploration, 30 min) is mandatory, not optional — it's the highest-value session

**Decision 5: Per-session bug log (not centralized)**
- Centralized bug table (v3 style) never gets filled because testers defer writing bugs
- Session-level bug block: tester writes bugs immediately while context is fresh
- Summary at end aggregates all bugs for sign-off

**Decision 6: Severity: Critical / Major / Minor / Suggestion (not P0/P1/P2)**
- P0/P1/P2 is development priority — irrelevant to tester
- Critical = data loss or crash; Major = feature broken; Minor = cosmetic; Suggestion = improvement idea

## Risks / Trade-offs

- **[Risk] Session quality depends on tester's domain knowledge** — a tester unfamiliar with CAD/drafting may miss subtle bugs. *Mitigation: sessions include concrete "what to watch for" checks derived from actual bug history.*
- **[Risk] 5 hours is still long for a full pass** — developers may skip. *Mitigation: individual sessions are standalone (15-30 min). Dev can run Session 1 (25 min) as a quick smoke check before commit.*
- **[Risk] Coverage matrix may drift** — new features added without updating sessions. *Mitigation: coverage matrix update becomes part of Definition of Done for new features.*
- **[Risk] Free exploration session may feel unstructured** — testers may not know what to do. *Mitigation: provide concrete prompts: "Try to break the grid", "Find what happens with 50 objects", "Switch tools rapidly".*
- **[Risk] Archive of 5 prior versions creates clutter** — *Mitigation: single archive entry for v6 transition, older versions truly archived (moved to dated subfolder).*
