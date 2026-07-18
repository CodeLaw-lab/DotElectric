## 1. Archive Current Document

- [ ] 1.1 Move `docs/План_ручного_тестирования.md` to `docs/archive/` with version suffix (e.g., `docs/archive/66_План_ручного_тестирования_v3.md`)
- [ ] 1.2 Remove the current document from `docs/00_Индекс_документов.md` active list
- [ ] 1.3 Add archived entry to `docs/00_Индекс_документов.md` archive section

## 2. Create Document Header and Introduction

- [ ] 2.1 Write document header: title, version (4.0), date, tester fields, revision history
- [ ] 2.2 Write prerequisites section: environment requirements, build steps, notation legend (PASS/FAIL/SKIP/BLOCKED)
- [ ] 2.3 Write overview explaining the two-tier structure (SMOKE / FULL) and when to run each
- [ ] 2.4 Create quick-reference summary table mapping test IDs to feature areas

## 3. Write Smoke Test Section (T-001 – T-020)

- [ ] 3.1 T-001: Application launch and build verification
- [ ] 3.2 T-002: Create A3 Landscape template
- [ ] 3.3 T-003: Draw line with Line tool
- [ ] 3.4 T-004: Draw rectangle with Rectangle tool
- [ ] 3.5 T-005: Create text with Text tool
- [ ] 3.6 T-006: Single-click object selection
- [ ] 3.7 T-007: Shift+Click multi-selection
- [ ] 3.8 T-008: Drag move single object
- [ ] 3.9 T-009: Arrow key nudge
- [ ] 3.10 T-010: Resize rectangle via corner handle
- [ ] 3.11 T-011: Rotate text 90° (E key)
- [ ] 3.12 T-012: Copy and paste object
- [ ] 3.13 T-013: Cut and paste object
- [ ] 3.14 T-014: Undo (Ctrl+Z) and Redo (Ctrl+Y)
- [ ] 3.15 T-015: Save new file as .tdel
- [ ] 3.16 T-016: Open saved .tdel file
- [ ] 3.17 T-017: Print Preview (Ctrl+Shift+P)
- [ ] 3.18 T-018: Grid toggle (F7) and Snap toggle (F8)
- [ ] 3.19 T-019: Zoom via mouse wheel and Ctrl+0 (Fit to Screen)
- [ ] 3.20 T-020: Middle-button pan

## 4. Write Drawing Tools Section (T-021 – T-030)

- [ ] 4.1 T-021: Line tool with Shift constraint (0°/45°/90°)
- [ ] 4.2 T-022: Rectangle tool with Shift constraint (square)
- [ ] 4.3 T-023: Text default properties check
- [ ] 4.4 T-024: Preview shapes follow cursor in real time
- [ ] 4.5 T-025: Escape cancels drawing (all tools)
- [ ] 4.6 T-026: Tool switching during drawing (V/L/R/T)
- [ ] 4.7 T-027: Line with snap constraint
- [ ] 4.8 T-028: Rectangle with snap constraint
- [ ] 4.9 T-029: Text created without inline editor opening (single click)
- [ ] 4.10 T-030: DrawingLineTool passes LineType to created line

## 5. Write Selection Section (T-031 – T-040)

- [ ] 5.1 T-031: Single click selection visual state (blue #0078D4 highlight)
- [ ] 5.2 T-032: Shift+Click additive selection
- [ ] 5.3 T-033: Ctrl+Click toggle selection
- [ ] 5.4 T-034: Click empty space clears selection
- [ ] 5.5 T-035: LTR selection box (enclosed only)
- [ ] 5.6 T-036: RTL selection box (any intersection)
- [ ] 5.7 T-037: Rectangle border-band hit test (center click no-select)
- [ ] 5.8 T-038: Small rectangle fully selectable (<10mm)
- [ ] 5.9 T-039: Z-order hit test (top object selected in overlap)
- [ ] 5.10 T-040: Multi-select shows markers on all selected objects

## 6. Write Move & Nudge Section (T-041 – T-048)

- [ ] 6.1 T-041: Single object drag without drift
- [ ] 6.2 T-042: Multi-object drag preserves relative positions
- [ ] 6.3 T-043: Drag threshold (sub-threshold does not move)
- [ ] 6.4 T-044: Snap-constrained drag
- [ ] 6.5 T-045: Sheet bounds clamp during drag
- [ ] 6.6 T-046: Arrow key nudge (Snap ON = grid step, OFF = 0.1mm)
- [ ] 6.7 T-047: Shift+arrow big nudge (10mm with Snap, grid step without)
- [ ] 6.8 T-048: Multi-object nudge

## 7. Write Resize Section (T-049 – T-058)

- [ ] 7.1 T-049: Resize rectangle via corner handle (opposite corner fixed)
- [ ] 7.2 T-050: Resize rectangle via side handle (one dimension only)
- [ ] 7.3 T-051: Resize with Shift (maintain proportions)
- [ ] 7.4 T-052: Resize with Ctrl (centered, symmetric)
- [ ] 7.5 T-053: Minimum size clamp — moving edge stops at 1mm
- [ ] 7.6 T-054: Minimum size clamp — fixed edge does not move
- [ ] 7.7 T-055: Resize line via endpoint handles
- [ ] 7.8 T-056: Resize text via corner handles
- [ ] 7.9 T-057: Sheet bounds clamp during resize
- [ ] 7.10 T-058: Resize undo/redo

## 8. Write Rotation Section (T-059 – T-064)

- [ ] 8.1 T-059: E key rotates 90° clockwise
- [ ] 8.2 T-060: Shift+E rotates 90° counter-clockwise
- [ ] 8.3 T-061: Multi-object rotation
- [ ] 8.4 T-062: Arbitrary angle via Properties Panel
- [ ] 8.5 T-063: Negative/normalized angles (-90 → 270, 360 → 0)
- [ ] 8.6 T-064: Rotation undo/redo

## 9. Write Properties Panel Section (T-065 – T-076)

- [ ] 9.1 T-065: Line properties visible on line selection
- [ ] 9.2 T-066: Rectangle properties visible on rectangle selection
- [ ] 9.3 T-067: Text properties visible on text selection
- [ ] 9.4 T-068: Change line StartX via TextBox + Enter
- [ ] 9.5 T-069: Change LineType via ComboBox (canvas updates)
- [ ] 9.6 T-070: Change StrokeThickness via TextBox + Enter
- [ ] 9.7 T-071: Change FillColor via HEX input
- [ ] 9.8 T-072: Change FontSize via TextBox + Enter
- [ ] 9.9 T-073: Change FontName (ГОСТ А ↔ ГОСТ Б)
- [ ] 9.10 T-074: Live update during resize/move
- [ ] 9.11 T-075: ComboBox sync on object switch
- [ ] 9.12 T-076: Enter commits, Escape reverts

## 10. Write Text Editing Section (T-077 – T-082)

- [ ] 10.1 T-077: Double-click opens inline editor
- [ ] 10.2 T-078: Ctrl+Enter commits inline edit
- [ ] 10.3 T-079: Escape cancels inline edit
- [ ] 10.4 T-080: Multi-line editing (Enter = new line, Ctrl+Enter = commit)
- [ ] 10.5 T-081: Inline edit on rotated text
- [ ] 10.6 T-082: IsEditable=false blocks inline editing

## 11. Write Colors Section (T-083 – T-089)

- [ ] 11.1 T-083: Default colors (Line/Rectangle/Text)
- [ ] 11.2 T-084: Change StrokeColor via HEX
- [ ] 11.3 T-085: Change FillColor to solid color
- [ ] 11.4 T-086: Change FillColor to Transparent
- [ ] 11.5 T-087: Change Foreground via HEX
- [ ] 11.6 T-088: ARGB hex (#AARRGGBB) support
- [ ] 11.7 T-089: Invalid HEX validation error

## 12. Write Clipboard Section (T-090 – T-098)

- [ ] 12.1 T-090: Copy single object
- [ ] 12.2 T-091: Copy multi-object
- [ ] 12.3 T-092: Paste with offset, auto-select pasted
- [ ] 12.4 T-093: Re-paste 5 times creates independent copies
- [ ] 12.5 T-094: Paste offset resets after new Copy
- [ ] 12.6 T-095: Cut single object
- [ ] 12.7 T-096: Cut multi-object (BatchCommand undo)
- [ ] 12.8 T-097: Paste on empty clipboard → StatusBar feedback
- [ ] 12.9 T-098: Clipboard clears on tab close

## 13. Write Undo/Redo Section (T-099 – T-106)

- [ ] 13.1 T-099: Basic undo/redo
- [ ] 13.2 T-100: 50-level undo stack
- [ ] 13.3 T-101: Undo after redo + new action clears redo stack
- [ ] 13.4 T-102: Undo display names in Edit menu
- [ ] 13.5 T-103: BatchCommand undo (multi-delete as single action)
- [ ] 13.6 T-104: Undo/Redo after Save
- [ ] 13.7 T-105: Orphaned selection cleanup after undo
- [ ] 13.8 T-106: Undo/Redo across all operation types

## 14. Write Grid & Snap Section (T-107 – T-114)

- [ ] 14.1 T-107: Grid toggle via F7 and StatusBar button
- [ ] 14.2 T-108: Snap toggle via F8 and StatusBar button
- [ ] 14.3 T-109: Grid steps 5mm, 10mm, 2mm visible at correct zoom
- [ ] 14.4 T-110: 1mm grid hidden at 100% zoom, visible at 500%+
- [ ] 14.5 T-111: Snap-constrained drawing
- [ ] 14.6 T-112: Snap-constrained dragging
- [ ] 14.7 T-113: Grid autoscroll during pan
- [ ] 14.8 T-114: MaxGridNodes boundary (A0+ at small zoom)

## 15. Write Zoom & Pan Section (T-115 – T-123)

- [ ] 15.1 T-115: Mouse wheel zoom (10%–1000%)
- [ ] 15.2 T-116: Zoom via ComboBox selection
- [ ] 15.3 T-117: Zoom via ComboBox manual entry
- [ ] 15.4 T-118: Ctrl+0 Fit to Screen
- [ ] 15.5 T-119: Middle-button pan
- [ ] 15.6 T-120: Space+left-button pan
- [ ] 15.7 T-121: Mouse capture outside canvas during pan
- [ ] 15.8 T-122: No runaway pan acceleration
- [ ] 15.9 T-123: Scrollbar interaction

## 16. Write File Operations Section (T-124 – T-134)

- [ ] 16.1 T-124: New template — all standard A0–A4 formats (both orientations)
- [ ] 16.2 T-125: New template — half-formats (A4x2 through A0x2)
- [ ] 16.3 T-126: New template — custom size
- [ ] 16.4 T-127: New template — custom size validation (negative, zero, max)
- [ ] 16.5 T-128: Save new file via Ctrl+S
- [ ] 16.6 T-129: Save existing file (no dialog)
- [ ] 16.7 T-130: Save As via F12
- [ ] 16.8 T-131: Open .tdel file via Ctrl+O
- [ ] 16.9 T-132: Close with unsaved changes dialog
- [ ] 16.10 T-133: Autosave and recovery
- [ ] 16.11 T-134: Template Library import and remove

## 17. Write Print Preview Section (T-135 – T-139)

- [ ] 17.1 T-135: Ctrl+Shift+P opens DocumentViewer
- [ ] 17.2 T-136: All object types render in preview
- [ ] 17.3 T-137: Colors and line types render correctly
- [ ] 17.4 T-138: Rotated text renders at correct angle
- [ ] 17.5 T-139: Print button in preview window works

## 18. Write Keyboard Shortcuts Section (T-140 – T-149)

- [ ] 18.1 T-140: File shortcuts (Ctrl+N/O/S/P, F12)
- [ ] 18.2 T-141: Edit shortcuts (Ctrl+Z/Y/C/V/X/A, Delete)
- [ ] 18.3 T-142: Tool shortcuts (V/L/R/T/E, Shift+E) with English layout
- [ ] 18.4 T-143: Tool shortcuts with Russian layout
- [ ] 18.5 T-144: Ctrl+modifier shortcuts not intercepted (Ctrl+V/L/R/T)
- [ ] 18.6 T-145: Escape routing in all states
- [ ] 18.7 T-146: View shortcuts (Ctrl+0/+/- , F7/F8/F9)
- [ ] 18.8 T-147: Arrow key nudge
- [ ] 18.9 T-148: Ctrl+Shift+P print preview

## 19. Write Context Menus Section (T-150 – T-153)

- [ ] 19.1 T-150: Right-click on canvas shows context menu
- [ ] 19.2 T-151: Right-click on object shows context menu with working commands
- [ ] 19.3 T-152: Right-click on tab shows Close/Close Others/Close All
- [ ] 19.4 T-153: Middle-click on tab closes it

## 20. Write Themes & Settings Section (T-154 – T-160)

- [ ] 20.1 T-154: F9 toggles Light/Dark theme
- [ ] 20.2 T-155: Canvas stays white regardless of theme
- [ ] 20.3 T-156: File → Settings opens dialog with 4 sections
- [ ] 20.4 T-157: Change theme via settings
- [ ] 20.5 T-158: Change grid step via settings
- [ ] 20.6 T-159: Change default format via settings
- [ ] 20.7 T-160: Settings persistence across restart

## 21. Write StatusBar Section (T-161 – T-164)

- [ ] 21.1 T-161: StatusBar shows format, size, orientation
- [ ] 21.2 T-162: StatusBar shows current zoom
- [ ] 21.3 T-163: StatusBar Grid/Snap toggle buttons
- [ ] 21.4 T-164: StatusBar clipboard feedback messages

## 22. Write Regression Section (T-165 – T-174)

- [ ] 22.1 T-165: S37 regression — ToModelPoint (click selects correct object)
- [ ] 22.2 T-166: S41 regression — drag without drift
- [ ] 22.3 T-167: S45 regression — pan without runaway acceleration
- [ ] 22.4 T-168: S47 regression — 1mm grid at 500% zoom
- [ ] 22.5 T-169: S49 regression — resize clamp preserves fixed edge
- [ ] 22.6 T-170: S50 regression — re-paste creates independent copies
- [ ] 22.7 T-171: S50 regression — Ctrl+V not intercepted
- [ ] 22.8 T-172: S51 regression — pan with mouse capture outside canvas
- [ ] 22.9 T-173: Heavy load — 50 objects, select all, delete, undo
- [ ] 22.10 T-174: Round-trip — micron coordinate precision

## 23. Write Results Section and Finalize

- [ ] 23.1 Write results summary table (per-section PASS/FAIL/SKIP/BLOCKED counts)
- [ ] 23.2 Write defect log template
- [ ] 23.3 Write sign-off section (tester signature, date, conclusion)
- [ ] 23.4 Update `docs/00_Индекс_документов.md` with new document entry
- [ ] 23.5 Final review — verify all test IDs are unique, no gaps, all features covered
