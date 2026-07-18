# Тема 7.2: Storyboard

### Теория

**Storyboard** — контейнер для организации и управления анимациями.

#### Storyboard возможности

- **Организация** — группировка нескольких анимаций
- **Управление** — Begin(), Pause(), Resume(), Stop()
- **Targeting** — TargetName, TargetProperty
- **Вложенность** — Storyboard внутри Storyboard
- **Ресурсы** — хранение в ResourceDictionary

#### Storyboard.TargetName и Storyboard.TargetProperty

```xml
<!-- Прикреплённые свойства для указания цели -->
<Storyboard>
    <DoubleAnimation Storyboard.TargetName="myRect"
                     Storyboard.TargetProperty="Width"/>
</Storyboard>
```

| Атрибут | Описание | Пример |
|---------|----------|--------|
| **TargetName** | Имя целевого элемента | `TargetName="myRect"` |
| **TargetProperty** | Свойство для анимации | `TargetProperty="Width"` |
| **TargetProperty (вложенный)** | Вложенное свойство | `TargetProperty="(RenderTransform).(RotateTransform.Angle)"` |

### Примеры кода

#### Пример 1: Storyboard в ResourceDictionary

```xml
<Window.Resources>
    <Storyboard x:Key="FadeInStoryboard">
        <DoubleAnimation Storyboard.TargetName="myElement"
                         Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.5"/>
    </Storyboard>
</Window.Resources>

<Rectangle x:Name="myElement" Width="100" Height="100" Fill="Blue"/>

<!-- Запуск из кода -->
<!-- var storyboard = (Storyboard)FindResource("FadeInStoryboard"); -->
<!-- storyboard.Begin(); -->
```

#### Пример 2: Storyboard с несколькими анимациями

```xml
<Window.Resources>
    <Storyboard x:Key="ComplexAnimation">
        <!-- Перемещение -->
        <DoubleAnimation Storyboard.TargetName="rect"
                         Storyboard.TargetProperty="(Canvas.Left)"
                         From="0" To="300"
                         Duration="0:0:2"/>
        
        <!-- Вращение -->
        <DoubleAnimation Storyboard.TargetName="rect"
                         Storyboard.TargetProperty="(UIElement.RenderTransform).(RotateTransform.Angle)"
                         From="0" To="360"
                         Duration="0:0:2"/>
        
        <!-- Масштабирование -->
        <DoubleAnimation Storyboard.TargetName="rectScale"
                         Storyboard.TargetProperty="ScaleX"
                         From="1" To="1.5"
                         Duration="0:0:2"/>
        <DoubleAnimation Storyboard.TargetName="rectScale"
                         Storyboard.TargetProperty="ScaleY"
                         From="1" To="1.5"
                         Duration="0:0:2"/>
        
        <!-- Изменение цвета -->
        <ColorAnimation Storyboard.TargetName="rect"
                        Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                        To="Red"
                        Duration="0:0:2"/>
    </Storyboard>
</Window.Resources>

<Canvas>
    <Rectangle x:Name="rect"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Blue">
        <Rectangle.RenderTransform>
            <TransformGroup>
                <ScaleTransform x:Name="rectScale" CenterX="25" CenterY="25"/>
                <RotateTransform CenterX="25" CenterY="25"/>
            </TransformGroup>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard Storyboard="{StaticResource ComplexAnimation}"/>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 3: Управление Storyboard из кода

```csharp
public partial class MainWindow : Window
{
    private Storyboard _currentStoryboard;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void StartAnimation_Click(object sender, RoutedEventArgs e)
    {
        _currentStoryboard = (Storyboard)FindResource("MyAnimation");
        
        // Подписка на события
        _currentStoryboard.Completed += (s, args) =>
        {
            MessageBox.Show("Анимация завершена!");
        };
        
        _currentStoryboard.Begin(myElement, true);
    }

    private void PauseAnimation_Click(object sender, RoutedEventArgs e)
    {
        _currentStoryboard?.Pause(myElement);
    }

    private void ResumeAnimation_Click(object sender, RoutedEventArgs e)
    {
        _currentStoryboard?.Resume(myElement);
    }

    private void StopAnimation_Click(object sender, RoutedEventArgs e)
    {
        _currentStoryboard?.Stop(myElement);
    }
}
```

#### Пример 4: Storyboard в ControlTemplate

```xml
<Style x:Key="AnimatedButton" TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="5">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                </Border>
                
                <ControlTemplate.Triggers>
                    <!-- Hover эффект -->
                    <Trigger Property="IsMouseOver" Value="True">
                        <Trigger.EnterActions>
                            <BeginStoryboard>
                                <Storyboard>
                                    <ColorAnimation Storyboard.TargetName="border"
                                                    Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                    To="#005A9E"
                                                    Duration="0:0:0.2"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard>
                                <Storyboard>
                                    <ColorAnimation Storyboard.TargetName="border"
                                                    Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                    To="#0078D4"
                                                    Duration="0:0:0.2"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </Trigger.ExitActions>
                    </Trigger>
                    
                    <!-- Click эффект -->
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="border" Property="RenderTransform">
                            <Setter.Value>
                                <ScaleTransform ScaleX="0.95" ScaleY="0.95"/>
                            </Setter.Value>
                        </Setter>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### Пример 5: Sequential Storyboard (последовательные анимации)

```xml
<Window.Resources>
    <Storyboard x:Key="SequentialAnimation">
        <!-- Сначала Fade In -->
        <DoubleAnimation Storyboard.TargetName="rect1"
                         Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.5"
                         BeginTime="0:0:0"/>
        
        <!-- Потом перемещение -->
        <DoubleAnimation Storyboard.TargetName="rect1"
                         Storyboard.TargetProperty="(Canvas.Left)"
                         From="0" To="200"
                         Duration="0:0:1"
                         BeginTime="0:0:0.5"/>
        
        <!-- Потом изменение цвета -->
        <ColorAnimation Storyboard.TargetName="rect1"
                        Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                        To="Red"
                        Duration="0:0:0.5"
                        BeginTime="0:0:1.5"/>
        
        <!-- Потом масштабирование -->
        <DoubleAnimation Storyboard.TargetName="rect1Scale"
                         Storyboard.TargetProperty="ScaleX"
                         From="1" To="1.5"
                         Duration="0:0:0.5"
                         BeginTime="0:0:2"/>
        <DoubleAnimation Storyboard.TargetName="rect1Scale"
                         Storyboard.TargetProperty="ScaleY"
                         From="1" To="1.5"
                         Duration="0:0:0.5"
                         BeginTime="0:0:2"/>
    </Storyboard>
</Window.Resources>

<Canvas>
    <Rectangle x:Name="rect1"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Blue"
               Opacity="0">
        <Rectangle.RenderTransform>
            <ScaleTransform x:Name="rect1Scale" CenterX="25" CenterY="25"/>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard Storyboard="{StaticResource SequentialAnimation}"/>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 6: Parallel Storyboard (параллельные анимации)

```xml
<Window.Resources>
    <Storyboard x:Key="ParallelAnimation">
        <!-- Все анимации начинаются одновременно -->
        <DoubleAnimation Storyboard.TargetName="rect2"
                         Storyboard.TargetProperty="(Canvas.Left)"
                         From="0" To="200"
                         Duration="0:0:1"/>
        
        <DoubleAnimation Storyboard.TargetName="rect2"
                         Storyboard.TargetProperty="(Canvas.Top)"
                         From="100" To="50"
                         Duration="0:0:1"/>
        
        <DoubleAnimation Storyboard.TargetName="rect2Scale"
                         Storyboard.TargetProperty="ScaleX"
                         From="1" To="1.3"
                         Duration="0:0:1"/>
        <DoubleAnimation Storyboard.TargetName="rect2Scale"
                         Storyboard.TargetProperty="ScaleY"
                         From="1" To="1.3"
                         Duration="0:0:1"/>
        
        <ColorAnimation Storyboard.TargetName="rect2"
                        Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                        To="Green"
                        Duration="0:0:1"/>
    </Storyboard>
</Window.Resources>

<Canvas>
    <Rectangle x:Name="rect2"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Blue">
        <Rectangle.RenderTransform>
            <ScaleTransform x:Name="rect2Scale" CenterX="25" CenterY="25"/>
        </Rectangle.RenderTransform>
        
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard Storyboard="{StaticResource ParallelAnimation}"/>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 7: Storyboard с KeyTime (временные ключи)

```xml
<Window.Resources>
    <Storyboard x:Key="KeyTimeAnimation">
        <!-- Анимация с ключевыми точками времени -->
        <DoubleAnimationUsingKeyFrames Storyboard.TargetName="rect3"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         Duration="0:0:4">
            <!-- Линейная интерполяция -->
            <LinearDoubleKeyFrame Value="0" KeyTime="0:0:0"/>
            <LinearDoubleKeyFrame Value="100" KeyTime="0:0:1"/>
            <LinearDoubleKeyFrame Value="300" KeyTime="0:0:2"/>
            <LinearDoubleKeyFrame Value="200" KeyTime="0:0:3"/>
            <LinearDoubleKeyFrame Value="0" KeyTime="0:0:4"/>
        </DoubleAnimationUsingKeyFrames>
        
        <!-- Анимация с дискретными скачками -->
        <DoubleAnimationUsingKeyFrames Storyboard.TargetName="rect3"
                                         Storyboard.TargetProperty="Opacity"
                                         Duration="0:0:4">
            <DiscreteDoubleKeyFrame Value="1" KeyTime="0:0:0"/>
            <DiscreteDoubleKeyFrame Value="0.5" KeyTime="0:0:2"/>
            <DiscreteDoubleKeyFrame Value="1" KeyTime="0:0:4"/>
        </DoubleAnimationUsingKeyFrames>
        
        <!-- Анимация с плавными переходами (Spline) -->
        <DoubleAnimationUsingKeyFrames Storyboard.TargetName="rect3"
                                         Storyboard.TargetProperty="(Canvas.Top)"
                                         Duration="0:0:4">
            <SplineDoubleKeyFrame Value="100" KeyTime="0:0:0"
                                  KeySpline="0,0,1,1"/>
            <SplineDoubleKeyFrame Value="50" KeyTime="0:0:2"
                                  KeySpline="0.5,0,0.5,1"/>
            <SplineDoubleKeyFrame Value="100" KeyTime="0:0:4"
                                  KeySpline="1,1,0,0"/>
        </DoubleAnimationUsingKeyFrames>
    </Storyboard>
</Window.Resources>

<Canvas>
    <Rectangle x:Name="rect3"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Purple">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard Storyboard="{StaticResource KeyTimeAnimation}"/>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml — анимация выделения объектов -->
<UserControl.Resources>
    <!-- Анимация появления маркеров -->
    <Storyboard x:Key="MarkersAppearStoryboard">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.15"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         From="0" To="1"
                         Duration="0:0:0.15"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         From="0" To="1"
                         Duration="0:0:0.15"/>
    </Storyboard>
    
    <!-- Анимация исчезновения маркеров -->
    <Storyboard x:Key="MarkersDisappearStoryboard">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="1" To="0"
                         Duration="0:0:0.1"/>
    </Storyboard>
    
    <!-- Анимация панорамирования -->
    <Storyboard x:Key="PanAnimation">
        <DoubleAnimation Storyboard.TargetName="PanTransform"
                         Storyboard.TargetProperty="X"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetName="PanTransform"
                         Storyboard.TargetProperty="Y"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
    <!-- Анимация zoom -->
    <Storyboard x:Key="ZoomAnimation">
        <DoubleAnimation Storyboard.TargetName="ZoomTransform"
                         Storyboard.TargetProperty="ScaleX"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetName="ZoomTransform"
                         Storyboard.TargetProperty="ScaleY"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
</UserControl.Resources>

<!-- Использование в коде -->
<!-- EditorCanvas.xaml.cs -->
public partial class EditorCanvas : UserControl
{
    public void AnimatePan(double targetX, double targetY)
    {
        var storyboard = (Storyboard)FindResource("PanAnimation");
        var xAnim = (DoubleAnimation)storyboard.Children[0];
        var yAnim = (DoubleAnimation)storyboard.Children[1];
        
        xAnim.To = targetX;
        yAnim.To = targetY;
        
        storyboard.Begin(PanTransform, true);
    }
    
    public void AnimateZoom(double targetZoom)
    {
        var storyboard = (Storyboard)FindResource("ZoomAnimation");
        var xAnim = (DoubleAnimation)storyboard.Children[0];
        var yAnim = (DoubleAnimation)storyboard.Children[1];
        
        xAnim.To = targetZoom;
        yAnim.To = targetZoom;
        
        storyboard.Begin(ZoomTransform, true);
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 7.2.1: Simple Storyboard**

Создайте Storyboard в ресурсах:
- DoubleAnimation для Width
- Запуск по клику
- Duration 1 секунда

**Задача 7.2.2: Fade Storyboard**

Создайте Storyboard:
- Fade In (Opacity 0→1)
- Сохраните в ResourceDictionary
- Запустите из кода

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 7.2.3: Multi-Property Storyboard**

Создайте Storyboard с 3+ анимациями:
- Перемещение (Canvas.Left, Canvas.Top)
- Вращение (RotateTransform.Angle)
- Изменение цвета

**Задача 7.2.4: Control Template Animation**

Создайте стиль для Button:
- Hover эффект (ColorAnimation)
- Click эффект (ScaleTransform)
- EnterActions и ExitActions

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 7.2.5: Sequential Animation**

Создайте последовательную анимацию:
- 4 этапа с BeginTime
- Fade → Move → Rotate → Color
- Общее время 4 секунды

**Задача 7.2.6: KeyFrame Animation**

Создайте анимацию с KeyFrames:
- LinearDoubleKeyFrame
- DiscreteDoubleKeyFrame
- SplineDoubleKeyFrame с KeySpline

---

### Решения

<details>
<summary>✅ Решение задачи 7.2.1</summary>

```xml
<Window.Resources>
    <Storyboard x:Key="WidthAnimation">
        <DoubleAnimation Storyboard.TargetName="myRect"
                         Storyboard.TargetProperty="Width"
                         From="100" To="300"
                         Duration="0:0:1"/>
    </Storyboard>
</Window.Resources>

<Rectangle x:Name="myRect"
           Width="100" Height="50"
           Fill="Blue"
           MouseLeftButtonDown="myRect_MouseLeftButtonDown"/>

<!-- Code-behind -->
private void myRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    var storyboard = (Storyboard)FindResource("WidthAnimation");
    storyboard.Begin();
}
```
</details>

<details>
<summary>✅ Решение задачи 7.2.4</summary>

```xml
<Style x:Key="AnimatedButton" TargetType="Button">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="20,10"/>
    
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="5"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                </Border>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Trigger.EnterActions>
                            <BeginStoryboard>
                                <Storyboard>
                                    <ColorAnimation Storyboard.TargetName="border"
                                                    Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                    To="#005A9E"
                                                    Duration="0:0:0.2"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard>
                                <Storyboard>
                                    <ColorAnimation Storyboard.TargetName="border"
                                                    Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                    To="#0078D4"
                                                    Duration="0:0:0.2"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </Trigger.ExitActions>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```
</details>

---

## Ключевые выводы

✅ **Storyboard** — контейнер для организации анимаций  
✅ **TargetName** — имя целевого элемента  
✅ **TargetProperty** — свойство для анимации  
✅ **Begin()** — запуск анимации  
✅ **Pause()/Resume()/Stop()** — управление  
✅ **EnterActions/ExitActions** — для триггеров  
✅ **KeyFrames** — временные ключи (Linear, Discrete, Spline)  
✅ **BeginTime** — задержка для последовательных анимаций  
✅ **ResourceDictionary** — хранение Storyboard

---

## Дополнительные ресурсы

- [Storyboard](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.storyboard)
- [Storyboards Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/storyboards-overview)
- [Key-Frame Animations](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/key-frame-animations)
