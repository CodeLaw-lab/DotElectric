namespace DotElectric.Document;

/// <summary>
/// Документные значения по умолчанию (шесть констант, вынесенных из настроек редактора).
/// Это знания документной библиотеки, а не редактора: ими пользуются объекты шаблона,
/// сервис шаблонов и любые будущие приложения-потребители.
/// </summary>
public static class DocumentDefaults
{
    public const string DefaultFontName = "ГОСТ А";
    public const string DefaultTextForeground = "#000000";
    public const string DefaultStrokeColor = "#000000";
    public const string DefaultFillColor = "Transparent";
    public const long DefaultStrokeThicknessMicrons = 500;
    public const long DefaultFontSizeMicrons = 14000;
}
