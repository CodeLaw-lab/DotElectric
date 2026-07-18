# Модуль 4: Data Binding

**Время прохождения:** 18 часов  
**Уровень:** Средний → Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте основы Data Binding (Source, Path, Mode, UpdateSourceTrigger)
- ✅ Научитесь создавать конвертеры (IValueConverter, IMultiValueConverter)
- ✅ Освоите binding к коллекциям (ObservableCollection, ICollectionView)
- ✅ Сможете реализовать валидацию (IDataErrorInfo, INotifyDataErrorInfo)
- ✅ Научитесь использовать RelativeSource, ElementName, FindAncestor

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 4.1 | [Основы Data Binding](./01_Binding_Basics.md) | 3 часа | Теория, примеры, 6 задач |
| 4.2 | [Конвертеры](./02_Converters.md) | 3 часа | Теория, примеры, 6 задач |
| 4.3 | [Binding к коллекциям](./03_Collections.md) | 4 часа | Теория, примеры, 6 задач |
| 4.4 | [Валидация](./04_Validation.md) | 4 часа | Теория, примеры, 6 задач |
| 4.5 | [Advanced Binding](./05_Advanced_Binding.md) | 3 часа | Теория, примеры, 6 задач |
| 4.6 | [Практическая работа](./06_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модуль 1 (архитектура WPF, DP, Routed Events)
- [ ] Прошли Модуль 2 (XAML, ресурсы, стили)
- [ ] Прошли Модуль 3 (Layout-контейнеры)
- [ ] Понимаете, что такое Dependency Properties
- [ ] Знаете основы C# (свойства, события, классы)

---

## Краткое содержание тем

### Тема 4.1: Основы Data Binding

**Изучите:**
- Синтаксис binding: `{Binding Path=PropertyName}`
- Режимы: OneTime, OneWay, TwoWay, OneWayToSource, Default
- UpdateSourceTrigger: Default, PropertyChanged, LostFocus, Explicit
- Источник данных: DataContext, ElementName, Source, RelativeSource

**Пример:**
```xml
<!-- Простой binding -->
<TextBlock Text="{Binding Name}"/>

<!-- С режимом -->
<TextBox Text="{Binding Name, Mode=TwoWay}"/>

<!-- С UpdateSourceTrigger -->
<TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>

<!-- С элементом -->
<TextBlock Text="{Binding Text, ElementName=textBox1}"/>
```

---

### Тема 4.2: Конвертеры

**Изучите:**
- IValueConverter для одностороннего преобразования
- IMultiValueConverter для нескольких значений
- ConverterParameter для передачи параметров
- Встроенные конвертеры

**Пример:**
```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, 
                              object parameter, CultureInfo culture)
    {
        return (Visibility)value == Visibility.Visible;
    }
}
```

```xml
<Window.Resources>
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
</Window.Resources>

<Image Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibility}}"/>
```

---

### Тема 4.3: Binding к коллекциям

**Изучите:**
- ObservableCollection<T> для автоматических уведомлений
- ICollectionView для фильтрации, сортировки, группировки
- DataTemplate для визуализации элементов
- ItemsControl, ListBox, ListView, ComboBox

**Пример:**
```csharp
public ObservableCollection<Person> People { get; } = new();

// Добавление → UI обновится автоматически
People.Add(new Person { Name = "John" });
```

```xml
<ListBox ItemsSource="{Binding People}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

---

### Тема 4.4: Валидация

**Изучите:**
- IDataErrorInfo для простой валидации
- INotifyDataErrorInfo для async валидации
- ValidatesOnDataErrors, ValidatesOnExceptions
- Визуализация ошибок

**Пример:**
```csharp
public class Person : IDataErrorInfo
{
    public string Name { get; set; }
    
    public string this[string columnName]
    {
        get
        {
            if (columnName == "Name" && string.IsNullOrEmpty(Name))
                return "Name is required";
            return null;
        }
    }
    
    public string Error => null;
}
```

```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="Name" 
                 ValidatesOnDataErrors="True"
                 NotifyOnValidationError="True"/>
    </TextBox.Text>
</TextBox>
```

---

### Тема 4.5: RelativeSource, ElementName

**Изучите:**
- ElementName — binding к элементу по имени
- RelativeSource — binding относительно текущего элемента
- AncestorType — поиск родителя по типу
- Self — binding к самому себе

**Пример:**
```xml
<!-- ElementName -->
<TextBlock Text="{Binding Text, ElementName=textBox1}"/>

<!-- RelativeSource AncestorType -->
<TextBlock Text="{Binding DataContext.Title, 
                    RelativeSource={RelativeSource AncestorType=Window}}"/>

<!-- Self -->
<Button Content="{Binding Content, RelativeSource={RelativeSource Self}}"/>
```

---

## Практическая работа

**Задание:** Создание приложения со связанными данными

**Время:** 4 часа

**Требования:**
1. ViewModel с INotifyPropertyChanged
2. Binding к свойствам (TwoWay, OneWay)
3. Конвертеры (BoolToVisibility, DateToString)
4. ObservableCollection с коллекцией элементов
5. Валидация (IDataErrorInfo)
6. ListBox/ListView с DataTemplate
7. Filter/Sort через ICollectionView

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 5 убедитесь, что вы:

- [ ] Понимаете режимы binding (OneTime, OneWay, TwoWay)
- [ ] Использовали UpdateSourceTrigger
- [ ] Создали хотя бы один IValueConverter
- [ ] Работали с ObservableCollection
- [ ] Реализовали валидацию через IDataErrorInfo
- [ ] Использовали RelativeSource AncestorType
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Binding** | Механизм связывания свойства UI со свойством источника |
| **DataContext** | Контекст данных элемента (источник binding по умолчанию) |
| **Mode** | Направление binding (OneWay, TwoWay, etc.) |
| **UpdateSourceTrigger** | Когда обновлять источник (PropertyChanged, LostFocus) |
| **Converter** | Преобразователь значений binding |
| **ObservableCollection** | Коллекция с уведомлением об изменениях |
| **ICollectionView** | Представление коллекции (фильтр, сортировка, группировка) |
| **IDataErrorInfo** | Интерфейс для валидации свойств |
| **INotifyDataErrorInfo** | Интерфейс для async валидации |
| **RelativeSource** | Источник binding относительно текущего элемента |

---

## Переход к следующему модулю

➡️ **[Модуль 5: MVVM-паттерн](../Module_05_MVVM/README.md)**

В Модуле 5 изучим:
- Model-View-ViewModel: разделение ответственности
- INotifyPropertyChanged, CommunityToolkit.Mvvm
- Commands (ICommand, RelayCommand, AsyncRelayCommand)
- Dependency Injection в WPF
- Messaging (WeakReferenceMessenger)
