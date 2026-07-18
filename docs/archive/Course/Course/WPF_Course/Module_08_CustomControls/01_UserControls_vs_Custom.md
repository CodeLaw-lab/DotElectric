# Тема 8.1: UserControls vs Custom Controls

### Теория

В WPF есть два основных способа создания собственных контролов:

#### UserControl

**UserControl** — композиция существующих контролов в новый reusable компонент.

**Преимущества:**
- ✅ Простота создания
- ✅ Designer support в Visual Studio
- ✅ Легко добавлять Dependency Properties
- ✅ Идеально для конкретных UI сценариев

**Недостатки:**
- ❌ Ограниченная темизация (нельзя изменить Template)
- ❌ Наследуется от UserControl, не от Control
- ❌ Меньше гибкости

**Когда использовать:**
- Конкретный UI компонент (форма, панель)
- Не нужна кастомизация Template
- Быстрая разработка

#### Custom Control

**Custom Control** — новый контроль с собственным стилем по умолчанию.

**Преимущества:**
- ✅ Полный контроль над внешним видом (Template)
- ✅ Поддержка тем (Styles)
- ✅ Наследуется от Control или другого контрола
- ✅ Переиспользование в разных проектах

**Недостатки:**
- ❌ Сложнее в создании
- ❌ Требует Themes/Generic.xaml
- ❌ Нет дизайнера для Template

**Когда использовать:**
- Библиотека контролов
- Нужна темизация
- Переиспользование в разных приложениях

### Примеры кода

#### Пример 1: Создание UserControl

```xml
<!-- PersonControl.xaml -->
<UserControl x:Class="WpfApp.PersonControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Border BorderBrush="Gray" BorderThickness="1" Padding="10" CornerRadius="5">
        <StackPanel>
            <TextBlock Text="{Binding FirstName, RelativeSource={RelativeSource AncestorType=UserControl}}"
                       FontWeight="Bold" FontSize="16"/>
            <TextBlock Text="{Binding LastName, RelativeSource={RelativeSource AncestorType=UserControl}}"
                       Foreground="Gray"/>
            <TextBlock Text="{Binding Email, RelativeSource={RelativeSource AncestorType=UserControl}}"
                       Foreground="Blue"/>
        </StackPanel>
    </Border>
</UserControl>
```

```csharp
// PersonControl.xaml.cs
public partial class PersonControl : UserControl
{
    public static readonly DependencyProperty FirstNameProperty =
        DependencyProperty.Register(nameof(FirstName), typeof(string), 
            typeof(PersonControl), new PropertyMetadata(""));

    public static readonly DependencyProperty LastNameProperty =
        DependencyProperty.Register(nameof(LastName), typeof(string), 
            typeof(PersonControl), new PropertyMetadata(""));

    public static readonly DependencyProperty EmailProperty =
        DependencyProperty.Register(nameof(Email), typeof(string), 
            typeof(PersonControl), new PropertyMetadata(""));

    public string FirstName
    {
        get => (string)GetValue(FirstNameProperty);
        set => SetValue(FirstNameProperty, value);
    }

    public string LastName
    {
        get => (string)GetValue(LastNameProperty);
        set => SetValue(LastNameProperty, value);
    }

    public string Email
    {
        get => (string)GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    public PersonControl()
    {
        InitializeComponent();
    }
}
```

#### Пример 2: Создание Custom Control

```csharp
// CustomButton.cs
using System.Windows;
using System.Windows.Controls;

namespace WpfApp
{
    [TemplatePart(Name = "PART_ButtonBorder", Type = typeof(Border))]
    public class CustomButton : Button
    {
        // Custom Dependency Property
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
                typeof(CustomButton), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        // Static constructor — обязателен для Custom Controls
        static CustomButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CustomButton),
                new FrameworkPropertyMetadata(typeof(CustomButton)));
        }

        // Переопределение OnApplyTemplate
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            // Получение частей шаблона
            var border = GetTemplateChild("PART_ButtonBorder") as Border;
            if (border != null)
            {
                border.MouseEnter += (s, e) => OnHoverEnter();
                border.MouseLeave += (s, e) => OnHoverLeave();
            }
        }

        protected virtual void OnHoverEnter()
        {
            // Логика при наведении
        }

        protected virtual void OnHoverLeave()
        {
            // Логика при уходе мыши
        }
    }
}
```

#### Пример 3: Themes/Generic.xaml для Custom Control

```xml
<!-- Themes/Generic.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:WpfApp">

    <!-- Default Style для CustomButton -->
    <Style TargetType="{x:Type local:CustomButton}">
        <Setter Property="Background" Value="#0078D4"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="20,10"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:CustomButton}">
                    <Border x:Name="PART_ButtonBorder"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="{TemplateBinding CornerRadius}">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"
                                          Margin="{TemplateBinding Padding}"/>
                    </Border>
                    
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="PART_ButtonBorder" 
                                    Property="Background" Value="#005A9E"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="PART_ButtonBorder" 
                                    Property="Background" Value="#004080"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="PART_ButtonBorder" 
                                    Property="Opacity" Value="0.5"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

#### Пример 4: Сравнение подходов

```csharp
// UserControl — для конкретного UI
public partial class AddressEditor : UserControl
{
    public AddressEditor()
    {
        InitializeComponent();
        // Жёстко заданный UI
    }
}

// Custom Control — для переиспользования
public class NumericTextBox : TextBox
{
    static NumericTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(typeof(NumericTextBox)));
    }

    protected override void OnTextInput(TextCompositionEventArgs e)
    {
        // Разрешаем только цифры
        if (!int.TryParse(e.Text, out _))
        {
            e.Handled = true;
        }
        base.OnTextInput(e);
    }
}
```

#### Пример 5: UserControl с валидацией

```csharp
// EmailEditor.xaml.cs
public partial class EmailEditor : UserControl
{
    public static readonly DependencyProperty EmailProperty =
        DependencyProperty.Register(nameof(Email), typeof(string),
            typeof(EmailEditor), 
            new FrameworkPropertyMetadata("", 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnEmailChanged));

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(EmailEditor), new PropertyMetadata(true));

    private static readonly Regex EmailRegex = 
        new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public string Email
    {
        get => (string)GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    private static void OnEmailChanged(DependencyObject d, 
                                        DependencyPropertyChangedEventArgs e)
    {
        var control = (EmailEditor)d;
        control.IsValid = EmailRegex.IsMatch(e.NewValue?.ToString() ?? "");
    }

    public EmailEditor()
    {
        InitializeComponent();
    }
}
```

```xml
<!-- EmailEditor.xaml -->
<UserControl x:Class="WpfApp.EmailEditor">
    <Grid>
        <TextBox Text="{Binding Email, RelativeSource={RelativeSource AncestorType=UserControl}, 
                          UpdateSourceTrigger=PropertyChanged}"
                 Watermark="Enter email">
            <TextBox.Style>
                <Style TargetType="TextBox">
                    <Style.Triggers>
                        <DataTrigger Binding="{Binding IsValid, RelativeSource={RelativeSource AncestorType=UserControl}}" 
                                     Value="False">
                            <Setter Property="BorderBrush" Value="Red"/>
                            <Setter Property="ToolTip" Value="Invalid email format"/>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </TextBox.Style>
        </TextBox>
    </Grid>
</UserControl>
```

#### Пример 6: Custom Control с частями шаблона

```csharp
// RangeSlider.cs
[TemplatePart(Name = "PART_Track", Type = typeof(Grid))]
[TemplatePart(Name = "PART_MinThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_MaxThumb", Type = typeof(Thumb))]
public class RangeSlider : Control
{
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double),
            typeof(RangeSlider), new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double),
            typeof(RangeSlider), new PropertyMetadata(100.0));

    public static readonly DependencyProperty SelectedMinProperty =
        DependencyProperty.Register(nameof(SelectedMin), typeof(double),
            typeof(RangeSlider), new FrameworkPropertyMetadata(0.0, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SelectedMaxProperty =
        DependencyProperty.Register(nameof(SelectedMax), typeof(double),
            typeof(RangeSlider), new FrameworkPropertyMetadata(100.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(typeof(RangeSlider)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var minThumb = GetTemplateChild("PART_MinThumb") as Thumb;
        var maxThumb = GetTemplateChild("PART_MaxThumb") as Thumb;

        if (minThumb != null)
            minThumb.DragDelta += MinThumb_DragDelta;

        if (maxThumb != null)
            maxThumb.DragDelta += MaxThumb_DragDelta;
    }

    private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        // Логика перемещения минимального ползунка
    }

    private void MaxThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        // Логика перемещения максимального ползунка
    }
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// ZoomComboBox.cs — кастомный контроль для выбора масштаба
public class ZoomComboBox : ComboBox
{
    public static readonly DependencyProperty ZoomValueProperty =
        DependencyProperty.Register(nameof(ZoomValue), typeof(double),
            typeof(ZoomComboBox), 
            new FrameworkPropertyMetadata(1.0, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnZoomValueChanged));

    static ZoomComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ZoomComboBox),
            new FrameworkPropertyMetadata(typeof(ZoomComboBox)));
    }

    public double ZoomValue
    {
        get => (double)GetValue(ZoomValueProperty);
        set => SetValue(ZoomValueProperty, value);
    }

    private static void OnZoomValueChanged(DependencyObject d, 
                                          DependencyPropertyChangedEventArgs e)
    {
        var control = (ZoomComboBox)d;
        control.SelectedItem = control.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(item => 
                double.TryParse(((TextBlock)item.Content).Text, out var val) && 
                Math.Abs(val / 100.0 - (double)e.NewValue) < 0.01);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        
        if (SelectedItem is ComboBoxItem item && 
            double.TryParse(((TextBlock)item.Content).Text, out var val))
        {
            ZoomValue = val / 100.0;
        }
    }
}
```

```xml
<!-- Themes/Generic.xaml для ZoomComboBox -->
<ResourceDictionary>
    <Style TargetType="{x:Type local:ZoomComboBox}">
        <Setter Property="Width" Value="90"/>
        <Setter Property="Height" Value="30"/>
        <Setter Property="IsEditable" Value="True"/>
        <Setter Property="ItemsSource">
            <Setter.Value>
                <x:Array Type="ComboBoxItem">
                    <ComboBoxItem><TextBlock Text="10%"/></ComboBoxItem>
                    <ComboBoxItem><TextBlock Text="25%"/></ComboBoxItem>
                    <ComboBoxItem><TextBlock Text="50%"/></ComboBoxItem>
                    <ComboBoxItem><TextBlock Text="100%"/></ComboBoxItem>
                    <ComboBoxItem><TextBlock Text="200%"/></ComboBoxItem>
                </x:Array>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 8.1.1: Simple UserControl**

Создайте UserControl:
- Заголовок (Dependency Property)
- Содержимое (ContentControl)
- Border с закруглением

**Задача 8.1.2: Custom TextBox**

Создайте Custom Control:
- Наследуется от TextBox
- Только цифры (override OnTextInput)
- Свойство MaxLength

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 8.1.3: Card UserControl**

Создайте карточку товара:
- Image, Title, Price, Description
- Dependency Properties для всех
- Hover эффект (тень)

**Задача 8.1.4: Custom ToggleButton**

Создайте Custom Control:
- Наследуется от ToggleButton
- Custom Template
- Анимация переключения

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 8.1.5: Templated ListBox**

Создайте Custom Control:
- Наследуется от ListBox
- Custom ItemsPanel
- Свойство ItemSpacing

**Задача 8.1.6: Reusable UserControl Library**

Создайте библиотеку:
- 3+ UserControl
- Общий проект
- NuGet пакет (опционально)

---

### Решения

<details>
<summary>✅ Решение задачи 8.1.1</summary>

```xml
<!-- SimpleCard.xaml -->
<UserControl x:Class="WpfApp.SimpleCard">
    <Border BorderBrush="Gray" BorderThickness="1" 
            CornerRadius="{Binding CornerRadius, RelativeSource={RelativeSource AncestorType=UserControl}}"
            Padding="10">
        <StackPanel>
            <TextBlock Text="{Binding Title, RelativeSource={RelativeSource AncestorType=UserControl}}"
                       FontWeight="Bold" FontSize="16"/>
            <ContentControl Content="{Binding Content, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
        </StackPanel>
    </Border>
</UserControl>
```

```csharp
public partial class SimpleCard : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string),
            typeof(SimpleCard), new PropertyMetadata(""));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
            typeof(SimpleCard), new PropertyMetadata(new CornerRadius(5)));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public SimpleCard()
    {
        InitializeComponent();
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 8.1.2</summary>

```csharp
public class NumericTextBox : TextBox
{
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(nameof(MaxLength), typeof(int),
            typeof(NumericTextBox), new PropertyMetadata(int.MaxValue));

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    static NumericTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(typeof(NumericTextBox)));
    }

    protected override void OnTextInput(TextCompositionEventArgs e)
    {
        // Разрешаем только цифры
        if (!int.TryParse(e.Text, out _))
        {
            e.Handled = true;
            return;
        }

        // Проверка MaxLength
        if (Text.Length >= MaxLength)
        {
            e.Handled = true;
            return;
        }

        base.OnTextInput(e);
    }
}
```
</details>

---

## Ключевые выводы

✅ **UserControl** — композиция, проще, для конкретного UI  
✅ **Custom Control** — гибкость, темизация, для библиотек  
✅ **DefaultStyleKey** — обязателен для Custom Controls  
✅ **Themes/Generic.xaml** — файл стиля по умолчанию  
✅ **TemplatePart** — атрибут для именованных частей  
✅ **OnApplyTemplate** — получение частей шаблона  
✅ **Dependency Properties** — для обоих подходов

---

## Дополнительные ресурсы

- [UserControl](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.usercontrol)
- [Custom Controls](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/authoring-overview-custom-control)
- [Control Authoring](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.control)
