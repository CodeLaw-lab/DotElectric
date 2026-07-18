using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Настройки TextTool по умолчанию.
/// </summary>
public sealed class TextToolSettings : ITextToolSettings
{
    public TextType DefaultTextType => TextType.Text;
    public long DefaultFontSizeMicrons => EditorSettings.DefaultFontSizeMicrons;
    public string DefaultFont => EditorSettings.DefaultFontName;
    public string DefaultContent => "Текст";
}