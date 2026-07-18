# ОТЧЁТ: SPRINT 6 — ИСПРАВЛЕНИЯ UI/UX

**Дата:** 07.04.2026
**Автор:** AI-ассистент DotElectric
**Статус:** ✅ ЗАВЕРШЁН (6 задач, 801 тест, 0 сбоев, 0 предупреждений)

---

## ОБЩАЯ СВОДКА

| Параметр | Значение |
|----------|----------|
| **Спринт** | 6 — Исправления UI/UX |
| **Задач выполнено** | 6 из 6 ✅ |
| **Тестов** | 801 (0 сбоев) |
| **Покрытие** | 60.3% line / 47.5% branch |
| **Сборка** | 0 ошибок, 0 предупреждений |

---

## ЗАДАЧИ СПРИНТА

### Задача 1: Ctrl+N не работает

**Проблема:** Горячая клавиша Ctrl+N не создавала новый шаблон.

**Корневая причина:** В `MainWindow.xaml` отсутствовал `<KeyBinding>` для Ctrl+N. `InputGestureText="Ctrl+N"` на MenuItem — это только визуальная подсказка, НЕ реальная привязка клавиши.

**Решение:** Добавлен `<KeyBinding Key="N" Modifiers="Control">` в `Window.InputBindings`.

**Файлы:**
- `MainWindow.xaml` — добавлен KeyBinding

---

### Задача 2: Ошибка привязки к readonly-свойствам

**Проблема:** При открытии вкладки появлялась ошибка: "Привязка типа TwoWay или OneWayToSource не может работать с доступным только для чтения свойством LineStartX..."

**Корневая причина:** 4 binding к readonly-свойствам (LineTypeValue, RectLineType, TextTypeValue, TextContent) не имели `Mode=OneWay`. WPF по умолчанию использует TwoWay для ComboBox.SelectedItem и TextBox.Text, что вызывает ошибку при попытке записать в свойство без setter.

**Решение:** Добавлен `Mode=OneWay` для всех binding к readonly-свойствам:
- `LineTypeValue` (ComboBox)
- `RectLineType` (ComboBox)
- `TextTypeValue` (ComboBox)
- `TextContent` (TextBox)

**Файлы:**
- `PropertiesPanelContent.xaml` — 4 binding исправлены

---

### Задача 3: Узкая белая полоса вместо вкладок

**Проблема:** При создании нового шаблона вместо содержимого вкладки отображалась узкая белая полоса с надписью.

**Корневая причина:** Кастомный `TabControl.Template` был полностью переопределён. В нём отсутствовали `TabPanel` (контейнер для заголовков) и `ContentPresenter` (содержимое активной вкладки). В результате TabControl рендерил только заголовки TabItem, но не содержимое.

**Решение:** Полностью отказались от кастомного ControlTemplate. Заглушка и TabControl размещены как **отдельные элементы Grid** с переключением через `Visibility`:
- `StackPanel` — заглушка при 0 вкладок (`ZeroToVisibilityConverter`)
- `Border` + `TabControl` — стандартный TabControl (`NonZeroToVisibilityConverter`)

**Файлы:**
- `MainWindow.xaml` — убран кастомный Template, добавлены два элемента Grid

---

### Задача 4: Кнопки меню и Toolbar не работают

**Проблема:** Многие MenuItem и Button в меню не имели Command binding — они были просто декоративными элементами.

**Затронутые элементы (16 штук):**
- Печать, Выход
- Копировать, Вставить, Удалить, Выделить все
- Сетка, Привязка к сетке, Увеличить, Уменьшить, Переключить тему
- Линия, Прямоугольник, Текст, Выделение

**Решение:**
1. Добавлены 6 новых команд в ViewModel:
   - `ZoomInCommand`, `ZoomOutCommand`
   - `ToggleGridCommand`, `ToggleSnapCommand`
   - `PrintCommand`, `ExitCommand`
2. Добавлены Command binding для всех 16 MenuItem/Button

**Файлы:**
- `MainViewModel.cs` — PrintCommand, ExitCommand
- `EditorViewModel.cs` — ZoomInCommand, ZoomOutCommand, ToggleGridCommand, ToggleSnapCommand
- `MainWindow.xaml` — 16 Command binding

---

### Задача 5: Горячие клавиши не работают

**Проблема:** Большинство горячих клавиш работали только когда EditorCanvas имел фокус.

**Корневая причина:** KeyBinding были объявлены только в `EditorCanvas.InputBindings`. Когда фокус на другом элементе (PropertiesPanel, Menu, TemplateLibraryView) — клавиши не работали.

**Решение:** Перенесены ВСЕ глобальные горячие клавиши в `Window.InputBindings` (24 штуки):

| Клавиша | Команда |
|---------|---------|
| Ctrl+N | Новый шаблон |
| Ctrl+O | Открыть файл |
| Ctrl+S | Сохранить |
| F12 | Сохранить как |
| Ctrl+P | Печать |
| Ctrl+Z | Отменить |
| Ctrl+Y | Повторить |
| Ctrl+C | Копировать |
| Ctrl+V | Вставить |
| Ctrl+A | Выделить все |
| Delete | Удалить |
| F7 | Переключить сетку |
| F8 | Переключить привязку |
| F9 | Переключить тему |
| Ctrl+0 | Вписать в экран |
| **Ctrl++** | Увеличить масштаб |
| **Ctrl+-** | Уменьшить масштаб |
| L | Рисовать линию |
| U | Рисовать прямоугольник |
| X | Рисовать текст |
| H | Инструмент выделения |
| R | Вращать по часовой |
| Shift+R | Вращать против часовой |
| ↑/↓/←/→ | Сдвиг выделенного объекта |

**Файлы:**
- `MainWindow.xaml` — 24 KeyBinding
- `EditorCanvas.xaml` — удалены дублирующие KeyBinding (оставлены только Enter/Escape для inline editing)

---

### Задача 6: ToggleGrid/ToggleSnap не обновляли UI

**Проблема:** После переключения сетки/привязки через команды, StatusBar и UI не обновлялись.

**Корневая причина:** Команды меняли `GridSettings.Enabled`/`GridSettings.SnapEnabled`, но не вызывали `OnPropertyChanged` для свойств-обёрток (`StatusBarGridEnabled`, `StatusBarSnapEnabled`), к которым привязан UI.

**Решение:** Добавлен `OnPropertyChanged(nameof(StatusBarGridEnabled))` и `OnPropertyChanged(nameof(StatusBarSnapEnabled))` в команды.

**Файлы:**
- `EditorViewModel.cs` — ToggleGridCommand, ToggleSnapCommand

---

## ДОПОЛНИТЕЛЬНЫЕ ИСПРАВЛЕНИЯ

### Глобальные конвертеры в App.xaml

**Проблема:** Конвертеры были объявлены в `Window.Resources`, но не находились из `ControlTemplate` и других UserControl.

**Решение:** Перенесены все 16 конвертеров в `App.xaml` (Application.Resources) для глобальной доступности. Добавлены алиасы для обратной совместимости (`BoolToVisibility` + `BoolToVisibilityConverter`).

**Файлы:**
- `App.xaml` — 16 конвертеров
- `MainWindow.xaml` — удалены дубликаты из Window.Resources

### Кнопка закрытия вкладки

**Добавлено:** Кнопка ✕ в заголовок каждой вкладки (LightTheme.xaml + DarkTheme.xaml):
- Появляется при наведении мыши (DataTrigger на `IsMouseOver`)
- Всегда видна для активной вкладки (DataTrigger на `IsSelected`)
- Привязана к `MainViewModel.CloseTabCommand`
- CommandParameter = `EditorViewModel` выбранной вкладки

**Файлы:**
- `LightTheme.xaml` — TabItem ControlTemplate
- `DarkTheme.xaml` — TabItem ControlTemplate

---

## ИСПРАВЛЕНИЯ ЗАКРЫТИЯ ВКЛАДОК (08.04.2026)

### Исправление 1: Кнопка закрытия не работала

**Проблема:** Кнопки закрытия вкладок не работали — вкладки не закрывались при клике на ✕.

**Корневая причина:** Несоответствие имён команд:
- В стилях тем (`LightTheme.xaml` / `DarkTheme.xaml`) была привязка к `CloseTabCommand`
- В `MainViewModel.cs` метод назывался `CloseTabAsync` → генерировалось `CloseTabAsyncCommand`
- Несоответствие → command = null, кнопка не работала

**Решение:** Переименован метод `CloseTabAsync` → `CloseTab` в `MainViewModel.cs` (строка 219). CommunityToolkit.Mvvm теперь генерирует `CloseTabCommand`, binding работает корректно.

**Файлы:**
- `MainViewModel.cs` — переименование `CloseTabAsync` → `CloseTab`, исправлен вызов в `CloseAllTabsAsync`

---

### Исправление 2: Не было крестика и контекстного меню

**Проблема:** Крестик закрытия не отображался, контекстное меню не появлялось.

**Корневая причина:** `TabControl.ItemContainerStyle` в `MainWindow.xaml` полностью переопределял стиль TabItem из тем (Light/Dark). Без `BasedOn` создавался новый стиль с нуля, в котором был только `Header` — все остальные настройки (крестик, контекстное меню, индикатор несохранённых) терялись.

**Решение:** 
1. Добавлен `BasedOn="{StaticResource {x:Type TabItem}}"` — стиль наследуется из темы
2. Контекстное меню перенесено в `ItemContainerStyle` — определяется один раз в MainWindow.xaml
3. Стили тем очищены — убран дублирующий ContextMenu, добавлен явный `x:Key="{x:Type TabItem}"`

**Файлы:**
- `MainWindow.xaml` — добавлен `BasedOn`, перенесён ContextMenu в ItemContainerStyle
- `LightTheme.xaml` — убран дублирующий ContextMenu, добавлен `x:Key`
- `DarkTheme.xaml` — убран дублирующий ContextMenu, добавлен `x:Key`

---

### Исправление 3: Крестик не менял цвет при наведении

**Проблема:** При наведении мыши на крестик он не изменял свой цвет (должен быть красный фон).

**Корневая причина:** Trigger в `Style` не мог изменить `Background`, потому что `ControlTemplate` уже определял `Border` с жёстко заданным `Background="Transparent"`.

**Решение:**
1. Добавлен `x:Name="buttonBorder"` на `Border` в ControlTemplate
2. Использован `Background="{TemplateBinding Background}` — Background кнопки применяется через TemplateBinding
3. Hover-эффект перенесён в `ControlTemplate.Triggers` с правильным `TargetName="buttonBorder"`
4. Увеличен размер кнопки с 16×16 до 20×20 пикселей
5. Добавлен `Focusable="False"` — кнопка не получает фокус при клике на вкладку

**Результат:**
- При наведении на крестик → фон становится красным (`#FF0000` для Light, `#FF4444` для Dark)
- Текст крестика становится белым
- При уходе мыши → возвращается к прозрачному фону

**Файлы:**
- `LightTheme.xaml` — ControlTemplate с TemplateBinding, hover в ControlTemplate.Triggers
- `DarkTheme.xaml` — аналогично

---

### Исправление 4: Вкладка закрывалась при любом клике мыши

**Проблема:** Вкладки закрывались при нажатии любой кнопки мыши (левой, правой, средней), а не только при клике на крестик или средней кнопке.

**Корневая причина (средняя кнопка):** Attached Behavior `TabItemMiddleClickBehavior` использовал неправильную проверку:
```csharp
if (e.MiddleButton != MouseButtonState.Released) return;
if (e.LeftButton == MouseButtonState.Released || e.RightButton == MouseButtonState.Released) return;
```
Условие было **всегда истинно**, потому что при клике средней кнопкой левая/правая кнопки обычно в состоянии Released.

**Решение:** Использовать `e.ChangedButton` — какая кнопка вызвала событие:
```csharp
if (e.ChangedButton != MouseButton.Middle) return;
if (e.MiddleButton != MouseButtonState.Released) return;
```

**Файлы:**
- `TabItemMiddleClickBehavior.cs` — исправлена проверка на `e.ChangedButton`

---

### Исправление 5: Контекстное меню не работало

**Проблема:** Команды в контекстном меню ("Закрыть", "Закрыть другие", "Закрыть все") не выполнялись.

**Корневая причина:** ContextMenu находится в отдельном popup visual tree, поэтому binding через `RelativeSource AncestorType=TabControl` не мог найти TabControl и его DataContext. Попытка использовать `x:Reference` вызывала циклическую зависимость.

**Решение:** Добавлены команды закрытия прямо в `EditorViewModel`, которые делегируют в `MainViewModel`:

```csharp
// В EditorViewModel.cs
public MainViewModel? MainVm { get; set; }

[RelayCommand]
private async Task CloseTabAsync()
{
    if (MainVm?.CloseTabCommand.CanExecute(this) == true)
        await MainVm.CloseTabCommand.ExecuteAsync(this);
}

[RelayCommand]
private async Task CloseOtherTabsAsync()
{
    if (MainVm?.CloseOtherTabsCommand.CanExecute(this) == true)
        await MainVm.CloseOtherTabsCommand.ExecuteAsync(this);
}

[RelayCommand]
private async Task CloseAllTabsAsync()
{
    if (MainVm?.CloseAllTabsCommand.CanExecute(null) == true)
        await MainVm.CloseAllTabsCommand.ExecuteAsync(null);
}
```

`MainVm` устанавливается при создании вкладки:
```csharp
// В MainViewModel.cs
var editor = new EditorViewModel(template, _templateService) { MainVm = this };
```

XAML теперь простой и чистый:
```xaml
<ContextMenu>
    <MenuItem Header="Закрыть" Command="{Binding CloseTabCommand}"/>
    <MenuItem Header="Закрыть другие" Command="{Binding CloseOtherTabsCommand}"/>
    <MenuItem Header="Закрыть все" Command="{Binding CloseAllTabsCommand}"/>
</ContextMenu>
```

**Файлы:**
- `EditorViewModel.cs` — добавлены `MainVm`, `CloseTabAsync`, `CloseOtherTabsAsync`, `CloseAllTabsAsync`
- `MainViewModel.cs` — установка `MainVm = this` при создании вкладки (NewTab, OpenFileAsync, OnTemplateDoubleClicked)
- `MainWindow.xaml` — упрощён ContextMenu (прямой binding к командам EditorViewModel)

---

### Индикатор несохранённых изменений

**Добавлено:** Оранжевая звёздочка `*` в заголовок вкладки при `IsDirty=True`:
- Цвет: `#FF8C00` (Light), `#FFA500` (Dark)
- Размер: 14px, Bold
- Показывается/скрывается через DataTrigger в ControlTemplate

**Файлы:**
- `LightTheme.xaml` — добавлен `DirtyIndicator` и `WarningOrangeBrush`
- `DarkTheme.xaml` — добавлен `DirtyIndicator` и `WarningOrangeBrush`

---

## ИТОГИ

### Метрики

| Метрика | До | После |
|---------|----|-------|
| **Горячие клавиши** | 1 (Ctrl+N) | 24 |
| **Команды** | 12 | 21 (+9) |
| **MenuItem с Command** | ~10 | 26 (+16) |
| **TabControl** | Сломан | Работает |
| **Кнопка закрытия** | Не работала | Работает + hover |
| **Контекстное меню** | Не работало | Работает (3 пункта) |
| **Средняя кнопка мыши** | Не работала | Закрывает вкладку |
| **Индикатор IsDirty** | Отсутствовал | Оранжевая `*` |
| **PropertiesPanel binding** | Ошибка | Работает |
| **Глобальные конвертеры** | Window.Resources | App.xaml |

### Качество кода

| Проверка | Результат |
|----------|-----------|
| **Сборка** | ✅ 0 ошибок, 0 предупреждений |
| **Тесты** | ✅ 801/801 проходят |
| **Новые тесты** | 0 (Sprint 6 — только исправления) |

---

## СЛЕДУЮЩИЕ ШАГИ

1. **Покрытие тестов** — добавить тесты для новых команд (ZoomIn, ZoomOut, ToggleGrid, ToggleSnap, Print, Exit, CloseTab, CloseOtherTabs, CloseAllTabs)
2. **Этап 2** — начать проектирование редактора УГО

---

**Документ подготовил:** AI-ассистент DotElectric
**Дата:** 07.04.2026
