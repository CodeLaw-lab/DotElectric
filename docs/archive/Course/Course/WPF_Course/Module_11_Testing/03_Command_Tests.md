# Тема 11.3: Command тесты

### Теория

**Command тесты** — тестирование ICommand реализаций (RelayCommand, AsyncRelayCommand).

#### Что тестировать

| Аспект | Описание | Пример |
|--------|----------|--------|
| **CanExecute** | Когда команда доступна | `CanExecute(null)` |
| **Execute** | Выполнение команды | `Execute(parameter)` |
| **CanExecuteChanged** | Событие изменения | Подписка на событие |
| **Async Execute** | Асинхронное выполнение | `ExecuteAsync()` |

### Примеры кода

#### Пример 1: Тест CanExecute

```csharp
using Xunit;
using Moq;
using WpfApp.ViewModels;
using WpfApp.Commands;

public class SaveCommandTests
{
    [Fact]
    public void CanExecute_WhenHasChanges_ShouldReturnTrue()
    {
        // Arrange
        var vm = new DocumentViewModel();
        vm.HasChanges = true;
        
        // Act & Assert
        Assert.True(vm.SaveCommand.CanExecute(null));
    }
    
    [Fact]
    public void CanExecute_WhenNoChanges_ShouldReturnFalse()
    {
        // Arrange
        var vm = new DocumentViewModel();
        vm.HasChanges = false;
        
        // Act & Assert
        Assert.False(vm.SaveCommand.CanExecute(null));
    }
    
    [Fact]
    public void CanExecute_Changes_ShouldRaiseCanExecuteChanged()
    {
        // Arrange
        var vm = new DocumentViewModel();
        var raised = false;
        
        ((RelayCommand)vm.SaveCommand).CanExecuteChanged += (s, e) => raised = true;
        
        // Act
        vm.HasChanges = true;
        ((RelayCommand)vm.SaveCommand).NotifyCanExecuteChanged();
        
        // Assert
        Assert.True(raised);
    }
}
```

#### Пример 2: Тест Execute

```csharp
[Fact]
public void Execute_ShouldCallSaveMethod()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    var vm = new DocumentViewModel(mockService.Object);
    vm.HasChanges = true;
    
    // Act
    vm.SaveCommand.Execute(null);
    
    // Assert
    mockService.Verify(x => x.Save(), Times.Once);
    Assert.False(vm.HasChanges); // Changes saved
}

[Fact]
public void Execute_WithParameter_ShouldPassParameter()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    var vm = new DocumentViewModel(mockService.Object);
    
    // Act
    vm.SaveCommand.Execute("test.tdel");
    
    // Assert
    mockService.Verify(x => x.Save("test.tdel"), Times.Once);
}
```

#### Пример 3: Тест Async Command

```csharp
[Fact]
public async Task ExecuteAsync_ShouldCompleteTask()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService
        .Setup(x => x.LoadDataAsync())
        .ReturnsAsync(new List<string> { "Item 1", "Item 2" });
    
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    await vm.LoadDataCommand.ExecuteAsync(null);
    
    // Assert
    Assert.NotEmpty(vm.Items);
    mockService.Verify(x => x.LoadDataAsync(), Times.Once);
}

[Fact]
public async Task ExecuteAsync_WhenException_ShouldSetErrorMessage()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService
        .Setup(x => x.LoadDataAsync())
        .ThrowsAsync(new Exception("Test error"));
    
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    await vm.LoadDataCommand.ExecuteAsync(null);
    
    // Assert
    Assert.Contains("Error", vm.StatusMessage);
}
```

#### Пример 4: Тест Command с CanExecute

```csharp
[Fact]
public void DeleteCommand_CanExecute_WhenItemSelected()
{
    // Arrange
    var vm = new MainViewModel();
    vm.SelectedItem = new DataItem();
    
    // Act & Assert
    Assert.True(vm.DeleteCommand.CanExecute(null));
}

[Fact]
public void DeleteCommand_CanExecute_WhenNoItemSelected_ShouldReturnFalse()
{
    // Arrange
    var vm = new MainViewModel();
    vm.SelectedItem = null;
    
    // Act & Assert
    Assert.False(vm.DeleteCommand.CanExecute(null));
}

[Fact]
public void DeleteCommand_Execute_ShouldRemoveSelectedItem()
{
    // Arrange
    var vm = new MainViewModel();
    var item = new DataItem();
    vm.Items.Add(item);
    vm.SelectedItem = item;
    
    // Act
    vm.DeleteCommand.Execute(null);
    
    // Assert
    Assert.DoesNotContain(item, vm.Items);
}
```

#### Пример 5: Реальные тесты из DotElectric

```csharp
// CommandTests.cs
using Xunit;
using Moq;
using DotElectric.ViewModels;
using DotElectric.Commands;
using DotElectric.Models;

public class EditorViewModelCommandTests
{
    [Fact]
    public void ZoomInCommand_Execute_ShouldIncreaseZoom()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        var initialZoom = vm.Zoom;
        
        // Act
        vm.ZoomInCommand.Execute(null);
        
        // Assert
        Assert.True(vm.Zoom > initialZoom);
        Assert.True(vm.Zoom <= 10.0); // Max zoom
    }
    
    [Fact]
    public void ZoomOutCommand_CanExecute_AlwaysTrue()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Act & Assert
        Assert.True(vm.ZoomOutCommand.CanExecute(null));
    }
    
    [Fact]
    public void UndoCommand_CanExecute_WhenHistoryHasItems()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Add item to history
        vm.History.Add(new Mock<ICommand>().Object);
        
        // Act & Assert
        Assert.True(vm.UndoCommand.CanExecute(null));
    }
    
    [Fact]
    public void UndoCommand_CanExecute_WhenHistoryEmpty_ShouldReturnFalse()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Act & Assert
        Assert.False(vm.UndoCommand.CanExecute(null));
    }
    
    [Fact]
    public void ToggleGridCommand_Execute_ShouldToggleValue()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        var initialState = vm.StatusBarGridEnabled;
        
        // Act
        vm.ToggleGridCommand.Execute(null);
        
        // Assert
        Assert.NotEqual(initialState, vm.StatusBarGridEnabled);
    }
    
    [Fact]
    public async Task SaveCommand_Execute_WhenNoPath_ShouldShowDialog()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var mockDialogService = new Mock<IDialogService>();
        var mockFileService = new Mock<IFileService>();
        
        mockDialogService
            .Setup(x => x.ShowSaveFileDialog(It.IsAny<string>()))
            .Returns("C:\\test.tdel");
        
        var vm = new EditorViewModel(
            mockTemplateService.Object,
            mockDialogService.Object,
            mockFileService.Object);
        
        // Act
        await vm.SaveCommand.ExecuteAsync(null);
        
        // Assert
        mockDialogService.Verify(
            x => x.ShowSaveFileDialog(It.IsAny<string>()), 
            Times.Once);
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 11.3.1: CanExecute Test**

Напишите тест:
- Команда с CanExecute
- Проверка true/false
- Разные состояния

**Задача 11.3.2: Execute Test**

Напишите тест:
- Execute метод
- Проверка результата
- Mock сервиса

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 11.3.3: Async Command Test**

Напишите тест:
- Async RelayCommand
- await Execute
- Проверка результата

**Задача 11.3.4: CanExecuteChanged Test**

Напишите тест:
- Подписка на событие
- Изменение состояния
- Проверка raised

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 11.3.5: Command With Parameter**

Напишите тест:
- Command с параметром
- Передача параметра
- Проверка параметра

**Задача 11.3.6: Full Command Coverage**

Напишите тесты:
- Все команды ViewModel
- CanExecute и Execute
- 100% coverage команд

---

### Решения

<details>
<summary>✅ Решение задачи 11.3.1</summary>

```csharp
[Fact]
public void SaveCommand_CanExecute_WhenHasChanges()
{
    // Arrange
    var vm = new DocumentViewModel();
    vm.HasChanges = true;
    
    // Act & Assert
    Assert.True(vm.SaveCommand.CanExecute(null));
}

[Fact]
public void SaveCommand_CanExecute_WhenNoChanges()
{
    // Arrange
    var vm = new DocumentViewModel();
    vm.HasChanges = false;
    
    // Act & Assert
    Assert.False(vm.SaveCommand.CanExecute(null));
}
```
</details>

<details>
<summary>✅ Решение задачи 11.3.3</summary>

```csharp
[Fact]
public async Task LoadDataCommand_ExecuteAsync_ShouldLoadItems()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService
        .Setup(x => x.GetDataAsync())
        .ReturnsAsync(new List<string> { "Item 1", "Item 2" });
    
    var vm = new MainViewModel(mockService.Object);
    
    // Act
    await vm.LoadDataCommand.ExecuteAsync(null);
    
    // Assert
    Assert.NotEmpty(vm.Items);
}
```
</details>

---

## Ключевые выводы

✅ **CanExecute** — проверка доступности команды  
✅ **Execute** — выполнение команды  
✅ **CanExecuteChanged** — уведомление об изменении  
✅ **AsyncRelayCommand** — для async операций  
✅ **NotifyCanExecuteChanged** — принудительное обновление  
✅ **Parameter** — передача параметров в команду  
✅ **Verify** — проверка вызова сервиса

---

## Дополнительные ресурсы

- [RelayCommand Testing](https://docs.microsoft.com/en-us/dotnet/architecture/maui/testing)
- [Async Command](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/relaycommand)
- [Command Best Practices](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand)
