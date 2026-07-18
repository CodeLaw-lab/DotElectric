# Модуль 5: MVVM-паттерн

**Время прохождения:** 18 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте архитектуру MVVM (Model-View-ViewModel)
- ✅ Научитесь реализовывать INotifyPropertyChanged (CommunityToolkit.Mvvm)
- ✅ Освоите Commands (ICommand, RelayCommand, AsyncRelayCommand)
- ✅ Сможете использовать Dependency Injection в WPF
- ✅ Научитесь применять Messaging (WeakReferenceMessenger)

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 5.1 | [Основы MVVM](./01_MVVM_Basics.md) | 3 часа | Теория, примеры, 6 задач |
| 5.2 | [INotifyPropertyChanged](./02_INotifyPropertyChanged.md) | 4 часа | Теория, примеры, 6 задач |
| 5.3 | [Commands](./03_Commands.md) | 4 часа | Теория, примеры, 6 задач |
| 5.4 | [Dependency Injection](./04_DI.md) | 4 часа | Теория, примеры, 6 задач |
| 5.5 | [Messaging](./05_Messaging.md) | 3 часа | Теория, примеры, 6 задач |
| 5.6 | [Практическая работа](./06_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-4
- [ ] Понимаете Data Binding и конвертеры
- [ ] Работали с ObservableCollection
- [ ] Знаете основы C# (интерфейсы, события, лямбда-выражения)

---

## Краткое содержание тем

### Тема 5.1: Основы MVVM

**Изучите:**
- Разделение ответственности: Model, View, ViewModel
- Преимущества MVVM перед Code-Behind
- Связь View ↔ ViewModel через Data Binding
- Locator паттерн для DataContext

**Пример:**
```xml
<!-- View -->
<Window x:Class="WpfApp.MainWindow">
    <Window.DataContext>
        <local:MainViewModel/>
    </Window.DataContext>
    
    <StackPanel>
        <TextBlock Text="{Binding Title}"/>
        <Button Content="Click" Command="{Binding ClickCommand}"/>
    </StackPanel>
</Window>
```

```csharp
// ViewModel
public class MainViewModel
{
    public string Title { get; } = "Hello MVVM";
    public ICommand ClickCommand { get; }
    
    public MainViewModel()
    {
        ClickCommand = new RelayCommand(OnClick);
    }
    
    private void OnClick() => MessageBox.Show("Clicked!");
}
```

---

### Тема 5.2: INotifyPropertyChanged

**Изучите:**
- Ручная реализация INotifyPropertyChanged
- CommunityToolkit.Mvvm ([ObservableProperty])
- [ObservableObject] базовый класс
- Partial methods для хуков

**Пример:**
```csharp
// CommunityToolkit.Mvvm
public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;
    
    [ObservableProperty]
    private string _lastName;
    
    // Генерируется автоматически:
    // - FirstName property с INotifyPropertyChanged
    // - OnFirstNameChanging/Changed partial methods
}
```

---

### Тема 5.3: Commands

**Изучите:**
- ICommand интерфейс
- RelayCommand, AsyncRelayCommand
- CanExecute и NotifyCanExecuteChanged
- Составные команды

**Пример:**
```csharp
public class MainViewModel : ObservableObject
{
    private string _searchText;
    
    [ObservableProperty]
    private bool _canSearch;
    
    public ICommand SearchCommand { get; }
    
    public MainViewModel()
    {
        SearchCommand = AsyncRelayCommand.Create(
            SearchAsync, 
            () => CanSearch
        );
    }
    
    private async Task SearchAsync()
    {
        // Async поиск
    }
}
```

---

### Тема 5.4: Dependency Injection

**Изучите:**
- Microsoft.Extensions.DependencyInjection
- Регистрация сервисов
- ViewModel Factory
- Service Locator анти-паттерн

**Пример:**
```csharp
// App.xaml.cs
var services = new ServiceCollection();

// Регистрация сервисов
services.AddSingleton<ITemplateService, TemplateService>();
services.AddSingleton<IFileService, FileService>();

// Регистрация ViewModel
services.AddTransient<MainViewModel>();
services.AddTransient<EditorViewModel>();

// ViewModel Factory
services.AddSingleton<IEditorViewModelFactory, EditorViewModelFactory>();

var provider = services.BuildServiceProvider();
```

---

### Тема 5.5: Messaging

**Изучите:**
- WeakReferenceMessenger
- Message паттерн для коммуникации
- Request/Response сообщения
- Избегание циклических зависимостей

**Пример:**
```csharp
// Message класс
public class CloseTabRequestMessage : IMessage
{
    public EditorViewModel Tab { get; }
}

// Отправка
WeakReferenceMessenger.Default.Send(
    new CloseTabRequestMessage(selectedTab)
);

// Получение
WeakReferenceMessenger.Default.Register<CloseTabRequestMessage>(
    this, 
    (r, m) => CloseTab(m.Tab)
);
```

---

## Практическая работа

**Задание:** Создание MVVM приложения "Менеджер задач"

**Время:** 4 часа

**Требования:**
1. ViewModel с CommunityToolkit.Mvvm
2. Commands для всех действий
3. DI для сервисов
4. Messaging между ViewModel
5. Async операции

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 6 убедитесь, что вы:

- [ ] Понимаете разделение MVVM (Model/View/ViewModel)
- [ ] Использовали [ObservableProperty] атрибут
- [ ] Создали RelayCommand и AsyncRelayCommand
- [ ] Настроили DI с ServiceCollection
- [ ] Использовали WeakReferenceMessenger
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **MVVM** | Model-View-ViewModel — архитектурный паттерн |
| **Model** | Бизнес-логика и данные |
| **View** | UI (XAML) |
| **ViewModel** | Посредник между Model и View |
| **ObservableObject** | Базовый класс с INotifyPropertyChanged |
| **RelayCommand** | Реализация ICommand с делегатом |
| **AsyncRelayCommand** | RelayCommand для async операций |
| **Dependency Injection** | Внедрение зависимостей через контейнер |
| **WeakReferenceMessenger** | Messenger без сильных ссылок |
| **Message** | Объект для коммуникации между VM |

---

## Переход к следующему модулю

➡️ **[Модуль 6: Работа с графикой](../Module_06_Graphics/README.md)**

В Модуле 6 изучим:
- Shapes (Line, Rectangle, Ellipse, Path)
- Transform (RotateTransform, ScaleTransform, TranslateTransform)
- Geometry и Path-язык
- Visual Layer и DrawingVisual
- OnRender override (кастомный рендеринг)
