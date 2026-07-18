# Тема 5.5: Messaging (WeakReferenceMessenger)

### Теория

**Messaging** — паттерн для коммуникации между ViewModel без циклических зависимостей.

#### Когда использовать Messaging

✅ **Слабая связанность** — ViewModel не знают друг о друге  
✅ **Cross-VM коммуникация** — связь между независимыми ViewModel  
✅ **События приложения** — глобальные уведомления  
✅ **Избегание циклических зависимостей** — вместо прямых ссылок  

#### WeakReferenceMessenger

- **WeakReference** — не предотвращает сборку мусора
- **Автоматическая отписка** — при сборке получателя
- **Thread-safe** — безопасная многопоточная работа

### Примеры кода

#### Пример 1: Базовое использование Messenger

```csharp
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

// Отправитель
public class SenderViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public SenderViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [RelayCommand]
    private void SendMessage()
    {
        // Отправка простого сообщения
        _messenger.Send("Hello from Sender!");
    }
}

// Получатель
public class ReceiverViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;

    public ReceiverViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        
        // Подписка на сообщения
        _messenger.Register<string>(this, (r, m) =>
        {
            Console.WriteLine($"Received: {m}");
        });
    }
}
```

#### Пример 2: ValueMessage<T>

```csharp
// Сообщение с значением
public class PersonSelectedMessage : ValueMessage<Person>
{
    public PersonSelectedMessage(Person person) : base(person) { }
}

// Отправитель
public class ListViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public ListViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [ObservableProperty]
    private Person _selectedPerson;

    partial void OnSelectedPersonChanged(Person value)
    {
        if (value != null)
        {
            _messenger.Send(new PersonSelectedMessage(value));
        }
    }
}

// Получатель
public class DetailViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Person _selectedPerson;

    public DetailViewModel(IMessenger messenger) : base(messenger)
    {
        this.Register<PersonSelectedMessage>(this, (r, m) =>
        {
            SelectedPerson = m.Value;
        });
    }
}
```

#### Пример 3: PropertyChangedMessage

```csharp
// Отправка уведомления об изменении свойства
public class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _title;

    partial void OnTitleChanged(string value)
    {
        // Автоматическая отправка PropertyChangedMessage
        // при использовании [AlsoBroadcastChange]
    }
}

// Получатель
public class ChildViewModel : ObservableRecipient
{
    public ChildViewModel(IMessenger messenger) : base(messenger)
    {
        this.Register<PropertyChangedMessage<string>>(this, (r, m) =>
        {
            if (m.PropertyName == nameof(MainViewModel.Title))
            {
                Console.WriteLine($"Title changed from {m.OldValue} to {m.NewValue}");
            }
        });
    }
}
```

#### Пример 4: Request/Response сообщения

```csharp
// Запрос с ответом
public class GetDataRequest : IRequestMessage<List<Item>>
{
    public List<Item> Reply { get; set; }
}

// Отправитель запроса
public class ConsumerViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public ConsumerViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [RelayCommand]
    private void LoadData()
    {
        var request = new GetDataRequest();
        var response = _messenger.Send(request);
        
        // Обработка ответа
        Console.WriteLine($"Received {response.Reply.Count} items");
    }
}

// Получатель запроса
public class ProviderViewModel : ObservableObject
{
    private readonly List<Item> _items;

    public ProviderViewModel(IMessenger messenger)
    {
        _items = new List<Item>();
        
        messenger.Register<GetDataRequest>(this, (r, m) =>
        {
            m.Reply = _items;
        });
    }
}
```

#### Пример 5: StrongReferenceMessenger

```csharp
// StrongReferenceMessenger хранит сильные ссылки
// Используется когда получатель должен оставаться в памяти

public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        // Регистрация на StrongReferenceMessenger по умолчанию
        WeakReferenceMessenger.Default.Register<SomeMessage>(this, (r, m) =>
        {
            // Обработка
        });
    }
}

// Очистка мессенджера
WeakReferenceMessenger.Default.Reset();
```

#### Пример 6: Каналы (Channel)

```csharp
// Разделение сообщений по каналам
public class ChannelViewModel : ObservableRecipient
{
    public ChannelViewModel(IMessenger messenger) : base(messenger)
    {
        // Подписка на сообщения в канале "UI"
        messenger.Register<UiMessage>(this, "UI", (r, m) =>
        {
            // Обработка UI сообщений
        });

        // Подписка на сообщения в канале "Data"
        messenger.Register<DataMessage>(this, "Data", (r, m) =>
        {
            // Обработка данных
        });

        // Отправка в канал
        messenger.Send<UiMessage, string>("UI", "Update UI");
    }
}
```

#### Пример 7: Реальное использование из DotElectric

```csharp
// Messages/CloseTabRequestMessage.cs
public class CloseTabRequestMessage : IRecipientMessage<CloseTabRequestMessage>
{
    public EditorViewModel TabToClose { get; }

    public CloseTabRequestMessage(EditorViewModel tab)
    {
        TabToClose = tab;
    }
}

// Messages/CloseOtherTabsRequestMessage.cs
public class CloseOtherTabsRequestMessage
{
    public EditorViewModel KeepTab { get; }
    public IEnumerable<EditorViewModel> TabsToClose { get; }

    public CloseOtherTabsRequestMessage(
        EditorViewModel keepTab, 
        IEnumerable<EditorViewModel> tabsToClose)
    {
        KeepTab = keepTab;
        TabsToClose = tabsToClose;
    }
}

// Messages/CloseAllTabsRequestMessage.cs
public class CloseAllTabsRequestMessage
{
    public IEnumerable<EditorViewModel> TabsToClose { get; }

    public CloseAllTabsRequestMessage(IEnumerable<EditorViewModel> tabs)
    {
        TabsToClose = tabs;
    }
}

// EditorViewModel.cs - Отправка сообщений
public partial class EditorViewModel : ObservableObject, IDisposable
{
    private readonly IMessenger _messenger;

    public EditorViewModel(
        ITemplateService templateService,
        IFileService fileService,
        IMessenger messenger)
    {
        _messenger = messenger;
        // ... остальные зависимости
    }

    [RelayCommand]
    private void CloseTab()
    {
        // Отправка сообщения вместо прямого вызова
        _messenger.Send(new CloseTabRequestMessage(this));
    }

    [RelayCommand]
    private void CloseOtherTabs(IEnumerable<EditorViewModel> tabs)
    {
        var tabsToClose = tabs.Where(t => t != this);
        _messenger.Send(new CloseOtherTabsRequestMessage(this, tabsToClose));
    }

    [RelayCommand]
    private void CloseAllTabs(IEnumerable<EditorViewModel> tabs)
    {
        _messenger.Send(new CloseAllTabsRequestMessage(tabs));
    }

    public void Dispose()
    {
        // Отписка от всех сообщений
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}

// MainViewModel.cs - Получение сообщений
public partial class MainViewModel : ObservableRecipient
{
    private readonly ITemplateService _templateService;
    private readonly IEditorViewModelFactory _editorVmFactory;

    [ObservableProperty]
    private ObservableCollection<EditorViewModel> _openedTabs;

    [ObservableProperty]
    private EditorViewModel _selectedTab;

    public MainViewModel(
        ITemplateService templateService,
        IEditorViewModelFactory editorVmFactory,
        IMessenger messenger) : base(messenger)
    {
        _templateService = templateService;
        _editorVmFactory = editorVmFactory;
        OpenedTabs = new ObservableCollection<EditorViewModel>();

        // Подписка на сообщения
        this.Register<CloseTabRequestMessage>(this, (r, m) =>
        {
            CloseTab(m.Value.TabToClose);
        });

        this.Register<CloseOtherTabsRequestMessage>(this, (r, m) =>
        {
            CloseOtherTabs(m.Value.KeepTab, m.Value.TabsToClose);
        });

        this.Register<CloseAllTabsRequestMessage>(this, (r, m) =>
        {
            CloseAllTabs(m.Value.TabsToClose);
        });
    }

    private void CloseTab(EditorViewModel tab)
    {
        if (tab != null && OpenedTabs.Contains(tab))
        {
            OpenedTabs.Remove(tab);
            tab.Dispose();
        }
    }

    private void CloseOtherTabs(EditorViewModel keepTab, 
                                 IEnumerable<EditorViewModel> tabsToClose)
    {
        foreach (var tab in tabsToClose)
        {
            CloseTab(tab);
        }
    }

    private void CloseAllTabs(IEnumerable<EditorViewModel> tabsToClose)
    {
        foreach (var tab in tabsToClose)
        {
            CloseTab(tab);
        }
    }
}
```

#### Пример 8: Notification Messages

```csharp
// Сообщение-уведомление
public class TemplateChangedMessage : ValueMessage<Template>
{
    public TemplateChangedMessage(Template template) : base(template) { }
}

// Сообщение с действием
public class ActionMessage : IMessage
{
    public Action Action { get; }

    public ActionMessage(Action action)
    {
        Action = action;
    }
}

// Использование
public class EditorViewModel : ObservableRecipient
{
    [RelayCommand]
    private void Save()
    {
        // Save logic
        _messenger.Send(new TemplateChangedMessage(Template));
    }
}

public class PropertiesViewModel : ObservableRecipient
{
    public PropertiesViewModel(IMessenger messenger) : base(messenger)
    {
        this.Register<TemplateChangedMessage>(this, (r, m) =>
        {
            // Обновление свойств при изменении шаблона
            UpdateProperties(m.Value);
        });
    }

    private void UpdateProperties(Template template)
    {
        // Update logic
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 5.5.1: Простое сообщение**

Создайте messaging:
- Отправитель отправляет строку
- Получатель принимает и выводит в консоль
- Используйте WeakReferenceMessenger.Default

**Задача 5.5.2: ValueMessage**

Создайте сообщение с данными:
- PersonSelectedMessage с Person
- Отправка при выборе человека
- Получение в Detail ViewModel

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 5.5.3: PropertyChangedMessage**

Реализуйте подписку на изменения:
- MainViewModel с property Title
- ChildViewModel подписывается на изменения
- Вывод старого и нового значения

**Задача 5.5.4: Request/Response**

Создайте запрос с ответом:
- GetDataRequest с List<Item>
- ProviderViewModel возвращает данные
- ConsumerViewModel получает и отображает

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 5.5.5: Multi-VM Communication**

Реализуйте коммуникацию между 3+ ViewModel:
- Master ViewModel отправляет сообщения
- Detail ViewModel получает
- List ViewModel фильтрует по сообщению
- Избегание циклических зависимостей

**Задача 5.5.6: Event Aggregator**

Создайте агрегатор событий:
- Разные типы сообщений
- Каналы для разделения
- Подписка/отписка
- Cleanup при Dispose

---

### Решения

<details>
<summary>✅ Решение задачи 5.5.1</summary>

```csharp
// Отправитель
public class SenderViewModel : ObservableObject
{
    [RelayCommand]
    private void SendGreeting()
    {
        WeakReferenceMessenger.Default.Send("Hello World!");
    }
}

// Получатель
public class ReceiverViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _lastMessage;

    public ReceiverViewModel()
    {
        this.Register<string>(this, (r, m) =>
        {
            LastMessage = m;
        });
    }
}
```
</details>

<details>
<summary>✅ Решение задачи 5.5.2</summary>

```csharp
// Сообщение
public class PersonSelectedMessage : ValueMessage<Person>
{
    public PersonSelectedMessage(Person person) : base(person) { }
}

// Отправитель (ListViewModel)
public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private Person _selectedPerson;

    partial void OnSelectedPersonChanged(Person value)
    {
        WeakReferenceMessenger.Default.Send(
            new PersonSelectedMessage(value)
        );
    }
}

// Получатель (DetailViewModel)
public partial class DetailViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Person _displayPerson;

    public DetailViewModel()
    {
        this.Register<PersonSelectedMessage>(this, (r, m) =>
        {
            DisplayPerson = m.Value;
        });
    }
}
```
</details>

---

## Ключевые выводы

✅ **WeakReferenceMessenger** — коммуникация без сильных ссылок  
✅ **ValueMessage<T>** — сообщение с данными  
✅ **PropertyChangedMessage<T>** — уведомление об изменении свойства  
✅ **IRequestMessage<T>** — запрос с ответом  
✅ **Register<T>** — подписка на сообщения  
✅ **Send<T>** — отправка сообщения  
✅ **UnregisterAll** — отписка при Dispose  
✅ **Каналы** — разделение сообщений по категориям

---

## Дополнительные ресурсы

- [WeakReferenceMessenger](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger)
- [Messages](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messages)
- [ObservableRecipient](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observablerecipient)
