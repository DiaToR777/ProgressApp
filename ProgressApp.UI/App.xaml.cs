using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Interfaces.IMessage;
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
        public ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appData, "ProgressApp");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var dbPath = Path.Combine(folder, "progress.db");

            services.AddDbContext<ProgressDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddSingleton<ILocalizationService>(TranslationSource.Instance);
            services.AddSingleton<IThemeService, ThemeWrapper>(); // Наш новий ворпер

            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IMessageService, MessageService>();   

            // ViewModels
            services.AddTransient<TableViewModel>();
            services.AddTransient<InitialSetupViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainViewModel>(); _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                db.Initialize();

                var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
                var locService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();

                themeService.SetTheme(settings.GetTheme());

                var savedLang = settings.GetLanguage();
                locService.ChangeLanguage(savedLang.CultureCode);
            }

            var mainVM = _serviceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow { DataContext = mainVM };
            mainWindow.Show();
        }
    }

}
