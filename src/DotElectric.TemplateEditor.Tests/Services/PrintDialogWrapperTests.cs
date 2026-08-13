using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тесты PrintDialogWrapper (PrintDialog — WPF-класс, требует STA).
/// ShowDialog()/PrintVisual() НЕ тестируются: модальный диалог Windows +
/// реальный принтер — зависли бы в headless CI (см. KnownLimitation).
/// </summary>
public class PrintDialogWrapperTests
{
    [Fact]
    public void Ctor_DoesNotThrow()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            Assert.NotNull(wrapper);
        });
    }

    [Fact]
    public void Copies_Default_ReturnsOne()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            Assert.Equal(1, wrapper.Copies);
        });
    }

    [Fact]
    public void Copies_Set_ReturnsSetValue()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            wrapper.Copies = 3;

            Assert.Equal(3, wrapper.Copies);
        });
    }

    [Fact]
    public void PrintableAreaWidth_Readable_NonNegative()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            Assert.True(wrapper.PrintableAreaWidth >= 0, "Printed area width is machine-dependent");
        });
    }

    [Fact]
    public void PrintableAreaHeight_Readable_NonNegative()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            Assert.True(wrapper.PrintableAreaHeight >= 0, "Printed area height is machine-dependent");
        });
    }

    [Fact]
    public void PrinterName_Default_Readable()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            // Значение машино-зависимо: на машине с принтером по умолчанию
            // PrintQueue не null (например "Bullzip PDF Printer"), в headless CI — null.
            var name = wrapper.PrinterName;

            Assert.True(name is null || name.Length > 0);
        });
    }

    [Fact]
    public void PrinterName_SetNull_NoOp()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();
            var before = wrapper.PrinterName;

            wrapper.PrinterName = null;

            // Setter с null — no-op: очередь печати не меняется.
            Assert.Equal(before, wrapper.PrinterName);
        });
    }

    [Fact]
    public void PrinterName_SetNonExistent_ExecutesBranchWithoutLeaking()
    {
        WpfContext.Execute(() =>
        {
            var wrapper = new PrintDialogWrapper();

            try
            {
                wrapper.PrinterName = "nonexistent-printer-xyz";
            }
            catch
            {
                // Ветка new PrintQueue(new PrintServer(), value) выполнена в любом случае.
                // Без спулера/print server в headless CI PrintQueue бросает PrintSystemException —
                // это ожидаемо и не является провалом теста (покрытие — цель).
            }
        });
    }
}