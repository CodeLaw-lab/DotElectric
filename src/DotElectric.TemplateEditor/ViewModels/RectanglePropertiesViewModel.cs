using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class RectanglePropertiesViewModel : ObservableObject, IDisposable
{
    private readonly CommandHistory? _commandHistory;
    private readonly Action? _markDirty;
    private readonly Action<string?> _setValidationError;

    private Rectangle? _rect;

    public void Dispose()
    {
        if (_rect is INotifyPropertyChanged inpc)
            inpc.PropertyChanged -= OnRectPropertyChanged;
        _rect = null;
    }

    public RectanglePropertiesViewModel(
        CommandHistory? commandHistory,
        Action? markDirty,
        Action<string?> setValidationError)
    {
        _commandHistory = commandHistory;
        _markDirty = markDirty;
        _setValidationError = setValidationError;
    }

    public void UpdateObject(Rectangle? rect)
    {
        if (_rect is INotifyPropertyChanged oldInpc)
            oldInpc.PropertyChanged -= OnRectPropertyChanged;

        _rect = rect;

        if (_rect is INotifyPropertyChanged newInpc)
            newInpc.PropertyChanged += OnRectPropertyChanged;

        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(LineTypeValue));
        OnPropertyChanged(nameof(StrokeThickness));
        OnPropertyChanged(nameof(StrokeColor));
        OnPropertyChanged(nameof(FillColor));
    }

    private void OnRectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "MicronsX":               OnPropertyChanged(nameof(X)); break;
            case "MicronsY":               OnPropertyChanged(nameof(Y)); break;
            case nameof(Rectangle.WidthMicrons):  OnPropertyChanged(nameof(Width)); break;
            case nameof(Rectangle.HeightMicrons): OnPropertyChanged(nameof(Height)); break;
            case "LineType":               OnPropertyChanged(nameof(LineTypeValue)); break;
            case "StrokeThicknessMicrons": OnPropertyChanged(nameof(StrokeThickness)); break;
            case nameof(Rectangle.StrokeColor):   OnPropertyChanged(nameof(StrokeColor)); break;
            case nameof(Rectangle.FillColor):     OnPropertyChanged(nameof(FillColor)); break;
        }
    }

    public long? X => _rect?.MicronsX;
    public long? Y => _rect?.MicronsY;
    public long? Width => _rect?.WidthMicrons;
    public long? Height => _rect?.HeightMicrons;
    public LineType? LineTypeValue => _rect?.LineType;
    public long? StrokeThickness => _rect?.StrokeThicknessMicrons;
    public string? StrokeColor => _rect?.StrokeColor;
    public string? FillColor => _rect?.FillColor;

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
    private void ChangeX(long value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.MicronsX, v => rect.MicronsX = v,
            ValidationService.ValidateCoordinate, nameof(X), "X прямоугольника");
    }

    [RelayCommand]
    private void ChangeY(long value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.MicronsY, v => rect.MicronsY = v,
            ValidationService.ValidateCoordinate, nameof(Y), "Y прямоугольника");
    }

    [RelayCommand]
    private void ChangeWidth(long value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.WidthMicrons, v => rect.WidthMicrons = v,
            ValidationService.ValidateDimension, nameof(Width), "Ширина",
            () => OnPropertyChanged(nameof(X)));
    }

    [RelayCommand]
    private void ChangeHeight(long value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.HeightMicrons, v => rect.HeightMicrons = v,
            ValidationService.ValidateDimension, nameof(Height), "Высота",
            () => OnPropertyChanged(nameof(Y)));
    }

    [RelayCommand]
    private void ChangeLineType(LineType value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.LineType, v => rect.LineType = v,
            null, nameof(LineTypeValue), "Тип линии прямоугольника");
    }

    [RelayCommand]
    private void ChangeStrokeThickness(long value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.StrokeThicknessMicrons, v => rect.StrokeThicknessMicrons = v,
            ValidationService.ValidateDimension, nameof(StrokeThickness), "Толщина обводки");
    }

    [RelayCommand]
    private void ChangeStrokeColor(string value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.StrokeColor, v => rect.StrokeColor = v,
            ValidationService.ValidateHexColor, nameof(StrokeColor), "Цвет обводки");
    }

    [RelayCommand]
    private void ChangeFillColor(string value)
    {
        if (_rect is null) return;
        var rect = _rect;
        SetProperty(value, () => rect.FillColor, v => rect.FillColor = v,
            ValidationService.ValidateHexColor, nameof(FillColor), "Цвет заливки");
    }

    private void ChangeFromMmString(string? value, Action<long> setter)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var mm))
            setter(Coordinate.ToMicrons(mm));
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
