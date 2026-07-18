# Тема 9.3: Динамическая смена тем

### Теория

**Динамическая смена тем** — переключение между темами в runtime.

#### Ключевые компоненты

| Компонент | Описание |
|-----------|----------|
| **IThemeService** | Интерфейс сервиса тем |
| **ThemeService** | Реализация переключения |
| **DynamicResource** | Для обновления в runtime |
| **MergedDictionaries** | Для добавления/удаления тем |

### Примеры кода

#### Пример 1: IThemeService интерфейс

```csharp
public interface IThemeService
{
    bool IsDarkTheme { get; }
    string CurrentThemeName { get; }
    
    void ToggleTheme();
    void SetTheme(string themeName);
    void SetDarkTheme();
    void SetLightTheme();
    
    event EventHandler ThemeChanged;
}
```

#### Пример 2: ThemeService реализация

```csharp
public class ThemeService : IThemeService
{
    private readonly ResourceDictionary _lightTheme;
    private readonly ResourceDictionary _darkTheme;
    private bool _isDarkTheme;

    public bool IsDarkTheme => _isDarkTheme;
    public string CurrentThemeName => _isDarkTheme ? "Dark" : "Light";
    
    public event EventHandler ThemeChanged;

    public ThemeService()
    {
        _lightTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative)
        };

        _darkTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        // Загрузка начальной темы
        _isDarkTheme = true;
        Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
    }

    public void ToggleTheme()
    {
        if (_isDarkTheme)
        {
            SetLightTheme();
        }
        else
        {
            SetDarkTheme();
        }
    }

    public void SetDarkTheme()
    {
        Application.Current.Resources.MergedDictionaries.Remove(_lightTheme);
        
        if (!Application.Current.Resources.MergedDictionaries.Contains(_darkTheme))
        {
            Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
        }
        
        _isDarkTheme = true;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLightTheme()
    {
        Application.Current.Resources.MergedDictionaries.Remove(_darkTheme);
        
        if (!Application.Current.Resources.MergedDictionaries.Contains(_lightTheme))
        {
            Application.Current.Resources.MergedDictionaries.Add(_lightTheme);
        }
        
        _isDarkTheme = false;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetTheme(string themeName)
    {
        switch (themeName.ToLower())
        {
            case "light":
                SetLightTheme();
                break;
            case "dark":
                SetDarkTheme();
                break;
        }
    }
}
```

#### Пример 3: ThemeService с настройками

```csharp
public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private readonly ResourceDictionary _lightTheme;
    private readonly ResourceDictionary _darkTheme;
    private bool _isDarkTheme;

    public bool IsDarkTheme => _isDarkTheme;
    public string CurrentThemeName => _isDarkTheme ? "Dark" : "Light";
    
    public event EventHandler ThemeChanged;

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        _lightTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative)
        };

        _darkTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        // Загрузка сохранённой темы
        _isDarkTheme = _settingsService.Get("IsDarkTheme", true);
        LoadCurrentTheme();
    }

    private void LoadCurrentTheme()
    {
        if (_isDarkTheme)
        {
            Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
        }
        else
        {
            Application.Current.Resources.MergedDictionaries.Add(_lightTheme);
        }
    }

    public void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        
        // Сохранение настройки
        _settingsService.Set("IsDarkTheme", _isDarkTheme);
        
        // Переключение темы
        Application.Current.Resources.MergedDictionaries.Clear();
        LoadCurrentTheme();
        
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

#### Пример 4: ViewModel с ThemeService

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private string _currentThemeName;

    [ObservableProperty]
    private bool _isDarkTheme;

    public ICommand ToggleThemeCommand { get; }

    public MainViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        
        IsDarkTheme = _themeService.IsDarkTheme;
        CurrentThemeName = _themeService.CurrentThemeName;
        
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        
        // Подписка на изменения темы
        _themeService.ThemeChanged += (s, e) =>
        {
            IsDarkTheme = _themeService.IsDarkTheme;
            CurrentThemeName = _themeService.CurrentThemeName;
        };
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }
}
```

#### Пример 5: View с переключением тем

```xml
<Window x:Class="WpfApp.MainWindow">
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>
    
    <Grid>
        <ToolBar>
            <Button Content="🌓 Тема" 
                    Command="{Binding ToggleThemeCommand}"
                    ToolTip="Переключить тему"/>
            
            <TextBlock Text="{Binding CurrentThemeName}"
                       VerticalAlignment="Center"
                       Margin="10,0,0,0"/>
        </ToolBar>
        
        <!-- Content с DynamicResource -->
        <Grid Background="{DynamicResource WindowBackgroundBrush}">
            <TextBlock Text="Hello World!"
                       Foreground="{DynamicResource TextPrimaryBrush}"/>
        </Grid>
    </Grid>
</Window>
```

#### Пример 6: Анимация при смене темы

```csharp
public class ThemeService : IThemeService
{
    public async Task ToggleThemeWithAnimationAsync()
    {
        // Fade out
        await FadeOutAsync();
        
        // Смена темы
        ToggleTheme();
        
        // Fade in
        await FadeInAsync();
    }

    private async Task FadeOutAsync()
    {
        var mainWindow = Application.Current.MainWindow;
        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        
        await Task.Factory.StartNew(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.BeginAnimation(UIElement.OpacityProperty, animation);
            });
        }, TaskScheduler.Default);
        
        await Task.Delay(200);
    }

    private async Task FadeInAsync()
    {
        var mainWindow = Application.Current.MainWindow;
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            mainWindow.BeginAnimation(UIElement.OpacityProperty, animation);
        });
        
        await Task.Delay(200);
    }
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// ThemeService.cs из DotElectric
public class ThemeService : IThemeService
{
    private readonly Serilog.ILogger _logger;
    private readonly ResourceDictionary _lightTheme;
    private readonly ResourceDictionary _darkTheme;
    private bool _isDarkTheme;

    public bool IsDarkTheme => _isDarkTheme;
    public string CurrentThemeName => _isDarkTheme ? "Dark" : "Light";
    
    public event EventHandler ThemeChanged;

    public ThemeService(Serilog.ILogger logger)
    {
        _logger = logger;

        _lightTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Styles/LightTheme.xaml", UriKind.Relative)
        };

        _darkTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Styles/DarkTheme.xaml", UriKind.Relative)
        };

        // Загрузка темы по умолчанию
        _isDarkTheme = true;
        Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
        
        _logger.Information("ThemeService initialized with {Theme} theme", CurrentThemeName);
    }

    public void ToggleTheme()
    {
        _logger.Debug("Toggling theme from {FromTheme} to {ToTheme}", 
            _isDarkTheme ? "Dark" : "Light",
            _isDarkTheme ? "Light" : "Dark");

        if (_isDarkTheme)
        {
            SetLightTheme();
        }
        else
        {
            SetDarkTheme();
        }
    }

    private void SetDarkTheme()
    {
        try
        {
            Application.Current.Resources.MergedDictionaries.Remove(_lightTheme);
            
            if (!Application.Current.Resources.MergedDictionaries.Contains(_darkTheme))
            {
                Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
            }
            
            _isDarkTheme = true;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
            
            _logger.Information("Theme changed to Dark");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set dark theme");
            throw;
        }
    }

    private void SetLightTheme()
    {
        try
        {
            Application.Current.Resources.MergedDictionaries.Remove(_darkTheme);
            
            if (!Application.Current.Resources.MergedDictionaries.Contains(_lightTheme))
            {
                Application.Current.Resources.MergedDictionaries.Add(_lightTheme);
            }
            
            _isDarkTheme = false;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
            
            _logger.Information("Theme changed to Light");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set light theme");
            throw;
        }
    }
}
```

```csharp
// App.xaml.cs — регистрация ThemeService
protected override void OnStartup(StartupEventArgs e)
{
    var services = new ServiceCollection();
    
    // Регистрация сервисов
    services.AddSingleton<Serilog.ILogger>(Log.Logger);
    services.AddSingleton<IThemeService, ThemeService>();
    services.AddSingleton<MainViewModel>();
    
    var provider = services.BuildServiceProvider();
    
    // Инициализация ThemeService
    var themeService = provider.GetRequiredService<IThemeService>();
    
    var mainWindow = new MainWindow
    {
        DataContext = provider.GetRequiredService<MainViewModel>()
    };
    mainWindow.Show();
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 9.3.1: Simple Theme Toggle**

Создайте IThemeService:
- ToggleTheme метод
- IsDarkTheme свойство
- Переключение в App.xaml

**Задача 9.3.2: Theme Button**

Создайте кнопку:
- Command для переключения
- TextBlock с названием темы
- DynamicResource для цветов

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 9.3.3: ThemeService с Settings**

Создайте сервис:
- Сохранение темы в настройки
- Загрузка при старте
- ISettingsService

**Задача 9.3.4: Theme ViewModel**

Создайте ViewModel:
- CurrentThemeName property
- ToggleThemeCommand
- Подписка на ThemeChanged

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 9.3.5: Animated Theme Switch**

Реализуйте анимацию:
- Fade out перед сменой
- Fade in после смены
- Async/await

**Задача 9.3.6: Multiple Themes**

Создайте поддержку:
- 3+ темы (Light, Dark, Blue)
- ComboBox для выбора
- Динамическая загрузка

---

### Решения

<details>
<summary>✅ Решение задачи 9.3.1</summary>

```csharp
public class ThemeService : IThemeService
{
    private readonly ResourceDictionary _lightTheme;
    private readonly ResourceDictionary _darkTheme;
    private bool _isDarkTheme;

    public bool IsDarkTheme => _isDarkTheme;
    public event EventHandler ThemeChanged;

    public ThemeService()
    {
        _lightTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative)
        };

        _darkTheme = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
        _isDarkTheme = true;
    }

    public void ToggleTheme()
    {
        if (_isDarkTheme)
        {
            Application.Current.Resources.MergedDictionaries.Remove(_darkTheme);
            Application.Current.Resources.MergedDictionaries.Add(_lightTheme);
        }
        else
        {
            Application.Current.Resources.MergedDictionaries.Remove(_lightTheme);
            Application.Current.Resources.MergedDictionaries.Add(_darkTheme);
        }
        
        _isDarkTheme = !_isDarkTheme;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 9.3.3</summary>

```csharp
public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private bool _isDarkTheme;

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _isDarkTheme = _settingsService.Get("IsDarkTheme", true);
        LoadTheme();
    }

    private void LoadTheme()
    {
        var theme = _isDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(
            new ResourceDictionary 
            { 
                Source = new Uri($"Resources/Themes/{theme}", UriKind.Relative) 
            });
    }

    public void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        _settingsService.Set("IsDarkTheme", _isDarkTheme);
        LoadTheme();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
```
</details>

---

## Ключевые выводы

✅ **IThemeService** — интерфейс для сервиса тем  
✅ **MergedDictionaries** — добавление/удаление тем  
✅ **DynamicResource** — обновление в runtime  
✅ **ThemeChanged event** — уведомление об изменении  
✅ **SettingsService** — сохранение предпочтений  
✅ **ToggleTheme** — переключение между темами  
✅ **Анимация** — плавная смена тем

---

## Дополнительные ресурсы

- [DynamicResource](https://docs.microsoft.com/en-us/dotnet/api/system.windows.dynamicresource)
- [ResourceDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.windows.resourcedictionary)
- [Settings Service](https://docs.microsoft.com/en-us/dotnet/api/system.configuration.appsettings)
