using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Настройки инструмента TextTool.
/// </summary>
public interface ITextToolSettings
{
    /// <summary>Тип текста по умолчанию.</summary>
    TextType DefaultTextType { get; }

    /// <summary>Размер шрифта по умолчанию в микронах.</summary>
    long DefaultFontSizeMicrons { get; }

    /// <summary>Название шрифта по умолчанию.</summary>
    string DefaultFont { get; }

    /// <summary>Содержимое текста по умолчанию.</summary>
    string DefaultContent { get; }
}
