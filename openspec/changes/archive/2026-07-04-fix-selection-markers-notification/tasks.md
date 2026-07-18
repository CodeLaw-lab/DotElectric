## 1. SelectionManager — Add `OnPropertyChanged` for `ShowSelectionMarkers`

- [x] 1.1 Add `OnPropertyChanged(nameof(ShowSelectionMarkers))` to the `CollectionChanged` handler in `SelectionManager` constructor

## 2. Verify

- [x] 2.1 Build: `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 2.2 Tests: `dotnet test src/DotElectric.TemplateEditor.Tests` — 1780 passed, 1 pre-existing skip
- [ ] 2.3 UI smoke test: запустить приложение, проверить маркеры выделения при выборе объектов
