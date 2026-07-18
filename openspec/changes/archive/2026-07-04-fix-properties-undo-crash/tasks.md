## 1. LinePropertiesViewModel — fix lambda captures

- [x] 1.1 `ChangeStartX` — capture `var line = _line`
- [x] 1.2 `ChangeStartY` — capture `var line = _line`
- [x] 1.3 `ChangeEndX` — capture `var line = _line`
- [x] 1.4 `ChangeEndY` — capture `var line = _line`
- [x] 1.5 `ChangeLineType` — capture `var line = _line`
- [x] 1.6 `ChangeStrokeThickness` — capture `var line = _line`
- [x] 1.7 `ChangeStrokeColor` — capture `var line = _line`

## 2. RectanglePropertiesViewModel — fix lambda captures

- [x] 2.1 `ChangeX` — capture `var rect = _rect`
- [x] 2.2 `ChangeY` — capture `var rect = _rect`
- [x] 2.3 `ChangeWidth` — capture `var rect = _rect`
- [x] 2.4 `ChangeHeight` — capture `var rect = _rect`
- [x] 2.5 `ChangeLineType` — capture `var rect = _rect`
- [x] 2.6 `ChangeStrokeThickness` — capture `var rect = _rect`
- [x] 2.7 `ChangeStrokeColor` — capture `var rect = _rect`
- [x] 2.8 `ChangeFillColor` — capture `var rect = _rect`

## 3. TextPropertiesViewModel — fix lambda captures

- [x] 3.1 `ChangeX` — capture `var text = _text`
- [x] 3.2 `ChangeY` — capture `var text = _text`
- [x] 3.3 `ChangeFontSize` — capture `var text = _text`
- [x] 3.4 `ChangeTextType` — capture `var text = _text`
- [x] 3.5 `ChangeRotation` — capture `var text = _text`
- [x] 3.6 `ChangeKey` — capture `var text = _text`
- [x] 3.7 `ChangeIsEditable` — capture `var text = _text`
- [x] 3.8 `ChangeForeground` — capture `var text = _text`
- [x] 3.9 `ChangeTextWrapping` — capture `var text = _text`
- [x] 3.10 `ChangeTextAlignment` — capture `var text = _text`
- [x] 3.11 `ChangeContent` (inline) — capture `var text = _text`
- [x] 3.12 `ChangeDefaultValue` (inline) — capture `var text = _text`
- [x] 3.13 `ChangeFontNameFromString` (inline) — capture `var text = _text`

## 4. Tests

- [x] 4.1 Add test: `LineProperties_Undo_AfterDeselect_DoesNotThrow`
- [x] 4.2 Add test: `RectangleProperties_Undo_AfterDeselect_DoesNotThrow`
- [x] 4.3 Add test: `TextProperties_Undo_AfterDeselect_DoesNotThrow`

## 5. Validation

- [x] 5.1 Run `dotnet build` — 0 errors, 0 warnings
- [x] 5.2 Run `dotnet test` — all tests pass, no regression
