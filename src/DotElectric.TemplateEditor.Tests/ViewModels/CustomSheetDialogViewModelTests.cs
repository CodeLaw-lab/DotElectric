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
}
