namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Узел сетки в абсолютных координатах листа (микроны, Y-up).
/// </summary>
public readonly struct GridNode
{
    public long XMicrons { get; }
    public long YMicrons { get; }

    public GridNode(long xMicrons, long yMicrons)
    {
        XMicrons = xMicrons;
        YMicrons = yMicrons;
    }
}
