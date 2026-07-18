# Тема 3.1: Система Layout (Measure/Arrange)

### Теория

**Layout-система WPF** — двухпроходной механизм измерения и расположения элементов.

#### Два прохода layout

```
┌─────────────────────────────────────────────────────────┐
│                   Layout Pass                            │
├─────────────────────────────────────────────────────────┤
│  1. Measure Pass (измерение)                             │
│     ├─ Родитель спрашивает: "Сколько места тебе нужно?" │
│     └─ Элемент отвечает: DesiredSize                    │
├─────────────────────────────────────────────────────────┤
│  2. Arrange Pass (расположение)                          │
│     ├─ Родитель говорит: "Вот твоё место"               │
│     └─ Элемент занимает: FinalSize                      │
└─────────────────────────────────────────────────────────┘
```

#### Measure Pass

```csharp
protected override Size MeasureOverride(Size availableSize)
{
    // 1. Измеряем каждый дочерний элемент
    foreach (UIElement child in InternalChildren)
    {
        // Передаём доступный размер
        child.Measure(availableSize);
        
        // Получаем желаемый размер
        Size desired = child.DesiredSize;
    }
    
    // 2. Возвращаем наш желаемый размер
    return new Size(width, height);
}
```

#### Arrange Pass

```csharp
protected override Size ArrangeOverride(Size finalSize)
{
    // 1. Располагаем каждый дочерний элемент
    foreach (UIElement child in InternalChildren)
    {
        // Задаём финальную позицию и размер
        child.Arrange(new Rect(x, y, width, height));
    }
    
    // 2. Возвращаем фактически использованный размер
    return finalSize;
}
```

#### DesiredSize, RenderSize, ActualWidth

| Свойство | Описание | Когда доступно |
|----------|----------|----------------|
| **DesiredSize** | Желаемый размер после Measure | После MeasureOverride |
| **RenderSize** | Фактический размер для рендеринга | После ArrangeOverride |
| **ActualWidth** | Публичное свойство (читаемое) | После layout |
| **ActualHeight** | Публичное свойство (читаемое) | После layout |

### Примеры кода

#### Пример 1: Custom Panel с простым layout

```csharp
public class SimplePanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Size desiredSize = new Size(0, 0);
        
        // Измеряем каждый элемент
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            
            // Складываем желаемые размеры
            desiredSize.Width += child.DesiredSize.Width;
            desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
        }
        
        return desiredSize;
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        double xOffset = 0;
        
        // Располагаем элементы горизонтально
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(new Rect(
                xOffset, 
                0, 
                child.DesiredSize.Width, 
                finalSize.Height
            ));
            
            xOffset += child.DesiredSize.Width;
        }
        
        return finalSize;
    }
}
```

```xml
<!-- Использование -->
<local:SimplePanel>
    <Button Content="Button 1" Width="100"/>
    <Button Content="Button 2" Width="150"/>
    <Button Content="Button 3" Width="120"/>
</local:SimplePanel>
```

#### Пример 2: Влияние Alignment на layout

```xml
<Grid>
    <!-- Stretch (по умолчанию) -- заполняет доступное место -->
    <Button Content="Stretch" HorizontalAlignment="Stretch"/>
    
    <!-- Center -- занимает DesiredSize, центрируется -->
    <Button Content="Center" HorizontalAlignment="Center"/>
    
    <!-- Left -- занимает DesiredSize, прижат влево -->
    <Button Content="Left" HorizontalAlignment="Left"/>
    
    <!-- Right -- занимает DesiredSize, прижат вправо -->
    <Button Content="Right" HorizontalAlignment="Right"/>
</Grid>
```

#### Пример 3: Layout rounding

```csharp
// Включение pixel snapping для чёткой графики
public class SnappingCanvas : Canvas
{
    protected override Size MeasureOverride(Size constraint)
    {
        // Округляем до целых пикселей
        double width = Math.Ceiling(constraint.Width);
        double height = Math.Ceiling(constraint.Height);
        
        return new Size(width, height);
    }
    
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        // Округляем позиции детей
        foreach (UIElement child in InternalChildren)
        {
            double left = Math.Ceiling(GetLeft(child));
            double top = Math.Ceiling(GetTop(child));
            
            child.Arrange(new Rect(
                left, top, 
                Math.Ceiling(child.DesiredSize.Width),
                Math.Ceiling(child.DesiredSize.Height)
            ));
        }
        
        return arrangeSize;
    }
}
```

#### Пример 4: Отладка layout

```csharp
public class DebugPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Measure: Available={availableSize}");
        
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            System.Diagnostics.Debug.WriteLine(
            $"  Child DesiredSize={child.DesiredSize}");
        }
        
        return base.MeasureOverride(availableSize);
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Arrange: Final={finalSize}");
        
        foreach (UIElement child in InternalChildren)
        {
            System.Diagnostics.Debug.WriteLine(
            $"  Child RenderSize={child.RenderSize}");
        }
        
        return base.ArrangeOverride(finalSize);
    }
}
```

#### Пример 5: LayoutInvalidation

```csharp
public class InvalidatePanel : Panel
{
    // Attached Property для принудительной invalidation
    public static readonly DependencyProperty InvalidateProperty =
        DependencyProperty.RegisterAttached(
            "Invalidate",
            typeof(bool),
            typeof(InvalidatePanel),
            new PropertyMetadata(false, OnInvalidateChanged)
        );

    private static void OnInvalidateChanged(
        DependencyObject d, 
        DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel && (bool)e.NewValue)
        {
            // Принудительно запускаем layout
            panel.InvalidateMeasure();
            panel.InvalidateArrange();
        }
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 3.1.1: SimpleStackPanel**

Создайте панель, располагающую элементы вертикально:
- Measure: суммирует высоты, берёт максимальную ширину
- Arrange: располагает элементы друг под другом

**Задача 3.1.2: Debugging Panel**

Добавьте в простую панель вывод в Output Window:
- DesiredSize каждого элемента после Measure
- RenderSize каждого элемента после Arrange
- FinalSize панели

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 3.1.3: UniformStackPanel**

Создайте панель с равномерным распределением:
- Все элементы одинаковой ширины/высоты
- Measure: делит availableSize на количество элементов
- Arrange: располагает с равными интервалами

**Задача 3.1.4: LayoutBenchmark**

Создайте панель для замера производительности:
- Замер времени Measure и Arrange
- Вывод статистики в Debug Window
- Счётчик вызовов layout

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 3.1.5: VirtualizingPanel**

Реализуйте базовую виртуализацию:
- Measure только видимых элементов
- Arrange только видимых элементов
- ScrollViewer integration через IScrollInfo

**Задача 3.1.6: LayoutTransformer**

Создайте панель с трансформацией:
- RotateTransform для детей
- Measure с учётом трансформации
- Arrange с компенсацией трансформации

---

### Решения

<details>
<summary>✅ Решение задачи 3.1.1</summary>

```csharp
public class SimpleStackPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double totalHeight = 0;
        double maxWidth = 0;
        
        foreach (UIElement child in InternalChildren)
        {
            // Измеряем с неограниченной высотой
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            
            totalHeight += child.DesiredSize.Height;
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
        }
        
        return new Size(maxWidth, totalHeight);
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        double yOffset = 0;
        
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(new Rect(
                0, 
                yOffset, 
                finalSize.Width, 
                child.DesiredSize.Height
            ));
            
            yOffset += child.DesiredSize.Height;
        }
        
        return finalSize;
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 3.1.3</summary>

```csharp
public class UniformStackPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count == 0)
            return new Size(0, 0);
        
        // Делим доступное место поровну
        double childWidth = availableSize.Width / InternalChildren.Count;
        double childHeight = availableSize.Height;
        
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(childWidth, childHeight));
        }
        
        return availableSize;
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0)
            return finalSize;
        
        double childWidth = finalSize.Width / InternalChildren.Count;
        double xOffset = 0;
        
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(new Rect(
                xOffset, 
                0, 
                childWidth, 
                finalSize.Height
            ));
            
            xOffset += childWidth;
        }
        
        return finalSize;
    }
}
```
</details>

---

## Ключевые выводы

✅ **Measure Pass** — элементы сообщают желаемый размер  
✅ **Arrange Pass** — элементы располагаются в финальной области  
✅ **DesiredSize** — после Measure, **RenderSize** — после Arrange  
✅ **HorizontalAlignment/VerticalAlignment** влияют на layout  
✅ **InvalidateMeasure()** запускает layout заново  
✅ **Custom Panel** — переопределите MeasureOverride и ArrangeOverride  
✅ **Pixel snapping** — округляйте координаты для чёткой графики

---

## Дополнительные ресурсы

- [Layout System](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/layout)
- [Custom Panels](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/authoring-overview-custom-control)
- [Layout Rounding](https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.layoutrounding)
