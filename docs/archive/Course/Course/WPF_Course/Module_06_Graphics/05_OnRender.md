# Тема 6.5: OnRender Override

### Теория

**OnRender** — метод для кастомного рендеринга в UIElement.

#### Когда переопределять OnRender

✅ **Кастомный элемент** — свой контроль с уникальным видом  
✅ **Производительность** — один проход рендеринга  
✅ **Динамическая графика** — изменение в runtime  
❌ **Сложность** — нет дизайнера для XAML  
❌ **Перерисовка** — InvalidateVisual для обновления

#### OnRender vs DrawingVisual

| Характеристика | OnRender | DrawingVisual |
|----------------|----------|---------------|
| **Базовый класс** | UIElement | DrawingVisual |
| **События** | Да | Нет (нужен хит-тест) |
| **Layout** | Да | Нет |
| **Производительность** | Хорошая | Отличная |
| **Использование** | Кастомные контролы | Тысячи объектов |

### Примеры кода

#### Пример 1: Простой OnRender

```csharp
public class SimpleShape : UIElement
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        // Рисуем прямоугольник
        drawingContext.DrawRectangle(
            Brushes.LightBlue,           // кисть заполнения
            new Pen(Brushes.Blue, 2),   // кисть обводки
            new Rect(0, 0, 100, 50)     // прямоугольник
        );
        
        // Рисуем эллипс
        drawingContext.DrawEllipse(
            Brushes.LightGreen,
            new Pen(Brushes.Green, 2),
            new Point(150, 25),
            40,
            20
        );
        
        // Рисуем линию
        drawingContext.DrawLine(
            new Pen(Brushes.Red, 3),
            new Point(0, 70),
            new Point(200, 70)
        );
    }
}
```

```xml
<!-- Использование -->
<StackPanel>
    <local:SimpleShape Width="200" Height="100"/>
</StackPanel>
```

#### Пример 2: Кастомный ProgressBar

```csharp
public class CustomProgressBar : RangeBase
{
    static CustomProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CustomProgressBar),
            new FrameworkPropertyMetadata(typeof(CustomProgressBar))
        );
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        double width = ActualWidth;
        double height = ActualHeight;
        double progress = (Value - Minimum) / (Maximum - Minimum);
        
        // Фон
        drawingContext.DrawRectangle(
            Brushes.LightGray,
            new Pen(Brushes.Gray, 1),
            new Rect(0, 0, width, height)
        );
        
        // Прогресс (градиент)
        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0)
        };
        gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(100, 200, 100), 0.0));
        gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(50, 150, 50), 1.0));
        
        drawingContext.DrawRectangle(
            gradientBrush,
            null,
            new Rect(0, 0, width * progress, height)
        );
        
        // Текст с процентом
        var text = new FormattedText(
            $"{progress * 100:F0}%",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial Bold"),
            14,
            Brushes.Black
        );
        
        drawingContext.DrawText(
            text,
            new Point(
                (width - text.Width) / 2,
                (height - text.Height) / 2
            )
        );
    }
}
```

```xml
<!-- Использование -->
<local:CustomProgressBar Minimum="0" Maximum="100" Value="75"
                         Width="300" Height="30"/>
```

#### Пример 3: Сетка через OnRender

```csharp
public class GridCanvas : Canvas
{
    public static readonly DependencyProperty GridStepProperty =
        DependencyProperty.Register(
            "GridStep",
            typeof(double),
            typeof(GridCanvas),
            new PropertyMetadata(20.0, (d, e) => d.InvalidateVisual())
        );

    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(
            "ShowGrid",
            typeof(bool),
            typeof(GridCanvas),
            new PropertyMetadata(true, (d, e) => d.InvalidateVisual())
        );

    public double GridStep
    {
        get => (double)GetValue(GridStepProperty);
        set => SetValue(GridStepProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        if (!ShowGrid) return;
        
        double step = GridStep;
        double width = ActualWidth;
        double height = ActualHeight;
        
        var pen = new Pen(Brushes.LightGray, 0.5);
        
        // Вертикальные линии
        for (double x = 0; x < width; x += step)
        {
            drawingContext.DrawLine(
                pen,
                new Point(x, 0),
                new Point(x, height)
            );
        }
        
        // Горизонтальные линии
        for (double y = 0; y < height; y += step)
        {
            drawingContext.DrawLine(
                pen,
                new Point(0, y),
                new Point(width, y)
            );
        }
        
        // Узлы сетки (точки)
        var brush = Brushes.Gray;
        for (double x = 0; x < width; x += step)
        {
            for (double y = 0; y < height; y += step)
            {
                drawingContext.DrawEllipse(
                    brush,
                    null,
                    new Point(x, y),
                    1.5,
                    1.5
                );
            }
        }
    }
}
```

#### Пример 4: Кастомный Knob (регулятор)

```csharp
public class KnobControl : RangeBase
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(KnobControl),
            new PropertyMetadata("", (d, e) => d.InvalidateVisual())
        );

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    static KnobControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(KnobControl),
            new FrameworkPropertyMetadata(typeof(KnobControl))
        );
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        double width = ActualWidth;
        double height = ActualHeight;
        double centerX = width / 2;
        double centerY = height / 2;
        double radius = Math.Min(width, height) / 2 - 5;
        
        // Фон (круг)
        drawingContext.DrawEllipse(
            Brushes.White,
            new Pen(Brushes.Gray, 2),
            new Point(centerX, centerY),
            radius,
            radius
        );
        
        // Угол поворота
        double normalizedValue = (Value - Minimum) / (Maximum - Minimum);
        double angle = normalizedValue * 270 - 135; // от -135 до +135 градусов
        double angleRad = angle * Math.PI / 180;
        
        // Линия индикатора
        double lineLength = radius * 0.8;
        double endX = centerX + lineLength * Math.Cos(angleRad);
        double endY = centerY + lineLength * Math.Sin(angleRad);
        
        drawingContext.DrawLine(
            new Pen(Brushes.Blue, 3),
            new Point(centerX, centerY),
            new Point(endX, endY)
        );
        
        // Центральная точка
        drawingContext.DrawEllipse(
            Brushes.Blue,
            null,
            new Point(centerX, centerY),
            5,
            5
        );
        
        // Текст
        if (!string.IsNullOrEmpty(Text))
        {
            var formattedText = new FormattedText(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Black
            );
            
            drawingContext.DrawText(
                formattedText,
                new Point(
                    centerX - formattedText.Width / 2,
                    height - 20
                )
            );
        }
    }
}
```

#### Пример 5: Анимированный OnRender

```csharp
public class PulsingCircle : UIElement
{
    private double _radius;
    private bool _expanding;
    private readonly DispatcherTimer _timer;

    public PulsingCircle()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _timer.Tick += (s, e) =>
        {
            if (_expanding)
            {
                _radius += 2;
                if (_radius >= Math.Min(ActualWidth, ActualHeight) / 2)
                    _expanding = false;
            }
            else
            {
                _radius -= 2;
                if (_radius <= 10)
                    _expanding = true;
            }
            InvalidateVisual(); // Принудительная перерисовка
        };
        _timer.Start();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        double centerX = ActualWidth / 2;
        double centerY = ActualHeight / 2;
        
        // Пульсирующий круг
        drawingContext.DrawEllipse(
            Brushes.LightBlue,
            new Pen(Brushes.Blue, 2),
            new Point(centerX, centerY),
            _radius,
            _radius
        );
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _timer.IsEnabled = !_timer.IsEnabled;
    }
}
```

#### Пример 6: Реальное использование из DotElectric

```csharp
// GridNodesLayer.cs — оптимизированная версия через OnRender
public class GridNodesLayer : UIElement
{
    public static readonly DependencyProperty GridStepMmProperty =
        DependencyProperty.Register(
            "GridStepMm",
            typeof(double),
            typeof(GridNodesLayer),
            new PropertyMetadata(5.0, (d, e) => d.InvalidateVisual())
        );

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(
            "Zoom",
            typeof(double),
            typeof(GridNodesLayer),
            new PropertyMetadata(1.0, (d, e) => d.InvalidateVisual())
        );

    public static readonly DependencyProperty PanOffsetXProperty =
        DependencyProperty.Register(
            "PanOffsetX",
            typeof(double),
            typeof(GridNodesLayer),
            new PropertyMetadata(0.0, (d, e) => d.InvalidateVisual())
        );

    public static readonly DependencyProperty PanOffsetYProperty =
        DependencyProperty.Register(
            "PanOffsetY",
            typeof(double),
            typeof(GridNodesLayer),
            new PropertyMetadata(0.0, (d, e) => d.InvalidateVisual())
        );

    public double GridStepMm
    {
        get => (double)GetValue(GridStepMmProperty);
        set => SetValue(GridStepMmProperty, value);
    }

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double PanOffsetX
    {
        get => (double)GetValue(PanOffsetXProperty);
        set => SetValue(PanOffsetXProperty, value);
    }

    public double PanOffsetY
    {
        get => (double)GetValue(PanOffsetYProperty);
        set => SetValue(PanOffsetYProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        double stepPixels = GridStepMm * 96 / 25.4 * Zoom;
        double width = ActualWidth;
        double height = ActualHeight;
        
        // Ограничение количества узлов для производительности
        const int MaxNodes = 10000;
        int nodesX = (int)(width / stepPixels) + 1;
        int nodesY = (int)(height / stepPixels) + 1;
        
        if (nodesX * nodesY > MaxNodes)
        {
            // Слишком много узлов — рисуем только линии
            DrawGridLines(drawingContext, stepPixels, width, height);
            return;
        }
        
        // Рисуем узлы сетки
        double offsetX = PanOffsetX % stepPixels;
        double offsetY = PanOffsetY % stepPixels;
        
        var nodeBrush = Brushes.Gray;
        double nodeRadius = Math.Max(1.5 * Zoom, 1);
        
        for (double x = offsetX; x < width; x += stepPixels)
        {
            for (double y = offsetY; y < height; y += stepPixels)
            {
                drawingContext.DrawEllipse(
                    nodeBrush,
                    null,
                    new Point(x, y),
                    nodeRadius,
                    nodeRadius
                );
            }
        }
    }

    private void DrawGridLines(DrawingContext drawingContext, 
                               double stepPixels, double width, double height)
    {
        var pen = new Pen(Brushes.LightGray, 0.5);
        double offsetX = PanOffsetX % stepPixels;
        double offsetY = PanOffsetY % stepPixels;
        
        // Только линии
        for (double x = offsetX; x < width; x += stepPixels)
        {
            drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, height));
        }
        
        for (double y = offsetY; y < height; y += stepPixels)
        {
            drawingContext.DrawLine(pen, new Point(0, y), new Point(width, y));
        }
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 6.5.1: Simple Shape**

Создайте UIElement с OnRender:
- Прямоугольник
- Эллипс
- Линия

**Задача 6.5.2: Colored Rectangle**

Создайте кастомный элемент:
- Зависимость свойство Fill
- Зависимость свойство BorderThickness
- Отрисовка через OnRender

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 6.5.3: Custom Button**

Создайте кнопку:
- Круглая форма
- Hover эффект (изменение цвета)
- Click эффект (уменьшение)

**Задача 6.5.4: Gradient Bar**

Создайте градиентную полосу:
- LinearGradientBrush
- Зависимость свойство Angle
- Перерисовка при изменении

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 6.5.5: Wave Visualizer**

Создайте визуализатор волны:
- Синусоида через DrawLine
- Анимация через DispatcherTimer
- Несколько волн с разной частотой

**Задача 6.5.6: Performance Grid**

Реализуйте оптимизированную сетку:
- Ограничение MaxNodes
- Переключение линии/узлы
- Виртуализация (только видимые)

---

### Решения

<details>
<summary>✅ Решение задачи 6.5.1</summary>

```csharp
public class SimpleShape : UIElement
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        // Прямоугольник
        drawingContext.DrawRectangle(
            Brushes.LightBlue,
            new Pen(Brushes.Blue, 2),
            new Rect(0, 0, 80, 50)
        );
        
        // Эллипс
        drawingContext.DrawEllipse(
            Brushes.LightGreen,
            new Pen(Brushes.Green, 2),
            new Point(120, 25),
            30,
            20
        );
        
        // Линия
        drawingContext.DrawLine(
            new Pen(Brushes.Red, 3),
            new Point(0, 70),
            new Point(200, 70)
        );
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 6.5.4</summary>

```csharp
public class GradientBar : UIElement
{
    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(
            "Angle",
            typeof(double),
            typeof(GradientBar),
            new PropertyMetadata(45.0, (d, e) => d.InvalidateVisual())
        );

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        double width = ActualWidth;
        double height = ActualHeight;
        
        // Вычисляем точки градиента
        double rad = Angle * Math.PI / 180;
        var start = new Point(width / 2 - width * Math.Cos(rad) / 2, 
                              height / 2 - height * Math.Sin(rad) / 2);
        var end = new Point(width / 2 + width * Math.Cos(rad) / 2, 
                            height / 2 + height * Math.Sin(rad) / 2);
        
        var brush = new LinearGradientBrush();
        brush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
        brush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.5));
        brush.GradientStops.Add(new GradientStop(Colors.Green, 1.0));
        brush.StartPoint = start;
        brush.EndPoint = end;
        
        drawingContext.DrawRectangle(
            brush,
            new Pen(Brushes.Gray, 1),
            new Rect(0, 0, width, height)
        );
    }
}
```
</details>

---

## Ключевые выводы

✅ **OnRender** — метод для кастомного рендеринга  
✅ **DrawingContext** — команды рисования  
✅ **InvalidateVisual()** — принудительная перерисовка  
✅ **DependencyProperty** — свойства с перерисовкой  
✅ **Производительность** — кэшируйте кисти и перья  
✅ **Ограничение** — MaxNodes для больших коллекций  
✅ **OverrideMetadata** — для кастомных контролов

---

## Дополнительные ресурсы

- [OnRender](https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.onrender)
- [DrawingContext](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.drawingcontext)
- [InvalidateVisual](https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.invalidatevisual)
