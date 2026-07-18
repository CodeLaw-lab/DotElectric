namespace DotElectric.TemplateEditor.Services;

public interface IDialogFileService
{
    string? OpenFileDialog(string filter);
    string? SaveFileDialog(string filter, string defaultFileName);
}
