namespace DotElectric.TemplateEditor.Models.Objects;

/// <summary>
/// Тип линии (ГОСТ 2.303-68).
/// </summary>
public enum LineType
{
    /// <summary>
    /// Сплошная основная (Тип 1).
    /// </summary>
    Solid = 0,

    /// <summary>
    /// Штриховая (Тип 2).
    /// </summary>
    Dashed = 1,

    /// <summary>
    /// Штрихпунктирная (Тип 3).
    /// </summary>
    DashDot = 2,

    /// <summary>
    /// Штрихпунктирная с двумя точками (Тип 4).
    /// </summary>
    DashDotDot = 3
}
