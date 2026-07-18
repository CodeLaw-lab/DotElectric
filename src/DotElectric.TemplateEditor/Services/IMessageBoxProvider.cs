namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// РђР±СЃС‚СЂР°РєС†РёСЏ РґР»СЏ РїРѕРєР°Р·Р° MessageBox. РџРѕР·РІРѕР»СЏРµС‚ РјРѕРєР°С‚СЊ РґРёР°Р»РѕРіРё РІ unit-С‚РµСЃС‚Р°С….
/// Р’ С‚РµСЃС‚Р°С… РјРѕР¶РЅРѕ РјРѕРєР°С‚СЊ, РІ РїСЂРѕРґРµ вЂ” WPF СЂРµР°Р»РёР·Р°С†РёСЏ СЃ Dispatcher.
/// </summary>
public interface IMessageBoxProvider
{
    /// <summary>
    /// РџРѕРєР°Р·Р°С‚СЊ MessageBox СЃ Р·Р°РґР°РЅРЅС‹РјРё РїР°СЂР°РјРµС‚СЂР°РјРё.
    /// Р’С‹Р·С‹РІР°РµС‚СЃСЏ РёР· UI-РїРѕС‚РѕРєР° (РёР»Рё Dispatcher.Invoke РІ WPF СЂРµР°Р»РёР·Р°С†РёРё).
    /// </summary>
    MsgrResult Show(string message, string caption, MsgrButtons buttons, MsgrIcon icon);
}

/// <summary>
/// РђР±СЃС‚СЂР°РєС†РёСЏ РґР»СЏ IDispatcher. РџРѕР·РІРѕР»СЏРµС‚ С‚РµСЃС‚РёСЂРѕРІР°С‚СЊ Р±РµР· WPF Application.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Р’С‹РїРѕР»РЅРёС‚СЊ РґРµР№СЃС‚РІРёРµ РІ UI-РїРѕС‚РѕРєРµ.
/// </summary>
    T Invoke<T>(Func<T> action);

    /// <summary>
    /// Р’С‹РїРѕР»РЅРёС‚СЊ РґРµР№СЃС‚РІРёРµ РІ UI-РїРѕС‚РѕРєРµ (Р±РµР· РІРѕР·РІСЂР°С‚Р° СЂРµР·СѓР»СЊС‚Р°С‚Р°).
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// Выполнить асинхронное действие в UI-потоке.
    /// </summary>
    Task InvokeAsync(Func<Task> action);
}

/// <summary>
/// Р РµР·СѓР»СЊС‚Р°С‚ MessageBox.
/// </summary>
public enum MsgrResult
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Yes = 6,
    No = 7
}

/// <summary>
/// РљРЅРѕРїРєРё MessageBox.
/// </summary>
public enum MsgrButtons
{
    OK = 0,
    OKCancel = 1,
    YesNoCancel = 3,
    YesNo = 4
}

/// <summary>
/// РРєРѕРЅРєР° MessageBox.
/// </summary>
public enum MsgrIcon
{
    None = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Question = 4
}
