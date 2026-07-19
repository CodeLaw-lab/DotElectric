using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Converters;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Converters;

// ==================== Common Converters ====================

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();

    [Theory]
    [InlineData(true, false, Visibility.Visible)]
    [InlineData(false, false, Visibility.Collapsed)]
    [InlineData(true, true, Visibility.Collapsed)]
    [InlineData(false, true, Visibility.Visible)]
    public void Convert_ReturnsExpected(bool value, bool invert, Visibility expected)
    {
        _converter.Invert = invert;
        var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Visibility.Visible, true)]
    [InlineData(Visibility.Collapsed, false)]
    public void ConvertBack_ReturnsExpected(Visibility visibility, bool expected)
    {
        var result = _converter.ConvertBack(visibility, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, (bool)result);
    }
}

public class MicronsToMmConverterTests
{
    private readonly MicronsToMmConverter _converter = new();

    [Theory]
    [InlineData(5000L, "5.000")]
    [InlineData(1000L, "1.000")]
    [InlineData(2500.0, "2.500")]
    public void Convert_ReturnsFormattedMm(object value, string expected)
    {
        var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_ReturnsEmpty()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("", result);
    }

    [Fact]
    public void ConvertBack_String_ParsesToMicrons()
    {
        var result = _converter.ConvertBack("5.500", typeof(long), null, CultureInfo.InvariantCulture);
        Assert.Equal(5500L, result);
    }
}

public class ZoomToStringConverterTests
{
    private readonly ZoomToStringConverter _converter = new();

    [Theory]
    [InlineData(1.0, "100%")]
    [InlineData(0.5, "50%")]
    [InlineData(2.0, "200%")]
    [InlineData(1.5, "150%")]
    public void Convert_FormatsPercentage(double zoom, string expected)
    {
        var result = _converter.Convert(zoom, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_Returns100Percent()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("100%", result);
    }

    [Fact]
    public void ConvertBack_ParsesPercentage()
    {
        var result = _converter.ConvertBack("150%", typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(1.5, (double)result, 10);
    }
}

public class LineTypeToStringConverterTests
{
    private readonly LineTypeToStringConverter _converter = new();

    [Theory]
    [InlineData(LineType.Solid, "Сплошная")]
    [InlineData(LineType.Dashed, "Штриховая")]
    [InlineData(LineType.DashDot, "Штрихпунктирная")]
    [InlineData(LineType.DashDotDot, "Штрихпунктирная с двумя штрихами")]
    public void Convert_ReturnsCorrectString(LineType type, string expected)
    {
        var result = _converter.Convert(type, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Сплошная", LineType.Solid)]
    [InlineData("Штриховая", LineType.Dashed)]
    [InlineData("Штрихпунктирная", LineType.DashDot)]
    [InlineData("Штрихпунктирная с двумя штрихами", LineType.DashDotDot)]
    public void ConvertBack_ReturnsCorrectLineType(string text, LineType expected)
    {
        var result = _converter.ConvertBack(text, typeof(LineType), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class TextTypeToStringConverterTests
{
    private readonly TextTypeToStringConverter _converter = new();

    [Theory]
    [InlineData(TextType.Text, "Текст")]
    [InlineData(TextType.Dimension, "Размер")]
    [InlineData(TextType.Tolerance, "Допуск")]
    [InlineData(TextType.Note, "Примечание")]
    [InlineData(TextType.Label, "Обозначение")]
    public void Convert_ReturnsCorrectString(TextType type, string expected)
    {
        var result = _converter.Convert(type, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Текст", TextType.Text)]
    [InlineData("Размер", TextType.Dimension)]
    [InlineData("Допуск", TextType.Tolerance)]
    [InlineData("Примечание", TextType.Note)]
    [InlineData("Обозначение", TextType.Label)]
    public void ConvertBack_ReturnsCorrectTextType(string text, TextType expected)
    {
        var result = _converter.ConvertBack(text, typeof(TextType), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class IsNullConverterTests
{
    private readonly IsNullConverter _converter = new();

    [Fact]
    public void Null_ReturnsTrue()
    {
        var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.True((bool)result);
    }

    [Fact]
    public void NonNull_ReturnsFalse()
    {
        var result = _converter.Convert("text", typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False((bool)result);
    }
}

public class ZeroToVisibilityConverterTests
{
    private readonly ZeroToVisibilityConverter _converter = new();

    [Theory]
    [InlineData(0, Visibility.Visible)]
    [InlineData(5, Visibility.Collapsed)]
    [InlineData(0L, Visibility.Visible)]
    public void Convert_ReturnsExpected(object value, Visibility expected)
    {
        var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class NonZeroToVisibilityConverterTests
{
    private readonly NonZeroToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_IntZero_ReturnsCollapsed()
    {
        var result = _converter.Convert(0, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_IntPositive_ReturnsVisible()
    {
        var result = _converter.Convert(1, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_IntNegative_ReturnsCollapsed()
    {
        var result = _converter.Convert(-1, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_LongZero_ReturnsCollapsed()
    {
        var result = _converter.Convert(0L, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_LongPositive_ReturnsVisible()
    {
        var result = _converter.Convert(5L, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_LongNegative_ReturnsCollapsed()
    {
        var result = _converter.Convert(-3L, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_DoubleZero_ReturnsCollapsed()
    {
        var result = _converter.Convert(0.0, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_DoublePositive_ReturnsVisible()
    {
        var result = _converter.Convert(2.5, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_DoubleNegative_ReturnsCollapsed()
    {
        var result = _converter.Convert(-1.0, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_FloatZero_ReturnsCollapsed()
    {
        var result = _converter.Convert(0.0f, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_FloatPositive_ReturnsVisible()
    {
        var result = _converter.Convert(3.0f, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_FloatNegative_ReturnsCollapsed()
    {
        var result = _converter.Convert(-2.0f, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_UnsupportedType_ReturnsCollapsed()
    {
        var result = _converter.Convert("not a number", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(Visibility.Visible, typeof(int), null, CultureInfo.InvariantCulture));
    }
}

public class EqualToBoolConverterTests
{
    private readonly EqualToBoolConverter _converter = new();

    [Theory]
    [InlineData("Line", "Line", true)]
    [InlineData("Line", "Rectangle", false)]
    [InlineData("line", "LINE", true)]
    public void Convert_ReturnsExpected(string value, string parameter, bool expected)
    {
        var result = _converter.Convert(value, typeof(bool), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, (bool)result);
    }

    [Fact]
    public void ConvertBack_Checked_ReturnsParameter()
    {
        var result = _converter.ConvertBack(true, typeof(string), "Select", CultureInfo.InvariantCulture);
        Assert.Equal("Select", result);
    }

    [Fact]
    public void ConvertBack_Unchecked_ReturnsDoNothing()
    {
        var result = _converter.ConvertBack(false, typeof(string), "Select", CultureInfo.InvariantCulture);
        Assert.Equal(System.Windows.Data.Binding.DoNothing, result);
    }
}

public class NotNullToVisibilityConverterTests
{
    private readonly NotNullToVisibilityConverter _converter = new();

    [Fact]
    public void NonNull_ReturnsVisible()
    {
        var result = _converter.Convert("text", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Null_ReturnsCollapsed()
    {
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }
}

// ==================== EditorCanvas Converters ====================

public class ModelXToCanvasLeftConverterTests
{
    private readonly ModelXToCanvasLeftConverter _converter = new();

    [Theory]
    [InlineData(1000L, 1.0, 1.0)]
    [InlineData(5000L, 2.0, 10.0)]
    [InlineData(0L, 1.0, 0.0)]
    public void Convert_ReturnsPixels(long microns, double zoom, double expected)
    {
        var values = new object[] { microns, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_Returns0()
    {
        var values = new object[] { 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_ZeroZoom_Returns0()
    {
        var values = new object[] { 1000L, 0.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class ModelYToCanvasTopConverterTests
{
    private readonly ModelYToCanvasTopConverter _converter = new();

    [Theory]
    [InlineData(0L, 297.0, 1.0, 297.0)]
    [InlineData(297000L, 297.0, 1.0, 0.0)]
    [InlineData(148500L, 297.0, 1.0, 148.5)]
    public void Convert_ReturnsExpected(long micronsY, double heightMm, double zoom, double expected)
    {
        var values = new object[] { micronsY, heightMm, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }
}

public class MicronsToPixelConverterTests
{
    private readonly MicronsToPixelConverter _converter = new();

    [Theory]
    [InlineData(1000L, 1.0, 1.0)]
    [InlineData(5000L, 2.0, 10.0)]
    public void Convert_ReturnsPixels(long microns, double zoom, double expected)
    {
        var values = new object[] { microns, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_Returns0()
    {
        var values = new object[] { 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class LineTypeToDashArrayConverterTests
{
    private readonly LineTypeToDashArrayConverter _converter = new();

    [Fact]
    public void Solid_ReturnsNull()
    {
        var result = _converter.Convert(LineType.Solid, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Fact]
    public void Dashed_ReturnsDashArray()
    {
        var result = _converter.Convert(LineType.Dashed, typeof(object), null, CultureInfo.InvariantCulture);
        var arr = Assert.IsType<System.Windows.Media.DoubleCollection>(result);
        Assert.Equal(new[] { 10.0, 5.0 }, arr.ToArray());
    }

    [Fact]
    public void DashDot_ReturnsDashArray()
    {
        var result = _converter.Convert(LineType.DashDot, typeof(object), null, CultureInfo.InvariantCulture);
        var arr = Assert.IsType<System.Windows.Media.DoubleCollection>(result);
        Assert.Equal(new[] { 10.0, 5.0, 2.0, 5.0 }, arr.ToArray());
    }

    [Fact]
    public void DashDotDot_ReturnsDashArray()
    {
        var result = _converter.Convert(LineType.DashDotDot, typeof(object), null, CultureInfo.InvariantCulture);
        var arr = Assert.IsType<System.Windows.Media.DoubleCollection>(result);
        Assert.Equal(new[] { 10.0, 5.0, 2.0, 5.0, 2.0, 5.0 }, arr.ToArray());
    }
}

public class NotConverterTests
{
    // NotConverter определён в EditorCanvasConverters.cs — тестируем его
    private readonly DotElectric.TemplateEditor.Converters.NotConverter _converter = new();

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Convert_ReturnsInverse(bool value, bool expected)
    {
        var result = _converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<bool>(result));
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False(Assert.IsType<bool>(result));
    }
}

public class HexToBrushConverterTests
{
    private readonly HexToBrushConverter _converter = new();

    [Theory]
    [InlineData("#000000", "#000000")]
    [InlineData("#FF0000", "#FF0000")]
    [InlineData("Transparent", "Transparent")]
    public void Convert_ValidHex_ReturnsMatchingBrush(string hex, string expectedHex)
    {
        var result = _converter.Convert(hex, typeof(System.Windows.Media.Brush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<System.Windows.Media.SolidColorBrush>(result);
        var expected = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(expectedHex));
        Assert.Equal(expected.Color, brush.Color);
    }

    [Fact]
    public void InvalidHex_ReturnsBlackBrush()
    {
        var result = _converter.Convert("not a color", typeof(System.Windows.Media.Brush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<System.Windows.Media.SolidColorBrush>(result);
        Assert.Equal(System.Windows.Media.Colors.Black, brush.Color);
    }

    [Fact]
    public void Null_ReturnsBlackBrush()
    {
        var result = _converter.Convert(null, typeof(System.Windows.Media.Brush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<System.Windows.Media.SolidColorBrush>(result);
        Assert.Equal(System.Windows.Media.Colors.Black, brush.Color);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(System.Windows.Media.Brushes.Black, typeof(string), null, CultureInfo.InvariantCulture));
    }
}

public class BoolToTextWrappingConverterTests
{
    private readonly BoolToTextWrappingConverter _converter = new();

    [Theory]
    [InlineData(true, System.Windows.TextWrapping.Wrap)]
    [InlineData(false, System.Windows.TextWrapping.NoWrap)]
    public void Convert_ReturnsExpected(bool value, System.Windows.TextWrapping expected)
    {
        var result = _converter.Convert(value, typeof(System.Windows.TextWrapping), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_ReturnsNoWrap()
    {
        var result = _converter.Convert(null, typeof(System.Windows.TextWrapping), null, CultureInfo.InvariantCulture);
        Assert.Equal(System.Windows.TextWrapping.NoWrap, result);
    }

    [Theory]
    [InlineData(System.Windows.TextWrapping.Wrap, true)]
    [InlineData(System.Windows.TextWrapping.NoWrap, false)]
    public void ConvertBack_ReturnsExpected(System.Windows.TextWrapping wrapping, bool expected)
    {
        var result = _converter.ConvertBack(wrapping, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<bool>(result));
    }
}

public class StringToTextAlignmentConverterTests
{
    private readonly StringToTextAlignmentConverter _converter = new();

    [Theory]
    [InlineData("Left", System.Windows.TextAlignment.Left)]
    [InlineData("Center", System.Windows.TextAlignment.Center)]
    [InlineData("Right", System.Windows.TextAlignment.Right)]
    [InlineData("Invalid", System.Windows.TextAlignment.Left)]
    public void Convert_ReturnsExpected(string value, System.Windows.TextAlignment expected)
    {
        var result = _converter.Convert(value, typeof(System.Windows.TextAlignment), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(System.Windows.TextAlignment.Left, "Left")]
    [InlineData(System.Windows.TextAlignment.Center, "Center")]
    [InlineData(System.Windows.TextAlignment.Right, "Right")]
    public void ConvertBack_ReturnsExpected(System.Windows.TextAlignment alignment, string expected)
    {
        var result = _converter.ConvertBack(alignment, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class TextAlignmentToIndexConverterTests
{
    private readonly TextAlignmentToIndexConverter _converter = new();

    [Theory]
    [InlineData("Left", 0)]
    [InlineData("Center", 1)]
    [InlineData("Right", 2)]
    [InlineData("Invalid", 0)]
    public void Convert_ReturnsExpected(string value, int expected)
    {
        var result = _converter.Convert(value, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "Left")]
    [InlineData(1, "Center")]
    [InlineData(2, "Right")]
    [InlineData(99, "Left")]
    public void ConvertBack_ReturnsExpected(int index, string expected)
    {
        var result = _converter.ConvertBack(index, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

// ==================== Extended Test Classes ====================

public class MicronsToMmConverterExtendedTests
{
    private readonly MicronsToMmConverter _converter = new();

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("-5.500", -5500L)]
    public void ConvertBack_ReturnsMicrons(string input, long expected)
    {
        var result = _converter.ConvertBack(input, typeof(long), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class ZoomToStringConverterExtendedTests
{
    private readonly ZoomToStringConverter _converter = new();

    [Fact]
    public void ConvertBack_InvalidString_ReturnsDefaultOne()
    {
        var result = _converter.ConvertBack("invalid", typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(1.0, Assert.IsType<double>(result));
    }

    [Fact]
    public void ConvertBack_ZeroPercent_ReturnsZero()
    {
        var result = _converter.ConvertBack("0%", typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class LineTypeToStringConverterExtendedTests
{
    private readonly LineTypeToStringConverter _converter = new();

    [Fact]
    public void Convert_Null_ReturnsEmpty()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("", result);
    }

    [Fact]
    public void ConvertBack_UnknownString_ReturnsSolid()
    {
        var result = _converter.ConvertBack("Неизвестный", typeof(LineType), null, CultureInfo.InvariantCulture);
        Assert.Equal(LineType.Solid, result);
    }
}

public class TextTypeToStringConverterExtendedTests
{
    private readonly TextTypeToStringConverter _converter = new();

    [Fact]
    public void Convert_Null_ReturnsEmpty()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("", result);
    }

    [Fact]
    public void ConvertBack_UnknownString_ReturnsText()
    {
        var result = _converter.ConvertBack("Неизвестный", typeof(TextType), null, CultureInfo.InvariantCulture);
        Assert.Equal(TextType.Text, result);
    }
}

public class EqualToBoolConverterExtendedTests
{
    private readonly EqualToBoolConverter _converter = new();

    [Fact]
    public void ConvertBack_False_ReturnsDoNothing()
    {
        var result = _converter.ConvertBack(false, typeof(string), "Select", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }
}

public class ZeroToVisibilityConverterExtendedTests
{
    private readonly ZeroToVisibilityConverter _converter = new();

    [Theory]
    [InlineData(-1, Visibility.Collapsed)]
    [InlineData("string", Visibility.Collapsed)]
    public void Convert_ReturnsExpected(object value, Visibility expected)
    {
        var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}

public class LineLocalConverterTests
{
    private readonly LineLocalConverter _converter = new();

    [Theory]
    [InlineData(1000L, 5000L, 1.0, "X1", 0.0)]
    [InlineData(5000L, 1000L, 1.0, "X1", 4.0)]
    [InlineData(3000L, 7000L, 2.0, "X1", 0.0)]
    [InlineData(1000L, 5000L, 1.0, "X2", 4.0)]
    [InlineData(5000L, 1000L, 1.0, "X2", 0.0)]
    [InlineData(2000L, 6000L, 1.0, "Y1", 4.0)]
    [InlineData(6000L, 2000L, 1.0, "Y1", 0.0)]
    [InlineData(2000L, 6000L, 1.0, "Y2", 0.0)]
    [InlineData(6000L, 2000L, 1.0, "Y2", 4.0)]
    public void Convert_ReturnsLocalCoord(long c1, long c2, double zoom, string param, double expected)
    {
        var values = new object[] { c1, c2, zoom };
        var result = _converter.Convert(values, typeof(double), param, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InvalidValues_ReturnsZero()
    {
        var values = new object[] { null!, 5000L, 1.0 };
        var result = _converter.Convert(values, typeof(double), "X1", CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_ZeroZoom_ReturnsZero()
    {
        var values = new object[] { 1000L, 5000L, 0.0 };
        var result = _converter.Convert(values, typeof(double), "X1", CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_UnknownParameter_ReturnsZero()
    {
        var values = new object[] { 1000L, 5000L, 1.0 };
        var result = _converter.Convert(values, typeof(double), "Z1", CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_FourBindings_IgnoresExtra()
    {
        var values = new object[] { 2000L, 6000L, 297.0, 1.0 };
        var result = _converter.Convert(values, typeof(double), "Y1", CultureInfo.InvariantCulture);
        Assert.Equal(4.0, Assert.IsType<double>(result), 10);
    }
}

public class EnumToIndexConverterTests
{
    private readonly EnumToIndexConverter _converter = new();

    [Fact]
    public void Convert_LineTypeSolid_ReturnsZero()
    {
        var result = _converter.Convert(LineType.Solid, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_LineTypeDashed_ReturnsOne()
    {
        var result = _converter.Convert(LineType.Dashed, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    [Fact]
    public void Convert_TextTypeText_ReturnsZero()
    {
        var result = _converter.Convert(TextType.Text, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_TextTypeLabel_ReturnsFour()
    {
        var result = _converter.Convert(TextType.Label, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(4, result);
    }

    [Fact]
    public void Convert_Null_ReturnsNegativeOne()
    {
        var result = _converter.Convert(null, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Convert_NonEnum_ReturnsNegativeOne()
    {
        var result = _converter.Convert(42, typeof(int), null, CultureInfo.InvariantCulture);
        Assert.Equal(-1, result);
    }
}

public class ModelXToCanvasLeftConverterExtendedTests
{
    private readonly ModelXToCanvasLeftConverter _converter = new();

    [Theory]
    [InlineData(841000L, 1.0, 841.0)]
    [InlineData(841000L, 2.0, 1682.0)]
    public void Convert_ReturnsPixels(long microns, double zoom, double expected)
    {
        var values = new object[] { microns, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InvalidFirstType_ReturnsZero()
    {
        var values = new object[] { "not_a_long", 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class ModelYToCanvasTopConverterExtendedTests
{
    private readonly ModelYToCanvasTopConverter _converter = new();

    [Theory]
    [InlineData(0L, 1189.0, 1.0, 1189.0)]
    [InlineData(0L, 297.0, 1.0, 297.0)]
    public void Convert_ReturnsInvertedY(long micronsY, double heightMm, double zoom, double expected)
    {
        var values = new object[] { micronsY, heightMm, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_A4SheetAt2xZoom()
    {
        var values = new object[] { 10000L, 297.0, 2.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(574.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 0L, 297.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class MicronsToPixelConverterExtendedTests
{
    private readonly MicronsToPixelConverter _converter = new();

    [Theory]
    [InlineData(-5000L, -5.0)]
    [InlineData(0L, 0.0)]
    [InlineData("not_a_long", 0.0)]
    public void Convert_ReturnsPixels(object microns, double expected)
    {
        var values = new object[] { microns, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }
}

public class GridStepToStringConverterTests
{
    private readonly GridStepToStringConverter _converter = new();

    [Theory]
    [InlineData(1.0, "1 мм")]
    [InlineData(5, "5 мм")]
    [InlineData(0.0, "0 мм")]
    public void Convert_ReturnsMmString(object value, string expected)
    {
        var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Null_ReturnsDefault()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("5 мм", result);
    }

    [Theory]
    [InlineData("5 мм", 5.0)]
    [InlineData("10", 10.0)]
    public void ConvertBack_ReturnsDouble(string input, double expected)
    {
        var result = _converter.ConvertBack(input, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_Invalid_ReturnsDoNothing()
    {
        var result = _converter.ConvertBack("invalid", typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_Null_ReturnsDefault()
    {
        var result = _converter.ConvertBack(null, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(5.0, result);
    }
}

public class TopEdgeMicronsMultiConverterTests
{
    private readonly TopEdgeMicronsMultiConverter _converter = new();

    [Fact]
    public void Convert_Line_ReturnsTopEdge()
    {
        var values = new object[] { 1000L, 5000L, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 297.0, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(292.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_Rectangle_ReturnsBottomEdge()
    {
        var values = new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 1000L, 3000L, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 297.0, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(293.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_Text_ReturnsBottomEdge()
    {
        var values = new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 2000L, 3500L, 297.0, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(291.5, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_At2xZoom_ScalesCorrectly()
    {
        var values = new object[] { 1000L, 5000L, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 297.0, 2.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(584.0, Assert.IsType<double>(result), 10);
    }
}

public class LeftEdgeMicronsMultiConverterTests
{
    private readonly LeftEdgeMicronsMultiConverter _converter = new();

    [Fact]
    public void Convert_Line_ReturnsLeftEdge()
    {
        var values = new object[] { 5000L, 1000L, DependencyProperty.UnsetValue, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(1.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_Rectangle_ReturnsMicronsX()
    {
        var values = new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue, 3000L, 1.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(3.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_At2xZoom_ScalesCorrectly()
    {
        var values = new object[] { 2000L, 6000L, DependencyProperty.UnsetValue, 2.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(4.0, Assert.IsType<double>(result), 10);
    }
}

public class RelativeMicronsToPixelConverterTests
{
    private readonly RelativeMicronsToPixelConverter _converter = new();

    [Theory]
    [InlineData(5000L, 1000L, 1.0, 4.0)]
    [InlineData(1000L, 5000L, 1.0, 4.0)]
    [InlineData(3000L, 3000L, 1.0, 0.0)]
    [InlineData(5000L, 1000L, 2.0, 8.0)]
    public void Convert_ReturnsPixels(long a, long b, double zoom, double expected)
    {
        var values = new object[] { a, b, zoom };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 5000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }

    [Fact]
    public void Convert_ZeroZoom_ReturnsZero()
    {
        var values = new object[] { 5000L, 1000L, 0.0 };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, Assert.IsType<double>(result), 10);
    }
}

public class IsHoveredConverterTests
{
    private readonly IsHoveredConverter _converter = new();

    [Fact]
    public void Convert_SameReference_ReturnsTrue()
    {
        var obj = new object();
        var values = new object[] { obj, obj };
        var result = _converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.True(Assert.IsType<bool>(result));
    }

    [Fact]
    public void Convert_DifferentReferences_ReturnsFalse()
    {
        var values = new object[] { new object(), new object() };
        var result = _converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False(Assert.IsType<bool>(result));
    }

    [Fact]
    public void Convert_NullHovered_ReturnsFalse()
    {
        var obj = new object();
        var values = new object[] { obj, null! };
        var result = _converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False(Assert.IsType<bool>(result));
    }

    [Fact]
    public void Convert_BothNull_ReturnsTrue()
    {
        var values = new object[] { null!, null! };
        var result = _converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.True(Assert.IsType<bool>(result));
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsFalse()
    {
        var values = new object[] { new object() };
        var result = _converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False(Assert.IsType<bool>(result));
    }
}

public class TopEdgeMicronsMultiConverterExtendedTests
{
    private readonly TopEdgeMicronsMultiConverter _converter = new();

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 0L, 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_Line_CorrectCalculation()
    {
        var values = new object[]
        {
            1000L, 5000L,
            0L, 0L,
            0L, 0L,
            297.0,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(292.0, result);
    }

    [Fact]
    public void Convert_Rectangle_CorrectCalculation()
    {
        var values = new object[]
        {
            DependencyProperty.UnsetValue, DependencyProperty.UnsetValue,
            2000L, 3000L,
            DependencyProperty.UnsetValue, DependencyProperty.UnsetValue,
            297.0,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(292.0, result);
    }

    [Fact]
    public void Convert_Text_CorrectCalculation()
    {
        var values = new object[]
        {
            DependencyProperty.UnsetValue, DependencyProperty.UnsetValue,
            0L, 0L,
            1000L, 2500L,
            297.0,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(293.5, result);
    }

    [Fact]
    public void Convert_WithZoom_AppliesZoom()
    {
        var values = new object[]
        {
            0L, 0L,
            0L, 0L,
            0L, 0L,
            297.0,
            2.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(594.0, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(0.0, new[] { typeof(long) }, null, CultureInfo.InvariantCulture));
    }
}

public class LeftEdgeMicronsMultiConverterExtendedTests
{
    private readonly LeftEdgeMicronsMultiConverter _converter = new();

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        var values = new object[] { 0L, 1000L };
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_Line_CorrectCalculation()
    {
        var values = new object[]
        {
            1000L, 5000L,
            0L,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void Convert_Line_Reversed_CorrectCalculation()
    {
        var values = new object[]
        {
            5000L, 1000L,
            0L,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void Convert_Rectangle_CorrectCalculation()
    {
        var values = new object[]
        {
            DependencyProperty.UnsetValue, DependencyProperty.UnsetValue,
            3000L,
            1.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(3.0, result);
    }

    [Fact]
    public void Convert_WithZoom_AppliesZoom()
    {
        var values = new object[]
        {
            DependencyProperty.UnsetValue, DependencyProperty.UnsetValue,
            5000L,
            2.0
        };

        var result = _converter.Convert(values, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(10.0, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(0.0, new[] { typeof(long) }, null, CultureInfo.InvariantCulture));
    }
}
