using System.Windows;
using System.Windows.Media;

namespace DotElectric.TemplateEditor.Tests.Helpers;

internal sealed class FakePresentationSource : PresentationSource
{
    private Visual? _rootVisual;

    public override bool IsDisposed => false;

    public override Visual? RootVisual
    {
        get => _rootVisual;
        set => _rootVisual = value;
    }

    protected override CompositionTarget? GetCompositionTargetCore() => null;
}