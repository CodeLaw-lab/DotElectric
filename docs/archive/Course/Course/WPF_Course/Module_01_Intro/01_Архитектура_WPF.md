# Модуль 1: Введение в WPF

## Тема 1.1: Архитектура WPF

### Теория

**WPF (Windows Presentation Foundation)** — это фреймворк для создания десктопных приложений Windows с богатым UI.

#### Ключевые компоненты архитектуры

```
┌─────────────────────────────────────────────────────────┐
│                    Ваше приложение                       │
├─────────────────────────────────────────────────────────┤
│                    PresentationFramework                 │
│              (ViewModel, Commands, Controls)             │
├─────────────────────────────────────────────────────────┤
│                    PresentationCore                      │
│            (Visual Tree, hit-testing, input)             │
├─────────────────────────────────────────────────────────┤
│                    MilCore (Media Integration Layer)     │
│              (композиция, рендеринг, DirectX)            │
├─────────────────────────────────────────────────────────┤
│                    DirectX / Windows Compositor          │
└─────────────────────────────────────────────────────────┘
```

#### Логическое и визуальное дерево

**Логическое дерево** — иерархия элементов, объявленная в XAML:

```xml
<Window>
    <Grid>
        <Button Content="Click me"/>
        <TextBox Text="Hello"/>
    </Grid>
</Window>
```

**Визуальное дерево** — детализированная структура всех визуальных элементов:

```
Window
└── Grid
    ├── Border (от Button template)
    │   └── ContentPresenter
    │       └── TextBlock (для Button.Content)
    └── Border (от TextBox template)
        └── ScrollViewer
            └── TextBoxView
```

#### Система рендеринга

WPF использует **аппаратное ускорение** через DirectX:

1. **Retained mode** — система хранит информацию о объектах
2. **Device-independent units** — 1/96 дюйма (независимо от DPI)
3. **Перерисовка только изменённых областей**

### Примеры кода

#### Пример 1: Простое WPF-приложение

```xml
<!-- MainWindow.xaml -->
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Hello WPF" Height="300" Width="400">
    <Grid>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="Добро пожаловать в WPF!" 
                       FontSize="20" 
                       FontWeight="Bold"
                       HorizontalAlignment="Center"/>
            <Button Content="Нажми меня" 
                    Margin="0,20,0,0"
                    Padding="20,10"
                    Click="Button_Click"/>
        </StackPanel>
    </Grid>
</Window>
```

```csharp
// MainWindow.xaml.cs
using System.Windows;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Привет, WPF!", "Инфо", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
```

#### Пример 2: App.xaml — точка входа

```xml
<!-- App.xaml -->
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <!-- Глобальные ресурсы -->
    </Application.Resources>
</Application>
```

```csharp
// App.xaml.cs
using System.Windows;

namespace WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Глобальная обработка исключений
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ошибка: {args.Exception.Message}");
                args.Handled = true;
            };
        }
    }
}
```

#### Пример 3: Реальное приложение (DotElectric)

```xml
<!-- Упрощённая структура MainWindow из DotElectric -->
<Window x:Class="DotElectric.TemplateEditor.MainWindow">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>     <!-- Menu -->
            <RowDefinition Height="Auto"/>     <!-- ToolBar -->
            <RowDefinition Height="*"/>        <!-- Editor -->
            <RowDefinition Height="Auto"/>     <!-- StatusBar -->
        </Grid.RowDefinitions>
        
        <Menu Grid.Row="0">
            <MenuItem Header="_Файл">
                <MenuItem Header="_Новый" Command="{Binding NewCommand}"/>
                <MenuItem Header="_Открыть" Command="{Binding OpenCommand}"/>
            </MenuItem>
        </Menu>
        
        <ToolBarTray Grid.Row="1">
            <ToolBar>
                <Button Command="{Binding NewCommand}">
                    <Image Source="/icons/new.png"/>
                </Button>
            </ToolBar>
        </ToolBarTray>
        
        <TabControl Grid.Row="2">
            <!-- Вкладки редактора -->
        </TabControl>
        
        <StatusBar Grid.Row="3">
            <StatusBarItem>
                <TextBlock Text="{Binding StatusMessage}"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 1.1.1: Первое окно**

Создайте WPF-приложение с окном, содержащим:
- Заголовок "Моё первое WPF-приложение"
- Размер 800×600
- Кнопку с текстом "Привет!"
- При клике на кнопку — `MessageBox` с текстом "Работает!"

**Задача 1.1.2: App.xaml**

Добавьте в `App.xaml` глобальный обработчик исключений, который показывает `MessageBox` с текстом ошибки и предотвращает падение приложения.

---

#### 🟡 Средний уровень (1 час)

**Задача 1.1.3: Главное окно с Menu и StatusBar**

Создайте окно со структурой:
```
┌────────────────────────────────────┐
│ Menu: Файл, Правка, Вид, Справка   │
├────────────────────────────────────┤
│                                    │
│         Основная область           │
│         (просто Grid)              │
│                                    │
├────────────────────────────────────┤
│ StatusBar: "Готов" | Масштаб: 100% │
└────────────────────────────────────┘
```

Требования:
- `Menu` с `MenuItem` (хотя бы по 2 пункта в каждом)
- `StatusBar` с двумя `StatusBarItem`
- Использовать `Grid` с `RowDefinitions`

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 1.1.4: Анализ визуального дерева**

Напишите утилиту для визуализации визуального дерева:

```csharp
public static class VisualTreeHelper
{
    public static void PrintVisualTree(DependencyObject element, int indent = 0)
    {
        // Рекурсивно обходит визуальное дерево
        // Выводит в Debug.WriteLine с отступами
        // Пример вывода:
        // Window
        //   Grid
        //     Border
        //       ContentPresenter
        //         TextBlock
    }
}
```

Используйте её для анализа `Button` — выведите полное визуальное дерево кнопки в Output Window.

**Задача 1.1.5: Custom Window с Chrome**

Создайте окно без стандартной рамки (`WindowStyle="None"`) с кастомной заголовочной панелью:
- Синяя полоса сверху (высота 30px)
- Название приложения слева
- Кнопки "Свернуть", "Развернуть", "Закрыть" справа
- Реализуйте перетаскивание окна за заголовок (через `MouseLeftButtonDown` + `DragMove()`)

---

### Решения

<details>
<summary>✅ Решение задачи 1.1.1</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Моё первое WPF-приложение" 
        Height="600" Width="800">
    <Grid>
        <Button Content="Привет!" 
                HorizontalAlignment="Center" 
                VerticalAlignment="Center"
                Click="Button_Click"/>
    </Grid>
</Window>
```

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    MessageBox.Show("Работает!");
}
```
</details>

<details>
<summary>✅ Решение задачи 1.1.2</summary>

```xml
<!-- App.xaml -->
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
</Application>
```

```csharp
// App.xaml.cs
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show(
                $"Произошла ошибка:\n{args.Exception.Message}", 
                "Ошибка", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            args.Handled = true; // Предотвращаем падение
        };
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 1.1.3</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="WPF App" Height="600" Width="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Menu -->
        <Menu Grid.Row="0">
            <MenuItem Header="_Файл">
                <MenuItem Header="_Новый"/>
                <MenuItem Header="_Открыть"/>
                <MenuItem Header="_Сохранить"/>
            </MenuItem>
            <MenuItem Header="_Правка">
                <MenuItem Header="_Копировать"/>
                <MenuItem Header="_Вставить"/>
            </MenuItem>
            <MenuItem Header="_Вид">
                <MenuItem Header="_Масштаб"/>
            </MenuItem>
            <MenuItem Header="_Справка">
                <MenuItem Header="_О программе"/>
            </MenuItem>
        </Menu>

        <!-- Content -->
        <Grid Grid.Row="1" Background="White">
            <!-- Основная область -->
        </Grid>

        <!-- StatusBar -->
        <StatusBar Grid.Row="3">
            <StatusBarItem>
                <TextBlock Text="Готов"/>
            </StatusBarItem>
            <Separator/>
            <StatusBarItem>
                <TextBlock Text="Масштаб: 100%"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```
</details>

<details>
<summary>✅ Решение задачи 1.1.4</summary>

```csharp
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

public static class VisualTreeHelper
{
    public static void PrintVisualTree(DependencyObject element, int indent = 0)
    {
        if (element == null) return;
        
        var name = element.GetType().Name;
        
        // Пробуем получить имя элемента
        if (element is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
        {
            name = $"{name} (x:Name={fe.Name})";
        }
        
        Debug.WriteLine(new string(' ', indent * 2) + name);
        
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
            PrintVisualTree(child, indent + 1);
        }
    }
}

// Использование в MainWindow:
protected override void OnContentRendered(EventArgs e)
{
    base.OnContentRendered(e);
    Debug.WriteLine("=== Визуальное дерево Button ===");
    var button = this.FindName("MyButton") as Button;
    VisualTreeHelper.PrintVisualTree(button);
}
```
</details>

<details>
<summary>✅ Решение задачи 1.1.5</summary>

```xml
<Window x:Class="WpfApp.CustomWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        Height="600" Width="800">
    <Border BorderBrush="Gray" BorderThickness="1" Background="White">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="30"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Заголовок -->
            <Border Grid.Row="0" Background="#0078D4" 
                    MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                <Grid>
                    <TextBlock Text="Моё приложение" 
                               Foreground="White"
                               VerticalAlignment="Center"
                               Margin="10,0,0,0"/>
                    
                    <StackPanel Orientation="Horizontal" 
                                HorizontalAlignment="Right"
                                VerticalAlignment="Center">
                        <Button Content="─" Width="30" Click="Minimize_Click"/>
                        <Button Content="□" Width="30" Click="Maximize_Click"/>
                        <Button Content="✕" Width="30" Click="Close_Click"/>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Контент -->
            <ContentControl Grid.Row="1" Content="{TemplateBinding Content}"/>
        </Grid>
    </Border>
</Window>
```

```csharp
private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        // Двойной клик — разворачиваем/сворачиваем
        WindowState = WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }
    else
    {
        DragMove();
    }
}

private void Minimize_Click(object sender, RoutedEventArgs e) 
    => WindowState = WindowState.Minimized;

private void Maximize_Click(object sender, RoutedEventArgs e) 
    => WindowState = WindowState == WindowState.Maximized 
        ? WindowState.Normal 
        : WindowState.Maximized;

private void Close_Click(object sender, RoutedEventArgs e) 
    => Close();
```
</details>

---

## Ключевые выводы

✅ **WPF использует DirectX** для аппаратного ускорения  
✅ **Логическое дерево** — ваша XAML-иерархия, **визуальное** — детализированная структура  
✅ **Device-independent units** (1/96") обеспечивают DPI-независимость  
✅ **Retained mode** — система хранит информацию об объектах и перерисовывает только изменённые области

---

## Дополнительные ресурсы

- [Microsoft Docs: WPF Architecture](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/)
- [Visual Tree and Logical Tree](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/trees)
- [DPI Awareness in WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/dpi)
