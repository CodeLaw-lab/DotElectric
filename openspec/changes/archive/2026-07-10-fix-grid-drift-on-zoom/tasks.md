## 1. Core fix

- [x] 1.1 In `ZoomPanManager.OnZoomChanged()`, add `if (IsCentered) CenterCanvas();` before `_onZoomChanged()` call

## 2. Regression test

- [x] 2.1 Add test `ZoomChanged_WhenNotCentered_DoesNotResetPanOffset` — zoom in, pan, zoom to still-not-centered → PanOffset preserved
- [x] 2.2 Add test `ZoomChanged_WhenBecomesCentered_ResetsPanOffset` — zoom in, pan, zoom out until centered → PanOffset reset to 0

## 3. Verify

- [x] 3.1 `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 3.2 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1868 passed, 0 failed, 1 pre-existing skip
