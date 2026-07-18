using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly Mock<ISettingsService> _settingsMock;
    private readonly AppSettings _defaultSettings;

    public SettingsViewModelTests()
    {
        _defaultSettings = new AppSettings
        {
            Theme = "Light",
            ShowGrid = true,
            SnapToGrid = true,
            GridStepMm = 5.0,
            AutosaveIntervalMinutes = 5,
            DefaultSheetFormat = "A3",
            DefaultZoom = 1.0
        };

        _settingsMock = new Mock<ISettingsService>();
        _settingsMock.Setup(s => s.Load()).Returns(_defaultSettings);
        _settingsMock.Setup(s => s.Get("Theme", "Light")).Returns("Light");
        _settingsMock.Setup(s => s.Get("ShowGrid", true)).Returns(true);
        _settingsMock.Setup(s => s.Get("SnapToGrid", true)).Returns(true);
        _settingsMock.Setup(s => s.Get("GridStepMm", 5.0)).Returns(5.0);
        _settingsMock.Setup(s => s.Get("AutosaveIntervalMinutes", 5)).Returns(5);
        _settingsMock.Setup(s => s.Get("DefaultSheetFormat", "A3")).Returns("A3");
        _settingsMock.Setup(s => s.Get("DefaultZoom", 1.0)).Returns(1.0);
    }

    [Fact]
    public void Constructor_LoadsSettingsFromService()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);

        Assert.Equal("Light", vm.Theme);
        Assert.True(vm.ShowGrid);
        Assert.True(vm.SnapToGrid);
        Assert.Equal(5.0, vm.GridStepMm);
        Assert.Equal(5, vm.AutosaveIntervalMinutes);
        Assert.Equal("A3", vm.DefaultSheetFormat);
        Assert.Equal(1.0, vm.DefaultZoom);
    }

    [Fact]
    public void Constructor_ContainsExpectedOptions()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);

        Assert.Contains("Light", vm.ThemeOptions);
        Assert.Contains("Dark", vm.ThemeOptions);
        Assert.Contains("A3", vm.FormatOptions);
        Assert.Contains("A4×2", vm.FormatOptions);
        Assert.Contains(1.0, vm.ZoomOptions);
    }

    [Fact]
    public void Confirm_SavesSettingsAndFiresEvent()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        var confirmed = false;
        vm.ConfirmRequested += () => confirmed = true;

        vm.Theme = "Dark";
        vm.GridStepMm = 10.0;

        vm.ConfirmCommand.Execute(null);

        Assert.True(confirmed);
        _settingsMock.Verify(s => s.Load(), Times.AtLeastOnce);
        _settingsMock.Verify(s => s.Save(It.Is<AppSettings>(a =>
            a.Theme == "Dark" &&
            a.GridStepMm == 10.0 &&
            a.DefaultSheetFormat == "A3")), Times.Once);
    }

    [Fact]
    public void Cancel_FiresCancelEvent()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        var cancelled = false;
        vm.CancelRequested += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
        _settingsMock.Verify(s => s.Save(It.IsAny<AppSettings>()), Times.Never);
    }

    [Fact]
    public void Title_IsNotEmpty()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        Assert.False(string.IsNullOrWhiteSpace(vm.Title));
    }

    [Fact]
    public void Constructor_NullSettings_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SettingsViewModel(null!));
    }
}
