using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Exercise4
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      protected override void OnContentRendered(EventArgs e)
      {
         base.OnContentRendered(e);
         Debug.WriteLine("=== Визуальное дерево Button ===");
         var button = this.FindName("MyButton") as Button;
         VisualTreeHelper.PrintVisualTree(button);
      }
   }
}