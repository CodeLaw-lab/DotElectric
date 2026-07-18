# Тема 5.3: Commands (ICommand, RelayCommand, AsyncRelayCommand)

### Теория

**ICommand** — интерфейс для инкапсуляции действий (альтернатива event handlers).

#### ICommand интерфейс

```csharp
public interface ICommand
{
    bool CanExecute(object parameter);
    void Execute(object parameter);
    event EventHandler CanExecuteChanged;
}
```

| Метод/Событие | Описание |
|---------------|----------|
| **CanExecute** | Возвращает true, если команда может выполниться |
| **Execute** | Выполняет действие команды |
| **CanExecuteChanged** | Событие для уведомления о изменении CanExecute |

### Примеры кода

#### Пример 1: Ручная реализация ICommand

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

// Generic версия
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        if (parameter is T t)
        {
            return _canExecute?.Invoke(t) ?? true;
        }
        return false;
    }

    public void Execute(object parameter)
    {
        if (parameter is T t)
        {
            _execute(t);
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

#### Пример 2: Использование RelayCommand в ViewModel

```csharp
public class MainViewModel : ObservableObject
{
    private string _searchText;

    [ObservableProperty]
    private bool _canSearch;

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            ((RelayCommand)SearchCommand).NotifyCanExecuteChanged();
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand LoadCommand { get; }

    public MainViewModel()
    {
        SearchCommand = new RelayCommand(Search, CanSearchExecute);
        ClearCommand = new RelayCommand(Clear);
        LoadCommand = new RelayCommand<string>(LoadById);
    }

    private bool CanSearchExecute() => CanSearch && !string.IsNullOrWhiteSpace(SearchText);

    private void Search()
    {
        // Search logic
        Console.WriteLine($"Searching for: {SearchText}");
    }

    private void Clear()
    {
        SearchText = string.Empty;
    }

    private void LoadById(string id)
    {
        Console.WriteLine($"Loading item with ID: {id}");
    }
}
```

#### Пример 3: AsyncRelayCommand

```csharp
public class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<DataItem> _items;

    public ICommand LoadDataCommand { get; }
    public ICommand SaveDataCommand { get; }

    public MainViewModel()
    {
        Items = new ObservableCollection<DataItem>();
        
        // Async команда
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        
        // Async команда с CanExecute
        SaveDataCommand = new AsyncRelayCommand(SaveDataAsync, CanSave);
    }

    private bool CanSave() => !IsLoading && Items.Any();

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ((AsyncRelayCommand)SaveDataCommand).NotifyCanExecuteChanged();

            await Task.Delay(1000); // Simulate API call

            Items.Clear();
            for (int i = 1; i <= 10; i++)
            {
                Items.Add(new DataItem { Name = $"Item {i}" });
            }
        }
        finally
        {
            IsLoading = false;
            ((AsyncRelayCommand)SaveDataCommand).NotifyCanExecuteChanged();
        }
    }

    private async Task SaveDataAsync()
    {
        IsLoading = true;
        await Task.Delay(500); // Simulate save
        IsLoading = false;
    }
}
```

#### Пример 4: CommunityToolkit [RelayCommand]

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText;

    [ObservableProperty]
    private bool _isLoading;

    // Простая команда
    [RelayCommand]
    private void Search()
    {
        Console.WriteLine($"Searching: {SearchText}");
    }

    // Команда с параметром
    [RelayCommand]
    private void DeleteItem(int id)
    {
        Console.WriteLine($"Deleting item {id}");
    }

    // Async команда
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        await Task.Delay(1000);
        IsLoading = false;
    }

    // Команда с CanExecute
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        Console.WriteLine("Saving...");
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(SearchText) && !IsLoading;

    // Async команда с CanExecute
    [RelayCommand(CanExecute = nameof(CanProcess))]
    private async Task ProcessAsync()
    {
        IsLoading = true;
        await Task.Delay(1000);
        IsLoading = false;
    }

    private bool CanProcess() => !IsLoading;
}
```

#### Пример 5: Составные команды (BatchCommand)

```csharp
public class BatchCommand : ICommand
{
    private readonly List<ICommand> _commands;

    public BatchCommand(params ICommand[] commands)
    {
        _commands = new List<ICommand>(commands);
        
        // Подписка на CanExecuteChanged всех команд
        foreach (var command in _commands)
        {
            command.CanExecuteChanged += (s, e) => 
                CanExecuteChanged?.Invoke(this, e);
        }
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _commands.All(c => c.CanExecute(parameter));
    }

    public void Execute(object parameter)
    {
        foreach (var command in _commands)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}

// Использование
public class EditorViewModel : ObservableObject
{
    public ICommand SaveAndCloseCommand { get; }

    public EditorViewModel()
    {
        var saveCommand = new RelayCommand(Save);
        var closeCommand = new RelayCommand(Close);
        
        // Составная команда
        SaveAndCloseCommand = new BatchCommand(saveCommand, closeCommand);
    }

    private void Save() { /* Save logic */ }
    private void Close() { /* Close logic */ }
}
```

#### Пример 6: Команды с подтверждением

```csharp
public class ConfirmCommand : ICommand
{
    private readonly ICommand _innerCommand;
    private readonly Func<Task<bool>> _confirmAction;

    public ConfirmCommand(ICommand innerCommand, Func<Task<bool>> confirmAction)
    {
        _innerCommand = innerCommand;
        _confirmAction = confirmAction;

        innerCommand.CanExecuteChanged += (s, e) =>
            CanExecuteChanged?.Invoke(this, e);
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _innerCommand.CanExecute(parameter);
    }

    public async void Execute(object parameter)
    {
        if (await _confirmAction())
        {
            _innerCommand.Execute(parameter);
        }
    }
}

// Использование
public class DocumentViewModel : ObservableObject
{
    public ICommand DeleteCommand { get; }

    public DocumentViewModel()
    {
        var deleteCommand = new RelayCommand(Delete);
        
        DeleteCommand = new ConfirmCommand(deleteCommand, async () =>
        {
            var result = MessageBox.Show("Are you sure?", "Confirm", 
                                         MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        });
    }

    private void Delete() { /* Delete logic */ }
}
```

#### Пример 7: Реальные команды из DotElectric

```csharp
// EditorViewModel.Commands.cs
public partial class EditorViewModel
{
    // Undo/Redo команды
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _history.Undo();
        MarkDirty();
        UpdateCanExecute();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _history.Redo();
        MarkDirty();
        UpdateCanExecute();
    }

    private bool CanUndo() => _history.CanUndo;
    private bool CanRedo() => _history.CanRedo;

    // Команды редактирования
    [RelayCommand]
    private void DeleteSelectedObjects()
    {
        if (SelectedObject != null)
        {
            var command = new DeleteObjectCommand(SelectedObject, Template.Objects);
            _history.Execute(command);
            SelectedObject = null;
            MarkDirty();
        }
    }

    [RelayCommand]
    private void DuplicateSelectedObject()
    {
        if (SelectedObject != null)
        {
            var duplicate = SelectedObject.Clone();
            Template.Objects.Add(duplicate);
            SelectedObject = duplicate;
            MarkDirty();
        }
    }

    // Zoom команды
    [RelayCommand]
    private void ZoomIn()
    {
        var newZoom = Math.Min(Zoom * 1.2, 10.0);
        SetZoom(newZoom);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        var newZoom = Math.Max(Zoom / 1.2, 0.1);
        SetZoom(newZoom);
    }

    [RelayCommand]
    private void ZoomToFit()
    {
        Zoom = CalculateFitToScreenZoom();
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    [RelayCommand]
    private void ZoomTo100Percent()
    {
        Zoom = 1.0;
    }

    private void SetZoom(double value)
    {
        Zoom = value;
        StatusMessage = $"Zoom: {value * 100:F0}%";
    }

    // Grid команды
    [RelayCommand]
    private void ToggleGrid()
    {
        StatusBarGridEnabled = !StatusBarGridEnabled;
    }

    [RelayCommand]
    private void ToggleSnap()
    {
        SnapToGridEnabled = !SnapToGridEnabled;
    }

    // File команды
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            await _fileService.SaveAsync(Template);
            IsDirty = false;
            StatusMessage = "Saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        await _printService.PrintAsync(Template);
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    private void UpdateCanExecute()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
}
```

#### Пример 8: Keyboard shortcuts с командами

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupKeyboardBindings();
    }

    private void SetupKeyboardBindings()
    {
        // Ctrl+S - Save
        InputBindings.Add(new KeyBinding(
            new RelayCommand(async () => await ((MainViewModel)DataContext).SaveAsync()),
            new KeyGesture(Key.S, ModifierKeys.Control)));

        // Ctrl+O - Open
        InputBindings.Add(new KeyBinding(
            new RelayCommand(async () => await ((MainViewModel)DataContext).OpenAsync()),
            new KeyGesture(Key.O, ModifierKeys.Control)));

        // Ctrl+Z - Undo
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => ((MainViewModel)DataContext).UndoCommand.Execute(null)),
            new KeyGesture(Key.Z, ModifierKeys.Control)));

        // Ctrl+Y - Redo
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => ((MainViewModel)DataContext).RedoCommand.Execute(null)),
            new KeyGesture(Key.Y, ModifierKeys.Control)));

        // Delete - Delete selected
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => ((MainViewModel)DataContext).DeleteSelectedCommand.Execute(null)),
            new KeyGesture(Key.Delete)));

        // F5 - Refresh
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => ((MainViewModel)DataContext).RefreshCommand.Execute(null)),
            new KeyGesture(Key.F5)));
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 5.3.1: Simple Command**

Создайте RelayCommand:
- Команда Increment (увеличивает Count)
- Команда Decrement (уменьшает Count)
- Отображение в UI с кнопками

**Задача 5.3.2: Command с параметром**

Создайте RelayCommand&lt;T&gt;:
- Команда AddItem (принимает string)
- Команда RemoveItem (принимает int index)
- ObservableCollection для хранения

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 5.3.3: Async Command**

Создайте AsyncRelayCommand:
- Загрузка данных (Task.Delay 1 сек)
- IsLoading property
- Блокировка кнопки во время загрузки

**Задача 5.3.4: Command с CanExecute**

Создайте команду с CanExecute:
- SaveCommand (CanExecute: текст не пустой)
- DeleteCommand (CanExecute: элемент выбран)
- NotifyCanExecuteChanged при изменении условий

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 5.3.5: Composite Command**

Реализуйте BatchCommand:
- Выполнение нескольких команд последовательно
- CanExecute = все команды могут выполниться
- Использование для Save+Close

**Задача 5.3.6: Confirm Command**

Создайте ConfirmCommand:
- Подтверждение перед выполнением
- Async подтверждение (диалог)
- Декоратор для существующих команд

---

### Решения

<details>
<summary>✅ Решение задачи 5.3.1</summary>

```csharp
public partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count;

    [RelayCommand]
    private void Increment()
    {
        Count++;
    }

    [RelayCommand]
    private void Decrement()
    {
        Count--;
    }

    [RelayCommand]
    private void Reset()
    {
        Count = 0;
    }
}
```

```xml
<StackPanel>
    <TextBlock Text="{Binding Count}" FontSize="48" HorizontalAlignment="Center"/>
    
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,20">
        <Button Content="-" Command="{Binding DecrementCommand}" Padding="20,10"/>
        <Button Content="Reset" Command="{Binding ResetCommand}" Margin="10,0" Padding="20,10"/>
        <Button Content="+" Command="{Binding IncrementCommand}" Padding="20,10"/>
    </StackPanel>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 5.3.3</summary>

```csharp
public partial class DataViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<string> _items;

    public DataViewModel()
    {
        Items = new ObservableCollection<string>();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(1000);
            
            Items.Clear();
            for (int i = 1; i <= 10; i++)
            {
                Items.Add($"Item {i}");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

```xml
<StackPanel>
    <Button Content="Load Data" 
            Command="{Binding LoadDataCommand}"
            IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBool}}"/>
    
    <ProgressBar IsIndeterminate="True"
                 Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}"
                 Height="5"
                 Margin="0,10"/>
    
    <ListBox ItemsSource="{Binding Items}" Height="300"/>
</StackPanel>
```
</details>

---

## Ключевые выводы

✅ **ICommand** — инкапсуляция действий вместо event handlers  
✅ **RelayCommand** — простая реализация ICommand с делегатом  
✅ **AsyncRelayCommand** — для async/await операций  
✅ **CanExecute** — определяет доступность команды  
✅ **NotifyCanExecuteChanged** — уведомление об изменении CanExecute  
✅ **[RelayCommand]** — source generator для команд  
✅ **CanExecute parameter** — метод для проверки возможности выполнения  
✅ **Keyboard shortcuts** — KeyBinding с командами

---

## Дополнительные ресурсы

- [ICommand](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand)
- [RelayCommand](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/relaycommand)
- [AsyncRelayCommand](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/asyncrelaycommand)
