# Тема 7.1: Timeline-анимации (DoubleAnimation, ColorAnimation, PointAnimation)

### Теория

**Timeline** — базовый класс для всех анимаций в WPF.

#### Типы анимаций

| Анимация | Тип значения | Пример использования |
|----------|--------------|---------------------|
| **DoubleAnimation** | double | Width, Height, Opacity, Angle |
| **ColorAnimation** | Color | Background, Foreground, Stroke |
| **PointAnimation** | Point | Position, Path points |
| **ThicknessAnimation** | Thickness | Margin, Padding, BorderThickness |
| **ObjectAnimation** | object | Content, DataContext |
| **StringAnimation** | string | Text (редко) |

#### Общие свойства

| Свойство | Описание | Пример |
|----------|----------|--------|
| **Duration** | Длительность анимации | `Duration="0:0:1"` (1 секунда) |
| **BeginTime** | Задержка перед началом | `BeginTime="0:0:0.5"` |
| **From** | Начальное значение | `From="100"` |
| **To** | Конечное значение | `To="200"` |
| **By** | Изменение относительно текущего | `By="50"` |
| **RepeatBehavior** | Повторение | `RepeatBehavior="Forever"` или `3x` |
| **AutoReverse** | Автоматический возврат | `AutoReverse="True"` |
| **FillBehavior** | Поведение после завершения | `HoldEnd`, `Stop` |

### Примеры кода

#### Пример 1: DoubleAnimation — анимация Width

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="animatedRect"
               Width="100" Height="50"
               Canvas.Left="50" Canvas.Top="100"
               Fill="LightBlue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="animatedRect"
                                         Storyboard.TargetProperty="Width"
                                         From="100" To="300"
                                         Duration="0:0:2"
                                         RepeatBehavior="Forever"
                                         AutoReverse="True"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 2: DoubleAnimation — анимация Opacity (Fade)

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="fadeRect"
               Width="100" Height="100"
               Canvas.Left="150" Canvas.Top="100"
               Fill="Blue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <!-- Fade Out -->
                        <DoubleAnimation Storyboard.TargetName="fadeRect"
                                         Storyboard.TargetProperty="Opacity"
                                         From="1.0" To="0.0"
                                         Duration="0:0:1"
                                         BeginTime="0:0:0"/>
                        <!-- Fade In -->
                        <DoubleAnimation Storyboard.TargetName="fadeRect"
                                         Storyboard.TargetProperty="Opacity"
                                         From="0.0" To="1.0"
                                         Duration="0:0:1"
                                         BeginTime="0:0:1"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 3: ColorAnimation — анимация цвета фона

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="colorRect"
               Width="150" Height="100"
               Canvas.Left="125" Canvas.Top="100"
               Fill="LightBlue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <ColorAnimation Storyboard.TargetName="colorRect"
                                        Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                                        To="LightGreen"
                                        Duration="0:0:2"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 4: ColorAnimation — градиент

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="gradientRect"
               Width="200" Height="100"
               Canvas.Left="100" Canvas.Top="100">
        <Rectangle.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop x:Name="stop1" Color="Red" Offset="0.0"/>
                <GradientStop x:Name="stop2" Color="Blue" Offset="1.0"/>
            </LinearGradientBrush>
        </Rectangle.Fill>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <ColorAnimation Storyboard.TargetName="stop1"
                                        Storyboard.TargetProperty="Color"
                                        To="Yellow"
                                        Duration="0:0:2"/>
                                       
                                        <ColorAnimation Storyboard.TargetName="stop2"
                                        Storyboard.TargetProperty="Color"
                                        To="Green"
                                        Duration="0:0:2"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 5: PointAnimation — анимация позиции

```xml
<Canvas Width="400" Height="300">
    <Ellipse x:Name="movingEllipse"
             Width="40" Height="40"
             Fill="Red"
             Canvas.Left="0" Canvas.Top="130">
        <Ellipse.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever">
                        <PointAnimation Storyboard.TargetName="movingEllipse"
                                        Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                                        From="0" To="360"
                                        Duration="0:0:3"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Ellipse.Triggers>
        
        <Ellipse.RenderTransform>
            <TranslateTransform X="0"/>
        </Ellipse.RenderTransform>
    </Ellipse>
</Canvas>
```

#### Пример 6: Multiple Animations одновременно

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="multiAnimRect"
               Width="50" Height="50"
               Canvas.Left="50" Canvas.Top="125"
               Fill="Blue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <!-- Перемещение -->
                        <DoubleAnimation Storyboard.TargetName="multiAnimRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="50" To="300"
                                         Duration="0:0:2"/>
                                        
                        <!-- Вращение -->
                        <DoubleAnimation Storyboard.TargetName="multiAnimRect"
                                         Storyboard.TargetProperty="(UIElement.RenderTransform).(RotateTransform.Angle)"
                                         From="0" To="360"
                                         Duration="0:0:2"/>
                                        
                        <!-- Изменение цвета -->
                                        <ColorAnimation Storyboard.TargetName="multiAnimRect"
                                        Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                                        To="Red"
                                        Duration="0:0:2"/>
                                        
                        <!-- Изменение размера -->
                                        <DoubleAnimation Storyboard.TargetName="multiAnimRect"
                                        Storyboard.TargetProperty="Width"
                                        From="50" To="100"
                                        Duration="0:0:2"/>
                                        <DoubleAnimation Storyboard.TargetName="multiAnimRect"
                                        Storyboard.TargetProperty="Height"
                                        From="50" To="100"
                                        Duration="0:0:2"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
        
        <Rectangle.RenderTransform>
            <RotateTransform CenterX="25" CenterY="25"/>
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

#### Пример 7: By и From/To/By комбинации

```xml
<Canvas Width="400" Height="300">
    <StackPanel Orientation="Horizontal">
        <!-- From/To -->
        <Rectangle x:Name="rect1" Width="50" Height="50" Fill="Blue">
            <Rectangle.Triggers>
                <EventTrigger RoutedEvent="Loaded">
                    <BeginStoryboard>
                        <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                            <DoubleAnimation Storyboard.TargetName="rect1"
                                             Storyboard.TargetProperty="Width"
                                             From="50" To="150"
                                             Duration="0:0:1"/>
                        </Storyboard>
                    </BeginStoryboard>
                </EventTrigger>
            </Rectangle.Triggers>
        </Rectangle>
        
        <!-- By (относительное изменение) -->
        <Rectangle x:Name="rect2" Width="50" Height="50" Fill="Green" Margin="20,0">
            <Rectangle.Triggers>
                <EventTrigger RoutedEvent="Loaded">
                    <BeginStoryboard>
                        <Storyboard RepeatBehavior="Forever">
                            <DoubleAnimation Storyboard.TargetName="rect2"
                                             Storyboard.TargetProperty="Width"
                                             By="50"
                                             Duration="0:0:0.5"/>
                        </Storyboard>
                    </BeginStoryboard>
                </EventTrigger>
            </Rectangle.Triggers>
        </Rectangle>
        
        <!-- From/By комбинация -->
        <Rectangle x:Name="rect3" Width="50" Height="50" Fill="Red">
            <Rectangle.Triggers>
                <EventTrigger RoutedEvent="Loaded">
                    <BeginStoryboard>
                        <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                            <DoubleAnimation Storyboard.TargetName="rect3"
                                             Storyboard.TargetProperty="Width"
                                             From="50" By="100"
                                             Duration="0:0:1"/>
                        </Storyboard>
                    </BeginStoryboard>
                </EventTrigger>
            </Rectangle.Triggers>
        </Rectangle>
    </StackPanel>
</Canvas>
```

#### Пример 8: RepeatBehavior варианты

```xml
<StackPanel>
    <!-- Forever (бесконечно) -->
    <Rectangle x:Name="foreverRect" Width="50" Height="20" Fill="Blue" Margin="5">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="foreverRect"
                                         Storyboard.TargetProperty="Opacity"
                                         From="1" To="0"
                                         Duration="0:0:1"
                                         RepeatBehavior="Forever"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- 3x (три раза) -->
    <Rectangle x:Name="threeTimesRect" Width="50" Height="20" Fill="Green" Margin="5">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="threeTimesRect"
                                         Storyboard.TargetProperty="Opacity"
                                         From="1" To="0"
                                         Duration="0:0:1"
                                         RepeatBehavior="3x"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- 0:0:5 (в течение 5 секунд) -->
    <Rectangle x:Name="fiveSecRect" Width="50" Height="20" Fill="Red" Margin="5">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="fiveSecRect"
                                         Storyboard.TargetProperty="Opacity"
                                         From="1" To="0"
                                         Duration="0:0:0.5"
                                         RepeatBehavior="0:0:5"/>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</StackPanel>
```

#### Пример 9: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml — анимация выделения -->
<UserControl.Resources>
    <!-- Анимация появления маркеров выделения -->
    <Storyboard x:Key="SelectionAppearStoryboard">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.2"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         From="0.5" To="1"
                         Duration="0:0:0.2"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         From="0.5" To="1"
                         Duration="0:0:0.2"/>
    </Storyboard>
    
    <!-- Анимация preview линии -->
    <Storyboard x:Key="PreviewLinePulseStoryboard" RepeatBehavior="Forever">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0.3" To="1.0"
                         Duration="0:0:0.5"
                         AutoReverse="True"/>
    </Storyboard>
</UserControl.Resources>

<!-- Маркеры выделения с анимацией -->
<Canvas x:Name="SelectionMarkersLayer">
    <Rectangle x:Name="selectionMarker"
               Width="6" Height="6"
               Fill="White"
               Stroke="#0078D4"
               StrokeThickness="1"
               Opacity="0">
        <Rectangle.RenderTransform>
            <ScaleTransform CenterX="3" CenterY="3"/>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard Storyboard="{StaticResource SelectionAppearStoryboard}"/>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>

<!-- Preview линия с пульсацией -->
<Line x:Name="PreviewLineElement"
      Stroke="Red"
      StrokeThickness="1.5"
      StrokeDashArray="4,2"
      Opacity="0.5">
    <Line.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard Storyboard="{StaticResource PreviewLinePulseStoryboard}"/>
        </EventTrigger>
    </Line.Triggers>
</Line>
```

```csharp
// EditorViewModel.cs — запуск анимаций из кода
public partial class EditorViewModel : ObservableObject
{
    private Storyboard _selectionStoryboard;
    
    public void AnimateSelection(UIElement element)
    {
        var storyboard = new Storyboard();
        
        // Fade in
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(fadeAnimation, element);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeAnimation);
        
        // Scale
        var scaleTransform = new ScaleTransform(0.5, 0.5);
        element.RenderTransform = scaleTransform;
        
        var scaleXAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(scaleXAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        storyboard.Children.Add(scaleXAnimation);
        
        var scaleYAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(scaleYAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 7.1.1: Fade Animation**

Создайте Fade In анимацию:
- Rectangle с Opacity=0
- DoubleAnimation Opacity 0→1
- Duration 1 секунда

**Задача 7.1.2: Color Change**

Создайте анимацию цвета:
- Rectangle с Fill
- ColorAnimation к другому цвету
- Duration 2 секунды

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 7.1.3: Move and Resize**

Создайте комбинированную анимацию:
- Перемещение (Canvas.Left)
- Изменение размера (Width, Height)
- Одновременное выполнение

**Задача 7.1.4: Pulsing Circle**

Создайте пульсирующий круг:
- ScaleTransform анимация
- AutoReverse=True
- RepeatBehavior=Forever

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 7.1.5: Loading Spinner**

Создайте спиннер загрузки:
- 8 линий по кругу
- RotateTransform анимация
- Opacity animation для каждой линии
- Staggered BeginTime

**Задача 7.1.6: Bouncing Ball**

Создайте прыгающий мяч:
- PointAnimation для траектории
- Easing для реализма
- Повторение с затуханием

---

### Решения

<details>
<summary>✅ Решение задачи 7.1.1</summary>

```xml
<Rectangle x:Name="fadeRect"
           Width="100" Height="100"
           Fill="Blue"
           Opacity="0">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetName="fadeRect"
                                     Storyboard.TargetProperty="Opacity"
                                     From="0" To="1"
                                     Duration="0:0:1"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Rectangle.Triggers>
</Rectangle>
```
</details>

<details>
<summary>✅ Решение задачи 7.1.4</summary>

```xml
<Ellipse Width="100" Height="100" Fill="Red">
    <Ellipse.RenderTransform>
        <ScaleTransform x:Name="pulseScale" CenterX="50" CenterY="50"/>
    </Ellipse.RenderTransform>
    
    <Ellipse.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                    <DoubleAnimation Storyboard.TargetName="pulseScale"
                                     Storyboard.TargetProperty="ScaleX"
                                     From="1" To="1.2"
                                     Duration="0:0:0.5"/>
                    <DoubleAnimation Storyboard.TargetName="pulseScale"
                                     Storyboard.TargetProperty="ScaleY"
                                     From="1" To="1.2"
                                     Duration="0:0:0.5"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Ellipse.Triggers>
</Ellipse>
```
</details>

---

## Ключевые выводы

✅ **DoubleAnimation** — для double значений (Width, Opacity, Angle)  
✅ **ColorAnimation** — для Color значений  
✅ **PointAnimation** — для Point значений  
✅ **Duration** — длительность анимации (TimeSpan)  
✅ **From/To/By** — начальное/конечное/изменение  
✅ **RepeatBehavior** — Forever, Nx, или TimeSpan  
✅ **AutoReverse** — автоматический возврат  
✅ **BeginTime** — задержка перед началом  
✅ **FillBehavior** — HoldEnd (сохраняет конечное значение) или Stop

---

## Дополнительные ресурсы

- [Animation Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/animation-overview)
- [DoubleAnimation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.doubleanimation)
- [ColorAnimation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.coloranimation)
