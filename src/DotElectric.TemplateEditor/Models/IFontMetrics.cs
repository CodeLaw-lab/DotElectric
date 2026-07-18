namespace DotElectric.TemplateEditor.Models;

public interface IFontMetrics
{
    bool IsInitialized { get; }
    void Initialize();
    void Reset();
    void InitializeWithTestValues(double heightRatio, double advWidthRatio, string fontName);
    double GetHeightRatio(string fontName);
    double GetAdvWidthRatio(string fontName);
}
