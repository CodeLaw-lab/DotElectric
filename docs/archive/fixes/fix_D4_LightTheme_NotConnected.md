# P3-D4: LightTheme не подключён в App.xaml

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/App.xaml:10-18`

**Симптом:** Тема `LightTheme` существует в проекте, но не подключена в `App.xaml`. В результате:
- Приложение всегда использует тёмную тему
- Пользователь не может переключиться на светлую тему
- Файл `Resources/Styles/LightTheme.xaml` — мёртвый код

**Текущий App.xaml:**
```xml
<ResourceDictionary.MergedDictionaries>
    <materialDesign:CustomColorTheme BaseTheme="Dark"
                                     PrimaryColor="#0078D4"
                                     SecondaryColor="#66BB6A"/>
    <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
    <ResourceDictionary Source="Resources/Styles/DarkTheme.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

**Существующий файл:** `Resources/Styles/LightTheme.xaml` — существует, но не загружается.

## Пошаговый план исправления

### Шаг 1: Создать ThemeService

Создать `Services/ThemeService.cs`:

```csharp
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace DotElectric.TemplateEditor.Services;

public enum AppTheme
{
    Dark,
    Light
}

public class ThemeService
{
    private const string DarkThemeUri = "Resources/Styles/DarkTheme.xaml";
    private const string LightThemeUri = "Resources/Styles/LightTheme.xaml";

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public void SetTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app == null) return;

        var oldThemeUri = CurrentTheme == AppTheme.Dark ? DarkThemeUri : LightThemeUri;
        var newThemeUri = theme == AppTheme.Dark ? DarkThemeUri : LightThemeUri;

        // Меняем базовую тему MaterialDesign
        var materialTheme = app.Resources.MergedDictionaries
            .OfType<CustomColorTheme>()
            .FirstOrDefault();
        if (materialTheme != null)
        {
            materialTheme.BaseTheme = theme == AppTheme.Dark ? BaseTheme.Dark : BaseTheme.Light;
        }

        // Меняем тему приложения
        var oldThemeDict = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.ToString().EndsWith(oldThemeUri.Split('/').Last()) == true);

        if (oldThemeDict != null)
        {
            app.Resources.MergedDictionaries.Remove(oldThemeDict);
        }

        var newThemeDict = new ResourceDictionary
        {
            Source = new Uri(newThemeUri, UriKind.Relative)
        };
        app.Resources.MergedDictionaries.Add(newThemeDict);

        CurrentTheme = theme;
    }
}
```

### Шаг 2: Обновить App.xaml

Подключить обе темы, но загружать только одну (тёмную по умолчанию):

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <materialDesign:CustomColorTheme BaseTheme="Dark"
                                             PrimaryColor="#0078D4"
                                             SecondaryColor="#66BB6A"/>
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
            <ResourceDictionary Source="Resources/Styles/DarkTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>

        <!-- Конвертеры (без изменений) -->
        ...
    </ResourceDictionary>
</Application.Resources>
```

Тему можно будет менять через `ThemeService.SetTheme()`.

### Шаг 3: Зарегистрировать ThemeService в DI

В `App.xaml.cs` (или в файле регистрации DI):

```csharp
services.AddSingleton<ThemeService>();
```

### Шаг 4: Добавить UI для переключения темы

Добавить в ViewModel команду переключения:

```csharp
[RelayCommand]
private void ToggleTheme()
{
    var themeService = App.ServiceProvider.GetService<ThemeService>();
    if (themeService == null) return;

    var newTheme = themeService.CurrentTheme == AppTheme.Dark
        ? AppTheme.Light
        : AppTheme.Dark;
    themeService.SetTheme(newTheme);

    // Обновляем иконку/текст в UI
    IsDarkTheme = newTheme == AppTheme.Dark;
}
```

В XAML добавить кнопку переключения (например, в строку меню или панель инструментов):

```xml
<ToggleButton IsChecked="{Binding IsDarkTheme}"
              ToolTip="Тёмная/светлая тема"
              Command="{Binding ToggleThemeCommand}">
    <ToggleButton.Content>
        <TextBlock Text="🌙"/>
    </ToggleButton.Content>
</ToggleButton>
```

### Шаг 5: Сохранять выбранную тему в настройках

Опционально — сохранять тему в `Properties.Settings`:

```csharp
public void SetTheme(AppTheme theme)
{
    // ... существующая логика ...
    Properties.Settings.Default.Theme = theme.ToString();
    Properties.Settings.Default.Save();
}
```

Загружать при старте:
```csharp
if (Enum.TryParse<AppTheme>(Properties.Settings.Default.Theme, out var savedTheme))
    themeService.SetTheme(savedTheme);
```

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet run --project src/DotElectric.TemplateEditor
# Проверить:
# 1. Приложение запускается в тёмной теме
# 2. После переключения — светлая тема
# 3. После перезапуска — тема сохраняется (если реализован шаг 5)
```

## Риски

- **Средний:** Смена `ResourceDictionary` в runtime может привести к временному мерцанию
- **Средний:** Не все стили могут быть корректно переключены — нужно удостовериться, что DarkTheme.xaml и LightTheme.xaml имеют одинаковый набор ресурсов
- **Низкий:** MaterialDesign Themes имеет собственный механизм переключения `BaseTheme`. Нужно убедиться, что оба механизма согласованы
