using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Комплексные тесты для PrintService с мокированием IPrintDialogFactory.
/// PrintDialog.ShowDialog() нельзя автоматизировать, поэтому используем абстракцию.
/// Тесты, требующие STA-поток (FrameworkElement), вынесены в отдельный класс.
/// </summary>
public class PrintServiceTests
{
    private readonly Mock<IPrintDialogFactory> _mockFactory;
    private readonly Mock<IPrintDialogWrapper> _mockDialog;
    private readonly PrintService _printService;

    public PrintServiceTests()
    {
        _mockFactory = new Mock<IPrintDialogFactory>();
        _mockDialog = new Mock<IPrintDialogWrapper>();
        _mockFactory.Setup(f => f.Create()).Returns(_mockDialog.Object);

        _printService = new PrintService(_mockFactory.Object);
    }

    #region PrintWithVisual — Argument-null проверки

    [Fact]
    public void PrintWithVisual_NullVisual_ThrowsArgumentNullException()
    {
        var settings = new PrintSettings();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            _printService.PrintWithVisual(null!, "test", settings));

        Assert.Equal("visual", ex.ParamName);
    }

    [Fact]
    public void PrintWithVisual_NullSettings_ThrowsArgumentNullException()
    {
        var visual = new DrawingVisual();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            _printService.PrintWithVisual(visual, "test", null!));

        Assert.Equal("settings", ex.ParamName);
    }

    #endregion

    #region PrintWithVisual — Диалог: отмена / подтверждение

    [Fact]
    public void PrintWithVisual_UserCancels_ReturnsFalse()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(false);

        var visual = new DrawingVisual();
        var settings = new PrintSettings();

        var result = _printService.PrintWithVisual(visual, "test", settings);

        Assert.False(result);
        _mockDialog.Verify(d => d.PrintVisual(It.IsAny<Visual>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PrintWithVisual_UserAccepts_ReturnsTrueAndCallsPrintVisual()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings();

        var result = _printService.PrintWithVisual(visual, "test", settings);

        Assert.True(result);
        _mockDialog.Verify(d => d.PrintVisual(visual, "test"), Times.Once);
    }

    #endregion

    #region PrintWithVisual — Масштабирование (без FrameworkElement)

    [Fact]
    public void PrintWithVisual_FitToPage_NonFrameworkElement_NoScalingApplied()
    {
        // DrawingVisual — не FrameworkElement, масштабирование не применяется
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Scaling = "FitToPage" };

        var result = _printService.PrintWithVisual(visual, "test", settings);

        Assert.True(result);
        _mockDialog.Verify(d => d.PrintVisual(visual, "test"), Times.Once);
        // PrintableAreaWidth/Height не запрашиваются, т.к. визуал не FrameworkElement
        _mockDialog.VerifyGet(d => d.PrintableAreaWidth, Times.Never);
    }

    [Fact]
    public void PrintWithVisual_CustomScale_DoesNotApplyTransform()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Scaling = "Custom", CustomScalePercent = 150.0 };

        var result = _printService.PrintWithVisual(visual, "test", settings);

        Assert.True(result);
        _mockDialog.Verify(d => d.PrintVisual(visual, "test"), Times.Once);
        // PrintableArea не запрашивается для Custom
        _mockDialog.VerifyGet(d => d.PrintableAreaWidth, Times.Never);
    }

    [Fact]
    public void PrintWithVisual_ActualSizeScaling_DoesNotApplyTransform()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Scaling = "ActualSize" };

        var result = _printService.PrintWithVisual(visual, "test", settings);

        Assert.True(result);
        _mockDialog.VerifyGet(d => d.PrintableAreaWidth, Times.Never);
    }

    #endregion

    #region PrintWithVisual — RenderTransform restore (finally) для non-FrameworkElement

    [Fact]
    public void PrintWithVisual_NoScaling_ExceptionStillReturnsFromFinally()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);
        _mockDialog.Setup(d => d.PrintVisual(It.IsAny<Visual>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Printer error"));

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Scaling = "ActualSize" };

        Assert.Throws<InvalidOperationException>(() =>
            _printService.PrintWithVisual(visual, "test", settings));

        // finally блок выполнился без ошибок (тест не упал с NullReferenceException)
    }

    #endregion

    #region PrintWithVisual — Printer name и Copies

    [Fact]
    public void PrintWithVisual_PrinterNameIsSet_WhenSpecified()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { PrinterName = "MyPrinter" };

        _printService.PrintWithVisual(visual, "test", settings);

        _mockDialog.VerifySet(d => d.PrinterName = "MyPrinter", Times.Once);
    }

    [Fact]
    public void PrintWithVisual_PrinterNameNotSet_WhenNull()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);
        _mockDialog.Setup(d => d.PrinterName).Returns((string?)null);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { PrinterName = null };

        _printService.PrintWithVisual(visual, "test", settings);

        // PrinterName не должен быть установлен
        _mockDialog.VerifySet(d => d.PrinterName = It.IsAny<string>(), Times.Never);
    }

    [Fact]
    public void PrintWithVisual_CopiesCountIsSet_WhenGreaterThanOne()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Copies = 3 };

        _printService.PrintWithVisual(visual, "test", settings);

        _mockDialog.VerifySet(d => d.Copies = 3, Times.Once);
    }

    [Fact]
    public void PrintWithVisual_CopiesNotSet_WhenEqualsOne()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var visual = new DrawingVisual();
        var settings = new PrintSettings { Copies = 1 };

        _printService.PrintWithVisual(visual, "test", settings);

        _mockDialog.VerifySet(d => d.Copies = It.IsAny<int>(), Times.Never);
    }

    #endregion

    #region ShowPrintDialog

    [Fact]
    public void ShowPrintDialog_UserAccepts_ReturnsTrue()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        var result = _printService.ShowPrintDialog();

        Assert.True(result);
    }

    [Fact]
    public void ShowPrintDialog_UserCancels_ReturnsFalse()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(false);

        var result = _printService.ShowPrintDialog();

        Assert.False(result);
    }

    [Fact]
    public void ShowPrintDialog_CreatesNewDialog()
    {
        _mockDialog.Setup(d => d.ShowDialog()).Returns(true);

        _printService.ShowPrintDialog();

        _mockFactory.Verify(f => f.Create(), Times.Once);
        _mockDialog.Verify(d => d.ShowDialog(), Times.Once);
    }

    #endregion

    #region PrintSettings — значения по умолчанию

    [Fact]
    public void PrintSettings_DefaultValues_AreCorrect()
    {
        var settings = new PrintSettings();

        Assert.Equal(1, settings.Copies);
        Assert.Equal("FitToPage", settings.Scaling);
        Assert.Equal(100.0, settings.CustomScalePercent);
        Assert.True(settings.Color);
        Assert.Null(settings.PrinterName);
    }

    #endregion

    #region Constructor

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new PrintService(null!));

        Assert.Equal("dialogFactory", ex.ParamName);
    }

    #endregion

}

/// <summary>
/// STA-тесты PrintService с реальным FrameworkElement (Border):
/// FitToPage scale, restore RenderTransform, zero-size no-op.
/// </summary>
public class PrintServiceStaTests
{
    private static (Mock<IPrintDialogFactory> factory, Mock<IPrintDialogWrapper> dialog) CreateMocks()
    {
        var factory = new Mock<IPrintDialogFactory>();
        var dialog = new Mock<IPrintDialogWrapper>();
        factory.Setup(f => f.Create()).Returns(dialog.Object);
        return (factory, dialog);
    }

    private static Border CreateMeasuredElement(double width, double height)
    {
        var border = new Border { Width = width, Height = height };
        border.Measure(new Size(width, height));
        border.Arrange(new Rect(0, 0, width, height));
        return border;
    }

    [Fact]
    public void PrintWithVisual_FitToPage_FrameworkElement_AppliesScale()
    {
        WpfContext.Execute(() =>
        {
            var (factory, dialog) = CreateMocks();
            dialog.SetupGet(d => d.PrintableAreaWidth).Returns(800.0);
            dialog.SetupGet(d => d.PrintableAreaHeight).Returns(600.0);
            dialog.Setup(d => d.ShowDialog()).Returns(true);

            var border = CreateMeasuredElement(400, 300);
            ScaleTransform? capturedScale = null;
            dialog.Setup(d => d.PrintVisual(It.IsAny<Visual>(), It.IsAny<string>()))
                .Callback(() => capturedScale = border.RenderTransform as ScaleTransform);

            var service = new PrintService(factory.Object);
            var result = service.PrintWithVisual(border, "test", new PrintSettings { Scaling = "FitToPage" });

            Assert.True(result);
            Assert.NotNull(capturedScale);
            Assert.Equal(2.0, capturedScale.ScaleX, precision: 3);
            Assert.Equal(2.0, capturedScale.ScaleY, precision: 3);
        });
    }

    [Fact]
    public void PrintWithVisual_FitToPage_RestoresOriginalTransformInFinally()
    {
        WpfContext.Execute(() =>
        {
            var (factory, dialog) = CreateMocks();
            dialog.SetupGet(d => d.PrintableAreaWidth).Returns(800.0);
            dialog.SetupGet(d => d.PrintableAreaHeight).Returns(600.0);
            dialog.Setup(d => d.ShowDialog()).Returns(true);

            var border = CreateMeasuredElement(400, 300);
            var original = new RotateTransform(45);
            border.RenderTransform = original;

            var service = new PrintService(factory.Object);
            service.PrintWithVisual(border, "test", new PrintSettings { Scaling = "FitToPage" });

            Assert.Same(original, border.RenderTransform);
        });
    }

    [Fact]
    public void PrintWithVisual_FitToPage_ZeroElementSize_NoTransform()
    {
        WpfContext.Execute(() =>
        {
            var (factory, dialog) = CreateMocks();
            dialog.SetupGet(d => d.PrintableAreaWidth).Returns(800.0);
            dialog.SetupGet(d => d.PrintableAreaHeight).Returns(600.0);
            dialog.Setup(d => d.ShowDialog()).Returns(true);

            // Без Measure/Arrange ActualWidth/Height == 0 → transform не применяется
            var border = new Border { Width = 400, Height = 300 };
            var original = new RotateTransform(45);
            border.RenderTransform = original;

            var service = new PrintService(factory.Object);
            service.PrintWithVisual(border, "test", new PrintSettings { Scaling = "FitToPage" });

            Assert.Same(original, border.RenderTransform);
            // Размер страницы запрашивается, но scale-transform НЕ применяется
            // (elementSize == 0 → ветка с установкой RenderTransform не выполняется).
            dialog.VerifyGet(d => d.PrintableAreaWidth, Times.Once);
        });
    }

    [Fact]
    public void PrintWithVisual_FitToPage_Cancel_NoPrintVisualAndRestoresTransform()
    {
        WpfContext.Execute(() =>
        {
            var (factory, dialog) = CreateMocks();
            dialog.SetupGet(d => d.PrintableAreaWidth).Returns(800.0);
            dialog.SetupGet(d => d.PrintableAreaHeight).Returns(600.0);
            dialog.Setup(d => d.ShowDialog()).Returns(false);

            var border = CreateMeasuredElement(400, 300);
            var original = new RotateTransform(45);
            border.RenderTransform = original;

            var service = new PrintService(factory.Object);
            var result = service.PrintWithVisual(border, "test", new PrintSettings { Scaling = "FitToPage" });

            Assert.False(result);
            dialog.Verify(d => d.PrintVisual(It.IsAny<Visual>(), It.IsAny<string>()), Times.Never);
            Assert.Same(original, border.RenderTransform);
        });
    }
}
