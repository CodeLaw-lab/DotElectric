using DotElectric.TemplateEditor.Constants;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Настройки TextTool по умолчанию.
/// </summary>
public sealed class TextToolSettings
{
    public TextType DefaultTextType => TextType.Text;
    public long DefaultFontSizeMicrons => DocumentDefaults.DefaultFontSizeMicrons;
    public string DefaultFont => DocumentDefaults.DefaultFontName;
    public string DefaultContent => "Текст";
}