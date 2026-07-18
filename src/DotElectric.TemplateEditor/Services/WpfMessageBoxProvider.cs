using System.Windows;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// WPF-реализация IMessageBoxProvider через System.Windows.MessageBox.
/// Эта реализация НЕ вызывает Dispatcher.Invoke — вызов делается из DialogService.
/// </summary>
public sealed class WpfMessageBoxProvider : IMessageBoxProvider
{
    public MsgrResult Show(string message, string caption, MsgrButtons buttons, MsgrIcon icon)
    {
        // Этот метод уже вызывается через Dispatcher.Invoke из DialogService
        var wpfButtons = buttons switch
        {
            MsgrButtons.OK => System.Windows.MessageBoxButton.OK,
            MsgrButtons.OKCancel => System.Windows.MessageBoxButton.OKCancel,
            MsgrButtons.YesNoCancel => System.Windows.MessageBoxButton.YesNoCancel,
            MsgrButtons.YesNo => System.Windows.MessageBoxButton.YesNo,
            _ => System.Windows.MessageBoxButton.OK
        };

        var wpfIcon = icon switch
        {
            MsgrIcon.Information => MessageBoxImage.Information,
            MsgrIcon.Warning => MessageBoxImage.Warning,
            MsgrIcon.Error => MessageBoxImage.Error,
            MsgrIcon.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        };

        var wpfResult = MessageBox.Show(message, caption, wpfButtons, wpfIcon);

        return wpfResult switch
        {
            System.Windows.MessageBoxResult.OK => MsgrResult.OK,
            System.Windows.MessageBoxResult.Cancel => MsgrResult.Cancel,
            System.Windows.MessageBoxResult.Yes => MsgrResult.Yes,
            System.Windows.MessageBoxResult.No => MsgrResult.No,
            _ => MsgrResult.None
        };
    }
}
