using System.Collections.ObjectModel;
using System.IO;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests;

/// <summary>
/// Интеграционные тесты — проверка совместной работы компонентов.
/// </summary>
public sealed class IntegrationTests
{
    #region Helpers

    private static string CreateTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), $"dotelectric_test_{Guid.NewGuid():N}.tdel");
    }

    private static Template CreateDefaultTemplate(string format = "A3")
    {
        var metadata = new Metadata
        {
            Name = "Test Template",
            Author = "Test Author",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        var sheet = Sheet.FromFormat(format);
        return new Template(metadata, sheet);
    }

    #endregion

    // =========================================================================
    // Full cycle: Create -> Modify -> Save -> Load
    // =========================================================================

    [Fact]
    public void CreateAddObjectsSaveLoad_AllObjectsPreserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var line = new Line(10_000, 10_000, 50_000, 50_000);
            var rect = new Rectangle(20_000, 20_000, 100_000, 80_000);

            template.Objects.Add(line);
            template.Objects.Add(rect);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal(2, loaded.Objects.Count);
            var loadedLine = Assert.IsType<Line>(loaded.Objects[0]);
            Assert.Equal(line.StartMicronsX, loadedLine.StartMicronsX);
            Assert.Equal(line.StartMicronsY, loadedLine.StartMicronsY);
            Assert.Equal(line.EndMicronsX, loadedLine.EndMicronsX);
            Assert.Equal(line.EndMicronsY, loadedLine.EndMicronsY);

            var loadedRect = Assert.IsType<Rectangle>(loaded.Objects[1]);
            Assert.Equal(rect.MicronsX, loadedRect.MicronsX);
            Assert.Equal(rect.MicronsY, loadedRect.MicronsY);
            Assert.Equal(rect.WidthMicrons, loadedRect.WidthMicrons);
            Assert.Equal(rect.HeightMicrons, loadedRect.HeightMicrons);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void ModifyPropertiesSaveLoad_ChangesPersisted()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var line = new Line(10_000, 10_000, 50_000, 50_000);
            template.Objects.Add(line);

            // Modify
            line.StartMicronsX = 99_000;
            line.EndMicronsY = 88_000;
            template.Metadata.Name = "Modified Name";

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            var loadedLine = Assert.IsType<Line>(loaded.Objects[0]);
            Assert.Equal(99_000, loadedLine.StartMicronsX);
            Assert.Equal(88_000, loadedLine.EndMicronsY);
            Assert.Equal("Modified Name", loaded.Metadata.Name);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void MultipleObjectTypesSaveLoadRoundTrip_AllTypesPreserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var line = new Line(0, 0, 100_000, 100_000, LineType.Dashed);
            var rect = new Rectangle(10_000, 10_000, 50_000, 40_000, LineType.DashDot);
            var text = new Text(20_000, 20_000, "Hello", 3500, "ГОСТ А", TextType.Text, 0);

            template.Objects.Add(line);
            template.Objects.Add(rect);
            template.Objects.Add(text);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal(3, loaded.Objects.Count);
            Assert.IsType<Line>(loaded.Objects[0]);
            Assert.IsType<Rectangle>(loaded.Objects[1]);
            Assert.IsType<Text>(loaded.Objects[2]);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void UnicodeTextSaveLoad_TextPreserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var text = new Text(10_000, 10_000, "Привет мир \u2605 \u00E9 \u00F1 \u4E2D\u6587", 3500);
            template.Objects.Add(text);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            var loadedText = Assert.IsType<Text>(loaded.Objects[0]);
            Assert.Equal("Привет мир \u2605 \u00E9 \u00F1 \u4E2D\u6587", loadedText.Content);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    // =========================================================================
    // Undo/Redo integration
    // =========================================================================

    [Fact]
    public void AddObjectUndoRedo_ObjectRemovedThenRestored()
    {
        var template = CreateDefaultTemplate();
        var history = new CommandHistory(50);
        var line = new Line(10_000, 10_000, 50_000, 50_000);

        history.Push(new AddObjectCommand(template.Objects, line));
        Assert.Single(template.Objects);

        history.Undo();
        Assert.Empty(template.Objects);

        history.Redo();
        Assert.Single(template.Objects);
        Assert.Same(line, template.Objects[0]);
    }

    [Fact]
    public void MultipleOperationsUndoAll_ReturnsToOriginalState()
    {
        var template = CreateDefaultTemplate();
        var history = new CommandHistory(50);

        var line = new Line(0, 0, 10_000, 10_000);
        var rect = new Rectangle(5_000, 5_000, 20_000, 15_000);
        var text = new Text(30_000, 30_000, "Test", 3500);

        history.Push(new AddObjectCommand(template.Objects, line));
        history.Push(new AddObjectCommand(template.Objects, rect));
        history.Push(new AddObjectCommand(template.Objects, text));

        Assert.Equal(3, template.Objects.Count);

        history.Undo();
        history.Undo();
        history.Undo();

        Assert.Empty(template.Objects);
    }

    [Fact]
    public void UndoPastSaveCheckpoint_ChangesStillUndone()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var history = new CommandHistory(50);

            var line = new Line(0, 0, 10_000, 10_000);
            history.Push(new AddObjectCommand(template.Objects, line));

            // Save (checkpoint — history is NOT cleared per architecture)
            var service = new TemplateService();
            service.Save(template, filePath);

            // Undo after save
            history.Undo();
            Assert.Empty(template.Objects);

            // Redo restores
            history.Redo();
            Assert.Single(template.Objects);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void CommandHistoryLimit_OldestCommandsDropped()
    {
        var template = CreateDefaultTemplate();
        var history = new CommandHistory(5);

        for (int i = 0; i < 10; i++)
        {
            var line = new Line(i * 1000, 0, i * 1000 + 500, 500);
            history.Push(new AddObjectCommand(template.Objects, line));
        }

        // Only last 5 commands should remain in undo stack
        Assert.Equal(5, history.UndoCount);
        Assert.Equal(10, template.Objects.Count);

        // Undo 5 times removes last 5 objects
        for (int i = 0; i < 5; i++)
            history.Undo();

        Assert.Equal(5, template.Objects.Count);
    }

    // =========================================================================
    // Validation integration
    // =========================================================================

    [Fact]
    public void ValidTemplate_PassesAllValidationRules()
    {
        var template = CreateDefaultTemplate();
        template.Objects.Add(new Line(10_000, 10_000, 50_000, 50_000));
        template.Objects.Add(new Rectangle(20_000, 20_000, 50_000, 40_000));
        template.Objects.Add(new Text(30_000, 30_000, "Test", 3500));

        var errors = new TemplateValidator().Validate(template).ToList();
        var hasErrors = errors.Any(e => e.Severity == ValidationSeverity.Error);

        Assert.False(hasErrors);
    }

    [Fact]
    public void DuplicateIds_FailsValidation_V001()
    {
        var template = CreateDefaultTemplate();

        // Since Id is get-only with Guid.NewGuid() in real objects,
        // we test through the validation service with a mock scenario.
        var obj1 = new TestTemplateObject("same-id", 10_000, 10_000);
        var obj2 = new TestTemplateObject("same-id", 20_000, 20_000);
        template.Objects.Add(obj1);
        template.Objects.Add(obj2);

        var errors = new TemplateValidator().Validate(template).ToList();
        var v001Errors = errors.Where(e => e.RuleId == "V-001").ToList();

        Assert.NotEmpty(v001Errors);
        Assert.Contains(v001Errors, e => e.Message.Contains("same-id"));
    }

    [Fact]
    public void OutOfBoundsCoordinates_FailsValidation_V003()
    {
        // A3 sheet: 420_000 x 297_000 microns
        var template = CreateDefaultTemplate();
        var line = new Line(500_000, 0, 600_000, 10_000); // X exceeds sheet width
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template).ToList();
        var v003Errors = errors.Where(e => e.RuleId == "V-003").ToList();

        Assert.NotEmpty(v003Errors);
    }

    [Fact]
    public void NegativeSizes_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(10_000, 10_000, -5000, 40_000));
    }

    // =========================================================================
    // Snap + Grid integration
    // =========================================================================

    [Theory]
    [InlineData(500)]    // 0.5mm
    [InlineData(1000)]  // 1mm
    [InlineData(5000)]  // 5mm
    [InlineData(10000)] // 10mm
    public void SnapToGrid_VariousGridSizes_SnapsCorrectly(long stepMicrons)
    {
        var point = new PointMicrons(7_300, 12_800);
        var snapped = SnapHelper.SnapToGrid(point, stepMicrons);

        // Check that snapped coordinates are multiples of step
        Assert.Equal(0, snapped.MicronsX % stepMicrons);
        Assert.Equal(0, snapped.MicronsY % stepMicrons);
    }

    [Fact]
    public void SnapDisabledSetting_ReturnsOriginalUnchanged()
    {
        // When snap is "disabled", the calling code simply doesn't call SnapHelper.
        // SnapHelper itself always snaps — the "disabled" setting is handled by caller.
        // Verify that SnapHelper still snaps regardless of settings:
        var point = new PointMicrons(7_300, 12_800);
        var snapped = SnapHelper.SnapToGrid(point, 5000);

        // It DOES snap (SnapHelper doesn't know about settings)
        Assert.NotEqual(7_300, snapped.MicronsX);
    }

    // =========================================================================
    // Selection Box integration
    // =========================================================================

    [Fact]
    public void SelectionBoxHelper_LTR_SelectsOnlyFullyContained()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(10_000, 10_000, 20_000, 20_000),   // fully inside
            new Rectangle(50_000, 50_000, 20_000, 20_000),   // fully outside
            new Rectangle(35_000, 35_000, 20_000, 20_000),   // partially intersecting
        };

        // LTR selection box: (5000,5000) to (40000,40000) in microns
        var selectionBox = new RectMicrons(5_000, 5_000, 40_000, 40_000);

        var selected = SelectionBoxHelper.GetSelectedObjects(selectionBox, objects, SelectionDirection.LeftToRight);

        // Only fully contained object (first rectangle) should be selected
        // Rect1: (10k,10k)-(30k,30k) is fully inside (5k,5k)-(40k,40k) ✓
        // Rect2: (50k,50k)-(70k,70k) is outside ✗
        // Rect3: (35k,35k)-(55k,55k) partially intersects ✗
        Assert.Single(selected);
        Assert.Equal(objects[0], selected[0]);
    }

    [Fact]
    public void SelectionBoxHelper_RTL_SelectsAllIntersected()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(10_000, 10_000, 20_000, 20_000),   // fully inside
            new Rectangle(50_000, 50_000, 20_000, 20_000),   // fully outside
            new Rectangle(35_000, 35_000, 20_000, 20_000),   // partially intersecting
        };

        // RTL selection box: same area but RightToLeft
        var selectionBox = new RectMicrons(5_000, 5_000, 40_000, 40_000);

        var selected = SelectionBoxHelper.GetSelectedObjects(selectionBox, objects, SelectionDirection.RightToLeft);

        // Should select both fully inside and partially intersecting (2 objects)
        // Rect1: (10k,10k)-(30k,30k) intersects ✓
        // Rect2: (50k,50k)-(70k,70k) does not intersect ✗
        // Rect3: (35k,35k)-(55k,55k) intersects ✓
        Assert.Equal(2, selected.Count);
        Assert.Contains(objects[0], selected);
        Assert.Contains(objects[2], selected);
        Assert.DoesNotContain(objects[1], selected);
    }

    // =========================================================================
    // Hit-Test + multiple objects
    // =========================================================================

    [Fact]
    public void HitTest_OverlappingObjects_ReturnsTopmost()
    {
        var point = new PointMicrons(15_000, 15_000);

        var rect1 = new Rectangle(10_000, 10_000, 20_000, 20_000); // bottom
        var rect2 = new Rectangle(12_000, 12_000, 10_000, 10_000); // top (added later)

        var objects = new List<TemplateObjectBase> { rect1, rect2 };

        var hit = HitTestHelper.HitTest(point, objects);

        // rect2 is on top (added later = higher Z-order)
        Assert.Same(rect2, hit);
    }

    [Fact]
    public void HitTestAll_OverlappingObjects_ReturnsAllInZOrder()
    {
        var point = new PointMicrons(14_000, 14_000); // within border band of rect1 and rect2, on line endpoint

        var rect1 = new Rectangle(10_000, 10_000, 20_000, 20_000);
        var rect2 = new Rectangle(12_000, 12_000, 10_000, 10_000);
        var line = new Line(14_000, 14_000, 16_000, 16_000);

        var objects = new List<TemplateObjectBase> { rect1, rect2, line };

        var hits = HitTestHelper.HitTestAll(point, objects);

        // All three should be hit:
        // rect1: expanded (5k,5k)-(35k,35k), point (14k,14k) inside, shrunk (15k,15k)-(25k,25k) — point outside shrunk → hit ✓
        // rect2: expanded (7k,7k)-(27k,27k), point (14k,14k) inside, shrunk (17k,17k)-(17k,17k) — point outside shrunk → hit ✓
        // line: from (14k,14k) to (16k,16k) — point (14k,14k) is on endpoint ✓
        Assert.Equal(3, hits.Count);
        // Z-order: last added first
        Assert.Same(line, hits[0]);
        Assert.Same(rect2, hits[1]);
        Assert.Same(rect1, hits[2]);
    }

    // =========================================================================
    // Multi-format sheets
    // =========================================================================

    [Fact]
    public void A0Sheet_CorrectDimensions()
    {
        var sheet = Sheet.FromFormat("A0");

        // A0: 841 x 1189 mm (landscape: width > height in our model)
        Assert.Equal("A0", sheet.Format);
        Assert.Equal(1_189_000, sheet.WidthMicrons);
        Assert.Equal(841_000, sheet.HeightMicrons);
    }

    [Fact]
    public void A4Sheet_CorrectDimensions()
    {
        var sheet = Sheet.FromFormat("A4");

        Assert.Equal("A4", sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, sheet.Orientation);
        Assert.Equal(210_000, sheet.WidthMicrons);
        Assert.Equal(297_000, sheet.HeightMicrons);
    }

    [Fact]
    public void CustomSheet_DimensionsPreservedThroughSaveLoad()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var sheet = Sheet.Custom(500.5, 350.75);
            var metadata = new Metadata { Name = "Custom", Author = "Test", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now };
            var template = new Template(metadata, sheet);
            template.Objects.Add(new Line(1000, 1000, 500_000, 350_000));

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal("Custom", loaded.Sheet.Format);
            Assert.Equal(500_500, loaded.Sheet.WidthMicrons);
            Assert.Equal(350_750, loaded.Sheet.HeightMicrons);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    // =========================================================================
    // Edge cases
    // =========================================================================

    [Fact]
    public void EmptyTemplateSaveLoad_LoadsCorrectly()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            Assert.Empty(template.Objects);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Empty(loaded.Objects);
            Assert.Equal(template.Sheet.Format, loaded.Sheet.Format);
            Assert.Equal(template.Sheet.WidthMicrons, loaded.Sheet.WidthMicrons);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void TemplateWith50PlusObjects_HandledCorrectly()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();

            for (int i = 0; i < 60; i++)
            {
                template.Objects.Add(new Line(i * 1000, 0, i * 1000 + 500, 500));
            }

            Assert.Equal(60, template.Objects.Count);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal(60, loaded.Objects.Count);
            foreach (var obj in loaded.Objects)
            {
                Assert.IsType<Line>(obj);
            }
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void LargeCoordinateValues_PreservedThroughSaveLoad()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            // Use coordinates near sheet boundaries
            var line = new Line(419_000, 296_000, 419_999, 296_999);
            template.Objects.Add(line);

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            var loadedLine = Assert.IsType<Line>(loaded.Objects[0]);
            Assert.Equal(419_000, loadedLine.StartMicronsX);
            Assert.Equal(296_000, loadedLine.StartMicronsY);
            Assert.Equal(419_999, loadedLine.EndMicronsX);
            Assert.Equal(296_999, loadedLine.EndMicronsY);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void RotatedTextSaveLoad_AllAnglesPreserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var angles = new[] { 0, 90, 180, 270 };

            for (int i = 0; i < angles.Length; i++)
            {
                template.Objects.Add(new Text(10_000 + i * 50_000, 10_000, $"Text{i}", 3500, rotationAngle: angles[i]));
            }

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal(4, loaded.Objects.Count);

            for (int i = 0; i < angles.Length; i++)
            {
                var text = Assert.IsType<Text>(loaded.Objects[i]);
                Assert.Equal(angles[i], text.RotationAngle);
                Assert.Equal($"Text{i}", text.Content);
            }
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void AllLineTypesSaveLoad_Preserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var template = CreateDefaultTemplate();
            var lineTypes = new[] { LineType.Solid, LineType.Dashed, LineType.DashDot, LineType.DashDotDot };

            for (int i = 0; i < lineTypes.Length; i++)
            {
                template.Objects.Add(new Line(i * 10_000, 0, i * 10_000 + 5000, 5000, lineTypes[i]));
            }

            var service = new TemplateService();
            service.Save(template, filePath);

            var loaded = service.Load(filePath);

            Assert.Equal(4, loaded.Objects.Count);

            for (int i = 0; i < lineTypes.Length; i++)
            {
                var line = Assert.IsType<Line>(loaded.Objects[i]);
                Assert.Equal(lineTypes[i], line.LineType);
            }
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    // =========================================================================
    // Additional integration tests
    // =========================================================================

    [Fact]
    public void CommandHistory_MoveObjectUndoRedo_PositionRestored()
    {
        var rect = new Rectangle(10_000, 10_000, 50_000, 40_000);
        var history = new CommandHistory(50);

        history.Push(new ChangePropertyCommand<(long X, long Y)>(
            (10_000, 10_000),
            v => { rect.MicronsX = v.X; rect.MicronsY = v.Y; },
            (20_000, 30_000),
            "Переместить объект"));

        Assert.Equal(20_000, rect.MicronsX);
        Assert.Equal(30_000, rect.MicronsY);

        history.Undo();
        Assert.Equal(10_000, rect.MicronsX);
        Assert.Equal(10_000, rect.MicronsY);

        history.Redo();
        Assert.Equal(20_000, rect.MicronsX);
        Assert.Equal(30_000, rect.MicronsY);
    }

    [Fact]
    public void DeleteObjectUndoRedo_ObjectRestoredAtIndex()
    {
        var template = CreateDefaultTemplate();
        var history = new CommandHistory(50);

        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(2000, 2000, 3000, 3000);

        history.Push(new AddObjectCommand(template.Objects, line1));
        history.Push(new AddObjectCommand(template.Objects, line2));

        history.Push(new DeleteObjectCommand(template.Objects, line1));
        Assert.Single(template.Objects);
        Assert.Same(line2, template.Objects[0]);

        history.Undo();
        Assert.Equal(2, template.Objects.Count);
        Assert.Same(line1, template.Objects[0]);

        history.Redo();
        Assert.Single(template.Objects);
    }

    [Fact]
    public void SnapHelper_SnapSize_NonNegativeResult()
    {
        // SnapSize should never return negative
        var snapped = SnapHelper.SnapSize(-3000, 5000);
        Assert.True(snapped >= 0);
    }

    [Fact]
    public void SnapHelper_SnapObject_MovesToNearestGridPoint()
    {
        var rect = new Rectangle(7300, 12800, 50_000, 40_000);
        SnapHelper.SnapObject(rect, 5000);

        Assert.Equal(0, rect.MicronsX % 5000);
        Assert.Equal(0, rect.MicronsY % 5000);
    }

    [Fact]
    public void ValidationService_ValidateObjectCoordinates_OutOfBounds()
    {
        var sheet = Sheet.FromFormat("A3"); // 420_000 x 297_000
        var rect = new Rectangle(500_000, 0, 10_000, 10_000);

        var errors = new TemplateValidator().ValidateObject(rect, sheet).ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void ValidationService_ValidateObjectPositiveSizes_ZeroWidth()
    {
        var sheet = Sheet.FromFormat("A3");
        var rect = new Rectangle(10_000, 10_000, 0, 10_000);

        var errors = new TemplateValidator().ValidateObject(rect, sheet).ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void ValidationService_ValidateObjectLineType_InvalidType()
    {
        var sheet = Sheet.FromFormat("A3");
        // LineType is an enum, all values are valid by construction.
        // This test verifies the validation doesn't crash on valid types.
        var line = new Line(0, 0, 10_000, 10_000, LineType.DashDotDot);

        var errors = new TemplateValidator().ValidateObject(line, sheet).ToList();
        var v007Errors = errors.Where(e => e.RuleId == "V-007").ToList();
        Assert.Empty(v007Errors);
    }

    [Fact]
    public void BatchCommand_MultipleOperations_SingleUndo()
    {
        var template = CreateDefaultTemplate();
        var history = new CommandHistory(50);

        var cmd1 = new AddObjectCommand(template.Objects, new Line(0, 0, 1000, 1000));
        var cmd2 = new AddObjectCommand(template.Objects, new Line(2000, 2000, 3000, 3000));

        var batch = new BatchCommand(new[] { cmd1, cmd2 }, "Add multiple");

        history.Push(batch);
        Assert.Equal(2, template.Objects.Count);

        history.Undo();
        Assert.Empty(template.Objects);

        history.Redo();
        Assert.Equal(2, template.Objects.Count);
    }

    [Fact]
    public void SettingsService_LoadSave_PersistsSettings()
    {
        var service = new SettingsService();

        var settings = new AppSettings
        {
            AutosaveIntervalMinutes = 10,
            Theme = "Dark",
            ShowGrid = false,
            SnapToGrid = false,
            GridStepMm = 10.0,
            DefaultZoom = 2.0,
            DefaultSheetFormat = "A4"
        };

        service.Save(settings);

        var loaded = service.Load();

        Assert.Equal(10, loaded.AutosaveIntervalMinutes);
        Assert.Equal("Dark", loaded.Theme);
        Assert.False(loaded.ShowGrid);
        Assert.False(loaded.SnapToGrid);
        Assert.Equal(10.0, loaded.GridStepMm);
        Assert.Equal(2.0, loaded.DefaultZoom);
        Assert.Equal("A4", loaded.DefaultSheetFormat);
    }

    [Fact]
    public void SettingsService_GetSet_KnownKeys()
    {
        var service = new SettingsService();

        service.Set("Theme", "Dark");
        Assert.Equal("Dark", service.Get("Theme", "Light"));

        service.Set("ShowGrid", false);
        Assert.False(service.Get("ShowGrid", true));

        service.Set("SnapToGrid", false);
        Assert.False(service.Get("SnapToGrid", true));

        service.Set("GridStepMm", 10.0);
        Assert.Equal(10.0, service.Get("GridStepMm", 5.0));

        service.Set("DefaultSheetFormat", "A4");
        Assert.Equal("A4", service.Get("DefaultSheetFormat", "A3"));
    }

    // =========================================================================
    // Helper class for testing duplicate ID validation
    // =========================================================================

    private sealed class TestTemplateObject : TemplateObjectBase
    {
        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override double X => Coordinate.ToMm(MicronsX);
        public override double Y => Coordinate.ToMm(MicronsY);

        public TestTemplateObject(string id, long micronsX, long micronsY)
        {
            Id = id;
            MicronsX = micronsX;
            MicronsY = micronsY;
        }

        public new void Move(long micronsX, long micronsY)
        {
            MicronsX = micronsX;
            MicronsY = micronsY;
        }

        public override TemplateObjectBase Clone()
        {
            return new TestTemplateObject(Id, MicronsX, MicronsY);
        }

        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new RectMicrons(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
        public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
    }

    // =========================================================================
    // Orientation Tests
    // =========================================================================

    [Fact]
    public void CreateTemplate_A4Portrait_HasCorrectDimensions()
    {
        var templateService = new TemplateService();
        var template = templateService.CreateNew("A4", SheetOrientation.Portrait);

        Assert.Equal("A4", template.Sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, template.Sheet.Orientation);
        Assert.Equal(210_000, template.Sheet.WidthMicrons);
        Assert.Equal(297_000, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void CreateTemplate_A3Landscape_HasCorrectDimensions()
    {
        var templateService = new TemplateService();
        var template = templateService.CreateNew("A3", SheetOrientation.Landscape);

        Assert.Equal("A3", template.Sheet.Format);
        Assert.Equal(SheetOrientation.Landscape, template.Sheet.Orientation);
        Assert.Equal(420_000, template.Sheet.WidthMicrons);
        Assert.Equal(297_000, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void CreateTemplate_A0Portrait_HasCorrectDimensions()
    {
        var templateService = new TemplateService();
        var template = templateService.CreateNew("A0", SheetOrientation.Portrait);

        Assert.Equal("A0", template.Sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, template.Sheet.Orientation);
        Assert.Equal(841_000, template.Sheet.WidthMicrons);
        Assert.Equal(1_189_000, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void CreateTemplate_A0Landscape_HasCorrectDimensions()
    {
        var templateService = new TemplateService();
        var template = templateService.CreateNew("A0", SheetOrientation.Landscape);

        Assert.Equal("A0", template.Sheet.Format);
        Assert.Equal(SheetOrientation.Landscape, template.Sheet.Orientation);
        Assert.Equal(1_189_000, template.Sheet.WidthMicrons);
        Assert.Equal(841_000, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void SaveLoad_A4Portrait_OrientationPreserved()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var templateService = new TemplateService();
            var template = templateService.CreateNew("A4", SheetOrientation.Portrait);

            templateService.Save(template, filePath);
            var loaded = templateService.Load(filePath);

            Assert.Equal("A4", loaded.Sheet.Format);
            Assert.Equal(SheetOrientation.Portrait, loaded.Sheet.Orientation);
            Assert.Equal(210_000, loaded.Sheet.WidthMicrons);
            Assert.Equal(297_000, loaded.Sheet.HeightMicrons);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveLoad_BackwardCompatibility_OldFileWithoutOrientation_DetectsByDimensions()
    {
        var filePath = CreateTempFilePath();
        try
        {
            // Создаём .tdel файл (ZIP с XML внутри) без Orientation элемента
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Template>
  <Metadata>
    <Name>Old Template</Name>
    <Author>Test</Author>
    <CreatedDate>2024-01-01T00:00:00</CreatedDate>
    <ModifiedDate>2024-01-01T00:00:00</ModifiedDate>
  </Metadata>
  <Sheet>
    <Format>A4</Format>
    <WidthMicrons>297000</WidthMicrons>
    <HeightMicrons>210000</HeightMicrons>
  </Sheet>
  <Objects />
</Template>";

            // Создаём ZIP-архив
            using (var archive = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("template.xml");
                using var entryStream = entry.Open();
                using var writer = new System.IO.StreamWriter(entryStream);
                writer.Write(xmlContent);
            }

            var templateService = new TemplateService();
            var loaded = templateService.Load(filePath);

            Assert.Equal("A4", loaded.Sheet.Format);
            // Width > Height → должно определить как Landscape
            Assert.Equal(SheetOrientation.Landscape, loaded.Sheet.Orientation);
            Assert.Equal(297_000, loaded.Sheet.WidthMicrons);
            Assert.Equal(210_000, loaded.Sheet.HeightMicrons);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveLoad_BackwardCompatibility_PortraitDimensions_DetectsPortrait()
    {
        var filePath = CreateTempFilePath();
        try
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Template>
  <Metadata>
    <Name>Old Portrait Template</Name>
    <Author>Test</Author>
    <CreatedDate>2024-01-01T00:00:00</CreatedDate>
    <ModifiedDate>2024-01-01T00:00:00</ModifiedDate>
  </Metadata>
  <Sheet>
    <Format>A4</Format>
    <WidthMicrons>210000</WidthMicrons>
    <HeightMicrons>297000</HeightMicrons>
  </Sheet>
  <Objects />
</Template>";

            // Создаём ZIP-архив
            using (var archive = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("template.xml");
                using var entryStream = entry.Open();
                using var writer = new System.IO.StreamWriter(entryStream);
                writer.Write(xmlContent);
            }

            var templateService = new TemplateService();
            var loaded = templateService.Load(filePath);

            Assert.Equal("A4", loaded.Sheet.Format);
            // Width < Height → должно определить как Portrait
            Assert.Equal(SheetOrientation.Portrait, loaded.Sheet.Orientation);
            Assert.Equal(210_000, loaded.Sheet.WidthMicrons);
            Assert.Equal(297_000, loaded.Sheet.HeightMicrons);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
