# Тема 1.2: Dependency Properties и Attached Properties

### Теория

**Dependency Property (DP)** — это специальная система свойств WPF, которая обеспечивает:
- **Data Binding** — привязка данных
- **Styling** — применение стилей
- **Animation** — анимация свойств
- **Inheritance** — наследование значений
- **Default values** — значения по умолчанию

#### Почему не обычные .NET свойства?

```csharp
// ❌ Обычное свойство — НЕ подходит для WPF
public string Text { get; set; }

// ✅ Dependency Property — правильный подход
public string Text
{
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
}
public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(MyControl));
```

#### Регистрация Dependency Property

```csharp
public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(
        "Text",                              // Имя свойства
        typeof(string),                      // Тип значения
        typeof(MyControl),                   // Владелец
        new PropertyMetadata(                // Метаданные
            defaultValue: "",                // Значение по умолчанию
            propertyChangedCallback: OnTextChanged, // Callback при изменении
            coerceValueCallback: CoerceText  // Callback для коррекции значения
        ),
        validateValueCallback: ValidateText  // Валидация значения
    );
```

#### Attached Properties

**Attached Property** — свойство, которое можно "прикрепить" к любому элементу:

```xml
<Grid>
    <Button Content="OK" Grid.Row="0" Grid.Column="1"/>
    <!-- Grid.Row и Grid.Column — Attached Properties -->
</Grid>
```

```csharp
// Регистрация Attached Property
public static readonly DependencyProperty RowProperty =
    DependencyProperty.RegisterAttached(
        "Row",
        typeof(int),
        typeof(Grid),
        new PropertyMetadata(0)
    );

// Getter и Setter
public static int GetRow(UIElement element) => (int)element.GetValue(RowProperty);
public static void SetRow(UIElement element, int value) => element.SetValue(RowProperty, value);
```

### Примеры кода

#### Пример 1: Простое Dependency Property

```csharp
public class PersonControl : UserControl
{
    // 1. Регистрация DP
    public static readonly DependencyProperty NameProperty =
        DependencyProperty.Register(
            "Name",
            typeof(string),
            typeof(PersonControl),
            new PropertyMetadata("Unknown")
        );

    // 2. CLR Wrapper
    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public PersonControl()
    {
        InitializeComponent();
    }
}
```

```xml
<!-- Использование -->
<local:PersonControl Name="person1" Name="Иван"/>
<!-- Или через binding -->
<local:PersonControl Name="{Binding SelectedPerson.Name}"/>
```

#### Пример 2: DP с callback и валидацией

```csharp
public class RangeSlider : Control
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value",
            typeof(int),
            typeof(RangeSlider),
            new PropertyMetadata(
                0,                              // Default
                OnValueChanged,                 // PropertyChangedCallback
                CoerceValue                     // CoerceValueCallback
            ),
            ValidateValue                     // ValidateValueCallback
        );

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        // Вызывается ПОСЛЕ изменения значения
        var control = (RangeSlider)d;
        control.UpdateVisualState();
        Console.WriteLine($"Value changed from {e.OldValue} to {e.NewValue}");
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        // Вызывается ДО установки значения — можно скорректировать
        var control = (RangeSlider)d;
        var value = (int)baseValue;
        
        // Ограничиваем диапазон [0, 100]
        return Math.Max(0, Math.Min(100, value));
    }

    private static bool ValidateValue(object value)
    {
        // Вызывается ПЕРЕД coercing — можно отклонить
        return value is int v && v >= 0;
    }

    private void UpdateVisualState()
    {
        // Обновление UI
    }
}
```

#### Пример 3: Attached Property для кастомной панели

```csharp
public class FlexPanel : Panel
{
    // Attached Property для гибкости элемента
    public static readonly DependencyProperty FlexProperty =
        DependencyProperty.RegisterAttached(
            "Flex",
            typeof(double),
            typeof(FlexPanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure
            )
        );

    // Getter / Setter
    public static double GetFlex(UIElement element)
        => (double)element.GetValue(FlexProperty);
    
    public static void SetFlex(UIElement element, double value)
        => element.SetValue(FlexProperty, value);

    // Attached Property для выравнивания
    public static readonly DependencyProperty AlignProperty =
        DependencyProperty.RegisterAttached(
            "Align",
            typeof(string),
            typeof(FlexPanel),
            new FrameworkPropertyMetadata(
                "stretch",
                FrameworkPropertyMetadataOptions.AffectsParentArrange
            )
        );

    public static string GetAlign(UIElement element)
        => (string)element.GetValue(AlignProperty);
    
    public static void SetAlign(UIElement element, string value)
        => element.SetValue(AlignProperty, value);
}
```

```xml
<!-- Использование -->
<local:FlexPanel>
    <Button Content="Flex=1" local:FlexPanel.Flex="1"/>
    <Button Content="Flex=2" local:FlexPanel.Flex="2"/>
    <Button Content="Align=center" local:FlexPanel.Align="center"/>
</local:FlexPanel>
```

#### Пример 4: Реальное DP из DotElectric (Coordinate)

```csharp
// Упрощённая версия из проекта
public class CoordinateControl : UserControl
{
    public static readonly DependencyProperty MicronsProperty =
        DependencyProperty.Register(
            "Microns",
            typeof(long),
            typeof(CoordinateControl),
            new FrameworkPropertyMetadata(
                0L,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnMicronsChanged,
                CoerceMicrons
            )
        );

    public long Microns
    {
        get => (long)GetValue(MicronsProperty);
        set => SetValue(MicronsProperty, value);
    }

    // Производное свойство (только для чтения)
    public double Millimeters => Microns / 1000.0;

    private static void OnMicronsChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        var control = (CoordinateControl)d;
        // Обновляем TextBox с мм
        control.UpdateDisplay();
    }

    private static object CoerceMicrons(DependencyObject d, object baseValue)
    {
        // Округляем до 100 микрон (0.1 мм)
        var value = (long)baseValue;
        return (value / 100) * 100;
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 1.2.1: Простое DP**

Создайте пользовательский контроль `ColorBox`:
- DP `FillColor` типа `Brush`
- DP `BorderColor` типа `Brush`
- DP `CornerRadius` типа `CornerRadius`
- Отображает прямоугольник с заданными параметрами

**Задача 1.2.2: Attached Property для позиционирования**

Создайте панель `AbsolutePanel` с Attached Properties:
- `X` (double) — координата X
- `Y` (double) — координата Y
- Реализуйте `MeasureOverride` и `ArrangeOverride` для позиционирования детей

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 1.2.3: DP с валидацией и coercion**

Создайте контроль `PercentageSlider`:
- DP `Value` типа `int` с диапазоном [0, 100]
- DP `Step` типа `int` со значением по умолчанию 5
- Валидация: значение должно быть кратно `Step`
- Coercion: округление до ближайшего `Step`
- PropertyChangedCallback для обновления визуального состояния

**Задача 1.2.4: Attached Property для сетки**

Создайте Attached Properties для кастомной `ResponsiveGrid`:
- `MinWidth` — минимальная ширина элемента
- `MaxWidth` — максимальная ширина
- `CollapseBelow` — скрывать элемент при ширине окна меньше указанной
- Реализуйте логику скрытия/показа элементов

---

#### 🔴 Продвинутый уровень (3 часа)

**Задача 1.2.5: Система стилей через DP**

Реализуйте мини-систему стилей:
- Attached Property `StyleManager.Class` для назначения CSS-подобных классов
- `StyleManager.Styles` — коллекция стилей в ресурсах
- Поддержка селекторов: `.class`, `#id`, `ElementType`
- Автоматическое применение стилей при изменении класса

```xml
<StackPanel local:StyleManager.Styles="{StaticResource MyStyles}">
    <Button Content="OK" local:StyleManager.Class="primary"/>
    <Button Content="Cancel" local:StyleManager.Class="secondary"/>
</StackPanel>
```

**Задача 1.2.6: DP с анимацией**

Создайте `AnimatedControl`:
- DP `AnimatedValue` с поддержкой анимации
- При изменении значения — плавная анимация от старого к новому
- DP `AnimationDuration` для настройки длительности
- DP `EasingFunction` для выбора функции плавности

---

### Решения

<details>
<summary>✅ Решение задачи 1.2.1</summary>

```csharp
public class ColorBox : Control
{
    public static readonly DependencyProperty FillColorProperty =
        DependencyProperty.Register(
            "FillColor",
            typeof(Brush),
            typeof(ColorBox),
            new PropertyMetadata(Brushes.White)
        );

    public static readonly DependencyProperty BorderColorProperty =
        DependencyProperty.Register(
            "BorderColor",
            typeof(Brush),
            typeof(ColorBox),
            new PropertyMetadata(Brushes.Black)
        );

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ColorBox),
            new PropertyMetadata(new CornerRadius(0))
        );

    public Brush FillColor
    {
        get => (Brush)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public Brush BorderColor
    {
        get => (Brush)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static ColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorBox),
            new FrameworkPropertyMetadata(typeof(ColorBox))
        );
    }
}
```

```xml
<!-- Themes/Generic.xaml -->
<Style TargetType="local:ColorBox">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="local:ColorBox">
                <Border Background="{TemplateBinding FillColor}"
                        BorderBrush="{TemplateBinding BorderColor}"
                        BorderThickness="1"
                        CornerRadius="{TemplateBinding CornerRadius}"/>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```
</details>

<details>
<summary>✅ Решение задачи 1.2.2</summary>

```csharp
public class AbsolutePanel : Panel
{
    public static readonly DependencyProperty XProperty =
        DependencyProperty.RegisterAttached(
            "X",
            typeof(double),
            typeof(AbsolutePanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsParentArrange
            )
        );

    public static readonly DependencyProperty YProperty =
        DependencyProperty.RegisterAttached(
            "Y",
            typeof(double),
            typeof(AbsolutePanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsParentArrange
            )
        );

    public static double GetX(UIElement element) 
        => (double)element.GetValue(XProperty);
    
    public static void SetX(UIElement element, double value) 
        => element.SetValue(XProperty, value);

    public static double GetY(UIElement element) 
        => (double)element.GetValue(YProperty);
    
    public static void SetY(UIElement element, double value) 
        => element.SetValue(YProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
        }
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            double x = GetX(child);
            double y = GetY(child);
            
            Size desiredSize = child.DesiredSize;
            child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));
        }
        return finalSize;
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 1.2.3</summary>

```csharp
public class PercentageSlider : Control
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value",
            typeof(int),
            typeof(PercentageSlider),
            new PropertyMetadata(
                0,
                OnValueChanged,
                CoerceValue
            ),
            ValidateValue
        );

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            "Step",
            typeof(int),
            typeof(PercentageSlider),
            new PropertyMetadata(5, OnStepChanged)
        );

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Step
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    private static bool ValidateValue(object value)
    {
        if (value is int v)
            return v >= 0 && v <= 100;
        return false;
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = (PercentageSlider)d;
        var value = (int)baseValue;
        
        // Округляем до ближайшего Step
        int rounded = (int)Math.Round((double)value / control.Step) * control.Step;
        return Math.Max(0, Math.Min(100, rounded));
    }

    private static void OnValueChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        var control = (PercentageSlider)d;
        control.UpdateVisualState();
    }

    private static void OnStepChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        var control = (PercentageSlider)d;
        // Принудительно применяем coercion при изменении Step
        control.CoerceValue(ValueProperty);
    }

    private void UpdateVisualState()
    {
        // Обновление UI (прогресс-бар, текст и т.д.)
    }
}
```
</details>

---

## Ключевые выводы

✅ **Dependency Property** — основа системы свойств WPF (binding, styling, animation)  
✅ **Attached Property** — позволяет "прикреплять" свойства к любым элементам  
✅ **PropertyMetadata** включает: default value, PropertyChangedCallback, CoerceValueCallback  
✅ **ValidateValueCallback** — валидация ДО установки значения  
✅ **FrameworkPropertyMetadataOptions** — влияет на layout (AffectsMeasure, AffectsArrange, AffectsRender)

---

## Дополнительные ресурсы

- [Dependency Properties Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview)
- [Attached Properties Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/attached-properties-overview)
- [Property Metadata](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/property-metadata)
