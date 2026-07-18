# Тема 11.4: UI тесты (FlaUI)

### Теория

**UI тесты** — автоматизированное тестирование пользовательского интерфейса.

#### FlaUI

**FlaUI** — библиотека для UI автоматизации WPF приложений.

```
Install-Package FlaUI.UIA3
```

#### Что тестировать

| Аспект | Описание | Пример |
|--------|----------|--------|
| **Launch App** | Запуск приложения | `Application.Launch()` |
| **Find Element** | Поиск элемента | `FindFirstDescendant()` |
| **Click** | Клик по элементу | `element.Click()` |
| **SetText** | Ввод текста | `element.AsTextBox().Text = "..."` |
| **Assert** | Проверка результата | `Assert.Equal(...)` |

### Примеры кода

#### Пример 1: Базовый UI тест

```csharp
using Xunit;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;

public class MainWindowTests
{
    [Fact]
    public void MainWindow_ShouldOpen()
    {
        // Arrange
        var app = Application.Launch("WpfApp.exe");
        
        using (var automation = new UIA3Automation())
        {
            var window = app.GetMainWindow(automation);
            
            // Assert
            Assert.NotNull(window);
            Assert.Contains("WpfApp", window.Name);
        }
        
        app.Close();
    }
}
```

#### Пример 2: Поиск элементов

```csharp
[Fact]
public void MainWindow_ShouldHaveSaveButton()
{
    // Arrange
    var app = Application.Launch("WpfApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Act
        var saveButton = window.FindFirstDescendant(
            cf => cf.ByText("Save"));
        
        // Assert
        Assert.NotNull(saveButton);
    }
    
    app.Close();
}

[Fact]
public void MainWindow_ShouldHaveTextBox()
{
    // Arrange
    var app = Application.Launch("WpfApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Act
        var textBox = window.FindFirstDescendant(
            cf => cf.ByAutomationId("NameTextBox"));
        
        // Assert
        Assert.NotNull(textBox);
    }
    
    app.Close();
}
```

#### Пример 3: Ввод текста и клик

```csharp
[Fact]
public void SaveButton_Click_ShouldShowMessage()
{
    // Arrange
    var app = Application.Launch("WpfApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Act - ввод текста
        var textBox = window.FindFirstDescendant(
            cf => cf.ByAutomationId("NameTextBox"));
        textBox.AsTextBox().Text = "Test Name";
        
        // Act - клик по кнопке
        var saveButton = window.FindFirstDescendant(
            cf => cf.ByText("Save"));
        saveButton.Click();
        
        // Assert
        var messageBox = window.FindFirstDescendant(
            cf => cf.ByText("Saved!"));
        Assert.NotNull(messageBox);
    }
    
    app.Close();
}
```

#### Пример 4: Тест с WaitForIdle

```csharp
[Fact]
public void LoadDataButton_Click_ShouldPopulateList()
{
    // Arrange
    var app = Application.Launch("WpfApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Act
        var loadButton = window.FindFirstDescendant(
            cf => cf.ByAutomationId("LoadButton"));
        loadButton.Click();
        
        // Wait for loading
        window.WaitForIdle(TimeSpan.FromSeconds(5));
        
        // Assert
        var listBox = window.FindFirstDescendant(
            cf => cf.ByAutomationId("DataListBox"));
        var items = listBox.FindAllDescendants(
            cf => cf.ControlType == FlaUI.Core.Definitions.ControlType.ListItem);
        
        Assert.NotEmpty(items);
    }
    
    app.Close();
}
```

#### Пример 5: Тест с Conditions

```csharp
using FlaUI.Core.Conditions;

[Fact]
public void MainWindow_Elements_ShouldBeEnabled()
{
    // Arrange
    var app = Application.Launch("WpfApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        var condition = new PropertyCondition(
            automation.PropertyLibrary.Element.IsEnabled, true);
        
        // Act
        var saveButton = window.FindFirstDescendant(
            cf => cf.ByText("Save").And(condition));
        
        // Assert
        Assert.NotNull(saveButton);
        Assert.True(saveButton.IsEnabled);
    }
    
    app.Close();
}
```

#### Пример 6: Page Object Pattern для UI тестов

```csharp
// MainWindowPage.cs
public class MainWindowPage
{
    private readonly AutomationElement _window;
    private readonly UIA3Automation _automation;

    public MainWindowPage(Application app, UIA3Automation automation)
    {
        _automation = automation;
        _window = app.GetMainWindow(automation);
    }

    public string Title => _window.Name;

    public void EnterName(string name)
    {
        var textBox = _window.FindFirstDescendant(
            cf => cf.ByAutomationId("NameTextBox"));
        textBox.AsTextBox().Text = name;
    }

    public void ClickSave()
    {
        var button = _window.FindFirstDescendant(
            cf => cf.ByText("Save"));
        button.Click();
    }

    public bool HasMessage(string message)
    {
        var element = _window.FindFirstDescendant(
            cf => cf.ByText(message));
        return element != null;
    }

    public int GetListCount()
    {
        var listBox = _window.FindFirstDescendant(
            cf => cf.ByAutomationId("DataListBox"));
        var items = listBox.FindAllDescendants(
            cf => cf.ControlType == FlaUI.Core.Definitions.ControlType.ListItem);
        return items.Count;
    }
}

// Tests
public class MainWindowTests
{
    [Fact]
    public void Save_WithValidName_ShouldShowSuccess()
    {
        // Arrange
        var app = Application.Launch("WpfApp.exe");
        
        using (var automation = new UIA3Automation())
        {
            var page = new MainWindowPage(app, automation);
            
            // Act
            page.EnterName("John Doe");
            page.ClickSave();
            
            // Assert
            Assert.True(page.HasMessage("Saved!"));
        }
        
        app.Close();
    }
}
```

#### Пример 7: Реальные тесты из DotElectric

```csharp
// EditorCanvasTests.cs
using Xunit;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;

public class EditorCanvasTests
{
    [Fact]
    public void EditorCanvas_ShouldOpen()
    {
        // Arrange
        var app = Application.Launch("DotElectric.TemplateEditor.exe");
        
        using (var automation = new UIA3Automation())
        {
            var window = app.GetMainWindow(automation);
            window.WaitForIdle(TimeSpan.FromSeconds(5));
            
            // Assert
            Assert.NotNull(window);
            Assert.Contains("DotElectric", window.Name);
        }
        
        app.Close();
    }
    
    [Fact]
    public void NewTemplateButton_Click_ShouldCreateNewTab()
    {
        // Arrange
        var app = Application.Launch("DotElectric.TemplateEditor.exe");
        
        using (var automation = new UIA3Automation())
        {
            var window = app.GetMainWindow(automation);
            window.WaitForIdle(TimeSpan.FromSeconds(5));
            
            // Act
            var newButton = window.FindFirstDescendant(
                cf => cf.ByAutomationId("NewTemplateButton"));
            newButton?.Click();
            
            window.WaitForIdle(TimeSpan.FromSeconds(2));
            
            // Assert
            var tabControl = window.FindFirstDescendant(
                cf => cf.ByAutomationId("MainTabControl"));
            var tabs = tabControl.FindAllDescendants(
                cf => cf.ControlType == FlaUI.Core.Definitions.ControlType.TabItem);
            
            Assert.NotEmpty(tabs);
        }
        
        app.Close();
    }
    
    [Fact]
    public void ZoomComboBox_Change_ShouldUpdateZoom()
    {
        // Arrange
        var app = Application.Launch("DotElectric.TemplateEditor.exe");
        
        using (var automation = new UIA3Automation())
        {
            var window = app.GetMainWindow(automation);
            window.WaitForIdle(TimeSpan.FromSeconds(5));
            
            // Act
            var zoomComboBox = window.FindFirstDescendant(
                cf => cf.ByAutomationId("ZoomComboBox"));
            zoomComboBox.AsComboBox().Select("200%");
            
            window.WaitForIdle(TimeSpan.FromSeconds(1));
            
            // Assert
            var zoomText = window.FindFirstDescendant(
                cf => cf.ByAutomationId("ZoomTextBlock"));
            Assert.Contains("200%", zoomText.Name);
        }
        
        app.Close();
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 11.4.1: Launch App**

Напишите тест:
- Запуск приложения
- Проверка заголовка
- Закрытие приложения

**Задача 11.4.2: Find Element**

Напишите тест:
- Поиск кнопки по тексту
- Assert.NotNull
- Закрытие приложения

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 11.4.3: Click and Verify**

Напишите тест:
- Клик по кнопке
- Проверка результата
- WaitForIdle

**Задача 11.4.4: Enter Text**

Напишите тест:
- Ввод текста в TextBox
- Клик по кнопке
- Проверка сообщения

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 11.4.5: Page Object Pattern**

Создайте:
- Page Object класс
- Методы для действий
- Тесты с Page Object

**Задача 11.4.6: Full UI Test Suite**

Напишите тесты:
- Все основные сценарии
- 10+ тестов
- CI/CD интеграция

---

### Решения

<details>
<summary>✅ Решение задачи 11.4.1</summary>

```csharp
[Fact]
public void App_ShouldLaunch()
{
    // Arrange & Act
    var app = Application.Launch("MyApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Assert
        Assert.NotNull(window);
    }
    
    app.Close();
}
```
</details>

<details>
<summary>✅ Решение задачи 11.4.3</summary>

```csharp
[Fact]
public void LoadButton_Click_ShouldPopulateList()
{
    // Arrange
    var app = Application.Launch("MyApp.exe");
    
    using (var automation = new UIA3Automation())
    {
        var window = app.GetMainWindow(automation);
        
        // Act
        var loadButton = window.FindFirstDescendant(
            cf => cf.ByAutomationId("LoadButton"));
        loadButton.Click();
        
        window.WaitForIdle(TimeSpan.FromSeconds(5));
        
        // Assert
        var listBox = window.FindFirstDescendant(
            cf => cf.ByAutomationId("DataListBox"));
        var items = listBox.FindAllDescendants(
            cf => cf.ControlType == FlaUI.Core.Definitions.ControlType.ListItem);
        
        Assert.NotEmpty(items);
    }
    
    app.Close();
}
```
</details>

---

## Ключевые выводы

✅ **FlaUI** — библиотека для UI автоматизации  
✅ **Application.Launch()** — запуск приложения  
✅ **FindFirstDescendant()** — поиск элемента  
✅ **Click()** — клик по элементу  
✅ **WaitForIdle()** — ожидание завершения  
✅ **Page Object** — паттерн для организации тестов  
✅ **AutomationId** — лучший способ поиска элементов

---

## Дополнительные ресурсы

- [FlaUI Documentation](https://github.com/FlaUI/FlaUI)
- [UI Testing Best Practices](https://docs.microsoft.com/en-us/visualstudio/test/ui-test-automation)
- [Page Object Pattern](https://docs.microsoft.com/en-us/visualstudio/test/use-page-object-pattern-with-your-ui-automation-code)
