using System.Threading.Tasks;
using System.Windows.Threading;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тесты WpfDispatcherService с явно переданным Dispatcher (без Application).
/// Default-ctor (Application.Current.Dispatcher) не покрыт: в тестовом хосте
/// Application.Current == null (WpfContext создаёт голый STA-поток) — см. KnownLimitation.
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

    [Fact]
    public void Invoke_RunsOnPassedDispatcher()
    {
        WpfContext.Execute(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var service = new WpfDispatcherService(dispatcher);
            Dispatcher? callbackDispatcher = null;

            service.Invoke(() => callbackDispatcher = Dispatcher.CurrentDispatcher);

            Assert.Same(dispatcher, callbackDispatcher);
        });
    }

    [Fact]
    public void Invoke_Exception_PropagatesToCaller()
    {
        WpfContext.Execute(() =>
        {
            var service = new WpfDispatcherService(Dispatcher.CurrentDispatcher);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.Invoke(() => throw new InvalidOperationException("boom")));

            Assert.Equal("boom", ex.Message);
        });
    }

    [Fact]
    public void InvokeAsync_AsyncLambda_CompletesAfterContinuation()
    {
        WpfContext.Execute(() =>
        {
            var service = new WpfDispatcherService(Dispatcher.CurrentDispatcher);
            var completed = false;

            var task = service.InvokeAsync(async () =>
            {
                await Task.Yield();
                completed = true;
            });

            // Продолжение после await постится через DispatcherSynchronizationContext —
            // pump на ApplicationIdle обрабатывает и его.
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

            Assert.True(completed);
            Assert.True(task.IsCompletedSuccessfully);
        });
    }
}