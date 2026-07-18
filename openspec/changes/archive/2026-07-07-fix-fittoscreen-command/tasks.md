## 1. Core fix

- [x] 1.1 Переименовать `EditorViewModel.FitToScreenCommand(string? parameter)` → `FitToScreen(string? parameter)`
- [x] 1.2 Удалить public overload `FitToScreen(double canvasWidth, double canvasHeight)` — делегирование перенести в тело команды

## 2. Тесты

- [x] 2.1 Проверить тесты на `editor.FitToScreen()` — заменить на `editor.FitToScreenCommand.Execute()` или прямой вызов ZoomPanManager

## 3. Verification

- [x] 3.1 Build solution: `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 3.2 Run full test suite: `dotnet test src/DotElectric.TemplateEditor.Tests` — all pass
