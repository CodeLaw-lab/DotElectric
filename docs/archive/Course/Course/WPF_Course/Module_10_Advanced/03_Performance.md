# Тема 10.3: Performance Optimization

### Теория

**Оптимизация производительности** в WPF включает несколько аспектов:
- Freezable объекты
- BitmapCache
- Binding оптимизация
- Layout оптимизация

### Примеры кода

#### Пример 1: Freezable объекты

```csharp
// ❌ Без Freeze - создаётся новый объект каждый раз
private void DrawWithoutFreeze()
{
    for (int i = 0; i < 1000; i++)
    {
        var brush = new SolidColorBrush(Colors.Blue);
        // Используется один раз и удаляется
    }
}

// ✅ С Freeze - переиспользуется
private void DrawWithFreeze()
{
    var brush = new SolidColorBrush(Colors.Blue);
    brush.Freeze(); // Делает объект неизменяемым
    
    for (int i = 0; i < 1000; i++)
    {
        // Переиспользует один объект
    }
}

// Freeze для Geometry
var geometry = new PathGeometry();
// ... добавляем фигуры
geometry.Freeze();

// Freeze для Pen
var pen = new Pen(Brushes.Blue, 2);
pen.Freeze();

// Freeze для Transform
var transform = new RotateTransform(45);
transform.Freeze();
```

#### Пример 2: BitmapCache

```xml
<!-- Без кэша - перерисовка каждый раз -->
<Grid>
    <Grid.RenderTransform>
        <RotateTransform x:Name="rotate" Angle="0"/>
    </Grid.RenderTransform>
    
    <StackPanel>
        <!-- Сложный контент -->
        <ListBox ItemsSource="{Binding Items}"/>
        <DataGrid ItemsSource="{Binding Data}"/>
    </StackPanel>
</Grid>

<!-- С BitmapCache - кэшируется как bitmap -->
<Grid>
    <Grid.CacheMode>
        <BitmapCache EnableClearType="True"
                     SnapsToDevicePixels="True"
                     RenderAtScale="1"/>
    </Grid.CacheMode>
    
    <Grid.RenderTransform>
        <RotateTransform x:Name="rotate" Angle="0"/>
    </Grid.RenderTransform>
    
    <StackPanel>
        <ListBox ItemsSource="{Binding Items}"/>
        <DataGrid ItemsSource="{Binding Data}"/>
    </StackPanel>
</Grid>
```

#### Пример 3: Binding оптимизация

```xml
<!-- ❌ Неоптимально -->
<TextBlock Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ✅ Оптимально для чтения -->
<TextBlock Text="{Binding Name, Mode=OneWay}"/>

<!-- ❌ Много binding на один объект -->
<TextBlock Text="{Binding Person.Name}"/>
<TextBlock Text="{Binding Person.Age}"/>
<TextBlock Text="{Binding Person.Email}"/>

<!-- ✅ Один DataContext -->
<StackPanel DataContext="{Binding Person}">
    <TextBlock Text="{Binding Name}"/>
    <TextBlock Text="{Binding Age}"/>
    <TextBlock Text="{Binding Email}"/>
</StackPanel>

<!-- ❌ Конвертер создаётся каждый раз -->
<TextBlock Text="{Binding Date, Converter={local:DateConverter}}"/>

<!-- ✅ Конвертер в ресурсах -->
<Window.Resources>
    <local:DateConverter x:Key="DateConverter"/>
</Window.Resources>
<TextBlock Text="{Binding Date, Converter={StaticResource DateConverter}}"/>
```

#### Пример 4: Layout оптимизация

```xml
<!-- ❌ Глубокая вложенность -->
<Grid>
    <StackPanel>
        <Grid>
            <StackPanel>
                <Grid>
                    <TextBlock Text="Deep nested"/>
                </Grid>
            </StackPanel>
        </Grid>
    </StackPanel>
</Grid>

<!-- ✅ Плоская структура -->
<Grid>
    <TextBlock Text="Flat structure"/>
</Grid>

<!-- ❌ LayoutTransform (дороже) -->
<Button Content="Click">
    <Button.LayoutTransform>
        <RotateTransform Angle="45"/>
    </Button.LayoutTransform>
</Button>

<!-- ✅ RenderTransform (дешевле) -->
<Button Content="Click">
    <Button.RenderTransform>
        <RotateTransform Angle="45"/>
    </Button.RenderTransform>
</Button>

<!-- ❌ Auto размер для больших списков -->
<ListBox Height="Auto">
    <!-- 1000 элементов -->
</ListBox>

<!-- ✅ Фиксированный размер -->
<ListBox Height="500">
    <!-- 1000 элементов -->
</ListBox>
```

#### Пример 5: Shared Size Group оптимизация

```xml
<!-- ❌ Одинаковая ширина вручную -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="150"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid Grid.Row="1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="150"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
    </Grid>
</Grid>

<!-- ✅ SharedSizeGroup -->
<Grid Grid.IsSharedSizeScope="True">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid Grid.Row="1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
    </Grid>
</Grid>
```

#### Пример 6: Deferred Loading

```csharp
// Отложенная загрузка изображений
public class LazyImageLoader
{
    private readonly Dictionary<string, BitmapImage> _cache;
    
    public LazyImageLoader()
    {
        _cache = new Dictionary<string, BitmapImage>();
    }
    
    public BitmapImage LoadImage(string path)
    {
        if (!_cache.ContainsKey(path))
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 200; // Оптимизация памяти
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze для производительности
            
            _cache[path] = bitmap;
        }
        
        return _cache[path];
    }
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// GridNodesLayer.cs - оптимизация рендеринга сетки
public class GridNodesLayer : UIElement
{
    private readonly DrawingVisual _visual;
    private readonly Pen _gridPen;
    private readonly SolidColorBrush _nodeBrush;
    
    public GridNodesLayer()
    {
        _visual = new DrawingVisual();
        AddVisualChild(_visual);
        
        // Freeze кистей и перьев для производительности
        _gridPen = new Pen(Brushes.LightGray, 0.5);
        _gridPen.Freeze();
        
        _nodeBrush = new SolidColorBrush(Colors.Gray);
        _nodeBrush.Freeze();
        
        RenderGrid();
    }
    
    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _visual;
    
    private void RenderGrid()
    {
        using (var context = _visual.RenderOpen())
        {
            double step = GridStepMm * 96 / 25.4 * Zoom;
            
            // Ограничение количества узлов для производительности
            const int MaxNodes = 10000;
            int nodesX = (int)(ActualWidth / step) + 1;
            int nodesY = (int)(ActualHeight / step) + 1;
            
            if (nodesX * nodesY > MaxNodes)
            {
                // Рисуем только линии без узлов
                DrawGridLinesOnly(context, step);
                return;
            }
            
            // Рисуем узлы
            for (double x = 0; x < ActualWidth; x += step)
            {
                for (double y = 0; y < ActualHeight; y += step)
                {
                    context.DrawEllipse(
                        _nodeBrush,  // Frozen brush
                        null,
                        new Point(x, y),
                        1.5 * Zoom,
                        1.5 * Zoom
                    );
                }
            }
        }
    }
    
    private void DrawGridLinesOnly(DrawingContext context, double step)
    {
        // Только линии для больших сеток
        for (double x = 0; x < ActualWidth; x += step)
        {
            context.DrawLine(_gridPen, new Point(x, 0), new Point(x, ActualHeight));
        }
        
        for (double y = 0; y < ActualHeight; y += step)
        {
            context.DrawLine(_gridPen, new Point(0, y), new Point(ActualWidth, y));
        }
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 10.3.1: Freeze Brush**

Создайте кисть:
- SolidColorBrush
- Freeze()
- Использование в DrawingContext

**Задача 10.3.2: OneWay Binding**

Измените binding:
- TextBlock с OneWay
- Уберите UpdateSourceTrigger
- Проверьте работу

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 10.3.3: BitmapCache**

Добавьте кэш:
- Сложный UI элемент
- BitmapCache на Grid
- Анимация вращения

**Задача 10.3.4: Layout Optimization**

Оптимизируйте layout:
- Уменьшите вложенность
- Замените LayoutTransform на RenderTransform
- Замерьте FPS

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 10.3.5: Performance Profiler**

Используйте profiler:
- Visual Studio Profiler
- Найдите узкие места
- Оптимизируйте

**Задача 10.3.6: Image Optimizer**

Создайте загрузчик:
- Deferred loading
- DecodePixelWidth
- Freeze BitmapImage
- Кэширование

---

### Решения

<details>
<summary>✅ Решение задачи 10.3.1</summary>

```csharp
// Создаём frozen кисть
var brush = new SolidColorBrush(Colors.Blue);
brush.Freeze();

// Используем в DrawingContext
using (var context = visual.RenderOpen())
{
    context.DrawRectangle(brush, null, new Rect(0, 0, 100, 50));
}
```
</details>

<details>
<summary>✅ Решение задачи 10.3.3</summary>

```xml
<Grid>
    <Grid.CacheMode>
        <BitmapCache EnableClearType="True" RenderAtScale="1"/>
    </Grid.CacheMode>
    
    <Grid.RenderTransform>
        <RotateTransform x:Name="rotate" Angle="0"/>
    </Grid.RenderTransform>
    
    <StackPanel>
        <ListBox ItemsSource="{Binding Items}"/>
    </StackPanel>
    
    <Grid.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetName="rotate"
                                     Storyboard.TargetProperty="Angle"
                                     From="0" To="360"
                                     Duration="0:0:5"
                                     RepeatBehavior="Forever"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Grid.Triggers>
</Grid>
```
</details>

---

## Ключевые выводы

✅ **Freezable.Freeze()** — делает объект неизменяемым и быстрым  
✅ **BitmapCache** — кэширует визуал в bitmap  
✅ **OneWay binding** — для чтения (быстрее TwoWay)  
✅ **RenderTransform** — дешевле чем LayoutTransform  
✅ **SharedSizeGroup** — для одинаковых размеров колонок  
✅ **Deferred loading** — отложенная загрузка изображений  
✅ **Profiler** — для поиска узких мест

---

## Дополнительные ресурсы

- [Freezable](https://docs.microsoft.com/en-us/dotnet/api/system.windows.freezable)
- [BitmapCache](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.bitmapcache)
- [Performance Profiler](https://docs.microsoft.com/en-us/visualstudio/profiling/)
