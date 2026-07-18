# Тема 6.1: Shapes (Line, Rectangle, Ellipse, Path)

### Теория

**Shape** — базовый класс для графических примитивов в WPF.

#### Основные Shapes

| Shape | Описание | Свойства |
|-------|----------|----------|
| **Line** | Линия между двумя точками | X1, Y1, X2, Y2 |
| **Rectangle** | Прямоугольник | Width, Height, RadiusX, RadiusY |
| **Ellipse** | Эллипс/круг | Width, Height |
| **Polygon** | Многоугольник (замкнутый) | Points |
| **Polyline** | Ломаная линия (не замкнутая) | Points |
| **Path** | Произвольная форма | Data |

#### Общие свойства

```xml
<Shape Stroke="Black"           <!-- Цвет линии -->
       StrokeThickness="2"      <!-- Толщина линии -->
       Fill="Blue"              <!-- Цвет заполнения -->
       StrokeDashArray="4,2"    <!-- Пунктирная линия -->
       Stretch="Fill"/>         <!-- Растягивание -->
```

### Примеры кода

#### Пример 1: Line — простая линия

```xml
<Canvas Width="400" Height="300">
    <!-- Горизонтальная линия -->
    <Line X1="0" Y1="50" X2="400" Y2="50"
          Stroke="Black" StrokeThickness="1"/>
    
    <!-- Вертикальная линия -->
    <Line X1="100" Y1="0" X2="100" Y2="300"
          Stroke="Black" StrokeThickness="1"/>
    
    <!-- Диагональная линия -->
    <Line X1="0" Y1="0" X2="400" Y2="300"
          Stroke="Red" StrokeThickness="3"/>
    
    <!-- Пунктирная линия -->
    <Line X1="50" Y1="100" X2="350" Y2="100"
          Stroke="Blue" StrokeThickness="2"
          StrokeDashArray="4,2"/>
    
    <!-- Штрих-пунктирная -->
    <Line X1="50" Y1="150" X2="350" Y2="150"
          Stroke="Green" StrokeThickness="2"
          StrokeDashArray="10,2,2,2"/>
</Canvas>
```

#### Пример 2: Rectangle — прямоугольник

```xml
<Canvas Width="400" Height="300">
    <!-- Простой прямоугольник -->
    <Rectangle Width="100" Height="60"
               Canvas.Left="50" Canvas.Top="50"
               Fill="LightBlue"
               Stroke="Blue"
               StrokeThickness="2"/>
    
    <!-- Закруглённый прямоугольник -->
    <Rectangle Width="100" Height="60"
               Canvas.Left="200" Canvas.Top="50"
               Fill="LightGreen"
               Stroke="Green"
               StrokeThickness="2"
               RadiusX="10"
               RadiusY="10"/>
    
    <!-- Прямоугольник с градиентом -->
    <Rectangle Width="100" Height="60"
               Canvas.Left="50" Canvas.Top="150">
        <Rectangle.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="Yellow" Offset="0.0"/>
                <GradientStop Color="Red" Offset="1.0"/>
            </LinearGradientBrush>
        </Rectangle.Fill>
    </Rectangle>
    
    <!-- Полупрозрачный прямоугольник -->
    <Rectangle Width="100" Height="60"
               Canvas.Left="200" Canvas.Top="150"
               Fill="#80FF0000"
               Stroke="Red"/>
</Canvas>
```

#### Пример 3: Ellipse — эллипс и круг

```xml
<Canvas Width="400" Height="300">
    <!-- Круг (равные ширина и высота) -->
    <Ellipse Width="80" Height="80"
             Canvas.Left="50" Canvas.Top="50"
             Fill="LightBlue"
             Stroke="Blue"
             StrokeThickness="2"/>
    
    <!-- Эллипс -->
    <Ellipse Width="120" Height="60"
             Canvas.Left="180" Canvas.Top="60"
             Fill="LightGreen"
             Stroke="Green"/>
    
    <!-- Кольцо -->
    <Ellipse Width="80" Height="80"
             Canvas.Left="50" Canvas.Top="180"
             Fill="Transparent"
             Stroke="Red"
             StrokeThickness="5"/>
    
    <!-- Концентрические круги -->
    <Canvas Canvas.Left="250" Canvas.Top="150">
        <Ellipse Width="100" Height="100" Fill="LightGray" Stroke="Gray"/>
        <Ellipse Width="70" Height="70" Fill="LightBlue" Stroke="Blue"/>
        <Ellipse Width="40" Height="40" Fill="White" Stroke="Navy"/>
    </Canvas>
</Canvas>
```

#### Пример 4: Polygon — многоугольник

```xml
<Canvas Width="400" Height="300">
    <!-- Треугольник -->
    <Polygon Points="100,50 150,150 50,150"
             Fill="LightBlue"
             Stroke="Blue"
             StrokeThickness="2"/>
    
    <!-- Квадрат -->
    <Polygon Points="200,50 250,50 250,100 200,100"
             Fill="LightGreen"
             Stroke="Green"/>
    
    <!-- Пятиугольник -->
    <Polygon Points="125,200 175,220 155,280 95,280 75,220"
             Fill="LightYellow"
             Stroke="Orange"
             StrokeThickness="2"/>
    
    <!-- Звезда -->
    <Polygon Points="225,180 240,220 285,220 250,245 260,285 
                     225,265 190,285 200,245 165,220 210,220"
             Fill="Gold"
             Stroke="Orange"
             StrokeThickness="2"/>
</Canvas>
```

#### Пример 5: Polyline — ломаная линия

```xml
<Canvas Width="400" Height="300">
    <!-- Простая ломаная -->
    <Polyline Points="50,50 100,100 150,50 200,100 250,50"
              Stroke="Blue"
              StrokeThickness="2"
              Fill="Transparent"/>
    
    <!-- График -->
    <Polyline Points="50,250 100,200 150,220 200,150 250,180 300,100"
              Stroke="Red"
              StrokeThickness="3"
              Fill="Transparent"/>
    
    <!-- Замкнутая ломаная (как Polygon) -->
    <Polyline Points="100,50 150,100 100,150 50,100 100,50"
              Stroke="Green"
              StrokeThickness="2"
              Fill="LightGreen"/>
</Canvas>
```

#### Пример 6: Path — произвольная форма

```xml
<Canvas Width="400" Height="300">
    <!-- Простой путь -->
    <Path Data="M 50,50 L 100,50 L 100,100 L 50,100 Z"
          Fill="LightBlue"
          Stroke="Blue"
          StrokeThickness="2"/>
    
    <!-- Кривая Безье -->
    <Path Data="M 50,150 C 100,100 150,200 200,150"
          Stroke="Red"
          StrokeThickness="3"
          Fill="Transparent"/>
    
    <!-- Дуга -->
    <Path Data="M 50,200 A 50,50 0 0,1 150,200"
          Stroke="Green"
          StrokeThickness="2"
          Fill="Transparent"/>
    
    <!-- Сложная фигура -->
    <Path Data="M 250,50 L 275,100 L 330,100 L 290,140 L 305,195 
                 L 250,165 L 195,195 L 210,140 L 170,100 L 225,100 Z"
          Fill="Gold"
          Stroke="Orange"
          StrokeThickness="2"/>
</Canvas>
```

#### Пример 7: Stretch и выравнивание

```xml
<Grid Width="400" Height="300">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- None -- оригинальный размер -->
    <Border Grid.Column="0" BorderBrush="Gray" BorderThickness="1">
        <Ellipse Width="50" Height="50" Fill="Blue" Stretch="None"/>
    </Border>
    
    <!-- Fill -- заполняет всё пространство -->
    <Border Grid.Column="1" BorderBrush="Gray" BorderThickness="1">
        <Ellipse Fill="Green" Stretch="Fill"/>
    </Border>
    
    <!-- Uniform -- пропорционально -->
    <Border Grid.Column="2" BorderBrush="Gray" BorderThickness="1">
        <Ellipse Fill="Red" Stretch="Uniform"/>
    </Border>
    
    <!-- UniformToFill -- пропорционально с обрезкой -->
    <Border Grid.Column="3" BorderBrush="Gray" BorderThickness="1">
        <Ellipse Fill="Orange" Stretch="UniformToFill"/>
    </Border>
</Grid>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml -- отрисовка объектов шаблона -->
<UserControl.Resources>
    <!-- Шаблон для Line -->
    <DataTemplate DataType="{x:Type models:Line}">
        <Line>
            <Line.Style>
                <Style TargetType="Line">
                    <Setter Property="Stroke" Value="Black"/>
                    <Setter Property="StrokeThickness" Value="1"/>
                    
                    <!-- Привязка координат через конвертеры -->
                    <Setter Property="X1">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource LocalX1Converter}">
                                <Binding Path="StartMicronsX"/>
                                <Binding Path="EndMicronsX"/>
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    <Setter Property="Y1">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource LocalY1Converter}">
                                <Binding Path="StartMicronsY"/>
                                <Binding Path="EndMicronsY"/>
                                <Binding Path="DataContext.Template.Sheet.HeightMm" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    <Setter Property="X2">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource LocalX2Converter}">
                                <Binding Path="StartMicronsX"/>
                                <Binding Path="EndMicronsX"/>
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    <Setter Property="Y2">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource LocalY2Converter}">
                                <Binding Path="StartMicronsY"/>
                                <Binding Path="EndMicronsY"/>
                                <Binding Path="DataContext.Template.Sheet.HeightMm" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    
                    <!-- Тип линии (сплошная, пунктирная) -->
                    <Setter Property="StrokeDashArray" 
                            Value="{Binding LineType, 
                                    Converter={StaticResource LineTypeToDashArrayConverter}}"/>
                    
                    <!-- Hover эффект -->
                    <Style.Triggers>
                        <DataTrigger Value="True">
                            <DataTrigger.Binding>
                                <MultiBinding Converter="{StaticResource IsHoveredConverter}">
                                    <Binding/>
                                    <Binding Path="DataContext.HoveredObject" 
                                             RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                                </MultiBinding>
                            </DataTrigger.Binding>
                            <Setter Property="StrokeThickness" Value="2.5"/>
                            <Setter Property="Stroke" Value="#333"/>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </Line.Style>
        </Line>
    </DataTemplate>
    
    <!-- Шаблон для Rectangle -->
    <DataTemplate DataType="{x:Type models:Rectangle}">
        <Rectangle Stroke="Black" Fill="Transparent" StrokeThickness="1">
            <Rectangle.Width>
                <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                    <Binding Path="WidthMicrons"/>
                    <Binding Path="DataContext.Zoom" 
                             RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                </MultiBinding>
            </Rectangle.Width>
            <Rectangle.Height>
                <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                    <Binding Path="HeightMicrons"/>
                    <Binding Path="DataContext.Zoom" 
                             RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                </MultiBinding>
            </Rectangle.Height>
            <Rectangle.Style>
                <Style TargetType="Rectangle">
                    <Setter Property="StrokeDashArray" 
                            Value="{Binding LineType, 
                                    Converter={StaticResource LineTypeToDashArrayConverter}}"/>
                </Style>
            </Rectangle.Style>
        </Rectangle>
    </DataTemplate>
    
    <!-- Шаблон для Ellipse (маркеры выделения) -->
    <DataTemplate DataType="{x:Type models:SelectionMarker}">
        <Ellipse Width="6" Height="6"
                 Fill="White"
                 Stroke="#0078D4"
                 StrokeThickness="1"/>
    </DataTemplate>
</UserControl.Resources>

<!-- Canvas для отрисовки -->
<Canvas x:Name="DrawingCanvas" Background="White">
    <ItemsControl ItemsSource="{Binding Template.Objects}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
    </ItemsControl>
</Canvas>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 6.1.1: Простые фигуры**

Создайте Canvas с:
- Line (диагональ)
- Rectangle (100x50)
- Ellipse (круг 80x80)

**Задача 6.1.2: Пунктирные линии**

Создайте 3 линии с разными StrokeDashArray:
- Пунктирная (4,2)
- Штрих-пунктирная (10,2,2,2)
- Точки (2,2)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 6.1.3: Домик**

Нарисуйте домик используя:
- Polygon для крыши (треугольник)
- Rectangle для основания
- Rectangle для двери
- Circle для окна

**Задача 6.1.4: График функции**

Нарисуйте график синуса:
- Polyline с точками sin(x)
- Оси координат (Line)
- Подписи (TextBlock)

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 6.1.5: Логотип компании**

Создайте сложный логотип:
- Комбинация Rectangle, Ellipse, Path
- Градиентная заливка
- Трансформации

**Задача 6.1.6: Векторная иконка**

Нарисуйте иконку используя Path:
- Дом, пользователь, настройки
- Path-язык (M, L, C, A)
- Единый стиль (толщина, цвета)

---

### Решения

<details>
<summary>✅ Решение задачи 6.1.1</summary>

```xml
<Canvas Width="400" Height="300" Background="White">
    <!-- Диагональная линия -->
    <Line X1="0" Y1="0" X2="400" Y2="300"
          Stroke="Black" StrokeThickness="2"/>
    
    <!-- Прямоугольник -->
    <Rectangle Width="100" Height="50"
               Canvas.Left="150" Canvas.Top="125"
               Fill="LightBlue"
               Stroke="Blue"
               StrokeThickness="2"/>
    
    <!-- Круг -->
    <Ellipse Width="80" Height="80"
             Canvas.Left="160" Canvas.Top="200"
             Fill="LightGreen"
             Stroke="Green"
             StrokeThickness="2"/>
</Canvas>
```
</details>

<details>
<summary>✅ Решение задачи 6.1.3</summary>

```xml
<Canvas Width="300" Height="250" Background="LightCyan">
    <!-- Небо -->
    <Ellipse Width="60" Height="60"
             Canvas.Left="200" Canvas.Top="30"
             Fill="Yellow" Stroke="Orange"/>
    
    <!-- Дом - основание -->
    <Rectangle Width="150" Height="100"
               Canvas.Left="75" Canvas.Top="120"
               Fill="LightYellow"
               Stroke="Brown"
               StrokeThickness="2"/>
    
    <!-- Крыша -->
    <Polygon Points="65,120 150,50 235,120"
             Fill="LightCoral"
             Stroke="Brown"
             StrokeThickness="2"/>
    
    <!-- Дверь -->
    <Rectangle Width="40" Height="60"
               Canvas.Left="130" Canvas.Top="160"
               Fill="Brown"
               Stroke="DarkBrown"/>
    
    <!-- Окно -->
    <Ellipse Width="40" Height="40"
             Canvas.Left="70" Canvas.Top="140"
             Fill="LightBlue"
             Stroke="Blue"
             StrokeThickness="2"/>
</Canvas>
```
</details>

---

## Ключевые выводы

✅ **Line** — линия между двумя точками (X1,Y1,X2,Y2)  
✅ **Rectangle** — прямоугольник с опциональным RadiusX/Y  
✅ **Ellipse** — эллипс (круг при равных Width/Height)  
✅ **Polygon** — замкнутый многоугольник (Points)  
✅ **Polyline** — ломаная линия (не замкнутая)  
✅ **Path** — произвольная форма через Data  
✅ **StrokeDashArray** — пунктирные линии  
✅ **Stretch** — None, Fill, Uniform, UniformToFill

---

## Дополнительные ресурсы

- [Shapes](https://docs.microsoft.com/en-us/dotnet/api/system.windows.shapes.shape)
- [Line](https://docs.microsoft.com/en-us/dotnet/api/system.windows.shapes.line)
- [Path Geometry](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.pathgeometry)
