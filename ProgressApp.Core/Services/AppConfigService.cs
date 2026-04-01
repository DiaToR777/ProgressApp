using ProgressApp.Core.Configuration;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;
using System.Text.Json;

namespace ProgressApp.Core.Services
{
    public class AppConfigService : IAppConfigService
    {
        private readonly string _path = AppPaths.ConfigPath;
        public AppConfig Load()
        {
            if (!File.Exists(_path))
                return new AppConfig();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        public void Save(AppConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

    }
}
