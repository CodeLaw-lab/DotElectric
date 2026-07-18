namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Интерфейс для ViewModel модального диалога с событиями Confirm/Cancel.
/// Позволяет Window подписаться на результат без знания конкретного типа ViewModel.
/// </summary>
public interface ICustomSheetDialogVm
{
    event Action ConfirmRequested;
    event Action CancelRequested;
}
