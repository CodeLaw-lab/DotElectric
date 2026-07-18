## 1. Marker DataTemplate

- [x] 1.1 Add `DataTemplate DataType="{x:Type models:Text}"` to `EditorCanvas.xaml` ItemsControl.Resources (после Rectangle DataTemplate)
- [x] 1.2 Add 4 square markers with `MarkerPosition` bindings to `RotatedCorner0X/Y`–`RotatedCorner3X/Y`
- [x] 1.3 Verify markers appear when Text is selected (including rotation)

## 2. Verify & Test

- [x] 2.1 Build solution — 0 errors
- [x] 2.2 Run existing tests — 0 failures (1834 passed, 1 pre-existing skip)
- [ ] 2.3 Manual test: create Text → verify 4 markers at corners
- [ ] 2.4 Manual test: rotate Text to 45° → verify markers follow corners
- [ ] 2.5 Manual test: multi-select Text + Line → both marker sets visible
