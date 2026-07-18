## Why

Session 1 ("Черновик схемы") ручного тестирования v6 выявил 4 критических (P1) и 2 важных (P2) дефекта в инструментах рисования/редактирования. Preview текста не следует за мышью, Escape при Resize блокирует канвас, смена инструмента во время рисования залипает, Shift не даёт 45° диагональ, Undo не восстанавливает выделение. Эти дефекты блокируют базовый сценарий работы пользователя и должны быть исправлены перед следующим прогоном.

## What Changes

- TextTool: ре-ассайн `PreviewText` после мутации координат в OnMouseMove
- ResizeTool: вызов `PopTool()` при Escape для разблокировки стека инструментов
- MainWindow.xaml.cs/ToolManager: сброс состояния текущего инструмента при смене
- DrawingLineTool: 45° диагональ при Shift (3-я ветка в ApplyConstraint)
- EditorViewModel: восстановление выделения после Undo удаления
- DrawingLineTool/TextTool: переключение на Select при DoubleClick (консистентность с Escape)
- DrawingRectangleTool: коррекция clamp при создании у границы листа

## Capabilities

### New Capabilities
- `text-preview-fix`: исправление позиционирования preview текста при перетаскивании
- `resize-escape-fix`: корректная обработка Escape в ResizeTool (PopTool)
- `tool-switch-reset`: сброс состояния инструмента при принудительной смене
- `line-shift-45`: поддержка 45° диагонали при Shift в DrawingLineTool
- `undo-selection-restore`: восстановление выделения после Undo удаления
- `doubleclick-select`: переключение на Select при DoubleClick в Line/Text инструментах
- `rect-clamp-fix`: защита от выхода прямоугольника за границу листа при создании

### Modified Capabilities
<!-- No spec-level requirement changes — implementation fixes only -->

## Impact

- `src/DotElectric.TemplateEditor/Tools/TextTool.cs:52-60` — добавлен ре-ассайн preview
- `src/DotElectric.TemplateEditor/Tools/ResizeTool.cs:233-241` — добавлен PopTool
- `src/DotElectric.TemplateEditor/MainWindow.xaml.cs:26-31` — сброс состояния при смене
- `src/DotElectric.TemplateEditor/Tools/DrawingLineTool.cs:123-139` — 45° ветка
- `src/DotElectric.TemplateEditor/ViewModels/EditorViewModel.cs:441` — восстановление выделения
- `src/DotElectric.TemplateEditor/Tools/DrawingLineTool.cs:84-90` — Select после DoubleClick
- `src/DotElectric.TemplateEditor/Tools/TextTool.cs:87-92` — Select после DoubleClick
- `src/DotElectric.TemplateEditor/Tools/DrawingRectangleTool.cs:68-78` — коррекция clamp
