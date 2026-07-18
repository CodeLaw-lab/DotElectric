using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет свойствами строки состояния (StatusBar).
/// </summary>
public sealed partial class StatusBarManager : ObservableObject
{
    private readonly Template _template;
    private readonly Func<bool> _getGridEnabled;
    private readonly Action<bool> _setGridEnabled;
    private readonly Func<double> _getGridStepMm;
    private readonly Action<double> _setGridStepMm;
    private readonly Func<bool> _getSnapEnabled;
    private readonly Action<bool> _setSnapEnabled;
    private readonly Action? _onGridRefresh;

    /// <summary>
    /// Текущий статус (отображается в строке состояния).
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Готово";

    /// <summary>
    /// Формат листа для отображения в StatusBar.
    /// </summary>
    public string SheetFormat =>
        $"{_template.Sheet.Format} ({OrientationLabel}) {Coordinate.FormatMm(_template.Sheet.WidthMicrons)}×{Coordinate.FormatMm(_template.Sheet.HeightMicrons)} мм";

    /// <summary>
    /// Метка ориентации для отображения (кн./алб.).
    /// </summary>
    private string OrientationLabel =>
        _template.Sheet.Orientation switch
        {
            SheetOrientation.Portrait => "кн.",
            SheetOrientation.Landscape => "алб.",
            _ => ""
        };

    /// <summary>
    /// Включена ли сетка (для binding в StatusBar). Делегирует GridManager.
    /// </summary>
    public bool GridEnabled
    {
        get => _getGridEnabled();
        set
        {
            _setGridEnabled(value);
            OnPropertyChanged();
            _onGridRefresh?.Invoke();
        }
    }

    /// <summary>
    /// Шаг сетки в мм (для binding в Toolbar). Делегирует GridManager.
    /// </summary>
    public double GridStepMm
    {
        get => _getGridStepMm();
        set
        {
            _setGridStepMm(value);
            OnPropertyChanged();
            _onGridRefresh?.Invoke();
        }
    }

    /// <summary>
    /// Включена ли привязка к сетке (для binding в StatusBar). Делегирует GridManager.
    /// </summary>
    public bool SnapEnabled
    {
        get => _getSnapEnabled();
        set
        {
            _setSnapEnabled(value);
            OnPropertyChanged();
        }
    }

    public StatusBarManager(
        Template template,
        Func<bool> getGridEnabled,
        Action<bool> setGridEnabled,
        Func<double> getGridStepMm,
        Action<double> setGridStepMm,
        Func<bool> getSnapEnabled,
        Action<bool> setSnapEnabled,
        Action? onGridRefresh = null)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _getGridEnabled = getGridEnabled ?? throw new ArgumentNullException(nameof(getGridEnabled));
        _setGridEnabled = setGridEnabled ?? throw new ArgumentNullException(nameof(setGridEnabled));
        _getGridStepMm = getGridStepMm ?? throw new ArgumentNullException(nameof(getGridStepMm));
        _setGridStepMm = setGridStepMm ?? throw new ArgumentNullException(nameof(setGridStepMm));
        _getSnapEnabled = getSnapEnabled ?? throw new ArgumentNullException(nameof(getSnapEnabled));
        _setSnapEnabled = setSnapEnabled ?? throw new ArgumentNullException(nameof(setSnapEnabled));
        _onGridRefresh = onGridRefresh;
    }
}
