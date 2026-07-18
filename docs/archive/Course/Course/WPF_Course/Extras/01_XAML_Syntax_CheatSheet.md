# Шпаргалка по XAML синтаксису

## Базовый синтаксис

```xml
<!-- Элемент с атрибутами -->
<Button Content="Click me" Width="100" Height="30"/>

<!-- Элемент с вложенными элементами -->
<Button>
    <Button.Content>
        <StackPanel>
            <Image Source="/icon.png"/>
            <TextBlock Text="Click me"/>
        </StackPanel>
    </Button.Content>
</Button>
```

## Пространства имён

```xml
<Window 
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:MyApp"
    xmlns:controls="clr-namespace:MyApp.Controls;assembly=MyApp.Controls">
```

## Markup Extensions

| Extension | Пример |
|-----------|--------|
| `{Binding}` | `{Binding Path=Name, Mode=TwoWay}` |
| `{StaticResource}` | `{StaticResource MyBrush}` |
| `{DynamicResource}` | `{DynamicResource ThemeBrush}` |
| `{x:Static}` | `{x:Static SystemInfo.OSVersion}` |
| `{x:Type}` | `{x:Type Button}` |
| `{x:Null}` | `{x:Null}` |
| `{x:Reference}` | `{x:Reference myTextBox}` |
| `{TemplateBinding}` | `{TemplateBinding Background}` |

## Type Converters

```xml
<!-- Color -->
<Rectangle Fill="Red"/>
<Rectangle Fill="#FF0000"/>
<Rectangle Fill="#F00"/>
<Rectangle Fill="rgb(255,0,0)"/>

<!-- Thickness -->
<Border BorderThickness="5"/>
<Border BorderThickness="1,5"/>
<Border BorderThickness="1,2,3,4"/>

<!-- CornerRadius -->
<Border CornerRadius="10"/>
<Border CornerRadius="10,20,10,20"/>

<!-- Enum -->
<Button HorizontalAlignment="Center"/>
<TextBlock FontWeight="Bold"/>
```

## Content Property

```xml
<!-- Эквивалентные записи -->
<Button Content="Click"/>
<Button>
    <Button.Content>
        <TextBlock Text="Click"/>
    </Button.Content>
</Button>

<!-- Content может быть любым объектом -->
<Button>
    <StackPanel Orientation="Horizontal">
        <Image Source="/icon.png"/>
        <TextBlock Text="Click"/>
    </StackPanel>
</Button>
```

## Collection Syntax

```xml
<!-- Явный -->
<ListBox.Items>
    <ListBoxItem Content="1"/>
    <ListBoxItem Content="2"/>
</ListBox.Items>

<!-- Неявный -->
<ListBox>
    <ListBoxItem Content="1"/>
    <ListBoxItem Content="2"/>
</ListBox>
```

## Attached Properties

```xml
<Grid>
    <Button Grid.Row="0" Grid.Column="1"/>
</Grid>

<Canvas>
    <Button Canvas.Left="100" Canvas.Top="50"/>
</Canvas>

<DockPanel>
    <Button DockPanel.Dock="Top"/>
</DockPanel>
```

## Ресурсы

```xml
<Window.Resources>
    <SolidColorBrush x:Key="MyBrush" Color="Blue"/>
    <Style x:Key="MyButton" TargetType="Button">
        <Setter Property="Background" Value="Blue"/>
    </Style>
</Window.Resources>

<!-- Использование -->
<Button Background="{StaticResource MyBrush}"
        Style="{StaticResource MyButton}"/>
```

## Стили и триггеры

```xml
<Style x:Key="MyButton" TargetType="Button">
    <Setter Property="Background" Value="Blue"/>
    
    <Style.Triggers>
        <!-- Property Trigger -->
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="DarkBlue"/>
        </Trigger>
        
        <!-- Data Trigger -->
        <DataTrigger Binding="{Binding IsActive}" Value="True">
            <Setter Property="Opacity" Value="0.5"/>
        </DataTrigger>
        
        <!-- Multi Trigger -->
        <MultiTrigger>
            <MultiTrigger.Conditions>
                <Condition Property="IsMouseOver" Value="True"/>
                <Condition Property="IsEnabled" Value="True"/>
            </MultiTrigger.Conditions>
            <Setter Property="Cursor" Value="Hand"/>
        </MultiTrigger>
    </Style.Triggers>
</Style>
```

## Шаблоны

```xml
<!-- ControlTemplate -->
<ControlTemplate TargetType="Button">
    <Border Background="{TemplateBinding Background}">
        <ContentPresenter/>
    </Border>
</ControlTemplate>

<!-- DataTemplate -->
<DataTemplate x:Key="PersonTemplate">
    <StackPanel>
        <TextBlock Text="{Binding Name}"/>
        <TextBlock Text="{Binding Email}"/>
    </StackPanel>
</DataTemplate>
```

## Binding

```xml
<!-- Basic -->
<TextBlock Text="{Binding Name}"/>

<!-- Mode -->
<TextBox Text="{Binding Name, Mode=TwoWay}"/>

<!-- UpdateSourceTrigger -->
<TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Converter -->
<TextBlock Text="{Binding Date, Converter={StaticResource DateConverter}}"/>

<!-- StringFormat -->
<TextBlock Text="{Binding Price, StringFormat={}{0:C}}"/>

<!-- MultiBinding -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} {1}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

## Layout

```xml
<!-- Grid -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="100"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
</Grid>

<!-- StackPanel -->
<StackPanel Orientation="Vertical">
    <Button Content="1"/>
    <Button Content="2"/>
</StackPanel>

<!-- WrapPanel -->
<WrapPanel>
    <Button Content="1"/>
    <Button Content="2"/>
</WrapPanel>

<!-- DockPanel -->
<DockPanel>
    <Button DockPanel.Dock="Top" Content="Top"/>
    <Button Content="Fill"/>
</DockPanel>

<!-- Canvas -->
<Canvas>
    <Button Canvas.Left="50" Canvas.Top="30"/>
</Canvas>
```

## Events

```xml
<!-- Event Handler -->
<Button Click="Button_Click"/>

<!-- EventTrigger -->
<Style.Triggers>
    <EventTrigger RoutedEvent="Loaded">
        <BeginStoryboard>
            <Storyboard>
                <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                 To="0" Duration="0:0:1"/>
            </Storyboard>
        </BeginStoryboard>
    </EventTrigger>
</Style.Triggers>
```

## Resources: Static vs Dynamic

```xml
<!-- StaticResource: вычисляется 1 раз -->
<Button Background="{StaticResource MyBrush}"/>

<!-- DynamicResource: обновляется при изменении -->
<Button Background="{DynamicResource ThemeBrush}"/>
```

## Common Properties

| Property | Описание | Пример |
|----------|----------|--------|
| `Margin` | Отступы снаружи | `Margin="10"` или `Margin="1,2,3,4"` |
| `Padding` | Отступы внутри | `Padding="5"` |
| `HorizontalAlignment` | Выравнивание по горизонтали | `Left`, `Center`, `Right`, `Stretch` |
| `VerticalAlignment` | Выравнивание по вертикали | `Top`, `Center`, `Bottom`, `Stretch` |
| `Visibility` | Видимость | `Visible`, `Collapsed`, `Hidden` |
| `Opacity` | Прозрачность | `0.0`–`1.0` |
| `IsEnabled` | Доступность | `True`, `False` |
| `Tag` | Пользовательские данные | Любое значение |

---

## Быстрая справка

### Dependency Property

```csharp
public static readonly DependencyProperty MyProperty =
    DependencyProperty.Register(
        "MyProperty",
        typeof(string),
        typeof(MyControl),
        new PropertyMetadata("default")
    );

public string MyProperty
{
    get => (string)GetValue(MyProperty);
    set => SetValue(MyProperty, value);
}
```

### Attached Property

```csharp
public static readonly DependencyProperty MyProperty =
    DependencyProperty.RegisterAttached(
        "MyProperty",
        typeof(string),
        typeof(MyClass),
        new PropertyMetadata("default")
    );

public static string GetMyProperty(UIElement e) => 
    (string)e.GetValue(MyProperty);

public static void SetMyProperty(UIElement e, string value) => 
    e.SetValue(MyProperty, value);
```

### Command

```csharp
public ICommand MyCommand { get; }

public ViewModel()
{
    MyCommand = new RelayCommand(Execute, CanExecute);
}

private void Execute(object parameter) { }
private bool CanExecute(object parameter) => true;
```
