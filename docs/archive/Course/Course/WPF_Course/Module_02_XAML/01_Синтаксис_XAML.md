# Тема 2.1: Синтаксис XAML

### Теория

**XAML (Extensible Application Markup Language)** — декларативный язык разметки на основе XML для создания UI в WPF.

#### Базовый синтаксис

```xml
<!-- Элемент с атрибутами -->
<Button Content="Click me" 
        Width="100" 
        Height="30"/>

<!-- Элемент с вложенными элементами -->
<Button>
    <Button.Content>
        <StackPanel Orientation="Horizontal">
            <Image Source="/icon.png" Width="16"/>
            <TextBlock Text="Click me" Margin="5,0,0,0"/>
        </StackPanel>
    </Button.Content>
</Button>
```

#### Пространства имён (xmlns)

```xml
<Window 
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:MyApp"
    xmlns:controls="clr-namespace:MyApp.Controls;assembly=MyApp.Controls">
    
    <!-- Стандартные контролы WPF -->
    <Button Content="Standard"/>
    
    <!-- Контролы с x-префиксом -->
    <Button x:Name="myButton"/>
    
    <!-- Локальные контролы -->
    <local:CustomControl/>
    
    <!-- Контролы из другой сборки -->
    <controls:AdvancedButton/>
</Window>
```

#### Content Property

У многих контролов есть **свойство содержимого по умолчанию**:

```xml
<!-- Эти записи эквивалентны -->
<Button Content="Click me"/>
<Button>
    <Button.Content>
        <TextBlock Text="Click me"/>
    </Button.Content>
</Button>

<!-- ContentProperty может быть любым объектом -->
<Button>
    <StackPanel Orientation="Horizontal">
        <Image Source="/icon.png"/>
        <TextBlock Text="Click me"/>
    </StackPanel>
</Button>
```

**ContentProperty атрибут:**
```csharp
[ContentProperty("Content")]
public class Button : Control
{
    public object Content { get; set; }
}
```

#### Collection Syntax

**Явный синтаксис:**
```xml
<ListBox>
    <ListBox.Items>
        <ListBoxItem Content="Item 1"/>
        <ListBoxItem Content="Item 2"/>
        <ListBoxItem Content="Item 3"/>
    </ListBox.Items>
</ListBox>
```

**Неявный синтаксис (короче):**
```xml
<ListBox>
    <ListBoxItem Content="Item 1"/>
    <ListBoxItem Content="Item 2"/>
    <ListBoxItem Content="Item 3"/>
</ListBox>
```

**Коллекции примитивов:**
```xml
<ComboBox>
    <sys:String>Первый</sys:String>
    <sys:String>Второй</sys:String>
    <sys:String>Третий</sys:String>
</ComboBox>
```

#### Type Converters

WPF автоматически конвертирует строки в нужные типы:

```xml
<!-- string → CornerRadius -->
<Border CornerRadius="10"/>
<Border CornerRadius="10,20,10,20"/>

<!-- string → Thickness -->
<Border BorderThickness="5"/>
<Border BorderThickness="1,2,3,4"/>

<!-- string → Color -->
<Border Background="Red"/>
<Border Background="#0078D4"/>
<Border Background="rgb(0,120,212)"/>

<!-- string → DateTime -->
<local:DateControl SelectedDate="2026-04-22"/>

<!-- string → Enum -->
<Button HorizontalAlignment="Center"/>
<TextBlock FontWeight="Bold"/>
```

#### Property Element Syntax

Когда значение нельзя задать строкой:

```xml
<!-- Сложное значение свойства -->
<Rectangle>
    <Rectangle.Fill>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
            <GradientStop Color="Yellow" Offset="0.0"/>
            <GradientStop Color="Red" Offset="0.25"/>
            <GradientStop Color="Blue" Offset="0.75"/>
            <GradientStop Color="Green" Offset="1.0"/>
        </LinearGradientBrush>
    </Rectangle.Fill>
</Rectangle>

<!-- Коллекция трансформаций -->
<Button>
    <Button.RenderTransform>
        <TransformGroup>
            <ScaleTransform ScaleX="1.5" ScaleY="1.5"/>
            <RotateTransform Angle="45"/>
        </TransformGroup>
    </Button.RenderTransform>
</Button>
```

#### Attached Properties

Свойства, определённые одним классом, но используемые на другом:

```xml
<Grid>
    <!-- Grid.Row и Grid.Column определены в Grid -->
    <Button Content="OK" Grid.Row="0" Grid.Column="1"/>
    <Button Content="Cancel" Grid.Row="1" Grid.Column="0"/>
</Grid>

<Canvas>
    <!-- Canvas.Left и Canvas.Top определены в Canvas -->
    <Button Content="Click" Canvas.Left="100" Canvas.Top="50"/>
</Canvas>

<!-- Синтаксис в коде -->
Grid.SetRow(button, 0);
Grid.SetColumn(button, 1);
```

#### Name Scope (x:Name)

```xml
<Window>
    <StackPanel>
        <Button x:Name="myButton" Content="Click me"/>
        <TextBox x:Name="myTextBox"/>
    </StackPanel>
</Window>
```

```csharp
// Доступ в code-behind
myButton.Content = "New text";
var text = myTextBox.Text;

// Поиск элемента по имени в runtime
var element = this.FindName("myButton") as Button;
```

#### x:Key и ресурсы

```xml
<Window.Resources>
    <!-- Ресурс с ключом -->
    <SolidColorBrush x:Key="MyBrush" Color="#0078D4"/>
    
    <!-- Использование -->
    <Button Background="{StaticResource MyBrush}"/>
</Window.Resources>
```

### Примеры кода

#### Пример 1: Полное окно с разными синтаксисами

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfApp"
        Title="XAML Syntax Demo" 
        Height="600" Width="800"
        Background="White">
    
    <Grid>
        <!-- Grid с определениями строк и колонок -->
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Menu с collection syntax -->
        <Menu Grid.Row="0" Grid.ColumnSpan="2">
            <MenuItem Header="_Файл">
                <MenuItem Header="_Новый" InputGestureText="Ctrl+N"/>
                <MenuItem Header="_Открыть" InputGestureText="Ctrl+O"/>
                <Separator/>
                <MenuItem Header="Вы_ход" InputGestureText="Alt+F4"/>
            </MenuItem>
            <MenuItem Header="_Правка"/>
            <MenuItem Header="_Справка"/>
        </Menu>

        <!-- Левая панель с ListBox -->
        <ListBox Grid.Row="1" Grid.Column="0" 
                 Margin="10"
                 SelectedIndex="0">
            <ListBoxItem Content="Элемент 1"/>
            <ListBoxItem Content="Элемент 2"/>
            <ListBoxItem Content="Элемент 3"/>
        </ListBox>

        <!-- Центральная область с ContentProperty -->
        <Border Grid.Row="1" Grid.Column="1" 
                Margin="10"
                BorderBrush="Gray"
                BorderThickness="1">
            <Border.Background>
                <LinearGradientBrush>
                    <GradientStop Color="LightBlue" Offset="0.0"/>
                    <GradientStop Color="White" Offset="1.0"/>
                </LinearGradientBrush>
            </Border.Background>
            
            <StackPanel HorizontalAlignment="Center" 
                        VerticalAlignment="Center">
                <TextBlock Text="Добро пожаловать!"
                           FontSize="24"
                           FontWeight="Bold"/>
                <Button Content="Нажми меня"
                        Margin="0,20,0,0"
                        Padding="20,10"/>
            </StackPanel>
        </Border>

        <!-- StatusBar -->
        <StatusBar Grid.Row="2" Grid.ColumnSpan="2">
            <StatusBarItem>
                <TextBlock Text="Готов"/>
            </StatusBarItem>
            <Separator/>
            <StatusBarItem>
                <TextBlock Text="Масштаб: 100%"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

#### Пример 2: Кастомный контроль с Attached Properties

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:WpfApp">
    
    <local:FlexPanel>
        <!-- Attached Properties от FlexPanel -->
        <Button Content="Flex=1" local:FlexPanel.Flex="1"/>
        <Button Content="Flex=2" local:FlexPanel.Flex="2"/>
        <Button Content="Align=Center" local:FlexPanel.Align="center"/>
    </local:FlexPanel>
</Window>
```

```csharp
// FlexPanel.cs
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

    public static double GetFlex(UIElement element) 
        => (double)element.GetValue(FlexProperty);
    
    public static void SetFlex(UIElement element, double value) 
        => element.SetValue(FlexProperty, value);

    // ... MeasureOverride и ArrangeOverride
}
```

#### Пример 3: Type Converters в действии

```xml
<StackPanel>
    <!-- CornerRadius: uniform -->
    <Border CornerRadius="10"
            Background="LightBlue"
            Margin="5">
        <TextBlock Text="Uniform radius"/>
    </Border>
    
    <!-- CornerRadius: разные углы -->
    <Border CornerRadius="10,20,30,40"
            Background="LightGreen"
            Margin="5">
        <TextBlock Text="Different radii"/>
    </Border>
    
    <!-- Thickness: все стороны -->
    <Border BorderBrush="Blue"
            BorderThickness="5"
            Margin="5">
        <TextBlock Text="Uniform thickness"/>
    </Border>
    
    <!-- Thickness: разные стороны -->
    <Border BorderBrush="Red"
            BorderThickness="1,2,3,4"
            Margin="5">
        <TextBlock Text="Different thickness"/>
    </Border>
    
    <!-- Color: разные форматы -->
    <Rectangle Fill="Red" Height="30"/>
    <Rectangle Fill="#FF0000" Height="30"/>
    <Rectangle Fill="#F00" Height="30"/>
    <Rectangle Fill="rgb(255,0,0)" Height="30"/>
    <Rectangle Fill="scRgb(1,0,0,0)" Height="30"/>
</StackPanel>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 2.1.1: Простое окно**

Создайте окно с использованием:
- Grid с 3 строками (Auto, *, Auto)
- Menu с 3 MenuItem
- ListBox с 5 элементами
- StatusBar с 2 статусами

**Задача 2.1.2: Type Converters**

Создайте XAML с использованием всех форматов:
- 4 способа задать Color
- 3 способа задать Thickness
- 2 способа задать CornerRadius
- Enum значения (HorizontalAlignment, VerticalAlignment, FontWeight)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 2.1.3: Вложенная структура**

Создайте сложную вложенную структуру:
```
Window
└── Grid (3x3)
    ├── Menu (Row 0)
    ├── ToolBar (Row 1)
    ├── Left Panel (Row 2, Col 0)
    │   └── TreeView
    ├── Center (Row 2, Col 1)
    │   └── TabControl (3 вкладки)
    │       └── Каждая вкладка: StackPanel + контролы
    ├── Right Panel (Row 2, Col 2)
    │   └── Properties Grid
    └── StatusBar (Row 3)
```

**Задача 2.1.4: Attached Properties**

Создайте `ResponsivePanel` с Attached Properties:
- `MinWidth` (double)
- `MaxWidth` (double)
- `VisibleOnSmall` (bool)
- Реализуйте `MeasureOverride` и `ArrangeOverride`

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 2.1.5: XAML-редактор**

Создайте приложение для редактирования XAML:
- Левая панель: TextBox для ввода XAML
- Правая панель: Preview (ContentControl)
- Кнопка "Preview": парсит XAML и отображает
- Обработка ошибок парсинга

```csharp
try
{
    var element = XamlReader.Parse(xamlText) as UIElement;
    previewContent.Content = element;
}
catch (XamlParseException ex)
{
    MessageBox.Show(ex.Message);
}
```

**Задача 2.1.6: Dynamic XAML Loader**

Реализуйте загрузку XAML из файла:
- TextBox для пути к файлу
- Кнопка "Load"
- Загружает XAML и отображает в ContentControl
- Поддержка внешних сборок через xmlns

---

### Решения

<details>
<summary>✅ Решение задачи 2.1.1</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Simple Window" Height="600" Width="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Menu -->
        <Menu Grid.Row="0">
            <MenuItem Header="_Файл">
                <MenuItem Header="_Новый"/>
                <MenuItem Header="_Открыть"/>
                <MenuItem Header="_Сохранить"/>
            </MenuItem>
            <MenuItem Header="_Правка">
                <MenuItem Header="_Копировать"/>
                <MenuItem Header="_Вставить"/>
            </MenuItem>
            <MenuItem Header="_Справка">
                <MenuItem Header="_О программе"/>
            </MenuItem>
        </Menu>

        <!-- Content -->
        <ListBox Grid.Row="1" Margin="10">
            <ListBoxItem Content="Элемент 1"/>
            <ListBoxItem Content="Элемент 2"/>
            <ListBoxItem Content="Элемент 3"/>
            <ListBoxItem Content="Элемент 4"/>
            <ListBoxItem Content="Элемент 5"/>
        </ListBox>

        <!-- StatusBar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem>
                <TextBlock Text="Готов"/>
            </StatusBarItem>
            <Separator/>
            <StatusBarItem>
                <TextBlock Text="Масштаб: 100%"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```
</details>

<details>
<summary>✅ Решение задачи 2.1.2</summary>

```xml
<StackPanel Margin="20">
    <!-- Color: 4 способа -->
    <Rectangle Fill="Red" Height="40" Margin="5"/>
    <Rectangle Fill="#FF0000" Height="40" Margin="5"/>
    <Rectangle Fill="#F00" Height="40" Margin="5"/>
    <Rectangle Fill="rgb(255,0,0)" Height="40" Margin="5"/>
    
    <!-- Thickness: 3 способа -->
    <Border BorderBrush="Blue" BorderThickness="5" Height="50" Margin="5">
        <TextBlock Text="Uniform: 5"/>
    </Border>
    <Border BorderBrush="Blue" BorderThickness="1,5" Height="50" Margin="5">
        <TextBlock Text="Horizontal, Vertical: 1,5"/>
    </Border>
    <Border BorderBrush="Blue" BorderThickness="1,2,3,4" Height="50" Margin="5">
        <TextBlock Text="Left, Top, Right, Bottom: 1,2,3,4"/>
    </Border>
    
    <!-- CornerRadius: 2 способа -->
    <Border Background="LightBlue" CornerRadius="10" Height="50" Margin="5">
        <TextBlock Text="Uniform: 10"/>
    </Border>
    <Border Background="LightGreen" CornerRadius="10,20,30,40" Height="50" Margin="5">
        <TextBlock Text="Different: 10,20,30,40"/>
    </Border>
    
    <!-- Enum values -->
    <TextBlock HorizontalAlignment="Center" 
               VerticalAlignment="Top"
               FontWeight="Bold"
               FontSize="16"
               Text="Enum Demo"/>
    
    <Button HorizontalAlignment="Left" Content="Left"/>
    <Button HorizontalAlignment="Center" Content="Center"/>
    <Button HorizontalAlignment="Right" Content="Right"/>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 2.1.3</summary>

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Complex Layout" Height="800" Width="1200">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="250"/>
        </Grid.ColumnDefinitions>

        <!-- Menu -->
        <Menu Grid.Row="0" Grid.ColumnSpan="3">
            <MenuItem Header="_Файл"/>
            <MenuItem Header="_Правка"/>
            <MenuItem Header="_Вид"/>
            <MenuItem Header="_Справка"/>
        </Menu>

        <!-- ToolBar -->
        <ToolBarTray Grid.Row="1" Grid.ColumnSpan="3">
            <ToolBar>
                <Button Content="New"/>
                <Button Content="Open"/>
                <Button Content="Save"/>
                <Separator/>
                <Button Content="Undo"/>
                <Button Content="Redo"/>
            </ToolBar>
        </ToolBarTray>

        <!-- Left Panel: TreeView -->
        <Border Grid.Row="2" Grid.Column="0" 
                BorderBrush="Gray" BorderThickness="0,0,1,0">
            <TreeView>
                <TreeViewItem Header="Root">
                    <TreeViewItem Header="Child 1"/>
                    <TreeViewItem Header="Child 2"/>
                </TreeViewItem>
            </TreeView>
        </Border>

        <!-- Center: TabControl -->
        <TabControl Grid.Row="2" Grid.Column="1" Margin="10">
            <TabItem Header="Вкладка 1">
                <StackPanel Margin="10">
                    <TextBlock Text="Content 1"/>
                    <Button Content="Button 1" Margin="0,10"/>
                    <TextBox Height="100"/>
                </StackPanel>
            </TabItem>
            <TabItem Header="Вкладка 2">
                <StackPanel Margin="10">
                    <TextBlock Text="Content 2"/>
                    <ListBox>
                        <ListBoxItem Content="Item A"/>
                        <ListBoxItem Content="Item B"/>
                    </ListBox>
                </StackPanel>
            </TabItem>
            <TabItem Header="Вкладка 3">
                <Grid Margin="10">
                    <Grid.RowDefinitions>
                        <RowDefinition/>
                        <RowDefinition/>
                    </Grid.RowDefinitions>
                    <TextBox Grid.Row="0" Margin="5"/>
                    <Button Grid.Row="1" Content="Submit" Margin="5"/>
                </Grid>
            </TabItem>
        </TabControl>

        <!-- Right Panel: Properties -->
        <Border Grid.Row="2" Grid.Column="2" 
                BorderBrush="Gray" BorderThickness="1,0,0,0">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Text="Свойства" FontWeight="Bold"/>
                <Grid Grid.Row="1" Margin="0,10">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition/>
                        <ColumnDefinition/>
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition/>
                        <RowDefinition/>
                        <RowDefinition/>
                    </Grid.RowDefinitions>
                    <TextBlock Grid.Row="0" Text="X:"/>
                    <TextBox Grid.Row="0" Grid.Column="1"/>
                    <TextBlock Grid.Row="1" Text="Y:"/>
                    <TextBox Grid.Row="1" Grid.Column="1"/>
                    <TextBlock Grid.Row="2" Text="Width:"/>
                    <TextBox Grid.Row="2" Grid.Column="1"/>
                </Grid>
            </Grid>
        </Border>

        <!-- StatusBar -->
        <StatusBar Grid.Row="3" Grid.ColumnSpan="3">
            <StatusBarItem>
                <TextBlock Text="Готов"/>
            </StatusBarItem>
            <Separator/>
            <StatusBarItem>
                <TextBlock Text="Объектов: 0"/>
            </StatusBarItem>
            <Separator/>
            <StatusBarItem>
                <TextBlock Text="Масштаб: 100%"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```
</details>

---

## Ключевые выводы

✅ **XAML** — декларативный язык на основе XML  
✅ **ContentProperty** позволяет опускать имя свойства по умолчанию  
✅ **Collection Syntax** может быть явным и неявным  
✅ **Type Converters** автоматически конвертируют строки в нужные типы  
✅ **Attached Properties** позволяют добавлять свойства любым элементам  
✅ **Property Element Syntax** используется для сложных значений

---

## Дополнительные ресурсы

- [XAML Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/xaml)
- [XAML Syntax](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/xaml/xaml-syntax)
- [Type Converters](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/type-converters-in-xaml)
