# Тема 6.2: Transform (RotateTransform, ScaleTransform, TranslateTransform)

### Теория

**Transform** — преобразование координат элементов для вращения, масштабирования и перемещения.

#### Типы Transform

| Transform | Описание | Свойства |
|-----------|----------|----------|
| **RotateTransform** | Вращение | Angle, CenterX, CenterY |
| **ScaleTransform** | Масштабирование | ScaleX, ScaleY, CenterX, CenterY |
| **TranslateTransform** | Перемещение | X, Y |
| **SkewTransform** | Искажение | AngleX, AngleY, CenterX, CenterY |
| **TransformGroup** | Группа трансформаций | Children |
| **MatrixTransform** | Матричное преобразование | Matrix |

#### RenderTransform vs LayoutTransform

- **RenderTransform** — применяется ПОСЛЕ layout (быстрее)
- **LayoutTransform** — применяется ДО layout (влияет на расположение)

### Примеры кода

#### Пример 1: RotateTransform — вращение

```xml
<Canvas Width="400" Height="300">
    <!-- Исходный прямоугольник -->
    <Rectangle Width="100" Height="50"
               Canvas.Left="50" Canvas.Top="50"
               Fill="LightBlue"
               Stroke="Blue"/>
    
    <!-- Поворот на 45 градусов -->
    <Rectangle Width="100" Height="50"
               Canvas.Left="200" Canvas.Top="50"
               Fill="LightGreen"
               Stroke="Green">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="45"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Поворот на 90 градусов -->
    <Rectangle Width="100" Height="50"
               Canvas.Left="50" Canvas.Top="150"
               Fill="LightYellow"
               Stroke="Orange">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="90"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Поворот вокруг центра -->
    <Rectangle Width="100" Height="50"
               Canvas.Left="200" Canvas.Top="150"
               Fill="LightCoral"
               Stroke="Red">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="45" CenterX="50" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 2: ScaleTransform — масштабирование

```xml
<Canvas Width="400" Height="300">
    <!-- Исходный -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="20" Canvas.Top="20"
               Fill="Gray"
               Stroke="Black"/>
    
    <!-- Увеличение в 2 раза -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="100" Canvas.Top="20"
               Fill="LightBlue"
               Stroke="Blue">
        <Rectangle.RenderTransform>
            <ScaleTransform ScaleX="2" ScaleY="2"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Уменьшение в 0.5 раза -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="250" Canvas.Top="20"
               Fill="LightGreen"
               Stroke="Green">
        <Rectangle.RenderTransform>
            <ScaleTransform ScaleX="0.5" ScaleY="0.5"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Растягивание по X -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="20" Canvas.Top="150"
               Fill="LightYellow"
               Stroke="Orange">
        <Rectangle.RenderTransform>
            <ScaleTransform ScaleX="3" ScaleY="1"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Отражение по горизонтали -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="150" Canvas.Top="150"
               Fill="LightCoral"
               Stroke="Red">
        <Rectangle.RenderTransform>
            <ScaleTransform ScaleX="-1" CenterX="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 3: TranslateTransform — перемещение

```xml
<Canvas Width="400" Height="300">
    <!-- Исходная позиция -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="0"
               Fill="Gray"/>
    
    <!-- Перемещение на (100, 50) -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="0"
               Fill="LightBlue">
        <Rectangle.RenderTransform>
            <TranslateTransform X="100" Y="50"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Перемещение на (200, 150) -->
    <Rectangle Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="0"
               Fill="LightGreen">
        <Rectangle.RenderTransform>
            <TranslateTransform X="200" Y="150"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 4: TransformGroup — комбинация

```xml
<Canvas Width="400" Height="300">
    <!-- Комбинация трансформаций -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="160" Canvas.Top="125"
               Fill="LightBlue"
               Stroke="Blue"
               StrokeThickness="2">
        <Rectangle.RenderTransform>
            <TransformGroup>
                <!-- 1. Масштабирование -->
                <ScaleTransform ScaleX="1.2" ScaleY="1.2"/>
                <!-- 2. Вращение -->
                <RotateTransform Angle="30" CenterX="40" CenterY="25"/>
                <!-- 3. Перемещение -->
                <TranslateTransform X="50" Y="30"/>
            </TransformGroup>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Порядок важен! -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="160" Canvas.Top="225"
               Fill="LightGreen"
               Stroke="Green"
               StrokeThickness="2">
        <Rectangle.RenderTransform>
            <TransformGroup>
                <!-- Другой порядок -->
                <TranslateTransform X="50" Y="30"/>
                <RotateTransform Angle="30" CenterX="40" CenterY="25"/>
                <ScaleTransform ScaleX="1.2" ScaleY="1.2"/>
            </TransformGroup>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 5: SkewTransform — искажение

```xml
<Canvas Width="400" Height="300">
    <!-- Исходный -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="20" Canvas.Top="20"
               Fill="Gray"/>
    
    <!-- Искажение по X -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="150" Canvas.Top="20"
               Fill="LightBlue">
        <Rectangle.RenderTransform>
            <SkewTransform AngleX="30" CenterX="40" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Искажение по Y -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="20" Canvas.Top="150"
               Fill="LightGreen">
        <Rectangle.RenderTransform>
            <SkewTransform AngleY="20" CenterX="40" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- Комбинированное искажение -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="150" Canvas.Top="150"
               Fill="LightYellow">
        <Rectangle.RenderTransform>
            <SkewTransform AngleX="20" AngleY="15"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 6: MatrixTransform — матричное преобразование

```xml
<Canvas Width="400" Height="300">
    <!-- Матрица трансформации -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="160" Canvas.Top="125"
               Fill="LightBlue">
        <Rectangle.RenderTransform>
            <MatrixTransform>
                <MatrixTransform.Matrix>
                    <!-- M11, M12, M21, M22, OffsetX, OffsetY -->
                    <Matrix M11="1.5" M12="0.2" 
                            M21="-0.2" M22="1.5"
                            OffsetX="50" OffsetY="30"/>
                </MatrixTransform.Matrix>
            </MatrixTransform>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 7: Анимация трансформаций

```xml
<Canvas Width="400" Height="300">
    <Rectangle Width="60" Height="60"
               Canvas.Left="170" Canvas.Top="120"
               Fill="LightBlue"
               Stroke="Blue">
        <Rectangle.RenderTransform>
            <TransformGroup>
                <RotateTransform x:Name="rotateTransform"/>
                <ScaleTransform x:Name="scaleTransform"/>
            </TransformGroup>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <!-- Вращение -->
                        <DoubleAnimation Storyboard.TargetName="rotateTransform"
                                         Storyboard.TargetProperty="Angle"
                                         From="0" To="360"
                                         Duration="0:0:2"/>
                        <!-- Масштабирование -->
                        <DoubleAnimation Storyboard.TargetName="scaleTransform"
                                         Storyboard.TargetProperty="ScaleX"
                                         From="1" To="1.5"
                                         AutoReverse="True"
                                         Duration="0:0:1"/>
                        <DoubleAnimation Storyboard.TargetName="scaleTransform"
                                         Storyboard.TargetProperty="ScaleY"
                                         From="1" To="1.5"
                                         AutoReverse="True"
                                         Duration="0:0:1"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml -- Transform для панорамирования и zoom -->
<UserControl>
    <Border Background="#F0F0F0" ClipToBounds="True">
        <Canvas x:Name="DrawingCanvas" Background="White">
            
            <!-- RenderTransform для панорамирования и масштабирования -->
            <Canvas.RenderTransform>
                <TransformGroup>
                    <!-- Масштабирование (Zoom) -->
                    <ScaleTransform x:Name="ZoomTransform"
                                    ScaleX="{Binding Zoom}"
                                    ScaleY="{Binding Zoom}"/>
                    <!-- Перемещение (Pan) -->
                    <TranslateTransform x:Name="PanTransform"
                                        X="{Binding PanOffsetX}"
                                        Y="{Binding PanOffsetY}"/>
                </TransformGroup>
            </Canvas.RenderTransform>
            
            <!-- Размер Canvas = размер листа -->
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
            
            <!-- Маркеры выделения с Transform -->
            <Canvas x:Name="SelectionMarkersLayer">
                <Canvas.RenderTransform>
                    <TransformGroup>
                        <ScaleTransform ScaleX="{Binding Zoom}" ScaleY="{Binding Zoom}"/>
                        <TranslateTransform X="{Binding PanOffsetX}" Y="{Binding PanOffsetY}"/>
                    </TransformGroup>
                </Canvas.RenderTransform>
                
                <!-- Маркеры рисуются здесь -->
            </Canvas>
            
            <!-- Сетка -->
            <Canvas x:Name="GridLayer">
                <Canvas.RenderTransform>
                    <TransformGroup>
                        <ScaleTransform ScaleX="{Binding Zoom}" ScaleY="{Binding Zoom}"/>
                        <TranslateTransform X="{Binding PanOffsetX}" Y="{Binding PanOffsetY}"/>
                    </TransformGroup>
                </Canvas.RenderTransform>
                
                <!-- Линии сетки рисуются через OnRender -->
            </Canvas>
            
            <!-- Preview линия -->
            <Line x:Name="PreviewLine"
                  Stroke="Red" StrokeThickness="1.5" StrokeDashArray="4,2"
                  Visibility="{Binding ShowPreviewLine, Converter={StaticResource BoolToVisibility}}">
                <Line.RenderTransform>
                    <TransformGroup>
                        <ScaleTransform ScaleX="{Binding Zoom}" ScaleY="{Binding Zoom}"/>
                        <TranslateTransform X="{Binding PanOffsetX}" Y="{Binding PanOffsetY}"/>
                    </TransformGroup>
                </Line.RenderTransform>
            </Line>
        </Canvas>
    </Border>
</UserControl>
```

```csharp
// EditorViewModel.cs - Zoom и Pan
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private double _panOffsetX;

    [ObservableProperty]
    private double _panOffsetY;

    [RelayCommand]
    private void ZoomIn()
    {
        Zoom = Math.Min(Zoom * 1.2, 10.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Zoom = Math.Max(Zoom / 1.2, 0.1);
    }

    [RelayCommand]
    private void ZoomToFit()
    {
        // Вычисление Zoom для FitToScreen
        var sheetWidth = Template.Sheet.WidthMm * 96 / 25.4;
        var sheetHeight = Template.Sheet.HeightMm * 96 / 25.4;
        
        var zoomX = ActualWidth / sheetWidth;
        var zoomY = ActualHeight / sheetHeight;
        
        Zoom = Math.Min(zoomX, zoomY) * 0.95;
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    public void Pan(double deltaX, double deltaY)
    {
        PanOffsetX += deltaX;
        PanOffsetY += deltaY;
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 6.2.1: Вращение**

Создайте Rectangle и поверните:
- На 30 градусов
- На 90 градусов
- На 180 градусов

**Задача 6.2.2: Масштабирование**

Создайте 3 Rectangle:
- Оригинал 50x50
- Увеличенный в 2 раза
- Уменьшенный в 0.5 раза

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 6.2.3: TransformGroup**

Создайте комбинацию:
- ScaleTransform (1.5, 1.5)
- RotateTransform (45 градусов)
- TranslateTransform (100, 50)

**Задача 6.2.4: Анимированный Rotate**

Создайте вращающийся квадрат:
- RotateTransform
- Storyboard с DoubleAnimation
- Бесконечное вращение (0-360)

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 6.2.5: Интерактивный Zoom/Pan**

Реализуйте:
- Canvas с изображениями
- Zoom колесом мыши
- Pan перетаскиванием
- Ограничения zoom (0.1-10)

**Задача 6.2.6: Transform Editor**

Создайте редактор трансформаций:
- Slider для ScaleX, ScaleY
- Slider для Rotation
- Slider для Translate X, Y
- Preview в реальном времени

---

### Решения

<details>
<summary>✅ Решение задачи 6.2.1</summary>

```xml
<Canvas Width="400" Height="200">
    <!-- 30 градусов -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="50" Canvas.Top="75"
               Fill="LightBlue">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="30" CenterX="40" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- 90 градусов -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="180" Canvas.Top="75"
               Fill="LightGreen">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="90" CenterX="40" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
    
    <!-- 180 градусов -->
    <Rectangle Width="80" Height="50"
               Canvas.Left="310" Canvas.Top="75"
               Fill="LightYellow">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="180" CenterX="40" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```
</details>

<details>
<summary>✅ Решение задачи 6.2.4</summary>

```xml
<Canvas Width="200" Height="200">
    <Rectangle Width="60" Height="60"
               Canvas.Left="70" Canvas.Top="70"
               Fill="LightBlue">
        <Rectangle.RenderTransform>
            <RotateTransform x:Name="rotateTransform" 
                             CenterX="30" CenterY="30"/>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <DoubleAnimation Storyboard.TargetName="rotateTransform"
                                         Storyboard.TargetProperty="Angle"
                                         From="0" To="360"
                                         Duration="0:0:2"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```
</details>

---

## Ключевые выводы

✅ **RotateTransform** — вращение (Angle, CenterX, CenterY)  
✅ **ScaleTransform** — масштабирование (ScaleX, ScaleY)  
✅ **TranslateTransform** — перемещение (X, Y)  
✅ **TransformGroup** — комбинация трансформаций  
✅ **Порядок важен** — трансформации применяются последовательно  
✅ **RenderTransform** — после layout (быстрее)  
✅ **LayoutTransform** — до layout (влияет на расположение)  
✅ **CenterX/CenterY** — точка применения трансформации

---

## Дополнительные ресурсы

- [Transform](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.transform)
- [RenderTransform](https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.rendertransform)
- [LayoutTransform](https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.layouttransform)
