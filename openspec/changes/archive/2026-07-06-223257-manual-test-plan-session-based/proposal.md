## Why

Current manual test plan (`docs/План_ручного_тестирования.md`, v3.0) uses 139 table-format test cases across 29 sections. Despite 5 prior versions (all archived), none have proven practical — results sections have never been filled, and the document's rigid step-by-step format fails its core purpose: finding bugs through realistic application use. 1796 automated unit tests already cover models, commands, and services; manual testing should focus on what automation cannot: visual rendering, interactive feel, keyboard routing, cross-feature integration, and real-time feedback.

A completely new approach — Session-Based Testing — replaces isolated checklists with realistic 15-30 minute workflow sessions designed to stress combinatorial feature interactions where bugs actually hide (Sprint 37-52 history shows 60% of bugs are cross-feature).

## What Changes

- **Archive** current `docs/План_ручного_тестирования.md` → `docs/archive/`
- **Archive** existing change `new-manual-test-plan` → completed/archived
- **Remove** current test plan from `docs/00_Индекс_документов.md` active list, add to archive
- **Create** `docs/План_ручного_тестирования_v6.md` — new session-based manual test plan
- **Create** `docs/00_Индекс_документов.md` — add new document entry

## Capabilities

### New Capabilities

- `session-based-test-plan`: A complete manual test document using Session-Based Testing methodology. Contains 15 workflow sessions (15-30 min each, ~5 hours total), each with a free-text scenario, a compact "what to watch for" checklist, and a per-session bug log. Includes a one-page coverage matrix proving 140 visual/interactive behaviors are covered across all 22 feature areas.

### Modified Capabilities

*(none — no existing specs are being changed)*

## Impact

- `docs/План_ручного_тестирования.md` — moved to archive
- `docs/План_ручного_тестирования_v6.md` — new file created
- `docs/00_Индекс_документов.md` — updated entries
- `openspec/changes/new-manual-test-plan/` — archived
- No code changes, no API changes, no dependency changes
