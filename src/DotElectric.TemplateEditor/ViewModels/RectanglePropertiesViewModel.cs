using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class RectanglePropertiesViewModel : ObjectPropertiesViewModel<Rectangle>
{
    private static readonly IReadOnlyDictionary<string, string> RectanglePropertyMap = new Dictionary<string, string>
    {
        [nameof(Rectangle.MicronsX)] = nameof(X),
        [nameof(Rectangle.MicronsY)] = nameof(Y),
        [nameof(Rectangle.WidthMicrons)] = nameof(Width),
        [nameof(Rectangle.HeightMicrons)] = nameof(Height),
        [nameof(Rectangle.LineType)] = nameof(LineTypeValue),
        [nameof(Rectangle.StrokeThicknessMicrons)] = nameof(StrokeThickness),
        [nameof(Rectangle.StrokeColor)] = nameof(StrokeColor),
        [nameof(Rectangle.FillColor)] = nameof(FillColor),
    };

    public RectanglePropertiesViewModel(
        CommandHistory? commandHistory,
        Action? markDirty,
        Action<string?> setValidationError)
        : base(commandHistory, markDirty, setValidationError)
    {
    }

    protected override IReadOnlyDictionary<string, string> PropertyMap => RectanglePropertyMap;

    public long? X => CurrentObject?.MicronsX;
    public long? Y => CurrentObject?.MicronsY;
    public long? Width => CurrentObject?.WidthMicrons;
    public long? Height => CurrentObject?.HeightMicrons;
    public LineType? LineTypeValue => CurrentObject?.LineType;
    public long? StrokeThickness => CurrentObject?.StrokeThicknessMicrons;
    public string? StrokeColor => CurrentObject?.StrokeColor;
    public string? FillColor => CurrentObject?.FillColor;

    [RelayCommand]
    private void ChangeX(long value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.MicronsX, v => rect.MicronsX = v,
            ValidationService.ValidateCoordinate, nameof(X), "X прямоугольника");
    }

    [RelayCommand]
    private void ChangeY(long value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.MicronsY, v => rect.MicronsY = v,
            ValidationService.ValidateCoordinate, nameof(Y), "Y прямоугольника");
    }

    [RelayCommand]
    private void ChangeWidth(long value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.WidthMicrons, v => rect.WidthMicrons = v,
            ValidationService.ValidateDimension, nameof(Width), "Ширина",
            () => OnPropertyChanged(nameof(X)));
    }

    [RelayCommand]
    private void ChangeHeight(long value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.HeightMicrons, v => rect.HeightMicrons = v,
            ValidationService.ValidateDimension, nameof(Height), "Высота",
            () => OnPropertyChanged(nameof(Y)));
    }

    [RelayCommand]
    private void ChangeLineType(LineType value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.LineType, v => rect.LineType = v,
            null, nameof(LineTypeValue), "Тип линии прямоугольника");
    }

    [RelayCommand]
    private void ChangeStrokeThickness(long value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.StrokeThicknessMicrons, v => rect.StrokeThicknessMicrons = v,
            ValidationService.ValidateDimension, nameof(StrokeThickness), "Толщина обводки");
    }

    [RelayCommand]
    private void ChangeStrokeColor(string value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.StrokeColor, v => rect.StrokeColor = v,
            ValidationService.ValidateHexColor, nameof(StrokeColor), "Цвет обводки");
    }

    [RelayCommand]
    private void ChangeFillColor(string value)
    {
        var rect = CurrentObject;
        if (rect is null) return;
        SetProperty(value, () => rect.FillColor, v => rect.FillColor = v,
            ValidationService.ValidateHexColor, nameof(FillColor), "Цвет заливки");
    }

    [RelayCommand]
    private void ChangeXFromString(string? value) => ChangeFromMmString(value, ChangeX);

    [RelayCommand]
    private void ChangeYFromString(string? value) => ChangeFromMmString(value, ChangeY);

    [RelayCommand]
    private void ChangeWidthFromString(string? value) => ChangeFromMmString(value, ChangeWidth);

    [RelayCommand]
    private void ChangeHeightFromString(string? value) => ChangeFromMmString(value, ChangeHeight);

    [RelayCommand]
    private void ChangeLineTypeFromString(string? value) => ChangeLineType(ParseLineType(value));

    [RelayCommand]
    private void ChangeStrokeThicknessFromString(string? value) => ChangeFromMmString(value, ChangeStrokeThickness);
}
