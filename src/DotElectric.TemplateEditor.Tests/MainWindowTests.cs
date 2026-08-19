using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests;

/// <summary>
/// Структурные пины XAML MainWindow (#123): сгенерированные контейнеры меню
/// «Файл > Новый шаблон» должны нести кисти темы.
/// </summary>
[Collection("AutosaveSharedState")]
public class MainWindowTests : IDisposable
{
    private readonly AutosaveService _autosaveService;
    private readonly MainViewModel _viewModel;
    private readonly string _testAutosaveFolder;

    public MainWindowTests()
    {
        var mockTabOperations = new Mock<ITabOperationsService>();
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings());

        _testAutosaveFolder = Path.Combine(Path.GetTempPath(), $"MainWindowAutosaveTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testAutosaveFolder);

        _autosaveService = new AutosaveService(
            new Mock<ITemplateService>().Object,
            mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: _testAutosaveFolder);

        _viewModel = new MainViewModel(
            mockTabOperations.Object,
            mockSettingsService.Object,
            new Mock<IThemeService>().Object,
            new Mock<ITemplateLibraryService>().Object,
            new Mock<IDialogService>().Object,
            new Mock<IDialogHostService>().Object,
            new Mock<IApplicationLifecycle>().Object,
            new Mock<ILogger<MainViewModel>>().Object,
            _autosaveService,
            new Mock<IPrintDocumentGenerator>().Object);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
        _autosaveService.Dispose();
        if (Directory.Exists(_testAutosaveFolder))
            Directory.Delete(_testAutosaveFolder, true);
    }

    /// <summary>
    /// Оба стиля контейнеров подменю «Новый шаблон» несут Background/Foreground
    /// темы (DynamicResource) и Padding implicit-стиля — иначе явный Style
    /// подавляет implicit-стиль MenuItem из темы и пункты рисуются светлыми (#123).
    /// </summary>
    [Fact]
    public void NewSheetMenuContainerStyles_CarryThemeBrushes()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();
            var window = new MainWindow(_viewModel);

            var grid = Assert.IsType<System.Windows.Controls.Grid>(window.Content);
            var menu = Assert.Single(grid.Children.OfType<Menu>());
            var fileMenu = menu.Items.OfType<MenuItem>()
                .FirstOrDefault(m => m.Header as string == "_Файл");
            Assert.NotNull(fileMenu);
            var newSheetItem = fileMenu!.Items.OfType<MenuItem>()
                .FirstOrDefault(m => m.Header as string == "_Новый шаблон");
            Assert.NotNull(newSheetItem);

            // Уровень 1 — группы форматов
            var groupStyle = newSheetItem!.ItemContainerStyle;
            Assert.NotNull(groupStyle);
            AssertThemeSetters(groupStyle!);

            // Уровень 2 — пункты ориентаций (NewSheetOrientationItemStyle)
            var nestedSetter = groupStyle!.Setters.OfType<Setter>()
                .FirstOrDefault(s => s.Property == ItemsControl.ItemContainerStyleProperty);
            Assert.NotNull(nestedSetter);
            var nestedStyle = Assert.IsType<Style>(nestedSetter!.Value);
            AssertThemeSetters(nestedStyle);

            window.Close();
        });
    }

    private static void AssertThemeSetters(Style style)
    {
        AssertDynamicBrushSetter(style, MenuItem.BackgroundProperty, "MenuBackgroundBrush");
        AssertDynamicBrushSetter(style, MenuItem.ForegroundProperty, "TextPrimaryBrush");

        var paddingSetter = style.Setters.OfType<Setter>()
            .FirstOrDefault(s => s.Property == Control.PaddingProperty);
        Assert.NotNull(paddingSetter);
        Assert.Equal(new Thickness(12, 6, 12, 6), Assert.IsType<Thickness>(paddingSetter!.Value));
    }

    private static void AssertDynamicBrushSetter(Style style, DependencyProperty property, string expectedKey)
    {
        var setter = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == property);
        Assert.NotNull(setter);
        var resource = Assert.IsType<DynamicResourceExtension>(setter!.Value);
        Assert.Equal(expectedKey, Assert.IsType<string>(resource.ResourceKey));
    }
}
