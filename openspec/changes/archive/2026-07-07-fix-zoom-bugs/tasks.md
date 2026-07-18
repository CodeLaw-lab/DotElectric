## 1. Bug 1 — Status bar zoom percent notification

- [x] 1.1 Add `OnPropertyChanged(nameof(ZoomPercent))` in `ZoomPanManager.OnZoomChanged()` at `ZoomPanManager.cs:110`
- [x] 1.2 Verify by checking `ZoomPanManagerTests` property-change assertions — add test that subscribes to `PropertyChanged` and verifies `ZoomPercent` is notified when `Zoom` changes
- [x] 1.3 Run all existing ZoomPanManagerTests and EditorViewModelTests — confirm no regressions

## 2. Bug 2 — Full-sheet grid nodes

- [x] 2.1 In `GridManager.RefreshGridNodes()`, remove the `if/else` branch (lines 92-115) — keep only the full-sheet branch: `startXMicrons = 0; startYMicrons = 0; widthMicrons = sheet.WidthMicrons; heightMicrons = sheet.HeightMicrons;`
- [x] 2.2 Remove now-unused field `_zoomPanManager` from GridManager if it's only used for the viewport branch (check — still needed for `_zoomPanManager.Zoom` in MinPixelSpacing check on line 85 and pixel calculations on lines 133-134)
- [x] 2.3 Update `GridManagerTests` — remove or adapt tests that verified viewport-mode grid bounds. Add test that grid nodes cover the full sheet at `IsCentered=false`
- [x] 2.4 Run GridManagerTests and ensure coverage stays above 75%

## 3. Bug 3 — ComboBox binding mode OneWay

- [x] 3.1 In `MainWindow.xaml:436`, change `Text="{Binding SelectedTab.ZoomPanManager.ZoomPercent, StringFormat={}{0}%}"` to add `Mode=OneWay`
- [x] 3.2 Verify `ZoomComboBoxBehavior` continues to work — user input via ComboBox still calls `SetZoomPercent`. No binding error in debug output
- [x] 3.3 Run `ZoomComboBoxBehaviorTests` — confirm all 11 tests pass

## 4. Integration verification

- [x] 4.1 Run the full test suite: `dotnet test src/DotElectric.TemplateEditor.Tests`
- [x] 4.2 Build with `dotnet build` — confirm 0 errors, 0 warnings
- [x] 4.3 Run with coverage: `dotnet test --collect:"XPlat Code Coverage"` — verify coverage gate 75% passes
- [ ] 4.4 Manual smoke test: launch app, zoom via mouse wheel, verify status bar updates. Zoom in past scrollbar threshold, verify grid covers full sheet. Open zoom ComboBox, select 150%, verify no error message.
