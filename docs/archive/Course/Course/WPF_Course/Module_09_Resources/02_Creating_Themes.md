# Тема 9.2: Создание тем (Light/Dark)

### Теория

**Темы** — наборы ресурсов для визуального стиля приложения.

#### Базовые темы

| Тема | Описание | Цвета |
|------|----------|-------|
| **Light** | Светлая тема | Белый фон, тёмный текст |
| **Dark** | Тёмная тема | Тёмный фон, светлый текст |

#### Структура файлов тем

```
Resources/
├── Themes/
│   ├── LightTheme.xaml
│   └── DarkTheme.xaml
├── Styles/
│   ├── Colors.xaml
│   ├── Buttons.xaml
│   └── TextBoxes.xaml
```

### Примеры кода

#### Пример 1: LightTheme.xaml

```xml
<!-- Resources/Themes/LightTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Основные цвета -->
    <Color x:Key="WindowBackgroundColor">#FFFFFF</Color>
    <Color x:Key="PanelBackgroundColor">#F5F5F5</Color>
    <Color x:Key="CardBackgroundColor">#FFFFFF</Color>
    <Color x:Key="TextPrimaryColor">#212121</Color>
    <Color x:Key="TextSecondaryColor">#757575</Color>
    <Color x:Key="BorderColor">#E0E0E0</Color>
    <Color x:Key="AccentColor">#0078D4</Color>
    <Color x:Key="ErrorColor">#DC3545</Color>
    <Color x:Key="SuccessColor">#28A745</Color>
    
    <!-- Кисти -->
    <SolidColorBrush x:Key="WindowBackgroundBrush" Color="{StaticResource WindowBackgroundColor}"/>
    <SolidColorBrush x:Key="PanelBackgroundBrush" Color="{StaticResource PanelBackgroundColor}"/>
    <SolidColorBrush x:Key="CardBackgroundBrush" Color="{StaticResource CardBackgroundColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource BorderColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="SuccessBrush" Color="{StaticResource SuccessColor}"/>
    
    <!-- Стили для TextBox -->
    <Style TargetType="TextBox">
        <Setter Property="Background" Value="{StaticResource CardBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="8,4"/>
        <Style.Triggers>
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="{StaticResource AccentBrush}"/>
                <Setter Property="BorderThickness" Value="2"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <!-- Стили для ComboBox -->
    <Style TargetType="ComboBox">
        <Setter Property="Background" Value="{StaticResource CardBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>
</ResourceDictionary>
```

#### Пример 2: DarkTheme.xaml

```xml
<!-- Resources/Themes/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Основные цвета -->
    <Color x:Key="WindowBackgroundColor">#212121</Color>
    <Color x:Key="PanelBackgroundColor">#303030</Color>
    <Color x:Key="CardBackgroundColor">#424242</Color>
    <Color x:Key="TextPrimaryColor">#FFFFFF</Color>
    <Color x:Key="TextSecondaryColor">#BDBDBD</Color>
    <Color x:Key="BorderColor">#555555</Color>
    <Color x:Key="AccentColor">#42A5F5</Color>
    <Color x:Key="ErrorColor">#EF5350</Color>
    <Color x:Key="SuccessColor">#66BB6A</Color>
    
    <!-- Кисти -->
    <SolidColorBrush x:Key="WindowBackgroundBrush" Color="{StaticResource WindowBackgroundColor}"/>
    <SolidColorBrush x:Key="PanelBackgroundBrush" Color="{StaticResource PanelBackgroundColor}"/>
    <SolidColorBrush x:Key="CardBackgroundBrush" Color="{StaticResource CardBackgroundColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource BorderColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="SuccessBrush" Color="{StaticResource SuccessColor}"/>
    
    <!-- Стили для TextBox -->
    <Style TargetType="TextBox">
        <Setter Property="Background" Value="{StaticResource CardBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="8,4"/>
        <Style.Triggers>
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="{StaticResource AccentBrush}"/>
                <Setter Property="BorderThickness" Value="2"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</ResourceDictionary>
```

#### Пример 3: Theme Colors (палитра)

```xml
<!-- Resources/Themes/ThemeColors.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Material Design Color Palette -->
    <!-- Primary -->
    <Color x:Key="Primary50">#E3F2FD</Color>
    <Color x:Key="Primary100">#BBDEFB</Color>
    <Color x:Key="Primary200">#90CAF9</Color>
    <Color x:Key="Primary300">#64B5F6</Color>
    <Color x:Key="Primary400">#42A5F5</Color>
    <Color x:Key="Primary500">#2196F3</Color>
    <Color x:Key="Primary600">#1E88E5</Color>
    <Color x:Key="Primary700">#1976D2</Color>
    <Color x:Key="Primary800">#1565C0</Color>
    <Color x:Key="Primary900">#0D47A1</Color>
    
    <!-- Accent -->
    <Color x:Key="Accent100">#FF8A80</Color>
    <Color x:Key="Accent200">#FF5252</Color>
    <Color x:Key="Accent400">#FF1744</Color>
    <Color x:Key="Accent700">#D50000</Color>
</ResourceDictionary>
```

#### Пример 4: Стили для всей темы

```xml
<!-- Resources/Styles/AllStyles.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Button Styles -->
    <Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="Background" Value="{DynamicResource AccentBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="20,10"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Opacity" Value="0.9"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <!-- ListBox Styles -->
    <Style TargetType="ListBox">
        <Setter Property="Background" Value="{DynamicResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource BorderBrush}"/>
    </Style>
    
    <!-- DataGrid Styles -->
    <Style TargetType="DataGrid">
        <Setter Property="Background" Value="{DynamicResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource BorderBrush}"/>
        <Setter Property="RowBackground" Value="{DynamicResource CardBackgroundBrush}"/>
    </Style>
    
    <!-- Menu Styles -->
    <Style TargetType="Menu">
        <Setter Property="Background" Value="{DynamicResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
    </Style>
    
    <!-- StatusBar Styles -->
    <Style TargetType="StatusBar">
        <Setter Property="Background" Value="{DynamicResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
    </Style>
</ResourceDictionary>
```

#### Пример 5: Реальное использование из DotElectric

```xml
<!-- Resources/Styles/DarkTheme.xaml (полный пример) -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    
    <!-- 310 строк стилей для DotElectric -->
    
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
    
    <!-- ToolBar Styles -->
    <Style x:Key="ToolBarButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="8"/>
        <Setter Property="Margin" Value="2,0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#3D3D3D"/>
            </Trigger>
            <Trigger Property="IsPressed" Value="True">
                <Setter Property="Background" Value="#0078D4"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <Style x:Key="ToolBarToggleButtonStyle" TargetType="ToggleButton">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="8"/>
        <Style.Triggers>
            <Trigger Property="IsChecked" Value="True">
                <Setter Property="Background" Value="#0078D4"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <Style x:Key="ToolBarSeparatorStyle" TargetType="Separator">
        <Setter Property="Background" Value="{StaticResource BorderBrush}"/>
        <Setter Property="Width" Value="1"/>
        <Setter Property="Margin" Value="4,4"/>
    </Style>
    
    <!-- ComboBox Styles -->
    <Style TargetType="ComboBox">
        <Setter Property="Background" Value="{StaticResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="8,4"/>
        <Setter Property="FontSize" Value="13"/>
    </Style>
    
    <!-- StatusBar Styles -->
    <Style TargetType="StatusBar">
        <Setter Property="Background" Value="{StaticResource PanelBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
        <Setter Property="Height" Value="25"/>
    </Style>
</ResourceDictionary>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 9.2.1: LightTheme**

Создайте LightTheme.xaml:
- WindowBackgroundColor (белый)
- TextPrimaryColor (тёмный)
- AccentColor (синий)

**Задача 9.2.2: DarkTheme**

Создайте DarkTheme.xaml:
- WindowBackgroundColor (тёмный)
- TextPrimaryColor (светлый)
- AccentColor (светло-синий)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 9.2.3: Complete Theme**

Создайте полную тему:
- 10+ цветов
- 10+ кистей
- Стили для TextBox, Button, ComboBox

**Задача 9.2.4: Color Palette**

Создайте палитру:
- Primary 50-900
- Accent 100-700
- Material Design стиль

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 9.2.5: Theme Generator**

Создайте генератор тем:
- Входные цвета
- Генерация оттенков
- Вывод ResourceDictionary

**Задача 9.2.6: Adaptive Theme**

Создайте адаптивную тему:
- Авто-определение системной темы
- Плавный переход
- Сохранение предпочтений

---

### Решения

<details>
<summary>✅ Решение задачи 9.2.1</summary>

```xml
<!-- Resources/Themes/LightTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Color x:Key="WindowBackgroundColor">#FFFFFF</Color>
    <Color x:Key="TextPrimaryColor">#212121</Color>
    <Color x:Key="AccentColor">#0078D4</Color>
    
    <SolidColorBrush x:Key="WindowBackgroundBrush" Color="{StaticResource WindowBackgroundColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
</ResourceDictionary>
```
</details>

<details>
<summary>✅ Решение задачи 9.2.2</summary>

```xml
<!-- Resources/Themes/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Color x:Key="WindowBackgroundColor">#212121</Color>
    <Color x:Key="TextPrimaryColor">#FFFFFF</Color>
    <Color x:Key="AccentColor">#42A5F5</Color>
    
    <SolidColorBrush x:Key="WindowBackgroundBrush" Color="{StaticResource WindowBackgroundColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
</ResourceDictionary>
```
</details>

---

## Ключевые выводы

✅ **Light/Dark темы** — наборы цветов и стилей  
✅ **DynamicResource** — для переключения тем  
✅ **Color → SolidColorBrush** — каскадное определение  
✅ **MergedDictionaries** — подключение тем  
✅ **Стилизация всех контролов** — единообразие  
✅ **Material Design** — популярная палитра

---

## Дополнительные ресурсы

- [Material Design Colors](https://material.io/design/color/the-color-system.html)
- [Theming in WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/styles-templates)
- [ResourceDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.windows.resourcedictionary)
