using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Messages;

/// <summary>
/// Запрос на закрытие указанной вкладки.
/// Посылается EditorViewModel, обрабатывается MainViewModel.
/// </summary>
public record CloseTabRequestMessage(EditorViewModel Tab);

/// <summary>
/// Запрос на закрытие всех вкладок, кроме указанной.
/// </summary>
public record CloseOtherTabsRequestMessage(EditorViewModel Tab);

/// <summary>
/// Запрос на закрытие всех вкладок.
/// </summary>
public record CloseAllTabsRequestMessage;
