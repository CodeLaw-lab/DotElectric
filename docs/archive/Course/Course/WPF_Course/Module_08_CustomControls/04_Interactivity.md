# Тема 8.4: Interactivity (Triggers, Actions, Behaviors)

### Теория

**Interactivity** — библиотека для декларативного добавления поведения в XAML через Triggers, Actions и Behaviors.

#### Microsoft.Xaml.Behaviors

Пакет: `Install-Package Microsoft.Xaml.Behaviors.Wpf`

```xml
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
```

#### Компоненты

| Компонент | Описание | Пример |
|-----------|----------|--------|
| **Interaction.Triggers** | Коллекция триггеров | `<i:Interaction.Triggers>` |
| **EventTrigger** | Реакция на событие | `<i:EventTrigger EventName="Click">` |
| **InvokeCommandAction** | Вызов команды | `<i:InvokeCommandAction Command="{Binding...}"/>` |
| **ChangePropertyAction** | Изменение свойства | `<i:ChangePropertyAction .../>` |
| **Behavior<T>** | Базовый класс для поведения | `class MyBehavior : Behavior<TextBox>` |

### Примеры кода

#### Пример 1: EventTrigger с InvokeCommandAction

```xml
<Window xmlns:i="http://schemas.microsoft.com/xaml/behaviors">
    <Button Content="Click me">
        <i:Interaction.Triggers>
            <i:EventTrigger EventName="Click">
                <i:InvokeCommandAction Command="{Binding ClickCommand}"/>
            </i:EventTrigger>
        </i:Interaction.Triggers>
    </Button>
</Window>
```

```csharp
// ViewModel
public class MainViewModel
{
    public ICommand ClickCommand { get; }

    public MainViewModel()
    {
        ClickCommand = new RelayCommand(OnClick);
    }

    private void OnClick()
    {
        MessageBox.Show("Clicked!");
    }
}
```

#### Пример 2: EventTrigger с CommandParameter

```xml
<ListBox ItemsSource="{Binding Items}">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="SelectionChanged">
            <i:InvokeCommandAction 
                Command="{Binding SelectionChangedCommand}"
                CommandParameter="{Binding SelectedItem, RelativeSource={RelativeSource AncestorType=ListBox}}"/>
        </i:EventTrigger>
    </i:Interaction.Triggers>
</ListBox>
```

```csharp
public class MainViewModel
{
    public ICommand SelectionChangedCommand { get; }

    public MainViewModel()
    {
        SelectionChangedCommand = new RelayCommand<object>(OnSelectionChanged);
    }

    private void OnSelectionChanged(object selectedItem)
    {
        // Обработка выбранного элемента
    }
}
```

#### Пример 3: ChangePropertyAction

```xml
<Button Content="Hover me">
    <i:Interaction.Triggers>
        <!-- Изменение цвета при наведении -->
        <i:EventTrigger EventName="MouseEnter">
            <i:ChangePropertyAction TargetObject="{Binding RelativeSource={RelativeSource Self}}"
                                    PropertyName="Background"
                                    Value="LightBlue"/>
        </i:EventTrigger>
        
        <!-- Возврат цвета при уходе -->
        <i:EventTrigger EventName="MouseLeave">
            <i:ChangePropertyAction TargetObject="{Binding RelativeSource={RelativeSource Self}}"
                                    PropertyName="Background"
                                    Value="White"/>
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>
```

#### Пример 4: DataTrigger в XAML

```xml
<StackPanel>
    <CheckBox x:Name="check"/>
    
    <TextBlock Text="Checked!">
        <TextBlock.Style>
            <Style TargetType="TextBlock">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsChecked, ElementName=check}" Value="True">
                        <Setter Property="Visibility" Value="Visible"/>
                    </DataTrigger>
                    <DataTrigger Binding="{Binding IsChecked, ElementName=check}" Value="False">
                        <Setter Property="Visibility" Value="Collapsed"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </TextBlock.Style>
    </TextBlock>
</StackPanel>
```

#### Пример 5: Custom Behavior

```csharp
// TextBoxSelectAllBehavior.cs
using Microsoft.Xaml.Behaviors;

public class TextBoxSelectAllBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.GotFocus += AssociatedObject_GotFocus;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
    }

    private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
    {
        AssociatedObject.SelectAll();
    }
}
```

```xml
<!-- Использование -->
<TextBox>
    <i:Interaction.Behaviors>
        <local:TextBoxSelectAllBehavior/>
    </i:Interaction.Behaviors>
</TextBox>
```

#### Пример 6: Custom Behavior с параметрами

```csharp
// NumericTextBoxBehavior.cs
using Microsoft.Xaml.Behaviors;

public class NumericTextBoxBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty AllowDecimalProperty =
        DependencyProperty.Register(nameof(AllowDecimal), typeof(bool),
            typeof(NumericTextBoxBehavior), new PropertyMetadata(false));

    public bool AllowDecimal
    {
        get => (bool)GetValue(AllowDecimalProperty);
        set => SetValue(AllowDecimalProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
    }

    private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только цифры
        if (!int.TryParse(e.Text, out _))
        {
            // Разрешаем точку если AllowDecimal=true
            if (AllowDecimal && e.Text == ".")
            {
                if (AssociatedObject.Text.Contains("."))
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
```

```xml
<!-- Использование -->
<TextBox>
    <i:Interaction.Behaviors>
        <local:NumericTextBoxBehavior AllowDecimal="True"/>
    </i:Interaction.Behaviors>
</TextBox>
```

#### Пример 7: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml -- Interactivity для команд -->
<UserControl xmlns:i="http://schemas.microsoft.com/xaml/behaviors">
    <Grid>
        <!-- Mouse events для рисования -->
        <Canvas x:Name="DrawingCanvas">
            <i:Interaction.Triggers>
                <!-- MouseDown -->
                <i:EventTrigger EventName="MouseLeftButtonDown">
                    <i:InvokeCommandAction 
                        Command="{Binding MouseDownCommand}"
                        PassEventArgsToCommand="True"/>
                </i:EventTrigger>
                
                <!-- MouseMove -->
                <i:EventTrigger EventName="MouseMove">
                    <i:InvokeCommandAction 
                        Command="{Binding MouseMoveCommand}"
                        PassEventArgsToCommand="True"/>
                </i:EventTrigger>
                
                <!-- MouseUp -->
                <i:EventTrigger EventName="MouseLeftButtonUp">
                    <i:InvokeCommandAction 
                        Command="{Binding MouseUpCommand}"
                        PassEventArgsToCommand="True"/>
                </i:EventTrigger>
            </i:Interaction.Triggers>
        </Canvas>
        
        <!-- Keyboard events -->
        <i:Interaction.Triggers>
            <i:EventTrigger EventName="KeyDown">
                <i:InvokeCommandAction 
                    Command="{Binding KeyDownCommand}"
                    PassEventArgsToCommand="True"/>
            </i:EventTrigger>
        </i:Interaction.Triggers>
    </Grid>
</UserControl>
```

```csharp
// EditorViewModel.cs
public partial class EditorViewModel : ObservableObject
{
    [RelayCommand]
    private void MouseDown(MouseButtonEventArgs e)
    {
        var position = e.GetPosition(DrawingCanvas);
        // Начало рисования
    }

    [RelayCommand]
    private void MouseMove(MouseEventArgs e)
    {
        var position = e.GetPosition(DrawingCanvas);
        // Обновление preview
    }

    [RelayCommand]
    private void MouseUp(MouseButtonEventArgs e)
    {
        var position = e.GetPosition(DrawingCanvas);
        // Завершение рисования
    }

    [RelayCommand]
    private void KeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete:
                DeleteSelectedObjects();
                break;
            case Key.Escape:
                CancelTool();
                break;
        }
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 8.4.1: EventToCommand**

Создайте EventTrigger:
- Button Click
- InvokeCommandAction
- Command в ViewModel

**Задача 8.4.2: ChangePropertyAction**

Создайте триггер:
- MouseEnter → Background
- MouseLeave → Background back

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 8.4.3: ListBox Selection**

Создайте EventTrigger:
- SelectionChanged event
- Command с SelectedItem parameter
- Обработка в ViewModel

**Задача 8.4.4: Custom Behavior**

Создайте Behavior<TextBox>:
- Auto UpperCase
- При вводе текст становится заглавным
- OnAttached/OnDetaching

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 8.4.5: DragDrop Behavior**

Создайте Behavior:
- DragDrop для ListBox
- IsDragSource, IsDropTarget
- Move items между списками

**Задача 8.4.6: MultiTrigger Behavior**

Создайте Behavior:
- Несколько событий
- Условия (CanExecute)
- Composite Command

---

### Решения

<details>
<summary>✅ Решение задачи 8.4.1</summary>

```xml
<Button Content="Click me">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
            <i:InvokeCommandAction Command="{Binding ClickCommand}"/>
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>
```

```csharp
public class MainViewModel
{
    public ICommand ClickCommand { get; }

    public MainViewModel()
    {
        ClickCommand = new RelayCommand(() => 
        {
            MessageBox.Show("Clicked!");
        });
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 8.4.4</summary>

```csharp
public class AutoUpperCaseBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanged += AssociatedObject_TextChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
    }

    private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        var caretIndex = textBox.CaretIndex;
        
        textBox.Text = textBox.Text.ToUpper();
        textBox.CaretIndex = caretIndex;
    }
}
```

```xml
<TextBox>
    <i:Interaction.Behaviors>
        <local:AutoUpperCaseBehavior/>
    </i:Interaction.Behaviors>
</TextBox>
```
</details>

---

## Ключевые выводы

✅ **Interaction.Triggers** — коллекция триггеров  
✅ **EventTrigger** — реакция на событие  
✅ **InvokeCommandAction** — вызов команды  
✅ **ChangePropertyAction** — изменение свойства  
✅ **Behavior<T>** — базовый класс для кастомных behaviors  
✅ **OnAttached/OnDetaching** — жизненный цикл behavior  
✅ **PassEventArgsToCommand** — передача EventArgs  
✅ **CommandParameter** — параметр команды

---

## Дополнительные ресурсы

- [Microsoft.Xaml.Behaviors](https://github.com/microsoft/XamlBehaviorsWpf)
- [Behaviors Overview](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/september/mvvm-lightning-quick-introduction-to-behaviors-in-mvvm)
- [EventToCommand](https://docs.microsoft.com/en-us/dotnet/api/microsoft.xaml.behaviors.core.eventtocommandaction)
