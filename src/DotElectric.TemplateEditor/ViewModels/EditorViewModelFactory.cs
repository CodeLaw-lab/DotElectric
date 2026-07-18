using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotElectric.TemplateEditor.ViewModels;

public class EditorViewModelFactory : IEditorViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EditorViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public EditorViewModel Create(
        Template template,
        GridSettings? gridSettings = null,
        IPrintService? printService = null)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));

        var templateService = _serviceProvider.GetRequiredService<ITemplateService>();
        var resolvedPrintService = printService ?? _serviceProvider.GetService<IPrintService>();

        return new EditorViewModel(
            template,
            templateService,
            gridSettings,
            resolvedPrintService);
    }

    public EditorViewModel CreateWithFilePath(
        Template template,
        string filePath,
        GridSettings? gridSettings = null,
        IPrintService? printService = null)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var templateService = _serviceProvider.GetRequiredService<ITemplateService>();
        var resolvedPrintService = printService ?? _serviceProvider.GetService<IPrintService>();

        return new EditorViewModel(
            template,
            filePath,
            templateService,
            gridSettings,
            resolvedPrintService);
    }
}
