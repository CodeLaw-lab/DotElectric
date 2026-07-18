# Тема 10.1: Multi-threading и Dispatcher

### Теория

**WPF использует single-threaded apartment (STA) модель** — весь UI выполняется в одном потоке.

#### UI Thread vs Background Thread

| UI Thread | Background Thread |
|-----------|-------------------|
| Обработка событий UI | Долгие вычисления |
| Отрисовка | Работа с файлами |
| Анимации | Сетевые запросы |
| **Нельзя блокировать** | **Можно блокировать** |

#### Dispatcher

**Dispatcher** — очередь задач для выполнения в UI потоке.

```csharp
// Выполнение в UI потоке
Application.Current.Dispatcher.Invoke(() =>
{
    // Код выполняется в UI потоке
    myTextBox.Text = "Updated";
});

// Асинхронное выполнение
Application.Current.Dispatcher.BeginInvoke(() =>
{
    // Код выполнится асинхронно в UI потоке
});
```

### Примеры кода

#### Пример 1: Неправильная работа с потоками

```csharp
// ❌ НЕПРАВИЛЬНО - блокировка UI
private void LoadDataButton_Click(object sender, RoutedEventArgs e)
{
    // Блокирует UI поток на 5 секунд
    Thread.Sleep(5000);
    
    // Попытка обновления UI из фонового потока
    Task.Run(() =>
    {
        var data = GetData();
        // InvalidOperationException!
        myListBox.ItemsSource = data;
    });
}
```

#### Пример 2: Правильная работа с async/await

```csharp
// ✅ ПРАВИЛЬНО - async/await
private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
{
    // Показываем индикатор загрузки
    LoadingIndicator.Visibility = Visibility.Visible;
    
    // Выполняем долгую операцию в фоне
    var data = await Task.Run(() => GetData());
    
    // Обновляем UI (автоматически в UI потоке после await)
    myListBox.ItemsSource = data;
    
    // Скрываем индикатор
    LoadingIndicator.Visibility = Visibility.Collapsed;
}

private List<string> GetData()
{
    Thread.Sleep(5000); // Имитация долгой операции
    return new List<string> { "Item 1", "Item 2", "Item 3" };
}
```

#### Пример 3: Dispatcher.Invoke и BeginInvoke

```csharp
private void UpdateUIFromBackground()
{
    Task.Run(() =>
    {
        var data = GetData();
        
        // Синхронное выполнение в UI потоке (блокирует фон)
        Application.Current.Dispatcher.Invoke(() =>
        {
            myListBox.ItemsSource = data;
        });
        
        // Асинхронное выполнение в UI потоке (не блокирует фон)
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            myListBox.ItemsSource = data;
        }));
        
        // С приоритетом
        Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                myListBox.ItemsSource = data;
            }));
    });
}
```

#### Пример 4: Progress Reporting

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        ProgressBar.Value = 0;
        StatusText.Text = "Загрузка...";

        try
        {
            var progress = new Progress<int>(percent =>
            {
                // Обновление UI из progress callback
                ProgressBar.Value = percent;
                StatusText.Text = $"Загрузка: {percent}%";
            });

            await Task.Run(() => DoWork(progress));

            StatusText.Text = "Готово!";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
        finally
        {
            StartButton.IsEnabled = true;
        }
    }

    private void DoWork(IProgress<int> progress)
    {
        for (int i = 0; i <= 100; i++)
        {
            Thread.Sleep(50); // Имитация работы
            progress.Report(i);
        }
    }
}
```

#### Пример 5: CancellationToken для отмены

```csharp
public partial class MainWindow : Window
{
    private CancellationTokenSource _cancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            // Отмена текущей операции
            _cancellationTokenSource.Cancel();
            StartButton.Content = "Старт";
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        StartButton.Content = "Отмена";
        ProgressBar.Value = 0;

        try
        {
            await DoWorkAsync(_cancellationTokenSource.Token);
            StatusText.Text = "Готово!";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Отменено";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
        finally
        {
            _cancellationTokenSource = null;
            StartButton.Content = "Старт";
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(50, cancellationToken);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = i;
            });
        }
    }
}
```

#### Пример 6: Async Command в ViewModel

```csharp
public class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private ObservableCollection<DataItem> _items;

    public ICommand LoadDataCommand { get; }
    public ICommand CancelCommand { get; }

    private CancellationTokenSource _cancellationTokenSource;

    public MainViewModel()
    {
        Items = new ObservableCollection<DataItem>();
        
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync, CanLoadData);
        CancelCommand = new RelayCommand(Cancel);
    }

    private bool CanLoadData() => !_isLoading;

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Загрузка данных...";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<int>(p => Progress = p);

            var items = await Task.Run(
                () => LoadDataFromDatabase(progress),
                _cancellationTokenSource.Token);

            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            StatusMessage = $"Загружено {Items.Count} элементов";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Отменено";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _cancellationTokenSource = null;
        }
    }

    private List<DataItem> LoadDataFromDatabase(IProgress<int> progress)
    {
        var items = new List<DataItem>();
        
        for (int i = 0; i < 1000; i++)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            Thread.Sleep(10); // Имитация загрузки
            items.Add(new DataItem { Id = i, Name = $"Item {i}" });
            
            progress.Report(i / 10);
        }
        
        return items;
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// EditorViewModel.cs - Async загрузка файла
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage;

    private CancellationTokenSource _loadCancellationTokenSource;

    [RelayCommand(CanExecute = nameof(CanLoadFile))]
    private async Task LoadFileAsync()
    {
        var path = _dialogService.ShowOpenFileDialog("TDEL files|*.tdel|All files|*.*");
        if (string.IsNullOrEmpty(path)) return;

        IsLoading = true;
        StatusMessage = "Загрузка файла...";
        _loadCancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<string>(status =>
            {
                StatusMessage = status;
            });

            var template = await Task.Run(
                () => _templateService.LoadAsync(path, progress),
                _loadCancellationTokenSource.Token);

            Template = template;
            Title = Path.GetFileName(path);
            StatusMessage = "Файл загружен";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Загрузка отменена";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
            _logger.Error(ex, "Failed to load file {Path}", path);
        }
        finally
        {
            IsLoading = false;
            _loadCancellationTokenSource = null;
        }
    }

    private bool CanLoadFile() => !_isLoading;

    [RelayCommand]
    private void CancelLoading()
    {
        _loadCancellationTokenSource?.Cancel();
    }
}

// TemplateService.cs
public class TemplateService : ITemplateService
{
    public async Task<Template> LoadAsync(string path, IProgress<string> progress)
    {
        progress?.Report("Чтение файла...");
        
        await Task.Delay(100); // Имитация
        
        progress?.Report("Разбор XML...");
        
        var xml = await File.ReadAllTextAsync(path);
        var dto = DeserializeTemplateDto(xml);
        
        progress?.Report("Создание моделей...");
        
        var template = CreateTemplateFromDto(dto);
        
        progress?.Report("Готово");
        
        return template;
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 10.1.1: Simple Async**

Создайте кнопку:
- async onClick handler
- Task.Delay 3 секунды
- Обновление TextBlock после

**Задача 10.1.2: Dispatcher Invoke**

Создайте:
- Фоновый поток
- Dispatcher.Invoke для обновления UI
- Progress bar update

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 10.1.3: Progress Reporting**

Реализуйте:
- Progress&lt;int&gt;
- Progress bar обновление
- Статус текст

**Задача 10.1.4: Async Command**

Создайте:
- AsyncRelayCommand
- IsLoading property
- Command CanExecute

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 10.1.5: Cancellation**

Реализуйте:
- CancellationTokenSource
- Отмена операции
- Cleanup при отмене

**Задача 10.1.6: Parallel Loading**

Загрузите данные:
- 3 источника параллельно
- Task.WhenAll
- Объединение результатов

---

### Решения

<details>
<summary>✅ Решение задачи 10.1.1</summary>

```csharp
private async void LoadButton_Click(object sender, RoutedEventArgs e)
{
    StatusText.Text = "Загрузка...";
    
    await Task.Delay(3000);
    
    StatusText.Text = "Загрузка завершена!";
}
```
</details>

<details>
<summary>✅ Решение задачи 10.1.3</summary>

```csharp
private async void StartButton_Click(object sender, RoutedEventArgs e)
{
    var progress = new Progress<int>(p => ProgressBar.Value = p);
    
    await Task.Run(() =>
    {
        for (int i = 0; i <= 100; i++)
        {
            Thread.Sleep(50);
            progress.Report(i);
        }
    });
}
```
</details>

---

## Ключевые выводы

✅ **UI Thread** — никогда не блокировать  
✅ **async/await** — предпочтительный способ  
✅ **Dispatcher** — для обновлений UI из фона  
✅ **Progress&lt;T&gt;** — для прогресс операций  
✅ **CancellationToken** — для отмены операций  
✅ **AsyncRelayCommand** — для MVVM  
✅ **ConfigureAwait(false)** — для библиотечного кода

---

## Дополнительные ресурсы

- [Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher)
- [Async/Await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Progress&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.progress-1)
