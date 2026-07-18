# Fix S50 — Clipboard improvements (Copy/Paste/Cut)

## S50-1: Ctrl+X keyboard shortcut + UI

**Проблема:** `InputGestureText="Ctrl+X"` отображался в контекстном меню, но:
- Привязки в `Window.InputBindings` не было — клавиша не работала
- В главном меню и тулбаре отсутствовал пункт «Вырезать» (были только Копировать/Вставить/Удалить)

**Исправление:**
- Добавлен `<KeyBinding Key="X" Modifiers="Control" Command="{Binding SelectedTab.CutSelectedCommand}"/>` в `MainWindow.xaml:36`
- В главное меню добавлен `MenuItem Header="Вы_резать"` с иконкой `ContentCut` между Копировать и Вставить
- В тулбар добавлена кнопка `Вырезать (Ctrl+X)` с иконкой `ContentCut` между Копировать и Вставить

---

## S50-2: Re-paste bug (same instance added twice)

**Проблема:** `ClipboardManager.GetClipboardContents()` возвращал `_clipboard.ToList().AsReadOnly()` — ссылки на те же клонированные экземпляры. Повторный Ctrl+V добавлял их в `Template.Objects` снова.

**Исправление:** `GetClipboardContents()` теперь клонирует объекты при каждом вызове:
```csharp
public IReadOnlyList<TemplateObjectBase> GetClipboardContents()
    => _clipboard.Select(o => o.Clone()).ToList().AsReadOnly();
```

**Файл:** `ClipboardManager.cs:27-28`

---

## S50-3: Paste offset (10mm step)

**Проблема:** Вставленные объекты появлялись точно поверх оригиналов.

**Исправление:**
- Добавлены поля `_pasteOffsetX`/`_pasteOffsetY` (long) и константа `PasteOffsetStepMicrons = 10_000` (10мм)
- При Copy offset устанавливается в 10мм (не 0) — первый Paste уже со смещением
- После каждого Paste offset += 10мм — накопление до следующего Copy
- При повторном Copy offset сбрасывается в 10мм

**Файл:** `ClipboardManager.cs:10-14,22-23,34-42`

---

## S50-4: BatchCommand для Cut/Paste нескольких объектов

**Проблема:** N отдельных команд в Undo-стеке при Cut/Paste N объектов.

**Исправление:** При >1 объекте `PasteFromClipboard()` и `DeleteSelected()` создают `BatchCommand`, группирующий все операции.

**Логика:**
- 1 объект → обычная команда (single Push)
- >1 объект → `BatchCommand` с именем "Вставить объекты" / "Удалить объекты"
- Cut вызывает `DeleteSelected()` внутри, поэтому группировка работает и для Cut

**Файлы:** `EditorViewModel.cs:570-587,982-996`

---

## S50-5: Auto-select pasted objects

**Проблема:** После Paste выделение не менялось — вставленные объекты не были видны.

**Исправление:**
- Добавлен `SelectionManager.SelectObjects(IEnumerable<TemplateObjectBase>)` — очищает и заполняет `SelectedObjects`
- В `PasteFromClipboard()` после всех `CommandHistory.Push()` вызывается `_selectionManager.SelectObjects(clipboard)`

**Файлы:** `SelectionManager.cs:56-62`, `EditorViewModel.cs:586`

---

## S50-6: StatusBar feedback

**Проблема:** Никакой обратной связи в строке состояния.

**Исправление:** В `CopySelected()`, `CutSelected()`, `PasteFromClipboard()` устанавливается `StatusBarManager.StatusMessage`:
- "Скопировано: N объектов" / "Нет выделенных объектов"
- "Вырезано: N объектов" / "Нет выделенных объектов"
- "Вставлено: N объектов" / "Буфер обмена пуст"

Добавлен `GetObjectWord(int count)` для русских числительных.

**Файл:** `EditorViewModel.cs:557-587,1047-1053`

---

## S50-7: Clipboard cleanup on tab close

**Проблема:** При закрытии вкладки объекты в буфере обмена ссылались на удалённый шаблон.

**Исправление:** `ClipboardManager.Clear()` вызывается в `EditorViewModel.Dispose()`.

**Файлы:** `ClipboardManager.cs:30`, `EditorViewModel.cs:1039`

---

## S50-8: Ctrl+V перехватывался PreviewKeyDown (tool switcher)

**Проблема:** `Window_PreviewKeyDown` в `MainWindow.xaml.cs:27` обрабатывал `case Key.V` без проверки модификаторов. При `Ctrl+V`:
1. Событие перехватывалось, устанавливался `ActiveTool = "Select"`
2. `e.Handled = true` — `KeyBinding` для Ctrl+V в `InputBindings` никогда не получал событие
3. Paste не работал

**Исправление:** Добавлена проверка `if (e.KeyboardDevice.Modifiers != ModifierKeys.None) break;` для всех tool-switching кейсов (V, L, R, T). Ctrl+V теперь доходит до `InputBindings`.

**Файл:** `MainWindow.xaml.cs:27-58`

---

## Проверка

- **Build:** 0 errors, 5 pre-existing warnings
- **Tests:** 1289+ пройдены (0 failures, pre-existing FileService hang excluded)
