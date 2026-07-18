# Тема 11.2: Мокирование сервисов (Moq)

### Теория

**Moq** — библиотека для создания mock-объектов (имитаций зависимостей).

#### Установка

```
Install-Package Moq
```

#### Ключевые концепции

| Концепция | Описание | Пример |
|-----------|----------|--------|
| **Mock<T>** | Создание мока интерфейса | `new Mock<IDialogService>()` |
| **Setup** | Настройка поведения | `Setup(x => x.Show())` |
| **Returns** | Возвращаемое значение | `Returns(true)` |
| **Verify** | Проверка вызова | `Verify(x => x.Show(), Times.Once)` |
| **It.IsAny<T>()** | Любой параметр | `It.IsAny<string>()` |

### Примеры кода

#### Пример 1: Базовое мокирование

```csharp
using Xunit;
using Moq;
using WpfApp.ViewModels;
using WpfApp.Services;

public class MainViewModelTests
{
    [Fact]
    public void ShowMessage_ShouldCallDialogService()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        var vm = new MainViewModel(mockDialogService.Object);
        
        // Act
        vm.ShowMessage("Hello");
        
        // Assert
        mockDialogService.Verify(
            x => x.ShowMessage("Hello"), 
            Times.Once);
    }
}
```

#### Пример 2: Setup с Returns

```csharp
[Fact]
public void OpenFile_WhenFileSelected_ShouldLoadData()
{
    // Arrange
    var mockDialogService = new Mock<IDialogService>();
    mockDialogService
        .Setup(x => x.ShowOpenFileDialog(It.IsAny<string>()))
        .Returns("C:\\test.tdel");
    
    var mockFileService = new Mock<IFileService>();
    mockFileService
        .Setup(x => x.LoadAsync("C:\\test.tdel"))
        .ReturnsAsync(new Template());
    
    var vm = new MainViewModel(
        mockDialogService.Object, 
        mockFileService.Object);
    
    // Act
    await vm.OpenFileCommand.ExecuteAsync(null);
    
    // Assert
    mockFileService.Verify(
        x => x.LoadAsync("C:\\test.tdel"), 
        Times.Once);
}
```

#### Пример 3: Setup с параметрами

```csharp
[Fact]
public void Save_WithValidPath_ShouldCallSaveAsync()
{
    // Arrange
    var mockDialogService = new Mock<IDialogService>();
    mockDialogService
        .Setup(x => x.ShowSaveFileDialog(It.IsAny<string>()))
        .Returns("C:\\save.tdel");
    
    var mockFileService = new Mock<IFileService>();
    mockFileService
        .Setup(x => x.SaveAsync(It.IsAny<Template>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);
    
    var vm = new MainViewModel(
        mockDialogService.Object, 
        mockFileService.Object);
    
    // Act
    await vm.SaveCommand.ExecuteAsync(null);
    
    // Assert
    mockFileService.Verify(
        x => x.SaveAsync(It.IsAny<Template>(), "C:\\save.tdel"), 
        Times.Once);
}
```

#### Пример 4: Verify с Times

```csharp
[Fact]
public void Refresh_ShouldCallDataService()
{
    // Arrange
    var mockDataService = new Mock<IDataService>();
    var vm = new MainViewModel(mockDataService.Object);
    
    // Act
    vm.Refresh();
    vm.Refresh();
    
    // Assert
    mockDataService.Verify(
        x => x.GetData(), 
        Times.Exactly(2));
}

[Fact]
public void Close_ShouldNeverCallSaveIfNoChanges()
{
    // Arrange
    var mockFileService = new Mock<IFileService>();
    var vm = new MainViewModel(mockFileService.Object);
    vm.HasChanges = false;
    
    // Act
    vm.Close();
    
    // Assert
    mockFileService.Verify(
        x => x.SaveAsync(It.IsAny<Template>(), It.IsAny<string>()), 
        Times.Never);
}
```

#### Пример 5: Mock событий

```csharp
[Fact]
public void TemplateChanged_Event_ShouldRaise()
{
    // Arrange
    var mockTemplateService = new Mock<ITemplateService>();
    var raised = false;
    
    mockTemplateService
        .SetupAdd(x => x.TemplateChanged += It.IsAny<EventHandler>())
        .Callback<EventHandler>(h => raised = true);
    
    var vm = new EditorViewModel(mockTemplateService.Object);
    
    // Act
    vm.LoadTemplate();
    
    // Assert
    Assert.True(raised);
}
```

#### Пример 6: Mock с callback

```csharp
[Fact]
public void ProcessData_ShouldCallCallback()
{
    // Arrange
    var mockService = new Mock<IProcessingService>();
    var callbackCalled = false;
    
    mockService
        .Setup(x => x.Process(It.IsAny<string>()))
        .Callback<string>(s => callbackCalled = true);
    
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    vm.ProcessData("test");
    
    // Assert
    Assert.True(callbackCalled);
}
```

#### Пример 7: Реальные тесты из DotElectric

```csharp
// EditorViewModelTests.cs
using Xunit;
using Moq;
using DotElectric.ViewModels;
using DotElectric.Services;
using DotElectric.Models;

public class EditorViewModelTests
{
    [Fact]
    public void NewTemplate_ShouldCallTemplateService()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var newTemplate = new Template();
        mockTemplateService
            .Setup(x => x.CreateNew())
            .Returns(newTemplate);
        
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Act
        vm.NewTemplateCommand.Execute(null);
        
        // Assert
        mockTemplateService.Verify(x => x.CreateNew(), Times.Once);
        Assert.Same(newTemplate, vm.Template);
    }
    
    [Fact]
    public async Task Save_WhenNoPath_ShouldShowSaveFileDialog()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var mockDialogService = new Mock<IDialogService>();
        var mockFileService = new Mock<IFileService>();
        
        mockDialogService
            .Setup(x => x.ShowSaveFileDialog("TDEL files|*.tdel"))
            .Returns("C:\\test.tdel");
        
        var vm = new EditorViewModel(
            mockTemplateService.Object,
            mockDialogService.Object,
            mockFileService.Object);
        
        // Act
        await vm.SaveCommand.ExecuteAsync(null);
        
        // Assert
        mockDialogService.Verify(
            x => x.ShowSaveFileDialog("TDEL files|*.tdel"), 
            Times.Once);
    }
    
    [Fact]
    public async Task Save_WithValidPath_ShouldCallFileService()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var mockDialogService = new Mock<IDialogService>();
        var mockFileService = new Mock<IFileService>();
        
        mockDialogService
            .Setup(x => x.ShowSaveFileDialog(It.IsAny<string>()))
            .Returns("C:\\test.tdel");
        
        mockFileService
            .Setup(x => x.SaveAsync(It.IsAny<Template>(), "C:\\test.tdel"))
            .Returns(Task.CompletedTask);
        
        var vm = new EditorViewModel(
            mockTemplateService.Object,
            mockDialogService.Object,
            mockFileService.Object);
        
        // Act
        await vm.SaveCommand.ExecuteAsync(null);
        
        // Assert
        mockFileService.Verify(
            x => x.SaveAsync(It.IsAny<Template>(), "C:\\test.tdel"), 
            Times.Once);
    }
    
    [Fact]
    public void Undo_WhenCanUndo_ShouldCallHistoryUndo()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Setup History.CanUndo = true
        vm.History.Add(new Mock<ICommand>().Object);
        
        // Act
        vm.UndoCommand.Execute(null);
        
        // Assert
        Assert.True(vm.History.CanUndo);
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 11.2.1: Simple Mock**

Создайте mock:
- Интерфейс IService
- Setup метода
- Verify вызова

**Задача 11.2.2: Mock Returns**

Создайте mock:
- Метод возвращает значение
- Setup с Returns
- Проверка результата

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 11.2.3: Multiple Mocks**

Создайте тест:
- 2+ mock сервиса
- ViewModel с зависимостями
- Verify всех вызовов

**Задача 11.2.4: Mock с It.IsAny**

Создайте тест:
- It.IsAny для параметров
- Setup с любым параметром
- Verify

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 11.2.5: Mock Callback**

Создайте тест:
- Callback при вызове
- Изменение состояния
- Проверка callback

**Задача 11.2.6: Mock Events**

Создайте тест:
- Mock событий
- SetupAdd для event
- Verify подписки

---

### Решения

<details>
<summary>✅ Решение задачи 11.2.1</summary>

```csharp
public interface IMessageService
{
    void Show(string message);
}

[Fact]
public void Show_ShouldCallMessageService()
{
    // Arrange
    var mockService = new Mock<IMessageService>();
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    vm.ShowMessage("Hello");
    
    // Assert
    mockService.Verify(x => x.Show("Hello"), Times.Once);
}
```
</details>

<details>
<summary>✅ Решение задачи 11.2.2</summary>

```csharp
public interface IDataService
{
    string GetData();
}

[Fact]
public void LoadData_ShouldReturnDataFromService()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService.Setup(x => x.GetData()).Returns("Test Data");
    
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    var result = vm.LoadData();
    
    // Assert
    Assert.Equal("Test Data", result);
}
```
</details>

---

## Ключевые выводы

✅ **Mock<T>** — создание mock-объекта  
✅ **Setup** — настройка поведения  
✅ **Returns** — возвращаемое значение  
✅ **Verify** — проверка вызова  
✅ **Times.Once/Exactly/Never** — количество вызовов  
✅ **It.IsAny<T>()** — любой параметр  
✅ **Callback** — выполнение кода при вызове

---

## Дополнительные ресурсы

- [Moq Documentation](https://github.com/moq/moq4)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [Mocking Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#mocking)
