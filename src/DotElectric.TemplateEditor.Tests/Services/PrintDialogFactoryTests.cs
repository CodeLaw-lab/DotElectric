using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тест PrintDialogFactory (PrintDialog — WPF-класс, требует STA).
/// </summary>
public class PrintDialogFactoryTests
{
    [Fact]
    public void Create_ReturnsPrintDialogWrapper()
    {
        WpfContext.Execute(() =>
        {
            var factory = new PrintDialogFactory();

            var wrapper = factory.Create();

            Assert.NotNull(wrapper);
            Assert.IsType<PrintDialogWrapper>(wrapper);
        });
    }
}
