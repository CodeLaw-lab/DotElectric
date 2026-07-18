# Тема 4.2: Конвертеры (IValueConverter, IMultiValueConverter)

### Теория

**Конвертеры** преобразуют значения binding из одного типа в другой.

#### IValueConverter

```csharp
public interface IValueConverter
{
    object Convert(object value, Type targetType, 
                   object parameter, CultureInfo culture);
    
    object ConvertBack(object value, Type targetType, 
                       object parameter, CultureInfo culture);
}
```

| Параметр | Описание |
|----------|----------|
| **value** | Исходное значение |
| **targetType** | Тип целевого свойства |
| **parameter** | Дополнительный параметр из XAML |
| **culture** | Культура для локализации |

#### IMultiValueConverter

```csharp
public interface IMultiValueConverter
{
    object Convert(object[] values, Type targetType, 
                   object parameter, CultureInfo culture);
    
    object[] ConvertBack(object value, Type[] targetTypes, 
                         object parameter, CultureInfo culture);
}
```

### Примеры кода

#### Пример 1: BoolToVisibilityConverter

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Параметр для инверсии
            bool invert = parameter is string s && s.ToLower() == "invert";
            
            if (invert)
                boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        
        return false;
    }
}
```

```xml
<Window.Resources>
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
    <local:BoolToVisibilityConverter x:Key="InvertedBoolVisibility"/>
</Window.Resources>

<StackPanel>
    <!-- Прямое преобразование -->
    <Image Source="/icon.png" 
           Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibility}}"/>
    
    <!-- С инверсией -->
    <TextBlock Text="Hidden when true"
               Visibility="{Binding IsVisible, 
                           Converter={StaticResource BoolToVisibility}, 
                           ConverterParameter=Invert}"/>
</StackPanel>
```

#### Пример 2: DateToStringConverter

```csharp
public class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            string format = parameter as string ?? "dd.MM.yyyy";
            return date.ToString(format, culture);
        }
        
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is string str && DateTime.TryParse(str, out var date))
        {
            return date;
        }
        
        return DateTime.MinValue;
    }
}
```

```xml
<Window.Resources>
    <local:DateToStringConverter x:Key="DateConverter"/>
</Window.Resources>

<StackPanel>
    <TextBlock Text="{Binding BirthDate, Converter={StaticResource DateConverter}}"/>
    <TextBlock Text="{Binding BirthDate, Converter={StaticResource DateConverter}, 
                        ConverterParameter='dd MMMM yyyy'}"/>
    <TextBox Text="{Binding BirthDate, Converter={StaticResource DateConverter}, 
                     Mode=TwoWay}"/>
</StackPanel>
```

#### Пример 3: NullToVisibilityConverter

```csharp
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        bool invert = parameter is string s && s.ToLower() == "invert";
        bool isNull = value == null || (value is string str && string.IsNullOrEmpty(str));
        
        if (invert)
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<Window.Resources>
    <local:NullToVisibilityConverter x:Key="NullToVisibility"/>
</Window.Resources>

<StackPanel>
    <!-- Видно, если не null -->
    <TextBlock Text="{Binding Description}"
               Visibility="{Binding Description, Converter={StaticResource NullToVisibility}}"/>
    
    <!-- Видно, если null (placeholder) -->
    <TextBlock Text="(No description)"
               Visibility="{Binding Description, 
                           Converter={StaticResource NullToVisibility}, 
                           ConverterParameter=Invert}"
               Foreground="Gray"/>
</StackPanel>
```

#### Пример 4: DoubleToThicknessConverter

```csharp
public class DoubleToThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            string sides = parameter as string ?? "all";
            
            return sides.ToLower() switch
            {
                "left" => new Thickness(d, 0, 0, 0),
                "top" => new Thickness(0, d, 0, 0),
                "right" => new Thickness(0, 0, d, 0),
                "bottom" => new Thickness(0, 0, 0, d),
                "horizontal" => new Thickness(d, 0, d, 0),
                "vertical" => new Thickness(0, d, 0, d),
                _ => new Thickness(d)
            };
        }
        
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is Thickness thickness)
        {
            return thickness.Left;
        }
        
        return 0.0;
    }
}
```

```xml
<Window.Resources>
    <local:DoubleToThicknessConverter x:Key="DoubleToThickness"/>
</Window.Resources>

<Border BorderBrush="Blue" 
        BorderThickness="{Binding BorderWidth, 
                          Converter={StaticResource DoubleToThickness}}"/>

<Border BorderBrush="Red"
        BorderThickness="{Binding BorderWidth, 
                          Converter={StaticResource DoubleToThickness}, 
                          ConverterParameter=left}"/>
```

#### Пример 5: MultiBinding с IMultiValueConverter

```csharp
public class FullNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (values.Length >= 2)
        {
            string firstName = values[0]?.ToString() ?? "";
            string lastName = values[1]?.ToString() ?? "";
            
            string format = parameter?.ToString() ?? "{0} {1}";
            
            return string.Format(culture, format, firstName, lastName);
        }
        
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, 
                                object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<Window.Resources>
    <local:FullNameConverter x:Key="FullNameConverter"/>
</Window.Resources>

<StackPanel>
    <!-- Простое форматирование -->
    <TextBlock>
        <TextBlock.Text>
            <MultiBinding Converter="{StaticResource FullNameConverter}">
                <Binding Path="FirstName"/>
                <Binding Path="LastName"/>
            </MultiBinding>
        </TextBlock.Text>
    </TextBlock>
    
    <!-- С параметром формата -->
    <TextBlock>
        <TextBlock.Text>
            <MultiBinding Converter="{StaticResource FullNameConverter}"
                          ConverterParameter="{}{1}, {0}">
                <Binding Path="FirstName"/>
                <Binding Path="LastName"/>
            </MultiBinding>
        </TextBlock.Text>
    </TextBlock>
</StackPanel>
```

#### Пример 6: ArithmeticConverter (с параметром)

```csharp
public class ArithmeticConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is double d && double.TryParse(parameter?.ToString(), out var factor))
        {
            return d * factor;
        }
        
        if (value is int i && int.TryParse(parameter?.ToString(), out var intFactor))
        {
            return i * intFactor;
        }
        
        return value;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is double d && double.TryParse(parameter?.ToString(), out var factor))
        {
            return d / factor;
        }
        
        return value;
    }
}
```

```xml
<Window.Resources>
    <local:ArithmeticConverter x:Key="MultiplyBy100" Parameter="100"/>
    <local:ArithmeticConverter x:Key="DivideBy100" Parameter="0.01"/>
</Window.Resources>

<!-- Проценты → доля (80% → 0.8) -->
<Slider Value="{Binding Percentage, Converter={StaticResource DivideBy100}}" 
        Minimum="0" Maximum="1"/>

<!-- Доля → проценты (0.8 → 80) -->
<TextBlock Text="{Binding Percentage, Converter={StaticResource MultiplyBy100}, 
                    StringFormat={}{0}%}"/>
```

#### Пример 7: EnumToBooleanConverter

```csharp
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;
        
        string enumValue = value.ToString();
        string targetValue = parameter.ToString();
        
        return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return parameter?.ToString();
        }
        
        return Binding.DoNothing;
    }
}
```

```xml
<Window.Resources>
    <local:EnumToBooleanConverter x:Key="EnumToBool"/>
</Window.Resources>

<StackPanel>
    <RadioButton Content="Option 1"
                 IsChecked="{Binding SelectedOption, 
                             Converter={StaticResource EnumToBool}, 
                             ConverterParameter=Option1}"/>
    <RadioButton Content="Option 2"
                 IsChecked="{Binding SelectedOption, 
                             Converter={StaticResource EnumToBool}, 
                             ConverterParameter=Option2}"/>
    <RadioButton Content="Option 3"
                 IsChecked="{Binding SelectedOption, 
                             Converter={StaticResource EnumToBool}, 
                             ConverterParameter=Option3}"/>
</StackPanel>
```

#### Пример 8: Реальные конвертеры из DotElectric

```csharp
// MicronsToPixelConverter.cs
public class MicronsToPixelConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && 
            values[0] is long microns && 
            values[1] is double zoom)
        {
            double mm = microns / 1000.0;
            double pixels = mm * 96.0 / 25.4 * zoom; // 96 DPI, 25.4 mm per inch
            return pixels;
        }
        
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, 
                                object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ZoomToStringConverter.cs
public class ZoomToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is double zoom)
        {
            return $"{zoom * 100:F0}%";
        }
        
        return "100%";
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is string str && 
            str.EndsWith("%") && 
            double.TryParse(str.TrimEnd('%'), out var percent))
        {
            return percent / 100.0;
        }
        
        return 1.0;
    }
}

// LineTypeToDashArrayConverter.cs
public class LineTypeToDashArrayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is LineType lineType)
        {
            return lineType switch
            {
                LineType.Solid => null,
                LineType.Dash => new DoubleCollection { 4, 2 },
                LineType.DashDot => new DoubleCollection { 4, 2, 1, 2 },
                LineType.DashDotDot => new DoubleCollection { 4, 2, 1, 2, 1, 2 },
                _ => null
            };
        }
        
        return null;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<!-- EditorCanvas.xaml -->
<UserControl.Resources>
    <converters:MicronsToPixelConverter x:Key="MicronsToPixelConverter"/>
    <converters:ZoomToStringConverter x:Key="ZoomToStringConverter"/>
    <converters:LineTypeToDashArrayConverter x:Key="LineTypeToDashArrayConverter"/>
</UserControl.Resources>

<!-- Использование MicronsToPixelConverter -->
<Rectangle.Width>
    <MultiBinding Converter="{StaticResource MicronsToPixelConverter}">
        <Binding Path="WidthMicrons"/>
        <Binding Path="Zoom"/>
    </MultiBinding>
</Rectangle.Width>

<!-- Использование ZoomToStringConverter -->
<ComboBox Text="{Binding Zoom, Converter={StaticResource ZoomToStringConverter}}"
          IsEditable="True">
    <ComboBoxItem Content="10%"/>
    <ComboBoxItem Content="25%"/>
    <ComboBoxItem Content="50%"/>
    <ComboBoxItem Content="100%"/>
    <ComboBoxItem Content="200%"/>
</ComboBox>

<!-- Использование LineTypeToDashArrayConverter -->
<Line StrokeDashArray="{Binding LineType, 
                        Converter={StaticResource LineTypeToDashArrayConverter}}"/>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 4.2.1: InverseBoolConverter**

Создайте конвертер, инвертирующий boolean:
- Convert: true → false, false → true
- Используйте для IsEnabled ↔ IsReadOnly

**Задача 4.2.2: StringLengthToVisibilityConverter**

Создайте конвертер:
- Пустая строка → Visibility.Collapsed
- Не пустая → Visibility.Visible

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 4.2.3: PriceConverter**

Создайте конвертер для отображения цены:
- Параметр: валюта (USD, EUR, RUB)
- Формат: $100.00, 100,00 €, 100 ₽
- ConvertBack для ввода пользователем

**Задача 4.2.4: MultiBinding FullName**

Создайте IMultiValueConverter:
- FirstName + LastName → FullName
- Поддержка формата: "{0} {1}", "{1}, {0}"
- Обработка null/empty значений

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 4.2.5: GenericMathConverter**

Создайте универсальный математический конвертер:
- Операции: +, -, *, /
- Parameter: "add:10", "subtract:5", "multiply:2", "divide:100"
- Поддержка double и int

**Задача 4.2.6: ValidationConverter**

Создайте конвертер с валидацией:
- Проверяет значение на соответствие правилам
- Возвращает ValidationError или успешное значение
- Интеграция с валидацией WPF

---

### Решения

<details>
<summary>✅ Решение задачи 4.2.1</summary>

```csharp
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return false;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return false;
    }
}
```

```xml
<Window.Resources>
    <local:InverseBoolConverter x:Key="InverseBool"/>
</Window.Resources>

<TextBox IsReadOnly="{Binding IsEditable, Converter={StaticResource InverseBool}}"/>
```
</details>

<details>
<summary>✅ Решение задачи 4.2.2</summary>

```csharp
public class StringLengthToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrEmpty(str) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 4.2.3</summary>

```csharp
public class PriceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        if (value is decimal price)
        {
            string currency = parameter?.ToString() ?? "USD";
            
            return currency.ToUpper() switch
            {
                "USD" => price.ToString("C", CultureInfo.GetCultureInfo("en-US")),
                "EUR" => price.ToString("C", CultureInfo.GetCultureInfo("de-DE")),
                "RUB" => price.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")) + " ₽",
                _ => price.ToString("C")
            };
        }
        
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            // Удаляем символы валюты и пробелы
            str = Regex.Replace(str, @"[^\d.,-]", "");
            
            if (decimal.TryParse(str, NumberStyles.Any, culture, out var price))
            {
                return price;
            }
        }
        
        return 0m;
    }
}
```
</details>

---

## Ключевые выводы

✅ **IValueConverter** — для преобразования одного значения  
✅ **IMultiValueConverter** — для преобразования нескольких значений  
✅ **Convert** — из источника в цель, **ConvertBack** — обратно  
✅ **ConverterParameter** — передача параметров в конвертер  
✅ **MultiBinding** — объединение нескольких binding в один  
✅ **Кэшируйте** конвертеры в ResourceDictionary  
✅ **Обрабатывайте** null и некорректные значения

---

## Дополнительные ресурсы

- [IValueConverter](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.ivalueconverter)
- [IMultiValueConverter](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.imultivalueconverter)
- [MultiBinding](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.multibinding)
