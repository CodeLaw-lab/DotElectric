using DotElectric.Sheets;

namespace DotElectric.Document;

public interface ITemplateValidator
{
    IEnumerable<ValidationError> Validate(Template template);
    IEnumerable<ValidationError> ValidateObject(TemplateObjectBase obj, Sheet sheet);
}
