# Тема 9.1: ResourceDictionary и MergedDictionaries

### Теория

**ResourceDictionary** — коллекция переиспользуемых ресурсов в WPF.

#### Иерархия ресурсов

```
Application.Resources (глобальные)
    ↓
Window.Resources (уровень окна)
    ↓
Layout.Resources (Grid, StackPanel...)
    ↓
Control.Resources (уровень элемента)
```

#### MergedDictionaries

Объединение нескольких ResourceDictionary в один:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Styles/Colors.xaml"/>
            <ResourceDictionary Source="Styles/Buttons.xaml"/>
            <ResourceDictionary Source="Themes/LightTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
        
        <!-- Локальные ресурсы -->
        <SolidColorBrush x:Key="LocalBrush" Color="Red"/>
    </ResourceDictionary>
</Application.Resources>
```

### Примеры кода

#### Пример 1: Базовый ResourceDictionary

```xml
<!-- Styles/Colors.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
    
    <!-- Цвета -->
    <Color x:Key="PrimaryColor">#0078D4</Color>
    <Color x:Key="SecondaryColor">#66BB6A</Color>
    <Color x:Key="AccentColor">#FF5722</Color>
    
    <!-- Кисти -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    
    <!-- Значения -->
    <sys:Double x:Key="DefaultFontSize">14</sys:Double>
    <Thickness x:Key="DefaultMargin">10</Thickness>
    <CornerRadius x:Key="DefaultCornerRadius">5</CornerRadius>
</ResourceDictionary>
```

#### Пример 2: Стили кнопок

```xml
<!-- Styles/Buttons.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Базовый стиль кнопки -->
    <Style x:Key="BaseButton" TargetType="Button">
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Padding" Value="15,8"/>
        <Setter Property="Margin" Value="5"/>
        <Setter Property="Cursor" Value="Hand"/>
    </Style>
    
    <!-- Primary кнопка -->
    <Style x:Key="PrimaryButton" 
           TargetType="Button" 
           BasedOn="{StaticResource BaseButton}">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#005A9E"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <!-- Secondary кнопка -->
    <Style x:Key="SecondaryButton" 
           TargetType="Button" 
           BasedOn="{StaticResource BaseButton}">
        <Setter Property="Background" Value="{StaticResource SecondaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
    </Style>
    
    <!-- Danger кнопка -->
    <Style x:Key="DangerButton" 
           TargetType="Button" 
           BasedOn="{StaticResource BaseButton}">
        <Setter Property="Background" Value="#DC3545"/>
        <Setter Property="Foreground" Value="White"/>
    </Style>
</ResourceDictionary>
```

#### Пример 3: MergedDictionaries в App.xaml

```xml
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Цвета -->
                <ResourceDictionary Source="Styles/Colors.xaml"/>
                
                <!-- Стили кнопок -->
                <ResourceDictionary Source="Styles/Buttons.xaml"/>
                
                <!-- Стили TextBox -->
                <ResourceDictionary Source="Styles/TextBoxes.xaml"/>
                
                <!-- Стили ComboBox -->
                <ResourceDictionary Source="Styles/ComboBoxes.xaml"/>
            </ResourceDictionary.MergedDictionaries>
            
            <!-- Глобальные конвертеры -->
            <converters:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
            <converters:InverseBoolConverter x:Key="InverseBool"/>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

#### Пример 4: Pack URI для внешних словарей

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Из той же сборки -->
            <ResourceDictionary Source="/MyAssembly;component/Styles/Colors.xaml"/>
            
            <!-- Из другой сборки -->
            <ResourceDictionary Source="/OtherAssembly;component/Themes/Theme.xaml"/>
            
            <!-- Pack URI формат -->
            <ResourceDictionary Source="pack://application:,,,/MyAssembly;component/Styles/Colors.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

#### Пример 5: Программная работа с ресурсами

```csharp
// Добавление ресурса
Resources["CustomBrush"] = new SolidColorBrush(Colors.Purple);

// Получение ресурса
var brush = (Brush)FindResource("PrimaryBrush");
var brush2 = (Brush)TryFindResource("SecondaryBrush");

// Проверка наличия
if (Resources.Contains("CustomBrush"))
{
    var brush = Resources["CustomBrush"];
}

// Удаление ресурса
Resources.Remove("CustomBrush");

// Слияние словарей в коде
var dict = new ResourceDictionary
{
    Source = new Uri("Styles/Colors.xaml", UriKind.Relative)
};
Application.Current.Resources.MergedDictionaries.Add(dict);
```

#### Пример 6: Словарь конвертеров

```xml
<!-- Converters/Converters.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:converters="clr-namespace:WpfApp.Converters">
    
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
    <converters:InverseBoolConverter x:Key="InverseBool"/>
    <converters:NullToVisibilityConverter x:Key="NullToVisibility"/>
    <converters:StringToVisibilityConverter x:Key="StringToVisibility"/>
    <converters:DoubleToThicknessConverter x:Key="DoubleToThickness"/>
    <converters:DateToStringConverter x:Key="DateToString"/>
    <converters:MicronsToPixelConverter x:Key="MicronsToPixel"/>
    <converters:ZoomToStringConverter x:Key="ZoomToString"/>
</ResourceDictionary>
```

#### Пример 7: Реальное использование из DotElectric

```xml
<!-- App.xaml -->
<Application x:Class="DotElectric.TemplateEditor.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design -->
                <materialDesign:CustomColorTheme BaseTheme="Dark"
                                                  PrimaryColor="#0078D4"
                                                  SecondaryColor="#66BB6A"/>
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
                
                <!-- Темы приложения -->
                <ResourceDictionary Source="Resources/Styles/DarkTheme.xaml"/>
                
                <!-- Конвертеры -->
                <ResourceDictionary Source="Resources/Converters/Converters.xaml"/>
                
                <!-- Стили -->
                <ResourceDictionary Source="Resources/Styles/Buttons.xaml"/>
                <ResourceDictionary Source="Resources/Styles/TextBoxes.xaml"/>
                <ResourceDictionary Source="Resources/Styles/ToolBar.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

```xml
<!-- Resources/Styles/DarkTheme.xaml (фрагмент) -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Основные цвета -->
    <Color x:Key="WindowBackgroundColor">#212121</Color>
    <Color x:Key="PanelBackgroundColor">#303030</Color>
    <Color x:Key="TextPrimaryColor">#FFFFFF</Color>
    <Color x:Key="TextSecondaryColor">#BDBDBD</Color>
    <Color x:Key="BorderColor">#424242</Color>
    <Color x:Key="AccentColor">#0078D4</Color>
    
    <!-- Кисти -->
    <SolidColorBrush x:Key="WindowBackgroundBrush" Color="{StaticResource WindowBackgroundColor}"/>
    <SolidColorBrush x:Key="PanelBackgroundBrush" Color="{StaticResource PanelBackgroundColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource BorderColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    
    <!-- Стили для ToolBar -->
    <Style x:Key="ToolBarButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="8"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#3D3D3D"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</ResourceDictionary>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 9.1.1: Simple ResourceDictionary**

Создайте ResourceDictionary:
- 3 цвета
- 3 кисти
- 1 значение double

**Задача 9.1.2: Button Styles**

Создайте 3 стиля кнопок:
- Primary (синий)
- Secondary (зелёный)
- Danger (красный)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 9.1.3: MergedDictionaries**

Создайте структуру:
- Styles/Colors.xaml
- Styles/Buttons.xaml
- Styles/TextBoxes.xaml
- Объедините в App.xaml

**Задача 9.1.4: Converter Dictionary**

Создайте словарь конвертеров:
- BoolToVisibility
- InverseBool
- NullToVisibility
- StringLengthToVisibility

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 9.1.5: External Assembly Resources**

Подключите ресурсы:
- Из другой сборки
- Pack URI
- MaterialDesignThemes

**Задача 9.1.6: Resource Manager**

Создайте сервис:
- Загрузка словарей
- Кэширование
- Динамическое добавление

---

### Решения

<details>
<summary>✅ Решение задачи 9.1.1</summary>

```xml
<!-- Styles/Colors.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Цвета -->
    <Color x:Key="PrimaryColor">#0078D4</Color>
    <Color x:Key="SecondaryColor">#28A745</Color>
    <Color x:Key="AccentColor">#FFC107</Color>
    
    <!-- Кисти -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    
    <!-- Значение -->
    <sys:Double x:Key="DefaultFontSize">14</sys:Double>
</ResourceDictionary>
```
</details>

<details>
<summary>✅ Решение задачи 9.1.3</summary>

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Styles/Colors.xaml"/>
            <ResourceDictionary Source="Styles/Buttons.xaml"/>
            <ResourceDictionary Source="Styles/TextBoxes.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```
</details>

---

## Ключевые выводы

✅ **ResourceDictionary** — коллекция переиспользуемых ресурсов  
✅ **MergedDictionaries** — объединение нескольких словарей  
✅ **Pack URI** — ссылка на ресурсы в других сборках  
✅ **StaticResource** — статическое получение ресурса  
✅ **DynamicResource** — динамическое получение ресурса  
✅ **Иерархия** — Application → Window → Layout → Control  
✅ **Программный доступ** — FindResource, TryFindResource

---

## Дополнительные ресурсы

- [ResourceDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.windows.resourcedictionary)
- [MergedDictionaries](https://docs.microsoft.com/en-us/dotnet/api/system.windows.resourcedictionary.mergeddictionaries)
- [Pack URI](https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf)
