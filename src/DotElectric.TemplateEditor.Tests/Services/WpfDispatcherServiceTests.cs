using System.Threading.Tasks;
using System.Windows.Threading;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тесты WpfDispatcherService с явно переданным Dispatcher (без Application).
/// </summary>
public class WpfDispatcherServiceTests
{
    [Fact]
    public void Invoke_ExecutesActionOnDispatcher()
    {
        WpfContext.Execute(() =>
        {
            var service = new WpfDispatcherService(Dispatcher.CurrentDispatcher);
            var executed = false;

            service.Invoke(() => executed = true);

            Assert.True(executed);
        });
    }

    [Fact]
    public void Invoke_ReturnsResult()
    {
        WpfContext.Execute(() =>
        {
            var service = new WpfDispatcherService(Dispatcher.CurrentDispatcher);

            var result = service.Invoke(() => 42);

            Assert.Equal(42, result);
        });
    }

    [Fact]
    public void InvokeAsync_CompletesTask()
    {
        WpfContext.Execute(() =>
        {
            var service = new WpfDispatcherService(Dispatcher.CurrentDispatcher);
            var completed = false;

            var task = service.InvokeAsync(() =>
            {
                completed = true;
                return Task.CompletedTask;
            });

            // Pump dispatcher queue — Wait() would deadlock the STA thread
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

            Assert.True(completed);
            Assert.True(task.IsCompletedSuccessfully);
        });
    }
}
