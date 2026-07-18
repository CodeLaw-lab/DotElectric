# Тема 3.4: Canvas, ScrollViewer, ViewBox

### Теория

#### Canvas

**Назначение:** Абсолютное позиционирование элементов.

- Координаты через `Canvas.Left`, `Canvas.Top`
- Порядок наложения через `Canvas.ZIndex`
- Не обрезает детей (ClipToBounds=False по умолчанию)
- Используется для графики, диаграмм, CAD

```
Canvas (400x300):
(0,0) ┌────────────────────────┐
      │  ● (50,30)              │
      │     ┌──────┐            │
      │     │      │ (100,80)  │
      │     └──────┘            │
      │                    ●    │
      │                  (350,  │
      │                   250)  │
      └────────────────────────┘
```

#### ScrollViewer

**Назначение:** Прокрутка содержимого.

- HorizontalScrollBarVisibility
- VerticalScrollBarVisibility
- ExtentWidth/Height, ViewportWidth/Height
- ScrollToHorizontalOffset, ScrollToVerticalOffset

#### ViewBox

**Назначение:** Масштабирование содержимого.

- Stretch: Uniform, UniformToFill, Fill, None
- Масштабирует один элемент-потомок
- Используется для иконок, масштабируемого UI

### Примеры кода

#### Пример 1: Canvas — базовое позиционирование

```xml
<Canvas Width="400" Height="300" Background="White">
    <!-- Кнопка по координатам -->
    <Button Content="Click" 
            Canvas.Left="50" 
            Canvas.Top="30"/>
    
    <!-- Прямоугольник -->
    <Rectangle Canvas.Left="100" 
               Canvas.Top="80"
               Width="100" Height="60" 
               Fill="Blue" 
               Stroke="Black" 
               StrokeThickness="2"/>
    
    <!-- Эллипс -->
    <Ellipse Canvas.Left="250" 
             Canvas.Top="150"
             Width="80" Height="80" 
             Fill="Red" 
             Stroke="Black" 
             StrokeThickness="2"/>
    
    <!-- Линия -->
    <Line X1="50" Y1="50" X2="200" Y2="150"
          Stroke="Green" StrokeThickness="3"/>
</Canvas>
```

#### Пример 2: Canvas — Z-Index

```xml
<Canvas Width="300" Height="200">
    <!-- Нижний слой -->
    <Rectangle Canvas.Left="50" Canvas.Top="50"
               Width="100" Height="100"
               Fill="Blue" Canvas.ZIndex="0"/>
    
    <!-- Средний слой -->
    <Rectangle Canvas.Left="80" Canvas.Top="80"
               Width="100" Height="100"
               Fill="Green" Canvas.ZIndex="1"/>
    
    <!-- Верхний слой -->
    <Rectangle Canvas.Left="110" Canvas.Top="110"
               Width="100" Height="100"
               Fill="Red" Canvas.ZIndex="2"/>
</Canvas>
```

#### Пример 3: Canvas — ClipToBounds

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- ClipToBounds=False (по умолчанию) -->
    <Canvas Grid.Column="0" 
            Width="200" Height="150" 
            Background="LightBlue">
        <Rectangle Canvas.Left="150" Canvas.Top="50"
                   Width="100" Height="80" Fill="Red"/>
        <!-- Прямоугольник виден за пределами Canvas -->
    </Canvas>
    
    <!-- ClipToBounds=True -->
    <Canvas Grid.Column="1" 
            Width="200" Height="150" 
            Background="LightGreen"
            ClipToBounds="True">
        <Rectangle Canvas.Left="150" Canvas.Top="50"
                   Width="100" Height="80" Fill="Red"/>
        <!-- Прямоугольник обрезается -->
    </Canvas>
</Grid>
```

#### Пример 4: ScrollViewer — базовая прокрутка

```xml
<ScrollViewer Width="300" Height="200"
              HorizontalScrollBarVisibility="Auto"
              VerticalScrollBarVisibility="Auto">
    <StackPanel Width="500" Height="400">
        <!-- Контент больше ScrollViewer -->
        <TextBlock Text="Line 1"/>
        <TextBlock Text="Line 2"/>
        <TextBlock Text="Line 3"/>
        <!-- ... 20 строк -->
    </StackPanel>
</ScrollViewer>
```

#### Пример 5: ScrollViewer — программа прокрутка

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <!-- Кнопки прокрутки -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="5">
        <Button Content="Top" Click="ScrollTop_Click"/>
        <Button Content="Up" Click="ScrollUp_Click" Margin="5,0"/>
        <Button Content="Down" Click="ScrollDown_Click"/>
        <Button Content="Bottom" Click="ScrollBottom_Click" Margin="5,0"/>
    </StackPanel>
    
    <!-- ScrollViewer -->
    <ScrollViewer x:Name="scrollViewer" 
                  Grid.Row="1"
                  VerticalScrollBarVisibility="Auto">
        <StackPanel>
            <!-- Много контента -->
            <TextBlock Text="Line 1"/>
            <TextBlock Text="Line 2"/>
            <!-- ... 100 строк -->
        </StackPanel>
    </ScrollViewer>
</Grid>
```

```csharp
private void ScrollTop_Click(object sender, RoutedEventArgs e)
{
    scrollViewer.ScrollToTop();
}

private void ScrollUp_Click(object sender, RoutedEventArgs e)
{
    scrollViewer.LineUp();
}

private void ScrollDown_Click(object sender, RoutedEventArgs e)
{
    scrollViewer.LineDown();
}

private void ScrollBottom_Click(object sender, RoutedEventArgs e)
{
    scrollViewer.ScrollToBottom();
}
```

#### Пример 6: ViewBox — масштабирование

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Uniform (по умолчанию) -->
    <Viewbox Grid.Column="0" Width="100" Height="100"
             Stretch="Uniform">
        <Rectangle Width="200" Height="100" Fill="Blue"/>
    </Viewbox>
    
    <!-- Fill -- растягивает -->
    <Viewbox Grid.Column="1" Width="100" Height="100"
             Stretch="Fill">
        <Rectangle Width="200" Height="100" Fill="Green"/>
    </Viewbox>
    
    <!-- UniformToFill -- заполняет с обрезкой -->
    <Viewbox Grid.Column="2" Width="100" Height="100"
             Stretch="UniformToFill">
        <Rectangle Width="200" Height="100" Fill="Red"/>
    </Viewbox>
</Grid>
```

#### Пример 7: Canvas + ScrollViewer — большая диаграмма

```xml
<ScrollViewer HorizontalScrollBarVisibility="Auto"
              VerticalScrollBarVisibility="Auto"
              Width="600" Height="400">
    <Canvas Width="2000" Height="1500" Background="White">
        <!-- Сетка -->
        <Lines X1="0" Y1="0" X2="2000" Y2="0" Stroke="LightGray"/>
        <!-- ... много линий сетки -->
        
        <!-- Объекты -->
        <Rectangle Canvas.Left="100" Canvas.Top="100"
                   Width="200" Height="150" Fill="LightBlue"/>
        <Ellipse Canvas.Left="400" Canvas.Top="200"
                 Width="150" Height="150" Fill="LightGreen"/>
        
        <!-- Соединительные линии -->
        <Line X1="300" Y1="175" X2="400" Y2="275"
              Stroke="Black" StrokeThickness="2"/>
    </Canvas>
</ScrollViewer>
```

#### Пример 8: ViewBox для масштабируемого UI

```xml
<!-- Иконка, масштабируемая под размер кнопки -->
<Button Width="50" Height="50">
    <Viewbox Stretch="Uniform">
        <Canvas Width="24" Height="24">
            <Rectangle Canvas.Left="4" Canvas.Top="4"
                       Width="16" Height="16"
                       Fill="Blue"/>
            <Line X1="4" Y1="4" X2="20" Y2="20"
                  Stroke="White" StrokeThickness="2"/>
        </Canvas>
    </Viewbox>
</Button>
```

#### Пример 9: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml -- область рисования -->
<Border Background="#F0F0F0" ClipToBounds="True"
        HorizontalAlignment="Left" VerticalAlignment="Top">
    <Canvas x:Name="DrawingCanvas"
            Background="White"
            ClipToBounds="True"
            Focusable="True">
        
        <!-- RenderTransform для панорамирования -->
        <Canvas.RenderTransform>
            <TranslateTransform X="{Binding PanOffsetX}" 
                                Y="{Binding PanOffsetY}"/>
        </Canvas.RenderTransform>
        
        <!-- Размер Canvas = размер листа в пикселях -->
        <Canvas.Width>
            <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                <Binding Path="Template.Sheet.WidthMicrons"/>
                <Binding Path="Zoom"/>
            </MultiBinding>
        </Canvas.Width>
        <Canvas.Height>
            <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                <Binding Path="Template.Sheet.HeightMicrons"/>
                <Binding Path="Zoom"/>
            </MultiBinding>
        </Canvas.Height>
        
        <!-- Объекты шаблона -->
        <ItemsControl ItemsSource="{Binding Template.Objects}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
        
        <!-- Preview линии -->
        <Line x:Name="PreviewLineElement"
              Stroke="Red" StrokeThickness="1.5" StrokeDashArray="4,2"
              Visibility="Collapsed"/>
        
        <!-- Маркеры выделения -->
        <ContentControl Content="{Binding SingleSelectedObject}"/>
    </Canvas>
</Border>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 3.4.1: Canvas с фигурами**

Создайте Canvas (400x300) с:
- Rectangle (100x80) в координатах (50, 50)
- Ellipse (80x80) в координатах (200, 100)
- Line от (50,50) до (200,100)
- Button в координатах (300, 200)

**Задача 3.4.2: ScrollViewer с текстом**

Создайте ScrollViewer (300x200) с:
- StackPanel внутри
- 50 TextBlock с текстом "Line 1" — "Line 50"
- ScrollBar должны появиться автоматически

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 3.4.3: Диаграмма с Canvas**

Создайте простую блок-схему:
- Canvas (800x600)
- 5-7 прямоугольников (блоки)
- Линии между блоками
- Подписи к блокам

**Задача 3.4.4: ViewBox для адаптивного UI**

Создайте кнопку с иконкой:
- ViewBox с Canvas (24x24)
- Иконка (дом, пользователь, настройки)
- Кнопка масштабируется (50x50, 100x100)
- Иконка сохраняет пропорции

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 3.4.5: Mini CAD Viewer**

Создайте просмотрщик чертежей:
- ScrollViewer с большим Canvas (2000x1500)
- Сетка (линии каждые 100px)
- Возможность панорамирования (мышью или кнопками)
- Zoom (кнопки +/–)

**Задача 3.4.6: Interactive Diagram**

Создайте интерактивную диаграмму:
- Canvas с фигурами
- Drag-and-drop фигур
- Выделение фигур (изменение цвета)
- Линии, соединяющие фигуры

---

### Решения

<details>
<summary>✅ Решение задачи 3.4.1</summary>

```xml
<Canvas Width="400" Height="300" Background="White">
    <!-- Rectangle -->
    <Rectangle Canvas.Left="50" Canvas.Top="50"
               Width="100" Height="80"
               Fill="LightBlue"
               Stroke="Blue"
               StrokeThickness="2"/>
    
    <!-- Ellipse -->
    <Ellipse Canvas.Left="200" Canvas.Top="100"
             Width="80" Height="80"
             Fill="LightGreen"
             Stroke="Green"
             StrokeThickness="2"/>
    
    <!-- Line -->
    <Line X1="50" Y1="50" X2="200" Y2="100"
          Stroke="Black" StrokeThickness="2"/>
    
    <!-- Button -->
    <Button Content="Click"
            Canvas.Left="300"
            Canvas.Top="200"/>
</Canvas>
```
</details>

<details>
<summary>✅ Решение задачи 3.4.3</summary>

```xml
<Canvas Width="800" Height="600" Background="White">
    <!-- Блок 1 -->
    <Rectangle Canvas.Left="350" Canvas.Top="50"
               Width="100" Height="60"
               Fill="LightBlue" Stroke="Blue" StrokeThickness="2"/>
    <TextBlock Canvas.Left="370" Canvas.Top="70"
               Text="Start"/>
    
    <!-- Линия 1->2 -->
    <Line X1="400" Y1="110" X2="400" Y2="150"
          Stroke="Black" StrokeThickness="2"/>
    
    <!-- Блок 2 -->
    <Rectangle Canvas.Left="350" Canvas.Top="150"
               Width="100" Height="60"
               Fill="LightGreen" Stroke="Green" StrokeThickness="2"/>
    <TextBlock Canvas.Left="380" Canvas.Top="170"
               Text="Process"/>
    
    <!-- Линия 2->3 -->
    <Line X1="400" Y1="210" X2="400" Y2="250"
          Stroke="Black" StrokeThickness="2"/>
    
    <!-- Блок 3 -->
    <Rectangle Canvas.Left="350" Canvas.Top="250"
               Width="100" Height="60"
               Fill="LightYellow" Stroke="Orange" StrokeThickness="2"/>
    <TextBlock Canvas.Left="375" Canvas.Top="270"
               Text="Decision"/>
    
    <!-- Линии 3->4 и 3->5 -->
    <Line X1="350" Y1="280" X2="250" Y2="280"
          Stroke="Black" StrokeThickness="2"/>
    <Line X1="450" Y1="280" X2="550" Y2="280"
          Stroke="Black" StrokeThickness="2"/>
    
    <!-- Блок 4 -->
    <Rectangle Canvas.Left="150" Canvas.Top="250"
               Width="100" Height="60"
               Fill="LightCoral" Stroke="Red" StrokeThickness="2"/>
    <TextBlock Canvas.Left="180" Canvas.Top="270"
               Text="End 1"/>
    
    <!-- Блок 5 -->
    <Rectangle Canvas.Left="550" Canvas.Top="250"
               Width="100" Height="60"
               Fill="LightCoral" Stroke="Red" StrokeThickness="2"/>
    <TextBlock Canvas.Left="580" Canvas.Top="270"
               Text="End 2"/>
</Canvas>
```
</details>

---

## Ключевые выводы

✅ **Canvas** — абсолютное позиционирование через Canvas.Left/Top  
✅ **Z-Index** — порядок наложения элементов  
✅ **ClipToBounds** — обрезка детей за пределами Canvas  
✅ **ScrollViewer** — прокрутка большого контента  
✅ **ViewBox** — масштабирование одного элемента  
✅ **Stretch** — Uniform, Fill, UniformToFill, None  
✅ **Canvas для графики** — диаграммы, CAD, чертежи

---

## Дополнительные ресурсы

- [Canvas](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.canvas)
- [ScrollViewer](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.scrollviewer)
- [ViewBox](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.viewbox)
