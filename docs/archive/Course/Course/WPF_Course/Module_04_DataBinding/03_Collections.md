# Тема 4.3: Binding к коллекциям (ObservableCollection, ICollectionView)

### Теория

**ObservableCollection&lt;T&gt;** — коллекция с уведомлением об изменениях.

| Событие | Описание |
|---------|----------|
| **CollectionChanged** | Уведомляет об изменении коллекции (добавление, удаление, перемещение) |
| **PropertyChanged** | Уведомляет об изменении свойства коллекции (Count, Item[]) |

**ICollectionView** — представление коллекции для фильтрации, сортировки, группировки без изменения исходной коллекции.

### Примеры кода

#### Пример 1: ObservableCollection базовое использование

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<string> _items;

    public ObservableCollection<string> Items => _items;

    public MainViewModel()
    {
        _items = new ObservableCollection<string>
        {
            "Item 1",
            "Item 2",
            "Item 3"
        };

        // Команды для добавления/удаления
        AddCommand = new RelayCommand(AddItem);
        RemoveCommand = new RelayCommand(RemoveItem, CanRemoveItem);
    }

    private void AddItem()
    {
        // UI автоматически обновится!
        _items.Add($"Item {_items.Count + 1}");
    }

    private void RemoveItem()
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
        }
    }

    private bool CanRemoveItem() => _items.Count > 0;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel>
    <ListBox ItemsSource="{Binding Items}" Height="200" Margin="0,0,0,10"/>
    
    <StackPanel Orientation="Horizontal">
        <Button Content="Add" Command="{Binding AddCommand}" Padding="20,5"/>
        <Button Content="Remove" Command="{Binding RemoveCommand}" 
                Margin="10,0,0,0" Padding="20,5"/>
    </StackPanel>
</StackPanel>
```

#### Пример 2: ObservableCollection с объектами

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

public class PeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Person> _people;

    public ObservableCollection<Person> People => _people;

    public PeopleViewModel()
    {
        _people = new ObservableCollection<Person>
        {
            new Person { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" },
            new Person { FirstName = "Jane", LastName = "Smith", Age = 25, Email = "jane@example.com" },
            new Person { FirstName = "Bob", LastName = "Johnson", Age = 35, Email = "bob@example.com" }
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<ListBox ItemsSource="{Binding People}" Height="300">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <StackPanel Width="150">
                    <TextBlock Text="{Binding FirstName}" FontWeight="Bold"/>
                    <TextBlock Text="{Binding LastName}" Foreground="Gray"/>
                </StackPanel>
                <TextBlock Text="{Binding Age, StringFormat='Age: {0}'}" Margin="20,0"/>
                <TextBlock Text="{Binding Email}" Foreground="Blue"/>
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

#### Пример 3: ICollectionView — фильтрация

```csharp
public class FilteredPeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Person> _people;
    private ICollectionView _peopleView;
    private string _filterText;

    public ICollectionView PeopleView => _peopleView;

    public string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnPropertyChanged();
            // Обновляем фильтр
            _peopleView.Refresh();
        }
    }

    public FilteredPeopleViewModel()
    {
        _people = new ObservableCollection<Person>
        {
            new Person { FirstName = "John", LastName = "Doe", Age = 30 },
            new Person { FirstName = "Jane", LastName = "Smith", Age = 25 },
            new Person { FirstName = "Bob", LastName = "Johnson", Age = 35 },
            new Person { FirstName = "Alice", LastName = "Williams", Age = 28 }
        };

        // Получаем представление коллекции
        _peopleView = CollectionViewSource.GetDefaultView(_people);
        
        // Устанавливаем фильтр
        _peopleView.Filter = FilterPeople;
    }

    private bool FilterPeople(object item)
    {
        if (string.IsNullOrWhiteSpace(FilterText))
            return true;

        var person = (Person)item;
        return person.FirstName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
               person.LastName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel Margin="20">
    <TextBox Text="{Binding FilterText, UpdateSourceTrigger=PropertyChanged}"
             Watermark="Filter by name..."
             Margin="0,0,0,10"/>
    
    <ListBox ItemsSource="{Binding PeopleView}" Height="300">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding FirstName, StringFormat='{}{0} {1}'}"/>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
    
    <TextBlock Margin="0,10,0,0">
        <Run Text="Showing "/>
        <Run Text="{Binding PeopleView.Count}"/>
        <Run Text=" of "/>
        <Run Text="{Binding People.Count}"/>
        <Run Text=" items"/>
    </TextBlock>
</StackPanel>
```

#### Пример 4: ICollectionView — сортировка

```csharp
public class SortedPeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Person> _people;
    private ICollectionView _peopleView;
    private string _sortBy;

    public ICollectionView PeopleView => _peopleView;

    public string SortBy
    {
        get => _sortBy;
        set
        {
            _sortBy = value;
            OnPropertyChanged();
            ApplySorting();
        }
    }

    public SortedPeopleViewModel()
    {
        _people = new ObservableCollection<Person>
        {
            new Person { FirstName = "John", LastName = "Doe", Age = 30 },
            new Person { FirstName = "Jane", LastName = "Smith", Age = 25 },
            new Person { FirstName = "Bob", LastName = "Johnson", Age = 35 }
        };

        _peopleView = CollectionViewSource.GetDefaultView(_people);
        ApplySorting();
    }

    private void ApplySorting()
    {
        _peopleView.SortDescriptions.Clear();

        switch (SortBy)
        {
            case "FirstName":
                _peopleView.SortDescriptions.Add(
                    new SortDescription("FirstName", ListSortDirection.Ascending));
                break;
            case "Age":
                _peopleView.SortDescriptions.Add(
                    new SortDescription("Age", ListSortDirection.Ascending));
                break;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel Margin="20">
    <ComboBox SelectedItem="{Binding SortBy}" Width="150" Margin="0,0,0,10">
        <ComboBoxItem Content="First Name"/>
        <ComboBoxItem Content="Age"/>
    </ComboBox>
    
    <ListBox ItemsSource="{Binding PeopleView}" Height="300">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding FirstName, StringFormat='{}{0} {1}'}" Width="150"/>
                    <TextBlock Text="{Binding Age, StringFormat='Age: {0}'}"/>
                </StackPanel>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</StackPanel>
```

#### Пример 5: ICollectionView — группировка

```csharp
public class GroupedPeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Person> _people;
    private ICollectionView _peopleView;

    public ICollectionView PeopleView => _peopleView;

    public GroupedPeopleViewModel()
    {
        _people = new ObservableCollection<Person>
        {
            new Person { FirstName = "John", LastName = "Doe", Age = 30, Department = "IT" },
            new Person { FirstName = "Jane", LastName = "Smith", Age = 25, Department = "HR" },
            new Person { FirstName = "Bob", LastName = "Johnson", Age = 35, Department = "IT" },
            new Person { FirstName = "Alice", LastName = "Williams", Age = 28, Department = "HR" }
        };

        _peopleView = CollectionViewSource.GetDefaultView(_people);
        
        // Группировка по отделу
        _peopleView.GroupDescriptions.Add(
            new PropertyGroupDescription("Department"));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<ListBox ItemsSource="{Binding PeopleView}" Height="400">
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="LightBlue" Padding="10,5">
                        <TextBlock FontWeight="Bold" 
                                   Text="{Binding Name, StringFormat='Department: {0}'}"/>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListBox.GroupStyle>
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding FirstName, StringFormat='{}{0} {1}'}" Margin="10,5"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

#### Пример 6: Уведомления об изменениях в элементах коллекции

```csharp
public class ObservablePerson : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(FullName));
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class PeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<ObservablePerson> _people;

    public ObservableCollection<ObservablePerson> People => _people;

    public PeopleViewModel()
    {
        _people = new ObservableCollection<ObservablePerson>();
        
        // Подписка на изменения элементов
        _people.CollectionChanged += OnPeopleCollectionChanged;
    }

    private void OnPeopleCollectionChanged(object sender, 
                                           NotifyCollectionChangedEventArgs e)
    {
        // Отписка от старых элементов
        if (e.OldItems != null)
        {
            foreach (INotifyPropertyChanged item in e.OldItems)
            {
                item.PropertyChanged -= OnPersonPropertyChanged;
            }
        }

        // Подписка на новые элементы
        if (e.NewItems != null)
        {
            foreach (INotifyPropertyChanged item in e.NewItems)
            {
                item.PropertyChanged += OnPersonPropertyChanged;
            }
        }
    }

    private void OnPersonPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Элемент коллекции изменился
        Debug.WriteLine($"Person changed: {e.PropertyName}");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// EditorViewModel.cs
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ITemplateObject> _templateObjects;

    [ObservableProperty]
    private ITemplateObject _selectedObject;

    [ObservableProperty]
    private ObservableCollection<ITemplateObject> _selectedObjects;

    public EditorViewModel(ITemplateService templateService)
    {
        _templateObjects = new ObservableCollection<ITemplateObject>();
        _selectedObjects = new ObservableCollection<ITemplateObject>();

        // Подписка на изменения коллекции
        _templateObjects.CollectionChanged += OnTemplateObjectsChanged;
    }

    private void OnTemplateObjectsChanged(object sender, 
                                          NotifyCollectionChangedEventArgs e)
    {
        // Обновление UI при изменении коллекции объектов
        if (e.NewItems != null)
        {
            foreach (ITemplateObject obj in e.NewItems)
            {
                // Подписка на изменения каждого объекта
                if (obj is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += OnObjectPropertyChanged;
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (ITemplateObject obj in e.OldItems)
            {
                if (obj is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= OnObjectPropertyChanged;
                }
            }
        }

        MarkDirty();
    }

    private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Объект изменился
        MarkDirty();
    }

    partial void OnSelectedObjectChanged(ITemplateObject value)
    {
        // Обработка изменения выбранного объекта
        PropertiesVm?.SetObject(value);
    }
}
```

```xml
<!-- EditorCanvas.xaml -->
<UserControl>
    <!-- ItemsControl для отрисовки объектов -->
    <ItemsControl ItemsSource="{Binding TemplateObjects}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <Canvas/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemContainerStyle>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left">
                    <Setter.Value>
                        <MultiBinding Converter="{StaticResource LeftConverter}">
                            <Binding Path="MicronsX"/>
                            <Binding Path="Zoom"/>
                        </MultiBinding>
                    </Setter.Value>
                </Setter>
                <Setter Property="Canvas.Top">
                    <Setter.Value>
                        <MultiBinding Converter="{StaticResource TopConverter}">
                            <Binding Path="MicronsY"/>
                            <Binding Path="Zoom"/>
                        </MultiBinding>
                    </Setter.Value>
                </Setter>
            </Style>
        </ItemsControl.ItemContainerStyle>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <!-- Шаблоны для разных типов объектов -->
                <ContentPresenter Content="{Binding}"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</UserControl>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 4.3.1: Simple List**

Создайте ViewModel с ObservableCollection&lt;string&gt;:
- 5 начальных элементов
- Кнопка Add (добавляет новый элемент)
- Кнопка Remove (удаляет последний элемент)
- ListBox для отображения

**Задача 4.3.2: Product List**

Создайте класс Product (Name, Price, Quantity):
- ObservableCollection&lt;Product&gt;
- DataTemplate для отображения
- Итого: сумма всех (Price × Quantity)

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 4.3.3: Filterable List**

Реализуйте фильтрацию через ICollectionView:
- Список людей (FirstName, LastName, Age)
- TextBox для поиска (по имени или фамилии)
- ComboBox для фильтра по возрасту (All, &lt;30, 30-50, &gt;50)

**Задача 4.3.4: Sortable List**

Реализуйте сортировку через ICollectionView:
- Список продуктов (Name, Price, Category)
- ComboBox для выбора сортировки (по имени, цене, категории)
- Кнопка Toggle для Ascending/Descending

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 4.3.5: Grouped List**

Реализуйте группировку через ICollectionView:
- Список сотрудников (Name, Department, Salary)
- GroupStyle для ListBox
- Заголовки групп с количеством элементов
- Сортировка внутри групп

**Задача 4.3.6: Master-Detail с коллекциями**

Создайте Master-Detail view:
- Список категорий (ObservableCollection)
- Список товаров выбранной категории
- Синхронизация выделения
- Добавление/удаление в обеих коллекциях

---

### Решения

<details>
<summary>✅ Решение задачи 4.3.1</summary>

```csharp
public class SimpleListViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<string> _items;
    private int _counter;

    public ObservableCollection<string> Items => _items;

    public SimpleListViewModel()
    {
        _items = new ObservableCollection<string>
        {
            "Item 1",
            "Item 2",
            "Item 3",
            "Item 4",
            "Item 5"
        };
        _counter = 5;

        AddCommand = new RelayCommand(AddItem);
        RemoveCommand = new RelayCommand(RemoveItem, CanRemove);
    }

    private void AddItem()
    {
        _counter++;
        _items.Add($"Item {_counter}");
    }

    private void RemoveItem()
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
        }
    }

    private bool CanRemove() => _items.Count > 0;

    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel Margin="20">
    <ListBox ItemsSource="{Binding Items}" Height="300" Margin="0,0,0,10"/>
    
    <StackPanel Orientation="Horizontal">
        <Button Content="Add" Command="{Binding AddCommand}" Padding="20,5"/>
        <Button Content="Remove" Command="{Binding RemoveCommand}" 
                Margin="10,0,0,0" Padding="20,5"/>
    </StackPanel>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 4.3.3</summary>

```csharp
public class FilterablePeopleViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Person> _people;
    private ICollectionView _peopleView;
    private string _searchText;
    private string _ageFilter;

    public ICollectionView PeopleView => _peopleView;

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            _peopleView?.Refresh();
        }
    }

    public string AgeFilter
    {
        get => _ageFilter;
        set
        {
            _ageFilter = value;
            OnPropertyChanged();
            _peopleView?.Refresh();
        }
    }

    public FilterablePeopleViewModel()
    {
        _people = new ObservableCollection<Person>
        {
            new Person { FirstName = "John", LastName = "Doe", Age = 30 },
            new Person { FirstName = "Jane", LastName = "Smith", Age = 25 },
            new Person { FirstName = "Bob", LastName = "Johnson", Age = 45 },
            new Person { FirstName = "Alice", LastName = "Williams", Age = 35 }
        };

        _peopleView = CollectionViewSource.GetDefaultView(_people);
        _peopleView.Filter = FilterPeople;
    }

    private bool FilterPeople(object item)
    {
        var person = (Person)item;

        // Поиск по имени
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            if (!person.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                !person.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Фильтр по возрасту
        if (!string.IsNullOrWhiteSpace(AgeFilter))
        {
            switch (AgeFilter)
            {
                case "<30":
                    if (person.Age >= 30) return false;
                    break;
                case "30-50":
                    if (person.Age < 30 || person.Age > 50) return false;
                    break;
                case ">50":
                    if (person.Age <= 50) return false;
                    break;
            }
        }

        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```
</details>

---

## Ключевые выводы

✅ **ObservableCollection** автоматически уведомляет UI об изменениях  
✅ **ICollectionView** для фильтрации, сортировки, группировки  
✅ **CollectionViewSource.GetDefaultView()** получает представление  
✅ **Filter** — делегат для фильтрации  
✅ **SortDescriptions** — коллекция описаний сортировки  
✅ **GroupDescriptions** — коллекция описаний группировки  
✅ **INotifyPropertyChanged** в элементах коллекции для уведомлений об изменениях свойств

---

## Дополнительные ресурсы

- [ObservableCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1)
- [ICollectionView](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.icollectionview)
- [CollectionViewSource](https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.collectionviewsource)
