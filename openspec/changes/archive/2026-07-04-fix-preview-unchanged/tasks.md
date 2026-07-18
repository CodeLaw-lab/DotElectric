## 1. PreviewManager — Replace `[ObservableProperty]` with Manual Setters

- [x] 1.1 Replace `[ObservableProperty] private Line? _previewLine;` with backing field + manual setter (`OnPropertyChanged()` unconditional)
- [x] 1.2 Replace `[ObservableProperty] private Rectangle? _previewRectangle;` with backing field + manual setter
- [x] 1.3 Replace `[ObservableProperty] private Text? _previewText;` with backing field + manual setter

## 2. Verify

- [x] 2.1 Build: `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 2.2 Tests: `dotnet test src/DotElectric.TemplateEditor.Tests` — 1780 passed, 1 pre-existing skip
- [ ] 2.3 UI smoke test: запустить приложение, проверить предпросмотр при рисовании Line, Rectangle, Text
