# Тема 7.4: Анимация в коде vs XAML

### Теория

**Анимацию в WPF** можно создавать двумя способами:
- **XAML** — декларативно, в разметке
- **Code-behind** — программно, в C#

#### Сравнение подходов

| Критерий | XAML | Code-behind |
|----------|------|-------------|
| **Читаемость** | Высокая | Средняя |
| **Гибкость** | Ограниченная | Полная |
| **Динамичность** | Статичная | Динамическая |
| **Повторное использование** | Ресурсы | Методы/классы |
| **Отладка** | Сложнее | Легче |
| **Designer support** | Да | Нет |
| **Производительность** | Одинаковая | Одинаковая |

### Примеры кода

#### Пример 1: Одна и та же анимация в XAML и Code

**XAML подход:**
```xml
<Rectangle x:Name="rect" Width="50" Height="50" Fill="Blue">
    <Rectangle.Triggers>
        <EventTrigger RoutedEvent="MouseLeftButtonDown">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetName="rect"
                                     Storyboard.TargetProperty="Width"
                                     To="200"
                                     Duration="0:0:0.5">
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

**Code-behind подход:**
```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        rect.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
    }

    private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var animation = new DoubleAnimation
        {
            To = 200,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        rect.BeginAnimation(Rectangle.WidthProperty, animation);
    }
}
```

#### Пример 2: Когда использовать Code-behind

**Сценарий 1: Динамические значения**
```csharp
public void AnimateToPosition(Point target)
{
    var xAnimation = new DoubleAnimation
    {
        To = target.X,
        Duration = TimeSpan.FromMilliseconds(300),
        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
    };
    
    var yAnimation = new DoubleAnimation
    {
        To = target.Y,
        Duration = TimeSpan.FromMilliseconds(300),
        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
    };
    
    myElement.BeginAnimation(Canvas.LeftProperty, xAnimation);
    myElement.BeginAnimation(Canvas.TopProperty, yAnimation);
}

// Использование при клике
private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
{
    var position = e.GetPosition(myCanvas);
    AnimateToPosition(position);
}
```

**Сценарий 2: Анимация на основе данных**
```csharp
public void AnimateValue(double from, double to, TimeSpan duration)
{
    var animation = new DoubleAnimation
    {
        From = from,
        To = to,
        Duration = new Duration(duration),
        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
    };
    
    myProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
}
```

**Сценарий 3: Управление анимацией**
```csharp
public partial class AnimationController : UserControl
{
    private Storyboard _currentStoryboard;

    public void StartAnimation()
    {
        _currentStoryboard = (Storyboard)FindResource("MyAnimation");
        _currentStoryboard.Completed += (s, e) =>
        {
            MessageBox.Show("Animation completed!");
        };
        _currentStoryboard.Begin();
    }

    public void PauseAnimation()
    {
        _currentStoryboard?.Pause();
    }

    public void ResumeAnimation()
    {
        _currentStoryboard?.Resume();
    }

    public void StopAnimation()
    {
        _currentStoryboard?.Stop();
    }

    public double GetProgress()
    {
        // Получение прогресса анимации
        return _currentStoryboard?.GetCurrentProgress() ?? 0;
    }
}
```

#### Пример 3: Когда использовать XAML

**Сценарий 1: Статичные анимации**
```xml
<Window.Resources>
    <Storyboard x:Key="LoadingStoryboard" RepeatBehavior="Forever">
        <DoubleAnimation Storyboard.TargetName="loadingCircle"
                         Storyboard.TargetProperty="(UIElement.RenderTransform).(RotateTransform.Angle)"
                         From="0" To="360"
                         Duration="0:0:1"/>
    </Storyboard>
</Window.Resources>

<Ellipse x:Name="loadingCircle" Width="50" Height="50" Fill="Blue">
    <Ellipse.RenderTransform>
        <RotateTransform CenterX="25" CenterY="25"/>
    </Ellipse.RenderTransform>
    <Ellipse.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard Storyboard="{StaticResource LoadingStoryboard}"/>
        </EventTrigger>
    </Ellipse.Triggers>
</Ellipse>
```

**Сценарий 2: Style анимации**
```xml
<Style x:Key="HoverButton" TargetType="Button">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border" Background="{TemplateBinding Background}" CornerRadius="5">
                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
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

**Сценарий 3: DataTrigger анимации**
```xml
<Rectangle x:Name="statusRect" Width="100" Height="20">
    <Rectangle.Style>
        <Style TargetType="Rectangle">
            <Setter Property="Fill" Value="Gray"/>
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsOnline}" Value="True">
                    <DataTrigger.EnterActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <ColorAnimation Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                                                To="Green"
                                                Duration="0:0:0.3"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </DataTrigger.EnterActions>
                    <DataTrigger.ExitActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <ColorAnimation Storyboard.TargetProperty="(Fill).(SolidColorBrush.Color)"
                                                To="Gray"
                                                Duration="0:0:0.3"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </DataTrigger.ExitActions>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Rectangle.Style>
</Rectangle>
```

#### Пример 4: Гибридный подход (XAML + Code)

```xml
<!-- XAML: определение Storyboard -->
<Window.Resources>
    <Storyboard x:Key="MoveAnimation">
        <DoubleAnimation x:Name="moveXAnim" 
                         Storyboard.TargetProperty="(Canvas.Left)"
                         Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation x:Name="moveYAnim" 
                         Storyboard.TargetProperty="(Canvas.Top)"
                         Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <QuadraticEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
</Window.Resources>

<Rectangle x:Name="myRect" Width="50" Height="50" Fill="Blue"/>
```

```csharp
// Code-behind: динамическая установка значений
public partial class MainWindow : Window
{
    public void MoveRectangleTo(double x, double y)
    {
        var storyboard = (Storyboard)FindResource("MoveAnimation");
        
        // Установка динамических значений
        moveXAnim.To = x;
        moveYAnim.To = y;
        
        storyboard.Begin(myRect, true);
    }
    
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(myCanvas);
        MoveRectangleTo(point.X, point.Y);
    }
}
```

#### Пример 5: Animation Helpers (переиспользуемый код)

```csharp
// AnimationHelpers.cs
public static class AnimationHelpers
{
    public static void FadeIn(this UIElement element, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(300);
        
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void FadeOut(this UIElement element, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(300);
        
        var animation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop
        };
        
        animation.Completed += (s, e) => element.Visibility = Visibility.Collapsed;
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void ScaleTo(this UIElement element, double scale, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(300);
        
        var transform = element.RenderTransform as ScaleTransform;
        if (transform == null)
        {
            transform = new ScaleTransform(1, 1);
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        
        var scaleXAnim = new DoubleAnimation
        {
            To = scale,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleYAnim = new DoubleAnimation
        {
            To = scale,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
        transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
    }
    
    public static void SlideInFromLeft(this UIElement element, double fromX = -100, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(400);
        
        element.Opacity = 0;
        var transform = new TranslateTransform(fromX, 0);
        element.RenderTransform = transform;
        
        var opacityAnim = new DoubleAnimation
        {
            To = 1,
            Duration = new Duration(duration)
        };
        
        var xAnim = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
    }
}

// Использование
public partial class MainWindow : Window
{
    private async void ShowPanel_Click(object sender, RoutedEventArgs e)
    {
        myPanel.Visibility = Visibility.Visible;
        myPanel.FadeIn();
        myPanel.SlideInFromLeft();
        
        await Task.Delay(500);
        
        myButton.ScaleTo(1.1);
    }
    
    private async void HidePanel_Click(object sender, RoutedEventArgs e)
    {
        myButton.ScaleTo(1);
        
        await Task.Delay(300);
        
        myPanel.FadeOut();
    }
}
```

#### Пример 6: Реальное использование из DotElectric

```csharp
// EditorCanvas.AnimationHelpers.cs
public partial class EditorCanvas
{
    /// <summary>
    /// Анимация панорамирования (XAML Storyboard + Code)
    /// </summary>
    public void AnimatePan(double targetX, double targetY, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(300);
        
        var storyboard = new Storyboard();
        
        var xAnim = new DoubleAnimation
        {
            From = PanTransform.X,
            To = targetX,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(xAnim, PanTransform);
        Storyboard.SetTargetProperty(xAnim, new PropertyPath(TranslateTransform.XProperty));
        storyboard.Children.Add(xAnim);
        
        var yAnim = new DoubleAnimation
        {
            From = PanTransform.Y,
            To = targetY,
            Duration = new Duration(duration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(yAnim, PanTransform);
        Storyboard.SetTargetProperty(yAnim, new PropertyPath(TranslateTransform.YProperty));
        storyboard.Children.Add(yAnim);
        
        storyboard.Begin();
    }
    
    /// <summary>
    /// Анимация zoom (Code-behind для динамических значений)
    /// </summary>
    public void AnimateZoom(double targetZoom, TimeSpan duration = default)
    {
        if (duration == default) duration = TimeSpan.FromMilliseconds(250);
        
        var animation = new DoubleAnimation
        {
            From = ZoomTransform.ScaleX,
            To = targetZoom,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        ZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        ZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }
    
    /// <summary>
    /// Анимация выделения объектов (XAML Resource + Code)
    /// </summary>
    public void AnimateSelectionAppear(UIElement element)
    {
        var storyboard = (Storyboard)FindResource("SelectionBounceAnimation");
        
        if (storyboard != null)
        {
            var clone = storyboard.Clone();
            element.BeginAnimation(UIElement.OpacityProperty, 
                ((DoubleAnimation)clone.Children[0]));
            
            var transform = element.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(0.8, 0.8);
                element.RenderTransform = transform;
            }
            
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, 
                ((DoubleAnimation)clone.Children[1]));
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, 
                ((DoubleAnimation)clone.Children[2]));
        }
    }
    
    /// <summary>
    /// Плавное скрытие элемента (Extension method)
    /// </summary>
    public async Task FadeOutAndRemoveAsync(UIElement element)
    {
        var animation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        
        var tcs = new TaskCompletionSource<bool>();
        
        animation.Completed += (s, e) =>
        {
            DrawingCanvas.Children.Remove(element);
            tcs.SetResult(true);
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
        await tcs.Task;
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 7.4.1: Code Animation**

Создайте анимацию в коде:
- DoubleAnimation для Width
- By click trigger
- Duration 0.5 секунды

**Задача 7.4.2: XAML Animation**

Создайте анимацию в XAML:
- Storyboard в ресурсах
- Запуск по Loaded
- RepeatBehavior=Forever

---

#### 🟡 Средний уровень (1 час)

**Задача 7.4.3: Hybrid Approach**

Создайте гибридную анимацию:
- Storyboard в XAML
- Динамические To значения в коде
- Запуск по клику

**Задача 7.4.4: Animation Helper**

Создайте extension method:
- FadeIn для UIElement
- QuadraticEase
- Настраиваемая duration

---

#### 🔴 Продвинутый уровень (1.5 часа)

**Задача 7.4.5: Animation Controller Class**

Создайте класс для управления:
- Start, Pause, Resume, Stop
- Progress tracking
- Completed event

**Задача 7.4.6: Full Animation Library**

Создайте библиотеку helpers:
- FadeIn, FadeOut
- ScaleTo, SlideInFrom
- RotateTo, Pulse
- Цепочка анимаций

---

### Решения

<details>
<summary>✅ Решение задачи 7.4.1</summary>

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        myRect.MouseLeftButtonDown += MyRect_MouseLeftButtonDown;
    }

    private void MyRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var animation = new DoubleAnimation
        {
            To = 300,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        myRect.BeginAnimation(Rectangle.WidthProperty, animation);
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 7.4.4</summary>

```csharp
public static class AnimationExtensions
{
    public static void FadeIn(this UIElement element, TimeSpan? duration = null)
    {
        var actualDuration = duration ?? TimeSpan.FromMilliseconds(300);
        
        element.Visibility = Visibility.Visible;
        
        var animation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 1,
            Duration = new Duration(actualDuration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void FadeOut(this UIElement element, TimeSpan? duration = null)
    {
        var actualDuration = duration ?? TimeSpan.FromMilliseconds(300);
        
        var animation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0,
            Duration = new Duration(actualDuration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
            FillBehavior = FillBehavior.Stop
        };
        
        animation.Completed += (s, e) =>
        {
            element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(UIElement.OpacityProperty, null);
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
}

// Использование
myPanel.FadeIn();
myPanel.FadeOut(TimeSpan.FromMilliseconds(500));
```
</details>

---

## Ключевые выводы

✅ **XAML** — для статичных, переиспользуемых анимаций  
✅ **Code-behind** — для динамических, управляемых анимаций  
✅ **Гибридный подход** — XAML Storyboard + Code значения  
✅ **BeginAnimation** — метод для анимации в коде  
✅ **Storyboard** — контейнер для организации анимаций  
✅ **Extension methods** — переиспользование анимаций  
✅ **EasingFunction** — плавность в коде и XAML  
✅ **Completed event** — обработка завершения

---

## Дополнительные ресурсы

- [BeginAnimation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.beginanimation)
- [Storyboard](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.animation.storyboard)
- [Animation Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/animation-overview)
