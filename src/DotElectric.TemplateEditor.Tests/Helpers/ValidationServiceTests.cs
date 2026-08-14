using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class ValidationServiceTests
{
    // ===== V-001: Уникальность ID =====

    [Fact]
    public void Validate_V001_DuplicateIds_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Clear();

        var obj1 = new TestTemplateObject("dup-id", 0, 0);
        var obj2 = new TestTemplateObject("dup-id", 5000, 5000);
        template.Objects.Add(obj1);
        template.Objects.Add(obj2);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-001");
    }

    [Fact]
    public void Validate_V001_UniqueIds_NoError()
    {
        var template = CreateValidTemplate();
        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-001").ToList();
        Assert.Empty(errors);
    }

    // ===== V-003: Координаты в пределах листа =====

    [Fact]
    public void Validate_V003_ObjectOutsideSheet_ReturnsError()
    {
        var template = CreateValidTemplate();
        // A4: 297x210 мм. Объект за пределами
        template.Objects.Add(new Line(300000, 0, 310000, 1000)); // 300мм > 297мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_NegativeCoordinates_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(-1000, 0, 0, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_ObjectsInsideSheet_NoError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(1000, 1000, 5000, 5000));
        template.Objects.Add(new Rectangle(1000, 1000, 10000, 5000));
        template.Objects.Add(new Text(1000, 1000, "Test", 3500));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-003").ToList();
        Assert.Empty(errors);
    }

    // ===== V-004: Положительные размеры =====

    [Fact]
    public void Validate_V004_RectangleZeroWidth_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Rectangle(1000, 1000, 0, 5000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_RectangleZeroHeight_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Rectangle(1000, 1000, 5000, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_TextZeroFontSize_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(1000, 1000, "Test", 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_LineZeroLength_ReturnsWarning()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(1000, 1000, 1000, 1000));

        var errors = new TemplateValidator().Validate(template).ToList();
        var v004Errors = errors.Where(e => e.RuleId == "V-004").ToList();
        Assert.Single(v004Errors);
        Assert.Equal(ValidationSeverity.Warning, v004Errors[0].Severity);
    }

    [Fact]
    public void Validate_V004_EmptyTextContent_ReturnsWarning()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(1000, 1000, "", 3500));

        var errors = new TemplateValidator().Validate(template).ToList();
        var v004Errors = errors.Where(e => e.RuleId == "V-004").ToList();
        Assert.Contains(v004Errors, e => e.Severity == ValidationSeverity.Warning);
    }

    // ===== V-006: Корректный формат листа =====

    [Fact]
    public void Validate_V006_ValidFormat_NoError()
    {
        var template = CreateValidTemplate();
        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-006").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V006_InvalidFormat_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Sheet.Format = "A5";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_CustomSheetZeroWidth_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Sheet.Format = "Custom";
        template.Sheet.WidthMicrons = 0;
        template.Sheet.HeightMicrons = 100000;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_CustomSheetValid_NoError()
    {
        var template = CreateValidTemplate();
        template.Sheet.Format = "Custom";
        template.Sheet.WidthMicrons = 500000;
        template.Sheet.HeightMicrons = 350000;

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-006").ToList();
        Assert.Empty(errors);
    }

    // ===== V-007: Тип линии =====

    [Fact]
    public void Validate_V007_ValidLineType_NoError()
    {
        var template = CreateValidTemplate();
        foreach (LineType type in Enum.GetValues<LineType>())
        {
            template.Objects.Add(new Line(0, 0, 1000, 1000, type));
        }

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-007").ToList();
        Assert.Empty(errors);
    }

    // ===== Utility Methods =====

    [Fact]
    public void Validate_NullTemplate_ReturnsError()
    {
        var errors = new TemplateValidator().Validate(null!).ToList();
        Assert.Single(errors);
        Assert.Equal("V-000", errors[0].RuleId);
    }

    // ===== V-005: HEX-формат цвета =====

    [Fact]
    public void Validate_V005_ValidHexColors_NoError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#000000"));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, strokeColor: "#FFFFFF", fillColor: "Transparent"));
        template.Objects.Add(new Text(0, 0, "Test", 2500, foreground: "#123ABC"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V005_InvalidLineColor_ReturnsError()
    {
        var template = CreateValidTemplate();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "bad-color";
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_InvalidRectFillColor_ReturnsError()
    {
        var template = CreateValidTemplate();
        var rect = new Rectangle(0, 0, 1000, 1000);
        rect.FillColor = "xyz";
        template.Objects.Add(rect);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_InvalidTextForeground_ReturnsError()
    {
        var template = CreateValidTemplate();
        var text = new Text(0, 0, "Test", 2500);
        text.Foreground = "#GGGGGG";
        template.Objects.Add(text);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_TransparentIsValid()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, fillColor: "Transparent"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V005_ArgbHexIsValid()
    {
        var template = CreateValidTemplate();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "#80FF0000"; // semi-transparent red
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    // ===== ValidateHexColor standalone =====

    [Fact]
    public void ValidateHexColor_ValidHex_ReturnsNull()
    {
        Assert.Null(ValidationService.ValidateHexColor("#FF0000"));
        Assert.Null(ValidationService.ValidateHexColor("#000000"));
        Assert.Null(ValidationService.ValidateHexColor("#123ABC"));
        Assert.Null(ValidationService.ValidateHexColor("#AABBCCDD"));
    }

    [Fact]
    public void ValidateHexColor_Transparent_ReturnsNull()
    {
        Assert.Null(ValidationService.ValidateHexColor("Transparent"));
    }

    [Fact]
    public void ValidateHexColor_InvalidHex_ReturnsError()
    {
        Assert.NotNull(ValidationService.ValidateHexColor("not-a-color"));
        Assert.NotNull(ValidationService.ValidateHexColor("#GGG"));
        Assert.NotNull(ValidationService.ValidateHexColor("#12345"));
        Assert.NotNull(ValidationService.ValidateHexColor(""));
    }

    [Fact]
    public void ValidateHexColor_Empty_ReturnsError()
    {
        Assert.NotNull(ValidationService.ValidateHexColor(""));
        Assert.NotNull(ValidationService.ValidateHexColor(null));
    }

    // ===== V-001: Пустой ID =====

    [Fact]
    public void Validate_V001_EmptyId_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new TestTemplateObject("   ", 0, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-001");
    }

    // ===== V-002: Метаданные =====

    [Fact]
    public void Validate_V002_AuthorNull_ReturnsWarning()
    {
        var template = CreateValidTemplate();
        template.Metadata.Author = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        var v002Errors = errors.Where(e => e.RuleId == "V-002").ToList();
        Assert.Single(v002Errors);
        Assert.Equal(ValidationSeverity.Warning, v002Errors[0].Severity);
    }

    [Fact]
    public void Validate_V002_AuthorWhitespace_ReturnsWarning()
    {
        var template = CreateValidTemplate();
        template.Metadata.Author = "   ";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_V002_MetadataNull_NoV002()
    {
        var template = CreateValidTemplate();
        template.Metadata = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    // ===== V-002: Ключи изменяемых полей =====

    [Fact]
    public void Validate_V002_DuplicateTextKeys_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "field1"));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "field1"));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_V002_DuplicateKeysCaseInsensitive_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "Key"));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "key"));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_V002_NonEditableDuplicateKey_NoError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "field1", isEditable: false));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "field1"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-002").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V002_EmptyOrWhitespaceKey_Skipped()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "", isEditable: true));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "  ", isEditable: true));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-002").ToList();
        Assert.Empty(errors);
    }

    // ===== V-003: Координаты за пределами листа =====

    [Fact]
    public void Validate_V003_RectangleRightBeyondSheet_ReturnsError()
    {
        var template = CreateValidTemplate();
        // A4 Portrait: width=210mm. right = 200+100 = 300мм > 210мм
        template.Objects.Add(new Rectangle(200000, 0, 100000, 1000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_RectangleTopBeyondSheet_ReturnsError()
    {
        var template = CreateValidTemplate();
        // A4 Portrait: 210x297 мм. top = 250+100 = 350 мм > 297 мм
        template.Objects.Add(new Rectangle(0, 250000, 1000, 100000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_TextOutsideSheet_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(300000, 0, "Test", 2500)); // 300 мм > 297 мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_LineEndBeyondSheet_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(0, 0, 300000, 0)); // end 300 мм > 297 мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    // ===== V-004: Отрицательные размеры (нулевые покрыты выше) =====

    [Fact]
    public void Validate_V004_RectangleNegativeWidth_ReturnsError()
    {
        var template = CreateValidTemplate();
        var rect = new Rectangle(1000, 1000, 1000, 1000);
        rect.WidthMicrons = -500;
        template.Objects.Add(rect);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_TextNegativeFontSize_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Text(1000, 1000, "Test", -2500));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    // ===== V-006: Пустой формат / Custom с нулевой высотой =====

    [Fact]
    public void Validate_V006_EmptyFormat_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Sheet.Format = "";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_CustomSheetZeroHeight_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Sheet.Format = "Custom";
        template.Sheet.WidthMicrons = 500000;
        template.Sheet.HeightMicrons = 0;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    // ===== Regression: null Sheet (NRE fix) =====

    [Fact]
    public void Validate_SheetNullWithObjects_NoThrow_ReturnsV006()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000));
        template.Objects.Add(new Text(0, 0, "Test", 2500));
        template.Sheet = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
        Assert.Single(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateObject_NullSheet_ReturnsV006_NoThrow()
    {
        var errors = new TemplateValidator()
            .ValidateObject(new Line(0, 0, 1000, 1000), null!)
            .ToList();

        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    // ===== V-007: Некорректный тип линии =====

    [Fact]
    public void Validate_V007_InvalidLineTypeOnLine_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000, (LineType)999));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-007");
    }

    [Fact]
    public void Validate_V007_InvalidLineTypeOnRectangle_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, (LineType)999));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-007");
    }

    // ===== V-005: Мок IValidationService =====

    [Fact]
    public void Validate_V005_MockReturnsError_ReturnsV005()
    {
        var mock = new Mock<IValidationService>();
        mock.Setup(s => s.ValidateHexColor(It.IsAny<string?>())).Returns("error");
        var validator = new TemplateValidator(mock.Object);

        var template = CreateValidTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#000000"));

        var errors = validator.Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_MockReturnsNull_NoV005()
    {
        var mock = new Mock<IValidationService>();
        mock.Setup(s => s.ValidateHexColor(It.IsAny<string?>())).Returns((string?)null);
        var validator = new TemplateValidator(mock.Object);

        var template = CreateValidTemplate();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "bad-color";
        template.Objects.Add(line);

        var errors = validator.Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    // ===== E2E: Комбо и положительный контроль =====

    [Fact]
    public void Validate_MultipleErrors_AllReported()
    {
        var template = CreateValidTemplate();
        // V-001: дублирующиеся ID
        template.Objects.Add(new TestTemplateObject("combo-1", 0, 0));
        template.Objects.Add(new TestTemplateObject("combo-1", 5000, 5000));
        // V-005: плохой цвет
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "bad-color";
        template.Objects.Add(line);
        // V-003: прямоугольник за пределами листа
        template.Objects.Add(new Rectangle(200000, 0, 100000, 1000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-001");
        Assert.Contains(errors, e => e.RuleId == "V-003");
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_ValidTemplate_NoErrors()
    {
        var template = CreateValidTemplate();

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Empty(errors);
    }

    // ===== Test Helper =====

    private static Template CreateValidTemplate()
    {
        var metadata = new Metadata { Name = "Test", Author = "User" };
        var sheet = Sheet.FromFormat("A4");
        return new Template(metadata, sheet);
    }

    private class TestTemplateObject : TemplateObjectBase
    {
        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override double X => Coordinate.ToMm(MicronsX);
        public override double Y => Coordinate.ToMm(MicronsY);

        public TestTemplateObject(string id, long x, long y)
        {
            Id = id;
            MicronsX = x;
            MicronsY = y;
        }

        public override void Move(long micronsX, long micronsY) { }
        public override TemplateObjectBase Clone() => new TestTemplateObject(Id!, MicronsX, MicronsY);
        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new RectMicrons(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
        public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
    }
}
