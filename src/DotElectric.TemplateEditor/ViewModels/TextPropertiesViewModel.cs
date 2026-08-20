using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class TextPropertiesViewModel : ObjectPropertiesViewModel<Text>
{
    private static readonly IReadOnlyDictionary<string, string> TextPropertyMap = new Dictionary<string, string>
    {
        [nameof(Text.MicronsX)] = nameof(X),
        [nameof(Text.MicronsY)] = nameof(Y),
        [nameof(Text.Content)] = nameof(Content),
        [nameof(Text.FontSizeMicrons)] = nameof(FontSize),
        [nameof(Text.FontName)] = nameof(FontName),
        [nameof(Text.TextType)] = nameof(TextTypeValue),
        [nameof(Text.RotationAngle)] = nameof(Rotation),
        [nameof(Text.Key)] = nameof(Key),
        [nameof(Text.IsEditable)] = nameof(IsEditable),
        [nameof(Text.DefaultValue)] = nameof(DefaultValue),
        [nameof(Text.Foreground)] = nameof(Foreground),
        [nameof(Text.TextWrapping)] = nameof(TextWrapping),
        [nameof(Text.TextAlignment)] = nameof(TextAlignment),
    };

    public TextPropertiesViewModel(
        CommandHistory? commandHistory,
        Action<string?> setValidationError)
        : base(commandHistory, setValidationError)
    {
    }

    protected override IReadOnlyDictionary<string, string> PropertyMap => TextPropertyMap;

    public long? X => CurrentObject?.MicronsX;
    public long? Y => CurrentObject?.MicronsY;
    public string? Content => CurrentObject?.Content;
    public long? FontSize => CurrentObject?.FontSizeMicrons;
    public string? FontName => CurrentObject?.FontName;
    public TextType? TextTypeValue => CurrentObject?.TextType;
    public int? Rotation => CurrentObject?.RotationAngle;
    public string? Key => CurrentObject?.Key;
    public bool? IsEditable => CurrentObject?.IsEditable;
    public string? DefaultValue => CurrentObject?.DefaultValue;
    public string? Foreground => CurrentObject?.Foreground;
    public bool? TextWrapping => CurrentObject?.TextWrapping;
    public string? TextAlignment => CurrentObject?.TextAlignment;

    [RelayCommand]
    private void ChangeX(long value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.MicronsX, v => text.MicronsX = v,
            ValidationService.ValidateCoordinate, nameof(X), "X текста");
    }

    [RelayCommand]
    private void ChangeY(long value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.MicronsY, v => text.MicronsY = v,
            ValidationService.ValidateCoordinate, nameof(Y), "Y текста");
    }

    [RelayCommand]
    private void ChangeContent(string? value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty<string?>(value ?? string.Empty, () => text.Content, v => text.Content = v ?? string.Empty,
            ValidationService.ValidateTextContent, nameof(Content), "Содержимое текста");
    }

    [RelayCommand]
    private void ChangeFontSize(long value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.FontSizeMicrons, v => text.FontSizeMicrons = v,
            ValidationService.ValidateFontSize, nameof(FontSize), "Размер шрифта");
    }

    [RelayCommand]
    private void ChangeTextType(TextType value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.TextType, v => text.TextType = v,
            null, nameof(TextTypeValue), "Тип текста");
    }

    [RelayCommand]
    private void ChangeRotation(int value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.RotationAngle, v => text.RotationAngle = v,
            null, nameof(Rotation), "Поворот текста");
    }

    [RelayCommand]
    private void ChangeKey(string? value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.Key, v => text.Key = v,
            null, nameof(Key), "Ключ поля");
    }

    [RelayCommand]
    private void ChangeIsEditable(bool value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.IsEditable, v => text.IsEditable = v,
            null, nameof(IsEditable), "Изменяемое");
    }

    [RelayCommand]
    private void ChangeDefaultValue(string? value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty<string?>(value ?? string.Empty, () => text.DefaultValue, v => text.DefaultValue = v,
            null, nameof(DefaultValue), "Значение по умолчанию");
    }

    [RelayCommand]
    private void ChangeForeground(string value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.Foreground, v => text.Foreground = v,
            HexColorValidation.Validate, nameof(Foreground), "Цвет текста");
    }

    [RelayCommand]
    private void ChangeTextWrapping(bool value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.TextWrapping, v => text.TextWrapping = v,
            null, nameof(TextWrapping), "Перенос текста");
    }

    [RelayCommand]
    private void ChangeTextAlignment(string value)
    {
        var text = CurrentObject;
        if (text is null) return;
        SetProperty(value, () => text.TextAlignment, v => text.TextAlignment = v,
            null, nameof(TextAlignment), "Выравнивание");
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
        var text = CurrentObject;
        if (text is null) return;
        if (string.IsNullOrWhiteSpace(value)) return;
        SetProperty(value, () => text.FontName, v => text.FontName = v,
            null, nameof(FontName), "Шрифт текста");
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
