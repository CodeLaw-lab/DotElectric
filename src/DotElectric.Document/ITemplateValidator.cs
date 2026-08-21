namespace DotElectric.Document;

public interface ITemplateValidator
{
    IEnumerable<ValidationError> Validate(Template template);
}
