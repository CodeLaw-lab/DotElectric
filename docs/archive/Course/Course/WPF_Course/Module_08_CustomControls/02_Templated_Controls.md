# Тема 8.2: Templated Controls (создание шаблонов для контролов)

### Теория

**Templated Control** — кастомный контроль с собственным ControlTemplate, определяющим визуальную структуру.

#### Ключевые концепции

| Концепция | Описание | Пример |
|-----------|----------|--------|
| **DefaultStyleKey** | Ключ стиля по умолчанию | `DefaultStyleKeyProperty.OverrideMetadata` |
| **Themes/Generic.xaml** | Файл стилей по умолчанию | `Themes/Generic.xaml` |
| **TemplatePart** | Атрибут для частей шаблона | `[TemplatePart(Name="PART_X")]` |
| **OnApplyTemplate** | Метод для получения частей | `GetTemplateChild("PART_X")` |
| **TemplateBinding** | Привязка в шаблоне | `{TemplateBinding Property}` |

#### Структура проекта Custom Control

```
MyLibrary/
├── Themes/
│   └── Generic.xaml          # Стили по умолчанию
├── Controls/
│   ├── CustomButton.cs
│   ├── CustomTextBox.cs
│   └── RangeSlider.cs
└── AssemblyInfo.cs           # ThemeInfo атрибут
```

```csharp
// AssemblyInfo.cs
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]
```

### Примеры кода

#### Пример 1: Минимальный Custom Control

```csharp
// SimpleButton.cs
using System.Windows;
using System.Windows.Controls;

namespace WpfApp
{
    public class SimpleButton : Button
    {
        // Статический конструктор — обязателен
        static SimpleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SimpleButton),
                new FrameworkPropertyMetadata(typeof(SimpleButton)));
        }
    }
}
```

```xml
<!-- Themes/Generic.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:WpfApp">
    
    <Style TargetType="{x:Type local:SimpleButton}">
        <Setter Property="Background" Value="Blue"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="15,5"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:SimpleButton}">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="5">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"
                                          Margin="{TemplateBinding Padding}"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

#### Пример 2: Custom Control с Custom Properties

```csharp
// IconButton.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp
{
    public class IconButton : Button
    {
        // Icon Property
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(Geometry),
                typeof(IconButton), new PropertyMetadata(null));

        // IconSize Property
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double),
                typeof(IconButton), new PropertyMetadata(16.0));

        // ShowIcon Property
        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool),
                typeof(IconButton), new PropertyMetadata(true));

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(IconButton),
                new FrameworkPropertyMetadata(typeof(IconButton)));
        }
    }
}
```

```xml
<!-- Themes/Generic.xaml -->
<Style TargetType="{x:Type local:IconButton}">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="15,8"/>
    
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:IconButton}">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="5">
                    <StackPanel Orientation="Horizontal"
                                HorizontalAlignment="Center"
                                VerticalAlignment="Center">
                        
                        <!-- Icon -->
                        <Path x:Name="iconPath"
                              Data="{TemplateBinding Icon}"
                              Fill="{TemplateBinding Foreground}"
                              Width="{TemplateBinding IconSize}"
                              Height="{TemplateBinding IconSize}"
                              Margin="0,0,8,0"
                              Stretch="Uniform"
                              Visibility="{TemplateBinding ShowIcon, Converter={StaticResource BoolToVisibility}}"/>
                        
                        <!-- Content -->
                        <ContentPresenter VerticalAlignment="Center"/>
                    </StackPanel>
                </Border>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" Value="#005A9E"/>
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="border" Property="Background" Value="#004080"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### Пример 3: Custom Control с TemplateParts

```csharp
// RangeSlider.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfApp
{
    [TemplatePart(Name = "PART_Track", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_MinThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_MaxThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_SelectionRange", Type = typeof(Border))]
    public class RangeSlider : Control
    {
        private Grid _track;
        private Thumb _minThumb;
        private Thumb _maxThumb;
        private Border _selectionRange;

        // Minimum Property
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double),
                typeof(RangeSlider), new PropertyMetadata(0.0));

        // Maximum Property
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double),
                typeof(RangeSlider), new PropertyMetadata(100.0));

        // SelectedMin Property
        public static readonly DependencyProperty SelectedMinProperty =
            DependencyProperty.Register(nameof(SelectedMin), typeof(double),
                typeof(RangeSlider), new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedMinChanged));

        // SelectedMax Property
        public static readonly DependencyProperty SelectedMaxProperty =
            DependencyProperty.Register(nameof(SelectedMax), typeof(double),
                typeof(RangeSlider), new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedMaxChanged));

        static RangeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RangeSlider),
                new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public double SelectedMin { get => (double)GetValue(SelectedMinProperty); set => SetValue(SelectedMinProperty, value); }
        public double SelectedMax { get => (double)GetValue(SelectedMaxProperty); set => SetValue(SelectedMaxProperty, value); }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Отписка от старых событий
            if (_minThumb != null) _minThumb.DragDelta -= MinThumb_DragDelta;
            if (_maxThumb != null) _maxThumb.DragDelta -= MaxThumb_DragDelta;

            // Получение частей шаблона
            _track = GetTemplateChild("PART_Track") as Grid;
            _minThumb = GetTemplateChild("PART_MinThumb") as Thumb;
            _maxThumb = GetTemplateChild("PART_MaxThumb") as Thumb;
            _selectionRange = GetTemplateChild("PART_SelectionRange") as Border;

            // Подписка на новые события
            if (_minThumb != null) _minThumb.DragDelta += MinThumb_DragDelta;
            if (_maxThumb != null) _maxThumb.DragDelta += MaxThumb_DragDelta;

            UpdateSelectionRange();
        }

        private static void OnSelectedMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RangeSlider)d;
            control.UpdateSelectionRange();
        }

        private static void OnSelectedMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RangeSlider)d;
            control.UpdateSelectionRange();
        }

        private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_track == null) return;

            double position = _minThumb.TranslatePoint(new Point(0, 0), _track).X;
            double range = _track.ActualWidth;
            double value = Minimum + (position / range) * (Maximum - Minimum);

            // Ограничение между Minimum и SelectedMax
            SelectedMin = Math.Max(Minimum, Math.Min(value, SelectedMax));
        }

        private void MaxThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_track == null) return;

            double position = _maxThumb.TranslatePoint(new Point(0, 0), _track).X;
            double range = _track.ActualWidth;
            double value = Minimum + (position / range) * (Maximum - Minimum);

            // Ограничение между SelectedMin и Maximum
            SelectedMax = Math.Max(value, Math.Min(SelectedMin, Maximum));
        }

        private void UpdateSelectionRange()
        {
            if (_selectionRange == null || _track == null) return;

            double range = Maximum - Minimum;
            double leftPercent = (SelectedMin - Minimum) / range;
            double rightPercent = (SelectedMax - Minimum) / range;

            Grid.SetLeft(_selectionRange, leftPercent * _track.ActualWidth);
            _selectionRange.Width = (rightPercent - leftPercent) * _track.ActualWidth;
        }
    }
}
```

```xml
<!-- Themes/Generic.xaml для RangeSlider -->
<Style TargetType="{x:Type local:RangeSlider}">
    <Setter Property="Minimum" Value="0"/>
    <Setter Property="Maximum" Value="100"/>
    <Setter Property="Height" Value="40"/>
    
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:RangeSlider}">
                <Grid x:Name="PART_Track" Height="{TemplateBinding Height}">
                    <!-- Фон -->
                    <Border Background="#E0E0E0" CornerRadius="3"/>
                    
                    <!-- Выделенный диапазон -->
                    <Border x:Name="PART_SelectionRange"
                            Background="#0078D4"
                            CornerRadius="3"
                            Height="10"
                            VerticalAlignment="Center"/>
                    
                    <!-- Минимальный ползунок -->
                    <Thumb x:Name="PART_MinThumb"
                           Width="20" Height="30"
                           HorizontalAlignment="Left"
                           VerticalAlignment="Center">
                        <Thumb.Template>
                            <ControlTemplate TargetType="Thumb">
                                <Ellipse Fill="White" Stroke="#0078D4" StrokeThickness="2"/>
                            </ControlTemplate>
                        </Thumb.Template>
                    </Thumb>
                    
                    <!-- Максимальный ползунок -->
                    <Thumb x:Name="PART_MaxThumb"
                           Width="20" Height="30"
                           HorizontalAlignment="Left"
                           VerticalAlignment="Center">
                        <Thumb.Template>
                            <ControlTemplate TargetType="Thumb">
                                <Ellipse Fill="White" Stroke="#0078D4" StrokeThickness="2"/>
                            </ControlTemplate>
                        </Thumb.Template>
                    </Thumb>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### Пример 4: Custom Control с анимацией

```csharp
// LoadingSpinner.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfApp
{
    [TemplatePart(Name = "PART_RotatingElement", Type = typeof(UIElement))]
    public class LoadingSpinner : Control
    {
        public static readonly DependencyProperty IsSpinningProperty =
            DependencyProperty.Register(nameof(IsSpinning), typeof(bool),
                typeof(LoadingSpinner), new PropertyMetadata(true, OnIsSpinningChanged));

        public static readonly DependencyProperty SpinDurationProperty =
            DependencyProperty.Register(nameof(SpinDuration), typeof(double),
                typeof(LoadingSpinner), new PropertyMetadata(1.0));

        private UIElement _rotatingElement;
        private Storyboard _spinStoryboard;

        static LoadingSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LoadingSpinner),
                new FrameworkPropertyMetadata(typeof(LoadingSpinner)));
        }

        public bool IsSpinning
        {
            get => (bool)GetValue(IsSpinningProperty);
            set => SetValue(IsSpinningProperty, value);
        }

        public double SpinDuration
        {
            get => (double)GetValue(SpinDurationProperty);
            set => SetValue(SpinDurationProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rotatingElement = GetTemplateChild("PART_RotatingElement") as UIElement;
            
            CreateSpinStoryboard();
            
            if (IsSpinning)
            {
                _spinStoryboard?.Begin();
            }
        }

        private void CreateSpinStoryboard()
        {
            if (_rotatingElement == null) return;

            var transform = _rotatingElement.RenderTransform as RotateTransform;
            if (transform == null)
            {
                transform = new RotateTransform();
                _rotatingElement.RenderTransform = transform;
                _rotatingElement.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(SpinDuration),
                RepeatBehavior = RepeatBehavior.Forever,
                IsLinear = true
            };

            _spinStoryboard = new Storyboard();
            _spinStoryboard.Children.Add(animation);
            Storyboard.SetTarget(animation, transform);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RotateTransform.AngleProperty));
        }

        private static void OnIsSpinningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LoadingSpinner)d;
            
            if ((bool)e.NewValue)
            {
                control._spinStoryboard?.Begin();
            }
            else
            {
                control._spinStoryboard?.Stop();
            }
        }
    }
}
```

```xml
<!-- Themes/Generic.xaml для LoadingSpinner -->
<Style TargetType="{x:Type local:LoadingSpinner}">
    <Setter Property="Width" Value="40"/>
    <Setter Property="Height" Value="40"/>
    
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:LoadingSpinner}">
                <Grid>
                    <!-- 8 линий для спиннера -->
                    <Canvas x:Name="PART_RotatingElement">
                        <Line X1="20" Y1="5" X2="20" Y2="10" Stroke="{TemplateBinding Foreground}" 
                              StrokeThickness="3" Opacity="1"/>
                        <Line X1="20" Y1="5" X2="20" Y2="10" Stroke="{TemplateBinding Foreground}" 
                              StrokeThickness="3" Opacity="0.875">
                            <Line.RenderTransform>
                                <RotateTransform CenterX="20" CenterY="20" Angle="45"/>
                            </Line.RenderTransform>
                        </Line>
                        <!-- Остальные 6 линий с углами 90, 135, 180, 225, 270, 315 -->
                    </Canvas>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### Пример 5: Реальное использование из DotElectric

```csharp
// GridSizeSelector.cs — контроль для выбора размера сетки
public class GridSizeSelector : ComboBox
{
    public static readonly DependencyProperty GridStepMmProperty =
        DependencyProperty.Register(nameof(GridStepMm), typeof(double),
            typeof(GridSizeSelector),
            new FrameworkPropertyMetadata(5.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnGridStepMmChanged));

    static GridSizeSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridSizeSelector),
            new FrameworkPropertyMetadata(typeof(GridSizeSelector)));
    }

    public double GridStepMm
    {
        get => (double)GetValue(GridStepMmProperty);
        set => SetValue(GridStepMmProperty, value);
    }

    public GridSizeSelector()
    {
        ItemsSource = new[] { 1.0, 2.0, 5.0, 10.0, 20.0, 50.0 };
        SelectedItem = GridStepMm;
    }

    private static void OnGridStepMmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (GridSizeSelector)d;
        control.SelectedItem = e.NewValue;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        GridStepMm = (double)(SelectedItem ?? 5.0);
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 8.2.1: Simple Custom Control**

Создайте Custom Control:
- Наследуется от Button
- Свойство CornerRadius
- Минимальный стиль в Generic.xaml

**Задача 8.2.2: Custom CheckBox**

Создайте Custom Control:
- Наследуется от CheckBox
- Custom Template
- Свойство CheckColor

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 8.2.3: NumericUpDown**

Создайте Custom Control:
- TextBox с кнопками + и –
- Свойства Minimum, Maximum, Value
- Increment/Decrement команды

**Задача 8.2.4: Custom Expander**

Создайте Custom Control:
- Наследуется от Expander
- Custom Template
- Анимация раскрытия

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 8.2.5: MultiSelect ComboBox**

Создайте Custom Control:
- ComboBox с CheckBox items
- SelectedItems коллекция
- Custom Template

**Задача 8.2.6: Custom DataGrid Column**

Создайте Custom Control:
- Наследуется от DataGridColumn
- Custom CellTemplate
- Сортировка, фильтрация

---

### Решения

<details>
<summary>✅ Решение задачи 8.2.1</summary>

```csharp
// RoundedButton.cs
public class RoundedButton : Button
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
            typeof(RoundedButton), new PropertyMetadata(new CornerRadius(5)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static RoundedButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RoundedButton),
            new FrameworkPropertyMetadata(typeof(RoundedButton)));
    }
}
```

```xml
<Style TargetType="{x:Type local:RoundedButton}">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="15,8"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:RoundedButton}">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="{TemplateBinding CornerRadius}">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"
                                      Margin="{TemplateBinding Padding}"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```
</details>

<details>
<summary>✅ Решение задачи 8.2.3</summary>

```csharp
// NumericUpDown.cs
[TemplatePart(Name = "PART_IncrementButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_DecrementButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
public class NumericUpDown : Control
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double),
            typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double),
            typeof(NumericUpDown), new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double),
            typeof(NumericUpDown), new PropertyMetadata(100.0));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double),
            typeof(NumericUpDown), new PropertyMetadata(1.0));

    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));
    }

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Step { get => (double)GetValue(StepProperty); set => SetValue(StepProperty, value); }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var incrementBtn = GetTemplateChild("PART_IncrementButton") as Button;
        var decrementBtn = GetTemplateChild("PART_DecrementButton") as Button;

        if (incrementBtn != null) incrementBtn.Click += (s, e) => Value = Math.Min(Value + Step, Maximum);
        if (decrementBtn != null) decrementBtn.Click += (s, e) => Value = Math.Max(Value - Step, Minimum);
    }
}
```
</details>

---

## Ключевые выводы

✅ **DefaultStyleKey** — обязателен для Custom Controls  
✅ **Themes/Generic.xaml** — файл стилей по умолчанию  
✅ **TemplatePart** — документирование частей шаблона  
✅ **OnApplyTemplate** — получение и настройка частей  
✅ **TemplateBinding** — привязка свойств в шаблоне  
✅ **AssemblyInfo ThemeInfo** — указание расположения тем  
✅ **Statically typed properties** — Dependency Properties для кастомных свойств

---

## Дополнительные ресурсы

- [Custom Controls](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/authoring-overview-custom-control)
- [Control Templating](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/control-templating)
- [ThemeInfo Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.windows.themedictionaryattribute)
