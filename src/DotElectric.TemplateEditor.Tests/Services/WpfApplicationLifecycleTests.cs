using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests.Services;

public class WpfApplicationLifecycleTests
{
    // NOTE: Этот тест требует WPF контекста (Application.Current).
    // В xUnit WPF тесты требуют STAThread атрибута.
    // Поскольку WpfApplicationLifecycle вызывает Application.Current.Shutdown(),
    // мы НЕ можем протестировать это в unit тестах без мокирования.
    // Поэтому тест — только на создание экземпляра.

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var lifecycle = new WpfApplicationLifecycle();
        Assert.NotNull(lifecycle);
    }
}
