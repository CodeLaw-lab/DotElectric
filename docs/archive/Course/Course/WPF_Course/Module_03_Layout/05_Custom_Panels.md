# Тема 3.5: Custom Panels (создание собственных панелей)

### Теория

**Custom Panel** — панель с кастомной логикой layout через переопределение `MeasureOverride` и `ArrangeOverride`.

#### Когда создавать Custom Panel?

✅ **Создавайте когда:**
- Стандартные панели не подходят
- Нужна特殊ная логика размещения
- Требуется оптимизация производительности
- Виртуализация элементов

❌ **Не создавайте когда:**
- Можно обойтись Grid/StackPanel
- Логика простая (используйте Attached Properties)
- Это разовый случай (используйте ControlTemplate)

#### Базовый класс Panel

```csharp
public abstract class Panel : FrameworkElement
{
    // Коллекция дочерних элементов
    public UIElementCollection Children { get; }
    protected UIElementCollection InternalChildren { get; }
    
    // Переопределяемые методы
    protected virtual Size MeasureOverride(Size availableSize);
    protected virtual Size ArrangeOverride(Size finalSize);
    
    // Invalidate methods
    public void InvalidateMeasure();
    public void InvalidateArrange();
}
```

### Примеры кода

#### Пример 1: SimpleWrapPanel — упрощённая WrapPanel

```csharp
public class SimpleWrapPanel : Panel
{
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(SimpleWrapPanel),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size curLineSize = new Size(0, 0);
        Size panelSize = new Size(0, 0);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            Size childDesiredSize = child.DesiredSize;

            if (Orientation == Orientation.Horizontal)
            {
                // Горизонтальная ориентация
                if (curLineSize.Width + childDesiredSize.Width > availableSize.Width)
                {
                    // Перенос на новую строку
                    panelSize.Width = Math.Max(panelSize.Width, curLineSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = new Size(childDesiredSize.Width, childDesiredSize.Height);
                }
                else
                {
                    curLineSize.Width += childDesiredSize.Width;
                    curLineSize.Height = Math.Max(curLineSize.Height, childDesiredSize.Height);
                }
            }
            else
            {
                // Вертикальная ориентация
                if (curLineSize.Height + childDesiredSize.Height > availableSize.Height)
                {
                    panelSize.Height = Math.Max(panelSize.Height, curLineSize.Height);
                    panelSize.Width += curLineSize.Width;
                    curLineSize = new Size(childDesiredSize.Width, childDesiredSize.Height);
                }
                else
                {
                    curLineSize.Height += childDesiredSize.Height;
                    curLineSize.Width = Math.Max(curLineSize.Width, childDesiredSize.Width);
                }
            }
        }

        // Добавляем последнюю строку
        panelSize.Width = Math.Max(panelSize.Width, curLineSize.Width);
        panelSize.Height += curLineSize.Height;

        return panelSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Rect rect = new Rect(0, 0, 0, 0);

        if (Orientation == Orientation.Horizontal)
        {
            double maxWidth = finalSize.Width;
            double rowHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (rect.X + child.DesiredSize.Width > maxWidth)
                {
                    rect.X = 0;
                    rect.Y += rowHeight;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(
                    rect.X, 
                    rect.Y, 
                    child.DesiredSize.Width, 
                    child.DesiredSize.Height
                ));

                rect.X += child.DesiredSize.Width;
                rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            }
        }
        else
        {
            // Вертикальная ориентация (аналогично)
        }

        return finalSize;
    }
}
```

```xml
<!-- Использование -->
<local:SimpleWrapPanel Orientation="Horizontal">
    <Button Content="Button 1"/>
    <Button Content="Button 2"/>
    <Button Content="Button 3"/>
</local:SimpleWrapPanel>
```

#### Пример 2: FlexPanel — аналог CSS Flexbox

```csharp
public class FlexPanel : Panel
{
    #region Attached Properties

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

    public static double GetFlex(UIElement element) => 
        (double)element.GetValue(FlexProperty);
    
    public static void SetFlex(UIElement element, double value) => 
        element.SetValue(FlexProperty, value);

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalFlex = 0;
        Size remainingSize = availableSize;

        // Считаем общий flex
        foreach (UIElement child in InternalChildren)
        {
            totalFlex += GetFlex(child);
        }

        // Измеряем элементы
        foreach (UIElement child in InternalChildren)
        {
            double flex = GetFlex(child);
            
            if (flex > 0 && totalFlex > 0)
            {
                // Элемент с flex — выделяем пропорциональное место
                double childWidth = (flex / totalFlex) * availableSize.Width;
                child.Measure(new Size(childWidth, availableSize.Height));
            }
            else
            {
                // Элемент без flex — измеряем как обычно
                child.Measure(availableSize);
            }
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double xOffset = 0;
        double totalFlex = 0;

        foreach (UIElement child in InternalChildren)
        {
            totalFlex += GetFlex(child);
        }

        foreach (UIElement child in InternalChildren)
        {
            double flex = GetFlex(child);
            double width;

            if (flex > 0 && totalFlex > 0)
            {
                // Пропорциональная ширина
                width = (flex / totalFlex) * finalSize.Width;
            }
            else
            {
                // Желаемая ширина
                width = child.DesiredSize.Width;
            }

            child.Arrange(new Rect(xOffset, 0, width, finalSize.Height));
            xOffset += width;
        }

        return finalSize;
    }
}
```

```xml
<!-- Использование -->
<local:FlexPanel>
    <Button Content="Fixed" Width="100"/>
    <Button Content="Flex=1" local:FlexPanel.Flex="1"/>
    <Button Content="Flex=2" local:FlexPanel.Flex="2"/>
    <!-- Кнопка с Flex=2 будет в 2 раза шире чем Flex=1 -->
</local:FlexPanel>
```

#### Пример 3: UniformGridPanel — сетка с равными ячейками

```csharp
public class UniformGridPanel : Panel
{
    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(
            "Rows",
            typeof(int),
            typeof(UniformGridPanel),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            "Columns",
            typeof(int),
            typeof(UniformGridPanel),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int rows = Rows > 0 ? Rows : (int)Math.Ceiling(Math.Sqrt(InternalChildren.Count));
        int cols = Columns > 0 ? Columns : (int)Math.Ceiling(Math.Sqrt(InternalChildren.Count));

        double itemWidth = availableSize.Width / cols;
        double itemHeight = availableSize.Height / rows;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(itemWidth, itemHeight));
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int rows = Rows > 0 ? Rows : (int)Math.Ceiling(Math.Sqrt(InternalChildren.Count));
        int cols = Columns > 0 ? Columns : (int)Math.Ceiling(Math.Sqrt(InternalChildren.Count));

        double itemWidth = finalSize.Width / cols;
        double itemHeight = finalSize.Height / rows;

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (index >= InternalChildren.Count)
                    break;

                UIElement child = InternalChildren[index];
                double x = col * itemWidth;
                double y = row * itemHeight;

                child.Arrange(new Rect(x, y, itemWidth, itemHeight));
                index++;
            }
        }

        return finalSize;
    }
}
```

```xml
<!-- Использование -->
<local:UniformGridPanel Rows="3" Columns="3">
    <Button Content="1"/>
    <Button Content="2"/>
    <!-- ... 9 кнопок -->
</local:UniformGridPanel>
```

#### Пример 4: TilePanel — панель с плитками

```csharp
public class TilePanel : Panel
{
    public static readonly DependencyProperty TileWidthProperty =
        DependencyProperty.Register(
            "TileWidth",
            typeof(double),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

    public static readonly DependencyProperty TileHeightProperty =
        DependencyProperty.Register(
            "TileHeight",
            typeof(double),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

    public double TileWidth
    {
        get => (double)GetValue(TileWidthProperty);
        set => SetValue(TileWidthProperty, value);
    }

    public double TileHeight
    {
        get => (double)GetValue(TileHeightProperty);
        set => SetValue(TileHeightProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size tileSize = new Size(TileWidth, TileHeight);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(tileSize);
        }

        // Вычисляем общий размер
        int count = InternalChildren.Count;
        int columns = (int)Math.Ceiling(availableSize.Width / TileWidth);
        int rows = (int)Math.Ceiling((double)count / columns);

        return new Size(
            Math.Min(availableSize.Width, columns * TileWidth),
            Math.Min(availableSize.Height, rows * TileHeight)
        );
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int columns = (int)Math.Ceiling(finalSize.Width / TileWidth);
        int index = 0;

        foreach (UIElement child in InternalChildren)
        {
            int row = index / columns;
            int col = index % columns;

            double x = col * TileWidth;
            double y = row * TileHeight;

            child.Arrange(new Rect(x, y, TileWidth, TileHeight));
            index++;
        }

        return finalSize;
    }
}
```

#### Пример 5: Custom Panel с Attached Properties

```csharp
public class ResponsivePanel : Panel
{
    #region Attached Properties

    public static readonly DependencyProperty MinWidthProperty =
        DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(ResponsivePanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure
            )
        );

    public static readonly DependencyProperty CollapseBelowProperty =
        DependencyProperty.RegisterAttached(
            "CollapseBelow",
            typeof(double),
            typeof(ResponsivePanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure
            )
        );

    public static double GetMinWidth(UIElement element) => 
        (double)element.GetValue(MinWidthProperty);
    
    public static void SetMinWidth(UIElement element, double value) => 
        element.SetValue(MinWidthProperty, value);

    public static double GetCollapseBelow(UIElement element) => 
        (double)element.GetValue(CollapseBelowProperty);
    
    public static void SetCollapseBelow(UIElement element, double value) => 
        element.SetValue(CollapseBelowProperty, value);

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        Size desiredSize = new Size(0, 0);
        double currentWidth = 0;
        double currentRowHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            // Проверяем CollapseBelow
            double collapseBelow = GetCollapseBelow(child);
            if (collapseBelow > 0 && availableSize.Width < collapseBelow)
            {
                child.Measure(new Size(0, 0));
                continue;
            }

            // Измеряем элемент
            child.Measure(availableSize);

            // Проверяем перенос строки
            if (currentWidth + child.DesiredSize.Width > availableSize.Width)
            {
                desiredSize.Width = Math.Max(desiredSize.Width, currentWidth);
                desiredSize.Height += currentRowHeight;
                currentWidth = 0;
                currentRowHeight = 0;
            }

            currentWidth += child.DesiredSize.Width;
            currentRowHeight = Math.Max(currentRowHeight, child.DesiredSize.Height);
        }

        // Добавляем последнюю строку
        desiredSize.Width = Math.Max(desiredSize.Width, currentWidth);
        desiredSize.Height += currentRowHeight;

        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double xOffset = 0;
        double yOffset = 0;
        double rowHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            double collapseBelow = GetCollapseBelow(child);
            if (collapseBelow > 0 && finalSize.Width < collapseBelow)
            {
                child.Arrange(new Rect(0, 0, 0, 0));
                continue;
            }

            // Перенос строки
            if (xOffset + child.DesiredSize.Width > finalSize.Width)
            {
                xOffset = 0;
                yOffset += rowHeight;
                rowHeight = 0;
            }

            child.Arrange(new Rect(
                xOffset, 
                yOffset, 
                child.DesiredSize.Width, 
                child.DesiredSize.Height
            ));

            xOffset += child.DesiredSize.Width;
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
        }

        return finalSize;
    }
}
```

```xml
<!-- Использование -->
<local:ResponsivePanel>
    <Button Content="Always Visible"/>
    <Button Content="Collapse on Small" 
            local:ResponsivePanel.CollapseBelow="600"/>
    <Button Content="Also Collapses" 
            local:ResponsivePanel.CollapseBelow="600"/>
</local:ResponsivePanel>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 3.5.1: HorizontalStackPanel**

Создайте панель, располагающую элементы только горизонтально:
- Measure: суммирует ширины, берёт максимальную высоту
- Arrange: располагает элементы слева направо

**Задача 3.5.2: CenterPanel**

Создайте панель, центрирующую все элементы:
- Measure: возвращает размер наибольшего элемента
- Arrange: центрирует все элементы

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 3.5.3: GridLayoutPanel**

Создайте панель с сеткой:
- Rows и Columns свойства
- Равномерное распределение элементов
- Поддержка пустых ячеек

**Задача 3.5.4: SpacedPanel**

Создайте панель с равными промежутками:
- SpaceBetween — равные промежутки между элементами
- SpaceAround — равные отступы вокруг элементов
- Orientation (Horizontal/Vertical)

---

#### 🔴 Продвинутый уровень (3 часа)

**Задача 3.5.5: MasonryPanel**

Создайте панель в стиле Pinterest:
- Элементы разной высоты
- Заполнение пустот (как кирпичная кладка)
- Минимальное количество пустого пространства

**Задача 3.5.6: VirtualizingCustomPanel**

Реализуйте панель с виртуализацией:
- IScrollInfo интерфейс
- Measure/Arrange только видимых элементов
- ScrollOwner integration

---

### Решения

<details>
<summary>✅ Решение задачи 3.5.1</summary>

```csharp
public class HorizontalStackPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        double maxHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            totalWidth += child.DesiredSize.Width;
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        return new Size(totalWidth, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double xOffset = 0;

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
</details>

<details>
<summary>✅ Решение задачи 3.5.2</summary>

```csharp
public class CenterPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Size desiredSize = new Size(0, 0);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            desiredSize.Width = Math.Max(desiredSize.Width, child.DesiredSize.Width);
            desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
        }

        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            double x = (finalSize.Width - child.DesiredSize.Width) / 2;
            double y = (finalSize.Height - child.DesiredSize.Height) / 2;

            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
        }

        return finalSize;
    }
}
```
</details>

---

## Ключевые выводы

✅ **Custom Panel** — переопределите MeasureOverride и ArrangeOverride  
✅ **InternalChildren** — коллекция дочерних элементов  
✅ **Attached Properties** — для настройки поведения элементов  
✅ **FrameworkPropertyMetadataOptions.AffectsMeasure** — для пересчёта layout  
✅ **InvalidateMeasure()** — принудительный запуск layout  
✅ **Виртуализация** — для больших коллекций элементов  
✅ **Производительность** — кэшируйте вычисления в Measure/Arrange

---

## Дополнительные ресурсы

- [Panel Class](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.panel)
- [Custom Panels](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/authoring-overview-custom-control)
- [Layout System](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/layout)
