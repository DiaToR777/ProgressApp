using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Data;
using ProgressApp.Localization.Manager;
using ProgressApp.Services;
using ProgressApp.Services.Message;
using ProgressApp.Themes.Managers;
using ProgressApp.ViewModels;
using ProgressApp.ViewModels.InitialSetup;
using ProgressApp.ViewModels.Settings;
using ProgressApp.ViewModels.Table;
using ProgressApp.ViewModels.Today;
using System.IO;
using System.Windows;

namespace ProgressApp
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

            services.AddSingleton<ISettingsService, SettingsService>(); 
            services.AddSingleton<IJournalService, JournalService>();
            services.AddSingleton<IMessageService, MessageService>();

            services.AddTransient<TableViewModel>();
            services.AddTransient<InitialSetupViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                db.Initialize();

                var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>();

                ThemeManager.ApplyTheme(settings.GetTheme());

                var savedLanguage = db.Settings.FirstOrDefault(s => s.Key == "Language")?.Value ?? "en-US";
                TranslationSource.Instance.ChangeLanguage(savedLanguage);
            }

            var mainVM = _serviceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow { DataContext = mainVM };
            mainWindow.Show();
        }
    }

}
