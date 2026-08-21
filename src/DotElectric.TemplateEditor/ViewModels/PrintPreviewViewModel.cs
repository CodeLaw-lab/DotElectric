using System.Windows.Documents;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Модель представления окна предпросмотра печати: документ печати
/// и имя вкладки для заголовка окна. Показывается через шов диалогов.
/// </summary>
public sealed record PrintPreviewViewModel(FixedDocument Document, string DisplayName);
