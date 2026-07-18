# Sprint 40 — Keyboard shortcuts + Selection markers

**Дата:** 24.06.2026
**Статус:** ✅ Исправлено

---

## Fix S40-1: KeyBindings инструментов не совпадали с UI

### Проблема

В `MainWindow.xaml` фактические KeyBindings не соответствовали UI:

| UI (Menu/Toolbar) | Фактический KeyBinding | Ожидаемый |
|---|---|---|
| V (Select) | H | V |
| L (Line) | L ✅ | L |
| R (Rectangle) | U ❌ | R |
| T (Text) | X ❌ | T |

### Исправление

`MainWindow.xaml` — строки 47-50 заменены:

```xml
<!-- Было -->
<KeyBinding Key="L" Command="..." CommandParameter="Line"/>
<KeyBinding Key="U" Command="..." CommandParameter="Rectangle"/>
<KeyBinding Key="X" Command="..." CommandParameter="Text"/>
<KeyBinding Key="H" Command="..." CommandParameter="Select"/>

<!-- Стало (временно, затем удалено в S40-3) -->
<KeyBinding Key="L" Command="..." CommandParameter="Line"/>
<KeyBinding Key="R" Command="..." CommandParameter="Rectangle"/>
<KeyBinding Key="T" Command="..." CommandParameter="Text"/>
<KeyBinding Key="V" Command="..." CommandParameter="Select"/>
```

---

## Fix S40-2: R-клавиша конфликтовала (Rectangle vs Rotate)

### Проблема

Клавиша `R` была занята RotateCW (`KeyBinding Key="R"`), не позволяя использовать её для Rectangle. Shift+R была занята RotateCCW.

### Исправление

Rotate перенесён с R на E / Shift+E:

```xml
<!-- Было -->
<KeyBinding Key="R" Command="{Binding SelectedTab.RotateSelectedClockwiseCommand}"/>
<KeyBinding Key="R" Modifiers="Shift" Command="{Binding SelectedTab.RotateSelectedCounterClockwiseCommand}"/>

<!-- Стало (временно, затем удалено в S40-3) -->
<KeyBinding Key="E" Command="{Binding SelectedTab.RotateSelectedClockwiseCommand}"/>
<KeyBinding Key="E" Modifiers="Shift" Command="{Binding SelectedTab.RotateSelectedCounterClockwiseCommand}"/>
```

---

## Fix S40-3: Переключение инструментов не работало с русской раскладкой

### Проблема

WPF `KeyBinding` с `KeyGesture` не срабатывал при русской раскладке клавиатуры. `KeyGesture.Matches` зависел от текущей локали и не всегда корректно обрабатывал физические клавиши для не-US раскладок.

### Исправление

Инструменты (V/L/R/T) и rotate (E/Shift+E) перенесены из `Window.InputBindings` в `PreviewKeyDown` handler на Window. `e.Key` в PreviewKeyDown возвращает физическую клавишу независимо от раскладки.

**MainWindow.xaml** — удалены KeyBindings для инструментов и rotate:
```xml
<!-- Удалены: -->
<!-- Инструменты рисования -->
<KeyBinding Key="L" .../>
<KeyBinding Key="R" .../>
<KeyBinding Key="T" .../>
<KeyBinding Key="V" .../>
<!-- Вращение текста -->
<KeyBinding Key="E" .../>
<KeyBinding Key="E" Modifiers="Shift" .../>
```

**MainWindow.xaml** — добавлен атрибут:
```xml
PreviewKeyDown="Window_PreviewKeyDown"
```

**MainWindow.xaml.cs** — новый handler:
```csharp
private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
{
    if (e.Handled) return;
    if (DataContext is not MainViewModel vm) return;
    if (vm.SelectedTab is not EditorViewModel editor) return;

    switch (e.Key)
    {
        case Key.V:
            editor.ActiveTool = "Select";
            e.Handled = true;
            break;
        case Key.L:
            editor.ActiveTool = "Line";
            e.Handled = true;
            break;
        case Key.R:
            if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                editor.ActiveTool = "Rectangle";
                e.Handled = true;
            }
            break;
        case Key.T:
            editor.ActiveTool = "Text";
            e.Handled = true;
            break;
        case Key.E:
            if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                editor.RotateSelectedClockwiseCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                editor.RotateSelectedCounterClockwiseCommand.Execute(null);
                e.Handled = true;
            }
            break;
    }
}
```

**Почему PreviewKeyDown:**
- `e.Key` возвращает физический код клавиши (VK), а не символ
- В русской раскладке клавиша 'М' (физически V) даёт `Key.V`
- В русской раскладке клавиша 'Е' (физически T) даёт `Key.T`
- Не требует проверки `Keyboard.Modifiers` для модификаторов

---

## Fix S40-4: Панель инструментов не обновлялась при горячих клавишах

### Проблема

`SetActiveToolCommand` (RelayCommand, `EditorViewModel.cs:537`) устанавливал `_toolManager.ActiveTool` напрямую:

```csharp
// Было — OnPropertyChanged не вызывался на EditorViewModel
private void SetActiveTool(string tool)
{
    _toolManager.ActiveTool = tool;
}
```

Радио-кнопки на панели инструментов биндятся к `SelectedTab.ActiveTool` через `EqualToBoolConverter`. Поскольку `OnPropertyChanged(nameof(ActiveTool))` не вызывался на `EditorViewModel`, биндинг не обновлялся.

### Исправление

`SetActiveTool()` теперь использует сеттер свойства `ActiveTool`, который вызывает `OnPropertyChanged()`:

```csharp
// Стало — сеттер вызывает OnPropertyChanged
[RelayCommand]
private void SetActiveTool(string tool)
{
    ActiveTool = tool;
}
```

Сеттер `ActiveTool` (`EditorViewModel.cs:323-337`) выполняет:
1. `ResetTool(prevTool)` — сброс предыдущего инструмента
2. `_previewManager.ClearAll()` — очистка preview
3. `_toolManager.ActiveTool = value` — установка нового
4. `OnPropertyChanged()` — уведомление биндингов

---

## Fix S40-5: Маркеры выделения не появлялись на выбранных объектах

### Проблема

`ShowSelectionMarkers` (`SelectionManager.cs:22`) возвращал `true` только при `SelectedObjects.Count == 1`. При мульти-выделении:

- `ContentControl Content="{Binding SingleSelectedObject}"` получал null (т.к. `SingleSelectedObject` = null при count > 1)
- `Visibility` привязывался к `ShowSelectionMarkers` = false → `Collapsed`

Таким образом, маркеры (ручки для ресайза) не отображались при выделении нескольких объектов.

### Исправление

**SelectionManager.cs** — изменено условие:

```csharp
// Было
public bool ShowSelectionMarkers => SelectedObjects.Count == 1;

// Стало — показывать маркеры при любом количестве > 0
public bool ShowSelectionMarkers => SelectedObjects.Count > 0;
```

**EditorCanvas.xaml** — `ContentControl` заменён на `ItemsControl`:

```xml
<!-- Было -->
<ContentControl Content="{Binding SingleSelectedObject}"
                Visibility="{Binding ShowSelectionMarkers, Converter={StaticResource BoolToVisibility}}"
                IsHitTestVisible="False">
    <ContentControl.Resources>
        <!-- DataTemplate для Line, Rectangle, Text -->
    </ContentControl.Resources>
</ContentControl>

<!-- Стало -->
<ItemsControl ItemsSource="{Binding SelectedObjects}"
              Visibility="{Binding ShowSelectionMarkers, Converter={StaticResource BoolToVisibility}}"
              IsHitTestVisible="False">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <Canvas/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.Resources>
        <!-- Те же DataTemplate для Line, Rectangle, Text -->
    </ItemsControl.Resources>
</ItemsControl>
```

**Почему ItemsControl:**
- `ItemsSource="{Binding SelectedObjects}"` — создаёт ContentPresenter для каждого объекта
- Для каждого объекта применяется соответствующий DataTemplate (Line → 2 круга, Rectangle → 8 квадратов, Text → 4 квадрата)
- `ItemsPanelTemplate` с Canvas обеспечивает корректное позиционирование маркеров в пиксельных координатах
- `RelativeSource AncestorType=UserControl` внутри DataTemplate продолжает работать

**Обновлены тесты:**
- `ManagerTests.ShowSelectionMarkers_FalseForMultipleSelection` → `ShowSelectionMarkers_TrueForMultipleSelection`
- `EditorViewModelTests.ShowSelectionMarkers_FalseForMultiSelection` → `ShowSelectionMarkers_TrueForMultiSelection`

---

## Файлы

| Файл | Изменения |
|------|-----------|
| `MainWindow.xaml` | KeyBindings удалены из InputBindings; добавлен PreviewKeyDown |
| `MainWindow.xaml.cs` | Новый handler Window_PreviewKeyDown |
| `ViewModels/EditorViewModel.cs` | `SetActiveTool()` → через сеттер ActiveTool |
| `ViewModels/Managers/SelectionManager.cs` | `ShowSelectionMarkers` → `Count > 0` |
| `Views/EditorCanvas.xaml` | ContentControl → ItemsControl |
| `Tests/ViewModels/Managers/ManagerTests.cs` | Обновлён assert |
| `Tests/ViewModels/EditorViewModelTests.cs` | Обновлён assert |

## Результаты

- **Build:** 0 errors, 4 warnings (pre-existing)
- **Tests:** 844+ пройдены (112 EditorVM + 44 Manager + 49 Integration + 62 Tool + 65 HitTest + 13 SelectionBox + прочие)
- **Hotkeys:** V/L/R/T/E работают независимо от раскладки
- **Markers:** Показываются на всех выделенных объектах, не только на одном
