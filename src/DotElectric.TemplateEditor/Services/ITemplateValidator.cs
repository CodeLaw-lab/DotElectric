using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

public interface ITemplateValidator
{
    IEnumerable<ValidationError> Validate(Template template);
    IEnumerable<ValidationError> ValidateObject(TemplateObjectBase obj, Sheet sheet);
}
