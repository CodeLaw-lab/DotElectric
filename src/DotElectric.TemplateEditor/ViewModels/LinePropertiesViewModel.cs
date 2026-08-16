using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class LinePropertiesViewModel : ObjectPropertiesViewModel<Line>
{
    private static readonly IReadOnlyDictionary<string, string> LinePropertyMap = new Dictionary<string, string>
    {
        [nameof(Line.StartMicronsX)] = nameof(StartX),
        [nameof(Line.StartMicronsY)] = nameof(StartY),
        [nameof(Line.EndMicronsX)] = nameof(EndX),
        [nameof(Line.EndMicronsY)] = nameof(EndY),
        [nameof(Line.LineType)] = nameof(LineTypeValue),
        [nameof(Line.StrokeThicknessMicrons)] = nameof(StrokeThickness),
        [nameof(Line.StrokeColor)] = nameof(StrokeColor),
    };

    public LinePropertiesViewModel(
        CommandHistory? commandHistory,
        Action? markDirty,
        Action<string?> setValidationError)
        : base(commandHistory, markDirty, setValidationError)
    {
    }

    protected override IReadOnlyDictionary<string, string> PropertyMap => LinePropertyMap;

    public long? StartX => CurrentObject?.StartMicronsX;
    public long? StartY => CurrentObject?.StartMicronsY;
    public long? EndX => CurrentObject?.EndMicronsX;
    public long? EndY => CurrentObject?.EndMicronsY;
    public LineType? LineTypeValue => CurrentObject?.LineType;
    public long? StrokeThickness => CurrentObject?.StrokeThicknessMicrons;
    public string? StrokeColor => CurrentObject?.StrokeColor;

    [RelayCommand]
    private void ChangeStartX(long value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.StartMicronsX, v => line.StartMicronsX = v,
            ValidationService.ValidateCoordinate, nameof(StartX), "X1 линии");
    }

    [RelayCommand]
    private void ChangeStartY(long value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.StartMicronsY, v => line.StartMicronsY = v,
            ValidationService.ValidateCoordinate, nameof(StartY), "Y1 линии");
    }

    [RelayCommand]
    private void ChangeEndX(long value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.EndMicronsX, v => line.EndMicronsX = v,
            ValidationService.ValidateCoordinate, nameof(EndX), "X2 линии");
    }

    [RelayCommand]
    private void ChangeEndY(long value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.EndMicronsY, v => line.EndMicronsY = v,
            ValidationService.ValidateCoordinate, nameof(EndY), "Y2 линии");
    }

    [RelayCommand]
    private void ChangeLineType(LineType value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.LineType, v => line.LineType = v,
            null, nameof(LineTypeValue), "Тип линии");
    }

    [RelayCommand]
    private void ChangeStrokeThickness(long value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.StrokeThicknessMicrons, v => line.StrokeThicknessMicrons = v,
            ValidationService.ValidateDimension, nameof(StrokeThickness), "Толщина линии");
    }

    [RelayCommand]
    private void ChangeStrokeColor(string value)
    {
        var line = CurrentObject;
        if (line is null) return;
        SetProperty(value, () => line.StrokeColor, v => line.StrokeColor = v,
            ValidationService.ValidateHexColor, nameof(StrokeColor), "Цвет линии");
    }

    [RelayCommand]
    private void ChangeStartXFromString(string? value) => ChangeFromMmString(value, ChangeStartX);

    [RelayCommand]
    private void ChangeStartYFromString(string? value) => ChangeFromMmString(value, ChangeStartY);

    [RelayCommand]
    private void ChangeEndXFromString(string? value) => ChangeFromMmString(value, ChangeEndX);

    [RelayCommand]
    private void ChangeEndYFromString(string? value) => ChangeFromMmString(value, ChangeEndY);

    [RelayCommand]
    private void ChangeLineTypeFromString(string? value) => ChangeLineType(ParseLineType(value));

    [RelayCommand]
    private void ChangeStrokeThicknessFromString(string? value) => ChangeFromMmString(value, ChangeStrokeThickness);
}
