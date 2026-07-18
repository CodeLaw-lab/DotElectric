# Модуль 8: Кастомные контролы и Behaviors

**Время прохождения:** 12 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте разницу между UserControls и Custom Controls
- ✅ Научитесь создавать Templated Controls (Themes/Generic.xaml)
- ✅ Освоите Attached Behaviors
- ✅ Сможете использовать Interactivity (System.Windows.Interactivity)

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 8.1 | [UserControls vs Custom Controls](./01_UserControls_vs_Custom.md) | 3 часа | Теория, примеры, 5 задач |
| 8.2 | [Templated Controls](./02_Templated_Controls.md) | 3 часа | Теория, примеры, 5 задач |
| 8.3 | [Attached Behaviors](./03_Attached_Behaviors.md) | 3 часа | Теория, примеры, 6 задач |
| 8.4 | [Interactivity](./04_Interactivity.md) | 2 часа | Теория, примеры, 4 задачи |
| 8.5 | [Практическая работа](./05_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-7
- [ ] Понимаете Dependency Properties
- [ ] Работали с ControlTemplate
- [ ] Знаете основы C# (классы, наследование, интерфейсы)

---

## Краткое содержание тем

### Тема 8.1: UserControls vs Custom Controls

**Изучите:**
- UserControl — композиция существующих контролов
- Custom Control — новый контроль с нуля
- Когда использовать каждый подход
- Шаблоны проектов

**Пример:**
```csharp
// UserControl — композиция
public partial class PersonControl : UserControl
{
    public PersonControl()
    {
        InitializeComponent();
    }
}

// Custom Control — новый контроль
public class CustomButton : Button
{
    static CustomButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CustomButton),
            new FrameworkPropertyMetadata(typeof(CustomButton)));
    }
}
```

---

### Тема 8.2: Templated Controls

**Изучите:**
- Themes/Generic.xaml
- DefaultStyleKeyProperty
- TemplatePart атрибут
- OnApplyTemplate override

**Пример:**
```csharp
[TemplatePart(Name = "PART_ButtonBorder", Type = typeof(Border))]
public class CustomButton : Button
{
    static CustomButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CustomButton),
            new FrameworkPropertyMetadata(typeof(CustomButton)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var border = GetTemplateChild("PART_ButtonBorder") as Border;
    }
}
```

---

### Тема 8.3: Attached Behaviors

**Изучите:**
- Attached Properties для поведения
- Event to Command
- Code-behind elimination
- Reusable behaviors

**Пример:**
```csharp
public static class TextBoxBehavior
{
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(TextBoxBehavior),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    private static void OnNumericOnlyChanged(DependencyObject d, 
                                              DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox && (bool)e.NewValue)
        {
            textBox.PreviewTextInput += TextBox_PreviewTextInput;
        }
    }

    private static void TextBox_PreviewTextInput(object sender, 
                                                  TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }
}
```

---

### Тема 8.4: Interactivity

**Изучите:**
- System.Windows.Interactivity
- Trigger, Action, Behavior
- EventToCommand
- DataTriggers в XAML

**Пример:**
```xml
xmlns:i="clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity"

<Button Content="Click">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
            <i:InvokeCommandAction Command="{Binding ClickCommand}"/>
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>
```

---

## Практическая работа

**Задание:** Создание библиотеки кастомных контролов

**Время:** 4 часа

**Требования:**
1. UserControl с композицией
2. Custom Control с Template
3. Attached Behavior
4. Interactivity Trigger

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 9 убедитесь, что вы:

- [ ] Создали UserControl с Dependency Properties
- [ ] Создали Custom Control с Themes/Generic.xaml
- [ ] Реализовали Attached Behavior
- [ ] Использовали EventToCommand
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **UserControl** | Композиция существующих контролов |
| **Custom Control** | Новый контроль с собственным шаблоном |
| **DefaultStyleKey** | Ключ стиля по умолчанию для контроля |
| **Generic.xaml** | Файл тем по умолчанию |
| **TemplatePart** | Атрибут для именованных частей шаблона |
| **Attached Behavior** | Поведение через Attached Properties |
| **Behavior** | Переиспользуемая логика для элементов |
| **Trigger** | Реакция на событие или изменение свойства |
| **Action** | Действие, выполняемое по триггеру |
| **EventToCommand** | Преобразование события в команду |

---

## Переход к следующему модулю

➡️ **[Модуль 9: Ресурсы и темы](../Module_09_Resources/README.md)**

В Модуле 9 изучим:
- ResourceDictionary, MergedDictionaries
- Темы (Light/Dark)
- Динамическая смена тем
- MaterialDesignThemes интеграция
