using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Тесты для DialogService с моками IMessageBoxProvider и IDispatcherService.
/// </summary>
public class DialogServiceTests
{
    private readonly MockMessageBoxProvider _mockMessageBox;
    private readonly MockDispatcherService _mockDispatcher;
    private readonly DialogService _dialogService;

    public DialogServiceTests()
    {
        _mockMessageBox = new MockMessageBoxProvider();
        _mockDispatcher = new MockDispatcherService();
        _dialogService = new DialogService(_mockMessageBox, _mockDispatcher);
    }

    [Fact]
    public async Task ShowUnsavedChangesDialogAsync_Yes_ReturnsSave()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        var result = await _dialogService.ShowUnsavedChangesDialogAsync("test.tdel");

        Assert.Equal(UnsavedChangesResult.Save, result);
        Assert.Equal("Несохранённые изменения", _mockMessageBox.LastCaption);
        Assert.Contains("test.tdel", _mockMessageBox.LastMessage);
    }

    [Fact]
    public async Task ShowUnsavedChangesDialogAsync_No_ReturnsDontSave()
    {
        _mockMessageBox.NextResult = MsgrResult.No;

        var result = await _dialogService.ShowUnsavedChangesDialogAsync("test.tdel");

        Assert.Equal(UnsavedChangesResult.DontSave, result);
    }

    [Fact]
    public async Task ShowUnsavedChangesDialogAsync_Cancel_ReturnsCancel()
    {
        _mockMessageBox.NextResult = MsgrResult.Cancel;

        var result = await _dialogService.ShowUnsavedChangesDialogAsync("test.tdel");

        Assert.Equal(UnsavedChangesResult.Cancel, result);
    }

    [Fact]
    public async Task ShowUnsavedChangesDialogAsync_PassesFileName()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        await _dialogService.ShowUnsavedChangesDialogAsync("my_template.tdel");

        Assert.Contains("my_template.tdel", _mockMessageBox.LastMessage);
    }

    [Fact]
    public async Task ShowRecoveryDialogAsync_Yes_ReturnsTrue()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        var result = await _dialogService.ShowRecoveryDialogAsync();

        Assert.True(result);
        Assert.Equal("Восстановление после сбоя", _mockMessageBox.LastCaption);
        Assert.Contains("несохранённые", _mockMessageBox.LastMessage);
    }

    [Fact]
    public async Task ShowRecoveryDialogAsync_No_ReturnsFalse()
    {
        _mockMessageBox.NextResult = MsgrResult.No;

        var result = await _dialogService.ShowRecoveryDialogAsync();

        Assert.False(result);
    }

    [Fact]
    public void ShowError_ShowsErrorMessage()
    {
        _dialogService.ShowError("Произошла ошибка");

        Assert.Equal("Ошибка", _mockMessageBox.LastCaption);
        Assert.Equal("Произошла ошибка", _mockMessageBox.LastMessage);
        Assert.Equal(MsgrButtons.OK, _mockMessageBox.LastButtons);
        Assert.Equal(MsgrIcon.Error, _mockMessageBox.LastIcon);
    }

    [Fact]
    public void ShowInfo_ShowsInfoMessage()
    {
        _dialogService.ShowInfo("Информация");

        Assert.Equal("DotElectric", _mockMessageBox.LastCaption);
        Assert.Equal("Информация", _mockMessageBox.LastMessage);
        Assert.Equal(MsgrButtons.OK, _mockMessageBox.LastButtons);
        Assert.Equal(MsgrIcon.Information, _mockMessageBox.LastIcon);
    }

    [Fact]
    public void ShowFatalError_ShowsErrorMessage()
    {
        _dialogService.ShowFatalError("Критическая ошибка");

        Assert.Equal("Критическая ошибка", _mockMessageBox.LastCaption);
        Assert.Equal(MsgrButtons.OK, _mockMessageBox.LastButtons);
        Assert.Equal(MsgrIcon.Error, _mockMessageBox.LastIcon);
    }

    [Fact]
    public void ShowConfirmation_Yes_ReturnsTrue()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        var result = _dialogService.ShowConfirmation("Удалить объект?");

        Assert.True(result);
        Assert.Equal("Подтверждение", _mockMessageBox.LastCaption);
        Assert.Equal("Удалить объект?", _mockMessageBox.LastMessage);
    }

    [Fact]
    public void ShowConfirmation_No_ReturnsFalse()
    {
        _mockMessageBox.NextResult = MsgrResult.No;

        var result = _dialogService.ShowConfirmation("Удалить объект?");

        Assert.False(result);
    }

    [Fact]
    public void ShowConfirmation_CustomTitle_UsesTitle()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        _dialogService.ShowConfirmation("Текст", "Код заголовок");

        Assert.Equal("Код заголовок", _mockMessageBox.LastCaption);
    }

    [Fact]
    public void ShowConfirmation_UsesYesNoButtons()
    {
        _mockMessageBox.NextResult = MsgrResult.Yes;

        _dialogService.ShowConfirmation("Вопрос");

        Assert.Equal(MsgrButtons.YesNo, _mockMessageBox.LastButtons);
        Assert.Equal(MsgrIcon.Question, _mockMessageBox.LastIcon);
    }
}

/// <summary>
/// Моковый IMessageBoxProvider для тестирования DialogService.
/// </summary>
public class MockMessageBoxProvider : IMessageBoxProvider
{
    public MsgrResult NextResult { get; set; }
    public string LastMessage { get; private set; } = string.Empty;
    public string LastCaption { get; private set; } = string.Empty;
    public MsgrButtons LastButtons { get; private set; }
    public MsgrIcon LastIcon { get; private set; }

    public MsgrResult Show(string message, string caption, MsgrButtons buttons, MsgrIcon icon)
    {
        LastMessage = message;
        LastCaption = caption;
        LastButtons = buttons;
        LastIcon = icon;
        return NextResult;
    }
}

/// <summary>
/// Моковый IDispatcherService — просто вызывает action напрямую (без Dispatcher).
/// </summary>
public class MockDispatcherService : IDispatcherService
{
    public T Invoke<T>(Func<T> action) => action();
    public void Invoke(Action action) => action();
    public Task InvokeAsync(Func<Task> action) => action();
}