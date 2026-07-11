using Microsoft.Extensions.DependencyInjection;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services.Message;
using ProgressApp.WpfUI.Themes;
using ProgressApp.WpfUI.ViewModels;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Login;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Analytics;
using ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;
using ProgressApp.WpfUI.ViewModels.Analytics.Table;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.WpfUI.Views;
using Serilog;
using System.Windows;
using ProgressApp.WpfUI.LogConfig;
using ProgressApp.WpfUI.Services;
using ProgressApp.Infrastructure.Configuration;
using ProgressApp.Infrastructure.Data;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Application.Services;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Infrastructure.Repositories;

namespace ProgressApp.WpfUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : System.Windows.Application
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

            services.AddSingleton<IDbState>(new DbState(dbPath));
            services.AddDbContext<ProgressDbContext>();


            services.AddSingleton<ILocalizationService>(TranslationSource.Instance);
            services.AddSingleton<IAppThemeService, ThemeWrapper>();

            services.AddSingleton<IDataExchangeRepository, DataExchangeRepository>();
            services.AddSingleton<IAnalyticsRepository, AnalyticsRepository>();
            services.AddSingleton<ICheckinRepository, CheckinRepository>();
            services.AddSingleton<IAuthRepository, AuthRepository>();
            services.AddSingleton<IGoalRepository, GoalRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IOnboardingService, OnboardingService>();

            services.AddSingleton<IGoalService, GoalService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAppConfigService, AppConfigService>();
            services.AddSingleton<IDataExchangeService, DataExchangeService>();
            services.AddSingleton<IAnalyticsService, AnalyticsService>();
            services.AddSingleton<ICheckinService, CheckinService>();

            services.AddSingleton<MainViewModel>();

            services.AddTransient<InitialSetupViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<TableViewModel>();
            services.AddTransient<AnalyticsViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<HeatmapViewModel>();
            services.AddTransient<SettingsViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            SQLitePCL.Batteries_V2.Init();
            base.OnStartup(e);

            var appConfig = _serviceProvider.GetRequiredService<IAppConfigService>();

            var config = appConfig.Load();

            var themeService = _serviceProvider.GetRequiredService<IAppThemeService>();
            themeService.SetTheme(Enum.Parse<AppTheme>(config.Theme));

            var locService = _serviceProvider.GetRequiredService<ILocalizationService>();
            locService.ChangeLanguage(config.Language);

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
                MessageBox.Show("Fatal error during startup. Check logs for details.", "Critical Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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