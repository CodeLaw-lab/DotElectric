# P3-D1: Конфликт имён ICommand

## Анализ проблемы

**Файлы:** `src/DotElectric.TemplateEditor/Commands/ICommand.cs:7`

**Симптом:** Собственный интерфейс `ICommand` находится в `DotElectric.TemplateEditor.Commands` и конфликтует с `System.Windows.Input.ICommand`.

**Код:**
```csharp
// Commands/ICommand.cs
namespace DotElectric.TemplateEditor.Commands;

public interface ICommand
{
    void Execute();
    void Undo();
    string Name { get; }
}
```

**Где проявляется конфликт:**
1. В XAML-файлах, использующих WPF-команды (`System.Windows.Input.ICommand`) — если в коде появляется `using System.Windows.Input;` и `using DotElectric.TemplateEditor.Commands;` одновременно
2. В любом классе, который использует `ICommand` и также ссылается на WPF `ICommand` (например, через DataBinding)
3. В `SelectTool.cs:286` уже используется явное указание `List<Commands.ICommand>` — это workaround для конфликта

**Проявления:**
```csharp
// Приходится писать полное имя:
List<Commands.ICommand> commands = new();

// Вместо:
List<ICommand> commands = new();
```

## Анализ: почему возникло

Проект WPF (.NET 10), `System.Windows.Input.ICommand` — фундаментальный WPF-интерфейс. Собственный `ICommand` был назван так для краткости, но это создаёт неоднозначность.

## Пошаговый план исправления

### Шаг 1: Переименовать файл

Переименовать `Commands/ICommand.cs` → `Commands/IUndoCommand.cs`.

### Шаг 2: Переименовать интерфейс

```csharp
namespace DotElectric.TemplateEditor.Commands;

public interface IUndoCommand
{
    void Execute();
    void Undo();
    string Name { get; }
}
```

Обновить XML-комментарий:
```csharp
/// <summary>
/// Интерфейс команды для системы Undo/Redo.
/// Отличается от System.Windows.Input.ICommand.
/// </summary>
public interface IUndoCommand
```

### Шаг 3: Обновить все реализации

Найти все классы, реализующие `ICommand`:

```bash
rg " : ICommand$" src/ --include "*.cs"
rg "ICommand " src/ --include "*.cs"
```

Заменить во всех найденных файлах:
- `: ICommand` → `: IUndoCommand`
- `Commands.ICommand` → `Commands.IUndoCommand`

Команды для замены:
- `AddObjectCommand.cs`
- `DeleteObjectCommand.cs`
- `MoveObjectCommand.cs`
- `ResizeObjectCommand.cs`
- `CustomResizeCommand.cs`
- `ChangePropertyCommand.cs`
- `RotateObjectCommand.cs`
- `BatchCommand.cs`
- `PasteObjectCommand.cs`

### Шаг 4: Обновить CommandHistory

```csharp
public class CommandHistory
{
    private readonly Stack<IUndoCommand> _undoStack = new();
    private readonly Stack<IUndoCommand> _redoStack = new();
    // ...
}
```

### Шаг 5: Обновить IEditorViewModelFactory (если использует ICommand)

```bash
rg "ICommand" src/ --include "*.cs"
```

Проверить все упоминания и заменить на `IUndoCommand` там, где речь идёт о нашем интерфейсе, а не WPF-овском.

### Шаг 6: Проверить SelectTool.cs

В `SelectTool.cs:286` есть `List<Commands.ICommand>`. После переименования будет:
```csharp
List<IUndoCommand> commands = new();
```
(если `using DotElectric.TemplateEditor.Commands;` уже есть в файле).

## Альтернатива: сделать alias

Вместо переименования можно добавить `using` alias:
```csharp
using IUndoCommand = DotElectric.TemplateEditor.Commands.ICommand;
```

Но это скрывает проблему, а не решает её. Переименование — правильный подход.

## Затрагиваемые файлы

Ожидается ~12-15 файлов:
1. `Commands/ICommand.cs` — переименование
2. `Commands/CommandHistory.cs` — изменение типа стека
3-11. Все файлы команд (9 impl + 1 command history)
12+ Файлы, использующие `List<Commands.ICommand>` или переменные типа `ICommand` из нашего namespace

## Проверка

```bash
# После изменений должен быть 0 ошибок сборки
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- **High:** Изменение затрагивает ~15 файлов. Легко пропустить одно вхождение.
- **Митигация:** Использовать IDE-рефакторинг (Rename Symbol) вместо ручной замены.
- Если используется `var` для переменных типа `ICommand`, замена не требуется.
- Если есть какой-то код, который использует `System.Windows.Input.ICommand.ICommand` — он останется без изменений.
