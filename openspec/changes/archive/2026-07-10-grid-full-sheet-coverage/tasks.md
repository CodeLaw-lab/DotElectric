## 1. GridManager — Full-sheet bounds + reusable buffer

- [x] 1.1 Заменить `GetViewportMicrons(2.0)` на `(0, 0, sheet.WidthMicrons, sheet.HeightMicrons)` в `GridManager.RefreshGridNodes()`
- [x] 1.2 Добавить поле `List<GridNode> _nodeBuffer` в GridManager для переиспользования списка
- [x] 1.3 Модифицировать `GridHelper.GenerateGridNodes()` — добавить опциональный параметр `List<GridNode>? reuseList = null`
- [x] 1.4 Передавать `_nodeBuffer` в `GenerateGridNodes()` из GridManager

## 2. Tests

- [x] 2.1 Переименовать `RefreshGridNodes_ViewportCulling_GeneratesLimitedNodes` → `RefreshGridNodes_FullSheet_GeneratesExpectedNodes`
- [x] 2.2 Убедиться, что `GridHelperTests` проходят без изменений (SelectDisplayStep + GenerateGridNodes сохраняют поведение)
- [x] 2.3 Добавить тест `RefreshGridNodes_GeneratesFullSheet_A4HighZoom` — A4, zoom 7.51×, 5mm step — узлы покрывают весь лист

## 3. Verify

- [x] 3.1 `dotnet test src/DotElectric.TemplateEditor.Tests` — все тесты проходят (1866 passed, 1 pre-existing skip)
- [x] 3.2 `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
