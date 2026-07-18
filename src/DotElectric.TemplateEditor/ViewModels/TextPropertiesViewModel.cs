using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class TextPropertiesViewModel : ObservableObject, IDisposable
{
    private readonly CommandHistory? _commandHistory;
    private readonly Action? _markDirty;
    private readonly Action<string?> _setValidationError;

    private Text? _text;

    public void Dispose()
    {
        if (_text is INotifyPropertyChanged inpc)
            inpc.PropertyChanged -= OnTextPropertyChanged;
        _text = null;
    }

    public TextPropertiesViewModel(
        CommandHistory? commandHistory,
        Action? markDirty,
        Action<string?> setValidationError)
    {
        _commandHistory = commandHistory;
        _markDirty = markDirty;
        _setValidationError = setValidationError;
    }

    public void UpdateObject(Text? text)
    {
        if (_text is INotifyPropertyChanged oldInpc)
            oldInpc.PropertyChanged -= OnTextPropertyChanged;

        _text = text;

        if (_text is INotifyPropertyChanged newInpc)
            newInpc.PropertyChanged += OnTextPropertyChanged;

        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(FontSize));
        OnPropertyChanged(nameof(FontName));
        OnPropertyChanged(nameof(TextTypeValue));
        OnPropertyChanged(nameof(Rotation));
        OnPropertyChanged(nameof(Key));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(DefaultValue));
        OnPropertyChanged(nameof(Foreground));
        OnPropertyChanged(nameof(TextWrapping));
        OnPropertyChanged(nameof(TextAlignment));
    }

    private void OnTextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "MicronsX":               OnPropertyChanged(nameof(X)); break;
            case "MicronsY":               OnPropertyChanged(nameof(Y)); break;
            case nameof(Text.Content):     OnPropertyChanged(nameof(Content)); break;
            case nameof(Text.FontSizeMicrons): OnPropertyChanged(nameof(FontSize)); break;
            case nameof(Text.FontName):    OnPropertyChanged(nameof(FontName)); break;
            case nameof(Text.TextType):    OnPropertyChanged(nameof(TextTypeValue)); break;
            case nameof(Text.RotationAngle): OnPropertyChanged(nameof(Rotation)); break;
            case nameof(Text.Key):         OnPropertyChanged(nameof(Key)); break;
            case nameof(Text.IsEditable):  OnPropertyChanged(nameof(IsEditable)); break;
            case nameof(Text.DefaultValue): OnPropertyChanged(nameof(DefaultValue)); break;
            case nameof(Text.Foreground):  OnPropertyChanged(nameof(Foreground)); break;
            case nameof(Text.TextWrapping): OnPropertyChanged(nameof(TextWrapping)); break;
            case nameof(Text.TextAlignment): OnPropertyChanged(nameof(TextAlignment)); break;
        }
    }

    public long? X => _text?.MicronsX;
    public long? Y => _text?.MicronsY;
    public string? Content => _text?.Content;
    public long? FontSize => _text?.FontSizeMicrons;
    public string? FontName => _text?.FontName;
    public TextType? TextTypeValue => _text?.TextType;
    public int? Rotation => _text?.RotationAngle;
    public string? Key => _text?.Key;
    public bool? IsEditable => _text?.IsEditable;
    public string? DefaultValue => _text?.DefaultValue;
    public string? Foreground => _text?.Foreground;
    public bool? TextWrapping => _text?.TextWrapping;
    public string? TextAlignment => _text?.TextAlignment;

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
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.MicronsX, v => text.MicronsX = v,
            ValidationService.ValidateCoordinate, nameof(X), "X текста");
    }

    [RelayCommand]
    private void ChangeY(long value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.MicronsY, v => text.MicronsY = v,
            ValidationService.ValidateCoordinate, nameof(Y), "Y текста");
    }

    [RelayCommand]
    private void ChangeContent(string? value)
    {
        if (_text is null) return;
        var text = _text;
        var error = ValidationService.ValidateTextContent(value);
        if (error != null) { _setValidationError(error); return; }
        var cmd = new ChangePropertyCommand<string?>(
            () => text.Content, v => text.Content = v ?? string.Empty,
            value ?? string.Empty, "Содержимое текста", _markDirty);
        _commandHistory?.Push(cmd);
        OnPropertyChanged(nameof(Content));
    }

    [RelayCommand]
    private void ChangeFontSize(long value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.FontSizeMicrons, v => text.FontSizeMicrons = v,
            ValidationService.ValidateFontSize, nameof(FontSize), "Размер шрифта");
    }

    [RelayCommand]
    private void ChangeTextType(TextType value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.TextType, v => text.TextType = v,
            null, nameof(TextTypeValue), "Тип текста");
    }

    [RelayCommand]
    private void ChangeRotation(int value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.RotationAngle, v => text.RotationAngle = v,
            null, nameof(Rotation), "Поворот текста");
    }

    [RelayCommand]
    private void ChangeKey(string? value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.Key, v => text.Key = v,
            null, nameof(Key), "Ключ поля");
    }

    [RelayCommand]
    private void ChangeIsEditable(bool value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.IsEditable, v => text.IsEditable = v,
            null, nameof(IsEditable), "Изменяемое");
    }

    [RelayCommand]
    private void ChangeDefaultValue(string? value)
    {
        if (_text is null) return;
        var text = _text;
        var cmd = new ChangePropertyCommand<string?>(
            () => text.DefaultValue, v => text.DefaultValue = v,
            value ?? string.Empty, "Значение по умолчанию", _markDirty);
        _commandHistory?.Push(cmd);
        OnPropertyChanged(nameof(DefaultValue));
    }

    [RelayCommand]
    private void ChangeForeground(string value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.Foreground, v => text.Foreground = v,
            ValidationService.ValidateHexColor, nameof(Foreground), "Цвет текста");
    }

    [RelayCommand]
    private void ChangeTextWrapping(bool value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.TextWrapping, v => text.TextWrapping = v,
            null, nameof(TextWrapping), "Перенос текста");
    }

    [RelayCommand]
    private void ChangeTextAlignment(string value)
    {
        if (_text is null) return;
        var text = _text;
        SetProperty(value, () => text.TextAlignment, v => text.TextAlignment = v,
            null, nameof(TextAlignment), "Выравнивание");
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
    private void ChangeFontSizeFromString(string? value) => ChangeFromMmString(value, ChangeFontSize);

    [RelayCommand]
    private void ChangeRotationFromString(string? value)
    {
        if (int.TryParse(value, out var rotation))
            ChangeRotation(rotation);
    }

    [RelayCommand]
    private void ChangeFontNameFromString(string? value)
    {
        if (_text is null) return;
        if (string.IsNullOrWhiteSpace(value)) return;
        var text = _text;
        var cmd = new ChangePropertyCommand<string>(
            () => text.FontName, v => text.FontName = v,
            value, "Шрифт текста", _markDirty);
        _commandHistory?.Push(cmd);
        OnPropertyChanged(nameof(FontName));
    }

    [RelayCommand]
    private void ChangeTextTypeFromString(string? value)
    {
        var textType = value switch
        {
            "Текст" => TextType.Text,
            "Размер" => TextType.Dimension,
            "Допуск" => TextType.Tolerance,
            "Примечание" => TextType.Note,
            "Обозначение" => TextType.Label,
            _ => TextType.Text
        };
        ChangeTextType(textType);
    }
}
