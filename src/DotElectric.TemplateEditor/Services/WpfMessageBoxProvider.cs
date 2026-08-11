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
        var wpfResult = MessageBox.Show(message, caption, ToWpfButtons(buttons), ToWpfIcon(icon));
        return ToMsgrResult(wpfResult);
    }

    internal static MessageBoxButton ToWpfButtons(MsgrButtons buttons)
    {
        return buttons switch
        {
            MsgrButtons.OK => MessageBoxButton.OK,
            MsgrButtons.OKCancel => MessageBoxButton.OKCancel,
            MsgrButtons.YesNoCancel => MessageBoxButton.YesNoCancel,
            MsgrButtons.YesNo => MessageBoxButton.YesNo,
            _ => MessageBoxButton.OK
        };
    }

    internal static MessageBoxImage ToWpfIcon(MsgrIcon icon)
    {
        return icon switch
        {
            MsgrIcon.Information => MessageBoxImage.Information,
            MsgrIcon.Warning => MessageBoxImage.Warning,
            MsgrIcon.Error => MessageBoxImage.Error,
            MsgrIcon.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        };
    }

    internal static MsgrResult ToMsgrResult(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.OK => MsgrResult.OK,
            MessageBoxResult.Cancel => MsgrResult.Cancel,
            MessageBoxResult.Yes => MsgrResult.Yes,
            MessageBoxResult.No => MsgrResult.No,
            _ => MsgrResult.None
        };
    }
}
