## 1. Core fix

- [x] 1.1 Remove `IsCentered ? 0 :` guard from `ZoomPanManager.CanvasOffsetX` — change to `=> -PanOffsetX`
- [x] 1.2 Remove `IsCentered ? 0 :` guard from `ZoomPanManager.CanvasOffsetY` — change to `=> -PanOffsetY`

## 2. Tests

- [x] 2.1 Add ZoomPanManager test: CanvasOffsetX equals -PanOffsetX when IsCentered=true after PanCanvas
- [x] 2.2 Add ZoomPanManager test: CanvasOffsetX equals -PanOffsetX when IsCentered=false after PanCanvas — already covered by existing `CanvasOffsetX_WhenNotCentered_IsNegativePanOffset`
- [x] 2.3 Add ZoomPanManager test: CanvasOffsetX equals 0 when IsCentered=true and no panning occurred — already covered by existing `CanvasOffsetX_WhenCentered_ReturnsZero`

## 3. Verification

- [x] 3.1 Build solution: 0 errors, 0 warnings
- [x] 3.2 Run full test suite: 1846 passed, 1 pre-existing skip
