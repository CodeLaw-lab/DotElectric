using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Пины эффективной ресурсной поверхности темы (кандидат 8 обзора №5):
/// одна общая структура стилей и две палитры (цвета + кисти). Словари
/// берутся те же, что грузит приложение, — скомпилированные ресурсы главной
/// сборки по pack-URI (прецедент ThemeDictionaryManagerTests), каждый тест
/// работает в своём STA-потоке и грузит их заново, чтобы значения были
/// аффинны его потоку. Подмена палитры повторяет механику
/// ThemeDictionaryManager.SetThemeDictionary: поиск реального
/// FindThemeDictionary по подстроке имени файла, удалить, добавить новую
/// в конец (BeginInit/EndInit против мигания). Логика ThemeService.SetTheme
/// (выбор пути, запись настроек, событие) запинена отдельно в
/// ThemeServiceTests и MainViewModelTests моками — здесь внешняя поверхность.
/// Дополнительно фиксируется композиция ресурсов приложения (структура перед
/// палитрой) — чтением только Source словарей, без потоко-аффинных значений.
/// </summary>
[Collection("ThemeDictionaryManager")]
public class ThemeResourceSurfaceTests
{
    private const string StructurePath =
        "pack://application:,,,/DotElectric.TemplateEditor;component/Resources/Styles/ThemeStructure.xaml";

    private const string DarkThemePath =
        "pack://application:,,,/DotElectric.TemplateEditor;component/Resources/Styles/DarkTheme.xaml";

    private const string LightThemePath =
        "pack://application:,,,/DotElectric.TemplateEditor;component/Resources/Styles/LightTheme.xaml";

    /// <summary>22 цвета палитры: ключ → ожидаемый HEX. Тёмная тема.</summary>
    private static readonly IReadOnlyDictionary<string, string> DarkPalette =
        new Dictionary<string, string>
        {
            ["AccentColor"] = "#0078D4",
            ["SelectionBlue"] = "#42A5F5",
            ["SelectionGreen"] = "#66BB6A",
            ["WindowBackgroundColor"] = "#212121",
            ["PanelBackgroundColor"] = "#303030",
            ["CanvasBackgroundColor"] = "#FFFFFF",
            ["ToolBarBackgroundColor"] = "#2D2D2D",
            ["StatusBarBackgroundColor"] = "#2D2D2D",
            ["MenuBackgroundColor"] = "#2D2D2D",
            ["TabControlBackgroundColor"] = "#252525",
            ["TabItemBackgroundColor"] = "#2D2D2D",
            ["TabItemSelectedBackgroundColor"] = "#3E3E3E",
            ["TextPrimaryColor"] = "#FFFFFF",
            ["TextSecondaryColor"] = "#BDBDBD",
            ["TextDisabledColor"] = "#757575",
            ["BorderColor"] = "#424242",
            ["BorderLightColor"] = "#505050",
            ["HoverBackgroundColor"] = "#3E3E42",
            ["PressedBackgroundColor"] = "#0078D4",
            ["SelectedBackgroundColor"] = "#0078D4",
            ["TabCloseButtonHoverColor"] = "#FF4444",
            ["WarningOrangeColor"] = "#FFA500",
        };

    /// <summary>22 цвета палитры: ключ → ожидаемый HEX. Светлая тема.</summary>
    private static readonly IReadOnlyDictionary<string, string> LightPalette =
        new Dictionary<string, string>
        {
            ["AccentColor"] = "#0078D4",
            ["SelectionBlue"] = "#42A5F5",
            ["SelectionGreen"] = "#66BB6A",
            ["WindowBackgroundColor"] = "#FAFAFA",
            ["PanelBackgroundColor"] = "#FFFFFF",
            ["CanvasBackgroundColor"] = "#FFFFFF",
            ["ToolBarBackgroundColor"] = "#F5F5F5",
            ["StatusBarBackgroundColor"] = "#F5F5F5",
            ["MenuBackgroundColor"] = "#F5F5F5",
            ["TabControlBackgroundColor"] = "#FAFAFA",
            ["TabItemBackgroundColor"] = "#FFFFFF",
            ["TabItemSelectedBackgroundColor"] = "#E3F2FD",
            ["TextPrimaryColor"] = "#212121",
            ["TextSecondaryColor"] = "#757575",
            ["TextDisabledColor"] = "#BDBDBD",
            ["BorderColor"] = "#E0E0E0",
            ["BorderLightColor"] = "#F5F5F5",
            ["HoverBackgroundColor"] = "#E3F2FD",
            ["PressedBackgroundColor"] = "#BBDEFB",
            ["SelectedBackgroundColor"] = "#0078D4",
            ["TabCloseButtonHoverColor"] = "#FF0000",
            ["WarningOrangeColor"] = "#FF8C00",
        };

    /// <summary>22 кисти палитры: ключ кисти → ключ цвета той же палитры.</summary>
    private static readonly IReadOnlyDictionary<string, string> PaletteBrushes =
        new Dictionary<string, string>
        {
            ["AccentBrush"] = "AccentColor",
            ["SelectionBlueBrush"] = "SelectionBlue",
            ["SelectionGreenBrush"] = "SelectionGreen",
            ["WarningOrangeBrush"] = "WarningOrangeColor",
            ["WindowBackgroundBrush"] = "WindowBackgroundColor",
            ["PanelBackgroundBrush"] = "PanelBackgroundColor",
            ["CanvasBackgroundBrush"] = "CanvasBackgroundColor",
            ["ToolBarBackgroundBrush"] = "ToolBarBackgroundColor",
            ["StatusBarBackgroundBrush"] = "StatusBarBackgroundColor",
            ["MenuBackgroundBrush"] = "MenuBackgroundColor",
            ["TabControlBackgroundBrush"] = "TabControlBackgroundColor",
            ["TabItemBackgroundBrush"] = "TabItemBackgroundColor",
            ["TabItemSelectedBackgroundBrush"] = "TabItemSelectedBackgroundColor",
            ["TextPrimaryBrush"] = "TextPrimaryColor",
            ["TextSecondaryBrush"] = "TextSecondaryColor",
            ["TextDisabledBrush"] = "TextDisabledColor",
            ["BorderBrush"] = "BorderColor",
            ["BorderLightBrush"] = "BorderLightColor",
            ["HoverBackgroundBrush"] = "HoverBackgroundColor",
            ["PressedBackgroundBrush"] = "PressedBackgroundColor",
            ["SelectedBackgroundBrush"] = "SelectedBackgroundColor",
            ["TabCloseButtonHoverBrush"] = "TabCloseButtonHoverColor",
        };

    /// <summary>Именованные стили структуры.</summary>
    private static readonly string[] NamedStyleKeys =
    [
        "ToolBarButtonStyle",
        "ToolBarToggleButtonStyle",
        "ToolBarRadioButtonStyle",
        "ToolBarSeparatorStyle",
    ];

    /// <summary>Типовые/implicit-стили структуры (ключ — тип).</summary>
    private static readonly Type[] TypeStyleKeys =
    [
        typeof(TabItem),
        typeof(Menu),
        typeof(MenuItem),
        typeof(ToolBar),
        typeof(StatusBar),
        typeof(TabControl),
        typeof(ListBox),
        typeof(ListBoxItem),
        typeof(System.Windows.Controls.RadioButton),
        typeof(ScrollViewer),
        typeof(GridSplitter),
    ];

    [Fact]
    public void Palette_DarkTheme_ExactColorsAndBrushes()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();

            AssertPalette(DarkThemePath, DarkPalette);
        });
    }

    [Fact]
    public void Palette_LightTheme_ExactColorsAndBrushes()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();

            AssertPalette(LightThemePath, LightPalette);
        });
    }

    [Fact]
    public void LiveSwitch_SurfaceColorsFollowPalette()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();
            var surface = BuildSurface(DarkThemePath);
            AssertPaletteColors(surface, DarkPalette);

            // Механика ThemeDictionaryManager.SetThemeDictionary: палитра
            // ищется реальным FindThemeDictionary по имени файла, удаляется,
            // новая добавляется в конец.
            SwapPalette(surface, LightThemePath);

            AssertPaletteColors(surface, LightPalette);

            SwapPalette(surface, DarkThemePath);

            AssertPaletteColors(surface, DarkPalette);
        });
    }

    [Fact]
    public void StructureDictionary_StylesOnly_NoColorsNoBrushes()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();
            var structure = LoadDictionary(StructurePath);

            // Инвариант консолидации: структура несёт только стили —
            // цвета и кисти живут в палитрах тем.
            foreach (var key in structure.Keys)
            {
                Assert.False(structure[key] is Color,
                    $"Структура содержит цвет {key} — цвета должны жить только в палитрах");
                Assert.False(structure[key] is SolidColorBrush,
                    $"Структура содержит кисть {key} — кисти должны жить только в палитрах");
                Assert.IsType<Style>(structure[key]);
            }

            foreach (var styleKey in NamedStyleKeys)
            {
                Assert.True(structure.Contains(styleKey), $"В структуре нет стиля {styleKey}");
            }

            foreach (var typeKey in TypeStyleKeys)
            {
                Assert.IsType<Style>(structure[typeKey]);
            }

            Assert.False(structure.Contains("MaterialFlatButton"));
            Assert.False(structure.Contains("MaterialFlatToggleButton"));
        });
    }

    /// <summary>
    /// Композиция ресурсов приложения: общая структура подключена перед
    /// стартовой тёмной палитрой (порядок «палитра последняя» — условие
    /// живой подмены). Читаются только Source словарей — значения ресурсов
    /// потоко-аффинны потоку создания приложения и здесь не трогаются.
    /// </summary>
    [Fact]
    public void AppResources_StructureBeforePalette()
    {
        WpfContext.Execute(() =>
        {
            WpfApplicationHost.Ensure();

            var merged = Application.Current.Resources.MergedDictionaries;
            var structureIndex = IndexOfSource(merged, "ThemeStructure.xaml");
            var darkIndex = IndexOfSource(merged, "DarkTheme.xaml");

            Assert.True(structureIndex >= 0, "Структура темы не подключена в ресурсах приложения");
            Assert.True(darkIndex >= 0, "Стартовая тёмная палитра не подключена в ресурсах приложения");
            Assert.True(structureIndex < darkIndex,
                "Структура темы должна быть подключена перед палитрой");
            Assert.Equal(merged.Count - 1, darkIndex);
        });
    }

    /// <summary>
    /// Собирает поверхность темы в потоке теста: общая структура + палитра —
    /// тот же состав и порядок, что в ресурсах приложения.
    /// </summary>
    private static ResourceDictionary BuildSurface(string palettePath)
    {
        var surface = new ResourceDictionary();
        surface.MergedDictionaries.Add(LoadDictionary(StructurePath));
        surface.MergedDictionaries.Add(LoadDictionary(palettePath));
        return surface;
    }

    /// <summary>
    /// Повторяет механику ThemeDictionaryManager.SetThemeDictionary на
    /// локальной поверхности: палитра ищется реальным поиском по подстроке
    /// имени файла, удаляется, новая добавляется в конец.
    /// </summary>
    private static void SwapPalette(ResourceDictionary surface, string palettePath)
    {
        var current = ThemeDictionaryManager.FindThemeDictionary(surface.MergedDictionaries);
        Assert.NotNull(current);
        surface.MergedDictionaries.Remove(current!);

        var newPalette = LoadDictionary(palettePath);
        newPalette.BeginInit();
        surface.MergedDictionaries.Add(newPalette);
        newPalette.EndInit();
    }

    /// <summary>
    /// Палитра: ровно 22 цвета и 22 кисти; значения цветов точные, кисти
    /// ссылаются на цвета своей палитры; ни стилей, ни лишних ключей.
    /// </summary>
    private static void AssertPalette(string palettePath, IReadOnlyDictionary<string, string> expected)
    {
        var palette = LoadDictionary(palettePath);

        AssertPaletteColors(palette, expected);
        Assert.Equal(expected.Count + PaletteBrushes.Count, palette.Count);

        foreach (var (brushKey, colorKey) in PaletteBrushes)
        {
            var brush = Assert.IsType<SolidColorBrush>(palette[brushKey]);
            var expectedColor = (Color)ColorConverter.ConvertFromString(expected[colorKey]);
            Assert.True(expectedColor == brush.Color,
                $"Кисть {brushKey}: ожидался цвет {expected[colorKey]}, фактически {brush.Color}");
        }

        foreach (var key in palette.Keys)
        {
            Assert.True(key is string, $"Лишний ключ палитры: {key}");
        }
    }

    private static void AssertPaletteColors(ResourceDictionary surface, IReadOnlyDictionary<string, string> expected)
    {
        foreach (var (key, hex) in expected)
        {
            var expectedColor = (Color)ColorConverter.ConvertFromString(hex);
            var actual = Assert.IsType<Color>(surface[key]);
            Assert.True(expectedColor == actual,
                $"Цвет {key}: ожидался {hex}, фактически {actual}");
        }
    }

    private static ResourceDictionary LoadDictionary(string packPath)
        => new() { Source = new Uri(packPath) };

    private static int IndexOfSource(IEnumerable<ResourceDictionary> dictionaries, string marker)
    {
        var index = 0;
        foreach (var dictionary in dictionaries)
        {
            if (dictionary.Source != null && dictionary.Source.OriginalString.Contains(marker))
                return index;
            index++;
        }

        return -1;
    }
}
