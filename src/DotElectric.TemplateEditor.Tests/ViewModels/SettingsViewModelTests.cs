using DotElectric.TemplateEditor.Models;
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

    [Fact]
    public void Constructor_LoadsGridNodeDefaults()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);

        Assert.Equal(250000, vm.GridMaxNodes);
        Assert.True(vm.GridNodeColorAuto);
        Assert.Equal("#C0C0C0", vm.GridNodeColor);
        Assert.Equal(2.0, vm.GridNodeSize);
    }

    [Fact]
    public void Constructor_ExplicitNodeColor_SetsAutoFalse()
    {
        _defaultSettings.GridNodeColor = "#FF0000";

        var vm = new SettingsViewModel(_settingsMock.Object);

        Assert.False(vm.GridNodeColorAuto);
        Assert.Equal("#FF0000", vm.GridNodeColor);
    }

    [Fact]
    public void Confirm_AutoNodeColor_SavesNull()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        vm.GridNodeColorAuto = true;
        vm.GridNodeColor = "#FF0000";

        vm.ConfirmCommand.Execute(null);

        _settingsMock.Verify(s => s.Save(It.Is<AppSettings>(a => a.GridNodeColor == null)), Times.Once);
    }

    [Fact]
    public void Confirm_ExplicitNodeColor_SavesValue()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        vm.GridNodeColorAuto = false;
        vm.GridNodeColor = "#FF0000";
        vm.GridMaxNodes = 100000;
        vm.GridNodeSize = 4.0;

        vm.ConfirmCommand.Execute(null);

        _settingsMock.Verify(s => s.Save(It.Is<AppSettings>(a =>
            a.GridNodeColor == "#FF0000" &&
            a.GridMaxNodes == 100000 &&
            a.GridNodeSize == 4.0)), Times.Once);
    }

    [Fact]
    public void Confirm_SavesRemainingSettingsFields()
    {
        var vm = new SettingsViewModel(_settingsMock.Object);
        vm.ShowGrid = false;
        vm.SnapToGrid = false;
        vm.AutosaveIntervalMinutes = 10;
        vm.DefaultSheetFormat = "A4";
        vm.DefaultZoom = 2.0;

        vm.ConfirmCommand.Execute(null);

        _settingsMock.Verify(s => s.Save(It.Is<AppSettings>(a =>
            a.ShowGrid == false &&
            a.SnapToGrid == false &&
            a.AutosaveIntervalMinutes == 10 &&
            a.DefaultSheetFormat == "A4" &&
            a.DefaultZoom == 2.0)), Times.Once);
    }

    [Fact]
    public void Confirm_PreservesFieldsNotExposedInViewModel()
    {
        var latest = new AppSettings
        {
            LastUsedSheetFormat = "A2",
            LastUsedSheetOrientation = "Portrait"
        };
        _settingsMock.Setup(s => s.Load()).Returns(latest);

        var vm = new SettingsViewModel(_settingsMock.Object);
        vm.ConfirmCommand.Execute(null);

        // Поля, которых нет в SettingsViewModel, берутся из актуального Load() и не теряются
        _settingsMock.Verify(s => s.Save(It.Is<AppSettings>(a =>
            a.LastUsedSheetFormat == "A2" &&
            a.LastUsedSheetOrientation == "Portrait")), Times.Once);
    }
}
