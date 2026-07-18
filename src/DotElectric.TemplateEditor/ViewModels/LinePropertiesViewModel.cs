using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class LinePropertiesViewModel : ObservableObject, IDisposable
{
    private readonly CommandHistory? _commandHistory;
    private readonly Action? _markDirty;
    private readonly Action<string?> _setValidationError;

    private Line? _line;

    public void Dispose()
    {
        if (_line is INotifyPropertyChanged inpc)
            inpc.PropertyChanged -= OnLinePropertyChanged;
        _line = null;
    }

    public LinePropertiesViewModel(
        CommandHistory? commandHistory,
        Action? markDirty,
        Action<string?> setValidationError)
    {
        _commandHistory = commandHistory;
        _markDirty = markDirty;
        _setValidationError = setValidationError;
    }

    public void UpdateObject(Line? line)
    {
        if (_line is INotifyPropertyChanged oldInpc)
            oldInpc.PropertyChanged -= OnLinePropertyChanged;

        _line = line;

        if (_line is INotifyPropertyChanged newInpc)
            newInpc.PropertyChanged += OnLinePropertyChanged;

        OnPropertyChanged(nameof(StartX));
        OnPropertyChanged(nameof(StartY));
        OnPropertyChanged(nameof(EndX));
        OnPropertyChanged(nameof(EndY));
        OnPropertyChanged(nameof(LineTypeValue));
        OnPropertyChanged(nameof(StrokeThickness));
        OnPropertyChanged(nameof(StrokeColor));
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Line.StartMicronsX): OnPropertyChanged(nameof(StartX)); break;
            case nameof(Line.StartMicronsY): OnPropertyChanged(nameof(StartY)); break;
            case nameof(Line.EndMicronsX):   OnPropertyChanged(nameof(EndX)); break;
            case nameof(Line.EndMicronsY):   OnPropertyChanged(nameof(EndY)); break;
            case "LineType":                 OnPropertyChanged(nameof(LineTypeValue)); break;
            case "StrokeThicknessMicrons":   OnPropertyChanged(nameof(StrokeThickness)); break;
            case nameof(Line.StrokeColor):   OnPropertyChanged(nameof(StrokeColor)); break;
        }
    }

    public long? StartX => _line?.StartMicronsX;
    public long? StartY => _line?.StartMicronsY;
    public long? EndX => _line?.EndMicronsX;
    public long? EndY => _line?.EndMicronsY;
    public LineType? LineTypeValue => _line?.LineType;
    public long? StrokeThickness => _line?.StrokeThicknessMicrons;
    public string? StrokeColor => _line?.StrokeColor;

    private void SetProperty<T>(T value, Func<T> getter, Action<T> setter,
        Func<T, string?>? validator, string propertyName, string commandName, Action? afterSet = null)
    {
        if (validator != null)
        {
            var error = validator(value);
            if (error != null) { _setValidationError(error); return; }
        }
        var cmd = new ChangePropertyCommand<T>(getter, setter, value, commandName, _markDirty);
        _commandHistory?.Push(cmd);
        OnPropertyChanged(propertyName);
        afterSet?.Invoke();
    }

    [RelayCommand]
    private void ChangeStartX(long value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.StartMicronsX, v => line.StartMicronsX = v,
            ValidationService.ValidateCoordinate, nameof(StartX), "X1 линии");
    }

    [RelayCommand]
    private void ChangeStartY(long value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.StartMicronsY, v => line.StartMicronsY = v,
            ValidationService.ValidateCoordinate, nameof(StartY), "Y1 линии");
    }

    [RelayCommand]
    private void ChangeEndX(long value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.EndMicronsX, v => line.EndMicronsX = v,
            ValidationService.ValidateCoordinate, nameof(EndX), "X2 линии");
    }

    [RelayCommand]
    private void ChangeEndY(long value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.EndMicronsY, v => line.EndMicronsY = v,
            ValidationService.ValidateCoordinate, nameof(EndY), "Y2 линии");
    }

    [RelayCommand]
    private void ChangeLineType(LineType value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.LineType, v => line.LineType = v,
            null, nameof(LineTypeValue), "Тип линии");
    }

    [RelayCommand]
    private void ChangeStrokeThickness(long value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.StrokeThicknessMicrons, v => line.StrokeThicknessMicrons = v,
            ValidationService.ValidateDimension, nameof(StrokeThickness), "Толщина линии");
    }

    [RelayCommand]
    private void ChangeStrokeColor(string value)
    {
        if (_line is null) return;
        var line = _line;
        SetProperty(value, () => line.StrokeColor, v => line.StrokeColor = v,
            ValidationService.ValidateHexColor, nameof(StrokeColor), "Цвет линии");
    }

    private void ChangeFromMmString(string? value, Action<long> setter)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var mm))
            setter(Coordinate.ToMicrons(mm));
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

    private static LineType ParseLineType(string? value)
    {
        return value switch
        {
            "Сплошная" => LineType.Solid,
            "Штриховая" => LineType.Dashed,
            "Штрихпунктирная" => LineType.DashDot,
            "Штрихпунктирная с двумя штрихами" => LineType.DashDotDot,
            _ => LineType.Solid
        };
    }
}
