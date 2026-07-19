using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class CustomSheetDialogViewModelTests
{
    [Fact]
    public void Constructor_DefaultValues_Correct()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.Equal(210, vm.WidthMm);
        Assert.Equal(297, vm.HeightMm);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.Equal("Пользовательский формат", vm.Title);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.Equal("Введите размеры листа в миллиметрах:", vm.Description);
    }

    [Fact]
    public void CanConfirm_DefaultValues_ReturnsTrue()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.True(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ZeroWidth_ReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel { WidthMm = 0 };
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ZeroHeight_ReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel { HeightMm = 0 };
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_NegativeWidth_ReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel { WidthMm = -10 };
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_TooLargeWidth_ReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel { WidthMm = 2001 };
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_TooLargeHeight_ReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel { HeightMm = 2001 };
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_MaxAllowedSize_ReturnsTrue()
    {
        var vm = new CustomSheetDialogViewModel { WidthMm = 2000, HeightMm = 2000 };
        Assert.True(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ChangingWidth_UpdatesCanConfirm()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.True(vm.CanConfirm);
        vm.WidthMm = 0;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ChangingHeight_UpdatesCanConfirm()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.True(vm.CanConfirm);
        vm.HeightMm = 0;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void ConfirmCommand_InvokesConfirmRequested()
    {
        var vm = new CustomSheetDialogViewModel();
        bool invoked = false;
        vm.ConfirmRequested += () => invoked = true;

        vm.ConfirmCommand.Execute(null);

        Assert.True(invoked);
    }

    [Fact]
    public void CancelCommand_InvokesCancelRequested()
    {
        var vm = new CustomSheetDialogViewModel();
        bool invoked = false;
        vm.CancelRequested += () => invoked = true;

        vm.CancelCommand.Execute(null);

        Assert.True(invoked);
    }

    [Fact]
    public void Implements_ICustomSheetDialogVm()
    {
        var vm = new CustomSheetDialogViewModel();
        Assert.IsAssignableFrom<ICustomSheetDialogVm>(vm);
    }

    // ===== SetQuickFormat =====

    [Fact]
    public void SetQuickFormat_A4_SetsWidth210AndHeight297()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("A4");
        Assert.Equal(210, vm.WidthMm);
        Assert.Equal(297, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_A3_SetsWidth420AndHeight297()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("A3");
        // A3 landscape default: 420 x 297
        Assert.Equal(420, vm.WidthMm);
        Assert.Equal(297, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_A0_SetsCorrectDimensions()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("A0");
        // A0 landscape default: 1189 x 841
        Assert.Equal(1189, vm.WidthMm);
        Assert.Equal(841, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_A4X2_SetsCorrectDimensions()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("A4×2");
        // A4×2 portrait default: 210 x 594
        Assert.Equal(210, vm.WidthMm);
        Assert.Equal(594, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_CaseInsensitive_Works()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("a4");
        Assert.Equal(210, vm.WidthMm);
        Assert.Equal(297, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_InvalidFormat_DoesNotChangeDimensions()
    {
        var vm = new CustomSheetDialogViewModel();
        double originalWidth = vm.WidthMm;
        double originalHeight = vm.HeightMm;

        vm.SetQuickFormatCommand.Execute("InvalidFormat");

        Assert.Equal(originalWidth, vm.WidthMm);
        Assert.Equal(originalHeight, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_NullFormat_DoesNotChangeDimensions()
    {
        var vm = new CustomSheetDialogViewModel();
        double originalWidth = vm.WidthMm;
        double originalHeight = vm.HeightMm;

        vm.SetQuickFormatCommand.Execute(null);

        Assert.Equal(originalWidth, vm.WidthMm);
        Assert.Equal(originalHeight, vm.HeightMm);
    }

    [Fact]
    public void SetQuickFormat_A4X2_WithLatinX_Works()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.SetQuickFormatCommand.Execute("A4X2");
        // A4×2 portrait default: 210 x 594
        Assert.Equal(210, vm.WidthMm);
        Assert.Equal(594, vm.HeightMm);
    }

    // ===== PropertyChanged for CanConfirm =====

    [Fact]
    public void WidthMm_Setter_DoesNotFireCanConfirmPropertyChanged_WhenValueUnchanged()
    {
        var vm = new CustomSheetDialogViewModel();
        bool canConfirmChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanConfirm))
                canConfirmChanged = true;
        };

        // Set to the same value
        vm.WidthMm = 210;

        Assert.False(canConfirmChanged);
    }

    [Fact]
    public void HeightMm_Setter_DoesNotFireCanConfirmPropertyChanged_WhenValueUnchanged()
    {
        var vm = new CustomSheetDialogViewModel();
        bool canConfirmChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanConfirm))
                canConfirmChanged = true;
        };

        // Set to the same value
        vm.HeightMm = 297;

        Assert.False(canConfirmChanged);
    }

    [Fact]
    public void SetQuickFormat_UpdatesDimensions_AndCanConfirm()
    {
        var vm = new CustomSheetDialogViewModel();
        int canConfirmChangedCount = 0;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanConfirm))
                canConfirmChangedCount++;
        };

        vm.SetQuickFormatCommand.Execute("A0");

        Assert.Equal(2, canConfirmChangedCount); // once for WidthMm, once for HeightMm
        Assert.True(vm.CanConfirm);
    }

    [Fact]
    public void SetWidthToZero_FiresCanConfirmPropertyChanged_AndUpdatesCanConfirm()
    {
        var vm = new CustomSheetDialogViewModel();
        bool canConfirmChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanConfirm))
                canConfirmChanged = true;
        };

        vm.WidthMm = 0;

        Assert.True(canConfirmChanged);
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void SetHeightToZero_FiresCanConfirmPropertyChanged_AndUpdatesCanConfirm()
    {
        var vm = new CustomSheetDialogViewModel();
        bool canConfirmChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanConfirm))
                canConfirmChanged = true;
        };

        vm.HeightMm = 0;

        Assert.True(canConfirmChanged);
        Assert.False(vm.CanConfirm);
    }

    // ===== Edge cases =====

    [Fact]
    public void WidthMm_SetToNan_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = double.NaN;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void HeightMm_SetToNan_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.HeightMm = double.NaN;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void WidthMm_SetToPositiveInfinity_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = double.PositiveInfinity;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void HeightMm_SetToPositiveInfinity_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.HeightMm = double.PositiveInfinity;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void WidthMm_SetToNegativeInfinity_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = double.NegativeInfinity;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void WidthMm_SetToMaxCustomSize_CanConfirmReturnsTrue()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = 2000;
        Assert.True(vm.CanConfirm);
    }

    [Fact]
    public void HeightMm_SetToMaxCustomSize_CanConfirmReturnsTrue()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.HeightMm = 2000;
        Assert.True(vm.CanConfirm);
    }

    [Fact]
    public void WidthMm_JustAboveMaxCustomSize_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = 2000.001;
        Assert.False(vm.CanConfirm);
    }

    [Fact]
    public void WidthMm_SetToDoubleMaxValue_CanConfirmReturnsFalse()
    {
        var vm = new CustomSheetDialogViewModel();
        vm.WidthMm = double.MaxValue;
        Assert.False(vm.CanConfirm);
    }
}
