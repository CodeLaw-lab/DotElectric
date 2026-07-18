using System.Windows.Documents;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

public interface IPrintDocumentGenerator
{
    FixedDocument Generate(Template template);
    FixedDocument Generate(Template template, string title);
}
