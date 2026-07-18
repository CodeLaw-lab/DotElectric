# Тема 7.3: Easing Functions (функции плавности)

### Теория

**Easing Function** — математическая функция для управления плавностью анимации.

#### Типы easing functions

| Функция | Описание | Визуализация |
|---------|----------|--------------|
| **Linear** | Без easing (линейная) | Прямая линия |
| **Quadratic** | Квадратичная (x²) | Плавная кривая |
| **Cubic** | Кубическая (x³) | Более резкая |
| **Quartic** | Четвёртая степень (x⁴) | Ещё резче |
| **Quintic** | Пятая степень (x⁵) | Очень резкая |
| **Sine** | Синусоидальная | Волна |
| **Exponential** | Экспоненциальная | Быстрый рост |
| **Circle** | Circular function | Половина круга |
| **Elastic** | Упругая (пружина) | Колебания |
| **Back** | Оттягивание | Уход назад |
| **Bounce** | Отскок | Прыжки |

#### EasingMode

| Режим | Описание | Пример |
|-------|----------|--------|
| **EaseIn** | Плавное начало | Начинается медленно, ускоряется |
| **EaseOut** | Плавное окончание | Начинается быстро, замедляется |
| **EaseInOut** | Плавное начало и окончание | Медленно → быстро → медленно |

### Примеры кода

#### Пример 1: Сравнение easing functions

```xml
<StackPanel>
    <!-- Linear - без easing -->
    <TextBlock Text="Linear (no easing)"/>
    <Rectangle x:Name="linearRect" Width="50" Height="20" Fill="Blue" Margin="5"/>
    
    <!-- Quadratic EaseOut -->
    <TextBlock Text="Quadratic EaseOut"/>
    <Rectangle x:Name="quadRect" Width="50" Height="20" Fill="Green" Margin="5"/>
    
    <!-- Cubic EaseOut -->
    <TextBlock Text="Cubic EaseOut"/>
    <Rectangle x:Name="cubicRect" Width="50" Height="20" Fill="Red" Margin="5"/>
    
    <!-- Bounce EaseOut -->
    <TextBlock Text="Bounce EaseOut"/>
    <Rectangle x:Name="bounceRect" Width="50" Height="20" Fill="Orange" Margin="5"/>
    
    <Button Content="Animate All" Click="AnimateAll_Click" Margin="10"/>
</StackPanel>
```

```csharp
private void AnimateAll_Click(object sender, RoutedEventArgs e)
{
    // Linear
    var linearAnim = new DoubleAnimation
    {
        From = 0,
        To = 300,
        Duration = TimeSpan.FromSeconds(1)
        // No easing
    };
    linearRect.BeginAnimation(Canvas.LeftProperty, linearAnim);
    
    // Quadratic EaseOut
    var quadAnim = new DoubleAnimation
    {
        From = 0,
        To = 300,
        Duration = TimeSpan.FromSeconds(1)
    };
    quadAnim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
    quadRect.BeginAnimation(Canvas.LeftProperty, quadAnim);
    
    // Cubic EaseOut
    var cubicAnim = new DoubleAnimation
    {
        From = 0,
        To = 300,
        Duration = TimeSpan.FromSeconds(1)
    };
    cubicAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
    cubicRect.BeginAnimation(Canvas.LeftProperty, cubicAnim);
    
    // Bounce EaseOut
    var bounceAnim = new DoubleAnimation
    {
        From = 0,
        To = 300,
        Duration = TimeSpan.FromSeconds(1)
    };
    bounceAnim.EasingFunction = new BounceEase { EasingMode = EasingMode.EaseOut };
    bounceRect.BeginAnimation(Canvas.LeftProperty, bounceAnim);
}
```

#### Пример 2: EasingMode сравнение

```xml
<Canvas Width="400" Height="400">
    <!-- EaseIn -->
    <Rectangle x:Name="easeInRect"
               Width="50" Height="20"
               Canvas.Top="50"
               Fill="Blue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeInRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseIn"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- EaseOut -->
    <Rectangle x:Name="easeOutRect"
               Width="50" Height="20"
               Canvas.Top="100"
               Fill="Green">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeOutRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- EaseInOut -->
    <Rectangle x:Name="easeInOutRect"
               Width="50" Height="20"
               Canvas.Top="150"
               Fill="Red">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeInOutRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseInOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 3: BounceEase с настройками

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="bounceRect"
               Width="40" Height="40"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Orange">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="bounceRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="360"
                                         Duration="0:0:2">
                            <DoubleAnimation.EasingFunction>
                                <BounceEase Bounces="5"
                                            Bounciness="2"
                                            EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 4: ElasticEase

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="elasticRect"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Purple">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="elasticRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:2">
                            <DoubleAnimation.EasingFunction>
                                <ElasticEase Oscillations="3"
                                             Springiness="3"
                                             EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 5: BackEase

```xml
<Canvas Width="400" Height="300">
    <Rectangle x:Name="backRect"
               Width="50" Height="50"
               Canvas.Left="0" Canvas.Top="100"
               Fill="Teal">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="backRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1">
                            <DoubleAnimation.EasingFunction>
                                <BackEase Amplitude="0.3"
                                          EasingMode="EaseInOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 6: CircleEase

```xml
<Canvas Width="400" Height="300">
    <Ellipse x:Name="circleRect"
             Width="50" Height="50"
             Canvas.Left="0" Canvas.Top="100"
             Fill="Coral">
        <Ellipse.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="circleRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1">
                            <DoubleAnimation.EasingFunction>
                                <CircleEase EasingMode="EaseInOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Ellipse.Triggers>
    </Ellipse>
</Canvas>
```

#### Пример 7: PowerEase (универсальная)

```xml
<!-- PowerEase заменяет Quadratic, Cubic, Quartic, Quintic -->
<Canvas Width="400" Height="300">
    <!-- Power=2 (как Quadratic) -->
    <Rectangle x:Name="power2Rect"
               Width="50" Height="20"
               Canvas.Top="50"
               Fill="Blue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="power2Rect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1">
                            <DoubleAnimation.EasingFunction>
                                <PowerEase Power="2" EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- Power=3 (как Cubic) -->
    <Rectangle x:Name="power3Rect"
               Width="50" Height="20"
               Canvas.Top="100"
               Fill="Green">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="power3Rect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1">
                            <DoubleAnimation.EasingFunction>
                                <PowerEase Power="3" EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- Power=5 (как Quintic) -->
    <Rectangle x:Name="power5Rect"
               Width="50" Height="20"
               Canvas.Top="150"
               Fill="Red">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="MouseLeftButtonDown">
                <BeginStoryboard>
                    <Storyboard>
                        <DoubleAnimation Storyboard.TargetName="power5Rect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1">
                            <DoubleAnimation.EasingFunction>
                                <PowerEase Power="5" EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
</Canvas>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml — плавное панорамирование и zoom -->
<UserControl.Resources>
    <!-- Анимация панорамирования с QuadraticEase -->
    <Storyboard x:Key="SmoothPanAnimation">
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
    
    <!-- Анимация zoom с CubicEase для более резкого старта -->
    <Storyboard x:Key="SmoothZoomAnimation">
        <DoubleAnimation Storyboard.TargetName="ZoomTransform"
                         Storyboard.TargetProperty="ScaleX"
                         Duration="0:0:0.25">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetName="ZoomTransform"
                         Storyboard.TargetProperty="ScaleY"
                         Duration="0:0:0.25">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
    <!-- Анимация выделения с BackEase для эффекта "оттягивания" -->
    <Storyboard x:Key="SelectionBounceAnimation">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <BackEase Amplitude="0.2" EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         From="0.8" To="1"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <BackEase Amplitude="0.2" EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         From="0.8" To="1"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <BackEase Amplitude="0.2" EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
</UserControl.Resources>
```

```csharp
// EditorCanvas.xaml.cs — использование анимаций с easing
public partial class EditorCanvas : UserControl
{
    public void SmoothPanTo(double targetX, double targetY)
    {
        var storyboard = (Storyboard)FindResource("SmoothPanAnimation");
        var xAnim = (DoubleAnimation)storyboard.Children[0];
        var yAnim = (DoubleAnimation)storyboard.Children[1];
        
        xAnim.To = targetX;
        yAnim.To = targetY;
        
        PanTransform.BeginAnimation(TranslateTransform.XProperty, xAnim);
        PanTransform.BeginAnimation(TranslateTransform.YProperty, yAnim);
    }
    
    public void SmoothZoomTo(double targetZoom)
    {
        var storyboard = (Storyboard)FindResource("SmoothZoomAnimation");
        var xAnim = (DoubleAnimation)storyboard.Children[0];
        var yAnim = (DoubleAnimation)storyboard.Children[1];
        
        xAnim.To = targetZoom;
        yAnim.To = targetZoom;
        
        ZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, xAnim);
        ZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, yAnim);
    }
    
    public void AnimateSelectionAppear(UIElement element)
    {
        var storyboard = (Storyboard)FindResource("SelectionBounceAnimation");
        
        // Клонирование storyboard для повторного использования
        var clone = storyboard.Clone();
        element.BeginAnimation(UIElement.OpacityProperty, 
            ((DoubleAnimation)clone.Children[0]));
        
        var transform = element.RenderTransform as ScaleTransform;
        if (transform == null)
        {
            transform = new ScaleTransform();
            element.RenderTransform = transform;
        }
        
        transform.BeginAnimation(ScaleTransform.ScaleXProperty, 
            ((DoubleAnimation)clone.Children[1]));
        transform.BeginAnimation(ScaleTransform.ScaleYProperty, 
            ((DoubleAnimation)clone.Children[2]));
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 7.3.1: QuadraticEase**

Создайте анимацию с QuadraticEase:
- EasingMode=EaseOut
- Перемещение прямоугольника
- Duration 1 секунда

**Задача 7.3.2: BounceEase**

Создайте анимацию с BounceEase:
- Bounces=3
- EasingMode=EaseOut
- Прыгающий мяч

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 7.3.3: EasingMode Comparison**

Создайте сравнение:
- 3 прямоугольника
- EaseIn, EaseOut, EaseInOut
- Одинаковая длительность

**Задача 7.3.4: Elastic Animation**

Создайте упругую анимацию:
- ElasticEase
- Oscillations=3
- Springiness=3

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 7.3.5: Custom Easing Sequence**

Создайте последовательность:
- 5 прямоугольников
- Разные easing для каждого
- Запуск по очереди

**Задача 7.3.6: Physics-based Animation**

Создайте физику-подобную анимацию:
- Падение с ускорением (EaseIn)
- Отскок (BounceEase)
- Затухание колебаний

---

### Решения

<details>
<summary>✅ Решение задачи 7.3.1</summary>

```xml
<Rectangle x:Name="quadRect"
           Width="50" Height="50"
           Fill="Blue">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="MouseLeftButtonDown">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetName="quadRect"
                                     Storyboard.TargetProperty="(Canvas.Left)"
                                     From="0" To="300"
                                     Duration="0:0:1">
                        <DoubleAnimation.EasingFunction>
                            <QuadraticEase EasingMode="EaseOut"/>
                        </DoubleAnimation.EasingFunction>
                    </DoubleAnimation>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Rectangle.Triggers>
</Rectangle>
```
</details>

<details>
<summary>✅ Решение задачи 7.3.3</summary>

```xml
<Canvas Width="400" Height="200">
    <!-- EaseIn -->
    <Rectangle x:Name="easeInRect" Width="50" Height="20" Canvas.Top="30" Fill="Blue">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeInRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseIn"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- EaseOut -->
    <Rectangle x:Name="easeOutRect" Width="50" Height="20" Canvas.Top="80" Fill="Green">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeOutRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </EventTrigger>
        </Rectangle.Triggers>
    </Rectangle>
    
    <!-- EaseInOut -->
    <Rectangle x:Name="easeInOutRect" Width="50" Height="20" Canvas.Top="130" Fill="Red">
        <Rectangle.Triggers>
            <EventTrigger RoutedEvent="Loaded">
                <BeginStoryboard>
                    <Storyboard RepeatBehavior="Forever" AutoReverse="True">
                        <DoubleAnimation Storyboard.TargetName="easeInOutRect"
                                         Storyboard.TargetProperty="(Canvas.Left)"
                                         From="0" To="350"
                                         Duration="0:0:1.5">
                            <DoubleAnimation.EasingFunction>
                                <QuadraticEase EasingMode="EaseInOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
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

✅ **Easing Functions** — управление плавностью анимации  
✅ **EaseIn** — медленное начало, ускорение  
✅ **EaseOut** — быстрое начало, замедление  
✅ **EaseInOut** — медленно → быстро → медленно  
✅ **Quadratic/Cubic/Quartic/Quintic** — степень кривой  
✅ **Bounce** — эффект отскока  
✅ **Elastic** — упругая пружина  
✅ **Back** — оттягивание назад  
✅ **PowerEase** — универсальная (заменяет Quadratic/Cubic/etc.)  
✅ **Circle/Sine/Exponential** — специальные функции

---

## Дополнительные ресурсы

- [Easing Functions](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.easingfunctionbase)
- [EasingFunctionBase](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.easingfunctionbase)
- [Easing Mode](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.easingmode)
