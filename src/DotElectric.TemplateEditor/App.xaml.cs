using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DotElectric.TemplateEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string MutexName = "DotElectric_TemplateEditor";
    private static Mutex? _mutex;
    private static IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Mutex: предотвращение запуска второго экземпляра
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Приложение DotElectric уже запущено.",
                "DotElectric",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Current.Shutdown();
            return;
        }

        // Serilog: настройка логирования
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DotElectric",
            "logs",
            "dotelectric-.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("=== Запуск DotElectric TemplateEditor ===");

            // Host: DI-контейнер
            _host = Host.CreateDefaultBuilder(e.Args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Регистрация сервисов (Singleton)
                    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
                    services.AddSingleton<IDialogFileService, WpfDialogFileService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IDispatcherService, WpfDispatcherService>();
                    services.AddSingleton<IMessageBoxProvider, WpfMessageBoxProvider>();
                    services.AddSingleton<IValidationService>(_ => Helpers.ValidationService.Default);
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<ITemplateValidator, TemplateValidator>();
                    services.AddSingleton<ITemplateService, TemplateService>();
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IPrintDialogFactory, PrintDialogFactory>();
                    services.AddSingleton<IPrintService, PrintService>();
                    services.AddTransient<IPrintDocumentGenerator, PrintDocumentGenerator>();
                    services.AddSingleton<ITemplateLibraryService, TemplateLibraryService>();
                    services.AddSingleton<AutosaveService>();
                    services.AddSingleton<ITextToolSettings, TextToolSettings>();
                    services.AddSingleton<IThemeDictionaryManager, ThemeDictionaryManager>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<IDialogHostService, WpfDialogHostService>();
                    services.AddSingleton<IApplicationLifecycle, WpfApplicationLifecycle>();
                    services.AddSingleton<IFontMetrics>(FontMetrics.Default);
                    services.AddSingleton<IEditorViewModelFactory, EditorViewModelFactory>();
                    services.AddSingleton<ITabOperationsService, TabOperationsService>();

                    // Регистрация EditorViewModel (Transient — свой экземпляр на каждую вкладку)
                    services.AddTransient<EditorViewModel>();

                    // Регистрация MainViewModel (Singleton)
                    services.AddSingleton<MainViewModel>();

                    // Регистрация MainWindow
                    services.AddSingleton<MainWindow>();

                    // ILogger уже настроен через UseSerilog()
                })
                .Build();

            await _host.StartAsync();

            // FontMetrics: загрузка метрик шрифтов GOST из TTF-ресурсов
            FontMetrics.Default.Initialize();
            Log.Information("FontMetrics инициализирована: {Initialized}", FontMetrics.Default.IsInitialized);

            // Глобальные обработчики исключений
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            // Отображение главного окна
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Применяем тему при запуске
            var themeService = _host.Services.GetRequiredService<IThemeService>();
            themeService.SetTheme(themeService.CurrentTheme);

            Log.Information("Главное окно отображено, тема: {Theme}", themeService.CurrentTheme);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Критическая ошибка при запуске приложения");
            MessageBox.Show(
                $"Произошла критическая ошибка при запуске:\n{ex.Message}",
                "DotElectric — Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("=== Завершение DotElectric TemplateEditor ===");

        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "Необработанное исключение в AppDomain");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Необработанное исключение в Dispatcher");
        MessageBox.Show(
            $"Произошла ошибка:\n{e.Exception.Message}",
            "DotElectric — Ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Необработанное исключение в Task");
        e.SetObserved();
    }
}
