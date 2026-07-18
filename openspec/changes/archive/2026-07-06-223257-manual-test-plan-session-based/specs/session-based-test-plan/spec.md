## ADDED Requirements

### Requirement: Document structure
The session-based test plan document SHALL have four sections: (1) cover page with instructions, (2) coverage matrix mapping 22 feature areas to 15 sessions, (3) 15 session cards with hybrid format, (4) summary and sign-off.

#### Scenario: Document has all required sections
- **WHEN** the document is opened
- **THEN** it contains cover page, coverage matrix, 15 session cards, and summary

### Requirement: Session card format (hybrid)
Each session card SHALL include: (a) free-text scenario description (1-3 paragraphs), (b) "What to watch for" checklist (5-15 items), (c) per-session bug log table with columns: #, Action, Expected, Actual, Severity.

#### Scenario: Session card has hybrid format
- **WHEN** a tester opens any session card
- **THEN** they see a free-text scenario, a checklist, and a blank bug log

### Requirement: Coverage matrix
The coverage matrix SHALL be a one-page table with 22 feature areas as rows and 15 sessions as columns. Each cell SHALL be marked ● (full coverage), ○ (partial), or blank. Every feature area SHALL have at least one ● or ○.

#### Scenario: Coverage matrix proves completeness
- **WHEN** a reviewer inspects the coverage matrix
- **THEN** every feature area row has at least one ● or ○ filled

### Requirement: Session 1 — "Черновик схемы" (25 min)
The first session SHALL simulate creating a real schematic drawing on A3 landscape: draw 3 lines (including with Shift constraint), 2 rectangles (including square with Shift), and 1 text object. The tester SHALL move objects via drag, resize via corner handles, rotate via E key, and exercise Undo/Redo chain. Save as .tdel, close tab, reopen, visually verify round-trip fidelity.

#### Scenario: Session 1 covers drawing, move, resize, rotate, undo, save round-trip
- **WHEN** the tester follows the Session 1 scenario
- **THEN** they verify preview shapes during drawing, Shift constraints (horizontal/vertical line, square rectangle), drag without drift, resize handles visible, rotate 90° CW/CCW, undo chain preserves state, save/reopen shows identical objects

### Requirement: Session 2 — "Текстовый шаблон" (20 min)
The second session SHALL create 10 text objects with varying font names (ГОСТ А, ГОСТ Б), font sizes (2.5mm, 3.5mm, 5mm, 10mm), rotation angles (0, 45, 90, 180, 270, 359), text alignments (Left, Center, Right), and text wrapping on/off. The tester SHALL exercise inline editing (double-click, Ctrl+Enter commit, Escape cancel), multi-line entry (Enter = new line, Ctrl+Enter = commit), and Key/IsEditable properties.

#### Scenario: Session 2 covers fonts, rotation, inline editing, multi-line
- **WHEN** the tester follows the Session 2 scenario
- **THEN** they verify GOST font rendering, rotation at arbitrary angles, inline editor commit/cancel, multi-line with wrapping, IsEditable=false blocks editing

### Requirement: Session 3 — "Копировальная фабрика" (20 min)
The third session SHALL create 5 objects, copy them, paste 5 times verifying offset accumulation, cut objects, delete originals, exercise BatchCommand undo, and verify clipboard clears on tab close.

#### Scenario: Session 3 covers clipboard, batch undo, offset
- **WHEN** the tester follows the Session 3 scenario
- **THEN** they verify paste offset increments, re-paste creates independent objects (deleting one doesn't affect others), multi-cut undo restores all in one action, StatusBar shows correct Russian pluralization, clipboard clears on tab close

### Requirement: Session 4 — "Навигатор" (20 min)
The fourth session SHALL exercise zoom across full range (10%-1000%), pan with middle mouse button and Space+drag, verify no runaway acceleration, mouse capture outside canvas during pan, Fit to Screen (Ctrl+0), scrollbar visibility, and zoom via ComboBox.

#### Scenario: Session 4 covers zoom, pan, scrollbars
- **WHEN** the tester follows the Session 4 scenario
- **THEN** they verify zoom range limits, pan without acceleration, mouse capture works outside canvas, Ctrl+0 fits correctly, scrollbars appear when viewport < canvas, ComboBox zoom entry works

### Requirement: Session 5 — "Панель управления" (25 min)
The fifth session SHALL select each object type and modify ALL available properties. The tester SHALL verify live property panel update during resize/move, ComboBox sync when switching objects, Enter commits field changes, Escape reverts, validation errors display.

#### Scenario: Session 5 covers properties panel, live update, sync
- **WHEN** the tester follows the Session 5 scenario
- **THEN** they verify all Line/Rectangle/Text properties are editable, ComboBox reflects current value on object switch, resize updates panel in real time, Undo restores previous values with panel update, invalid HEX shows validation error

### Requirement: Session 6 — "Ресайз-тур" (20 min)
The sixth session SHALL exercise all resize handles (8 for rectangle, 2 for line, 4 for text), Shift proportions, Ctrl centered resize, minimum size clamp (1mm), fixed-edge preservation during clamp, snap-constrained resize, sheet bounds clamp, and undo.

#### Scenario: Session 6 covers resize handles, modifiers, clamp
- **WHEN** the tester follows the Session 6 scenario
- **THEN** they verify all handle positions, Shift maintains aspect ratio, Ctrl resizes from center, minimum size stops at 1mm, fixed edge does NOT move during clamp, line endpoint resize moves only one endpoint

### Requirement: Session 7 — "Сетка и привязка" (15 min)
The seventh session SHALL test grid at steps 1mm, 2mm, 5mm, 10mm at various zoom levels, verify MinPixelSpacing (1mm hidden at 100%, visible at 500%+), snap-constrained drawing and dragging, A0x2 sheet with 1mm grid (MaxGridNodes boundary), and grid toggle via F7 and StatusBar.

#### Scenario: Session 7 covers grid, snap, thresholds
- **WHEN** the tester follows the Session 7 scenario
- **THEN** they verify grid dot rendering, 1mm grid auto-hides below 500% zoom, snap snaps to correct step, drawing with snap creates aligned objects, grid toggles work from keyboard and StatusBar

### Requirement: Session 8 — "Файлы и мультивкладки" (20 min)
The eighth session SHALL create 3 tabs (A3 Landscape, A4 Portrait, A0x2), verify object isolation between tabs, save each with Ctrl+S, create unsaved changes on one tab and close with dialog (Save/Discard/Cancel), verify dirty indicator (*), and test autosave recovery.

#### Scenario: Session 8 covers tabs, save, dirty indicator, autosave
- **WHEN** the tester follows the Session 8 scenario
- **THEN** they verify each tab has independent objects, dirty star appears on edit and disappears on save, close dialog offers 3 options, autosave creates files in %APPDATA%, recovery dialog appears on restart

### Requirement: Session 9 — "Библиотека и форматы" (15 min)
The ninth session SHALL create all half-formats (A4x2 through A0x2), create custom format via dialog, verify validation on negative/zero sizes, and exercise Template Library (import .tdel, remove template, double-click to open).

#### Scenario: Session 9 covers half-formats, custom sizes, template library
- **WHEN** the tester follows the Session 9 scenario
- **THEN** they verify half-format dimensions match specification, custom dialog validates inputs, library import copies file to %APPDATA%, library remove deletes with confirmation, double-click opens template in new tab

### Requirement: Session 10 — "Цвет и стиль" (15 min)
The tenth session SHALL change StrokeColor, FillColor, and Foreground on all object types using HEX values (#RRGGBB, #AARRGGBB, Transparent). Verify colors render on canvas, survive save/load round-trip, and appear correctly in Print Preview.

#### Scenario: Session 10 covers colors, transparency, round-trip
- **WHEN** the tester follows the Session 10 scenario
- **THEN** they verify HEX color renders correctly, ARGB alpha works, Transparent fill shows no fill, colors persist after save/reopen, Print Preview shows correct colors, invalid HEX shows validation error

### Requirement: Session 11 — "Клавиатурный тур" (15 min)
The eleventh session SHALL test ALL keyboard shortcuts with both English and Russian keyboard layouts: tool switching (V/L/R/T), undo/redo (Ctrl+Z/Y), clipboard (Ctrl+C/V/X), file ops (Ctrl+N/O/S, F12), zoom (Ctrl+0/+/-), grid/snap (F7/F9), rotate (E/Shift+E), nudge (arrows, Shift+arrows), and Escape routing in all states.

#### Scenario: Session 11 covers keyboard shortcuts, layout independence
- **WHEN** the tester follows the Session 11 scenario
- **THEN** they verify all 25+ shortcuts work, RU layout tool switching works via physical key position, Ctrl+V/L/R/T are NOT intercepted by tool switcher, Escape cancels drawing/inline-edit/resize/selection appropriately

### Requirement: Session 12 — "Темы, настройки, UX" (15 min)
The twelfth session SHALL toggle Light/Dark theme via F9 and Settings dialog, verify canvas stays white, change grid step and default format via Settings, verify persistence across restart, test context menus on canvas (right-click on empty space and on object) and on tabs (Close/Close Other/Close All).

#### Scenario: Session 12 covers themes, settings, context menus, persistence
- **WHEN** the tester follows the Session 12 scenario
- **THEN** they verify theme toggle updates all panels, canvas remains white, Settings dialog has 4 sections, settings persist after restart, context menu commands work (Cut/Copy/Paste/Delete/Select All), tab context menu closes correct tabs

### Requirement: Session 13 — "Граничные условия" (15 min)
The thirteenth session SHALL stress test with 1x1mm objects, objects positioned at 5mm from sheet edges, 50 objects created, Select All + Delete + Undo batch, rapid tool switching, Copy 1 object + Paste 20 times, Undo 50 levels.

#### Scenario: Session 13 covers edge cases, stress, batch operations
- **WHEN** the tester follows the Session 13 scenario
- **THEN** they verify tiny objects render and select correctly, sheet bounds clamp prevents overflow, 50 objects handled without performance degradation, batch undo restores all in one action, 20 pasted objects are all independent, 50-level undo stack works

### Requirement: Session 14 — "Print Preview + Печать" (10 min)
The fourteenth session SHALL open Print Preview (Ctrl+Shift+P) with template containing all object types, colors, line types, and rotated text. Verify DocumentViewer rendering, FitToWidth, Print button opens standard print dialog.

#### Scenario: Session 14 covers print preview, rendering
- **WHEN** the tester follows the Session 14 scenario
- **THEN** they verify DocumentViewer opens, all object types render with correct colors and line styles, rotated text appears at correct angle, Print button works, Close returns to editor

### Requirement: Session 15 — "Свободное исследование" (30 min)
The fifteenth session SHALL be unstructured: the tester uses the application freely for 30 minutes, performing any tasks they choose. ALL observations, issues, and suggestions SHALL be logged in the bug block. This session is expected to find ~60% of all bugs discovered during the test pass.

#### Scenario: Session 15 covers exploratory testing
- **WHEN** the tester performs any actions for 30 minutes
- **THEN** they log every unexpected behavior, visual glitch, or missing feature in the bug block

### Requirement: Bug severity classification
Bugs SHALL be classified as: Critical (data loss, crash), Major (feature broken or unusable), Minor (cosmetic, layout, or behavior quirk), Suggestion (improvement idea).

#### Scenario: Bug severity labels are clear
- **WHEN** the tester logs a bug
- **THEN** they assign one of four severity levels

### Requirement: Summary and sign-off
The document SHALL end with a summary section aggregating bugs by severity, a conclusion (Pass/Conditional Pass/Fail), and signature fields for tester and responsible developer.

#### Scenario: Summary aggregates all session bugs
- **WHEN** the test pass is complete
- **THEN** the tester copies bugs from all session blocks into the summary, calculates totals by severity, and records a conclusion
