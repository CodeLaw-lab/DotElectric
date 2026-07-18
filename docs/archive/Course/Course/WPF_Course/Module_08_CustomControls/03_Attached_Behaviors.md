# Тема 8.3: Attached Behaviors

### Теория

**Attached Behaviors** — паттерн для добавления поведения элементам через Attached Properties без наследования.

#### Преимущества

✅ **Code-behind elimination** — логика в XAML  
✅ **MVVM-friendly** — поддержка Commands  
✅ **Reusable** — переиспользование в проекте  
✅ **Testable** — тестирование без UI

#### Когда использовать

- Event to Command конвертация
- Поведение для сторонних контролов
- Универсальная логика (drag-drop, validation)
- Избегание Code-behind

### Примеры кода

#### Пример 1: Простой Attached Behavior

```csharp
// TextBoxBehavior.cs
public static class TextBoxBehavior
{
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(TextBoxBehavior),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    public static bool GetNumericOnly(TextBox element) =>
        (bool)element.GetValue(NumericOnlyProperty);

    public static void SetNumericOnly(TextBox element, bool value) =>
        element.SetValue(NumericOnlyProperty, value);

    private static void OnNumericOnlyChanged(DependencyObject d, 
                                              DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, TextBox_OnPaste);
            }
            else
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, TextBox_OnPaste);
            }
        }
    }

    private static void TextBox_PreviewTextInput(object sender, 
                                                  TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = e.DataObject.GetData(typeof(string)) as string;
            if (!int.TryParse(text, out _))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }
}
```

```xml
<!-- Использование -->
<TextBox local:TextBoxBehavior.NumericOnly="True"/>
```

#### Пример 2: PasswordBox Binding

```csharp
// PasswordBoxBehavior.cs
public static class PasswordBoxBehavior
{
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached(
            "Password",
            typeof(string),
            typeof(PasswordBoxBehavior),
            new FrameworkPropertyMetadata("", 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordChanged));

    public static string GetPassword(PasswordBox element) =>
        (string)element.GetValue(PasswordProperty);

    public static void SetPassword(PasswordBox element, string value) =>
        element.SetValue(PasswordProperty, value);

    private static void OnPasswordChanged(DependencyObject d, 
                                           DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            // Отписка от старого события
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            // Установка значения из ViewModel
            if (e.NewValue?.ToString() != passwordBox.Password)
            {
                passwordBox.Password = e.NewValue?.ToString() ?? "";
            }

            // Подписка на новое событие
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // Обновление ViewModel
            SetPassword(passwordBox, passwordBox.Password);
        }
    }
}
```

```xml
<!-- Использование -->
<PasswordBox local:PasswordBoxBehavior.Password="{Binding Password, UpdateSourceTrigger=PropertyChanged}"/>
```

#### Пример 3: ListView Scroll to Bottom

```csharp
// ListViewBehavior.cs
public static class ListViewBehavior
{
    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.RegisterAttached(
            "AutoScroll",
            typeof(bool),
            typeof(ListViewBehavior),
            new PropertyMetadata(false, OnAutoScrollChanged));

    public static bool GetAutoScroll(ListView element) =>
        (bool)element.GetValue(AutoScrollProperty);

    public static void SetAutoScroll(ListView element, bool value) =>
        element.SetValue(AutoScrollProperty, value);

    private static void OnAutoScrollChanged(DependencyObject d, 
                                             DependencyPropertyChangedEventArgs e)
    {
        if (d is ListView listView)
        {
            if ((bool)e.NewValue)
            {
                // Подписка на изменение коллекции
                if (listView.Items is INotifyCollectionChanged collection)
                {
                    collection.CollectionChanged += (s, args) =>
                    {
                        if (args.Action == NotifyCollectionChangedAction.Add)
                        {
                            listView.ScrollIntoView(listView.Items[listView.Items.Count - 1]);
                        }
                    };
                }
            }
        }
    }
}
```

```xml
<!-- Использование для чата или логов -->
<ListView ItemsSource="{Binding Messages}"
          local:ListViewBehavior.AutoScroll="True"/>
```

#### Пример 4: Drag and Drop Behavior

```csharp
// DragDropBehavior.cs
public static class DragDropBehavior
{
    public static readonly DependencyProperty IsDragSourceProperty =
        DependencyProperty.RegisterAttached(
            "IsDragSource",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(false, OnIsDragSourceChanged));

    public static readonly DependencyProperty IsDropTargetProperty =
        DependencyProperty.RegisterAttached(
            "IsDropTarget",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(false, OnIsDropTargetChanged));

    public static bool GetIsDragSource(UIElement element) =>
        (bool)element.GetValue(IsDragSourceProperty);

    public static void SetIsDragSource(UIElement element, bool value) =>
        element.SetValue(IsDragSourceProperty, value);

    public static bool GetIsDropTarget(UIElement element) =>
        (bool)element.GetValue(IsDropTargetProperty);

    public static void SetIsDropTarget(UIElement element, bool value) =>
        element.SetValue(IsDropTargetProperty, value);

    private static void OnIsDragSourceChanged(DependencyObject d, 
                                               DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && (bool)e.NewValue)
        {
            element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
            element.MouseMove += Element_MouseMove;
        }
    }

    private static void OnIsDropTargetChanged(DependencyObject d, 
                                               DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && (bool)e.NewValue)
        {
            element.AllowDrop = true;
            element.DragOver += Element_DragOver;
            element.Drop += Element_Drop;
        }
    }

    private static Point _startPoint;

    private static void Element_PreviewMouseLeftButtonDown(object sender, 
                                                            MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
    }

    private static void Element_MouseMove(object sender, MouseEventArgs e)
    {
        var point = e.GetPosition(null);
        var diff = _startPoint - point;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            if (sender is UIElement element && 
                element.DataContext is IDataTransfer dataTransfer)
            {
                DragDrop.DoDragDrop(element, dataTransfer, DragDropEffects.Move);
            }
        }
    }

    private static void Element_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private static void Element_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(IDataTransfer)))
        {
            var data = (IDataTransfer)e.Data.GetData(typeof(IDataTransfer));
            
            if (sender is FrameworkElement element &&
                element.DataContext is IDropTarget dropTarget)
            {
                dropTarget.OnDrop(data);
            }
        }
    }
}

// Интерфейсы для типобезопасности
public interface IDataTransfer { }
public interface IDropTarget
{
    void OnDrop(IDataTransfer data);
}
```

```xml
<!-- Использование -->
<ListBox ItemsSource="{Binding Items}"
         local:DragDropBehavior.IsDragSource="True"/>

<ListBox ItemsSource="{Binding DroppedItems}"
         local:DragDropBehavior.IsDropTarget="True"/>
```

#### Пример 5: Focus Behavior

```csharp
// FocusBehavior.cs
public static class FocusBehavior
{
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusBehavior),
            new FrameworkPropertyMetadata(false, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsFocusedChanged));

    public static bool GetIsFocused(UIElement element) =>
        (bool)element.GetValue(IsFocusedProperty);

    public static void SetIsFocused(UIElement element, bool value) =>
        element.SetValue(IsFocusedProperty, value);

    private static void OnIsFocusedChanged(DependencyObject d, 
                                            DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.Focus();
            }

            // Подписка на потерю фокуса для обновления ViewModel
            element.LostFocus += (s, args) => SetIsFocused(element, false);
            element.GotFocus += (s, args) => SetIsFocused(element, true);
        }
    }
}

// Behavior для автофокуса при загрузке
public static class AutoFocusBehavior
{
    public static readonly DependencyProperty AutoFocusProperty =
        DependencyProperty.RegisterAttached(
            "AutoFocus",
            typeof(bool),
            typeof(AutoFocusBehavior),
            new PropertyMetadata(false, OnAutoFocusChanged));

    public static bool GetAutoFocus(UIElement element) =>
        (bool)element.GetValue(AutoFocusProperty);

    public static void SetAutoFocus(UIElement element, bool value) =>
        element.SetValue(AutoFocusProperty, value);

    private static void OnAutoFocusChanged(DependencyObject d, 
                                            DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && (bool)e.NewValue)
        {
            element.Loaded += (s, args) =>
            {
                if (element is IInputElement inputElement)
                {
                    inputElement.Focus();
                    
                    // Выделение текста для TextBox
                    if (element is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                }
            };
        }
    }
}
```

```xml
<!-- Использование -->
<TextBox Text="{Binding SearchText}"
         local:FocusBehavior.IsFocused="{Binding IsSearchFocused}"
         local:AutoFocusBehavior.AutoFocus="True"/>
```

#### Пример 6: Реальное использование из DotElectric

```csharp
// PreviewLineBehavior.cs — поведение для preview линии в редакторе
public static class PreviewLineBehavior
{
    public static readonly DependencyProperty ShowPreviewProperty =
        DependencyProperty.RegisterAttached(
            "ShowPreview",
            typeof(bool),
            typeof(PreviewLineBehavior),
            new PropertyMetadata(false, OnShowPreviewChanged));

    public static readonly DependencyProperty StartPointProperty =
        DependencyProperty.RegisterAttached(
            "StartPoint",
            typeof(Point),
            typeof(PreviewLineBehavior),
            new PropertyMetadata(default(Point)));

    public static readonly DependencyProperty EndPointProperty =
        DependencyProperty.RegisterAttached(
            "EndPoint",
            typeof(Point),
            typeof(PreviewLineBehavior),
            new PropertyMetadata(default(Point)));

    public static bool GetShowPreview(Line element) =>
        (bool)element.GetValue(ShowPreviewProperty);

    public static void SetShowPreview(Line element, bool value) =>
        element.SetValue(ShowPreviewProperty, value);

    public static Point GetStartPoint(Line element) =>
        (Point)element.GetValue(StartPointProperty);

    public static void SetStartPoint(Line element, Point value) =>
        element.SetValue(StartPointProperty, value);

    public static Point GetEndPoint(Line element) =>
        (Point)element.GetValue(EndPointProperty);

    public static void SetEndPoint(Line element, Point value) =>
        element.SetValue(EndPointProperty, value);

    private static void OnShowPreviewChanged(DependencyObject d, 
                                              DependencyPropertyChangedEventArgs e)
    {
        if (d is Line line)
        {
            line.Visibility = (bool)e.NewValue 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }
}

// MouseBehavior — для отслеживания мыши на Canvas
public static class MouseBehavior
{
    public static readonly DependencyProperty TrackMousePositionProperty =
        DependencyProperty.RegisterAttached(
            "TrackMousePosition",
            typeof(bool),
            typeof(MouseBehavior),
            new PropertyMetadata(false, OnTrackMousePositionChanged));

    public static readonly DependencyProperty MouseXProperty =
        DependencyProperty.RegisterAttached(
            "MouseX",
            typeof(double),
            typeof(MouseBehavior),
            new FrameworkPropertyMetadata(0.0, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MouseYProperty =
        DependencyProperty.RegisterAttached(
            "MouseY",
            typeof(double),
            typeof(MouseBehavior),
            new FrameworkPropertyMetadata(0.0, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static bool GetTrackMousePosition(UIElement element) =>
        (bool)element.GetValue(TrackMousePositionProperty);

    public static void SetTrackMousePosition(UIElement element, bool value) =>
        element.SetValue(TrackMousePositionProperty, value);

    public static double GetMouseX(UIElement element) =>
        (double)element.GetValue(MouseXProperty);

    public static void SetMouseX(UIElement element, double value) =>
        element.SetValue(MouseXProperty, value);

    public static double GetMouseY(UIElement element) =>
        (double)element.GetValue(MouseYProperty);

    public static void SetMouseY(UIElement element, double value) =>
        element.SetValue(MouseYProperty, value);

    private static void OnTrackMousePositionChanged(DependencyObject d, 
                                                     DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && (bool)e.NewValue)
        {
            element.MouseMove += Element_MouseMove;
        }
    }

    private static void Element_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is UIElement element)
        {
            var position = e.GetPosition(element);
            SetMouseX(element, position.X);
            SetMouseY(element, position.Y);
        }
    }
}
```

```xml
<!-- EditorCanvas.xaml -->
<Canvas local:MouseBehavior.TrackMousePosition="True">
    <!-- Preview линия -->
    <Line local:PreviewLineBehavior.ShowPreview="{Binding ShowPreviewLine}"
          local:PreviewLineBehavior.StartPoint="{Binding PreviewStartPoint}"
          local:PreviewLineBehavior.EndPoint="{Binding PreviewEndPoint}"
          Stroke="Red"
          StrokeThickness="1.5"
          StrokeDashArray="4,2"/>
    
    <!-- StatusBar с координатами мыши -->
    <TextBlock Text="{Binding ElementName=canvas, 
                        Path=(local:MouseBehavior.MouseX), 
                        StringFormat=X: {0:F0}}"/>
    <TextBlock Text="{Binding ElementName=canvas, 
                        Path=(local:MouseBehavior.MouseY), 
                        StringFormat=Y: {0:F0}}"/>
</Canvas>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 8.3.1: Numeric TextBox**

Создайте Attached Behavior:
- NumericOnly Property
- Только цифры в TextBox
- Обработка Paste

**Задача 8.3.2: AutoFocus**

Создайте Attached Behavior:
- AutoFocus Property
- Фокус при загрузке
- SelectAll для TextBox

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 8.3.3: PasswordBox Binding**

Создайте Attached Behavior:
- Password Property
- TwoWay binding
- PasswordChanged event handler

**Задача 8.3.4: ListView AutoScroll**

Создайте Attached Behavior:
- AutoScroll Property
- Scroll to bottom при добавлении
- INotifyCollectionChanged

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 8.3.5: Drag and Drop**

Создайте Attached Behavior:
- IsDragSource, IsDropTarget
- DragDrop.DoDragDrop
- IDropTarget интерфейс

**Задача 8.3.6: EventToCommand Behavior**

Создайте Attached Behavior:
- EventName Property
- Command Property
- CommandParameter Property
- Invoke при событии

---

### Решения

<details>
<summary>✅ Решение задачи 8.3.1</summary>

```csharp
public static class TextBoxBehavior
{
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(TextBoxBehavior),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    public static bool GetNumericOnly(TextBox element) =>
        (bool)element.GetValue(NumericOnlyProperty);

    public static void SetNumericOnly(TextBox element, bool value) =>
        element.SetValue(NumericOnlyProperty, value);

    private static void OnNumericOnlyChanged(DependencyObject d, 
                                              DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox && (bool)e.NewValue)
        {
            textBox.PreviewTextInput += (s, args) =>
            {
                args.Handled = !int.TryParse(args.Text, out _);
            };
        }
    }
}
```

```xml
<TextBox local:TextBoxBehavior.NumericOnly="True"/>
```
</details>

<details>
<summary>✅ Решение задачи 8.3.3</summary>

```csharp
public static class PasswordBoxBehavior
{
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached(
            "Password",
            typeof(string),
            typeof(PasswordBoxBehavior),
            new FrameworkPropertyMetadata("", 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordChanged));

    public static string GetPassword(PasswordBox element) =>
        (string)element.GetValue(PasswordProperty);

    public static void SetPassword(PasswordBox element, string value) =>
        element.SetValue(PasswordProperty, value);

    private static void OnPasswordChanged(DependencyObject d, 
                                           DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            
            if (e.NewValue?.ToString() != passwordBox.Password)
            {
                passwordBox.Password = e.NewValue?.ToString() ?? "";
            }
            
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetPassword(passwordBox, passwordBox.Password);
        }
    }
}
```
</details>

---

## Ключевые выводы

✅ **Attached Properties** — для добавления поведения  
✅ **PropertyMetadata Callback** — реакция на изменение  
✅ **Event Subscription** — подписка/отписка в callback  
✅ **TwoWay Binding** — FrameworkPropertyMetadataOptions  
✅ **Code-behind elimination** — логика в Behavior  
✅ **MVVM-friendly** — Commands через Behaviors  
✅ **Reusable** — переиспользование в проекте

---

## Дополнительные ресурсы

- [Attached Properties](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/attached-properties-overview)
- [Behaviors](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/september/mvvm-lightning-quick-introduction-to-behaviors-in-mvvm)
- [EventToCommand](https://docs.microsoft.com/en-us/dotnet/api/microsoft.xaml.behaviors.core.eventtocommandaction)
