# Модуль 2: XAML — язык разметки

**Время прохождения:** 13 часов  
**Уровень:** Базовый → Средний

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте синтаксис XAML и пространства имён
- ✅ Научитесь использовать Markup Extensions
- ✅ Освоите ресурсы (StaticResource, DynamicResource)
- ✅ Сможете создавать стили и триггеры
- ✅ Научитесь писать ControlTemplate и DataTemplate

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 2.1 | [Синтаксис XAML](./01_Синтаксис_XAML.md) | 2 часа | Теория, примеры, 6 задач |
| 2.2 | [Markup Extensions](./02_Markup_Extensions.md) | 2 часа | Теория, примеры, 6 задач |
| 2.3 | [Ресурсы](./03_Ресурсы.md) | 2 часа | Теория, примеры, 6 задач |
| 2.4 | [Стили и триггеры](./04_Стили_и_Триггеры.md) | 3 часа | Теория, примеры, 8 задач |
| 2.5 | [Шаблоны](./05_Шаблоны.md) | 3 часа | Теория, примеры, 6 задач |
| 2.6 | [Практическая работа](./06_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модуль 1 (архитектура WPF, DP, Routed Events)
- [ ] Понимаете, что такое Dependency Properties
- [ ] Создавали простые WPF-приложения

---

## Краткое содержание тем

### Тема 2.1: Синтаксис XAML

**Изучите:**
- Базовый синтаксис элементов и атрибутов
- Пространства имён (xmlns)
- Content Property
- Collection Syntax
- Type Converters

**Пример:**
```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:MyApp">
    
    <!-- Content Property -->
    <Button>Click me</Button>
    
    <!-- Collection Syntax -->
    <ListBox.Items>
        <ListBoxItem Content="Item 1"/>
        <ListBoxItem Content="Item 2"/>
    </ListBox.Items>
    
    <!-- Type Converter: string → CornerRadius -->
    <Border CornerRadius="10,20,10,20"/>
</Window>
```

---

### Тема 2.2: Markup Extensions

**Изучите:**
- Встроенные markup extensions
- {Binding}, {StaticResource}, {DynamicResource}
- {x:Static}, {x:Type}, {x:Null}
- Кастомные markup extensions

**Пример:**
```xml
<Window>
    <Window.Resources>
        <sys:String x:Key="AppName">My Application</sys:String>
        <Color x:Key="AccentColor">#0078D4</Color>
        <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    </Window.Resources>
    
    <StackPanel>
        <!-- StaticResource -->
        <TextBlock Text="{StaticResource AppName}"/>
        
        <!-- DynamicResource (может измениться в runtime) -->
        <Button Background="{DynamicResource AccentBrush}"/>
        
        <!-- x:Static -->
        <TextBlock Text="{x:Static SystemInfo.OSVersion}"/>
        
        <!-- x:Type -->
        <Style TargetType="{x:Type Button}"/>
    </StackPanel>
</Window>
```

---

### Тема 2.3: Ресурсы

**Изучите:**
- ResourceDictionary
- Область видимости ресурсов
- StaticResource vs DynamicResource
- MergedDictionaries
- Темы и переключение ресурсов

**Пример:**
```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/LightTheme.xaml"/>
            <ResourceDictionary Source="Styles/Buttons.xaml"/>
        </ResourceDictionary.MergedDictionaries>
        
        <!-- Глобальные ресурсы -->
        <Color x:Key="PrimaryColor">#0078D4</Color>
    </ResourceDictionary>
</Application.Resources>
```

---

### Тема 2.4: Стили и триггеры

**Изучите:**
- Style и Setter
- Property Triggers
- Data Triggers
- Event Triggers
- BasedOn (наследование стилей)

**Пример:**
```xml
<Style x:Key="PrimaryButton" TargetType="Button">
    <Setter Property="Background" Value="#0078D4"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="20,10"/>
    
    <Style.Triggers>
        <!-- Property Trigger -->
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#005A9E"/>
        </Trigger>
        
        <!-- Data Trigger -->
        <DataTrigger Binding="{Binding IsAdmin}" Value="True">
            <Setter Property="ToolTip" Value="Admin mode"/>
        </DataTrigger>
        
        <!-- MultiTrigger -->
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

---

### Тема 2.5: Шаблоны

**Изучите:**
- ControlTemplate (визуальная структура контрола)
- DataTemplate (визуализация данных)
- TemplateBinding
- TemplatePart (именованные части шаблона)
- ItemsPanelTemplate

**Пример ControlTemplate:**
```xml
<ControlTemplate TargetType="Button">
    <Border x:Name="border"
            Background="{TemplateBinding Background}"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}"
            CornerRadius="5">
        <ContentPresenter HorizontalAlignment="Center"
                          VerticalAlignment="Center"/>
    </Border>
    
    <ControlTemplate.Triggers>
        <Trigger Property="IsPressed" Value="True">
            <Setter TargetName="border" Property="Background" Value="#004080"/>
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

**Пример DataTemplate:**
```xml
<ListBox ItemsSource="{Binding Users}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Image Source="{Binding AvatarUrl}" Width="40" Height="40"/>
                <StackPanel Margin="10,0,0,0">
                    <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                    <TextBlock Text="{Binding Email}" 
                               Foreground="Gray"
                               FontSize="12"/>
                </StackPanel>
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

---

## Практическая работа

**Задание:** Создание библиотеки стилей и шаблонов

**Время:** 4 часа

**Требования:**
1. Создать ResourceDictionary с темами (Light/Dark)
2. Разработать 5+ стилей для кнопок
3. Создать DataTemplate для отображения списка сотрудников
4. Реализовать кастомный ControlTemplate для ComboBox
5. Использовать все виды триггеров

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 3 убедитесь, что вы:

- [ ] Понимаете синтаксис XAML и пространства имён
- [ ] Использовали хотя бы 5 markup extensions
- [ ] Создали ResourceDictionary с ресурсами
- [ ] Написали стиль с Property Trigger
- [ ] Создали DataTemplate для коллекции
- [ ] Реализовали ControlTemplate для простого контрола
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **XAML** | Extensible Application Markup Language — язык разметки WPF |
| **Markup Extension** | Синтаксис `{...}` для вычисления значений в runtime |
| **StaticResource** | Однократное получение ресурса (быстрее) |
| **DynamicResource** | Получение ресурса с поддержкой изменений (медленнее) |
| **ControlTemplate** | Шаблон, определяющий визуальную структуру контрола |
| **DataTemplate** | Шаблон для визуализации данных |
| **Trigger** | Изменение свойств при выполнении условия |
| **Setter** | Установка свойства в стиле или триггере |
| **TemplateBinding** | Привязка внутри ControlTemplate к свойствам контрола |

---

## Переход к следующему модулю

➡️ **[Модуль 3: Layout-контейнеры](../Module_03_Layout/README.md)**

В Модуле 3 изучим:
- Grid (Row/Column, GridSplitter, Star Sizing)
- StackPanel, WrapPanel, DockPanel
- Canvas (абсолютное позиционирование)
- Custom Panels (создание собственной панели)
