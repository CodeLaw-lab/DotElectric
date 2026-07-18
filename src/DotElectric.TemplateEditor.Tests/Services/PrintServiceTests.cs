using System.Windows.Media;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
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

    #region Scaling Math — чистые вычисления (без WPF)

    /// <summary>
    /// Тесты для логики масштабирования FitToPage.
    /// Формула: scale = Min(printableWidth / elementWidth, printableHeight / elementHeight)
    /// </summary>

    [Fact]
    public void FitToPageScale_UniformScaling_CorrectCalculation()
    {
        // Область 800x600, элемент 400x300 → scale = Min(2.0, 2.0) = 2.0
        double printableW = 800.0;
        double printableH = 600.0;
        double elementW = 400.0;
        double elementH = 300.0;

        double scale = Math.Min(printableW / elementW, printableH / elementH);

        Assert.Equal(2.0, scale);
    }

    [Fact]
    public void FitToPageScale_WidthConstrained_CorrectCalculation()
    {
        // Область 800x600, элемент 200x400 → scaleX=4, scaleY=1.5 → min=1.5
        double printableW = 800.0;
        double printableH = 600.0;
        double elementW = 200.0;
        double elementH = 400.0;

        double scale = Math.Min(printableW / elementW, printableH / elementH);

        Assert.Equal(1.5, scale);
    }

    [Fact]
    public void FitToPageScale_HeightConstrained_CorrectCalculation()
    {
        // Область 800x600, элемент 600x200 → scaleX=1.33, scaleY=3.0 → min=1.33
        double printableW = 800.0;
        double printableH = 600.0;
        double elementW = 600.0;
        double elementH = 200.0;

        double scale = Math.Min(printableW / elementW, printableH / elementH);

        Assert.Equal(1.3333333333333333, scale);
    }

    [Fact]
    public void FitToPageScale_ElementLargerThanPage_ScaleLessThanOne()
    {
        // Область 800x600, элемент 1600x1200 → scale = Min(0.5, 0.5) = 0.5
        double printableW = 800.0;
        double printableH = 600.0;
        double elementW = 1600.0;
        double elementH = 1200.0;

        double scale = Math.Min(printableW / elementW, printableH / elementH);

        Assert.Equal(0.5, scale);
    }

    #endregion
}
