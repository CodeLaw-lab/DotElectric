## Why

Current manual test plan (`docs/План_ручного_тестирования.md`, v3.0) contains 139 tests across 29 sections but is impractical for regular use — it has no priority tiers, no smoke gate, and no coverage for the recently added Print Preview feature. All 4 prior versions (v1-v3, 50) live in archive and are explicitly deprecated. A new document authored from the current codebase is needed to serve both quick pre-commit verification and comprehensive pre-PR validation.

## What Changes

- Archive the current `docs/План_ручного_тестирования.md` to `docs/archive/`
- Create `docs/План_ручного_тестирования_v4.md` with two-tier structure:
  - **SMOKE** (~20 tests, ~10 min) — pre-commit gate: critical path only
  - **FULL** (~150 tests, ~60 min) — pre-PR gate: all functionality
- Every test is a compact checklist (not a table), ~40% denser than v3
- Results summary with per-section pass/fail/skip counts
- Full coverage of Print Preview (Ctrl+Shift+P → DocumentViewer) — the last implemented feature

## Capabilities

### New Capabilities
- `smoke-test-suite`: Quick pre-commit smoke tests covering the critical path (launch, create, draw, select, undo, save, print preview)
- `full-test-suite`: Comprehensive pre-PR tests covering all 23 modules (drawing, selection, resize, rotation, properties, colors, clipboard, undo/redo, grid/snap, zoom/pan, file operations, print preview, shortcuts, context menus, themes/settings, edge cases)
- `regression-test-suite`: Edge case and regression tests based on historically fixed bugs (Sprint 37–52)

### Modified Capabilities
*(none — no existing specs are being changed)*

## Impact

- `docs/План_ручного_тестирования.md` — moved to archive
- `docs/План_ручного_тестирования_v4.md` — new file created
- `docs/00_Индекс_документов.md` — updated to reflect archived status and new document
- No code changes, no API changes, no dependency changes
