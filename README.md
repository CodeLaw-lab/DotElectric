# DotElectric Template Editor

**Статус:** ✅ Этап 1 ЗАВЕРШЁН (все 44 FR выполнены)
**Тестов:** 2625, 0 сбоев, 0 пропусков
**Сборка:** 0 errors, 0 warnings
**Покрытие:** 92.05% line-rate (CI gate 80%)
**Последнее обновление:** 21.08.2026 — Кандидат 7 обзора №5 (#167, тикет #168, PR #170): предпросмотр печати через шов диалогов и типизированная ориентация меню. `MainViewModel.PreviewPrint` больше не создаёт окно — показ через `IDialogHostService` (тонкая `PrintPreviewViewModel`, окно с конструктором от модели); владелец диалогов по умолчанию — главное окно (инъектируемый провайдер в WPF-реализации шва, CenterOwner работает у всех трёх окон); пункты меню несут типизированную ориентацию (`NewSheetOrientationEntry(Header, Format, Orientation)`), строковый протокол «A4L» и парсер суффиксов удалены целиком; фасад `CreateNewTab` с типизированной ориентацией, цепочки запасных значений побайтно; settings.json байт-совместим; CONTEXT.md — статья «Предпросмотр печати»; байт-в-байт кроме центрирования двух диалогов, тесты 2625 (2655 − 33 теста парсера + 3 пина; распределение: приложение 2044 + Document 454 + Sheets 127), coverage 92.05%

[![CI](https://github.com/anomalyco/dotelectric/actions/workflows/ci.yml/badge.svg)](https://github.com/anomalyco/dotelectric/actions)
[![Coverage](https://img.shields.io/badge/coverage-92%25-green)](https://github.com/anomalyco/dotelectric/actions)

---

## 📋 О ПРОЕКТЕ

DotElectric — собственная CAD-система для электриков, предназначенная для разработки конструкторской документации (схемы, чертежи, перечни элементов, спецификации, ведомости покупных изделий, таблицы соединений и т.д.).

### Цели проекта

- Создание полноценной CAD-системы для электриков
- Поддержка ГОСТ ЕСКД
- Интеграция с 1С (в будущем)
- Современный и удобный интерфейс

---

## 🚀 ЭТАПЫ РАЗРАБОТКИ

| Этап | Название | Статус | Срок |
|------|----------|--------|------|
| **1** | Редактор шаблонов листов | ✅ ЗАВЕРШЁН (2625 тестов, 100%) | Q2 2026 |
| **2** | Редактор УГО | ⚪ Запланирован | Q3 2026 |
| **3** | Работа с БД компонентов | ⚪ Запланирован | Q4 2026 |
| **4** | Главный редактор схем | ⚪ Запланирован | Q1 2027 |

---

## 📁 СТРУКТУРА РЕПОЗИТОРИЯ

```
dotElectric/
├── .github/                         # GitHub Actions CI/CD
├── .opencode/                       # OpenCode агенты и скилы
├── docs/                            # Документация
│   ├── 00_Индекс_документов.md
│   ├── 01_Техническое_задание_Этап1.md
│   ├── 02_User_Stories_Этап1.md
│   ├── 03_Спецификация_требований_Этап1.md
│   ├── 05_Руководство_пользователя_черновик.md
│   ├── 09_UI_решения.md
│   ├── 19_Статус_проекта.md
│   ├── 47_План_развития_Этап2.md
│   ├── 48_Архитектурный_анализ_и_план_рефакторинга.md
│   ├── 49_План_рефакторинга_R1-R4.md
│   ├── План_ручного_тестирования.md
│   └── archive/                     # Архивные отчёты спринтов
├── fonts-GOST/                      # GOST A/B шрифты
├── src/                             # Исходный код
│   ├── Directory.Build.props
│   ├── DotElectric.TemplateEditor.slnx
│   ├── DotElectric.TemplateEditor/
│   │   ├── App.xaml / App.xaml.cs   # DI, Serilog, Mutex
│   │   ├── AssemblyInfo.cs
│   │   ├── MainWindow.xaml / .cs
│   │   ├── Constants/               # EditorSettings (interaction constants included)
│   │   ├── Models/                  # AppSettings, GridSettings, Grid
│   │   ├── Messages/                # WeakReferenceMessenger сообщения
│   │   ├── ViewModels/
│   │   │   ├── Abstractions/
│   │   │   ├── Managers/            # ZoomPan, Selection, Tool, Grid, etc.
│   │   │   ├── Templates/
│   │   │   └── *.cs                 # EditorVM, PropertiesVM, MainVM, etc.
│   │   ├── Views/
│   │   │   ├── Templates/
│   │   │   └── *.xaml / .cs         # EditorCanvas, PropertiesPanel, Settings, etc.
│   │   ├── Services/                # FileService, Autosave, Settings, Print, DI
│   │   ├── Tools/                   # Select, Pan, Line, Rectangle, Text, Resize
│   │   ├── Commands/                # AddObject, Delete, ChangeProperty, Batch
│   │   ├── Helpers/                 # HitTest, Grid, Snap, Validation, ShortcutRegistry
│   │   ├── Converters/              # 27 sealed converter classes
│   │   ├── Behaviors/               # CanvasInputRouter, MarkerPosition, etc.
│   │   └── Resources/
│   │       ├── Fonts/               # GOST Type AU/BU
│   │       ├── Icons/               # SVG/PNG иконки
│   │       └── Styles/              # XAML темы Light/Dark
│   ├── DotElectric.Document/        # Документная модель (объекты, .tdel, проверка) — без знания редактора
│   ├── DotElectric.Sheets/          # Форматы листа (изолирована, без зависимостей)
│   ├── DotElectric.Document.Tests/  # xUnit v3 тесты DotElectric.Document (418)
│   ├── DotElectric.Sheets.Tests/    # xUnit v3 тесты DotElectric.Sheets (127)
│   └── DotElectric.TemplateEditor.Tests/
│       ├── Models/
│       │   └── Objects/             # Line, Rectangle, Text тесты
│       ├── ViewModels/
│       │   ├── Managers/            # Grid, Tool, ZoomPan, DirtyState тесты
│       │   └── Templates/
│       ├── Services/                # File, Autosave, Print, Template, Settings тесты
│       ├── Tools/                   # Select, Pan, Resize, Drawing тесты
│       ├── Commands/                # Command, History, ResizeCommand тесты
│       ├── Helpers/                 # HitTest, Grid, Snap, Validation тесты
│       ├── Converters/              # Converter тесты
│       └── Behaviors/               # STA-тесты поведения
├── AGENTS.md                        # Master-файл агентов (правила, история)
├── CHANGELOG.md                     # Changelog (Keep a Changelog)
├── CONTRIBUTING.md                  # Рекомендации контрибьюторам
├── .coverage-baseline.txt           # Baseline покрытия (CI gate)
├── A3_shtamp.tdel                   # Пример шаблона A3
├── A3_shtamp.pdf                    # Пример PDF A3
└── voprosy.txt                      # Вопросы заказчика
```

---

## 🛠 ТЕХНОЛОГИИ

### Основной стек

- **.NET 10** — платформа разработки
- **WPF** — UI фреймворк
- **C#** — язык программирования

### Библиотеки и инструменты

- **CommunityToolkit.Mvvm** — MVVM-фреймворк
- **xUnit** — тестирование
- **Moq** — мокирование
- **Microsoft.Extensions.DependencyInjection** — внедрение зависимостей

### Архитектура

- **MVVM** — паттерн проектирования
- **Dependency Injection** — внедрение зависимостей
- **Command Pattern** — реализация команд
- **Fixed-Point** — координаты в микронах (без погрешности)

### Форматы

- **.tdel** — собственный формат шаблонов (XML в ZIP)
- **XML** — хранение данных шаблона

---

## 📚 ДОКУМЕНТАЦИЯ

### Для заказчика

| Документ | Описание |
|----------|----------|
| [Техническое задание](docs/01_Техническое_задание_Этап1.md) | Требования к системе |
| [User Stories](docs/02_User_Stories_Этап1.md) | Функции с точки зрения пользователя |
| План разработки | Дорожная карта и спринты |
| [Руководство пользователя](docs/05_Руководство_пользователя_черновик.md) | Инструкция по использованию |

### Для разработчиков

| Документ | Описание |
|----------|----------|
| [Спецификация требований](docs/03_Спецификация_требований_Этап1.md) | Детальное описание архитектуры и API |
| Анализ техлида + Sprint 23 | 16 задач (6 P0, 6 P1, 4 P2), 39 SP |

---

## 🔧 ТРЕБОВАНИЯ К РАЗРАБОТКЕ

### Системные требования

- Windows 10/11
- Visual Studio 2022 или JetBrains Rider
- .NET 10 SDK

### Сборка проекта

```bash
# Клонирование репозитория
git clone https://github.com/anomalyco/dotelectric.git
cd dotElectric

# Сборка
dotnet build src/DotElectric.TemplateEditor.slnx

# Запуск
dotnet run --project src/DotElectric.TemplateEditor/DotElectric.TemplateEditor

# Тесты (покрытие >= 80%)
dotnet test src/DotElectric.TemplateEditor.Tests --collect:"XPlat Code Coverage"
```

### Требования к разработке

- **IDE:** Visual Studio 2022 / JetBrains Rider
- **.NET 10 SDK**
- **Шрифты:** ГОСТ А, ГОСТ Б (в комплекте)

---

## 📝 ФУНКЦИОНАЛЬНОСТЬ ЭТАПА 1

### Реализованные функции

#### Управление шаблонами
- Создание, открытие, сохранение (Ctrl+N/O/S)
- Форматы: A0–A4, A4×2–A0×2, Custom (до 2000×2000 мм), Portrait/Landscape
- Multi-tab с изолированным Undo/Redo
- In-memory clipboard (Copy/Paste/Cut) со смещением
- Автосохранение
- **Settings UI** — диалог настроек (тема, сетка, привязка, шаг, автосохранение, формат/масштаб по умолчанию)

#### Инструменты рисования
- Линия, Прямоугольник, Текст с preview-паттерном
- Shift — привязка к 45°/квадрат
- Настраиваемая толщина обводки (StrokeThicknessMicrons)

#### Выделение и трансформации
- Одиночное и множественное выделение (Shift/Ctrl+Click, selection box)
- Перетаскивание с привязкой к границам листа
- Nudge (стрелки) / BigNudge (Shift+стрелки) — динамический шаг
- Resize через 8 маркеров (Shift — пропорции, Ctrl — от центра)
- Поворот (E/Shift+E — 90°, свободный 0-359°)
- Inline-редактирование текста (двойной клик, Ctrl+Enter — коммит)
- MultiLine + TextAlignment (Left/Center/Right)

#### Свойства
- Панель свойств с группировкой по секциям
- Цвета: StrokeColor, FillColor, Foreground (HEX #RRGGBB / #AARRGGBB / Transparent)
- LineType (Solid, Dash, DashDot)
- Text Key, IsEditable, DefaultValue
- Live update при изменении объектов

#### Навигация
- Zoom: 10%–1000% (колесо, Ctrl++/-, Fit, ComboBox)
- Pan: средняя кнопка мыши / Space+ЛКМ, CaptureMouse
- Scrollbar синхронизация

#### Сетка
- Шаг 0.5–10 мм, отображение узлов
- Привязка к сетке
- MinPixelSpacing — скрытие сетки при высокой плотности

#### Печать
- Ctrl+P — системный диалог печати
- Предпросмотр печати (Ctrl+Shift+P) — DocumentViewer с FullPage

#### UI
- Material Design, темы Light/Dark (F9, с сохранением)
- StatusBar (формат, zoom, grid/snap toggle, clipboard feedback)
- Библиотека шаблонов (импорт .tdel, удаление)
- Контекстные меню (холст, вкладки)
- Keyboard shortcuts (все, включая русскую раскладку)

### Тестирование

- ✅ **2625 тестов**, 0 сбоев, 0 пропусков
- ✅ **Покрытие:** 92.05% line-rate (CI gate 80%)
- ✅ **Сборка:** 0 errors, 0 warnings
- ✅ **#167–#168:** Кандидат 7 обзора №5 — предпросмотр печати через шов диалогов и типизированная ориентация меню: `PreviewPrint` показывает окно через `IDialogHostService` (`PrintPreviewViewModel` + маппинг, окно с конструктором от модели), владелец диалогов по умолчанию — главное окно (инъектируемый провайдер, CenterOwner у всех трёх окон), пункты меню несут типизированную ориентацию, строковый протокол «A4L» и парсер суффиксов удалены целиком, фасад `CreateNewTab` с типизированной ориентацией (цепочки побайтно), settings.json байт-совместим, CONTEXT.md — «Предпросмотр печати»; байт-в-байт кроме центрирования двух диалогов, тесты 2625 (2655 − 33 теста парсера + 3 пина), coverage 92.05%
- ✅ **#162–#163:** Кандидат 6 обзора №5 — запасной шрифт: рендер и метрики сходятся в каталоге шрифтов: неизвестное имя шрифта = шрифт по умолчанию («ГОСТ А») в рендере и геометрии; `FontCatalog` + `FontDescriptor` в документной библиотеке — единственный владелец идентичности, внутренних имён файлов и запасных коэффициентов; `FallbackFontMetrics`/`WpfFontMetrics`/`RenderRules` делегируют (кэш `FontFamily`); раунд-трип неизвестного имени байт-совместим; ADR-0003 правлен по месту; известные шрифты байт-в-байт, два декларированных отклонения для неизвестного имени, тесты 2655 (2617 + 38 пинов), coverage 91.16%
- ✅ **#157–#158:** Кандидат 2 обзора №5 — формат и проверка знают тип (каталог типов объекта): `ObjectTypeCatalog` + `ObjectTypeDescriptor` в документной библиотеке — единственный владелец строк идентичности «Line»/«Rectangle»/«Text», маппинга «запись ↔ модель» и объектных правил проверки (V-003/V-004/V-007/V-005); `TemplateService` — ноль switch'ей по типу; идентификатор объекта переживает «сохранил → загрузил» (отсутствующий/пустой → новый, копия получает новый); мёртвый `ValidateObject` удалён (контракт валидации 2 → 1); ошибки проверки группируются по объекту; ADR-0004; модель не изменена, поведение байт-в-байт, кроме двух декларированных отклонений, тесты 2617 (2603 − 6 тестов удалённого шва + 20 пинов), coverage 91.18%
- ✅ **#152–#153:** Кандидат 5 обзора №5 — deletion sweep №2 (мёртвое знание приложения): `TextToolSettings` удалён (дефолты инлайнены в `TextTool`), класс `Grid` удалён (шаг сетки — одна константа), настройка-призрак `DefaultZoom` удалена целиком (settings.json байт-совместим), мёртвые константы удалены + `NudgeStepMicrons` → 100 подключён в `NudgeStep` (расхождение устранено, поведение сохранено), метки ориентации «кн.»/«алб.» — одна точка `OrientationLabels.For`, Rotate ±90 — приватное ядро со знаком, mojibake восстановлен (86 строк в трёх файлах), `SnapHelper.SnapObject` удалён с 3 тестами, три теста-реликта перенесены в `Document.Tests`; поведение байт-в-байт (видимое изменение — только строки логов), тесты 2603 (2613 − 10 тестов удалённого кода), coverage 91.13% без изменений
- ✅ **#147–#148:** Кандидат 4 обзора №5 — добить миграцию тестов библиотек: тесты библиоточных типов перенесены из app-проекта в `Document.Tests`/`Sheets.Tests` (V-правила тремя источниками в один `TemplateValidatorTests` + `HexColorValidationTests`, новые `TemplateTests`/`MetadataTests`/`RectMicronsTests`/`SheetTests`, слияния в `CoordinateTests`/`PointMicronsTests`/`TemplateServiceTests`), общая фикстура `TestTemplates`, 5 файлов-источников и 3 реликта удалены; 2613 тестов без добавлений и удалений, production-код и XAML — ноль изменений
- ✅ **#142–#143:** Кандидат 3 обзора №5 — один шов проверки документа: обёртка `ITemplateService.Validate` удалена (интерфейс 5→4 члена), `TabOperationsService` инжектит `ITemplateValidator` напрямую (фильтр «только Error» + формат в потребителе, побайтно), цепочка цвета сокращена (`HexColorValidation` напрямую, `IValidationService` сохранён), поведение байт-в-байт
- ✅ **#137–#138:** Кандидат 1 обзора №5 — документ без редактора: `PhysicalConstants` → `DocumentConstants` (только допуск хита тела), 6 констант взаимодействия в `EditorSettings`, привязка к сетке вне библиотеки (только `SnapHelper`), дубль `MicronsPerMm` устранён, поведение байт-в-байт
- ✅ **#108–#109:** Кандидат 8 обзора №4 — мёртвые швы deletion sweep: удалены callback-параметры ZoomPanManager, мёртвые зависимости EditorViewModel (`_templateService`, `_gridNodeGenerator`-поле), ITextToolSettings, DI-регистрация IFontMetrics, 4 конвертера без биндингов, ValidateRotation; ресурсы конвертеров — каждый класс ровно один раз в корневом словаре; контракт Автосохранения типизирован (механизм сохранён), поведение байт-в-байт
- ✅ **#103–#104:** Кандидат 4 обзора №4 — раскладка маркеров один модуль MarkerLayout: каталог/позиции/hit с допуском #82/классификация/курсоры; HitTestHelper — хит по телу, ResizeMath — чистая математика, ResizeTool — единственный ResizeState, IEditorContext 26→25, поведение байт-в-байт
- ✅ **#98–#99:** Кандидат 3 обзора №4 — CommandHistory единолично владеет грязностью: Push/Undo/Redo стреляют markDirty, команды делегат не носят, компенсация в VM удалена, IEditorContext 27→26, поведение байт-в-байт
- ✅ **#93–#94:** Кандидат 2 обзора №4 — preview-сейм: PreviewManager единственный источник истины preview-состояния, ре-ассайн-трюк удалён структурно (рендерер подписан на INPC preview-объекта), IEditorContext 29→27, визуально байт-в-байт
- ✅ **#88–#89:** Кандидат 1 обзора №4 — один конвейер рендеринга: модуль правил RenderRules, канвас-конвертеры — тонкие делегаты (байт-в-байт), 2 дефекта anchor'ов текста в preview и печати устранены, дубли правил удалены бесследно
- ✅ **#84–#85:** Кандидат 7 обзора №4 — пан — жест роутера: PanTool удалён целиком, IEditorContext 30→29, первый ADR проекта, live-рассинхронизация состояний устранена структурно, поведение байт-в-байт
- ✅ **#75–#76:** Кандидат 1, срез 1 — рамка выделения через узкий шов: 7 свойств IEditorContext → методы SetSelectionBox/ClearSelectionBox (интерфейс 35→30 членов), поведение байт-в-байт
- ✅ **#70–#71:** Кандидат 7 — deletion-пара: «Вписать в экран» на живом viewport (fallback 800×600 удалён), рамка выделения через полиморфный GetBoundingBox
- ✅ **#65–#66:** Настройки приложения — только типизированный интерфейс: ISettingsService = Load()/Save(AppSettings), строковый Get/Set удалён, AppSettings в Models
- ✅ **#53–#57:** ToolRegistry — типизированная идентичность инструментов ToolKind, строковые карты и ITool.Name удалены
- ✅ **#45–#48:** Глубокая база панелей свойств ObjectPropertiesViewModel<TObject> + миграция Line/Rectangle/Text sub-VM
- ✅ **Sprint 56-57:** Colors (V-005), Half-formats, Settings UI, MultiLine, Library
- ✅ **Sprint 52-55:** Free rotation, IDateTimeProvider, DialogService, 1599+ тестов
- ✅ **Sprint 45-51:** PanTool fix, ContextMenu, Grid, Clipboard, Text/Fonts
- ✅ **Sprint 42-44:** StrokeThickness, live-обновление панели свойств, ResizeTool dispatch
- ✅ **Sprint 38-41:** INPC моделей, Rectangle border-band hit-test, keyboard shortcuts

---

## 📅 ПЛАН-ГРАФИК

```
12-14 недель (Q2 2026)
├─ Недели 1-2:  Инициализация и архитектура
├─ Недели 3-4:  Модели и сервисы
├─ Недели 5-7:  Редактор и инструменты
├─ Недели 8-9:  Панели и Undo/Redo
├─ Недели 10-11: UI/UX и темы
└─ Недели 12-14: Тестирование, печать, релиз
```

---

## 👥 КОМАНДА

| Роль | Количество | Примечание |
|------|------------|------------|
| Team Lead / Architect | 1 | Проектирование, код-ревью |
| WPF Developer | 1+ | Разработка UI и логики |
| QA Engineer | 1 | Тестирование |
| Business Analyst | 1 | Сопровождение требований |

**Вакансии:** Открыты (GitHub Issues)

---

## 📊 СТАТУС РАЗРАБОТКИ

### Бэклог продукта

| Приоритет | Задач | Story Points |
|-----------|-------|--------------|
| P0 (Must Have) | 11 | 59 SP |
| P1 (Should Have) | 10 | 37 SP |
| P2 (Could Have) | 6 | 15 SP |
| **Всего** | **27** | **111 SP** |

### Прогресс

```
Этап 1: [████████████████████] 100% ✅
Спринт 1: [████████████████████] 100% ✅
Спринт 2: [████████████████████] 100% ✅
Спринт 3: [████████████████████] 100% ✅
Спринт 4: [████████████████████] 100% ✅
Спринт 5: [████████████████████] 100% ✅
Спринт 6: [████████████████████] 100% ✅
Спринт 7-11: [████████████████████] 100% ✅ (стабилизация)
Спринт 12: [████████████████████] 100% ✅
Спринт 13: [████████████████████] 100% ✅
Спринт 14: [████████████████████] 100% ✅ (ориентация листа)
```

### История спринтов

| Спринт | Фокус | Статус |
|--------|-------|--------|
| 38-41 | LineType, HitTest, Selection, Drag/Text INPC | ✅ |
| 42-44 | StrokeThickness, ResizeTool, PropertiesPanel live | ✅ |
| 45-51 | PanTool fix, ContextMenu, Grid, Clipboard, Text/Fonts | ✅ |
| 52-55 | Free rotation, IDateTimeProvider, DialogService, Coverage | ✅ |
| 56-57 | Colors (V-005), Half-formats, Settings UI, MultiLine, Library | ✅ |
| **Итого** | **Все 44 FR Этапа 1 выполнены** | ✅ |

### Структура проекта (текущая)

```
src/
├── Directory.Build.props              ✅
├── DotElectric.TemplateEditor.slnx    ✅
├── DotElectric.TemplateEditor/        ✅
│   ├── App.xaml / App.xaml.cs         ✅ (DI, Serilog, Mutex, обработчики)
│   ├── MainWindow.xaml / .cs          ✅ (каркас UI)
│   ├── Models/Objects/                ✅ (папки созданы)
│   ├── ViewModels/Templates/          ✅ (папки созданы)
│   ├── Views/Templates/               ✅ (папки созданы)
│   ├── Services/                      ✅ (папки созданы)
│   ├── Tools/                         ✅ (папки созданы)
│   ├── Commands/                      ✅ (папки созданы)
│   ├── Helpers/                       ✅ (папки созданы)
│   ├── Converters/                    ✅ (папки созданы)
│   ├── Resources/Styles/Fonts/Icons/  ✅ (папки созданы)
│   └── DotElectric.TemplateEditor.csproj ✅
└── DotElectric.TemplateEditor.Tests/  ✅
    ├── Models/Objects/                ✅ (папки созданы)
    ├── ViewModels/Templates/          ✅ (папки созданы)
    ├── Services/                      ✅ (папки созданы)
    ├── Tools/                         ✅ (папки созданы)
    ├── Commands/                      ✅ (папки созданы)
    ├── Helpers/                       ✅ (папки созданы)
    └── DotElectric.TemplateEditor.Tests.csproj ✅
```

### Подключённые NuGet пакеты

**DotElectric.TemplateEditor:**
| Пакет | Версия | Назначение |
|-------|--------|------------|
| CommunityToolkit.Mvvm | 8.4.2 | MVVM (ObservableObject, RelayCommand) |
| MaterialDesignThemes | 5.3.2 | Material Design стили и иконки |
| MaterialDesignColors | 5.3.2 | Цветовые палитры Material Design |
| Microsoft.Extensions.DependencyInjection | 10.0.9 | DI-контейнер |
| Microsoft.Extensions.Hosting | 10.0.9 | Host builder, Serilog интеграция |
| Serilog | 4.3.1 | Логирование |
| Serilog.Sinks.File | 7.0.0 | Rolling file (30 дней) |
| Serilog.Extensions.Hosting | 10.0.0 | UseSerilog() для IHostBuilder |

**DotElectric.TemplateEditor.Tests:**
| Пакет | Версия |
|-------|--------|
| xunit.v3 | 3.2.2 |
| xunit.runner.visualstudio | 3.1.5 |
| Moq | 4.20.72 |
| coverlet.collector | 10.0.1 |
| Microsoft.NET.Test.Sdk | 18.7.0 |

---

## 🔗 ССЫЛКИ

### ГОСТ

- [ГОСТ 2.301-2008](https://docs.cntd.ru/document/120104495) — Форматы
- [ГОСТ 2.104-2006](https://docs.cntd.ru/document/120102895) — Основная надпись
- [ГОСТ 2.105-95](https://docs.cntd.ru/document/120102894) — Общие требования
- [ГОСТ 2.701-2008](https://docs.cntd.ru/document/120104493) — Виды и типы документов

### Ресурсы

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [MVVM Pattern](https://docs.microsoft.com/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)

---

## 📞 КОНТАКТЫ

**Заказчик:** [Контактная информация]  
**Разработчик:** [Контактная информация]

---

## 📄 ЛИЦЕНЗИЯ

[Информация о лицензии]

---

**Последнее обновление:** 21.08.2026 — Кандидат 7 обзора №5 (#167, тикет #168, PR #170): предпросмотр печати через шов диалогов и типизированная ориентация меню. `MainViewModel.PreviewPrint` больше не создаёт WPF-окно напрямую — показ через `IDialogHostService`: тонкая `PrintPreviewViewModel` (документ печати + имя вкладки) + одна строка маппинга в определителе «модель → окно», окно принимает модель через конструктор; импорт слоя Views и статический доступ к приложению удалены из модели представлений. Владелец диалогов по умолчанию — главное окно: инъектируемый провайдер в WPF-реализации шва (прецедент `WpfDispatcherService`), дефолт диспетчеробезопасен; объявленное в XAML CenterOwner начинает работать у всех трёх окон. Пункты меню несут типизированную ориентацию (`NewSheetOrientationEntry(Header, Format, Orientation)`), команда принимает пункт целиком; фасад — один `CreateNewTab` с типизированной ориентацией и побайтными цепочками запасных значений; парсер суффиксов удалён целиком; `/1000` в заголовках — через `Coordinate.FormatMm`; схема settings.json не меняется; XML-доки приведены к коду; CONTEXT.md — статья «Предпросмотр печати». Поведение байт-в-байт, кроме декларированного отклонения: центрирование Настроек и диалога произвольного размера на главном окне. Тесты 2625 (2655 − 33 теста парсера + 3 пина; распределение: приложение 2044 + Document.Tests 454 + Sheets.Tests 127); coverage 92.05%





