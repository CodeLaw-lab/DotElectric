# Модуль 11: Тестирование WPF-приложений

**Время прохождения:** 8 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Научитесь писать unit-тесты для ViewModel
- ✅ Освоите мокирование сервисов (Moq)
- ✅ Сможете тестировать Commands
- ✅ Поймёте основы UI-тестирования (FlaUI)

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 11.1 | [Unit-тесты ViewModel](./01_ViewModel_Tests.md) | 2 часа | Теория, примеры, 5 задач |
| 11.2 | [Мокирование сервисов](./02_Mocking_Services.md) | 2 часа | Теория, примеры, 5 задач |
| 11.3 | [Command тесты](./03_Command_Tests.md) | 2 часа | Теория, примеры, 5 задач |
| 11.4 | [UI тесты](./04_UI_Tests.md) | 2 часа | Теория, примеры, 4 задачи |
| 11.5 | [Практическая работа](./05_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-10
- [ ] Понимаете MVVM паттерн
- [ ] Работали с Commands
- [ ] Знаете основы xUnit или NUnit

---

## Краткое содержание тем

### Тема 11.1: Unit-тесты ViewModel

**Изучите:**
- xUnit настройка проекта
- Тестирование свойств
- Тестирование INotifyPropertyChanged
- Arrange-Act-Assert паттерн

**Пример:**
```csharp
[Fact]
public void FirstName_Setter_ShouldRaisePropertyChanged()
{
    // Arrange
    var vm = new PersonViewModel();
    var raised = false;
    vm.PropertyChanged += (s, e) => 
    {
        if (e.PropertyName == nameof(PersonViewModel.FirstName))
            raised = true;
    };
    
    // Act
    vm.FirstName = "John";
    
    // Assert
    Assert.True(raised);
}
```

---

### Тема 11.2: Мокирование сервисов

**Изучите:**
- Moq библиотека
- Создание моков интерфейсов
- Setup и Verify
- Mock поведения

**Пример:**
```csharp
var mockService = new Mock<IDialogService>();
mockService.Setup(x => x.ShowMessage(It.IsAny<string>()))
           .Returns(true);

var vm = new MainViewModel(mockService.Object);
```

---

### Тема 11.3: Command тесты

**Изучите:**
- Тестирование ICommand
- CanExecute проверка
- Execute проверка
- Async Command тесты

**Пример:**
```csharp
[Fact]
public void SaveCommand_CanExecute_WhenHasChanges()
{
    var vm = new DocumentViewModel();
    vm.HasChanges = true;
    
    Assert.True(vm.SaveCommand.CanExecute(null));
}
```

---

### Тема 11.4: UI тесты

**Изучите:**
- FlaUI для UI автоматизации
- Поиск элементов
- Симуляция ввода
- Проверка результатов

**Пример:**
```csharp
var app = Application.Launch("MyApp.exe");
var window = app.GetMainWindow();
var button = window.FindFirstDescendant(cf => cf.ByText("Click"));
button.Click();
```

---

## Практическая работа

**Задание:** Тестирование MVVM приложения

**Время:** 4 часа

**Требования:**
1. Unit-тесты для ViewModel (80% coverage)
2. Мокирование сервисов
3. Command тесты
4. UI тесты с FlaUI

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 12 убедитесь, что вы:

- [ ] Настроили xUnit проект
- [ ] Написали тесты для свойств
- [ ] Использовали Moq для сервисов
- [ ] Протестировали Commands
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Unit Test** | Тестирование отдельных компонентов |
| **xUnit** | Фреймворк для unit-тестов |
| **Moq** | Библиотека для мокирования |
| **Mock** | Имитация зависимости |
| **Setup** | Настройка поведения мока |
| **Verify** | Проверка вызова мока |
| **FlaUI** | UI автоматизация для WPF |
| **Code Coverage** | Процент покрытого кода тестами |

---

## Переход к следующему модулю

➡️ **[Модуль 12: Финальный проект](../Module_12_FinalProject/README.md)**

В Модуле 12 создадим:
- CAD-редактор схем
- Интеграция всех изученных тем
- Production-ready код
