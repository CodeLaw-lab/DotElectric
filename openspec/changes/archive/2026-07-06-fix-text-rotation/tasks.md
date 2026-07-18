## 1. Fix RotatedCorners in Text.cs

- [x] 1.1 Fix `RotatedCorner1Y`: `+ W·sinθ` → `- W·sinθ`
- [x] 1.2 Fix `RotatedCorner2X`: `+ H·sinθ` → `- H·sinθ`
- [x] 1.3 Fix `RotatedCorner3X`: `+ H·sinθ` → `- H·sinθ`
- [x] 1.4 Fix `RotatedCorner3Y`: `+ W·sinθ` → `- W·sinθ`
- [x] 1.5 Fix `GetBoundingBox()` — independent formulas fixed (lines 270-271)

## 2. Build & Test

- [x] 2.1 Build solution — 0 errors
- [x] 2.2 Run all tests — 1834 passed, 0 failures, 1 pre-existing skip
- [x] 2.3 Verify `ContainsPoint` rotation still works correctly (existing tests cover this)
