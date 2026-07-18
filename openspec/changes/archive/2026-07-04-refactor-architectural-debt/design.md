## Context

Текущая кодовая база DotElectric Template Editor (Этап 1 завершён, 1780 тестов) содержит системный архитектурный долг в 5 слоях. Долг накоплен естественным образом в процессе спринтов 1–57 и рефакторинга R1–R4. Ключевые проблемные зоны:

- **PropertiesViewModel** (649 строк) — 3 параллельных реализации (Line/Rectangle/Text) с 30-case INPC-switch
- **Команды** (8 типов, 635 строк) — неоднородная иерархия: есть generic (`ChangePropertyCommand<T>`) и специализированные (`RotateObjectCommand`, `MoveObjectCommand`, `CustomResizeCommand`) с дублирующей логикой
- **Конвертеры** (36 файлов, 937 строк) — дубликаты (`InverseBoolConverter` = `NotConverter`), избыточная раздробленность
- **Инфраструктура** — `IAutosaveTab` определён в сервисе, а не рядом с потребителем; `async void` без обработки ошибок
- **Тесты** — 6 WPF-сервисов без покрытия, 7 файлов <80%

Все изменения — исключительно рефакторинг. Ни одно изменение не меняет поведение системы.

## Goals / Non-Goals

**Goals:**
- Сократить общий объём кода на ~542 строки без потери функциональности
- Устранить дублирование в PropertiesViewModel (3× одинаковые блоки → 3 per-type VM)
- Упростить иерархию команд (8 типов → 5)
- Устранить мёртвый код и дубликаты конвертеров
- Поднять coverage с ~82% до ~87%
- Зафиксировать IAutosaveTab в правильном месте

**Non-Goals:**
- Не меняется модель данных (TemplateObjectBase, Line, Rectangle, Text)
- Не меняется API инструментов (ITool, IEditorContext)
- Не меняется DI-регистрация (кроме удаления мёртвых registrations)
- Не добавляются новые пользовательские возможности
- Не меняется формат файла .tdel
- Не переписывается PropertiesPanelContent.xaml целиком — только замена 3 StackPanel на ContentControl

## Decisions

### D1: ChangePropertyCommand<T> — второй конструктор

**Решение:** Добавить конструктор `ChangePropertyCommand(T oldValue, Action<T> setter, T newValue, string propertyName, Action? markDirty = null)`

**Зачем:** Текущий конструктор вычисляет `_oldValue = getter()` при создании. Для MoveObjectCommand в SelectTool объект уже перемещён к моменту создания команды — getter() вернёт новое значение. Явная передача oldValue решает эту проблему без создания отдельного класса команды.

**Альтернатива:** Оставить MoveObjectCommand. Отвергнуто — 80 строк ради одной разницы в конструкторе.

### D2: MoveObjectCommand → ChangePropertyCommand<(long X, long Y)>

**Решение:** Заменить на `ChangePropertyCommand<(long,long)>` с getter `() => (obj.MicronsX, obj.MicronsY)` и setter `v => obj.Move(v.X, v.Y)`.

**Почему `Move()`, а не раздельные `MicronsX`/`MicronsY`:** `Line.Move()` сдвигает оба конца линии (start + end) на дельту. Раздельная установка `MicronsX`/`MicronsY` сломала бы Line. Tuple сохраняет атомарность.

### D3: PropertiesViewModel — стратегия разбивки

**Решение:** Не наследование, а композиция через partial class-заглушки:

```
PropertiesViewModel (база)
├── подписка на SelectedObjects.CollectionChanged
├── подписка на SelectedObject.PropertyChanged → диспетчеризация к sub-VM
├── SetProperty<T>() — generic helper (общий)
├── ChangeFromMmString() — общий
├── IDisposable
└── ActiveLineVm / ActiveRectVm / ActiveTextVm (lazy-loaded)
```

Каждая sub-VM — отдельный `ObservableObject`, подписывается на INPC своего типа, содержит свойства и команды только своего типа.

**XAML:** `ContentControl Content="{Binding ActiveLineVm}"` с implicit DataTemplate по типу.

**Альтернатива:** Единый PropertiesViewModel с partial class по типу. Отвергнуто — partial class требует все части в одном namespace и не решает проблему type-guard switch.

### D4: Converters — не сливать всё в один файл

**Решение:** Только целевое удаление дубликатов и слияние очевидных групп. Координатные конвертеры (11 файлов IMultiValueConverter) остаются раздельными — их сигнатуры слишком разные.

**Альтернатива:** Один общий Converters.cs. Отвергнуто — файл получится 400+ строк с разнородной логикой.

### D5: STA-тесты для WPF-сервисов

**Решение:** Использовать существующий `WpfContext` (создан в Sprint STA) для всех WPF-зависимых тестов.

**WpfDialogFileService** — требует mocking `OpenFileDialog`/`SaveFileDialog`. Создать `Mock<IDialogFileService>` аналогично существующим тестам в FileServiceTests.

**WpfDialogHostService** — создаёт WPF-окна. Тестировать через WpfContext + проверку создания окна нужного типа.

**WpfDispatcherService** — тестировать через WpfContext с проверкой `Application.Current.Dispatcher`.

**PrintDialogFactory** — самый простой: проверить создание `PrintDialogWrapper`.

**ThemeDictionaryManager** — WpfContext + `Application.Current.Resources.MergedDictionaries`.

## Risks / Trade-offs

| Риск | Влияние | Митигация |
|------|---------|-----------|
| MoveObjectCommand → tuple: поломка Line.Move() | Критическое (Line не двигается) | Тесты SelectTool 18/18; отдельный тест на MoveSelected с Line |
| PropertiesViewModel split: XAML не найдёт биндинги | Панель свойств пуста | Проверить 50 тестов PropertiesViewModel; визуальная проверка |
| RotateObjectCommand → ChangePropertyCommand<int>: потеря нормализации угла | Некорректный угол после Undo | Нормализация `% 360` в сеттере RotationAngle (уже есть в модели) |
| Converters rename: App.xaml не найдёт ресурс | Ошибка XAML в runtime | App.xaml — удалить старые registration, добавить новые; проверить 156 тестов конвертеров |
| WpfDialogHostService тесты: ShowDialog() требует STA | Ошибка InvalidOperationException | WpfContext.Execute() — уже проверенный паттерн (45 STA-тестов) |

## Open Questions

— Нет. Все решения приняты на основе анализа кодовой базы и 61 Common Mistakes из AGENTS.md.
