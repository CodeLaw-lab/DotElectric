## 1. Refactor Constructor

- [x] 1.1 Remove `VisualTreeHelper.GetParent()` call from constructor
- [x] 1.2 Remove `CreateMarkers()` call from constructor
- [x] 1.3 Remove `_overlayCanvas` field assignment from constructor; make it nullable (`Canvas?`)
- [x] 1.4 Keep: `_textBlock` store, event subscriptions (Loaded/Unloaded/SizeChanged/DataContextChanged), `SubscribeToDataContext()`
- [x] 1.5 Keep `ScheduleUpdate()` call for eager-but-safe first paint after Loaded

## 2. Move Marker Creation to `OnLoaded`

- [x] 2.1 Add `_markersCreated` bool field
- [x] 2.2 In `OnLoaded`: if `_markersCreated` → return; else find sibling Canvas via `VisualTreeHelper.GetParent()` + `Children.OfType<Canvas>()`
- [x] 2.3 If sibling Canvas not found → silently return (no crash, no markers)
- [x] 2.4 Store `_overlayCanvas`, call `CreateMarkers()`, set `_markersCreated = true`
- [x] 2.5 Call `ScheduleUpdate()` for initial marker positioning

## 3. Update `Dispose()`

- [x] 3.1 Null-check `_overlayCanvas` before `Children.Remove()` (it may be null if Loaded never fired)
- [x] 3.2 Reset `_markersCreated = false` so markers can be recreated on reload

## 4. Update Tests

- [x] 4.1 Verify all 10 existing tests still pass (they already use `PumpDispatcher()` after `RaiseEvent(LoadedEvent)`)
- [x] 4.2 Run full test suite: 0 failures (1834 passed, 1 pre-existing skip)

## 5. Build & Verify

- [x] 5.1 Build solution — 0 errors, 0 warnings
- [x] 5.2 Launch app — no crash on startup
- [ ] 5.3 Insert text object via TextTool — no crash, markers appear
- [ ] 5.4 Paste text objects — no crash
- [ ] 5.5 Open file with existing text objects — no crash
