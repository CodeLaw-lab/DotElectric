namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Тип пункта модели меню «Файл > Новый шаблон».
/// </summary>
public enum NewSheetMenuKind
{
    /// <summary>
    /// Группа формата с вложенными пунктами ориентаций.
    /// </summary>
    FormatGroup,

    /// <summary>
    /// Визуальный разделитель между группами.
    /// </summary>
    Separator,

    /// <summary>
    /// Пункт-команда «Пользовательский...».
    /// </summary>
    CustomCommand
}

/// <summary>
/// Вложенный пункт ориентации внутри группы формата.
/// Несёт типизированную ориентацию: формат (строковая идентичность)
/// и значение перечисления — без строковых суффиксов.
/// </summary>
public sealed record NewSheetOrientationEntry(string Header, string Format, SheetOrientation Orientation);

/// <summary>
/// Пункт модели меню «Файл > Новый шаблон». Модель генерируется из
/// каталога стандартных форматов и рендерится через ItemsSource.
/// </summary>
public sealed record NewSheetMenuEntry(
    NewSheetMenuKind Kind,
    string Header,
    IReadOnlyList<NewSheetOrientationEntry>? Orientations = null);
