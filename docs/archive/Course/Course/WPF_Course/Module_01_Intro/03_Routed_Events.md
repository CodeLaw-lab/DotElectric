# Тема 1.3: Routed Events (маршрутизированные события)

### Теория

**Routed Events** — это механизм WPF для обработки событий, которые могут проходить через несколько элементов в дереве элементов.

#### Три стратегии маршрутизации

```
1. **Direct** (прямое) — как обычное .NET событие
   Источник → Обработчик на источнике

2. **Bubbling** (всплывающее) — от источника к корню
   Button → Grid → Window

3. **Tunneling** (нисходящее) — от корня к источнику
   Window → Grid → Button
```

#### Визуализация маршрутизации

```
┌────────────────────────────────────────┐
│            Window (root)               │
│  ┌──────────────────────────────────┐  │
│  │              Grid                │  │
│  │  ┌────────────────────────────┐  │  │
│  │  │         Border             │  │  │
│  │  │  ┌──────────────────────┐  │  │  │
│  │  │  │       Button         │  │  │  │
│  │  │  │   (source event)     │  │  │  │
│  │  │  └──────────────────────┘  │  │  │
│  │  └────────────────────────────┘  │  │
│  └──────────────────────────────────┘  │
└────────────────────────────────────────┘

Bubbling:  Button → Border → Grid → Window
Tunneling: Window → Grid → Border → Button
```

#### Convention naming

- **Tunneling события** начинаются с `Preview`: `PreviewMouseDown`, `PreviewKeyDown`
- **Bubbling события** — обычные названия: `MouseDown`, `KeyDown`

#### Почему это важно?

1. **Preview-события** позволяют перехватить действие ДО того, как элемент его обработает
2. **Bubbling-события** позволяют обрабатывать события дочерних элементов на родителе
3. **Handled flag** позволяет остановить маршрутизацию

### Примеры кода

#### Пример 1: Bubbling event (MouseUp)

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        MouseUp="Window_MouseUp">
    
    <Grid MouseUp="Grid_MouseUp">
        <Button Content="Нажми меня" 
                MouseUp="Button_MouseUp"/>
    </Grid>
</Window>
```

```csharp
private void Button_MouseUp(object sender, MouseButtonEventArgs e)
{
    MessageBox.Show("Button MouseUp");
    // e.Handled = true; // Если true — событие не пойдёт дальше
}

private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
{
    MessageBox.Show("Grid MouseUp");
}

private void Window_MouseUp(object sender, MouseButtonEventArgs e)
{
    MessageBox.Show("Window MouseUp");
}

// Порядок вывода (если e.Handled не установлен):
// 1. Button MouseUp
// 2. Grid MouseUp
// 3. Window MouseUp
```

#### Пример 2: Tunneling event (PreviewKeyDown)

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        PreviewKeyDown="Window_PreviewKeyDown">
    
    <Grid PreviewKeyDown="Grid_PreviewKeyDown">
        <TextBox PreviewKeyDown="TextBox_PreviewKeyDown"/>
    </Grid>
</Window>
```

```csharp
private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
{
    Debug.WriteLine("Window PreviewKeyDown");
    
    // Блокируем определённые клавиши
    if (e.Key == Key.F12)
    {
        MessageBox.Show("F12 заблокирована!");
        e.Handled = true; // Останавливаем маршрутизацию
    }
}

private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
{
    Debug.WriteLine("Grid PreviewKeyDown");
}

private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
{
    Debug.WriteLine("TextBox PreviewKeyDown");
}

// Порядок вывода (если e.Handled не установлен):
// 1. Window PreviewKeyDown
// 2. Grid PreviewKeyDown
// 3. TextBox PreviewKeyDown
```

#### Пример 3: Обработка Handled flag

```csharp
// Подписка на уже обработанные события (через AddHandler)
public MainWindow()
{
    InitializeComponent();
    
    // Обычная подписка — не получит обработанные события
    myButton.Click += MyButton_Click;
    
    // Подписка с handledEventsToo = true — получит ВСЕ события
    myButton.AddHandler(
        Button.ClickEvent, 
        new RoutedEventHandler(MyButton_ClickHandled), 
        handledEventsToo: true
    );
}

private void MyButton_Click(object sender, RoutedEventArgs e)
{
    // Не вызовется, если кто-то установил e.Handled = true
}

private void MyButton_ClickHandled(object sender, RoutedEventArgs e)
{
    // Вызовется ДАЖЕ если e.Handled = true
    Debug.WriteLine($"Click handled! Handled={e.Handled}");
}
```

#### Пример 4: Создание собственного routed event

```csharp
public class CustomButton : Button
{
    // 1. Регистрация routed event
    public static readonly RoutedEvent CustomClickEvent =
        EventManager.RegisterRoutedEvent(
            "CustomClick",
            RoutingStrategy.Bubble,      // Bubble, Tunnel или Direct
            typeof(RoutedEventHandler),
            typeof(CustomButton)
        );

    // 2. CLR wrapper для подписки
    public event RoutedEventHandler CustomClick
    {
        add => AddHandler(CustomClickEvent, value);
        remove => RemoveHandler(CustomClickEvent, value);
    }

    // 3. Генерация события
    protected override void OnClick()
    {
        base.OnClick();
        
        // Создаём и поднимаем событие
        var args = new RoutedEventArgs(CustomClickEvent, this);
        RaiseEvent(args);
    }
}
```

```xml
<!-- Использование -->
<local:CustomButton Content="Click me" 
                    local:CustomButton.CustomClick="CustomButton_CustomClick"/>
```

```csharp
private void CustomButton_CustomClick(object sender, RoutedEventArgs e)
{
    MessageBox.Show("Custom click!");
}
```

#### Пример 5: Реальное использование в DotElectric

```csharp
// В EditorCanvas.xaml.cs — обработка ввода через Preview-события
public partial class EditorCanvas : UserControl
{
    public EditorCanvas()
    {
        InitializeComponent();
        
        // Подписываемся на Preview-события для перехвата ввода
        PreviewMouseDown += EditorCanvas_PreviewMouseDown;
        PreviewMouseMove += EditorCanvas_PreviewMouseMove;
        PreviewMouseUp += EditorCanvas_PreviewMouseUp;
        PreviewKeyDown += EditorCanvas_PreviewKeyDown;
    }

    private void EditorCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Перехватываем нажатие ДО того, как его обработают дочерние элементы
        var tool = ViewModel?.ActiveTool;
        
        if (tool != null)
        {
            var position = GetMousePosition(e);
            tool.OnMouseDown(position, e.ChangedButton, e);
            e.Handled = true; // Останавливаем дальнейшую маршрутизацию
        }
    }

    private void EditorCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Обработка горячих клавиш
        switch (e.Key)
        {
            case Key.Delete:
                ViewModel?.DeleteSelectedObjectsCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.Escape:
                ViewModel?.CancelToolCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.Space:
                // Временное переключение на PanTool
                PushTool(new PanTool());
                e.Handled = true;
                break;
        }
    }
}
```

```xml
<!-- EditorCanvas.xaml -->
<UserControl x:Class="DotElectric.TemplateEditor.Views.EditorCanvas"
             PreviewMouseDown="EditorCanvas_PreviewMouseDown"
             PreviewMouseMove="EditorCanvas_PreviewMouseMove"
             PreviewMouseUp="EditorCanvas_PreviewMouseUp"
             PreviewKeyDown="EditorCanvas_PreviewKeyDown">
    
    <Canvas x:Name="DrawingCanvas">
        <!-- Объекты рисуются здесь -->
        
        <!-- TextBox для inline-редактирования -->
        <TextBox x:Name="InlineTextEditor"
                 PreviewKeyDown="InlineTextEditor_PreviewKeyDown"/>
    </Canvas>
</UserControl>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 1.3.1: Изучение маршрутизации**

Создайте окно с вложенной структурой:
```
Window → Grid → Border → StackPanel → Button
```

Подпишите `PreviewMouseDown` и `MouseDown` на каждом уровне. Выводите в `Output Window` название элемента и тип события. Убедитесь, что:
- Preview-события идут сверху вниз
- Bubbling-события идут снизу вверх

**Задача 1.3.2: Блокировка клавиш**

Создайте окно с `TextBox`. Реализуйте блокировку:
- Клавиши `F1`–`F12` (показывать `MessageBox` "F-клавиши заблокированы")
- `Ctrl+S` (перехватывать, показывать "Save intercepted")
- `Alt+F4` (запретить закрытие окна)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 1.3.3: Глобальный перехватчик клавиш**

Создайте Attached Behavior `GlobalKeyInterceptor`:
- Attached Property `InterceptKeys` (коллекция `Key`)
- Attached Property `Intercepted` ( routed event)
- При нажатии указанной клавиши — поднимать событие `Intercepted`
- Использовать `PreviewKeyDown` с `e.Handled = true`

```xml
<Window local:GlobalKeyInterceptor.InterceptKeys="F1,F2,F3"
        local:GlobalKeyInterceptor.Intercepted="OnKeyIntercepted">
```

**Задача 1.3.4: Custom Routed Event с данными**

Создайте `SmartButton` с routed event `SmartClick`:
- Event args содержат `ClickCount`, `ShiftPressed`, `CtrlPressed`
- Поддержка bubbling-маршрутизации
- Пример использования: подсчёт двойных кликов с зажатым Ctrl

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 1.3.5: Система команд через routed events**

Реализуйте мини-аналог `ICommand` через routed events:
- Attached Property `CommandExecutor.Command`
- Attached Property `CommandExecutor.CommandParameter`
- Routed event `ExecuteCommand` (bubbling)
- Обработчик на Window уровне, который ищет команду в DataContext

```xml
<Button local:CommandExecutor.Command="{Binding SaveCommand}"
        local:CommandExecutor.CommandParameter="SelectedFile"/>
```

**Задача 1.3.6: Event-to-Command Behavior**

Реализуйте `EventToCommand` behavior:
- Attached Property `EventName` (строка)
- Attached Property `Command` (ICommand)
- Attached Property `CommandParameter`
- Использование reflection для подписки на событие по имени
- Конвертация `EventArgs` в `CommandParameter`

```xml
<TextBox local:EventToCommand.EventName="TextChanged"
         local:EventToCommand.Command="{Binding TextChangedCommand}"
         local:EventToCommand.CommandParameter="{Binding Text, RelativeSource={RelativeSource Self}}"/>
```

---

### Решения

<details>
<summary>✅ Решение задачи 1.3.1</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        PreviewMouseDown="Window_PreviewMouseDown"
        MouseDown="Window_MouseDown">
    <Grid PreviewMouseDown="Grid_PreviewMouseDown"
          MouseDown="Grid_MouseDown">
        <Border PreviewMouseDown="Border_PreviewMouseDown"
                MouseDown="Border_MouseDown">
            <StackPanel PreviewMouseDown="StackPanel_PreviewMouseDown"
                        MouseDown="StackPanel_MouseDown">
                <Button Content="Click me"
                        PreviewMouseDown="Button_PreviewMouseDown"
                        MouseDown="Button_MouseDown"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

```csharp
private void Log(string element, string eventType)
{
    System.Diagnostics.Debug.WriteLine($"{element} - {eventType}");
}

private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) 
    => Log("Window", "PreviewMouseDown (Tunneling)");
private void Window_MouseDown(object sender, MouseButtonEventArgs e) 
    => Log("Window", "MouseDown (Bubbling)");

private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e) 
    => Log("Grid", "PreviewMouseDown (Tunneling)");
private void Grid_MouseDown(object sender, MouseButtonEventArgs e) 
    => Log("Grid", "MouseDown (Bubbling)");

// ... аналогично для Border, StackPanel, Button

// Output:
// Window - PreviewMouseDown (Tunneling)
// Grid - PreviewMouseDown (Tunneling)
// Border - PreviewMouseDown (Tunneling)
// StackPanel - PreviewMouseDown (Tunneling)
// Button - PreviewMouseDown (Tunneling)
// Button - MouseDown (Bubbling)
// StackPanel - MouseDown (Bubbling)
// Border - MouseDown (Bubbling)
// Grid - MouseDown (Bubbling)
// Window - MouseDown (Bubbling)
```
</details>

<details>
<summary>✅ Решение задачи 1.3.2</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        PreviewKeyDown="Window_PreviewKeyDown">
    <TextBox x:Name="textBox"/>
</Window>
```

```csharp
private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
{
    // Блокировка F1-F12
    if (e.Key >= Key.F1 && e.Key <= Key.F12)
    {
        MessageBox.Show("F-клавиши заблокированы!");
        e.Handled = true;
        return;
    }
    
    // Блокировка Ctrl+S
    if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
    {
        MessageBox.Show("Save intercepted!");
        e.Handled = true;
        return;
    }
    
    // Блокировка Alt+F4
    if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
    {
        e.Handled = true;
        return;
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 1.3.3</summary>

```csharp
public static class GlobalKeyInterceptor
{
    public static readonly DependencyProperty InterceptKeysProperty =
        DependencyProperty.RegisterAttached(
            "InterceptKeys",
            typeof(IList<Key>),
            typeof(GlobalKeyInterceptor),
            new PropertyMetadata(null, OnInterceptKeysChanged)
        );

    public static readonly DependencyProperty InterceptedEvent =
        EventManager.RegisterRoutedEvent(
            "Intercepted",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(GlobalKeyInterceptor)
        );

    public static void SetInterceptKeys(DependencyObject element, IList<Key> value)
        => element.SetValue(InterceptKeysProperty, value);
    
    public static IList<Key> GetInterceptKeys(DependencyObject element)
        => (IList<Key>)element.GetValue(InterceptKeysProperty);

    public static event RoutedEventHandler Intercepted
    {
        add => EventManager.RegisterClassHandler(
            typeof(UIElement), InterceptedEvent, value);
        remove => EventManager.RemoveClassHandler(
            typeof(UIElement), InterceptedEvent, value);
    }

    private static void OnInterceptKeysChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.PreviewKeyDown -= Element_PreviewKeyDown;
            element.PreviewKeyDown += Element_PreviewKeyDown;
        }
    }

    private static void Element_PreviewKeyDown(
        object sender, 
        KeyEventArgs e)
    {
        var element = (UIElement)sender;
        var keys = GetInterceptKeys(element);
        
        if (keys != null && keys.Contains(e.Key))
        {
            // Создаём event args с информацией о нажатой клавише
            var args = new KeyInterceptedEventArgs(InterceptedEvent, element, e.Key);
            element.RaiseEvent(args);
            e.Handled = true;
        }
    }
}

public class KeyInterceptedEventArgs : RoutedEventArgs
{
    public Key Key { get; }

    public KeyInterceptedEventArgs(
        RoutedEvent routedEvent, 
        object source, 
        Key key) : base(routedEvent, source)
    {
        Key = key;
    }
}
```

```xml
<!-- Использование -->
<Window local:GlobalKeyInterceptor.InterceptKeys="F1,F2,Delete"
        local:GlobalKeyInterceptor.Intercepted="OnKeyIntercepted">
    <TextBox/>
</Window>
```

```csharp
private void OnKeyIntercepted(object sender, RoutedEventArgs e)
{
    var args = (KeyInterceptedEventArgs)e;
    MessageBox.Show($"Перехвачена клавиша: {args.Key}");
}
```
</details>

---

## Ключевые выводы

✅ **Bubbling events** идут от источника к корню (используются чаще всего)  
✅ **Tunneling events** (Preview) идут от корня к источнику (для перехвата)  
✅ **e.Handled = true** останавливает маршрутизацию  
✅ **AddHandler(event, handler, handledEventsToo: true)** — подписка на обработанные события  
✅ **Свои routed events** регистрируются через `EventManager.RegisterRoutedEvent`

---

## Дополнительные ресурсы

- [Routed Events Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/input/routed-events-overview)
- [Routing Strategies](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/input/routing-strategies)
- [Register Routed Event](https://docs.microsoft.com/en-us/dotnet/api/system.windows.eventmanager.registerroutedevent)
