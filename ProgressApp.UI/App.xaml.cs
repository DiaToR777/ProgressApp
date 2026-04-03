using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Configuration;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Services;
using ProgressApp.Core.Services.Auth;   
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services.Message;
using ProgressApp.WpfUI.Themes;
using ProgressApp.WpfUI.ViewModels;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Table;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.WpfUI.Views;
using ProgressApp.WpfUI.ViewModels.Login;
using Serilog;
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

            AppPaths.EnsureDirectoriesExist();

            LoggerConfigurator.Setup();

            Log.Information("==========================================");
            Log.Information("Application ProgressApp is starting...");

            var dbPath = AppPaths.DbPath;

            services.AddDbContext<ProgressDbContext>();

            services.AddSingleton<IDbState>(new DbState(dbPath));

            services.AddSingleton<ILocalizationService>(TranslationSource.Instance);
            services.AddSingleton<IThemeService, ThemeWrapper>();

            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAppConfigService, AppConfigService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<TableViewModel>();
            services.AddTransient<InitialSetupViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();
            
            _serviceProvider = services.BuildServiceProvider();
        }
        protected async override void OnStartup(StartupEventArgs e)
        {
            SQLitePCL.Batteries_V2.Init();
            base.OnStartup(e);

            try
            {
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
            _serviceProvider.Dispose();
            Log.Information("Application is exiting.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

}
