using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services.Message;
using ProgressApp.WpfUI.Themes;
using ProgressApp.WpfUI.ViewModels;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Table;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.WpfUI.Views;
using Serilog;
using Serilog.Exceptions;
using System.IO;
using System.Windows;

namespace ProgressApp.WpfUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "ProgressApp");
            var logFolder = Path.Combine(appFolder, "logs");

            if (!Directory.Exists(appFolder)) Directory.CreateDirectory(appFolder);
            if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails() 
                .WriteTo.Debug()               
                .WriteTo.File(
                    path: Path.Combine(logFolder, "log-.txt"),
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 7,            
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("==========================================");
            Log.Information("Application ProgressApp is starting...");

            var dbPath = Path.Combine(appFolder, "progress.db");

            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Password = "12345"
            }.ToString();

            services.AddDbContext<ProgressDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
            
            services.AddSingleton<ILocalizationService>(TranslationSource.Instance);
            services.AddSingleton<IThemeService, ThemeWrapper>(); 

            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IMessageService, MessageService>();   

            services.AddTransient<TableViewModel>();
            services.AddTransient<InitialSetupViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainViewModel>(); _serviceProvider = services.BuildServiceProvider();
        }
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    Log.Information("Initializing Database and Settings...");
                    var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                    db.Initialize();

                    var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                    var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
                    var locService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();

                    themeService.SetTheme(await settings.GetThemeAsync());

                    var savedLang = await settings.GetLanguageAsync();
                    locService.ChangeLanguage(savedLang.CultureCode);
                }

                var mainVM = _serviceProvider.GetRequiredService<MainViewModel>();
                var mainWindow = new MainWindow { DataContext = mainVM };
                mainWindow.Show();

                Log.Information("App window shown successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Critical error during application startup!");
                MessageBox.Show("Fatal error during startup. Check logs for details.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application is exiting.");
            Log.CloseAndFlush(); 
            base.OnExit(e);
        }
    }

}
