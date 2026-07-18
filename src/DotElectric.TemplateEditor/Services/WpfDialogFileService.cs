using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DotElectric.TemplateEditor.Services;

public sealed class WpfDialogFileService : IDialogFileService
{
    private readonly ILogger<FileService>? _logger;

    public WpfDialogFileService(ILogger<FileService>? logger = null)
    {
        _logger = logger;
    }

    public string? OpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter ?? "DotElectric Template|*.tdel",
            DefaultExt = ".tdel",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            _logger?.LogInformation("Открыт диалог выбора файла: filePath={FileName}", dialog.FileName);
        }
        return result == true ? dialog.FileName : null;
    }

    public string? SaveFileDialog(string filter, string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter ?? "DotElectric Template|*.tdel|All Files|*.*",
            DefaultExt = ".tdel",
            FileName = defaultFileName,
            CheckPathExists = true,
            OverwritePrompt = true
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            _logger?.LogInformation("Открыт диалог сохранения: filePath={FileName}", dialog.FileName);
        }
        return result == true ? dialog.FileName : null;
    }
}
