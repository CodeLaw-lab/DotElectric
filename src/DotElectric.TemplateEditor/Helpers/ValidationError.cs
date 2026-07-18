namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Уровень серьёзности ошибки валидации.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Ошибка — блокирует сохранение.
    /// </summary>
    Error,

    /// <summary>
    /// Предупреждение — не блокирует, но требует внимания.
    /// </summary>
    Warning
}

/// <summary>
/// Описание ошибки валидации шаблона.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Код правила (например: "V-001").
    /// </summary>
    public string RuleId { get; }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// ID объекта, к которому относится ошибка (если применимо).
    /// </summary>
    public string? ObjectId { get; }

    /// <summary>
    /// Уровень серьёзности.
    /// </summary>
    public ValidationSeverity Severity { get; }

    public ValidationError(string ruleId, string message, string? objectId = null, ValidationSeverity severity = ValidationSeverity.Error)
    {
        RuleId = ruleId;
        Message = message;
        ObjectId = objectId;
        Severity = severity;
    }

    public override string ToString() => $"[{RuleId}] {Message}";
}
