# Тема 11.1: Unit-тесты ViewModel

### Теория

**Unit-тесты** — тестирование отдельных компонентов (ViewModel) изолированно от UI.

#### xUnit настройка

```
Install-Package xunit
Install-Package xunit.runner.visualstudio
Install-Package Microsoft.NET.Test.Sdk
```

#### Структура тестов

```
MyApp.Tests/
├── ViewModels/
│   ├── MainViewModelTests.cs
│   └── EditorViewModelTests.cs
├── Services/
│   └── TemplateServiceTests.cs
└── Commands/
    └── RelayCommandTests.cs
```

### Примеры кода

#### Пример 1: Базовый тест свойства

```csharp
using Xunit;
using WpfApp.ViewModels;

public class PersonViewModelTests
{
    [Fact]
    public void FirstName_Get_ReturnsDefaultValue()
    {
        // Arrange
        var vm = new PersonViewModel();
        
        // Act
        var result = vm.FirstName;
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void FirstName_Setter_UpdatesValue()
    {
        // Arrange
        var vm = new PersonViewModel();
        
        // Act
        vm.FirstName = "John";
        
        // Assert
        Assert.Equal("John", vm.FirstName);
    }
    
    [Fact]
    public void FullName_ReturnsConcatenatedName()
    {
        // Arrange
        var vm = new PersonViewModel
        {
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        var result = vm.FullName;
        
        // Assert
        Assert.Equal("John Doe", result);
    }
}
```

#### Пример 2: Тест INotifyPropertyChanged

```csharp
using Xunit;
using System.ComponentModel;
using WpfApp.ViewModels;

public class PersonViewModelTests
{
    [Fact]
    public void FirstName_Setter_ShouldRaisePropertyChanged()
    {
        // Arrange
        var vm = new PersonViewModel();
        var raised = false;
        var propertyName = "";
        
        vm.PropertyChanged += (sender, e) =>
        {
            raised = true;
            propertyName = e.PropertyName;
        };
        
        // Act
        vm.FirstName = "John";
        
        // Assert
        Assert.True(raised);
        Assert.Equal(nameof(PersonViewModel.FirstName), propertyName);
    }
    
    [Fact]
    public void FirstName_Setter_ToSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var vm = new PersonViewModel();
        vm.FirstName = "John";
        var raised = false;
        
        vm.PropertyChanged += (sender, e) => raised = true;
        
        // Act
        vm.FirstName = "John"; // То же значение
        
        // Assert
        Assert.False(raised);
    }
    
    [Fact]
    public void LastName_Setter_ShouldRaisePropertyChangedForFullName()
    {
        // Arrange
        var vm = new PersonViewModel();
        var raisedProperties = new List<string>();
        
        vm.PropertyChanged += (sender, e) => raisedProperties.Add(e.PropertyName);
        
        // Act
        vm.LastName = "Doe";
        
        // Assert
        Assert.Contains(nameof(PersonViewModel.LastName), raisedProperties);
        Assert.Contains(nameof(PersonViewModel.FullName), raisedProperties);
    }
}
```

#### Пример 3: Тест валидации

```csharp
using Xunit;
using WpfApp.ViewModels;

public class PersonViewModelTests
{
    [Fact]
    public void Email_InvalidFormat_ShouldHaveError()
    {
        // Arrange
        var vm = new PersonViewModel();
        
        // Act
        vm.Email = "invalid-email";
        
        // Assert
        Assert.NotNull(vm["Email"]);
        Assert.Contains("invalid", vm["Email"].ToLower());
    }
    
    [Fact]
    public void Email_ValidFormat_ShouldNotHaveError()
    {
        // Arrange
        var vm = new PersonViewModel();
        
        // Act
        vm.Email = "test@example.com";
        
        // Assert
        Assert.Null(vm["Email"]);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("@no-local.com")]
    public void Email_InvalidFormats_ShouldHaveErrors(string email)
    {
        // Arrange
        var vm = new PersonViewModel();
        
        // Act
        vm.Email = email;
        
        // Assert
        Assert.NotNull(vm["Email"]);
    }
}
```

#### Пример 4: Тест коллекции

```csharp
using Xunit;
using System.Collections.ObjectModel;
using WpfApp.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void Items_InitialCount_ShouldBeZero()
    {
        // Arrange
        var vm = new MainViewModel();
        
        // Act & Assert
        Assert.Empty(vm.Items);
    }
    
    [Fact]
    public void AddItem_ShouldIncreaseCount()
    {
        // Arrange
        var vm = new MainViewModel();
        
        // Act
        vm.AddItem(new DataItem { Name = "Test" });
        
        // Assert
        Assert.Single(vm.Items);
    }
    
    [Fact]
    public void RemoveItem_ShouldDecreaseCount()
    {
        // Arrange
        var vm = new MainViewModel();
        var item = new DataItem { Name = "Test" };
        vm.AddItem(item);
        
        // Act
        vm.RemoveItem(item);
        
        // Assert
        Assert.Empty(vm.Items);
    }
    
    [Fact]
    public void Items_Add_ShouldRaiseCollectionChanged()
    {
        // Arrange
        var vm = new MainViewModel();
        var raised = false;
        
        vm.Items.CollectionChanged += (s, e) =>
        {
            raised = true;
            Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
        };
        
        // Act
        vm.AddItem(new DataItem { Name = "Test" });
        
        // Assert
        Assert.True(raised);
    }
}
```

#### Пример 5: Тест с CommunityToolkit.Mvvm

```csharp
using Xunit;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

public class ObservableObjectTests
{
    [Fact]
    public void SetProperty_ShouldRaisePropertyChanged()
    {
        // Arrange
        var vm = new TestViewModel();
        var raised = false;
        
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "Name")
                raised = true;
        };
        
        // Act
        vm.Name = "New Value";
        
        // Assert
        Assert.True(raised);
    }
    
    [Fact]
    public void SetProperty_ToSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var vm = new TestViewModel();
        vm.Name = "Initial";
        var raised = false;
        
        vm.PropertyChanged += (s, e) => raised = true;
        
        // Act
        vm.Name = "Initial"; // Same value
        
        // Assert
        Assert.False(raised);
    }
}

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "";
}
```

#### Пример 6: Реальные тесты из DotElectric

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
    public void Zoom_Increase_ShouldUpdateZoom()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        var initialZoom = vm.Zoom;
        
        // Act
        vm.ZoomInCommand.Execute(null);
        
        // Assert
        Assert.True(vm.Zoom > initialZoom);
    }
    
    [Fact]
    public void Zoom_OutOfRange_ShouldClamp()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        
        // Act - Zoom out multiple times
        for (int i = 0; i < 50; i++)
        {
            vm.ZoomOutCommand.Execute(null);
        }
        
        // Assert
        Assert.Equal(0.1, vm.Zoom, 2); // Min zoom = 0.1
    }
    
    [Fact]
    public void DeleteSelectedObject_ShouldRemoveFromCollection()
    {
        // Arrange
        var mockTemplateService = new Mock<ITemplateService>();
        var vm = new EditorViewModel(mockTemplateService.Object);
        var line = new Line { StartMicronsX = 0, StartMicronsY = 0, EndMicronsX = 100, EndMicronsY = 100 };
        vm.Template.Objects.Add(line);
        vm.SelectedObject = line;
        
        // Act
        vm.DeleteSelectedObjectsCommand.Execute(null);
        
        // Assert
        Assert.DoesNotContain(line, vm.Template.Objects);
        Assert.Null(vm.SelectedObject);
    }
    
    [Fact]
    public void ToggleGrid_ShouldToggleStatusBarGridEnabled()
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
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 11.1.1: Simple Property Test**

Напишите тест:
- Создайте ViewModel с свойством
- Тест на getter
- Тест на setter

**Задача 11.1.2: PropertyChanged Test**

Напишите тест:
- Подписка на PropertyChanged
- Изменение свойства
- Проверка raised

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 11.1.3: Validation Test**

Напишите тесты:
- Валидация email
- Theory с [InlineData]
- Проверка ошибок

**Задача 11.1.4: Collection Test**

Напишите тесты:
- Добавление в коллекцию
- Удаление из коллекции
- CollectionChanged event

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 11.1.5: Full ViewModel Test**

Напишите тесты:
- Все свойства
- Все методы
- 80% coverage

**Задача 11.1.6: Integration Test**

Напишите тест:
- Несколько ViewModel вместе
- Взаимодействие между ними
- Состояние после действий

---

### Решения

<details>
<summary>✅ Решение задачи 11.1.1</summary>

```csharp
[Fact]
public void Name_Getter_ReturnsDefaultValue()
{
    // Arrange
    var vm = new PersonViewModel();
    
    // Act
    var result = vm.Name;
    
    // Assert
    Assert.Equal("", result);
}

[Fact]
public void Name_Setter_UpdatesValue()
{
    // Arrange
    var vm = new PersonViewModel();
    
    // Act
    vm.Name = "John";
    
    // Assert
    Assert.Equal("John", vm.Name);
}
```
</details>

<details>
<summary>✅ Решение задачи 11.1.2</summary>

```csharp
[Fact]
public void Age_Setter_ShouldRaisePropertyChanged()
{
    // Arrange
    var vm = new PersonViewModel();
    var raised = false;
    
    vm.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(PersonViewModel.Age))
            raised = true;
    };
    
    // Act
    vm.Age = 25;
    
    // Assert
    Assert.True(raised);
}
```
</details>

---

## Ключевые выводы

✅ **xUnit** — фреймворк для unit-тестов  
✅ **Arrange-Act-Assert** — паттерн написания тестов  
✅ **Fact** — простой тест  
✅ **Theory + InlineData** — параметризованный тест  
✅ **PropertyChanged** — тестирование INotifyPropertyChanged  
✅ **CollectionChanged** — тестирование коллекций  
✅ **Assert** — проверка результатов

---

## Дополнительные ресурсы

- [xUnit Documentation](https://xunit.net/docs)
- [Testing ViewModel](https://docs.microsoft.com/en-us/dotnet/architecture/maui/testing)
- [Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
