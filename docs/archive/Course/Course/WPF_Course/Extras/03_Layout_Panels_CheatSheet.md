# Шпаргалка по Layout-панелям

## Сравнение панелей

| Панель | Описание | Использование |
|--------|----------|---------------|
| **Grid** | Строки и колонки | Сложные макеты, формы, главное окно |
| **StackPanel** | Элементы в стопку | Списки кнопок, вертикальные/горизонтальные группы |
| **WrapPanel** | Элементы с переносом | Теги, плитки, адаптивные списки |
| **DockPanel** | Прикрепление к краям | ToolBar, StatusBar, классический MDI |
| **Canvas** | Абсолютное позиционирование | Графика, диаграммы, CAD |
| **UniformGrid** | Равные ячейки | Калькулятор, плитка одинакового размера |

---

## Grid

### Row/Column Definitions

```xml
<Grid>
    <Grid.RowDefinitions>
        <!-- Auto: по содержимому -->
        <RowDefinition Height="Auto"/>
        
        <!-- Фиксированный размер -->
        <RowDefinition Height="100"/>
        
        <!-- Star: пропорционально -->
        <RowDefinition Height="*"/>
        <RowDefinition Height="2*"/>
    </Grid.RowDefinitions>
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
</Grid>
```

### Grid Attached Properties

```xml
<Button Grid.Row="0" Grid.Column="1"
        Grid.RowSpan="2" Grid.ColumnSpan="3"/>
```

### GridSplitter

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <Border Grid.Column="0" Background="LightBlue"/>
    
    <!-- Вертикальный разделитель -->
    <GridSplitter Grid.Column="1" Width="5" 
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"/>
    
    <Border Grid.Column="2" Background="White"/>
</Grid>

<!-- Горизонтальный разделитель -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="200"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <GridSplitter Grid.Row="1" Height="5"
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Center"/>
</Grid>
```

### SharedSizeGroup

```xml
<Grid Grid.IsSharedSizeScope="True">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Все колонки с SharedSizeGroup="Label" будут одинаковой ширины -->
</Grid>
```

---

## StackPanel

### Orientation

```xml
<!-- Вертикально (по умолчанию) -->
<StackPanel>
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
</StackPanel>

<!-- Горизонтально -->
<StackPanel Orientation="Horizontal">
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
</StackPanel>
```

### FlowDirection

```xml
<StackPanel FlowDirection="RightToLeft">
    <Button Content="1"/>
    <Button Content="2"/>
</StackPanel>
```

---

## WrapPanel

### Orientation

```xml
<!-- Горизонтально с переносом -->
<WrapPanel>
    <Button Content="Button 1"/>
    <Button Content="Button 2"/>
    <Button Content="Button 3"/>
    <!-- Переносится на новую строку -->
</WrapPanel>

<!-- Вертикально с переносом -->
<WrapPanel Orientation="Vertical">
    <Button Content="1"/>
    <Button Content="2"/>
    <!-- Переносится в новую колонку -->
</WrapPanel>
```

### ItemWidth/ItemHeight

```xml
<WrapPanel ItemWidth="100" ItemHeight="50">
    <Button Content="1"/>
    <Button Content="2"/>
</WrapPanel>
```

---

## DockPanel

### Dock

```xml
<DockPanel>
    <Button Content="Top" DockPanel.Dock="Top"/>
    <Button Content="Bottom" DockPanel.Dock="Bottom"/>
    <Button Content="Left" DockPanel.Dock="Left"/>
    <Button Content="Right" DockPanel.Dock="Right"/>
    <Button Content="Fill (Last, автоматически)"/>
</DockPanel>
```

### LastChildFill

```xml
<!-- Последний элемент заполняет剩余 пространство (по умолчанию True) -->
<DockPanel LastChildFill="True">
    <Button Content="Left" DockPanel.Dock="Left"/>
    <Button Content="Fill"/>
</DockPanel>

<!-- Все элементы dock, последний не заполняет -->
<DockPanel LastChildFill="False">
    <Button Content="Left" DockPanel.Dock="Left"/>
    <Button Content="Right" DockPanel.Dock="Right"/>
</DockPanel>
```

---

## Canvas

### Positioning

```xml
<Canvas Width="400" Height="300">
    <Button Content="Click" Canvas.Left="50" Canvas.Top="30"/>
    <Rectangle Canvas.Left="100" Canvas.Top="80"
               Width="100" Height="60" Fill="Blue"/>
    <Ellipse Canvas.Left="250" Canvas.Top="150"
             Width="80" Height="80" Fill="Red"/>
</Canvas>
```

### Z-Index

```xml
<Canvas>
    <Rectangle Canvas.ZIndex="1" .../>
    <Rectangle Canvas.ZIndex="2" .../> <!-- Поверх -->
    <Rectangle Canvas.ZIndex="0" .../>
</Canvas>
```

---

## UniformGrid

```xml
<!-- 3x3 сетка -->
<UniformGrid Rows="3" Columns="3">
    <Button Content="1"/>
    <Button Content="2"/>
    <!-- ... 9 кнопок -->
</UniformGrid>

<!-- Автоматическое определение -->
<UniformGrid>
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
    <Button Content="4"/>
    <!-- 2x2 автоматически -->
</UniformGrid>
```

---

## Custom Panel: Measure/Arrange

```csharp
public class CustomPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        // Измеряем каждый элемент
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
        }
        
        // Возвращаем желаемый размер
        return new Size(100, 100);
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Располагаем элементы
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }
        
        return finalSize;
    }
}
```

---

## Attached Property для Custom Panel

```csharp
public class FlexPanel : Panel
{
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
}
```

```xml
<!-- Использование -->
<local:FlexPanel>
    <Button Content="Flex=1" local:FlexPanel.Flex="1"/>
    <Button Content="Flex=2" local:FlexPanel.Flex="2"/>
</local:FlexPanel>
```

---

## Выбор панели

| Требование | Панель |
|------------|--------|
| Сложный макет с строками/колонками | **Grid** |
| Простая стопка элементов | **StackPanel** |
| Элементы с переносом | **WrapPanel** |
| Прикрепление к краям | **DockPanel** |
| Точное позиционирование | **Canvas** |
| Равные ячейки | **UniformGrid** |
| Кастомная логика | **Custom Panel** |

---

## Performance Tips

✅ **Grid:** Минимизируйте вложенность  
✅ **Grid:** Используйте `SharedSizeGroup` вместо вложенных Grid  
✅ **VirtualizingPanel:** Для больших списков  
✅ **Canvas:** Не используйте для сложного UI (нет virtualization)  
✅ **Measure/Arrange:** Кэшируйте результаты  

❌ **Grid:** Избегайте глубокой вложенности  
❌ **StackPanel:** Не используйте для прокручиваемых списков (нет virtualization)  
❌ **Canvas:** Не для динамического контента  

---

## Common Patterns

### Главное окно

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>  <!-- Menu -->
        <RowDefinition Height="Auto"/>  <!-- ToolBar -->
        <RowDefinition Height="*"/>     <!-- Content -->
        <RowDefinition Height="Auto"/>  <!-- StatusBar -->
    </Grid.RowDefinitions>
</Grid>
```

### Форма с labels

```xml
<Grid Grid.IsSharedSizeScope="True">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Name:"/>
    <TextBox Grid.Row="0" Grid.Column="1"/>
</Grid>
```

### Resizable панели

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150" MaxWidth="400"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="250" MinWidth="200" MaxWidth="500"/>
    </Grid.ColumnDefinitions>
    
    <Border Grid.Column="0"/>
    <GridSplitter Grid.Column="1" Width="5"/>
    <Border Grid.Column="2"/>
    <GridSplitter Grid.Column="3" Width="5"/>
    <Border Grid.Column="4"/>
</Grid>
```
