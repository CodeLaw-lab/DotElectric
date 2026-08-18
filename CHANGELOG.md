# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Sprint 63: Regression test `Clone_CopiesAllPublicProperties_ExceptId` for `Template.Clone()` — reflection-based check against future property drift
- Pipeline: увеличение покрытия тестами до 75.15% (было ~59-67%)
- ~195 новых тестов: Commands null guards, Tools Reset(), Grid edge cases, Services, Models, MainViewModel, FontMetrics, TemplateObjectBase, NonZeroToVisibilityConverter, CustomSheetDialogViewModel, ShortcutRegistry
- Template.Clone() — deep copy с Metadata, Sheet и Objects
- PointMicrons operator+ и operator- (компонентное сложение/вычитание)

### Changed
- Кандидат 3 обзора №4 — CommandHistory единолично владеет грязностью (#98, тикет #99, PR #101): `CommandHistory.Push`/`Undo`/`Redo` стреляют `markDirty` ровно один раз после успешного выполнения (rollback при исключении не стреляет); делегат `markDirty` удалён из всех команд (`AddObjectCommand`, `DeleteObjectCommand`, `ChangePropertyCommand<T>` — оба конструктора; трёхпараметровый конструктор `BatchCommand` удалён целиком), 9 точек передачи делегата исчезли; ручная компенсация `MarkDirty()` в `EditorViewModel.Undo()`/`Redo()` удалена; волна сужения конструкторов: база панелей свойств тройка→пара, 3 sub-VM, `PropertiesViewModel`, `InlineEditManager`; член `MarkDirty` удалён из `IEditorContext` (27 → 26 членов); поведение байт-в-байт, XAML не менялся; термин «Грязность» в CONTEXT.md; тесты 2674, line-rate 90.08%
- Кандидат 2 обзора №4 — preview-сейм (#93, тикет #94, PR #96): PreviewManager — единственный источник истины preview-состояния. `IEditorContext` выставляет `PreviewManager` вместо трёх свойств `PreviewLine`/`PreviewRectangle`/`PreviewText` (29 → 27 членов), 3 forwarding-свойства EditorViewModel удалены целиком. Ре-ассайн-трюк удалён структурно: инструменты ассайнят preview-объект только при создании и обнулении, на MouseMove — только мутация свойств; рендерер `PreviewLineChangedBehavior` подписан на INPC preview-объекта (отписка при swap/clear/unregister), читает только из PreviewManager, координаты через `RenderRules`/`Coordinate.ToMm` напрямую без экземпляров конвертеров. PreviewManager переведён на `[ObservableProperty]` (контракт «уведомление только при смене ссылки», workaround R3.1-HF1 удалён), мёртвый `SelectionBoxRight` снесён. Визуально байт-в-байт, XAML не менялся; термин «Предпросмотр» в CONTEXT.md; тесты 2691, line-rate 90.13%
- Кандидат 1 обзора №4 — один конвейер рендеринга (#88, тикет #89, PR #91): правила рендеринга «модель → примитивы» собраны в единый модуль `RenderRules` (Helpers, static): карта ГОСТ-шрифтов, frozen dash-карта, hex→Brush, `ModelYToTop(yMicrons, sheetHeightMm, scale)`, карта выравнивания текста, anchor-политика на тип объекта (явный throw для неизвестных типов). Три конвейера — тонкие потребители: конвертеры канваса делегируют (байт-в-байт, XAML не менялся), preview ставит верх текста по `HeightMicrons` через правила, печать удалила приватные дубли (hex/dash/шрифт/выравнивание) и ставит текст по канвас-семантике (`MicronsY + HeightMicrons`, LayoutTransform при повороте), Y-flip слоя сетки через правила; копии правил удалены бесследно; dash-коллекции frozen (unfrozen кэш thread-affine); тесты 2685, line-rate 90.04%
- Кандидат 7 обзора №4 — пан — жест роутера (#84, PR #85): `PanTool` удалён целиком (класс, фабрика ToolRegistry, 20 тестов) — панорамирование стало жестом `CanvasInputRouter` (`IsPanGesture`/`RoutePanDown`/`ApplyPan`) с применением через `ZoomPanManager.PanCanvas`; live-рассинхронизация двух источников истины (роутер vs `PanTool._isPanning` на Space+Left без Alt) устранена структурно; член `PanCanvas` удалён из `IEditorContext` (30 → 29 членов); поведение байт-в-байт; «Пан» в CONTEXT.md — жест, не инструмент; первый ADR проекта `docs/adr/0001-pan-gesture-not-tool.md`; тесты 2619, line-rate 89.89%
- Кандидат 1 обзора №3, срез 1 — рамка выделения через узкий шов (#75–#76, PR #78): 7 свойств `IEditorContext` (`SelectionBoxLeft/Bottom/Top/Width/Height/Right`, `SelectionDirection`) заменены двумя методами `SetSelectionBox(...)`/`ClearSelectionBox()` с делегацией в PreviewManager (интерфейс 35 → 30 членов, включая удаление 2 мёртвых read-only); 7 forwarding-свойств EditorViewModel удалены; 4 блока SelectTool заменены одиночными вызовами (направление через `SelectionBoxHelper.GetDirection`); глубокие методы PreviewManager получили первого production-потребителя; XAML и PreviewManager не изменялись; поведение байт-в-байт; тесты 2622, line-rate 89.85%
- Кандидат 7 обзора №3 — deletion-пара (#70–#71, PR #73): «Вписать в экран» переведён на живой viewport ZoomPanManager — строковый параметр команды и fallback 800×600 удалены целиком (все три XAML-входа считали для 800×600 независимо от окна), безаргументный `FitToScreen()` с no-op guard при viewport 0×0; рамка выделения переведена на полиморфный `GetBoundingBox()` — три частных копии границ и мёртвый default-кейс удалены; публичный API и LTR/RTL-семантика не изменены; поведение байт-в-байт (единственное изменение — исправленный дефект вписывания); термины «Вписать в экран» и «Рамка выделения» в CONTEXT.md; тесты 2622, line-rate 89.86%
- Рефакторинг настроек приложения «типизированный интерфейс» (#65–#66, атомарный flip): `ISettingsService` сужен до `Load()` + `Save(AppSettings)` — строковые `Get<T>`/`Set<T>` и оба switch-диспетчера (12+12) удалены целиком, 19 вызовов в 5 классах мигрированы на типизированный доступ; `AppSettings` перенесён Services → Models (инверсия слоёв устранена), `CustomSettings` удалён; создание вкладки пишет файл настроек один раз вместо двух; дефекты (сломанная типовая проверка `LastUsedSheetOrientation`, двойная запись файла, хрупкий round-trip) исчезли структурно; схема settings.json байт-совместима; интеграционные тесты переведены на temp-файлы; тесты 2619, line-rate 89.91%
- Рефакторинг инструментов «ToolRegistry» (#53–#57, expand→migrate→contract): идентичность инструментов — enum `ToolKind`; ToolManager переименован в глубокий `ToolRegistry` (фабрики/кэш, `ActiveToolKind`/`ActiveToolInstance`, `Stack<ToolKind>`, `SwitchTo` с reset'ом предыдущего); строковые карты (4 шт.) и строковая поверхность удалены целиком; мёртвый `ITool.Name` удалён из интерфейса и 6 реализаций; `IEditorContext` без WPF ICommand (типизированные `PushTool`/`PopTool`/`ActivateTool`); роутер без строкового switch и silent-default; XAML — `{x:Static tools:ToolKind.X}` + OneWay; карта горячих клавиш Key→ToolKind в ShortcutRegistry; поведение байт-в-байт; тесты 2646, line-rate 89.98%
- Рефакторинг панелей свойств «Глубокая база» (#45–#48): новая абстрактная база `ObjectPropertiesViewModel<TObject>` (конструктор-тройка CommandHistory/markDirty/setValidationError, декларативная nameof-карта `PropertyMap` для диспетчеризации INPC и notify-all, `UpdateObject`, `Dispose`, `SetProperty<T>`, `ChangeFromMmString`, `ParseLineType`); миграция всех 3 sub-VM — Line (карта 7 пар), Rectangle (карта 8 пар, afterSet Width→X / Height→Y), Text (карта 13 пар; особые команды `ChangeContent`/`ChangeDefaultValue`/`ChangeFontNameFromString` с null-coalescing сохранены в sub-VM); дублирование инфраструктуры sub-VM устранено полностью; XAML, `PropertiesViewModel`-держатель и code-behind не изменены; +13 тестов механики карты
- AGENTS.md: обновлены счётчики тестов (2035) и покрытие (75.15%)

### Fixed
- Preview-текст при рисовании ставился по `FontSizeMicrons` вместо `HeightMicrons` — верх preview теперь совпадает с позицией текста после коммита (канвас-семантика через RenderRules)
- Печать ставила текст по `MicronsY` без высоты и без LayoutTransform-offset при повороте — текст в print preview в позиции канваса, включая повёрнутый и многострочный (#88, PR #91)
- «Вписать в экран» (меню, тулбар, Ctrl+0) всегда вычислял масштаб для viewport 800×600 независимо от фактического размера окна — теперь используется живой viewport редактора
- CI: Added missing `--configuration Release` to Build step in `opencode-pipeline.yml` — was causing test failures due to Debug/Release mismatch
- README.md: повторное исправление кодировки (UTF-8 double-encoding / mojibake)
- README.md: исправлена кодировка (UTF-8 double-encoding / mojibake) — восстановлены русский текст и эмодзи
- H1: `AutosaveService` event `Action?` → `Func<Task>?` + `InvokeAsync` in `IDispatcherService` (eliminates async-void in `MainViewModel`)
- H2: `IValidationService` interface added; `TemplateValidator` uses injectable `IValidationService` instead of static `ValidationService.ValidateHexColor()`
- H3: `DialogServiceFactory` dead code removed from `IDialogService.cs`
- H4: `PrintVisualProvider` nulled in `EditorViewModel.Dispose()` (dangling reference cleanup)
- H5: No-op `Свойства (F4)` MenuItem removed from `EditorCanvas.xaml` context menu
- TemplateTests.cs: исправлен синтаксис (лишняя закрывающая скобка)
- Documentation: исправлены 27 ошибок в 9 md-файлах — битые ссылки на archive, EditorConstants → PhysicalConstants/EditorSettings, устаревшие метрики, XAML-биндинги в docs/09, placeholder URL, фактические ошибки в README, docs/19 динамика покрытия
- Sprint 60: Inline text editing — AutoFocusOnVisibleBehavior, CanvasInputRouter guards (Escape/Enter during edit), ShortcutRegistry guard (V/L/R/T/E blocked during edit)
- Sprint 60: `RouteKeyDown` guard for `IsEditing` (matching existing `RoutePreviewKeyDown` guard)
- Sprint 60: LostFocus → Commit for inline text editor
- Sprint "Fix Session 2 bugs": StatusBar info for text selection — shows font name and size
- Sprint "Fix Session 2 bugs": MultiLine — `AcceptsReturn="True"` unconditional (always Enter=newline)
- Sprint "Fix Session 2 bugs": Ctrl+Enter/Escape routing — conflicting UserControl.InputBindings removed, routing via TextBox.InputBindings
- Sprint "Fix Session 2 bugs": TextAlignment binding in inline editor TextBox
- Sprint "Fix Session 2 bugs": IsEditable=false guard in `OnDoubleClick()` and `InlineEditManager.Start()` (defense-in-depth)
- Sprint 61: Text rotation marker fix — `GetLayoutTransformOffset()` in Text.cs accounts for WPF `LayoutTransform` bounding box offset at non-zero angles
- Sprint 61: `HitTestHelper.GetTextHandle()` simplified to use `Text.RotatedCorner0-3` directly
- Sprint 61: Updated TextTests.cs (4 new tests) and HitTestHelperTests.cs (stale rotated text test points)

### Added
- Sprint 62: STA-based unit tests for TabItemMiddleClickBehavior (12 tests) — middle-click-close, button filtering, sender validation, event subscription lifecycle
- Sprint 62: STA-based unit tests for PreviewLineChangedBehavior (11 tests) — register/unregister, update preview shapes, null preview handling, PropertyChanged flow
- CI workflow: GitHub Actions (build, test, coverage gate 75%) на `windows-latest`
- Sprint STA: `WpfContext` helper — STA-thread dispatcher for WPF unit tests
- Sprint STA: `TextBoxLostFocusCommandBehaviorTests` — 14 tests (DP get/set, OnLostFocus, OnKeyDown Enter via STA)
- Sprint STA: `ComboBoxSelectionChangedCommandBehaviorTests` — 10 tests (DP get/set, OnSelectionChanged via STA)
- Sprint STA: `ZoomComboBoxBehaviorTests` — 11 tests (DP get/set, ApplyZoom parsing, events via real EditorViewModel)
- Sprint STA: `MarkerPositionTests` — 10 tests (DP get/set for XPropertyPath/YPropertyPath)
- Sprint STA: 4 behavior files made handlers `internal static` for testability (matching existing CanvasInputRouter pattern)
- Sprint 54: `IDialogFileService`/`WpfDialogFileService` — WPF dialog isolation from FileService for CI/testability
- Sprint 54: FileService now delegates `OpenFileDialog()`/`SaveFileDialog()` to `IDialogFileService` (optional DI)
- Sprint 54: 5 dialog tests rewritten with `Mock<IDialogFileService>` (zero UI calls in headless)
- Sprint 53: `IDateTimeProvider`/`DateTimeProvider` — abstraction over `DateTime.UtcNow`, injected into 3 services
- Sprint 53: `MarkerPosition` attached behavior — `XPropertyPath`/`YPropertyPath` auto-create Canvas.Left/Top MultiBindings (XAML markers reduced from 250→40 lines)
- Sprint 53: `EditorCanvasBehaviorTests` — 18 unit tests for `ToToolMouseButton`, `ToToolModifiers`, `ToToolKey`
- Sprint 53: `[InternalsVisibleTo]` — methods changed from `private` to `internal static` for testability
- Sprint 53: All `Thread.Sleep` removed from test code (5 files, ~2310ms total), replaced with `Mock<IDateTimeProvider>`
- Sprint 52: Free rotation for text (0-359°, `cos`/`sin` math in `ContainsPoint`/`GetBoundingBox`)
- Sprint 52: GOST font names fixed (`#GOST Type AU`/`BU`) with FontNameToFamilyConverter
- Sprint 52: Double-click inline text editing with rotation-aligned TextBox
- Sprint 51: Panning `CaptureMouse()`/`ReleaseMouseCapture()` for stable drag outside canvas
- Sprint 50: Clipboard (Copy/Paste/Cut) with 10mm offset, BatchCommand, auto-select, statusbar
- Sprint 50: Ctrl+X shortcut, toolbar/menu buttons for Cut
- Sprint 49: Edge-based minimum-size clamp in ResizeTool (moving edges only)
- Sprint 48: Dirty indicator `*` in tab header via PropertyChanged forwarding
- Sprint 47: Grid 1mm step hidden at low zoom (MinPixelSpacing=5px in GridManager + GridHelper)
- Sprint 46: Right-click context menu on canvas, tab context menu close commands
- Sprint 46: `CloseTab`/`CloseOtherTabs`/`CloseAllTabs` (renamed from `*Async` for RelayCommand)
- Sprint 45: Pan delta from Window-relative coordinates (fixes runaway acceleration)
- Sprint 44: `PropertiesViewModel` subscribes to `INotifyPropertyChanged` on selected object
- Sprint 44: Text INPC for all properties (FontSizeMicrons, Content, FontName, TextType, RotationAngle)
- Sprint 43: `GetCurrentTool()` `case "Resize"` dispatch fix
- Sprint 42: `StrokeThicknessMicrons` end-to-end (default 500µ = 0.5mm, converter + INPC)
- Sprint 41: Drag delta from saved initial position (`_initialPositions[obj]`), fixes drift
- Sprint 41: Text INPC implementations (MicronsX/Y backing fields + Right/Bottom/Center notifications)
- Sprint 40: Layout-independent keyboard shortcuts via `PreviewKeyDown` (V/L/R/T, E/Shift+E)
- Sprint 40: Selection markers via `ItemsControl ItemsSource="{Binding SelectedObjects}"` (multi-select)
- Sprint 39: Rectangle border-band hit-test (not full AABB, LineHitToleranceMicrons=5mm)
- Sprint 38: INPC for Line/Rectangle (LineType, coordinates, dimensions)
- Sprint 38: `LineTypeToIndexConverter` for ComboBox binding
- Sprint 38: `PurgeOrphanedSelection()` after Undo/Redo
- Sprint 38: `TextBoxLostFocusCommandBehavior` with Enter key handling
- Sprint 38: DrawingRectangleTool passes `_lineType` to `CalculateRectangle()`
- Sprint 37: `IsObjectSelectedConverter` + DataTrigger visual selection (#0078D4 highlight)
- Sprint 37: Preview shape re-assign pattern (create once, update in MouseMove, re-assign reference)
- Sprint 37: `SelectionVersion` (int) for binding re-evaluation
- Sprint 37: `OnPropertyChanged(nameof(Zoom))` in `OnZoomChangedInternal`
- Sprint 37: Escape → `ActiveTool = "Select"` in all tools
- Sprint 31: Decomposed EditorViewModel into 9 managers (ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar, Grid, DirtyState)
- Sprint 31: 21 new manager unit tests

### Changed
- Sprint 38: DrawingRectangleTool `CalculateRectangle()` accepts lineType parameter
- Sprint 37: `ToModelPoint()` no longer subtracts PanOffset (e.GetPosition already accounts for RenderTransform)
- Sprint 28: Split `CommonConverters.cs` into 16 individual converter files
- Sprint 28: Renamed `ITemplateObject` → `TemplateObjectBase` across 50+ files
- Sprint 27: `EditorViewModel` reduced from ~1037 to ~700 lines (-32%)
- Documentation: XAML-биндинги в docs/09_UI_решения.md обновлены до manager-свойств (после R3.1/A.2)

### Fixed
- Sprint 41: Drag delta accumulation (was `obj.MicronsX + delta`, now `initialPos + delta`)
- Sprint 40: Tool switching not working with Russian keyboard layout (InputBindings → PreviewKeyDown)
- Sprint 40: `SetActiveTool()` bypassing `ActiveTool` setter (no `OnPropertyChanged`)
- Sprint 39: Rectangle interior area selectable (now border-band only)
- Sprint 38: ComboBox not showing current LineType value (missing SelectedIndex binding)
- Sprint 38: Undo leaving orphaned selection markers
- Sprint 37: Preview shapes not appearing (reference not re-assigned)
- Sprint 37: Canvas not resizing on zoom (missing OnPropertyChanged)
- Sprint 27: Restored missing `HitTest` method in `HitTestHelper`
- Sprint 27: Fixed `ResizeHandle` namespace import

### Added
- Sprint "Архитектурный рефакторинг P2": `ITabOperationsService` facade for tab operations (NewTab, OpenFile, Save, SaveAs)
- `ViewModels/Abstractions/ITabOperationsService.cs` (new interface)
- `Services/TabOperationsService.cs` (new implementation)
- Sprint "Grid Refactoring": `IGridNodeGenerator`/`GridNodeGenerator` (DI Singleton) — вся логика сетки инъектируема; `GridNode` — top-level struct в `Helpers/GridNode.cs`
- Sprint "Grid Refactoring": `GridSettings` новые поля — `MaxGridNodes` (default 250000), `NodeColor` (null = авто по теме), `NodeSize` (default 2.0)
- Sprint "Grid Refactoring": Settings UI — 3 новых поля в секции СЕТКА (Макс. узлов, Цвет узлов с чекбоксом «Авто (по теме)», Размер узлов Slider 1-6)
- Sprint "Grid Refactoring": `IThemeService.ThemeChanged` event; `EditorViewModel.IsDarkTheme` проброс; `GridNodesLayer.IsDarkTheme` DP → `UpdateThemeBrush` (Light #C0C0C0 / Dark #808080)
- Sprint "Grid Refactoring": `GridNodeColorConverter` (HEX → Brush, null/invalid → темо-зависимый fallback) и `InverseBooleanConverter`
- Sprint "Grid Refactoring": `GridNodeGeneratorTests` (+36), `GridManagerTests` переписан (32), конвертеры (+12), SettingsViewModel (+4), ThemeService ThemeChanged (+3), SettingsService round-trip (+3)

### Changed
- Sprint "Архитектурный рефакторинг P2": MainViewModel constructor reduced from 13 to 10 dependencies (removed `_templateService`, `_fileService`, `_printService`, `_printDocumentGenerator`, `_editorViewModelFactory`; added `_tabOperations`)
- Sprint "Архитектурный рефакторинг P2": 14 test methods renamed in `CommandTests.cs` — `MoveObjectCommand_*` → `ChangePropertyCommand_Move_*`, etc.
- Sprint "Архитектурный рефакторинг P2": `EditorViewModelFactory` marked `sealed`
- Sprint "Grid Refactoring": `GridHelper` (static) → `IGridNodeGenerator` (DI Singleton); `GridManager` принимает генератор через DI, реализует `IDisposable`
- Sprint "Grid Refactoring": узлы сетки генерируются в абсолютных координатах листа (0,0 = нижний левый угол) для всей площади — панорамирование не вызывает регенерацию (RenderTransform)
- Sprint "Grid Refactoring": `GridManager.Nodes` — `IReadOnlyList<GridNode>`, новый список на каждый refresh (нет shared mutable state); удалены `ViewportMargin`, `_nodeBuffer`, `RawNodeData`/`RawNodeCount`
- Sprint "Grid Refactoring": `Template` переведён на `ObservableObject` (INPC на `Sheet` → регенерация сетки при смене формата листа)
- Sprint "Grid Refactoring": `ComputeDisplayStep` уважает пользовательский шаг если укладывается в бюджет и pixel-spacing

### Removed
- Sprint "Grid Refactoring": `GridHelper.cs` и `GridHelperTests.cs` удалены (−21 viewport-тест, устарели)

### Fixed
- Sprint "Grid Refactoring": сетка больше не исчезает молча из-за бюджета `MaxGridNodes` — defense-in-depth coarsen (удвоение шага до вписывания в бюджет), `GenerateGridNodes` никогда не возвращает пустой список из-за бюджета

### Metrics
- **Tests:** 2140 (2139 passed, 0 failures, 1 pre-existing skip)
- **Coverage:** 76.3% line-rate ✅ (GridManager/GridNodeGenerator/GridNode/SettingsViewModel/ThemeService/GridNodeColorConverter/InverseBooleanConverter — 100%)
- **Build:** 0 errors, 0 warnings
- **P0/P1 bugs:** 0
- **EditorViewModel:** ~784 lines (9 managers, post R3.1 de-bloat)
- **DI services:** IDateTimeProvider, IDialogFileService, IPrintVisualProvider, ITemplateValidator, IEditorContext, ITabOperationsService, IGridNodeGenerator

### Added
- Sprint "AppSettings → GridSettings chain": `GridSettings.FromAppSettings(AppSettings)` static factory (6 полей: ShowGrid→Enabled, SnapToGrid→SnapEnabled, GridStepMm→StepMicrons, GridMaxNodes→MaxGridNodes, GridNodeColor→NodeColor, GridNodeSize→NodeSize; clamping: StepMicrons ≥ 1 мкм, MaxGridNodes ≥ 1, NodeSize не NaN/∞/≤0); `GridSettings` → sealed; `EditorSettings.DefaultGridNodeSize = 2.0`
- Sprint "AppSettings → GridSettings chain": `EditorViewModelFactory.ResolveGridSettings()` (explicit gridSettings → AppSettings → FromDefaultGrid), опциональный `ISettingsService?` в ctor — настройки сетки из Settings UI применяются к новым/открытым вкладкам (Create, CreateWithFilePath; TabOperationsService: CreateNewTab, OpenFileAsync, OpenFromFilePath, CreateNewCustomTab)
- Sprint "AppSettings → GridSettings chain": +18 тестов (9 FromAppSettings + 6 factory + 3 по ревью)

### Metrics
- **Tests:** 2160 (2159 passed, 0 failures, 1 pre-existing skip)
- **Coverage:** 76.4% line-rate ✅
- **Build:** 0 errors, 0 warnings

### Added
- Sprint "Tech debt + coverage": Text markers tech debt закрыт — regression-тест `RotatedCorners_AllLieOnBoundingBoxEdges` (6 углов: 0/45/90/135/180/270°) подтверждает, что маркеры выделения (RotatedCorner0-3) лежат на границе `GetBoundingBox()`; подтверждено: `TextSelectionMarkerBehavior` не существует, пустой `<Canvas/>` внутри DataTemplate Text удалён, маркеры Text рендерятся в ItemsControl через `MarkerPosition` (RotatedCorner0-3X/Y)
- Sprint "Tech debt + coverage": Inline edit guards защищены тестами — +5 тестов InlineEditManager (Start_NonEditable_NoOp, Commit_UnchangedText_NoCommand, Commit_Twice_PushesSingleCommand, Cancel_WhenNotEditing_NoThrow, Start_WhileEditing_SwitchesObject) + 2 STA-теста AutoFocusOnVisibleBehavior (реальный фокус + SelectAll)
- Sprint "Tech debt + coverage": WPF-обёртки изолированы — `internal static` handlers в 5 production-файлах (WpfMessageBoxProvider: ToWpfButtons/ToWpfIcon/ToMsgrResult; WpfDispatcherService: ctor с `Dispatcher?`; WpfDialogFileService: CreateOpenDialog/CreateSaveDialog; WpfDialogHostService: ResolveWindowDescriptor; ThemeDictionaryManager: FindThemeDictionary) + 27 тестов (WpfMessageBoxProviderTests 11, WpfDispatcherServiceTests 3 STA, WpfDialogFileServiceTests 6 STA, WpfDialogHostServiceTests 2, ThemeDictionaryManagerTests 4 STA, PrintDialogFactoryTests 1 STA)
- Sprint "Tech debt + coverage": покрытие повышено до 80.22% (было 76.4%, +3.82 п.п., ≥80% gate) — ConverterTests (+13), AutosaveServiceTests (+5), SettingsServiceTests (+6), PropertiesViewModelTests (+41)

### Changed
- Sprint "Tech debt + coverage": закрыты 6 MINOR-замечаний ревью — удалены misleading-тесты (AutoFocus fake, WpfApplicationLifecycle placeholder, 4 FitToPageScale тавтологии), `CustomResizeCommandTests.cs` → `ChangePropertyCommandResizeTests.cs`, `ThemeDictionaryManagerTests` + `ThemeDictionaryManagerTestCollection` (DisableParallelization), AutoFocus retry-Activate

### Metrics
- **Tests:** 2295 total (2294 passed, 0 failures, 1 pre-existing skip)
- **Coverage:** 80.22% line-rate ✅
- **Build:** 0 errors, 0 warnings

### Added
- Coverage sprint series (11–13.08.2026): input routing chain (CanvasInputRouter/EditorCanvasState/CoordinateTransform/EditorCanvasBehavior — internal static + 69 STA/pure тестов; CanvasInputRouter 97.2%, остальные 100%), TabOperationsService 24%→100% (+51), MainViewModel async flows → 97.1% (+35), TemplateLibraryService/ViewModel 35%/51%→100% (+41), dialog wrappers (WpfDialogHostService 100%, WpfDispatcherService 92.3%, PrintDialogWrapper, +18)

### Changed
- CI: coverage gate raised 75% → 80% (ci.yml + opencode-pipeline.yml); actual 88.45% line-rate
- CHANGELOG.md: UTF-8 BOM removed (content encoding already clean) — file is now BOM-less UTF-8 (CRLF preserved)

### Metrics
- **Tests:** 2515 total (2514 passed, 0 failures, 1 pre-existing skip)
- **Coverage:** 88.45% line-rate ✅
- **Build:** 0 errors, 0 warnings

### Added
- Sprint "Coverage weak zones" (14.08.2026): ResizeMathTests.cs — новый файл, 78 unit-кейсов (ClampLong, ComputeRectangleResize/ComputeLineResize/ComputeTextResize, ApplyTextFontSizeClamp, IsResizeHandle, CursorForHandle) — ResizeMath 97.35%; PanToolTests +8 → 100%; FontMetricsTests +15 → 91.11% (было ~40%); ConverterTests +2 → IsNullConverter/NotNullToVisibilityConverter 100%; ValidationServiceTests +24 → TemplateValidator 98.95% (было 65–93%). *(Примечание от 17.08.2026: список функций в скобках был неточным уже на момент записи; фактический API ResizeMath: ComputeRectangleResize, ComputeTextResize, ComputeLineEndpoint, CursorForHandle, VisualCursorForHandle — см. AGENTS.md WZ-1)*

### Changed
- FontMetrics: рефакторинг тестируемости — `ComputeAverageAdvanceWidth` → internal static, `SampleChars` → static readonly, fallback через `ApplyFallback`/`HandleFallbackWithLog` (поведение-эквивалентно, публичные сигнатуры не тронуты)

### Fixed
- TemplateValidator: null-sheet guard — `Validate()` ранний return V-006 при `Sheet == null` (фикс NRE, дублирование N+1→1); `ValidateObjectCoordinates` guard V-006 (защита `ValidateObject` path)

### Metrics
- **Tests:** 2636 total (2635 passed, 0 failures, 1 pre-existing skip)
- **Coverage:** 90.18% line-rate ✅
- **Build:** 0 errors, 0 warnings

---

## [0.1.0] — 2026-04-01

### Added
- Initial release of DotElectric Template Editor
- WPF .NET 10 CAD application for electrical template design
- MVVM architecture with CommunityToolkit.Mvvm
- DI via Microsoft.Extensions.DependencyInjection
- Fixed-point coordinate system (microns)
- `.tdel` file format (XML in ZIP)
- Undo/Redo with 50 levels via `CommandHistory`
- Tools: Select, Rectangle, Line, Text, Resize
- Grid with snap-to-grid functionality
- Zoom/Pan with mouse wheel and drag
- Properties panel for object editing
- Template library with drag & drop
- Autosave service with session recovery
- Settings service with JSON persistence
- Print service with FitToPage scaling
- xUnit v3 test suite (1,394 tests)

[Unreleased]: https://github.com/anomalyco/dotelectric/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/anomalyco/dotelectric/releases/tag/v0.1.0

