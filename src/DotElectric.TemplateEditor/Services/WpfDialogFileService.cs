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
        var dialog = CreateOpenDialog(filter);

        var result = dialog.ShowDialog();
        if (result == true)
        {
            _logger?.LogInformation("Открыт диалог выбора файла: filePath={FileName}", dialog.FileName);
        }
        return result == true ? dialog.FileName : null;
    }

    public string? SaveFileDialog(string filter, string defaultFileName)
    {
        var dialog = CreateSaveDialog(filter, defaultFileName);

        var result = dialog.ShowDialog();
        if (result == true)
        {
            _logger?.LogInformation("Открыт диалог сохранения: filePath={FileName}", dialog.FileName);
        }
        return result == true ? dialog.FileName : null;
    }

    internal static OpenFileDialog CreateOpenDialog(string? filter)
    {
        return new OpenFileDialog
        {
            Filter = filter ?? "DotElectric Template|*.tdel",
            DefaultExt = ".tdel",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };
    }

    internal static SaveFileDialog CreateSaveDialog(string? filter, string? defaultFileName)
    {
        return new SaveFileDialog
        {
            Filter = filter ?? "DotElectric Template|*.tdel|All Files|*.*",
            DefaultExt = ".tdel",
            FileName = defaultFileName,
            CheckPathExists = true,
            OverwritePrompt = true
        };
    }
}
