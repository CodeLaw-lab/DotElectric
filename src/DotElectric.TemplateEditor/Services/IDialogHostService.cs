namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Сервис для показа модальных диалогов с кастомными ViewModel.
/// Отделён от IDialogService для тестируемости (MessageBox ≠ Window).
/// </summary>
public interface IDialogHostService
{
    /// <summary>
    /// Показать модальное окно с заданной ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel диалога.</param>
    /// <param name="owner">Владелец окна (опционально).</param>
    /// <returns>true = ОК, false = Отмена.</returns>
    bool? ShowDialog(object viewModel, object? owner = null);
}
