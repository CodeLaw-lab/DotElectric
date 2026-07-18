## 1. Archive Previous Assets

- [x] 1.1 Move `docs/План_ручного_тестирования.md` to `docs/archive/66_План_ручного_тестирования_v3.md`
- [x] 1.2 Archive existing change `new-manual-test-plan` via `openspec archive`
- [x] 1.3 Remove current test plan from `docs/00_Индекс_документов.md` active list
- [x] 1.4 Add archived entry for v3 and note about new v6 document

## 2. Create Document Header and Instructions

- [x] 2.1 Write cover page: title "План ручного тестирования v6", date, tester fields, revision history
- [x] 2.2 Write introduction explaining Session-Based Testing approach and goal (find max bugs in realistic use)
- [x] 2.3 Write prerequisites: build steps, environment requirements, Severity classification (Critical/Major/Minor/Suggestion)
- [x] 2.4 Write instructions: how to run sessions, how to fill bug blocks, how to use coverage matrix

## 3. Build Coverage Matrix

- [x] 3.1 Create one-page table with 22 feature area rows (Drawing, Selection, Move/Nudge, Resize, Rotate, Properties, Text, Colors, Clipboard, Undo/Redo, Grid/Snap, Zoom/Pan, File Ops, Print Preview, Keyboard, Context Menu, Themes/Settings, StatusBar, Template Library, Canvas Rendering, Validation, Regression)
- [x] 3.2 Map each feature area to sessions using ● / ○ / blank notation
- [x] 3.3 Verify every row has at least one ● or ○

## 4. Write Session Cards 1–5 (Core Drawing)

- [x] 4.1 Session 1 "Черновик схемы" (25 min): scenario text + checklist + bug block
- [x] 4.2 Session 2 "Текстовый шаблон" (20 min): fonts, rotation, inline edit, multi-line
- [x] 4.3 Session 3 "Копировальная фабрика" (20 min): clipboard, batch undo, offset
- [x] 4.4 Session 4 "Навигатор" (20 min): zoom, pan, scrollbars, mouse capture
- [x] 4.5 Session 5 "Панель управления" (25 min): all properties, live update, ComboBox sync

## 5. Write Session Cards 6–10 (Advanced Manipulation)

- [x] 5.1 Session 6 "Ресайз-тур" (20 min): all handles, modifiers, min clamp, fixed edge
- [x] 5.2 Session 7 "Сетка и привязка" (15 min): grid steps, MinPixelSpacing, snap
- [x] 5.3 Session 8 "Файлы и мультивкладки" (20 min): tabs, save, dirty indicator, autosave
- [x] 5.4 Session 9 "Библиотека и форматы" (15 min): half-formats, custom, template library
- [x] 5.5 Session 10 "Цвет и стиль" (15 min): HEX, ARGB, Transparent, round-trip

## 6. Write Session Cards 11–15 (Integration & Free)

- [x] 6.1 Session 11 "Клавиатурный тур" (15 min): all shortcuts, RU/EN layouts, Escape routing
- [x] 6.2 Session 12 "Темы, настройки, UX" (15 min): themes, settings, context menus
- [x] 6.3 Session 13 "Граничные условия" (15 min): tiny objects, 50 objects, stress
- [x] 6.4 Session 14 "Print Preview + Печать" (10 min): DocumentViewer, rendering, print
- [x] 6.5 Session 15 "Свободное исследование" (30 min): unstructured exploration, log everything

## 7. Write Summary and Sign-off

- [x] 7.1 Create bug aggregation table from all session blocks
- [x] 7.2 Write conclusion section (Pass/Conditional Pass/Fail)
- [x] 7.3 Add signature fields (tester, developer, dates)
- [x] 7.4 Update `docs/00_Индекс_документов.md` with new document entry
