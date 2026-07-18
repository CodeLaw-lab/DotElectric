# Тема 10.2: Virtualization (Виртуализация)

### Теория

**Virtualization** — отрисовка только видимых элементов коллекции для улучшения производительности.

#### Типы виртуизации

| Тип | Описание | Когда использовать |
|-----|----------|-------------------|
| **UI Virtualization** | Отрисовка только видимых UI элементов | Большие списки (1000+) |
| **Data Virtualization** | Загрузка данных по требованию | Очень большие наборы (100000+) |

#### VirtualizingStackPanel

```xml
<!-- Без виртуализации (медленно для больших списков) -->
<ListBox>
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>

<!-- С виртуализацией (быстро) -->
<ListBox VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

### Примеры кода

#### Пример 1: Включение виртуализации

```xml
<!-- ListBox с виртуализацией -->
<ListBox ItemsSource="{Binding LargeCollection}"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         ScrollViewer.IsDeferredScrollingEnabled="False">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

#### Пример 2: VirtualizationMode

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <!-- Standard mode - создаёт новые контейнеры -->
    <ListBox Grid.Row="0"
             ItemsSource="{Binding Collection1}"
             VirtualizingPanel.IsVirtualizing="True"
             VirtualizingPanel.VirtualizationMode="Standard">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding}"/>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
    
    <!-- Recycling mode - переиспользует контейнеры (быстрее) -->
    <ListBox Grid.Row="1"
             ItemsSource="{Binding Collection2}"
             VirtualizingPanel.IsVirtualizing="True"
             VirtualizingPanel.VirtualizationMode="Recycling">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding}"/>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</Grid>
```

#### Пример 3: DataGrid с виртуизацией

```xml
<DataGrid ItemsSource="{Binding LargeCollection}"
          AutoGenerateColumns="False"
          EnableRowVirtualization="True"
          EnableColumnVirtualization="True"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling">
    <DataGrid.Columns>
        <DataGridTextColumn Header="ID" Binding="{Binding Id}"/>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}"/>
        <DataGridTextColumn Header="Email" Binding="{Binding Email}"/>
    </DataGrid.Columns>
</DataGrid>
```

#### Пример 4: Отключение виртуализации (когда нужно)

```xml
<!-- Когда виртуализация ломает функциональность -->
<ListBox ItemsSource="{Binding Collection}"
         VirtualizingPanel.IsVirtualizing="False">
    <!-- GroupStyle требует отключения виртуализации -->
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListBox.GroupStyle>
</ListBox>
```

#### Пример 5: Custom VirtualizingPanel

```csharp
// CustomVirtualizingPanel.cs
public class CustomVirtualizingPanel : VirtualizingPanel, IScrollInfo
{
    private readonly UIElementCollection _children;
    private readonly List<Item> _items;
    
    // Реализация IScrollInfo
    public double HorizontalOffset => _offsetX;
    public double VerticalOffset => _offsetY;
    public double ExtentWidth => _items.Count * ItemWidth;
    public double ExtentHeight => ItemHeight;
    public double ViewportWidth => ActualWidth;
    public double ViewportHeight => ActualHeight;
    
    public void SetHorizontalOffset(double offset) { }
    public void SetVerticalOffset(double offset) { }
    
    public Rect MakeVisible(Visual child, Rect rectangle)
    {
        return rectangle;
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        // Измеряем только видимые элементы
        int firstVisible = (int)(_offsetY / ItemHeight);
        int lastVisible = firstVisible + (int)(availableSize.Height / ItemHeight) + 1;
        
        for (int i = firstVisible; i < Math.Min(lastVisible, _items.Count); i++)
        {
            var child = GetOrCreateElement(i);
            child.Measure(new Size(availableSize.Width, ItemHeight));
        }
        
        return availableSize;
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        int firstVisible = (int)(_offsetY / ItemHeight);
        int lastVisible = firstVisible + (int)(finalSize.Height / ItemHeight) + 1;
        
        for (int i = firstVisible; i < Math.Min(lastVisible, _items.Count); i++)
        {
            var child = GetOrCreateElement(i);
            child.Arrange(new Rect(0, i * ItemHeight - _offsetY, finalSize.Width, ItemHeight));
        }
        
        return finalSize;
    }
}
```

#### Пример 6: Data Virtualization

```csharp
// DataVirtualizingCollection.cs
public class DataVirtualizingCollection<T> : IList, INotifyCollectionChanged
{
    private readonly int _pageSize;
    private readonly Dictionary<int, T> _cache;
    private readonly Func<int, int, Task<IEnumerable<T>>> _fetchFunc;
    
    public int Count { get; }
    
    public object this[int index]
    {
        get
        {
            if (!_cache.ContainsKey(index))
            {
                LoadPage(index);
            }
            return _cache[index];
        }
    }
    
    public DataVirtualizingCollection(int totalCount, int pageSize, 
                                       Func<int, int, Task<IEnumerable<T>>> fetchFunc)
    {
        Count = totalCount;
        _pageSize = pageSize;
        _fetchFunc = fetchFunc;
        _cache = new Dictionary<int, T>();
    }
    
    private async void LoadPage(int index)
    {
        int pageIndex = index / _pageSize;
        int startIndex = pageIndex * _pageSize;
        
        var items = await _fetchFunc(startIndex, _pageSize);
        
        int i = startIndex;
        foreach (var item in items)
        {
            _cache[i++] = item;
        }
        
        CollectionChanged?.Invoke(this, 
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    
    public event NotifyCollectionChangedEventHandler CollectionChanged;
}
```

#### Пример 7: Реальное использование из DotElectric

```xml
<!-- TemplateLibraryView.xaml -->
<UserControl>
    <Grid>
        <!-- Список шаблонов с виртуализацией -->
        <ListBox ItemsSource="{Binding Templates}"
                 SelectedItem="{Binding SelectedTemplate}"
                 VirtualizingPanel.IsVirtualizing="True"
                 VirtualizingPanel.VirtualizationMode="Recycling"
                 ScrollViewer.IsDeferredScrollingEnabled="False">
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <VirtualizingStackPanel/>
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
            
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Padding="10" Margin="2" BorderThickness="1" BorderBrush="#E0E0E0">
                        <StackPanel>
                            <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                            <TextBlock Text="{Binding SheetFormat}" FontSize="12" Foreground="Gray"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </Grid>
</UserControl>
```

```csharp
// EditorViewModel.cs - оптимизация большой коллекции
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ITemplateObject> _templateObjects;

    public EditorViewModel()
    {
        // Для больших коллекций используем ObservableCollection
        // с включенной виртуализацией в View
        TemplateObjects = new ObservableCollection<ITemplateObject>();
    }
    
    // Пакетное добавление для уменьшения уведомлений
    public void AddObjectsRange(IEnumerable<ITemplateObject> objects)
    {
        // Отключаем уведомления на время добавления
        foreach (var obj in objects)
        {
            TemplateObjects.Add(obj);
        }
        // Одно уведомление вместо N
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 10.2.1: Enable Virtualization**

Включите виртуализацию:
- ListBox с 10000 элементов
- VirtualizingPanel.IsVirtualizing=True
- VirtualizationMode=Recycling

**Задача 10.2.2: DataGrid Virtualization**

Включите виртуализацию:
- DataGrid с 1000 строк
- EnableRowVirtualization=True
- EnableColumnVirtualization=True

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 10.2.3: Performance Comparison**

Сравните производительность:
- ListBox без виртуализации
- ListBox с виртуализацией
- Замер времени загрузки

**Задача 10.2.4: Grouping without Virtualization**

Создайте:
- ListBox с группировкой
- GroupStyle
- Virtualization=False (требуется для групп)

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 10.2.5: Data Virtualization**

Реализуйте:
- DataVirtualizingCollection
- Загрузка страницами
- Кэширование

**Задача 10.2.6: Custom VirtualizingPanel**

Создайте:
- Наследник VirtualizingPanel
- IScrollInfo реализация
- Wrap-like layout

---

### Решения

<details>
<summary>✅ Решение задачи 10.2.1</summary>

```xml
<ListBox ItemsSource="{Binding LargeCollection}"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         ScrollViewer.IsDeferredScrollingEnabled="False">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

```csharp
// ViewModel
public class MainViewModel
{
    public ObservableCollection<string> LargeCollection { get; }
    
    public MainViewModel()
    {
        LargeCollection = new ObservableCollection<string>();
        for (int i = 0; i < 10000; i++)
        {
            LargeCollection.Add($"Item {i}");
        }
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 10.2.3</summary>

```csharp
// Performance тест
var sw = Stopwatch.StartNew();

// Без виртуализации: 500ms+
listBox1.ItemsSource = Enumerable.Range(1, 10000)
    .Select(i => $"Item {i}");
sw.Stop();
Debug.WriteLine($"Without virtualization: {sw.ElapsedMilliseconds}ms");

// С виртуализацией: 50ms
sw.Restart();
listBox2.ItemsSource = Enumerable.Range(1, 10000)
    .Select(i => $"Item {i}");
sw.Stop();
Debug.WriteLine($"With virtualization: {sw.ElapsedMilliseconds}ms");
```
</details>

---

## Ключевые выводы

✅ **VirtualizingStackPanel** — отрисовка только видимых элементов  
✅ **Recycling mode** — переиспользование контейнеров (быстрее)  
✅ **Standard mode** — создание новых контейнеров  
✅ **EnableRowVirtualization** — для DataGrid  
✅ **Группировка** — требует отключения виртуализации  
✅ **Data Virtualization** — загрузка данных по страницам  
✅ **1000+ элементов** — порог для включения виртуализации

---

## Дополнительные ресурсы

- [UI Virtualization](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.virtualizingpanel)
- [VirtualizingStackPanel](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.virtualizingstackpanel)
- [Data Virtualization](https://docs.microsoft.com/en-us/archive/msdn-magazine/2010/march/patterns-washed-ashore-your-guide-to-clr-4-0-part-2)
