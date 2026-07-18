# Тема 6.3: Geometry и Path-язык

### Теория

**Geometry** — описание формы без визуализации. **Path** — контрол для отображения Geometry.

#### Типы Geometry

| Тип | Описание | Пример |
|-----|----------|--------|
| **LineGeometry** | Линия | `M 0,0 L 100,100` |
| **RectangleGeometry** | Прямоугольник | `M 0,0 L 100,0 L 100,50 L 0,50 Z` |
| **EllipseGeometry** | Эллипс | `M 50,0 A 50,50 0 0,1 50,100 A 50,50 0 0,1 50,0` |
| **PathGeometry** | Произвольная форма | Комбинация фигур |
| **CombinedGeometry** | Комбинация геометрий | Union, Intersect, XOR |
| **GeometryGroup** | Группа геометрий | Несколько фигур |

#### Path-язык (Mini Language)

| Команда | Описание | Пример |
|---------|----------|--------|
| **M** x,y | Move to (переместить) | `M 10,10` |
| **L** x,y | Line to (линия к) | `L 100,100` |
| **C** x1,y1 x2,y2 x,y | Cubic Bezier curve | `C 50,0 100,100 150,50` |
| **Q** x1,y1 x,y | Quadratic Bezier curve | `Q 50,100 100,50` |
| **A** rx,ry angle largeArc sweep x,y | Arc (дуга) | `A 50,50 0 0,1 100,0` |
| **Z** | Close path (закрыть) | `Z` |

### Примеры кода

#### Пример 1: Простой Path с Line

```xml
<Canvas Width="400" Height="300">
    <!-- Треугольник -->
    <Path Data="M 100,50 L 200,200 L 50,200 Z"
          Fill="LightBlue"
          Stroke="Blue"
          StrokeThickness="2"/>
    
    <!-- Квадрат -->
    <Path Data="M 250,50 L 350,50 L 350,150 L 250,150 Z"
          Fill="LightGreen"
          Stroke="Green"/>
    
    <!-- Ломаная линия -->
    <Path Data="M 50,250 L 100,200 L 150,250 L 200,200 L 250,250"
          Stroke="Red"
          StrokeThickness="3"
          Fill="Transparent"/>
</Canvas>
```

#### Пример 2: Кривые Безье

```xml
<Canvas Width="400" Height="300">
    <!-- Cubic Bezier кривая -->
    <Path Data="M 50,150 C 100,50 200,250 250,150"
          Stroke="Blue"
          StrokeThickness="3"
          Fill="Transparent"/>
    
    <!-- Quadratic Bezier кривая -->
    <Path Data="M 50,200 Q 150,100 250,200"
          Stroke="Green"
          StrokeThickness="3"
          Fill="Transparent"/>
    
    <!-- Контрольные точки для Cubic -->
    <Ellipse Canvas.Left="95" Canvas.Top="145" Width="10" Height="10" Fill="Red"/>
    <Ellipse Canvas.Left="195" Canvas.Top="245" Width="10" Height="10" Fill="Red"/>
    
    <!-- Контрольная точка для Quadratic -->
    <Ellipse Canvas.Left="145" Canvas.Top="195" Width="10" Height="10" Fill="Orange"/>
</Canvas>
```

#### Пример 3: Дуги (Arc)

```xml
<Canvas Width="400" Height="300">
    <!-- Полуокружность -->
    <Path Data="M 50,150 A 100,100 0 0,1 250,150"
          Stroke="Blue"
          StrokeThickness="2"
          Fill="LightBlue"/>
    
    <!-- Другая полуокружность -->
    <Path Data="M 50,150 A 100,100 0 0,0 250,150"
          Stroke="Green"
          StrokeThickness="2"
          Fill="LightGreen"/>
    
    <!-- Кольцо (две дуги) -->
    <Path Data="M 300,75 A 50,50 0 0,1 300,175 A 50,50 0 0,1 300,75"
          Stroke="Red"
          StrokeThickness="2"
          Fill="Transparent"/>
    
    <!-- Pie slice -->
    <Path Data="M 300,125 L 350,75 A 50,50 0 0,1 350,175 Z"
          Fill="Orange"/>
</Canvas>
```

#### Пример 4: GeometryGroup

```xml
<Canvas Width="400" Height="300">
    <Path>
        <Path.Data>
            <GeometryGroup>
                <!-- Круг -->
                <EllipseGeometry Center="100,100" RadiusX="50" RadiusY="50"/>
                <!-- Прямоугольник -->
                <RectangleGeometry Rect="150,75 100,50"/>
                <!-- Линия -->
                <LineGeometry StartPoint="50,200" EndPoint="350,200"/>
                <!-- Ещё один круг -->
                <EllipseGeometry Center="300,100" RadiusX="30" RadiusY="30"/>
            </GeometryGroup>
        </Path.Data>
        <Path.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="Yellow" Offset="0.0"/>
                <GradientStop Color="Red" Offset="1.0"/>
            </LinearGradientBrush>
        </Path.Fill>
        <Path.Stroke>
            <SolidColorBrush Color="Black"/>
        </Path.Stroke>
        <Path.StrokeThickness>2</Path.StrokeThickness>
    </Path>
</Canvas>
```

#### Пример 5: CombinedGeometry

```xml
<Canvas Width="400" Height="300">
    <StackPanel Orientation="Horizontal">
        <!-- Union (объединение) -->
        <StackPanel>
            <TextBlock Text="Union"/>
            <Path>
                <Path.Data>
                    <CombinedGeometry GeometryCombineMode="Union">
                        <CombinedGeometry.Geometry1>
                            <EllipseGeometry Center="50,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry1>
                        <CombinedGeometry.Geometry2>
                            <EllipseGeometry Center="100,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry2>
                    </CombinedGeometry>
                </Path.Data>
                <Path.Fill>
                    <SolidColorBrush Color="LightBlue"/>
                </Path.Fill>
            </Path>
        </StackPanel>
        
        <!-- Intersect (пересечение) -->
        <StackPanel Margin="20,0,0,0">
            <TextBlock Text="Intersect"/>
            <Path>
                <Path.Data>
                    <CombinedGeometry GeometryCombineMode="Intersect">
                        <CombinedGeometry.Geometry1>
                            <EllipseGeometry Center="50,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry1>
                        <CombinedGeometry.Geometry2>
                            <EllipseGeometry Center="100,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry2>
                    </CombinedGeometry>
                </Path.Data>
                <Path.Fill>
                    <SolidColorBrush Color="LightGreen"/>
                </Path.Fill>
            </Path>
        </StackPanel>
        
        <!-- Exclude (исключение) -->
        <StackPanel Margin="20,0,0,0">
            <TextBlock Text="Exclude"/>
            <Path>
                <Path.Data>
                    <CombinedGeometry GeometryCombineMode="Exclude">
                        <CombinedGeometry.Geometry1>
                            <EllipseGeometry Center="50,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry1>
                        <CombinedGeometry.Geometry2>
                            <EllipseGeometry Center="100,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry2>
                    </CombinedGeometry>
                </Path.Data>
                <Path.Fill>
                    <SolidColorBrush Color="LightYellow"/>
                </Path.Fill>
            </Path>
        </StackPanel>
        
        <!-- XOR (исключающее ИЛИ) -->
        <StackPanel Margin="20,0,0,0">
            <TextBlock Text="XOR"/>
            <Path>
                <Path.Data>
                    <CombinedGeometry GeometryCombineMode="Xor">
                        <CombinedGeometry.Geometry1>
                            <EllipseGeometry Center="50,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry1>
                        <CombinedGeometry.Geometry2>
                            <EllipseGeometry Center="100,50" RadiusX="40" RadiusY="40"/>
                        </CombinedGeometry.Geometry2>
                    </CombinedGeometry>
                </Path.Data>
                <Path.Fill>
                    <SolidColorBrush Color="LightCoral"/>
                </Path.Fill>
            </Path>
        </StackPanel>
    </StackPanel>
</Canvas>
```

#### Пример 6: Сложная фигура (домик)

```xml
<Canvas Width="400" Height="300">
    <Path>
        <Path.Data>
            <GeometryGroup>
                <!-- Основание дома -->
                <RectangleGeometry Rect="100,150 200,150"/>
                <!-- Крыша -->
                <PathGeometry>
                    <PathGeometry.Figures>
                        <PathFigure StartPoint="80,150" IsClosed="True">
                            <LineSegment Point="200,50"/>
                            <LineSegment Point="320,150"/>
                        </PathFigure>
                    </PathGeometry.Figures>
                </PathGeometry>
                <!-- Дверь -->
                <RectangleGeometry Rect="175,220 50,80"/>
                <!-- Окно -->
                <RectangleGeometry Rect="120,180 40,40"/>
                <RectangleGeometry Rect="240,180 40,40"/>
            </GeometryGroup>
        </Path.Data>
        <Path.Fill>
            <LinearGradientBrush>
                <GradientStop Color="LightYellow" Offset="0.0"/>
                <GradientStop Color="Orange" Offset="1.0"/>
            </LinearGradientBrush>
        </Path.Fill>
        <Path.Stroke>
            <SolidColorBrush Color="Brown"/>
        </Path.Stroke>
        <Path.StrokeThickness>2</Path.StrokeThickness>
    </Path>
</Canvas>
```

#### Пример 7: Path с StreamGeometry (производительность)

```xml
<!-- StreamGeometry легче чем PathGeometry -->
<Path>
    <Path.Data>
        <StreamGeometry>
            M 50,100 
            C 50,50 100,0 150,50 
            C 200,100 250,100 250,50
            L 250,150
            C 250,200 200,250 150,200
            C 100,150 50,150 50,100 Z
        </StreamGeometry>
    </Path.Data>
    <Path.Fill>
        <SolidColorBrush Color="LightBlue"/>
    </Path.Fill>
    <Path.Stroke>
        <SolidColorBrush Color="Blue"/>
    </Path.Stroke>
</Path>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml — стрелки и маркеры -->
<UserControl.Resources>
    <!-- Стрелка для Line -->
    <Geometry x:Key="ArrowGeometry">M 0,0 L 10,5 L 10,-5 Z</Geometry>
    
    <!-- Маркер выделения (квадрат) -->
    <Geometry x:Key="SelectionMarkerGeometry">M 0,0 L 6,0 L 6,6 L 0,6 Z</Geometry>
    
    <!-- Маркер выделения (круг) -->
    <Geometry x:Key="CircleMarkerGeometry">
        M 3,0 A 3,3 0 0,1 3,6 A 3,3 0 0,1 3,0
    </Geometry>
    
    <!-- Шаблон для Line с маркерами -->
    <DataTemplate DataType="{x:Type models:Line}">
        <Canvas>
            <!-- Основная линия -->
            <Line X1="{Binding StartX}" Y1="{Binding StartY}"
                  X2="{Binding EndX}" Y2="{Binding EndY}"
                  Stroke="{Binding StrokeColor}"
                  StrokeThickness="{Binding StrokeThickness}"
                  StrokeDashArray="{Binding LineType, Converter={StaticResource LineTypeToDashArray}}"/>
            
            <!-- Маркеры на концах (при выделении) -->
            <Path Data="{StaticResource CircleMarkerGeometry}"
                  Canvas.Left="{Binding EndX, Converter={StaticResource OffsetConverter}}"
                  Canvas.Top="{Binding EndY, Converter={StaticResource OffsetConverter}}"
                  Fill="White"
                  Stroke="#0078D4"
                  StrokeThickness="1"
                  Visibility="{Binding IsSelected, Converter={StaticResource BoolToVisibility}}"/>
        </Canvas>
    </DataTemplate>
    
    <!-- Шаблон для Rectangle с маркерами -->
    <DataTemplate DataType="{x:Type models:Rectangle}">
        <Canvas>
            <!-- Основной прямоугольник -->
            <Rectangle Canvas.Left="{Binding X}" Canvas.Top="{Binding Y}"
                       Width="{Binding Width}" Height="{Binding Height}"
                       Stroke="{Binding StrokeColor}"
                       StrokeThickness="{Binding StrokeThickness}"
                       Fill="Transparent"/>
            
            <!-- Маркеры выделения (8 штук) -->
            <Canvas Visibility="{Binding IsSelected, Converter={StaticResource BoolToVisibility}}">
                <!-- Углы -->
                <Path Data="{StaticResource SelectionMarkerGeometry}"
                      Canvas.Left="{Binding X, Converter={StaticResource MarkerOffsetConverter}, ConverterParameter=-3}"
                      Canvas.Top="{Binding Y, Converter={StaticResource MarkerOffsetConverter}, ConverterParameter=-3}"
                      Fill="White" Stroke="#0078D4"/>
                <Path Data="{StaticResource SelectionMarkerGeometry}"
                      Canvas.Left="{Binding X, Converter={StaticResource AddWidthConverter}}"
                      Canvas.Top="{Binding Y, Converter={StaticResource MarkerOffsetConverter}, ConverterParameter=-3}"
                      Fill="White" Stroke="#0078D4"/>
                <!-- ... остальные 6 маркеров ... -->
            </Canvas>
        </Canvas>
    </DataTemplate>
</UserControl.Resources>
```

```csharp
// Конвертеры для позиционирования маркеров
public class MarkerOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d && parameter is string p)
        {
            double offset = double.Parse(p);
            return d + offset;
        }
        return value;
    }

    public object ConvertBack(...) => throw new NotImplementedException();
}

public class AddWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double x && parameter is RectangleViewModel rect)
        {
            return x + rect.Width - 3; // -3 для центровки маркера
        }
        return value;
    }

    public object ConvertBack(...) => throw new NotImplementedException();
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 6.3.1: Простой Path**

Нарисуйте Path с:
- Треугольник (M, L, Z)
- Квадрат (M, L, Z)
- Буква "L" (M, L, L)

**Задача 6.3.2: Arc дуги**

Нарисуйте:
- Полуокружность (A с largeArc=0)
- Полная окружность (две дуги)
- Pie slice (дуга + линии)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 6.3.3: Кривые Безье**

Нарисуйте:
- Cubic Bezier (C команда)
- Quadratic Bezier (Q команда)
- S-образная кривая (две C)

**Задача 6.3.4: Логотип**

Создайте логотип используя:
- GeometryGroup
- RectangleGeometry, EllipseGeometry
- Градиентная заливка

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 6.3.5: Векторная иконка**

Нарисуйте сложную иконку:
- Дом, дерево, солнце
- Только Path-язык
- Единый стиль

**Задача 6.3.6: Анимированный Path**

Создайте анимацию:
- PathGeometry анимация (PointAnimation)
- Изменение формы в реальном времени
- Storyboard

---

### Решения

<details>
<summary>✅ Решение задачи 6.3.1</summary>

```xml
<Canvas Width="400" Height="200">
    <!-- Треугольник -->
    <Path Data="M 50,150 L 100,50 L 150,150 Z"
          Fill="LightBlue"
          Stroke="Blue"
          StrokeThickness="2"/>
    
    <!-- Квадрат -->
    <Path Data="M 200,50 L 300,50 L 300,150 L 200,150 Z"
          Fill="LightGreen"
          Stroke="Green"/>
    
    <!-- Буква L -->
    <Path Data="M 350,50 L 350,150 L 400,150"
          Stroke="Red"
          StrokeThickness="5"
          Fill="Transparent"/>
</Canvas>
```
</details>

<details>
<summary>✅ Решение задачи 6.3.3</summary>

```xml
<Canvas Width="400" Height="200">
    <!-- Cubic Bezier -->
    <Path Data="M 20,100 C 80,0 160,200 220,100"
          Stroke="Blue"
          StrokeThickness="3"
          Fill="Transparent"/>
    
    <!-- Quadratic Bezier -->
    <Path Data="M 20,150 Q 120,50 220,150"
          Stroke="Green"
          StrokeThickness="3"
          Fill="Transparent"/>
    
    <!-- S-образная кривая -->
    <Path Data="M 250,100 C 280,50 320,150 350,100"
          Stroke="Red"
          StrokeThickness="3"
          Fill="Transparent"/>
</Canvas>
```
</details>

---

## Ключевые выводы

✅ **Path-язык** — компактный способ описания геометрии  
✅ **M** — перемещение, **L** — линия, **Z** — закрыть  
✅ **C** — Cubic Bezier (2 контрольные точки)  
✅ **Q** — Quadratic Bezier (1 контрольная точка)  
✅ **A** — дуга (rx, ry, angle, largeArc, sweep)  
✅ **GeometryGroup** — группа нескольких геометрий  
✅ **CombinedGeometry** — комбинация с режимами (Union, Intersect, Exclude, XOR)  
✅ **StreamGeometry** — лёгкая альтернатива PathGeometry

---

## Дополнительные ресурсы

- [Path Markup Syntax](https://docs.microsoft.com/en-us/dotnet/api/system.windows.markup.path-markup-syntax)
- [Geometry](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.geometry)
- [Path.Data](https://docs.microsoft.com/en-us/dotnet/api/system.windows.shapes.path.data)
