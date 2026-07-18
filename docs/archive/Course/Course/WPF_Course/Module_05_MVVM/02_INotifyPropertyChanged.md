# Тема 5.2: INotifyPropertyChanged и CommunityToolkit.Mvvm

### Теория

**INotifyPropertyChanged** — интерфейс для уведомления View об изменении свойств ViewModel.

#### Эволюция реализации

```
Ручная реализация → Fody.PropertyChanged → CommunityToolkit.Mvvm
(много кода)        (IL weaving)           (source generators)
```

#### CommunityToolkit.Mvvm атрибуты

| Атрибут | Описание | Пример |
|---------|----------|--------|
| **[ObservableObject]** | Базовый класс с INPC | `public partial class VM : ObservableObject` |
| **[ObservableProperty]** | Генерация свойства | `[ObservableProperty] private string _name;` |
| **[RelayCommand]** | Генерация ICommand | `[RelayCommand] void Save() {}` |
| **[ObservableRecipient]** | ViewModel с Messenger | `[ObservableRecipient]` |
| **[AlsoNotifyChangeFor]** | Уведомление других свойств | `[AlsoNotifyChangeFor(nameof(FullName))]` |
| **[AlsoBroadcastChange]** | Рассылка изменения | `[AlsoBroadcastChange]` |

### Примеры кода

#### Пример 1: Ручная реализация INotifyPropertyChanged

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;
    private int _age;

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName != value)
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName != value)
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public int Age
    {
        get => _age;
        set
        {
            if (_age != value)
            {
                _age = value;
                OnPropertyChanged(nameof(Age));
            }
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

#### Пример 2: С CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private int _age;

    // Генерируется автоматически:
    // - FirstName property с getter/setter
    // - OnPropertyChanged в setter
    // - OnFirstNameChanging partial method
    // - OnFirstNameChanged partial method

    public string FullName => $"{FirstName} {LastName}";
}
```

**Сгенерированный код (первый вариант):**
```csharp
// То, что генерирует source generator
public partial class PersonViewModel
{
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(_firstName, value))
            {
                OnFirstNameChanging(value);
                _firstName = value;
                OnFirstNameChanged(value);
                OnPropertyChanged("FirstName");
                OnPropertyChanged("FullName");
            }
        }
    }
}
```

#### Пример 3: Partial methods для хуков

```csharp
public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private bool _isLoading;

    // Partial method вызывается ПЕРЕД изменением
    partial void OnFirstNameChanging(string value)
    {
        // Валидация перед изменением
        if (value?.Length > 50)
        {
            throw new ArgumentException("First name too long");
        }
    }

    // Partial method вызывается ПОСЛЕ изменения
    partial void OnFirstNameChanged(string value)
    {
        // Логика после изменения
        Console.WriteLine($"First name changed to: {value}");
        LogChange("FirstName", value);
    }

    partial void OnIsLoadingChanged(bool value)
    {
        // Автоматическая отмена при загрузке
        if (value)
        {
            // Disable commands
        }
    }

    private void LogChange(string property, string newValue)
    {
        // Logging logic
    }
}
```

#### Пример 4: AlsoNotifyChangeFor

```csharp
public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    [AlsoNotifyChangeFor(nameof(DisplayName))]
    private string _firstName;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    [AlsoNotifyChangeFor(nameof(DisplayName))]
    private string _lastName;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsAdult))]
    private int _age;

    // Эти свойства обновляются автоматически при изменении FirstName/LastName
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayName => $"{FirstName} {LastName} ({Age})";
    
    // Это свойство обновляется при изменении Age
    public bool IsAdult => Age >= 18;
}
```

#### Пример 5: Также уведомление для коллекций

```csharp
public partial class TasksViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CompletedCount))]
    [AlsoNotifyChangeFor(nameof(PendingCount))]
    private int _completedTasks;

    public TasksViewModel()
    {
        Tasks = new ObservableCollection<TaskItem>();
        Tasks.CollectionChanged += Tasks_CollectionChanged;
    }

    private void Tasks_CollectionChanged(object sender, 
                                         NotifyCollectionChangedEventArgs e)
    {
        // Обновляем счётчики при изменении коллекции
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(PendingCount));
    }

    public int CompletedCount => Tasks?.Count(t => t.IsCompleted) ?? 0;
    public int PendingCount => Tasks?.Count(t => !t.IsCompleted) ?? 0;
}

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isCompleted;
}
```

#### Пример 6: ObservableRecipient для Messenger

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private PersonViewModel _selectedPerson;

    public MainViewModel(IMessenger messenger) : base(messenger)
    {
        // Подписка на сообщения
        Register<SelectedPersonChangedMessage>(this, (vm, msg) =>
        {
            SelectedPerson = msg.Value;
        });

        // Подписка на PropertyChangedMessage
        Register<PropertyChangedMessage<string>>(this, (vm, msg) =>
        {
            if (msg.PropertyName == nameof(PersonViewModel.FullName))
            {
                Title = $"Selected: {msg.NewValue}";
            }
        });
    }

    [RelayCommand]
    private void SelectPerson(PersonViewModel person)
    {
        SelectedPerson = person;
        
        // Отправка сообщения
        Messenger.Send(new SelectedPersonChangedMessage(person));
    }

    protected override void OnActivated()
    {
        // Вызывается при активации ViewModel
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        // Вызывается при деактивации (автоматическая отписка)
        base.OnDeactivated();
    }
}

// Message класс
public class SelectedPersonChangedMessage : ValueMessage<PersonViewModel>
{
    public SelectedPersonChangedMessage(PersonViewModel value) : base(value) { }
}
```

#### Пример 7: Async свойства с уведомлением

```csharp
public partial class DataViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private ObservableCollection<DataItem> _items;

    public DataViewModel()
    {
        Items = new ObservableCollection<DataItem>();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            HasData = false;

            // Имитация async операции
            await Task.Delay(1000);
            
            var data = await FetchDataAsync();
            
            Items.Clear();
            foreach (var item in data)
            {
                Items.Add(item);
            }
            
            HasData = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<DataItem>> FetchDataAsync()
    {
        await Task.Delay(500);
        return new List<DataItem>
        {
            new DataItem { Name = "Item 1" },
            new DataItem { Name = "Item 2" }
        };
    }
}

public partial class DataItem : ObservableObject
{
    [ObservableProperty]
    private string _name;
}
```

#### Пример 8: Реальное использование из DotElectric

```csharp
// EditorViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class EditorViewModel : ObservableObject, IDisposable
{
    private readonly ITemplateService _templateService;
    private readonly CommandHistory _history;

    [ObservableProperty]
    private Template _template;

    [ObservableProperty]
    private ITemplateObject _selectedObject;

    [ObservableProperty]
    private ObservableCollection<ITemplateObject> _selectedObjects;

    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private double _panOffsetX;

    [ObservableProperty]
    private double _panOffsetY;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private bool _statusBarGridEnabled;

    [ObservableProperty]
    private double _statusBarGridStepMm = 5.0;

    public string Title => Template?.Metadata?.FileName ?? "Untitled";

    public string StatusBarSheetFormat => 
        Template?.Sheet?.ToString() ?? "No sheet";

    public EditorViewModel(ITemplateService templateService)
    {
        _templateService = templateService;
        _history = new CommandHistory();
        SelectedObjects = new ObservableCollection<ITemplateObject>();

        // Подписка на изменения Template
        this.PropertyChanged += OnEditorViewModelPropertyChanged;
    }

    partial void OnTemplateChanged(Template value)
    {
        // Отписка от старого Template
        if (value != null)
        {
            value.PropertyChanged += Template_PropertyChanged;
        }
        
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(StatusBarSheetFormat));
    }

    private void Template_PropertyChanged(object sender, 
                                          PropertyChangedEventArgs e)
    {
        MarkDirty();
        
        if (e.PropertyName == nameof(Template.Objects))
        {
            OnPropertyChanged(nameof(StatusBarSheetFormat));
        }
    }

    partial void OnSelectedObjectChanged(ITemplateObject value)
    {
        // Обновление Properties Panel
        PropertiesVm?.SetObject(value);
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _history.Undo();
        MarkDirty();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _history.Redo();
        MarkDirty();
    }

    private bool CanUndo() => _history.CanUndo;
    private bool CanRedo() => _history.CanRedo;

    [RelayCommand]
    private void DeleteSelectedObjects()
    {
        if (SelectedObject != null)
        {
            Template.Objects.Remove(SelectedObject);
            SelectedObject = null;
            MarkDirty();
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Zoom = Math.Min(Zoom * 1.2, 10.0);
        StatusMessage = $"Zoom: {Zoom * 100:F0}%";
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Zoom = Math.Max(Zoom / 1.2, 0.1);
        StatusMessage = $"Zoom: {Zoom * 100:F0}%";
    }

    [RelayCommand]
    private void FitToScreen()
    {
        Zoom = CalculateFitToScreenZoom();
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    private double CalculateFitToScreenZoom()
    {
        // Calculation logic
        return 1.0;
    }

    protected void MarkDirty()
    {
        if (!IsDirty)
        {
            IsDirty = true;
        }
    }

    public void Dispose()
    {
        this.PropertyChanged -= OnEditorViewModelPropertyChanged;
        
        if (Template != null)
        {
            Template.PropertyChanged -= Template_PropertyChanged;
        }
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 5.2.1: Ручная реализация INPC**

Создайте класс с ручной реализацией:
- 3 свойства (Name, Age, Email)
- INotifyPropertyChanged
- OnPropertyChanged в каждом setter

**Задача 5.2.2: CommunityToolkit базовый**

Создайте класс с CommunityToolkit:
- [ObservableObject] базовый класс
- [ObservableProperty] для 3 свойств
- Read-only свойство (FullName)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 5.2.3: AlsoNotifyChangeFor**

Создайте ViewModel:
- FirstName, LastName с [AlsoNotifyChangeFor(nameof(FullName))]
- Age с [AlsoNotifyChangeFor(nameof(IsAdult))]
- FullName, IsAdult (read-only)

**Задача 5.2.4: Partial Methods**

Реализуйте хуки:
- OnFirstNameChanging (валидация длины)
- OnFirstNameChanged (логирование)
- OnAgeChanged (проверка диапазона)

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 5.2.5: ObservableRecipient**

Создайте ViewModel с Messenger:
- [ObservableRecipient]
- Подписка на PropertyChangedMessage
- Отправка сообщений при изменении свойств
- Автоматическая отписка при деактивации

**Задача 5.2.6: Async Loading State**

Реализуйте загрузку данных:
- IsLoading, HasData, ErrorMessage
- [RelayCommand] для LoadAsync
- Обновление коллекции Items
- Обработка ошибок

---

### Решения

<details>
<summary>✅ Решение задачи 5.2.2</summary>

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private int _age;

    // Генерируется автоматически
    public string FullName => $"{FirstName} {LastName}";
}
```

**Использование:**
```csharp
var vm = new PersonViewModel();
vm.FirstName = "John";
vm.LastName = "Doe";
vm.Age = 30;

Console.WriteLine(vm.FullName); // John Doe
```
</details>

<details>
<summary>✅ Решение задачи 5.2.3</summary>

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    private string _firstName;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    private string _lastName;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsAdult))]
    private int _age;

    public string FullName => $"{FirstName} {LastName}";
    public bool IsAdult => Age >= 18;
}
```

**Сгенерированный код:**
```csharp
// FirstName setter автоматически вызывает OnPropertyChanged(nameof(FullName))
public string FirstName
{
    get => _firstName;
    set
    {
        if (!EqualityComparer<string>.Default.Equals(_firstName, value))
        {
            _firstName = value;
            OnPropertyChanged("FirstName");
            OnPropertyChanged("FullName");
        }
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 5.2.6</summary>

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class DataViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private ObservableCollection<DataItem> _items;

    public DataViewModel()
    {
        Items = new ObservableCollection<DataItem>();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await Task.Delay(1000); // Simulate API call

            Items.Clear();
            for (int i = 1; i <= 10; i++)
            {
                Items.Add(new DataItem { Name = $"Item {i}" });
            }

            HasData = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class DataItem : ObservableObject
{
    [ObservableProperty]
    private string _name;
}
```

**XAML:**
```xml
<StackPanel>
    <Button Content="Load Data" 
            Command="{Binding LoadDataCommand}"
            IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBool}}"/>
    
    <ProgressBar IsIndeterminate="True"
                 Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}"
                 Height="5"/>
    
    <TextBlock Text="{Binding ErrorMessage}"
               Foreground="Red"
               Margin="0,10,0,0"/>
    
    <ListBox ItemsSource="{Binding Items}"
             Visibility="{Binding HasData, Converter={StaticResource BoolToVisibility}}"
             Height="300"/>
</StackPanel>
```
</details>

---

## Ключевые выводы

✅ **INotifyPropertyChanged** — основа binding в WPF  
✅ **CommunityToolkit.Mvvm** — source generators для MVVM  
✅ **[ObservableProperty]** — генерация свойства с уведомлением  
✅ **Partial methods** — хуки OnPropertyChanging/OnPropertyChanged  
✅ **[AlsoNotifyChangeFor]** — уведомление зависимых свойств  
✅ **[ObservableRecipient]** — ViewModel с Messenger  
✅ **OnActivated/OnDeactivated** — жизненный цикл ViewModel

---

## Дополнительные ресурсы

- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Source Generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [ObservableProperty](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observableproperty)
