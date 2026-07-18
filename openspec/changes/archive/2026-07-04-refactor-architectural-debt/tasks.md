## 1. Command cleanup — ChangePropertyCommand<T> enhancement

- [x] 1.1 Add explicit-old-value constructor to `ChangePropertyCommand<T>`:
      `ChangePropertyCommand(T oldValue, Action<T> setter, T newValue, string propertyName, Action? markDirty)`

## 2. Command cleanup — Eliminate specialized commands

- [x] 2.1 Replace `RotateObjectCommand` → `ChangePropertyCommand<int>`:
      Delete `RotateObjectCommand.cs`; update `EditorViewModel.RotateSelected` / `RotateSelectedCounterClockwise`
- [x] 2.2 Replace `CustomResizeCommand` → `ChangePropertyCommand<ResizeState>`:
      Delete `CustomResizeCommand.cs`; update `ResizeTool.cs` (all factory method call sites)
- [x] 2.3 Replace `MoveObjectCommand` → `ChangePropertyCommand<(long,long)>`:
      Delete `MoveObjectCommand.cs`; update `SelectTool.cs` (constructor A and B patterns) and `EditorViewModel.MoveSelected`
- [x] 2.4 Merge `PasteObjectCommand` → `AddObjectCommand(nameOverride)`:
      Add nameOverride parameter to `AddObjectCommand`; delete `PasteObjectCommand.cs`; update `EditorViewModel.PasteFromClipboard`

## 3. Converters cleanup — Eliminate duplicates and merge

- [ ] 3.1 Remove `InverseBoolConverter.cs`: delete file; remove registration from `App.xaml`; replace all `InverseBoolConverter` XAML references with `NotConverter`
- [ ] 3.2 Merge 4 LocalX/Y converters → `LineLocalConverter`:
      Create `LineLocalConverter` accepting parameter "X1"/"X2"/"Y1"/"Y2"; delete 4 old files; update XAML references
- [ ] 3.3 Merge 4 EdgeMicrons converters → 2 files:
      Create consolidated `LeftEdgeMicronsConverter` (single + multi); `TopEdgeMicronsConverter` (single + multi); delete old files; update XAML refs
- [ ] 3.4 Merge 3 EnumToIndex converters → generic `EnumToIndexConverter<T>`:
      Create generic; delete `LineTypeToIndexConverter`, `TextTypeToIndexConverter`, `RotationToIndexConverter`; update XAML refs

## 4. PropertiesViewModel — per-type split

- [ ] 4.1 Create `LinePropertiesViewModel`:
      Extract Line-specific properties (7) + commands (7) + string-wrappers (7) + INPC forwarding into separate `ObservableObject`
- [ ] 4.2 Create `RectanglePropertiesViewModel`:
      Extract Rectangle-specific properties (8) + commands (8) + string-wrappers (8) + INPC forwarding
- [ ] 4.3 Create `TextPropertiesViewModel`:
      Extract Text-specific properties (12) + commands (12) + string-wrappers (3) + INPC forwarding; fix 3 commands that bypass SetProperty<T> (ChangeTextContent, ChangeTextDefaultValue, ChangeTextFontNameFromString)
- [ ] 4.4 Condense `PropertiesViewModel` to base (~150 lines):
      Keep: selection subscription, INPC dispatch to sub-VMs, SetProperty<T>, ChangeFromMmString, ParseLineType, IDisposable, ObjectId/ObjectTypeName
- [ ] 4.5 Rewrite `PropertiesPanelContent.xaml`:
      Replace 3 StackPanel sections with `ContentControl` + implicit `DataTemplate` per sub-VM type; verify all binginds survive
- [ ] 4.6 Update `PropertiesViewModel` tests:
      Adapt existing tests (~50) to the new composition model; add dedicated tests for each sub-VM

## 5. Infrastructure fixes

- [ ] 5.1 Move `IAutosaveTab` interface definition:
      Move from `Services/AutosaveService.cs` to `ViewModels/Abstractions/IAutosaveTab.cs`; update all usings
- [ ] 5.2 Fix `async void` in `MainViewModel.OnAutosaveTickHandler`:
      Wrap body in try/catch with `ILogger`; check if `SafeFireAndForget` pattern can be used
- [ ] 5.3 Verify `CustomResizeCommand` factory methods fully eliminated:
      Confirm no remaining references after Task 2.2; remove dead factory code

## 6. Test coverage — WPF services (STA)

- [ ] 6.1 Write STA tests for `WpfDialogFileService`:
      Mock `IDialogFileService` (verify filter/fileName behavior); use WpfContext
- [ ] 6.2 Write STA tests for `WpfDialogHostService`:
      Verify window creation by ViewModel type (SettingsViewModel → SettingsView, etc.)
- [ ] 6.3 Write STA tests for `WpfMessageBoxProvider`:
      Test all MsgrButtons/MsgrIcon → MessageBoxButton/MessageBoxImage mappings
- [ ] 6.4 Write STA tests for `WpfDispatcherService`:
      Verify Invoke dispatches to STA thread
- [ ] 6.5 Write tests for `PrintDialogFactory`:
      Verify `Create()` returns `PrintDialogWrapper`
- [ ] 6.6 Write STA tests for `ThemeDictionaryManager`:
      Verify theme dictionary swap on `Application.Current.Resources.MergedDictionaries`

## 7. Test coverage — Raise files to 80%+

- [ ] 7.1 Add tests for `PrintService` (64.7% → 80%+)
- [ ] 7.2 Add tests for `AutosaveService` (77.1% → 80%+)
- [ ] 7.3 Add tests for `SettingsService` (78.2% → 80%+)
- [ ] 7.4 Add tests for `CustomResizeCommand` (71.2% → 80%+) — after replacement, test `ChangePropertyCommand<ResizeState>`
- [ ] 7.5 Add tests for `TextTool` (72.6% → 80%+)
- [ ] 7.6 Add tests for `MainViewModel` (72.6% → 80%+)
- [ ] 7.7 Add tests for converters (63-75% → 80%+)

## 8. Validation and cleanup

- [ ] 8.1 Run full build: `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [ ] 8.2 Run all tests: `dotnet test src/DotElectric.TemplateEditor.Tests` — 1780+ passed
- [ ] 8.3 Run coverage: `dotnet test --collect:"XPlat Code Coverage"` — verify ~87% line-rate
- [ ] 8.4 Remove dead App.xaml registrations for deleted converters
- [ ] 8.5 Verify no lingering references to deleted files (find . -name "*.cs" | xargs grep -l "InverseBoolConverter\|RotateObjectCommand\|MoveObjectCommand\|CustomResizeCommand\|PasteObjectCommand")
