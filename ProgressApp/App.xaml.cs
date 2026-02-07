using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Data;
using ProgressApp.Services;
using ProgressApp.Themes;
using ProgressApp.ViewModels;
using ProgressApp.ViewModels.InitialSetup;
using ProgressApp.ViewModels.Settings;
using ProgressApp.ViewModels.Table;
using ProgressApp.ViewModels.Today;
using System.Configuration;
using System.Data;
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

            // 1. Налаштування шляху до БД (твій прикол з Desktop)
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var folder = Path.Combine(desktop, "ProgressApp");
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "progress.db");

            // 2. Реєстрація БД
            services.AddDbContext<ProgressDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // 3. Реєстрація Сервісів
            services.AddSingleton<SettingsService>();
            services.AddSingleton<JournalService>();

            // 4. Реєстрація ViewModels
            services.AddTransient<TableViewModel>();
            services.AddTransient<InitialSetupViewModel>();// Transient, бо дані мають оновлюватися при відкритті
            services.AddSingleton<MainViewModel>();
            services.AddTransient<TodayViewModel>();
            services.AddTransient<SettingsViewModel>();
            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            // Ініціалізація бази
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                db.Initialize();

                var settings = scope.ServiceProvider.GetRequiredService<SettingsService>();

                ThemeManager.ApplyTheme(settings.GetTheme());

            }

            var mainVM = _serviceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow { DataContext = mainVM };
            mainWindow.Show();
        }

    }

}
