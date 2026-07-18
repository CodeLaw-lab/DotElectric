## 1. Behavior Implementation

- [x] 1.1 Create `Behaviors/TextSelectionMarkerBehavior.cs` — attached behavior class with `IsEnabled` attached property
- [x] 1.2 Implement `OnLoaded`: measure text via `FormattedText`, compute 4 rotated corners, create 4 Rectangle markers in the overlay Canvas
- [x] 1.3 Implement `OnUnloaded`: clean up markers, unsubscribe events
- [x] 1.4 Implement `FormattedText` dimension measurement (Content, FontSize, FontFamily, FontStretch, FontStyle, FontWeight from DataContext)
- [x] 1.5 Implement CW rotation corner computation in WPF Y↓ space (TL, TR, BL, BR)
- [x] 1.6 Implement marker positioning: set `Canvas.Left`/`Canvas.Top` on each Rectangle, offset by half marker size for centering
- [x] 1.7 Subscribe to `SizeChanged` on TextBlock and `PropertyChanged` on DataContext (RotationAngle, Content, FontSize, FontName, TextWrapping) for reactive updates
- [x] 1.8 Guard against null/empty content — skip measurement, hide markers
- [x] 1.9 Implement `Dispatcher.BeginInvoke` coalescing for performance
- [x] 1.10 Handle InlineTextEditor interaction — InlineTextEditor replaces the DataTemplate, so markers are naturally hidden during editing

## 2. XAML Changes

- [x] 2.1 Wrap TextBlock and overlay Canvas in a Grid inside the Text DataTemplate (`EditorCanvas.xaml:193-237`)
- [x] 2.2 Attach `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` on the TextBlock
- [x] 2.3 Remove Background (#E0F0FF) setter from selection DataTrigger (keep Foreground + Bold)
- [x] 2.4 Remove Text DataTemplate from marker ItemsControl (`EditorCanvas.xaml:521-536`)
- [x] 2.5 Verify the `behaviors` XML namespace is imported in the XAML (add if missing)

## 3. Tests

- [x] 3.1 Create `Tests/Behaviors/TextSelectionMarkerBehaviorTests.cs` — 10 STA-thread tests
- [x] 3.2 Test: markers created after TextBlock Loaded
- [x] 3.3 Test: markers reposition on Content change
- [x] 3.4 Test: markers reposition on RotationAngle change
- [x] 3.5 Test: markers reposition on FontSizeMicrons change (via DataContext PropertyChanged)
- [x] 3.6 Test: markers hidden on empty content
- [x] 3.7 Test: markers cleaned up on disable
- [x] 3.8 Test: markers are Rectangle instances in overlay Canvas
- [x] 3.9 Test: DP get/set for IsEnabled
- [x] 3.10 Test: overlay Canvas created and IsHitTestVisible=False

## 4. Build & Verify

- [x] 4.1 Build solution — 0 errors, 0 warnings (verified at 14:22)
- [x] 4.2 Run all tests — all pass (verified at 14:22)
- [ ] 4.3 Manual visual check: select text at 0° — markers match visual bounds
- [ ] 4.4 Manual visual check: select text at 90° — markers at correct corners
- [ ] 4.5 Manual visual check: select text at 45° — markers axis-aligned at corners
- [ ] 4.6 Manual visual check: edit text inline — markers update after commit
- [ ] 4.7 Manual visual check: no blue Background on selected text
