namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Стандартная фабрика, создающая реальные обёртки PrintDialogWrapper.
/// </summary>
public sealed class PrintDialogFactory : IPrintDialogFactory
{
    public IPrintDialogWrapper Create()
    {
        return new PrintDialogWrapper();
    }
}
