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
            editor.PreviewManager.PreviewLine = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 1000,
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

            editor.PreviewManager.PreviewLine = null;
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

            editor.PreviewManager.PreviewRectangle = new Rectangle(micronsX: 0, micronsY: 0,
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

            editor.PreviewManager.PreviewRectangle = null;
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

            editor.PreviewManager.PreviewText = new Text(micronsX: 1000, micronsY: 2000,
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

            editor.PreviewManager.PreviewText = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            Assert.Equal(Visibility.Collapsed, elements.TextElement.Visibility);
        });
    }

    // ===== Anchor'ы preview (фиксация единых правил рендеринга, спека #88) =====

    [Fact]
    public void UpdatePreviewText_TopAnchor_MatchesCanvasBottomMicronsY()
    {
        // Regression: preview обязан ставить верх текста там же, где канвас после коммита —
        // по MicronsY + HeightMicrons (а не FontSizeMicrons).
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            var preview = new Text(micronsX: 1000, micronsY: 2000,
                content: "Anchor", fontSizeMicrons: 5000, fontName: "ГОСТ А");
            editor.PreviewManager.PreviewText = preview;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            var expectedTop = (sheetHeightMm - Coordinate.ToMm(preview.BottomMicronsY)) * zoom;
            Assert.Equal(expectedTop, Canvas.GetTop(elements.TextElement), 4);
        });
    }

    [Fact]
    public void UpdatePreviewText_TopAnchor_MultilinePreview_UsesFullHeight()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 2.0;
            double sheetHeightMm = 297.0;

            var preview = new Text(micronsX: 0, micronsY: 10_000,
                content: "A\nB", fontSizeMicrons: 5000, fontName: "ГОСТ Б");
            editor.PreviewManager.PreviewText = preview;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            var expectedTop = (sheetHeightMm - Coordinate.ToMm(preview.BottomMicronsY)) * zoom;
            Assert.Equal(expectedTop, Canvas.GetTop(elements.TextElement), 4);
        });
    }

    [Fact]
    public void UpdatePreviewText_LeftAnchor_UsesMicronsX()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.5;
            double sheetHeightMm = 297.0;

            editor.PreviewManager.PreviewText = new Text(micronsX: 4_000, micronsY: 2000,
                content: "Left", fontSizeMicrons: 5000);
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, zoom, sheetHeightMm);

            Assert.Equal(4.0 * 1.5, Canvas.GetLeft(elements.TextElement), 4);
        });
    }

    [Fact]
    public void UpdatePreviewText_FontFamily_FollowsRenderRules()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            editor.PreviewManager.PreviewText = new Text(micronsX: 0, micronsY: 0,
                content: "Font", fontSizeMicrons: 5000, fontName: "ГОСТ А");
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewText(canvas, editor, 1.0, 297.0);

            Assert.Equal("pack://application:,,,/Resources/Fonts/#GOST Type AU",
                elements.TextElement.FontFamily.Source);
        });
    }

    [Fact]
    public void UpdatePreviewRectangle_TopAnchor_MatchesCanvasSemantics()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            double zoom = 1.0;
            double sheetHeightMm = 297.0;

            var preview = new Rectangle(micronsX: 5_000, micronsY: 10_000,
                widthMicrons: 20_000, heightMicrons: 8_000, strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewRectangle = preview;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            PreviewLineChangedBehavior.UpdatePreviewRectangle(canvas, editor, zoom, sheetHeightMm);

            var expectedTop = (sheetHeightMm - Coordinate.ToMm(preview.MicronsY + preview.HeightMicrons)) * zoom;
            var expectedLeft = Coordinate.ToMm(preview.MicronsX) * zoom;
            Assert.Equal(expectedTop, Canvas.GetTop(elements.RectangleElement), 4);
            Assert.Equal(expectedLeft, Canvas.GetLeft(elements.RectangleElement), 4);
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
            editor.PreviewManager.PreviewLine = null;
            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);

            // Set a preview line - this triggers PropertyChanged
            editor.PreviewManager.PreviewLine = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000,
                strokeThicknessMicrons: 500);

            // The PropertyChanged handler should have updated the canvas
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);
        });
    }

    // ===== P2: рендерер подписан на INPC preview-объекта (спека #93) =====

    [Fact]
    public void MutatingPreviewLine_UpdatesElement_WithoutReassign()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            var zoom = editor.ZoomPanManager.Zoom;

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var preview = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000,
                strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewLine = preview;
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);

            // Мутация свойства без ре-ассайна — обновление приходит через INPC объекта
            preview.EndMicronsX = 20_000;

            Assert.Equal(Coordinate.ToMm(20_000) * zoom, elements.LineElement.X2, 4);
        });
    }

    [Fact]
    public void MutatingPreviewRectangle_UpdatesElement_WithoutReassign()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();
            var zoom = editor.ZoomPanManager.Zoom;

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var preview = new Rectangle(micronsX: 0, micronsY: 0,
                widthMicrons: 10_000, heightMicrons: 5_000,
                strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewRectangle = preview;
            Assert.Equal(Visibility.Visible, elements.RectangleElement.Visibility);

            preview.WidthMicrons = 30_000;

            Assert.Equal(Coordinate.ToMm(30_000) * zoom, elements.RectangleElement.Width, 4);
        });
    }

    [Fact]
    public void MutatingPreviewText_UpdatesElement_WithoutReassign()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var preview = new Text(micronsX: 0, micronsY: 0,
                content: "До", fontSizeMicrons: 5000);
            editor.PreviewManager.PreviewText = preview;
            Assert.Equal(Visibility.Visible, elements.TextElement.Visibility);

            preview.Content = "После";

            Assert.Equal("После", elements.TextElement.Text);
        });
    }

    [Fact]
    public void ClearedPreviewLine_MutationsNoLongerUpdateElement()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var preview = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000,
                strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewLine = preview;
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);
            Assert.Equal(Coordinate.ToMm(5000) * editor.ZoomPanManager.Zoom, elements.LineElement.X2, 4);

            editor.PreviewManager.PreviewLine = null;
            Assert.Equal(Visibility.Collapsed, elements.LineElement.Visibility);

            // Брошенный объект отписан: мутация не двигает элемент
            preview.EndMicronsX = 99_000;
            Assert.Equal(Coordinate.ToMm(5000) * editor.ZoomPanManager.Zoom, elements.LineElement.X2, 4);
        });
    }

    [Fact]
    public void SwappedPreviewLine_OldObjectNoLongerUpdatesElement()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var first = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000, strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewLine = first;
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);

            // Swap на живой объект: старый отписывается, новый управляет элементом
            var second = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 20_000, endMicronsY: 20_000, strokeThicknessMicrons: 500);
            editor.PreviewManager.PreviewLine = second;

            first.EndMicronsX = 99_000;
            Assert.Equal(Coordinate.ToMm(20_000) * editor.ZoomPanManager.Zoom, elements.LineElement.X2, 4);

            second.EndMicronsX = 30_000;
            Assert.Equal(Coordinate.ToMm(30_000) * editor.ZoomPanManager.Zoom, elements.LineElement.X2, 4);
        });
    }

    [Fact]
    public void Unregister_WithActivePreviews_UnsubscribesAllObjects()
    {
        WpfContext.Execute(() =>
        {
            var (canvas, elements) = CreateCanvasWithNamedElements();
            var editor = CreateEditorViewModel();

            PreviewLineChangedBehavior.RegisterCanvas(canvas, editor);
            var line = new DotElectric.TemplateEditor.Models.Objects.Line(startMicronsX: 0, startMicronsY: 0,
                endMicronsX: 5000, endMicronsY: 5000, strokeThicknessMicrons: 500);
            var rect = new Rectangle(micronsX: 0, micronsY: 0, widthMicrons: 10_000, heightMicrons: 5_000, strokeThicknessMicrons: 500);
            var text = new Text(micronsX: 0, micronsY: 0, content: "До", fontSizeMicrons: 5000);
            editor.PreviewManager.PreviewLine = line;
            editor.PreviewManager.PreviewRectangle = rect;
            editor.PreviewManager.PreviewText = text;
            Assert.Equal(Visibility.Visible, elements.LineElement.Visibility);

            PreviewLineChangedBehavior.Unregister(editor);

            // После отвязки канваса мутации не доходят до элементов (подписки сняты, утечки нет)
            line.EndMicronsX = 99_000;
            rect.WidthMicrons = 99_000;
            text.Content = "После";
            Assert.Equal(Coordinate.ToMm(5000) * editor.ZoomPanManager.Zoom, elements.LineElement.X2, 4);
            Assert.Equal(Coordinate.ToMm(10_000) * editor.ZoomPanManager.Zoom, elements.RectangleElement.Width, 4);
            Assert.Equal("До", elements.TextElement.Text);
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
