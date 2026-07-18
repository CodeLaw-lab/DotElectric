# Тема 5.1: Основы MVVM (Model-View-ViewModel)

### Теория

**MVVM (Model-View-ViewModel)** — архитектурный паттерн для разделения UI и бизнес-логики.

#### Компоненты MVVM

```
┌─────────────────────────────────────────────────────────┐
│                      MVVM Architecture                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────┐      Data Binding      ┌──────────────┐  │
│  │   View   │ ◄────────────────────► │  ViewModel   │  │
│  │  (XAML)  │      Commands          │   (C#)       │  │
│  └──────────┘                        └──────────────┘  │
│       ▲                                      │          │
│       │                                      ▼          │
│       │                               ┌──────────────┐  │
│       │                               │    Model     │  │
│       └──────────────────────────────►│   (Data)     │  │
│              Direct Access            └──────────────┘  │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

#### Компоненты

| Компонент | Описание | Пример |
|-----------|----------|--------|
| **Model** | Данные и бизнес-логика | `Person`, `Order`, `Product` |
| **View** | UI (XAML), отображение | `MainWindow.xaml` |
| **ViewModel** | Посредник, состояние View | `MainViewModel`, `EditorViewModel` |

#### Преимущества MVVM

✅ **Разделение ответственности** — UI и логика разделены  
✅ **Тестируемость** — ViewModel можно тестировать без UI  
✅ **Поддержка** — легче изменять UI без изменения логики  
✅ **Data Binding** — автоматическая синхронизация  
✅ **Designer-Developer workflow** — дизайнер работает в XAML, разработчик в C#

#### Недостатки MVVM

❌ **Избыточность** для простых приложений  
❌ **Сложность** для начинающих  
❌ **Debugging** — сложнее отлаживать binding  
❌ **Performance** — overhead от binding для больших коллекций

### Примеры кода

#### Пример 1: Простой MVVM без фреймворка

```csharp
// Model
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

// ViewModel
public class PersonViewModel : INotifyPropertyChanged
{
    private Person _person;
    private string _firstName;
    private string _lastName;

    public PersonViewModel()
    {
        _person = new Person();
        SaveCommand = new RelayCommand(Save, CanSave);
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(FullName));
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(FullName));
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public ICommand SaveCommand { get; }

    private bool CanSave() => !string.IsNullOrWhiteSpace(FirstName) && 
                               !string.IsNullOrWhiteSpace(LastName);

    private void Save()
    {
        _person.FirstName = FirstName;
        _person.LastName = LastName;
        MessageBox.Show($"Saved: {FullName}");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Command implementation
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object parameter) => _execute();

    public void NotifyCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

```xml
<!-- View -->
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfApp"
        Title="MVVM Example" Height="300" Width="400">
    
    <Window.DataContext>
        <local:PersonViewModel/>
    </Window.DataContext>
    
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <TextBlock Grid.Row="0" Grid.Column="0" Text="First Name:" Margin="5"/>
        <TextBox Grid.Row="0" Grid.Column="1" 
                 Text="{Binding FirstName, UpdateSourceTrigger=PropertyChanged}"
                 Margin="5"/>
        
        <TextBlock Grid.Row="1" Grid.Column="0" Text="Last Name:" Margin="5"/>
        <TextBox Grid.Row="1" Grid.Column="1" 
                 Text="{Binding LastName, UpdateSourceTrigger=PropertyChanged}"
                 Margin="5"/>
        
        <TextBlock Grid.Row="2" Grid.Column="0" Text="Full Name:" Margin="5"/>
        <TextBlock Grid.Row="2" Grid.Column="1" 
                   Text="{Binding FullName}"
                   FontWeight="Bold"
                   Margin="5"/>
        
        <Button Grid.Row="3" Grid.Column="1" 
                Content="Save"
                Command="{Binding SaveCommand}"
                Padding="20,5"
                Margin="5"/>
    </Grid>
</Window>
```

#### Пример 2: MVVM с CommunityToolkit.Mvvm

```csharp
// Установка пакета: Install-Package CommunityToolkit.Mvvm

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// ViewModel с source generators
public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private bool _isLoading;

    public string FullName => $"{FirstName} {LastName}";

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // Save logic
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(FirstName);

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        await Task.Delay(1000); // Simulate load
        IsLoading = false;
    }

    partial void OnFirstNameChanged(string value)
    {
        // Hook for FirstName change
        Console.WriteLine($"First name changed to: {value}");
    }
}
```

```xml
<!-- View с CommunityToolkit -->
<Window x:Class="WpfApp.MainWindow">
    <Window.DataContext>
        <local:PersonViewModel/>
    </Window.DataContext>
    
    <StackPanel Margin="20">
        <TextBox Text="{Binding FirstName, UpdateSourceTrigger=PropertyChanged}"
                 Margin="0,0,0,10"/>
        <TextBox Text="{Binding LastName, UpdateSourceTrigger=PropertyChanged}"
                 Margin="0,0,0,10"/>
        
        <TextBlock Text="{Binding FullName}" 
                   FontWeight="Bold"
                   Margin="0,0,0,10"/>
        
        <Button Content="Save" 
                Command="{Binding SaveCommand}"/>
        
        <Button Content="Load" 
                Command="{Binding LoadDataCommand}"
                Margin="0,10,0,0"/>
        
        <ProgressBar IsIndeterminate="True"
                     Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}"
                     Height="5"
                     Margin="0,10,0,0"/>
    </StackPanel>
</Window>
```

#### Пример 3: Locator паттерн

```csharp
// ViewModelLocator
public class ViewModelLocator
{
    private static ViewModelLocator _instance;
    private readonly IServiceProvider _serviceProvider;

    public static ViewModelLocator Instance => _instance;

    public MainViewModel MainViewModel => 
        _serviceProvider.GetRequiredService<MainViewModel>();

    public EditorViewModel EditorViewModel => 
        _serviceProvider.GetRequiredService<EditorViewModel>();

    public ViewModelLocator(IServiceProvider serviceProvider)
    {
        _instance = this;
        _serviceProvider = serviceProvider;
    }
}

// App.xaml.cs
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        new ViewModelLocator(_serviceProvider);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<EditorViewModel>();
        services.AddSingleton<ITemplateService, TemplateService>();
    }
}
```

```xml
<!-- App.xaml -->
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:WpfApp">
    <Application.Resources>
        <local:ViewModelLocator x:Key="Locator"/>
    </Application.Resources>
</Application>

<!-- MainWindow.xaml -->
<Window x:Class="WpfApp.MainWindow"
        DataContext="{Binding MainViewModel, Source={StaticResource Locator}}">
    <!-- View content -->
</Window>
```

#### Пример 4: Code-Behind vs MVVM

```csharp
// ❌ Code-Behind approach (избегайте)
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Бизнес-логика в UI слое ❌
        var name = nameTextBox.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Name is required");
            return;
        }
        
        // Save to database...
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        // Загрузка данных в UI коде ❌
        var data = LoadFromDatabase();
        resultTextBlock.Text = data;
    }
}
```

```csharp
// ✅ MVVM approach
// ViewModel
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _result;

    [ObservableProperty]
    private string _errorMessage;

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = null;
        
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required";
            return;
        }
        
        // Save logic
        Result = $"Saved: {Name}";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Result = await LoadFromDatabaseAsync();
    }
}
```

```xml
<!-- View -->
<Window x:Class="WpfApp.MainWindow">
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>
    
    <StackPanel Margin="20">
        <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"
                 Margin="0,0,0,10"/>
        
        <TextBlock Text="{Binding ErrorMessage}"
                   Foreground="Red"
                   Margin="0,0,0,10"/>
        
        <Button Content="Save" Command="{Binding SaveCommand}" Margin="0,0,0,10"/>
        <Button Content="Load" Command="{Binding LoadCommand}"/>
        
        <TextBlock Text="{Binding Result}" 
                   FontWeight="Bold"
                   Margin="0,10,0,0"/>
    </StackPanel>
</Window>
```

#### Пример 5: Реальный ViewModel из DotElectric

```csharp
// EditorViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class EditorViewModel : ObservableObject, IDisposable
{
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private Template _template;

    [ObservableProperty]
    private ITemplateObject _selectedObject;

    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _title;

    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }

    public EditorViewModel(
        ITemplateService templateService,
        IFileService fileService)
    {
        _templateService = templateService;
        _fileService = fileService;

        UndoCommand = new RelayCommand(Undo, CanUndo);
        RedoCommand = new RelayCommand(Redo, CanRedo);
        DeleteCommand = new RelayCommand(DeleteSelectedObjects);
        ZoomInCommand = new RelayCommand(ZoomIn);
        ZoomOutCommand = new RelayCommand(ZoomOut);
    }

    private bool CanUndo() => History.CanUndo;
    private bool CanRedo() => History.CanRedo;

    private void Undo()
    {
        History.Undo();
        IsDirty = true;
    }

    private void Redo()
    {
        History.Redo();
        IsDirty = true;
    }

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

    private void ZoomIn()
    {
        Zoom = Math.Min(Zoom * 1.2, 10.0);
    }

    private void ZoomOut()
    {
        Zoom = Math.Max(Zoom / 1.2, 0.1);
    }

    partial void OnTitleChanged(string value)
    {
        // Update window title
    }

    protected void MarkDirty()
    {
        IsDirty = true;
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 5.1.1: Простой ViewModel**

Создайте ViewModel с:
- Свойство Name (string)
- Свойство Count (int)
- Кнопка Increment (увеличивает Count)
- Реализуйте INotifyPropertyChanged вручную

**Задача 5.1.2: Calculator ViewModel**

Создайте ViewModel калькулятора:
- Свойства Value1, Value2 (double)
- Commands: Add, Subtract, Multiply, Divide
- Свойство Result для отображения

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 5.1.3: Person Manager с CommunityToolkit**

Создайте ViewModel с CommunityToolkit.Mvvm:
- [ObservableProperty] для FirstName, LastName, Age
- [RelayCommand] для Save, Reset
- Валидация: Name не пустой, Age 0-150
- FullName (read-only)

**Задача 5.1.4: Counter с историей**

Создайте ViewModel счётчика:
- CurrentValue, History (ObservableCollection)
- Commands: Increment, Decrement, Reset, ShowHistory
- Сохранение истории изменений

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 5.1.5: Multi-ViewModel Navigation**

Создайте навигацию между ViewModel:
- MainViewModel с коллекцией "Pages"
- NavigationService для переключения
- HomeViewModel, SettingsViewModel, AboutViewModel
- ActivePage property

**Задача 5.1.6: Undo/Redo Framework**

Реализуйте систему Undo/Redo:
- ICommand с Undo/Redo методами
- CommandHistory (stack)
- Поддержка составных команд
- Интеграция с ViewModel

---

### Решения

<details>
<summary>✅ Решение задачи 5.1.1</summary>

```csharp
public class CounterViewModel : INotifyPropertyChanged
{
    private string _name;
    private int _count;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            OnPropertyChanged(nameof(Count));
        }
    }

    public ICommand IncrementCommand { get; }

    public CounterViewModel()
    {
        IncrementCommand = new RelayCommand(Increment);
    }

    private void Increment()
    {
        Count++;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```
</details>

<details>
<summary>✅ Решение задачи 5.1.2</summary>

```csharp
public class CalculatorViewModel : INotifyPropertyChanged
{
    private double _value1;
    private double _value2;
    private double _result;

    public double Value1
    {
        get => _value1;
        set { _value1 = value; OnPropertyChanged(); }
    }

    public double Value2
    {
        get => _value2;
        set { _value2 = value; OnPropertyChanged(); }
    }

    public double Result
    {
        get => _result;
        set { _result = value; OnPropertyChanged(); }
    }

    public ICommand AddCommand { get; }
    public ICommand SubtractCommand { get; }
    public ICommand MultiplyCommand { get; }
    public ICommand DivideCommand { get; }

    public CalculatorViewModel()
    {
        AddCommand = new RelayCommand(() => Calculate((a, b) => a + b));
        SubtractCommand = new RelayCommand(() => Calculate((a, b) => a - b));
        MultiplyCommand = new RelayCommand(() => Calculate((a, b) => a * b));
        DivideCommand = new RelayCommand(() => Calculate((a, b) => a / b));
    }

    private void Calculate(Func<double, double, double> operation)
    {
        Result = operation(Value1, Value2);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```
</details>

---

## Ключевые выводы

✅ **MVVM** разделяет UI (View) и логику (ViewModel)  
✅ **Model** — данные и бизнес-логика  
✅ **View** ↔ **ViewModel** через Data Binding и Commands  
✅ **INotifyPropertyChanged** — уведомление об изменениях свойств  
✅ **CommunityToolkit.Mvvm** — source generators для MVVM  
✅ **Locator** — централизованное создание ViewModel  
✅ **Code-Behind** — избегайте бизнес-логики в UI слое

---

## Дополнительные ресурсы

- [MVVM Pattern](https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [ObservableProperty](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observableproperty)
