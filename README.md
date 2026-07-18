# DotElectric Template Editor

**Статус:** ✅ Этап 1 ЗАВЕРШЁН (все 44 FR выполнены)
**Тестов:** 1760, 0 сбоев, 1 предопределённый skip
**Сборка:** 0 errors, 0 warnings
**Покрытие:** ~82% line-rate
**Последнее обновление:** 02.07.2026 — Sprint 57: Настройки (UI), Цвета объектов, Half-форматы, MultiLine/TextAlignment, Библиотека шаблонов

[![CI](https://github.com/your-org/dotElectric/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/dotElectric/actions)
[![Coverage](https://img.shields.io/badge/coverage-82%25-brightgreen)](https://github.com/your-org/dotElectric/actions)

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
| **1** | Редактор шаблонов листов | ✅ ЗАВЕРШЁН (1760 тестов, 100%) | Q2 2026 |
| **2** | Редактор УГО | ⚪ Запланирован | Q3 2026 |
| **3** | Работа с БД компонентов | ⚪ Запланирован | Q4 2026 |
| **4** | Главный редактор схем | ⚪ Запланирован | Q1 2027 |

---

## 📁 СТРУКТУРА РЕПОЗИТОРИЯ

```
dotElectric/
├── docs/                           # Документация
│   ├── 01_Техническое_задание_Этап1.md
│   ├── 02_User_Stories_Этап1.md
│   ├── 03_Спецификация_требований_Этап1.md
│   ├── 04_План_разработки_Этап1.md
│   ├── 05_Руководство_пользователя_черновик.md
│   ├── 06_Оставшиеся_вопросы.md
│   ├── 07_План_реализации_Этап1.md
│   ├── 08_Подробный_план_реализации.md
│   ├── 09_UI_решения.md
│   ├── 10_Отчёт_Спринт1.md
│   ├── 11_TODO_Спринт2.md
│   ├── 12_Отчёт_Модели.md
│   ├── 13_Отчёт_Команды.md
│   ├── 13_Отчёт_Helpers.md
│   ├── 14_Отчёт_Сервисы.md
│   ├── 15_Отчёт_Спринт3a.md
│   ├── 16_Отчёт_Спринт3b.md
│   ├── 17_Отчёт_Спринт3c_Интеграция.md
│   ├── 18_Отчёт_Спринт4_Темы_и_StatusBar.md
│   ├── 19_Статус_проекта.md
│   └── 20_Отчёт_Спринт6_UI_Fixes.md (+NEW)
├── src/                            # Исходный код
│   ├── DotElectric.TemplateEditor/
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── MainWindow.xaml / .cs
│   │   ├── Models/Objects/
│   │   ├── ViewModels/
│   │   ├── Views/
│   │   ├── Services/
│   │   ├── Tools/
│   │   ├── Commands/
│   │   ├── Helpers/
│   │   ├── Converters/
│   │   ├── Behaviors/
│   │   ├── WpfCommands/
│   │   └── Resources/Styles/
│   └── DotElectric.TemplateEditor.Tests/
│       ├── Models/Objects/
│       ├── ViewModels/
│       ├── Services/
│       ├── Tools/
│       ├── Commands/
│       ├── Helpers/
│       └── Converters/
├── templates/                      # Шаблоны (создаётся при установке)
├── Answers.txt                     # Ответы заказчика (этап 1)
├── Answers2.txt                    # Ответы заказчика (этап 1, дополн.)
├── Answers3.txt                    # Ответы заказчика (этап 1, дополн.)
├── Answers4.txt                    # Ответы заказчика (этап 1, дополн.)
└── README.md                       # Этот файл
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
| [План разработки](docs/04_План_разработки_Этап1.md) | Дорожная карта и спринты |
| [Руководство пользователя](docs/05_Руководство_пользователя_черновик.md) | Инструкция по использованию |

### Для разработчиков

| Документ | Описание |
|----------|----------|
| [Спецификация требований](docs/03_Спецификация_требований_Этап1.md) | Детальное описание архитектуры и API |
| [Анализ техлида + Sprint 23](docs/39_Анализ_техлида_Sprint23.md) | 16 задач (6 P0, 6 P1, 4 P2), 39 SP |

---

## 🔧 ТРЕБОВАНИЯ К РАЗРАБОТКЕ

### Системные требования

- Windows 10/11
- Visual Studio 2022 или JetBrains Rider
- .NET 10 SDK

### Сборка проекта

```bash
# Клонирование репозитория
git clone https://github.com/your-org/dotElectric.git
cd dotElectric

# Сборка
dotnet build src/DotElectric.TemplateEditor.slnx

# Запуск
dotnet run --project src/DotElectric.TemplateEditor/DotElectric.TemplateEditor

# Тесты (покрытие >= 70%)
dotnet test src/DotElectric.TemplateEditor/DotElectric.TemplateEditor.Tests --collect:"XPlat Code Coverage"
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
- Шаг 1–50 мм, отображение узлов и линий
- Привязка к сетке
- MinPixelSpacing — скрытие сетки при высокой плотности

#### Печать
- Ctrl+P — системный диалог печати
- Прямая печать (без предпросмотра)

#### UI
- Material Design, темы Light/Dark (F9, с сохранением)
- StatusBar (формат, zoom, grid/snap toggle, clipboard feedback)
- Библиотека шаблонов (импорт .tdel, удаление)
- Контекстные меню (холст, вкладки)
- Keyboard shortcuts (все, включая русскую раскладку)

### Тестирование

- ✅ **1760 тестов**, 0 сбоев, 1 предопределённый skip
- ✅ **Покрытие:** ~82% line-rate
- ✅ **Сборка:** 0 errors, 0 warnings
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
| MaterialDesignThemes | 5.3.1 | Material Design стили и иконки |
| MaterialDesignColors | 5.3.1 | Цветовые палитры Material Design |
| Microsoft.Extensions.DependencyInjection | 10.0.5 | DI-контейнер |
| Microsoft.Extensions.Hosting | 10.0.5 | Host builder, Serilog интеграция |
| Serilog | 4.3.1 | Логирование |
| Serilog.Sinks.File | 7.0.0 | Rolling file (30 дней) |
| Serilog.Extensions.Hosting | 10.0.0 | UseSerilog() для IHostBuilder |

**DotElectric.TemplateEditor.Tests:**
| Пакет | Версия |
|-------|--------|
| xunit.v3 | 3.2.2 |
| xunit.runner.visualstudio | 3.1.5 |
| Moq | 4.20.72 |
| coverlet.collector | 8.0.1 |
| Microsoft.NET.Test.Sdk | 18.3.0 |

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

**Последнее обновление:** 02.07.2026 — Sprint 57: Этап 1 завершён





