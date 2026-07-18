using System;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Модификаторы клавиш для инструментов.
/// Независимая от WPF абстракция.
/// </summary>
[Flags]
public enum ToolModifiers
{
    None = 0,
    Ctrl = 1,
    Shift = 2,
    Alt = 4
}