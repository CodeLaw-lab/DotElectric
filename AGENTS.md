# AGENTS.md — DotElectric

## Current Focus

**Кандидат 5 архитектурного обзора №5 — «Deletion sweep №2: мёртвое знание приложения» завершён (20.08.2026, спека #152, тикет #153, PR #155): тесты 2603 (2603 passed, 0 пропусков; распределение: приложение 2072 + Document.Tests 404 + Sheets.Tests 127), line-rate 91.13% (gate 80%) — без изменений.** Мёртвое и расходящееся знание приложения удалено одним проходом: `TextToolSettings` удалён (четыре дефолта инлайнены в `TextTool`, осиротевшие сеттеры `SetTextType`/`SetFontSize`/`SetDefaultContent` удалены с тестами); класс `Grid` удалён — шаг сетки живёт одной константой `EditorSettings.DefaultGridStepMicrons` (`GridSettings.FromDefaultGrid` переименован в `CreateDefault` — имя не ссылается на несуществующую сущность); мёртвая настройка `DefaultZoom` удалена целиком (модель + VM + ComboBox «Масштаб:» + тесты; settings.json байт-совместим — неизвестные ключи игнорируются); мёртвые константы `DoubleClickThresholdMs`/`DefaultSheetOffsetMm` удалены, `NudgeStepMicrons` переиспользован со значением 100 и подключён в `EditorViewModel.NudgeStep` (расхождение «константа 1 мм vs хардкод 0.1 мм» устранено, поведение сохранено); метки ориентации «кн.»/«алб.» — одна статическая точка `Helpers/OrientationLabels.For` (три делегата, неизвестная ориентация — явный throw); Rotate ±90 — приватное ядро со знаком как параметром, команды — тонкие делегаты; mojibake восстановлен (86 строк в `AutosaveService`/`MainViewModel`/`IMessageBoxProvider`, включая 11 строковых литералов логов); `SnapHelper.SnapObject` удалён с 3 тестами; три теста-реликта `ValidationService_ValidateObject*` перенесены в `Document.Tests`; поведение байт-в-байт (единственное видимое изменение — восстановленные строки логов). Ранее: **Кандидат 4 архитектурного обзора №5 — «Добить миграцию: тесты библиотек застряли в приложении» завершён (20.08.2026, спека #147, тикет #148, PR #150): тесты 2613 (2613 passed, 0 пропусков; распределение: приложение 2085 + Document.Tests 401 + Sheets.Tests 127), line-rate 91.13% (gate 80%) — точно как базовая линия.** Тесты библиотечных типов перенесены из app-проекта в родные проекты библиотек: V-правила тремя источниками слиты в один `Document.Tests/TemplateValidatorTests.cs` (75 тестов, имя-реликт «ValidationServiceTests» устранено) + `HexColorValidationTests.cs`; новые `TemplateTests.cs`/`MetadataTests.cs`/`RectMicronsTests.cs` в `Document.Tests` и `SheetTests.cs` в `Sheets.Tests`; влито в существующие `CoordinateTests`/`PointMicronsTests`/`TemplateServiceTests`; четыре локальные фабрики шаблона переехавших тестов слиты в общую фикстуру `TestTemplates`; app-остатки смешанных файлов влиты в родительские (конвенция R4.4), `Models/TemplateTests.cs` переименован в `GridSettingsTests.cs`; 5 файлов-источников и 3 реликтовых отчёта удалены; `FixedFontMetrics`/`FontMetricsTestCollection` ×2 — осознанные копии (коллекция xUnit обязана жить в сборке тестов); ни один тест не добавлен и не удалён; production-код и XAML — ноль изменений (чистый перенос). Ранее: **Кандидат 3 архитектурного обзора №5 — «Один шов проверки документа» завершён (20.08.2026, спека #142, тикет #143, PR #145): тесты 2613 (2613 passed, 0 пропусков), line-rate 91.13% (gate 80%).** Проверка документа выставлена одним швом: обёртка `ITemplateService.Validate → IEnumerable<string>` (побайтно дублировала `ValidationError.ToString()`, прятала фильтр серьёзности, несла мёртвую null-ветку) удалена из интерфейса (5 → 4 члена) и из `TemplateService` (метод, поле, параметр конструктора, запасное создание); единственный потребитель `TabOperationsService` инжектит `ITemplateValidator` напрямую — фильтр «в диалог только `Error`» и склейка сообщений (`ToString()` через `\n`) живут в потребителе побайтно; цепочка цвета сокращена: поле-посредник `ValidationService.Default` и обёртка `ValidationService.ValidateHexColor` удалены (DI регистрирует `HexColorValidation.Default` напрямую; три панели свойств вызывают `HexColorValidation.Validate`), `IValidationService` сохранён (шов конструктора `TemplateValidator`, два мока в тестах); документация приведена к поведению (XML-комментарий `ValidationSeverity.Error`, статья «Правила проверки документа» в CONTEXT.md); 6 тестов удалённой обёртки удалены (включая единственный пропуск набора), путь сохранения на моке валидатора + пин-тест «предупреждения не показывают диалог»; поведение байт-в-байт (XAML — 0 изменений). Ранее: **Кандидат 1 архитектурного обзора №5 — «Документ без редактора» завершён (20.08.2026, спека #137, тикет #138, PR #140): тесты 2617 (2616 passed + 1 pre-existing skip), line-rate 91.11% (gate 80%).** Библиотека `DotElectric.Document` перестала нести знание редактора: `PhysicalConstants` переименован в `DocumentConstants` и несёт только `LineHitToleranceMicrons` (единственная константа, которую читает сам документ — попадание в тело линии/прямоугольника); шесть констант взаимодействия (`HandleHitToleranceMicrons`, `SelectionBoxThresholdMicrons`, `MinResizeSizeMicrons`, `MinFontSizeMicrons`, `MinDimensionMicrons`, `MaxCustomSheetSizeMm`) перенесены в `EditorSettings` (секция Interaction, имена и значения сохранены); привязка к сетке удалена из библиотеки целиком (`Coordinate.SnapToGrid`, `PointMicrons.SnapToGrid`) — формула живёт приватным скалярным ядром в `SnapHelper` (публичная поверхность без изменений, семантика исключения побайтно; CONTEXT.md: «Сетка — не понятие документа»); дубль `MicronsPerMm` устранён (потребители на `Coordinate.MicronsPerMm`, включая хардкод слоя узлов сетки); комментарий `Template.Sheet` без имени потребителя из приложения; 6 тестов формулы переехали из `DotElectric.Document.Tests` в `SnapHelperTests`; поведение байт-в-байт (XAML — 0 изменений). Ранее: кандидат 6 архитектурного обзора №4 — «TextGeometry из модели» завершён (19.08.2026, спека #118, тикет #119, PR #121): тесты 2630 (2629 passed + 1 pre-existing skip), line-rate 90.34% (gate 80%). WPF-знание о геометрии повёрнутого текста (~60% класса `Text`) вынесено в статический модуль `TextGeometry` (Helpers, прецеденты RenderRules/MarkerLayout): `LayoutOffset` (смещение WPF LayoutTransform — повёрнутый элемент позиционируется по верхнему левому углу трансформированного bounding box), `Corner` (углы 0–3 в порядке каталога MarkerLayout, индекс вне диапазона — явный throw), `Contains` (обратное вращение вокруг фактического центра вращения), `BoundingBox`. Модель `Text` — тонкие делегации: 8 свойств повёрнутых углов (XAML маркеров — 0 изменений) + полиморфные `ContainsPoint`/`GetBoundingBox`; INPC-проводка (`NotifyAllRotatedCorners`, partial-хуки) осталась обязанностью модели; 393 → 217 строк. Мёртвые швы удалены: `VisualLeft/VisualRight/VisualBottom/VisualTop` ×4 вместе с INPC-обвязкой (0 потребителей в production/XAML/тестах) и `RotationAngleValid` (тавтология после свободного поворота). 12 геометрических тестов мигрированы в `TextGeometryTests` (offset-тесты переписаны на прямой вызов `LayoutOffset` — раньше private без прямых тестов), `RotatedCorners_AllLieOnBoundingBoxEdges` остался пином поверхности свойств для XAML; print-дефект обзора («повёрнутый текст смещён в print preview») проверен на коде и не подтвердился — печать и канвас компенсируются WPF-раскладкой одинаково (фикс кандидата 1); XAML и все потребители (MarkerLayout, HitTestHelper, SelectionBoxHelper, PrintDocumentGenerator, PreviewLineChangedBehavior) — 0 изменений; поведение байт-в-байт. Ранее: кандидат 5 архитектурного обзора №4 — «Форматы листа — один каталог» завершён (19.08.2026, спека #113, тикет #114, PR #116): тесты 2630 (2629 passed + 1 pre-existing skip), line-rate 90.38% (gate 80%). Знание о стандартных форматах листа собрано в статический `SheetFormatCatalog` (Models) + sealed record `SheetFormat` (имя, длинная/короткая сторона в микронах, ориентация по умолчанию): 11 точек знания (два switch'а `Sheet`, HashSet `ValidFormats` валидатора, `FormatOptions` настроек, 20 MenuItem меню с захардкоженными размерами в заголовках, 5 кнопок диалога произвольного размера, парсер суффиксов P/L, дефолт «A3» ×6) стали потребителями каталога; строковая идентичность формата зафиксирована ADR-0002 (сериализуется в .tdel/settings.json); `FromFormat`/`GetDefaultOrientation` делегируют каталогу — единый throw, молчаливый Landscape удалён; валидатор — `Contains` + Custom (V-006 перечисляет канонические 10 + Custom без латинских дублей, файлы с `A4X2` по-прежнему валидны); меню «Файл > Новый шаблон» генерируется из каталога через ItemsSource (заголовки компонуются из размеров, байт-в-байт); мёртвая настройка `DefaultSheetFormat` оживает fallback'ом в цепочке создания вкладки; дефолт «A3» ×6 — через `const DefaultName`; диалог — TryGet-no-op вместо глотания исключений; термины «Стандартный формат» и «Пользовательский формат» внесены в CONTEXT.md; поведение байт-в-байт. Ранее: кандидат 8 архитектурного обзора №4 — «Мёртвые швы — deletion sweep» завершён (18.08.2026, спека #108, тикет #109, PR #111): тесты 2576 (2575 passed + 1 pre-existing skip), line-rate 90.6% (gate 80%). Весь код с нулём production-потребителей удалён вместе с тестами: callback-параметры ZoomPanManager (wiring сетки — только `SetGridRefreshCallback`), `_templateService` EditorViewModel (поле + параметр обоих конструкторов + проброс фабрики) и `_gridNodeGenerator`-поле (локальная переменная конструктора), `ITextToolSettings` целиком (DI-регистрация, интерфейс, опциональный параметр TextTool), DI-регистрация `IFontMetrics`, 4 конвертера без биндингов (ZoomToString/LineTypeToString/TextTypeToString/RelativeMicronsToPixel), `ValidationService.ValidateRotation`; ресурсы всех конвертеров консолидированы в корневом словаре приложения — каждый класс объявлен ровно один раз (исключение — view-локальный `InverseBooleanConverter` в SettingsView: окно загружается в STA-тестах без ресурсов приложения); контракт Автосохранения типизирован (`Template` вместо `object`, мёртвый сеттер `FilePath` удалён из интерфейса), горизонт очистки — константа `AutosaveCleanupDays`; механизм Автосохранения сохранён (решение Q5-b), read-поверхность (`LoadSession`/`GetAutosaveFilePath`/`ClearAutosaveFolder`) сознательно оставлена до продуктовой спеки восстановления; `EnumToIndexConverter` sealed; поведение байт-в-байт; термин «Автосохранение» внесён в CONTEXT.md. Ранее: кандидат 4 архитектурного обзора №4 — «Раскладка маркеров — один модуль» завершён (18.08.2026, спека #103, тикет #104, PR #106): тесты 2614 (2613 passed + 1 pre-existing skip), line-rate 90.25% (gate 80%). Знание «какие маркеры выделения есть у объекта и где они» собрано в один статический модуль `MarkerLayout` (Helpers): каталог маркеров по типу объекта в порядке приоритета hit-проверки (у линии конец первым), позиции в модельных координатах, hit-тест с допуском `min(8 мм, minDim/3)` (семантика фикса #82), классификация (угол/ребро/конец линии), курсорная политика. Четыре копии удалены: `HitTestHelper` сжат до хита по телу (зоны/допуск/per-type методы + 6 мёртвых публичных методов удалены целиком), `ResizeMath` остался чистой математикой drag'а (курсорные функции и 4 набора классификации переехали в модуль), `ResizeTool` лишился 8 ручных снапшот-полей (единственный модельный `ResizeState`), `IEditorContext` потерял `HoveredHandle` (26 → 25 членов; hover — приватное поле SelectTool). Четыре silent-default ветки заменены явными throw; enum `ResizeHandle` определён единственно в MarkerLayout; поведение байт-в-байт (XAML не менялся); термин «Маркер выделения» внесён в CONTEXT.md. Ранее: кандидат 3 архитектурного обзора №4 — «CommandHistory единолично владеет грязностью» завершён (18.08.2026, спека #98, тикет #99, PR #101): тесты 2674 (2673 passed + 1 pre-existing skip), line-rate 90.08% (gate 80%). `CommandHistory` — единственный источник грязности шаблона: `Push`/`Undo`/`Redo` стреляют `markDirty` ровно один раз в конце метода после успешного перемещения команды по стекам (rollback-путь при исключении не стреляет). Делегат `markDirty` удалён из всех команд (`AddObjectCommand`, `DeleteObjectCommand`, `ChangePropertyCommand<T>` — оба конструктора; трёхпараметровый конструктор `BatchCommand` удалён целиком) — 8 командных invoke'ов и 9 точек передачи делегата исчезли; ручная компенсация `MarkDirty()` в `EditorViewModel.Undo()`/`Redo()` удалена (хвостовые UI-refresh relay-команд сохранены). Волна сужения конструкторов: база панелей свойств `ObjectPropertiesViewModel<TObject>` тройка→пара, 3 sub-VM, `PropertiesViewModel`, `InlineEditManager`. Член `MarkDirty` удалён из `IEditorContext` (27 → 26 членов); публичные `MarkDirty()`/`ClearDirty()` VM сохранены. Поведение байт-в-байт (XAML не менялся); термин «Грязность» внесён в CONTEXT.md; checkpoint-семантика сохранной точки — на радаре. Ранее: кандидат 2 архитектурного обзора №4 — «Preview-сейм» завершён (18.08.2026, спека #93, тикет #94, PR #96): тесты 2691 (2690 passed + 1 pre-existing skip), line-rate 90.13% (gate 80%). PreviewManager — единственный источник истины preview-состояния: `IEditorContext` выставляет `PreviewManager` вместо трёх свойств `PreviewLine`/`PreviewRectangle`/`PreviewText` (29 → 27 членов), 3 forwarding-свойства EditorViewModel удалены целиком. Ре-ассайн-трюк (мутация + ре-ассайн той же ссылки «чтобы уведомить») удалён структурно: инструменты ассайнят preview-объект только при создании и обнулении, на MouseMove — только мутация свойств; рендерер `PreviewLineChangedBehavior` подписан на INPC preview-объекта (отписка при swap/clear/unregister), читает только из PreviewManager и ходит в `RenderRules`/`Coordinate.ToMm` напрямую без экземпляров конвертеров. PreviewManager: `[ObservableProperty]` вместо ручных сеттеров с безусловным notify (workaround R3.1-HF1 умер); мёртвый `SelectionBoxRight` удалён. Визуально байт-в-байт (XAML не менялся); термин «Предпросмотр» внесён в CONTEXT.md. Ранее: кандидат 1 архитектурного обзора №4 — «Один конвейер рендеринга» завершён (18.08.2026, спека #88, тикет #89, PR #91): тесты 2685 (2684 passed + 1 pre-existing skip), line-rate 90.04% (gate 80%). Правила рендеринга «модель → примитивы» собраны в один статический модуль `RenderRules` (Helpers): карта ГОСТ-шрифтов, frozen dash-карта, hex→Brush, `ModelYToTop(yMicrons, sheetHeightMm, scale)`, карта выравнивания текста, anchor-политика на тип объекта (единый dispatch, явный throw для неизвестных типов — без silent-default). Три конвейера — тонкие потребители: 7 конвертеров канваса делегируют в правила (XAML не менялся; 216 тестов конвертеров без изменений, +1 новый = байт-паритет), preview-behavior ставит верх текста по `HeightMicrons` через правила (сейм не перестраивался — кандидат 2), печать удалила 4 приватных дубля (hex/dash/шрифт/выравнивание) и ставит текст по канвас-семантике (`MicronsY + HeightMicrons`, LayoutTransform при повороте), Y-flip слоя сетки через правила. Два живых дефекта устранены: preview-текст ставился по `FontSizeMicrons` вместо `HeightMicrons`; печать ставила текст по `MicronsY` без высоты и без компенсации поворота. Единственные видимые изменения — исправленные дефекты; копии правил исчезли бесследно (deletion-проверка); попутно frozen dash-коллекции (unfrozen кэш thread-affine). Ранее: кандидат 7 архитектурного обзора №4 — «Пан — жест роутера» завершён (18.08.2026, спека #84, PR #85): тесты 2619 (2618 passed + 1 pre-existing skip), line-rate 89.89% (gate 80%). `PanTool` удалён целиком: панорамирование — жест `CanvasInputRouter` (`IsPanGesture`/`RoutePanDown`/`ApplyPan`) с применением через `ZoomPanManager.PanCanvas` — live-рассинхронизация двух источников истины (роутер vs `PanTool._isPanning` на Space+Left без Alt) устранена структурно; член `PanCanvas` удалён из `IEditorContext` (30 → 29 членов); фабрика ToolRegistry и 20 тестов PanTool удалены; поведение байт-в-байт (дельта в Window-координатах с Y-flip, capture, RefreshGridNodes по концу, курсор SizeAll, пермиссивные жесты Middle | Left+Space | Left+Alt); глоссарий CONTEXT.md обновлён («Пан» — жест, не инструмент); первый ADR проекта — `docs/adr/0001-pan-gesture-not-tool.md`. Счётчики включают fix #82 (допуск маркеров, влит в develop до среза). Ранее: кандидат 1 архитектурного обзора №3 — срез 1 «Рамка выделения через узкий шов» завершён (17.08.2026): тесты 2622 (2621 passed + 1 pre-existing skip), line-rate 89.85% (gate 80%). Рамка выделения ставится и очищается двумя методами — `SetSelectionBox(left, bottom, width, height, direction)` + `ClearSelectionBox()` — вместо 7 свойств `IEditorContext`: семь членов интерфейса удалены (включая 2 мёртвых read-only — верхний и правый края), IEditorContext сжат с 35 до 30 членов; 7 forwarding-свойств EditorViewModel удалены целиком; 4 блока SelectTool (установка 5 свойств в OnMouseMove + три обнуления в OnMouseDown/OnMouseUp/Reset) заменены одиночными вызовами, направление через `SelectionBoxHelper.GetDirection` (дубль формулы устранён); глубокие методы PreviewManager получили первого production-потребителя; XAML не изменялся (все биндинги рамки уже на PreviewManager); поведение байт-в-байт (threshold 3 мм, LTR/RTL-семантика, порядок INPC-уведомлений). Первый срез кандидата 1 «фасад EditorViewModel / IEditorContext → узкие role-seams»; preview-тройка и остальные кластеры — будущие отдельные спеки. Ранее: кандидат 7 — deletion-пара (#70–#71): «Вписать в экран» переведён на живой viewport ZoomPanManager: строковый параметр команды и fallback 800×600 удалены целиком — все три XAML-входа (Ctrl+0 с хардкодом, меню, тулбар) считали для 800×600 независимо от окна, живой пользовательский дефект устранён; безаргументный `FitToScreen()` с no-op guard при viewport 0×0, overload `(double,double)` удалён. Рамка выделения переведена на полиморфный `GetBoundingBox()`: три частных копии границ и мёртвый default-кейс удалены, циклы вызывают метод модели напрямую; публичный API и LTR/RTL-семантика не изменены. Поведение байт-в-байт (единственное изменение — исправленный дефект вписывания); regression-тест фиксирует живой viewport; термины «Вписать в экран» и «Рамка выделения» внесены в CONTEXT.md. Ранее: рефакторинг настроек приложения «типизированный интерфейс» (#65–#66) — `ISettingsService` сужен до двух методов — `Load()` + `Save(AppSettings)`: строковые `Get<T>`/`Set<T>` и оба switch-диспетчера (12+12 кейсов) удалены целиком, 19 вызовов в 5 классах (SettingsViewModel, MainViewModel, ThemeService, AutosaveService, TabOperationsService) мигрированы на типизированный доступ; `AppSettings` перенесён Services → Models (инверсия слоёв устранена), мёртвый escape-hatch `CustomSettings` удалён; создание вкладки пишет файл настроек один раз вместо двух. Три дефекта исчезли структурно: сломанная типовая проверка `LastUsedSheetOrientation`, двойная запись файла, хрупкий culture-зависимый round-trip. Схема settings.json байт-совместима (legacy-ключ игнорируется при чтении), семантика Load/Save (кэш, corrupt → дефолты, null-guard) не изменена, поведение байт-в-байт. Ранее: рефакторинг инструментов «ToolRegistry» (#53–#57) — идентичность инструментов — enum `ToolKind` (Select/Line/Rectangle/Text/Resize; Пан — type-адресуемый, без идентичности); единый глубокий module `ToolRegistry` (`ViewModels/Managers/ToolRegistry.cs`, переименован из ToolManager): фабрики+кэш, `ActiveToolKind`/`ActiveToolInstance`, `Stack<ToolKind>`, `SwitchTo` с reset'ом предыдущего. Удалены: 4 строковые карты (ToolManager×2, switch роутера, XAML-параметры), строковая поверхность (`ActiveTool`-string, `PushTool(string)`, `ResetTool(string)`, `ToolNameMap`), мёртвый `ITool.Name` (0 потребителей), silent-default роутера; `IEditorContext` без WPF ICommand (типизированные `PushTool`/`PopTool`/`ActivateTool`); XAML — `{x:Static tools:ToolKind.X}` + OneWay; карта клавиш Key→ToolKind — в ShortcutRegistry (§3.1: WPF-типы не в ViewModels). Поведение байт-в-байт; рефакторинг панелей свойств «Глубокая база» (#45–#48) — создана глубокая база `ObjectPropertiesViewModel<TObject>` (`ViewModels/ObjectPropertiesViewModel.cs`): конструктор-тройка (CommandHistory?, markDirty, setValidationError), абстрактная декларативная nameof-карта `PropertyMap` («свойство модели → свойство VM») для диспетчеризации INPC и notify-all, `UpdateObject` (отписка → присвоение → подписка → notify-all), `Dispose` с отпиской, `SetProperty<T>` (валидация → `ChangePropertyCommand<T>` → уведомление → afterSet), `ChangeFromMmString`, `ParseLineType`. Мигрированы все 3 sub-VM: `LinePropertiesViewModel` (карта 7 пар, 13 RelayCommand), `RectanglePropertiesViewModel` (карта 8 пар, 14 RelayCommand, afterSet Width→X / Height→Y), `TextPropertiesViewModel` (карта 13 пар, 18 RelayCommand; особые команды с null-coalescing `ChangeContent`/`ChangeDefaultValue`/`ChangeFontNameFromString` сохранены в sub-VM, поведение неизменно). Инфраструктурное дублирование трёх sub-VM устранено полностью; XAML, `PropertiesViewModel`-держатель и code-behind не изменены; Sprint "Coverage weak zones" (14.08.2026) — 6 слабых зон покрытия закрыты (ResizeMath 97.35%, PanTool 100%, FontMetrics 91.11%, TemplateValidator 98.95%, IsNull/NotNullToVisibility 100%), line-rate 90.18%, 2636 тестов.

### Ключевые результаты
| Область | Было | Стало |
|---------|------|-------|
| Иерархия моделей | 3 уровня (ObjectBase→ModelBase→TemplateObjectBase) | 1 уровень (TemplateObjectBase→ObservableObject) |
| INPC-сеттеры | ~50 ручных | [ObservableProperty] sourcegen |
| EditorCanvasBehavior | 406 строк (монолит) | 78 строк (3 файла: State, Transform, Router) |
| Tools-EditorVM | Прямая зависимость | IEditorContext |
| DI-конструктор | internal + ручная фабрика | public + Transient + ActivatorUtilities |
| PrintVisualProvider | Func<Visual?> | IPrintVisualProvider |
| Validation | static ValidationService (537 строк) | ITemplateValidator |
| Resize | 520 строк (switch) | ResizeMath + полиморфный ApplyResize |
| Shortcuts | switch в code-behind | ShortcutRegistry |
| Extended тесты | 16 файлов | все слиты в родительские |
| CI | нет coverage-gate, нет NuGet кэша | coverage-gate 80% + actions/cache |
| CPM | нет | Directory.Packages.props |
| **Print Preview** | **нет** | **Ctrl+Shift+P → DocumentViewer** |
| **EditorConstants** | **36-line proxy** | **удалён → PhysicalConstants/EditorSettings** |
| **FontMetrics** | **static class** | **IFontMetrics (тестовые моки) + статический FontMetrics.Default в production; DI-регистрация удалена** |
| **Sealed classes** | **0** | **68 классов (Converters, Services, Tools, Managers, Commands)** |
| **Shortcut dispatch** | **code-behind 30 строк** | **ShortcutRegistry.TryHandle()** |
| **ITool.OnMouseWheel** | **void** | **bool (tool может блокировать zoom)** |
| **GridHelper** | **static class** | **IGridNodeGenerator/GridNodeGenerator (DI Singleton)** |
| **Grid nodes** | **viewport-координаты + регенерация на pan** | **абсолютные координаты листа, pan = RenderTransform** |
| **Grid settings chain** | **настройки сохранялись, но не применялись к вкладкам** | **FromAppSettings → применяются ко всем новым/открытым вкладкам** |
| **Text markers tech debt** | **открыт (Sprint 61: маркеры смещены при повороте)** | **закрыт: regression-тест RotatedCorners_AllLieOnBoundingBoxEdges (6 углов), маркеры на границе GetBoundingBox()** |
| **WPF-обёртки** | **private логика (нетестируема)** | **internal static handlers + 27 тестов (unit + STA)** |
| **Coverage** | **76.4% line-rate** | **90.34% line-rate (gate 80% достигнут)** |
| **CI coverage gate** | **75%** | **80% (факт 90.34%)** |
| **Weak zones coverage** | **6 зон 40–93% (FontMetrics ~40%, TemplateValidator 65–93%)** | **все цели ≥90%: FontMetrics 91.11%, TemplateValidator 98.95%, ResizeMath 97.35%, PanTool/IsNull/NotNullToVisibility 100%** |
| **Панели свойств** | **3 sub-VM с дублирующейся инфраструктурой (UpdateObject/Dispose/INPC-dispatch/SetProperty в каждой)** | **глубокая база ObjectPropertiesViewModel<TObject> + декларативные nameof-карты (7/8/13 пар)** |
| **Идентичность инструментов** | **строки в 4 несинхронизированных картах, silent-default роутера, WPF ICommand в инструментах** | **enum ToolKind + глубокий ToolRegistry (фабрики/кэш/active/стек/SwitchTo); ITool без Name** |
| **Настройки приложения** | **строковый Get/Set + 2 switch-диспетчера (12+12), 12 ключей-литералов в 5 классах, тройной дефект** | **типизированный ISettingsService (Load/Save); AppSettings в Models; CustomSettings удалён; одна запись файла на вкладку** |
| **FitToScreen** | **строковый параметр + fallback 800×600 (все три входа считали для 800×600 независимо от окна)** | **живой viewport ZoomPanManager; безаргументная команда; no-op при 0×0; overload удалён** |
| **Рамка выделения (границы)** | **3 частных копии границ объектов + мёртвый default-кейс** | **полиморфный GetBoundingBox() модели; публичный API и LTR/RTL-семантика не изменены** |
| **Рамка выделения (шов)** | **7 свойств IEditorContext, 7 forwarding-свойств VM, 4 блока по 5 записей в SelectTool** | **2 метода SetSelectionBox/ClearSelectionBox → PreviewManager; IEditorContext 35→30; поведение байт-в-байт** |
| **Пан (панорамирование)** | **PanTool + дубль состояния роутера (live-рассинхрон на Space+Left без Alt)** | **жест роутера IsPanGesture/RoutePanDown/ApplyPan; PanTool удалён; IEditorContext 30→29; ADR-0001** |
| **Конвейеры рендеринга** | **3 независимых конвейера (канвас/preview/печать): шрифт ×3, dash ×2, hex ×2, Y-flip ×5 копий; 3 разных anchor'а верха текста** | **один модуль RenderRules (карты + Y-flip + anchor-политика на тип); поверхности — тонкие делегаты; 2 дефекта anchor'ов устранены** |
| **Preview-сейм** | **3 forwarding-свойства VM + 3 члена IEditorContext; ре-ассайн-трюк на каждый MouseMove; рендерер читает через фасад** | **PreviewManager — единственный источник истины; рендерер подписан на INPC preview-объекта; IEditorContext 29→27; трюк удалён структурно; байт-в-байт** |
| **Грязность** | **2 канала (команда + история): 8 командных invoke'ов, Undo/Redo истории не стреляют, VM компенсирует вручную; 9 точек передачи делегата** | **CommandHistory — единственный источник (Push/Undo/Redo, один выстрел, rollback не стреляет); команды без markDirty; IEditorContext 27→26; байт-в-байт** |
| **Раскладка маркеров** | **4 копии геометрии: зоны/допуски в HitTestHelper, курсоры/классификация в ResizeMath, per-type снапшот в ResizeTool; silent-default'ы `_ => null`/`(0,0)`/Arrow** | **один модуль MarkerLayout (каталог с приоритетом, позиции, hit с допуском #82, классификация, курсоры); HitTestHelper — хит по телу (6 мёртвых методов удалены); ResizeMath — чистая математика; ResizeTool — единственный ResizeState; IEditorContext 26→25; байт-в-байт** |
| **Мёртвые швы** | **callback-параметры ZoomPanManager, мёртвые зависимости EditorViewModel (`_templateService`, `_gridNodeGenerator`-поле), `ITextToolSettings`, DI-регистрация `IFontMetrics`, 4 конвертера без биндингов, `ValidateRotation`, ресурсы конвертеров в 4 словарях (дубли + неиспользуемый ключ `Not`)** | **deletion sweep: всё с 0 потребителей удалено вместе с тестами; каждый конвертер объявлен ровно один раз в корневом словаре (InverseBooleanConverter view-локально для STA-тестов); контракт Автосохранения типизирован (без мёртвого сеттера), механизм сохранён; байт-в-байт** |
| **Форматы листа** | **11 точек знания: switch'и `Sheet`, HashSet валидатора, список настроек, 20 MenuItem с размерами в заголовках, 5 кнопок диалога, парсер суффиксов, дефолт «A3» ×6; латинские X-дубли расходятся с настройками; мёртвая настройка формата** | **один каталог `SheetFormatCatalog` + record `SheetFormat` (Models, строковая идентичность — ADR-0002); все поверхности — потребители; меню генерируется из каталога; мёртвая настройка оживает fallback'ом; байт-в-байт** |
| **Геометрия повёрнутого текста** | **модель `Text` с WPF-знанием ~60% класса (393 строки: LayoutTransform-offset, 8 повёрнутых углов, повёрнутые contains/bounding box); мёртвые `Visual*`×4 и тавтология `RotationAngleValid`** | **один модуль `TextGeometry` (Helpers: `LayoutOffset`/`Corner`/`Contains`/`BoundingBox`) — единственный носитель формул поворота; модель — тонкие делегации (XAML маркеров 0 изменений); мёртвые швы удалены; байт-в-байт** |
| **Документ без редактора** | **библиотека несла 6 из 8 констант взаимодействия редактора + семантику привязки к сетке при живом CONTEXT.md «Сетка — не понятие документа»; дубль `MicronsPerMm` (3 копии)** | **`DocumentConstants` (единственный член — допуск хита тела, читает сам документ); 6 констант взаимодействия в `EditorSettings` (секция Interaction); привязка к сетке — только `SnapHelper` (приватное скалярное ядро); одна публичная `Coordinate.MicronsPerMm`; байт-в-байт** |
| **Проверка документа** | **две поверхности: обёртка `ITemplateService.Validate → IEnumerable<string>` (побайтный дубль `ToString()`, спрятанный фильтр серьёзности, мёртвая null-ветка) + типизированный `ITemplateValidator`; цвет — цепочка трёх хопов вокруг одной функции** | **один шов: `ITemplateService` 5→4 члена (только чтение/запись); `TabOperationsService` инжектит `ITemplateValidator` напрямую (фильтр «только `Error`» + формат — в потребителе, побайтно); мёртвая null-ветка удалена; цвет — `HexColorValidation` напрямую (DI-регистрация `HexColorValidation.Default`, панели вызывают её; `IValidationService` сохранён); 6 тестов обёртки удалены, + пин-тест предупреждений; байт-в-байт** |
| **Тесты библиотек** | **тесты библиотечных типов в app-проекте (V-правила в файле с именем-реликтом `ValidationServiceTests`, Template/Coordinate/Sheet/сериализация вперемешку с app-тестами); 4 локальные фабрики шаблона; реликтовые отчёты** | **тесты едут с кодом: `Document.Tests` (+TemplateValidatorTests 75 / HexColorValidationTests / TemplateTests / MetadataTests / RectMicronsTests, слияния в CoordinateTests / PointMicronsTests / TemplateServiceTests, общая фикстура TestTemplates), `Sheets.Tests` (+SheetTests); 5 файлов-источников + 3 реликта удалены; 2613 без добавлений и удалений; покрытие 91.13% без изменений** |
| **Мёртвое знание (deletion sweep №2)** | **`TextToolSettings`-посредник, класс `Grid` ради одной константы (шаг 5 мм в трёх копиях), настройка-призрак `DefaultZoom` (пишется и показывается, не применяется), мёртвые константы + расхождение nudge (константа 1 мм vs хардкод 0.1 мм), метки ориентации ×3, дубль Rotate ±90, mojibake в трёх файлах, осиротевшие методы** | **всё удалено одним проходом: дефолты инлайнены в `TextTool`; шаг сетки — одна константа (`GridSettings.CreateDefault`); `DefaultZoom` удалён целиком (settings.json байт-совместим); `NudgeStepMicrons` → 100 и подключён в `NudgeStep`; `OrientationLabels.For` — одна точка (явный throw); Rotate — приватное ядро со знаком; 86 строк кодировки восстановлено; тесты 2613 → 2603 (10 тестов удалённого кода), покрытие 91.13% без изменений** |

**Build:** 0 errors, 0 warnings
**Tests:** 2603 total (2603 passed, 0 пропусков)

### H1–H5 — Архитектурные исправления высокой важности (14.07.2026)
- **H1: async-void AutosaveTick** — `event Action?` → `event Func<Task>?`. `IDispatcherService` получил `InvokeAsync(Func<Task>)`. `AutosaveService.OnAutosaveTick` вызывает `InvokeAsync`. `MainViewModel` — `async Task` вместо `async void`.
- **H2: ValidationService → injectable** — Создан `IValidationService` (интерфейс с `ValidateHexColor`). `ValidationService` содержит статический `Default` (instance-обёртка). `TemplateValidator` принимает `IValidationService` через DI (опциональный параметр для обратной совместимости). Статические методы `ValidationService` сохранены.
- **H3: DialogServiceFactory удалён** — мёртвый код (public static class, не используется) удалён из `IDialogService.cs`.
- **H4: PrintVisualProvider null-out** — `PrintVisualProvider = null` добавлен в `EditorViewModel.Dispose()`.
- **H5: No-op F4 MenuItem удалён** — `<MenuItem Header="Свойства" InputGestureText="F4">` удалён из контекстного меню `EditorCanvas.xaml`.

### Что сделано после R1–R4
- **EditorViewModel де-bloat** — ~1194 → 784 строк (−410, −34%). Удалены ~25 forwarding-свойств, 4 PropertyChanged-обработчика, 4 подписки/отписки. Свойства IEditorContext оставлены как bare delegation (без OnPropertyChanged). IAutosaveTab — explicit interface implementation.
- **Preview fix** — `[ObservableProperty]` на `PreviewLine`/`PreviewRectangle`/`PreviewText` подавлял PropertyChanged при re-assign той же ссылки. Заменён на ручные сеттеры с безусловным `OnPropertyChanged()`.
- **Selection markers fix** — `ShowSelectionMarkers` (computed property) не вызывал `OnPropertyChanged()` при изменении `SelectedObjects`. Добавлен вызов в `CollectionChanged`-обработчик.
- **PropertiesViewModel split** — 649 → 85 строк (база). 3 sub-VM: LinePropertiesViewModel (148), RectanglePropertiesViewModel (168), TextPropertiesViewModel (233). XAML: 3 StackPanel → ContentControl + DataTemplate per sub-VM.
- **Print Preview** — Ctrl+Shift+P открывает DocumentViewer с FixedDocument. IPrintDocumentGenerator, PrintDocumentGenerator, PrintPreviewWindow. 19 тестов.
- **Text rotation fix (Sprint 59)** — ContainsPoint() исправлен на inverse WPF RotateTransform (standard CCW matrix). RotatedCorner0-3, GetBoundingBox — reverted к оригинальным (корректным) формулам. HitTestHelper/HitTestText для 90°/270°/45° — все проходят. Осознана и зафиксирована матрица WPF `x'=x*cosθ−y*sinθ`. Архитектурный инсайт: ContainsPoint() был багнут (forward вместо inverse) независимо от путаницы со знаками.

### Что не вошло / отложено
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior — STA-тесты (требуют полного визуального дерева)~~ — решено в Sprint 62
- ~~**Text markers — tech debt:** исправлен поворот (`RotatedCorner0–3`, `GetBoundingBox`, `HitTestHelper`), но остаются недочёты отображения маркеров: `TextSelectionMarkerBehavior` не используется, пустой `<Canvas/>` внутри DataTemplate Text, маркеры в отдельном ItemsControl вместо внутри DataTemplate~~ — **закрыт в Sprint "Tech debt + coverage" (11.08.2026):** `TextSelectionMarkerBehavior` не существует, пустой `<Canvas/>` удалён, маркеры Text рендерятся в ItemsControl через `MarkerPosition` (RotatedCorner0-3X/Y); regression-тест `RotatedCorners_AllLieOnBoundingBoxEdges` (6 углов: 0/45/90/135/180/270°) подтверждает, что маркеры лежат на границе `GetBoundingBox()`
- **Inline text editing — tech debt (вся работа с текстом):**
  - Escape не отменял редактирование — **исправлено** (focus guard в CanvasInputRouter). **Guards защищены тестами (Sprint "Tech debt + coverage", 11.08.2026):** +5 тестов InlineEditManager (Start_NonEditable_NoOp, Commit_UnchangedText_NoCommand, Commit_Twice_PushesSingleCommand, Cancel_WhenNotEditing_NoThrow, Start_WhileEditing_SwitchesObject) + 2 STA-теста AutoFocusOnVisibleBehavior (реальный фокус + SelectAll). Остаётся:
    - Enter/Ctrl+Enter/Escape routing relies on fragile WPF event ordering (PreviewKeyDown vs KeyDown) — при изменении CanvasInputRouter или появлении новых child control'ов может сломаться
    - Ручная верификация Escape при редактировании не проведена (таски 2.2, 2.3 в fix-escape-inline-editing)

### Next Steps
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior — integration/UI тесты с STA~~ — решено в Sprint 62

## Build Commands

```bash
# Build solution
dotnet build src/DotElectric.TemplateEditor.slnx

# Run application
dotnet run --project src/DotElectric.TemplateEditor

# Run all tests (все три тестовых проекта)
dotnet test src/DotElectric.TemplateEditor.slnx

# Run single test
dotnet test src/DotElectric.TemplateEditor.slnx --filter "FullyQualifiedName~YourTestName"

# Run tests with coverage
dotnet test src/DotElectric.TemplateEditor.slnx --collect:"XPlat Code Coverage"
```

## Project Structure

- **Main app:** `src/DotElectric.TemplateEditor/` — WPF .NET 10 CAD application
- **Document library:** `src/DotElectric.Document/` — документная модель (объекты, лист-метаданные, сериализация .tdel, проверка, `TextGeometry`, слот метрик шрифта); без знания редактора
- **Sheets library:** `src/DotElectric.Sheets/` — форматы листа (изолирована, без зависимостей)
- **Tests:** `src/DotElectric.TemplateEditor.Tests/` (app, net10.0-windows), `src/DotElectric.Document.Tests/`, `src/DotElectric.Sheets.Tests/` (net10.0) — xUnit v3 tests; тесты живут с кодом: библиотечные типы тестируются в проектах библиотек (кандидат 4 обзора №5)
- **Solution:** `src/DotElectric.TemplateEditor.slnx` (XML format, not `.sln`)
- **Shared props:** `src/Directory.Build.props` (net10.0-windows, nullable, implicit usings)

## Architecture Must-Know

### Fixed-Point Coordinates
- All internal coordinates in **microns** (`long`, not double)
- 1mm = 1000 microns
- XML serialization also uses microns (`xs:long`)
- Round-trip without precision loss

### Coordinate System
- **Model:** Cartesian (0,0 = bottom-left, Y↑)
- **WPF:** Inverted Y (Y↓)
- Conversion only in `EditorCanvas` via `FromWpfPoint()` / `ToWpfPoint()`
- ViewModels/Services know NOTHING about WPF coordinates

### Key Patterns
- **MVVM** with CommunityToolkit.Mvvm
- **DI** via Microsoft.Extensions.DependencyInjection (all services Singleton, EditorViewModelFactory as IEditorViewModelFactory)
- **Undo/Redo:** 50 levels via `CommandHistory` — commands implement custom `ICommand` interface (NOT `System.Windows.Input.ICommand`)
- **Tools:** State pattern via `ITool` interface
- **Messaging:** WeakReferenceMessenger for cross-VM communication (e.g., tab close)
- **IEditorContext** — Sprint R3: инструменты получают контекст через интерфейс, а не EditorViewModel напрямую
- **ResizeMath** — Sprint R4: чистые статические функции для resize-геометрии
- **ShortcutRegistry** — Sprint R4: централизованный маппинг V/L/R/T/ E/E+Shift

### File Format
- **.tdel:** XML packed in ZIP (custom template format)

### Fonts
- GOST A/B fonts required: `Resources/Fonts/*.ttf` (embedded as resources)
- Font files: GostA.ttf, GostB.ttf
- **Внутренние имена шрифтов (чувствительны к регистру):**
  - `GostA.ttf` → `#GOST Type AU`
  - `GostB.ttf` → `#GOST Type BU`
- URI: `pack://application:,,,/Resources/Fonts/#GOST Type AU`
- FontNameToFamilyConverter маппит "ГОСТ А" / "ГОСТ Б" на правильные URI

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

- `docs/03_Спецификация_требований_Этап1.md` — Detailed architecture and API
- `docs/00_Индекс_документов.md` — Document index

Актуальные описания всех изменений, Common Mistakes и архитектурных решений — в этом документе (AGENTS.md).
Архивные sprint-отчёты и fix-документы удалены из git для оптимизации репозитория.

## Common Mistakes to Avoid

1. Don't use double for coordinates — use microns (`long`)
2. Don't create new Shape on every MouseMove — update properties instead
3. Don't do hit-testing on MouseMove — only on MouseDown
4. Don't use Grid/StackPanel in EditorCanvas — use Canvas (layout pass issues)
5. Always use `Mode=OneWay` when binding to readonly properties
6. IsDirty must be set by `CommandHistory` (Push/Undo/Redo fire `markDirty`), NOT by individual commands (they carry no delegate since кандидат 3 обзора №4) and NOT manually
7. Preview shapes: create once, update properties only
8. EditorViewModel — instantiate via `IEditorViewModelFactory`, NOT `new` directly (ensures DI-managed dependencies)
9. CenterCanvas — always use `Math.Max(0, (canvasPx - viewportPx) / 2)` for each axis independently; portrait sheets may fit width but not height
10. ModelYToCanvasTopConverter binding — pass `Template.Sheet.HeightMm` (double), NOT `HeightMicrons` (long), or converter returns 0.0
11. ToModelPoint — `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset). Do NOT subtract PanOffset — it double-compensates and breaks hit-test
12. Selection visual — use `SelectionVersion` (int) + `IsObjectSelectedConverter` to trigger DataBinding re-evaluation; model objects don't implement INotifyPropertyChanged for selection state
13. **ИСТОРИЧЕСКОЕ (паттерн удалён в кандидате 2 обзора №4, PR #96).** Preview shapes — create once in OnMouseDown, update properties in OnMouseMove, then re-assign reference to trigger ViewModel setter (unconditional OnPropertyChanged). Новый контракт: PreviewManager уведомляет только при смене ссылки (создание/очистка), мутации геометрии рендерер получает через INPC самого preview-объекта — ре-ассайн не нужен и не используется.
14. Model INPC (Item 12 correction) — model objects DO implement INotifyPropertyChanged for **persistent properties** (LineType, coordinates, dimensions). This is necessary for canvas DataTemplate bindings (StrokeDashArray, Width/Height, Canvas.Left/Top) to update when properties change via commands. INPC is NOT implemented for transient UI state like selection.
15. ComboBox with hardcoded items — always add `SelectedIndex` (or `SelectedItem`) binding when using `SelectionChangedCommand` behavior, otherwise the ComboBox never reflects the current model value
16. After Undo/Redo — always purge orphaned objects from `SelectedObjects`; `CommandHistory.Undo()`/`Redo()` removes/re-adds objects from the template collection without updating selection
17. `Rectangle.ContainsPoint()` — use **border-band** approach (expanded bounds minus shrunk interior), NOT full AABB. Interior area > `LineHitToleranceMicrons` from edges must NOT be selectable. Only clicks near the border count.
18. Tool switching keys (V/L/R/T) — handled via `PreviewKeyDown` on Window, NOT `Window.InputBindings`, for keyboard layout independence. `e.Key` returns physical key position regardless of RU/EN layout.
19. Selection markers (`ShowSelectionMarkers`) — returns `SelectedObjects.Count > 0` (not `Count == 1`). Markers render via `ItemsControl ItemsSource="{Binding SelectedObjects}"`, showing handles on ALL selected objects, not just single-selection.
20. Drag delta — compute from **saved initial position** (`_initialPositions[obj]`), NOT from current `obj.MicronsX`. The current value is already updated on previous MouseMove, so `obj.MicronsX + delta` drifts on every frame. Use `initialPos + delta` where `delta` is total mouse movement from drag start.
21. Every model class participating in canvas DataTemplate bindings (`Canvas.Left`/`Canvas.Top`/`StrokeDashArray`/etc) MUST implement `INotifyPropertyChanged` with backing fields for persistent properties (coordinates, dimensions, LineType). This applies to ALL object types: `Line`, `Rectangle`, AND `Text`.
22. Pan delta — compute from **Window-relative coordinates** (stable frame), NOT from `e.GetPosition(canvas)`. `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset), so comparing canvas-relative positions across `MouseMove` events where the canvas has moved produces a delta that includes the previous pan offset — causing runaway acceleration.

## Current State (Sprint R1–R4 + R3.1 + A–D + Coverage Improvement + Sprint 60–63 + Fix Session 2 bugs + Grid Refactoring + AppSettings → GridSettings chain + Tech debt + coverage + Coverage series + Docs/CI (gate 80%) + Weak zones + Глубокая база панелей свойств (#45–#48) + ToolRegistry (#53–#57) + AppSettings типизированный интерфейс (#65–#66) + Кандидат 7 — deletion-пара FitToScreen/рамка выделения (#70–#71) + Кандидат 1 срез 1 — рамка выделения через узкий шов (#75–#76) + Кандидат 7 обзора №4 — пан жест роутера (#84–#85) + Кандидат 1 обзора №4 — один конвейер рендеринга (#88–#89) + Кандидат 2 обзора №4 — preview-сейм (#93–#94) + Кандидат 3 обзора №4 — грязность в CommandHistory (#98–#99) + Кандидат 4 обзора №4 — раскладка маркеров один модуль (#103–#104) + Кандидат 8 обзора №4 — мёртвые швы deletion sweep (#108–#109) + Кандидат 5 обзора №4 — форматы листа один каталог (#113–#114) + Кандидат 6 обзора №4 — TextGeometry из модели (#118–#119) + вынос документной модели в библиотеки DotElectric.Document + DotElectric.Sheets (#135–#136) + Кандидат 1 обзора №5 — документ без редактора (#137–#138) + Кандидат 3 обзора №5 — один шов проверки документа (#142–#143) + Кандидат 4 обзора №5 — добить миграцию тестов библиотек (#147–#148) + Кандидат 5 обзора №5 — deletion sweep №2 (#152–#153) завершены)

- **Tests:** 2603 total (2603 passed, 0 пропусков; распределение: приложение 2072 + Document.Tests 404 + Sheets.Tests 127 — тесты библиотечных типов в проектах библиотек)
- **Coverage:** 91.13% line-rate ✅
- **Build:** 0 errors, 0 warnings
- **CI/CD:** GitHub Actions — build + test + coverage-gate 80% + NuGet кэш
- **EditorViewModel:** ~752 строки (де-bloat R3.1: −410 строк, 25 forwarding-свойств удалено, 4 INPC-обработчика удалены; срез 1 кандидата 1: ещё 7 forwarding-свойств рамки выделения удалены; кандидат 2 обзора №4: ещё 3 forwarding-свойства preview-тройки удалены; кандидат 3 обзора №4: компенсация MarkDirty в Undo/Redo удалена; кандидат 4 обзора №4: член HoveredHandle удалён; кандидат 8 обзора №4: мёртвое `_templateService` удалено, `_gridNodeGenerator` — локальная переменная конструктора)
- **Managers:** ZoomPan, Selection, Clipboard, ToolRegistry, Preview, InlineEdit, StatusBar, Grid, DirtyState
- **PreviewManager:** единственный источник истины preview-состояния (PreviewLine/Rectangle/Text + рамка выделения); контракт «уведомление только при смене ссылки» (`[ObservableProperty]`); рендерер подписан на INPC preview-объекта
- **Tools:** ITool (без Name) + IEditorContext (25 членов, типизированные `PushTool`/`PopTool`/`ActivateTool`, без WPF ICommand; рамка выделения — методы `SetSelectionBox`/`ClearSelectionBox`; preview — член `PreviewManager`; пан — жест роутера, не член интерфейса; грязность — не член интерфейса; hover-маркер — приватное состояние SelectTool) + `ToolKind` + ResizeMath (чистые функции) + ShortcutRegistry
- **ToolRegistry:** глубокий module (бывший ToolManager) — идентичность `ToolKind`, фабрики+кэш, `ActiveToolKind`/`ActiveToolInstance`, `Stack<ToolKind>`, `SwitchTo` с reset'ом предыдущего; Пан — жест роутера, не инструмент (ADR-0001); строковые карты удалены
- **Converters:** 26 файлов (все sealed); каждый конвертер объявлен ровно один раз — в корневом словаре ресурсов приложения (исключение: `InverseBooleanConverter` view-локально в SettingsView — окно загружается в STA-тестах без ресурсов приложения)
- **RenderRules:** статический модуль правил рендеринга (Helpers) — карта шрифтов, frozen dash-карта, hex→Brush, `ModelYToTop`, карта выравнивания, anchor-политика на тип; единый источник для канваса/preview/печати/сетки; конвертеры — тонкие делегаты (guards в адаптерах)
- **MarkerLayout:** статический модуль геометрии маркеров выделения (Helpers) — каталог маркеров по типу в порядке приоритета hit-проверки, позиции в модельных координатах, hit-тест с допуском `min(8 мм, minDim/3)` (#82), классификация (угол/ребро/конец), курсорная политика; HitTestHelper — только хит по телу; ResizeMath — только математика drag'а; ResizeTool — единственный снапшот ResizeState
- **SheetFormatCatalog:** статический каталог стандартных форматов листа (Models) + sealed record `SheetFormat` (Name, LongSideMicrons, ShortSideMicrons, DefaultOrientation) — единственный источник фиксированного набора (10 форматов: A0–A4 + пять полуформатов); API All/Get/TryGet/Contains/Normalize + `const DefaultName = "A3"`; строковая идентичность (ADR-0002); пользовательский формат вне каталога (`Sheet.CustomName`); все поверхности — потребители (модель листа, валидатор, настройки, меню «Новый шаблон» генерируется из каталога, диалог); дефолт «A3» ×6 — константа; мёртвая настройка `DefaultSheetFormat` — fallback в цепочке создания вкладки
- **TextGeometry:** статический модуль геометрии повёрнутого текста (Helpers, прецеденты RenderRules/MarkerLayout) — WPF LayoutTransform-offset, углы повёрнутого бокса (индексы 0–3 = каталог MarkerLayout, вне диапазона — throw), хит повёрнутого текста, bounding box; модель `Text` — тонкие делегации (8 свойств углов для XAML маркеров + полиморфные contains/bounding box), INPC-проводка на модели; `WidthMicrons`/`HeightMicrons` (FontMetrics) — в модели; мёртвые `Visual*`×4 и `RotationAngleValid` удалены (кандидат 6 обзора №4)
- **Naming:** `TemplateObjectBase` (не `ITemplateObject`)
- **Commands:** `IUndoCommand`, `CommandHistory`, `AddObjectCommand`, `DeleteObjectCommand`, `ChangePropertyCommand<T>` (в т.ч. resize через `ResizeState` + полиморфный `ApplyResize`), `BatchCommand`
- **CommandHistory:** единственный источник грязности — `Push`/`Undo`/`Redo` стреляют `markDirty` один раз после успешного выполнения (rollback при исключении не стреляет); команды делегат не носят (кандидат 3 обзора №4)
- **Model INPC:** `[ObservableProperty]` sourcegen на Line, Rectangle, Text; `Template` → ObservableObject (INPC на `Sheet`)
- **Constants:** `DocumentConstants` (DotElectric.Document — только допуск хита тела) + `EditorSettings` (редактор; секция Interaction — константы взаимодействия) вместо `EditorConstants.cs`-прокладки; критерий «константы живут в сборке читателя» (кандидат 1 обзора №5)
- **Validation:** `ITemplateValidator`/`TemplateValidator` (domain) + `ValidationService` (UI)
- **EditorCanvasBehavior:** 78 строк (AttachedProperty + stubs), 3 файла: State, Transform, Router
- **FontMetrics:** `WpfFontMetrics` (Services) — WPF-реализация; read-only `IFontMetrics` + эмбиентный слот `FontMetricsProvider.Current` в `DotElectric.Document` (ADR-0003); `Text` читает метрики из слота, запасной поставщик для headless
- **ShortcutRegistry:** `TryHandle()` — единая точка входа для всех горячих клавиш
- **Grid:** `IGridNodeGenerator`/`GridNodeGenerator` (DI Singleton), узлы в абсолютных координатах листа, pan без регенерации; `GridSettings` + `MaxGridNodes`/`NodeColor`/`NodeSize`; `GridSettings.FromAppSettings` + `EditorViewModelFactory.ResolveGridSettings` — настройки применяются к вкладкам
- **Панели свойств:** глубокая база `ObjectPropertiesViewModel<TObject>` (конструктор-пара — markDirty удалён в кандидате 3 обзора №4, декларативная nameof-карта `PropertyMap`, `UpdateObject`/notify-all, `Dispose`, `SetProperty<T>`, `ChangeFromMmString`, `ParseLineType`) + 3 тонких наследника: Line (7 пар), Rectangle (8 пар), Text (13 пар + особые null-coalescing команды в sub-VM)
- **Настройки:** `ISettingsService` = только `Load()` + `Save(AppSettings)` (строковый Get/Set и switch-диспетчеры удалены); `AppSettings` — POCO в Models; одна запись файла на создание вкладки

## Sprint — Coverage Improvement (19.07.2026)

### Pipeline: Увеличение покрытия до ≥75%
**Проблема:** Фактическое покрытие составляло ~59-67% (оценка 82% была неточной). CI gate требовал ≥75%.
**Исправление:** Добавлено ~195 тестов в 6 зонах + 2 retry-цикла. Ключевые добавления:
- **Commands:** 16 тестов на null guards + edge cases (AddObjectCommand, DeleteObjectCommand, ChangePropertyCommand, BatchCommand)
- **Tools Reset():** 9 тестов на DrawingLineTool/DrawingRectangleTool/TextTool.Reset()
- **Grid:** 8 тестов на ComputeDisplayStep/GenerateGridNodes edge cases
- **Services:** 8 тестов на TemplateService, AutosaveService, PrintDocumentGenerator, DialogService
- **Models:** 15+ тестов на Template.Clone(), Sheet.FromFormat(), Coordinate, PointMicrons операторы
- **MainViewModel:** 6 тестов на AutosaveTickHandler, PrintPreviewCommand, OpenSettingsCommand
- **Retry 1:** FontMetrics (22 теста, instance+IFontMetrics), TemplateObjectBase (43 теста, Move/Clone/CaptureResizeState/ContainsPoint), NonZeroToVisibilityConverter (15 тестов), CustomSheetDialogViewModel (23 теста)
- **Retry 2:** ShortcutRegistry (22 теста, 100% покрытие)
- **Production fixes:** Template.Clone() deep copy, PointMicrons operator+/-, исправлен синтаксис TemplateTests.cs

**Файлы:** 15+ test files, Models/Template.cs, Models/PointMicrons.cs
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.15% line-rate ✅ (порог 75% достигнут)

## Sprint 37 — Selection fixes + visual feedback

### Fix S37-1: ToModelPoint double-compensation

**Проблема:** `EditorCanvasBehavior.ToModelPoint()` вычитал `PanOffsetX`/`PanOffsetY` из `e.GetPosition(canvas)` — но `e.GetPosition` уже учитывает `RenderTransform` (сдвиг на `CanvasOffset = -PanOffset`). Двойное вычитание смещало модельные координаты на `PanOffset/zoom` мм, из-за чего HitTest не находил объекты под курсором.

**Исправление:** Убрано вычитание PanOffset из `ToModelPoint()`.

### Fix S37-2: Visual selection state

**Проблема:** У объектов не было визуального состояния «выделен». SingleSelectedObject (маркеры) показывался, но при мульти-выделении или одиночном клике внешний вид не менялся.

**Исправление:** 
- `SelectionVersion` (int) + `IsObjectSelected(obj)` в EditorViewModel
- `IsObjectSelectedConverter` (IMultiValueConverter) — проверяет `SelectedObjects.Contains(obj)`
- DataTrigger'ы в DataTemplate'ах Line/Rectangle/Text — синяя подсветка `#0078D4`, StrokeThickness=2

### Fix S37-3: Preview shapes not appearing

**Проблема:** После мутации свойств `_previewLine`/`_previewRect` ссылка не менялась, `EditorViewModel.PreviewLine` setter не вызывался.

**Исправление:** Ре-ассайн `_editor.PreviewLine = _previewLine` / `_editor.PreviewRectangle = _previewRect` в OnMouseMove.

### Fix S37-4: Canvas not resizing on zoom

**Проблема:** `OnPropertyChanged(nameof(Zoom))` не вызывался при изменении зума через `SetZoom`/`ZoomIn`/`ZoomOut`.

**Исправление:** Добавлен `OnPropertyChanged(nameof(Zoom))` в `OnZoomChangedInternal()`.

### Fix S37-5: Escape doesn't switch to Select

**Проблема:** Escape в инструментах рисования/текста очищал состояние, но не активировал SelectTool.

**Исправление:** Добавлен `_editor.ActiveTool = "Select"` после `Reset()` во всех трёх инструментах.

### Fix S37-6: SelectionBoxTop not rendering

**Проблема:** `SelectionBoxTop` (вычисляемое = SelectionBoxBottom + SelectionBoxHeight) не пробрасывался на EditorViewModel → XAML не обновлялся.

**Исправление:** Подписка на `PropertyChanged` PreviewManager в EditorViewModel.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 465+ пройдены (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120)

## Sprint 38 — LineType панели свойств + Undo + координаты

### Fix S38-1: ComboBox типа линии не отображает текущее значение

**Проблема:** ComboBox в панели свойств не имел `SelectedItem`/`SelectedIndex` биндинга — показывал пустое значение при первом выборе объекта. Изменение через UI работало, но ComboBox не синхронизировался с моделью.

**Исправление:** Создан `LineTypeToIndexConverter` (LineType → int). Добавлен `SelectedIndex="{Binding LineTypeValue/RectLineType, Converter=...}"` на оба ComboBox (Line и Rectangle).

### Fix S38-2: Изменение LineType не перерисовывает канвас

**Проблема:** `Line` и `Rectangle` без INPC — мутация `LineType` через `ChangePropertyCommand` не обновляла `StrokeDashArray` на канвасе.

**Исправление:** `Line.cs` и `Rectangle.cs` — `INotifyPropertyChanged` + backing field для `LineType`.

### Fix S38-3: DrawingRectangleTool не передаёт _lineType

**Проблема:** `CalculateRectangle()` создавал `new Rectangle(x, y, w, h)` — всегда `LineType.Solid`.

**Исправление:** `CalculateRectangle()` принимает `lineType` и передаёт в конструктор.

### Fix S38-4: Изменение координат не перерисовывает канвас

**Проблема:** `Line.StartMicronsX/Y`, `EndMicronsX/Y` и `Rectangle.WidthMicrons/HeightMicrons/MicronsX/Y` без INPC — канвас не обновлялся при редактировании через панель свойств.

**Исправление:** Все свойства координат — backing fields + `OnPropertyChanged()`. В `Rectangle` добавлены уведомления для `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY` (маркеры выделения).

### Fix S38-5: Enter не коммитит поля ввода координат

**Проблема:** `TextBoxLostFocusCommandBehavior` реагировал только на LostFocus. Enter не применял значение.

**Исправление:** Добавлен обработчик `KeyDown.Enter` в `TextBoxLostFocusCommandBehavior`.

### Fix S38-6: Undo оставляет «висячее» выделение

**Проблема:** После Undo (`AddObjectCommand.Undo()` удаляет объект) `SelectedObjects` не очищался — маркеры выделения оставались на канвасе.

**Исправление:** В `Undo()`/`Redo()` добавлен вызов `PurgeOrphanedSelection()`, удаляющий из `SelectedObjects` объекты не из `Template.Objects`.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 589+ пройдены (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120 + PropertiesViewModel 50 + Command 137 + Line/Rectangle 30)

## Sprint 39 — Rectangle HitTest: селекция только по границе

### Fix S39-1: Прямоугольник выделяется при клике внутри области

**Проблема:** `Rectangle.ContainsPoint()` использовал полную AABB-проверку — любая точка внутри прямоугольника (включая центр) считалась попаданием.

**Исправление:** Метод переписан на **border-band подход** — точка считается попавшей на прямоугольник только если она находится в пределах `LineHitToleranceMicrons` (5 мм) от любой из четырёх сторон. Внутренняя область (дальше 5 мм от краёв) не селектируется. Для маленьких прямоугольников (< 10 мм) вся область остаётся селектируемой.

**Файлы:**
- `Models/Objects/Rectangle.cs` — `ContainsPoint()` заменён на border-band
- `Tests/Helpers/HitTestHelperTests.cs` — обновлены тесты
- `Tests/Helpers/HitTestHelperExtendedTests.cs` — обновлены тесты + новый `PointNearEdgeLargeRect_ReturnsTrue`
- `Tests/Helpers/AdditionalHelperTests.cs` — обновлены тесты
- `Tests/IntegrationTests.cs` — обновлён `HitTestAll_OverlappingObjects`

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 840+ пройдены (все ключевые категории)

## Sprint 40 — Keyboard shortcuts + Selection markers

### Fix S40-1: KeyBindings инструментов не совпадали с UI

**Проблема:** Фактические KeyBindings (H/L/U/X) не соответствовали UI (V/L/R/T). Select был на H вместо V, Rectangle на U вместо R, Text на X вместо T.

**Исправление:** `MainWindow.xaml` — H→V, U→R, X→T.

### Fix S40-2: R-клавиша конфликтовала (Rectangle vs Rotate)

**Проблема:** R была занята Rotate, не позволяя использовать её для Rectangle.

**Исправление:** Rotate перенесён с R на E (rotatE) / Shift+E.

### Fix S40-3: Переключение инструментов не работало с русской раскладкой

**Проблема:** WPF `KeyBinding` с `KeyGesture` не срабатывал при русской раскладке клавиатуры.

**Исправление:** Инструменты (V/L/R/T) и rotate (E/Shift+E) перенесены из `Window.InputBindings` в `PreviewKeyDown` handler на Window. `e.Key` в PreviewKeyDown возвращает физическую клавишу независимо от раскладки.

### Fix S40-4: Панель инструментов не обновлялась при горячих клавишах

**Проблема:** `SetActiveToolCommand` устанавливал `_toolManager.ActiveTool` напрямую, минуя сеттер `EditorViewModel.ActiveTool`, который вызывает `OnPropertyChanged()`. RadioButton на toolbar не получал уведомление.

**Исправление:** `SetActiveTool()` теперь вызывает `ActiveTool = tool` (сеттер свойства с `OnPropertyChanged()`).

### Fix S40-5: Маркеры выделения не появлялись на выбранных объектах

**Проблема:** `ShowSelectionMarkers` возвращал `true` только при `SelectedObjects.Count == 1`. При мульти-выделении `ContentControl` с маркерами был скрыт.

**Исправление:**
- `SelectionManager.ShowSelectionMarkers` — `Count > 0` (вместо `Count == 1`)
- `ContentControl Content="{Binding SingleSelectedObject}"` заменён на `ItemsControl ItemsSource="{Binding SelectedObjects}"` с `Canvas` ItemsPanel — маркеры рендерятся для каждого выделенного объекта

**Файлы:**
- `MainWindow.xaml` — KeyBindings → PreviewKeyDown
- `MainWindow.xaml.cs` — `Window_PreviewKeyDown()` handler
- `ViewModels/Managers/ToolManager.cs` — не изменялся
- `ViewModels/EditorViewModel.cs` — `SetActiveTool()` через сеттер
- `ViewModels/Managers/SelectionManager.cs` — `ShowSelectionMarkers` → `Count > 0`
- `Views/EditorCanvas.xaml` — ContentControl → ItemsControl

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 844+ пройдены (все ключевые категории)

## Sprint 41 — Drag move delta drift + Text INPC

### Fix S41-1: Delta accumulation drift on multi-MouseMove

**Проблема:** `SelectTool.OnMouseMove()` вычислял `newX = obj.MicronsX + delta`, где `delta` — полное смещение от точки старта. Но `obj.MicronsX` уже обновлён на предыдущем `MouseMove`, поэтому каждое новое событие добавляло дельту к уже смещённой позиции. Объект «убегал» от курсора (дрифт, пропорциональный количеству `MouseMove`).

**Исправление:** Дельта прибавляется к **сохранённой начальной позиции** из `_initialPositions[obj]`.

**Файл:** `Tools/SelectTool.cs:208-210`

### Fix S41-2: Text INPC for MicronsX/MicronsY

**Проблема:** `Text` не реализовывал `INotifyPropertyChanged` — `Text.Move()` устанавливал `MicronsX`/`MicronsY` (auto-properties), но WPF-биндинги `Canvas.Left`/`Canvas.Top` не обновлялись. Текст визуально не двигался при перетаскивании.

**Исправление:**
- `Text` реализует `INotifyPropertyChanged`
- Override `MicronsX`/`MicronsY` с backing fields + `OnPropertyChanged()`
- Уведомления для `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`

**Файл:** `Models/Objects/Text.cs:12-52`

### Cleanup
- Удалены мёртвые поля `_dragStartX`/`_dragStartY`
- Упрощён расчёт дельты (оба if-else вычисляли одно и то же)

**Build:** 0 errors, 0 warnings
**Tests:** 844+ пройдены (все ключевые категории)

## Sprint 42 — StrokeThicknessMicrons (толщина линии)

### Feature S42-1: Добавлено свойство StrokeThicknessMicrons

**Проблема:** В XSD-спецификации предусмотрен `StrokeThickness` (xs:long) для Line и Rectangle, но в коде свойство отсутствовало на всех уровнях — модели, сериализация, UI панели свойств, отрисовка на канвасе.

**Исправление:** Реализовано end-to-end:

| Уровень | Файл | Изменение |
|---------|------|-----------|
| Константа | `Constants/EditorConstants.cs:86-88` | `DefaultStrokeThicknessMicrons = 500` (0.5 мм) |
| Модель Line | `Models/Objects/Line.cs:23,87-101,126,133,148` | Поле + INPC-свойство + параметр конструктора + Clone |
| Модель Rectangle | `Models/Objects/Rectangle.cs:23,88-102,152,161,173` | Аналогично |
| Сериализация | `Services/TemplateService.cs:106,121,124,361,367,425,434` | DTO-поле + MapToObject/MapToDto |
| ViewModel | `ViewModels/PropertiesViewModel.cs:169,177,275-290,348-362,525-536` | Свойства + команды + string-обёртки + UpdateSelection |
| UI панели | `Views/PropertiesPanelContent.xaml:130-142,227-240` | TextBox «Толщина (мм)» для Line и Rectangle |
| Canvas | `Views/EditorCanvas.xaml:67-77,153-163` | `StrokeThickness` привязан к модели через `MicronsToPixelConverter` (Style Setter + MultiBinding) |

**Детали реализации:**
- Все внутренние координаты в микронах (`long`), WPF-пиксели через `MicronsToPixelConverter` с учётом Zoom
- DataTrigger'ы выделения (StrokeThickness=2) и наведения (StrokeThickness=2.5) остаются неизменными — они override базовый Style Setter через WPF precedence
- Значение по умолчанию: 500 микрон (0.5 мм) — соответствует ГОСТ 2.303-68 для тонкой линии
- Drawing инструменты (LineTool/RectangleTool) не требуют изменений — дефолтный параметр конструктора 500 микрон

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ пройдены (0 failures)

## Sprint 43 — ResizeTool dispatch fix

### Fix S43-1: GetCurrentTool() не имеет case "Resize"

**Проблема:** `EditorCanvasBehavior.GetCurrentTool()` не обрабатывал `"Resize"` в switch — при падении на default возвращал `SelectTool`. После того как `SelectTool.OnMouseDown()` детектил хендл и пушил `"Resize"` в стек инструментов, последующие `OnMouseMove`/`OnMouseUp` уходили в `SelectTool` вместо `ResizeTool`. Размеры объектов не менялись при drag за угловые маркеры, команда `CustomResizeCommand` не создавалась.

**Исправление:** Добавлен case `"Resize" => editor.GetOrCreateTool<ResizeTool>()` в `GetCurrentTool()`.

**Файл:** `Behaviors/EditorCanvasBehavior.cs:288-298`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ пройдены (0 failures)

## Sprint 44 — PropertiesPanel live update after resize

### Fix S44-1: PropertiesViewModel не подписан на INPC объекта

**Проблема:** `PropertiesViewModel` не подписывался на `INotifyPropertyChanged.PropertyChanged` выделенного объекта. При изменении размеров через `ResizeTool` модель оповещала (`OnPropertyChanged`), но ViewModel не перезапрашивала свои computed-свойства (`RectX`, `LineEndX`, `TextFontSize` и т.д.). WPF-биндинги на панели свойств не обновлялись.

**Исправление:**
- `PropertiesViewModel.UpdateSelection()` — при смене выделения отписывается от старого объекта, подписывается на новый
- Добавлен метод `OnSelectedObjectPropertyChanged()`, который по имени свойства модели определяет, какое ViewModel-свойство оповестить
- При `Dispose()` — гарантированная отписка

**Файл:** `ViewModels/PropertiesViewModel.cs:109-210`

### Fix S44-2: Text INPC для всех свойств

**Проблема:** `Text.FontSizeMicrons`, `Content`, `FontName`, `TextType`, `RotationAngle` были auto-properties без INPC. Даже с подпиской PropertiesViewModel на `PropertyChanged`, эти свойства не оповещали об изменениях.

**Исправление:** Все свойства переведены на backing fields + `OnPropertyChanged()`. Для `FontSizeMicrons` и `Content` добавлены уведомления для зависимых computed-свойств (`WidthMicrons`, `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`).

**Файл:** `Models/Objects/Text.cs:53-110`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1287+ пройдены (0 failures)

## Sprint 45 — Pan delta accumulation fix (RenderTransform drift)

### Fix S45-1: Панорамирование ускоряется из-за RenderTransform в e.GetPosition

**Проблема:** `EditorCanvasBehavior.State_MouseMove()` вычислял дельту панорамирования из `e.GetPosition(canvas)`, который учитывает `RenderTransform` canvas'а (`TranslateTransform CanvasOffsetX/Y`). После каждого `MouseMove` canvas сдвигался, и на следующем `MouseMove` `e.GetPosition(canvas)` возвращал координаты, уже включающие предыдущий сдвиг. Каждое движение мыши добавляло дельту предыдущего пана — панорамирование неконтролируемо ускорялось (`runaway pan`).

**Исправление:** Дельта вычисляется в **Window-координатах** (`e.GetPosition(window)`), которые не меняются при сдвиге canvas'а. Добавлены поля `PanStartWpfPoint` и `PanAppliedModelDelta` в `EditorCanvasState` для корректного инкрементального расчёта.

**Файл:** `Behaviors/EditorCanvasBehavior.cs:96-165,199,343-349`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** PanTool 13/13, EditorCanvas/ZoomPan 10/10 — все пройдены

## Sprint 46 — Context menu fixes

### Fix S46-1: Canvas context menu blocked by State_MouseDown e.Handled

**Проблема:** `EditorCanvasBehavior.State_MouseDown()` безусловно устанавливал `e.Handled = true` для ВСЕХ кнопок мыши, включая правую. WPF не показывал `ContextMenu` на UserControl, т.к. событие было помечено как обработанное.

**Исправление:** В `State_MouseDown()` при правом клике явно открываем `UserControl.ContextMenu` программно через `VisualTreeHelper`. В `State_MouseUp()` добавлен ранний return для правой кнопки.

**Файлы:**
- `Behaviors/EditorCanvasBehavior.cs:94-108,210-212` — явное открытие ContextMenu + ранний return в MouseUp
- `EditorCanvas.xaml:22-43` — контекстное меню определено на UserControl (без изменений)

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 3/3 RightClick_Ignored + 13/13 PanTool — пройдены

### Fix S46-2: TabItem context menu commands not working (Async suffix mismatch)

**Проблема:** Методы `CloseTabAsync()`, `CloseOtherTabsAsync()`, `CloseAllTabsAsync()` в `EditorViewModel` возвращали `void` (не async). CommunityToolkit.Mvvm 8.4.2 source generator обрезает суффикс `Async` **только для асинхронных методов** (возвращающих `Task`). Для `void`-методов суффикс сохраняется → генерировались `CloseTabAsyncCommand`, а XAML биндился к `CloseTabCommand` — команда не находилась, MenuItem был неактивен.

**Исправление:** Методы переименованы — убран суффикс `Async`:
- `CloseTabAsync()` → `CloseTab()`
- `CloseOtherTabsAsync()` → `CloseOtherTabs()`
- `CloseAllTabsAsync()` → `CloseAllTabs()`

**Файл:** `ViewModels/EditorViewModel.cs:45-67`

**Common Mistakes (new):**
23. `[RelayCommand]` on `void` method with `Async` suffix — source generator НЕ обрезает суффикс для синхронных методов. Имя команды будет `MethodAsyncCommand`, а не `MethodCommand`. Для async методов (возвращающих `Task`/`Task<T>`) суффикс обрезается.
24. ContextMenu внутри `Style` (`Setter.Value`) — не полагайся на автоматическое наследование `DataContext` через `PlacementTarget`. Если команды не работают, используй явное указание `DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"`.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 12/12 CloseTab + RightClick — пройдены

## Sprint 47 — Grid 1mm MinPixelSpacing fix

### Fix S47-1: Сетка не отображается при шаге 1мм

**Проблема:** При установке шага сетки 1мм сетка полностью пропадала с холста. Два сценария:
- **A3+:** `cols * rows` (125K+) превышает `MaxGridNodes (100000)` → `GenerateGridNodes()` возвращает пустой список
- **A4:** 62K узлов генерируются, но при Zoom=1.0 расстояние 1px < 2px (диаметр точки 2px) → сплошная серая заливка

**Причина:** `GridManager.RefreshGridNodes()` и `GridHelper.GenerateGridNodes()` не проверяли `MinPixelSpacing` (5px) — минимальное расстояние между узлами в пикселях, при котором точки сетки различимы.

**Исправление:**
- `GridManager.RefreshGridNodes()` — проверка `pixelSpacing < MinPixelSpacing` → ранний return
- `GridHelper.GenerateGridNodes()` — defense-in-depth: та же проверка

**Поведение:**
- 1мм сетка при Zoom < 500%: скрыта (точки < 5px — слишком плотно)
- 1мм сетка при Zoom ≥ 500%: отображается
- 5мм сетка при Zoom ≥ 100%: отображается (как и раньше)

**Файлы:** `ViewModels/Managers/GridManager.cs`, `Helpers/GridHelper.cs`, `Tests/Helpers/GridHelperTests.cs`

**Common Mistakes (new):**
25. Grid nodes (GenerateGridNodes) must check MinPixelSpacing — unlike lines, nodes don't auto-hide when too dense, causing either MaxGridNodes overflow (A3+) or solid grey fill (A4). Always check `stepMm * zoom < MinPixelSpacing` before generating nodes.

## Sprint 48 — Dirty indicator (*) in tab header not appearing

### Fix S48-1: PropertyChanged notification not forwarded from DirtyStateManager

**Проблема:** `EditorViewModel.IsDirty`, `DisplayName` и `FilePath` — plain forwarding properties к `DirtyStateManager` без `PropertyChanged`. WPF DataTrigger `{Binding IsDirty}` в ControlTemplate TabItem подписан на `EditorViewModel.PropertyChanged`, но уведомление приходит от `DirtyStateManager` (через `[ObservableProperty]`). В итоге `DirtyIndicator` (звёздочка `*`) никогда не становится `Visible`.

**Исправление:** Добавлена подписка на `_dirtyStateManager.PropertyChanged` в конструкторе `EditorViewModel` — проброс `IsDirty`, `DisplayName`, `FilePath` через `OnPropertyChanged()` (аналогично существующему паттерну для `_zoomPanManager` и `_previewManager`).

**Файл:** `ViewModels/EditorViewModel.cs:752-764`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** EditorViewModel 112/112, ToolTests 113/113, IntegrationTests 49/49, MarkDirty 27/27 — все пройдены

**Common Mistakes (new):**
26. Plain forwarding properties to a delegated `[ObservableObject]` manager — if the ViewModel wraps a manager's `[ObservableProperty]` with a regular property, `PropertyChanged` fires from the manager, not the ViewModel. Always subscribe to manager's `PropertyChanged` and forward needed notifications (same pattern as `_zoomPanManager`, `_previewManager`, `_dirtyStateManager`).

## Sprint 49 — ResizeTool clamp fix + test corrections

### Fix S49-1: Minimum-size clamp moves fixed edges

**Проблема:** Clamp минимального размера (`MinResizeSizeMicrons = 1000`) применял `Min`/`Max` безусловно к **обеим** граням в оси, двигая фиксированные грани. Например, для `TopRight` (правая+верхняя движутся, левая+нижняя фиксированы) при пересечении правой гранью левой, clamp сдвигал **левую** (фиксированную) грань, а не ограничивал **правую** (движущуюся).

**Исправление:** Clamp стал handle-зависимым:
- Определяются `leftMoves`, `rightMoves`, `bottomMoves`, `topMoves` по типу маркера
- Ограничивается **только движущаяся** грань: `Min()` для левой/нижней, `Max()` для правой/верхней
- При Ctrl обе грани движутся → симметричный схлоп через середину при нарушении minSize

**Файл:** `Tools/ResizeTool.cs:248-282`

### Fix S49-2: Тесты под старую бажную формулу

**Проблема:** 14 тестов в `ResizeToolTests.cs` и `ResizeToolExtendedTests.cs` содержали ожидаемые значения, соответствующие старой бажной формуле (double-delta, half-delta, неправильный pivot).

**Исправление:** Все тесты переписаны под корректную edge-based модель. Добавлен `SnapEnabled = false` в тесты, где он отсутствовал.

**Файлы:** `Tests/Tools/ResizeToolTests.cs`, `Tests/Tools/ResizeToolExtendedTests.cs`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 63/63 ResizeTool, 1500+ остальных — пройдены

**Common Mistakes (new):**
27. Minimum-size clamp in edge-based resize — don't apply `Min`/`Max` to both edges in an axis. Only the MOVING edge should be constrained. Determine `leftMoves`/`rightMoves`/`bottomMoves`/`topMoves` per handle type (or set all true for Ctrl) and constrain only the moving edge(s). Fixed edges must NEVER be moved by the clamp.

## Sprint 50 — Clipboard improvements (Copy/Paste/Cut)

### Feature S50-1: Ctrl+X keyboard shortcut + UI

**Проблема:** `InputGestureText="Ctrl+X"` отображался в контекстном меню, но:
- Привязки в `Window.InputBindings` не было — клавиша не работала
- В главном меню и тулбаре отсутствовал пункт «Вырезать» (были только Копировать/Вставить/Удалить)

**Исправление:**
- Добавлен `<KeyBinding Key="X" Modifiers="Control" Command="{Binding SelectedTab.CutSelectedCommand}"/>`
- В главное меню добавлен `MenuItem Header="Вы_резать"` с иконкой `ContentCut` между Копировать и Вставить
- В тулбар добавлена кнопка `Вырезать (Ctrl+X)` с иконкой `ContentCut` между Копировать и Вставить

**Файл:** `MainWindow.xaml:36,151-154,275-279`

### Fix S50-2: Re-paste bug (same instance added twice)

**Проблема:** `GetClipboardContents()` возвращал ссылки на те же объекты из `_clipboard`. Повторный Ctrl+V добавлял те же экземпляры в `Template.Objects` снова — объект оказывался в коллекции дважды.

**Исправление:** `GetClipboardContents()` теперь клонирует объекты при каждом вызове, а не при Copy:
```csharp
public IReadOnlyList<TemplateObjectBase> GetClipboardContents()
    => _clipboard.Select(o => o.Clone()).ToList().AsReadOnly();
```

**Файл:** `ClipboardManager.cs:27-28`

### Feature S50-3: Paste offset (10mm step)

**Проблема:** Вставленные объекты появлялись точно поверх оригиналов — их не было видно.

**Исправление:** Добавлено смещение при вставке. После Copy offset = 10мм. Каждый последующий Paste без Copy увеличивает offset ещё на 10мм по X и Y. При повторном Copy offset сбрасывается.

**Файл:** `ClipboardManager.cs:10-14,22-23,34-42`

### Feature S50-4: BatchCommand для Cut/Paste

**Проблема:** При Cut/Paste 5 объектов создавалось 5 отдельных команд в Undo-стеке. Пользователь нажимал Ctrl+Z 5 раз для отмены одного действия.

**Исправление:** При >1 объекте `PasteFromClipboard()` и `DeleteSelected()` создают `BatchCommand`, группирующий все операции в одну Undo-команду:
- Paste: "Вставить объекты" (N объектов)
- Delete: "Удалить объекты" (N объектов)

**Файлы:** `EditorViewModel.cs:570-587,982-996`

### Feature S50-5: Auto-select pasted objects

**Проблема:** После Paste вставленные объекты не выделялись — пользователь не видел, что было добавлено.

**Исправление:** Добавлен метод `SelectionManager.SelectObjects()`. `PasteFromClipboard()` вызывает `_selectionManager.SelectObjects(clipboard)` после Push команд.

**Файлы:** `SelectionManager.cs:56-62`, `EditorViewModel.cs:586`

### Feature S50-6: StatusBar feedback

**Проблема:** Copy/Paste/Cut не давали обратной связи в строке состояния.

**Исправление:** Добавлены сообщения в `StatusBarManager.StatusMessage`:
- Copy: "Скопировано: N объектов" / "Нет выделенных объектов"
- Cut: "Вырезано: N объектов" / "Нет выделенных объектов"
- Paste: "Вставлено: N объектов" / "Буфер обмена пуст"

Добавлен вспомогательный метод `GetObjectWord()` для русских числительных (объект/объекта/объектов).

**Файл:** `EditorViewModel.cs:557-587,1047-1053`

### Feature S50-7: Clipboard cleanup on tab close

**Проблема:** При закрытии вкладки объекты в буфере обмена могли ссылаться на удалённый шаблон.

**Исправление:** `ClipboardManager.Clear()` вызывается в `EditorViewModel.Dispose()`.

**Файлы:** `ClipboardManager.cs:30`, `EditorViewModel.cs:1039`

### Fix S50-8: Ctrl+V перехватывался PreviewKeyDown (tool switcher)

**Проблема:** `Window_PreviewKeyDown` в `MainWindow.xaml.cs:27` обрабатывал `case Key.V` **без проверки модификаторов**. При нажатии `Ctrl+V` событие перехватывалось, устанавливался `ActiveTool = "Select"` и `e.Handled = true`. `KeyBinding` для Ctrl+V в `Window.InputBindings` никогда не получал событие — Paste не работал.

**Исправление:** Добавлена проверка `if (e.KeyboardDevice.Modifiers != ModifierKeys.None) break;` для всех tool-switching кейсов (V, L, R, T). Теперь Ctrl+V доходит до InputBindings.

**Файл:** `MainWindow.xaml.cs:27-58`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 1289+ пройдены (0 failures)

**Common Mistakes (new):**
28. Re-paste bug — `GetClipboardContents()` must clone objects on EVERY call, not only during `Copy()`. If it returns references to the same cached instances, repeated Paste adds the same object to the collection. Always `_clipboard.Select(o => o.Clone())` in `GetClipboardContents()`. Paste offset counter must reset to `PasteOffsetStepMicrons` (not 0) after Copy, so the first paste already has an offset.
29. `PreviewKeyDown` for tool switching must check `e.KeyboardDevice.Modifiers != ModifierKeys.None` before handling V/L/R/T. Without the check, `Ctrl+V` (Paste), `Ctrl+L`, `Ctrl+R`, `Ctrl+T` get intercepted by the tool switcher and never reach their `Window.InputBindings`. Always add `if (modifiers != None) break;` at the start of each tool-switching case.
30. Panning without `CaptureMouse()` — if the mouse leaves the canvas during middle-button drag, `MouseMove` and `MouseUp` stop being delivered, panning freezes, and `IsPanning` never resets. Always call `canvas.CaptureMouse()` on pan start and `canvas.ReleaseMouseCapture()` on pan end.

## Sprint 51 — Panning mouse capture fix

### Fix S51-1: Panning breaks/corrupts when mouse leaves canvas during drag

**Проблема:** `EditorCanvasBehavior.State_MouseDown()` не вызывал `canvas.CaptureMouse()` при старте панорамирования. Без захвата мыши:
- При выходе курсора за границу канваса `MouseMove` перестаёт доставляться — панорамирование замирает
- `MouseUp` вне канваса не доходит до `State_MouseUp` — `IsPanning` навсегда `true`
- Последующий клик средней кнопкой сбрасывает `IsPanning`, но первый `MouseMove` применяет большую накопленную дельту — canvas «прыгает»

**Исправление:** Добавлен `CaptureMouse()` / `ReleaseMouseCapture()` в трёх местах:
- Middle button branch: `canvas.CaptureMouse()` после `state.IsPanning = true`
- Space/Alt+Left branch: `canvas.CaptureMouse()` после `state.IsPanning = true`
- Panning end в `State_MouseUp`: `canvas.ReleaseMouseCapture()` перед `e.Handled = true`

**Файл:** `Behaviors/EditorCanvasBehavior.cs:122,140,223`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** PanTool 13/13, ZoomPanManager 10/10, SelectTool 18/18 — все пройдены

## Sprint 52 — Text improvements (fonts, immediate edit, free rotation)

### Fix S52-1: Font internal names mismatch

**Проблема:** `FontNameToFamilyConverter` и `PreviewLineChangedBehavior` использовали URI-фрагменты `#GOST type A`/`#GOST type B`, но фактические внутренние имена — `GOST Type AU`/`GOST Type BU` (регистрозависимые). Шрифты не отображались.

**Исправление:** URI приведены к правильным внутренним именам:
- `#GOST type A` → `#GOST Type AU`
- `#GOST type B` → `#GOST Type BU`

**Файлы:** `Converters/FontNameToFamilyConverter.cs`, `Behaviors/PreviewLineChangedBehavior.cs`, `Resources/Fonts/README.md`

### Fix S52-2: Double-click opens inline editor

**Поведение:** `SelectTool.OnDoubleClick()` вызывает `StartInlineEditing(text)` при двойном клике на текстовый объект. Создание текста через TextTool НЕ открывает редактор — только выделяет объект.

### Fix S52-3: Free rotation angle (0-359°)

**Проблема:** `RotationAngle` был ограничен `{0,90,180,270}`. `ContainsPoint()` и `GetBoundingBox()` — switch-case с неверной геометрией для 90°/270°.

**Исправление:**
- Удалён `ValidRotationAngles`. Сеттер нормализует `value % 360`
- `ContainsPoint()` / `GetBoundingBox()` — общая математика через `cos`/`sin`
- UI: ComboBox → TextBox (произвольный ввод градусов)
- InlineTextEditor: `LayoutTransform` с `RotateTransform`
- `PropertiesViewModel`: удалён вызов `ValidateRotation()`

**Common Mistakes (new):**
31. Rotation direction in WPF RotateTransform — WPF's `RotateTransform` rotates CLOCKWISE (Y-down screen space), which equals COUNTERCLOCKWISE in model Y-up space. `ContainsPoint` must compute `localX = dx*cos + dy*sin; localY = -dx*sin + dy*cos` with `angleRad = RotationAngle * PI / 180`.
32. `PreviewKeyDown` tool switching + InlineTextEditor — when inline editing is active, `Escape`/`Enter` must be intercepted by the TextBox InputBindings, NOT by Window PreviewKeyDown. The `CommitInlineEditingCommand`/`CancelInlineEditingCommand` handlers set `ActiveTool = "Select"` so subsequent PreviewKeyDown events go to select.
33. GostA.ttf internal name is `GOST Type AU` (not `GOST type A` or `GOST Type A`) — case-sensitive. Verify via `GlyphTypeface.FamilyNames`.
34. Text rotation center in WPF — `RotateTransform` rotates around the TextBlock's top-left corner, which is placed at the ContentPresenter's origin. The ContentPresenter top-left maps to model `(X, Y+H)` (the TOP of the text box), NOT the baseline `(X, Y)`. `GetBoundingBox()` and `ContainsPoint()` must rotate around `(X, Y+H)` in ContentPresenter-local space (Y-down), then convert back to model coordinates.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 165+ релевантных пройдены (все ключевые категории)

## Sprint 53 — DateTimeProvider + MarkerPosition + Behaviour tests

### Feature S53-1: IDateTimeProvider (замена Thread.Sleep)

**Проблема:** `FileService.CreateBackup()`, `AutosaveService`, `TemplateService` использовали `DateTime.UtcNow` напрямую. Тесты использовали `Thread.Sleep` (суммарно ~2.3s) для гарантии уникальности timestamp-ов, замедляя тесты и делая их недетерминированными.

**Исправление:**
- Создан `Services/IDateTimeProvider.cs` (интерфейс: `DateTime UtcNow`)
- Создан `Services/DateTimeProvider.cs` (реализация — обёртка над `DateTime.UtcNow`)
- Во все 3 сервиса добавлен опциональный DI-параметр `IDateTimeProvider? dateTimeProvider = null`
- `App.xaml.cs` — регистрация `services.AddSingleton<IDateTimeProvider, DateTimeProvider>()`
- Все 12 случаев `DateTime.UtcNow` заменены на `_dateTimeProvider.UtcNow`

**Тесты:** Все 5 тестовых файлов обновлены:
- `FileServiceTests` — `Mock<IDateTimeProvider>`, строки `Thread.Sleep` из `CreateBackup_MultipleBackups_CreatesUniqueFiles` и `CreateBackup_OverwritesExistingBackup` удалены, тесты используют `SetupSequence`
- `AutosaveServiceTests` — добавлен `Mock<IDateTimeProvider>`
- `TemplateServiceTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_UpdatesModifiedDate`
- `TemplateServiceRoundTripTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_UpdatesModifiedDate`, `CreateTestTemplate` использует `FixedDate`
- `ExtendedServiceTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_OverwritesExistingFile`, `CreateTestTemplate` использует `FixedDate`

### Feature S53-2: MarkerPosition attached behavior (сокращение XAML)

**Проблема:** 14 маркеров выделения (Line×2, Rectangle×8, Text×4) занимали ~250 строк XAML с повторяющимися MultiBinding-блоками `Canvas.Left`/`Canvas.Top`.

**Исправление:** Создан `Behaviors/MarkerPosition.cs` — два attached properties:
- `XPropertyPath` (string) — путь к свойству X-координаты
- `YPropertyPath` (string) — путь к свойству Y-координаты

При установке обоих свойств создаёт MultiBinding для `Canvas.Left` (ModelXToCanvasLeftConverter + Zoom) и `Canvas.Top` (ModelYToCanvasTopConverter + HeightMm + Zoom) через `FindAncestor UserControl`.

XAML каждого маркера сокращён с 12 строк до 2:
```xml
<Rectangle Style="{StaticResource SquareMarker}"
           behaviors:MarkerPosition.XPropertyPath="MicronsX"
           behaviors:MarkerPosition.YPropertyPath="BottomMicronsY"/>
```

**Файлы:**
- `Behaviors/MarkerPosition.cs` — новый файл
- `Views/EditorCanvas.xaml` — 14 маркеров переписаны (~250→40 строк)

### Feature S53-3: Behaviour unit tests (pure functions)

**Проблема:** `EditorCanvasBehavior` содержал 3 конвертации (MouseButton, ModifierKeys, Key) с private-методами, не покрытыми тестами.

**Исправление:**
- `ToToolMouseButton`, `ToToolModifiers`, `ToToolKey` — изменены с `private static` на `internal static`
- Создан `Tests/Behaviors/EditorCanvasBehaviorTests.cs` с 18 theory/fact-тестами:
  - 5 тестов ToToolMouseButton (все MouseButton + fallback)
  - 7 тестов ToToolModifiers (None/Ctrl/Shift/Alt/комбинации)
  - 6 тестов ToToolKey (Escape/Enter/Delete + unknown → null)

**Common Mistakes (new):**
35. `Thread.Sleep` in tests — never use it for timestamp uniqueness. Create `IDateTimeProvider` interface and inject `Mock<IDateTimeProvider>` with `SetupSequence` for different return values.
36. XAML MultiBinding repetition for Canvas.Left/Canvas.Top — create an attached behavior (`MarkerPosition.XPropertyPath`/`YPropertyPath`) that auto-creates MultiBindings with the standard converters and FindAncestor. Reduces ~250 lines to ~40.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 77+ новых тестов (FileService 19 + 5 диалоговых на IDialogFileService, AutosaveService 1, TemplateService 7, RoundTrip 12, Extended 5, EditorCanvasBehavior 18, EditorViewModel 15) — все пройдены

## Sprint 54 — IDialogFileService (изоляция WPF-диалогов)

### Feature S54-1: IDialogFileService (замена OpenFileDialog/SaveFileDialog)

**Проблема:** `FileService.OpenFileDialog()` и `SaveFileDialog()` использовали напрямую WPF `OpenFileDialog`/`SaveFileDialog`. В головной среде (CI, headless) `ShowDialog()` зависает — тесты не могли быть запущены в автоматических пайплайнах. Фильтр xUnit не поддерживал `not`-исключение для этих тестов.

**Исправление:**
- Создан `Services/IDialogFileService.cs` (интерфейс: `OpenFileDialog`, `SaveFileDialog`)
- Создан `Services/WpfDialogFileService.cs` (реализация — перенесён код WPF-диалогов из FileService)
- `FileService` принимает опциональный `IDialogFileService? dialogService = null` (fallback на `WpfDialogFileService(logger)`)
- `App.xaml.cs` — регистрация `services.AddSingleton<IDialogFileService, WpfDialogFileService>()`
- Все 5 тестов диалогов переписаны: используют `Mock<IDialogFileService>` (Verify фильтра/имени файла + возвращаемое значение), никаких вызовов `ShowDialog()` в headless

**Файлы:**
- `Services/IDialogFileService.cs` — новый интерфейс
- `Services/WpfDialogFileService.cs` — новая реализация
- `Services/FileService.cs` — DI + делегирование (строки 13-41)
- `App.xaml.cs:64` — регистрация в DI
- `Tests/Services/FileServiceTests.cs` — 5 тестов с Mock

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 19/19 FileServiceTests — все пройдены

**Common Mistakes (new):**
37. WPF dialogs (OpenFileDialog/SaveFileDialog) must NOT be used directly in services that need CI/testability. Always extract to `IDialogFileService` interface + `WpfDialogFileService` implementation, inject as optional `= null` parameter. Tests use `Mock<IDialogFileService>` returning null — zero UI calls in headless.

## Sprint 55 — Unit test coverage for managers + SelectTool

### Feature S55-1: ToolManagerTests — 17 tests

**Файл:** `Tests/ViewModels/Managers/ToolManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: defaults (ActiveTool="Select"), null logger guard
- GetOrCreateTool<T>: creates new, returns cached, different types, unknown type throws
- ActiveTool setter + PropertyChanged
- PushTool/PopTool: stack behaviour, Pop on empty returns null
- ResetTool: existing, unknown, not-cached

### Feature S55-2: DirtyStateManagerTests — 16 tests

**Файл:** `Tests/ViewModels/Managers/DirtyStateManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: null template guard
- Defaults: IsDirty=false, FilePath=null, DisplayName=""
- MarkDirty: sets IsDirty, idempotent, PropertyChanged
- ClearDirty: PropertyChanged
- UpdateDisplayName: with/without FilePath, Portrait/Landscape
- FilePath setter

### Feature S55-3: GridManagerTests — 24 tests

**Файл:** `Tests/ViewModels/Managers/GridManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: 3× null guard (template, zoomPanManager, logger)
- ToggleGrid / ToggleSnap
- IsGridEnabled / IsSnapEnabled get/set
- GridStepMm / GridStepMicrons conversion
- RefreshGridNodes: disabled, not visible, MinPixelSpacing, centered, not centered, callback, node validation

### Feature S55-4: ZoomPanManagerExtendedTests — 28 tests

**Файл:** `Tests/ViewModels/Managers/ZoomPanManagerExtendedTests.cs` (новый)

**Протестированные сценарии:**
- IsCentered: viewport > canvas, smaller, zero
- CanvasWidth/HeightPixels: zoom scaling
- ViewportWidth/HeightPixels
- ScrollXRange/YRange: zero when centered, positive when not
- ScrollXValue/YValue
- SetScrollX/Y: centered → no-op, not-centered → clamp + pan offset
- CanvasOffsetX/Y
- CenterCanvas, SetGridRefreshCallback, PanCanvas
- PropertyChanged for dependent properties

### Feature S55-5: ClipboardManager + SelectionManager — 13 tests

**Файл:** `Tests/ViewModels/Managers/ManagerTests.cs` (дополнен)

**ClipboardManager:**
- Cut: copies + calls delete action (single, multiple, empty)
- Clear
- GetClipboardContents: clones + offset, offset increment, offset reset after Copy

**SelectionManager:**
- SelectObjects: clears previous, empty → clears, multiple
- IsObjectSelected: true/false/removed/empty
- Constructor fires onSelectionChanged callback
- SelectAll

### Feature S55-6: SelectToolExtendedTests — 22 tests

**Файл:** `Tests/Tools/SelectToolExtendedTests.cs` (новый)

**Протестированные сценарии:**
- OnDoubleClick: text→inline, line→noop, rect→noop, empty→noop
- OnKeyDown: Delete (single/multi/empty/undoable), Escape (clears state + Reset), unknown key → false
- SelectionBox: start, <threshold, >threshold, direction, finalize select, small-move clear
- Reset: clears drag state
- Cursor: hand on hover, cross on handle, arrow by default

### Feature S55-7: Behavior tests removed (STA requirement)

**Проблема:** 9 тестов для WPF attached-property get/set (MarkerPosition, TextBoxLostFocusCommandBehavior, ComboBoxSelectionChangedCommandBehavior, ZoomComboBoxBehavior, TabItemMiddleClickBehavior) создавали WPF-элементы (TextBox/ComboBox/TabControl), что требует STA thread. xUnit runner использует MTA → `InvalidOperationException`.

**Решение:** Файл `BehaviorAttachedPropertyTests.cs` удалён. Поведения остаются без unit-покрытия — требуют integration/UI тестов с STA-инфраструктурой.

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 1599 (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
38. WPF DependencyProperty tests require STA thread — creating WPF elements (`TextBox`, `ComboBox`, `TabControl`) in xUnit tests without STA causes `InvalidOperationException`. Use `[WpfFact]` attribute or STA collection fixture. Pure DP registration (without creating owner elements) may work in MTA.

## Sprint 56 — Colors (StrokeColor/FillColor/Foreground + V-005)

### Feature S56-1: StrokeColor, FillColor, Foreground

**Проблема:** Line и Rectangle не имели StrokeColor, Rectangle не имел FillColor, Text не имел Foreground. Цвета были фиксированным чёрным.

**Исправление (end-to-end):**
- `EditorConstants.cs` — `DefaultStrokeColor = "#000000"`, `DefaultFillColor = "Transparent"`, `DefaultForeground = "#000000"`
- `Line.cs` — `StrokeColor` с INPC + backing field
- `Rectangle.cs` — `StrokeColor` + `FillColor` с INPC
- `Text.cs` — `Foreground` с INPC
- `TemplateDto.cs` / `TemplateService.cs` — маппинг всех цветов (DTO ↔ Model)
- `HexToBrushConverter` — `#RRGGBB`, `#AARRGGBB`, `"Transparent"` → `SolidColorBrush`
- `PropertiesViewModel` — +6 свойств цвета +4 команды изменения
- `PropertiesPanelContent.xaml` — ColorPicker UI с Hex-полем и выбором Transparent
- `EditorCanvas.xaml` — DataTemplate биндинги через Style Setter (Stroke/Fill/Foreground)
- `ValidationService` — V-005: `ValidateHexColor()` — проверка формата HEX + Transparent
- `DrawingLineTool.cs` / `DrawingRectangleTool.cs` / `TextTool.cs` — цвета по умолчанию
- `+36 тестов` (Line/Rectangle/Text цвета, Converter, Validation, RoundTrip)

**Build:** 0 errors, 0 warnings
**Tests:** 1639 passed (0 failures, 1 pre-existing skip)

## Sprint 57 — MultiLine, Half-formats, Library UI, Settings, Documentation

### Feature S57-1: MultiLine + TextAlignment (FR-032)

**Проблема:** Text не поддерживал многострочный текст и выравнивание. InlineTextEditor не имел AcceptsReturn.

**Исправление:**
- `Text.cs` — `TextWrapping` (bool) + `TextAlignment` (string: "Left"/"Center"/"Right") с INPC
- `BoolToTextWrappingConverter` — bool → TextWrapping
- `StringToTextAlignmentConverter` — string → TextAlignment
- `TextAlignmentToIndexConverter` — string → int (ComboBox SelectedIndex)
- `EditorCanvas.xaml` — TextBlock биндинги TextWrapping/TextAlignment
- `InlineTextEditor` — AcceptsReturn=True привязан к TextWrapping; Ctrl+Enter → commit, Enter → новая строка
- `PropertiesViewModel` — +TextTextWrapping/TextTextAlignment + relay-команды
- `PropertiesPanelContent.xaml` — ComboBox выравнивания, CheckBox переноса строк
- `+22 теста`

### Feature S57-2: Half-formats (A4×2, A3×2, A2×2, A1×2, A0×2)

**Проблема:** Требовались форматы с удвоенной длинной стороной для чертежей.

**Исправление:**
- `Sheet.FromFormat()` — +5 форматов: 210×594…841×2378 мм, все Portrait по умолчанию
- `Sheet.GetDefaultOrientation()` — ×2 форматы → Portrait
- `ValidationService.ValidFormats` — +10 entry (×2/X2 для каждого формата × P/L)
- `MainWindow.xaml` — подменю в File > New для half-форматов
- `+25 тестов`

### Feature S57-3: Библиотека шаблонов (FR-043)

**Проблема:** Кнопки Import/Remove в TemplateLibraryViewModel существовали как команды, но не были привязаны в XAML.

**Исправление:**
- `MainViewModel.cs` — передача `IFileService` в `TemplateLibraryViewModel`
- `MainWindow.xaml` — тулбар с кнопками «Импорт» / «Удалить» в левой панели

### Feature S57-4: Настройки (UI)

**Проблема:** Отсутствовал графический интерфейс для изменения настроек приложения.

**Исправление:**
- `SettingsViewModel` — Theme, ShowGrid, SnapToGrid, GridStepMm, AutosaveIntervalMinutes, DefaultSheetFormat, DefaultZoom
- `SettingsView.xaml` + `.cs` — модальное окно 420×440 с 4 секциями, Сохранить/Отмена
- `WpfDialogHostService` — dispatch SettingsViewModel → SettingsView
- `MainViewModel` — +OpenSettingsCommand
- `MainWindow.xaml` — пункт «Настройки...» в меню File
- `+6 тестов`

### Fix S57-5: Документация

**Обновлено:**
- `02_User_Stories_Этап1.md` — 122 чекбокса → ✅
- `19_Статус_проекта.md` — 1760 тестов, Sprint 57 в динамике
- `05_Руководство_пользователя_черновик.md` — раздел 10 переписан (Settings), хоткей V/L/R/T/E
- `docs/archive/` — 82 устаревших файла перемещены
- `AGENTS.md` — добавлены Sprint 56-57, пути к архиву обновлены

**Build:** 0 errors, 0 warnings
**Tests:** 1760 passed (0 failures, 1 pre-existing skip)

## Sprint STA — Unit tests for WPF behaviors (STA-thread)

### Feature STA-1: WpfContext helper
Создан `Tests/Helpers/WpfContext.cs` — STA-thread dispatcher. Создаёт поток с `ApartmentState.STA`, устанавливает `DispatcherSynchronizationContext`, выполняет action и завершает `Dispatcher`.

### Feature STA-2: Behavior handlers → internal static
4 файла изменены — `private static` handlers → `internal static`:
- `TextBoxLostFocusCommandBehavior.OnLostFocus` / `OnKeyDown`
- `ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged`
- `ZoomComboBoxBehavior.OnSelectionChanged` / `OnDropDownClosed` / `ApplyZoom`

Паттерн уже используется в `CanvasInputRouter` (Sprint 53).

### Feature STA-3: TextBoxLostFocusCommandBehaviorTests — 14 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set | Set, Get, Clear (3) |
| OnLostFocus | Execute, CanExecute=false, null command, non-TextBox sender (4) |
| OnKeyDown | Enter execute + Handled, non-Enter skip, CanExecute=false, null command, non-TextBox sender (7) |

### Feature STA-4: ComboBoxSelectionChangedCommandBehaviorTests — 10 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set | Set, Get, Clear, non-ComboBox (4) |
| OnSelectionChanged | Execute, CanExecute=false, null command, non-ComboBox sender, null SelectedItem (6) |

### Feature STA-5: ZoomComboBoxBehaviorTests — 11 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set (DependencyObject) | Set, Get, Clear (3) |
| DP get/set (ComboBox) | Set (1) |
| ApplyZoom | Percent, plain, invalid, zero/negative, no-editor, spaces (6) |
| Events | SelectionChanged, DropDownClosed (2) |

`EditorViewModel` — real instance (not mock) via `ITemplateService`/`IPrintService`. Verify via `editor.ZoomPanManager.Zoom`.

### Feature STA-6: MarkerPositionTests — 10 тестов

DP get/set для `XPropertyPath` и `YPropertyPath` (DependencyObject, null/storage, independence).

**Пропущено (исторически):** `TabItemMiddleClickBehavior`, `PreviewLineChangedBehavior` — требовали полного визуального дерева. **Р ешено в Sprint 62** (23 STA-теста, 12 + 11).

**Build:** 0 errors, 0 warnings
**Tests:** 1780 passed (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
39. WPF `Control` constructor requires STA — `new ComboBox()`, `new TextBox()`, `new Button()` throw `InvalidOperationException` on MTA. Always create WPF elements inside an STA thread (via `WpfContext.Execute`).
40. Moq cannot mock non-virtual methods — `SetZoomPercent` is not virtual → use real `EditorViewModel` instance and verify via `editor.Zoom` instead of `mock.Verify`.
41. `Mock<T>(MockBehavior, params object[] args)` with nullable reference types — passing `(GridSettings?)null` to `object[]` triggers CS8625/CS8604. Use a `GridSettings?` local variable set to `null` or `null!`.
42. `PresentationSource` in .NET 10 WPF — the abstract class requires `GetCompositionTargetCore()`, `RootVisual` getter/setter, and `IsDisposed`. `GetVisualRoot()` no longer exists. Create `FakePresentationSource` implementing all abstract members.

## Sprint R3.1 — EditorViewModel де-bloat (forwarding-свойства → менеджеры)

### Что сделано

**Проблема:** EditorViewModel содержал ~60 forwarding-свойств, дублирующих свойства менеджеров (ZoomPanManager, PreviewManager, StatusBarManager и др.). Каждое свойство имело `OnPropertyChanged()` в сеттере для ретрансляции уведомлений на EditorViewModel. Ретрансляция требовалась, когда XAML биндился к EditorViewModel, но после R3.1 XAML уже биндился напрямую к менеджерам — forwarding стал мёртвым грузом. Дополнительно 4 обработчика `PropertyChanged` подписывались на менеджеров и пере-оповещали EditorViewModel.

**Исправление:**
- Удалены ~25 forwarding-свойств (те, что не требуются IEditorContext):
  - `CanvasWidthPixels`, `CanvasHeightPixels`, `PanOffsetX/Y`, `ZoomPercent`, `ViewportWidth/HeightMm`, `ViewportWidth/HeightPixels`, `ScrollX/YRange`, `ScrollX/YValue`, `IsCentered`, `CanvasOffsetX/Y`
  - `ShowSelectionMarkers`, `GridNodes`, `GridInvalidated`
  - `StatusBarSheetFormat`, `StatusBarGridEnabled`, `StatusBarGridStepMm`, `StatusBarSnapEnabled`
  - `ActiveTool`, `InlineEditingText`, `InlineEditText`
- Упрощены ~15 свойств IEditorContext (убраны `OnPropertyChanged()`):
  - `PreviewLine`, `PreviewRectangle`, `PreviewText`, `SelectionBoxLeft/Bottom/Top/Width/Right/Height`, `SelectionDirection`, `StatusMessage`, `Zoom`
- Удалены 4 поля-обработчика, 4 подписки `PropertyChanged` в конструкторе, 4 отписки в `Dispose`
- `OnZoomChangedInternal` удалён (заменён на `() => { }`)
- `IAutosaveTab` (`IsDirty`, `FilePath`, `DisplayName`) — explicit interface implementation
- `OnSelectionChangedInternal` упрощён (убраны `ShowSelectionMarkers`, `SingleSelectedObject`)
- `PreviewLineChangedBehavior` переписан на `PreviewManager.PropertyChanged`
- ~90 тестов исправлены на manager-свойства

**Результат:**
```
EditorViewModel: ~1194 → 784 строк (−410, −34%)
Build:  0 errors, 0 warnings
Tests:  1780 passed, 1 skip
```

**Файлы:**
- `ViewModels/EditorViewModel.cs` — основной файл рефакторинга
- `Behaviors/PreviewLineChangedBehavior.cs` — переписан на PreviewManager
- `MainViewModel.cs` — 9 замен (DirtyStateManager)
- `EditorCanvas.xaml` / `.xaml.cs` — 7 замен (ZoomPanManager, GridManager)
- `CanvasInputRouter.cs` — 2 замены
- `EditorViewModelTests.cs` — ~90 исправлений

## Sprint R3.1-HF1 — Preview fix (unconditional PropertyChanged)

**Проблема:** `[ObservableProperty]` на `PreviewLine`/`PreviewRectangle`/`PreviewText` в `PreviewManager` подавлял `PropertyChanged` при re-assign той же ссылки (`EqualityComparer<T>.Default.Equals()` для reference-типов = `ReferenceEquals`). Три инструмента (DrawingLineTool, DrawingRectangleTool, TextTool) мутируют существующий preview-объект и переустанавливают его — PropertyChanged не стреляет, `PreviewLineChangedBehavior` не обновляет WPF-элементы, предпросмотр пропадает.

**Исправление:** `[ObservableProperty]` заменён на ручные сеттеры с безусловным `OnPropertyChanged()` для трёх полей. SelectionBox-поля (`long`, `byte`) не тронуты — equality check для value-типов корректен.

**Файл:** `ViewModels/Managers/PreviewManager.cs` (3 поля, ~6 строк)

## Sprint R3.1-HF2 — Selection markers fix (ShowSelectionMarkers notification)

**Проблема:** После R3.1 XAML биндится напрямую к `SelectionManager.ShowSelectionMarkers` (computed property: `=> SelectedObjects.Count > 0`). Однако `PropertyChanged` для этого свойства никогда не вызывался — при изменении коллекции `SelectedObjects` срабатывал только переданный `_onSelectionChanged`-коллбэк в `EditorViewModel`. WPF-биндинг застывает на `Collapsed`.

**Исправление:** В конструктор `SelectionManager` добавлен `OnPropertyChanged(nameof(ShowSelectionMarkers))` в лямбду `CollectionChanged`.

**Файл:** `ViewModels/Managers/SelectionManager.cs` (1 строка)

## Phase 4 — PropertiesViewModel split (649→85 lines)

**Done:** PropertiesViewModel разделён на базу + 3 sub-VM. Прямые биндинги заменены на ContentControl + DataTemplate.

| Файл | Было | Стало |
|------|------|-------|
| PropertiesViewModel.cs | 649 строк (монолит) | 85 строк (база: selection + sub-VM lifecyle) |
| LinePropertiesViewModel.cs | — | 148 строк (7 свойств + 14 команд + INPC) |
| RectanglePropertiesViewModel.cs | — | 168 строк (8 свойств + 16 команд + INPC) |
| TextPropertiesViewModel.cs | — | 233 строки (13 свойств + 20 команд + INPC) |
| PropertiesPanelContent.xaml | 549 строк (3×StackPanel) | ~620 строк (3×DataTemplate + ContentControl) |
| PropertiesViewModelTests.cs | 1313 строк | 1106 строк (sub-VM property/command paths) |
| PropertiesViewModelCommandTests.cs | 325 строк | 262 строки (sub-VM command paths) |

**Изменения:**
- Каждый sub-VM: `ObservableObject` + `UpdateObject(T?)` + INPC forwarding + `SetProperty` + `[RelayCommand]`
- Sub-VM подписываются на `INotifyPropertyChanged.PropertyChanged` модели для live-обновления
- XAML: 3 visible StackPanel → 3 DataTemplate на тип + `ContentControl Content="{Binding LineVM/RectVM/TextVM}"`
- Base VM: только `SelectedObject`, `SelectionCount`, `IsSingleSelection`, `IsLineSelected`, `IsRectangleSelected`, `IsTextSelected`, `ObjectId`, `ObjectTypeName`, `ValidationError`; sub-VM паблишеры через конструктор
- `PropertiesViewModel.SetProperty()` удалён из базы (логика в sub-VM)
- `PropertiesPanelContent.xaml.cs`: `OnTextIsEditableClick` обновлён на `textVm.ChangeIsEditableCommand.Execute()`

**Build:** 0 errors, 0 warnings
**Tests:** 1796 passed, 1 pre-existing skip

## Sprint — Print Preview (Ctrl+Shift+P)

### Feature: Предпросмотр печати

**Проблема:** Отсутствовал предпросмотр печати — пользователи не могли видеть, как будет выглядеть шаблон на листе перед печатью. Ранее был только прямой вывод на принтер через `PrintDialog`.

**Исправление:** Реализован end-to-end предпросмотр через `DocumentViewer` с `FixedDocument`:

| Компонент | Файл | Назначение |
|-----------|------|------------|
| Интерфейс | `Services/IPrintDocumentGenerator.cs` | Контракт: `FixedDocument Generate(Template)` |
| Генератор | `Services/PrintDocumentGenerator.cs` | Model → WPF элементы (Line, Rectangle, TextBlock) с конвертацией координат (микроны→WPF, Y-flip) |
| Окно | `Views/PrintPreviewWindow.xaml` + `.cs` | DocumentViewer с FitToWidth, Print кнопкой, Close |
| Интеграция | `ViewModels/MainViewModel.cs` | PreviewPrintCommand, DI IPrintDocumentGenerator |
| UI | `MainWindow.xaml` | MenuItem + Ctrl+Shift+P KeyBinding |
| DI | `App.xaml.cs` | Transient registration |
| Тесты | `Tests/Services/PrintDocumentGeneratorTests.cs` | 19 тестов: элементы, координаты, цвета, типы линий, поворот, несколько объектов |

**Архитектурные решения:**
- FixedDocument + WPF-элементы (не RenderTargetBitmap) — векторное качество, совместимость с DocumentViewer
- Отдельный `IPrintDocumentGenerator` — не заменяет `IPrintService`
- Transient регистрация — stateless генератор
- FitToWidth при загрузке — автоподгонка под окно

## Sprint 58 — Архитектурный анализ (аналитический спринт)

Полный отчёт: [`docs/48_Архитектурный_анализ_и_план_рефакторинга.md`](docs/48_Архитектурный_анализ_и_план_рефакторинга.md)

### Найденные архитектурные проблемы (25 замечаний)

Ключевые находки:
- **P0:** EditorViewModel — god-object «фасад с пробросом» (1160 строк, ~60 forwarding-свойств, 4 switch-обработчика для ретрансляции INPC). **Решено (Sprint R3.1):** ~1194→784 строк, forwarding удалён, XAML биндится к менеджерам.
- **P1:** Избыточная 3-уровневая иерархия моделей (ObjectBase → ModelBase → TemplateObjectBase). Решение: схлопнуть в один уровень.
- **P1:** ~50 дублированных INPC-setter в Line/Rectangle/Text. Решение: `[ObservableProperty]` source generator.
- **P1:** MoveSelected/RotateSelected не группируются в BatchCommand (inconsistent Undo).
- **P1:** ValidationService — static 537-строчный god-service, untestable.
- **P1:** Нет Central Package Management, `TreatWarningsAsErrors` только в CI.

### План рефакторинга R1–R4

| Спринт | Цель | Длительность |
|--------|------|-------------|
| R1 | Быстрые победы: CPM, TreatWarningsAsErrors, Undo-группировка, flaky-тесты | 2–3 дня |
| R2 | Models cleanup: иерархия, `[ObservableProperty]`, ITemplateValidator | 3–4 дня |
| R3 | EditorVM de-bloat: проброс через менеджеры, IEditorContext, DI | 4–5 дней |
| R4 | Presentation + Tests: EditorCanvasBehavior, CI coverage-gate | 4–5 дней |

## Sprint 59 — Grid bug fixes (PropertyChanged, мёртвый код, ComputeDisplayStep)

### Fix SG-1: IsGridEnabled/IsSnapEnabled не дёргали PropertyChanged

**Проблема:** Сеттеры `GridManager.IsGridEnabled` и `IsSnapEnabled` не вызывали `OnPropertyChanged()`. При программном изменении (меню, код) XAML ToggleButton на тулбаре не обновлял `IsChecked`.

**Исправление:** Добавлен `OnPropertyChanged()` в оба сеттера.

### Fix SG-2: ToggleGrid() десинхронизировал Enabled и Visible

**Проблема:** `ToggleGrid()` переключал только `Enabled`. Если до вызова было `Enabled=false, Visible=false` (через сеттер), после `ToggleGrid()` становилось `Enabled=true, Visible=false` — сетка скрыта.

**Исправление:** `ToggleGrid()` переписан через `IsGridEnabled = !IsGridEnabled` (сеттер).

### Fix SG-3: Truncation вместо Rounding в координатах узлов

**Проблема:** `(long)` каст в `RefreshGridNodes()` отбрасывал дробную часть. На высоком zoom — ошибка позиционирования.

**Исправление:** `(long)` → `(long)Math.Round()`.

### Fix SG-4: Мёртвый код GridLine/GenerateGridLines/GenerateVisibleGridLines

**Проблема:** `GridHelper` содержал struct `GridLine` и два метода генерации линий (~90 строк), которые никогда не вызывались в production. Только тесты.

**Исправление:** Удалены `GridLine`, `GenerateGridLines()`, `GenerateVisibleGridLines()`. Удалены 13 тестов для этих методов.

### Fix SG-5: GridStepToStringConverter — ненадёжный парсинг

**Проблема:** `ConvertBack` удалял только `"мм"`. Другие форматы (`"5 mm"`, `"5,5"`) молча возвращали `5.0`.

**Исправление:** Конвертер удаляет любой нечисловой суффикс (Regex), нормализует comma→dot, при ошибке парсинга возвращает `Binding.DoNothing`.

### Fix SG-6: Изменение шага сетки не влияло на отображение

**Корневая причина:** `GridManager.RefreshGridNodes()` вызывал `ComputeDisplayStep()`, который **полностью игнорировал** `_gridSettings.StepMicrons`. Шаг вычислялся только из `MinPixelSpacing / zoom`. Пользовательский шаг нигде не участвовал.

**Исправление:**
- `ComputeDisplayStep()` принимает `preferredStepMicrons` (опциональный параметр)
- Если `preferredStep` даёт `pixelSpacing >= MinPixelSpacing` — используется как целевой
- Если `pixelSpacing < MinPixelSpacing` — fallback на `MinPixelSpacing / zoom`
- В обоих случаях шаг coarsen'ится если `cols * rows > maxNodes`
- В `GridStepMm` сеттер добавлен `OnPropertyChanged()`

### Common Mistakes (new)
43. `async void` in timer handler — `MainViewModel.OnAutosaveTickHandler()` is `async void` subscribed to `AutosaveTick`. If `AutosaveAllTabsAsync` throws, exception is lost (not caught). Always use try/catch with logging inside `async void`, or wrap in `SafeFireAndForget`.
44. Multi-Undo inconsistency — `DeleteSelected()` and `PasteFromClipboard()` group multi-object operations into `BatchCommand`, but `MoveSelected()` and `RotateSelectedClockwise()` do NOT. User presses Undo N times for N objects. Always apply `BatchCommand` when `SelectedObjects.Count > 1`.
45. File name ≠ type name — `Commands/IUndoCommand.cs` contains interface `IUndoCommand` (renamed from `ICommand.cs` to avoid WPF conflict). The file name is misleading and may cause wrong `using` imports. Rename to `IUndoCommand.cs`.
46. `IAutosaveTab` defined inside service — `Services/AutosaveService.cs` contains `public interface IAutosaveTab`. EditorViewModel explicitly implements it. Service dictates interface to ViewModel (inverted dependency). Always define interfaces near their consumer, not provider.
47. `PrintVisualProvider` leaks WPF type — `Func<System.Windows.Media.Visual?>` on EditorViewModel exposes WPF rendering to ViewModel. View sets it, creating a potential dangling reference after tab close. Encapsulate via interface or use WeakReference/Messenger.
48. Resize undo — use `ChangePropertyCommand<ResizeState>` (initial state, `ApplyResize` setter, final state), NOT a dedicated command class with `switch (_object)` per type (the old `CustomResizeCommand` violated OCP and reused `_newHeight` as FontSize for Text — it was removed). Polymorphic `CaptureResizeState()`/`ApplyResize()` on the object is the single source of truth.
49. `ValidationService` is static and untestable — `Helpers/ValidationService` is a `static class` called directly from PropertiesViewModel and TemplateService. Cannot be mocked. Make domain validation injectable (`ITemplateValidator`). UI field validators can stay static as pure functions.
50. No Central Package Management — package versions are hardcoded in two csproj files. No `Directory.Packages.props`. Versions drift independently. Adopt CPM (`<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` + `Directory.Packages.props`).
51. csproj duplicates Directory.Build.props — `TargetFramework`, `Nullable`, `ImplicitUsings` declared in both `Directory.Build.props` and each csproj. Remove duplicates from csproj, keep only project-specific properties (`OutputType`, `UseWPF`).
52. `TreatWarningsAsErrors` only in CI — local builds don't catch warnings. CI `analyze` job uses `/warnaserror`, but developers see warnings only after push. Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props`.

53. `ResizeTool.OnMouseUp` pushes `ChangePropertyCommand<ResizeState>(initialState, s => captured.ApplyResize(s), finalState, "размер")` (аргумент markDirty удалён в кандидате 3 обзора №4 — грязность обеспечивает CommandHistory). Do NOT pass raw `long` coordinates to resize commands and do NOT create per-type factory methods — resize is expressed through the object's `CaptureResizeState()`/`ApplyResize()`.
54. `ResizeMath` — all pure resize calculations live in `Tools/ResizeMath.cs`. `ResizeTool.cs` delegates to it. Do NOT add new resize math to ResizeTool directly.
55. `ShortcutRegistry` — add new keyboard shortcuts to `Helpers/ShortcutRegistry.cs`, NOT to `MainWindow.xaml.cs`.
56. `CaptureResizeState`/`ApplyResize` — every new model subclass of `TemplateObjectBase` MUST implement these two methods for undoable resize to work.
57. Test file merging — after R4.4, there are NO Extended/Additional test files. All tests live in the parent files. Create tests in the parent, not in separate files.
58. Coverage gate — CI checks coverage ≥80%. On failure, the build is red. Generate coverage locally with `dotnet test --collect:"XPlat Code Coverage"` before pushing.
59. Forwarding properties after R3.1 — after XAML was migrated to bind to managers directly (R3.1), forwarding properties on EditorViewModel became dead code. Remove them: delete the property, delete the `OnPropertyChanged()` in setters of IEditorContext-required properties, remove PropertyChanged forwarding handlers (`_zoomPanHandler`, `_previewHandler`, `_dirtyStateHandler`, `_toolManagerHandler`), remove `OnZoomChangedInternal()`, and simplify `OnSelectionChangedInternal()`. IAutosaveTab properties become explicit interface implementation. Test references must use `editor.XManager.Y` instead of `editor.Y`.
60. `[ObservableProperty]` on reference-type fields with re-assign — the source-generated setter uses `EqualityComparer<T>.Default.Equals()`, which for reference types defaults to `ReferenceEquals`. If you mutate the same instance and re-assign it, `PropertyChanged` is suppressed. **Историческое применение к preview (ручные сеттеры с безусловным notify под ре-ассайн-трюк) удалено в кандидате 2 обзора №4 (PR #96): preview-объекты назначаются только новой ссылкой, контракт «уведомление только при смене ссылки» зафиксирован тестом; правило остаётся в силе для любых других reference-полей, где ре-ассайн той же ссылки значим.**
61. Computed properties (expression-bodied, no `[ObservableProperty]`) on ObservableObject managers that are bound from XAML must fire `OnPropertyChanged()` explicitly when their dependencies change. The binding engine only re-evaluates when `PropertyChanged` fires for that property name — it does NOT infer dependencies from the expression body.
62. WPF RotateTransform matrix — WPF использует STANDARD CCW Cartesian matrix `x'=x*cosθ−y*sinθ, y'=x*sinθ+y*cosθ`. В Y-down (screen) это даёт CW-вращение. RotatedCorner*/GetBoundingBox используют forward transform. ContainsPoint() использует INVERSE transform `u=x*cos+y*sin, v=−x*sin+y*cos` для unrotate точки в локальное пространство текста. Не путать с CW-specific формулами — они неверны для WPF.

## Sprint A–D — Архитектурный рефакторинг (18 замечаний)

После Sprint 59 был проведён Архитектурный анализ (48_Архитектурный_анализ_и_план_рефакторинга.md) и составлен план рефакторинга на 4 спринта (A–D). Выполнено 12 из 14 пунктов, 2 пропущены (P4).

### A.1 — IDisposable в sub-VM (утечка памяти)

**Проблема:** `LinePropertiesViewModel`, `RectanglePropertiesViewModel`, `TextPropertiesViewModel` подписывались на `INotifyPropertyChanged.PropertyChanged` модели в `UpdateObject()`, но никогда не отписывались. При закрытии вкладки sub-VM продолжали висеть в памяти через delegate.

**Исправление:** Добавлены `IDisposable.Dispose()` во все 3 sub-VM с отпиской. `PropertiesViewModel.Dispose()` каскадно вызывает dispose всех трёх.

**Файлы:**
- `LinePropertiesViewModel.cs`, `RectanglePropertiesViewModel.cs`, `TextPropertiesViewModel.cs`, `PropertiesViewModel.cs`

### A.2 — Dual-write GridManager/StatusBarManager

**Проблема:** `StatusBarManager` владел отдельной копией `GridSettings` (StepMicrons, GridEnabled, SnapEnabled), дублируя состояние `GridManager`. Два независимых источника истины — мутация через UI (StatusBar) не синхронизировалась с GridManager.

**Исправление:** `StatusBarManager` больше не содержит `GridSettings`. Конструктор принимает 6 делегатов (get/set для GridEnabled, GridStepMm, SnapEnabled) + `Action onGridRefresh`. `EditorViewModel` передаёт лямбды к `GridManager`. `GridManager` — единственный owner `GridSettings`.

**Файлы:** `StatusBarManager.cs`, `EditorViewModel.cs`, `GridManager.cs`

**Common Mistakes (new):**
63. Dual-write in managers — never give two managers independent copies of the same mutable settings. One must be the single source of truth; others delegate via lambdas or events.

### B.1 — FontMetrics: static → instance + DI

**Проблема:** `FontMetrics` — полностью static class. Тесты не могли мокировать, DI-контейнер не имел интерфейса.

**Исправление:** Создан `IFontMetrics` interface. `FontMetrics` переведён из static в instance class, реализующий `IFontMetrics`. Добавлен `static readonly FontMetrics Default = new()` для backward compat. DI-регистрация: `services.AddSingleton<IFontMetrics>(FontMetrics.Default)`. `Text.cs` использует `FontMetrics.Default.GetHeightRatio/FontMetrics.Default.GetAdvWidthRatio`.

Для устранения flaky race-условий в параллельных тестах (FontMetricsTests и TextTests/HitTestHelperTests модифицировали shared state одновременно) добавлен `[Collection("FontMetrics", DisableParallelization = true)]`.

**Файлы:** `Models/IFontMetrics.cs` (новый), `FontMetrics.cs`, `Text.cs`, `App.xaml.cs`, `FontMetricsTests.cs`, `TextTests.cs`, `HitTestHelperTests.cs`, `FontMetricsTestCollection.cs` (новый)

### B.2 — PanOffsetX/Y forwarding удалены из EditorViewModel

**Проблема:** После R3.1 XAML биндится напрямую к `ZoomPanManager.PanOffsetX/Y`, но EditorViewModel продолжал содержать forwarding-свойства `PanOffsetX`/`PanOffsetY`. Тесты использовали `editor.PanOffsetX` вместо `editor.ZoomPanManager.PanOffsetX`.

**Исправление:** Свойства удалены из EditorViewModel. Тесты заменены: `editor.PanOffsetX` → `editor.ZoomPanManager.PanOffsetX`.

**Файлы:** `EditorViewModel.cs`, `EditorViewModelTests.cs`, `PanToolTests.cs`

### B.3 — EditorConstants → PhysicalConstants/EditorSettings

**Проблема:** `EditorConstants.cs` — 36-line pure proxy, каждая константа ре-экспортировала `PhysicalConstants.XXX` или `EditorSettings.XXX`. 69 references в 20 файлах.

**Исправление:** Все 69 references заменены прямыми вызовами `PhysicalConstants.XXX` или `EditorSettings.XXX`. `EditorConstants.cs` удалён.

**Файлы:** 20 файлов обновлены, `EditorConstants.cs` удалён.

### C.1 — Shortcuts из code-behind в ShortcutRegistry

**Проблема:** `Window_PreviewKeyDown` (30 строк) содержал логику диспетчеризации клавиш. Добавление нового хоткея требовало изменения code-behind.

**Исправление:** Создан `ShortcutRegistry.TryHandle(Key, ModifierKeys, EditorViewModel) → bool` — единая точка входа. `Window_PreviewKeyDown` сокращён до 3 строк.

**Файлы:** `ShortcutRegistry.cs`, `MainWindow.xaml.cs`

### C.2 — Tag-parsing в CustomSheetDialogViewModel

**Проблема:** Кнопки быстрого выбора формата (A4/A3/…) использовали `Tag="210,297"` с парсингом в code-behind (`OnQuickFormatClick`, `string.Split(',')`). Нетестируемо, XAML-зависимо.

**Исправление:** Код из code-behind удалён. Добавлен `SetQuickFormatCommand(string formatName)` в `CustomSheetDialogViewModel`, вызывающий `Sheet.FromFormat(formatName)`. XAML: `Click` + `Tag` → `Command="{Binding SetQuickFormatCommand}" CommandParameter="A4"`. Code-behind файл сокращён до конструктора.

**Файлы:** `CustomSheetDialogViewModel.cs`, `CustomSheetDialog.xaml`, `CustomSheetDialog.xaml.cs`

### C.3 — No-op Dispose удалён из TemplateLibraryViewModel

**Проблема:** `TemplateLibraryViewModel` реализовывал `IDisposable` с пустым телом. Вызов `Dispose()` в `MainViewModel.Dispose()` — мёртвый код.

**Исправление:** `IDisposable` удалён из класса. Вызов `TemplateLibraryVm?.Dispose()` удалён из `MainViewModel.Dispose()`.

**Файлы:** `TemplateLibraryViewModel.cs`, `MainViewModel.cs`

### C.4 — ITool.OnMouseWheel → bool

**Проблема:** `ITool.OnMouseWheel` возвращал `void` — инструменты не могли заблокировать зум. CanvasInputRouter безусловно применял zoom после вызова `OnMouseWheel`.

**Исправление:** `ITool.OnMouseWheel` теперь возвращает `bool` — `true` означает «событие обработано, зум не применять». Все 6 реализаций обновлены (возвращают `false`). `CanvasInputRouter` проверяет return value.

**Файлы:** `ITool.cs`, `SelectTool.cs`, `PanTool.cs`, `DrawingLineTool.cs`, `DrawingRectangleTool.cs`, `TextTool.cs`, `ResizeTool.cs`, `CanvasInputRouter.cs`, `ToolManagerTests.cs`

### C.5 — Memory leak SelectionManager

**Проблема:** `SelectionManager` подписывался на `SelectedObjects.CollectionChanged` в конструкторе, но отписка не была предусмотрена.

**Исправление:** `SelectionManager` реализует `IDisposable`. Хэндлер сохранён в поле `_onCollectionChanged`, отписка в `Dispose()`. `EditorViewModel.Dispose()` вызывает `_selectionManager.Dispose()`.

**Файлы:** `SelectionManager.cs`, `EditorViewModel.cs`

### D.1 — MockBehavior.Strict → Loose

3 мока `ICommand` в behavior-тестах использовали `MockBehavior.Strict` — при добавлении нового метода в `ICommand` тесты падали. Заменены на `MockBehavior.Loose`.

**Файлы:** `TextBoxLostFocusCommandBehaviorTests.cs`, `ComboBoxSelectionChangedCommandBehaviorTests.cs`

### D.3 — Sealed классы

66 классов помечены `sealed`: все Converters (27), Commands (5), Services (20), Tools (8), Managers (9).

**Common Mistakes (new):**
64. `IDisposable` with lambda subscriptions — always save the handler reference to a field and unsubscribe in `Dispose()`. Lambda-in-constructor subscriptions can't be removed without a stored reference.
65. `ITool.OnMouseWheel` return type — use `bool` (handled flag), consistent with `OnKeyDown`. Tools that don't need wheel handling return `false`; future tools can block zoom by returning `true`.

## Sprint — Grid refactoring (Points 1, 4, 5 из архитектурного обсуждения)

### Что сделано

Три взаимосвязанных изменения в архитектуре сетки:

**Point 1 — Хранение микронов вместо пикселей:**
- `GridNodesLayer` теперь хранит координаты узлов в **микронах** (model space), а не в пикселях
- `OnRender` сам конвертирует микроны → пиксели (zoom + Y-flip)
- Добавлены DependencyProperty `Zoom` и `SheetHeightMm` — при изменении zoom`а или высоты листа перерисовка через `InvalidateVisual`
- `GridNodesLayer` изменён с `UIElement` на `FrameworkElement` (нужен для WPF Data Binding через DPs)
- При зуме больше НЕ требуется регенерация узлов (только смена шага или viewport)

**Point 4 — Упрощение pan-кэширования (удалено целиком):**
- Удалены: `_cachedRegionLeftMicrons`, `_cachedRegionBottomMicrons`, `_cachedRegionWidthMicrons`, `_cachedRegionHeightMicrons`, `_hasCachedRegion`
- Удалены: `IsWithinCachedRegion()`, `InvalidateCacheOnPan()`, `RefreshOnPanEnd()`
- Удалены: `_debounceCts`, `SuppressDebounce`, `PanDebounceMs`
- Удален: `_onPanRefresh` в ZoomPanManager, `SetPanRefreshCallback()`, вызов в `PanCanvas()`
- При панорамировании сетка движется через `RenderTransform` (TranslateTransform) — без регенерации
- Регенерация на pan-end: прямой вызов `RefreshGridNodes()` (без дебаунса, без кэша)

**Point 5 — Buffer safety:**
- `GridManager._nodeData` больше не `readonly` с мутацией — каждый `RefreshGridNodes()` аллоцирует **новый** `long[]`
- Нет shared mutable state между GridManager и GridNodesLayer
- SetNodes сохраняет ссылку на массив, который гарантированно не мутируется после передачи

### Итоговый diff

| Мера | До | После |
|------|----|-------|
| GridManager строк | 246 | 145 |
| Полей кэша | 5 | 0 |
| Методов pan-caching | 3 | 0 |
| CTS / Debounce | 2 (InvalidateGrid + InvalidateCacheOnPan) | 0 |
| Shared mutable long[] | Да (одна аллокация, мутация) | Нет (новая аллокация на refresh) |
| Регенерация на zoom | Full (pixel conv + nodes) | Full (nodes only, ~2× быстрее) |
| Регенерация на pan-move | Debounced 50ms | Нет (только RenderTransform) |
| Регенерация на pan-end | Всегда + сброс кэша | Всегда (без pixel conv, без кэша) |

**Файлы:**
- `Views/GridNodesLayer.cs` — DPs, OnRender, FrameworkElement
- `ViewModels/Managers/GridManager.cs` — -105 строк (удалён кэш, pixel conv, debounce)
- `ViewModels/Managers/ZoomPanManager.cs` — удалён _onPanRefresh
- `ViewModels/EditorViewModel.cs` — удалён SetPanRefreshCallback
- `Behaviors/CanvasInputRouter.cs` — RefreshOnPanEnd → RefreshGridNodes
- `Views/EditorCanvas.xaml` — Zoom/SheetHeightMm bindings
- `Views/EditorCanvas.xaml.cs` — упрощён GridInvalidated handler
- `Tests/ViewModels/Managers/GridManagerTests.cs` — SuppressDebounce removed, PoolReusesSameArray → AllocatesNewArrayEachCall, pixel→micron asserts
- `Tests/ViewModels/EditorViewModelTests.cs` — pixel→micron asserts

**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Sprint 60 — Fix inline text editing (auto-focus, Escape/LostFocus routing, ShortcutRegistry guard)

### 6 исправлений

**Fix 60-1: AutoFocusOnVisibleBehavior**
- Новый attached behavior: при `IsEnabled=True` и `IsVisibleChanged` → `element.Focus()` + `SelectAll()` для TextBox
- Через `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` — layout должен завершиться
- Отписка от `IsVisibleChanged` при `IsEnabled→false`

**Fix 60-2: EditorCanvas.xaml — InlineTextEditor**
- Добавлен `behaviors:AutoFocusOnVisibleBehavior.IsEnabled="True"`
- Добавлен `LostFocus="InlineTextEditor_LostFocus"`

**Fix 60-3: EditorCanvas.xaml.cs — LostFocus→Commit**
- `InlineTextEditor_LostFocus`: если `IsEditing`, вызывает `CommitInlineEditingCommand`
- Безопасность: `Commit()` проверяет `InlineEditingText==null`

**Fix 60-4/5: CanvasInputRouter — guards**
- `RoutePreviewKeyDown` и `RouteKeyDown`: если `InlineEditManager.IsEditing` → `return`
- Escape/Enter при редактировании не уходят в инструменты

**Fix 60-6: ShortcutRegistry — guard**
- `TryHandle`: если `InlineEditManager.IsEditing` → `return false`
- V/L/R/T/E при редактировании не переключают инструменты

### Новые тесты
- `ShortcutRegistryTests.cs` — 7 тестов (V/L/R/T/E/ShiftE при IsEditing + положительный контроль)
- `AutoFocusOnVisibleBehaviorTests.cs` — 5 тестов (DP get/set + registration check)

### Common Mistakes (new)
66. `RouteKeyDown` must have the same `IsEditing` guard as `RoutePreviewKeyDown`. Without it, key events during inline editing reach the active tool and can clear selection, switch tools, or delete objects.
67. `ShortcutRegistry.TryHandle` must check `editor.InlineEditManager.IsEditing` before processing shortcuts. Without the guard, V/L/R/T/E hotkeys during inline editing switch tools or rotate objects instead of being handled by the TextBox.
68. WPF `LayoutTransform` offset on rotated elements ( WPF positions a `LayoutTransform`-ed element so the **top-left of the transformed bounding box** (not the local origin `(0,0)`) lands at the layout position. For `Text` with `RotateTransform(angle, 0, 0)`, this creates an offset `(-minX, +minY)` where `minX = min(0, W·cosθ, −H·sinθ, W·cosθ−H·sinθ)` and `minY = min(0, W·sinθ, H·cosθ, W·sinθ+H·cosθ)`. Model formulas (`RotatedCorner0-3`, `ContainsPoint`, `GetBoundingBox`) MUST apply this offset to match the visual position. At 0° the offset is (0,0) — no change. Позиции угловых маркеров текста (хит по маркерам — `MarkerLayout.GetPosition`, исторически `HitTestHelper.GetTextHandle`) должны читать `Text.RotatedCorner0-3` напрямую (не перевычислять углы), чтобы оставаться консистентными.
69. Inline editor TextBox must have ALL properties bound — проверь весь комплект: `Text`, `FontFamily` (конвертер), `FontSize` (`MicronsToPixelConverter + Zoom`), `AcceptsReturn="True"` (безусловно, НЕ привязан к `TextWrapping`!), `TextWrapping` (`BoolToTextWrappingConverter`), `TextAlignment` (`StringToTextAlignmentConverter`), `LayoutTransform` (`RotateTransform` для `RotationAngle`), `Visibility`, `Canvas.Left`/`Canvas.Top` (конвертеры), `AutoFocus` behavior, `LostFocus` → commit, `InputBindings` (`Ctrl+Enter`→commit, `Escape`→cancel на TextBox, НЕ на UserControl).
70. Guard conditions at ALL public entry points — defense-in-depth: guard-свойство (`IsEditable`, `IsEnabled`, `CanExecute`) проверяется не только на уровне Tool/View (первый consumer), но и на уровне Manager/Service. Один пропущенный entry point = баг. При `guard=false` не должно быть side effects.
71. InputBindings routing — Enter/Escape для inline editor должны быть на `TextBox.InputBindings`, НЕ на `UserControl.InputBindings`. UserControl.InputBindings перехватывают события ДО TextBox, даже если AcceptsReturn=True. Это вызывает конфликт: Enter (новая строка) не доходит до TextBox.


**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — Автоматизированный цикл разработки (18.07.2026)

Создан multi-agent pipeline для автоматизации полного цикла: Plan → Implement → Test → Review → Docs → Critic → PR.

### Архитектура

```
Conductor (primary) → делегирует subagent'ам через Task tool
├── planner     — read-only, пишет спеки
├── implementor — edit + bash, пишет код
├── tester      — edit + bash, тесты
├── reviewer    — read-only, код-ревью
├── critic      — read-only, финальный контроль
└── gh-ops      — bash, git/gh операции
```

### Команды
| Команда | Описание |
|---------|----------|
| `/pipeline full <desc>` | Полный цикл с Critic в конце |
| `/pipeline quick <desc>` | Быстрый цикл (без plan/docs/critic) |
| `/plan <desc>` | Только планирование |
| `/review` | Только ревью текущих изменений |

### Files
| Path | Назначение |
|------|-----------|
| `.opencode/agents/conductor.md` | Оркестратор (primary) |
| `.opencode/agents/planner.md` | Планирование |
| `.opencode/agents/implementor.md` | Реализация |
| `.opencode/agents/tester.md` | Тестирование |
| `.opencode/agents/reviewer.md` | Code review |
| `.opencode/agents/critic.md` | Финальный контроль |
| `.opencode/agents/gh-ops.md` | GitHub операции |
| `.opencode/skills/code-reviewer/SKILL.md` | Правила ревью |
| `.opencode/skills/documentation-writer/SKILL.md` | Правила документирования |
| `.opencode/skills/github-workflow/SKILL.md` | git/gh инструкции |
| `.opencode/commands/pipeline.md` | Команда полного pipeline |
| `.opencode/commands/pipeline-quick.md` | Команда быстрого pipeline |
| `.opencode/commands/plan.md` | Команда планирования |
| `.opencode/commands/review.md` | Команда ревью |
| `.github/workflows/opencode-pipeline.yml` | CI + OpenCode review |

## Pipeline — README encoding fix (18.07.2026)

### Fix README encoding
**Проблема:** README.md содержал UTF-8 double-encoding — русский текст и эмодзи отображались как mojibake (`рџ“‹ Рћ РџР РћР•РљРўР•` вместо `📋 О ПРОЕКТЕ`).
**Исправление:** 180 строк с mojibake декодированы через UTF-8 → CP1251 → UTF-8 селективно (строка за строкой). 220 правильно закодированных строк сохранены без изменений.
**Файл:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — README encoding fix v2 (18.07.2026)

### Fix README encoding (v2)
**Проблема:** README.md снова содержал UTF-8 double-encoding — русский текст и эмодзи отображались как mojibake.
**Исправление:** Селективное декодирование строк 14–401 через UTF-8 → CP1251 → UTF-8. Строки 1–13 не затронуты.
**Файл:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — CI/CD GitHub Actions (18.07.2026)

### Feature CI workflow
**Проблема:** Отсутствовал CI/CD — PR не проверялись автоматически, coverage не контролировался.
**Исправление:** Добавлен `.github/workflows/ci.yml` — build + test + coverage gate 75% на `windows-latest` с NuGet кэшированием.
**Файл:** `.github/workflows/ci.yml`
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
**Tests:** 2095 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint — Fix Session 2 bugs (22.07.2026)

### 6 исправлений по результатам ручного тестирования Session 2
**Bug 1: StatusBar info for text selection** — При выделении текста статус-бар показывает "Текст: {FontName}, {FontSizeMm}мм". При выделении линии — "Линия", прямоугольника — "Прямоугольник". При пустом выделении — "Готово".
**Файлы:** `EditorViewModel.cs`

**Bug 2: Enter in MultiLine** — `AcceptsReturn="True"` теперь безусловно (было привязано к TextWrapping). Enter всегда создаёт новую строку, Ctrl+Enter — commit.
**Файлы:** `EditorCanvas.xaml`

**Bug 3: Ctrl+Enter/Escape routing** — Удалены конфликтующие `UserControl.InputBindings` для Enter/Escape. Всё обрабатывается через `TextBox.InputBindings`.
**Файлы:** `EditorCanvas.xaml`

**Bug 4: TextAlignment in inline editor** — Добавлен `TextAlignment` биндинг на TextBox inline-редактора.
**Файлы:** `EditorCanvas.xaml`

**Bug 5: IsEditable=false guard** — Добавлена проверка `text.IsEditable` в `OnDoubleClick()` и `InlineEditManager.Start()` — defense-in-depth.
**Файлы:** `SelectTool.cs`, `InlineEditManager.cs`

**Bug 6: Font rendering** — Проверено: TTF-файлы с internal names `#GOST Type AU` / `#GOST Type BU` присутствуют. Конвертер корректен.

**Build:** 0 errors, 0 warnings
**Tests:** 2095 passed (0 failures, 1 pre-existing skip)
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
**Tests:** 2095 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint — Grid Refactoring (22.07.2026)

### Feature GR-1: GridHelper → IGridNodeGenerator (DI Singleton)
**Проблема:** `GridHelper` — статический класс с логикой `ComputeDisplayStep`/`GenerateGridNodes`. Нетестируем через DI, viewport-математика (`ViewportMargin = 1.5`) вызывала регенерацию узлов при каждом панорамировании.
**Решение:** Создан `IGridNodeGenerator`/`GridNodeGenerator` (DI Singleton). `GridNode` вынесен в top-level struct `Helpers/GridNode.cs`. `GridHelper.cs` и `GridHelperTests.cs` удалены (−21 viewport-тест, устарели). Создан `GridNodeGeneratorTests.cs` (+36 тестов).

### Feature GR-2: Абсолютная генерация узлов
**Проблема:** Узлы генерировались в viewport-координатах — каждый pan вызывал регенерацию + требовал `ViewportMargin = 1.5` для покрытия краёв. `_nodeBuffer` (shared mutable) и `RawNodeData`/`RawNodeCount` — небезопасное переиспользование буфера.
**Решение:** Узлы генерируются для всей площади листа в **абсолютных координатах** (0,0 = нижний левый угол). Панорамирование НЕ вызывает регенерацию — `RenderTransform` двигает сетку бесплатно (GPU). Удалены `ViewportMargin`, `_nodeBuffer`, `RawNodeData`/`RawNodeCount`. `GridManager.Nodes` — `IReadOnlyList<GridNode>`, каждый refresh аллоцирует новый список (нет shared mutable state).

### Feature GR-3: Defense-in-depth coarsen
**Проблема:** При превышении бюджета `MaxGridNodes` (A3+ с мелким шагом) сетка молча исчезала — `GenerateGridNodes` возвращал пустой список.
**Решение:** Двухуровневая защита: (1) `ComputeDisplayStep` уважает пользовательский шаг если он укладывается в бюджет и pixel-spacing; (2) генератор удваивает шаг (coarsen) пока не влезет в `MaxGridNodes` — никогда не возвращает пустой список из-за бюджета. Сетка не исчезает молча.

### Feature GR-4: Template → ObservableObject + GridManager IDisposable
**Проблема:** При смене формата листа (`Template.Sheet`) сетка не регенерировалась — `Template` не имел INPC.
**Решение:** `Template` переведён на `ObservableObject` (INPC на `Sheet`). `GridManager` подписывается на `Template.PropertyChanged` → регенерация при смене формата. `GridManager` реализует `IDisposable` (отписка).

### Feature GR-5: Настройки сетки (GridSettings + AppSettings + Settings UI)
**Решение:** Добавлены 3 новых поля:
- `MaxGridNodes` (int, default 250000) — бюджет узлов
- `NodeColor` (string?, null = авто по теме) — цвет узлов
- `NodeSize` (double, default 2.0) — размер узлов в px
SettingsView: 3 новых поля в секции СЕТКА (Макс. узлов TextBox, Цвет узлов с чекбоксом «Авто (по теме)», Размер узлов Slider 1-6).

### Feature GR-6: Темо-зависимый цвет узлов
**Решение:** `IThemeService.ThemeChanged` — новое событие. `EditorViewModel.IsDarkTheme` — проброс темы (подписка/отписка в Dispose). `GridNodesLayer.IsDarkTheme` DP → `UpdateThemeBrush`: Light #C0C0C0 / Dark #808080. `GridNodeColorConverter` — HEX → Brush, null/invalid → null (темо-зависимый fallback). `InverseBooleanConverter` — новый конвертер для Settings UI.

### Follow-up (РЕШЁН — Sprint "AppSettings → GridSettings chain", 10.08.2026)
- **Цепочка AppSettings → GridSettings** при создании редактора отсутствовала (`GridSettings.FromDefaultGrid` не читал AppSettings) — настройки сохранялись, но не применялись к открытым/новым вкладкам. **Решение:** см. секцию «Sprint — AppSettings → GridSettings chain (10.08.2026)» ниже: `GridSettings.FromAppSettings` + `EditorViewModelFactory.ResolveGridSettings()` применяют настройки сетки из Settings UI во всех путях создания вкладок.

### Тесты
- Удалён `GridHelperTests.cs` (−21 тест, viewport-кейсы устарели)
- Создан `GridNodeGeneratorTests.cs` (+36 тестов)
- Переписан `GridManagerTests.cs` (32 теста)
- Новые: конвертеры (+12), SettingsViewModel (+4), ThemeService ThemeChanged (+3), SettingsService round-trip (+3)
- Coverage: GridManager/GridNodeGenerator/GridNode/SettingsViewModel/ThemeService/GridNodeColorConverter/InverseBooleanConverter — 100%
- Release ×3 стабильно (flaky нет)

### Common Mistakes (new)
72. Grid nodes generation — always generate in ABSOLUTE sheet coordinates (0,0 = bottom-left), not viewport coordinates. RenderTransform handles pan offset for free (GPU). Viewport-based generation causes unnecessary regeneration on every pan + ViewportMargin complexity.
73. GridHelper — fully replaced by IGridNodeGenerator (DI Singleton). All grid logic (ComputeDisplayStep, GenerateGridNodes) is injectable. Never use static helper classes for testable domain logic.
74. Defense-in-depth for budget constraints — GenerateGridNodes never returns empty due to MaxGridNodes budget: coarsen (double step) until fits. First gate (ComputeDisplayStep) respects user intent; second gate (generator) guarantees grid never silently disappears.
75. Theme-aware resources — use "auto" (null) as default that follows theme (Light/Dark), and explicit user choice as permanent override. GridNodesLayer: NodeColor null → theme brush (#C0C0C0 Light / #808080 Dark); explicit HEX → always user color.
76. Tool identity — enum `ToolKind`, NEVER strings. Строковые карты инструментов запрещены: исторически их было 4 (ToolManager×2, switch роутера, XAML), они расходились («Pan» недостижим) и давали silent-default в роутере. Новый инструмент: значение `ToolKind` (если переключаемый) + запись в `KindToToolType` + фабрика в `ToolRegistry` (+ пункт XAML с `{x:Static tools:ToolKind.X}` и, при необходимости, клавиша в `ShortcutRegistry`). Пан — исключение: это жест роутера, а не инструмент (`PanTool` удалён, см. `docs/adr/0001-pan-gesture-not-tool.md`); новый инструмент для панорамирования не добавлять. Переключение — `ActivateTool(kind)`/`SwitchTo(kind)`; XAML-биндинги — к `ToolRegistry.ActiveToolKind` (OneWay).
77. App settings — only through the typed POCO: `ISettingsService` выставляет только `Load()` + `Save(AppSettings)`; строковые ключи и диспетчеры по ключам запрещены (исторически 12 ключей дублировались литералами в двух switch-блоках и 19 вызовах, породив дефекты: сломанная типовая проверка, двойная запись файла, culture-зависимый round-trip). Новая настройка: свойство в `AppSettings` (Models) + UI в SettingsViewModel — без строковых ключей. Паттерн записи — load-mutate-save в одной точке; `Load()` возвращает кэшированный экземпляр, поэтому мутации видны всем потребителям ещё до `Save()` (семантика осознанная, статус-кво).
78. STA-тесты панорамирования: с ПОКАЗАННЫМ окном `e.GetPosition(window)` возвращает реальную позицию курсора — дельта становится недетерминированной. Тесты дельты/цикла пана — с непоказанным окном (`GetPosition` = 0,0 детерминированно); `CaptureMouse` требует подключённый PresentationSource, поэтому capture-assert'ы — в отдельном тесте с показанным окном и без assert'ов позиций. Триггер жеста (`Keyboard.IsKeyDown`) не фейчится — детекция тестируется через чистый предикат (`IsPanGesture`), цикл — через `RoutePanDown` напрямую.

**Build:** 0 errors, 0 warnings
**Tests:** 2140 total, 2139 passed (0 failures, 1 pre-existing skip)
**Coverage:** 76.3% line-rate ✅

## Sprint — AppSettings → GridSettings chain (10.08.2026)

### Feature AGS-1: GridSettings.FromAppSettings static factory
**Проблема:** Настройки сетки из Settings UI (ShowGrid/SnapToGrid/GridStepMm/GridMaxNodes/GridNodeColor/GridNodeSize) сохранялись в `AppSettings`, но при создании EditorViewModel использовался только `GridSettings.FromDefaultGrid` — цепочка «настройки → вкладки» была разорвана (follow-up из Grid Refactoring).
**Решение:** Добавлена static factory `GridSettings.FromAppSettings(AppSettings)` — маппинг 6 полей:
- `ShowGrid` → `Enabled`
- `SnapToGrid` → `SnapEnabled`
- `GridStepMm` → `StepMicrons`
- `GridMaxNodes` → `MaxGridNodes`
- `GridNodeColor` → `NodeColor`
- `GridNodeSize` → `NodeSize`
Плюс clamping: `StepMicrons` ≥ 1 мкм, `MaxGridNodes` ≥ 1, `NodeSize` не NaN/∞/≤0. `GridSettings` помечен `sealed`. Добавлена константа `EditorSettings.DefaultGridNodeSize = 2.0`.

### Feature AGS-2: EditorViewModelFactory.ResolveGridSettings()
**Проблема:** Фабрика редактора не имела доступа к настройкам приложения — каждый путь создания вкладки создавал GridSettings из дефолтов.
**Решение:** `EditorViewModelFactory` получил опциональный `ISettingsService?` в ctor. Новый метод `ResolveGridSettings()` — каскад разрешения:
1. explicit `gridSettings` (переданный параметр) — приоритет
2. `AppSettings` через `ISettingsService` (если доступен) → `GridSettings.FromAppSettings`
3. fallback `GridSettings.FromDefaultGrid()`
Применяется во всех путях создания вкладок: `Create`, `CreateWithFilePath` (EditorViewModelFactory), `TabOperationsService`: `CreateNewTab`, `OpenFileAsync`, `OpenFromFilePath`, `CreateNewCustomTab`.

### Тесты
- `GridSettingsTests`: 9 тестов `FromAppSettings` (маппинг всех 6 полей + clamping: StepMicrons ≥ 1 мкм, MaxGridNodes ≥ 1, NodeSize NaN/∞/≤0 → default)
- `EditorViewModelFactoryTests`: 6 тестов `ResolveGridSettings` (explicit > AppSettings > FromDefaultGrid, null ISettingsService)
- 3 теста по ревью
- Итого: +18 тестов

**Файлы:** `Models/GridSettings.cs`, `ViewModels/EditorViewModelFactory.cs`, `ViewModels/IEditorViewModelFactory.cs`, `Constants/EditorSettings.cs`, `Services/TabOperationsService.cs`, `ViewModels/MainViewModel.cs` (производственные); `Tests/Models/GridSettingsTests.cs`, `Tests/ViewModels/EditorViewModelFactoryTests.cs` (тесты)

**Build:** 0 errors, 0 warnings
**Tests:** 2160 total, 2159 passed (0 failures, 1 pre-existing skip)
**Coverage:** 76.4% line-rate ✅

## Sprint — Tech debt + coverage (11.08.2026)

### Feature TD-1: Text markers tech debt закрыт (regression-тест)
**Проблема:** После Sprint 61 (LayoutTransform offset) маркеры выделения Text могли смещаться от реальных углов повёрнутого текста — отсутствовал regression-тест, подтверждающий, что RotatedCorner0-3 лежат на границе `GetBoundingBox()`.
**Решение:** Добавлен theory-тест `RotatedCorners_AllLieOnBoundingBoxEdges` (6 углов: 0/45/90/135/180/270°) в `TextTests.cs` — для каждого угла все 4 маркера (RotatedCorner0X/Y–3X/Y) лежат на границе `GetBoundingBox()`. Подтверждено в коде: `TextSelectionMarkerBehavior` не существует, пустой `<Canvas/>` внутри DataTemplate Text отсутствует, маркеры Text рендерятся в ItemsControl через `MarkerPosition.XPropertyPath/YPropertyPath="RotatedCornerNX/Y"` (EditorCanvas.xaml).
**Файлы:** `Tests/Models/Objects/TextTests.cs`

### Feature TD-2: Inline edit guards + тесты
**Решение:** Guards InlineEditManager защищены тестами:
- +5 тестов: `Start_NonEditable_NoOp`, `Commit_UnchangedText_NoCommand`, `Commit_Twice_PushesSingleCommand`, `Cancel_WhenNotEditing_NoThrow`, `Start_WhileEditing_SwitchesObject` (ManagerTests.cs:596–645)
- +2 STA-теста AutoFocusOnVisibleBehavior: `VisibleTextBox_BecomesFocused` (реальный фокус через WPF window + retry-Activate при flaky IsFocused в headless CI), `VisibleTextBox_SelectsAllText`
**Файлы:** `Tests/ViewModels/Managers/ManagerTests.cs`, `Tests/Behaviors/AutoFocusOnVisibleBehaviorTests.cs`

### Feature TD-3: WPF-обёртки — internal static handlers + 27 тестов
**Проблема:** Тестируемая логика WPF-обёрток была `private` — покрытие обёрток (WpfMessageBoxProvider, WpfDispatcherService, WpfDialogFileService, WpfDialogHostService, ThemeDictionaryManager, PrintDialogFactory) оставалось пробелом (docs/19 «Следующие шаги»).
**Решение:** Логика изолирована в `internal static` в 5 production-файлах:
- `WpfMessageBoxProvider`: `ToWpfButtons`, `ToWpfIcon`, `ToMsgrResult`
- `WpfDispatcherService`: ctor с `Dispatcher? dispatcher = null`
- `WpfDialogFileService`: `CreateOpenDialog`, `CreateSaveDialog`
- `WpfDialogHostService`: `ResolveWindowDescriptor`
- `ThemeDictionaryManager`: `FindThemeDictionary`
+27 новых тестов: WpfMessageBoxProviderTests (11), WpfDispatcherServiceTests (3 STA), WpfDialogFileServiceTests (6 STA), WpfDialogHostServiceTests (2), ThemeDictionaryManagerTests (4 STA), PrintDialogFactoryTests (1 STA).
**Файлы:** production: `Services/WpfMessageBoxProvider.cs`, `Services/WpfDispatcherService.cs`, `Services/WpfDialogFileService.cs`, `Services/WpfDialogHostService.cs`, `Services/ThemeDictionaryManager.cs`; tests: `Tests/Services/Wpf*Tests.cs`, `ThemeDictionaryManagerTests.cs`, `PrintDialogFactoryTests.cs`

### Feature TD-4: Coverage 76.4% → 80.22%
**Решение:** Покрытие повышено с 76.4% до **80.22% line-rate** (5473/6822, +3.82 п.п., ≥80% gate достигнут). Journey: 76.4% (baseline) → 77.88% (implementor, ~76 тестов) → 80.22% (tester, +65 тестов). Зоны tester: ConverterTests (+13: IsObjectSelectedConverter, FontNameToFamilyConverter, LineLocalConverter/RelativeMicronsToPixelConverter ConvertBack), AutosaveServiceTests (+5), SettingsServiceTests (+6), PropertiesViewModelTests (+41: Text 18 / Rectangle 12 / Line 11, команды через CommandHistory + INPC-forwarding + Dispose). Исправлены 2 неверных теста implementor (порядок чтения PrintableAreaWidth, SetupSequence SettingsViewModel).
**Файлы:** `Tests/Converters/ConverterTests.cs`, `Tests/Services/AutosaveServiceTests.cs`, `Tests/Services/SettingsServiceTests.cs`, `Tests/ViewModels/PropertiesViewModelTests.cs`, `Tests/Services/PrintServiceTests.cs`, `Tests/ViewModels/SettingsViewModelTests.cs`

### Feature TD-5: 6 MINOR-замечаний ревью закрыты (11.08.2026)
- Удалены misleading-тесты: AutoFocus fake (`IsEnabledTrue_RegistersIsVisibleChanged`), `WpfApplicationLifecycleTests.cs` (placeholder, git rm), 4 тавтологичных `FitToPageScale_*` + пустой регион Scaling Math (реальное поведение покрыто `PrintServiceStaTests.PrintWithVisual_FitToPage_FrameworkElement_AppliesScale`)
- `CustomResizeCommandTests.cs` → `ChangePropertyCommandResizeTests.cs` (git mv + rename класса, stale имя)
- `ThemeDictionaryManagerTests` — `[Collection("ThemeDictionaryManager")]` + `ThemeDictionaryManagerTestCollection.cs` (DisableParallelization, паттерн AutosaveTestCollection)
- AutoFocus retry-Activate — смягчение flaky `IsFocused` в headless CI (повторный `window.Activate()` + pump один раз)

**Build:** 0 errors, 0 warnings
**Tests:** 2295 total, 2294 passed (0 failures, 1 pre-existing skip)
**Coverage:** 80.22% line-rate ✅

## Sprint — Coverage series + Docs/CI (11–13.08.2026)

### Feature CS-1: Input routing chain coverage (11.08.2026)
**Решение:** Минимальный рефакторинг `CanvasInputRouter.cs` (private static → internal static: `GetCurrentTool`, `RoutePanDown`, `ApplyPan`, `ToWpfCursor`) + 69 новых тестов: CanvasInputRouterTests (35), CoordinateTransformTests (7), EditorCanvasStateTests (14), EditorCanvasBehaviorTests (+13 STA). Покрытие: CanvasInputRouter 97.2%, CoordinateTransform/EditorCanvasState/EditorCanvasBehavior 100%.
**Файлы:** `Behaviors/CanvasInputRouter.cs`, `Tests/Behaviors/*`

### Feature CS-2: TabOperationsService coverage (12.08.2026)
**Решение:** `TabOperationsService` покрыт на 100% (было 24%, +51 тест).
**Файлы:** `Tests/Services/TabOperationsServiceTests.cs`

### Feature CS-3: MainViewModel async flows coverage (12.08.2026)
**Решение:** Async-потоки `MainViewModel` (autosave, async-void, print, tab ops) покрыты до 97.1% (+35 тестов).
**Файлы:** `Tests/ViewModels/MainViewModelAsyncTests.cs`

### Feature CS-4: TemplateLibraryService/ViewModel coverage (12.08.2026)
**Решение:** TemplateLibraryService/ViewModel покрыты 35%/51% → 100% (+41 тест).
**Файлы:** `Tests/ViewModels/TemplateLibrary*Tests.cs`, `Tests/Services/TemplateLibraryServiceTests.cs`

### Feature CS-5: Dialog wrappers coverage (12.08.2026)
**Решение:** WpfDialogHostService 100%, WpfDispatcherService 92.3%, PrintDialogWrapper 68.2% (+18 тестов).
**Файлы:** `Tests/Services/WpfDialogHostServiceTests.cs`, `WpfDispatcherServiceTests.cs`, `PrintDialogWrapperTests.cs`

### Feature CS-6: Docs + CI gate 80% (13.08.2026)
**Решение:** Coverage gate поднят 75% → 80% в `.github/workflows/ci.yml` + `opencode-pipeline.yml`; CHANGELOG.md: UTF-8 BOM удалён (контент уже чистый); метрики синхронизированы во всех источниках (README, AGENTS.md, CONTRIBUTING.md, docs/00, docs/19, CODING_STANDARDS, agents, skills, .coverage-baseline.txt).

**Build:** 0 errors, 0 warnings
**Tests:** 2515 total, 2514 passed (0 failures, 1 pre-existing skip)
**Coverage:** 88.45% line-rate ✅ (gate 80% достигнут)

## Sprint — Coverage weak zones (14.08.2026)

### Feature WZ-1: ResizeMathTests.cs (новый файл, 78 кейсов)
**Решение:** Новый тестовый файл `Tests/Tools/ResizeMathTests.cs` — 78 unit-кейсов / 46 тест-методов (100% line coverage `Tools/ResizeMath.cs`, итоговая zone line-rate 97.35%): ComputeRectangleResize (21: все 8 хендлов + Ctrl от центра + Shift aspect-ratio на диагоналях + snap + min-size clamp + clamp к листу), ComputeTextResize (13: non-corner move, height/width-based, corner shift, Ctrl scale sign, проекция дельт при повороте 90°/45°, snap, min font clamp), ComputeLineEndpoint (6: snap/clamp/non-endpoint), CursorForHandle (14), VisualCursorForHandle (24: ротация диагональных курсоров на 90°/270°/±углы, прямые углы, edge-хендлы).
**Файлы:** `Tests/Tools/ResizeMathTests.cs`

### Feature WZ-2: Остальные зоны — PanTool, FontMetrics, Converters, TemplateValidator
**Решение:** PanToolTests +8 → 100% (66/66), FontMetricsTests +15 → 91.11% (было ~40%, рефакторинг тестируемости), ConverterTests +2 → IsNullConverter/NotNullToVisibilityConverter 100%, ValidationServiceTests +24 → TemplateValidator 98.95% (было 65–93%).
**Файлы:** `Tests/Tools/PanToolTests.cs`, `Tests/Models/FontMetricsTests.cs`, `Tests/Converters/ConverterTests.cs`, `Tests/Helpers/ValidationServiceTests.cs`

### Fix WZ-3: TemplateValidator null-sheet guard (фикс NRE)
**Проблема:** `TemplateValidator.Validate()` при `template.Sheet == null` кидал NullReferenceException (через ValidateSheetFormat/ValidateCoordinates/ValidateObject); при нескольких объектах дублировал ошибку (N+1 вместо 1).
**Исправление:** Ранний return V-006 при `Sheet == null` ДО вызова остальных валидаторов; guard в `ValidateObjectCoordinates()` (sheet==null → V-006 + yield break) защищает `ValidateObject` path. Regression-тесты: `Validate_SheetNullWithObjects_NoThrow_ReturnsV006` (усилен `Assert.Single`), `ValidateObject_NullSheet_ReturnsV006_NoThrow`.

### Feature WZ-4: FontMetrics рефакторинг тестируемости
**Решение:** `ComputeAverageAdvanceWidth` → `internal static` (чистое вычисление по `IDictionary<int,ushort>`/`IDictionary<ushort,double>`), sampleChars (A-Z/a-z/А-Я/а-я) → `private static readonly SampleChars`, fallback-присваивания → `ApplyFallback()`, catch-логика → `HandleFallbackWithLog()`. Поведение не изменено, публичные сигнатуры не тронуты (2-й production-change спринта, оправдан тестируемостью).

### Review findings (3× MINOR закрыты)
1. TemplateValidator: ранний return V-006 в `Validate()` (N+1→1), guard в `ValidateObjectCoordinates` оставлен как defense-in-depth, тест усилен `Assert.Single(errors, e => e.RuleId == "V-006")`.
2. ValidationServiceTests.cs:398 — комментарий A4 Portrait исправлен (300мм > 210мм, было вводящее в заблуждение).
3. ResizeMathTests.cs:402 — mojibake `90В°` → `90°`.

**Build:** 0 errors, 0 warnings
**Tests:** 2636 total, 2635 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.18% line-rate ✅ (gate 80% достигнут)

## Sprint — Глубокая база панелей свойств (#45–#48, 16–17.08.2026)

### Проблема
Три sub-VM панелей свойств (`LinePropertiesViewModel`, `RectanglePropertiesViewModel`, `TextPropertiesViewModel`) дублировали одну и ту же инфраструктуру: конструктор-тройку зависимостей, поля `_commandHistory`/`_markDirty`/`_setValidationError`, `UpdateObject` (отписка → присвоение → подписка → notify-all), `Dispose` с отпиской, `SetProperty<T>` (валидация → undo-команда → уведомление), `ChangeFromMmString`, switch-диспетчеризацию `PropertyChanged` модели по сырым строкам.

### Feature PB-1: ObjectPropertiesViewModel<TObject> (#46)
**Решение:** Новая абстрактная generic-база `ViewModels/ObjectPropertiesViewModel.cs` (127 строк, `where TObject : TemplateObjectBase`):
- конструктор-тройка (`CommandHistory?`, `Action? markDirty`, `Action<string?> setValidationError`) — private-поля, наружу только через protected-механику;
- `protected abstract IReadOnlyDictionary<string, string> PropertyMap` — декларативная карта «свойство модели → свойство VM», только `nameof`; одна карта обслуживает и диспетчеризацию `PropertyChanged` модели (`OnModelPropertyChanged`), и notify-all в `UpdateObject`;
- `UpdateObject(TObject?)` — отписка от старого объекта, присвоение, подписка, notify-all по `PropertyMap.Values`;
- `Dispose()` — отписка + обнуление `CurrentObject`;
- `SetProperty<T>(value, getter, setter, validator, propertyName, commandName, afterSet)` — валидация (reject → `_setValidationError`, без команды и без уведомления) → `ChangePropertyCommand<T>` → `CommandHistory.Push` → `OnPropertyChanged` → `afterSet`;
- `ChangeFromMmString` (InvariantCulture-парсинг мм → микроны) и `ParseLineType` (русские имена → `LineType`, неизвестные → Solid) — protected, переиспользуются наследниками.

### Feature PB-2: Миграция Line (#46), Rectangle (#47), Text (#48)
**Решение:** Каждый sub-VM — тонкий наследник: static readonly карта + `override PropertyMap` + именованные свойства-делегаты к `CurrentObject?` + `[RelayCommand]`-команды.
- **Line:** карта 7 пар (`StartMicronsX`→`StartX` … `StrokeColor`), 13 RelayCommand (7 типизированных + 6 строковых).
- **Rectangle:** карта 8 пар (+`FillColor`), 14 RelayCommand; afterSet-хуки `Width`→notify `X` и `Height`→notify `Y` дословно перенесены.
- **Text:** карта 13 пар (сырые строки `"MicronsX"`/`"MicronsY"` заменены на `nameof`), 18 RelayCommand. Три особые команды с null-coalescing сохранены в sub-VM и не обобщены в базу: `ChangeContent` (валидация `value ?? ""` эквивалентна — `ValidateTextContent` на `IsNullOrWhiteSpace`; setter с `?? string.Empty` сохранён), `ChangeDefaultValue` (null → `""`), `ChangeFontNameFromString` (whitespace-guard до команды) — выражены через base `SetProperty<string?>` с coalescing на стороне вызова, поведение неотличимо.

### Итоги
- Инфраструктурное дублирование трёх sub-VM устранено полностью (−71/−72/−76 строк в sub-VM: Line 190→119, Rect 204→132, Text 275→199; вся механика — в базе 127 строк).
- Внешняя поверхность байт-в-байт: XAML (`PropertiesPanelContent.xaml`), `PropertiesViewModel`-держатель, code-behind (`OnTextIsEditableClick` → `ChangeIsEditableCommand`) не изменены; имена свойств и команд сохранены.
- +13 тестов механики карты в `PropertiesViewModelTests.cs` (диспетчеризация по всем свойствам карты, notify-all, отписка при смене объекта и в Dispose, validation-reject, null-coalescing особых команд Text); тесты через существующий шов `PropertiesViewModel`→sub-VM, реальные `CommandHistory`/модели, без моков.
- Документация (#49): секция в AGENTS.md, устаревшие упоминания `CustomResizeCommand`/`ComputeLineResize` почищены в current-state секциях (исторические записи сохранены, запись CHANGELOG аннотирована), метрики синхронизированы (README, CONTRIBUTING, docs/00, docs/19, CODING_STANDARDS, CHANGELOG [Unreleased], .coverage-baseline.txt, DOCS_MANIFEST).

**Build:** 0 errors, 0 warnings
**Tests:** 2649 total, 2648 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.99% line-rate ✅ (gate 80% достигнут)

## Sprint — ToolRegistry: единая типизированная идентичность инструментов (#53–#57, 17.08.2026)

### Проблема
Идентичность инструментов — строки в 4 несинхронизированных картах: ToolManager (`ToolNameMap` + параллельная `ToolFactories`), switch роутера ввода (с silent-default на SelectTool), `ShortcutRegistry` (Key→string), XAML (`CommandParameter` + `ConverterParameter` ×2 на кнопку тулбара). Наборы расходились: «Pan» присутствовал только в `ToolNameMap` и был недостижим (в роутере нет case), «Resize» отсутствовал в шорткатах/XAML. Добавление инструмента = синхронная правка 6+ мест; компилятор строковые опечатки не ловил. Инструменты переключали себя через WPF `ICommand` (`SetActiveToolCommand.Execute("Select")`) — WPF-тип в шве инструментов. `ITool.Name` объявлялся в 6 инструментах при 0 потребителях.

### Решение — expand → migrate → contract (wide refactor: атомарный flip невозжен без временного дублирования)
- **#54 Expand** (PR #58): enum `ToolKind { Select, Line, Rectangle, Text, Resize }` в Tools; Пан не входит — type-адресуется через `GetOrCreateTool<PanTool>()`. Типизированная поверхность в ToolManager рядом со строковой: `ActiveToolKind` (канон), `ActiveToolInstance`, `SwitchTo(kind)` с reset'ом предыдущего, типизированный стек; строковые шимы делегируют через parse (неизвестные значения игнорируются).
- **#55 Migrate UI** (PR #59): relay-команда принимает идентичность; XAML — 8× `{x:Static tools:ToolKind.X}` (ещё 4 `ConverterParameter` типизированы фиксом contract-фазы), IsChecked RadioButtons — `Mode=OneWay` к `ActiveToolKind` (конвертер равенства не менялся); карта горячих клавиш Key→ToolKind создана в самом `ShortcutRegistry` (Helpers) — CODING_STANDARDS §3.1 запрещает WPF-типы (`Key`) в ViewModels, поэтому в реестр она не вошла. Побочный эффект OneWay: прежний TwoWay записывал строку до выполнения команды, из-за чего reset+очистка preview на кликах тулбара не выполнялись; теперь команда — единственный путь, поведение тулбара сравнялось с меню.
- **#56 Migrate инструменты/контекст/роутер** (PR #60): `IEditorContext` — `PushTool(ToolKind)`/`PopTool()`/новый `ActivateTool(ToolKind)`, член WPF `ICommand` из интерфейса удалён; роутер — `ActiveToolInstance` из реестра вместо строкового switch с silent-default (неизвестная идентичность непредставима).
- **#57 Contract** (PR #61): ToolManager → **ToolRegistry** (класс, файл, свойство EditorViewModel, пути биндингов XAML); строковая поверхность удалена целиком (`ActiveTool`-string, `PushTool(string)`, `ResetTool(string)`, `ToolNameMap`, parse-шимы); `ITool.Name` удалён из интерфейса и 6 реализаций; ToolManagerTests → ToolRegistryTests (in-place); `ShortcutRegistry` вызывает `ActivateTool` напрямую.

### Итоги
- 4 строковые карты и silent-default удалены; идентичность проверяется компилятором; поведение байт-в-байт (каждая правка — типизированная замена того же вызова).
- Удалены только тесты несуществующего больше поведения (строковые шимы ×8, fallback роутера, `ITool.Name` ×6); сохранённое поведение re-covered в ToolRegistryTests.
- CODING_STANDARDS синхронизирован: ToolRegistry в таблице менеджеров (§8.2) + санкционированное исключение Manager-suffix (§12.1 — реестр идентичности/фабрик, а не владелец UI-состояния).
- Примечание для будущих работ: `ActiveToolInstance` создаёт/кэширует инструмент при первом чтении (семантика GetOrCreateTool, как в роутере до рефакторинга).

**Build:** 0 errors, 0 warnings
**Tests:** 2646 total, 2645 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.98% line-rate ✅ (gate 80% достигнут)

## Sprint — AppSettings: типизированный интерфейс настроек (#65–#66, 17.08.2026)

### Проблема
Кандидат 4 архитектурного обзора №3. Настройки приложения жили в типизированном POCO, но сервис параллельно выставлял строковый API `Get<T>(key, default)`/`Set<T>(key, value)`: 12 ключей-литералов, продублированных в двух switch-диспетчерах (Get и Set) и в 19 вызовах пяти классов. Опечатку в ключе компилятор не ловил. Живые дефекты: (a) кейс `LastUsedSheetOrientation` в Get-диспетчере проверял рантайм-тип `defaultValue` вместо generic-параметра; (b) создание вкладки делало два `Set` подряд — каждый с полной сериализацией и записью файла; (c) fallback в `CustomSettings` сериализовал через `ToString()` и парсил `Convert.ChangeType` без формат-провайдера (culture-зависимость, null → "" необратимо). Плюс инверсия слоёв: `GridSettings` (Models) зависел от типа из Services.

### Решение — атомарный flip (один PR #68 по спеке #65)
- `ISettingsService` сужен до `Load()` + `Save(AppSettings)`; строковые `Get<T>`/`Set<T>` удалены из интерфейса и реализации, оба switch-диспетчера (12+12) удалены целиком.
- 19 вызовов мигрированы: SettingsViewModel (10 Get → один `Load()`), MainViewModel (4 Get → `Load().LastUsed*`), ThemeService (Load в ctor + load-mutate-save в `SetTheme`), AutosaveService (`Load().AutosaveIntervalMinutes`, читается один раз при старте), TabOperationsService (2 Set → одна load-mutate-save).
- `AppSettings` перенесён Services → Models (`git mv`, чистый POCO); `using DotElectric.TemplateEditor.Services` из GridSettings удалён — инверсия слоёв устранена; `FromAppSettings` + clamping остались в Models.
- `CustomSettings` удалён из POCO (мёртвый escape-hatch: production в него не писал); legacy-файлы с этим ключом читаются — неизвестные JSON-ключи игнорируются (regression-тест `Load_LegacyFileWithCustomSettingsKey_IgnoresUnknownKey`).
- Семантика Load/Save не изменена: кэшированный экземпляр, повреждённый JSON → warning + дефолты, `ArgumentNullException` на null, опции JSON (WriteIndented + relaxed-экранирование), путь `%APPDATA%\DotElectric\settings.json`; схема файла байт-совместима.
- Два integration-теста больше не пишут в реальный пользовательский файл: один переведён на temp-путь, второй удалён вместе со строковым API.

### Итоги
- Три дефекта исчезли структурно: (a) — вместе с Get-диспетчером; (b) — одна запись файла на создание вкладки (`Verify(Save, Times.Once)`); (c) — вместе со строковым fallback'ом и `CustomSettings`.
- 28 тестов умирающего поведения удалены (27 в SettingsServiceTests — весь строковой API — + 1 integration-тест Get/Set); типизированные round-trip/кэш/corrupt-json сохранены; +1 новый тест legacy-совместимости; моки 6 тестовых файлов переведены на Load/Save.
- Покрытие затронутых классов: SettingsViewModel/AppSettings/GridSettings — 100%, SettingsService — 81% (не покрыта только ветка реального %APPDATA%-пути — тесты не должны трогать пользовательские настройки).
- Документация (#67): секция в AGENTS.md, метрики синхронизированы (README, CONTRIBUTING, docs/00, docs/19, CHANGELOG [Unreleased], .coverage-baseline.txt, DOCS_MANIFEST).
- Кандидат 4 архитектурного обзора №3 закрыт.

**Build:** 0 errors, 0 warnings
**Tests:** 2619 total, 2618 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.91% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 7: deletion-пара FitToScreen + SelectionBoxHelper (#70–#71, PR #73, 17.08.2026)

### Проблема
Кандидат 7 архитектурного обзора №3 — «быстрая пара» deletion-тестов. (a) «Вписать в экран» всегда считал для 800×600: строковый параметр команды с fallback, Ctrl+0 с хардкодом `CommandParameter="800,600"`, меню и тулбар без параметра — живой viewport ZoomPanManager не использовал ни один из трёх входов (живой пользовательский дефект). (b) Рамка выделения вычисляла границы Line/Rectangle частными копиями при полиморфном `GetBoundingBox()` в модели (копия для Text уже делегировала модели).

### Решение — TDD на существующих швах (новых ноль)
- **FitToScreen:** `ZoomPanManager.FitToScreen()` без аргументов — читает собственный живой viewport; guard `viewport ≤ 0` → no-op (без изменения масштаба, центрирования и исключений); overload `(double,double)` удалён. Команда `EditorViewModel` без параметра: делегирование + безусловное «Вписано в экран»; строковый парсинг и fallback 800×600 удалены целиком. XAML: все три входа без параметра. Формула не изменена (0.95, min по осям, cap ZoomMax, центрирование; bypass ZoomMin сохранён).
- **Рамка выделения:** циклы `GetFullyContained`/`GetIntersecting` вызывают `obj.GetBoundingBox()` напрямую; `GetLineBounds` (дословная копия `Line.GetBoundingBox()`), `GetRectangleBounds` (эквивалент `Rectangle.GetBoundingBox()`) и мёртвый `_ =>` default-кейс удалены (abstract-метод гарантирует реализацию компилятором для будущих типов). Публичный API и семантика LTR/RTL не изменены.
- **Тесты:** 6 параметризованных тестов мигрированы на `SetViewportSize(w,h)` + команда без параметра; +3 новых: regression «живой viewport ≠ 800×600» (1000×500 → zoom 1.5993; fallback дал бы 1.8095), no-op нулевого viewport в менеджере и no-op через командный шов с безусловным статус-сообщением. Тесты рамки выделения без изменений — поведение байт-в-байт.
- **Глоссарий:** термины «Вписать в экран» и «Рамка выделения» внесены в CONTEXT.md.

### Итоги
- Две поверхности удалены целиком: строковый парсинг viewport и дубли геометрических формул исчезли как класс ошибок; единственное изменение поведения — исправленный дефект вписывания.
- Двухосевое code-review: Standards — 0 hard violations (дифф структурно устранил Primitive Obsession, Repeated Switches, Feature Envy), Spec — полное соответствие; MINOR spec-оси (no-op через командный шов) и judgment call standards-оси (инлайн Middle Man-делегата) закрыты коммитами.

**Build:** 0 errors, 0 warnings
**Tests:** 2622 total, 2621 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.86% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 1, срез 1: рамка выделения через узкий шов (#75–#76, PR #78, 17.08.2026)

### Проблема
Кандидат 1 архитектурного обзора №3 — фасад EditorViewModel / IEditorContext (35 членов) → узкие role-seams. Первый срез — рамка выделения: 7 членов интерфейса (`SelectionBoxLeft/Bottom/Top/Width/Height/Right`, `SelectionDirection`) выставляли состояние PreviewManager поштучно — инструмент ставил рамку пятью записями, блок обнуления 5 свойств повторён в трёх местах; `SelectionBoxTop`/`SelectionBoxRight` — мёртвые read-only члены (0 потребителей); EditorViewModel держал 7 чистых forwarding-свойств; глубокие методы `PreviewManager.SetSelectionBox`/`ClearSelectionBox` существовали покрытые тестами при 0 потребителей в production.

### Решение — существующие швы (новых ноль)
- **IEditorContext:** 7 членов рамки удалены → 2 метода `SetSelectionBox(long left, long bottom, long width, long height, SelectionDirection direction)` + `ClearSelectionBox()`; интерфейс сжат 35 → 30 членов.
- **EditorViewModel:** 7 forwarding-свойств удалены целиком (XAML их не использовал — все 6 биндингов рамки уже на `PreviewManager.SelectionBox*`); 2 метода — голая делегация.
- **SelectTool:** 4 блока заменены — 5 записей в OnMouseMove → один `SetSelectionBox(...)` (направление через `SelectionBoxHelper.GetDirection` — дубль формулы устранён по замечанию code-review), три обнуления (OnMouseDown по пустому месту, OnMouseUp, Reset) → `ClearSelectionBox()`; байт-эквивалентно — все блоки уже сбрасывали direction в LTR.
- **PreviewManager и XAML: 0 изменений** — глубокие методы получили первого production-потребителя.
- **Тесты:** ~27 assert'ов мигрированы с VM-свойств на `editor.PreviewManager.*` (+ rename `SelectionDirection` → `SelectionBoxDirection`); 2 INPC-теста перенесены из EditorViewModelTests в ManagerTests; новых тестов ноль — мигрированные SelectionBoxTests/ToolTests через реальный EditorViewModel фиксируют шов end-to-end (паттерн кандидата 7).
- **Поведение байт-в-байт:** threshold 3 мм, LTR/RTL-семантика, порядок INPC (Left, Bottom, Width, Height, Direction).

### Итоги
- Состояние рамки передаётся целиком, а не собирается по свойству; мёртвые члены интерфейса и forwarding-прослойка исчезли; следующий срез кандидата 1 — preview-тройка (отдельная спека).
- Двухосевое code-review: Standards — 0 hard violations; judgment calls закрыты коммитами (дубль формулы направления → `GetDirection`; INPC-тесты перенесены в ManagerTests); Data Clumps (RectMicrons в сигнатуре) отклонён — сигнатура зафиксирована спекой, PreviewManager не изменяется. Spec — полное соответствие, 1 MINOR (размещение INPC-тестов) закрыт.
- Примечание docs-фазы: §8.2 CODING_STANDARDS (таблица менеджеров) приписывал SelectionBox SelectionManager'у — фактическое владение в PreviewManager, таблица исправлена.

**Build:** 0 errors, 0 warnings
**Tests:** 2622 total, 2621 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.85% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 7 обзора №4: пан — жест роутера (#84–#85, PR #85, 17–18.08.2026)

### Проблема
Кандидат 7 архитектурного обзора №4. Панорамирование владело двумя источниками истины: живая математика в `CanvasInputRouter` (`RoutePanDown`/`ApplyPan` — дельта в Window-координатах, capture, RefreshGridNodes по концу) и мёртвый `PanTool` (`OnMouseMove` на production-пути не вызывался никогда) с собственным `_isPanning`, который расходился с `state.IsPanning` на живом сценарии Space+Left без Alt (guard PanTool требовал Alt, роутер — нет). `PanTool` — единственный носитель `using System.Windows` в слое инструментов; `IEditorContext.PanCanvas` имел единственным потребителем PanTool.

### Решение — deletion на существующих швах
- **PanTool удалён целиком** (+ фабрика `[typeof(PanTool)]` в ToolRegistry, + 20 тестов PanToolTests): пан — жест `CanvasInputRouter`, применяемый через `ZoomPanManager.PanCanvas`; единственный источник истины — `EditorCanvasState`. Вариант «PanTool — владелец пана» отклонён: дельта пана обязана считаться в стабильной оконной системе координат (S45/S51) — это знание View-слоя, а ITool говорит модельными координатами; пан-specific seam был бы швом с единственным адаптером.
- **Роутер:** два pan-бранча (Middle / Space-Alt-Left) объединены; предикат жеста вынесен в `internal static IsPanGesture` (без него ветка Space/Alt нетестируема — `Keyboard.IsKeyDown` не фейчится); `RoutePanDown` сжат до `(canvas, state, windowPoint)` — параметры `canvasPoint`/`button` жили ради мёртвого вызова; вызов `panTool.OnMouseUp(default, ...)` в RouteMouseUp удалён.
- **IEditorContext:** член `PanCanvas` удалён (**30 → 29 членов**) — единственный потребитель PanTool; follow-up'ом удалён и forwarding-метод с EditorViewModel — роутер ходит в `ZoomPanManager.PanCanvas` напрямую (Middle Man с единственным потребителем, по Standards-ревью).
- **Тесты:** −20 PanToolTests, −1 InlineData в ToolRegistryTests; +8 кейсов `IsPanGesture` (канонический набор жестов) + полный цикл Left-пути (`RoutePanDown_LeftGestureFullCycle_StopsCleanlyAndRefreshesGrid`) + capture-assert в показанном окне; re-coverage удалённого поведения — существующие `ApplyPan_*` и `PanCanvas_*`.
- **Домены:** CONTEXT.md — «Пан» вынесен из секции «Инструмент»: жест, не инструмент; **первый ADR проекта** `docs/adr/0001-pan-gesture-not-tool.md`.

### Итоги
- Поведение байт-в-байт: формула дельты с Y-flip и инкрементальным применением, `CaptureMouse`/`ReleaseMouseCapture`, `RefreshGridNodes()` после конца пана, курсор `SizeAll`, пермиссивные жесты (Middle | Left+Space | Left+Alt по отдельности).
- Live-рассинхронизация состояний устранена структурно (второй источник истины удалён).
- Двухосевое code-review: Standards — 0 hard violations (удаление Middle Man/Duplicated Code/dead-параметров); Spec — 2 MINOR (capture-assert, имена тестов) закрыты коммитом; зафиксированное отклонение от буквы спеки: триггер RouteMouseDown с зажатым модификатором unit-тестом не покрывается (`Keyboard.IsKeyDown` не фейчится) — детекция закрыта теорией `IsPanGesture`.
- Счётчики включают fix #82 (допуск маркеров, влит в develop до среза).

**Build:** 0 errors, 0 warnings
**Tests:** 2619 total, 2618 passed (0 failures, 1 pre-existing skip)
**Coverage:** 89.89% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 1 обзора №4: один конвейер рендеринга (#88–#89, PR #91, 18.08.2026)

### Проблема
«Показать объект на листе» было реализовано трижды независимо (DataTemplates канваса + конвертеры; императивный preview-behavior; генератор FixedDocument для печати), и поверхности разошлись: preview ставил верх текста по `FontSizeMicrons`, канвас — по `HeightMicrons`, печать — по `MicronsY` без высоты и без LayoutTransform-offset при повороте. Копии правил: шрифт-switch ×3, dash ×2, hex→brush ×2, Y-flip ×5, выравнивание ×2.

### Решение — атомарный flip (один PR на существующих швах)
- **`RenderRules`** (Helpers, статический, без DI): карта ГОСТ-шрифтов (имя → pack-URI → FontFamily, unknown/null → Segoe UI), frozen dash-карта (`LineType` → DoubleCollection, Solid/unknown → null), hex→Brush (семантика канваса), `ModelYToTop(yMicrons, sheetHeightMm, scale)` (масштаб — параметр: zoom vs 96/25.4), карта выравнивания текста, anchor-политика на тип (единый dispatch; Text = `MicronsY + HeightMicrons` — верх нетрансформированного бокса; неизвестный тип → явный throw, без silent-default). Guards MultiBinding остались в адаптерах-конвертерах.
- **Канвас:** 7 конвертеров — тонкие делегаты (шрифт, dash, hex, Y-flip, TopEdge/LeftEdge, выравнивание); XAML не менялся; 216 тестов конвертеров без изменений (+1 новый; байт-паритет). Формулы anchor'ов уехали в правила; позиционная дискриминация MultiBinding (isLine/isText по индексам) осталась в адаптерах — объектный dispatch невозможен без правки XAML (зафиксированное отклонение).
- **Preview:** верх текста по `HeightMicrons` через `AnchorTopMicrons` (вместо `FontSizeMicrons`), шрифт из правил; сейм не перестраивался — кандидат 2.
- **Печать:** приватные дубли hex/dash/шрифт/выравнивание удалены целиком; anchor текста = канвас-семантика (высота через свойство модели, многострочность учитывается, LayoutTransform при повороте сохранён). Отклонение от буквы спеки зафиксировано: компенсацию LayoutTransform-offset применяет WPF при раскладке (тот же механизм, что канвас); прямой вызов `GetLayoutTransformOffset` дал бы двойную компенсацию.
- **Сетка:** Y-flip слоя узлов через правила (`ToMm` вместо `*1/1000` — не побитово идентично на последний ulp, визуально ничтожно; строгий байт-паритет требовался только для канваса).
- **Побочное улучшение:** unfrozen кэшированный DoubleCollection оказался thread-affine (поймано STA-тестами печати) — правила отдают frozen-инстансы, читаемые из любого потока; зафиксировано тестом.
- **Тесты:** RenderRules 53 unit-теста (точные значения, углы 45°/90°/135°/270° инвариантности slot-anchor, frozen-семантика, throw на неизвестный тип), +5 preview (точные anchor'ы, multiline поймал дефект), +7 печать (Y-позиция с высотой, многострочность, поворот, точные dash-значения), +1 конвертер — итого +66.

### Итоги
- Два живых дефекта устранены (единственные видимые изменения); копии правил исчезли бесследно (deletion-проверка); локализация: расхождение anchor'ов исчезло как класс ошибок.
- Двухосевое code-review: Standards — 0 hard violations (judgement call «TextAlignment-дубль» закрыт коммитом); Spec — ядро выполнено, три отклонения зафиксированы в теле PR #91.

### Не вошло / отложено
- Несовпадение font-fallback (рендер unknown → Segoe UI vs FontMetrics ratios 1.0/0.6) — отдельный дефект, радар.
- Стратегии размещения линий (канвас — локальные координаты, preview/печать — абсолютные) — не трогали.
- Мёртвый `RelativeMicronsToPixelConverter` + двойная регистрация конвертеров → кандидат 8 (мёртвые швы).
- Реструктуризация preview-сейма → кандидат 2 обзора №4.

**Build:** 0 errors, 0 warnings
**Tests:** 2685 total, 2684 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.04% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 2 обзора №4: preview-сейм (#93–#94, PR #96, 18.08.2026)

### Проблема
«Показать предпросмотр» шло через фасад: 3 инструмента рисования писали preview через 3 свойства `IEditorContext` (`PreviewLine`/`PreviewRectangle`/`PreviewText { get; set; }`), реализованные чистым форвардингом на EditorViewModel; единственный читатель (`PreviewLineChangedBehavior`) был подписан на `PreviewManager.PropertyChanged`, но читал через тот же форвардинг. Уведомление держалось на хрупком ре-ассайн-трюке: инструмент мутировал свойства объекта и ре-ассайнил ту же ссылку, чтобы стрельнул ручной сеттер PreviewManager с безусловным `OnPropertyChanged()` (workaround R3.1-HF1; Common Mistakes #13/#60) — забыть ре-ассайн = preview молча перестаёт обновляться. Плюс мёртвый `SelectionBoxRight` в PreviewManager (0 production-потребителей, остаток среза 1).

### Решение — атомарный flip (один PR на существующих швах)
- **IEditorContext 29 → 27:** три preview-члена удалены; выставлено существующее свойство `PreviewManager` (ноль нового кода VM). Конструкторы инструментов и фабрики ToolRegistry не менялись.
- **EditorViewModel:** 3 forwarding-свойства preview-тройки удалены целиком; очистка при смене инструмента (`ClearAll`) сохранена.
- **Протокол P2 (центр среза):** инструменты ассайнят preview-объект только при создании (MouseDown) и обнулении (MouseUp/DoubleClick/Reset); на MouseMove — только мутация свойств, без ре-ассайна. Рендерер подписан на INPC текущего preview-объекта с отпиской при swap/clear/unregister (generic `Swap<T>`/`Unsubscribe<T>` — дедупликация по Standards-ревью); чтения — только из PreviewManager; координаты — через `RenderRules`/`Coordinate.ToMm` напрямую, экземпляры конвертеров удалены.
- **PreviewManager:** preview-свойства на `[ObservableProperty]` — контракт «уведомление только при смене ссылки» (ручные сеттеры с безусловным notify удалены, комментарии «NOT [ObservableProperty]» тоже); мёртвый `SelectionBoxRight` удалён вместе с атрибутом `[NotifyPropertyChangedFor]` на ширине.
- **XAML: 0 изменений** (preview-элементы — named, без биндингов). Визуально байт-в-байт.
- **CONTEXT.md:** термин «Предпросмотр» (решение Q8 grilling-сессии).
- **Тесты:** миграция ~70 чтений с forwarding-свойств VM на `PreviewManager.*`; +7: контракт same-reference на менеджере, мутации без ре-ассайна (Line/Rectangle/Text — guard'ы маршрутизации подписки), отписка при swap/clear/unregister; −1 (Right). TDD: контрактные и мутационные тесты прошли red → green на согласованных швах (PreviewManager unit, behavior STA через WpfContext, реальный EditorViewModel e2e).

### Итоги
- Ре-ассайн-трюк исчез как класс ошибок; PreviewManager — единственный источник истины для писателей и читателя; локализация: preview-состояние в одном модуле.
- Двухосевое code-review: Standards — 0 открытых hard violations (дедупликация Swap/Render закрыта коммитом; HARD по тексту CODING_STANDARDS §4.6/§7.3 переписан в docs-PR; имя класса и двойной доступ к рамке отложены с обоснованием). Spec — ядро выполнено полностью; MINOR закрыты (тест swap добавлен; приватный вложенный `ObjectSubscriptions` — зафиксированная девиация «буква vs дух»: внутренний state-контейнер модуля, не публичная абстракция).

### Не вошло / отложено
- Имя класса `PreviewLineChangedBehavior` (pre-existing; исторические отсылки в документации) — класс рендерит все три типа предпросмотра.
- Перерисовка preview при смене zoom во время рисования (статус-кво до следующего MouseMove), стилизация preview (красный/dash 4,2 — намеренный хардкод), TextWrapping/TextAlignment/поворот preview-текста, DI-регистрация PreviewManager — наблюдения спеки #93.

**Build:** 0 errors, 0 warnings
**Tests:** 2691 total, 2690 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.13% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 3 обзора №4: CommandHistory единолично владеет грязностью (#98–#99, PR #101, 18.08.2026)

### Проблема
Грязность стреляла двумя каналами: каждая undo-команда носила собственный делегат `markDirty` (8 invoke'ов в AddObject/Delete/ChangeProperty/Batch), и история команд стреляла тем же делегатом при Push — двойной выстрел в 9 точках сборки. Undo/Redo истории не стреляли вовсе, и EditorViewModel компенсировал асимметрию ручным `MarkDirty()` после вызова истории. Конвенция «null в подкомандах, делегат в batch» уже не соблюдалась: инструменты рисования и вставка/удаление создавали команды без делегата, перемещение/поворот/resize/inline-редактирование/панели свойств — с делегатом.

### Решение — атомарный flip (один PR на существующих швах)
- **CommandHistory — единственный источник:** `Push` (позиция не меняется), `Undo` и `Redo` стреляют `_markDirty` один раз в конце метода после успешного перемещения команды по стекам; rollback-путь при исключении (команда возвращается в стек) не стреляет — операция не состоялась. Сигнатура конструктора `(maxLevels, Action? markDirty = null)` не меняется; единственная production-точка инъекции — модель редактора.
- **Команды без делегата:** `markDirty` удалён из `AddObjectCommand`, `DeleteObjectCommand` (оба бранча Execute, включая no-op), `ChangePropertyCommand<T>` (оба конструктора); трёхпараметровый конструктор `BatchCommand (commands, name, markDirty)` удалён целиком.
- **Компенсация удалена:** ручного `MarkDirty()` в `EditorViewModel.Undo()`/`Redo()` больше нет; хвостовые `NotifyCanExecuteChanged` + уведомления `UndoDisplayName`/`RedoDisplayName` сохранены (UI-жизненный цикл relay-команд). Принятый побочный эффект: INPC `IsDirty` приходит чуть раньше — внутри вызова истории, до чистки осиротевшего выделения; зависимостей от порядка нет.
- **Волна сужения конструкторов:** `ObjectPropertiesViewModel<TObject>` — конструктор-тройка → пара (`CommandHistory?`, `setValidationError`), поле `_markDirty` и проброс в `SetProperty` удалены; `Line/Rectangle/TextPropertiesViewModel`, `PropertiesViewModel` (одноарговый конструктор сохранён, цепочка на пару), `InlineEditManager` — без параметра; две точки создания в конструкторе EditorViewModel обновлены.
- **IEditorContext 27 → 26:** член `MarkDirty` удалён (потребители — только передачи делегата в конструкторы команд). Публичные `MarkDirty()`/`ClearDirty()` VM сохранены: первый — цель единственного делегата истории, второй вызывается сервисом операций с вкладками после сохранения.
- **XAML: 0 изменений.** Визуально байт-в-байт.
- **CONTEXT.md:** термин «Грязность» (добавлен в grilling-сессии).
- **Тесты:** TDD red-фаза — 4 теста CommandHistory (`Undo_WithMarkDirty_CallsCallback`, `Redo_WithMarkDirty_CallsCallback`, rollback-пути Undo/Redo при исключении не стреляют); e2e-pinning через реальный EditorViewModel — 3 теста («Push → dirty», «ClearDirty → Undo → dirty снова», «Undo → Redo → dirty»; эквивалентов среди 48 IsDirty-assert'ов suite не было). Удалены 24 теста собственного делегата команд и панелей (7 ResizeObjectCommandTests + 4 CommandTests + 7 ChangePropertyCommandResizeTests + 5 PropertiesViewModelTests + 1 PropertiesViewModelCommandTests); 97 механических правок call-site'ов (96 третьих аргументов конструктора PropertiesViewModel + 1 фикстура InlineEditManagerTests).

### Итоги
- Двойной выстрел и асимметрия Undo/Redo устранены структурно (deletion-чеклист 4/4: markDirty только в CommandHistory; MarkDirty нет в IEditorContext; нет в Undo/Redo VM; нет в конструкторах волны).
- Локализация: «что делает шаблон грязным» — один модуль; новая undo-команда не заботится о флаге.
- Двухосевое code-review: Spec — полное соответствие (ничего не пропущено, scope creep нет); Standards — 0 hard violations в коде, документационный конфликт (старое правило «MarkDirty в командах») переписан этим docs-PR (CODING_STANDARDS §5.4, §15#16, Common Mistakes #6, счётчики IEditorContext).
- Поведение байт-в-байт: любой Push/Undo/Redo помечает шаблон грязным в точности как раньше; сохранение сбрасывает флаг; автосохранение только читает IsDirty.

### Не вошло / отложено
- **Checkpoint-семантика сохранной точки** (отмена до сохранённого состояния снимает «*»; IsDirty = глубина стека ≠ checkpoint на момент Save) — на радаре: меняет видимое поведение и усложняется обрезкой истории на 50 уровней.
- Порядок захвата method group `MarkDirty` в конструкторе EditorViewModel до создания `DirtyStateManager` — pre-existing, безопасен сегодня (Push в конструкторе нет); наблюдение Standards-ревью.
- Двойные уведомления undo-меню (VM.MarkDirty + хвосты Undo/Redo) — pre-existing, не в диффе.

**Common Mistakes (new):**
79. Новые undo-команды не несут делегат markDirty — грязность обеспечивает `CommandHistory.Push/Undo/Redo` (один выстрел после успешного выполнения; rollback при исключении не стреляет). Не добавлять делегат в интерфейс undo-команд и не помечать IsDirty вручную: единственный источник — история команд.

**Build:** 0 errors, 0 warnings
**Tests:** 2674 total, 2673 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.08% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 4 обзора №4: раскладка маркеров — один модуль MarkerLayout (#103–#104, PR #106, 18.08.2026)

### Проблема
Знание «какие маркеры выделения есть у объекта и где они» жило в четырёх независимых местах: таблица зон попадания с допуском в HitTestHelper (пер-тип приватные методы), знание «какие маркеры угловые/рёберные» — четырьмя наборами паттернов в ResizeMath, визуальная раскладка 14 маркеров — строковыми путями к свойствам модели в XAML, ResizeTool держал ручной per-type снапшот геометрии параллельно модельному ResizeState. Согласованность копий поддерживалась вручную — баг #82 (фиксированная зона 8 мм поглощала клики по телу маленького объекта) был дефектом взаимодействия двух копий. Четыре silent-default ветки маскировали непредставимые состояния.

### Решение — атомарный flip (один PR на существующих швах)
- **`MarkerLayout`** (Helpers, статический, без DI — по образцу RenderRules): каталог маркеров по типу объекта в порядке приоритета hit-проверки (у линии конец первым — он наверху в Z-order; прямоугольник — все 8; текст — 4 угла с существующим маппингом RotatedCorner0–3), `GetPosition` (позиции в модельных координатах, единая точка чтения свойств модели), `HitHandle` (дистанция ≤ допуска, first-match по каталогу), `GetTolerance` — семантика фикса #82 `min(8 мм, minDim/3)` сохранена, классификация (`TouchesLeft/Right/Top/Bottom`, `IsCorner`), курсорная политика — оба mapping'а переехали из ResizeMath (включая свап диагональных курсоров при 90°/270°).
- **HitTestHelper сжат до хита по телу** (207 → 27 строк): `GetHitHandle`/`GetHandleTolerance`/три приватных зонных метода удалены; шесть мёртвых публичных методов (`HitTestAll`, `HitTestLine/Rectangle/Text`, `HitTestObject`, `DistanceFromPointToLine` — байт-дубль приватного в Line) удалены целиком вместе с тестами.
- **ResizeMath — чистая математика drag'а:** курсорные функции удалены, четыре набора `handle is ... or ...` заменены вызовами классификации модуля; `ComputeLineEndpoint` для не-концевого маркера — явный throw вместо молчаливых (0, 0).
- **ResizeTool — единственный снапшот:** 8 ручных per-type полей удалены, математика читает вход из модельного `ResizeState`, снятого один раз на MouseDown (он же идёт в undo-команду); значения идентичны (Line — Width/Height состояния = дельты концов, Text — в Height записан FontSizeMicrons).
- **IEditorContext 26 → 25:** член `HoveredHandle` удалён (писатель и единственный читатель — SelectTool; состояние стало приватным полем инструмента). `ActiveResizeHandle` сохранён — межинструментная передача SelectTool → ResizeTool + повторный вход роутера.
- **Четыре silent-default ветки → явные throw** (`NotSupportedException`, прецедент RenderRules): неизвестный тип объекта в каталоге/допуске/позиции, маркер вне каталога типа, не-концевой маркер линии, неизвестный маркер в курсорах.
- **Enum `ResizeHandle`** — единственное определение в MarkerLayout (перенос из ResizeTool, имя без изменений, namespace-правки потребителей механические).
- **XAML: 0 изменений.** Визуальные позиции маркеров и зоны модуля читают одни свойства модели — расхождение невозможно без правки обеих сторон. Поведение байт-в-байт.
- **CONTEXT.md:** термин «Маркер выделения» (у линии 2 маркера (концы), у прямоугольника 8 (углы + середины сторон), у текста 4 (угла); Avoid: handle, grip, ручка).
- **Тесты:** миграция зон/допуска/повёрнутых углов текста из HitTestHelperTests и 38 курсорных кейсов из ResizeMathTests в MarkerLayoutTests; UnknownObject-тесты → `Assert.Throws`; новые: каталог (состав + порядок приоритета), позиции (литеральные ожидания), приоритет на нулевой линии, 4 throw-ветки, классификация всех 8 маркеров; удалены тесты шести мёртвых методов; ResizeToolTests (71), математика ResizeMath (40 кейсов), tool-e2e с блоком Bug82 ×5, DP-тесты MarkerPosition — без изменений; два теста ToolTests переведены на реальный hover-поток.

### Итоги
- Четыре копии геометрии исчезли; класс дефектов «зона hit разошлась с визуальной раскладкой» локализован в одном модуле (deletion-чеклист 6/6: в HitTestHelper только хит по телу; в ResizeMath нет курсоров и паттерн-наборов; в ResizeTool нет ручных снапшот-полей; enum определён ровно в одном месте; HoveredHandle отсутствует в IEditorContext; дифф XAML — ноль).
- Двухосевое code-review: Standards — 0 hard violations (judgment call: декодирование ResizeState линии `X + Width`/`Y + Height` продублировано в ResizeTool.ResizeLine и Line.ApplyResize — цена удаления снапшот-полей, знание локализовано комментарием); Spec — 2 MINOR (Escape-тест → реальный hover-поток, закрыт коммитом; потеря дублей body-hit тестов при поворотах вместе с `HitTestText` санкционирована планом удаления — поведение модели покрыто TextTests/ObjectBaseTests).

### Не вошло / отложено
- Compile-time валидация строковых путей `MarkerPosition` (опечатка = тихий runtime-провал биндинга) — лечится только правкой XAML; на радаре.
- SelectTool показывает Cross при наведении на маркер, resize-курсоры появляются только внутри ResizeTool — статус-кво; на радаре.
- Микро-перформанс RotatedCorner (GetLayoutTransformOffset вычисляется на каждое обращение каждого из 8 свойств) — на радаре.
- Переименование идентификаторов с «handle» — глоссарный термин зафиксирован, код не переименовывается.

**Common Mistakes (new):**
80. Геометрия маркеров выделения — только в `MarkerLayout`: не добавлять таблицы координат маркеров, зоны/допуски, классификацию маркеров или курсоры в другие модули. `ResizeMath` — чистая математика drag'а (курсоры и классификация — в MarkerLayout), `HitTestHelper` — только хит по телу объекта (хит по маркерам — в MarkerLayout). Неизвестный тип объекта или маркер вне каталога типа — явный throw, без silent-default.

**Build:** 0 errors, 0 warnings
**Tests:** 2614 total, 2613 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.25% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 8 обзора №4: мёртвые швы — deletion sweep (#108–#109, PR #111, 18.08.2026)

### Проблема
Кодовая база несла серию мёртвых швов — кода с нулём production-потребителей, создающего иллюзию функциональности: конструктор ZoomPanManager с двумя callback-параметрами (во всех точках создания no-op), мёртвый сервис шаблонов в модели редактора (фабрика резолвила его только для проброса), DI-регистрации без потребителей (`ITextToolSettings`, `IFontMetrics`), четыре конвертера без единого биндинга, девять живых конвертеров продублированы в двух-трёх resource-словарях (один — с неиспользуемым ключом), осиротевший метод валидации поворота, контракт автосохранения с `object`-шаблоном и мёртвым сеттером пути.

### Решение — атомарный флип (один PR на существующих швах)
- **ZoomPanManager**: оба callback-параметра удалены из конструктора; поле callback'а обновления сетки с no-op-инициализацией, единственный канал wiring'а — существующий `SetGridRefreshCallback` (production ставит его до первого события — семантика не меняется); мёртвый вызов zoom-callback из обработчика изменения зума удалён.
- **EditorViewModel**: мёртвое поле `_templateService` и параметр `ITemplateService` обоих конструкторов удалены; фабрика перестаёт резолвить сервис шаблонов (единственным потребителем был проброс); поле `_gridNodeGenerator` понижено до локальной переменной конструктора. Каскад: ~40 механических обновлений test call-sites, осиротевшие моки удалены.
- **ITextToolSettings удалён целиком**: DI-регистрация, интерфейс, опциональный параметр конструктора TextTool; инструмент создаёт `TextToolSettings` напрямую (fallback `?? new TextToolSettings()` выигрывал всегда — поведение идентично).
- **IFontMetrics**: удалена только DI-регистрация; интерфейс жив тестовыми моками, production ходит через статический `FontMetrics.Default`.
- **Конвертеры**: четыре мёртвых класса (`ZoomToStringConverter`, `LineTypeToStringConverter`, `TextTypeToStringConverter`, `RelativeMicronsToPixelConverter`) удалены вместе с тестами; ресурсы всех живых конвертеров консолидированы в корневом словаре ресурсов приложения — каждый класс объявлен ровно один раз, ключ — тот, который используют биндинги (неиспользуемый ключ `Not` исчез, единственный ключ `NotConverter`); view-декларации удалены. Исключение: `InverseBooleanConverter` объявлен view-локально в SettingsView — окно загружается в STA-тестах без ресурсов приложения (перенос в корневой словарь пойман тестом как XamlParseException).
- **ValidateRotation** (0 точек вызова после введения свободного поворота) удалён.
- **Автосохранение (механизм сохранён, решение Q5-b)**: контракт вкладки типизирован — шаблон отдаётся модельным типом вместо `object` (паттерн-матчинг в пути записи исчез), мёртвый сеттер пути файла удалён из интерфейса; горизонт очистки задаётся существующей константой `AutosaveCleanupDays` вместо литерала. Read-поверхность (`LoadSession`/`GetAutosaveFilePath`/`ClearAutosaveFolder`) сознательно оставлена — это поверхность будущего аварийного восстановления; продуктовая спека «построить restore или убить» — в радаре. Мёртвый тест ветки `is Models.Template` (ставшей невозможной после типизации) удалён.
- **Мелкая механика**: `EnumToIndexConverter` запечатан (конвенция семейства). Пункт чеклиста «`GridStepToStringConverter` без `partial`» не подтвердился фактами: у класса есть source-generated вторая часть (`[GeneratedRegex]`), модификатор необходим — задокументированная девиация.
- **CONTEXT.md**: термин «Автосохранение» (write-only семантика: файлы пишутся, восстановление состояния не выполняется).

### Итоги
- Deletion-чеклист спеки 9/9 (с двумя задокументированными девиациями); класс «мёртвый шов с нулём потребителей» устранён: grep-проверки по всем удалённым символам — 0 вхождений в src/.
- Двухосевое code-review: Standards — 0 hard violations (дифф закрывает задокументированные стандарты; judgment calls: общая тестовая фикстура оставлена per-file по конвенции проекта, `TextToolSettings` как кандидат на инлайн — в радаре, дрейф счётчиков документации закрыт этим docs-PR; косметика — формулировка комментария в App.xaml и trailing newline в CONTEXT.md — закрыта фикс-коммитом); Spec — полное соответствие, scope creep не найден, обе девиации исполнены и задокументированы.
- Поведение байт-в-байт: UI неотличим; Автосохранение пишет файлы с прежним периодом, именами и путями. Смоук приложения: старт и загрузка MainWindow/EditorCanvas/панели свойств без ошибок разрешения ресурсов.

### Не вошло / отложено
- ValidationService «static vs injectable» (прямые вызовы из панелей свойств) — не мёртвый код, а отдельный рефакторинг; статические UI-валидаторы санкционированы H2.
- `GetViewportMicrons` — легитимный тестовый шов, сохранён.
- Аварийное восстановление сессии (построить restore или убить Автосохранение целиком) — продуктовая спека; до её решения read-поверхность сохраняется.
- `TextToolSettings` как кандидат на инлайн (держатель четырёх констант, делегирующих в `EditorSettings`) — на радаре.
- Общая тестовая фикстура создания модели редактора — оставлена per-file (конвенция проекта — локальные хелперы).

**Common Mistakes (new):**
81. Ресурсы конвертеров объявляются ровно один раз — в корневом словаре ресурсов приложения; view-локальная декларация допустима, только если вид загружается вне контекста приложения (STA-тесты без ресурсов приложения — прецедент SettingsView/InverseBooleanConverter). Не добавлять дубли деклараций в view-словари. Перед удалением модификатора `partial` проверить source-generated части класса (`[GeneratedRegex]` и др.) — они не видны в файле, их отсутствие в репо не означает «мёртвый partial».

**Build:** 0 errors, 0 warnings
**Tests:** 2576 total, 2575 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.6% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 5 обзора №4: форматы листа — один каталог (#113–#114, 19.08.2026)

### Проблема
Знание о стандартных форматах листа жило в 11 независимых точках: два switch'а в `Sheet` (размеры и дефолтные ориентации), HashSet `ValidFormats` валидатора (16 записей с латинскими X-дублями), `FormatOptions` настроек, меню «Файл > Новый шаблон» (20 MenuItem с захардкоженными размерами в заголовках), 5 кнопок быстрого выбора диалога произвольного размера, парсер суффиксов P/L, дефолт «A3» ×6. Копии разошлись: латинское `A4X2` валидно в файлах, но не представимо в ComboBox настроек; настройка «формат нового шаблона по умолчанию» была мёртвой (писалась, не читалась); неизвестный формат обрабатывался несогласованно (throw в FromFormat, молчаливый Landscape в GetDefaultOrientation).

### Решение
- **`SheetFormatCatalog`** (Models, статический, без DI — прецеденты RenderRules/MarkerLayout) + sealed record **`SheetFormat`** (Name, LongSideMicrons, ShortSideMicrons, DefaultOrientation) + `IsHalfFormat`: 10 записей (A0–A4 + пять полуформатов); API `All`/`Get` (throw «Неизвестный формат листа: {format}»)/`TryGet`/`Contains` (регистр + x/X нечувствительно)/`Normalize` + `const DefaultName = "A3"`; пользовательский формат в каталог не входит.
- **Идентичность — строка, не enum** (формат сериализуется в .tdel и settings.json) — **ADR-0002**; новый формат = одна запись каталога.
- **Модель листа** делегирует каталогу: оба перегруза `FromFormat` и `GetDefaultOrientation`; switch'и удалены; молчаливый Landscape удалён — единый throw на обоих путях.
- **Валидатор:** HashSet удалён → `Contains` + Custom; V-006 перечисляет канонические 10 + Custom без латинских дублей; файлы с `A4X2` по-прежнему валидны.
- **Настройки:** `FormatOptions` генерируется из каталога.
- **Меню «Файл > Новый шаблон» генерируется из каталога:** `MainViewModel.NewSheetMenu` (13 пунктов: 10 групп, 2 разделителя, «Пользовательский...»; заголовки ориентаций компонуются из размеров каталога, ориентация по умолчанию первая), XAML — `ItemsSource` + `ItemContainerStyle` с DataTrigger'ами (разделитель — disabled-контейнер с Separator-темплейтом; пункт-команда — `NewCustomTabCommand`); визуально байт-в-байт (пины заголовков/параметров/порядка в MainViewModelTests).
- **Сервис вкладок:** цепочка fallback'ов «явный → последний использованный → `DefaultSheetFormat` из настроек → дефолт каталога» — мёртвая настройка оживает; мусорные значения отбрасываются (байт-в-байт с прежним хардкодом «A3»).
- **Дефолт «A3» ×6** заменён константой каталога (TabOperationsService, TemplateService — включая аварийный fallback чтения повреждённого файла с сохранением семантики «A3 + 420×297», ITemplateService, AppSettings ×2, Template).
- **Диалог произвольного размера:** `SetQuickFormat` — TryGet-no-op вместо глотания исключений; 5 кнопок без изменений.
- **CONTEXT.md:** термины «Стандартный формат» и «Пользовательский формат».

### Итоги
- Поведение байт-в-байт; единственные видимые изменения — устранённые расхождения (латинские дубли в сообщении V-006).
- Двухосевое code-review: Standards — 0 hard violations (5 judgement calls: `Normalize` сохранён без production-потребителя — входит в API спеки, зафиксированная девиация; `SheetFormatCatalog.Default` удалён как вне чеклиста; `IsHalfFormat` локализован в record; `Sheet.CustomName` вместо литерала; Mode=OneWay на readonly-биндингах меню; файл модели меню — по основному типу); Spec — 0 неверно, 2 MINOR (прямой пин throw GetDefaultOrientation добавлен; Default удалён) — закрыты фикс-коммитом.
- Смоук приложения: старт, меню «Файл > Новый шаблон» рендерится из каталога (группы, «Книжная (210×594)» у A4×2, «Пользовательский...»), клик по сгенерированному пункту открывает вкладку «A4×2 (кн.) — Без имени».
- Новые Common Mistakes не добавлены: ревью не выявило устойчивых паттернов вне уже задокументированных.

### Не вошло / отложено (радар)
- Полуформаты среди кнопок быстрого выбора диалога произвольного размера — продуктовое изменение.
- Продуктовая семантика «формат по умолчанию побеждает последний использованный» — противоречит идентичности Ctrl+N (NewTabWithLastFormat); отдельная спека.
- Метки ориентации «кн.»/«алб.» — три копии (StatusBarManager, DirtyStateManager, EditorViewModel).
- Мёртвая настройка `DefaultZoom` — близнец проблемы формата по умолчанию.

**Build:** 0 errors, 0 warnings
**Tests:** 2630 total, 2629 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.38% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 6 обзора №4: TextGeometry — геометрия повёрнутого текста из модели (#118–#119, PR #121, 19.08.2026)

### Проблема
Модельный класс `Text` (~393 строки) нёс ~60% кода, который является не доменным знанием, а знанием о поведении WPF: компенсация LayoutTransform-offset'а повёрнутого TextBlock (private `GetLayoutTransformOffset` без прямых тестов), 8 свойств повёрнутых углов с тригонометрией в каждом getter'е, повёрнутые `ContainsPoint`/`GetBoundingBox`. Дополнительно мёртвые швы: `VisualLeft/VisualRight/VisualBottom/VisualTop` (0 потребителей в production/XAML/тестах) и `RotationAngleValid => true` (тавтология после свободного поворота, потребители — 2 тавтологичных теста). Архитектурный обзор также заявлял живой дефект печати повёрнутого текста.

### Решение — атомарный флип (TDD на новом шве)
- **`TextGeometry`** (Helpers, статический, без DI — прецеденты RenderRules/MarkerLayout), 4 функции: `LayoutOffset(Text)` — публичная (смещение WPF LayoutTransform: минимум по отрицательным компонентам четырёх углов локального Y-down бокса, повёрнутых вокруг origin); `Corner(Text, index)` — углы 0–3 в порядке каталога MarkerLayout (TopLeft/TopRight/BottomLeft/BottomRight), индекс вне диапазона — явный throw; `Contains(Text, PointMicrons)` — обратное вращение вокруг фактического центра вращения (anchor + offset); `BoundingBox(Text)`. Формулы и округления перенесены дословно; дедупликация прелюда по Standards-ревью (приватные `Trig`/`LayoutOffset(w,h,cos,sin)`/`RotationCenter`).
- **Модель Text (393 → 217 строк):** 8 свойств повёрнутых углов — однострочные делегации в `Corner` (XAML маркеров — 0 изменений, строковые пути MarkerPosition продолжают резолвиться); override'ы `ContainsPoint`/`GetBoundingBox` — делегации; полиморфный контракт базы сохранён (Line/Rectangle не тронуты); INPC-проводка (`NotifyAllRotatedCorners`, partial-хуки размера/контента/шрифта/переноса, уведомления сеттеров координат и угла) — обязанность модели. `WidthMicrons`/`HeightMicrons` (FontMetrics), `LineSpacingFactor`, производные края — в модели (доменные размеры).
- **Мёртвые швы удалены:** `Visual*`×4 + вся INPC-обвязка (12 атрибутов + 8 вызовов в сеттерах); `RotationAngleValid` + 2 тавтологичных теста.
- **Print-дефект обзора проверен на коде и НЕ подтвердился:** печать и канвас ставят текст одинаковым механизмом (`LayoutTransform = RotateTransform(угол)` + anchor `MicronsY + HeightMicrons` через `RenderRules.AnchorTopMicrons`), смещение трансформированного бокса применяет WPF-раскладка на обеих поверхностях; пин — тест `Generate_RotatedText_SlotAnchor_SameAsUnrotated`. Ручная компенсация в генераторе была бы двойной.
- **Тесты:** TDD red→green (red = CS0103×30 до создания модуля, green 22/22 до переподключения модели); новый `Tests/Helpers/TextGeometryTests.cs` — 13 методов / 22 кейса: 12 мигрированы из TextTests (углы 0°/45°/90°/180°/270°, хит повёрнутого текста, bounding box), offset-тесты переписаны на прямой вызов `LayoutOffset`, + тест throw-ветки индекса; `RotatedCorners_AllLieOnBoundingBoxEdges` остался в TextTests пином поверхности свойств для XAML (счётчик спеки «13 мигрируют» разрешён в пользу Q9: 12 мигрируют + 1 пин).
- **CONTEXT.md/ADR:** без нового термина (модуль — реализационная структура), без ADR (прецедент RenderRules/MarkerLayout).

### Итоги
- Поведение байт-в-байт; XAML, MarkerLayout, HitTestHelper, SelectionBoxHelper, PrintDocumentGenerator, PreviewLineChangedBehavior — 0 изменений (zero-diff guard).
- Deletion-чеклист 3/3: grep `Math.Cos/Math.Sin` в Models = 0; `Visual*`/`RotationAngleValid`/`GetLayoutTransformOffset` в src = 0; XAML и модули guard'а без diff.
- Двухосевое code-review: Standards — 0 hard violations (дедупликация тригонометрического прелюда закрыта фикс-коммитом; Primitive Obsession по индексу 0–3 отклонён — маппинг задокументирован); Spec — полное соответствие.
- TDD, CI 5/5, смоук не требовался (структурный срез без UI-изменений).

### Не вошло / отложено (радар)
- Кэширование вычисления layout-offset'а (пересчёт на каждое обращение свойства — байт-в-байт статус-кво; радар кандидата 4).
- Compile-time валидация строковых путей MarkerPosition (опечатка = тихий runtime-провал биндинга; радар кандидата 4).

**Common Mistakes (new):**
82. Геометрия повёрнутого текста — только в `TextGeometry`: не добавлять формулы повёрнутых углов, LayoutTransform-offset, хит-тест или bounding box повёрнутого текста в модель `Text` или другие модули. Модель — тонкие делегации: 8 свойств углов (в них биндится XAML маркеров через строковые пути MarkerPosition) и полиморфные contains/bounding box; INPC-проводка углов — обязанность модели. Знание «WPF позиционирует повёрнутый элемент по верхнему левому углу трансформированного bounding box» — знание модуля, не модели; доменные размеры (`WidthMicrons`/`HeightMicrons` через FontMetrics) остаются в модели. Индекс угла вне 0–3 — явный throw, без silent-default.

**Build:** 0 errors, 0 warnings
**Tests:** 2630 total, 2629 passed (0 failures, 1 pre-existing skip)
**Coverage:** 90.34% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 1 обзора №5: документ без редактора (#137–#138, PR #140, 20.08.2026)

### Проблема
Документная библиотека `DotElectric.Document`, вынесенная «без знания редактора» (спека #135, PR #136), несла это знание: 6 из 8 констант `PhysicalConstants` читало только приложение (маркеры, рамка выделения, ресайз, поля панели свойств, диалог произвольного формата), а сама модель документа — только допуск попадания в тело линии/прямоугольника. Плюс семантика привязки к сетке (`Coordinate.SnapToGrid`, `PointMicrons.SnapToGrid`) при живом CONTEXT.md «Сетка — не понятие документа» и побайтовый дубль формулы снапа точки в приложении (`SnapHelper.SnapToGrid`). Отношение миллиметра к микрону существовало в трёх копиях (`PhysicalConstants.MicronsPerMm`, публичный `Coordinate.MicronsPerMm`, приватный в `Sheet`).

### Решение — атомарный флип (чистый перенос на существующих швах)
- **`PhysicalConstants` → `DocumentConstants`** (git mv): единственный член — `LineHitToleranceMicrons` (единственная константа, которую читает сам документ: `Line.ContainsPoint`, `Rectangle.ContainsPoint`); класс-комментарий фиксирует критерий размещения.
- **Шесть констант взаимодействия — в `EditorSettings`** (новая секция `// Interaction`, имена и значения сохранены): `HandleHitToleranceMicrons` (MarkerLayout), `SelectionBoxThresholdMicrons` (SelectTool ×3), `MinResizeSizeMicrons` (DrawingRectangleTool, ResizeTool), `MinFontSizeMicrons` (ResizeTool, ValidationService), `MinDimensionMicrons` (ValidationService), `MaxCustomSheetSizeMm` (CustomSheetDialogViewModel).
- **Привязка к сетке вне библиотеки:** `Coordinate.SnapToGrid(long,long)` и `PointMicrons.SnapToGrid(long)` удалены целиком; формула — приватное скалярное ядро `SnapHelper.Snap` (публичная поверхность без изменений: `SnapToGrid(point, step)`, `SnapX`, `SnapY`, `SnapSize`, `SnapObject`, `SnapIfEnabled`; `SnapIfEnabled` делегирует статическому снапу точки; `ArgumentOutOfRangeException` при неположительном шаге побайтно — то же сообщение и имя параметра).
- **`MicronsPerMm` консолидирован:** константа удалена из `DocumentConstants`; `GridManager.GridStepMm`, `GridSettings.FromAppSettings` и хардкод `1.0/1000.0` в `GridNodesLayer` переведены на `Coordinate.MicronsPerMm`. Приватная копия в `Sheet` не тронута — `DotElectric.Sheets` изолирована (без ссылок на Document).
- **Попутный фикс радара:** XML-комментарий `Template.Sheet` переписан без имени потребителя из приложения (GridManager); попутно починен битый XML-тег `SnapY` (`</param>` → `</returns>`, поведение не задевает — задокументированная девиация ревью).
- **Тесты:** 6 тестов формулы привязки переехали из `DotElectric.Document.Tests` в `SnapHelperTests` (5 скалярных через `SnapX` + снап точки); `point.SnapToGrid` в тестах приложения переписан через `SnapHelper`; ссылки на константы — механически. Новых поведенческих тестов нет (чистый перенос).

### Итоги
- Поведение байт-в-байт; XAML — 0 изменений; смоук не требовался (структурный срез без UI-изменений).
- Инвариант «суммарное число тестов не изменилось» подтверждён эмпирически: baseline ветки миграции 9571776 замерен в чистом worktree — 274+97+2246 = **2617**; после среза 268+97+2252 = **2617** (2616 passed + 1 pre-existing skip). Число 2630 из AGENTS.md — метрика develop-эпохи до миграции тестов.
- Deletion-чеклист: `PhysicalConstants`, `Coordinate.SnapToGrid` — 0 вхождений в src/; все вызовы `.SnapToGrid(` — через `SnapHelper`; шесть констант — только в `EditorSettings` и потребителях.
- Двухосевое code-review: Standards — 1 MINOR-хард (CODING_STANDARDS §6.3/§15#5 описывали удалённый `PhysicalConstants` — синхронизированы этим docs-PR); Spec — полное соответствие, 2 MINOR scope-девиации задокументированы (фикс XML-тега `SnapY`, класс-комментарий `DocumentConstants`).
- CI: 5/5 зелёный, merged line-rate **91.11%** (gate 80%) — рост против 90.34%: тесты переехали вместе с кодом в проекты библиотек.
- Слияние: PR #140 в develop (миграция #136 слилась в процессе — стек развернулся в прямой PR; merge 7505fb2).

### Не вошло / отложено (радар)
- Кандидат 3 обзора №5 (один шов проверки документа) — следующая спека.
- Мёртвый `SnapHelper.SnapObject` (0 потребителей в production) — кандидат 5 обзора №5 (deletion sweep №2).
- Унификация приватного `MicronsPerMm` в `Sheet` — изоляция `DotElectric.Sheets` осознанная.
- Проверка верхней границы Custom-формата документным валидатором — статус-кво (предел живёт в диалоге).

**Common Mistakes (new):**
83. Константы живут в сборке читателя: документная библиотека (`DotElectric.Document`) несёт только то, что читает сама модель документа (`DocumentConstants.LineHitToleranceMicrons`); константы взаимодействия редактора (допуски маркеров, пороги рамки, минимумы ресайза и шрифта, пределы диалогов) — в `EditorSettings` приложения (секция Interaction). Привязка к сетке — понятие редактора (CONTEXT.md «Сетка»): формула живёт только в `SnapHelper` (приватное скалярное ядро); не возвращать `SnapToGrid` в `Coordinate`/`PointMicrons` и не добавлять новые константы редактора в `DocumentConstants`.

**Build:** 0 errors, 0 warnings
**Tests:** 2617 total, 2616 passed (0 failures, 1 pre-existing skip)
**Coverage:** 91.11% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 3 обзора №5: один шов проверки документа (#142–#143, PR #145, 20.08.2026)

### Проблема
Проверка документа была выставлена двумя поверхностями: обёрткой `ITemplateService.Validate(Template) → IEnumerable<string>` в сервисе шаблона (побайтно дублировала `ValidationError.ToString()`, прятала фильтр серьёзности, несла мёртвую null-ветку «Шаблон не может быть null.» без RuleId) и типизированным `ITemplateValidator`. Проверка цвета — цепочка трёх хопов вокруг одной функции: DI-регистрация `IValidationService` → проброс-поле `Helpers.ValidationService.Default` → приватный адаптер `HexColorValidation.DefaultValidationService` → статическая `HexColorValidation.Validate`. Факты на момент среза: единственный production-вызов обёртки — одна строка в `TabOperationsService.SaveTabAsync`; ошибки НЕ блокируют сохранение (диалог «Save anyway?»), предупреждения до пути сохранения не доходят (фильтр обёртки); автосохранение и «Сохранить как» проверку не выполняют.

### Решение — чистый deletion на существующих швах
- **`Validate` удалён из `ITemplateService`** (5 → 4 члена: CreateNew/CreateFromSheet/Load/Save) и из `TemplateService` (метод, поле `_templateValidator`, параметр конструктора, запасное создание `new TemplateValidator()`) — сервис шаблона только чтение/запись.
- **Потребитель напрямую:** `TabOperationsService` — новый параметр конструктора `ITemplateValidator` (DI резолвит существующей singleton-регистрацией); фильтр серьёзности (в диалог только `ValidationSeverity.Error`) и склейка сообщений (`ToString()` через `\n`) — инлайн в потребителе, побайтно с форматом обёртки. Мёртвая null-ветка удалена без переноса: `tab.Template` в производстве не null, валидатор покрывает null правилом V-000.
- **Цвет — два удаления:** поле-посредник `ValidationService.Default` (DI регистрирует `HexColorValidation.Default` напрямую) и обёртка `ValidationService.ValidateHexColor` (три панели свойств вызывают `HexColorValidation.Validate` напрямую; 4 теста цвета ретаргечены, имена `HexColorValidation_*`). `IValidationService` сохранён — шов конструктора `TemplateValidator` (два теста с моками).
- **Документация к поведению (в срезе):** XML-комментарий `ValidationSeverity.Error` («блокирует сохранение» → фактическая семантика: сохранение с ошибками подтверждается пользователем); статья «Правила проверки документа» в CONTEXT.md — «проверка выполняется при сохранении: ошибки спрашивают подтверждение пользователя (сохранение при ошибках возможно), предупреждения в решении о сохранении не участвуют. Загрузка файл не проверяет.»
- **Тесты:** 6 тестов удалённой обёртки удалены (`TemplateServiceTests` ×2, `TemplateServiceRoundTripTests` ×4 — включая единственный пропуск набора, V-001 уже покрыт тестами валидатора); путь сохранения в `TabOperationsServiceTests` переведён со строкового стаба `Mock<ITemplateService>` на `Mock<ITemplateValidator>` (+ null-guard нового параметра конструктора); +1 пин-тест «предупреждения не показывают диалог, сохранение идёт» (фильтр серьёзности запинен на стороне потребителя).

### Итоги
- Поведение байт-в-байт; XAML — 0 изменений; единственный шов проверки документа — `ITemplateValidator`.
- Deletion-чеклист спеки 7/7: grep удалённых символов по src/ = 0; `IValidationService` сохранён; счётчик 2617 → **2613** (−6 обёртка, +2 новых), пропусков 1 → 0.
- Двухосевое code-review: Spec — полное соответствие (1 MINOR: дополнительный null-guard тест оправдан конвенцией файла); Standards — 1 MINOR-хард (CODING_STANDARDS §6.3 описывал удалённое делегирование цвета — синхронизирован фикс-коммитом) + judgment call (имена тестов цвета `HexColorValidation_*`) — закрыты; размещение V-тестов в проекте приложения — территория кандидата 4 обзора №5.
- CI 5/5 зелёный, merged line-rate **91.13%** (gate 80%).

### Не вошло / отложено (радар)
- `SaveAsAsync` без проверки; переиспользование диалога несохранённых изменений для текста ошибок + англоязычные строки save-пути; `public static ValidateMetadataKeys`; недостижимая ветка V-007 (`Enum.IsDefined` для определённых значений enum); проверка при загрузке как продуктовый вопрос.
- Перенос V-тестов (ValidationServiceTests/SelectionAndValidationTests/HelperTests/IntegrationTests) в проект библиотеки — кандидат 4 обзора №5 («добить миграцию»).

**Common Mistakes (new):**
84. Документация шва следует за швом: удаляя или перестраивая шов (интерфейс, делегирование, поле-посредник), упомянутый в CODING_STANDARDS, синхронизируй стандарт в том же PR — grep файла стандарта по именам удаляемых символов. Два рецидива: кандидат 1 обзора №5 (`PhysicalConstants` в §6.3/§15#5) и кандидат 3 обзора №5 (делегирование цвета `ValidationService` в §6.3) — оба находки MINOR-хард Standards-ревью.

**Build:** 0 errors, 0 warnings
**Tests:** 2613 total (2613 passed, 0 пропусков)
**Coverage:** 91.13% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 4 обзора №5: добить миграцию — тесты библиотек застряли в приложении (#147–#148, PR #150, 20.08.2026)

### Проблема
Миграция документной модели в библиотеки (спека #135, PR #136) перенесла production-код в `DotElectric.Document` и `DotElectric.Sheets`, но решение «тесты едут с кодом» не доведено до конца: тесты библиотечных типов остались в app-проекте — `ValidationServiceTests.cs` (52 теста доменного валидатора V-000…V-007 — имя-реликт: app-`ValidationService` там не тестировалась вовсе), `ModelTests.cs` (`Coordinate`/`PointMicrons`), `TemplateTests.cs` (библиотечные `Template`/`Sheet`/`Metadata` вперемешку с app-`Grid`/`GridSettings`), `ServiceTests.cs` (`TemplateServiceExtendedTests` — сам `TemplateService` и весь XML/ZIP .tdel в библиотеке), библиотечные части `HelperTests.cs` и `SelectionAndValidationTests.cs`; четыре локальные фабрики шаблона в переезжающих файлах; реликтовые отчёты (`coverage_report.txt`, `final_coverage_report.txt`, `test_results.txt`).

### Решение — чистый перенос на существующих швах
- **Document.Tests (созданы):** `TemplateValidatorTests.cs` — 75 тестов, слияние трёх источников (48 из `ValidationServiceTests` + 10 из `AdditionalValidationServiceTests` + 17 из `ExtendedValidationServiceTests`), имя-реликт устранено, коллизия `Validate_NullTemplate_*` решена переименованием; `HexColorValidationTests.cs` (4); `TemplateTests.cs` (11, включая reflection-тест `Clone`); `MetadataTests.cs` (2); `RectMicronsTests.cs` (16: 9 из `SelectionAndValidationTests.cs` по карте спеки + 7 из секции RectMicrons `SelectionBoxHelperTests.cs` — по deletion-чеклисту спеки в приложении не остаётся тестов, чей предмет — собственно библиотечный тип).
- **Document.Tests (влито в существующие):** `CoordinateTests.cs` (+8), `PointMicronsTests.cs` (+7), `TemplateServiceTests.cs` (+4 из `TemplateServiceExtendedTests`); коллизии имён устранены переименованием (4 штуки).
- **Sheets.Tests:** новый `SheetTests.cs` (10 атрибутов / 24 исполняемых теста).
- **Общая фикстура:** `TestTemplates.cs` — четыре локальные фабрики шаблона переехавших тестов слиты в один помощник (`CreateValidA4`/`CreateA3`/`CreateA3(fixedDate)`), туда же `TestTemplateObject` и `SetId`.
- **Приложение:** app-остатки смешанных файлов влиты в родительские (конвенция R4.4 — Additional/Extended-файлов нет): `SnapHelperTests` (+6, включая единственный снап-тест из `PointMicronsExtendedTests` — привязка к сетке сознательно понятие редактора), `HitTestHelperTests` (+2), `SelectionBoxHelperTests` (+8 тестов рамки выделения, секция RectMicrons уехала в библиотеку), `FileServiceTests` (+6; три коллизии имён — суффикс `_DefaultCtor`); `Models/TemplateTests.cs` переименован в `Models/GridSettingsTests.cs` (остаток: app-`Grid`/`GridSettings` + `MockTemplateFileService`).
- **Удалены:** 5 файлов-источников (`ValidationServiceTests.cs`, `HelperTests.cs`, `SelectionAndValidationTests.cs`, `ModelTests.cs`, `ServiceTests.cs`) + 3 реликтовых отчёта.
- **Осознанные дубли:** `FixedFontMetrics` ×2 и `FontMetricsTestCollection` ×2 (приложение + `Document.Tests`) — нового общего тестового проекта нет: определение коллекции xUnit обязано жить в той же сборке, что и использующие её тесты.

### Итоги
- Тесты: **2613 (2613 passed, 0 пропусков)** — ни один тест не добавлен и не удалён; распределение: приложение 2085 + `Document.Tests` 401 + `Sheets.Tests` 127; assert'ы перенесены как есть.
- Production-код и XAML — ноль изменений (чистый перенос тестов); WPF/STA в переезжающем коде проверен — отсутствует.
- `IntegrationTests.cs` остался в приложении (настоящая интеграция: библиотечный `TemplateService` × app-`CommandHistory`) — единственное разрешённое пересечение.
- CI 5/5 зелёный, merged line-rate **91.13%** (gate 80%) — точно как базовая линия: состав покрытия перераспределился между проектами без уменьшения объёма.
- Слияние: PR #150 в develop (merge d212ce9); спека #147 и тикет #148 закрылись автоматически (оба Closes в теле PR).

### Не вошло / отложено (радар)
- Кандидат 5 обзора №5 (deletion sweep №2: `TextToolSettings`, `Grid`, мёртвая `DefaultZoom`, мёртвые константы `EditorSettings`, mojibake) — отдельная спека.
- Кандидат 6 обзора №5 (запасной шрифт: fallback рендера `RenderRules` не совпадает с `FallbackFontMetrics`) — отдельная спека.
- App-`ValidationService` (четыре UI-валидатора полей) — остаётся; возможная мёртвость `ValidateMetadataKeys` — территория кандидата 5.

**Build:** 0 errors, 0 warnings
**Tests:** 2613 total (2613 passed, 0 пропусков; распределение: приложение 2085 + Document.Tests 401 + Sheets.Tests 127)
**Coverage:** 91.13% line-rate ✅ (gate 80% достигнут)

## Sprint — Кандидат 5 обзора №5: deletion sweep №2 — мёртвое знание приложения (#152–#153, PR #155, 20.08.2026)

### Проблема
Кандидат 5 архитектурного обзора №5 (отчёт `architecture-review-20260820-1540.html`) — «Deletion sweep №2: мёртвое знание приложения» (Сильная рекомендация). Приложение несло слой мёртвого и расходящегося знания: `TextToolSettings` (15-строчный посредник, единственный потребитель сам создаёт экземпляр; два свойства — делегации в `DocumentDefaults`); класс `Grid` — синглтон ради одной константы 5000, продублированной в `EditorSettings.DefaultGridStepMicrons` и `AppSettings.GridStepMm = 5.0`; настройка-призрак `DefaultZoom` (пишется в Settings UI и settings.json, но не применяется нигде); мёртвые константы `DoubleClickThresholdMs = 500` (двойной клик обрабатывает WPF нативно) и `DefaultSheetOffsetMm = 10.0`; расхождение nudge: `NudgeStepMicrons = 1000` (1 мм) мёртв, а живое поведение хардкодит 100 (0.1 мм); метки ориентации «кн.»/«алб.» тремя копиями (StatusBarManager/DirtyStateManager/EditorViewModel); Rotate ±90 двумя почти идентичными методами; mojibake (двойная UTF-8-кодировка) в трёх файлах — включая 11 строковых литералов логов `AutosaveService`; осиротевшие методы `SnapHelper.SnapObject` и сеттеры `TextTool`; три теста-реликта `ValidationService_ValidateObject*` в `IntegrationTests` (фактически тесты библиотечного валидатора). Grilling-сессия 20.08.2026: все решения Q1–Q12 подтверждены одним раундом (Q4 — вариант (а): `DefaultZoom` удалить, не оживлять).

### Решение — один deletion-проход по списку
- **`TextToolSettings` удалён:** четыре дефолта инлайнены в `TextTool` — шрифт и размер из библиотечного `DocumentDefaults` (как уже делается для цвета), тип `TextType.Text`, содержимое — локальная константа «Текст». Осиротевшие сеттеры `SetTextType`/`SetFontSize`/`SetDefaultContent` (0 вызовов в production) удалены вместе с 5 тестами.
- **Класс `Grid` удалён:** `GridSettings.CreateDefault()` (переименован из `FromDefaultGrid` — имя не ссылается на несуществующую сущность, фикс по code-review) и запасной путь `FromAppSettings` переведены на `EditorSettings.DefaultGridStepMicrons` — одна константа шага 5 мм вместо трёх копий; `GridTests` (2 теста синглтона) удалён вместе с классом.
- **`DefaultZoom` удалён целиком:** свойство `AppSettings`, свойство `SettingsViewModel` + `ZoomOptions`, ComboBox «Масштаб:» в секции «НОВЫЙ ШАБЛОН» `SettingsView.xaml`, ссылки в тестах. Схема settings.json байт-совместима — неизвестные ключи при чтении игнорируются (прецедент `CustomSettings`). Оживление — продуктовая фича в радаре.
- **Константы:** `DoubleClickThresholdMs` и `DefaultSheetOffsetMm` удалены (0 потребителей); `NudgeStepMicrons` переиспользован: значение 1000 → 100 и подключение в `EditorViewModel.NudgeStep` — расхождение «константа 1 мм vs хардкод 0.1 мм» устранено, фактическое поведение 0.1 мм сохранено; `BigNudgeStepMicrons` (10 мм) не тронут.
- **Метки ориентации:** новая статическая точка `Helpers/OrientationLabels.For` — `StatusBarManager`, `DirtyStateManager`, `EditorViewModel` делегируют; неизвестная ориентация — явный throw (конвенция RenderRules/MarkerLayout, фикс по code-review). Генерируемое меню «Файл > Новый шаблон» не трогалось.
- **Rotate ±90:** приватное ядро со знаком как параметром; публичные `RotateSelectedClockwise`/`RotateSelectedCounterClockwise` — тонкие делегаты (вызовы `ShortcutRegistry` E/Shift+E без изменений).
- **Mojibake восстановлен:** селективное декодирование CP1251→UTF-8, 86 строк, 0 пропусков — `AutosaveService` (11 строковых литералов Serilog «AutosaveService запущен. Интервал: … мин.» и др. + XML-доки/комментарии), `MainViewModel` (комментарии/доки), `IMessageBoxProvider` (XML-доки). Контрольный grep сигнатур двойной кодировки по src/ = 0.
- **`SnapHelper.SnapObject` удалён** (0 потребителей в production; радарная позиция кандидата 1 обзора №5) вместе с 3 тестами; остальная поверхность `SnapHelper` не тронута.
- **Три тест-метода `ValidationService_ValidateObject*`** перенесены из `IntegrationTests` в `Document.Tests/TemplateValidatorTests` — assert'ы побайтно, реликт имени устранён (последний осколок «V-тестов в приложении» после кандидата 4).

### Итоги
- Поведение байт-в-байт; единственное видимое изменение — восстановленные строки логов (исправление дефекта). XAML — только удаление ComboBox «Масштаб:». Библиотеки не изменены (кроме пополнения `Document.Tests`).
- Тесты: **2603 (2603 passed, 0 пропусков)** = базовая линия 2613 − 10 тестов удалённого кода (`GridTests` 2 + `SnapObject` 3 + сеттеры `TextTool` 5); перенос 3 тестов число не меняет (приложение 2085→2072, `Document.Tests` 401→404, `Sheets.Tests` 127).
- Deletion-чеклист: grep удалённых символов (`TextToolSettings`, `Grid.Default`, `DefaultZoom`, `ZoomOptions`, `DoubleClickThresholdMs`, `DefaultSheetOffsetMm`, `SnapObject`, `SetTextType`, `SetDefaultContent`) по src/ = 0 вхождений; `NudgeStepMicrons` = одна константа, подключённая в `NudgeStep`; `OrientationLabels` = одно определение, три делегата.
- Двухосевое code-review: Standards — 0 hard violations (2 judgment calls закрыты фикс-коммитом: явный throw в `OrientationLabels`; `FromDefaultGrid` → `CreateDefault`; комментарий «Перенесено из…» оставлен — одобренный стиль кандидата 4); Spec — полное соответствие, 0 находок.
- CI 5/5 зелёный, merged line-rate **91.13%** (gate 80%) — без изменений (мёртвый код не вносил покрытия).
- Слияние: PR #155 в develop (merge d3c9b7a); спека #152 и тикет #153 закрылись автоматически (оба Closes в теле PR).

### Не вошло / отложено (радар)
- Кандидат 6 обзора №5 (запасной шрифт: fallback рендера `RenderRules` не совпадает с `FallbackFontMetrics`) — отдельная спека.
- Оживление `DefaultZoom` (применение масштаба при создании вкладки) — продуктовая фича, вне deletion-среза.
- Публичная поверхность `TemplateValidator.ValidateMetadataKeys` (библиотека; метод жив — вызывается из `Validate()`, вопрос только в статической поверхности) — вне скоупа приложения.
- App-`ValidationService` (четыре UI-валидатора полей) — остаётся, покрыт существующими тестами панелей свойств.

**Build:** 0 errors, 0 warnings
**Tests:** 2603 total (2603 passed, 0 пропусков; распределение: приложение 2072 + Document.Tests 404 + Sheets.Tests 127)
**Coverage:** 91.13% line-rate ✅ (gate 80% достигнут)

## Agent skills

### Issue tracker

Issues и спеки живут в GitHub Issues CodeLaw-lab/DotElectric (через `gh` CLI). См. `docs/agents/issue-tracker.md`.

### Triage labels

Пять дефолтных ролей: needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix. См. `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `CONTEXT.md` в корне (создаётся лениво) + `docs/adr/`. См. `docs/agents/domain.md`.

