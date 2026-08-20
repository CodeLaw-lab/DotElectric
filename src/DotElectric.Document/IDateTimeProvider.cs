namespace DotElectric.Document;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
