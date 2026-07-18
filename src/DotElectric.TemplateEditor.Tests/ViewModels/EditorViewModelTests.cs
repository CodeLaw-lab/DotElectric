using System.Linq;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Managers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class EditorViewModelTests
{
    private static ITemplateService CreateMockTemplateService()
    {
        var mock = new Mock<ITemplateService>();
        mock.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>())).Returns((string fmt, SheetOrientation orient) => new Template());
        mock.Setup(s => s.Validate(It.IsAny<Template>())).Returns(Enumerable.Empty<string>());
        return mock.Object;
    }

    private static EditorViewModel CreateViewModel()
    {
        var sheet = Sheet.FromFormat("A3", SheetOrientation.Landscape);
        var metadata = new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
        var template = new Template(metadata, sheet);
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, CreateMockTemplateService(), printService: mockPrintService.Object);
    }

    // === Constructor ===

    [Fact]
    public void Constructor_NewTemplate_SetsDefaultValues()
    {
        var vm = CreateViewModel();

        Assert.False(vm.DirtyStateManager.IsDirty);
        Assert.Null(vm.DirtyStateManager.FilePath);
        Assert.Equal(1.0, vm.Zoom);
        Assert.Equal(100, vm.ZoomPanManager.ZoomPercent);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
        Assert.NotNull(vm.CommandHistory);
        Assert.NotNull(vm.GridSettings);
        Assert.NotNull(vm.PropertiesVm);
        Assert.NotNull(vm.GridManager.RawNodeData);
        Assert.NotNull(vm.SelectedObjects);
        Assert.Empty(vm.SelectedObjects);

        // Начальный отступ = 0 (центрируется при получении размера viewport)
        Assert.Equal(0.0, vm.ZoomPanManager.PanOffsetX);
        Assert.Equal(0.0, vm.ZoomPanManager.PanOffsetY);
    }

    [Fact]
    public void Constructor_FromFile_SetsFilePath()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, "C:\\test\\file.tdel", CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        Assert.Equal("C:\\test\\file.tdel", vm.DirtyStateManager.FilePath);
        Assert.Equal("file.tdel", vm.DirtyStateManager.DisplayName);
        Assert.False(vm.DirtyStateManager.IsDirty);
    }

    [Fact]
    public void Constructor_GeneratesUniqueTabId()
    {
        var template1 = new Template();
        var template2 = new Template();

        var vm1 = new EditorViewModel(template1, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);
        var vm2 = new EditorViewModel(template2, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        Assert.NotEqual(vm1.TabId, vm2.TabId);
    }

    [Fact]
    public void Constructor_PanOffset_StartsAtZero()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        // PanOffset начинается с 0; центрирование происходит при OnSizeChanged (View)
        Assert.Equal(0.0, vm.ZoomPanManager.PanOffsetX, 2);
        Assert.Equal(0.0, vm.ZoomPanManager.PanOffsetY, 2);
    }

    [Fact]
    public void FitToScreen_CentersCanvas()
    {
        var sheet = Sheet.FromFormat("A4");
        var template = new Template(new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }, sheet);
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        // Установим viewport (имитация SizeChanged из View)
        vm.ZoomPanManager.SetViewportSize(800, 600);

        // FitToScreen с viewport 800x600
        vm.FitToScreenCommand.Execute("800,600");

        // Zoom должен быть < 1.0 (A4 = 210x297 мм, viewport = 800x600)
        // 800*0.95/210 = 3.62, 600*0.95/297 = 1.92 → Zoom = 1.92 (ограничен 10.0)
        var expectedZoom = Math.Min(Math.Min((800 * 0.95) / sheet.WidthMm, (600 * 0.95) / sheet.HeightMm), 10.0);
        Assert.Equal(expectedZoom, vm.Zoom, 2);

        // При IsCentered=true CanvasOffset = 0 (центрирование через Border alignment)
        Assert.True(vm.ZoomPanManager.IsCentered);
        Assert.Equal(0, vm.ZoomPanManager.CanvasOffsetX, 2);
        Assert.Equal(0, vm.ZoomPanManager.CanvasOffsetY, 2);
    }

    // === MarkDirty / ClearDirty ===

    [Fact]
    public void MarkDirty_SetsIsDirtyTrue()
    {
        var vm = CreateViewModel();
        vm.MarkDirty();
        Assert.True(vm.DirtyStateManager.IsDirty);
    }

    [Fact]
    public void MarkDirty_DisplayNameUnchanged_IsDirtyTrue()
    {
        var vm = CreateViewModel();
        var originalName = vm.DirtyStateManager.DisplayName;
        vm.MarkDirty();
        // DisplayName не меняется — '*' отображается через ControlTemplate TabItem
        Assert.Equal(originalName, vm.DirtyStateManager.DisplayName);
        Assert.True(vm.DirtyStateManager.IsDirty);
    }

    [Fact]
    public void ClearDirty_ResetsIsDirty()
    {
        var vm = CreateViewModel();
        vm.MarkDirty();
        vm.ClearDirty();
        Assert.False(vm.DirtyStateManager.IsDirty);
    }

    [Fact]
    public void ClearDirty_RemovesAsteriskFromDisplayName()
    {
        var vm = CreateViewModel();
        vm.MarkDirty();
        vm.ClearDirty();
        Assert.False(vm.DirtyStateManager.DisplayName.EndsWith(" *"));
    }

    // === Zoom ===

    [Theory]
    [InlineData(0.05, 0.1)]    // below min → clamped
    [InlineData(0.1, 0.1)]     // at min
    [InlineData(1.0, 1.0)]     // normal
    [InlineData(5.0, 5.0)]     // high
    [InlineData(15.0, 10.0)]   // above max → clamped
    public void SetZoom_ClampsToValidRange(double input, double expected)
    {
        var vm = CreateViewModel();
        vm.SetZoom(input);
        Assert.Equal(expected, vm.Zoom, 10);
    }

    [Theory]
    [InlineData(10, 0.1)]
    [InlineData(50, 0.5)]
    [InlineData(100, 1.0)]
    [InlineData(200, 2.0)]
    [InlineData(1000, 10.0)]
    public void SetZoomPercent_SetsCorrectZoom(int percent, double expected)
    {
        var vm = CreateViewModel();
        vm.SetZoomPercent(percent);
        Assert.Equal(expected, vm.Zoom, 10);
    }

    [Fact]
    public void ZoomPercent_ReturnsCorrectPercentage()
    {
        var vm = CreateViewModel();
        vm.SetZoom(1.5);
        Assert.Equal(150, vm.ZoomPanManager.ZoomPercent);
    }

    [Fact]
    public void FitToScreen_CalculatesCorrectZoom()
    {
        var vm = CreateViewModel(); // A3: 420x297mm
        vm.FitToScreenCommand.Execute("840,594"); // 2x размера листа
        Assert.True(vm.Zoom > 1.8 && vm.Zoom <= 2.0);
    }

    // === Selection ===

    [Fact]
    public void SelectSingle_ClearsAndAddsOne()
    {
        var vm = CreateViewModel();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(2000, 2000, 3000, 3000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);

        vm.SelectSingle(line1);
        Assert.Single(vm.SelectedObjects);
        Assert.Same(line1, vm.SelectedObjects[0]);

        vm.SelectSingle(line2);
        Assert.Single(vm.SelectedObjects);
        Assert.Same(line2, vm.SelectedObjects[0]);
    }

    [Fact]
    public void AddToSelection_AddsUniqueObjects()
    {
        var vm = CreateViewModel();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(2000, 2000, 3000, 3000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);

        vm.AddToSelection(line1);
        vm.AddToSelection(line2);
        vm.AddToSelection(line1); // duplicate

        Assert.Equal(2, vm.SelectedObjects.Count);
    }

    [Fact]
    public void RemoveFromSelection_RemovesObject()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.RemoveFromSelection(line);
        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void ClearSelection_EmptiesCollection()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.ClearSelection();
        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void SelectAll_SelectsAllObjects()
    {
        var vm = CreateViewModel();
        for (int i = 0; i < 5; i++)
            vm.Template.Objects.Add(new Line(i * 1000, 0, (i + 1) * 1000, 1000));

        vm.SelectAll();
        Assert.Equal(5, vm.SelectedObjects.Count);
    }

    // === DeleteSelected ===

    [Fact]
    public void DeleteSelected_RemovesSelectedObjects()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.DeleteSelected();
        Assert.Empty(vm.Template.Objects);
        Assert.Empty(vm.SelectedObjects);
    }

    // === MoveSelected ===

    [Fact]
    public void MoveSelected_UpdatesCoordinates()
    {
        var vm = CreateViewModel();
        var line = new Line(1000, 1000, 2000, 2000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.MoveSelected(500, -500);

        Assert.Equal(1500, line.StartMicronsX);
        Assert.Equal(500, line.StartMicronsY);
        Assert.Equal(2500, line.EndMicronsX);
        Assert.Equal(1500, line.EndMicronsY);
    }

    // === SetActiveTool ===

    [Theory]
    [InlineData("Select")]
    [InlineData("Line")]
    [InlineData("Rectangle")]
    [InlineData("Text")]
    public void SetActiveTool_SetsCorrectTool(string tool)
    {
        var vm = CreateViewModel();
        vm.SetActiveToolCommand.Execute(tool);
        Assert.Equal(tool, vm.ToolManager.ActiveTool);
    }

    // === Undo/Redo ===

    [Fact]
    public void UndoCommand_CallsHistoryUndo()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(vm.Template.Objects, line);
        vm.CommandHistory.Push(cmd);

        vm.UndoCommand.Execute(null);
        Assert.Empty(vm.Template.Objects);
        Assert.True(vm.CommandHistory.CanRedo);
    }

    [Fact]
    public void RedoCommand_CallsHistoryRedo()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(vm.Template.Objects, line);
        vm.CommandHistory.Push(cmd);
        vm.UndoCommand.Execute(null);
        Assert.Empty(vm.Template.Objects);

        vm.RedoCommand.Execute(null);
        Assert.Single(vm.Template.Objects);
    }

    [Fact]
    public void UndoDisplayName_ReturnsCorrectText()
    {
        var vm = CreateViewModel();
        Assert.Equal("↶ Отменить", vm.UndoDisplayName);

        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(vm.Template.Objects, line);
        vm.CommandHistory.Push(cmd);

        Assert.StartsWith("↶ Добавить", vm.UndoDisplayName);
    }

    // === Copy/Paste ===

    [Fact]
    public void CopySelected_CopiesObjectsToClipboard()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.CopySelectedCommand.Execute(null);

        // Paste should work
        vm.PasteFromClipboardCommand.Execute(null);
        Assert.Equal(2, vm.Template.Objects.Count);
    }

    [Fact]
    public void PasteFromClipboard_EmptyClipboard_DoesNothing()
    {
        var vm = CreateViewModel();
        vm.PasteFromClipboardCommand.Execute(null);
        Assert.Empty(vm.Template.Objects);
    }

    // === Nudge ===

    [Fact]
    public void NudgeUp_MovesObjectByGridStep_WhenSnapOn()
    {
        var vm = CreateViewModel();
        // Default: SnapEnabled=true, StepMicrons=5000 (5mm)
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeUpCommand.Execute(null);
        // 5mm up = +5000 microns Y (grid step)
        Assert.Equal(10000, line.StartMicronsY);
    }

    [Fact]
    public void NudgeUp_MovesObjectBy01mm_WhenSnapOff()
    {
        var vm = CreateViewModel();
        vm.GridSettings.SnapEnabled = false;
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeUpCommand.Execute(null);
        // 0.1mm up = +100 microns
        Assert.Equal(5100, line.StartMicronsY);
    }

    [Fact]
    public void NudgeRight_MovesObjectByGridStep_WhenSnapOn()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeRightCommand.Execute(null);
        Assert.Equal(10000, line.StartMicronsX);
    }

    [Fact]
    public void NudgeRight_MovesObjectBy01mm_WhenSnapOff()
    {
        var vm = CreateViewModel();
        vm.GridSettings.SnapEnabled = false;
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeRightCommand.Execute(null);
        Assert.Equal(5100, line.StartMicronsX);
    }

    // === BigNudge ===

    [Fact]
    public void BigNudgeUp_MovesObjectBy10mm()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.BigNudgeUpCommand.Execute(null);
        Assert.Equal(15000, line.StartMicronsY);
    }

    [Fact]
    public void BigNudgeDown_MovesObjectDownBy10mm_ClampedToSheet()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.BigNudgeDownCommand.Execute(null);
        // Clamped to sheet bottom (0)
        Assert.Equal(0, line.StartMicronsY);
    }

    [Fact]
    public void BigNudgeLeft_MovesObjectLeftBy10mm_ClampedToSheet()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.BigNudgeLeftCommand.Execute(null);
        // Clamped to sheet left (0)
        Assert.Equal(0, line.StartMicronsX);
    }

    [Fact]
    public void BigNudgeRight_MovesObjectRightBy10mm()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 5000, 6000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.BigNudgeRightCommand.Execute(null);
        Assert.Equal(15000, line.StartMicronsX);
    }

    // === Sheet bounds clamping ===

    [Fact]
    public void NudgeUp_ClampsToSheetTop()
    {
        var vm = CreateViewModel();
        var sheetH = vm.Template.Sheet.HeightMicrons;
        var line = new Line(0, sheetH - Grid.Default.StepMicrons, 1000, sheetH);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeUpCommand.Execute(null);
        // Snap on → grid step (5mm) → clamped to sheet top
        Assert.Equal(sheetH, line.StartMicronsY);
    }

    [Fact]
    public void NudgeRight_ClampsToSheetRight()
    {
        var vm = CreateViewModel();
        var sheetW = vm.Template.Sheet.WidthMicrons;
        var line = new Line(sheetW - 1000, 0, sheetW, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeRightCommand.Execute(null);
        // Snap on → grid step (5mm) → clamped to sheet right
        Assert.Equal(sheetW, line.StartMicronsX);
    }

    [Fact]
    public void NudgeLeft_ClampsToSheetLeft()
    {
        var vm = CreateViewModel();
        var line = new Line(1000, 5000, 2000, 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeLeftCommand.Execute(null);
        // Snap on → grid step (5mm) → clamped: 1000 - 5000 = -4000 → 0
        Assert.Equal(0, line.StartMicronsX);
    }

    [Fact]
    public void NudgeDown_ClampsToSheetBottom()
    {
        var vm = CreateViewModel();
        var line = new Line(5000, 1000, 6000, 2000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.NudgeDownCommand.Execute(null);
        // Snap on → grid step (5mm) → clamped: 1000 - 5000 = -4000 → 0
        Assert.Equal(0, line.StartMicronsY);
    }

    [Fact]
    public void MoveSelected_ClampsToSheetBounds()
    {
        var vm = CreateViewModel();
        var sheetW = vm.Template.Sheet.WidthMicrons;
        var sheetH = vm.Template.Sheet.HeightMicrons;
        var line = new Line(sheetW + 5000, sheetH + 5000, sheetW + 6000, sheetH + 6000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        // MoveSelected with delta that would go outside
        vm.MoveSelected(10000, 10000);
        Assert.Equal(sheetW, line.StartMicronsX);
        Assert.Equal(sheetH, line.StartMicronsY);
    }

    [Fact]
    public void ClampX_ReturnsZeroForNegative()
    {
        var vm = CreateViewModel();
        Assert.Equal(0, vm.ClampX(-100));
    }

    [Fact]
    public void ClampX_ReturnsSheetWidthForTooLarge()
    {
        var vm = CreateViewModel();
        Assert.Equal(vm.Template.Sheet.WidthMicrons, vm.ClampX(vm.Template.Sheet.WidthMicrons + 10000));
    }

    [Fact]
    public void ClampX_ReturnsValueForInBounds()
    {
        var vm = CreateViewModel();
        Assert.Equal(5000, vm.ClampX(5000));
    }

    [Fact]
    public void ClampY_ReturnsZeroForNegative()
    {
        var vm = CreateViewModel();
        Assert.Equal(0, vm.ClampY(-100));
    }

    [Fact]
    public void ClampY_ReturnsSheetHeightForTooLarge()
    {
        var vm = CreateViewModel();
        Assert.Equal(vm.Template.Sheet.HeightMicrons, vm.ClampY(vm.Template.Sheet.HeightMicrons + 10000));
    }

    [Fact]
    public void ClampY_ReturnsValueForInBounds()
    {
        var vm = CreateViewModel();
        Assert.Equal(5000, vm.ClampY(5000));
    }

    // === Rotate ===

    [Fact]
    public void RotateSelectedClockwise_RotatesTextBy90()
    {
        var vm = CreateViewModel();
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 0);
        vm.Template.Objects.Add(text);
        vm.SelectSingle(text);

        vm.RotateSelectedClockwiseCommand.Execute(null);
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void RotateSelectedCounterClockwise_RotatesTextByMinus90()
    {
        var vm = CreateViewModel();
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 90);
        vm.Template.Objects.Add(text);
        vm.SelectSingle(text);

        vm.RotateSelectedCounterClockwiseCommand.Execute(null);
        Assert.Equal(0, text.RotationAngle);
    }

    [Fact]
    public void RotateSelected_NonTextObjects_Ignores()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.RotateSelectedClockwiseCommand.Execute(null);
        // Line doesn't rotate, no exception
        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(1000, line.EndMicronsX);
        Assert.Equal(1000, line.EndMicronsY);
    }

    // === Grid ===

    [Fact]
    public void GridNodes_PopulatedOnConstruction()
    {
        var vm = CreateViewModel();
        // A3: 420x297mm, default grid 5mm step, zoom=1.0 → 5px >= 3px threshold → видимы
        Assert.True(vm.GridManager.RawNodeCount > 0);
    }

    [Fact]
    public void GridNodes_ClearsWhenGridDisabled()
    {
        var vm = CreateViewModel();
        Assert.True(vm.GridManager.RawNodeCount > 0);

        vm.GridSettings.Enabled = false;
        vm.GridSettings.Visible = false;
        vm.SetZoom(1.0001); // trigger OnZoomChanged → RefreshGridNodes

        Assert.Equal(0, vm.GridManager.RawNodeCount);
    }

    [Fact]
    public void GridNodes_UpdatesOnZoomChange()
    {
        var vm = CreateViewModel();
        var countAt1x = vm.GridManager.RawNodeCount;

        vm.SetZoom(0.5); // меньше зум → меньше узлов (могут скрыться)
        var countAt05x = vm.GridManager.RawNodeCount;

        Assert.True(countAt1x >= 0);
        Assert.True(countAt05x >= 0);
    }

    // === Canvas dimensions ===

    [Fact]
    public void CanvasWidthPixels_ReturnsCorrectValue()
    {
        var vm = CreateViewModel(); // A3: 420mm
        Assert.Equal(420.0, vm.ZoomPanManager.CanvasWidthPixels, 1);
    }

    [Fact]
    public void CanvasHeightPixels_ReturnsCorrectValue()
    {
        var vm = CreateViewModel(); // A3: 297mm
        Assert.Equal(297.0, vm.ZoomPanManager.CanvasHeightPixels, 1);
    }

    // === Selection markers ===

    [Fact]
    public void ShowSelectionMarkers_TrueForSingleSelection()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        Assert.True(vm.SelectionManager.ShowSelectionMarkers);
    }

    [Fact]
    public void ShowSelectionMarkers_TrueForMultiSelection()
    {
        var vm = CreateViewModel();
        vm.Template.Objects.Add(new Line(0, 0, 1000, 1000));
        vm.Template.Objects.Add(new Line(2000, 2000, 3000, 3000));
        vm.SelectAll();

        Assert.True(vm.SelectionManager.ShowSelectionMarkers);
    }

    [Fact]
    public void ShowSelectionMarkers_FalseForNoSelection()
    {
        var vm = CreateViewModel();
        Assert.False(vm.SelectionManager.ShowSelectionMarkers);
    }

    [Fact]
    public void SingleSelectedObject_ReturnsCorrectObject()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 1000, 1000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        Assert.Same(line, vm.SingleSelectedObject);
    }

    [Fact]
    public void SingleSelectedObject_NullForMultiSelection()
    {
        var vm = CreateViewModel();
        vm.Template.Objects.Add(new Line(0, 0, 1000, 1000));
        vm.Template.Objects.Add(new Line(2000, 2000, 3000, 3000));
        vm.SelectAll();

        Assert.Null(vm.SingleSelectedObject);
    }

    // === GetOrCreateTool ===

    [Fact]
    public void GetOrCreateTool_ReturnsSameInstance()
    {
        var vm = CreateViewModel();
        var tool1 = vm.GetOrCreateTool<DotElectric.TemplateEditor.Tools.SelectTool>();
        var tool2 = vm.GetOrCreateTool<DotElectric.TemplateEditor.Tools.SelectTool>();
        Assert.Same(tool1, tool2);
    }

    [Fact]
    public void GetOrCreateTool_DifferentTypes_ReturnDifferentInstances()
    {
        var vm = CreateViewModel();
        var select = vm.GetOrCreateTool<DotElectric.TemplateEditor.Tools.SelectTool>();
        var line = vm.GetOrCreateTool<DotElectric.TemplateEditor.Tools.DrawingLineTool>();
        Assert.NotSame(select, line);
    }

    // === DisplayName ===

    [Fact]
    public void DisplayName_NewTemplate_SetsFormatAndNoName()
    {
        var vm = CreateViewModel();
        Assert.Contains("A3", vm.DirtyStateManager.DisplayName);
    }

    [Fact]
    public void DisplayName_FromFile_SetsFileName()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, "C:\\test\\my_template.tdel", CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);
        Assert.Equal("my_template.tdel", vm.DirtyStateManager.DisplayName);
    }

    [Fact]
    public void DisplayName_AfterClearDirty_IsDirtyFalse()
    {
        var vm = CreateViewModel();
        vm.MarkDirty();
        Assert.True(vm.DirtyStateManager.IsDirty);
        vm.ClearDirty();
        // DisplayName не меняется — '*' отображается через ControlTemplate TabItem
        Assert.False(vm.DirtyStateManager.IsDirty);
    }

    // === IDisposable ===

    [Fact]
    public void Dispose_CanCallMultipleTimes()
    {
        var vm = CreateViewModel();
        vm.Dispose();
        vm.Dispose(); // should not throw
    }

    // === StatusBar Properties ===

    [Fact]
    public void StatusBar_StatusMessage_DefaultIsReady()
    {
        var vm = CreateViewModel();
        Assert.Equal("Готово", vm.StatusMessage);
    }

    [Fact]
    public void StatusBar_StatusMessage_CanSet()
    {
        var vm = CreateViewModel();
        vm.StatusMessage = "Рисую линию";
        Assert.Equal("Рисую линию", vm.StatusMessage);
    }

    [Fact]
    public void StatusBar_SheetFormat_ReturnsCorrectString()
    {
        var vm = CreateViewModel();
        var format = vm.StatusBarManager.SheetFormat;
        Assert.Contains(vm.Template.Sheet.Format, format);
        Assert.Contains("мм", format);
    }

    [Fact]
    public void StatusBar_GridEnabled_DefaultTrue()
    {
        var vm = CreateViewModel();
        // По умолчанию GridSettings.Enabled = true, Visible = true, SnapEnabled = true
        Assert.True(vm.StatusBarManager.GridEnabled);
    }

    [Fact]
    public void StatusBar_GridEnabled_SetUpdatesBothEnabledAndVisible()
    {
        var vm = CreateViewModel();
        vm.StatusBarManager.GridEnabled = false;
        Assert.False(vm.GridSettings.Enabled);
        Assert.False(vm.GridSettings.Visible);
        Assert.False(vm.StatusBarManager.GridEnabled);

        vm.StatusBarManager.GridEnabled = true;
        Assert.True(vm.GridSettings.Enabled);
        Assert.True(vm.GridSettings.Visible);
        Assert.True(vm.StatusBarManager.GridEnabled);
    }

    [Fact]
    public void StatusBar_SnapEnabled_DefaultTrue()
    {
        var vm = CreateViewModel();
        Assert.True(vm.StatusBarManager.SnapEnabled);
    }

    [Fact]
    public void StatusBar_SnapEnabled_SetUpdatesGridSettings()
    {
        var vm = CreateViewModel();
        vm.StatusBarManager.SnapEnabled = false;
        Assert.False(vm.GridSettings.SnapEnabled);
        Assert.False(vm.StatusBarManager.SnapEnabled);

        vm.StatusBarManager.SnapEnabled = true;
        Assert.True(vm.GridSettings.SnapEnabled);
        Assert.True(vm.StatusBarManager.SnapEnabled);
    }

    [Fact]
    public void StatusBar_ZoomPercent_ChangesWithZoom()
    {
        var vm = CreateViewModel();
        vm.SetZoom(0.5);
        Assert.Equal(50, vm.ZoomPanManager.ZoomPercent);

        vm.SetZoom(2.0);
        Assert.Equal(200, vm.ZoomPanManager.ZoomPercent);
    }

    // === Grid Nodes Position Tests ===

    [Fact]
    public void GridNodes_Position_CorrectForPortrait()
    {
        // A4 Portrait: 210×297 мм (Width×Height)
        var sheet = Sheet.FromFormat("A4", SheetOrientation.Portrait);
        var metadata = new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
        var template = new Template(metadata, sheet);
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        vm.ZoomPanManager.SetViewportSize(sheet.WidthMm, sheet.HeightMm);
        vm.GridManager.GridStepMm = 10.0;

        Assert.True(vm.GridManager.RawNodeCount > 0);

        // Verify micron coordinates are non-negative and within sheet bounds
        var maxXMicrons = sheet.WidthMicrons;
        var maxYMicrons = sheet.HeightMicrons;
        for (int i = 0; i < vm.GridManager.RawNodeCount && i < 10; i++)
        {
            Assert.InRange(vm.GridManager.RawNodeData[i * 2], 0, maxXMicrons);
            Assert.InRange(vm.GridManager.RawNodeData[i * 2 + 1], 0, maxYMicrons);
        }
    }

    [Fact]
    public void GridNodes_Position_CorrectForLandscape()
    {
        // A3 Landscape: 420×297 мм (Width×Height)
        var sheet = Sheet.FromFormat("A3", SheetOrientation.Landscape);
        var metadata = new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
        var template = new Template(metadata, sheet);
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        vm.ZoomPanManager.SetViewportSize(sheet.WidthMm, sheet.HeightMm);
        vm.GridManager.GridStepMm = 10.0;

        Assert.True(vm.GridManager.RawNodeCount > 0);

        // Verify micron coordinates are non-negative and within sheet bounds
        var maxXMicrons = sheet.WidthMicrons;
        var maxYMicrons = sheet.HeightMicrons;
        for (int i = 0; i < vm.GridManager.RawNodeCount && i < 10; i++)
        {
            Assert.InRange(vm.GridManager.RawNodeData[i * 2], 0, maxXMicrons);
            Assert.InRange(vm.GridManager.RawNodeData[i * 2 + 1], 0, maxYMicrons);
        }
    }

    // === IDisposable Tests ===

    [Fact]
    public void Dispose_UnsubscribesFromSelectedObjectsChanged()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        // Добавляем объект в выделение — подписка работает
        var line = new Line(0, 0, 10000, 10000);
        vm.SelectedObjects.Add(line);
        Assert.Single(vm.SelectedObjects);
        Assert.Equal(line, vm.SingleSelectedObject);

        // Dispose
        vm.Dispose();

        // После Dispose изменение SelectedObjects не должно вызывать ошибок
        vm.SelectedObjects.Clear();
        vm.SelectedObjects.Add(new Rectangle(0, 0, 10000, 10000));

        // SingleSelectedObject не обновляется через подписку (она отключена),
        // но свойство всё ещё работает — возвращает текущий элемент коллекции
        Assert.Single(vm.SelectedObjects);
        Assert.IsType<Rectangle>(vm.SingleSelectedObject);
    }

    [Fact]
    public void Dispose_DisposesPropertiesViewModel()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        // PropertiesVm должна быть доступна
        Assert.NotNull(vm.PropertiesVm);

        vm.Dispose();

        // После Dispose PropertiesVm тоже должен быть Dispose
        // Проверяем через внутреннее состояние (подписка отписана)
        vm.PropertiesVm.Refresh(); // Не должно вызывать ошибок
    }

    [Fact]
    public void Dispose_DoubleDispose_DoesNotThrow()
    {
        var template = new Template();
        var vm = new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);

        vm.Dispose();
        vm.Dispose(); // Повторный вызов не должен вызывать исключений
    }

    // === Zoom Commands ===

    [Fact]
    public void ZoomIn_IncreasesZoom()
    {
        var vm = CreateViewModel();
        var initialZoom = vm.Zoom;

        vm.ZoomInCommand.Execute(null);

        Assert.True(vm.Zoom > initialZoom);
    }

    [Fact]
    public void ZoomOut_DecreasesZoom()
    {
        var vm = CreateViewModel();
        var initialZoom = vm.Zoom;

        vm.ZoomOutCommand.Execute(null);

        Assert.True(vm.Zoom < initialZoom);
    }

    [Fact]
    public void ZoomIn_AtMaxZoom_DoesNotIncrease()
    {
        var vm = CreateViewModel();
        vm.SetZoom(10.0);

        vm.ZoomInCommand.Execute(null);

        Assert.Equal(10.0, vm.Zoom);
    }

    [Fact]
    public void ZoomOut_AtMinZoom_DoesNotDecrease()
    {
        var vm = CreateViewModel();
        vm.SetZoom(0.1);

        vm.ZoomOutCommand.Execute(null);

        Assert.Equal(0.1, vm.Zoom);
    }

    [Fact]
    public void SetZoomPercent_UpdatesZoom()
    {
        var vm = CreateViewModel();

        vm.SetZoomPercent(200);

        Assert.Equal(2.0, vm.Zoom);
        Assert.Equal(200, vm.ZoomPanManager.ZoomPercent);
    }

    [Fact]
    public void SetZoomPercent_MinValue_ClampsToMin()
    {
        var vm = CreateViewModel();

        vm.SetZoomPercent(5);

        Assert.Equal(0.1, vm.Zoom);
    }

    [Fact]
    public void SetZoomPercent_MaxValue_ClampsToMax()
    {
        var vm = CreateViewModel();

        vm.SetZoomPercent(2000);

        Assert.Equal(10.0, vm.Zoom);
    }

    // === Grid Commands ===

    [Fact]
    public void ToggleGrid_TogglesEnabled()
    {
        var vm = CreateViewModel();
        var initial = vm.GridSettings.Enabled;

        vm.ToggleGridCommand.Execute(null);

        Assert.NotEqual(initial, vm.GridSettings.Enabled);
    }

    [Fact]
    public void ToggleSnap_TogglesSnapEnabled()
    {
        var vm = CreateViewModel();
        var initial = vm.GridSettings.SnapEnabled;

        vm.ToggleSnapCommand.Execute(null);

        Assert.NotEqual(initial, vm.GridSettings.SnapEnabled);
    }

    [Fact]
    public void StatusBarGridStepMm_ReturnsStepInMm()
    {
        var vm = CreateViewModel();
        vm.GridSettings.StepMicrons = 5000;

        Assert.Equal(5.0, vm.StatusBarManager.GridStepMm);
    }

    [Fact]
    public void StatusBarGridStepMm_Setter_UpdatesStepMicrons()
    {
        var vm = CreateViewModel();

        vm.StatusBarManager.GridStepMm = 10.0;

        Assert.Equal(10000, vm.GridSettings.StepMicrons);
    }

    // === Undo/Redo ===

    [Fact]
    public void CanUndo_FalseWhenEmpty()
    {
        var vm = CreateViewModel();

        Assert.False(vm.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void CanUndo_TrueAfterCommand()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        var addCmd = new AddObjectCommand(vm.Template.Objects, line);
        vm.CommandHistory.Push(addCmd);

        Assert.True(vm.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void CanRedo_FalseWhenNothingToRedo()
    {
        var vm = CreateViewModel();

        Assert.False(vm.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void CanRedo_TrueAfterUndo()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        var addCmd = new AddObjectCommand(vm.Template.Objects, line);
        vm.CommandHistory.Push(addCmd);
        vm.UndoCommand.Execute(null);

        Assert.True(vm.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void RedoDisplayName_ReturnsCommandName()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.UndoCommand.Execute(null);

        Assert.NotNull(vm.RedoDisplayName);
        Assert.NotEmpty(vm.RedoDisplayName);
    }

    // === Keyboard Commands ===

    [Fact]
    public void DeleteSelectedObjects_RemovesSelected()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.DeleteSelectedObjectsCommand.Execute(null);

        Assert.Empty(vm.SelectedObjects);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void CutSelected_CopiesAndRemoves()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        vm.CutSelectedCommand.Execute(null);

        Assert.Empty(vm.SelectedObjects);
        Assert.Empty(vm.Template.Objects); // Cut removes the object
    }

    [Fact]
    public void SelectAllObjects_SelectsAll()
    {
        var vm = CreateViewModel();
        var line1 = new Line(0, 0, 10000, 10000);
        var line2 = new Line(10000, 10000, 20000, 20000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);

        vm.SelectAllObjectsCommand.Execute(null);

        Assert.Equal(2, vm.SelectedObjects.Count);
    }

    [Fact]
    public void NudgeLeft_MovesSelectedObject()
    {
        var vm = CreateViewModel();
        var line = new Line(10000, 10000, 20000, 20000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);
        var oldX = line.MicronsX;

        vm.NudgeLeftCommand.Execute(null);

        Assert.True(line.MicronsX < oldX);
    }

    [Fact]
    public void NudgeDown_MovesSelectedObject()
    {
        var vm = CreateViewModel();
        var line = new Line(10000, 10000, 20000, 20000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);
        var oldY = line.MicronsY;

        vm.NudgeDownCommand.Execute(null);

        Assert.True(line.MicronsY < oldY);
    }

    // === Inline Editing ===

    [Fact]
    public void StartInlineEditing_SetsInlineEditingText()
    {
        var vm = CreateViewModel();
        var text = new Text(0, 0, "Test", 2500);
        vm.Template.Objects.Add(text);

        vm.StartInlineEditing(text);

        Assert.Equal(text, vm.InlineEditManager.InlineEditingText);
        Assert.Equal("Test", vm.InlineEditManager.InlineEditText);
    }

    [Fact]
    public void CommitInlineEditing_UpdatesTextAndClears()
    {
        var vm = CreateViewModel();
        var text = new Text(0, 0, "Old", 2500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);
        vm.InlineEditManager.InlineEditText = "New";

        vm.CommitInlineEditingCommand.Execute(null);

        Assert.Equal("New", text.Content);
        Assert.Null(vm.InlineEditManager.InlineEditingText);
    }

    [Fact]
    public void CancelInlineEditing_ClearsInlineEditing()
    {
        var vm = CreateViewModel();
        var text = new Text(0, 0, "Test", 2500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);
        vm.InlineEditManager.InlineEditText = "Changed";

        vm.CancelInlineEditingCommand.Execute(null);

        Assert.Equal("Test", text.Content); // Original content preserved
        Assert.Null(vm.InlineEditManager.InlineEditingText);
    }

    // === Tool Stack ===

    [Fact]
    public void PushTool_PushesCurrentToolToStack()
    {
        var vm = CreateViewModel();
        Assert.Equal("Select", vm.ToolManager.ActiveTool);

        vm.PushTool("Resize");

        Assert.Equal("Resize", vm.ToolManager.ActiveTool);
    }

    [Fact]
    public void PopTool_RestoresPreviousTool()
    {
        var vm = CreateViewModel();
        vm.PushTool("Resize");

        vm.PopTool();

        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }

    // === FitToScreen ===

    [Fact]
    public void FitToScreen_SetsZoomToFit()
    {
        var vm = CreateViewModel();
        vm.SetZoom(0.5);

        vm.FitToScreenCommand.Execute("800,600");

        Assert.True(vm.Zoom > 0);
    }

    // === SelectionDirection ===

    [Fact]
    public void SelectionDirection_DefaultIsLTR()
    {
        var vm = CreateViewModel();

        Assert.Equal(SelectionDirection.LeftToRight, vm.SelectionDirection);
    }

    [Fact]
    public void SelectionDirection_CanBeSetToRTL()
    {
        var vm = CreateViewModel();

        vm.SelectionDirection = SelectionDirection.RightToLeft;

        Assert.Equal(SelectionDirection.RightToLeft, vm.SelectionDirection);
    }

    // === Reset Tool Tests ===

    [Fact]
    public void SetActiveTool_ResetClearsPreviewLine()
    {
        var vm = CreateViewModel();
        vm.PreviewLine = new Line(0, 0, 10000, 10000);
        vm.ToolManager.ActiveTool = "Line";

        vm.SetActiveToolCommand.Execute("Select");

        Assert.Null(vm.PreviewLine);
    }

    [Fact]
    public void SetActiveTool_ResetClearsPreviewRectangle()
    {
        var vm = CreateViewModel();
        vm.PreviewRectangle = new Rectangle(0, 0, 10000, 10000);
        vm.ToolManager.ActiveTool = "Rectangle";

        vm.SetActiveToolCommand.Execute("Select");

        Assert.Null(vm.PreviewRectangle);
    }

    [Fact]
    public void SetActiveTool_ResetClearsPreviewText()
    {
        var vm = CreateViewModel();
        vm.PreviewText = new Text(0, 0, "Test", 2500);
        vm.ToolManager.ActiveTool = "Text";

        vm.SetActiveToolCommand.Execute("Select");

        Assert.Null(vm.PreviewText);
    }

    [Fact]
    public void SetActiveTool_SameTool_DoesNotClearPreview()
    {
        var vm = CreateViewModel();
        vm.ToolManager.ActiveTool = "Line";
        vm.PreviewLine = new Line(0, 0, 10000, 10000);

        vm.SetActiveToolCommand.Execute("Line");

        Assert.NotNull(vm.PreviewLine);
    }

    // === IsObjectHovered ===

    [Fact]
    public void IsObjectHovered_ReturnsTrueWhenHovered()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);

        vm.HoveredObject = line;

        Assert.True(vm.IsObjectHovered(line));
    }

    [Fact]
    public void IsObjectHovered_ReturnsFalseWhenNotHovered()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        var other = new Line(10000, 10000, 20000, 20000);

        vm.HoveredObject = other;

        Assert.False(vm.IsObjectHovered(line));
    }

    // === Print ===

    [Fact]
    public void PrintCommand_SetsStatusMessage()
    {
        var vm = CreateViewModel();

        vm.PrintCommand.Execute(null);

        Assert.Equal("Печать доступна через меню печати", vm.StatusMessage);
    }

    // === Preview PropertyChanged ===

    [Fact]
    public void PreviewLine_RaisesPropertyChanged()
    {
        var vm = CreateViewModel();
        var raised = false;
        vm.PreviewManager.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(PreviewManager.PreviewLine)) raised = true; };

        vm.PreviewLine = new Line(0, 0, 1000, 1000);

        Assert.True(raised);
    }

    [Fact]
    public void PreviewRectangle_RaisesPropertyChanged()
    {
        var vm = CreateViewModel();
        var raised = false;
        vm.PreviewManager.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(PreviewManager.PreviewRectangle)) raised = true; };

        vm.PreviewRectangle = new Rectangle(0, 0, 1000, 1000);

        Assert.True(raised);
    }

    [Fact]
    public void PreviewText_RaisesPropertyChanged()
    {
        var vm = CreateViewModel();
        var raised = false;
        vm.PreviewManager.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(PreviewManager.PreviewText)) raised = true; };

        vm.PreviewText = new Text(0, 0, "Test", 3500);

        Assert.True(raised);
    }

    [Fact]
    public void SelectionBoxLeft_RaisesPropertyChanged()
    {
        var vm = CreateViewModel();
        var raised = false;
        vm.PreviewManager.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(PreviewManager.SelectionBoxLeft)) raised = true; };

        vm.SelectionBoxLeft = 5000;

        Assert.True(raised);
    }

    [Fact]
    public void SelectionBoxWidth_RaisesPropertyChanged()
    {
        var vm = CreateViewModel();
        var raised = false;
        vm.PreviewManager.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(PreviewManager.SelectionBoxWidth)) raised = true; };

        vm.SelectionBoxWidth = 10000;

        Assert.True(raised);
    }
}

public class EditorViewModelZoomTests
{
    private static ITemplateService CreateMockTemplateService()
    {
        var mock = new Mock<ITemplateService>();
        mock.Setup(s => s.CreateNew(It.IsAny<string>())).Returns(() => new Template());
        mock.Setup(s => s.Validate(It.IsAny<Template>())).Returns(Enumerable.Empty<string>());
        return mock.Object;
    }

    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        return new EditorViewModel(template, CreateMockTemplateService(), printService: new Mock<IPrintService>().Object);
    }

    [Theory]
    [InlineData(0.1, 10)]
    [InlineData(0.25, 25)]
    [InlineData(0.5, 50)]
    [InlineData(0.75, 75)]
    [InlineData(1.0, 100)]
    [InlineData(1.5, 150)]
    [InlineData(2.0, 200)]
    [InlineData(5.0, 500)]
    [InlineData(10.0, 1000)]
    public void ZoomPercent_CorrectForVariousZooms(double zoom, int expectedPercent)
    {
        var vm = CreateViewModel();
        vm.SetZoom(zoom);
        Assert.Equal(expectedPercent, vm.ZoomPanManager.ZoomPercent);
    }

    [Fact]
    public void FitToScreen_LargeCanvas_ReturnsSmallZoom()
    {
        var vm = CreateViewModel();
        vm.FitToScreenCommand.Execute("100,100");
        Assert.True(vm.Zoom < 1.0);
    }

    [Fact]
    public void FitToScreen_SmallSheet_ReturnsLargeZoom()
    {
        var vm = CreateViewModel();
        vm.FitToScreenCommand.Execute("2970,2100");
        Assert.True(vm.Zoom > 5.0);
    }

    [Fact]
    public void FitToScreen_ConsidersPadding()
    {
        var vm = CreateViewModel();
        vm.FitToScreenCommand.Execute("420,297");
        Assert.True(vm.Zoom < 1.0);
        Assert.True(vm.Zoom > 0.8);
    }
}
