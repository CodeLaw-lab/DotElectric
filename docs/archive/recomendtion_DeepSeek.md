# Экспертное мнение: DotElectric TemplateEditor

**Аналитик:** DeepSeek  
**Дата:** 20.05.2026  
**Оценка:** 8/10

---

## Общая архитектура

| Компонент | Технология |
|---|---|
| Платформа | WPF .NET 10, net10.0-windows |
| Архитектура | MVVM + DI (Microsoft.Extensions.DependencyInjection) |
| ViewModel | CommunityToolkit.Mvvm ([ObservableProperty], [RelayCommand]) |
| Тесты | xUnit v3 + Moq, 1507 тестов, ~80% coverage |
| Логирование | Serilog (rolling file, 30 дней) |
| UI | MaterialDesignThemes 5.3.1 |
| Сборка | GitHub Actions CI/CD |

---

## Сильные стороны

### 1. Архитектура
- **Чистый MVVM** — source generators CommunityToolkit.Mvvm, никакого code-behind
- **DI-контейнер** — 20+ синглтонов, все сервисы через интерфейсы для тестирования
- **Фиксированные координаты** — `long` в микронах (1мм = 1000мкм), никакого floating-point дрейфа
- **Инверсия Y** строго на границе WPF→Model в `EditorCanvasBehavior.ToModelPoint()`
- ViewModel и сервисы ничего не знают о WPF-координатах

### 2. Управление состоянием
- **CommandHistory** (50 уровней Undo/Redo) с `BatchCommand` для композитных операций
- `ChangePropertyCommand<T>` — generic, переиспользуемый
- **ToolManager** со стеком push/pop для временного переключения инструментов
- **7 менеджеров** в EditorViewModel: ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar

### 3. Тестирование
- 1507 тестов, 0 failures (Sprint 31)
- 3-tier: unit → mock (Moq) → integration (real .tdel round-trip)
- Backward compatibility тесты для старого формата
- Все сервисы имеют интерфейсы — полная мокируемость

### 4. Инфраструктура
- Serilog с rolling file, retained 30 дней
- Mutex на единственный экземпляр
- Глобальные обработчики AppDomain/Dispatcher/Task
- `GridNodesLayer` через `DrawingContext` (200× быстрее Ellipse)
- CI/CD: build → test → coverage → static analysis

### 5. Качество кода
- XML-комментарии на русском к каждому классу/методу
- Единый `EditorConstants.cs` — ни одного magic number
- `ArgumentNullException` на всех публичных конструкторах
- `IDisposable` на всех ViewModel/сервисах с жизненным циклом
- Thread-safe: `SemaphoreSlim` в AutosaveService, `lock` в SettingsService

---

## Проблемы и технический долг

| # | Проблема | Серьёзность | Описание |
|---|---|---|---|
| 1 | **EditorViewModel — 987 строк** | Высокая | 30 pass-through свойств, 20 relay-команд, генерация сетки. Рекомендуется <500 строк. |
| 2 | **WpfCommands/ToolCommands.cs — мёртвый код** | Средняя | 176 строк. EditorCanvasBehaviour полностью заменил. |
| 3 | **ClearSelectionCommand — мёртвый код** | Низкая | 41 строка. Очистка делается через SelectionManager. |
| 4 | **ResizeObjectCommand vs CustomResizeCommand** | Средняя | Дублирование. CustomResizeCommand предпочтительнее. |
| 5 | **async void в командах** | Средняя | Вместо `IAsyncRelayCommand` из CommunityToolkit.Mvvm |
| 6 | **Нет InternalsVisibleTo** | Низкая | Тесты без доступа к internal членам |
| 7 | **Duplicate Change*FromString** | Средняя | 20 идентичных команд в PropertiesViewModel |
| 8 | **Нет CancellationToken** | Средняя | Save/Load/Autosave без отмены |
| 9 | **SettingsService — string dispatch** | Низкая | `Get<T>()` через switch по строке |
| 10 | **PanTool — double для координат** | Средняя | Прямое деление вместо `Coordinate.ToMm()` |
| 11 | **Нет Shape-абстракции** | Низкая | HitTestHelper с 3 ветвистыми методами |
| 12 | **Нет keyboard-роутинга через ITool** | Средняя | Delete/Nudge/Ctrl+C/V в VM напрямую |

---

## Рекомендации

### Приоритет 1 (срочно)
1. Удалить мёртвый код: `WpfCommands/ToolCommands.cs`, `ClearSelectionCommand`
2. Объединить `ResizeObjectCommand` и `CustomResizeCommand` в один класс

### Приоритет 2 (важно)
3. Декомпозировать EditorViewModel:
   - Вынести `RefreshGridNodes()` в GridHelper
   - Создать `DirtyStateManager` для MarkDirty/ClearDirty/UpdateDisplayName
   - Цель: <500 строк
4. Заменить `async void` на `IAsyncRelayCommand`
5. Исправить `PanTool` — использовать `Coordinate.ToMm()` вместо прямого деления

### Приоритет 3 (улучшения)
6. Убрать дублирование `Change*FromString` — один generic-метод с парсингом mm
7. Добавить keyboard-интерфейс в `ITool` для роутинга клавиатурных команд
8. Добавить `CancellationToken` в TemplateService (Save/Load)
9. Добавить виртуальные методы `ContainsPoint(PointMicrons)` и `GetBoundingBox()` на `TemplateObjectBase`
10. Выделить TemplateMapper из TemplateService (DTO→Domain mapping)
11. Заменить string-based dispatch в SettingsService на `Dictionary<string, JsonElement>`
12. Добавить `[InternalsVisibleTo]` для тестов

---

## Итог

Проект находится в хорошем состоянии для коммерческого CAD-редактора под ГОСТ. Архитектура продумана, кодовая база чистая, тестовое покрытие отличное. Основные риски — разрастание EditorViewModel (987 строк), мёртвый код и дублирование команд свойств. После устранения этих проблем проект готов к Этапу 2.
