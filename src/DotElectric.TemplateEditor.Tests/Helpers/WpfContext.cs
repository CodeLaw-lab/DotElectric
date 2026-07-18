using System.Windows.Threading;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public static class WpfContext
{
    public static void Execute(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("WpfContext test failed", exception);
        }
    }
}
