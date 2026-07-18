# Тема 2.2: Markup Extensions

### Теория

**Markup Extensions** — механизм XAML для вычисления значений свойств в runtime. Синтаксис: `{ExtensionName Параметры}`.

#### Встроенные markup extensions

| Extension | Описание | Пример |
|-----------|----------|--------|
| `{Binding}` | Data binding | `{Binding Path=Name}` |
| `{StaticResource}` | Статический ресурс | `{StaticResource MyBrush}` |
| `{DynamicResource}` | Динамический ресурс | `{DynamicResource ThemeBrush}` |
| `{x:Static}` | Статическое свойство/поле | `{x:Static SystemInfo.Version}` |
| `{x:Type}` | Получение типа | `{x:Type Button}` |
| `{x:Null}` | Null значение | `{x:Null}` |
| `{x:Reference}` | Ссылка на элемент | `{x:Reference myTextBox}` |
| `{TemplateBinding}` | Binding в template | `{TemplateBinding Background}` |

#### Синтаксис

```xml
<!-- Базовый синтаксис -->
<Property>{ExtensionName Параметры}</Property>

<!-- Именованные параметры -->
<Property>{ExtensionName Param1=Value1, Param2=Value2}</Property>

<!-- Вложенные markup extensions -->
<Property>{Extension1 {Extension2 Value}}</Property>
```

### Примеры кода

#### Пример 1: StaticResource vs DynamicResource

```xml
<Window.Resources>
    <SolidColorBrush x:Key="MyBrush" Color="#0078D4"/>
</Window.Resources>

<StackPanel>
    <!-- StaticResource: значение вычисляется 1 раз при загрузке -->
    <Button Background="{StaticResource MyBrush}" 
            Content="Static"/>
    
    <!-- DynamicResource: значение может измениться в runtime -->
    <Button Background="{DynamicResource MyBrush}" 
            Content="Dynamic"/>
    
    <!-- Кнопка для смены ресурса -->
    <Button Content="Сменить кисть"
            Click="ChangeBrush_Click"/>
</StackPanel>
```

```csharp
private void ChangeBrush_Click(object sender, RoutedEventArgs e)
{
    // StaticResource НЕ обновится
    // DynamicResource обновится автоматически
    var newBrush = new SolidColorBrush(Colors.Green);
    Resources["MyBrush"] = newBrush;
}
```

#### Пример 2: x:Static для системных констант

```xml
<Window xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <StackPanel>
        <!-- Системные цвета -->
        <Border Background="{x:Static SystemColors.ControlBrush}"
                Height="50">
            <TextBlock Text="System Control Color"/>
        </Border>
        
        <!-- Системные параметры -->
        <TextBlock Text="{x:Static SystemInfo.OSVersion}"/>
        <TextBlock Text="{x:Static Environment.CurrentDirectory}"/>
        
        <!-- Статические поля -->
        <TextBlock Text="{x:Static sys.Math.PI}"/>
        <TextBlock Text="{x:Static sys.DateTime.Now}"/>
    </StackPanel>
</Window>
```

#### Пример 3: x:Type и x:Null

```xml
<Window.Resources>
    <!-- Стиль для всех Button -->
    <Style TargetType="{x:Type Button}">
        <Setter Property="Background" Value="Blue"/>
    </Style>
    
    <!-- Стиль с null значением -->
    <Style x:Key="NoBorderButton" TargetType="{x:Type Button}">
        <Setter Property="BorderThickness" Value="{x:Null}"/>
        <Setter Property="Background" Value="{x:Null}"/>
    </Style>
</Window.Resources>

<StackPanel>
    <Button Content="Стиль по умолчанию"/>
    <Button Content="Без рамки" Style="{StaticResource NoBorderButton}"/>
</StackPanel>
```

#### Пример 4: x:Reference для связи элементов

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Slider -->
    <Slider x:Name="fontSizeSlider" 
            Minimum="12" Maximum="48" Value="16"
            Grid.Row="0"/>
    
    <!-- TextBlock, связанный со Slider -->
    <TextBlock Text="Размер шрифта меняется"
               FontSize="{Binding Value, ElementName=fontSizeSlider}"
               Grid.Row="1"/>
    
    <!-- Альтернатива через x:Reference -->
    <TextBlock Text="Дублирование"
               FontSize="{Binding Value, Source={x:Reference fontSizeSlider}}"
               Grid.Row="2"/>
</Grid>
```

#### Пример 5: TemplateBinding в ControlTemplate

```xml
<ControlTemplate TargetType="Button">
    <Border x:Name="border"
            Background="{TemplateBinding Background}"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}"
            CornerRadius="5">
        <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                          VerticalAlignment="{TemplateBinding VerticalContentAlignment}"/>
    </Border>
    
    <ControlTemplate.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <!-- TemplateBinding в триггере -->
            <Setter TargetName="border" Property="Background" 
                    Value="{TemplateBinding BorderBrush}"/>
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

#### Пример 6: Вложенные markup extensions

```xml
<Window.Resources>
    <sys:Double x:Key="FontSizeLarge">24</sys:Double>
</Window.Resources>

<StackPanel>
    <!-- Вложенный Binding в ConverterParameter -->
    <TextBlock Text="{Binding Date, StringFormat={x:Static sys:String.Format}, 
                    ConverterParameter={x:Static sys:DateTime.Now}}"/>
    
    <!-- DynamicResource в Setter -->
    <Style TargetType="Button">
        <Setter Property="Background" 
                Value="{DynamicResource {x:Static SystemColors.HighlightBrushKey}}"/>
    </Style>
</StackPanel>
```

#### Пример 7: Кастомная markup extension

```csharp
// CustomExtension.cs
using System;
using System.Windows.Markup;
using System.Windows.Media;

public class ColorFromHexExtension : MarkupExtension
{
    public string Hex { get; set; }

    public ColorFromHexExtension(string hex)
    {
        Hex = hex;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // Парсим hex строку в Color
        var color = (Color)ColorConverter.ConvertFromString(Hex);
        return new SolidColorBrush(color);
    }
}

// Использование в XAML:
// <Button Background="{local:ColorFromHex #0078D4}"/>
```

```xml
<!-- Использование кастомной extension -->
<Window xmlns:local="clr-namespace:WpfApp">
    <StackPanel>
        <Button Content="Custom Color"
                Background="{local:ColorFromHex #0078D4}"/>
        <Button Content="Another Color"
                Background="{local:ColorFromHex #FF5722}"/>
    </StackPanel>
</Window>
```

#### Пример 8: Кастомная extension с параметрами

```csharp
// TranslateExtension.cs
using System;
using System.Windows.Markup;
using System.Resources;
using System.Reflection;

public class TranslateExtension : MarkupExtension
{
    public string Key { get; set; }
    public string ResourceManager { get; set; }

    public TranslateExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return Key;

        // Загружаем ресурс из ResourceManager
        var assembly = Assembly.GetExecutingAssembly();
        var rm = new ResourceManager(ResourceManager, assembly);
        return rm.GetString(Key) ?? Key;
    }
}
```

```xml
<!-- Использование -->
<Window xmlns:local="clr-namespace:WpfApp">
    <StackPanel>
        <TextBlock Text="{local:Translate WelcomeMessage, 
                          ResourceManager=WpfApp.Resources.Strings}"/>
        <Button Content="{local:Translate OkButton, 
                            ResourceManager=WpfApp.Resources.Strings}"/>
    </StackPanel>
</Window>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 2.2.1: StaticResource практика**

Создайте окно с ResourceDictionary, содержащим:
- 3 кисти (SolidColorBrush)
- 2 цвета (Color)
- 1 строку
- 1 значение double

Используйте все ресурсы через `{StaticResource}` в UI элементах.

**Задача 2.2.2: x:Static практика**

Создайте информационную панель, отображающую:
- Версию ОС (`SystemInfo.OSVersion`)
- Имя компьютера (`Environment.MachineName`)
- Текущую директорию (`Environment.CurrentDirectory`)
- Число π (`Math.PI`)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 2.2.3: DynamicResource темы**

Создайте приложение с двумя темами (Light/Dark):
- ResourceDictionary с кистями для каждой темы
- Кнопка переключения темы
- Все элементы используют `{DynamicResource}`
- При переключении цвета обновляются

**Задача 2.2.4: x:Reference связь элементов**

Создайте панель управления:
- 3 Slider (для Red, Green, Blue компонентов)
- Rectangle, цвет которого зависит от Slider
- TextBlock, отображающий hex значения цвета
- Используйте `{Binding ElementName=...}` или `{x:Reference}`

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 2.2.5: Кастомная Markup Extension для конвертации**

Создайте `FileSizeExtension`:
- Принимает размер в байтах (long)
- Возвращает строку в формате "1.5 MB", "2.3 GB"
- Поддержка: B, KB, MB, GB, TB

```xml
<TextBlock Text="{local:FileSize 1572864}"/>
<!-- Вывод: "1.5 MB" -->
```

**Задача 2.2.6: MultiMarkup Extension**

Создайте универсальную extension:
- Поддержка нескольких параметров через запятую
- Пример: `{local:Multi Format=Date, Value={Binding Date}, Culture=ru-RU}`
- Используйте `IProvideValueTarget` для получения контекста

---

### Решения

<details>
<summary>✅ Решение задачи 2.2.1</summary>

```xml
<Window.Resources>
    <!-- Кисти -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#0078D4"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="#66BB6A"/>
    <SolidColorBrush x:Key="AccentBrush" Color="#FF5722"/>
    
    <!-- Цвета -->
    <Color x:Key="PrimaryColor">#0078D4</Color>
    <Color x:Key="BackgroundColor">#F5F5F5</Color>
    
    <!-- Строка -->
    <sys:String x:Key="AppTitle">Моё приложение</sys:String>
    
    <!-- Double -->
    <sys:Double x:Key="DefaultFontSize">14</sys:Double>
</Window.Resources>

<Grid>
    <Border Background="{StaticResource PrimaryBrush}"
            Height="100">
        <TextBlock Text="{StaticResource AppTitle}"
                   FontSize="{StaticResource DefaultFontSize}"
                   Foreground="White"/>
    </Border>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 2.2.2</summary>

```xml
<Window xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <StackPanel Margin="20">
        <TextBlock FontSize="20" FontWeight="Bold" 
                   Text="Системная информация"/>
        
        <Separator Margin="0,10"/>
        
        <TextBlock Margin="0,5">
            <Run Text="ОС: "/>
            <Run Text="{x:Static SystemInfo.OSVersion}"/>
        </TextBlock>
        
        <TextBlock Margin="0,5">
            <Run Text="Компьютер: "/>
            <Run Text="{x:Static Environment.MachineName}"/>
        </TextBlock>
        
        <TextBlock Margin="0,5">
            <Run Text="Директория: "/>
            <Run Text="{x:Static Environment.CurrentDirectory}"/>
        </TextBlock>
        
        <TextBlock Margin="0,5">
            <Run Text="Число π: "/>
            <Run Text="{x:Static sys:Math.PI}"/>
        </TextBlock>
    </StackPanel>
</Window>
```
</details>

<details>
<summary>✅ Решение задачи 2.2.3</summary>

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <!-- Light Theme -->
        <SolidColorBrush x:Key="WindowBackgroundBrush" Color="#FFFFFF"/>
        <SolidColorBrush x:Key="TextPrimaryBrush" Color="#212121"/>
        <SolidColorBrush x:Key="AccentBrush" Color="#0078D4"/>
    </ResourceDictionary>
</Application.Resources>
```

```xml
<!-- MainWindow.xaml -->
<Window>
    <Window.Resources>
        <!-- Dark Theme (отдельный словарь) -->
        <ResourceDictionary x:Key="DarkTheme">
            <SolidColorBrush x:Key="WindowBackgroundBrush" Color="#212121"/>
            <SolidColorBrush x:Key="TextPrimaryBrush" Color="#FFFFFF"/>
            <SolidColorBrush x:Key="AccentBrush" Color="#42A5F5"/>
        </ResourceDictionary>
    </Window.Resources>
    
    <Grid Background="{DynamicResource WindowBackgroundBrush}">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <TextBlock Text="Тема приложения"
                       FontSize="24"
                       Foreground="{DynamicResource TextPrimaryBrush}"/>
            
            <Button Content="Переключить тему"
                    Margin="0,20"
                    Padding="20,10"
                    Background="{DynamicResource AccentBrush}"
                    Foreground="White"
                    Click="ToggleTheme_Click"/>
        </StackPanel>
    </Grid>
</Window>
```

```csharp
private void ToggleTheme_Click(object sender, RoutedEventArgs e)
{
    var darkTheme = (ResourceDictionary)FindResource("DarkTheme");
    
    if (Application.Current.Resources.MergedDictionaries.Contains(darkTheme))
    {
        Application.Current.Resources.MergedDictionaries.Remove(darkTheme);
    }
    else
    {
        Application.Current.Resources.MergedDictionaries.Add(darkTheme);
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 2.2.4</summary>

```xml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Red Slider -->
    <TextBlock Grid.Row="0" Text="Red:"/>
    <Slider x:Name="redSlider" Grid.Row="0" Grid.Column="1"
            Minimum="0" Maximum="255" Value="120"/>
    
    <!-- Green Slider -->
    <TextBlock Grid.Row="1" Text="Green:"/>
    <Slider x:Name="greenSlider" Grid.Row="1" Grid.Column="1"
            Minimum="0" Maximum="255" Value="180"/>
    
    <!-- Blue Slider -->
    <TextBlock Grid.Row="2" Text="Blue:"/>
    <Slider x:Name="blueSlider" Grid.Row="2" Grid.Column="1"
            Minimum="0" Maximum="255" Value="200"/>
    
    <!-- Preview Rectangle -->
    <Rectangle Grid.Row="3" Grid.Column="1" Height="100" Margin="0,10">
        <Rectangle.Fill>
            <SolidColorBrush>
                <SolidColorBrush.Color>
                    <Color R="{Binding Value, ElementName=redSlider}"
                           G="{Binding Value, ElementName=greenSlider}"
                           B="{Binding Value, ElementName=blueSlider}"/>
                </SolidColorBrush.Color>
            </SolidColorBrush>
        </Rectangle.Fill>
    </Rectangle>
    
    <!-- Hex Value Display -->
    <TextBlock Grid.Row="4" Grid.Column="1" 
               HorizontalAlignment="Center"
               FontSize="16"
               FontWeight="Bold">
        <TextBlock.Text>
            <MultiBinding StringFormat="#{0:X2}{1:X2}{2:X2}">
                <Binding ElementName="redSlider" Path="Value"/>
                <Binding ElementName="greenSlider" Path="Value"/>
                <Binding ElementName="blueSlider" Path="Value"/>
            </MultiBinding>
        </TextBlock.Text>
    </TextBlock>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 2.2.5</summary>

```csharp
using System;
using System.Windows.Markup;

public class FileSizeExtension : MarkupExtension
{
    public long Bytes { get; set; }

    public FileSizeExtension(long bytes)
    {
        Bytes = bytes;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = Bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

// Использование:
// <TextBlock Text="{local:FileSize 1572864}"/>
// Вывод: "1.5 MB"
```
</details>

---

## Ключевые выводы

✅ **Markup Extensions** вычисляются в runtime, не в compile-time  
✅ **StaticResource** — быстрое, однократное получение  
✅ **DynamicResource** — медленнее, но обновляется при изменении  
✅ **x:Static** — доступ к статическим членам .NET  
✅ **x:Reference** — связь элементов без DataContext  
✅ **TemplateBinding** — быстрее чем Binding, но только однонаправленный  
✅ **Кастомные extensions** наследуются от `MarkupExtension`

---

## Дополнительные ресурсы

- [Markup Extensions](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/xaml/markup-extensions-and-xaml)
- [StaticResource vs DynamicResource](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/resources/static-and-dynamic-resources)
- [Custom Markup Extensions](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/markup-extensions-and-wpf-xaml)
