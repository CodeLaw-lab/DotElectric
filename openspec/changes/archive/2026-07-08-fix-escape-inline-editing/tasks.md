## 1. Focus guard in CanvasInputRouter

- [x] 1.1 Add focus guard check at top of `RoutePreviewKeyDown`: `if (FocusManager.GetFocusedElement(canvas) is UIElement focused && focused != canvas) return;`
- [x] 1.2 Build solution and verify 0 errors

## 2. Verify behaviour

- [x] 2.1 Run all existing tests: `dotnet test src/DotElectric.TemplateEditor.Tests`
- [ ] 2.2 Manual verification: Escape cancels inline editing when TextBox in focus
- [ ] 2.3 Manual verification: Escape clears selection when TextBox not in focus
