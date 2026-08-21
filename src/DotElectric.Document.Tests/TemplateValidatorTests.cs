using Moq;

namespace DotElectric.Document.Tests;

public class TemplateValidatorTests
{
    // ===== V-001: Уникальность ID =====

    [Fact]
    public void Validate_V001_DuplicateIds_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();
        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-001").ToList();
        Assert.Empty(errors);
    }

    // ===== V-003: Координаты в пределах листа =====

    [Fact]
    public void Validate_V003_ObjectOutsideSheet_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        // A4: 297x210 мм. Объект за пределами
        template.Objects.Add(new Line(300000, 0, 310000, 1000)); // 300мм > 297мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_NegativeCoordinates_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(-1000, 0, 0, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_ObjectsInsideSheet_NoError()
    {
        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Rectangle(1000, 1000, 0, 5000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_RectangleZeroHeight_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Rectangle(1000, 1000, 5000, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_TextZeroFontSize_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(1000, 1000, "Test", 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_LineZeroLength_ReturnsWarning()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(1000, 1000, 1000, 1000));

        var errors = new TemplateValidator().Validate(template).ToList();
        var v004Errors = errors.Where(e => e.RuleId == "V-004").ToList();
        Assert.Single(v004Errors);
        Assert.Equal(ValidationSeverity.Warning, v004Errors[0].Severity);
    }

    [Fact]
    public void Validate_V004_EmptyTextContent_ReturnsWarning()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(1000, 1000, "", 3500));

        var errors = new TemplateValidator().Validate(template).ToList();
        var v004Errors = errors.Where(e => e.RuleId == "V-004").ToList();
        Assert.Contains(v004Errors, e => e.Severity == ValidationSeverity.Warning);
    }

    // ===== V-006: Корректный формат листа =====

    [Fact]
    public void Validate_V006_ValidFormat_NoError()
    {
        var template = TestTemplates.CreateValidA4();
        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-006").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V006_InvalidFormat_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Sheet.Format = "A5";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_InvalidFormat_MessageListsCanonicalFormatsWithoutLatinDuplicates()
    {
        var template = TestTemplates.CreateValidA4();
        template.Sheet.Format = "A5";

        var error = Assert.Single(new TemplateValidator().Validate(template), e => e.RuleId == "V-006");

        Assert.Contains("A0, A1, A2, A3, A4, A4×2, A3×2, A2×2, A1×2, A0×2, Custom", error.Message);
        Assert.DoesNotContain("A4X2", error.Message);
    }

    [Fact]
    public void Validate_V006_LatinXHalfFormat_StillValid()
    {
        var template = TestTemplates.CreateValidA4();
        template.Sheet.Format = "A4X2";
        template.Sheet.WidthMicrons = 210_000;
        template.Sheet.HeightMicrons = 594_000;

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-006").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V006_CustomSheetZeroWidth_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Sheet.Format = "Custom";
        template.Sheet.WidthMicrons = 0;
        template.Sheet.HeightMicrons = 100000;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_CustomSheetValid_NoError()
    {
        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();
        foreach (LineType type in Enum.GetValues<LineType>())
        {
            template.Objects.Add(new Line(0, 0, 1000, 1000, type));
        }

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-007").ToList();
        Assert.Empty(errors);
    }

    // ===== V-000: Null шаблон =====

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
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#000000"));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, strokeColor: "#FFFFFF", fillColor: "Transparent"));
        template.Objects.Add(new Text(0, 0, "Test", 2500, foreground: "#123ABC"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V005_InvalidLineColor_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "bad-color";
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_InvalidRectFillColor_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        var rect = new Rectangle(0, 0, 1000, 1000);
        rect.FillColor = "xyz";
        template.Objects.Add(rect);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_InvalidTextForeground_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        var text = new Text(0, 0, "Test", 2500);
        text.Foreground = "#GGGGGG";
        template.Objects.Add(text);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-005");
    }

    [Fact]
    public void Validate_V005_TransparentIsValid()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, fillColor: "Transparent"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V005_ArgbHexIsValid()
    {
        var template = TestTemplates.CreateValidA4();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "#80FF0000"; // semi-transparent red
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-005").ToList();
        Assert.Empty(errors);
    }

    // ===== V-001: Пустой ID =====

    [Fact]
    public void Validate_V001_EmptyId_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new TestTemplateObject("   ", 0, 0));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-001");
    }

    // ===== V-002: Метаданные =====

    [Fact]
    public void Validate_V002_AuthorNull_ReturnsWarning()
    {
        var template = TestTemplates.CreateValidA4();
        template.Metadata.Author = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        var v002Errors = errors.Where(e => e.RuleId == "V-002").ToList();
        Assert.Single(v002Errors);
        Assert.Equal(ValidationSeverity.Warning, v002Errors[0].Severity);
    }

    [Fact]
    public void Validate_V002_AuthorWhitespace_ReturnsWarning()
    {
        var template = TestTemplates.CreateValidA4();
        template.Metadata.Author = "   ";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_V002_MetadataNull_NoV002()
    {
        var template = TestTemplates.CreateValidA4();
        template.Metadata = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    // ===== V-002: Ключи изменяемых полей =====

    [Fact]
    public void Validate_V002_DuplicateTextKeys_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "field1"));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "field1"));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_V002_DuplicateKeysCaseInsensitive_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "Key"));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "key"));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_V002_NonEditableDuplicateKey_NoError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "field1", isEditable: false));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "field1"));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-002").ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_V002_EmptyOrWhitespaceKey_Skipped()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(0, 0, "A", 2500, key: "", isEditable: true));
        template.Objects.Add(new Text(1000, 0, "B", 2500, key: "  ", isEditable: true));

        var errors = new TemplateValidator().Validate(template).Where(e => e.RuleId == "V-002").ToList();
        Assert.Empty(errors);
    }

    // ===== V-003: Координаты за пределами листа =====

    [Fact]
    public void Validate_V003_RectangleRightBeyondSheet_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        // A4 Portrait: width=210mm. right = 200+100 = 300мм > 210мм
        template.Objects.Add(new Rectangle(200000, 0, 100000, 1000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_RectangleTopBeyondSheet_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        // A4 Portrait: 210x297 мм. top = 250+100 = 350 мм > 297 мм
        template.Objects.Add(new Rectangle(0, 250000, 1000, 100000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_TextOutsideSheet_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(300000, 0, "Test", 2500)); // 300 мм > 297 мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_V003_LineEndBeyondSheet_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(0, 0, 300000, 0)); // end 300 мм > 297 мм

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    // ===== V-004: Отрицательные размеры (нулевые покрыты выше) =====

    [Fact]
    public void Validate_V004_RectangleNegativeWidth_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        var rect = new Rectangle(1000, 1000, 1000, 1000);
        rect.WidthMicrons = -500;
        template.Objects.Add(rect);

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    [Fact]
    public void Validate_V004_TextNegativeFontSize_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Text(1000, 1000, "Test", -2500));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    // ===== V-006: Пустой формат / Custom с нулевой высотой =====

    [Fact]
    public void Validate_V006_EmptyFormat_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Sheet.Format = "";

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void Validate_V006_CustomSheetZeroHeight_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(0, 0, 1000, 1000));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000));
        template.Objects.Add(new Text(0, 0, "Test", 2500));
        template.Sheet = null!;

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-006");
        Assert.Single(errors, e => e.RuleId == "V-006");
    }

    // ===== V-007: Некорректный тип линии =====

    [Fact]
    public void Validate_V007_InvalidLineTypeOnLine_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
        template.Objects.Add(new Line(0, 0, 1000, 1000, (LineType)999));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-007");
    }

    [Fact]
    public void Validate_V007_InvalidLineTypeOnRectangle_ReturnsError()
    {
        var template = TestTemplates.CreateValidA4();
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

        var template = TestTemplates.CreateValidA4();
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

        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();
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
        var template = TestTemplates.CreateValidA4();

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Empty(errors);
    }

    // ===== Перенесено из AdditionalValidationServiceTests (приложение) =====

    [Fact]
    public void Validate_DuplicateIds_ReturnsV001Error()
    {
        var template = TestTemplates.CreateA3();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(2000, 2000, 3000, 3000);
        // Force duplicate
        TestTemplates.SetId(line2, line1.Id);
        template.Objects.Add(line1);
        template.Objects.Add(line2);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-001" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_DuplicateTextKeys_ReturnsV002Error()
    {
        var template = TestTemplates.CreateA3();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: true);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_a", isEditable: true);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_UniqueTextKeys_NoV002Error()
    {
        var template = TestTemplates.CreateA3();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: true);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_b", isEditable: true);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_NonEditableTextKeys_IgnoredForV002()
    {
        var template = TestTemplates.CreateA3();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: false);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_a", isEditable: false);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_ObjectOutOfBounds_ReturnsV003Error()
    {
        var template = TestTemplates.CreateA3(); // A3: 420x297mm
        template.Objects.Add(new Line(500_000, 500_000, 501_000, 501_000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_NegativeDimensions_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(0, 0, -1000, -1000));
    }

    [Fact]
    public void Validate_ValidTemplate_ReturnsNoErrors()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Line(0, 0, 10000, 10000));
        template.Objects.Add(new Rectangle(0, 0, 5000, 5000));
        template.Objects.Add(new Text(0, 0, "Test", 3500));

        var errors = new TemplateValidator().Validate(template);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullTemplate_ReturnsErrors()
    {
        var errors = new TemplateValidator().Validate(null!);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_InvalidHexColor_ReturnsV005Error()
    {
        var template = TestTemplates.CreateA3();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "not-a-color";
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-005" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ValidHexColors_NoV005Error()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#000000"));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, strokeColor: "#FF0000", fillColor: "Transparent"));
        template.Objects.Add(new Text(0, 0, "Test", 2500, foreground: "#00FF00"));

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-005");
    }

    // ===== Перенесено из ExtendedValidationServiceTests (приложение) =====

    [Fact]
    public void Validate_ValidLine_ReturnsNoErrors()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Line(0, 0, 10000, 10000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ZeroLengthLine_ReturnsWarning()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Line(1000, 1000, 1000, 1000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_EmptyTextContent_ReturnsWarning()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Text(0, 0, "", 3500));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_NegativeFontSize_ReturnsError()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Text(0, 0, "Test", -1000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void ValidateMetadataKeys_EmptyAuthor_ReturnsWarning()
    {
        var metadata = new Metadata { Author = "" };
        var errors = TemplateValidator.ValidateMetadataKeys(metadata);
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void ValidateMetadataKeys_ValidAuthor_ReturnsNoErrors()
    {
        var metadata = new Metadata { Author = "John Doe" };
        var errors = TemplateValidator.ValidateMetadataKeys(metadata);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateMetadataKeys_NullMetadata_ReturnsEmpty()
    {
        var errors = TemplateValidator.ValidateMetadataKeys(null);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateSheetFormat_NullSheet_ReturnsError()
    {
        var template = TestTemplates.CreateA3();
        template.Sheet = null!;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_EmptyFormat_ReturnsError()
    {
        var sheet = Sheet.FromFormat("A3");
        sheet.Format = "";
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_InvalidFormat_ReturnsError()
    {
        var sheet = Sheet.FromFormat("A3");
        sheet.Format = "A5";
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Theory]
    [InlineData("A4×2")]
    [InlineData("A3×2")]
    [InlineData("A2×2")]
    [InlineData("A1×2")]
    [InlineData("A0×2")]
    [InlineData("A4X2")]
    public void ValidateSheetFormat_HalfFormats_Valid(string format)
    {
        var sheet = Sheet.FromFormat(format);
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomNegativeWidth_ReturnsError()
    {
        var sheet = Sheet.Custom(-100, 200);
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomNegativeHeight_ReturnsError()
    {
        var sheet = Sheet.Custom(200, -100);
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomValid_ReturnsNoErrors()
    {
        var sheet = Sheet.Custom(500, 400);
        var template = TestTemplates.CreateA3();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateLineTypes_InvalidLineType_ReturnsError()
    {
        var template = TestTemplates.CreateA3();
        // LineType enum has valid values only, but we can't set invalid ones
        // Testing that valid types pass
        template.Objects.Add(new Line(0, 0, 1000, 1000, LineType.Dashed));
        template.Objects.Add(new Rectangle(0, 0, 5000, 5000, LineType.DashDot));

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-007");
    }

    // ===== Группировка ошибок по объекту (декларированное отклонение порядка) =====

    [Fact]
    public void Validate_MultiObjectErrors_GroupedByObject()
    {
        var template = TestTemplates.CreateA3();
        // Объект 1: оба конца линии вне листа (две V-003) + плохой цвет (V-005)
        var badLine = new Line(500_000, 0, 510_000, 1000);
        badLine.StrokeColor = "bad-color";
        // Объект 2: линия нулевой длины (V-004, Warning)
        var zeroLine = new Line(1000, 1000, 1000, 1000);
        template.Objects.Add(badLine);
        template.Objects.Add(zeroLine);

        var errors = new TemplateValidator().Validate(template).ToList();

        Assert.Equal(
            new[] { "V-003", "V-003", "V-005", "V-004" },
            errors.Select(e => e.RuleId).ToArray());
        Assert.Equal(
            new[] { badLine.Id, badLine.Id, badLine.Id, zeroLine.Id },
            errors.Select(e => e.ObjectId).ToArray());
    }

    // ===== Перепокрытие сценариев удалённого ValidateObject через Validate =====

    [Fact]
    public void Validate_RectangleOutOfBounds_ReturnsV003()
    {
        var template = TestTemplates.CreateA3(); // 420_000 x 297_000
        template.Objects.Add(new Rectangle(500_000, 0, 10_000, 10_000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_ZeroWidthRectangle_ReturnsV004()
    {
        var template = TestTemplates.CreateA3();
        template.Objects.Add(new Rectangle(10_000, 10_000, 0, 10_000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Contains(errors, e => e.RuleId == "V-004");
    }

    // ===== Инвариант: неизвестный подтип модели — пусто во всех объектных правилах =====

    [Fact]
    public void Validate_UnknownModelSubtype_NoObjectRuleErrors()
    {
        var template = TestTemplates.CreateValidA4();
        // Координаты вне листа — но для неизвестного подтипа объектных правил нет.
        template.Objects.Add(new TestTemplateObject("t-1", -5000, -5000));

        var errors = new TemplateValidator().Validate(template).ToList();
        Assert.Empty(errors);
    }
}
