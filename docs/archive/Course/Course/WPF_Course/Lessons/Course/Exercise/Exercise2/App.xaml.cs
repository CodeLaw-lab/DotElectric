using System.Configuration;
using System.Data;
using System.Windows;

namespace Exercise2
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      protected override void OnStartup(StartupEventArgs e)
      {
         base.OnStartup(e);

         DispatcherUnhandledException += (s, args) =>
         {
            MessageBox.Show(
               $"Произошла ошибка:\n{args.Exception.Message}",
               "Ошибка",
               MessageBoxButton.OK,
               MessageBoxImage.Error);
            args.Handled = true;
         };
      }
   }

}
