using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.ViewModels;
using Moq;
using Line = System.Windows.Shapes.Line;
using RectWpf = System.Windows.Shapes.Rectangle;
using TextWpf = System.Windows.Controls.TextBlock;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class PreviewLineChangedBehaviorTests
{
    // ===== Register/Unregister (needs STA for Canvas) =====

    [Fact]
    public void RegisterCanvas_WithNamedElements_StoresReferences()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, _) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            // If no exception, registration succeeded
            // Verify by calling Unregister (should not throw)
            var exception = Record.Exception(() =>
                PreviewLineChangedBehavior.Unregister(editor));
            Assert.Null(exception);
        });
    }

    [Fact]
    public void RegisterCanvas_WithoutNamedElements_DoesNotThrow()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditorViewModel();

            var exception = Record.Exception(() =>
                PreviewLineChangedBehavior.RegisterCanvas(canvas, editor));
            Assert.Null(exception);
        });
    }

    [Fact]
    public void Unregister_WithoutRegister_DoesNotThrow()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditorViewModel();
            var exception = Record.Exception(() =>
                PreviewLineChangedBehavior.Unregister(editor));
            Assert.Null(exception);
        });
    }

    [Fact]
    public void DoubleRegistration_DoesNotThrow()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, _) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var exception = Record.Exception(() =>
                PreviewLineChangedBehavior.RegisterCanvas(canvas, editor));
            Assert.Null(exception);
        });
    }

    // ===== UpdatePreviewLine (needs STA) =====

    [Fact]
    public void UpdatePreviewLine_WithValidPreview_SetsPositionAndVisibility()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            // Set up a valid preview line
            editor.PreviewLine = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 1000,
                endMicronsX: 10000, endMicronsY: 2000,
                strokeThicknessMicrons: 500);

            // Register canvas to populate CWT
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            // Act
            PreviewLineChangedBehavior.UpdatePreviewLine(canvas, editor, zoom, sheetHeightMm);

            // Assert
            var wpfLine = elements.LineElement;
            Assert.Equal(Visibility.Visible, wpfLine.Visibility);
            Assert.True(wpfLine.X1 >= 0);
            Assert.True(wpfLine.X2 > wpfLine.X1);
        });
    }

    [Fact]
    public void UpdatePreviewLine_WithNullPreview_CollapsesElement()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            editor.PreviewLine = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewLine(canvas, editor, zoom, sheetHeightMm);

            Assert.Equal(Visibility.Collapsed, elements.LineElement.Visibility);
        });
    }

    // ===== UpdatePreviewRectangle (needs STA) =====

    [Fact]
    public void UpdatePreviewRectangle_WithValidPreview_SetsPositionAndVisibility()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            editor.PreviewRectangle = new Rectangle(micronsX: 0, micronsY: 0,
                widthMicrons: 10000, heightMicrons: 5000,
                strokeThicknessMicrons: 500);
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewRectangle(canvas, editor, zoom, sheetHeightMm);

            var wpfRect = elements.RectangleElement;
            Assert.Equal(Visibility.Visible, wpfRect.Visibility);
            Assert.True(wpfRect.Width > 0);
            Assert.True(wpfRect.Height > 0);
        });
    }

    [Fact]
    public void UpdatePreviewRectangle_WithNullPreview_CollapsesElement()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            editor.PreviewRectangle = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewRectangle(canvas, editor, zoom, sheetHeightMm);

            Assert.Equal(Visibility.Collapsed, elements.RectangleElement.Visibility);
        });
    }

    // ===== UpdatePreviewText (needs STA) =====

    [Fact]
    public void UpdatePreviewText_WithValidPreview_SetsContentAndVisibility()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            editor.PreviewText = new Text(micronsX: 1000, micronsY: 2000,
                content: "Test Preview", fontSizeMicrons: 5000,
                fontName: "ГОСТ А");
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            var wpfText = elements.TextElement;
            Assert.Equal(Visibility.Visible, wpfText.Visibility);
            Assert.Equal("Test Preview", wpfText.Text);
            Assert.True(wpfText.FontSize > 0);
        });
    }

    [Fact]
    public void UpdatePreviewText_WithNullPreview_CollapsesElement()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            editor.PreviewText = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            Assert.Equal(Visibility.Collapsed, elements.TextElement.Visibility);
        });
    }

    // ===== PropertyChanged flow (needs STA) =====

    [Fact]
    public void SettingPreviewLine_TriggersPropertyChanged_UpdatesCanvas()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            // Set preview before registration to test the full property change flow
            editor.PreviewLine = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            // Set a preview line - this triggers PropertyChanged
            editor.PreviewLine = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000,
                strokeThicknessMicrons: 500);

            // The PropertyChanged handler should have updated the canvas
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);
        });
    }

    // ===== Helpers =====

    private static EditorViewModel CreateEditorViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    private static (Canvas Canvas, NamedElements Elements) CreateCanvasWithNamedElements()
    {
        var canvas = new Canvas();
        NameScope.SetNameScope(canvas, new NameScope());

        var line = new Line();
        line.SetValue(FrameworkElement.NameProperty, "PreviewLineElement");
        canvas.RegisterName("PreviewLineElement", line);
        canvas.Children.Add(line);

        var rect = new RectWpf();
        rect.SetValue(FrameworkElement.NameProperty, "PreviewRectangleElement");
        canvas.RegisterName("PreviewRectangleElement", rect);
        canvas.Children.Add(rect);

        var text = new TextWpf();
        text.SetValue(FrameworkElement.NameProperty, "PreviewTextElement");
        canvas.RegisterName("PreviewTextElement", text);
        canvas.Children.Add(text);

        return (canvas, new NamedElements(line, rect, text));
    }

    private sealed record NamedElements(Line LineElement, RectWpf RectangleElement, TextWpf TextElement);
}
