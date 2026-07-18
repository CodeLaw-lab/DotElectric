# Тема 4.5: RelativeSource, ElementName, FindAncestor (Advanced Binding)

### Теория

**Источники данных (Source)** в binding определяют, откуда брать значение.

#### Способы указания источника

| Способ | Синтаксис | Описание |
|--------|-----------|----------|
| **DataContext** | `{Binding Path=Name}` | Источник по умолчанию (DataContext) |
| **ElementName** | `{Binding ElementName=txt, Path=Text}` | Другой элемент по имени |
| **RelativeSource** | `{Binding RelativeSource={RelativeSource ...}}` | Относительно текущего элемента |
| **Source** | `{Binding Source={StaticResource ...}}` | Явный источник через ресурс |
| **x:Reference** | `{Binding Source={x:Reference ...}}` | Ссылка на элемент (альтернатива ElementName) |

#### RelativeSource Modes

| Mode | Описание | Пример |
|------|----------|--------|
| **Self** | Binding к самому себе | `{Binding Path=Width, RelativeSource={RelativeSource Self}}` |
| **TemplatedParent** | В ControlTemplate к родительскому контролу | `{Binding Path=Background, RelativeSource={RelativeSource TemplatedParent}}` |
| **FindAncestor** | Поиск родителя по типу | `{Binding Path=DataContext, RelativeSource={RelativeSource AncestorType=Window}}` |
| **FindAncestor** с уровнем | Поиск родителя с указанием уровня | `{Binding Path=..., RelativeSource={RelativeSource AncestorType=Grid, AncestorLevel=2}}` |

### Примеры кода

#### Пример 1: ElementName — binding к другому элементу

```xml
<StackPanel Margin="20">
    <!-- Slider для управления размером шрифта -->
    <Slider x:Name="fontSizeSlider" 
            Minimum="12" Maximum="48" Value="16"
            Margin="0,0,0,10"/>
    
    <!-- TextBlock с binding к Slider -->
    <TextBlock Text="Изменяемый текст"
               FontSize="{Binding Value, ElementName=fontSizeSlider}"/>
    
    <!-- TextBox с binding к другому TextBox -->
    <TextBox x:Name="sourceTextBox" 
             Text="Введите текст"
             Margin="0,10,0,5"/>
    
    <TextBlock Text="{Binding Text, ElementName=sourceTextBox}"
               FontWeight="Bold"/>
</StackPanel>
```

#### Пример 2: RelativeSource Self — binding к самому себе

```xml
<StackPanel Margin="20">
    <!-- Ширина зависит от высоты -->
    <Rectangle Height="100"
               Width="{Binding ActualHeight, RelativeSource={RelativeSource Self}}"
               Fill="Blue"/>
    
    <!-- Вращение кнопки -->
    <Button Content="Rotate me"
            Width="100" Height="50"
            Margin="0,10,0,0">
        <Button.RenderTransform>
            <RotateTransform Angle="{Binding ActualWidth, 
                                     RelativeSource={RelativeSource Self},
                                     Converter={StaticResource WidthToAngleConverter}}"/>
        </Button.RenderTransform>
    </Button>
    
    <!-- Прогресс бар с круговым индикатором -->
    <ProgressBar Value="50" 
                 Width="100" Height="100"
                 Margin="0,10,0,0">
        <ProgressBar.Style>
            <Style TargetType="ProgressBar">
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="ProgressBar">
                            <Border CornerRadius="50"
                                    BorderBrush="Gray"
                                    BorderThickness="2"
                                    Width="{Binding ActualWidth, 
                                            RelativeSource={RelativeSource TemplatedParent}}"
                                    Height="{Binding ActualHeight, 
                                             RelativeSource={RelativeSource TemplatedParent}}"/>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </ProgressBar.Style>
    </ProgressBar>
</StackPanel>
```

#### Пример 3: FindAncestor — поиск родителя

```xml
<Window x:Class="WpfApp.MainWindow">
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Menu с binding к Window.DataContext -->
        <Menu Grid.Row="0">
            <MenuItem Header="File">
                <!-- Binding к DataContext окна через AncestorType -->
                <MenuItem Header="New" 
                          Command="{Binding DataContext.NewCommand, 
                                    RelativeSource={RelativeSource AncestorType=Window}}"/>
                <MenuItem Header="Open"
                          Command="{Binding DataContext.OpenCommand, 
                                    RelativeSource={RelativeSource AncestorType=Window}}"/>
            </MenuItem>
        </Menu>
        
        <!-- Content -->
        <ContentControl Grid.Row="1"
                        Content="{Binding SelectedItem}"/>
    </Grid>
</Window>
```

#### Пример 4: AncestorLevel — указание уровня родителя

```xml
<Grid>
    <Grid>
        <Border BorderBrush="Blue" BorderThickness="1">
            <Border BorderBrush="Green" BorderThickness="1">
                <StackPanel>
                    <TextBlock Text="Вложенный текст"/>
                    
                    <!-- Binding к DataContext внешнего Grid (уровень 2) -->
                    <TextBlock Text="{Binding DataContext.Title, 
                                        RelativeSource={RelativeSource AncestorType=Grid, 
                                        AncestorLevel=2}}"/>
                    
                    <!-- Binding к DataContext внутреннего Border (уровень 1) -->
                    <TextBlock Text="{Binding DataContext.Subtitle, 
                                        RelativeSource={RelativeSource AncestorType=Border, 
                                        AncestorLevel=1}}"/>
                </StackPanel>
            </Border>
        </Border>
    </Grid>
</Grid>
```

#### Пример 5: TemplatedParent в ControlTemplate

```xml
<ControlTemplate TargetType="Button">
    <Border x:Name="border"
            Background="{TemplateBinding Background}"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}"
            CornerRadius="5">
        <!-- ContentPresenter с binding к содержимому кнопки -->
        <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                          VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                          Margin="{TemplateBinding Padding}"/>
    </Border>
    
    <ControlTemplate.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <!-- TemplateBinding эквивалентен RelativeSource={RelativeSource TemplatedParent} -->
            <Setter TargetName="border" Property="Background" 
                    Value="{TemplateBinding BorderBrush}"/>
        </Trigger>
        
        <Trigger Property="IsPressed" Value="True">
            <Setter TargetName="border" Property="Background" Value="#004080"/>
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>

<!-- Использование -->
<Button Content="Click me"
        Background="#0078D4"
        BorderBrush="#005A9E"
        BorderThickness="2"
        Padding="20,10"/>
```

#### Пример 6: x:Reference — альтернатива ElementName

```xml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Левая панель -->
    <ListBox x:Name="leftListBox" Grid.Column="0">
        <ListBoxItem Content="Item 1"/>
        <ListBoxItem Content="Item 2"/>
        <ListBoxItem Content="Item 3"/>
    </ListBox>
    
    <!-- Правая панель с binding через x:Reference -->
    <StackPanel Grid.Column="1" Margin="10,0,0,0">
        <TextBlock Text="Selected Item:" FontWeight="Bold"/>
        
        <!-- Binding к выбранному элементу через x:Reference -->
        <TextBlock Text="{Binding SelectedItem.Content, 
                            Source={x:Reference leftListBox}}"/>
        
        <TextBlock Text="Selected Index:" FontWeight="Bold" Margin="0,10,0,0"/>
        <TextBlock Text="{Binding SelectedIndex, 
                            Source={x:Reference leftListBox}}"/>
    </StackPanel>
</Grid>
```

#### Пример 7: Source с StaticResource

```xml
<Window.Resources>
    <!-- ViewModel как ресурс -->
    <local:MainViewModel x:Key="MainVM"/>
    
    <!-- Конвертер как ресурс -->
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
</Window.Resources>

<StackPanel Margin="20">
    <!-- Binding к ресурсу через Source -->
    <TextBlock Text="{Binding Title, Source={StaticResource MainVM}}"
               FontSize="20" FontWeight="Bold"/>
    
    <CheckBox Content="Is Visible"
              IsChecked="{Binding IsVisible, Source={StaticResource MainVM}}"
              Margin="0,10,0,0"/>
    
    <Image Source="/icon.png"
           Visibility="{Binding IsVisible, 
                        Source={StaticResource MainVM}, 
                        Converter={StaticResource BoolToVisibility}}"
           Margin="0,10,0,0"/>
</StackPanel>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- EditorCanvas.xaml -->
<UserControl x:Class="DotElectric.TemplateEditor.Views.EditorCanvas">
    
    <UserControl.Resources>
        <!-- Конвертеры -->
        <converters:ModelXToCanvasLeftConverter x:Key="ModelXToCanvasLeftConverter"/>
        <converters:ModelYToCanvasTopConverter x:Key="ModelYToCanvasTopConverter"/>
        <converters:MicronsToPixelConverter x:Key="MicronsToPixelConverter"/>
    </UserControl.Resources>
    
    <Canvas x:Name="DrawingCanvas"
            Background="White"
            ClipToBounds="True">
        
        <!-- RenderTransform для панорамирования -->
        <Canvas.RenderTransform>
            <TranslateTransform X="{Binding PanOffsetX}" 
                                Y="{Binding PanOffsetY}"/>
        </Canvas.RenderTransform>
        
        <!-- Размер Canvas = размер листа в пикселях -->
        <Canvas.Width>
            <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                <Binding Path="Template.Sheet.WidthMicrons"/>
                <Binding Path="Zoom" 
                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
            </MultiBinding>
        </Canvas.Width>
        
        <Canvas.Height>
            <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
                <Binding Path="Template.Sheet.HeightMicrons"/>
                <Binding Path="Zoom" 
                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
            </MultiBinding>
        </Canvas.Height>
        
        <!-- ItemsControl для объектов -->
        <ItemsControl ItemsSource="{Binding Template.Objects}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            
            <!-- Container для позиционирования -->
            <ItemsControl.ItemContainerStyle>
                <Style TargetType="ContentPresenter">
                    <Setter Property="Canvas.Left">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource ModelXToCanvasLeftConverter}">
                                <!-- Свойства объекта -->
                                <Binding Path="StartMicronsX"/>
                                <Binding Path="EndMicronsX"/>
                                <Binding Path="MicronsX"/>
                                <!-- Zoom из DataContext UserControl -->
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    
                    <Setter Property="Canvas.Top">
                        <Setter.Value>
                            <MultiBinding Converter="{StaticResource ModelYToCanvasTopConverter}">
                                <!-- Свойства объекта -->
                                <Binding Path="StartMicronsY"/>
                                <Binding Path="EndMicronsY"/>
                                <Binding Path="MicronsY"/>
                                <Binding Path="HeightMicrons"/>
                                <!-- Высота листа и Zoom из UserControl -->
                                <Binding Path="DataContext.Template.Sheet.HeightMm" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                                <Binding Path="DataContext.Zoom" 
                                         RelativeSource="{RelativeSource AncestorType=UserControl}"/>
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                </Style>
            </ItemsControl.ItemContainerStyle>
            
            <!-- Templates для разных типов объектов -->
            <ItemsControl.Resources>
                <DataTemplate DataType="{x:Type models:Line}">
                    <Line Stroke="Black" StrokeThickness="1">
                        <Line.Style>
                            <Style TargetType="Line">
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
                                <!-- Остальные координаты... -->
                            </Style>
                        </Line.Style>
                    </Line>
                </DataTemplate>
            </ItemsControl.Resources>
        </ItemsControl>
        
        <!-- Preview линии -->
        <Line x:Name="PreviewLineElement"
              Stroke="Red" 
              StrokeThickness="1.5" 
              StrokeDashArray="4,2"
              Visibility="{Binding ShowPreviewLine, 
                           Converter={StaticResource BoolToVisibility}}"/>
        
        <!-- Маркеры выделения -->
        <ContentControl Content="{Binding SingleSelectedObject}"
                        Visibility="{Binding ShowSelectionMarkers, 
                                     Converter={StaticResource BoolToVisibility}}"/>
    </Canvas>
</UserControl>
```

#### Пример 9: ComboBox с SelectedItem binding

```xml
<StackPanel Margin="20">
    <!-- Список людей -->
    <ListBox x:Name="peopleListBox" 
             ItemsSource="{Binding People}"
             SelectedItem="{Binding SelectedPerson}"
             DisplayMemberPath="Name"
             Width="200"/>
    
    <!-- Детали с binding к SelectedItem через ElementName -->
    <StackPanel Margin="20,0,0,0">
        <TextBlock Text="Selected Person Details:" FontWeight="Bold"/>
        
        <TextBlock Margin="0,10,0,0">
            <Run Text="Name: "/>
            <Run Text="{Binding SelectedItem.Name, ElementName=peopleListBox}"/>
        </TextBlock>
        
        <TextBlock Margin="0,5,0,0">
            <Run Text="Age: "/>
            <Run Text="{Binding SelectedItem.Age, ElementName=peopleListBox}"/>
        </TextBlock>
        
        <TextBlock Margin="0,5,0,0">
            <Run Text="Email: "/>
            <Run Text="{Binding SelectedItem.Email, ElementName=peopleListBox}"/>
        </TextBlock>
    </StackPanel>
</StackPanel>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 4.5.1: ElementName Binding**

Создайте Slider и TextBlock:
- Slider (Minimum=0, Maximum=100)
- TextBlock отображает значение Slider
- Используйте `{Binding ElementName=slider, Path=Value}`

**Задача 4.5.2: Self Binding**

Создайте квадратный Rectangle:
- Height=100
- Width binding к ActualHeight через RelativeSource Self

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 4.5.3: AncestorType Binding**

Создайте вложенную структуру:
- Window с DataContext (ViewModel с Title)
- Grid внутри Border внутри StackPanel
- TextBlock в глубине с binding к Window.DataContext.Title через AncestorType

**Задача 4.5.4: TemplatedParent**

Создайте ControlTemplate для Button:
- Border с Background из TemplateBinding
- ContentPresenter
- Trigger на IsMouseOver с изменением Background

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 4.5.5: Multi-Level Ancestor**

Создайте сложную вложенность:
- 3 уровня Grid с разными DataContext
- Элемент в глубине с binding к каждому уровню через AncestorLevel
- Демонстрация работы

**Задача 4.5.6: Dynamic Source Switcher**

Реализуйте переключение источника:
- ComboBox для выбора источника (ElementName, RelativeSource, StaticResource)
- TextBlock меняет binding в зависимости от выбора
- Используйте Attached Property или Behavior

---

### Решения

<details>
<summary>✅ Решение задачи 4.5.1</summary>

```xml
<StackPanel Margin="20">
    <Slider x:Name="valueSlider" 
            Minimum="0" Maximum="100" Value="50"
            Margin="0,0,0,10"/>
    
    <TextBlock>
        <Run Text="Current value: "/>
        <Run Text="{Binding Value, ElementName=valueSlider}"/>
    </TextBlock>
    
    <!-- Альтернатива с ProgressBar -->
    <ProgressBar Value="{Binding Value, ElementName=valueSlider}"
                 Height="20" Margin="0,10,0,0"/>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 4.5.2</summary>

```xml
<Grid>
    <!-- Квадратный Rectangle -->
    <Rectangle Height="100"
               Width="{Binding ActualHeight, RelativeSource={RelativeSource Self}}"
               Fill="LightBlue"
               Stroke="Blue"
               StrokeThickness="2"/>
    
    <!-- Квадратный Border -->
    <Border Height="150"
            Width="{Binding ActualHeight, RelativeSource={RelativeSource Self}}"
            BorderBrush="Green"
            BorderThickness="2"
            Margin="0,120,0,0">
        <TextBlock Text="Square Border"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"/>
    </Border>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 4.5.3</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfApp">
    
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>
    
    <!-- Вложенная структура -->
    <Border BorderBrush="Blue" BorderThickness="1" Margin="20">
        <StackPanel>
            <Grid>
                <Border BorderBrush="Green" BorderThickness="1">
                    <StackPanel>
                        <TextBlock Text="Deep nested text"/>
                        
                        <!-- Binding к Window.DataContext.Title -->
                        <TextBlock Text="{Binding Title, 
                                            RelativeSource={RelativeSource AncestorType=Window}}"
                                   FontWeight="Bold"
                                   Margin="0,10,0,0"/>
                        
                        <!-- Binding к Grid.DataContext (если бы был установлен) -->
                        <TextBlock Text="{Binding Subtitle, 
                                            RelativeSource={RelativeSource AncestorType=Grid}}"
                                   Foreground="Gray"
                                   Margin="0,5,0,0"/>
                    </StackPanel>
                </Border>
            </Grid>
        </StackPanel>
    </Border>
</Window>
```
</details>

---

## Ключевые выводы

✅ **ElementName** — binding к элементу по имени  
✅ **RelativeSource Self** — binding к самому себе  
✅ **RelativeSource TemplatedParent** — в ControlTemplate  
✅ **RelativeSource FindAncestor** — поиск родителя по типу  
✅ **AncestorLevel** — указание уровня родителя  
✅ **x:Reference** — альтернатива ElementName  
✅ **Source={StaticResource}** — binding к ресурсу  
✅ **TemplateBinding** — краткая форма для TemplatedParent

---

## Дополнительные ресурсы

- [Binding.Source](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.binding.source)
- [RelativeSource](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.relativesource)
- [ElementName](https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.elementname)
