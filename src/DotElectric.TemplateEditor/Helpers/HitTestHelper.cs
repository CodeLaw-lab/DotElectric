using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Helper для hit-testing объектов по телу (полиморфный ContainsPoint).
/// Hit по маркерам выделения — в MarkerLayout.
/// </summary>
public static class HitTestHelper
{
    /// <summary>
    /// Найти верхний объект под точкой (для single-selection).
    /// Возвращает верхний объект в Z-order или null.
    /// </summary>
    public static TemplateObjectBase? HitTest(
        PointMicrons point,
        IList<TemplateObjectBase> objects)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i].ContainsPoint(point))
                return objects[i];
        }
        return null;
    }
}
