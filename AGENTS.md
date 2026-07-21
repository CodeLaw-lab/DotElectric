# AGENTS.md вЂ” DotElectric

## Current Focus

**РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№ СЂРµС„Р°РєС‚РѕСЂРёРЅРі P2 Р·Р°РІРµСЂС€С‘РЅ.** РЎРѕР·РґР°РЅ `ITabOperationsService` — С„Р°СЃР°Рґ РґР»СЏ РѕРїРµСЂР°С†РёР№ СЃ РІРєР»Р°РґРєР°РјРё (NewTab, OpenFile, Save, SaveAs), СЃРѕРєСЂР°С‚РёРІС€РёР№ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ `MainViewModel` СЃ 13 РґРѕ 10 Р·Р°РІРёСЃРёРјРѕСЃС‚РµР№. РРЅС‚РµСЂС„РµР№СЃ СЂР°Р·РјРµС‰С‘РЅ РІ `ViewModels.Abstractions` РІРѕ РёР·Р±РµР¶Р°РЅРёРµ С†РёРєР»РёС‡РµСЃРєРёС… Р·Р°РІРёСЃРёРјРѕСЃС‚РµР№. РџРµСЂРµРёРјРµРЅРѕРІР°РЅС‹ 14 С‚РµСЃС‚РѕРІ РєРѕРјР°РЅРґ РґР»СЏ РµРґРёРЅРѕРѕР±СЂР°Р·РёСЏ (`MoveObjectCommand_*` в†’ `ChangePropertyCommand_Move_*`).

### РљР»СЋС‡РµРІС‹Рµ СЂРµР·СѓР»СЊС‚Р°С‚С‹
| РћР±Р»Р°СЃС‚СЊ | Р‘С‹Р»Рѕ | РЎС‚Р°Р»Рѕ |
|---------|------|-------|
| РРµСЂР°СЂС…РёСЏ РјРѕРґРµР»РµР№ | 3 СѓСЂРѕРІРЅСЏ (ObjectBaseв†’ModelBaseв†’TemplateObjectBase) | 1 СѓСЂРѕРІРµРЅСЊ (TemplateObjectBaseв†’ObservableObject) |
| INPC-СЃРµС‚С‚РµСЂС‹ | ~50 СЂСѓС‡РЅС‹С… | [ObservableProperty] sourcegen |
| EditorCanvasBehavior | 406 СЃС‚СЂРѕРє (РјРѕРЅРѕР»РёС‚) | 78 СЃС‚СЂРѕРє (3 С„Р°Р№Р»Р°: State, Transform, Router) |
| Tools-EditorVM | РџСЂСЏРјР°СЏ Р·Р°РІРёСЃРёРјРѕСЃС‚СЊ | IEditorContext |
| DI-РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ | internal + СЂСѓС‡РЅР°СЏ С„Р°Р±СЂРёРєР° | public + Transient + ActivatorUtilities |
| PrintVisualProvider | Func<Visual?> | IPrintVisualProvider |
| Validation | static ValidationService (537 СЃС‚СЂРѕРє) | ITemplateValidator |
| Resize | 520 СЃС‚СЂРѕРє (switch) | ResizeMath + РїРѕР»РёРјРѕСЂС„РЅС‹Р№ ApplyResize |
| Shortcuts | switch РІ code-behind | ShortcutRegistry |
| Extended С‚РµСЃС‚С‹ | 16 С„Р°Р№Р»РѕРІ | РІСЃРµ СЃР»РёС‚С‹ РІ СЂРѕРґРёС‚РµР»СЊСЃРєРёРµ |
| CI | РЅРµС‚ coverage-gate, РЅРµС‚ NuGet РєСЌС€Р° | coverage-gate 75% + actions/cache |
| CPM | РЅРµС‚ | Directory.Packages.props |
| **Print Preview** | **РЅРµС‚** | **Ctrl+Shift+P в†’ DocumentViewer** |
| **EditorConstants** | **36-line proxy** | **СѓРґР°Р»С‘РЅ в†’ PhysicalConstants/EditorSettings** |
| **FontMetrics** | **static class** | **IFontMetrics + DI Singleton** |
| **Sealed classes** | **0** | **66 РєР»Р°СЃСЃРѕРІ (Converters, Services, Tools, Managers, Commands)** |
| **Shortcut dispatch** | **code-behind 30 СЃС‚СЂРѕРє** | **ShortcutRegistry.TryHandle()** |
| **ITool.OnMouseWheel** | **void** | **bool (tool РјРѕР¶РµС‚ Р±Р»РѕРєРёСЂРѕРІР°С‚СЊ zoom)** |

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed, 1 pre-existing skip

### H1вЂ“H5 вЂ” РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Рµ РёСЃРїСЂР°РІР»РµРЅРёСЏ РІС‹СЃРѕРєРѕР№ РІР°Р¶РЅРѕСЃС‚Рё (14.07.2026)
- **H1: async-void AutosaveTick** вЂ” `event Action?` в†’ `event Func<Task>?`. `IDispatcherService` РїРѕР»СѓС‡РёР» `InvokeAsync(Func<Task>)`. `AutosaveService.OnAutosaveTick` РІС‹Р·С‹РІР°РµС‚ `InvokeAsync`. `MainViewModel` вЂ” `async Task` РІРјРµСЃС‚Рѕ `async void`.
- **H2: ValidationService в†’ injectable** вЂ” РЎРѕР·РґР°РЅ `IValidationService` (РёРЅС‚РµСЂС„РµР№СЃ СЃ `ValidateHexColor`). `ValidationService` СЃРѕРґРµСЂР¶РёС‚ СЃС‚Р°С‚РёС‡РµСЃРєРёР№ `Default` (instance-РѕР±С‘СЂС‚РєР°). `TemplateValidator` РїСЂРёРЅРёРјР°РµС‚ `IValidationService` С‡РµСЂРµР· DI (РѕРїС†РёРѕРЅР°Р»СЊРЅС‹Р№ РїР°СЂР°РјРµС‚СЂ РґР»СЏ РѕР±СЂР°С‚РЅРѕР№ СЃРѕРІРјРµСЃС‚РёРјРѕСЃС‚Рё). РЎС‚Р°С‚РёС‡РµСЃРєРёРµ РјРµС‚РѕРґС‹ `ValidationService` СЃРѕС…СЂР°РЅРµРЅС‹.
- **H3: DialogServiceFactory СѓРґР°Р»С‘РЅ** вЂ” РјС‘СЂС‚РІС‹Р№ РєРѕРґ (public static class, РЅРµ РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ) СѓРґР°Р»С‘РЅ РёР· `IDialogService.cs`.
- **H4: PrintVisualProvider null-out** вЂ” `PrintVisualProvider = null` РґРѕР±Р°РІР»РµРЅ РІ `EditorViewModel.Dispose()`.
- **H5: No-op F4 MenuItem СѓРґР°Р»С‘РЅ** вЂ” `<MenuItem Header="РЎРІРѕР№СЃС‚РІР°" InputGestureText="F4">` СѓРґР°Р»С‘РЅ РёР· РєРѕРЅС‚РµРєСЃС‚РЅРѕРіРѕ РјРµРЅСЋ `EditorCanvas.xaml`.

### Р§С‚Рѕ СЃРґРµР»Р°РЅРѕ РїРѕСЃР»Рµ R1вЂ“R4
- **EditorViewModel РґРµ-bloat** вЂ” ~1194 в†’ 784 СЃС‚СЂРѕРє (в€’410, в€’34%). РЈРґР°Р»РµРЅС‹ ~25 forwarding-СЃРІРѕР№СЃС‚РІ, 4 PropertyChanged-РѕР±СЂР°Р±РѕС‚С‡РёРєР°, 4 РїРѕРґРїРёСЃРєРё/РѕС‚РїРёСЃРєРё. РЎРІРѕР№СЃС‚РІР° IEditorContext РѕСЃС‚Р°РІР»РµРЅС‹ РєР°Рє bare delegation (Р±РµР· OnPropertyChanged). IAutosaveTab вЂ” explicit interface implementation.
- **Preview fix** вЂ” `[ObservableProperty]` РЅР° `PreviewLine`/`PreviewRectangle`/`PreviewText` РїРѕРґР°РІР»СЏР» PropertyChanged РїСЂРё re-assign С‚РѕР№ Р¶Рµ СЃСЃС‹Р»РєРё. Р—Р°РјРµРЅС‘РЅ РЅР° СЂСѓС‡РЅС‹Рµ СЃРµС‚С‚РµСЂС‹ СЃ Р±РµР·СѓСЃР»РѕРІРЅС‹Рј `OnPropertyChanged()`.
- **Selection markers fix** вЂ” `ShowSelectionMarkers` (computed property) РЅРµ РІС‹Р·С‹РІР°Р» `OnPropertyChanged()` РїСЂРё РёР·РјРµРЅРµРЅРёРё `SelectedObjects`. Р”РѕР±Р°РІР»РµРЅ РІС‹Р·РѕРІ РІ `CollectionChanged`-РѕР±СЂР°Р±РѕС‚С‡РёРє.
- **PropertiesViewModel split** вЂ” 649 в†’ 85 СЃС‚СЂРѕРє (Р±Р°Р·Р°). 3 sub-VM: LinePropertiesViewModel (148), RectanglePropertiesViewModel (168), TextPropertiesViewModel (233). XAML: 3 StackPanel в†’ ContentControl + DataTemplate per sub-VM.
- **Print Preview** вЂ” Ctrl+Shift+P РѕС‚РєСЂС‹РІР°РµС‚ DocumentViewer СЃ FixedDocument. IPrintDocumentGenerator, PrintDocumentGenerator, PrintPreviewWindow. 19 С‚РµСЃС‚РѕРІ.
- **Text rotation fix (Sprint 59)** вЂ” ContainsPoint() РёСЃРїСЂР°РІР»РµРЅ РЅР° inverse WPF RotateTransform (standard CCW matrix). RotatedCorner0-3, GetBoundingBox вЂ” reverted Рє РѕСЂРёРіРёРЅР°Р»СЊРЅС‹Рј (РєРѕСЂСЂРµРєС‚РЅС‹Рј) С„РѕСЂРјСѓР»Р°Рј. HitTestHelper/HitTestText РґР»СЏ 90В°/270В°/45В° вЂ” РІСЃРµ РїСЂРѕС…РѕРґСЏС‚. РћСЃРѕР·РЅР°РЅР° Рё Р·Р°С„РёРєСЃРёСЂРѕРІР°РЅР° РјР°С‚СЂРёС†Р° WPF `x'=x*cosОёв€’y*sinОё`. РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№ РёРЅСЃР°Р№С‚: ContainsPoint() Р±С‹Р» Р±Р°РіРЅСѓС‚ (forward РІРјРµСЃС‚Рѕ inverse) РЅРµР·Р°РІРёСЃРёРјРѕ РѕС‚ РїСѓС‚Р°РЅРёС†С‹ СЃРѕ Р·РЅР°РєР°РјРё.

### Р§С‚Рѕ РЅРµ РІРѕС€Р»Рѕ / РѕС‚Р»РѕР¶РµРЅРѕ
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior вЂ” STA-С‚РµСЃС‚С‹ (С‚СЂРµР±СѓСЋС‚ РїРѕР»РЅРѕРіРѕ РІРёР·СѓР°Р»СЊРЅРѕРіРѕ РґРµСЂРµРІР°)~~ вЂ” Р РµС€РµРЅРѕ РІ Sprint 62
- **Text markers вЂ” tech debt:** РёСЃРїСЂР°РІР»РµРЅ РїРѕРІРѕСЂРѕС‚ (`RotatedCorner0вЂ“3`, `GetBoundingBox`, `HitTestHelper`), РЅРѕ РѕСЃС‚Р°СЋС‚СЃСЏ РЅРµРґРѕС‡С‘С‚С‹ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ РјР°СЂРєРµСЂРѕРІ: `TextSelectionMarkerBehavior` РЅРµ РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ, РїСѓСЃС‚РѕР№ `<Canvas/>` РІРЅСѓС‚СЂРё DataTemplate Text, РјР°СЂРєРµСЂС‹ РІ РѕС‚РґРµР»СЊРЅРѕРј ItemsControl РІРјРµСЃС‚Рѕ РІРЅСѓС‚СЂРё DataTemplate
- **Inline text editing вЂ” tech debt (РІСЃСЏ СЂР°Р±РѕС‚Р° СЃ С‚РµРєСЃС‚РѕРј):**
  - Escape РЅРµ РѕС‚РјРµРЅСЏР» СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёРµ вЂ” **РёСЃРїСЂР°РІР»РµРЅРѕ** (focus guard РІ CanvasInputRouter). РћСЃС‚Р°С‘С‚СЃСЏ:
    - TextBox РЅРµ РїРѕР»СѓС‡Р°РµС‚ Р°РІС‚Рѕ-С„РѕРєСѓСЃ РїРѕСЃР»Рµ double-click вЂ” РµСЃР»Рё РЅРµ РєР»РёРєРЅСѓС‚СЊ РІ TextBox, Escape СѓС…РѕРґРёС‚ РІ SelectTool (РѕС‡РёС‰Р°РµС‚ РІС‹РґРµР»РµРЅРёРµ, СЂРµРґР°РєС‚РѕСЂ РѕСЃС‚Р°С‘С‚СЃСЏ)
    - Enter/Ctrl+Enter/Escape routing relies on fragile WPF event ordering (PreviewKeyDown vs KeyDown) вЂ” РїСЂРё РёР·РјРµРЅРµРЅРёРё CanvasInputRouter РёР»Рё РїРѕСЏРІР»РµРЅРёРё РЅРѕРІС‹С… child control'РѕРІ РјРѕР¶РµС‚ СЃР»РѕРјР°С‚СЊСЃСЏ
    - Р СѓС‡РЅР°СЏ РІРµСЂРёС„РёРєР°С†РёСЏ Escape РїСЂРё СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёРё РЅРµ РїСЂРѕРІРµРґРµРЅР° (С‚Р°СЃРєРё 2.2, 2.3 РІ fix-escape-inline-editing)

### Next Steps
- Р­С‚Р°Рї 2 вЂ” Р РµРґР°РєС‚РѕСЂ РЈР“Рћ (РїР»Р°РЅРёСЂРѕРІР°РЅРёРµ)
- FR-021 Drag&Drop РёР· Р±РёР±Р»РёРѕС‚РµРєРё
- FR-022 Preview С€Р°Р±Р»РѕРЅРѕРІ
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior вЂ” integration/UI С‚РµСЃС‚С‹ СЃ STA~~ вЂ” Р РµС€РµРЅРѕ РІ Sprint 62

## Build Commands

```bash
# Build solution
dotnet build src/DotElectric.TemplateEditor.slnx

# Run application
dotnet run --project src/DotElectric.TemplateEditor

# Run all tests
dotnet test src/DotElectric.TemplateEditor.Tests

# Run single test
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~YourTestName"

# Run tests with coverage
dotnet test src/DotElectric.TemplateEditor.Tests --collect:"XPlat Code Coverage"
```

## Project Structure

- **Main app:** `src/DotElectric.TemplateEditor/` вЂ” WPF .NET 10 CAD application
- **Tests:** `src/DotElectric.TemplateEditor.Tests/` вЂ” xUnit v3 tests
- **Solution:** `src/DotElectric.TemplateEditor.slnx` (XML format, not `.sln`)
- **Shared props:** `src/Directory.Build.props` (net10.0-windows, nullable, implicit usings)

## Architecture Must-Know

### Fixed-Point Coordinates
- All internal coordinates in **microns** (`long`, not double)
- 1mm = 1000 microns
- XML serialization also uses microns (`xs:long`)
- Round-trip without precision loss

### Coordinate System
- **Model:** Cartesian (0,0 = bottom-left, Yв†‘)
- **WPF:** Inverted Y (Yв†“)
- Conversion only in `EditorCanvas` via `FromWpfPoint()` / `ToWpfPoint()`
- ViewModels/Services know NOTHING about WPF coordinates

### Key Patterns
- **MVVM** with CommunityToolkit.Mvvm
- **DI** via Microsoft.Extensions.DependencyInjection (all services Singleton, EditorViewModelFactory as IEditorViewModelFactory)
- **Undo/Redo:** 50 levels via `CommandHistory` вЂ” commands implement custom `ICommand` interface (NOT `System.Windows.Input.ICommand`)
- **Tools:** State pattern via `ITool` interface
- **Messaging:** WeakReferenceMessenger for cross-VM communication (e.g., tab close)
- **IEditorContext** вЂ” Sprint R3: РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹ РїРѕР»СѓС‡Р°СЋС‚ РєРѕРЅС‚РµРєСЃС‚ С‡РµСЂРµР· РёРЅС‚РµСЂС„РµР№СЃ, Р° РЅРµ EditorViewModel РЅР°РїСЂСЏРјСѓСЋ
- **ResizeMath** вЂ” Sprint R4: С‡РёСЃС‚С‹Рµ СЃС‚Р°С‚РёС‡РµСЃРєРёРµ С„СѓРЅРєС†РёРё РґР»СЏ resize-РіРµРѕРјРµС‚СЂРёРё
- **ShortcutRegistry** вЂ” Sprint R4: С†РµРЅС‚СЂР°Р»РёР·РѕРІР°РЅРЅС‹Р№ РјР°РїРїРёРЅРі V/L/R/T/ E/E+Shift

### File Format
- **.tdel:** XML packed in ZIP (custom template format)

### Fonts
- GOST A/B fonts required: `Resources/Fonts/*.ttf` (embedded as resources)
- Font files: GostA.ttf, GostB.ttf
- **Р’РЅСѓС‚СЂРµРЅРЅРёРµ РёРјРµРЅР° С€СЂРёС„С‚РѕРІ (С‡СѓРІСЃС‚РІРёС‚РµР»СЊРЅС‹ Рє СЂРµРіРёСЃС‚СЂСѓ):**
  - `GostA.ttf` в†’ `#GOST Type AU`
  - `GostB.ttf` в†’ `#GOST Type BU`
- URI: `pack://application:,,,/Resources/Fonts/#GOST Type AU`
- FontNameToFamilyConverter РјР°РїРїРёС‚ "Р“РћРЎРў Рђ" / "Р“РћРЎРў Р‘" РЅР° РїСЂР°РІРёР»СЊРЅС‹Рµ URI

## Framework Versions

| Package | Version |
|---------|---------|
| .NET | 10.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| MaterialDesignThemes | 5.3.1 |
| Microsoft.Extensions.DependencyInjection | 10.0.5 |
| Serilog | 4.3.1 |
| xunit.v3 | 3.2.2 |
| Moq | 4.20.72 |

## Reference Documentation

- `docs/03_РЎРїРµС†РёС„РёРєР°С†РёСЏ_С‚СЂРµР±РѕРІР°РЅРёР№_Р­С‚Р°Рї1.md` вЂ” Detailed architecture and API
- `docs/00_РРЅРґРµРєСЃ_РґРѕРєСѓРјРµРЅС‚РѕРІ.md` вЂ” Document index

РђРєС‚СѓР°Р»СЊРЅС‹Рµ РѕРїРёСЃР°РЅРёСЏ РІСЃРµС… РёР·РјРµРЅРµРЅРёР№, Common Mistakes Рё Р°СЂС…РёС‚РµРєС‚СѓСЂРЅС‹С… СЂРµС€РµРЅРёР№ вЂ” РІ СЌС‚РѕРј РґРѕРєСѓРјРµРЅС‚Рµ (AGENTS.md).
РђСЂС…РёРІРЅС‹Рµ sprint-РѕС‚С‡С‘С‚С‹ Рё fix-РґРѕРєСѓРјРµРЅС‚С‹ СѓРґР°Р»РµРЅС‹ РёР· git РґР»СЏ РѕРїС‚РёРјРёР·Р°С†РёРё СЂРµРїРѕР·РёС‚РѕСЂРёСЏ.

## Common Mistakes to Avoid

1. Don't use double for coordinates вЂ” use microns (`long`)
2. Don't create new Shape on every MouseMove вЂ” update properties instead
3. Don't do hit-testing on MouseMove вЂ” only on MouseDown
4. Don't use Grid/StackPanel in EditorCanvas вЂ” use Canvas (layout pass issues)
5. Always use `Mode=OneWay` when binding to readonly properties
6. IsDirty must be set by commands (`MarkDirty()`), NOT manually
7. Preview shapes: create once, update properties only
8. EditorViewModel вЂ” instantiate via `IEditorViewModelFactory`, NOT `new` directly (ensures DI-managed dependencies)
9. CenterCanvas вЂ” always use `Math.Max(0, (canvasPx - viewportPx) / 2)` for each axis independently; portrait sheets may fit width but not height
10. ModelYToCanvasTopConverter binding вЂ” pass `Template.Sheet.HeightMm` (double), NOT `HeightMicrons` (long), or converter returns 0.0
11. ToModelPoint вЂ” `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset). Do NOT subtract PanOffset вЂ” it double-compensates and breaks hit-test
12. Selection visual вЂ” use `SelectionVersion` (int) + `IsObjectSelectedConverter` to trigger DataBinding re-evaluation; model objects don't implement INotifyPropertyChanged for selection state
13. Preview shapes вЂ” create once in OnMouseDown, update properties in OnMouseMove, then re-assign reference to trigger ViewModel setter (unconditional OnPropertyChanged)
14. Model INPC (Item 12 correction) вЂ” model objects DO implement INotifyPropertyChanged for **persistent properties** (LineType, coordinates, dimensions). This is necessary for canvas DataTemplate bindings (StrokeDashArray, Width/Height, Canvas.Left/Top) to update when properties change via commands. INPC is NOT implemented for transient UI state like selection.
15. ComboBox with hardcoded items вЂ” always add `SelectedIndex` (or `SelectedItem`) binding when using `SelectionChangedCommand` behavior, otherwise the ComboBox never reflects the current model value
16. After Undo/Redo вЂ” always purge orphaned objects from `SelectedObjects`; `CommandHistory.Undo()`/`Redo()` removes/re-adds objects from the template collection without updating selection
17. `Rectangle.ContainsPoint()` вЂ” use **border-band** approach (expanded bounds minus shrunk interior), NOT full AABB. Interior area > `LineHitToleranceMicrons` from edges must NOT be selectable. Only clicks near the border count.
18. Tool switching keys (V/L/R/T) вЂ” handled via `PreviewKeyDown` on Window, NOT `Window.InputBindings`, for keyboard layout independence. `e.Key` returns physical key position regardless of RU/EN layout.
19. Selection markers (`ShowSelectionMarkers`) вЂ” returns `SelectedObjects.Count > 0` (not `Count == 1`). Markers render via `ItemsControl ItemsSource="{Binding SelectedObjects}"`, showing handles on ALL selected objects, not just single-selection.
20. Drag delta вЂ” compute from **saved initial position** (`_initialPositions[obj]`), NOT from current `obj.MicronsX`. The current value is already updated on previous MouseMove, so `obj.MicronsX + delta` drifts on every frame. Use `initialPos + delta` where `delta` is total mouse movement from drag start.
21. Every model class participating in canvas DataTemplate bindings (`Canvas.Left`/`Canvas.Top`/`StrokeDashArray`/etc) MUST implement `INotifyPropertyChanged` with backing fields for persistent properties (coordinates, dimensions, LineType). This applies to ALL object types: `Line`, `Rectangle`, AND `Text`.
22. Pan delta вЂ” compute from **Window-relative coordinates** (stable frame), NOT from `e.GetPosition(canvas)`. `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset), so comparing canvas-relative positions across `MouseMove` events where the canvas has moved produces a delta that includes the previous pan offset вЂ” causing runaway acceleration.

## Current State (Sprint R1вЂ“R4 + R3.1 + AвЂ“D + Coverage Improvement + Sprint 60вЂ“63 Р·Р°РІРµСЂС€РµРЅС‹)

- **Tests:** 2094 (0 failures, 1 pre-existing skip)
- **Coverage:** 75.3% line-rate вњ…
- **Build:** 0 errors, 0 warnings
- **CI/CD:** GitHub Actions вЂ” build + test + coverage-gate 75% + NuGet РєСЌС€
- **EditorViewModel:** ~784 СЃС‚СЂРѕРє (РґРµ-bloat: в€’410 СЃС‚СЂРѕРє, 25 forwarding-СЃРІРѕР№СЃС‚РІ СѓРґР°Р»РµРЅРѕ, 4 INPC-РѕР±СЂР°Р±РѕС‚С‡РёРєР° СѓРґР°Р»РµРЅС‹)
- **Managers:** ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar, Grid, DirtyState
- **Tools:** ITool + IEditorContext + ResizeMath (С‡РёСЃС‚С‹Рµ С„СѓРЅРєС†РёРё) + ShortcutRegistry
- **Converters:** 27 С„Р°Р№Р»РѕРІ (РІСЃРµ sealed)
- **Naming:** `TemplateObjectBase` (РЅРµ `ITemplateObject`)
- **Commands:** `IUndoCommand` + `CustomResizeCommand` (РїРѕР»РёРјРѕСЂС„РЅС‹Р№ ApplyResize)
- **Model INPC:** `[ObservableProperty]` sourcegen РЅР° Line, Rectangle, Text
- **Constants:** `PhysicalConstants` + `EditorSettings` (РІРјРµСЃС‚Рѕ `EditorConstants.cs`-РїСЂРѕРєР»Р°РґРєРё)
- **Validation:** `ITemplateValidator`/`TemplateValidator` (domain) + `ValidationService` (UI)
- **EditorCanvasBehavior:** 78 СЃС‚СЂРѕРє (AttachedProperty + stubs), 3 С„Р°Р№Р»Р°: State, Transform, Router
- **FontMetrics:** `IFontMetrics` + `FontMetrics.Default` static Singleton (DI-registered)
- **ShortcutRegistry:** `TryHandle()` вЂ” РµРґРёРЅР°СЏ С‚РѕС‡РєР° РІС…РѕРґР° РґР»СЏ РІСЃРµС… РіРѕСЂСЏС‡РёС… РєР»Р°РІРёС€

## Sprint вЂ” Coverage Improvement (19.07.2026)

### Pipeline: РЈРІРµР»РёС‡РµРЅРёРµ РїРѕРєСЂС‹С‚РёСЏ РґРѕ в‰Ґ75%
**РџСЂРѕР±Р»РµРјР°:** Р¤Р°РєС‚РёС‡РµСЃРєРѕРµ РїРѕРєСЂС‹С‚РёРµ СЃРѕСЃС‚Р°РІР»СЏР»Рѕ ~59-67% (РѕС†РµРЅРєР° 82% Р±С‹Р»Р° РЅРµС‚РѕС‡РЅРѕР№). CI gate С‚СЂРµР±РѕРІР°Р» в‰Ґ75%.
**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅРѕ ~195 С‚РµСЃС‚РѕРІ РІ 6 Р·РѕРЅР°С… + 2 retry-С†РёРєР»Р°. РљР»СЋС‡РµРІС‹Рµ РґРѕР±Р°РІР»РµРЅРёСЏ:
- **Commands:** 16 С‚РµСЃС‚РѕРІ РЅР° null guards + edge cases (AddObjectCommand, DeleteObjectCommand, ChangePropertyCommand, BatchCommand)
- **Tools Reset():** 9 С‚РµСЃС‚РѕРІ РЅР° DrawingLineTool/DrawingRectangleTool/TextTool.Reset()
- **Grid:** 8 С‚РµСЃС‚РѕРІ РЅР° ComputeDisplayStep/GenerateGridNodes edge cases
- **Services:** 8 С‚РµСЃС‚РѕРІ РЅР° TemplateService, AutosaveService, PrintDocumentGenerator, DialogService
- **Models:** 15+ С‚РµСЃС‚РѕРІ РЅР° Template.Clone(), Sheet.FromFormat(), Coordinate, PointMicrons РѕРїРµСЂР°С‚РѕСЂС‹
- **MainViewModel:** 6 С‚РµСЃС‚РѕРІ РЅР° AutosaveTickHandler, PrintPreviewCommand, OpenSettingsCommand
- **Retry 1:** FontMetrics (22 С‚РµСЃС‚Р°, instance+IFontMetrics), TemplateObjectBase (43 С‚РµСЃС‚Р°, Move/Clone/CaptureResizeState/ContainsPoint), NonZeroToVisibilityConverter (15 С‚РµСЃС‚РѕРІ), CustomSheetDialogViewModel (23 С‚РµСЃС‚Р°)
- **Retry 2:** ShortcutRegistry (22 С‚РµСЃС‚Р°, 100% РїРѕРєСЂС‹С‚РёРµ)
- **Production fixes:** Template.Clone() deep copy, PointMicrons operator+/-, РёСЃРїСЂР°РІР»РµРЅ СЃРёРЅС‚Р°РєСЃРёСЃ TemplateTests.cs

**Р¤Р°Р№Р»С‹:** 15+ test files, Models/Template.cs, Models/PointMicrons.cs
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.15% line-rate вњ… (РїРѕСЂРѕРі 75% РґРѕСЃС‚РёРіРЅСѓС‚)

## Sprint 37 вЂ” Selection fixes + visual feedback

### Fix S37-1: ToModelPoint double-compensation

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior.ToModelPoint()` РІС‹С‡РёС‚Р°Р» `PanOffsetX`/`PanOffsetY` РёР· `e.GetPosition(canvas)` вЂ” РЅРѕ `e.GetPosition` СѓР¶Рµ СѓС‡РёС‚С‹РІР°РµС‚ `RenderTransform` (СЃРґРІРёРі РЅР° `CanvasOffset = -PanOffset`). Р”РІРѕР№РЅРѕРµ РІС‹С‡РёС‚Р°РЅРёРµ СЃРјРµС‰Р°Р»Рѕ РјРѕРґРµР»СЊРЅС‹Рµ РєРѕРѕСЂРґРёРЅР°С‚С‹ РЅР° `PanOffset/zoom` РјРј, РёР·-Р·Р° С‡РµРіРѕ HitTest РЅРµ РЅР°С…РѕРґРёР» РѕР±СЉРµРєС‚С‹ РїРѕРґ РєСѓСЂСЃРѕСЂРѕРј.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЈР±СЂР°РЅРѕ РІС‹С‡РёС‚Р°РЅРёРµ PanOffset РёР· `ToModelPoint()`.

### Fix S37-2: Visual selection state

**РџСЂРѕР±Р»РµРјР°:** РЈ РѕР±СЉРµРєС‚РѕРІ РЅРµ Р±С‹Р»Рѕ РІРёР·СѓР°Р»СЊРЅРѕРіРѕ СЃРѕСЃС‚РѕСЏРЅРёСЏ В«РІС‹РґРµР»РµРЅВ». SingleSelectedObject (РјР°СЂРєРµСЂС‹) РїРѕРєР°Р·С‹РІР°Р»СЃСЏ, РЅРѕ РїСЂРё РјСѓР»СЊС‚Рё-РІС‹РґРµР»РµРЅРёРё РёР»Рё РѕРґРёРЅРѕС‡РЅРѕРј РєР»РёРєРµ РІРЅРµС€РЅРёР№ РІРёРґ РЅРµ РјРµРЅСЏР»СЃСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** 
- `SelectionVersion` (int) + `IsObjectSelected(obj)` РІ EditorViewModel
- `IsObjectSelectedConverter` (IMultiValueConverter) вЂ” РїСЂРѕРІРµСЂСЏРµС‚ `SelectedObjects.Contains(obj)`
- DataTrigger'С‹ РІ DataTemplate'Р°С… Line/Rectangle/Text вЂ” СЃРёРЅСЏСЏ РїРѕРґСЃРІРµС‚РєР° `#0078D4`, StrokeThickness=2

### Fix S37-3: Preview shapes not appearing

**РџСЂРѕР±Р»РµРјР°:** РџРѕСЃР»Рµ РјСѓС‚Р°С†РёРё СЃРІРѕР№СЃС‚РІ `_previewLine`/`_previewRect` СЃСЃС‹Р»РєР° РЅРµ РјРµРЅСЏР»Р°СЃСЊ, `EditorViewModel.PreviewLine` setter РЅРµ РІС‹Р·С‹РІР°Р»СЃСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р Рµ-Р°СЃСЃР°Р№РЅ `_editor.PreviewLine = _previewLine` / `_editor.PreviewRectangle = _previewRect` РІ OnMouseMove.

### Fix S37-4: Canvas not resizing on zoom

**РџСЂРѕР±Р»РµРјР°:** `OnPropertyChanged(nameof(Zoom))` РЅРµ РІС‹Р·С‹РІР°Р»СЃСЏ РїСЂРё РёР·РјРµРЅРµРЅРёРё Р·СѓРјР° С‡РµСЂРµР· `SetZoom`/`ZoomIn`/`ZoomOut`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ `OnPropertyChanged(nameof(Zoom))` РІ `OnZoomChangedInternal()`.

### Fix S37-5: Escape doesn't switch to Select

**РџСЂРѕР±Р»РµРјР°:** Escape РІ РёРЅСЃС‚СЂСѓРјРµРЅС‚Р°С… СЂРёСЃРѕРІР°РЅРёСЏ/С‚РµРєСЃС‚Р° РѕС‡РёС‰Р°Р» СЃРѕСЃС‚РѕСЏРЅРёРµ, РЅРѕ РЅРµ Р°РєС‚РёРІРёСЂРѕРІР°Р» SelectTool.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ `_editor.ActiveTool = "Select"` РїРѕСЃР»Рµ `Reset()` РІРѕ РІСЃРµС… С‚СЂС‘С… РёРЅСЃС‚СЂСѓРјРµРЅС‚Р°С….

### Fix S37-6: SelectionBoxTop not rendering

**РџСЂРѕР±Р»РµРјР°:** `SelectionBoxTop` (РІС‹С‡РёСЃР»СЏРµРјРѕРµ = SelectionBoxBottom + SelectionBoxHeight) РЅРµ РїСЂРѕР±СЂР°СЃС‹РІР°Р»СЃСЏ РЅР° EditorViewModel в†’ XAML РЅРµ РѕР±РЅРѕРІР»СЏР»СЃСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РџРѕРґРїРёСЃРєР° РЅР° `PropertyChanged` PreviewManager РІ EditorViewModel.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 465+ РїСЂРѕР№РґРµРЅС‹ (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120)

## Sprint 38 вЂ” LineType РїР°РЅРµР»Рё СЃРІРѕР№СЃС‚РІ + Undo + РєРѕРѕСЂРґРёРЅР°С‚С‹

### Fix S38-1: ComboBox С‚РёРїР° Р»РёРЅРёРё РЅРµ РѕС‚РѕР±СЂР°Р¶Р°РµС‚ С‚РµРєСѓС‰РµРµ Р·РЅР°С‡РµРЅРёРµ

**РџСЂРѕР±Р»РµРјР°:** ComboBox РІ РїР°РЅРµР»Рё СЃРІРѕР№СЃС‚РІ РЅРµ РёРјРµР» `SelectedItem`/`SelectedIndex` Р±РёРЅРґРёРЅРіР° вЂ” РїРѕРєР°Р·С‹РІР°Р» РїСѓСЃС‚РѕРµ Р·РЅР°С‡РµРЅРёРµ РїСЂРё РїРµСЂРІРѕРј РІС‹Р±РѕСЂРµ РѕР±СЉРµРєС‚Р°. РР·РјРµРЅРµРЅРёРµ С‡РµСЂРµР· UI СЂР°Р±РѕС‚Р°Р»Рѕ, РЅРѕ ComboBox РЅРµ СЃРёРЅС…СЂРѕРЅРёР·РёСЂРѕРІР°Р»СЃСЏ СЃ РјРѕРґРµР»СЊСЋ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРѕР·РґР°РЅ `LineTypeToIndexConverter` (LineType в†’ int). Р”РѕР±Р°РІР»РµРЅ `SelectedIndex="{Binding LineTypeValue/RectLineType, Converter=...}"` РЅР° РѕР±Р° ComboBox (Line Рё Rectangle).

### Fix S38-2: РР·РјРµРЅРµРЅРёРµ LineType РЅРµ РїРµСЂРµСЂРёСЃРѕРІС‹РІР°РµС‚ РєР°РЅРІР°СЃ

**РџСЂРѕР±Р»РµРјР°:** `Line` Рё `Rectangle` Р±РµР· INPC вЂ” РјСѓС‚Р°С†РёСЏ `LineType` С‡РµСЂРµР· `ChangePropertyCommand` РЅРµ РѕР±РЅРѕРІР»СЏР»Р° `StrokeDashArray` РЅР° РєР°РЅРІР°СЃРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `Line.cs` Рё `Rectangle.cs` вЂ” `INotifyPropertyChanged` + backing field РґР»СЏ `LineType`.

### Fix S38-3: DrawingRectangleTool РЅРµ РїРµСЂРµРґР°С‘С‚ _lineType

**РџСЂРѕР±Р»РµРјР°:** `CalculateRectangle()` СЃРѕР·РґР°РІР°Р» `new Rectangle(x, y, w, h)` вЂ” РІСЃРµРіРґР° `LineType.Solid`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `CalculateRectangle()` РїСЂРёРЅРёРјР°РµС‚ `lineType` Рё РїРµСЂРµРґР°С‘С‚ РІ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ.

### Fix S38-4: РР·РјРµРЅРµРЅРёРµ РєРѕРѕСЂРґРёРЅР°С‚ РЅРµ РїРµСЂРµСЂРёСЃРѕРІС‹РІР°РµС‚ РєР°РЅРІР°СЃ

**РџСЂРѕР±Р»РµРјР°:** `Line.StartMicronsX/Y`, `EndMicronsX/Y` Рё `Rectangle.WidthMicrons/HeightMicrons/MicronsX/Y` Р±РµР· INPC вЂ” РєР°РЅРІР°СЃ РЅРµ РѕР±РЅРѕРІР»СЏР»СЃСЏ РїСЂРё СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёРё С‡РµСЂРµР· РїР°РЅРµР»СЊ СЃРІРѕР№СЃС‚РІ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’СЃРµ СЃРІРѕР№СЃС‚РІР° РєРѕРѕСЂРґРёРЅР°С‚ вЂ” backing fields + `OnPropertyChanged()`. Р’ `Rectangle` РґРѕР±Р°РІР»РµРЅС‹ СѓРІРµРґРѕРјР»РµРЅРёСЏ РґР»СЏ `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY` (РјР°СЂРєРµСЂС‹ РІС‹РґРµР»РµРЅРёСЏ).

### Fix S38-5: Enter РЅРµ РєРѕРјРјРёС‚РёС‚ РїРѕР»СЏ РІРІРѕРґР° РєРѕРѕСЂРґРёРЅР°С‚

**РџСЂРѕР±Р»РµРјР°:** `TextBoxLostFocusCommandBehavior` СЂРµР°РіРёСЂРѕРІР°Р» С‚РѕР»СЊРєРѕ РЅР° LostFocus. Enter РЅРµ РїСЂРёРјРµРЅСЏР» Р·РЅР°С‡РµРЅРёРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ РѕР±СЂР°Р±РѕС‚С‡РёРє `KeyDown.Enter` РІ `TextBoxLostFocusCommandBehavior`.

### Fix S38-6: Undo РѕСЃС‚Р°РІР»СЏРµС‚ В«РІРёСЃСЏС‡РµРµВ» РІС‹РґРµР»РµРЅРёРµ

**РџСЂРѕР±Р»РµРјР°:** РџРѕСЃР»Рµ Undo (`AddObjectCommand.Undo()` СѓРґР°Р»СЏРµС‚ РѕР±СЉРµРєС‚) `SelectedObjects` РЅРµ РѕС‡РёС‰Р°Р»СЃСЏ вЂ” РјР°СЂРєРµСЂС‹ РІС‹РґРµР»РµРЅРёСЏ РѕСЃС‚Р°РІР°Р»РёСЃСЊ РЅР° РєР°РЅРІР°СЃРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’ `Undo()`/`Redo()` РґРѕР±Р°РІР»РµРЅ РІС‹Р·РѕРІ `PurgeOrphanedSelection()`, СѓРґР°Р»СЏСЋС‰РёР№ РёР· `SelectedObjects` РѕР±СЉРµРєС‚С‹ РЅРµ РёР· `Template.Objects`.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 589+ РїСЂРѕР№РґРµРЅС‹ (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120 + PropertiesViewModel 50 + Command 137 + Line/Rectangle 30)

## Sprint 39 вЂ” Rectangle HitTest: СЃРµР»РµРєС†РёСЏ С‚РѕР»СЊРєРѕ РїРѕ РіСЂР°РЅРёС†Рµ

### Fix S39-1: РџСЂСЏРјРѕСѓРіРѕР»СЊРЅРёРє РІС‹РґРµР»СЏРµС‚СЃСЏ РїСЂРё РєР»РёРєРµ РІРЅСѓС‚СЂРё РѕР±Р»Р°СЃС‚Рё

**РџСЂРѕР±Р»РµРјР°:** `Rectangle.ContainsPoint()` РёСЃРїРѕР»СЊР·РѕРІР°Р» РїРѕР»РЅСѓСЋ AABB-РїСЂРѕРІРµСЂРєСѓ вЂ” Р»СЋР±Р°СЏ С‚РѕС‡РєР° РІРЅСѓС‚СЂРё РїСЂСЏРјРѕСѓРіРѕР»СЊРЅРёРєР° (РІРєР»СЋС‡Р°СЏ С†РµРЅС‚СЂ) СЃС‡РёС‚Р°Р»Р°СЃСЊ РїРѕРїР°РґР°РЅРёРµРј.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РњРµС‚РѕРґ РїРµСЂРµРїРёСЃР°РЅ РЅР° **border-band РїРѕРґС…РѕРґ** вЂ” С‚РѕС‡РєР° СЃС‡РёС‚Р°РµС‚СЃСЏ РїРѕРїР°РІС€РµР№ РЅР° РїСЂСЏРјРѕСѓРіРѕР»СЊРЅРёРє С‚РѕР»СЊРєРѕ РµСЃР»Рё РѕРЅР° РЅР°С…РѕРґРёС‚СЃСЏ РІ РїСЂРµРґРµР»Р°С… `LineHitToleranceMicrons` (5 РјРј) РѕС‚ Р»СЋР±РѕР№ РёР· С‡РµС‚С‹СЂС‘С… СЃС‚РѕСЂРѕРЅ. Р’РЅСѓС‚СЂРµРЅРЅСЏСЏ РѕР±Р»Р°СЃС‚СЊ (РґР°Р»СЊС€Рµ 5 РјРј РѕС‚ РєСЂР°С‘РІ) РЅРµ СЃРµР»РµРєС‚РёСЂСѓРµС‚СЃСЏ. Р”Р»СЏ РјР°Р»РµРЅСЊРєРёС… РїСЂСЏРјРѕСѓРіРѕР»СЊРЅРёРєРѕРІ (< 10 РјРј) РІСЃСЏ РѕР±Р»Р°СЃС‚СЊ РѕСЃС‚Р°С‘С‚СЃСЏ СЃРµР»РµРєС‚РёСЂСѓРµРјРѕР№.

**Р¤Р°Р№Р»С‹:**
- `Models/Objects/Rectangle.cs` вЂ” `ContainsPoint()` Р·Р°РјРµРЅС‘РЅ РЅР° border-band
- `Tests/Helpers/HitTestHelperTests.cs` вЂ” РѕР±РЅРѕРІР»РµРЅС‹ С‚РµСЃС‚С‹
- `Tests/Helpers/HitTestHelperExtendedTests.cs` вЂ” РѕР±РЅРѕРІР»РµРЅС‹ С‚РµСЃС‚С‹ + РЅРѕРІС‹Р№ `PointNearEdgeLargeRect_ReturnsTrue`
- `Tests/Helpers/AdditionalHelperTests.cs` вЂ” РѕР±РЅРѕРІР»РµРЅС‹ С‚РµСЃС‚С‹
- `Tests/IntegrationTests.cs` вЂ” РѕР±РЅРѕРІР»С‘РЅ `HitTestAll_OverlappingObjects`

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 840+ РїСЂРѕР№РґРµРЅС‹ (РІСЃРµ РєР»СЋС‡РµРІС‹Рµ РєР°С‚РµРіРѕСЂРёРё)

## Sprint 40 вЂ” Keyboard shortcuts + Selection markers

### Fix S40-1: KeyBindings РёРЅСЃС‚СЂСѓРјРµРЅС‚РѕРІ РЅРµ СЃРѕРІРїР°РґР°Р»Рё СЃ UI

**РџСЂРѕР±Р»РµРјР°:** Р¤Р°РєС‚РёС‡РµСЃРєРёРµ KeyBindings (H/L/U/X) РЅРµ СЃРѕРѕС‚РІРµС‚СЃС‚РІРѕРІР°Р»Рё UI (V/L/R/T). Select Р±С‹Р» РЅР° H РІРјРµСЃС‚Рѕ V, Rectangle РЅР° U РІРјРµСЃС‚Рѕ R, Text РЅР° X РІРјРµСЃС‚Рѕ T.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `MainWindow.xaml` вЂ” Hв†’V, Uв†’R, Xв†’T.

### Fix S40-2: R-РєР»Р°РІРёС€Р° РєРѕРЅС„Р»РёРєС‚РѕРІР°Р»Р° (Rectangle vs Rotate)

**РџСЂРѕР±Р»РµРјР°:** R Р±С‹Р»Р° Р·Р°РЅСЏС‚Р° Rotate, РЅРµ РїРѕР·РІРѕР»СЏСЏ РёСЃРїРѕР»СЊР·РѕРІР°С‚СЊ РµС‘ РґР»СЏ Rectangle.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Rotate РїРµСЂРµРЅРµСЃС‘РЅ СЃ R РЅР° E (rotatE) / Shift+E.

### Fix S40-3: РџРµСЂРµРєР»СЋС‡РµРЅРёРµ РёРЅСЃС‚СЂСѓРјРµРЅС‚РѕРІ РЅРµ СЂР°Р±РѕС‚Р°Р»Рѕ СЃ СЂСѓСЃСЃРєРѕР№ СЂР°СЃРєР»Р°РґРєРѕР№

**РџСЂРѕР±Р»РµРјР°:** WPF `KeyBinding` СЃ `KeyGesture` РЅРµ СЃСЂР°Р±Р°С‚С‹РІР°Р» РїСЂРё СЂСѓСЃСЃРєРѕР№ СЂР°СЃРєР»Р°РґРєРµ РєР»Р°РІРёР°С‚СѓСЂС‹.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РРЅСЃС‚СЂСѓРјРµРЅС‚С‹ (V/L/R/T) Рё rotate (E/Shift+E) РїРµСЂРµРЅРµСЃРµРЅС‹ РёР· `Window.InputBindings` РІ `PreviewKeyDown` handler РЅР° Window. `e.Key` РІ PreviewKeyDown РІРѕР·РІСЂР°С‰Р°РµС‚ С„РёР·РёС‡РµСЃРєСѓСЋ РєР»Р°РІРёС€Сѓ РЅРµР·Р°РІРёСЃРёРјРѕ РѕС‚ СЂР°СЃРєР»Р°РґРєРё.

### Fix S40-4: РџР°РЅРµР»СЊ РёРЅСЃС‚СЂСѓРјРµРЅС‚РѕРІ РЅРµ РѕР±РЅРѕРІР»СЏР»Р°СЃСЊ РїСЂРё РіРѕСЂСЏС‡РёС… РєР»Р°РІРёС€Р°С…

**РџСЂРѕР±Р»РµРјР°:** `SetActiveToolCommand` СѓСЃС‚Р°РЅР°РІР»РёРІР°Р» `_toolManager.ActiveTool` РЅР°РїСЂСЏРјСѓСЋ, РјРёРЅСѓСЏ СЃРµС‚С‚РµСЂ `EditorViewModel.ActiveTool`, РєРѕС‚РѕСЂС‹Р№ РІС‹Р·С‹РІР°РµС‚ `OnPropertyChanged()`. RadioButton РЅР° toolbar РЅРµ РїРѕР»СѓС‡Р°Р» СѓРІРµРґРѕРјР»РµРЅРёРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `SetActiveTool()` С‚РµРїРµСЂСЊ РІС‹Р·С‹РІР°РµС‚ `ActiveTool = tool` (СЃРµС‚С‚РµСЂ СЃРІРѕР№СЃС‚РІР° СЃ `OnPropertyChanged()`).

### Fix S40-5: РњР°СЂРєРµСЂС‹ РІС‹РґРµР»РµРЅРёСЏ РЅРµ РїРѕСЏРІР»СЏР»РёСЃСЊ РЅР° РІС‹Р±СЂР°РЅРЅС‹С… РѕР±СЉРµРєС‚Р°С…

**РџСЂРѕР±Р»РµРјР°:** `ShowSelectionMarkers` РІРѕР·РІСЂР°С‰Р°Р» `true` С‚РѕР»СЊРєРѕ РїСЂРё `SelectedObjects.Count == 1`. РџСЂРё РјСѓР»СЊС‚Рё-РІС‹РґРµР»РµРЅРёРё `ContentControl` СЃ РјР°СЂРєРµСЂР°РјРё Р±С‹Р» СЃРєСЂС‹С‚.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `SelectionManager.ShowSelectionMarkers` вЂ” `Count > 0` (РІРјРµСЃС‚Рѕ `Count == 1`)
- `ContentControl Content="{Binding SingleSelectedObject}"` Р·Р°РјРµРЅС‘РЅ РЅР° `ItemsControl ItemsSource="{Binding SelectedObjects}"` СЃ `Canvas` ItemsPanel вЂ” РјР°СЂРєРµСЂС‹ СЂРµРЅРґРµСЂСЏС‚СЃСЏ РґР»СЏ РєР°Р¶РґРѕРіРѕ РІС‹РґРµР»РµРЅРЅРѕРіРѕ РѕР±СЉРµРєС‚Р°

**Р¤Р°Р№Р»С‹:**
- `MainWindow.xaml` вЂ” KeyBindings в†’ PreviewKeyDown
- `MainWindow.xaml.cs` вЂ” `Window_PreviewKeyDown()` handler
- `ViewModels/Managers/ToolManager.cs` вЂ” РЅРµ РёР·РјРµРЅСЏР»СЃСЏ
- `ViewModels/EditorViewModel.cs` вЂ” `SetActiveTool()` С‡РµСЂРµР· СЃРµС‚С‚РµСЂ
- `ViewModels/Managers/SelectionManager.cs` вЂ” `ShowSelectionMarkers` в†’ `Count > 0`
- `Views/EditorCanvas.xaml` вЂ” ContentControl в†’ ItemsControl

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 844+ РїСЂРѕР№РґРµРЅС‹ (РІСЃРµ РєР»СЋС‡РµРІС‹Рµ РєР°С‚РµРіРѕСЂРёРё)

## Sprint 41 вЂ” Drag move delta drift + Text INPC

### Fix S41-1: Delta accumulation drift on multi-MouseMove

**РџСЂРѕР±Р»РµРјР°:** `SelectTool.OnMouseMove()` РІС‹С‡РёСЃР»СЏР» `newX = obj.MicronsX + delta`, РіРґРµ `delta` вЂ” РїРѕР»РЅРѕРµ СЃРјРµС‰РµРЅРёРµ РѕС‚ С‚РѕС‡РєРё СЃС‚Р°СЂС‚Р°. РќРѕ `obj.MicronsX` СѓР¶Рµ РѕР±РЅРѕРІР»С‘РЅ РЅР° РїСЂРµРґС‹РґСѓС‰РµРј `MouseMove`, РїРѕСЌС‚РѕРјСѓ РєР°Р¶РґРѕРµ РЅРѕРІРѕРµ СЃРѕР±С‹С‚РёРµ РґРѕР±Р°РІР»СЏР»Рѕ РґРµР»СЊС‚Сѓ Рє СѓР¶Рµ СЃРјРµС‰С‘РЅРЅРѕР№ РїРѕР·РёС†РёРё. РћР±СЉРµРєС‚ В«СѓР±РµРіР°Р»В» РѕС‚ РєСѓСЂСЃРѕСЂР° (РґСЂРёС„С‚, РїСЂРѕРїРѕСЂС†РёРѕРЅР°Р»СЊРЅС‹Р№ РєРѕР»РёС‡РµСЃС‚РІСѓ `MouseMove`).

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РµР»СЊС‚Р° РїСЂРёР±Р°РІР»СЏРµС‚СЃСЏ Рє **СЃРѕС…СЂР°РЅС‘РЅРЅРѕР№ РЅР°С‡Р°Р»СЊРЅРѕР№ РїРѕР·РёС†РёРё** РёР· `_initialPositions[obj]`.

**Р¤Р°Р№Р»:** `Tools/SelectTool.cs:208-210`

### Fix S41-2: Text INPC for MicronsX/MicronsY

**РџСЂРѕР±Р»РµРјР°:** `Text` РЅРµ СЂРµР°Р»РёР·РѕРІС‹РІР°Р» `INotifyPropertyChanged` вЂ” `Text.Move()` СѓСЃС‚Р°РЅР°РІР»РёРІР°Р» `MicronsX`/`MicronsY` (auto-properties), РЅРѕ WPF-Р±РёРЅРґРёРЅРіРё `Canvas.Left`/`Canvas.Top` РЅРµ РѕР±РЅРѕРІР»СЏР»РёСЃСЊ. РўРµРєСЃС‚ РІРёР·СѓР°Р»СЊРЅРѕ РЅРµ РґРІРёРіР°Р»СЃСЏ РїСЂРё РїРµСЂРµС‚Р°СЃРєРёРІР°РЅРёРё.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `Text` СЂРµР°Р»РёР·СѓРµС‚ `INotifyPropertyChanged`
- Override `MicronsX`/`MicronsY` СЃ backing fields + `OnPropertyChanged()`
- РЈРІРµРґРѕРјР»РµРЅРёСЏ РґР»СЏ `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`

**Р¤Р°Р№Р»:** `Models/Objects/Text.cs:12-52`

### Cleanup
- РЈРґР°Р»РµРЅС‹ РјС‘СЂС‚РІС‹Рµ РїРѕР»СЏ `_dragStartX`/`_dragStartY`
- РЈРїСЂРѕС‰С‘РЅ СЂР°СЃС‡С‘С‚ РґРµР»СЊС‚С‹ (РѕР±Р° if-else РІС‹С‡РёСЃР»СЏР»Рё РѕРґРЅРѕ Рё С‚Рѕ Р¶Рµ)

**Build:** 0 errors, 0 warnings
**Tests:** 844+ РїСЂРѕР№РґРµРЅС‹ (РІСЃРµ РєР»СЋС‡РµРІС‹Рµ РєР°С‚РµРіРѕСЂРёРё)

## Sprint 42 вЂ” StrokeThicknessMicrons (С‚РѕР»С‰РёРЅР° Р»РёРЅРёРё)

### Feature S42-1: Р”РѕР±Р°РІР»РµРЅРѕ СЃРІРѕР№СЃС‚РІРѕ StrokeThicknessMicrons

**РџСЂРѕР±Р»РµРјР°:** Р’ XSD-СЃРїРµС†РёС„РёРєР°С†РёРё РїСЂРµРґСѓСЃРјРѕС‚СЂРµРЅ `StrokeThickness` (xs:long) РґР»СЏ Line Рё Rectangle, РЅРѕ РІ РєРѕРґРµ СЃРІРѕР№СЃС‚РІРѕ РѕС‚СЃСѓС‚СЃС‚РІРѕРІР°Р»Рѕ РЅР° РІСЃРµС… СѓСЂРѕРІРЅСЏС… вЂ” РјРѕРґРµР»Рё, СЃРµСЂРёР°Р»РёР·Р°С†РёСЏ, UI РїР°РЅРµР»Рё СЃРІРѕР№СЃС‚РІ, РѕС‚СЂРёСЃРѕРІРєР° РЅР° РєР°РЅРІР°СЃРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р РµР°Р»РёР·РѕРІР°РЅРѕ end-to-end:

| РЈСЂРѕРІРµРЅСЊ | Р¤Р°Р№Р» | РР·РјРµРЅРµРЅРёРµ |
|---------|------|-----------|
| РљРѕРЅСЃС‚Р°РЅС‚Р° | `Constants/EditorConstants.cs:86-88` | `DefaultStrokeThicknessMicrons = 500` (0.5 РјРј) |
| РњРѕРґРµР»СЊ Line | `Models/Objects/Line.cs:23,87-101,126,133,148` | РџРѕР»Рµ + INPC-СЃРІРѕР№СЃС‚РІРѕ + РїР°СЂР°РјРµС‚СЂ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂР° + Clone |
| РњРѕРґРµР»СЊ Rectangle | `Models/Objects/Rectangle.cs:23,88-102,152,161,173` | РђРЅР°Р»РѕРіРёС‡РЅРѕ |
| РЎРµСЂРёР°Р»РёР·Р°С†РёСЏ | `Services/TemplateService.cs:106,121,124,361,367,425,434` | DTO-РїРѕР»Рµ + MapToObject/MapToDto |
| ViewModel | `ViewModels/PropertiesViewModel.cs:169,177,275-290,348-362,525-536` | РЎРІРѕР№СЃС‚РІР° + РєРѕРјР°РЅРґС‹ + string-РѕР±С‘СЂС‚РєРё + UpdateSelection |
| UI РїР°РЅРµР»Рё | `Views/PropertiesPanelContent.xaml:130-142,227-240` | TextBox В«РўРѕР»С‰РёРЅР° (РјРј)В» РґР»СЏ Line Рё Rectangle |
| Canvas | `Views/EditorCanvas.xaml:67-77,153-163` | `StrokeThickness` РїСЂРёРІСЏР·Р°РЅ Рє РјРѕРґРµР»Рё С‡РµСЂРµР· `MicronsToPixelConverter` (Style Setter + MultiBinding) |

**Р”РµС‚Р°Р»Рё СЂРµР°Р»РёР·Р°С†РёРё:**
- Р’СЃРµ РІРЅСѓС‚СЂРµРЅРЅРёРµ РєРѕРѕСЂРґРёРЅР°С‚С‹ РІ РјРёРєСЂРѕРЅР°С… (`long`), WPF-РїРёРєСЃРµР»Рё С‡РµСЂРµР· `MicronsToPixelConverter` СЃ СѓС‡С‘С‚РѕРј Zoom
- DataTrigger'С‹ РІС‹РґРµР»РµРЅРёСЏ (StrokeThickness=2) Рё РЅР°РІРµРґРµРЅРёСЏ (StrokeThickness=2.5) РѕСЃС‚Р°СЋС‚СЃСЏ РЅРµРёР·РјРµРЅРЅС‹РјРё вЂ” РѕРЅРё override Р±Р°Р·РѕРІС‹Р№ Style Setter С‡РµСЂРµР· WPF precedence
- Р—РЅР°С‡РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ: 500 РјРёРєСЂРѕРЅ (0.5 РјРј) вЂ” СЃРѕРѕС‚РІРµС‚СЃС‚РІСѓРµС‚ Р“РћРЎРў 2.303-68 РґР»СЏ С‚РѕРЅРєРѕР№ Р»РёРЅРёРё
- Drawing РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹ (LineTool/RectangleTool) РЅРµ С‚СЂРµР±СѓСЋС‚ РёР·РјРµРЅРµРЅРёР№ вЂ” РґРµС„РѕР»С‚РЅС‹Р№ РїР°СЂР°РјРµС‚СЂ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂР° 500 РјРёРєСЂРѕРЅ

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ РїСЂРѕР№РґРµРЅС‹ (0 failures)

## Sprint 43 вЂ” ResizeTool dispatch fix

### Fix S43-1: GetCurrentTool() РЅРµ РёРјРµРµС‚ case "Resize"

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior.GetCurrentTool()` РЅРµ РѕР±СЂР°Р±Р°С‚С‹РІР°Р» `"Resize"` РІ switch вЂ” РїСЂРё РїР°РґРµРЅРёРё РЅР° default РІРѕР·РІСЂР°С‰Р°Р» `SelectTool`. РџРѕСЃР»Рµ С‚РѕРіРѕ РєР°Рє `SelectTool.OnMouseDown()` РґРµС‚РµРєС‚РёР» С…РµРЅРґР» Рё РїСѓС€РёР» `"Resize"` РІ СЃС‚РµРє РёРЅСЃС‚СЂСѓРјРµРЅС‚РѕРІ, РїРѕСЃР»РµРґСѓСЋС‰РёРµ `OnMouseMove`/`OnMouseUp` СѓС…РѕРґРёР»Рё РІ `SelectTool` РІРјРµСЃС‚Рѕ `ResizeTool`. Р Р°Р·РјРµСЂС‹ РѕР±СЉРµРєС‚РѕРІ РЅРµ РјРµРЅСЏР»РёСЃСЊ РїСЂРё drag Р·Р° СѓРіР»РѕРІС‹Рµ РјР°СЂРєРµСЂС‹, РєРѕРјР°РЅРґР° `CustomResizeCommand` РЅРµ СЃРѕР·РґР°РІР°Р»Р°СЃСЊ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ case `"Resize" => editor.GetOrCreateTool<ResizeTool>()` РІ `GetCurrentTool()`.

**Р¤Р°Р№Р»:** `Behaviors/EditorCanvasBehavior.cs:288-298`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ РїСЂРѕР№РґРµРЅС‹ (0 failures)

## Sprint 44 вЂ” PropertiesPanel live update after resize

### Fix S44-1: PropertiesViewModel РЅРµ РїРѕРґРїРёСЃР°РЅ РЅР° INPC РѕР±СЉРµРєС‚Р°

**РџСЂРѕР±Р»РµРјР°:** `PropertiesViewModel` РЅРµ РїРѕРґРїРёСЃС‹РІР°Р»СЃСЏ РЅР° `INotifyPropertyChanged.PropertyChanged` РІС‹РґРµР»РµРЅРЅРѕРіРѕ РѕР±СЉРµРєС‚Р°. РџСЂРё РёР·РјРµРЅРµРЅРёРё СЂР°Р·РјРµСЂРѕРІ С‡РµСЂРµР· `ResizeTool` РјРѕРґРµР»СЊ РѕРїРѕРІРµС‰Р°Р»Р° (`OnPropertyChanged`), РЅРѕ ViewModel РЅРµ РїРµСЂРµР·Р°РїСЂР°С€РёРІР°Р»Р° СЃРІРѕРё computed-СЃРІРѕР№СЃС‚РІР° (`RectX`, `LineEndX`, `TextFontSize` Рё С‚.Рґ.). WPF-Р±РёРЅРґРёРЅРіРё РЅР° РїР°РЅРµР»Рё СЃРІРѕР№СЃС‚РІ РЅРµ РѕР±РЅРѕРІР»СЏР»РёСЃСЊ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `PropertiesViewModel.UpdateSelection()` вЂ” РїСЂРё СЃРјРµРЅРµ РІС‹РґРµР»РµРЅРёСЏ РѕС‚РїРёСЃС‹РІР°РµС‚СЃСЏ РѕС‚ СЃС‚Р°СЂРѕРіРѕ РѕР±СЉРµРєС‚Р°, РїРѕРґРїРёСЃС‹РІР°РµС‚СЃСЏ РЅР° РЅРѕРІС‹Р№
- Р”РѕР±Р°РІР»РµРЅ РјРµС‚РѕРґ `OnSelectedObjectPropertyChanged()`, РєРѕС‚РѕСЂС‹Р№ РїРѕ РёРјРµРЅРё СЃРІРѕР№СЃС‚РІР° РјРѕРґРµР»Рё РѕРїСЂРµРґРµР»СЏРµС‚, РєР°РєРѕРµ ViewModel-СЃРІРѕР№СЃС‚РІРѕ РѕРїРѕРІРµСЃС‚РёС‚СЊ
- РџСЂРё `Dispose()` вЂ” РіР°СЂР°РЅС‚РёСЂРѕРІР°РЅРЅР°СЏ РѕС‚РїРёСЃРєР°

**Р¤Р°Р№Р»:** `ViewModels/PropertiesViewModel.cs:109-210`

### Fix S44-2: Text INPC РґР»СЏ РІСЃРµС… СЃРІРѕР№СЃС‚РІ

**РџСЂРѕР±Р»РµРјР°:** `Text.FontSizeMicrons`, `Content`, `FontName`, `TextType`, `RotationAngle` Р±С‹Р»Рё auto-properties Р±РµР· INPC. Р”Р°Р¶Рµ СЃ РїРѕРґРїРёСЃРєРѕР№ PropertiesViewModel РЅР° `PropertyChanged`, СЌС‚Рё СЃРІРѕР№СЃС‚РІР° РЅРµ РѕРїРѕРІРµС‰Р°Р»Рё РѕР± РёР·РјРµРЅРµРЅРёСЏС….

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’СЃРµ СЃРІРѕР№СЃС‚РІР° РїРµСЂРµРІРµРґРµРЅС‹ РЅР° backing fields + `OnPropertyChanged()`. Р”Р»СЏ `FontSizeMicrons` Рё `Content` РґРѕР±Р°РІР»РµРЅС‹ СѓРІРµРґРѕРјР»РµРЅРёСЏ РґР»СЏ Р·Р°РІРёСЃРёРјС‹С… computed-СЃРІРѕР№СЃС‚РІ (`WidthMicrons`, `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`).

**Р¤Р°Р№Р»:** `Models/Objects/Text.cs:53-110`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1287+ РїСЂРѕР№РґРµРЅС‹ (0 failures)

## Sprint 45 вЂ” Pan delta accumulation fix (RenderTransform drift)

### Fix S45-1: РџР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёРµ СѓСЃРєРѕСЂСЏРµС‚СЃСЏ РёР·-Р·Р° RenderTransform РІ e.GetPosition

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior.State_MouseMove()` РІС‹С‡РёСЃР»СЏР» РґРµР»СЊС‚Сѓ РїР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёСЏ РёР· `e.GetPosition(canvas)`, РєРѕС‚РѕСЂС‹Р№ СѓС‡РёС‚С‹РІР°РµС‚ `RenderTransform` canvas'Р° (`TranslateTransform CanvasOffsetX/Y`). РџРѕСЃР»Рµ РєР°Р¶РґРѕРіРѕ `MouseMove` canvas СЃРґРІРёРіР°Р»СЃСЏ, Рё РЅР° СЃР»РµРґСѓСЋС‰РµРј `MouseMove` `e.GetPosition(canvas)` РІРѕР·РІСЂР°С‰Р°Р» РєРѕРѕСЂРґРёРЅР°С‚С‹, СѓР¶Рµ РІРєР»СЋС‡Р°СЋС‰РёРµ РїСЂРµРґС‹РґСѓС‰РёР№ СЃРґРІРёРі. РљР°Р¶РґРѕРµ РґРІРёР¶РµРЅРёРµ РјС‹С€Рё РґРѕР±Р°РІР»СЏР»Рѕ РґРµР»СЊС‚Сѓ РїСЂРµРґС‹РґСѓС‰РµРіРѕ РїР°РЅР° вЂ” РїР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёРµ РЅРµРєРѕРЅС‚СЂРѕР»РёСЂСѓРµРјРѕ СѓСЃРєРѕСЂСЏР»РѕСЃСЊ (`runaway pan`).

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РµР»СЊС‚Р° РІС‹С‡РёСЃР»СЏРµС‚СЃСЏ РІ **Window-РєРѕРѕСЂРґРёРЅР°С‚Р°С…** (`e.GetPosition(window)`), РєРѕС‚РѕСЂС‹Рµ РЅРµ РјРµРЅСЏСЋС‚СЃСЏ РїСЂРё СЃРґРІРёРіРµ canvas'Р°. Р”РѕР±Р°РІР»РµРЅС‹ РїРѕР»СЏ `PanStartWpfPoint` Рё `PanAppliedModelDelta` РІ `EditorCanvasState` РґР»СЏ РєРѕСЂСЂРµРєС‚РЅРѕРіРѕ РёРЅРєСЂРµРјРµРЅС‚Р°Р»СЊРЅРѕРіРѕ СЂР°СЃС‡С‘С‚Р°.

**Р¤Р°Р№Р»:** `Behaviors/EditorCanvasBehavior.cs:96-165,199,343-349`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** PanTool 13/13, EditorCanvas/ZoomPan 10/10 вЂ” РІСЃРµ РїСЂРѕР№РґРµРЅС‹

## Sprint 46 вЂ” Context menu fixes

### Fix S46-1: Canvas context menu blocked by State_MouseDown e.Handled

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior.State_MouseDown()` Р±РµР·СѓСЃР»РѕРІРЅРѕ СѓСЃС‚Р°РЅР°РІР»РёРІР°Р» `e.Handled = true` РґР»СЏ Р’РЎР•РҐ РєРЅРѕРїРѕРє РјС‹С€Рё, РІРєР»СЋС‡Р°СЏ РїСЂР°РІСѓСЋ. WPF РЅРµ РїРѕРєР°Р·С‹РІР°Р» `ContextMenu` РЅР° UserControl, С‚.Рє. СЃРѕР±С‹С‚РёРµ Р±С‹Р»Рѕ РїРѕРјРµС‡РµРЅРѕ РєР°Рє РѕР±СЂР°Р±РѕС‚Р°РЅРЅРѕРµ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’ `State_MouseDown()` РїСЂРё РїСЂР°РІРѕРј РєР»РёРєРµ СЏРІРЅРѕ РѕС‚РєСЂС‹РІР°РµРј `UserControl.ContextMenu` РїСЂРѕРіСЂР°РјРјРЅРѕ С‡РµСЂРµР· `VisualTreeHelper`. Р’ `State_MouseUp()` РґРѕР±Р°РІР»РµРЅ СЂР°РЅРЅРёР№ return РґР»СЏ РїСЂР°РІРѕР№ РєРЅРѕРїРєРё.

**Р¤Р°Р№Р»С‹:**
- `Behaviors/EditorCanvasBehavior.cs:94-108,210-212` вЂ” СЏРІРЅРѕРµ РѕС‚РєСЂС‹С‚РёРµ ContextMenu + СЂР°РЅРЅРёР№ return РІ MouseUp
- `EditorCanvas.xaml:22-43` вЂ” РєРѕРЅС‚РµРєСЃС‚РЅРѕРµ РјРµРЅСЋ РѕРїСЂРµРґРµР»РµРЅРѕ РЅР° UserControl (Р±РµР· РёР·РјРµРЅРµРЅРёР№)

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 3/3 RightClick_Ignored + 13/13 PanTool вЂ” РїСЂРѕР№РґРµРЅС‹

### Fix S46-2: TabItem context menu commands not working (Async suffix mismatch)

**РџСЂРѕР±Р»РµРјР°:** РњРµС‚РѕРґС‹ `CloseTabAsync()`, `CloseOtherTabsAsync()`, `CloseAllTabsAsync()` РІ `EditorViewModel` РІРѕР·РІСЂР°С‰Р°Р»Рё `void` (РЅРµ async). CommunityToolkit.Mvvm 8.4.2 source generator РѕР±СЂРµР·Р°РµС‚ СЃСѓС„С„РёРєСЃ `Async` **С‚РѕР»СЊРєРѕ РґР»СЏ Р°СЃРёРЅС…СЂРѕРЅРЅС‹С… РјРµС‚РѕРґРѕРІ** (РІРѕР·РІСЂР°С‰Р°СЋС‰РёС… `Task`). Р”Р»СЏ `void`-РјРµС‚РѕРґРѕРІ СЃСѓС„С„РёРєСЃ СЃРѕС…СЂР°РЅСЏРµС‚СЃСЏ в†’ РіРµРЅРµСЂРёСЂРѕРІР°Р»РёСЃСЊ `CloseTabAsyncCommand`, Р° XAML Р±РёРЅРґРёР»СЃСЏ Рє `CloseTabCommand` вЂ” РєРѕРјР°РЅРґР° РЅРµ РЅР°С…РѕРґРёР»Р°СЃСЊ, MenuItem Р±С‹Р» РЅРµР°РєС‚РёРІРµРЅ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РњРµС‚РѕРґС‹ РїРµСЂРµРёРјРµРЅРѕРІР°РЅС‹ вЂ” СѓР±СЂР°РЅ СЃСѓС„С„РёРєСЃ `Async`:
- `CloseTabAsync()` в†’ `CloseTab()`
- `CloseOtherTabsAsync()` в†’ `CloseOtherTabs()`
- `CloseAllTabsAsync()` в†’ `CloseAllTabs()`

**Р¤Р°Р№Р»:** `ViewModels/EditorViewModel.cs:45-67`

**Common Mistakes (new):**
23. `[RelayCommand]` on `void` method with `Async` suffix вЂ” source generator РќР• РѕР±СЂРµР·Р°РµС‚ СЃСѓС„С„РёРєСЃ РґР»СЏ СЃРёРЅС…СЂРѕРЅРЅС‹С… РјРµС‚РѕРґРѕРІ. РРјСЏ РєРѕРјР°РЅРґС‹ Р±СѓРґРµС‚ `MethodAsyncCommand`, Р° РЅРµ `MethodCommand`. Р”Р»СЏ async РјРµС‚РѕРґРѕРІ (РІРѕР·РІСЂР°С‰Р°СЋС‰РёС… `Task`/`Task<T>`) СЃСѓС„С„РёРєСЃ РѕР±СЂРµР·Р°РµС‚СЃСЏ.
24. ContextMenu РІРЅСѓС‚СЂРё `Style` (`Setter.Value`) вЂ” РЅРµ РїРѕР»Р°РіР°Р№СЃСЏ РЅР° Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРѕРµ РЅР°СЃР»РµРґРѕРІР°РЅРёРµ `DataContext` С‡РµСЂРµР· `PlacementTarget`. Р•СЃР»Рё РєРѕРјР°РЅРґС‹ РЅРµ СЂР°Р±РѕС‚Р°СЋС‚, РёСЃРїРѕР»СЊР·СѓР№ СЏРІРЅРѕРµ СѓРєР°Р·Р°РЅРёРµ `DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"`.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 12/12 CloseTab + RightClick вЂ” РїСЂРѕР№РґРµРЅС‹

## Sprint 47 вЂ” Grid 1mm MinPixelSpacing fix

### Fix S47-1: РЎРµС‚РєР° РЅРµ РѕС‚РѕР±СЂР°Р¶Р°РµС‚СЃСЏ РїСЂРё С€Р°РіРµ 1РјРј

**РџСЂРѕР±Р»РµРјР°:** РџСЂРё СѓСЃС‚Р°РЅРѕРІРєРµ С€Р°РіР° СЃРµС‚РєРё 1РјРј СЃРµС‚РєР° РїРѕР»РЅРѕСЃС‚СЊСЋ РїСЂРѕРїР°РґР°Р»Р° СЃ С…РѕР»СЃС‚Р°. Р”РІР° СЃС†РµРЅР°СЂРёСЏ:
- **A3+:** `cols * rows` (125K+) РїСЂРµРІС‹С€Р°РµС‚ `MaxGridNodes (100000)` в†’ `GenerateGridNodes()` РІРѕР·РІСЂР°С‰Р°РµС‚ РїСѓСЃС‚РѕР№ СЃРїРёСЃРѕРє
- **A4:** 62K СѓР·Р»РѕРІ РіРµРЅРµСЂРёСЂСѓСЋС‚СЃСЏ, РЅРѕ РїСЂРё Zoom=1.0 СЂР°СЃСЃС‚РѕСЏРЅРёРµ 1px < 2px (РґРёР°РјРµС‚СЂ С‚РѕС‡РєРё 2px) в†’ СЃРїР»РѕС€РЅР°СЏ СЃРµСЂР°СЏ Р·Р°Р»РёРІРєР°

**РџСЂРёС‡РёРЅР°:** `GridManager.RefreshGridNodes()` Рё `GridHelper.GenerateGridNodes()` РЅРµ РїСЂРѕРІРµСЂСЏР»Рё `MinPixelSpacing` (5px) вЂ” РјРёРЅРёРјР°Р»СЊРЅРѕРµ СЂР°СЃСЃС‚РѕСЏРЅРёРµ РјРµР¶РґСѓ СѓР·Р»Р°РјРё РІ РїРёРєСЃРµР»СЏС…, РїСЂРё РєРѕС‚РѕСЂРѕРј С‚РѕС‡РєРё СЃРµС‚РєРё СЂР°Р·Р»РёС‡РёРјС‹.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `GridManager.RefreshGridNodes()` вЂ” РїСЂРѕРІРµСЂРєР° `pixelSpacing < MinPixelSpacing` в†’ СЂР°РЅРЅРёР№ return
- `GridHelper.GenerateGridNodes()` вЂ” defense-in-depth: С‚Р° Р¶Рµ РїСЂРѕРІРµСЂРєР°

**РџРѕРІРµРґРµРЅРёРµ:**
- 1РјРј СЃРµС‚РєР° РїСЂРё Zoom < 500%: СЃРєСЂС‹С‚Р° (С‚РѕС‡РєРё < 5px вЂ” СЃР»РёС€РєРѕРј РїР»РѕС‚РЅРѕ)
- 1РјРј СЃРµС‚РєР° РїСЂРё Zoom в‰Ґ 500%: РѕС‚РѕР±СЂР°Р¶Р°РµС‚СЃСЏ
- 5РјРј СЃРµС‚РєР° РїСЂРё Zoom в‰Ґ 100%: РѕС‚РѕР±СЂР°Р¶Р°РµС‚СЃСЏ (РєР°Рє Рё СЂР°РЅСЊС€Рµ)

**Р¤Р°Р№Р»С‹:** `ViewModels/Managers/GridManager.cs`, `Helpers/GridHelper.cs`, `Tests/Helpers/GridHelperTests.cs`

**Common Mistakes (new):**
25. Grid nodes (GenerateGridNodes) must check MinPixelSpacing вЂ” unlike lines, nodes don't auto-hide when too dense, causing either MaxGridNodes overflow (A3+) or solid grey fill (A4). Always check `stepMm * zoom < MinPixelSpacing` before generating nodes.

## Sprint 48 вЂ” Dirty indicator (*) in tab header not appearing

### Fix S48-1: PropertyChanged notification not forwarded from DirtyStateManager

**РџСЂРѕР±Р»РµРјР°:** `EditorViewModel.IsDirty`, `DisplayName` Рё `FilePath` вЂ” plain forwarding properties Рє `DirtyStateManager` Р±РµР· `PropertyChanged`. WPF DataTrigger `{Binding IsDirty}` РІ ControlTemplate TabItem РїРѕРґРїРёСЃР°РЅ РЅР° `EditorViewModel.PropertyChanged`, РЅРѕ СѓРІРµРґРѕРјР»РµРЅРёРµ РїСЂРёС…РѕРґРёС‚ РѕС‚ `DirtyStateManager` (С‡РµСЂРµР· `[ObservableProperty]`). Р’ РёС‚РѕРіРµ `DirtyIndicator` (Р·РІС‘Р·РґРѕС‡РєР° `*`) РЅРёРєРѕРіРґР° РЅРµ СЃС‚Р°РЅРѕРІРёС‚СЃСЏ `Visible`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅР° РїРѕРґРїРёСЃРєР° РЅР° `_dirtyStateManager.PropertyChanged` РІ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂРµ `EditorViewModel` вЂ” РїСЂРѕР±СЂРѕСЃ `IsDirty`, `DisplayName`, `FilePath` С‡РµСЂРµР· `OnPropertyChanged()` (Р°РЅР°Р»РѕРіРёС‡РЅРѕ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РµРјСѓ РїР°С‚С‚РµСЂРЅСѓ РґР»СЏ `_zoomPanManager` Рё `_previewManager`).

**Р¤Р°Р№Р»:** `ViewModels/EditorViewModel.cs:752-764`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** EditorViewModel 112/112, ToolTests 113/113, IntegrationTests 49/49, MarkDirty 27/27 вЂ” РІСЃРµ РїСЂРѕР№РґРµРЅС‹

**Common Mistakes (new):**
26. Plain forwarding properties to a delegated `[ObservableObject]` manager вЂ” if the ViewModel wraps a manager's `[ObservableProperty]` with a regular property, `PropertyChanged` fires from the manager, not the ViewModel. Always subscribe to manager's `PropertyChanged` and forward needed notifications (same pattern as `_zoomPanManager`, `_previewManager`, `_dirtyStateManager`).

## Sprint 49 вЂ” ResizeTool clamp fix + test corrections

### Fix S49-1: Minimum-size clamp moves fixed edges

**РџСЂРѕР±Р»РµРјР°:** Clamp РјРёРЅРёРјР°Р»СЊРЅРѕРіРѕ СЂР°Р·РјРµСЂР° (`MinResizeSizeMicrons = 1000`) РїСЂРёРјРµРЅСЏР» `Min`/`Max` Р±РµР·СѓСЃР»РѕРІРЅРѕ Рє **РѕР±РµРёРј** РіСЂР°РЅСЏРј РІ РѕСЃРё, РґРІРёРіР°СЏ С„РёРєСЃРёСЂРѕРІР°РЅРЅС‹Рµ РіСЂР°РЅРё. РќР°РїСЂРёРјРµСЂ, РґР»СЏ `TopRight` (РїСЂР°РІР°СЏ+РІРµСЂС…РЅСЏСЏ РґРІРёР¶СѓС‚СЃСЏ, Р»РµРІР°СЏ+РЅРёР¶РЅСЏСЏ С„РёРєСЃРёСЂРѕРІР°РЅС‹) РїСЂРё РїРµСЂРµСЃРµС‡РµРЅРёРё РїСЂР°РІРѕР№ РіСЂР°РЅСЊСЋ Р»РµРІРѕР№, clamp СЃРґРІРёРіР°Р» **Р»РµРІСѓСЋ** (С„РёРєСЃРёСЂРѕРІР°РЅРЅСѓСЋ) РіСЂР°РЅСЊ, Р° РЅРµ РѕРіСЂР°РЅРёС‡РёРІР°Р» **РїСЂР°РІСѓСЋ** (РґРІРёР¶СѓС‰СѓСЋСЃСЏ).

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Clamp СЃС‚Р°Р» handle-Р·Р°РІРёСЃРёРјС‹Рј:
- РћРїСЂРµРґРµР»СЏСЋС‚СЃСЏ `leftMoves`, `rightMoves`, `bottomMoves`, `topMoves` РїРѕ С‚РёРїСѓ РјР°СЂРєРµСЂР°
- РћРіСЂР°РЅРёС‡РёРІР°РµС‚СЃСЏ **С‚РѕР»СЊРєРѕ РґРІРёР¶СѓС‰Р°СЏСЃСЏ** РіСЂР°РЅСЊ: `Min()` РґР»СЏ Р»РµРІРѕР№/РЅРёР¶РЅРµР№, `Max()` РґР»СЏ РїСЂР°РІРѕР№/РІРµСЂС…РЅРµР№
- РџСЂРё Ctrl РѕР±Рµ РіСЂР°РЅРё РґРІРёР¶СѓС‚СЃСЏ в†’ СЃРёРјРјРµС‚СЂРёС‡РЅС‹Р№ СЃС…Р»РѕРї С‡РµСЂРµР· СЃРµСЂРµРґРёРЅСѓ РїСЂРё РЅР°СЂСѓС€РµРЅРёРё minSize

**Р¤Р°Р№Р»:** `Tools/ResizeTool.cs:248-282`

### Fix S49-2: РўРµСЃС‚С‹ РїРѕРґ СЃС‚Р°СЂСѓСЋ Р±Р°Р¶РЅСѓСЋ С„РѕСЂРјСѓР»Сѓ

**РџСЂРѕР±Р»РµРјР°:** 14 С‚РµСЃС‚РѕРІ РІ `ResizeToolTests.cs` Рё `ResizeToolExtendedTests.cs` СЃРѕРґРµСЂР¶Р°Р»Рё РѕР¶РёРґР°РµРјС‹Рµ Р·РЅР°С‡РµРЅРёСЏ, СЃРѕРѕС‚РІРµС‚СЃС‚РІСѓСЋС‰РёРµ СЃС‚Р°СЂРѕР№ Р±Р°Р¶РЅРѕР№ С„РѕСЂРјСѓР»Рµ (double-delta, half-delta, РЅРµРїСЂР°РІРёР»СЊРЅС‹Р№ pivot).

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’СЃРµ С‚РµСЃС‚С‹ РїРµСЂРµРїРёСЃР°РЅС‹ РїРѕРґ РєРѕСЂСЂРµРєС‚РЅСѓСЋ edge-based РјРѕРґРµР»СЊ. Р”РѕР±Р°РІР»РµРЅ `SnapEnabled = false` РІ С‚РµСЃС‚С‹, РіРґРµ РѕРЅ РѕС‚СЃСѓС‚СЃС‚РІРѕРІР°Р».

**Р¤Р°Р№Р»С‹:** `Tests/Tools/ResizeToolTests.cs`, `Tests/Tools/ResizeToolExtendedTests.cs`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 63/63 ResizeTool, 1500+ РѕСЃС‚Р°Р»СЊРЅС‹С… вЂ” РїСЂРѕР№РґРµРЅС‹

**Common Mistakes (new):**
27. Minimum-size clamp in edge-based resize вЂ” don't apply `Min`/`Max` to both edges in an axis. Only the MOVING edge should be constrained. Determine `leftMoves`/`rightMoves`/`bottomMoves`/`topMoves` per handle type (or set all true for Ctrl) and constrain only the moving edge(s). Fixed edges must NEVER be moved by the clamp.

## Sprint 50 вЂ” Clipboard improvements (Copy/Paste/Cut)

### Feature S50-1: Ctrl+X keyboard shortcut + UI

**РџСЂРѕР±Р»РµРјР°:** `InputGestureText="Ctrl+X"` РѕС‚РѕР±СЂР°Р¶Р°Р»СЃСЏ РІ РєРѕРЅС‚РµРєСЃС‚РЅРѕРј РјРµРЅСЋ, РЅРѕ:
- РџСЂРёРІСЏР·РєРё РІ `Window.InputBindings` РЅРµ Р±С‹Р»Рѕ вЂ” РєР»Р°РІРёС€Р° РЅРµ СЂР°Р±РѕС‚Р°Р»Р°
- Р’ РіР»Р°РІРЅРѕРј РјРµРЅСЋ Рё С‚СѓР»Р±Р°СЂРµ РѕС‚СЃСѓС‚СЃС‚РІРѕРІР°Р» РїСѓРЅРєС‚ В«Р’С‹СЂРµР·Р°С‚СЊВ» (Р±С‹Р»Рё С‚РѕР»СЊРєРѕ РљРѕРїРёСЂРѕРІР°С‚СЊ/Р’СЃС‚Р°РІРёС‚СЊ/РЈРґР°Р»РёС‚СЊ)

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- Р”РѕР±Р°РІР»РµРЅ `<KeyBinding Key="X" Modifiers="Control" Command="{Binding SelectedTab.CutSelectedCommand}"/>`
- Р’ РіР»Р°РІРЅРѕРµ РјРµРЅСЋ РґРѕР±Р°РІР»РµРЅ `MenuItem Header="Р’С‹_СЂРµР·Р°С‚СЊ"` СЃ РёРєРѕРЅРєРѕР№ `ContentCut` РјРµР¶РґСѓ РљРѕРїРёСЂРѕРІР°С‚СЊ Рё Р’СЃС‚Р°РІРёС‚СЊ
- Р’ С‚СѓР»Р±Р°СЂ РґРѕР±Р°РІР»РµРЅР° РєРЅРѕРїРєР° `Р’С‹СЂРµР·Р°С‚СЊ (Ctrl+X)` СЃ РёРєРѕРЅРєРѕР№ `ContentCut` РјРµР¶РґСѓ РљРѕРїРёСЂРѕРІР°С‚СЊ Рё Р’СЃС‚Р°РІРёС‚СЊ

**Р¤Р°Р№Р»:** `MainWindow.xaml:36,151-154,275-279`

### Fix S50-2: Re-paste bug (same instance added twice)

**РџСЂРѕР±Р»РµРјР°:** `GetClipboardContents()` РІРѕР·РІСЂР°С‰Р°Р» СЃСЃС‹Р»РєРё РЅР° С‚Рµ Р¶Рµ РѕР±СЉРµРєС‚С‹ РёР· `_clipboard`. РџРѕРІС‚РѕСЂРЅС‹Р№ Ctrl+V РґРѕР±Р°РІР»СЏР» С‚Рµ Р¶Рµ СЌРєР·РµРјРїР»СЏСЂС‹ РІ `Template.Objects` СЃРЅРѕРІР° вЂ” РѕР±СЉРµРєС‚ РѕРєР°Р·С‹РІР°Р»СЃСЏ РІ РєРѕР»Р»РµРєС†РёРё РґРІР°Р¶РґС‹.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `GetClipboardContents()` С‚РµРїРµСЂСЊ РєР»РѕРЅРёСЂСѓРµС‚ РѕР±СЉРµРєС‚С‹ РїСЂРё РєР°Р¶РґРѕРј РІС‹Р·РѕРІРµ, Р° РЅРµ РїСЂРё Copy:
```csharp
public IReadOnlyList<TemplateObjectBase> GetClipboardContents()
    => _clipboard.Select(o => o.Clone()).ToList().AsReadOnly();
```

**Р¤Р°Р№Р»:** `ClipboardManager.cs:27-28`

### Feature S50-3: Paste offset (10mm step)

**РџСЂРѕР±Р»РµРјР°:** Р’СЃС‚Р°РІР»РµРЅРЅС‹Рµ РѕР±СЉРµРєС‚С‹ РїРѕСЏРІР»СЏР»РёСЃСЊ С‚РѕС‡РЅРѕ РїРѕРІРµСЂС… РѕСЂРёРіРёРЅР°Р»РѕРІ вЂ” РёС… РЅРµ Р±С‹Р»Рѕ РІРёРґРЅРѕ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅРѕ СЃРјРµС‰РµРЅРёРµ РїСЂРё РІСЃС‚Р°РІРєРµ. РџРѕСЃР»Рµ Copy offset = 10РјРј. РљР°Р¶РґС‹Р№ РїРѕСЃР»РµРґСѓСЋС‰РёР№ Paste Р±РµР· Copy СѓРІРµР»РёС‡РёРІР°РµС‚ offset РµС‰С‘ РЅР° 10РјРј РїРѕ X Рё Y. РџСЂРё РїРѕРІС‚РѕСЂРЅРѕРј Copy offset СЃР±СЂР°СЃС‹РІР°РµС‚СЃСЏ.

**Р¤Р°Р№Р»:** `ClipboardManager.cs:10-14,22-23,34-42`

### Feature S50-4: BatchCommand РґР»СЏ Cut/Paste

**РџСЂРѕР±Р»РµРјР°:** РџСЂРё Cut/Paste 5 РѕР±СЉРµРєС‚РѕРІ СЃРѕР·РґР°РІР°Р»РѕСЃСЊ 5 РѕС‚РґРµР»СЊРЅС‹С… РєРѕРјР°РЅРґ РІ Undo-СЃС‚РµРєРµ. РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РЅР°Р¶РёРјР°Р» Ctrl+Z 5 СЂР°Р· РґР»СЏ РѕС‚РјРµРЅС‹ РѕРґРЅРѕРіРѕ РґРµР№СЃС‚РІРёСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РџСЂРё >1 РѕР±СЉРµРєС‚Рµ `PasteFromClipboard()` Рё `DeleteSelected()` СЃРѕР·РґР°СЋС‚ `BatchCommand`, РіСЂСѓРїРїРёСЂСѓСЋС‰РёР№ РІСЃРµ РѕРїРµСЂР°С†РёРё РІ РѕРґРЅСѓ Undo-РєРѕРјР°РЅРґСѓ:
- Paste: "Р’СЃС‚Р°РІРёС‚СЊ РѕР±СЉРµРєС‚С‹" (N РѕР±СЉРµРєС‚РѕРІ)
- Delete: "РЈРґР°Р»РёС‚СЊ РѕР±СЉРµРєС‚С‹" (N РѕР±СЉРµРєС‚РѕРІ)

**Р¤Р°Р№Р»С‹:** `EditorViewModel.cs:570-587,982-996`

### Feature S50-5: Auto-select pasted objects

**РџСЂРѕР±Р»РµРјР°:** РџРѕСЃР»Рµ Paste РІСЃС‚Р°РІР»РµРЅРЅС‹Рµ РѕР±СЉРµРєС‚С‹ РЅРµ РІС‹РґРµР»СЏР»РёСЃСЊ вЂ” РїРѕР»СЊР·РѕРІР°С‚РµР»СЊ РЅРµ РІРёРґРµР», С‡С‚Рѕ Р±С‹Р»Рѕ РґРѕР±Р°РІР»РµРЅРѕ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ РјРµС‚РѕРґ `SelectionManager.SelectObjects()`. `PasteFromClipboard()` РІС‹Р·С‹РІР°РµС‚ `_selectionManager.SelectObjects(clipboard)` РїРѕСЃР»Рµ Push РєРѕРјР°РЅРґ.

**Р¤Р°Р№Р»С‹:** `SelectionManager.cs:56-62`, `EditorViewModel.cs:586`

### Feature S50-6: StatusBar feedback

**РџСЂРѕР±Р»РµРјР°:** Copy/Paste/Cut РЅРµ РґР°РІР°Р»Рё РѕР±СЂР°С‚РЅРѕР№ СЃРІСЏР·Рё РІ СЃС‚СЂРѕРєРµ СЃРѕСЃС‚РѕСЏРЅРёСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅС‹ СЃРѕРѕР±С‰РµРЅРёСЏ РІ `StatusBarManager.StatusMessage`:
- Copy: "РЎРєРѕРїРёСЂРѕРІР°РЅРѕ: N РѕР±СЉРµРєС‚РѕРІ" / "РќРµС‚ РІС‹РґРµР»РµРЅРЅС‹С… РѕР±СЉРµРєС‚РѕРІ"
- Cut: "Р’С‹СЂРµР·Р°РЅРѕ: N РѕР±СЉРµРєС‚РѕРІ" / "РќРµС‚ РІС‹РґРµР»РµРЅРЅС‹С… РѕР±СЉРµРєС‚РѕРІ"
- Paste: "Р’СЃС‚Р°РІР»РµРЅРѕ: N РѕР±СЉРµРєС‚РѕРІ" / "Р‘СѓС„РµСЂ РѕР±РјРµРЅР° РїСѓСЃС‚"

Р”РѕР±Р°РІР»РµРЅ РІСЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ `GetObjectWord()` РґР»СЏ СЂСѓСЃСЃРєРёС… С‡РёСЃР»РёС‚РµР»СЊРЅС‹С… (РѕР±СЉРµРєС‚/РѕР±СЉРµРєС‚Р°/РѕР±СЉРµРєС‚РѕРІ).

**Р¤Р°Р№Р»:** `EditorViewModel.cs:557-587,1047-1053`

### Feature S50-7: Clipboard cleanup on tab close

**РџСЂРѕР±Р»РµРјР°:** РџСЂРё Р·Р°РєСЂС‹С‚РёРё РІРєР»Р°РґРєРё РѕР±СЉРµРєС‚С‹ РІ Р±СѓС„РµСЂРµ РѕР±РјРµРЅР° РјРѕРіР»Рё СЃСЃС‹Р»Р°С‚СЊСЃСЏ РЅР° СѓРґР°Р»С‘РЅРЅС‹Р№ С€Р°Р±Р»РѕРЅ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `ClipboardManager.Clear()` РІС‹Р·С‹РІР°РµС‚СЃСЏ РІ `EditorViewModel.Dispose()`.

**Р¤Р°Р№Р»С‹:** `ClipboardManager.cs:30`, `EditorViewModel.cs:1039`

### Fix S50-8: Ctrl+V РїРµСЂРµС…РІР°С‚С‹РІР°Р»СЃСЏ PreviewKeyDown (tool switcher)

**РџСЂРѕР±Р»РµРјР°:** `Window_PreviewKeyDown` РІ `MainWindow.xaml.cs:27` РѕР±СЂР°Р±Р°С‚С‹РІР°Р» `case Key.V` **Р±РµР· РїСЂРѕРІРµСЂРєРё РјРѕРґРёС„РёРєР°С‚РѕСЂРѕРІ**. РџСЂРё РЅР°Р¶Р°С‚РёРё `Ctrl+V` СЃРѕР±С‹С‚РёРµ РїРµСЂРµС…РІР°С‚С‹РІР°Р»РѕСЃСЊ, СѓСЃС‚Р°РЅР°РІР»РёРІР°Р»СЃСЏ `ActiveTool = "Select"` Рё `e.Handled = true`. `KeyBinding` РґР»СЏ Ctrl+V РІ `Window.InputBindings` РЅРёРєРѕРіРґР° РЅРµ РїРѕР»СѓС‡Р°Р» СЃРѕР±С‹С‚РёРµ вЂ” Paste РЅРµ СЂР°Р±РѕС‚Р°Р».

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅР° РїСЂРѕРІРµСЂРєР° `if (e.KeyboardDevice.Modifiers != ModifierKeys.None) break;` РґР»СЏ РІСЃРµС… tool-switching РєРµР№СЃРѕРІ (V, L, R, T). РўРµРїРµСЂСЊ Ctrl+V РґРѕС…РѕРґРёС‚ РґРѕ InputBindings.

**Р¤Р°Р№Р»:** `MainWindow.xaml.cs:27-58`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 1289+ РїСЂРѕР№РґРµРЅС‹ (0 failures)

**Common Mistakes (new):**
28. Re-paste bug вЂ” `GetClipboardContents()` must clone objects on EVERY call, not only during `Copy()`. If it returns references to the same cached instances, repeated Paste adds the same object to the collection. Always `_clipboard.Select(o => o.Clone())` in `GetClipboardContents()`. Paste offset counter must reset to `PasteOffsetStepMicrons` (not 0) after Copy, so the first paste already has an offset.
29. `PreviewKeyDown` for tool switching must check `e.KeyboardDevice.Modifiers != ModifierKeys.None` before handling V/L/R/T. Without the check, `Ctrl+V` (Paste), `Ctrl+L`, `Ctrl+R`, `Ctrl+T` get intercepted by the tool switcher and never reach their `Window.InputBindings`. Always add `if (modifiers != None) break;` at the start of each tool-switching case.
30. Panning without `CaptureMouse()` вЂ” if the mouse leaves the canvas during middle-button drag, `MouseMove` and `MouseUp` stop being delivered, panning freezes, and `IsPanning` never resets. Always call `canvas.CaptureMouse()` on pan start and `canvas.ReleaseMouseCapture()` on pan end.

## Sprint 51 вЂ” Panning mouse capture fix

### Fix S51-1: Panning breaks/corrupts when mouse leaves canvas during drag

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior.State_MouseDown()` РЅРµ РІС‹Р·С‹РІР°Р» `canvas.CaptureMouse()` РїСЂРё СЃС‚Р°СЂС‚Рµ РїР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёСЏ. Р‘РµР· Р·Р°С…РІР°С‚Р° РјС‹С€Рё:
- РџСЂРё РІС‹С…РѕРґРµ РєСѓСЂСЃРѕСЂР° Р·Р° РіСЂР°РЅРёС†Сѓ РєР°РЅРІР°СЃР° `MouseMove` РїРµСЂРµСЃС‚Р°С‘С‚ РґРѕСЃС‚Р°РІР»СЏС‚СЊСЃСЏ вЂ” РїР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёРµ Р·Р°РјРёСЂР°РµС‚
- `MouseUp` РІРЅРµ РєР°РЅРІР°СЃР° РЅРµ РґРѕС…РѕРґРёС‚ РґРѕ `State_MouseUp` вЂ” `IsPanning` РЅР°РІСЃРµРіРґР° `true`
- РџРѕСЃР»РµРґСѓСЋС‰РёР№ РєР»РёРє СЃСЂРµРґРЅРµР№ РєРЅРѕРїРєРѕР№ СЃР±СЂР°СЃС‹РІР°РµС‚ `IsPanning`, РЅРѕ РїРµСЂРІС‹Р№ `MouseMove` РїСЂРёРјРµРЅСЏРµС‚ Р±РѕР»СЊС€СѓСЋ РЅР°РєРѕРїР»РµРЅРЅСѓСЋ РґРµР»СЊС‚Сѓ вЂ” canvas В«РїСЂС‹РіР°РµС‚В»

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ `CaptureMouse()` / `ReleaseMouseCapture()` РІ С‚СЂС‘С… РјРµСЃС‚Р°С…:
- Middle button branch: `canvas.CaptureMouse()` РїРѕСЃР»Рµ `state.IsPanning = true`
- Space/Alt+Left branch: `canvas.CaptureMouse()` РїРѕСЃР»Рµ `state.IsPanning = true`
- Panning end РІ `State_MouseUp`: `canvas.ReleaseMouseCapture()` РїРµСЂРµРґ `e.Handled = true`

**Р¤Р°Р№Р»:** `Behaviors/EditorCanvasBehavior.cs:122,140,223`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** PanTool 13/13, ZoomPanManager 10/10, SelectTool 18/18 вЂ” РІСЃРµ РїСЂРѕР№РґРµРЅС‹

## Sprint 52 вЂ” Text improvements (fonts, immediate edit, free rotation)

### Fix S52-1: Font internal names mismatch

**РџСЂРѕР±Р»РµРјР°:** `FontNameToFamilyConverter` Рё `PreviewLineChangedBehavior` РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё URI-С„СЂР°РіРјРµРЅС‚С‹ `#GOST type A`/`#GOST type B`, РЅРѕ С„Р°РєС‚РёС‡РµСЃРєРёРµ РІРЅСѓС‚СЂРµРЅРЅРёРµ РёРјРµРЅР° вЂ” `GOST Type AU`/`GOST Type BU` (СЂРµРіРёСЃС‚СЂРѕР·Р°РІРёСЃРёРјС‹Рµ). РЁСЂРёС„С‚С‹ РЅРµ РѕС‚РѕР±СЂР°Р¶Р°Р»РёСЃСЊ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** URI РїСЂРёРІРµРґРµРЅС‹ Рє РїСЂР°РІРёР»СЊРЅС‹Рј РІРЅСѓС‚СЂРµРЅРЅРёРј РёРјРµРЅР°Рј:
- `#GOST type A` в†’ `#GOST Type AU`
- `#GOST type B` в†’ `#GOST Type BU`

**Р¤Р°Р№Р»С‹:** `Converters/FontNameToFamilyConverter.cs`, `Behaviors/PreviewLineChangedBehavior.cs`, `Resources/Fonts/README.md`

### Fix S52-2: Double-click opens inline editor

**РџРѕРІРµРґРµРЅРёРµ:** `SelectTool.OnDoubleClick()` РІС‹Р·С‹РІР°РµС‚ `StartInlineEditing(text)` РїСЂРё РґРІРѕР№РЅРѕРј РєР»РёРєРµ РЅР° С‚РµРєСЃС‚РѕРІС‹Р№ РѕР±СЉРµРєС‚. РЎРѕР·РґР°РЅРёРµ С‚РµРєСЃС‚Р° С‡РµСЂРµР· TextTool РќР• РѕС‚РєСЂС‹РІР°РµС‚ СЂРµРґР°РєС‚РѕСЂ вЂ” С‚РѕР»СЊРєРѕ РІС‹РґРµР»СЏРµС‚ РѕР±СЉРµРєС‚.

### Fix S52-3: Free rotation angle (0-359В°)

**РџСЂРѕР±Р»РµРјР°:** `RotationAngle` Р±С‹Р» РѕРіСЂР°РЅРёС‡РµРЅ `{0,90,180,270}`. `ContainsPoint()` Рё `GetBoundingBox()` вЂ” switch-case СЃ РЅРµРІРµСЂРЅРѕР№ РіРµРѕРјРµС‚СЂРёРµР№ РґР»СЏ 90В°/270В°.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- РЈРґР°Р»С‘РЅ `ValidRotationAngles`. РЎРµС‚С‚РµСЂ РЅРѕСЂРјР°Р»РёР·СѓРµС‚ `value % 360`
- `ContainsPoint()` / `GetBoundingBox()` вЂ” РѕР±С‰Р°СЏ РјР°С‚РµРјР°С‚РёРєР° С‡РµСЂРµР· `cos`/`sin`
- UI: ComboBox в†’ TextBox (РїСЂРѕРёР·РІРѕР»СЊРЅС‹Р№ РІРІРѕРґ РіСЂР°РґСѓСЃРѕРІ)
- InlineTextEditor: `LayoutTransform` СЃ `RotateTransform`
- `PropertiesViewModel`: СѓРґР°Р»С‘РЅ РІС‹Р·РѕРІ `ValidateRotation()`

**Common Mistakes (new):**
31. Rotation direction in WPF RotateTransform вЂ” WPF's `RotateTransform` rotates CLOCKWISE (Y-down screen space), which equals COUNTERCLOCKWISE in model Y-up space. `ContainsPoint` must compute `localX = dx*cos + dy*sin; localY = -dx*sin + dy*cos` with `angleRad = RotationAngle * PI / 180`.
32. `PreviewKeyDown` tool switching + InlineTextEditor вЂ” when inline editing is active, `Escape`/`Enter` must be intercepted by the TextBox InputBindings, NOT by Window PreviewKeyDown. The `CommitInlineEditingCommand`/`CancelInlineEditingCommand` handlers set `ActiveTool = "Select"` so subsequent PreviewKeyDown events go to select.
33. GostA.ttf internal name is `GOST Type AU` (not `GOST type A` or `GOST Type A`) вЂ” case-sensitive. Verify via `GlyphTypeface.FamilyNames`.
34. Text rotation center in WPF вЂ” `RotateTransform` rotates around the TextBlock's top-left corner, which is placed at the ContentPresenter's origin. The ContentPresenter top-left maps to model `(X, Y+H)` (the TOP of the text box), NOT the baseline `(X, Y)`. `GetBoundingBox()` and `ContainsPoint()` must rotate around `(X, Y+H)` in ContentPresenter-local space (Y-down), then convert back to model coordinates.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 165+ СЂРµР»РµРІР°РЅС‚РЅС‹С… РїСЂРѕР№РґРµРЅС‹ (РІСЃРµ РєР»СЋС‡РµРІС‹Рµ РєР°С‚РµРіРѕСЂРёРё)

## Sprint 53 вЂ” DateTimeProvider + MarkerPosition + Behaviour tests

### Feature S53-1: IDateTimeProvider (Р·Р°РјРµРЅР° Thread.Sleep)

**РџСЂРѕР±Р»РµРјР°:** `FileService.CreateBackup()`, `AutosaveService`, `TemplateService` РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё `DateTime.UtcNow` РЅР°РїСЂСЏРјСѓСЋ. РўРµСЃС‚С‹ РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё `Thread.Sleep` (СЃСѓРјРјР°СЂРЅРѕ ~2.3s) РґР»СЏ РіР°СЂР°РЅС‚РёРё СѓРЅРёРєР°Р»СЊРЅРѕСЃС‚Рё timestamp-РѕРІ, Р·Р°РјРµРґР»СЏСЏ С‚РµСЃС‚С‹ Рё РґРµР»Р°СЏ РёС… РЅРµРґРµС‚РµСЂРјРёРЅРёСЂРѕРІР°РЅРЅС‹РјРё.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- РЎРѕР·РґР°РЅ `Services/IDateTimeProvider.cs` (РёРЅС‚РµСЂС„РµР№СЃ: `DateTime UtcNow`)
- РЎРѕР·РґР°РЅ `Services/DateTimeProvider.cs` (СЂРµР°Р»РёР·Р°С†РёСЏ вЂ” РѕР±С‘СЂС‚РєР° РЅР°Рґ `DateTime.UtcNow`)
- Р’Рѕ РІСЃРµ 3 СЃРµСЂРІРёСЃР° РґРѕР±Р°РІР»РµРЅ РѕРїС†РёРѕРЅР°Р»СЊРЅС‹Р№ DI-РїР°СЂР°РјРµС‚СЂ `IDateTimeProvider? dateTimeProvider = null`
- `App.xaml.cs` вЂ” СЂРµРіРёСЃС‚СЂР°С†РёСЏ `services.AddSingleton<IDateTimeProvider, DateTimeProvider>()`
- Р’СЃРµ 12 СЃР»СѓС‡Р°РµРІ `DateTime.UtcNow` Р·Р°РјРµРЅРµРЅС‹ РЅР° `_dateTimeProvider.UtcNow`

**РўРµСЃС‚С‹:** Р’СЃРµ 5 С‚РµСЃС‚РѕРІС‹С… С„Р°Р№Р»РѕРІ РѕР±РЅРѕРІР»РµРЅС‹:
- `FileServiceTests` вЂ” `Mock<IDateTimeProvider>`, СЃС‚СЂРѕРєРё `Thread.Sleep` РёР· `CreateBackup_MultipleBackups_CreatesUniqueFiles` Рё `CreateBackup_OverwritesExistingBackup` СѓРґР°Р»РµРЅС‹, С‚РµСЃС‚С‹ РёСЃРїРѕР»СЊР·СѓСЋС‚ `SetupSequence`
- `AutosaveServiceTests` вЂ” РґРѕР±Р°РІР»РµРЅ `Mock<IDateTimeProvider>`
- `TemplateServiceTests` вЂ” `Mock<IDateTimeProvider>`, СѓРґР°Р»С‘РЅ `Thread.Sleep` РёР· `Save_UpdatesModifiedDate`
- `TemplateServiceRoundTripTests` вЂ” `Mock<IDateTimeProvider>`, СѓРґР°Р»С‘РЅ `Thread.Sleep` РёР· `Save_UpdatesModifiedDate`, `CreateTestTemplate` РёСЃРїРѕР»СЊР·СѓРµС‚ `FixedDate`
- `ExtendedServiceTests` вЂ” `Mock<IDateTimeProvider>`, СѓРґР°Р»С‘РЅ `Thread.Sleep` РёР· `Save_OverwritesExistingFile`, `CreateTestTemplate` РёСЃРїРѕР»СЊР·СѓРµС‚ `FixedDate`

### Feature S53-2: MarkerPosition attached behavior (СЃРѕРєСЂР°С‰РµРЅРёРµ XAML)

**РџСЂРѕР±Р»РµРјР°:** 14 РјР°СЂРєРµСЂРѕРІ РІС‹РґРµР»РµРЅРёСЏ (LineГ—2, RectangleГ—8, TextГ—4) Р·Р°РЅРёРјР°Р»Рё ~250 СЃС‚СЂРѕРє XAML СЃ РїРѕРІС‚РѕСЂСЏСЋС‰РёРјРёСЃСЏ MultiBinding-Р±Р»РѕРєР°РјРё `Canvas.Left`/`Canvas.Top`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРѕР·РґР°РЅ `Behaviors/MarkerPosition.cs` вЂ” РґРІР° attached properties:
- `XPropertyPath` (string) вЂ” РїСѓС‚СЊ Рє СЃРІРѕР№СЃС‚РІСѓ X-РєРѕРѕСЂРґРёРЅР°С‚С‹
- `YPropertyPath` (string) вЂ” РїСѓС‚СЊ Рє СЃРІРѕР№СЃС‚РІСѓ Y-РєРѕРѕСЂРґРёРЅР°С‚С‹

РџСЂРё СѓСЃС‚Р°РЅРѕРІРєРµ РѕР±РѕРёС… СЃРІРѕР№СЃС‚РІ СЃРѕР·РґР°С‘С‚ MultiBinding РґР»СЏ `Canvas.Left` (ModelXToCanvasLeftConverter + Zoom) Рё `Canvas.Top` (ModelYToCanvasTopConverter + HeightMm + Zoom) С‡РµСЂРµР· `FindAncestor UserControl`.

XAML РєР°Р¶РґРѕРіРѕ РјР°СЂРєРµСЂР° СЃРѕРєСЂР°С‰С‘РЅ СЃ 12 СЃС‚СЂРѕРє РґРѕ 2:
```xml
<Rectangle Style="{StaticResource SquareMarker}"
           behaviors:MarkerPosition.XPropertyPath="MicronsX"
           behaviors:MarkerPosition.YPropertyPath="BottomMicronsY"/>
```

**Р¤Р°Р№Р»С‹:**
- `Behaviors/MarkerPosition.cs` вЂ” РЅРѕРІС‹Р№ С„Р°Р№Р»
- `Views/EditorCanvas.xaml` вЂ” 14 РјР°СЂРєРµСЂРѕРІ РїРµСЂРµРїРёСЃР°РЅС‹ (~250в†’40 СЃС‚СЂРѕРє)

### Feature S53-3: Behaviour unit tests (pure functions)

**РџСЂРѕР±Р»РµРјР°:** `EditorCanvasBehavior` СЃРѕРґРµСЂР¶Р°Р» 3 РєРѕРЅРІРµСЂС‚Р°С†РёРё (MouseButton, ModifierKeys, Key) СЃ private-РјРµС‚РѕРґР°РјРё, РЅРµ РїРѕРєСЂС‹С‚С‹РјРё С‚РµСЃС‚Р°РјРё.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `ToToolMouseButton`, `ToToolModifiers`, `ToToolKey` вЂ” РёР·РјРµРЅРµРЅС‹ СЃ `private static` РЅР° `internal static`
- РЎРѕР·РґР°РЅ `Tests/Behaviors/EditorCanvasBehaviorTests.cs` СЃ 18 theory/fact-С‚РµСЃС‚Р°РјРё:
  - 5 С‚РµСЃС‚РѕРІ ToToolMouseButton (РІСЃРµ MouseButton + fallback)
  - 7 С‚РµСЃС‚РѕРІ ToToolModifiers (None/Ctrl/Shift/Alt/РєРѕРјР±РёРЅР°С†РёРё)
  - 6 С‚РµСЃС‚РѕРІ ToToolKey (Escape/Enter/Delete + unknown в†’ null)

**Common Mistakes (new):**
35. `Thread.Sleep` in tests вЂ” never use it for timestamp uniqueness. Create `IDateTimeProvider` interface and inject `Mock<IDateTimeProvider>` with `SetupSequence` for different return values.
36. XAML MultiBinding repetition for Canvas.Left/Canvas.Top вЂ” create an attached behavior (`MarkerPosition.XPropertyPath`/`YPropertyPath`) that auto-creates MultiBindings with the standard converters and FindAncestor. Reduces ~250 lines to ~40.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 77+ РЅРѕРІС‹С… С‚РµСЃС‚РѕРІ (FileService 19 + 5 РґРёР°Р»РѕРіРѕРІС‹С… РЅР° IDialogFileService, AutosaveService 1, TemplateService 7, RoundTrip 12, Extended 5, EditorCanvasBehavior 18, EditorViewModel 15) вЂ” РІСЃРµ РїСЂРѕР№РґРµРЅС‹

## Sprint 54 вЂ” IDialogFileService (РёР·РѕР»СЏС†РёСЏ WPF-РґРёР°Р»РѕРіРѕРІ)

### Feature S54-1: IDialogFileService (Р·Р°РјРµРЅР° OpenFileDialog/SaveFileDialog)

**РџСЂРѕР±Р»РµРјР°:** `FileService.OpenFileDialog()` Рё `SaveFileDialog()` РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё РЅР°РїСЂСЏРјСѓСЋ WPF `OpenFileDialog`/`SaveFileDialog`. Р’ РіРѕР»РѕРІРЅРѕР№ СЃСЂРµРґРµ (CI, headless) `ShowDialog()` Р·Р°РІРёСЃР°РµС‚ вЂ” С‚РµСЃС‚С‹ РЅРµ РјРѕРіР»Рё Р±С‹С‚СЊ Р·Р°РїСѓС‰РµРЅС‹ РІ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРёС… РїР°Р№РїР»Р°Р№РЅР°С…. Р¤РёР»СЊС‚СЂ xUnit РЅРµ РїРѕРґРґРµСЂР¶РёРІР°Р» `not`-РёСЃРєР»СЋС‡РµРЅРёРµ РґР»СЏ СЌС‚РёС… С‚РµСЃС‚РѕРІ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- РЎРѕР·РґР°РЅ `Services/IDialogFileService.cs` (РёРЅС‚РµСЂС„РµР№СЃ: `OpenFileDialog`, `SaveFileDialog`)
- РЎРѕР·РґР°РЅ `Services/WpfDialogFileService.cs` (СЂРµР°Р»РёР·Р°С†РёСЏ вЂ” РїРµСЂРµРЅРµСЃС‘РЅ РєРѕРґ WPF-РґРёР°Р»РѕРіРѕРІ РёР· FileService)
- `FileService` РїСЂРёРЅРёРјР°РµС‚ РѕРїС†РёРѕРЅР°Р»СЊРЅС‹Р№ `IDialogFileService? dialogService = null` (fallback РЅР° `WpfDialogFileService(logger)`)
- `App.xaml.cs` вЂ” СЂРµРіРёСЃС‚СЂР°С†РёСЏ `services.AddSingleton<IDialogFileService, WpfDialogFileService>()`
- Р’СЃРµ 5 С‚РµСЃС‚РѕРІ РґРёР°Р»РѕРіРѕРІ РїРµСЂРµРїРёСЃР°РЅС‹: РёСЃРїРѕР»СЊР·СѓСЋС‚ `Mock<IDialogFileService>` (Verify С„РёР»СЊС‚СЂР°/РёРјРµРЅРё С„Р°Р№Р»Р° + РІРѕР·РІСЂР°С‰Р°РµРјРѕРµ Р·РЅР°С‡РµРЅРёРµ), РЅРёРєР°РєРёС… РІС‹Р·РѕРІРѕРІ `ShowDialog()` РІ headless

**Р¤Р°Р№Р»С‹:**
- `Services/IDialogFileService.cs` вЂ” РЅРѕРІС‹Р№ РёРЅС‚РµСЂС„РµР№СЃ
- `Services/WpfDialogFileService.cs` вЂ” РЅРѕРІР°СЏ СЂРµР°Р»РёР·Р°С†РёСЏ
- `Services/FileService.cs` вЂ” DI + РґРµР»РµРіРёСЂРѕРІР°РЅРёРµ (СЃС‚СЂРѕРєРё 13-41)
- `App.xaml.cs:64` вЂ” СЂРµРіРёСЃС‚СЂР°С†РёСЏ РІ DI
- `Tests/Services/FileServiceTests.cs` вЂ” 5 С‚РµСЃС‚РѕРІ СЃ Mock

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 19/19 FileServiceTests вЂ” РІСЃРµ РїСЂРѕР№РґРµРЅС‹

**Common Mistakes (new):**
37. WPF dialogs (OpenFileDialog/SaveFileDialog) must NOT be used directly in services that need CI/testability. Always extract to `IDialogFileService` interface + `WpfDialogFileService` implementation, inject as optional `= null` parameter. Tests use `Mock<IDialogFileService>` returning null вЂ” zero UI calls in headless.

## Sprint 55 вЂ” Unit test coverage for managers + SelectTool

### Feature S55-1: ToolManagerTests вЂ” 17 tests

**Р¤Р°Р№Р»:** `Tests/ViewModels/Managers/ToolManagerTests.cs` (РЅРѕРІС‹Р№)

**РџСЂРѕС‚РµСЃС‚РёСЂРѕРІР°РЅРЅС‹Рµ СЃС†РµРЅР°СЂРёРё:**
- Constructor: defaults (ActiveTool="Select"), null logger guard
- GetOrCreateTool<T>: creates new, returns cached, different types, unknown type throws
- ActiveTool setter + PropertyChanged
- PushTool/PopTool: stack behaviour, Pop on empty returns null
- ResetTool: existing, unknown, not-cached

### Feature S55-2: DirtyStateManagerTests вЂ” 16 tests

**Р¤Р°Р№Р»:** `Tests/ViewModels/Managers/DirtyStateManagerTests.cs` (РЅРѕРІС‹Р№)

**РџСЂРѕС‚РµСЃС‚РёСЂРѕРІР°РЅРЅС‹Рµ СЃС†РµРЅР°СЂРёРё:**
- Constructor: null template guard
- Defaults: IsDirty=false, FilePath=null, DisplayName=""
- MarkDirty: sets IsDirty, idempotent, PropertyChanged
- ClearDirty: PropertyChanged
- UpdateDisplayName: with/without FilePath, Portrait/Landscape
- FilePath setter

### Feature S55-3: GridManagerTests вЂ” 24 tests

**Р¤Р°Р№Р»:** `Tests/ViewModels/Managers/GridManagerTests.cs` (РЅРѕРІС‹Р№)

**РџСЂРѕС‚РµСЃС‚РёСЂРѕРІР°РЅРЅС‹Рµ СЃС†РµРЅР°СЂРёРё:**
- Constructor: 3Г— null guard (template, zoomPanManager, logger)
- ToggleGrid / ToggleSnap
- IsGridEnabled / IsSnapEnabled get/set
- GridStepMm / GridStepMicrons conversion
- RefreshGridNodes: disabled, not visible, MinPixelSpacing, centered, not centered, callback, node validation

### Feature S55-4: ZoomPanManagerExtendedTests вЂ” 28 tests

**Р¤Р°Р№Р»:** `Tests/ViewModels/Managers/ZoomPanManagerExtendedTests.cs` (РЅРѕРІС‹Р№)

**РџСЂРѕС‚РµСЃС‚РёСЂРѕРІР°РЅРЅС‹Рµ СЃС†РµРЅР°СЂРёРё:**
- IsCentered: viewport > canvas, smaller, zero
- CanvasWidth/HeightPixels: zoom scaling
- ViewportWidth/HeightPixels
- ScrollXRange/YRange: zero when centered, positive when not
- ScrollXValue/YValue
- SetScrollX/Y: centered в†’ no-op, not-centered в†’ clamp + pan offset
- CanvasOffsetX/Y
- CenterCanvas, SetGridRefreshCallback, PanCanvas
- PropertyChanged for dependent properties

### Feature S55-5: ClipboardManager + SelectionManager вЂ” 13 tests

**Р¤Р°Р№Р»:** `Tests/ViewModels/Managers/ManagerTests.cs` (РґРѕРїРѕР»РЅРµРЅ)

**ClipboardManager:**
- Cut: copies + calls delete action (single, multiple, empty)
- Clear
- GetClipboardContents: clones + offset, offset increment, offset reset after Copy

**SelectionManager:**
- SelectObjects: clears previous, empty в†’ clears, multiple
- IsObjectSelected: true/false/removed/empty
- Constructor fires onSelectionChanged callback
- SelectAll

### Feature S55-6: SelectToolExtendedTests вЂ” 22 tests

**Р¤Р°Р№Р»:** `Tests/Tools/SelectToolExtendedTests.cs` (РЅРѕРІС‹Р№)

**РџСЂРѕС‚РµСЃС‚РёСЂРѕРІР°РЅРЅС‹Рµ СЃС†РµРЅР°СЂРёРё:**
- OnDoubleClick: textв†’inline, lineв†’noop, rectв†’noop, emptyв†’noop
- OnKeyDown: Delete (single/multi/empty/undoable), Escape (clears state + Reset), unknown key в†’ false
- SelectionBox: start, <threshold, >threshold, direction, finalize select, small-move clear
- Reset: clears drag state
- Cursor: hand on hover, cross on handle, arrow by default

### Feature S55-7: Behavior tests removed (STA requirement)

**РџСЂРѕР±Р»РµРјР°:** 9 С‚РµСЃС‚РѕРІ РґР»СЏ WPF attached-property get/set (MarkerPosition, TextBoxLostFocusCommandBehavior, ComboBoxSelectionChangedCommandBehavior, ZoomComboBoxBehavior, TabItemMiddleClickBehavior) СЃРѕР·РґР°РІР°Р»Рё WPF-СЌР»РµРјРµРЅС‚С‹ (TextBox/ComboBox/TabControl), С‡С‚Рѕ С‚СЂРµР±СѓРµС‚ STA thread. xUnit runner РёСЃРїРѕР»СЊР·СѓРµС‚ MTA в†’ `InvalidOperationException`.

**Р РµС€РµРЅРёРµ:** Р¤Р°Р№Р» `BehaviorAttachedPropertyTests.cs` СѓРґР°Р»С‘РЅ. РџРѕРІРµРґРµРЅРёСЏ РѕСЃС‚Р°СЋС‚СЃСЏ Р±РµР· unit-РїРѕРєСЂС‹С‚РёСЏ вЂ” С‚СЂРµР±СѓСЋС‚ integration/UI С‚РµСЃС‚РѕРІ СЃ STA-РёРЅС„СЂР°СЃС‚СЂСѓРєС‚СѓСЂРѕР№.

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 1599 (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
38. WPF DependencyProperty tests require STA thread вЂ” creating WPF elements (`TextBox`, `ComboBox`, `TabControl`) in xUnit tests without STA causes `InvalidOperationException`. Use `[WpfFact]` attribute or STA collection fixture. Pure DP registration (without creating owner elements) may work in MTA.

## Sprint 56 вЂ” Colors (StrokeColor/FillColor/Foreground + V-005)

### Feature S56-1: StrokeColor, FillColor, Foreground

**РџСЂРѕР±Р»РµРјР°:** Line Рё Rectangle РЅРµ РёРјРµР»Рё StrokeColor, Rectangle РЅРµ РёРјРµР» FillColor, Text РЅРµ РёРјРµР» Foreground. Р¦РІРµС‚Р° Р±С‹Р»Рё С„РёРєСЃРёСЂРѕРІР°РЅРЅС‹Рј С‡С‘СЂРЅС‹Рј.

**РСЃРїСЂР°РІР»РµРЅРёРµ (end-to-end):**
- `EditorConstants.cs` вЂ” `DefaultStrokeColor = "#000000"`, `DefaultFillColor = "Transparent"`, `DefaultForeground = "#000000"`
- `Line.cs` вЂ” `StrokeColor` СЃ INPC + backing field
- `Rectangle.cs` вЂ” `StrokeColor` + `FillColor` СЃ INPC
- `Text.cs` вЂ” `Foreground` СЃ INPC
- `TemplateDto.cs` / `TemplateService.cs` вЂ” РјР°РїРїРёРЅРі РІСЃРµС… С†РІРµС‚РѕРІ (DTO в†” Model)
- `HexToBrushConverter` вЂ” `#RRGGBB`, `#AARRGGBB`, `"Transparent"` в†’ `SolidColorBrush`
- `PropertiesViewModel` вЂ” +6 СЃРІРѕР№СЃС‚РІ С†РІРµС‚Р° +4 РєРѕРјР°РЅРґС‹ РёР·РјРµРЅРµРЅРёСЏ
- `PropertiesPanelContent.xaml` вЂ” ColorPicker UI СЃ Hex-РїРѕР»РµРј Рё РІС‹Р±РѕСЂРѕРј Transparent
- `EditorCanvas.xaml` вЂ” DataTemplate Р±РёРЅРґРёРЅРіРё С‡РµСЂРµР· Style Setter (Stroke/Fill/Foreground)
- `ValidationService` вЂ” V-005: `ValidateHexColor()` вЂ” РїСЂРѕРІРµСЂРєР° С„РѕСЂРјР°С‚Р° HEX + Transparent
- `DrawingLineTool.cs` / `DrawingRectangleTool.cs` / `TextTool.cs` вЂ” С†РІРµС‚Р° РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ
- `+36 С‚РµСЃС‚РѕРІ` (Line/Rectangle/Text С†РІРµС‚Р°, Converter, Validation, RoundTrip)

**Build:** 0 errors, 0 warnings
**Tests:** 1639 passed (0 failures, 1 pre-existing skip)

## Sprint 57 вЂ” MultiLine, Half-formats, Library UI, Settings, Documentation

### Feature S57-1: MultiLine + TextAlignment (FR-032)

**РџСЂРѕР±Р»РµРјР°:** Text РЅРµ РїРѕРґРґРµСЂР¶РёРІР°Р» РјРЅРѕРіРѕСЃС‚СЂРѕС‡РЅС‹Р№ С‚РµРєСЃС‚ Рё РІС‹СЂР°РІРЅРёРІР°РЅРёРµ. InlineTextEditor РЅРµ РёРјРµР» AcceptsReturn.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `Text.cs` вЂ” `TextWrapping` (bool) + `TextAlignment` (string: "Left"/"Center"/"Right") СЃ INPC
- `BoolToTextWrappingConverter` вЂ” bool в†’ TextWrapping
- `StringToTextAlignmentConverter` вЂ” string в†’ TextAlignment
- `TextAlignmentToIndexConverter` вЂ” string в†’ int (ComboBox SelectedIndex)
- `EditorCanvas.xaml` вЂ” TextBlock Р±РёРЅРґРёРЅРіРё TextWrapping/TextAlignment
- `InlineTextEditor` вЂ” AcceptsReturn=True РїСЂРёРІСЏР·Р°РЅ Рє TextWrapping; Ctrl+Enter в†’ commit, Enter в†’ РЅРѕРІР°СЏ СЃС‚СЂРѕРєР°
- `PropertiesViewModel` вЂ” +TextTextWrapping/TextTextAlignment + relay-РєРѕРјР°РЅРґС‹
- `PropertiesPanelContent.xaml` вЂ” ComboBox РІС‹СЂР°РІРЅРёРІР°РЅРёСЏ, CheckBox РїРµСЂРµРЅРѕСЃР° СЃС‚СЂРѕРє
- `+22 С‚РµСЃС‚Р°`

### Feature S57-2: Half-formats (A4Г—2, A3Г—2, A2Г—2, A1Г—2, A0Г—2)

**РџСЂРѕР±Р»РµРјР°:** РўСЂРµР±РѕРІР°Р»РёСЃСЊ С„РѕСЂРјР°С‚С‹ СЃ СѓРґРІРѕРµРЅРЅРѕР№ РґР»РёРЅРЅРѕР№ СЃС‚РѕСЂРѕРЅРѕР№ РґР»СЏ С‡РµСЂС‚РµР¶РµР№.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `Sheet.FromFormat()` вЂ” +5 С„РѕСЂРјР°С‚РѕРІ: 210Г—594вЂ¦841Г—2378 РјРј, РІСЃРµ Portrait РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ
- `Sheet.GetDefaultOrientation()` вЂ” Г—2 С„РѕСЂРјР°С‚С‹ в†’ Portrait
- `ValidationService.ValidFormats` вЂ” +10 entry (Г—2/X2 РґР»СЏ РєР°Р¶РґРѕРіРѕ С„РѕСЂРјР°С‚Р° Г— P/L)
- `MainWindow.xaml` вЂ” РїРѕРґРјРµРЅСЋ РІ File > New РґР»СЏ half-С„РѕСЂРјР°С‚РѕРІ
- `+25 С‚РµСЃС‚РѕРІ`

### Feature S57-3: Р‘РёР±Р»РёРѕС‚РµРєР° С€Р°Р±Р»РѕРЅРѕРІ (FR-043)

**РџСЂРѕР±Р»РµРјР°:** РљРЅРѕРїРєРё Import/Remove РІ TemplateLibraryViewModel СЃСѓС‰РµСЃС‚РІРѕРІР°Р»Рё РєР°Рє РєРѕРјР°РЅРґС‹, РЅРѕ РЅРµ Р±С‹Р»Рё РїСЂРёРІСЏР·Р°РЅС‹ РІ XAML.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `MainViewModel.cs` вЂ” РїРµСЂРµРґР°С‡Р° `IFileService` РІ `TemplateLibraryViewModel`
- `MainWindow.xaml` вЂ” С‚СѓР»Р±Р°СЂ СЃ РєРЅРѕРїРєР°РјРё В«РРјРїРѕСЂС‚В» / В«РЈРґР°Р»РёС‚СЊВ» РІ Р»РµРІРѕР№ РїР°РЅРµР»Рё

### Feature S57-4: РќР°СЃС‚СЂРѕР№РєРё (UI)

**РџСЂРѕР±Р»РµРјР°:** РћС‚СЃСѓС‚СЃС‚РІРѕРІР°Р» РіСЂР°С„РёС‡РµСЃРєРёР№ РёРЅС‚РµСЂС„РµР№СЃ РґР»СЏ РёР·РјРµРЅРµРЅРёСЏ РЅР°СЃС‚СЂРѕРµРє РїСЂРёР»РѕР¶РµРЅРёСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `SettingsViewModel` вЂ” Theme, ShowGrid, SnapToGrid, GridStepMm, AutosaveIntervalMinutes, DefaultSheetFormat, DefaultZoom
- `SettingsView.xaml` + `.cs` вЂ” РјРѕРґР°Р»СЊРЅРѕРµ РѕРєРЅРѕ 420Г—440 СЃ 4 СЃРµРєС†РёСЏРјРё, РЎРѕС…СЂР°РЅРёС‚СЊ/РћС‚РјРµРЅР°
- `WpfDialogHostService` вЂ” dispatch SettingsViewModel в†’ SettingsView
- `MainViewModel` вЂ” +OpenSettingsCommand
- `MainWindow.xaml` вЂ” РїСѓРЅРєС‚ В«РќР°СЃС‚СЂРѕР№РєРё...В» РІ РјРµРЅСЋ File
- `+6 С‚РµСЃС‚РѕРІ`

### Fix S57-5: Р”РѕРєСѓРјРµРЅС‚Р°С†РёСЏ

**РћР±РЅРѕРІР»РµРЅРѕ:**
- `02_User_Stories_Р­С‚Р°Рї1.md` вЂ” 122 С‡РµРєР±РѕРєСЃР° в†’ вњ…
- `19_РЎС‚Р°С‚СѓСЃ_РїСЂРѕРµРєС‚Р°.md` вЂ” 1760 С‚РµСЃС‚РѕРІ, Sprint 57 РІ РґРёРЅР°РјРёРєРµ
- `05_Р СѓРєРѕРІРѕРґСЃС‚РІРѕ_РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ_С‡РµСЂРЅРѕРІРёРє.md` вЂ” СЂР°Р·РґРµР» 10 РїРµСЂРµРїРёСЃР°РЅ (Settings), С…РѕС‚РєРµР№ V/L/R/T/E
- `docs/archive/` вЂ” 82 СѓСЃС‚Р°СЂРµРІС€РёС… С„Р°Р№Р»Р° РїРµСЂРµРјРµС‰РµРЅС‹
- `AGENTS.md` вЂ” РґРѕР±Р°РІР»РµРЅС‹ Sprint 56-57, РїСѓС‚Рё Рє Р°СЂС…РёРІСѓ РѕР±РЅРѕРІР»РµРЅС‹

**Build:** 0 errors, 0 warnings
**Tests:** 1760 passed (0 failures, 1 pre-existing skip)

## Sprint STA вЂ” Unit tests for WPF behaviors (STA-thread)

### Feature STA-1: WpfContext helper
РЎРѕР·РґР°РЅ `Tests/Helpers/WpfContext.cs` вЂ” STA-thread dispatcher. РЎРѕР·РґР°С‘С‚ РїРѕС‚РѕРє СЃ `ApartmentState.STA`, СѓСЃС‚Р°РЅР°РІР»РёРІР°РµС‚ `DispatcherSynchronizationContext`, РІС‹РїРѕР»РЅСЏРµС‚ action Рё Р·Р°РІРµСЂС€Р°РµС‚ `Dispatcher`.

### Feature STA-2: Behavior handlers в†’ internal static
4 С„Р°Р№Р»Р° РёР·РјРµРЅРµРЅС‹ вЂ” `private static` handlers в†’ `internal static`:
- `TextBoxLostFocusCommandBehavior.OnLostFocus` / `OnKeyDown`
- `ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged`
- `ZoomComboBoxBehavior.OnSelectionChanged` / `OnDropDownClosed` / `ApplyZoom`

РџР°С‚С‚РµСЂРЅ СѓР¶Рµ РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РІ `CanvasInputRouter` (Sprint 53).

### Feature STA-3: TextBoxLostFocusCommandBehaviorTests вЂ” 14 С‚РµСЃС‚РѕРІ

| РљР°С‚РµРіРѕСЂРёСЏ | РўРµСЃС‚С‹ |
|-----------|-------|
| DP get/set | Set, Get, Clear (3) |
| OnLostFocus | Execute, CanExecute=false, null command, non-TextBox sender (4) |
| OnKeyDown | Enter execute + Handled, non-Enter skip, CanExecute=false, null command, non-TextBox sender (7) |

### Feature STA-4: ComboBoxSelectionChangedCommandBehaviorTests вЂ” 10 С‚РµСЃС‚РѕРІ

| РљР°С‚РµРіРѕСЂРёСЏ | РўРµСЃС‚С‹ |
|-----------|-------|
| DP get/set | Set, Get, Clear, non-ComboBox (4) |
| OnSelectionChanged | Execute, CanExecute=false, null command, non-ComboBox sender, null SelectedItem (6) |

### Feature STA-5: ZoomComboBoxBehaviorTests вЂ” 11 С‚РµСЃС‚РѕРІ

| РљР°С‚РµРіРѕСЂРёСЏ | РўРµСЃС‚С‹ |
|-----------|-------|
| DP get/set (DependencyObject) | Set, Get, Clear (3) |
| DP get/set (ComboBox) | Set (1) |
| ApplyZoom | Percent, plain, invalid, zero/negative, no-editor, spaces (6) |
| Events | SelectionChanged, DropDownClosed (2) |

`EditorViewModel` вЂ” real instance (not mock) via `ITemplateService`/`IPrintService`. Verify via `editor.ZoomPanManager.Zoom`.

### Feature STA-6: MarkerPositionTests вЂ” 10 С‚РµСЃС‚РѕРІ

DP get/set РґР»СЏ `XPropertyPath` Рё `YPropertyPath` (DependencyObject, null/storage, independence).

**РџСЂРѕРїСѓС‰РµРЅРѕ (РёСЃС‚РѕСЂРёС‡РµСЃРєРё):** `TabItemMiddleClickBehavior`, `PreviewLineChangedBehavior` вЂ” С‚СЂРµР±РѕРІР°Р»Рё РїРѕР»РЅРѕРіРѕ РІРёР·СѓР°Р»СЊРЅРѕРіРѕ РґРµСЂРµРІР°. **Р РµС€РµРЅРѕ РІ Sprint 62** (23 STA-С‚РµСЃС‚Р°, 12 + 11).

**Build:** 0 errors, 0 warnings
**Tests:** 1780 passed (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
39. WPF `Control` constructor requires STA вЂ” `new ComboBox()`, `new TextBox()`, `new Button()` throw `InvalidOperationException` on MTA. Always create WPF elements inside an STA thread (via `WpfContext.Execute`).
40. Moq cannot mock non-virtual methods вЂ” `SetZoomPercent` is not virtual в†’ use real `EditorViewModel` instance and verify via `editor.Zoom` instead of `mock.Verify`.
41. `Mock<T>(MockBehavior, params object[] args)` with nullable reference types вЂ” passing `(GridSettings?)null` to `object[]` triggers CS8625/CS8604. Use a `GridSettings?` local variable set to `null` or `null!`.
42. `PresentationSource` in .NET 10 WPF вЂ” the abstract class requires `GetCompositionTargetCore()`, `RootVisual` getter/setter, and `IsDisposed`. `GetVisualRoot()` no longer exists. Create `FakePresentationSource` implementing all abstract members.

## Sprint R3.1 вЂ” EditorViewModel РґРµ-bloat (forwarding-СЃРІРѕР№СЃС‚РІР° в†’ РјРµРЅРµРґР¶РµСЂС‹)

### Р§С‚Рѕ СЃРґРµР»Р°РЅРѕ

**РџСЂРѕР±Р»РµРјР°:** EditorViewModel СЃРѕРґРµСЂР¶Р°Р» ~60 forwarding-СЃРІРѕР№СЃС‚РІ, РґСѓР±Р»РёСЂСѓСЋС‰РёС… СЃРІРѕР№СЃС‚РІР° РјРµРЅРµРґР¶РµСЂРѕРІ (ZoomPanManager, PreviewManager, StatusBarManager Рё РґСЂ.). РљР°Р¶РґРѕРµ СЃРІРѕР№СЃС‚РІРѕ РёРјРµР»Рѕ `OnPropertyChanged()` РІ СЃРµС‚С‚РµСЂРµ РґР»СЏ СЂРµС‚СЂР°РЅСЃР»СЏС†РёРё СѓРІРµРґРѕРјР»РµРЅРёР№ РЅР° EditorViewModel. Р РµС‚СЂР°РЅСЃР»СЏС†РёСЏ С‚СЂРµР±РѕРІР°Р»Р°СЃСЊ, РєРѕРіРґР° XAML Р±РёРЅРґРёР»СЃСЏ Рє EditorViewModel, РЅРѕ РїРѕСЃР»Рµ R3.1 XAML СѓР¶Рµ Р±РёРЅРґРёР»СЃСЏ РЅР°РїСЂСЏРјСѓСЋ Рє РјРµРЅРµРґР¶РµСЂР°Рј вЂ” forwarding СЃС‚Р°Р» РјС‘СЂС‚РІС‹Рј РіСЂСѓР·РѕРј. Р”РѕРїРѕР»РЅРёС‚РµР»СЊРЅРѕ 4 РѕР±СЂР°Р±РѕС‚С‡РёРєР° `PropertyChanged` РїРѕРґРїРёСЃС‹РІР°Р»РёСЃСЊ РЅР° РјРµРЅРµРґР¶РµСЂРѕРІ Рё РїРµСЂРµ-РѕРїРѕРІРµС‰Р°Р»Рё EditorViewModel.

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- РЈРґР°Р»РµРЅС‹ ~25 forwarding-СЃРІРѕР№СЃС‚РІ (С‚Рµ, С‡С‚Рѕ РЅРµ С‚СЂРµР±СѓСЋС‚СЃСЏ IEditorContext):
  - `CanvasWidthPixels`, `CanvasHeightPixels`, `PanOffsetX/Y`, `ZoomPercent`, `ViewportWidth/HeightMm`, `ViewportWidth/HeightPixels`, `ScrollX/YRange`, `ScrollX/YValue`, `IsCentered`, `CanvasOffsetX/Y`
  - `ShowSelectionMarkers`, `GridNodes`, `GridInvalidated`
  - `StatusBarSheetFormat`, `StatusBarGridEnabled`, `StatusBarGridStepMm`, `StatusBarSnapEnabled`
  - `ActiveTool`, `InlineEditingText`, `InlineEditText`
- РЈРїСЂРѕС‰РµРЅС‹ ~15 СЃРІРѕР№СЃС‚РІ IEditorContext (СѓР±СЂР°РЅС‹ `OnPropertyChanged()`):
  - `PreviewLine`, `PreviewRectangle`, `PreviewText`, `SelectionBoxLeft/Bottom/Top/Width/Right/Height`, `SelectionDirection`, `StatusMessage`, `Zoom`
- РЈРґР°Р»РµРЅС‹ 4 РїРѕР»СЏ-РѕР±СЂР°Р±РѕС‚С‡РёРєР°, 4 РїРѕРґРїРёСЃРєРё `PropertyChanged` РІ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂРµ, 4 РѕС‚РїРёСЃРєРё РІ `Dispose`
- `OnZoomChangedInternal` СѓРґР°Р»С‘РЅ (Р·Р°РјРµРЅС‘РЅ РЅР° `() => { }`)
- `IAutosaveTab` (`IsDirty`, `FilePath`, `DisplayName`) вЂ” explicit interface implementation
- `OnSelectionChangedInternal` СѓРїСЂРѕС‰С‘РЅ (СѓР±СЂР°РЅС‹ `ShowSelectionMarkers`, `SingleSelectedObject`)
- `PreviewLineChangedBehavior` РїРµСЂРµРїРёСЃР°РЅ РЅР° `PreviewManager.PropertyChanged`
- ~90 С‚РµСЃС‚РѕРІ РёСЃРїСЂР°РІР»РµРЅС‹ РЅР° manager-СЃРІРѕР№СЃС‚РІР°

**Р РµР·СѓР»СЊС‚Р°С‚:**
```
EditorViewModel: ~1194 в†’ 784 СЃС‚СЂРѕРє (в€’410, в€’34%)
Build:  0 errors, 0 warnings
Tests:  1780 passed, 1 skip
```

**Р¤Р°Р№Р»С‹:**
- `ViewModels/EditorViewModel.cs` вЂ” РѕСЃРЅРѕРІРЅРѕР№ С„Р°Р№Р» СЂРµС„Р°РєС‚РѕСЂРёРЅРіР°
- `Behaviors/PreviewLineChangedBehavior.cs` вЂ” РїРµСЂРµРїРёСЃР°РЅ РЅР° PreviewManager
- `MainViewModel.cs` вЂ” 9 Р·Р°РјРµРЅ (DirtyStateManager)
- `EditorCanvas.xaml` / `.xaml.cs` вЂ” 7 Р·Р°РјРµРЅ (ZoomPanManager, GridManager)
- `CanvasInputRouter.cs` вЂ” 2 Р·Р°РјРµРЅС‹
- `EditorViewModelTests.cs` вЂ” ~90 РёСЃРїСЂР°РІР»РµРЅРёР№

## Sprint R3.1-HF1 вЂ” Preview fix (unconditional PropertyChanged)

**РџСЂРѕР±Р»РµРјР°:** `[ObservableProperty]` РЅР° `PreviewLine`/`PreviewRectangle`/`PreviewText` РІ `PreviewManager` РїРѕРґР°РІР»СЏР» `PropertyChanged` РїСЂРё re-assign С‚РѕР№ Р¶Рµ СЃСЃС‹Р»РєРё (`EqualityComparer<T>.Default.Equals()` РґР»СЏ reference-С‚РёРїРѕРІ = `ReferenceEquals`). РўСЂРё РёРЅСЃС‚СЂСѓРјРµРЅС‚Р° (DrawingLineTool, DrawingRectangleTool, TextTool) РјСѓС‚РёСЂСѓСЋС‚ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РёР№ preview-РѕР±СЉРµРєС‚ Рё РїРµСЂРµСѓСЃС‚Р°РЅР°РІР»РёРІР°СЋС‚ РµРіРѕ вЂ” PropertyChanged РЅРµ СЃС‚СЂРµР»СЏРµС‚, `PreviewLineChangedBehavior` РЅРµ РѕР±РЅРѕРІР»СЏРµС‚ WPF-СЌР»РµРјРµРЅС‚С‹, РїСЂРµРґРїСЂРѕСЃРјРѕС‚СЂ РїСЂРѕРїР°РґР°РµС‚.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `[ObservableProperty]` Р·Р°РјРµРЅС‘РЅ РЅР° СЂСѓС‡РЅС‹Рµ СЃРµС‚С‚РµСЂС‹ СЃ Р±РµР·СѓСЃР»РѕРІРЅС‹Рј `OnPropertyChanged()` РґР»СЏ С‚СЂС‘С… РїРѕР»РµР№. SelectionBox-РїРѕР»СЏ (`long`, `byte`) РЅРµ С‚СЂРѕРЅСѓС‚С‹ вЂ” equality check РґР»СЏ value-С‚РёРїРѕРІ РєРѕСЂСЂРµРєС‚РµРЅ.

**Р¤Р°Р№Р»:** `ViewModels/Managers/PreviewManager.cs` (3 РїРѕР»СЏ, ~6 СЃС‚СЂРѕРє)

## Sprint R3.1-HF2 вЂ” Selection markers fix (ShowSelectionMarkers notification)

**РџСЂРѕР±Р»РµРјР°:** РџРѕСЃР»Рµ R3.1 XAML Р±РёРЅРґРёС‚СЃСЏ РЅР°РїСЂСЏРјСѓСЋ Рє `SelectionManager.ShowSelectionMarkers` (computed property: `=> SelectedObjects.Count > 0`). РћРґРЅР°РєРѕ `PropertyChanged` РґР»СЏ СЌС‚РѕРіРѕ СЃРІРѕР№СЃС‚РІР° РЅРёРєРѕРіРґР° РЅРµ РІС‹Р·С‹РІР°Р»СЃСЏ вЂ” РїСЂРё РёР·РјРµРЅРµРЅРёРё РєРѕР»Р»РµРєС†РёРё `SelectedObjects` СЃСЂР°Р±Р°С‚С‹РІР°Р» С‚РѕР»СЊРєРѕ РїРµСЂРµРґР°РЅРЅС‹Р№ `_onSelectionChanged`-РєРѕР»Р»Р±СЌРє РІ `EditorViewModel`. WPF-Р±РёРЅРґРёРЅРі Р·Р°СЃС‚С‹РІР°РµС‚ РЅР° `Collapsed`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ `SelectionManager` РґРѕР±Р°РІР»РµРЅ `OnPropertyChanged(nameof(ShowSelectionMarkers))` РІ Р»СЏРјР±РґСѓ `CollectionChanged`.

**Р¤Р°Р№Р»:** `ViewModels/Managers/SelectionManager.cs` (1 СЃС‚СЂРѕРєР°)

## Phase 4 вЂ” PropertiesViewModel split (649в†’85 lines)

**Done:** PropertiesViewModel СЂР°Р·РґРµР»С‘РЅ РЅР° Р±Р°Р·Сѓ + 3 sub-VM. РџСЂСЏРјС‹Рµ Р±РёРЅРґРёРЅРіРё Р·Р°РјРµРЅРµРЅС‹ РЅР° ContentControl + DataTemplate.

| Р¤Р°Р№Р» | Р‘С‹Р»Рѕ | РЎС‚Р°Р»Рѕ |
|------|------|-------|
| PropertiesViewModel.cs | 649 СЃС‚СЂРѕРє (РјРѕРЅРѕР»РёС‚) | 85 СЃС‚СЂРѕРє (Р±Р°Р·Р°: selection + sub-VM lifecyle) |
| LinePropertiesViewModel.cs | вЂ” | 148 СЃС‚СЂРѕРє (7 СЃРІРѕР№СЃС‚РІ + 14 РєРѕРјР°РЅРґ + INPC) |
| RectanglePropertiesViewModel.cs | вЂ” | 168 СЃС‚СЂРѕРє (8 СЃРІРѕР№СЃС‚РІ + 16 РєРѕРјР°РЅРґ + INPC) |
| TextPropertiesViewModel.cs | вЂ” | 233 СЃС‚СЂРѕРєРё (13 СЃРІРѕР№СЃС‚РІ + 20 РєРѕРјР°РЅРґ + INPC) |
| PropertiesPanelContent.xaml | 549 СЃС‚СЂРѕРє (3Г—StackPanel) | ~620 СЃС‚СЂРѕРє (3Г—DataTemplate + ContentControl) |
| PropertiesViewModelTests.cs | 1313 СЃС‚СЂРѕРє | 1106 СЃС‚СЂРѕРє (sub-VM property/command paths) |
| PropertiesViewModelCommandTests.cs | 325 СЃС‚СЂРѕРє | 262 СЃС‚СЂРѕРєРё (sub-VM command paths) |

**РР·РјРµРЅРµРЅРёСЏ:**
- РљР°Р¶РґС‹Р№ sub-VM: `ObservableObject` + `UpdateObject(T?)` + INPC forwarding + `SetProperty` + `[RelayCommand]`
- Sub-VM РїРѕРґРїРёСЃС‹РІР°СЋС‚СЃСЏ РЅР° `INotifyPropertyChanged.PropertyChanged` РјРѕРґРµР»Рё РґР»СЏ live-РѕР±РЅРѕРІР»РµРЅРёСЏ
- XAML: 3 visible StackPanel в†’ 3 DataTemplate РЅР° С‚РёРї + `ContentControl Content="{Binding LineVM/RectVM/TextVM}"`
- Base VM: С‚РѕР»СЊРєРѕ `SelectedObject`, `SelectionCount`, `IsSingleSelection`, `IsLineSelected`, `IsRectangleSelected`, `IsTextSelected`, `ObjectId`, `ObjectTypeName`, `ValidationError`; sub-VM РїР°Р±Р»РёС€РµСЂС‹ С‡РµСЂРµР· РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ
- `PropertiesViewModel.SetProperty()` СѓРґР°Р»С‘РЅ РёР· Р±Р°Р·С‹ (Р»РѕРіРёРєР° РІ sub-VM)
- `PropertiesPanelContent.xaml.cs`: `OnTextIsEditableClick` РѕР±РЅРѕРІР»С‘РЅ РЅР° `textVm.ChangeIsEditableCommand.Execute()`

**Build:** 0 errors, 0 warnings
**Tests:** 1796 passed, 1 pre-existing skip

## Sprint вЂ” Print Preview (Ctrl+Shift+P)

### Feature: РџСЂРµРґРїСЂРѕСЃРјРѕС‚СЂ РїРµС‡Р°С‚Рё

**РџСЂРѕР±Р»РµРјР°:** РћС‚СЃСѓС‚СЃС‚РІРѕРІР°Р» РїСЂРµРґРїСЂРѕСЃРјРѕС‚СЂ РїРµС‡Р°С‚Рё вЂ” РїРѕР»СЊР·РѕРІР°С‚РµР»Рё РЅРµ РјРѕРіР»Рё РІРёРґРµС‚СЊ, РєР°Рє Р±СѓРґРµС‚ РІС‹РіР»СЏРґРµС‚СЊ С€Р°Р±Р»РѕРЅ РЅР° Р»РёСЃС‚Рµ РїРµСЂРµРґ РїРµС‡Р°С‚СЊСЋ. Р Р°РЅРµРµ Р±С‹Р» С‚РѕР»СЊРєРѕ РїСЂСЏРјРѕР№ РІС‹РІРѕРґ РЅР° РїСЂРёРЅС‚РµСЂ С‡РµСЂРµР· `PrintDialog`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р РµР°Р»РёР·РѕРІР°РЅ end-to-end РїСЂРµРґРїСЂРѕСЃРјРѕС‚СЂ С‡РµСЂРµР· `DocumentViewer` СЃ `FixedDocument`:

| РљРѕРјРїРѕРЅРµРЅС‚ | Р¤Р°Р№Р» | РќР°Р·РЅР°С‡РµРЅРёРµ |
|-----------|------|------------|
| РРЅС‚РµСЂС„РµР№СЃ | `Services/IPrintDocumentGenerator.cs` | РљРѕРЅС‚СЂР°РєС‚: `FixedDocument Generate(Template)` |
| Р“РµРЅРµСЂР°С‚РѕСЂ | `Services/PrintDocumentGenerator.cs` | Model в†’ WPF СЌР»РµРјРµРЅС‚С‹ (Line, Rectangle, TextBlock) СЃ РєРѕРЅРІРµСЂС‚Р°С†РёРµР№ РєРѕРѕСЂРґРёРЅР°С‚ (РјРёРєСЂРѕРЅС‹в†’WPF, Y-flip) |
| РћРєРЅРѕ | `Views/PrintPreviewWindow.xaml` + `.cs` | DocumentViewer СЃ FitToWidth, Print РєРЅРѕРїРєРѕР№, Close |
| РРЅС‚РµРіСЂР°С†РёСЏ | `ViewModels/MainViewModel.cs` | PreviewPrintCommand, DI IPrintDocumentGenerator |
| UI | `MainWindow.xaml` | MenuItem + Ctrl+Shift+P KeyBinding |
| DI | `App.xaml.cs` | Transient registration |
| РўРµСЃС‚С‹ | `Tests/Services/PrintDocumentGeneratorTests.cs` | 19 С‚РµСЃС‚РѕРІ: СЌР»РµРјРµРЅС‚С‹, РєРѕРѕСЂРґРёРЅР°С‚С‹, С†РІРµС‚Р°, С‚РёРїС‹ Р»РёРЅРёР№, РїРѕРІРѕСЂРѕС‚, РЅРµСЃРєРѕР»СЊРєРѕ РѕР±СЉРµРєС‚РѕРІ |

**РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Рµ СЂРµС€РµРЅРёСЏ:**
- FixedDocument + WPF-СЌР»РµРјРµРЅС‚С‹ (РЅРµ RenderTargetBitmap) вЂ” РІРµРєС‚РѕСЂРЅРѕРµ РєР°С‡РµСЃС‚РІРѕ, СЃРѕРІРјРµСЃС‚РёРјРѕСЃС‚СЊ СЃ DocumentViewer
- РћС‚РґРµР»СЊРЅС‹Р№ `IPrintDocumentGenerator` вЂ” РЅРµ Р·Р°РјРµРЅСЏРµС‚ `IPrintService`
- Transient СЂРµРіРёСЃС‚СЂР°С†РёСЏ вЂ” stateless РіРµРЅРµСЂР°С‚РѕСЂ
- FitToWidth РїСЂРё Р·Р°РіСЂСѓР·РєРµ вЂ” Р°РІС‚РѕРїРѕРґРіРѕРЅРєР° РїРѕРґ РѕРєРЅРѕ

## Sprint 58 вЂ” РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№ Р°РЅР°Р»РёР· (Р°РЅР°Р»РёС‚РёС‡РµСЃРєРёР№ СЃРїСЂРёРЅС‚)

РџРѕР»РЅС‹Р№ РѕС‚С‡С‘С‚: [`docs/48_РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№_Р°РЅР°Р»РёР·_Рё_РїР»Р°РЅ_СЂРµС„Р°РєС‚РѕСЂРёРЅРіР°.md`](docs/48_РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№_Р°РЅР°Р»РёР·_Рё_РїР»Р°РЅ_СЂРµС„Р°РєС‚РѕСЂРёРЅРіР°.md)

### РќР°Р№РґРµРЅРЅС‹Рµ Р°СЂС…РёС‚РµРєС‚СѓСЂРЅС‹Рµ РїСЂРѕР±Р»РµРјС‹ (25 Р·Р°РјРµС‡Р°РЅРёР№)

РљР»СЋС‡РµРІС‹Рµ РЅР°С…РѕРґРєРё:
- **P0:** EditorViewModel вЂ” god-object В«С„Р°СЃР°Рґ СЃ РїСЂРѕР±СЂРѕСЃРѕРјВ» (1160 СЃС‚СЂРѕРє, ~60 forwarding-СЃРІРѕР№СЃС‚РІ, 4 switch-РѕР±СЂР°Р±РѕС‚С‡РёРєР° РґР»СЏ СЂРµС‚СЂР°РЅСЃР»СЏС†РёРё INPC). **Р РµС€РµРЅРѕ (Sprint R3.1):** ~1194в†’784 СЃС‚СЂРѕРє, forwarding СѓРґР°Р»С‘РЅ, XAML Р±РёРЅРґРёС‚СЃСЏ Рє РјРµРЅРµРґР¶РµСЂР°Рј.
- **P1:** РР·Р±С‹С‚РѕС‡РЅР°СЏ 3-СѓСЂРѕРІРЅРµРІР°СЏ РёРµСЂР°СЂС…РёСЏ РјРѕРґРµР»РµР№ (ObjectBase в†’ ModelBase в†’ TemplateObjectBase). Р РµС€РµРЅРёРµ: СЃС…Р»РѕРїРЅСѓС‚СЊ РІ РѕРґРёРЅ СѓСЂРѕРІРµРЅСЊ.
- **P1:** ~50 РґСѓР±Р»РёСЂРѕРІР°РЅРЅС‹С… INPC-setter РІ Line/Rectangle/Text. Р РµС€РµРЅРёРµ: `[ObservableProperty]` source generator.
- **P1:** MoveSelected/RotateSelected РЅРµ РіСЂСѓРїРїРёСЂСѓСЋС‚СЃСЏ РІ BatchCommand (inconsistent Undo).
- **P1:** ValidationService вЂ” static 537-СЃС‚СЂРѕС‡РЅС‹Р№ god-service, untestable.
- **P1:** РќРµС‚ Central Package Management, `TreatWarningsAsErrors` С‚РѕР»СЊРєРѕ РІ CI.

### РџР»Р°РЅ СЂРµС„Р°РєС‚РѕСЂРёРЅРіР° R1вЂ“R4

| РЎРїСЂРёРЅС‚ | Р¦РµР»СЊ | Р”Р»РёС‚РµР»СЊРЅРѕСЃС‚СЊ |
|--------|------|-------------|
| R1 | Р‘С‹СЃС‚СЂС‹Рµ РїРѕР±РµРґС‹: CPM, TreatWarningsAsErrors, Undo-РіСЂСѓРїРїРёСЂРѕРІРєР°, flaky-С‚РµСЃС‚С‹ | 2вЂ“3 РґРЅСЏ |
| R2 | Models cleanup: РёРµСЂР°СЂС…РёСЏ, `[ObservableProperty]`, ITemplateValidator | 3вЂ“4 РґРЅСЏ |
| R3 | EditorVM de-bloat: РїСЂРѕР±СЂРѕСЃ С‡РµСЂРµР· РјРµРЅРµРґР¶РµСЂС‹, IEditorContext, DI | 4вЂ“5 РґРЅРµР№ |
| R4 | Presentation + Tests: EditorCanvasBehavior, CI coverage-gate | 4вЂ“5 РґРЅРµР№ |

## Sprint 59 вЂ” Grid bug fixes (PropertyChanged, РјС‘СЂС‚РІС‹Р№ РєРѕРґ, ComputeDisplayStep)

### Fix SG-1: IsGridEnabled/IsSnapEnabled РЅРµ РґС‘СЂРіР°Р»Рё PropertyChanged

**РџСЂРѕР±Р»РµРјР°:** РЎРµС‚С‚РµСЂС‹ `GridManager.IsGridEnabled` Рё `IsSnapEnabled` РЅРµ РІС‹Р·С‹РІР°Р»Рё `OnPropertyChanged()`. РџСЂРё РїСЂРѕРіСЂР°РјРјРЅРѕРј РёР·РјРµРЅРµРЅРёРё (РјРµРЅСЋ, РєРѕРґ) XAML ToggleButton РЅР° С‚СѓР»Р±Р°СЂРµ РЅРµ РѕР±РЅРѕРІР»СЏР» `IsChecked`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ `OnPropertyChanged()` РІ РѕР±Р° СЃРµС‚С‚РµСЂР°.

### Fix SG-2: ToggleGrid() РґРµСЃРёРЅС…СЂРѕРЅРёР·РёСЂРѕРІР°Р» Enabled Рё Visible

**РџСЂРѕР±Р»РµРјР°:** `ToggleGrid()` РїРµСЂРµРєР»СЋС‡Р°Р» С‚РѕР»СЊРєРѕ `Enabled`. Р•СЃР»Рё РґРѕ РІС‹Р·РѕРІР° Р±С‹Р»Рѕ `Enabled=false, Visible=false` (С‡РµСЂРµР· СЃРµС‚С‚РµСЂ), РїРѕСЃР»Рµ `ToggleGrid()` СЃС‚Р°РЅРѕРІРёР»РѕСЃСЊ `Enabled=true, Visible=false` вЂ” СЃРµС‚РєР° СЃРєСЂС‹С‚Р°.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `ToggleGrid()` РїРµСЂРµРїРёСЃР°РЅ С‡РµСЂРµР· `IsGridEnabled = !IsGridEnabled` (СЃРµС‚С‚РµСЂ).

### Fix SG-3: Truncation РІРјРµСЃС‚Рѕ Rounding РІ РєРѕРѕСЂРґРёРЅР°С‚Р°С… СѓР·Р»РѕРІ

**РџСЂРѕР±Р»РµРјР°:** `(long)` РєР°СЃС‚ РІ `RefreshGridNodes()` РѕС‚Р±СЂР°СЃС‹РІР°Р» РґСЂРѕР±РЅСѓСЋ С‡Р°СЃС‚СЊ. РќР° РІС‹СЃРѕРєРѕРј zoom вЂ” РѕС€РёР±РєР° РїРѕР·РёС†РёРѕРЅРёСЂРѕРІР°РЅРёСЏ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `(long)` в†’ `(long)Math.Round()`.

### Fix SG-4: РњС‘СЂС‚РІС‹Р№ РєРѕРґ GridLine/GenerateGridLines/GenerateVisibleGridLines

**РџСЂРѕР±Р»РµРјР°:** `GridHelper` СЃРѕРґРµСЂР¶Р°Р» struct `GridLine` Рё РґРІР° РјРµС‚РѕРґР° РіРµРЅРµСЂР°С†РёРё Р»РёРЅРёР№ (~90 СЃС‚СЂРѕРє), РєРѕС‚РѕСЂС‹Рµ РЅРёРєРѕРіРґР° РЅРµ РІС‹Р·С‹РІР°Р»РёСЃСЊ РІ production. РўРѕР»СЊРєРѕ С‚РµСЃС‚С‹.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЈРґР°Р»РµРЅС‹ `GridLine`, `GenerateGridLines()`, `GenerateVisibleGridLines()`. РЈРґР°Р»РµРЅС‹ 13 С‚РµСЃС‚РѕРІ РґР»СЏ СЌС‚РёС… РјРµС‚РѕРґРѕРІ.

### Fix SG-5: GridStepToStringConverter вЂ” РЅРµРЅР°РґС‘Р¶РЅС‹Р№ РїР°СЂСЃРёРЅРі

**РџСЂРѕР±Р»РµРјР°:** `ConvertBack` СѓРґР°Р»СЏР» С‚РѕР»СЊРєРѕ `"РјРј"`. Р”СЂСѓРіРёРµ С„РѕСЂРјР°С‚С‹ (`"5 mm"`, `"5,5"`) РјРѕР»С‡Р° РІРѕР·РІСЂР°С‰Р°Р»Рё `5.0`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РљРѕРЅРІРµСЂС‚РµСЂ СѓРґР°Р»СЏРµС‚ Р»СЋР±РѕР№ РЅРµС‡РёСЃР»РѕРІРѕР№ СЃСѓС„С„РёРєСЃ (Regex), РЅРѕСЂРјР°Р»РёР·СѓРµС‚ commaв†’dot, РїСЂРё РѕС€РёР±РєРµ РїР°СЂСЃРёРЅРіР° РІРѕР·РІСЂР°С‰Р°РµС‚ `Binding.DoNothing`.

### Fix SG-6: РР·РјРµРЅРµРЅРёРµ С€Р°РіР° СЃРµС‚РєРё РЅРµ РІР»РёСЏР»Рѕ РЅР° РѕС‚РѕР±СЂР°Р¶РµРЅРёРµ

**РљРѕСЂРЅРµРІР°СЏ РїСЂРёС‡РёРЅР°:** `GridManager.RefreshGridNodes()` РІС‹Р·С‹РІР°Р» `ComputeDisplayStep()`, РєРѕС‚РѕСЂС‹Р№ **РїРѕР»РЅРѕСЃС‚СЊСЋ РёРіРЅРѕСЂРёСЂРѕРІР°Р»** `_gridSettings.StepMicrons`. РЁР°Рі РІС‹С‡РёСЃР»СЏР»СЃСЏ С‚РѕР»СЊРєРѕ РёР· `MinPixelSpacing / zoom`. РџРѕР»СЊР·РѕРІР°С‚РµР»СЊСЃРєРёР№ С€Р°Рі РЅРёРіРґРµ РЅРµ СѓС‡Р°СЃС‚РІРѕРІР°Р».

**РСЃРїСЂР°РІР»РµРЅРёРµ:**
- `ComputeDisplayStep()` РїСЂРёРЅРёРјР°РµС‚ `preferredStepMicrons` (РѕРїС†РёРѕРЅР°Р»СЊРЅС‹Р№ РїР°СЂР°РјРµС‚СЂ)
- Р•СЃР»Рё `preferredStep` РґР°С‘С‚ `pixelSpacing >= MinPixelSpacing` вЂ” РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РєР°Рє С†РµР»РµРІРѕР№
- Р•СЃР»Рё `pixelSpacing < MinPixelSpacing` вЂ” fallback РЅР° `MinPixelSpacing / zoom`
- Р’ РѕР±РѕРёС… СЃР»СѓС‡Р°СЏС… С€Р°Рі coarsen'РёС‚СЃСЏ РµСЃР»Рё `cols * rows > maxNodes`
- Р’ `GridStepMm` СЃРµС‚С‚РµСЂ РґРѕР±Р°РІР»РµРЅ `OnPropertyChanged()`

### Common Mistakes (new)
43. `async void` in timer handler вЂ” `MainViewModel.OnAutosaveTickHandler()` is `async void` subscribed to `AutosaveTick`. If `AutosaveAllTabsAsync` throws, exception is lost (not caught). Always use try/catch with logging inside `async void`, or wrap in `SafeFireAndForget`.
44. Multi-Undo inconsistency вЂ” `DeleteSelected()` and `PasteFromClipboard()` group multi-object operations into `BatchCommand`, but `MoveSelected()` and `RotateSelectedClockwise()` do NOT. User presses Undo N times for N objects. Always apply `BatchCommand` when `SelectedObjects.Count > 1`.
45. File name в‰  type name вЂ” `Commands/IUndoCommand.cs` contains interface `IUndoCommand` (renamed from `ICommand.cs` to avoid WPF conflict). The file name is misleading and may cause wrong `using` imports. Rename to `IUndoCommand.cs`.
46. `IAutosaveTab` defined inside service вЂ” `Services/AutosaveService.cs` contains `public interface IAutosaveTab`. EditorViewModel explicitly implements it. Service dictates interface to ViewModel (inverted dependency). Always define interfaces near their consumer, not provider.
47. `PrintVisualProvider` leaks WPF type вЂ” `Func<System.Windows.Media.Visual?>` on EditorViewModel exposes WPF rendering to ViewModel. View sets it, creating a potential dangling reference after tab close. Encapsulate via interface or use WeakReference/Messenger.
48. `CustomResizeCommand` reuses `_newHeight` as FontSize for Text вЂ” semantic confusion in the same field. The command's `Execute()`/`Undo()` use `switch (_object)` instead of polymorphism, violating OCP. Prefer `ApplyResize(CaptureState())` on the object.
49. `ValidationService` is static and untestable вЂ” `Helpers/ValidationService` is a `static class` called directly from PropertiesViewModel and TemplateService. Cannot be mocked. Make domain validation injectable (`ITemplateValidator`). UI field validators can stay static as pure functions.
50. No Central Package Management вЂ” package versions are hardcoded in two csproj files. No `Directory.Packages.props`. Versions drift independently. Adopt CPM (`<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` + `Directory.Packages.props`).
51. csproj duplicates Directory.Build.props вЂ” `TargetFramework`, `Nullable`, `ImplicitUsings` declared in both `Directory.Build.props` and each csproj. Remove duplicates from csproj, keep only project-specific properties (`OutputType`, `UseWPF`).
52. `TreatWarningsAsErrors` only in CI вЂ” local builds don't catch warnings. CI `analyze` job uses `/warnaserror`, but developers see warnings only after push. Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props`.

53. `CustomResizeCommand` refactoring вЂ” after R4.3, the constructor takes `(TemplateObjectBase, ResizeState, ResizeState, Action?)` NOT 9 `long` params. The factory methods `ForRectangle`, `ForText`, `ForLine` are kept for backward compatibility.
54. `ResizeMath` вЂ” all pure resize calculations live in `Tools/ResizeMath.cs`. `ResizeTool.cs` delegates to it. Do NOT add new resize math to ResizeTool directly.
55. `ShortcutRegistry` вЂ” add new keyboard shortcuts to `Helpers/ShortcutRegistry.cs`, NOT to `MainWindow.xaml.cs`.
56. `CaptureResizeState`/`ApplyResize` вЂ” every new model subclass of `TemplateObjectBase` MUST implement these two methods for undoable resize to work.
57. Test file merging вЂ” after R4.4, there are NO Extended/Additional test files. All tests live in the parent files. Create tests in the parent, not in separate files.
58. Coverage gate вЂ” CI checks coverage в‰Ґ75%. On failure, the build is red. Generate coverage locally with `dotnet test --collect:"XPlat Code Coverage"` before pushing.
59. Forwarding properties after R3.1 вЂ” after XAML was migrated to bind to managers directly (R3.1), forwarding properties on EditorViewModel became dead code. Remove them: delete the property, delete the `OnPropertyChanged()` in setters of IEditorContext-required properties, remove PropertyChanged forwarding handlers (`_zoomPanHandler`, `_previewHandler`, `_dirtyStateHandler`, `_toolManagerHandler`), remove `OnZoomChangedInternal()`, and simplify `OnSelectionChangedInternal()`. IAutosaveTab properties become explicit interface implementation. Test references must use `editor.XManager.Y` instead of `editor.Y`.
60. `[ObservableProperty]` on reference-type fields with re-assign вЂ” the source-generated setter uses `EqualityComparer<T>.Default.Equals()`, which for reference types defaults to `ReferenceEquals`. If you mutate the same instance and re-assign it, `PropertyChanged` is suppressed. Use manual setters with unconditional `OnPropertyChanged()` for preview/re-assign patterns.
61. Computed properties (expression-bodied, no `[ObservableProperty]`) on ObservableObject managers that are bound from XAML must fire `OnPropertyChanged()` explicitly when their dependencies change. The binding engine only re-evaluates when `PropertyChanged` fires for that property name вЂ” it does NOT infer dependencies from the expression body.
62. WPF RotateTransform matrix вЂ” WPF РёСЃРїРѕР»СЊР·СѓРµС‚ STANDARD CCW Cartesian matrix `x'=x*cosОёв€’y*sinОё, y'=x*sinОё+y*cosОё`. Р’ Y-down (screen) СЌС‚Рѕ РґР°С‘С‚ CW-РІСЂР°С‰РµРЅРёРµ. RotatedCorner*/GetBoundingBox РёСЃРїРѕР»СЊР·СѓСЋС‚ forward transform. ContainsPoint() РёСЃРїРѕР»СЊР·СѓРµС‚ INVERSE transform `u=x*cos+y*sin, v=в€’x*sin+y*cos` РґР»СЏ unrotate С‚РѕС‡РєРё РІ Р»РѕРєР°Р»СЊРЅРѕРµ РїСЂРѕСЃС‚СЂР°РЅСЃС‚РІРѕ С‚РµРєСЃС‚Р°. РќРµ РїСѓС‚Р°С‚СЊ СЃ CW-specific С„РѕСЂРјСѓР»Р°РјРё вЂ” РѕРЅРё РЅРµРІРµСЂРЅС‹ РґР»СЏ WPF.

## Sprint AвЂ“D вЂ” РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№ СЂРµС„Р°РєС‚РѕСЂРёРЅРі (18 Р·Р°РјРµС‡Р°РЅРёР№)

РџРѕСЃР»Рµ Sprint 59 Р±С‹Р» РїСЂРѕРІРµРґС‘РЅ РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№ Р°РЅР°Р»РёР· (48_РђСЂС…РёС‚РµРєС‚СѓСЂРЅС‹Р№_Р°РЅР°Р»РёР·_Рё_РїР»Р°РЅ_СЂРµС„Р°РєС‚РѕСЂРёРЅРіР°.md) Рё СЃРѕСЃС‚Р°РІР»РµРЅ РїР»Р°РЅ СЂРµС„Р°РєС‚РѕСЂРёРЅРіР° РЅР° 4 СЃРїСЂРёРЅС‚Р° (AвЂ“D). Р’С‹РїРѕР»РЅРµРЅРѕ 12 РёР· 14 РїСѓРЅРєС‚РѕРІ, 2 РїСЂРѕРїСѓС‰РµРЅС‹ (P4).

### A.1 вЂ” IDisposable РІ sub-VM (СѓС‚РµС‡РєР° РїР°РјСЏС‚Рё)

**РџСЂРѕР±Р»РµРјР°:** `LinePropertiesViewModel`, `RectanglePropertiesViewModel`, `TextPropertiesViewModel` РїРѕРґРїРёСЃС‹РІР°Р»РёСЃСЊ РЅР° `INotifyPropertyChanged.PropertyChanged` РјРѕРґРµР»Рё РІ `UpdateObject()`, РЅРѕ РЅРёРєРѕРіРґР° РЅРµ РѕС‚РїРёСЃС‹РІР°Р»РёСЃСЊ. РџСЂРё Р·Р°РєСЂС‹С‚РёРё РІРєР»Р°РґРєРё sub-VM РїСЂРѕРґРѕР»Р¶Р°Р»Рё РІРёСЃРµС‚СЊ РІ РїР°РјСЏС‚Рё С‡РµСЂРµР· delegate.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅС‹ `IDisposable.Dispose()` РІРѕ РІСЃРµ 3 sub-VM СЃ РѕС‚РїРёСЃРєРѕР№. `PropertiesViewModel.Dispose()` РєР°СЃРєР°РґРЅРѕ РІС‹Р·С‹РІР°РµС‚ dispose РІСЃРµС… С‚СЂС‘С….

**Р¤Р°Р№Р»С‹:**
- `LinePropertiesViewModel.cs`, `RectanglePropertiesViewModel.cs`, `TextPropertiesViewModel.cs`, `PropertiesViewModel.cs`

### A.2 вЂ” Dual-write GridManager/StatusBarManager

**РџСЂРѕР±Р»РµРјР°:** `StatusBarManager` РІР»Р°РґРµР» РѕС‚РґРµР»СЊРЅРѕР№ РєРѕРїРёРµР№ `GridSettings` (StepMicrons, GridEnabled, SnapEnabled), РґСѓР±Р»РёСЂСѓСЏ СЃРѕСЃС‚РѕСЏРЅРёРµ `GridManager`. Р”РІР° РЅРµР·Р°РІРёСЃРёРјС‹С… РёСЃС‚РѕС‡РЅРёРєР° РёСЃС‚РёРЅС‹ вЂ” РјСѓС‚Р°С†РёСЏ С‡РµСЂРµР· UI (StatusBar) РЅРµ СЃРёРЅС…СЂРѕРЅРёР·РёСЂРѕРІР°Р»Р°СЃСЊ СЃ GridManager.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `StatusBarManager` Р±РѕР»СЊС€Рµ РЅРµ СЃРѕРґРµСЂР¶РёС‚ `GridSettings`. РљРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ РїСЂРёРЅРёРјР°РµС‚ 6 РґРµР»РµРіР°С‚РѕРІ (get/set РґР»СЏ GridEnabled, GridStepMm, SnapEnabled) + `Action onGridRefresh`. `EditorViewModel` РїРµСЂРµРґР°С‘С‚ Р»СЏРјР±РґС‹ Рє `GridManager`. `GridManager` вЂ” РµРґРёРЅСЃС‚РІРµРЅРЅС‹Р№ owner `GridSettings`.

**Р¤Р°Р№Р»С‹:** `StatusBarManager.cs`, `EditorViewModel.cs`, `GridManager.cs`

**Common Mistakes (new):**
63. Dual-write in managers вЂ” never give two managers independent copies of the same mutable settings. One must be the single source of truth; others delegate via lambdas or events.

### B.1 вЂ” FontMetrics: static в†’ instance + DI

**РџСЂРѕР±Р»РµРјР°:** `FontMetrics` вЂ” РїРѕР»РЅРѕСЃС‚СЊСЋ static class. РўРµСЃС‚С‹ РЅРµ РјРѕРіР»Рё РјРѕРєРёСЂРѕРІР°С‚СЊ, DI-РєРѕРЅС‚РµР№РЅРµСЂ РЅРµ РёРјРµР» РёРЅС‚РµСЂС„РµР№СЃР°.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРѕР·РґР°РЅ `IFontMetrics` interface. `FontMetrics` РїРµСЂРµРІРµРґС‘РЅ РёР· static РІ instance class, СЂРµР°Р»РёР·СѓСЋС‰РёР№ `IFontMetrics`. Р”РѕР±Р°РІР»РµРЅ `static readonly FontMetrics Default = new()` РґР»СЏ backward compat. DI-СЂРµРіРёСЃС‚СЂР°С†РёСЏ: `services.AddSingleton<IFontMetrics>(FontMetrics.Default)`. `Text.cs` РёСЃРїРѕР»СЊР·СѓРµС‚ `FontMetrics.Default.GetHeightRatio/FontMetrics.Default.GetAdvWidthRatio`.

Р”Р»СЏ СѓСЃС‚СЂР°РЅРµРЅРёСЏ flaky race-СѓСЃР»РѕРІРёР№ РІ РїР°СЂР°Р»Р»РµР»СЊРЅС‹С… С‚РµСЃС‚Р°С… (FontMetricsTests Рё TextTests/HitTestHelperTests РјРѕРґРёС„РёС†РёСЂРѕРІР°Р»Рё shared state РѕРґРЅРѕРІСЂРµРјРµРЅРЅРѕ) РґРѕР±Р°РІР»РµРЅ `[Collection("FontMetrics", DisableParallelization = true)]`.

**Р¤Р°Р№Р»С‹:** `Models/IFontMetrics.cs` (РЅРѕРІС‹Р№), `FontMetrics.cs`, `Text.cs`, `App.xaml.cs`, `FontMetricsTests.cs`, `TextTests.cs`, `HitTestHelperTests.cs`, `FontMetricsTestCollection.cs` (РЅРѕРІС‹Р№)

### B.2 вЂ” PanOffsetX/Y forwarding СѓРґР°Р»РµРЅС‹ РёР· EditorViewModel

**РџСЂРѕР±Р»РµРјР°:** РџРѕСЃР»Рµ R3.1 XAML Р±РёРЅРґРёС‚СЃСЏ РЅР°РїСЂСЏРјСѓСЋ Рє `ZoomPanManager.PanOffsetX/Y`, РЅРѕ EditorViewModel РїСЂРѕРґРѕР»Р¶Р°Р» СЃРѕРґРµСЂР¶Р°С‚СЊ forwarding-СЃРІРѕР№СЃС‚РІР° `PanOffsetX`/`PanOffsetY`. РўРµСЃС‚С‹ РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё `editor.PanOffsetX` РІРјРµСЃС‚Рѕ `editor.ZoomPanManager.PanOffsetX`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРІРѕР№СЃС‚РІР° СѓРґР°Р»РµРЅС‹ РёР· EditorViewModel. РўРµСЃС‚С‹ Р·Р°РјРµРЅРµРЅС‹: `editor.PanOffsetX` в†’ `editor.ZoomPanManager.PanOffsetX`.

**Р¤Р°Р№Р»С‹:** `EditorViewModel.cs`, `EditorViewModelTests.cs`, `PanToolTests.cs`

### B.3 вЂ” EditorConstants в†’ PhysicalConstants/EditorSettings

**РџСЂРѕР±Р»РµРјР°:** `EditorConstants.cs` вЂ” 36-line pure proxy, РєР°Р¶РґР°СЏ РєРѕРЅСЃС‚Р°РЅС‚Р° СЂРµ-СЌРєСЃРїРѕСЂС‚РёСЂРѕРІР°Р»Р° `PhysicalConstants.XXX` РёР»Рё `EditorSettings.XXX`. 69 references РІ 20 С„Р°Р№Р»Р°С….

**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р’СЃРµ 69 references Р·Р°РјРµРЅРµРЅС‹ РїСЂСЏРјС‹РјРё РІС‹Р·РѕРІР°РјРё `PhysicalConstants.XXX` РёР»Рё `EditorSettings.XXX`. `EditorConstants.cs` СѓРґР°Р»С‘РЅ.

**Р¤Р°Р№Р»С‹:** 20 С„Р°Р№Р»РѕРІ РѕР±РЅРѕРІР»РµРЅС‹, `EditorConstants.cs` СѓРґР°Р»С‘РЅ.

### C.1 вЂ” Shortcuts РёР· code-behind РІ ShortcutRegistry

**РџСЂРѕР±Р»РµРјР°:** `Window_PreviewKeyDown` (30 СЃС‚СЂРѕРє) СЃРѕРґРµСЂР¶Р°Р» Р»РѕРіРёРєСѓ РґРёСЃРїРµС‚С‡РµСЂРёР·Р°С†РёРё РєР»Р°РІРёС€. Р”РѕР±Р°РІР»РµРЅРёРµ РЅРѕРІРѕРіРѕ С…РѕС‚РєРµСЏ С‚СЂРµР±РѕРІР°Р»Рѕ РёР·РјРµРЅРµРЅРёСЏ code-behind.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРѕР·РґР°РЅ `ShortcutRegistry.TryHandle(Key, ModifierKeys, EditorViewModel) в†’ bool` вЂ” РµРґРёРЅР°СЏ С‚РѕС‡РєР° РІС…РѕРґР°. `Window_PreviewKeyDown` СЃРѕРєСЂР°С‰С‘РЅ РґРѕ 3 СЃС‚СЂРѕРє.

**Р¤Р°Р№Р»С‹:** `ShortcutRegistry.cs`, `MainWindow.xaml.cs`

### C.2 вЂ” Tag-parsing РІ CustomSheetDialogViewModel

**РџСЂРѕР±Р»РµРјР°:** РљРЅРѕРїРєРё Р±С‹СЃС‚СЂРѕРіРѕ РІС‹Р±РѕСЂР° С„РѕСЂРјР°С‚Р° (A4/A3/вЂ¦) РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё `Tag="210,297"` СЃ РїР°СЂСЃРёРЅРіРѕРј РІ code-behind (`OnQuickFormatClick`, `string.Split(',')`). РќРµС‚РµСЃС‚РёСЂСѓРµРјРѕ, XAML-Р·Р°РІРёСЃРёРјРѕ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** РљРѕРґ РёР· code-behind СѓРґР°Р»С‘РЅ. Р”РѕР±Р°РІР»РµРЅ `SetQuickFormatCommand(string formatName)` РІ `CustomSheetDialogViewModel`, РІС‹Р·С‹РІР°СЋС‰РёР№ `Sheet.FromFormat(formatName)`. XAML: `Click` + `Tag` в†’ `Command="{Binding SetQuickFormatCommand}" CommandParameter="A4"`. Code-behind С„Р°Р№Р» СЃРѕРєСЂР°С‰С‘РЅ РґРѕ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂР°.

**Р¤Р°Р№Р»С‹:** `CustomSheetDialogViewModel.cs`, `CustomSheetDialog.xaml`, `CustomSheetDialog.xaml.cs`

### C.3 вЂ” No-op Dispose СѓРґР°Р»С‘РЅ РёР· TemplateLibraryViewModel

**РџСЂРѕР±Р»РµРјР°:** `TemplateLibraryViewModel` СЂРµР°Р»РёР·РѕРІС‹РІР°Р» `IDisposable` СЃ РїСѓСЃС‚С‹Рј С‚РµР»РѕРј. Р’С‹Р·РѕРІ `Dispose()` РІ `MainViewModel.Dispose()` вЂ” РјС‘СЂС‚РІС‹Р№ РєРѕРґ.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `IDisposable` СѓРґР°Р»С‘РЅ РёР· РєР»Р°СЃСЃР°. Р’С‹Р·РѕРІ `TemplateLibraryVm?.Dispose()` СѓРґР°Р»С‘РЅ РёР· `MainViewModel.Dispose()`.

**Р¤Р°Р№Р»С‹:** `TemplateLibraryViewModel.cs`, `MainViewModel.cs`

### C.4 вЂ” ITool.OnMouseWheel в†’ bool

**РџСЂРѕР±Р»РµРјР°:** `ITool.OnMouseWheel` РІРѕР·РІСЂР°С‰Р°Р» `void` вЂ” РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹ РЅРµ РјРѕРіР»Рё Р·Р°Р±Р»РѕРєРёСЂРѕРІР°С‚СЊ Р·СѓРј. CanvasInputRouter Р±РµР·СѓСЃР»РѕРІРЅРѕ РїСЂРёРјРµРЅСЏР» zoom РїРѕСЃР»Рµ РІС‹Р·РѕРІР° `OnMouseWheel`.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `ITool.OnMouseWheel` С‚РµРїРµСЂСЊ РІРѕР·РІСЂР°С‰Р°РµС‚ `bool` вЂ” `true` РѕР·РЅР°С‡Р°РµС‚ В«СЃРѕР±С‹С‚РёРµ РѕР±СЂР°Р±РѕС‚Р°РЅРѕ, Р·СѓРј РЅРµ РїСЂРёРјРµРЅСЏС‚СЊВ». Р’СЃРµ 6 СЂРµР°Р»РёР·Р°С†РёР№ РѕР±РЅРѕРІР»РµРЅС‹ (РІРѕР·РІСЂР°С‰Р°СЋС‚ `false`). `CanvasInputRouter` РїСЂРѕРІРµСЂСЏРµС‚ return value.

**Р¤Р°Р№Р»С‹:** `ITool.cs`, `SelectTool.cs`, `PanTool.cs`, `DrawingLineTool.cs`, `DrawingRectangleTool.cs`, `TextTool.cs`, `ResizeTool.cs`, `CanvasInputRouter.cs`, `ToolManagerTests.cs`

### C.5 вЂ” Memory leak SelectionManager

**РџСЂРѕР±Р»РµРјР°:** `SelectionManager` РїРѕРґРїРёСЃС‹РІР°Р»СЃСЏ РЅР° `SelectedObjects.CollectionChanged` РІ РєРѕРЅСЃС‚СЂСѓРєС‚РѕСЂРµ, РЅРѕ РѕС‚РїРёСЃРєР° РЅРµ Р±С‹Р»Р° РїСЂРµРґСѓСЃРјРѕС‚СЂРµРЅР°.

**РСЃРїСЂР°РІР»РµРЅРёРµ:** `SelectionManager` СЂРµР°Р»РёР·СѓРµС‚ `IDisposable`. РҐСЌРЅРґР»РµСЂ СЃРѕС…СЂР°РЅС‘РЅ РІ РїРѕР»Рµ `_onCollectionChanged`, РѕС‚РїРёСЃРєР° РІ `Dispose()`. `EditorViewModel.Dispose()` РІС‹Р·С‹РІР°РµС‚ `_selectionManager.Dispose()`.

**Р¤Р°Р№Р»С‹:** `SelectionManager.cs`, `EditorViewModel.cs`

### D.1 вЂ” MockBehavior.Strict в†’ Loose

3 РјРѕРєР° `ICommand` РІ behavior-С‚РµСЃС‚Р°С… РёСЃРїРѕР»СЊР·РѕРІР°Р»Рё `MockBehavior.Strict` вЂ” РїСЂРё РґРѕР±Р°РІР»РµРЅРёРё РЅРѕРІРѕРіРѕ РјРµС‚РѕРґР° РІ `ICommand` С‚РµСЃС‚С‹ РїР°РґР°Р»Рё. Р—Р°РјРµРЅРµРЅС‹ РЅР° `MockBehavior.Loose`.

**Р¤Р°Р№Р»С‹:** `TextBoxLostFocusCommandBehaviorTests.cs`, `ComboBoxSelectionChangedCommandBehaviorTests.cs`

### D.3 вЂ” Sealed РєР»Р°СЃСЃС‹

66 РєР»Р°СЃСЃРѕРІ РїРѕРјРµС‡РµРЅС‹ `sealed`: РІСЃРµ Converters (27), Commands (5), Services (20), Tools (8), Managers (9).

**Common Mistakes (new):**
64. `IDisposable` with lambda subscriptions вЂ” always save the handler reference to a field and unsubscribe in `Dispose()`. Lambda-in-constructor subscriptions can't be removed without a stored reference.
65. `ITool.OnMouseWheel` return type вЂ” use `bool` (handled flag), consistent with `OnKeyDown`. Tools that don't need wheel handling return `false`; future tools can block zoom by returning `true`.

## Sprint вЂ” Grid refactoring (Points 1, 4, 5 РёР· Р°СЂС…РёС‚РµРєС‚СѓСЂРЅРѕРіРѕ РѕР±СЃСѓР¶РґРµРЅРёСЏ)

### Р§С‚Рѕ СЃРґРµР»Р°РЅРѕ

РўСЂРё РІР·Р°РёРјРѕСЃРІСЏР·Р°РЅРЅС‹С… РёР·РјРµРЅРµРЅРёСЏ РІ Р°СЂС…РёС‚РµРєС‚СѓСЂРµ СЃРµС‚РєРё:

**Point 1 вЂ” РҐСЂР°РЅРµРЅРёРµ РјРёРєСЂРѕРЅРѕРІ РІРјРµСЃС‚Рѕ РїРёРєСЃРµР»РµР№:**
- `GridNodesLayer` С‚РµРїРµСЂСЊ С…СЂР°РЅРёС‚ РєРѕРѕСЂРґРёРЅР°С‚С‹ СѓР·Р»РѕРІ РІ **РјРёРєСЂРѕРЅР°С…** (model space), Р° РЅРµ РІ РїРёРєСЃРµР»СЏС…
- `OnRender` СЃР°Рј РєРѕРЅРІРµСЂС‚РёСЂСѓРµС‚ РјРёРєСЂРѕРЅС‹ в†’ РїРёРєСЃРµР»Рё (zoom + Y-flip)
- Р”РѕР±Р°РІР»РµРЅС‹ DependencyProperty `Zoom` Рё `SheetHeightMm` вЂ” РїСЂРё РёР·РјРµРЅРµРЅРёРё zoom`Р° РёР»Рё РІС‹СЃРѕС‚С‹ Р»РёСЃС‚Р° РїРµСЂРµСЂРёСЃРѕРІРєР° С‡РµСЂРµР· `InvalidateVisual`
- `GridNodesLayer` РёР·РјРµРЅС‘РЅ СЃ `UIElement` РЅР° `FrameworkElement` (РЅСѓР¶РµРЅ РґР»СЏ WPF Data Binding С‡РµСЂРµР· DPs)
- РџСЂРё Р·СѓРјРµ Р±РѕР»СЊС€Рµ РќР• С‚СЂРµР±СѓРµС‚СЃСЏ СЂРµРіРµРЅРµСЂР°С†РёСЏ СѓР·Р»РѕРІ (С‚РѕР»СЊРєРѕ СЃРјРµРЅР° С€Р°РіР° РёР»Рё viewport)

**Point 4 вЂ” РЈРїСЂРѕС‰РµРЅРёРµ pan-РєСЌС€РёСЂРѕРІР°РЅРёСЏ (СѓРґР°Р»РµРЅРѕ С†РµР»РёРєРѕРј):**
- РЈРґР°Р»РµРЅС‹: `_cachedRegionLeftMicrons`, `_cachedRegionBottomMicrons`, `_cachedRegionWidthMicrons`, `_cachedRegionHeightMicrons`, `_hasCachedRegion`
- РЈРґР°Р»РµРЅС‹: `IsWithinCachedRegion()`, `InvalidateCacheOnPan()`, `RefreshOnPanEnd()`
- РЈРґР°Р»РµРЅС‹: `_debounceCts`, `SuppressDebounce`, `PanDebounceMs`
- РЈРґР°Р»РµРЅ: `_onPanRefresh` РІ ZoomPanManager, `SetPanRefreshCallback()`, РІС‹Р·РѕРІ РІ `PanCanvas()`
- РџСЂРё РїР°РЅРѕСЂР°РјРёСЂРѕРІР°РЅРёРё СЃРµС‚РєР° РґРІРёР¶РµС‚СЃСЏ С‡РµСЂРµР· `RenderTransform` (TranslateTransform) вЂ” Р±РµР· СЂРµРіРµРЅРµСЂР°С†РёРё
- Р РµРіРµРЅРµСЂР°С†РёСЏ РЅР° pan-end: РїСЂСЏРјРѕР№ РІС‹Р·РѕРІ `RefreshGridNodes()` (Р±РµР· РґРµР±Р°СѓРЅСЃР°, Р±РµР· РєСЌС€Р°)

**Point 5 вЂ” Buffer safety:**
- `GridManager._nodeData` Р±РѕР»СЊС€Рµ РЅРµ `readonly` СЃ РјСѓС‚Р°С†РёРµР№ вЂ” РєР°Р¶РґС‹Р№ `RefreshGridNodes()` Р°Р»Р»РѕС†РёСЂСѓРµС‚ **РЅРѕРІС‹Р№** `long[]`
- РќРµС‚ shared mutable state РјРµР¶РґСѓ GridManager Рё GridNodesLayer
- SetNodes СЃРѕС…СЂР°РЅСЏРµС‚ СЃСЃС‹Р»РєСѓ РЅР° РјР°СЃСЃРёРІ, РєРѕС‚РѕСЂС‹Р№ РіР°СЂР°РЅС‚РёСЂРѕРІР°РЅРЅРѕ РЅРµ РјСѓС‚РёСЂСѓРµС‚СЃСЏ РїРѕСЃР»Рµ РїРµСЂРµРґР°С‡Рё

### РС‚РѕРіРѕРІС‹Р№ diff

| РњРµСЂР° | Р”Рѕ | РџРѕСЃР»Рµ |
|------|----|-------|
| GridManager СЃС‚СЂРѕРє | 246 | 145 |
| РџРѕР»РµР№ РєСЌС€Р° | 5 | 0 |
| РњРµС‚РѕРґРѕРІ pan-caching | 3 | 0 |
| CTS / Debounce | 2 (InvalidateGrid + InvalidateCacheOnPan) | 0 |
| Shared mutable long[] | Р”Р° (РѕРґРЅР° Р°Р»Р»РѕРєР°С†РёСЏ, РјСѓС‚Р°С†РёСЏ) | РќРµС‚ (РЅРѕРІР°СЏ Р°Р»Р»РѕРєР°С†РёСЏ РЅР° refresh) |
| Р РµРіРµРЅРµСЂР°С†РёСЏ РЅР° zoom | Full (pixel conv + nodes) | Full (nodes only, ~2Г— Р±С‹СЃС‚СЂРµРµ) |
| Р РµРіРµРЅРµСЂР°С†РёСЏ РЅР° pan-move | Debounced 50ms | РќРµС‚ (С‚РѕР»СЊРєРѕ RenderTransform) |
| Р РµРіРµРЅРµСЂР°С†РёСЏ РЅР° pan-end | Р’СЃРµРіРґР° + СЃР±СЂРѕСЃ РєСЌС€Р° | Р’СЃРµРіРґР° (Р±РµР· pixel conv, Р±РµР· РєСЌС€Р°) |

**Р¤Р°Р№Р»С‹:**
- `Views/GridNodesLayer.cs` вЂ” DPs, OnRender, FrameworkElement
- `ViewModels/Managers/GridManager.cs` вЂ” -105 СЃС‚СЂРѕРє (СѓРґР°Р»С‘РЅ РєСЌС€, pixel conv, debounce)
- `ViewModels/Managers/ZoomPanManager.cs` вЂ” СѓРґР°Р»С‘РЅ _onPanRefresh
- `ViewModels/EditorViewModel.cs` вЂ” СѓРґР°Р»С‘РЅ SetPanRefreshCallback
- `Behaviors/CanvasInputRouter.cs` вЂ” RefreshOnPanEnd в†’ RefreshGridNodes
- `Views/EditorCanvas.xaml` вЂ” Zoom/SheetHeightMm bindings
- `Views/EditorCanvas.xaml.cs` вЂ” СѓРїСЂРѕС‰С‘РЅ GridInvalidated handler
- `Tests/ViewModels/Managers/GridManagerTests.cs` вЂ” SuppressDebounce removed, PoolReusesSameArray в†’ AllocatesNewArrayEachCall, pixelв†’micron asserts
- `Tests/ViewModels/EditorViewModelTests.cs` вЂ” pixelв†’micron asserts

**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Sprint 60 вЂ” Fix inline text editing (auto-focus, Escape/LostFocus routing, ShortcutRegistry guard)

### 6 РёСЃРїСЂР°РІР»РµРЅРёР№

**Fix 60-1: AutoFocusOnVisibleBehavior**
- РќРѕРІС‹Р№ attached behavior: РїСЂРё `IsEnabled=True` Рё `IsVisibleChanged` в†’ `element.Focus()` + `SelectAll()` РґР»СЏ TextBox
- Р§РµСЂРµР· `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` вЂ” layout РґРѕР»Р¶РµРЅ Р·Р°РІРµСЂС€РёС‚СЊСЃСЏ
- РћС‚РїРёСЃРєР° РѕС‚ `IsVisibleChanged` РїСЂРё `IsEnabledв†’false`

**Fix 60-2: EditorCanvas.xaml вЂ” InlineTextEditor**
- Р”РѕР±Р°РІР»РµРЅ `behaviors:AutoFocusOnVisibleBehavior.IsEnabled="True"`
- Р”РѕР±Р°РІР»РµРЅ `LostFocus="InlineTextEditor_LostFocus"`

**Fix 60-3: EditorCanvas.xaml.cs вЂ” LostFocusв†’Commit**
- `InlineTextEditor_LostFocus`: РµСЃР»Рё `IsEditing`, РІС‹Р·С‹РІР°РµС‚ `CommitInlineEditingCommand`
- Р‘РµР·РѕРїР°СЃРЅРѕСЃС‚СЊ: `Commit()` РїСЂРѕРІРµСЂСЏРµС‚ `InlineEditingText==null`

**Fix 60-4/5: CanvasInputRouter вЂ” guards**
- `RoutePreviewKeyDown` Рё `RouteKeyDown`: РµСЃР»Рё `InlineEditManager.IsEditing` в†’ `return`
- Escape/Enter РїСЂРё СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёРё РЅРµ СѓС…РѕРґСЏС‚ РІ РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹

**Fix 60-6: ShortcutRegistry вЂ” guard**
- `TryHandle`: РµСЃР»Рё `InlineEditManager.IsEditing` в†’ `return false`
- V/L/R/T/E РїСЂРё СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёРё РЅРµ РїРµСЂРµРєР»СЋС‡Р°СЋС‚ РёРЅСЃС‚СЂСѓРјРµРЅС‚С‹

### РќРѕРІС‹Рµ С‚РµСЃС‚С‹
- `ShortcutRegistryTests.cs` вЂ” 7 С‚РµСЃС‚РѕРІ (V/L/R/T/E/ShiftE РїСЂРё IsEditing + РїРѕР»РѕР¶РёС‚РµР»СЊРЅС‹Р№ РєРѕРЅС‚СЂРѕР»СЊ)
- `AutoFocusOnVisibleBehaviorTests.cs` вЂ” 5 С‚РµСЃС‚РѕРІ (DP get/set + registration check)

### Common Mistakes (new)
66. `RouteKeyDown` must have the same `IsEditing` guard as `RoutePreviewKeyDown`. Without it, key events during inline editing reach the active tool and can clear selection, switch tools, or delete objects.
67. `ShortcutRegistry.TryHandle` must check `editor.InlineEditManager.IsEditing` before processing shortcuts. Without the guard, V/L/R/T/E hotkeys during inline editing switch tools or rotate objects instead of being handled by the TextBox.
68. WPF `LayoutTransform` offset on rotated elements — WPF positions a `LayoutTransform`-ed element so the **top-left of the transformed bounding box** (not the local origin `(0,0)`) lands at the layout position. For `Text` with `RotateTransform(angle, 0, 0)`, this creates an offset `(-minX, +minY)` where `minX = min(0, W·cosθ, −H·sinθ, W·cosθ−H·sinθ)` and `minY = min(0, W·sinθ, H·cosθ, W·sinθ+H·cosθ)`. Model formulas (`RotatedCorner0-3`, `ContainsPoint`, `GetBoundingBox`) MUST apply this offset to match the visual position. At 0° the offset is (0,0) — no change. `HitTestHelper.GetTextHandle` must use `Text.RotatedCorner0-3` directly (not recompute corners) to stay consistent.

**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline вЂ” РђРІС‚РѕРјР°С‚РёР·РёСЂРѕРІР°РЅРЅС‹Р№ С†РёРєР» СЂР°Р·СЂР°Р±РѕС‚РєРё (18.07.2026)

РЎРѕР·РґР°РЅ multi-agent pipeline РґР»СЏ Р°РІС‚РѕРјР°С‚РёР·Р°С†РёРё РїРѕР»РЅРѕРіРѕ С†РёРєР»Р°: Plan в†’ Implement в†’ Test в†’ Review в†’ Docs в†’ Critic в†’ PR.

### РђСЂС…РёС‚РµРєС‚СѓСЂР°

```
Conductor (primary) в†’ РґРµР»РµРіРёСЂСѓРµС‚ subagent'Р°Рј С‡РµСЂРµР· Task tool
в”њв”Ђв”Ђ planner     вЂ” read-only, РїРёС€РµС‚ СЃРїРµРєРё
в”њв”Ђв”Ђ implementor вЂ” edit + bash, РїРёС€РµС‚ РєРѕРґ
в”њв”Ђв”Ђ tester      вЂ” edit + bash, С‚РµСЃС‚С‹
в”њв”Ђв”Ђ reviewer    вЂ” read-only, РєРѕРґ-СЂРµРІСЊСЋ
в”њв”Ђв”Ђ critic      вЂ” read-only, С„РёРЅР°Р»СЊРЅС‹Р№ РєРѕРЅС‚СЂРѕР»СЊ
в””в”Ђв”Ђ gh-ops      вЂ” bash, git/gh РѕРїРµСЂР°С†РёРё
```

### РљРѕРјР°РЅРґС‹
| РљРѕРјР°РЅРґР° | РћРїРёСЃР°РЅРёРµ |
|---------|----------|
| `/pipeline full <desc>` | РџРѕР»РЅС‹Р№ С†РёРєР» СЃ Critic РІ РєРѕРЅС†Рµ |
| `/pipeline quick <desc>` | Р‘С‹СЃС‚СЂС‹Р№ С†РёРєР» (Р±РµР· plan/docs/critic) |
| `/plan <desc>` | РўРѕР»СЊРєРѕ РїР»Р°РЅРёСЂРѕРІР°РЅРёРµ |
| `/review` | РўРѕР»СЊРєРѕ СЂРµРІСЊСЋ С‚РµРєСѓС‰РёС… РёР·РјРµРЅРµРЅРёР№ |

### Files
| Path | РќР°Р·РЅР°С‡РµРЅРёРµ |
|------|-----------|
| `.opencode/agents/conductor.md` | РћСЂРєРµСЃС‚СЂР°С‚РѕСЂ (primary) |
| `.opencode/agents/planner.md` | РџР»Р°РЅРёСЂРѕРІР°РЅРёРµ |
| `.opencode/agents/implementor.md` | Р РµР°Р»РёР·Р°С†РёСЏ |
| `.opencode/agents/tester.md` | РўРµСЃС‚РёСЂРѕРІР°РЅРёРµ |
| `.opencode/agents/reviewer.md` | Code review |
| `.opencode/agents/critic.md` | Р¤РёРЅР°Р»СЊРЅС‹Р№ РєРѕРЅС‚СЂРѕР»СЊ |
| `.opencode/agents/gh-ops.md` | GitHub РѕРїРµСЂР°С†РёРё |
| `.opencode/skills/code-reviewer/SKILL.md` | РџСЂР°РІРёР»Р° СЂРµРІСЊСЋ |
| `.opencode/skills/documentation-writer/SKILL.md` | РџСЂР°РІРёР»Р° РґРѕРєСѓРјРµРЅС‚РёСЂРѕРІР°РЅРёСЏ |
| `.opencode/skills/github-workflow/SKILL.md` | git/gh РёРЅСЃС‚СЂСѓРєС†РёРё |
| `.opencode/commands/pipeline.md` | РљРѕРјР°РЅРґР° РїРѕР»РЅРѕРіРѕ pipeline |
| `.opencode/commands/pipeline-quick.md` | РљРѕРјР°РЅРґР° Р±С‹СЃС‚СЂРѕРіРѕ pipeline |
| `.opencode/commands/plan.md` | РљРѕРјР°РЅРґР° РїР»Р°РЅРёСЂРѕРІР°РЅРёСЏ |
| `.opencode/commands/review.md` | РљРѕРјР°РЅРґР° СЂРµРІСЊСЋ |
| `.github/workflows/opencode-pipeline.yml` | CI + OpenCode review |

## Pipeline вЂ” README encoding fix (18.07.2026)

### Fix README encoding
**РџСЂРѕР±Р»РµРјР°:** README.md СЃРѕРґРµСЂР¶Р°Р» UTF-8 double-encoding вЂ” СЂСѓСЃСЃРєРёР№ С‚РµРєСЃС‚ Рё СЌРјРѕРґР·Рё РѕС‚РѕР±СЂР°Р¶Р°Р»РёСЃСЊ РєР°Рє mojibake (`СЂСџвЂњвЂ№ Р С› Р СџР  Р С›Р вЂўР С™Р СћР вЂў` РІРјРµСЃС‚Рѕ `рџ“‹ Рћ РџР РћР•РљРўР•`).
**РСЃРїСЂР°РІР»РµРЅРёРµ:** 180 СЃС‚СЂРѕРє СЃ mojibake РґРµРєРѕРґРёСЂРѕРІР°РЅС‹ С‡РµСЂРµР· UTF-8 в†’ CP1251 в†’ UTF-8 СЃРµР»РµРєС‚РёРІРЅРѕ (СЃС‚СЂРѕРєР° Р·Р° СЃС‚СЂРѕРєРѕР№). 220 РїСЂР°РІРёР»СЊРЅРѕ Р·Р°РєРѕРґРёСЂРѕРІР°РЅРЅС‹С… СЃС‚СЂРѕРє СЃРѕС…СЂР°РЅРµРЅС‹ Р±РµР· РёР·РјРµРЅРµРЅРёР№.
**Р¤Р°Р№Р»:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline вЂ” README encoding fix v2 (18.07.2026)

### Fix README encoding (v2)
**РџСЂРѕР±Р»РµРјР°:** README.md СЃРЅРѕРІР° СЃРѕРґРµСЂР¶Р°Р» UTF-8 double-encoding вЂ” СЂСѓСЃСЃРєРёР№ С‚РµРєСЃС‚ Рё СЌРјРѕРґР·Рё РѕС‚РѕР±СЂР°Р¶Р°Р»РёСЃСЊ РєР°Рє mojibake.
**РСЃРїСЂР°РІР»РµРЅРёРµ:** РЎРµР»РµРєС‚РёРІРЅРѕРµ РґРµРєРѕРґРёСЂРѕРІР°РЅРёРµ СЃС‚СЂРѕРє 14вЂ“401 С‡РµСЂРµР· UTF-8 в†’ CP1251 в†’ UTF-8. РЎС‚СЂРѕРєРё 1вЂ“13 РЅРµ Р·Р°С‚СЂРѕРЅСѓС‚С‹.
**Р¤Р°Р№Р»:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline вЂ” CI/CD GitHub Actions (18.07.2026)

### Feature CI workflow
**РџСЂРѕР±Р»РµРјР°:** РћС‚СЃСѓС‚СЃС‚РІРѕРІР°Р» CI/CD вЂ” PR РЅРµ РїСЂРѕРІРµСЂСЏР»РёСЃСЊ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРё, coverage РЅРµ РєРѕРЅС‚СЂРѕР»РёСЂРѕРІР°Р»СЃСЏ.
**РСЃРїСЂР°РІР»РµРЅРёРµ:** Р”РѕР±Р°РІР»РµРЅ `.github/workflows/ci.yml` вЂ” build + test + coverage gate 75% РЅР° `windows-latest` СЃ NuGet РєСЌС€РёСЂРѕРІР°РЅРёРµРј.
**Р¤Р°Р№Р»:** `.github/workflows/ci.yml`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Sprint 61 — Text rotation marker fix (LayoutTransform offset)

### Fix S61-1: Rotated text markers offset at non-zero angles

**Проблема:** Маркеры выделения текста (4 квадрата по углам) корректно отображались только при угле поворота 0°. При других углах (45°, 90°, 135°, 180°, 270°) маркеры были смещены относительно реальных углов повёрнутого текста.

**Причина:** `TextBlock` использует WPF `LayoutTransform = RotateTransform(angle, 0, 0)`. WPF позиционирует трансформированный элемент так, что **верхний левый угол трансформированного bounding box** (а НЕ origin `(0,0)`) попадает в точку layout position `(Canvas.Left, Canvas.Top)`. Это создаёт смещение `(-minX, +minY)` между anchor `(MicronsX, MicronsY+HeightMicrons)` и фактическим центром вращения. Модельные формулы `RotatedCorner0-3`, `ContainsPoint()`, `GetBoundingBox()` не учитывали это смещение.

**Исправление:**
- `Text.cs` — добавлен `GetLayoutTransformOffset()` private helper, вычисляющий `(minX, minY)` — верхний левый угол трансформированного bounding box в локальных Y-down координатах. `RotatedCorner0-3` (8 свойств), `ContainsPoint()`, `GetBoundingBox()` обновлены с применением offset `(-minX, +minY)`.
- `HitTestHelper.cs` — `GetTextHandle()` упрощён: использует `text.RotatedCorner0-3` напрямую (single source of truth) вместо независимого вычисления углов без offset.
- Тесты: `TextTests.cs` (обновлены ожидаемые значения + 4 новых теста), `HitTestHelperTests.cs` (обновлены stale test points для rotated text hit-testing).

**Файлы:**
- `Models/Objects/Text.cs` — GetLayoutTransformOffset + RotatedCorner0-3 + ContainsPoint + GetBoundingBox
- `Helpers/HitTestHelper.cs` — GetTextHandle simplified
- `Tests/Models/Objects/TextTests.cs` — updated + new tests
- `Tests/Helpers/HitTestHelperTests.cs` — updated test points

**Build:** 0 errors, 0 warnings
**Tests:** 2069 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint 62 — STA unit tests for TabItemMiddleClickBehavior and PreviewLineChangedBehavior

### Feature: TabItemMiddleClickBehaviorTests (12 tests)
**Проблема:** TabItemMiddleClickBehavior не имел unit-тестов из-за STA-зависимости (TabControl, TabItem, MouseButtonEventArgs).
**Исправление:** 
- `OnEnableMiddleClickToCloseChanged`, `OnPreviewMouseUp` сделаны `internal static` (по паттерну других behavior-тестов)
- Создан `TabItemMiddleClickBehaviorTests.cs`: 4 DP-теста (без STA) + 6 handler-тестов (STA via WpfContext.Execute) + 2 event-subscription теста
- Тесты проверяют: middle-click на TabItem → CloseTabRequestMessage, wrong button → no-op, non-TabItem sender → no-op, subscription lifecycle

### Feature: PreviewLineChangedBehaviorTests (11 tests)
**Проблема:** PreviewLineChangedBehavior не имел unit-тестов из-за STA-зависимости (Canvas с named-элементами).
**Исправление:**
- `CachedElements`, `UpdatePreviewLine`, `UpdatePreviewRectangle`, `UpdatePreviewText` сделаны `internal`/`internal static`
- Создан `PreviewLineChangedBehaviorTests.cs`: 4 Register/Unregister теста + 6 update-тестов + 1 PropertyChanged flow тест
- Тесты проверяют: валидный preview → Visible + позиция, null preview → Collapsed, double registration → no throw

**Файлы:**
- `Behaviors/TabItemMiddleClickBehavior.cs` — 2 изменения visibility
- `Behaviors/PreviewLineChangedBehavior.cs` — 4 изменения visibility
- `Tests/Behaviors/TabItemMiddleClickBehaviorTests.cs` — создан (12 тестов)
- `Tests/Behaviors/PreviewLineChangedBehaviorTests.cs` — создан (11 тестов)

**Build:** 0 errors, 0 warnings
**Tests:** 2092 passed, 1 pre-existing skip
**Coverage:** 75.3% line-rate ✅

## Sprint 63 — Template.Clone() regression test

### Feature: Clone_CopiesAllPublicProperties_ExceptId regression test
**Проблема:** `Template.Clone()` может потерять консистентность при добавлении новых свойств в будущем — нет теста, проверяющего, что все публичные свойства (кроме `Id`) скопированы.
**Исправление:** Добавлен тест `Clone_CopiesAllPublicProperties_ExceptId` в `TemplateTests.cs`, который через reflection проверяет, что каждое публичное свойство `Template` (кроме `Id`) имеет одинаковое значение на исходном и клонированном объекте после `Clone()`.
**Файлы:**
- `Tests/Services/TemplateTests.cs` — добавлен regression test

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint — Архитектурный рефакторинг P2 — ITabOperationsService (21.07.2026)

### Feature: MainViewModel DI reduction
**Проблема:** 13 зависимостей в конструкторе MainViewModel — потенциальный god-class.
**Решение:** Создан ITabOperationsService — фасад для операций с вкладками (NewTab, OpenFile, Save, SaveAs). Конструктор сокращён с 13 до 10 параметров.

**Файлы:**
- `ViewModels/Abstractions/ITabOperationsService.cs` (новый)
- `Services/TabOperationsService.cs` (новый)
- `ViewModels/MainViewModel.cs` (рефакторинг)
- `App.xaml.cs` (DI-регистрация)
- `EditorViewModelFactory.cs` (sealed)

### Fix: Command naming consistency
**Проблема:** Тесты `MoveObjectCommand_*`, `RotateObjectCommand_*` не отражают реализацию через `ChangePropertyCommand<T>`.
**Решение:** 14 тестов переименованы: `MoveObjectCommand_*` → `ChangePropertyCommand_Move_*`, и т.д.

**Файлы:**
- `Tests/Commands/CommandTests.cs`

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅