using System.Windows;

namespace DotElectric.TemplateEditor.Tests.Helpers;

/// <summary>
/// Гарантирует наличие Application в тестовом хосте.
/// Создаёт полный App (ресурсы App.xaml: словари тем + глобальные конвертеры) —
/// это нужно тестам, загружающим представления с StaticResource на ресурсы
/// приложения, и попутно инициализирует pack-инфраструктуру WPF.
/// Вызывать только из STA-потока (WpfContext.Execute).
/// </summary>
public static class WpfApplicationHost
{
    public static void Ensure()
    {
        if (Application.Current != null)
            return;

        var app = new App();
        app.InitializeComponent();
    }
}
