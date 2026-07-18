using System.Configuration;
using System.Data;
using System.Windows;

namespace Example2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
   protected override void OnStartup(StartupEventArgs e)
   {
      base.OnStartup(e);
      // Глобальная обработка исключений
      DispatcherUnhandledException += (s, args) =>
      {
         MessageBox.Show($"Ошибка: {args.Exception.Message}");
         args.Handled = true;
      };
   }
}
