# Тема 4.1: Основы Data Binding

### Теория

**Data Binding** — механизм связывания свойств UI со свойствами источника данных (обычно ViewModel).

#### Базовый синтаксис

```xml
<!-- Простой binding -->
<TextBlock Text="{Binding Name}"/>

<!-- С указанием пути -->
<TextBlock Text="{Binding Path=Name}"/>

<!-- С режимом -->
<TextBox Text="{Binding Name, Mode=TwoWay}"/>

<!-- С обновлением источника -->
<TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>
```

#### Binding Modes

| Mode | Описание | Направление | Пример использования |
|------|----------|-------------|---------------------|
| **OneTime** | Один раз при загрузке | Source → Target | Статичные данные |
| **OneWay** | В одну сторону | Source → Target | TextBlock, Label |
| **TwoWay** | В обе стороны | Source ⇄ Target | TextBox, CheckBox |
| **OneWayToSource** | Только в источник | Target → Source | Специфичные сценарии |
| **Default** | По умолчанию для контрола | Зависит от свойства | TextBox.Text = TwoWay |

#### UpdateSourceTrigger

| Trigger | Когда обновляет | Пример |
|---------|----------------|--------|
| **Default** | По умолчанию для свойства | TextBox.Text = LostFocus |
| **PropertyChanged** | При каждом изменении | Для поиска в реальном времени |
| **LostFocus** | При потере фокуса | Для форм ввода |
| **Explicit** | Явно через код | Для кнопки "Применить" |

#### Источник данных (Source)

```xml
<!-- DataContext (по умолчанию) -->
<TextBlock Text="{Binding Name}"/>

<!-- ElementName (другой элемент) -->
<TextBlock Text="{Binding Text, ElementName=textBox1}"/>

<!-- StaticResource -->
<TextBlock Text="{Binding Name, Source={StaticResource MyViewModel}}"/>

<!-- RelativeSource (родитель) -->
<TextBlock Text="{Binding DataContext.Title, 
                    RelativeSource={RelativeSource AncestorType=Window}}"/>
```

### Примеры кода

#### Пример 1: Простой OneWay binding

```csharp
// ViewModel
public class PersonViewModel
{
    public string Name { get; } = "John Doe";
    public int Age { get; } = 30;
    public string Email { get; } = "john@example.com";
}
```

```xml
<!-- View -->
<Window.DataContext>
    <local:PersonViewModel/>
</Window.DataContext>

<StackPanel Margin="20">
    <TextBlock Text="{Binding Name}"/>
    <TextBlock Text="{Binding Age}"/>
    <TextBlock Text="{Binding Email}"/>
</StackPanel>
```

#### Пример 2: TwoWay binding с TextBox

```csharp
// ViewModel с INotifyPropertyChanged
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name;
    private string _email;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

```xml
<StackPanel Margin="20">
    <TextBlock Text="Имя:"/>
    <TextBox Text="{Binding Name, Mode=TwoWay}" Margin="0,5,0,15"/>
    
    <TextBlock Text="Email:"/>
    <TextBox Text="{Binding Email, Mode=TwoWay}" Margin="0,5,0,15"/>
    
    <!-- Отображение в реальном времени -->
    <TextBlock Text="Preview:" FontWeight="Bold"/>
    <TextBlock Text="{Binding Name, StringFormat=Привет, {0}!}"/>
    <TextBlock Text="{Binding Email, StringFormat='Email: {0}'}"/>
</StackPanel>
```

#### Пример 3: UpdateSourceTrigger

```xml
<StackPanel Margin="20">
    <!-- LostFocus (по умолчанию для TextBox) -->
    <TextBlock Text="LostFocus (default):"/>
    <TextBox Text="{Binding SearchQuery, Mode=TwoWay}" 
             Margin="0,5,0,15"/>
    
    <!-- PropertyChanged (обновление при каждом нажатии) -->
    <TextBlock Text="PropertyChanged (real-time):"/>
    <TextBox Text="{Binding SearchQuery, Mode=TwoWay, 
                              UpdateSourceTrigger=PropertyChanged}" 
             Margin="0,5,0,15"/>
    
    <!-- Explicit (только по кнопке) -->
    <TextBlock Text="Explicit (by button):"/>
    <TextBox x:Name="explicitTextBox" 
             Text="{Binding SearchQuery, Mode=TwoWay, 
                            UpdateSourceTrigger=Explicit}"
             Margin="0,5,0,15"/>
    <Button Content="Apply" Click="Apply_Click"/>
</StackPanel>
```

```csharp
private void Apply_Click(object sender, RoutedEventArgs e)
{
    // Явное обновление источника
    var binding = explicitTextBox.GetBindingExpression(TextBox.TextProperty);
    binding?.UpdateSource();
}
```

#### Пример 4: Binding к вложенным свойствам

```csharp
public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public string ZipCode { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public Address Address { get; set; }
}
```

```xml
<StackPanel>
    <TextBlock Text="{Binding Name}"/>
    <TextBlock Text="{Binding Address.City}"/>
    <TextBlock Text="{Binding Address.Street}"/>
    <TextBlock Text="{Binding Address.ZipCode}"/>
</StackPanel>
```

#### Пример 5: StringFormat

```xml
<StackPanel Margin="20">
    <!-- Простой формат -->
    <TextBlock Text="{Binding Name, StringFormat='Hello, {0}!'}"/>
    
    <!-- Формат даты -->
    <TextBlock Text="{Binding BirthDate, StringFormat={}{0:dd.MM.yyyy}}"/>
    
    <!-- Формат числа -->
    <TextBlock Text="{Binding Salary, StringFormat={}{0:C}}"/>
    
    <!-- Формат с несколькими полями -->
    <TextBlock>
        <TextBlock.Text>
            <MultiBinding StringFormat="{}{0} {1} ({2})">
                <Binding Path="FirstName"/>
                <Binding Path="LastName"/>
                <Binding Path="Age"/>
            </MultiBinding>
        </TextBlock.Text>
    </TextBlock>
</StackPanel>
```

#### Пример 6: FallbackValue и TargetNullValue

```xml
<StackPanel>
    <!-- FallbackValue при ошибке binding -->
    <TextBlock Text="{Binding NonExistentProperty, 
                                FallbackValue='Property not found'}"/>
    
    <!-- TargetNullValue при null значении -->
    <TextBlock Text="{Binding Name, 
                                TargetNullValue='(No name)'}"/>
    
    <!-- Комбинация -->
    <TextBlock Text="{Binding Description, 
                                TargetNullValue='(No description)',
                                FallbackValue='Error loading'}"/>
</StackPanel>
```

#### Пример 7: Binding к коллекциям

```csharp
public class MainViewModel
{
    public ObservableCollection<string> Items { get; } = new()
    {
        "Item 1",
        "Item 2",
        "Item 3"
    };
}
```

```xml
<ListBox ItemsSource="{Binding Items}"/>
```

#### Пример 8: Реальное использование из DotElectric

```csharp
// EditorViewModel.cs
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private bool _statusBarGridEnabled;

    [ObservableProperty]
    private double _statusBarGridStepMm = 5.0;

    [ObservableProperty]
    private string _title = "Untitled";

    [ObservableProperty]
    private bool _isDirty;

    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
}
```

```xml
<!-- MainWindow.xaml -->
<Window>
    <!-- Zoom ComboBox -->
    <ComboBox Width="90" Height="30"
              Text="{Binding ZoomPercent, StringFormat={}{0}%}"
              IsEditable="True">
        <ComboBoxItem Content="10%"/>
        <ComboBoxItem Content="25%"/>
        <ComboBoxItem Content="50%"/>
        <ComboBoxItem Content="100%"/>
        <ComboBoxItem Content="200%"/>
    </ComboBox>
    
    <!-- Grid Toggle -->
    <ToggleButton IsChecked="{Binding StatusBarGridEnabled}"
                  ToolTip="Сетка (F7)"/>
    
    <!-- Grid Step -->
    <ComboBox Width="75"
              Text="{Binding StatusBarGridStepMm, StringFormat={}{0} мм}">
        <ComboBoxItem Content="1 мм"/>
        <ComboBoxItem Content="2 мм"/>
        <ComboBoxItem Content="5 мм"/>
        <ComboBoxItem Content="10 мм"/>
    </ComboBox>
    
    <!-- Status Bar -->
    <StatusBar>
        <TextBlock Text="{Binding Title}"/>
        <TextBlock Text="{Binding IsDirty, Converter={StaticResource DirtyConverter}}"/>
    </StatusBar>
</Window>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 4.1.1: Простой ViewModel**

Создайте ViewModel с свойствами:
- Name (string)
- Age (int)
- Email (string)

Отобразите в View через TextBlock с OneWay binding.

**Задача 4.1.2: TwoWay binding форма**

Создайте форму с TextBox:
- Name (TwoWay binding)
- Email (TwoWay binding)
- Отображение введённых данных ниже формы

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 4.1.3: Person Editor**

Создайте редактор персоны:
- ViewModel с INotifyPropertyChanged
- Форма: Name, Age, Email, Address (City, Street)
- Preview секция с форматированным выводом
- Кнопки "Reset" (сброс к исходным значениям)

**Задача 4.1.4: Search Box**

Реализуйте поиск с разными UpdateSourceTrigger:
- TextBox 1: LostFocus (default)
- TextBox 2: PropertyChanged (real-time)
- TextBox 3: Explicit (по кнопке)
- ListBox с результатами "поиска" (фильтрация коллекции)

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 4.1.5: Master-Detail**

Создайте Master-Detail view:
- ListBox со списком людей
- Detail секция с редактированием выбранного
- Binding через ElementName
- Синхронизация выделения

**Задача 4.1.6: Dynamic Binding**

Реализуйте динамический binding:
- ComboBox для выбора свойства (Name, Age, Email)
- TextBlock отображает значение выбранного свойства
- Используйте binding с динамическим Path

---

### Решения

<details>
<summary>✅ Решение задачи 4.1.1</summary>

```csharp
public class PersonViewModel
{
    public string Name { get; } = "John Doe";
    public int Age { get; } = 30;
    public string Email { get; } = "john@example.com";
}
```

```xml
<Window.DataContext>
    <local:PersonViewModel/>
</Window.DataContext>

<StackPanel Margin="20">
    <TextBlock Text="Name: "/>
    <TextBlock Text="{Binding Name}" Margin="0,5,0,15"/>
    
    <TextBlock Text="Age: "/>
    <TextBlock Text="{Binding Age}" Margin="0,5,0,15"/>
    
    <TextBlock Text="Email: "/>
    <TextBlock Text="{Binding Email}" Margin="0,5,0,15"/>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 4.1.2</summary>

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name;
    private string _email;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel Margin="20">
    <TextBlock Text="Name:"/>
    <TextBox Text="{Binding Name, Mode=TwoWay}" Margin="0,5,0,15"/>
    
    <TextBlock Text="Email:"/>
    <TextBox Text="{Binding Email, Mode=TwoWay}" Margin="0,5,0,15"/>
    
    <Separator Margin="0,20"/>
    
    <TextBlock Text="Preview:" FontWeight="Bold"/>
    <TextBlock Text="{Binding Name, StringFormat='Name: {0}'}"/>
    <TextBlock Text="{Binding Email, StringFormat='Email: {0}'}"/>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 4.1.3</summary>

```csharp
public class PersonEditorViewModel : INotifyPropertyChanged
{
    private string _name;
    private int _age;
    private string _email;
    private string _city;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public int Age
    {
        get => _age;
        set { _age = value; OnPropertyChanged(nameof(Age)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public string City
    {
        get => _city;
        set { _city = value; OnPropertyChanged(nameof(City)); }
    }

    // Original values for reset
    private string _originalName;
    
    public void SaveOriginal()
    {
        _originalName = Name;
        // Save others...
    }
    
    public void Reset()
    {
        Name = _originalName;
        // Reset others...
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Form -->
    <Grid Grid.Row="0">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <TextBlock Grid.Row="0" Grid.Column="0" Text="Name:" Margin="5"/>
        <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" Margin="5"/>
        
        <TextBlock Grid.Row="1" Grid.Column="0" Text="Age:" Margin="5"/>
        <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding Age, UpdateSourceTrigger=PropertyChanged}" Margin="5"/>
        
        <TextBlock Grid.Row="2" Grid.Column="0" Text="Email:" Margin="5"/>
        <TextBox Grid.Row="2" Grid.Column="1" Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}" Margin="5"/>
        
        <TextBlock Grid.Row="3" Grid.Column="0" Text="City:" Margin="5"/>
        <TextBox Grid.Row="3" Grid.Column="1" Text="{Binding City, UpdateSourceTrigger=PropertyChanged}" Margin="5"/>
    </Grid>
    
    <!-- Preview -->
    <Border Grid.Row="1" BorderBrush="Gray" BorderThickness="1" Padding="10" Margin="0,20">
        <StackPanel>
            <TextBlock Text="Preview:" FontWeight="Bold"/>
            <TextBlock Text="{Binding Name, StringFormat='Name: {0}'}"/>
            <TextBlock Text="{Binding Age, StringFormat='Age: {0}'}"/>
            <TextBlock Text="{Binding Email, StringFormat='Email: {0}'}"/>
            <TextBlock Text="{Binding City, StringFormat='City: {0}'}"/>
        </StackPanel>
    </Border>
    
    <!-- Buttons -->
    <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
        <Button Content="Reset" Command="{Binding ResetCommand}" Padding="20,5" Margin="0,0,10,0"/>
        <Button Content="Save" Padding="20,5"/>
    </StackPanel>
</Grid>
```
</details>

---

## Ключевые выводы

✅ **Binding** связывает свойство UI со свойством источника  
✅ **Mode:** OneTime (один раз), OneWay (в одну сторону), TwoWay (в обе)  
✅ **UpdateSourceTrigger:** PropertyChanged (мгновенно), LostFocus (при потере фокуса)  
✅ **DataContext** — источник binding по умолчанию  
✅ **StringFormat** для форматирования вывода  
✅ **FallbackValue** при ошибке binding, **TargetNullValue** при null  
✅ **Вложенные свойства:** `{Binding Address.City}`

---

## Дополнительные ресурсы

- [Data Binding Overview](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview)
- [Binding Modes](https://docs.microsoft.com/en-us/dotnet/api/system.windows.bindingmode)
- [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged)
