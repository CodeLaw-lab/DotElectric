namespace DotElectric.TemplateEditor.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
