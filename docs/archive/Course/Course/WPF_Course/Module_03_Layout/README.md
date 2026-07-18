# Модуль 3: Layout-контейнеры

**Время прохождения:** 12 часов  
**Уровень:** Средний

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте систему измерения и расположения (Measure/Arrange)
- ✅ Научитесь использовать Grid (Row/Column, GridSplitter, Star Sizing)
- ✅ Освоите StackPanel, WrapPanel, DockPanel
- ✅ Сможете работать с Canvas для абсолютного позиционирования
- ✅ Создадите кастомную панель (Custom Panel)

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 3.1 | [Система Layout (Measure/Arrange)](./01_Layout_System.md) | 2 часа | Теория, примеры, 6 задач |
| 3.2 | [Grid (Row/Column, GridSplitter, Star Sizing)](./02_Grid.md) | 3 часа | Теория, примеры, 6 задач |
| 3.3 | [StackPanel, WrapPanel, DockPanel](./03_Panels.md) | 2 часа | Теория, примеры, 6 задач |
| 3.4 | [Canvas, ScrollViewer, ViewBox](./04_Canvas_ScrollViewer.md) | 2 часа | Теория, примеры, 6 задач |
| 3.5 | [Custom Panels](./05_Custom_Panels.md) | 2 часа | Теория, примеры, 6 задач |
| 3.6 | [Практическая работа](./06_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модуль 1 (архитектура WPF, DP, Routed Events)
- [ ] Прошли Модуль 2 (XAML, ресурсы, стили)
- [ ] Понимаете, что такое Dependency Properties
- [ ] Создавали простые WPF-приложения

---

## Краткое содержание тем

### Тема 3.1: Система Layout (Measure/Arrange)

**Изучите:**
- Два прохода layout-системы: Measure и Arrange
- DesiredSize, RenderSize, ActualWidth/ActualHeight
- Влияние Alignment на layout
- Layout rounding и pixel snapping

**Пример:**
```csharp
public class CustomPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        // Первый проход: каждый элемент измеряется
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
        }
        return new Size(100, 100); // Возвращаем желаемый размер
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Второй проход: элементы располагаются
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }
        return finalSize;
    }
}
```

---

### Тема 3.2: Grid

**Изучите:**
- RowDefinitions и ColumnDefinitions
- GridUnitType: Auto, Pixel (число), Star (*)
- GridSplitter для изменения размеров
- SharedSizeGroup для синхронизации размеров
- Вложенные Grid

**Пример:**
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>     <!-- По содержимому -->
        <RowDefinition Height="100"/>      <!-- Фиксировано 100px -->
        <RowDefinition Height="*"/>        <!-- Всё оставшееся место -->
        <RowDefinition Height="2*"/>       <!-- В 2 раза больше чем * -->
    </Grid.RowDefinitions>
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <!-- Элементы с Grid.Row и Grid.Column -->
    <Button Grid.Row="0" Grid.Column="0" Content="OK"/>
    <Button Grid.Row="0" Grid.Column="1" Content="Cancel"/>
    
    <!-- GridSplitter -->
    <GridSplitter Grid.Row="1" Grid.Column="0" 
                  Width="5" HorizontalAlignment="Right"/>
</Grid>
```

---

### Тема 3.3: StackPanel, WrapPanel, DockPanel

**StackPanel:**
```xml
<!-- Вертикальный -->
<StackPanel>
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
</StackPanel>

<!-- Горизонтальный -->
<StackPanel Orientation="Horizontal">
    <Button Content="1"/>
    <Button Content="2"/>
</StackPanel>
```

**WrapPanel:**
```xml
<WrapPanel>
    <!-- Элементы переносятся на новую строку -->
    <Button Content="Button 1"/>
    <Button Content="Button 2"/>
    <Button Content="Button 3"/>
</WrapPanel>
```

**DockPanel:**
```xml
<DockPanel>
    <Button Content="Top" DockPanel.Dock="Top"/>
    <Button Content="Bottom" DockPanel.Dock="Bottom"/>
    <Button Content="Left" DockPanel.Dock="Left"/>
    <Button Content="Right" DockPanel.Dock="Right"/>
    <Button Content="Fill (Last)"/> <!-- LastFill=true -->
</DockPanel>
```

---

### Тема 3.4: Canvas и абсолютное позиционирование

**Canvas:**
```xml
<Canvas Width="400" Height="300">
    <Button Content="Click" Canvas.Left="50" Canvas.Top="30"/>
    <Rectangle Canvas.Left="100" Canvas.Top="80" 
               Width="100" Height="60" Fill="Blue"/>
    <Ellipse Canvas.Left="250" Canvas.Top="150" 
             Width="80" Height="80" Fill="Red"/>
</Canvas>
```

**Z-index:**
```xml
<Canvas>
    <Rectangle Canvas.ZIndex="1" .../>
    <Rectangle Canvas.ZIndex="2" .../> <!-- Поверх -->
</Canvas>
```

---

### Тема 3.5: Custom Panels

**FlexPanel (аналог CSS Flexbox):**
```csharp
public class FlexPanel : Panel
{
    public static readonly DependencyProperty FlexProperty =
        DependencyProperty.RegisterAttached(
            "Flex", typeof(double), typeof(FlexPanel),
            new FrameworkPropertyMetadata(0.0, 
                FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    // Getter/Setter...

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
            if (flex > 0)
            {
                child.Measure(new Size(remainingSize.Width * flex / totalFlex, 
                                       availableSize.Height));
            }
            else
            {
                child.Measure(availableSize);
            }
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double xOffset = 0;
        
        foreach (UIElement child in InternalChildren)
        {
            double flex = GetFlex(child);
            double width = flex > 0 
                ? finalSize.Width * flex / GetTotalFlex() 
                : child.DesiredSize.Width;

            child.Arrange(new Rect(xOffset, 0, width, finalSize.Height));
            xOffset += width;
        }

        return finalSize;
    }
}
```

---

## Практическая работа

**Задание:** Создание главного окна приложения

**Время:** 4 часа

**Требования:**
1. Главное окно с Grid (3 строки, 3 колонки)
2. Menu в первой строке
3. ToolBar во второй строке
4. Левая панель (TreeView) с GridSplitter
5. Центральная область (TabControl)
6. Правая панель (Properties) с GridSplitter
7. StatusBar в последней строке
8. Поддержка изменения размеров окна

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 4 убедитесь, что вы:

- [ ] Понимаете Measure/Arrange проходы
- [ ] Использовали Grid с Auto/Pixel/Star sizing
- [ ] Применяли GridSplitter для resizable панелей
- [ ] Создавали layout с StackPanel и WrapPanel
- [ ] Использовали Canvas для абсолютного позиционирования
- [ ] Написали кастомную панель с Attached Properties
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Measure Pass** | Первый проход: элементы сообщают желаемый размер |
| **Arrange Pass** | Второй проход: элементы располагаются в финальной области |
| **Star Sizing** | Распределение места пропорционально (*, 2*, 3*) |
| **GridSplitter** | Контрол для изменения размеров колонок/строк |
| **Attached Property** | Свойство, определённое одним классом, используемое на другом |
| **Z-Index** | Порядок наложения элементов (в Canvas) |
| **Custom Panel** | Пользовательская панель с кастомной логикой layout |

---

## Переход к следующему модулю

➡️ **[Модуль 4: Data Binding](../Module_04_DataBinding/README.md)**

В Модуле 4 изучим:
- Основы binding: Source, Path, Mode, UpdateSourceTrigger
- Конвертеры (IValueConverter, IMultiValueConverter)
- Binding к коллекциям (ObservableCollection)
- Validation (IDataErrorInfo, INotifyDataErrorInfo)
- RelativeSource, ElementName, FindAncestor
