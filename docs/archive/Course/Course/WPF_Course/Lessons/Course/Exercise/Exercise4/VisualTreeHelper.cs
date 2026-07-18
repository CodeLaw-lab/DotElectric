using System.Diagnostics;
using System.Windows;

namespace Exercise4;

public static class VisualTreeHelper
{
   public static void PrintVisualTree(DependencyObject element, int indent = 0)
   {
      if (element == null) return;

      var name = element.GetType().Name;

      if (element is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
      {
         name = $"{name} (x:Name={fe.Name})";
      }

      Debug.WriteLine(new string(' ', indent * 2) + name);

      int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
      for (int i = 0; i < count; i++)
      {
         var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
         PrintVisualTree(child, indent + 1);
      }
   }
}
