using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// ViewModel для диалога выбора пользовательского формата листа.
/// </summary>
public partial class CustomSheetDialogViewModel : ObservableObject, ICustomSheetDialogVm
{
    /// <summary>
    /// Ширина листа в мм.
    /// </summary>
    [ObservableProperty]
    private double _widthMm = 210;

    /// <summary>
    /// Высота листа в мм.
    /// </summary>
    [ObservableProperty]
    private double _heightMm = 297;

    /// <summary>
    /// Можно ли подтвердить (ширина и высота > 0).
    /// </summary>
    public bool CanConfirm => WidthMm > 0 && HeightMm > 0 && WidthMm <= PhysicalConstants.MaxCustomSheetSizeMm && HeightMm <= PhysicalConstants.MaxCustomSheetSizeMm;

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public string Title => "Пользовательский формат";

    /// <summary>
    /// Описание.
    /// </summary>
    public string Description => "Введите размеры листа в миллиметрах:";

    partial void OnWidthMmChanged(double value)
    {
        OnPropertyChanged(nameof(CanConfirm));
    }

    partial void OnHeightMmChanged(double value)
    {
        OnPropertyChanged(nameof(CanConfirm));
    }

    /// <summary>
    /// Быстрый выбор стандартного формата.
    /// </summary>
    [RelayCommand]
    private void SetQuickFormat(string formatName)
    {
        try
        {
            var sheet = Sheet.FromFormat(formatName);
            WidthMm = sheet.WidthMm;
            HeightMm = sheet.HeightMm;
        }
        catch
        {
            // ignore invalid format names
        }
    }

    /// <summary>
    /// Команда подтверждения.
    /// </summary>
    [RelayCommand]
    private void Confirm()
    {
        // Window.Close( DialogResult = true ) обрабатывается в View
        ConfirmRequested?.Invoke();
    }

    /// <summary>
    /// Команда отмены.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }

    /// <summary>
    /// Событие для View — подтверждение.
    /// </summary>
    public event Action? ConfirmRequested;

    /// <summary>
    /// Событие для View — отмена.
    /// </summary>
    public event Action? CancelRequested;
}
