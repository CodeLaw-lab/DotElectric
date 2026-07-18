# Тема 6.4: Visual Layer и DrawingVisual

### Теория

**Visual Layer** — низкоуровневый API для рендеринга в WPF.

#### Иерархия

```
System.Windows.Media.Visual
    └── DrawingVisual (для рисования)
        └── ContainerVisual (для контейнеров)

System.Windows.UIElement (высокий уровень)
    └── FrameworkElement
        └── Shape, Control, etc.
```

#### Когда использовать Visual Layer

✅ **Производительность** — тысячи объектов  
✅ **Низкоуровневый контроль** — прямой доступ к рендерингу  
✅ **Кастомные визуалы** — свои классы  
❌ **Сложность** — нет событий, команд, binding  
❌ **No layout** — ручное позиционирование

### Примеры кода

#### Пример 1: Простой DrawingVisual

```csharp
public class SimpleVisual : DrawingVisual
{
    public SimpleVisual()
    {
        using (var context = RenderOpen())
        {
            // Рисуем прямоугольник
            context.DrawRectangle(
                Brushes.LightBlue,           // кисть заполнения
                new Pen(Brushes.Blue, 2),   // кисть обводки
                new Rect(0, 0, 100, 50)     // прямоугольник
            );
            
            // Рисуем текст
            context.DrawText(
                new FormattedText(
                    "Hello Visual Layer!",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    12,
                    Brushes.Black
                ),
                new Point(10, 10)
            );
        }
    }
}
```

```xml
<!-- MainWindow.xaml -->
<Window x:Class="WpfApp.MainWindow">
    <Canvas x:Name="VisualCanvas"/>
</Window>
```

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var visual = new SimpleVisual();
        VisualCanvas.Children.Add(visual);
    }
}
```

#### Пример 2: VisualCollection

```csharp
public class VisualContainer : UIElement
{
    private readonly VisualCollection _children;
    private readonly List<DrawingVisual> _visuals;

    public VisualContainer()
    {
        _children = new VisualCollection(this);
        _visuals = new List<DrawingVisual>();
        
        // Добавляем 1000 прямоугольников
        for (int i = 0; i < 1000; i++)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                double x = (i % 50) * 20;
                double y = (i / 50) * 20;
                
                context.DrawRectangle(
                    GetBrush(i % 10),
                    null,
                    new Rect(x, y, 18, 18)
                );
            }
            _visuals.Add(visual);
            _children.Add(visual);
        }
    }

    protected override int VisualChildrenCount => _children.Count;

    protected override Visual GetVisualChild(int index) => _children[index];

    private Brush GetBrush(int index) => index switch
    {
        0 => Brushes.Red,
        1 => Brushes.Green,
        2 => Brushes.Blue,
        3 => Brushes.Yellow,
        4 => Brushes.Orange,
        5 => Brushes.Purple,
        6 => Brushes.Pink,
        7 => Brushes.Cyan,
        8 => Brushes.Magenta,
        _ => Brushes.Gray
    };
}
```

#### Пример 3: Hit Testing

```csharp
public class InteractiveVisual : DrawingVisual
{
    private bool _isHovered;
    private readonly Rect _bounds;

    public InteractiveVisual(Rect bounds)
    {
        _bounds = bounds;
        Render();
    }

    private void Render()
    {
        using (var context = RenderOpen())
        {
            var brush = _isHovered ? Brushes.Red : Brushes.Blue;
            context.DrawRectangle(brush, null, _bounds);
        }
    }

    public bool HitTest(Point point)
    {
        return _bounds.Contains(point);
    }

    public void SetHovered(bool hovered)
    {
        if (_isHovered != hovered)
        {
            _isHovered = hovered;
            Render();
        }
    }
}

// В MainWindow
public partial class MainWindow : Window
{
    private readonly List<InteractiveVisual> _visuals;

    public MainWindow()
    {
        InitializeComponent();
        
        _visuals = new List<InteractiveVisual>();
        
        for (int i = 0; i < 10; i++)
        {
            var visual = new InteractiveVisual(
                new Rect(i * 50, 50, 40, 40)
            );
            _visuals.Add(visual);
            VisualCanvas.Children.Add(visual);
        }
        
        VisualCanvas.MouseMove += VisualCanvas_MouseMove;
    }

    private void VisualCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        var point = e.GetPosition(VisualCanvas);
        
        foreach (var visual in _visuals)
        {
            visual.SetHovered(visual.HitTest(point));
        }
    }
}
```

#### Пример 4: DrawingContext команды

```csharp
public class AdvancedVisual : DrawingVisual
{
    public AdvancedVisual()
    {
        using (var context = RenderOpen())
        {
            // 1. Rectangle
            context.DrawRectangle(
                Brushes.LightBlue,
                new Pen(Brushes.Blue, 2),
                new Rect(0, 0, 100, 50)
            );
            
            // 2. Ellipse
            context.DrawEllipse(
                Brushes.LightGreen,
                new Pen(Brushes.Green, 2),
                new Point(150, 25),
                40, 20
            );
            
            // 3. Line
            context.DrawLine(
                new Pen(Brushes.Red, 3),
                new Point(0, 70),
                new Point(200, 70)
            );
            
            // 4. Polygon (через PathGeometry)
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(50, 100),
                IsClosed = true
            };
            figure.Segments.Add(new LineSegment(new Point(75, 150), true));
            figure.Segments.Add(new LineSegment(new Point(25, 150), true));
            geometry.Figures.Add(figure);
            
            context.DrawGeometry(Brushes.Yellow, new Pen(Brushes.Orange, 2), geometry);
            
            // 5. Text
            var formattedText = new FormattedText(
                "Hello!",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                16,
                Brushes.Black
            );
            context.DrawText(formattedText, new Point(10, 170));
            
            // 6. Image
            var bitmap = new BitmapImage(
                new Uri("pack://application:,,,/images/icon.png")
            );
            context.DrawImage(bitmap, new Rect(100, 150, 50, 50));
            
            // 7. Rounded Rectangle
            var roundedRect = new RoundedRectGeometry(
                new Rect(200, 100, 80, 40),
                10
            );
            context.DrawGeometry(
                Brushes.LightCoral,
                new Pen(Brushes.Red, 2),
                roundedRect
            );
        }
    }
}
```

#### Пример 5: Анимация Visual

```csharp
public class AnimatedVisual : DrawingVisual
{
    private double _angle;
    private readonly Point _center;
    private readonly DispatcherTimer _timer;

    public AnimatedVisual(Point center, double radius)
    {
        _center = center;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _timer.Tick += (s, e) =>
        {
            _angle += 5;
            if (_angle >= 360) _angle = 0;
            Render();
        };
        _timer.Start();
        
        Render();
    }

    private void Render()
    {
        using (var context = RenderOpen())
        {
            // Вращающийся прямоугольник
            var transform = new RotateTransform(_angle, _center.X, _center.Y);
            context.PushTransform(transform);
            
            context.DrawRectangle(
                Brushes.LightBlue,
                new Pen(Brushes.Blue, 2),
                new Rect(
                    _center.X - 30,
                    _center.Y - 15,
                    60,
                    30
                )
            );
            
            context.Pop();
        }
    }

    public void Stop() => _timer.Stop();
}
```

#### Пример 6: Реальное использование из DotElectric

```csharp
// GridNodesLayer.cs — слой сетки через DrawingVisual
public class GridNodesLayer : UIElement
{
    private readonly DrawingVisual _visual;
    private readonly DrawingContext _context;
    
    private double _gridStepMm = 5.0;
    private double _zoom = 1.0;
    private double _offsetX;
    private double _offsetY;

    public GridNodesLayer()
    {
        _visual = new DrawingVisual();
        AddVisualChild(_visual);
        
        RenderGrid();
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => _visual;

    public double GridStepMm
    {
        get => _gridStepMm;
        set
        {
            _gridStepMm = value;
            RenderGrid();
        }
    }

    public double Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            RenderGrid();
        }
    }

    public double OffsetX
    {
        get => _offsetX;
        set
        {
            _offsetX = value;
            RenderGrid();
        }
    }

    public double OffsetY
    {
        get => _offsetY;
        set
        {
            _offsetY = value;
            RenderGrid();
        }
    }

    private void RenderGrid()
    {
        using (var context = _visual.RenderOpen())
        {
            double stepPixels = _gridStepMm * 96 / 25.4 * _zoom;
            double width = ActualWidth;
            double height = ActualHeight;

            // Рисуем узлы сетки (точки)
            var pen = new Pen(Brushes.LightGray, 0.5);
            
            for (double x = _offsetX % stepPixels; x < width; x += stepPixels)
            {
                for (double y = _offsetY % stepPixels; y < height; y += stepPixels)
                {
                    // Точка в узле сетки
                    context.DrawEllipse(
                        Brushes.Gray,
                        null,
                        new Point(x, y),
                        1.5 * _zoom,
                        1.5 * _zoom
                    );
                }
            }
        }
        
        InvalidateVisual();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RenderGrid();
    }
}
```

```xml
<!-- EditorCanvas.xaml -->
<UserControl>
    <Grid>
        <!-- Слой сетки -->
        <local:GridNodesLayer x:Name="GridLayer"
                              GridStepMm="{Binding StatusBarGridStepMm}"
                              Zoom="{Binding Zoom}"
                              OffsetX="{Binding PanOffsetX}"
                              OffsetY="{Binding PanOffsetY}"/>
        
        <!-- Слой объектов -->
        <ItemsControl ItemsSource="{Binding Template.Objects}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
        
        <!-- Слой выделения -->
        <local:SelectionLayer x:Name="SelectionLayer"
                              SelectedObject="{Binding SingleSelectedObject}"/>
    </Grid>
</UserControl>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 6.4.1: Простой Visual**

Создайте DrawingVisual:
- Прямоугольник 100x50
- Круг в центре
- Текст "Hello"

**Задача 6.4.2: Multiple Visuals**

Создайте 10 DrawingVisual:
- Разные цвета
- Расположение в ряд
- Добавьте в VisualCollection

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 6.4.3: Hit Testing**

Реализуйте:
- 5 интерактивных прямоугольников
- Hover эффект при наведении
- Click для изменения цвета

**Задача 6.4.4: Grid через Visual**

Создайте сетку:
- DrawingVisual для линий
- Вертикальные и горизонтальные линии
- Шаг 20px

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 6.4.5: Анимированные частицы**

Создайте систему частиц:
- 100+ частиц
- Анимация движения
- Отскок от краёв

**Задача 6.4.6: Mini Canvas Editor**

Реализуйте:
- Рисование через DrawingVisual
- Перемещение фигур
- Выделение фигур

---

### Решения

<details>
<summary>✅ Решение задачи 6.4.1</summary>

```csharp
public class SimpleVisual : DrawingVisual
{
    public SimpleVisual()
    {
        using (var context = RenderOpen())
        {
            // Прямоугольник
            context.DrawRectangle(
                Brushes.LightBlue,
                new Pen(Brushes.Blue, 2),
                new Rect(0, 0, 100, 50)
            );
            
            // Круг в центре
            context.DrawEllipse(
                Brushes.LightGreen,
                new Pen(Brushes.Green, 2),
                new Point(50, 25),
                15,
                15
            );
            
            // Текст
            var text = new FormattedText(
                "Hello",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Black
            );
            context.DrawText(text, new Point(25, 55));
        }
    }
}
```
</details>

---

## Ключевые выводы

✅ **DrawingVisual** — низкоуровневый визуал для рендеринга  
✅ **DrawingContext** — команды рисования (Rectangle, Ellipse, Line, Text)  
✅ **VisualCollection** — управление коллекцией визуалов  
✅ **Hit Testing** — ручная проверка попадания  
✅ **Производительность** — лучше для тысяч объектов  
✅ **No Layout** — ручное позиционирование  
✅ **RenderOpen()** — открывает контекст для рисования

---

## Дополнительные ресурсы

- [DrawingVisual](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.drawingvisual)
- [VisualCollection](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.visualcollection)
- [DrawingContext](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.drawingcontext)
