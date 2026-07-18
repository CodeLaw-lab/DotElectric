# Шпаргалка по Data Binding

## Базовый синтаксис

```xml
<!-- Простой binding -->
<TextBlock Text="{Binding Name}"/>

<!-- С путём -->
<TextBlock Text="{Binding Path=Name}"/>

<!-- С режимом -->
<TextBox Text="{Binding Name, Mode=TwoWay}"/>

<!-- С конвертером -->
<TextBlock Text="{Binding Date, Converter={StaticResource DateConverter}}"/>

<!-- Со строкой формата -->
<TextBlock Text="{Binding Price, StringFormat={}{0:C}}"/>

<!-- С fallback -->
<TextBlock Text="{Binding Name, FallbackValue='Unknown'}"/>

<!-- С target null value -->
<TextBox Text="{Binding Name, TargetNullValue='(empty)'}"/>
```

## Binding Mode

| Mode | Описание | Направление | Пример |
|------|----------|-------------|--------|
| **OneTime** | Один раз при загрузке | Source → Target | `{Binding Name, Mode=OneTime}` |
| **OneWay** | В одну сторону | Source → Target | `{Binding Name, Mode=OneWay}` |
| **TwoWay** | В обе стороны | Source ⇄ Target | `{Binding Name, Mode=TwoWay}` |
| **OneWayToSource** | Обратное направление | Target → Source | `{Binding Name, Mode=OneWayToSource}` |
| **Default** | Зависит от контрола | | TextBox.Text = TwoWay, TextBlock.Text = OneWay |

## UpdateSourceTrigger

| Trigger | Когда обновляет | Пример |
|---------|----------------|--------|
| **Default** | По умолчанию для контрола | `{Binding Name, UpdateSourceTrigger=Default}` |
| **PropertyChanged** | При каждом изменении | `{Binding Name, UpdateSourceTrigger=PropertyChanged}` |
| **LostFocus** | При потере фокуса | `{Binding Name, UpdateSourceTrigger=LostFocus}` |
| **Explicit** | Явно через UpdateSource() | `{Binding Name, UpdateSourceTrigger=Explicit}` |

## Источник данных (Source)

```xml
<!-- DataContext (по умолчанию) -->
<TextBlock Text="{Binding Name}"/>

<!-- ElementName -->
<TextBlock Text="{Binding Text, ElementName=textBox1}"/>

<!-- RelativeSource -->
<TextBlock Text="{Binding Text, RelativeSource={RelativeSource AncestorType=Window}}"/>

<!-- Self -->
<Button Content="{Binding Content, RelativeSource={RelativeSource Self}}"/>

<!-- StaticResource -->
<TextBlock Text="{Binding Name, Source={StaticResource MyViewModel}}"/>

<!-- x:Reference -->
<TextBlock Text="{Binding Text, Source={x:Reference textBox1}}"/>
```

## RelativeSource Modes

```xml
<!-- AncestorType (поиск родителя) -->
<TextBlock Text="{Binding DataContext.Title, 
                    RelativeSource={RelativeSource AncestorType=Window}}"/>

<!-- AncestorLevel (уровень родителя) -->
<TextBlock Text="{Binding Name, 
                    RelativeSource={RelativeSource AncestorType=Grid, AncestorLevel=2}}"/>

<!-- Self (сам элемент) -->
<TextBox Text="{Binding Text, RelativeSource={RelativeSource Self}}"/>

<!-- FindAncestor -->
<TextBlock Text="{Binding DataContext.Name, 
                    RelativeSource={RelativeSource FindAncestor, 
                    AncestorType={x:Type Window}}}"/>

<!-- TemplatedParent (в ControlTemplate) -->
<Border Background="{TemplateBinding Background}"/>
```

## MultiBinding

```xml
<!-- С StringFormat -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} {1}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>

<!-- С конвертером -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>

<!-- С ConverterParameter -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource DateRangeConverter}"
                      ConverterParameter="Short">
            <Binding Path="StartDate"/>
            <Binding Path="EndDate"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

## Конвертеры

```csharp
// IValueConverter
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

// IMultiValueConverter
public class FullNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, 
                          object parameter, CultureInfo culture)
    {
        return $"{values[0]} {values[1]}";
    }
    
    public object[] ConvertBack(object value, Type[] targetTypes, 
                                object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<!-- Использование -->
<Window.Resources>
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
    <local:FullNameConverter x:Key="FullNameConverter"/>
</Window.Resources>

<Image Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibility}}"/>

<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

## Валидация

```csharp
// IDataErrorInfo
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

// INotifyDataErrorInfo (async валидация)
public class Person : INotifyDataErrorInfo
{
    private Dictionary<string, List<string>> _errors = new();
    
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    
    public bool HasErrors => _errors.Any();
    
    public IEnumerable GetErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
    }
    
    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        
        _errors[propertyName].Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
```

```xml
<!-- Binding с валидацией -->
<TextBox>
    <TextBox.Text>
        <Binding Path="Name" 
                 UpdateSourceTrigger="PropertyChanged"
                 ValidatesOnDataErrors="True"
                 ValidatesOnExceptions="True"
                 NotifyOnValidationError="True"/>
    </TextBox.Text>
</TextBox>

<!-- Визуализация ошибки -->
<Style TargetType="TextBox">
    <Style.Triggers>
        <Trigger Property="Validation.HasError" Value="True">
            <Setter Property="BorderBrush" Value="Red"/>
            <Setter Property="ToolTip" 
                    Value="{Binding (Validation.Errors)[0].ErrorContent, 
                    RelativeSource={RelativeSource Self}}"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

## Collection Binding

```csharp
// ObservableCollection (уведомляет об изменениях коллекции)
public ObservableCollection<Person> People { get; } = new();

// Добавление элемента → UI обновится
People.Add(new Person { Name = "John" });

// Удаление элемента → UI обновится
People.RemoveAt(0);
```

```xml
<!-- ListBox с коллекцией -->
<ListBox ItemsSource="{Binding People}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>

<!-- ListView с группировкой -->
<ListView ItemsSource="{Binding People}">
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
    <ListView.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListView.GroupStyle>
</ListView>
```

## Collection Views

```csharp
// Получение представления коллекции
ICollectionView view = CollectionViewSource.GetDefaultView(People);

// Фильтрация
view.Filter = item =>
{
    var person = (Person)item;
    return person.Age >= 18;
};

// Сортировка
view.SortDescriptions.Add(
    new SortDescription("Name", ListSortDirection.Ascending));

// Группировка
view.GroupDescriptions.Add(
    new PropertyGroupDescription("Department"));

// Текущий элемент
var current = view.CurrentItem;
view.MoveCurrentToFirst();
view.MoveCurrentToNext();
```

## Priority Binding

```xml
<!-- Binding с приоритетами (первый успешный) -->
<TextBlock>
    <TextBlock.Text>
        <PriorityBinding>
            <Binding Path="Title"/>
            <Binding Path="DisplayName"/>
            <Binding Path="Name"/>
            <Binding Source="(No Name)"/>
        </PriorityBinding>
    </TextBlock.Text>
</TextBlock>
```

## Binding Debugging

```xml
<!-- Diagnostic output в Output window -->
<TextBlock Text="{Binding Name, diag:PresentationTraceSources.TraceLevel=High}"/>

<!-- FallbackValue для отладки -->
<TextBlock Text="{Binding NonExistentProperty, FallbackValue='DEBUG: Property not found'}"/>

<!-- TargetNullValue для отладки -->
<TextBlock Text="{Binding Name, TargetNullValue='DEBUG: Value is null'}"/>
```

## Common Issues

| Проблема | Решение |
|----------|---------|
| Binding не обновляется | Проверьте `INotifyPropertyChanged` |
| Коллекция не обновляется | Используйте `ObservableCollection` |
| Binding не работает | Проверьте `DataContext` |
| Ошибка в XAML | Проверьте Output window |
| Круговая зависимость | Используйте `Mode=OneWay` |
| Конвертер не вызывается | Проверьте типы `Convert`/`ConvertBack` |

## Best Practices

✅ Используйте `x:Bind` (UWP/WinUI) для производительности  
✅ `OneTime` когда не нужно обновление  
✅ `ObservableCollection` для коллекций  
✅ `CancellationToken` для async операций  
✅ `WeakReference` для предотвращения утечек  
✅ Валидация через `INotifyDataErrorInfo`  
✅ Конвертеры для сложной логики отображения  

❌ Не используйте `TwoWay` без необходимости  
❌ Не забывайте отписываться от событий  
❌ Не блокируйте UI-thread в конвертерах  
❌ Не используйте `DynamicResource` без необходимости  

---

## Быстрая справка

### INotifyPropertyChanged

```csharp
public class Person : INotifyPropertyChanged
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

### CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class Person : ObservableObject
{
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private int _age;
}

// Генерируется автоматически:
// - Name property с INotifyPropertyChanged
// - Age property с INotifyPropertyChanged
```
